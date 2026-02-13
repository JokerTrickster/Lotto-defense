using UnityEngine;
using UnityEngine.UI;
using LottoDefense.Units;
using LottoDefense.Gameplay;
using LottoDefense.Grid;
using LottoDefense.VFX;

namespace LottoDefense.UI
{
    /// <summary>
    /// Bottom UI panel with 2-row layout:
    /// Top row (visible on unit select): Sell + Synthesis
    /// Bottom row (always visible): Auto Synthesis, Attack Upgrade, Attack Speed Upgrade.
    /// </summary>
    public class GameBottomUI : MonoBehaviour
    {
        #region Serialized Fields
        [Header("UI References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private GameObject topRow;

        [Header("Bottom Row Buttons")]
        [SerializeField] private Button autoSynthesisButton;
        [SerializeField] private Button attackUpgradeButton;
        [SerializeField] private Button attackSpeedUpgradeButton;

        [Header("Top Row Buttons")]
        [SerializeField] private Button sellButton;
        [SerializeField] private Button synthesisButton;

        [Header("Text References")]
        [SerializeField] private Text autoSynthesisButtonText;
        [SerializeField] private Text attackUpgradeButtonText;
        [SerializeField] private Text attackSpeedUpgradeButtonText;
        [SerializeField] private Text sellButtonText;
        [SerializeField] private Text synthesisButtonText;
        #endregion

        #region Private Fields
        private Unit selectedUnit;
        private bool listenersInitialized;
        private GameBalanceConfig balanceConfig;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            balanceConfig = Resources.Load<GameBalanceConfig>("GameBalanceConfig");

            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnStateChanged += HandleStateChanged;
            }
        }

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
            if (sellButton != null)
                sellButton.onClick.AddListener(OnSellButtonClicked);
            if (synthesisButton != null)
                synthesisButton.onClick.AddListener(OnSynthesisButtonClicked);
        }

        private void Start()
        {
            EnsureListeners();
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
            if (GameplayManager.Instance != null)
            {
                UpdateButtonStates();
            }
        }
        #endregion

        #region Public Methods
        public void Show()
        {
            EnsureListeners();
            if (panel != null)
            {
                panel.SetActive(true);
            }
            UpdateButtonStates();
        }

        public void Hide()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
            selectedUnit = null;
        }

        public void SetSelectedUnit(Unit unit)
        {
            selectedUnit = unit;
            UpdateButtonStates();
        }
        #endregion

        #region State Handlers
        private void HandleStateChanged(GameState oldState, GameState newState)
        {
            UpdateButtonStates();
        }
        #endregion

        #region Button Handlers
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

        private void OnAttackUpgradeClicked()
        {
            if (selectedUnit == null) return;

            if (UnitUpgradeManager.Instance != null)
            {
                bool success = UnitUpgradeManager.Instance.UpgradeAttack(selectedUnit);
                if (success)
                {
                    Debug.Log($"[GameBottomUI] Upgraded {selectedUnit.Data.GetDisplayName()} attack");
                    VFXManager.Instance?.ShowUpgradeEffect(selectedUnit.transform.position, selectedUnit.Data.rarity);
                    UpdateButtonStates();
                }
            }
        }

        private void OnAttackSpeedUpgradeClicked()
        {
            if (selectedUnit == null) return;

            if (UnitUpgradeManager.Instance != null)
            {
                bool success = UnitUpgradeManager.Instance.UpgradeAttackSpeed(selectedUnit);
                if (success)
                {
                    Debug.Log($"[GameBottomUI] Upgraded {selectedUnit.Data.GetDisplayName()} attack speed");
                    VFXManager.Instance?.ShowUpgradeEffect(selectedUnit.transform.position, selectedUnit.Data.rarity);
                    UpdateButtonStates();
                }
            }
        }

        private void OnSellButtonClicked()
        {
            if (selectedUnit == null || balanceConfig == null) return;

            int sellPrice = balanceConfig.GetSellGold(selectedUnit.Data.rarity);
            Debug.Log($"[GameBottomUI] Selling {selectedUnit.Data.GetDisplayName()} for {sellPrice} gold");

            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.ModifyGold(sellPrice);
            }

            if (GridManager.Instance != null)
            {
                GridManager.Instance.RemoveUnit(selectedUnit.GridPosition);
            }

            Destroy(selectedUnit.gameObject);

            UnitSelectionUI selectionUI = FindFirstObjectByType<UnitSelectionUI>();
            if (selectionUI != null)
            {
                selectionUI.HideUI();
            }

            selectedUnit = null;
            UpdateButtonStates();
        }

        private void OnSynthesisButtonClicked()
        {
            if (selectedUnit == null || balanceConfig == null) return;

            if (SynthesisManager.Instance == null)
            {
                Debug.LogError("[GameBottomUI] SynthesisManager not found!");
                return;
            }

            Unit target = FindCompatibleUnit(selectedUnit);
            if (target == null)
            {
                Debug.LogWarning("[GameBottomUI] No compatible unit found for synthesis");
                return;
            }

            bool success = SynthesisManager.Instance.TrySynthesize(selectedUnit, target);
            if (success)
            {
                Debug.Log("[GameBottomUI] Synthesis successful!");
            }

            UnitSelectionUI selectionUI = FindFirstObjectByType<UnitSelectionUI>();
            if (selectionUI != null)
            {
                selectionUI.HideUI();
            }

            selectedUnit = null;
            UpdateButtonStates();
        }
        #endregion

        #region UI Update
        private void UpdateButtonStates()
        {
            bool canManage = CanManageUnits();
            bool hasUnit = selectedUnit != null;

            // Top row: visible only when unit is selected
            if (topRow != null)
            {
                topRow.SetActive(hasUnit);
            }

            // Sell button
            if (sellButton != null && hasUnit && balanceConfig != null)
            {
                sellButton.interactable = canManage;
                int sellPrice = balanceConfig.GetSellGold(selectedUnit.Data.rarity);
                if (sellButtonText != null)
                {
                    sellButtonText.text = $"\uD310\uB9E4 (+{sellPrice}G)";
                }
                UpdateButtonColor(sellButton, sellButton.interactable);
            }

            // Synthesis button
            if (synthesisButton != null)
            {
                if (hasUnit && balanceConfig != null)
                {
                    var recipe = balanceConfig.GetSynthesisRecipe(selectedUnit.Data.unitName);
                    bool hasRecipe = recipe != null;
                    synthesisButton.gameObject.SetActive(hasRecipe);
                    synthesisButton.interactable = canManage && hasRecipe;
                    if (synthesisButtonText != null)
                    {
                        synthesisButtonText.text = "\uC870\uD569";
                    }
                    UpdateButtonColor(synthesisButton, synthesisButton.interactable);
                }
            }

            // Auto Synthesis button
            if (autoSynthesisButton != null)
            {
                int possibleSynthesis = AutoSynthesisManager.Instance != null ?
                    AutoSynthesisManager.Instance.GetPossibleSynthesisCount() : 0;

                autoSynthesisButton.interactable = canManage && possibleSynthesis > 0;

                if (autoSynthesisButtonText != null)
                {
                    autoSynthesisButtonText.text = possibleSynthesis > 0
                        ? $"\uC790\uB3D9 \uC870\uD569 ({possibleSynthesis})"
                        : "\uC790\uB3D9 \uC870\uD569 (0)";
                }

                UpdateButtonColor(autoSynthesisButton, autoSynthesisButton.interactable);
            }

            // Attack Upgrade button
            if (attackUpgradeButton != null)
            {
                bool canUpgradeAttack = canManage && hasUnit &&
                    selectedUnit.AttackUpgradeLevel < 10;

                int cost = 0;
                if (hasUnit && UnitUpgradeManager.Instance != null)
                {
                    cost = UnitUpgradeManager.Instance.GetUpgradeCost(selectedUnit, UpgradeType.Attack);
                }

                bool hasEnoughGold = GameplayManager.Instance != null && GameplayManager.Instance.CurrentGold >= cost;
                attackUpgradeButton.interactable = canUpgradeAttack && hasEnoughGold;

                if (attackUpgradeButtonText != null)
                {
                    if (!hasUnit)
                    {
                        attackUpgradeButtonText.text = "\uACF5\uACA9\uB825 UP";
                    }
                    else if (selectedUnit.AttackUpgradeLevel >= 10)
                    {
                        attackUpgradeButtonText.text = "\uACF5\uACA9\uB825 MAX";
                    }
                    else
                    {
                        attackUpgradeButtonText.text = $"\uACF5\uACA9\uB825 UP ({cost}G) Lv.{selectedUnit.AttackUpgradeLevel}";
                    }
                }

                UpdateButtonColor(attackUpgradeButton, attackUpgradeButton.interactable);
            }

            // Attack Speed Upgrade button
            if (attackSpeedUpgradeButton != null)
            {
                bool canUpgradeSpeed = canManage && hasUnit &&
                    selectedUnit.AttackSpeedUpgradeLevel < 10;

                int cost = 0;
                if (hasUnit && UnitUpgradeManager.Instance != null)
                {
                    cost = UnitUpgradeManager.Instance.GetUpgradeCost(selectedUnit, UpgradeType.AttackSpeed);
                }

                bool hasEnoughGold = GameplayManager.Instance != null && GameplayManager.Instance.CurrentGold >= cost;
                attackSpeedUpgradeButton.interactable = canUpgradeSpeed && hasEnoughGold;

                if (attackSpeedUpgradeButtonText != null)
                {
                    if (!hasUnit)
                    {
                        attackSpeedUpgradeButtonText.text = "\uACF5\uC18D UP";
                    }
                    else if (selectedUnit.AttackSpeedUpgradeLevel >= 10)
                    {
                        attackSpeedUpgradeButtonText.text = "\uACF5\uC18D MAX";
                    }
                    else
                    {
                        attackSpeedUpgradeButtonText.text = $"\uACF5\uC18D UP ({cost}G) Lv.{selectedUnit.AttackSpeedUpgradeLevel}";
                    }
                }

                UpdateButtonColor(attackSpeedUpgradeButton, attackSpeedUpgradeButton.interactable);
            }
        }

        private void UpdateButtonColor(Button button, bool isInteractable)
        {
            if (button == null) return;

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

        private bool CanManageUnits()
        {
            return GameplayManager.Instance != null;
        }
        #endregion

        #region Helper Methods
        private Unit FindCompatibleUnit(Unit source)
        {
            if (GridManager.Instance == null) return null;

            for (int x = 0; x < GridManager.GRID_WIDTH; x++)
            {
                for (int y = 0; y < GridManager.GRID_HEIGHT; y++)
                {
                    Unit candidate = GridManager.Instance.GetUnitAt(x, y);
                    if (candidate == null) continue;
                    if (candidate == source) continue;
                    if (candidate.Data.unitName == source.Data.unitName)
                        return candidate;
                }
            }
            return null;
        }
        #endregion
    }
}
