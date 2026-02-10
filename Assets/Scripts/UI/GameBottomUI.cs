using UnityEngine;
using UnityEngine.UI;
using LottoDefense.Units;
using LottoDefense.Gameplay;

namespace LottoDefense.UI
{
    /// <summary>
    /// Bottom UI panel with game action buttons: Auto Synthesis, Attack Upgrade, Attack Speed Upgrade.
    /// Only active during Preparation phase.
    /// </summary>
    public class GameBottomUI : MonoBehaviour
    {
        #region Serialized Fields
        [Header("UI References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Button autoSynthesisButton;
        [SerializeField] private Button attackUpgradeButton;
        [SerializeField] private Button attackSpeedUpgradeButton;

        [Header("Text References")]
        [SerializeField] private Text autoSynthesisButtonText;
        [SerializeField] private Text attackUpgradeButtonText;
        [SerializeField] private Text attackSpeedUpgradeButtonText;

        [Header("Colors")]
        [SerializeField] private Color buttonNormalColor = new Color(0.2f, 0.6f, 1f, 1f); // Blue
        [SerializeField] private Color buttonDisabledColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Gray
        [SerializeField] private Color buttonHighlightColor = new Color(0.3f, 0.8f, 1f, 1f); // Bright blue
        #endregion

        #region Private Fields
        private Unit selectedUnit;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Setup button listeners
            if (autoSynthesisButton != null)
            {
                autoSynthesisButton.onClick.AddListener(OnAutoSynthesisClicked);
            }

            if (attackUpgradeButton != null)
            {
                attackUpgradeButton.onClick.AddListener(OnAttackUpgradeClicked);
            }

            if (attackSpeedUpgradeButton != null)
            {
                attackSpeedUpgradeButton.onClick.AddListener(OnAttackSpeedUpgradeClicked);
            }

            // Hide by default
            if (panel != null)
            {
                panel.SetActive(false);
            }

            // Subscribe in Awake since this GameObject may be disabled before OnEnable fires.
            // The panel field points to the same GameObject, so SetActive(false) prevents OnEnable.
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnStateChanged += HandleStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnStateChanged -= HandleStateChanged;
            }
        }

        private void Update()
        {
            // Update button states every frame during Preparation phase
            if (GameplayManager.Instance != null && GameplayManager.Instance.CurrentState == GameState.Preparation)
            {
                UpdateButtonStates();
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Show bottom UI panel.
        /// </summary>
        public void Show()
        {
            if (panel != null)
            {
                panel.SetActive(true);
            }
            UpdateButtonStates();
        }

        /// <summary>
        /// Hide bottom UI panel.
        /// </summary>
        public void Hide()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
            selectedUnit = null;
        }

        /// <summary>
        /// Update selected unit for upgrade buttons.
        /// </summary>
        public void SetSelectedUnit(Unit unit)
        {
            selectedUnit = unit;
            UpdateButtonStates();
        }
        #endregion

        #region State Handlers
        /// <summary>
        /// Auto show/hide based on game state changes.
        /// Shows during Preparation, hides otherwise.
        /// </summary>
        private void HandleStateChanged(GameState oldState, GameState newState)
        {
            if (newState == GameState.Preparation)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }
        #endregion

        #region Button Handlers
        /// <summary>
        /// Handle auto synthesis button click.
        /// </summary>
        private void OnAutoSynthesisClicked()
        {
            if (AutoSynthesisManager.Instance != null)
            {
                int count = AutoSynthesisManager.Instance.PerformAutoSynthesis();
                if (count > 0)
                {
                    Debug.Log($"[GameBottomUI] Auto-synthesized {count} groups");
                }
                else
                {
                    Debug.Log("[GameBottomUI] No units eligible for auto-synthesis");
                }
            }
        }

        /// <summary>
        /// Handle attack upgrade button click.
        /// </summary>
        private void OnAttackUpgradeClicked()
        {
            if (selectedUnit == null)
            {
                Debug.LogWarning("[GameBottomUI] No unit selected for attack upgrade");
                return;
            }

            if (UnitUpgradeManager.Instance != null)
            {
                bool success = UnitUpgradeManager.Instance.UpgradeAttack(selectedUnit);
                if (success)
                {
                    Debug.Log($"[GameBottomUI] Upgraded {selectedUnit.Data.GetDisplayName()} attack");
                    UpdateButtonStates();
                }
            }
        }

        /// <summary>
        /// Handle attack speed upgrade button click.
        /// </summary>
        private void OnAttackSpeedUpgradeClicked()
        {
            if (selectedUnit == null)
            {
                Debug.LogWarning("[GameBottomUI] No unit selected for attack speed upgrade");
                return;
            }

            if (UnitUpgradeManager.Instance != null)
            {
                bool success = UnitUpgradeManager.Instance.UpgradeAttackSpeed(selectedUnit);
                if (success)
                {
                    Debug.Log($"[GameBottomUI] Upgraded {selectedUnit.Data.GetDisplayName()} attack speed");
                    UpdateButtonStates();
                }
            }
        }
        #endregion

        #region UI Update
        /// <summary>
        /// Update button states and text based on current game state.
        /// </summary>
        private void UpdateButtonStates()
        {
            bool canManage = CanManageUnits();

            // Auto Synthesis button
            if (autoSynthesisButton != null)
            {
                int possibleSynthesis = AutoSynthesisManager.Instance != null ?
                    AutoSynthesisManager.Instance.GetPossibleSynthesisCount() : 0;

                autoSynthesisButton.interactable = canManage && possibleSynthesis > 0;

                if (autoSynthesisButtonText != null)
                {
                    if (!canManage)
                    {
                        autoSynthesisButtonText.text = "준비 시간에만 가능";
                    }
                    else if (possibleSynthesis > 0)
                    {
                        autoSynthesisButtonText.text = $"자동 조합 ({possibleSynthesis})";
                    }
                    else
                    {
                        autoSynthesisButtonText.text = "자동 조합 (0)";
                    }
                }

                UpdateButtonColor(autoSynthesisButton, autoSynthesisButton.interactable);
            }

            // Attack Upgrade button
            if (attackUpgradeButton != null)
            {
                bool canUpgradeAttack = canManage && selectedUnit != null &&
                    selectedUnit.AttackUpgradeLevel < 10;

                int cost = 0;
                if (selectedUnit != null && UnitUpgradeManager.Instance != null)
                {
                    cost = UnitUpgradeManager.Instance.GetUpgradeCost(selectedUnit, UpgradeType.Attack);
                }

                bool hasEnoughGold = GameplayManager.Instance != null && GameplayManager.Instance.CurrentGold >= cost;
                attackUpgradeButton.interactable = canUpgradeAttack && hasEnoughGold;

                if (attackUpgradeButtonText != null)
                {
                    if (selectedUnit == null)
                    {
                        attackUpgradeButtonText.text = "유닛을 선택하세요";
                    }
                    else if (!canManage)
                    {
                        attackUpgradeButtonText.text = "준비 시간에만 가능";
                    }
                    else if (selectedUnit.AttackUpgradeLevel >= 10)
                    {
                        attackUpgradeButtonText.text = "최대 레벨";
                    }
                    else
                    {
                        attackUpgradeButtonText.text = $"공격력 업그레이드 ({cost}G) Lv.{selectedUnit.AttackUpgradeLevel}";
                    }
                }

                UpdateButtonColor(attackUpgradeButton, attackUpgradeButton.interactable);
            }

            // Attack Speed Upgrade button
            if (attackSpeedUpgradeButton != null)
            {
                bool canUpgradeSpeed = canManage && selectedUnit != null &&
                    selectedUnit.AttackSpeedUpgradeLevel < 10;

                int cost = 0;
                if (selectedUnit != null && UnitUpgradeManager.Instance != null)
                {
                    cost = UnitUpgradeManager.Instance.GetUpgradeCost(selectedUnit, UpgradeType.AttackSpeed);
                }

                bool hasEnoughGold = GameplayManager.Instance != null && GameplayManager.Instance.CurrentGold >= cost;
                attackSpeedUpgradeButton.interactable = canUpgradeSpeed && hasEnoughGold;

                if (attackSpeedUpgradeButtonText != null)
                {
                    if (selectedUnit == null)
                    {
                        attackSpeedUpgradeButtonText.text = "유닛을 선택하세요";
                    }
                    else if (!canManage)
                    {
                        attackSpeedUpgradeButtonText.text = "준비 시간에만 가능";
                    }
                    else if (selectedUnit.AttackSpeedUpgradeLevel >= 10)
                    {
                        attackSpeedUpgradeButtonText.text = "최대 레벨";
                    }
                    else
                    {
                        attackSpeedUpgradeButtonText.text = $"공속 업그레이드 ({cost}G) Lv.{selectedUnit.AttackSpeedUpgradeLevel}";
                    }
                }

                UpdateButtonColor(attackSpeedUpgradeButton, attackSpeedUpgradeButton.interactable);
            }
        }

        /// <summary>
        /// Update button background color based on interactive state.
        /// </summary>
        private void UpdateButtonColor(Button button, bool isInteractable)
        {
            if (button == null) return;

            ColorBlock colors = button.colors;
            colors.normalColor = isInteractable ? buttonNormalColor : buttonDisabledColor;
            colors.highlightedColor = isInteractable ? buttonHighlightColor : buttonDisabledColor;
            colors.pressedColor = isInteractable ? buttonHighlightColor * 0.8f : buttonDisabledColor;
            colors.disabledColor = buttonDisabledColor;
            button.colors = colors;
        }

        /// <summary>
        /// Check if units can be managed (Preparation phase only).
        /// </summary>
        private bool CanManageUnits()
        {
            return GameplayManager.Instance != null &&
                   GameplayManager.Instance.CurrentState == GameState.Preparation;
        }
        #endregion
    }
}
