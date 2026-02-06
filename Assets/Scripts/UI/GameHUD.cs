using UnityEngine;
using UnityEngine.UI;
using LottoDefense.Gameplay;

namespace LottoDefense.UI
{
    /// <summary>
    /// Manages the game HUD display showing 6 critical stats:
    /// Round number, remaining time, monster count, gold, unit count, and life.
    /// Updates in real-time when game values change.
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        #region Inspector Fields
        [Header("HUD Text Components")]
        [SerializeField] private Text roundText;
        [SerializeField] private Text phaseText;
        [SerializeField] private Text timeText;
        [SerializeField] private Text monsterText;
        [SerializeField] private Text goldText;
        [SerializeField] private Text unitText;
        [SerializeField] private Text lifeText;

        [Header("Settings")]
        [SerializeField] private string roundFormat = "R{0}";
        [SerializeField] private string phaseFormat = "{0}";
        [SerializeField] private string timeFormat = "{0:00}:{1:00}";
        [SerializeField] private string monsterFormat = "M:{0}";
        [SerializeField] private string goldFormat = "G:{0}";
        [SerializeField] private string unitFormat = "U:{0}";
        [SerializeField] private string lifeFormat = "♥{0}";
        #endregion

        #region Private Fields
        private int currentRound = 1;
        private string currentPhase = "COUNTDOWN";
        private float currentTime = 0f;
        private int currentMonsterCount = 0;
        private int currentGold = 0;
        private int currentUnitCount = 0;
        private int currentLife = 0;
        #endregion

        #region Unity Lifecycle
        private void OnEnable()
        {
            // Subscribe to GameplayManager events
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnGameValueChanged += HandleGameValueChanged;
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from GameplayManager events
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnGameValueChanged -= HandleGameValueChanged;
            }
        }

        private void Start()
        {
            // Initialize HUD with current game values
            InitializeFromGameplayManager();
        }

        private void Update()
        {
            // Update time countdown during Combat state
            if (GameplayManager.Instance != null &&
                GameplayManager.Instance.CurrentState == GameState.Combat)
            {
                // This will be updated by round manager in the future
                // For now, just display the current time
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize HUD values from GameplayManager current state.
        /// </summary>
        private void InitializeFromGameplayManager()
        {
            if (GameplayManager.Instance == null)
            {
                Debug.LogWarning("[GameHUD] GameplayManager not found during initialization");
                return;
            }

            UpdateRound(GameplayManager.Instance.CurrentRound);
            UpdateGold(GameplayManager.Instance.CurrentGold);
            UpdateLife(GameplayManager.Instance.CurrentLife);

            // Initialize other values to defaults
            UpdatePhase("COUNTDOWN");
            UpdateTime(0f);
            UpdateMonsterCount(0);
            UpdateUnitCount(0);

            Debug.Log("[GameHUD] Initialized with GameplayManager values");
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handle game value changes from GameplayManager.
        /// </summary>
        /// <param name="valueType">Type of value that changed (Round/Life/Gold)</param>
        /// <param name="newValue">New value</param>
        private void HandleGameValueChanged(string valueType, int newValue)
        {
            switch (valueType)
            {
                case "Round":
                    UpdateRound(newValue);
                    break;

                case "Life":
                    UpdateLife(newValue);
                    break;

                case "Gold":
                    UpdateGold(newValue);
                    break;

                default:
                    Debug.LogWarning($"[GameHUD] Unknown value type: {valueType}");
                    break;
            }
        }
        #endregion

        #region Update Methods
        /// <summary>
        /// Update the round number display.
        /// </summary>
        /// <param name="round">Current round number</param>
        public void UpdateRound(int round)
        {
            currentRound = round;

            if (roundText != null)
            {
                roundText.text = string.Format(roundFormat, currentRound);
            }
            else
            {
                Debug.LogWarning("[GameHUD] Round text component not assigned");
            }
        }

        /// <summary>
        /// Update the phase display.
        /// </summary>
        /// <param name="phaseName">Phase name to display</param>
        public void UpdatePhase(string phaseName)
        {
            currentPhase = phaseName;

            if (phaseText != null)
            {
                phaseText.text = string.Format(phaseFormat, currentPhase);
            }
            else
            {
                Debug.LogWarning("[GameHUD] Phase text component not assigned");
            }
        }

        /// <summary>
        /// Update the remaining time display.
        /// </summary>
        /// <param name="seconds">Remaining time in seconds</param>
        public void UpdateTime(float seconds)
        {
            currentTime = seconds;

            if (timeText != null)
            {
                int minutes = Mathf.FloorToInt(currentTime / 60f);
                int secs = Mathf.FloorToInt(currentTime % 60f);
                timeText.text = string.Format(timeFormat, minutes, secs);
            }
            else
            {
                Debug.LogWarning("[GameHUD] Time text component not assigned");
            }
        }

        /// <summary>
        /// Update the monster count display.
        /// </summary>
        /// <param name="count">Number of monsters remaining</param>
        public void UpdateMonsterCount(int count)
        {
            currentMonsterCount = count;

            if (monsterText != null)
            {
                monsterText.text = string.Format(monsterFormat, currentMonsterCount);
            }
            else
            {
                Debug.LogWarning("[GameHUD] Monster text component not assigned");
            }
        }

        /// <summary>
        /// Update the gold display.
        /// </summary>
        /// <param name="gold">Current gold amount</param>
        public void UpdateGold(int gold)
        {
            currentGold = gold;

            if (goldText != null)
            {
                goldText.text = string.Format(goldFormat, currentGold);
            }
            else
            {
                Debug.LogWarning("[GameHUD] Gold text component not assigned");
            }
        }

        /// <summary>
        /// Update the unit count display.
        /// </summary>
        /// <param name="count">Number of units placed</param>
        public void UpdateUnitCount(int count)
        {
            currentUnitCount = count;

            if (unitText != null)
            {
                unitText.text = string.Format(unitFormat, currentUnitCount);
            }
            else
            {
                Debug.LogWarning("[GameHUD] Unit text component not assigned");
            }
        }

        /// <summary>
        /// Update the life display.
        /// </summary>
        /// <param name="life">Current life points</param>
        public void UpdateLife(int life)
        {
            currentLife = life;

            if (lifeText != null)
            {
                lifeText.text = string.Format(lifeFormat, currentLife);
            }
            else
            {
                Debug.LogWarning("[GameHUD] Life text component not assigned");
            }
        }
        #endregion

        #region Public API
        /// <summary>
        /// Manually refresh all HUD values from GameplayManager.
        /// Useful for debugging or after scene changes.
        /// </summary>
        public void RefreshAllValues()
        {
            InitializeFromGameplayManager();
        }

        /// <summary>
        /// Get the current round displayed on HUD.
        /// </summary>
        public int CurrentRound => currentRound;

        /// <summary>
        /// Get the current phase displayed on HUD.
        /// </summary>
        public string CurrentPhase => currentPhase;

        /// <summary>
        /// Get the current time displayed on HUD.
        /// </summary>
        public float CurrentTime => currentTime;

        /// <summary>
        /// Get the current monster count displayed on HUD.
        /// </summary>
        public int CurrentMonsterCount => currentMonsterCount;

        /// <summary>
        /// Get the current gold displayed on HUD.
        /// </summary>
        public int CurrentGold => currentGold;

        /// <summary>
        /// Get the current unit count displayed on HUD.
        /// </summary>
        public int CurrentUnitCount => currentUnitCount;

        /// <summary>
        /// Get the current life displayed on HUD.
        /// </summary>
        public int CurrentLife => currentLife;
        #endregion
    }
}
