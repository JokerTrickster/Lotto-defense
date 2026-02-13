using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using LottoDefense.Units;
using LottoDefense.Gameplay;
using LottoDefense.Grid;
using LottoDefense.VFX;

namespace LottoDefense.UI
{
    /// <summary>
    /// StarCraft-style fixed bottom command panel.
    /// Left column: UnitInfoPanel (always visible, shows empty state when no unit selected).
    /// Right column: 3x2 button grid (summon, auto-synth, sell, atk up, spd up, synthesis).
    /// Panel is always visible - buttons are dimmed when not applicable.
    /// </summary>
    public class GameBottomUI : MonoBehaviour
    {
        #region Serialized Fields
        [Header("UI References")]
        [SerializeField] private GameObject panel;

        [Header("Button Grid")]
        [SerializeField] private Button summonButton;
        [SerializeField] private Button autoSynthesisButton;
        [SerializeField] private Button sellButton;
        [SerializeField] private Button attackUpgradeButton;
        [SerializeField] private Button attackSpeedUpgradeButton;
        [SerializeField] private Button synthesisButton;

        [Header("Text References")]
        [SerializeField] private Text summonButtonText;
        [SerializeField] private Text autoSynthesisButtonText;
        [SerializeField] private Text sellButtonText;
        [SerializeField] private Text attackUpgradeButtonText;
        [SerializeField] private Text attackSpeedUpgradeButtonText;
        [SerializeField] private Text synthesisButtonText;
        #endregion

        #region Private Fields
        private Unit selectedUnit;
        private bool listenersInitialized;
        private GameBalanceConfig balanceConfig;
        private UnitInfoPanel unitInfoPanel;
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

        /// <summary>
        /// Wire button onClick listeners. Can be called externally from bootstrapper.
        /// </summary>
        public void EnsureListeners()
        {
            if (listenersInitialized) return;

            if (summonButton == null && attackUpgradeButton == null && sellButton == null && autoSynthesisButton == null)
            {
                Debug.LogWarning("[GameBottomUI] EnsureListeners called but all button refs are null - skipping");
                return;
            }

            listenersInitialized = true;

            int count = 0;
            if (summonButton != null) { summonButton.onClick.AddListener(OnSummonClicked); count++; }
            if (autoSynthesisButton != null) { autoSynthesisButton.onClick.AddListener(OnAutoSynthesisClicked); count++; }
            if (sellButton != null) { sellButton.onClick.AddListener(OnSellButtonClicked); count++; }
            if (attackUpgradeButton != null) { attackUpgradeButton.onClick.AddListener(OnAttackUpgradeClicked); count++; }
            if (attackSpeedUpgradeButton != null) { attackSpeedUpgradeButton.onClick.AddListener(OnAttackSpeedUpgradeClicked); count++; }
            if (synthesisButton != null) { synthesisButton.onClick.AddListener(OnSynthesisButtonClicked); count++; }

            Debug.Log($"[GameBottomUI] Wired {count} button listeners");
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
            EnsureListeners();
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

        public void SetUnitInfoPanel(UnitInfoPanel infoPanel)
        {
            unitInfoPanel = infoPanel;
        }

        public void SetSelectedUnit(Unit unit)
        {
            selectedUnit = unit;
            Debug.Log($"[GameBottomUI] SetSelectedUnit: {(unit != null ? unit.Data.GetDisplayName() : "NULL")}");

            if (unitInfoPanel != null)
            {
                if (unit != null)
                    unitInfoPanel.Show(unit);
                else
                    unitInfoPanel.Hide();
            }

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
        private void OnSummonClicked()
        {
            if (GameplayManager.Instance == null) return;

            if (UnitManager.Instance == null)
            {
                Debug.LogError("[GameBottomUI] UnitManager not found");
                return;
            }

            if (!UnitManager.Instance.CanDraw())
            {
                StartCoroutine(FlashSummonText("\uBD80\uC871!", "\uACE8\uB4DC \uBD80\uC871"));
                return;
            }

            UnitData drawnUnit = UnitManager.Instance.DrawUnit();
            if (drawnUnit != null)
            {
                if (UnitPlacementManager.Instance != null)
                {
                    UnitPlacementManager.Instance.SelectUnitForPlacement(drawnUnit);
                    StartCoroutine(FlashSummonText(drawnUnit.unitName, "\uBC30\uCE58\uD560 \uC704\uCE58\uB97C \uD130\uCE58!"));
                }
            }
        }

        private IEnumerator FlashSummonText(string mainMsg, string subMsg)
        {
            if (summonButtonText == null) yield break;

            string origText = GetSummonButtonLabel();
            summonButtonText.text = $"{mainMsg}\n<size=11>{subMsg}</size>";

            yield return new WaitForSeconds(1.5f);

            summonButtonText.text = origText;
        }

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
            Debug.Log($"[GameBottomUI] Attack upgrade button clicked! selectedUnit={(selectedUnit != null ? selectedUnit.Data.GetDisplayName() : "NULL")}");
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
            Debug.Log($"[GameBottomUI] Attack speed upgrade button clicked! selectedUnit={(selectedUnit != null ? selectedUnit.Data.GetDisplayName() : "NULL")}");
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
            Debug.Log($"[GameBottomUI] Sell button clicked! selectedUnit={(selectedUnit != null ? selectedUnit.Data.GetDisplayName() : "NULL")}");
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

            // Summon button - always enabled if we can manage and have gold
            if (summonButton != null)
            {
                bool canSummon = canManage && UnitManager.Instance != null && UnitManager.Instance.CanDraw();
                summonButton.interactable = canSummon;

                if (summonButtonText != null)
                {
                    summonButtonText.text = GetSummonButtonLabel();
                }

                UpdateButtonColor(summonButton, summonButton.interactable);
            }

            // Auto Synthesis button - always enabled if possible
            if (autoSynthesisButton != null)
            {
                int possibleSynthesis = AutoSynthesisManager.Instance != null ?
                    AutoSynthesisManager.Instance.GetPossibleSynthesisCount() : 0;

                autoSynthesisButton.interactable = canManage && possibleSynthesis > 0;

                if (autoSynthesisButtonText != null)
                {
                    autoSynthesisButtonText.text = possibleSynthesis > 0
                        ? $"\uC790\uB3D9\uC870\uD569({possibleSynthesis})"
                        : "\uC790\uB3D9\uC870\uD569(0)";
                }

                UpdateButtonColor(autoSynthesisButton, autoSynthesisButton.interactable);
            }

            // Sell button - requires unit selected
            if (sellButton != null)
            {
                sellButton.interactable = canManage && hasUnit;
                if (sellButtonText != null)
                {
                    if (hasUnit && balanceConfig != null)
                    {
                        int sellPrice = balanceConfig.GetSellGold(selectedUnit.Data.rarity);
                        sellButtonText.text = $"\uD310\uB9E4+{sellPrice}G";
                    }
                    else
                    {
                        sellButtonText.text = "\uD310\uB9E4";
                    }
                }
                UpdateButtonColor(sellButton, sellButton.interactable);
            }

            // Attack Upgrade button - requires unit selected
            if (attackUpgradeButton != null)
            {
                int atkLevel = 0;
                if (hasUnit && UnitUpgradeManager.Instance != null)
                    atkLevel = UnitUpgradeManager.Instance.GetRarityAttackLevel(selectedUnit.Data.rarity);

                bool canUpgradeAttack = canManage && hasUnit && atkLevel < 10;

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
                        attackUpgradeButtonText.text = "\uACF5\uACA9\u2191";
                    }
                    else if (atkLevel >= 10)
                    {
                        attackUpgradeButtonText.text = "\uACF5\uACA9 MAX";
                    }
                    else
                    {
                        attackUpgradeButtonText.text = $"\uACF5\uACA9\u2191 Lv{atkLevel}\n{cost}G";
                    }
                }

                UpdateButtonColor(attackUpgradeButton, attackUpgradeButton.interactable);
            }

            // Attack Speed Upgrade button - requires unit selected
            if (attackSpeedUpgradeButton != null)
            {
                int spdLevel = 0;
                if (hasUnit && UnitUpgradeManager.Instance != null)
                    spdLevel = UnitUpgradeManager.Instance.GetRaritySpeedLevel(selectedUnit.Data.rarity);

                bool canUpgradeSpeed = canManage && hasUnit && spdLevel < 10;

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
                        attackSpeedUpgradeButtonText.text = "\uACF5\uC18D\u2191";
                    }
                    else if (spdLevel >= 10)
                    {
                        attackSpeedUpgradeButtonText.text = "\uACF5\uC18D MAX";
                    }
                    else
                    {
                        attackSpeedUpgradeButtonText.text = $"\uACF5\uC18D\u2191 Lv{spdLevel}\n{cost}G";
                    }
                }

                UpdateButtonColor(attackSpeedUpgradeButton, attackSpeedUpgradeButton.interactable);
            }

            // Synthesis button - requires unit selected + recipe exists
            if (synthesisButton != null)
            {
                bool canSynth = false;
                if (hasUnit && balanceConfig != null)
                {
                    var recipe = balanceConfig.GetSynthesisRecipe(selectedUnit.Data.unitName);
                    canSynth = canManage && recipe != null;
                }
                synthesisButton.interactable = canSynth;

                if (synthesisButtonText != null)
                {
                    synthesisButtonText.text = "\uC870\uD569";
                }

                UpdateButtonColor(synthesisButton, synthesisButton.interactable);
            }
        }

        private string GetSummonButtonLabel()
        {
            int cost = balanceConfig != null ? balanceConfig.gameRules.summonCost : 5;
            return $"\uC18C\uD658 {cost}G";
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
