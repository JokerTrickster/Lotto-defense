using UnityEngine;
using UnityEngine.UI;
using LottoDefense.Units;
using LottoDefense.Gameplay;

namespace LottoDefense.UI
{
    /// <summary>
    /// Bottom UI panel with game action buttons: Auto Synthesis, Attack Upgrade, Attack Speed Upgrade.
    /// Active in all game states for unit management.
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

        #endregion

        #region Private Fields
        private Unit selectedUnit;
        private bool listenersInitialized;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Subscribe in Awake since this GameObject may be disabled before OnEnable fires.
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnStateChanged += HandleStateChanged;
            }
        }

        /// <summary>
        /// Lazy-init button listeners. Serialized fields are set via reflection by
        /// GameSceneBootstrapper AFTER Awake, so we init on first Show/Update.
        /// </summary>
        private void EnsureListeners()
        {
            if (listenersInitialized) return;
            listenersInitialized = true;

            if (autoSynthesisButton != null)
                autoSynthesisButton.onClick.AddListener(OnAutoSynthesisClicked);
            if (attackUpgradeButton != null)
                attackUpgradeButton.onClick.AddListener(OnAttackUpgradeClicked);
            if (attackSpeedUpgradeButton != null)
                attackSpeedUpgradeButton.onClick.AddListener(OnAttackSpeedUpgradeClicked);
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
            // Update button states every frame
            if (GameplayManager.Instance != null)
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
            EnsureListeners();
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
        /// Handle game state changes. Bottom UI stays visible in all states.
        /// </summary>
        private void HandleStateChanged(GameState oldState, GameState newState)
        {
            UpdateButtonStates();
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
                    autoSynthesisButtonText.text = possibleSynthesis > 0
                        ? $"자동 조합 ({possibleSynthesis})"
                        : "자동 조합 (0)";
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

            // Button colors use white-based tinting: Image.color holds the identity color,
            // ColorBlock tints it (white = full color, gray = dimmed).
            ColorBlock colors = button.colors;
            if (isInteractable)
            {
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
                colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            }
            else
            {
                colors.normalColor = new Color(0.4f, 0.4f, 0.4f, 0.7f);
                colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 0.7f);
                colors.pressedColor = new Color(0.4f, 0.4f, 0.4f, 0.7f);
            }
            colors.disabledColor = new Color(0.35f, 0.35f, 0.35f, 0.7f);
            button.colors = colors;
        }

        /// <summary>
        /// Check if units can be managed (any active game state).
        /// </summary>
        private bool CanManageUnits()
        {
            return GameplayManager.Instance != null;
        }
        #endregion
    }
}
