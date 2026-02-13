using UnityEngine;
using System;
using System.Collections.Generic;

namespace LottoDefense.Lobby
{
    public class LobbyQuestManager : MonoBehaviour
    {
        #region Events
        public event Action OnQuestClaimed;
        #endregion

        #region Quest Logic
        public int GetProgress(LobbyQuestDefinition quest)
        {
            if (LobbyDataManager.Instance == null) return 0;

            // For daily quests, use quest-specific progress stored in PlayerPrefs
            if (quest.isDaily)
            {
                return LobbyDataManager.Instance.GetQuestProgress(quest.id);
            }

            // For permanent quests, map to cumulative stats
            return quest.type switch
            {
                LobbyQuestType.GameClear => LobbyDataManager.Instance.TotalClears,
                LobbyQuestType.Synthesis => LobbyDataManager.Instance.TotalSynthesis,
                LobbyQuestType.Upgrade => LobbyDataManager.Instance.TotalUpgrades,
                LobbyQuestType.UnitUnlock => LobbyDataManager.Instance.UnlockedUnits.Count,
                _ => 0
            };
        }

        public bool IsCompleted(LobbyQuestDefinition quest)
        {
            return GetProgress(quest) >= quest.targetCount;
        }

        public bool IsClaimed(LobbyQuestDefinition quest)
        {
            if (LobbyDataManager.Instance == null) return false;
            return LobbyDataManager.Instance.IsQuestClaimed(quest.id);
        }

        public bool CanClaim(LobbyQuestDefinition quest)
        {
            return IsCompleted(quest) && !IsClaimed(quest);
        }

        public bool TryClaimReward(LobbyQuestDefinition quest)
        {
            if (!CanClaim(quest)) return false;
            if (LobbyDataManager.Instance == null) return false;

            // Grant rewards
            foreach (var reward in quest.rewards)
            {
                switch (reward.itemType)
                {
                    case "gold":
                        LobbyDataManager.Instance.Gold += reward.amount;
                        break;
                    case "ticket":
                        LobbyDataManager.Instance.Tickets += reward.amount;
                        break;
                }
            }

            // Mark as claimed
            LobbyDataManager.Instance.SetQuestClaimed(quest.id, true);

            OnQuestClaimed?.Invoke();
            Debug.Log($"[LobbyQuestManager] Quest '{quest.title}' reward claimed");
            return true;
        }

        public bool HasClaimableQuests()
        {
            return GetClaimableQuestCount() > 0;
        }

        public int GetClaimableQuestCount()
        {
            int count = 0;
            foreach (var quest in LobbyQuestConfig.GetAllQuests())
            {
                if (CanClaim(quest)) count++;
            }
            return count;
        }

        /// <summary>
        /// Update daily quest progress from session data.
        /// Called when returning from GameScene.
        /// Daily quests track within the same day using PlayerPrefs.
        /// </summary>
        public void RefreshDailyQuestProgress()
        {
            if (LobbyDataManager.Instance == null) return;

            foreach (var quest in LobbyQuestConfig.GetAllQuests())
            {
                if (!quest.isDaily) continue;

                int progress = quest.type switch
                {
                    LobbyQuestType.GameClear => LobbyDataManager.Instance.DailyClearCount,
                    LobbyQuestType.Upgrade => PlayerPrefs.GetInt($"quest_progress_{quest.id}", 0),
                    _ => PlayerPrefs.GetInt($"quest_progress_{quest.id}", 0)
                };

                // For daily game clear quests, sync from daily clear count
                if (quest.type == LobbyQuestType.GameClear)
                {
                    LobbyDataManager.Instance.SetQuestProgress(quest.id, progress);
                }
            }
        }
        #endregion
    }
}
