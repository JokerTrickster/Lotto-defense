using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using LottoDefense.Combat;
using LottoDefense.Grid;
using LottoDefense.Monsters;
using LottoDefense.Units;
using LottoDefense.VFX;
using LottoDefense.Quests;
using LottoDefense.Networking;
using LottoDefense.Backend;
using LottoDefense.Backend.Models;

namespace LottoDefense.Gameplay
{
    /// <summary>
    /// Singleton manager that orchestrates the core gameplay flow and state transitions.
    /// Manages game values (round, life, gold) and notifies other systems of state changes.
    /// </summary>
    public class GameplayManager : MonoBehaviour
    {
        #region Singleton
        private static GameplayManager _instance;
        private static bool _isCleaningUp;

        /// <summary>
        /// True while CleanupAllGameplaySingletons is running.
        /// All singletons should check this in their Instance getter to prevent auto-creation.
        /// </summary>
        public static bool IsCleaningUp => _isCleaningUp;

        /// <summary>
        /// Global access point for the GameplayManager singleton.
        /// Returns null during cleanup to prevent auto-creation cascade from OnDisable callbacks.
        /// </summary>
        public static GameplayManager Instance
        {
            get
            {
                if (_isCleaningUp) return null;

                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<GameplayManager>();

                    if (_instance == null)
                    {
                        GameObject go = new GameObject("GameplayManager");
                        _instance = go.AddComponent<GameplayManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Constants
        private const int INITIAL_ROUND = 1;
        private const int INITIAL_LIFE = 10;
        #endregion

        #region Properties
        /// <summary>
        /// Current game state.
        /// </summary>
        public GameState CurrentState { get; private set; }

        /// <summary>
        /// Current round number (starts at 1).
        /// </summary>
        public int CurrentRound { get; private set; }

        /// <summary>
        /// Current player life points.
        /// </summary>
        public int CurrentLife { get; private set; }

        /// <summary>
        /// Current player gold amount.
        /// </summary>
        public int CurrentGold { get; private set; }
        #endregion

        #region Events
        /// <summary>
        /// Fired when game state changes.
        /// </summary>
        public event Action<GameState, GameState> OnStateChanged;

        /// <summary>
        /// Fired when a game value (round, life, gold) changes.
        /// Parameters: valueType (Round/Life/Gold), newValue
        /// </summary>
        public event Action<string, int> OnGameValueChanged;
        #endregion

        #region Auto-Initialization
        /// <summary>
        /// Registers a scene-loaded callback at app startup so GameplayManager
        /// auto-creates itself whenever GameScene loads (regardless of how you get there).
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneLoadHandler()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "GameScene" && _instance == null && !_isCleaningUp)
            {
                var _ = Instance;
            }
        }
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
            DontDestroyOnLoad(gameObject);

            Initialize();

            // Create bootstrapper immediately in Awake (not Start) to avoid
            // timing issues with DontDestroyOnLoad objects during sceneLoaded callbacks
            EnsureGameSystemsBootstrapped();
        }

        private void Start()
        {
            // Start countdown (bootstrapper already created in Awake)
            StartCoroutine(StartCountdownDelayed());
        }

        private System.Collections.IEnumerator StartCountdownDelayed()
        {
            // Wait two frames for all managers to fully initialize
            yield return null;
            yield return null;

            // Start countdown if still in Countdown state
            if (CurrentState == GameState.Countdown)
            {
                StartCountdown();
            }
        }

        /// <summary>
        /// Ensures all required game systems exist and are properly configured.
        /// Creates GameSceneBootstrapper if it doesn't exist.
        /// </summary>
        private void EnsureGameSystemsBootstrapped()
        {

            // Check if bootstrapper already exists
            GameSceneBootstrapper bootstrapper = FindFirstObjectByType<GameSceneBootstrapper>();

            if (bootstrapper == null)
            {
                GameObject bootstrapperObj = new GameObject("GameSceneBootstrapper");

                var component = bootstrapperObj.AddComponent<GameSceneBootstrapper>();
            }
            else
            {
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize game with starting values from GameBalanceConfig.
        /// </summary>
        public void Initialize()
        {
            CurrentState = GameState.Countdown;
            CurrentRound = INITIAL_ROUND;
            CurrentLife = INITIAL_LIFE;

            // Load starting gold from GameBalanceConfig (single source of truth)
            GameBalanceConfig balanceConfig = Resources.Load<GameBalanceConfig>("GameBalanceConfig");
            int startingGold = balanceConfig != null ? balanceConfig.gameRules.startingGold : 30;
            CurrentGold = startingGold;

        }
        #endregion

        #region State Management
        /// <summary>
        /// Change to a new game state with validation and event notifications.
        /// </summary>
        /// <param name="newState">Target game state</param>
        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState)
            {
                Debug.LogWarning($"[GameplayManager] Already in state: {newState}");
                return;
            }

            // Validate state transitions
            if (!IsValidTransition(CurrentState, newState))
            {
                Debug.LogError($"[GameplayManager] Invalid transition: {CurrentState} -> {newState}");
                return;
            }

            GameState oldState = CurrentState;
            CurrentState = newState;

            // Send game result to backend if game ended
            if (newState == GameState.Victory || newState == GameState.Defeat)
            {
                SendGameResultToBackend(newState == GameState.Victory);
            }

            OnStateChanged?.Invoke(oldState, newState);
        }

        /// <summary>
        /// Validates whether a state transition is allowed.
        /// </summary>
        private bool IsValidTransition(GameState from, GameState to)
        {
            // Any state can transition to Defeat
            if (to == GameState.Defeat)
                return true;

            switch (from)
            {
                case GameState.Countdown:
                    return to == GameState.Preparation;

                case GameState.Preparation:
                    return to == GameState.Combat;

                case GameState.Combat:
                    return to == GameState.RoundResult || to == GameState.Victory;

                case GameState.RoundResult:
                    return to == GameState.Preparation || to == GameState.Victory;

                case GameState.Victory:
                case GameState.Defeat:
                    return false; // Terminal states

                default:
                    return false;
            }
        }
        #endregion

        #region Countdown
        /// <summary>
        /// Start the countdown animation sequence.
        /// </summary>
        public void StartCountdown()
        {

            // CountdownUI will handle the animation and notify when complete
            CountdownUI countdownUI = FindFirstObjectByType<CountdownUI>();

            if (countdownUI != null)
            {
                countdownUI.StartCountdown(OnCountdownComplete);
            }
            else
            {
                Debug.LogError("[GameplayManager] CountdownUI not found in scene!");
                // Fallback: directly transition to Preparation
                OnCountdownComplete();
            }
        }

        /// <summary>
        /// Called when countdown animation completes.
        /// </summary>
        private void OnCountdownComplete()
        {
            ChangeState(GameState.Preparation);
        }
        #endregion

        #region Game Value Management
        /// <summary>
        /// Update the current round number.
        /// </summary>
        public void SetRound(int round)
        {
            if (round < 1)
            {
                Debug.LogError($"[GameplayManager] Invalid round value: {round}");
                return;
            }

            CurrentRound = round;
            OnGameValueChanged?.Invoke("Round", CurrentRound);
        }

        /// <summary>
        /// Update the current life points.
        /// </summary>
        public void SetLife(int life)
        {
            CurrentLife = Mathf.Max(0, life);
            OnGameValueChanged?.Invoke("Life", CurrentLife);

            // Automatic defeat when life reaches 0
            if (CurrentLife <= 0 && CurrentState != GameState.Defeat)
            {
                ChangeState(GameState.Defeat);
            }
        }

        /// <summary>
        /// Modify life by a delta amount (positive or negative).
        /// </summary>
        public void ModifyLife(int delta)
        {
            SetLife(CurrentLife + delta);
        }

        /// <summary>
        /// Update the current gold amount.
        /// </summary>
        public void SetGold(int gold)
        {
            CurrentGold = Mathf.Max(0, gold);
            OnGameValueChanged?.Invoke("Gold", CurrentGold);
        }

        /// <summary>
        /// Modify gold by a delta amount (positive or negative).
        /// </summary>
        public void ModifyGold(int delta)
        {
            SetGold(CurrentGold + delta);
        }

        /// <summary>
        /// Advance to the next round.
        /// </summary>
        public void NextRound()
        {
            SetRound(CurrentRound + 1);
        }
        #endregion

        #region Backend Integration
        /// <summary>
        /// Sends game result to backend server (if logged in).
        /// </summary>
        private void SendGameResultToBackend(bool isVictory)
        {
            // Skip if not logged in
            if (!APIManager.Instance.IsLoggedIn)
            {
                Debug.Log("[GameplayManager] Not logged in, skipping result upload");
                return;
            }

            // Calculate stats
            int roundsReached = CurrentRound;
            int monstersKilled = MonsterManager.Instance != null ? MonsterManager.Instance.TotalMonstersKilled : 0;
            int goldEarned = CurrentGold; // Approximate, could track separately
            string result = isVictory ? "victory" : "defeat";

            Debug.Log($"[GameplayManager] Sending result: Round {roundsReached}, Kills {monstersKilled}, Gold {goldEarned}, Result {result}");

            APIManager.Instance.SaveGameResult(
                roundsReached,
                monstersKilled,
                goldEarned,
                result,
                (GameResultResponse response) =>
                {
                    Debug.Log($"[GameplayManager] Result saved! Game ID: {response.game_id}, New highest round: {response.new_highest_round}");
                },
                (string error) =>
                {
                    Debug.LogError($"[GameplayManager] Failed to save result: {error}");
                }
            );
        }
        #endregion

        #region Cleanup
        /// <summary>
        /// Destroys all gameplay-scoped singletons created during a game session.
        /// Call before returning to main menu to prevent DontDestroyOnLoad objects
        /// from persisting and covering the menu UI.
        /// Uses FindFirstObjectByType directly to avoid auto-creation via Instance getters.
        /// </summary>
        public static void CleanupAllGameplaySingletons()
        {
            _isCleaningUp = true;

            DestroyIfExists<LottoDefense.Grid.GridManager>();
            DestroyIfExists<MonsterManager>();
            DestroyIfExists<RoundManager>();
            DestroyIfExists<CombatManager>();
            DestroyIfExists<UnitManager>();
            DestroyIfExists<UnitPlacementManager>();
            DestroyIfExists<SynthesisManager>();
            DestroyIfExists<AutoSynthesisManager>();
            DestroyIfExists<UnitUpgradeManager>();
            DestroyIfExists<VFXManager>();
            DestroyIfExists<MobileOptimizationManager>();
            DestroyIfExists<QuestManager>();
            DestroyIfExists<MultiplayerManager>();
            DestroyIfExists<GameSceneBootstrapper>();

            // Destroy GameCanvas if it exists (created by bootstrapper with sortingOrder 100)
            GameObject gameCanvas = GameObject.Find("GameCanvas");
            if (gameCanvas != null)
            {
                Destroy(gameCanvas);
            }

            // Destroy self last
            if (_instance != null)
            {
                Destroy(_instance.gameObject);
                _instance = null;
            }

            _isCleaningUp = false;

        }

        private static void DestroyIfExists<T>() where T : MonoBehaviour
        {
            T obj = UnityEngine.Object.FindFirstObjectByType<T>();
            if (obj != null)
            {
                Destroy(obj.gameObject);
            }
        }
        #endregion
    }
}
