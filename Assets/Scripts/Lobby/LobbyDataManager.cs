using UnityEngine;
using System;
using System.Collections.Generic;

namespace LottoDefense.Lobby
{
    public interface ILobbyData
    {
        int Gold { get; set; }
        int Tickets { get; set; }
        List<string> UnlockedUnits { get; }
        bool IsUnitUnlocked(string unitName);
        void UnlockUnit(string unitName);
        int TotalClears { get; set; }
        int TotalSynthesis { get; set; }
        int TotalUpgrades { get; set; }
        int DailyClearCount { get; set; }
        int DailyClaimedStages { get; set; }
        string DailyResetDate { get; set; }
        int GetQuestProgress(string questId);
        void SetQuestProgress(string questId, int value);
        bool IsQuestClaimed(string questId);
        void SetQuestClaimed(string questId, bool claimed);
        bool IsMailRead(string mailId);
        void SetMailRead(string mailId, bool read);
        bool IsMailClaimed(string mailId);
        void SetMailClaimed(string mailId, bool claimed);
    }

    public class LobbyDataManager : MonoBehaviour, ILobbyData
    {
        #region Singleton (Scene-Scoped)
        private static LobbyDataManager _instance;

        public static LobbyDataManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<LobbyDataManager>();
                }
                return _instance;
            }
        }
        #endregion

        #region Constants
        private const string KEY_GOLD = "lobby_gold";
        private const string KEY_TICKETS = "entry_tickets";
        private const string KEY_UNLOCKED_UNITS = "unlocked_units";
        private const string KEY_TOTAL_CLEARS = "total_clears";
        private const string KEY_TOTAL_SYNTHESIS = "total_synthesis";
        private const string KEY_TOTAL_UPGRADES = "total_upgrades";
        private const string KEY_DAILY_CLEAR_COUNT = "daily_clear_count";
        private const string KEY_DAILY_CLAIMED_STAGES = "daily_claimed_stages";
        private const string KEY_DAILY_RESET_DATE = "daily_reset_date";

        private const int INITIAL_GOLD = 0;
        private const int INITIAL_TICKETS = 5;
        private const string INITIAL_UNLOCKED_UNIT = "Warrior";
        #endregion

        #region Events
        public event Action<int> OnGoldChanged;
        public event Action<int> OnTicketsChanged;
        public event Action<string> OnUnitUnlocked;
        public event Action OnDataChanged;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            // NO DontDestroyOnLoad - scene-scoped

            InitializeDefaults();
            CheckDailyReset();

            Debug.Log($"[LobbyDataManager] Initialized - Gold: {Gold}, Tickets: {Tickets}, Unlocked: {string.Join(",", UnlockedUnits)}");
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }
        #endregion

        #region Initialization
        private void InitializeDefaults()
        {
            if (!PlayerPrefs.HasKey(KEY_GOLD))
                PlayerPrefs.SetInt(KEY_GOLD, INITIAL_GOLD);

            if (!PlayerPrefs.HasKey(KEY_TICKETS))
                PlayerPrefs.SetInt(KEY_TICKETS, INITIAL_TICKETS);

            if (!PlayerPrefs.HasKey(KEY_UNLOCKED_UNITS))
            {
                var initial = new UnlockedUnitsData { units = new List<string> { INITIAL_UNLOCKED_UNIT } };
                PlayerPrefs.SetString(KEY_UNLOCKED_UNITS, JsonUtility.ToJson(initial));
            }

            PlayerPrefs.Save();
        }

        private void CheckDailyReset()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            string lastReset = PlayerPrefs.GetString(KEY_DAILY_RESET_DATE, "");

            if (lastReset != today)
            {
                Debug.Log($"[LobbyDataManager] Daily reset: {lastReset} -> {today}");
                PlayerPrefs.SetInt(KEY_DAILY_CLEAR_COUNT, 0);
                PlayerPrefs.SetInt(KEY_DAILY_CLAIMED_STAGES, 0);

                // Reset daily quest progress
                ResetDailyQuests();

                PlayerPrefs.SetString(KEY_DAILY_RESET_DATE, today);
                PlayerPrefs.Save();
            }
        }

        private void ResetDailyQuests()
        {
            // Reset daily quest progress and claimed state
            string[] dailyQuestIds = { "Q006", "Q007" };
            foreach (string id in dailyQuestIds)
            {
                PlayerPrefs.SetInt($"quest_progress_{id}", 0);
                PlayerPrefs.SetInt($"quest_claimed_{id}", 0);
            }
        }
        #endregion

        #region ILobbyData Implementation
        public int Gold
        {
            get => PlayerPrefs.GetInt(KEY_GOLD, INITIAL_GOLD);
            set
            {
                PlayerPrefs.SetInt(KEY_GOLD, Mathf.Max(0, value));
                PlayerPrefs.Save();
                OnGoldChanged?.Invoke(value);
                OnDataChanged?.Invoke();
            }
        }

        public int Tickets
        {
            get => PlayerPrefs.GetInt(KEY_TICKETS, INITIAL_TICKETS);
            set
            {
                PlayerPrefs.SetInt(KEY_TICKETS, Mathf.Max(0, value));
                PlayerPrefs.Save();
                OnTicketsChanged?.Invoke(value);
                OnDataChanged?.Invoke();
            }
        }

        public List<string> UnlockedUnits
        {
            get
            {
                string json = PlayerPrefs.GetString(KEY_UNLOCKED_UNITS, "");
                if (string.IsNullOrEmpty(json))
                    return new List<string> { INITIAL_UNLOCKED_UNIT };

                var data = JsonUtility.FromJson<UnlockedUnitsData>(json);
                return data?.units ?? new List<string> { INITIAL_UNLOCKED_UNIT };
            }
        }

        public bool IsUnitUnlocked(string unitName)
        {
            return UnlockedUnits.Contains(unitName);
        }

        public void UnlockUnit(string unitName)
        {
            var units = UnlockedUnits;
            if (units.Contains(unitName)) return;

            units.Add(unitName);
            var data = new UnlockedUnitsData { units = units };
            PlayerPrefs.SetString(KEY_UNLOCKED_UNITS, JsonUtility.ToJson(data));
            PlayerPrefs.Save();

            OnUnitUnlocked?.Invoke(unitName);
            OnDataChanged?.Invoke();
            Debug.Log($"[LobbyDataManager] Unit unlocked: {unitName}");
        }

        public int TotalClears
        {
            get => PlayerPrefs.GetInt(KEY_TOTAL_CLEARS, 0);
            set
            {
                PlayerPrefs.SetInt(KEY_TOTAL_CLEARS, value);
                PlayerPrefs.Save();
                OnDataChanged?.Invoke();
            }
        }

        public int TotalSynthesis
        {
            get => PlayerPrefs.GetInt(KEY_TOTAL_SYNTHESIS, 0);
            set
            {
                PlayerPrefs.SetInt(KEY_TOTAL_SYNTHESIS, value);
                PlayerPrefs.Save();
                OnDataChanged?.Invoke();
            }
        }

        public int TotalUpgrades
        {
            get => PlayerPrefs.GetInt(KEY_TOTAL_UPGRADES, 0);
            set
            {
                PlayerPrefs.SetInt(KEY_TOTAL_UPGRADES, value);
                PlayerPrefs.Save();
                OnDataChanged?.Invoke();
            }
        }

        public int DailyClearCount
        {
            get => PlayerPrefs.GetInt(KEY_DAILY_CLEAR_COUNT, 0);
            set
            {
                PlayerPrefs.SetInt(KEY_DAILY_CLEAR_COUNT, value);
                PlayerPrefs.Save();
                OnDataChanged?.Invoke();
            }
        }

        public int DailyClaimedStages
        {
            get => PlayerPrefs.GetInt(KEY_DAILY_CLAIMED_STAGES, 0);
            set
            {
                PlayerPrefs.SetInt(KEY_DAILY_CLAIMED_STAGES, value);
                PlayerPrefs.Save();
                OnDataChanged?.Invoke();
            }
        }

        public string DailyResetDate
        {
            get => PlayerPrefs.GetString(KEY_DAILY_RESET_DATE, "");
            set
            {
                PlayerPrefs.SetString(KEY_DAILY_RESET_DATE, value);
                PlayerPrefs.Save();
            }
        }

        public int GetQuestProgress(string questId)
        {
            return PlayerPrefs.GetInt($"quest_progress_{questId}", 0);
        }

        public void SetQuestProgress(string questId, int value)
        {
            PlayerPrefs.SetInt($"quest_progress_{questId}", value);
            PlayerPrefs.Save();
            OnDataChanged?.Invoke();
        }

        public bool IsQuestClaimed(string questId)
        {
            return PlayerPrefs.GetInt($"quest_claimed_{questId}", 0) == 1;
        }

        public void SetQuestClaimed(string questId, bool claimed)
        {
            PlayerPrefs.SetInt($"quest_claimed_{questId}", claimed ? 1 : 0);
            PlayerPrefs.Save();
            OnDataChanged?.Invoke();
        }

        public bool IsMailRead(string mailId)
        {
            return PlayerPrefs.GetInt($"mail_read_{mailId}", 0) == 1;
        }

        public void SetMailRead(string mailId, bool read)
        {
            PlayerPrefs.SetInt($"mail_read_{mailId}", read ? 1 : 0);
            PlayerPrefs.Save();
            OnDataChanged?.Invoke();
        }

        public bool IsMailClaimed(string mailId)
        {
            return PlayerPrefs.GetInt($"mail_claimed_{mailId}", 0) == 1;
        }

        public void SetMailClaimed(string mailId, bool claimed)
        {
            PlayerPrefs.SetInt($"mail_claimed_{mailId}", claimed ? 1 : 0);
            PlayerPrefs.Save();
            OnDataChanged?.Invoke();
        }
        #endregion

        #region Cross-Scene Static Methods
        /// <summary>
        /// Grant rewards after a game ends. Called from GameScene before cleanup.
        /// Writes directly to PlayerPrefs since LobbyDataManager instance may not exist.
        /// </summary>
        public static void GrantGameRewards(int roundReached, int synthesisCount, int upgradeCount)
        {
            // Calculate gold reward based on round reached
            int goldReward = GetGameResultGold(roundReached);

            // Add gold
            int currentGold = PlayerPrefs.GetInt(KEY_GOLD, 0);
            PlayerPrefs.SetInt(KEY_GOLD, currentGold + goldReward);

            // Increment total clears
            int totalClears = PlayerPrefs.GetInt(KEY_TOTAL_CLEARS, 0) + 1;
            PlayerPrefs.SetInt(KEY_TOTAL_CLEARS, totalClears);

            // Increment daily clear count
            int dailyClears = PlayerPrefs.GetInt(KEY_DAILY_CLEAR_COUNT, 0) + 1;
            PlayerPrefs.SetInt(KEY_DAILY_CLEAR_COUNT, dailyClears);

            // Increment cumulative synthesis/upgrade counts
            int totalSynthesis = PlayerPrefs.GetInt(KEY_TOTAL_SYNTHESIS, 0) + synthesisCount;
            PlayerPrefs.SetInt(KEY_TOTAL_SYNTHESIS, totalSynthesis);

            int totalUpgrades = PlayerPrefs.GetInt(KEY_TOTAL_UPGRADES, 0) + upgradeCount;
            PlayerPrefs.SetInt(KEY_TOTAL_UPGRADES, totalUpgrades);

            PlayerPrefs.Save();

            Debug.Log($"[LobbyDataManager] Game rewards granted - Gold: +{goldReward}, Clears: {totalClears}, DailyClears: {dailyClears}, Synth: +{synthesisCount}, Upgrades: +{upgradeCount}");
        }

        /// <summary>
        /// Calculate gold reward for a game result based on round reached.
        /// </summary>
        public static int GetGameResultGold(int roundReached)
        {
            if (roundReached >= 30) return 100;
            if (roundReached >= 20) return 60;
            if (roundReached >= 10) return 30;
            return 10;
        }

        /// <summary>
        /// Get unlocked units list directly from PlayerPrefs (for use in GameScene).
        /// </summary>
        public static List<string> GetUnlockedUnitsStatic()
        {
            string json = PlayerPrefs.GetString(KEY_UNLOCKED_UNITS, "");
            if (string.IsNullOrEmpty(json))
                return new List<string> { INITIAL_UNLOCKED_UNIT };

            var data = JsonUtility.FromJson<UnlockedUnitsData>(json);
            return data?.units ?? new List<string> { INITIAL_UNLOCKED_UNIT };
        }
        #endregion

        #region Serialization Helper
        [Serializable]
        private class UnlockedUnitsData
        {
            public List<string> units;
        }
        #endregion
    }
}
