using UnityEngine;
using System;
using System.Collections;
using LottoDefense.Monsters;
using LottoDefense.UI;
using LottoDefense.Networking;

namespace LottoDefense.Gameplay
{
    /// <summary>
    /// Game phase during a round.
    /// </summary>
    public enum GamePhase
    {
        Preparation,
        Combat
    }

    /// <summary>
    /// Singleton manager orchestrating round progression, phase transitions, and difficulty scaling.
    /// Manages preparation/combat timers and coordinates with MonsterManager and GameplayManager.
    /// </summary>
    public class RoundManager : MonoBehaviour
    {
        #region Singleton
        private static RoundManager _instance;

        /// <summary>
        /// Global access point for the RoundManager singleton.
        /// </summary>
        public static RoundManager Instance
        {
            get
            {
                if (GameplayManager.IsCleaningUp) return null;

                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<RoundManager>();

                    if (_instance == null)
                    {
                        GameObject go = new GameObject("RoundManager");
                        _instance = go.AddComponent<RoundManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Inspector Fields
        [Header("Configuration")]
        [Tooltip("라운드별 몬스터 설정 (RoundConfig 사용 권장)")]
        [SerializeField] private RoundConfig roundConfig;

        [Tooltip("Difficulty scaling configuration (Fallback)")]
        [SerializeField] private DifficultyConfig difficultyConfig;

        [Header("Phase Durations")]
        [Tooltip("Preparation phase duration in seconds")]
        [SerializeField] private float preparationDuration = 15f;

        [Tooltip("Combat phase duration in seconds (spawn 15s, then 30s total before next round)")]
        [SerializeField] private float combatDuration = 30f;

        [Header("Life Loss Settings")]
        [Tooltip("Life lost per monster remaining when combat time expires")]
        [SerializeField] private int lifePerMonster = 1;
        #endregion

        #region Properties
        /// <summary>
        /// Current game phase.
        /// </summary>
        public GamePhase CurrentPhase { get; private set; }

        /// <summary>
        /// Remaining time in current phase.
        /// </summary>
        public float RemainingTime { get; private set; }

        /// <summary>
        /// Current round number (1-based).
        /// </summary>
        public int CurrentRound => GameplayManager.Instance != null ? GameplayManager.Instance.CurrentRound : 1;

        /// <summary>
        /// Maximum number of rounds.
        /// RoundConfig 우선, 없으면 DifficultyConfig 사용.
        /// </summary>
        public int MaxRounds
        {
            get
            {
                if (roundConfig != null)
                    return roundConfig.TotalRounds;
                if (difficultyConfig != null)
                    return difficultyConfig.MaxRounds;
                return 10; // Fallback default
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// Fired when phase changes.
        /// Parameters: oldPhase, newPhase
        /// </summary>
        public event Action<GamePhase, GamePhase> OnPhaseChanged;

        /// <summary>
        /// Fired when timer updates (useful for UI).
        /// Parameter: remainingTime
        /// </summary>
        public event Action<float> OnTimerUpdated;

        /// <summary>
        /// Fired when a round starts.
        /// Parameter: roundNumber
        /// </summary>
        public event Action<int> OnRoundStarted;

        /// <summary>
        /// Fired when a round completes successfully.
        /// Parameter: roundNumber
        /// </summary>
        public event Action<int> OnRoundCompleted;
        #endregion

        #region Private Fields
        private Coroutine phaseTimerCoroutine;
        private bool isInitialized = false;
        private bool waitingForWaveSync = false;
        private GameHUD cachedGameHUD;
        private RoundStartUI cachedRoundStartUI;
        #endregion

        #region Unity Lifecycle
        private Coroutine subscribeCoroutine;

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
        }

        private void OnEnable()
        {
            subscribeCoroutine = StartCoroutine(SubscribeWhenReady());
        }

        private IEnumerator SubscribeWhenReady()
        {
            int maxRetries = 300;
            int retries = 0;
            while (GameplayManager.Instance == null && retries < maxRetries)
            {
                retries++;
                yield return null;
            }
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnStateChanged -= HandleGameStateChanged;
                GameplayManager.Instance.OnStateChanged += HandleGameStateChanged;
            }
            else
            {
                Debug.LogError("[RoundManager] Failed to subscribe - GameplayManager not found after 300 frames!");
            }

            // Subscribe to MonsterManager events
            retries = 0;
            while (MonsterManager.Instance == null && retries < maxRetries)
            {
                retries++;
                yield return null;
            }
            if (MonsterManager.Instance != null)
            {
                MonsterManager.Instance.OnRoundComplete -= HandleMonsterRoundComplete;
                MonsterManager.Instance.OnRoundComplete += HandleMonsterRoundComplete;
            }

            // Subscribe to multiplayer wave sync
            if (MultiplayerManager.Instance != null)
            {
                MultiplayerManager.Instance.OnWaveSync -= HandleWaveSync;
                MultiplayerManager.Instance.OnWaveSync += HandleWaveSync;
            }
        }

        private void OnDisable()
        {
            if (subscribeCoroutine != null)
            {
                StopCoroutine(subscribeCoroutine);
                subscribeCoroutine = null;
            }

            // Unsubscribe from GameplayManager
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnStateChanged -= HandleGameStateChanged;
            }

            // Unsubscribe from MonsterManager
            if (MonsterManager.Instance != null)
            {
                MonsterManager.Instance.OnRoundComplete -= HandleMonsterRoundComplete;
            }

            // Unsubscribe from multiplayer wave sync
            if (MultiplayerManager.Instance != null)
            {
                MultiplayerManager.Instance.OnWaveSync -= HandleWaveSync;
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize RoundManager with default configuration.
        /// </summary>
        private void Initialize()
        {
            // Configuration validated (uses defaults if null)
            isInitialized = true;
        }
        #endregion

        #region State Management
        /// <summary>
        /// Handle GameplayManager state changes.
        /// </summary>
        private void HandleGameStateChanged(GameState oldState, GameState newState)
        {

            switch (newState)
            {
                case GameState.Preparation:
                    StartPreparationPhase();
                    break;

                case GameState.Combat:
                    StartCombatPhase();
                    break;

                case GameState.RoundResult:
                case GameState.Victory:
                case GameState.Defeat:
                    StopPhaseTimer();
                    break;
            }
        }
        #endregion

        #region Phase Management
        /// <summary>
        /// Start the preparation phase.
        /// </summary>
        public void StartPreparationPhase()
        {
            if (!isInitialized)
            {
                Debug.LogError("[RoundManager] Not initialized!");
                return;
            }

            GamePhase oldPhase = CurrentPhase;
            CurrentPhase = GamePhase.Preparation;
            RemainingTime = preparationDuration;


            // Show round start notification
            ShowRoundStartNotification();

            // Fire events
            OnPhaseChanged?.Invoke(oldPhase, CurrentPhase);
            OnRoundStarted?.Invoke(CurrentRound);

            // Start timer
            StartPhaseTimer(preparationDuration, OnPreparationPhaseComplete);

            // Update HUD
            UpdateGameHUD();
        }

        /// <summary>
        /// Start the combat phase.
        /// </summary>
        public void StartCombatPhase()
        {
            if (!isInitialized)
            {
                Debug.LogError("[RoundManager] Not initialized!");
                return;
            }

            GamePhase oldPhase = CurrentPhase;
            CurrentPhase = GamePhase.Combat;
            RemainingTime = combatDuration;


            // Safety: ensure CombatManager starts combat even if it missed the state change event
            if (LottoDefense.Combat.CombatManager.Instance != null && !LottoDefense.Combat.CombatManager.Instance.IsCombatActive)
            {
                Debug.LogWarning("[RoundManager] CombatManager was not active! Starting combat as safety net.");
                LottoDefense.Combat.CombatManager.Instance.StartCombat();
            }

            // Safety: ensure MonsterManager starts spawning even if it missed the state change event
            if (MonsterManager.Instance != null && !MonsterManager.Instance.IsSpawning)
            {
                Debug.LogWarning("[RoundManager] MonsterManager was not spawning! Starting as safety net.");
                MonsterManager.Instance.StartSpawning();
            }

            // Fire events
            OnPhaseChanged?.Invoke(oldPhase, CurrentPhase);

            // Start timer
            StartPhaseTimer(combatDuration, OnCombatPhaseComplete);

            // Update HUD
            UpdateGameHUD();
        }

        /// <summary>
        /// Start phase timer coroutine.
        /// </summary>
        private void StartPhaseTimer(float duration, Action onComplete)
        {
            StopPhaseTimer();
            phaseTimerCoroutine = StartCoroutine(PhaseTimerRoutine(duration, onComplete));
        }

        /// <summary>
        /// Stop current phase timer.
        /// </summary>
        private void StopPhaseTimer()
        {
            if (phaseTimerCoroutine != null)
            {
                StopCoroutine(phaseTimerCoroutine);
                phaseTimerCoroutine = null;
            }
        }

        /// <summary>
        /// Phase timer countdown coroutine.
        /// </summary>
        private IEnumerator PhaseTimerRoutine(float duration, Action onComplete)
        {
            RemainingTime = duration;

            int frameCount = 0;
            while (RemainingTime > 0f)
            {
                yield return null;
                frameCount++;

                if (frameCount == 1)
                {
                }

                RemainingTime -= Time.deltaTime;
                RemainingTime = Mathf.Max(0f, RemainingTime);

                // Update HUD every frame
                UpdateGameHUDTime();

                // Fire timer update event
                OnTimerUpdated?.Invoke(RemainingTime);
            }

            onComplete?.Invoke();
        }
        #endregion

        #region Phase Callbacks
        /// <summary>
        /// Called when preparation phase completes.
        /// </summary>
        private void OnPreparationPhaseComplete()
        {

            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.ChangeState(GameState.Combat);
            }
        }

        /// <summary>
        /// Called when combat phase time expires.
        /// </summary>
        private void OnCombatPhaseComplete()
        {

            // Check remaining monsters and apply life loss
            int remainingMonsters = MonsterManager.Instance != null ? MonsterManager.Instance.ActiveMonsterCount : 0;

            if (remainingMonsters > 0)
            {
                int lifeLoss = remainingMonsters * lifePerMonster;
                Debug.LogWarning($"[RoundManager] {remainingMonsters} monsters remain! Losing {lifeLoss} life");

                if (GameplayManager.Instance != null)
                {
                    GameplayManager.Instance.ModifyLife(-lifeLoss);
                }

                // Clear remaining monsters
                if (MonsterManager.Instance != null)
                {
                    MonsterManager.Instance.ClearAllMonsters();
                }
            }

            // Transition to round result
            EndRound(remainingMonsters == 0);
        }

        /// <summary>
        /// Called when server sends wave sync signal in multiplayer mode.
        /// </summary>
        private void HandleWaveSync(int round)
        {
            waitingForWaveSync = false;
        }

        /// <summary>
        /// Called when MonsterManager reports all monsters are cleared.
        /// </summary>
        private void HandleMonsterRoundComplete()
        {

            // Stop combat timer early
            StopPhaseTimer();

            // Complete round successfully
            EndRound(true);
        }
        #endregion

        #region Round Completion
        /// <summary>
        /// End the current round and check for victory/next round.
        /// </summary>
        /// <param name="success">Whether all monsters were defeated</param>
        private void EndRound(bool success)
        {
            int completedRound = CurrentRound;


            // Fire round completed event
            OnRoundCompleted?.Invoke(completedRound);

            // Check for victory condition
            if (completedRound >= MaxRounds)
            {
                if (GameplayManager.Instance != null)
                {
                    GameplayManager.Instance.ChangeState(GameState.Victory);
                }
                return;
            }

            // Check if player is still alive
            if (GameplayManager.Instance != null && GameplayManager.Instance.CurrentLife <= 0)
            {
                GameplayManager.Instance.ChangeState(GameState.Defeat);
                return;
            }

            // Advance to next round
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.NextRound();
            }

            // Transition to RoundResult state briefly, then back to Preparation
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.ChangeState(GameState.RoundResult);
                StartCoroutine(TransitionToNextRound());
            }
        }

        /// <summary>
        /// Brief pause before starting next round preparation.
        /// In multiplayer mode, waits for server wave_sync signal.
        /// </summary>
        private IEnumerator TransitionToNextRound()
        {
            yield return new WaitForSeconds(2f); // 2 second pause

            // In multiplayer mode, wait for wave sync from server
            if (MultiplayerManager.Instance != null && MultiplayerManager.Instance.IsMultiplayer)
            {
                waitingForWaveSync = true;

                float waveSyncTimeout = 30f;
                float elapsed = 0f;
                while (waitingForWaveSync && elapsed < waveSyncTimeout)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                if (waitingForWaveSync)
                {
                    Debug.LogWarning("[RoundManager] Wave sync timeout - proceeding anyway");
                    waitingForWaveSync = false;
                }
            }

            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.ChangeState(GameState.Preparation);
            }
        }
        #endregion

        #region Difficulty Scaling
        /// <summary>
        /// Get difficulty multipliers for the current round.
        /// </summary>
        public DifficultyMultipliers GetCurrentDifficultyMultipliers()
        {
            if (difficultyConfig == null)
            {
                return new DifficultyMultipliers
                {
                    hpMultiplier = 1f,
                    defenseMultiplier = 1f
                };
            }

            return difficultyConfig.GetMultipliersForRound(CurrentRound);
        }
        #endregion

        #region UI Integration
        /// <summary>
        /// Get cached GameHUD reference (avoids expensive FindFirstObjectByType each frame).
        /// </summary>
        private GameHUD GetGameHUD()
        {
            if (cachedGameHUD == null)
            {
                cachedGameHUD = FindFirstObjectByType<GameHUD>();
            }
            return cachedGameHUD;
        }

        /// <summary>
        /// Update GameHUD with current values.
        /// </summary>
        private void UpdateGameHUD()
        {
            GameHUD hud = GetGameHUD();
            if (hud != null)
            {
                hud.UpdatePhase(GetPhaseDisplayName());
                hud.UpdateTime(RemainingTime);
            }
        }

        /// <summary>
        /// Update only the time display on GameHUD.
        /// </summary>
        private void UpdateGameHUDTime()
        {
            GameHUD hud = GetGameHUD();
            if (hud != null)
            {
                hud.UpdateTime(RemainingTime);
            }
        }

        /// <summary>
        /// Show round start notification UI.
        /// </summary>
        private void ShowRoundStartNotification()
        {
            // Cache RoundStartUI reference (it may be inactive, so FindObjectsOfType with includeInactive)
            if (cachedRoundStartUI == null)
            {
                // Find including inactive objects
                RoundStartUI[] allRoundStartUIs = FindObjectsByType<RoundStartUI>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );
                if (allRoundStartUIs.Length > 0)
                {
                    cachedRoundStartUI = allRoundStartUIs[0];
                }
            }

            if (cachedRoundStartUI != null)
            {
                cachedRoundStartUI.ShowRoundStart(CurrentRound);
            }
            else
            {
                Debug.LogWarning("[RoundManager] RoundStartUI not found!");
            }
        }
        #endregion

        #region Public API
        /// <summary>
        /// Get phase name as a string for UI display.
        /// </summary>
        public string GetPhaseDisplayName()
        {
            return CurrentPhase == GamePhase.Preparation ? "PREPARATION" : "COMBAT";
        }

        /// <summary>
        /// Get formatted time string (MM:SS).
        /// </summary>
        public string GetFormattedTime()
        {
            int minutes = Mathf.FloorToInt(RemainingTime / 60f);
            int seconds = Mathf.FloorToInt(RemainingTime % 60f);
            return $"{minutes:00}:{seconds:00}";
        }

        /// <summary>
        /// Check if currently in preparation phase.
        /// </summary>
        public bool IsPreparationPhase()
        {
            return CurrentPhase == GamePhase.Preparation;
        }

        /// <summary>
        /// Check if currently in combat phase.
        /// </summary>
        public bool IsCombatPhase()
        {
            return CurrentPhase == GamePhase.Combat;
        }
        #endregion

        #region Debugging
        /// <summary>
        /// Get current round manager statistics.
        /// </summary>
        public string GetStats()
        {
            return $"Round: {CurrentRound}/{MaxRounds}, Phase: {CurrentPhase}, Time: {GetFormattedTime()}";
        }
        #endregion
    }
}
