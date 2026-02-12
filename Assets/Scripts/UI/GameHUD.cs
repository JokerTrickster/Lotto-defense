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

        [Header("Tooltip")]
        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private Text tooltipText;
        [SerializeField] private RectTransform[] statContainers;

        [Header("Settings")]
        [SerializeField] private string roundFormat = "R{0}";
        [SerializeField] private string phaseFormat = "{0}";
        [SerializeField] private string timeFormat = "{0:00}:{1:00}";
        [SerializeField] private string monsterFormat = "{0}";
        [SerializeField] private string goldFormat = "{0}";
        [SerializeField] private string unitFormat = "{0}";
        [SerializeField] private string lifeFormat = "{0}";
        #endregion

        #region Private Fields
        private int currentRound = 1;
        private string currentPhase = "COUNTDOWN";
        private float currentTime = 0f;
        private int currentMonsterCount = 0;
        private int currentGold = 0;
        private int currentUnitCount = 0;
        private int currentLife = 0;
        private int activeTooltipIndex = -1;

        private static readonly string[] StatDescriptions = new string[]
        {
            "\uD604\uC7AC \uB77C\uC6B4\uB4DC",
            "\uD604\uC7AC \uAC8C\uC784 \uB2E8\uACC4",
            "\uB0A8\uC740 \uC2DC\uAC04",
            "\uB0A8\uC740 \uC0DD\uBA85 - 0\uC774 \uB418\uBA74 \uD328\uBC30",
            "\uBCF4\uC720 \uACE8\uB4DC - \uC720\uB2DB \uC18C\uD658\uC5D0 \uC0AC\uC6A9",
            "\uB0A8\uC740 \uBAAC\uC2A4\uD130 \uC218",
            "\uBC30\uCE58\uB41C \uC720\uB2DB \uC218"
        };
        #endregion

        #region Unity Lifecycle
        private void OnEnable()
        {
            // Subscribe to GameplayManager events
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnGameValueChanged += HandleGameValueChanged;
                GameplayManager.Instance.OnStateChanged += HandleStateChanged;
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from GameplayManager events
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnGameValueChanged -= HandleGameValueChanged;
                GameplayManager.Instance.OnStateChanged -= HandleStateChanged;
            }
        }

        private void Start()
        {
            // Initialize HUD with current game values
            InitializeFromGameplayManager();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && statContainers != null && statContainers.Length > 0)
            {
                Vector2 screenPos = Input.mousePosition;
                bool hitStat = false;

                for (int i = 0; i < statContainers.Length; i++)
                {
                    if (statContainers[i] != null &&
                        RectTransformUtility.RectangleContainsScreenPoint(statContainers[i], screenPos, null))
                    {
                        if (activeTooltipIndex == i)
                            HideTooltip();
                        else
                            ShowTooltip(i);
                        hitStat = true;
                        break;
                    }
                }

                if (!hitStat && activeTooltipIndex >= 0)
                {
                    HideTooltip();
                }
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
        /// Handle game state changes from GameplayManager.
        /// Maps GameState to Korean display string and updates phase text.
        /// </summary>
        private void HandleStateChanged(GameState oldState, GameState newState)
        {
            string phaseDisplay;
            switch (newState)
            {
                case GameState.Countdown:
                    phaseDisplay = "\uCE74\uC6B4\uD2B8\uB2E4\uC6B4";
                    break;
                case GameState.Preparation:
                    phaseDisplay = "\uC900\uBE44 \uB2E8\uACC4";
                    break;
                case GameState.Combat:
                    phaseDisplay = "\uC804\uD22C \uC911";
                    break;
                case GameState.RoundResult:
                    phaseDisplay = "\uB77C\uC6B4\uB4DC \uC644\uB8CC";
                    break;
                case GameState.Victory:
                    phaseDisplay = "\uC2B9\uB9AC";
                    break;
                case GameState.Defeat:
                    phaseDisplay = "\uD328\uBC30";
                    break;
                default:
                    phaseDisplay = newState.ToString();
                    break;
            }
            UpdatePhase(phaseDisplay);
        }

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

        #region Tooltip
        private void ShowTooltip(int index)
        {
            if (tooltipPanel == null || tooltipText == null) return;
            if (index < 0 || index >= StatDescriptions.Length) return;

            activeTooltipIndex = index;
            tooltipText.text = StatDescriptions[index];
            tooltipPanel.SetActive(true);
        }

        private void HideTooltip()
        {
            activeTooltipIndex = -1;
            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(false);
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
