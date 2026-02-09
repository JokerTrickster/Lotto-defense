using UnityEngine;
using System;
using System.Collections;
using LottoDefense.Monsters;
using LottoDefense.UI;

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
        [Tooltip("Difficulty scaling configuration")]
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
        /// </summary>
        public int MaxRounds => difficultyConfig != null ? difficultyConfig.MaxRounds : 30;
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
        private GameHUD cachedGameHUD;
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
        }

        private void OnEnable()
        {
            // Subscribe to GameplayManager state changes
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnStateChanged += HandleGameStateChanged;
            }

            // Subscribe to MonsterManager events
            if (MonsterManager.Instance != null)
            {
                MonsterManager.Instance.OnRoundComplete += HandleMonsterRoundComplete;
            }
        }

        private void OnDisable()
        {
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
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize RoundManager with default configuration.
        /// </summary>
        private void Initialize()
        {
            // Validate configuration
            if (difficultyConfig == null)
            {
                Debug.LogWarning("[RoundManager] No DifficultyConfig assigned! Using default values.");
            }

            isInitialized = true;
            Debug.Log("[RoundManager] Initialized");
        }
        #endregion

        #region State Management
        /// <summary>
        /// Handle GameplayManager state changes.
        /// </summary>
        private void HandleGameStateChanged(GameState oldState, GameState newState)
        {
            Debug.Log($"[RoundManager] GameState changed: {oldState} -> {newState}");

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

            Debug.Log($"[RoundManager] Starting Preparation Phase - Round {CurrentRound}, Duration: {preparationDuration}s");

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

            Debug.Log($"[RoundManager] Starting Combat Phase - Round {CurrentRound}, Duration: {combatDuration}s");

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
            Debug.Log($"[RoundManager] PhaseTimerRoutine started - duration: {duration}s, phase: {CurrentPhase}");
            RemainingTime = duration;

            int frameCount = 0;
            while (RemainingTime > 0f)
            {
                yield return null;
                frameCount++;

                if (frameCount == 1)
                {
                    Debug.Log($"[RoundManager] PhaseTimerRoutine first frame - Time.deltaTime: {Time.deltaTime}, Time.timeScale: {Time.timeScale}");
                }

                RemainingTime -= Time.deltaTime;
                RemainingTime = Mathf.Max(0f, RemainingTime);

                // Update HUD every frame
                UpdateGameHUDTime();

                // Fire timer update event
                OnTimerUpdated?.Invoke(RemainingTime);
            }

            Debug.Log($"[RoundManager] Phase timer expired for {CurrentPhase} after {frameCount} frames");
            onComplete?.Invoke();
        }
        #endregion

        #region Phase Callbacks
        /// <summary>
        /// Called when preparation phase completes.
        /// </summary>
        private void OnPreparationPhaseComplete()
        {
            Debug.Log("[RoundManager] Preparation phase complete - transitioning to Combat");

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
            Debug.Log("[RoundManager] Combat phase time expired");

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
        /// Called when MonsterManager reports all monsters are cleared.
        /// </summary>
        private void HandleMonsterRoundComplete()
        {
            Debug.Log("[RoundManager] All monsters cleared before time expired");

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

            Debug.Log($"[RoundManager] Round {completedRound} ended - Success: {success}");

            // Fire round completed event
            OnRoundCompleted?.Invoke(completedRound);

            // Check for victory condition
            if (completedRound >= MaxRounds)
            {
                Debug.Log("[RoundManager] All rounds complete - VICTORY!");
                if (GameplayManager.Instance != null)
                {
                    GameplayManager.Instance.ChangeState(GameState.Victory);
                }
                return;
            }

            // Check if player is still alive
            if (GameplayManager.Instance != null && GameplayManager.Instance.CurrentLife <= 0)
            {
                Debug.Log("[RoundManager] Player life depleted - DEFEAT!");
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
        /// </summary>
        private IEnumerator TransitionToNextRound()
        {
            yield return new WaitForSeconds(2f); // 2 second pause

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
            RoundStartUI roundStartUI = FindFirstObjectByType<RoundStartUI>();
            if (roundStartUI != null)
            {
                roundStartUI.ShowRoundStart(CurrentRound);
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
