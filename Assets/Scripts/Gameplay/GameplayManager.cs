using UnityEngine;
using System;

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

        /// <summary>
        /// Global access point for the GameplayManager singleton.
        /// </summary>
        public static GameplayManager Instance
        {
            get
            {
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
        private const int INITIAL_GOLD = 50;
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

        private void Start()
        {
            // Automatically start countdown when scene loads
            StartCountdown();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize game with starting values.
        /// </summary>
        public void Initialize()
        {
            CurrentState = GameState.Countdown;
            CurrentRound = INITIAL_ROUND;
            CurrentLife = INITIAL_LIFE;
            CurrentGold = INITIAL_GOLD;

            Debug.Log($"[GameplayManager] Initialized - Round: {CurrentRound}, Life: {CurrentLife}, Gold: {CurrentGold}");
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

            Debug.Log($"[GameplayManager] State changed: {oldState} -> {newState}");

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
                    return to == GameState.RoundResult;

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
            Debug.Log("[GameplayManager] Starting countdown sequence");

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
            Debug.Log("[GameplayManager] Countdown complete, transitioning to Preparation");
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
            Debug.Log($"[GameplayManager] Round updated: {CurrentRound}");
            OnGameValueChanged?.Invoke("Round", CurrentRound);
        }

        /// <summary>
        /// Update the current life points.
        /// </summary>
        public void SetLife(int life)
        {
            CurrentLife = Mathf.Max(0, life);
            Debug.Log($"[GameplayManager] Life updated: {CurrentLife}");
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
            Debug.Log($"[GameplayManager] Gold updated: {CurrentGold}");
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
    }
}
