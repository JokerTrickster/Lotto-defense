using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LottoDefense.Units;
using LottoDefense.Gameplay;

namespace LottoDefense.UI
{
    /// <summary>
    /// UI component that displays the unit upgrade panel.
    /// Shows current stats, upgrade cost, and provides upgrade interaction.
    /// </summary>
    public class UpgradeUI : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Panel References")]
        [SerializeField] private GameObject panelRoot;

        [Header("Unit Info Display")]
        [SerializeField] private Image unitIcon;
        [SerializeField] private TextMeshProUGUI unitNameText;
        [SerializeField] private TextMeshProUGUI unitTypeText;
        [SerializeField] private TextMeshProUGUI currentLevelText;

        [Header("Stats Display")]
        [SerializeField] private TextMeshProUGUI currentAttackText;
        [SerializeField] private TextMeshProUGUI nextAttackText;
        [SerializeField] private TextMeshProUGUI attackGainText;
        [SerializeField] private GameObject maxLevelIndicator;
        [SerializeField] private GameObject statsContainer;

        [Header("Upgrade Button")]
        [SerializeField] private Button upgradeButton;
        [SerializeField] private TextMeshProUGUI upgradeCostText;
        [SerializeField] private TextMeshProUGUI upgradeButtonText;
        [SerializeField] private Image goldIcon;

        [Header("Visual Feedback")]
        [SerializeField] private Color affordableColor = Color.green;
        [SerializeField] private Color unaffordableColor = Color.red;
        [SerializeField] private Color maxLevelColor = Color.yellow;

        [Header("Animation (Optional)")]
        [SerializeField] private ParticleSystem upgradeSuccessParticles;
        [SerializeField] private AudioClip upgradeSuccessSound;
        #endregion

        #region Components
        private AudioSource audioSource;
        #endregion

        #region Properties
        private Unit currentUnit;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Get or add AudioSource
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && upgradeSuccessSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            // Setup button listener
            if (upgradeButton != null)
            {
                upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
            }

            // Hide panel initially
            Hide();
        }

        private void Start()
        {
            // Subscribe to upgrade manager events
            if (UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.OnUnitUpgraded += OnUnitUpgraded;
                UpgradeManager.Instance.OnUpgradeFailed += OnUpgradeFailed;
            }

            // Subscribe to gameplay manager events for gold changes
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnGameValueChanged += OnGameValueChanged;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.OnUnitUpgraded -= OnUnitUpgraded;
                UpgradeManager.Instance.OnUpgradeFailed -= OnUpgradeFailed;
            }

            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnGameValueChanged -= OnGameValueChanged;
            }

            // Remove button listener
            if (upgradeButton != null)
            {
                upgradeButton.onClick.RemoveListener(OnUpgradeButtonClicked);
            }
        }
        #endregion

        #region Panel Management
        /// <summary>
        /// Show the upgrade panel for a specific unit.
        /// </summary>
        /// <param name="unit">Unit to display upgrade info for</param>
        public void Show(Unit unit)
        {
            if (unit == null)
            {
                Debug.LogWarning("[UpgradeUI] Cannot show panel for null unit");
                return;
            }

            currentUnit = unit;

            // Show panel
            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }

            // Update all display elements
            UpdateDisplay();

            Debug.Log($"[UpgradeUI] Showing upgrade panel for {unit.Data.GetDisplayName()}");
        }

        /// <summary>
        /// Hide the upgrade panel.
        /// </summary>
        public void Hide()
        {
            currentUnit = null;

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            Debug.Log("[UpgradeUI] Upgrade panel hidden");
        }

        /// <summary>
        /// Check if the panel is currently visible.
        /// </summary>
        public bool IsVisible()
        {
            return panelRoot != null && panelRoot.activeSelf;
        }
        #endregion

        #region Display Updates
        /// <summary>
        /// Update all UI elements with current unit data.
        /// </summary>
        private void UpdateDisplay()
        {
            if (currentUnit == null) return;

            UpdateUnitInfo();
            UpdateStatsDisplay();
            UpdateUpgradeButton();
        }

        /// <summary>
        /// Update unit basic information (icon, name, type, level).
        /// </summary>
        private void UpdateUnitInfo()
        {
            if (currentUnit == null) return;

            // Unit icon
            if (unitIcon != null && currentUnit.Data.icon != null)
            {
                unitIcon.sprite = currentUnit.Data.icon;
                unitIcon.enabled = true;
            }

            // Unit name
            if (unitNameText != null)
            {
                unitNameText.text = currentUnit.Data.GetDisplayName();
            }

            // Unit type
            if (unitTypeText != null)
            {
                unitTypeText.text = $"Type: {currentUnit.Data.type}";
            }

            // Current level
            if (currentLevelText != null)
            {
                int maxLevel = UpgradeManager.Instance.GetMaxLevel();
                currentLevelText.text = $"Level {currentUnit.UpgradeLevel}/{maxLevel}";

                // Color based on level
                if (currentUnit.UpgradeLevel >= maxLevel)
                {
                    currentLevelText.color = maxLevelColor;
                }
            }
        }

        /// <summary>
        /// Update stats display (current attack, next attack, gain).
        /// </summary>
        private void UpdateStatsDisplay()
        {
            if (currentUnit == null) return;

            UpgradeStats stats = UpgradeManager.Instance.GetUpgradeStats(currentUnit);

            // Show/hide max level indicator
            if (maxLevelIndicator != null)
            {
                maxLevelIndicator.SetActive(stats.isMaxLevel);
            }

            if (statsContainer != null)
            {
                statsContainer.SetActive(!stats.isMaxLevel);
            }

            if (stats.isMaxLevel)
            {
                // At max level, just show current attack
                if (currentAttackText != null)
                {
                    currentAttackText.text = $"Attack: {stats.currentAttack} (MAX)";
                }

                return;
            }

            // Current attack
            if (currentAttackText != null)
            {
                currentAttackText.text = $"Current: {stats.currentAttack}";
            }

            // Next level attack
            if (nextAttackText != null)
            {
                nextAttackText.text = $"Next: {stats.nextAttack}";
            }

            // Attack gain
            if (attackGainText != null)
            {
                attackGainText.text = $"+{stats.attackGain}";
                attackGainText.color = affordableColor;
            }
        }

        /// <summary>
        /// Update upgrade button state and cost display.
        /// </summary>
        private void UpdateUpgradeButton()
        {
            if (currentUnit == null || upgradeButton == null) return;

            UpgradeStats stats = UpgradeManager.Instance.GetUpgradeStats(currentUnit);

            if (stats.isMaxLevel)
            {
                // Max level - disable button
                upgradeButton.interactable = false;

                if (upgradeButtonText != null)
                {
                    upgradeButtonText.text = "MAX LEVEL";
                }

                if (upgradeCostText != null)
                {
                    upgradeCostText.text = "";
                }
            }
            else
            {
                // Can upgrade - check affordability
                bool canAfford = stats.canAfford;
                upgradeButton.interactable = canAfford;

                // Update button text
                if (upgradeButtonText != null)
                {
                    upgradeButtonText.text = canAfford ? "UPGRADE" : "NOT ENOUGH GOLD";
                }

                // Update cost display
                if (upgradeCostText != null)
                {
                    upgradeCostText.text = $"{stats.upgradeCost}";
                    upgradeCostText.color = canAfford ? affordableColor : unaffordableColor;
                }

                // Update gold icon color
                if (goldIcon != null)
                {
                    goldIcon.color = canAfford ? affordableColor : unaffordableColor;
                }
            }
        }
        #endregion

        #region Button Handlers
        /// <summary>
        /// Called when the upgrade button is clicked.
        /// </summary>
        public void OnUpgradeButtonClicked()
        {
            if (currentUnit == null)
            {
                Debug.LogWarning("[UpgradeUI] No unit selected for upgrade");
                return;
            }

            Debug.Log($"[UpgradeUI] Upgrade button clicked for {currentUnit.Data.GetDisplayName()}");

            // Attempt upgrade via manager
            bool success = UpgradeManager.Instance.TryUpgradeUnit(currentUnit);

            if (success)
            {
                // Success feedback handled by event callback
                Debug.Log("[UpgradeUI] Upgrade successful");
            }
            else
            {
                // Failure feedback handled by event callback
                Debug.Log("[UpgradeUI] Upgrade failed");
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Called when a unit is successfully upgraded.
        /// </summary>
        private void OnUnitUpgraded(Unit unit, int newLevel, int newAttack)
        {
            // Play success feedback
            ShowUpgradeSuccess();

            // Refresh display
            if (currentUnit == unit)
            {
                UpdateDisplay();
            }

            Debug.Log($"[UpgradeUI] Unit upgraded to L{newLevel}, ATK {newAttack}");
        }

        /// <summary>
        /// Called when an upgrade attempt fails.
        /// </summary>
        private void OnUpgradeFailed(string reason)
        {
            ShowUpgradeError(reason);
            Debug.LogWarning($"[UpgradeUI] Upgrade failed: {reason}");
        }

        /// <summary>
        /// Called when game values change (gold, life, round).
        /// </summary>
        private void OnGameValueChanged(string valueType, int newValue)
        {
            // Refresh upgrade button when gold changes
            if (valueType == "Gold" && IsVisible())
            {
                UpdateUpgradeButton();
            }
        }
        #endregion

        #region Visual Feedback
        /// <summary>
        /// Show success animation and play sound.
        /// </summary>
        private void ShowUpgradeSuccess()
        {
            // Play particle effect
            if (upgradeSuccessParticles != null)
            {
                upgradeSuccessParticles.Play();
            }

            // Play sound
            if (audioSource != null && upgradeSuccessSound != null)
            {
                audioSource.PlayOneShot(upgradeSuccessSound);
            }

            // TODO: Add button flash/glow animation
        }

        /// <summary>
        /// Show error feedback.
        /// </summary>
        private void ShowUpgradeError(string message)
        {
            // TODO: Add shake animation to button
            // TODO: Show error tooltip/popup
            Debug.Log($"[UpgradeUI] Error feedback: {message}");
        }
        #endregion

        #region Public API
        /// <summary>
        /// Get the currently displayed unit.
        /// </summary>
        public Unit GetCurrentUnit()
        {
            return currentUnit;
        }

        /// <summary>
        /// Refresh the display with latest data.
        /// </summary>
        public void RefreshDisplay()
        {
            if (IsVisible())
            {
                UpdateDisplay();
            }
        }
        #endregion
    }
}
