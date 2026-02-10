using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using LottoDefense.Units;
using LottoDefense.Gameplay;
using LottoDefense.Grid;

namespace LottoDefense.UI
{
    /// <summary>
    /// Displays sell/synthesize buttons when a unit is selected.
    /// Handles unit selling (+3 gold) and synthesis (3 same units → 1 upgraded unit).
    /// </summary>
    public class UnitSelectionUI : MonoBehaviour
    {
        #region Serialized Fields
        [Header("UI References")]
        [SerializeField] private GameObject selectionPanel;
        [SerializeField] private Button sellButton;
        [SerializeField] private Button synthesizeButton;
        [SerializeField] private Text unitNameText;
        [SerializeField] private Text sellButtonText;
        [SerializeField] private Text synthesizeButtonText;
        #endregion

        #region Private Fields
        private Unit selectedUnit;
        private GameBalanceConfig balanceConfig;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Load balance config
            balanceConfig = Resources.Load<GameBalanceConfig>("GameBalanceConfig");
            if (balanceConfig == null)
            {
                Debug.LogError("[UnitSelectionUI] GameBalanceConfig not found in Resources!");
            }

            // Setup button listeners
            if (sellButton != null)
            {
                sellButton.onClick.AddListener(OnSellButtonClicked);
            }

            if (synthesizeButton != null)
            {
                synthesizeButton.onClick.AddListener(OnSynthesizeButtonClicked);
            }

            // Start hidden
            if (selectionPanel != null)
            {
                selectionPanel.SetActive(false);
            }
        }

        private void Update()
        {
            // Hide UI if clicked outside (background click)
            if (Input.GetMouseButtonDown(0) && selectionPanel != null && selectionPanel.activeSelf)
            {
                // Check if clicking on UI element (button, panel, etc.)
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    return; // Don't hide if clicking on UI
                }

                // Check if clicking on a unit
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

                if (hit.collider != null)
                {
                    // Check if we hit a unit
                    Unit clickedUnit = hit.collider.GetComponent<Unit>();
                    if (clickedUnit != null)
                    {
                        return; // Don't hide if clicking on a unit (let OnMouseDown handle it)
                    }
                }

                // If we reach here, we clicked on background → hide UI
                HideUI();
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Show selection UI for a unit.
        /// Buttons are disabled during Combat phase.
        /// </summary>
        public void ShowForUnit(Unit unit)
        {
            if (unit == null || balanceConfig == null) return;

            selectedUnit = unit;

            // Check if we can manage units (Preparation phase only)
            bool canManage = CanManageUnits();

            // Update UI text
            if (unitNameText != null)
            {
                unitNameText.text = unit.Data.GetDisplayName();
            }

            // Sell button
            if (sellButton != null)
            {
                sellButton.interactable = canManage;
            }

            if (sellButtonText != null)
            {
                if (canManage)
                {
                    sellButtonText.text = $"판매 (+{balanceConfig.unitSellGold} 골드)";
                }
                else
                {
                    sellButtonText.text = "준비 시간에만 가능";
                }
            }

            // Synthesize button
            bool canSynthesize = CanSynthesizeSelectedUnit();
            if (synthesizeButton != null)
            {
                synthesizeButton.interactable = canSynthesize;
            }

            if (synthesizeButtonText != null)
            {
                if (!canManage)
                {
                    synthesizeButtonText.text = "준비 시간에만 가능";
                }
                else if (canSynthesize)
                {
                    var recipe = balanceConfig.GetSynthesisRecipe(unit.Data.unitName);
                    synthesizeButtonText.text = $"조합 → {recipe.resultUnitName}";
                }
                else
                {
                    synthesizeButtonText.text = "조합 불가 (3개 필요)";
                }
            }

            // Position panel near unit
            PositionPanelNearUnit(unit);

            // Show panel
            if (selectionPanel != null)
            {
                selectionPanel.SetActive(true);
            }

            // Update GameBottomUI with selected unit
            GameBottomUI bottomUI = FindFirstObjectByType<GameBottomUI>();
            if (bottomUI != null)
            {
                bottomUI.SetSelectedUnit(unit);
                bottomUI.Show();
            }

            Debug.Log($"[UnitSelectionUI] Showing UI for {unit.Data.GetDisplayName()} (CanManage: {canManage})");
        }

        /// <summary>
        /// Hide selection UI.
        /// </summary>
        public void HideUI()
        {
            if (selectionPanel != null)
            {
                selectionPanel.SetActive(false);
            }

            // Clear selected unit from GameBottomUI
            GameBottomUI bottomUI = FindFirstObjectByType<GameBottomUI>();
            if (bottomUI != null)
            {
                bottomUI.SetSelectedUnit(null);
            }

            selectedUnit = null;
        }
        #endregion

        #region Button Handlers
        /// <summary>
        /// Handle sell button click.
        /// Only allowed during Preparation phase.
        /// </summary>
        private void OnSellButtonClicked()
        {
            if (selectedUnit == null || balanceConfig == null) return;

            // Check if we're in Preparation phase
            if (!CanManageUnits())
            {
                Debug.LogWarning("[UnitSelectionUI] Cannot sell units outside Preparation phase!");
                return;
            }

            Debug.Log($"[UnitSelectionUI] Selling {selectedUnit.Data.GetDisplayName()} for {balanceConfig.unitSellGold} gold");

            // Add gold
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.ModifyGold(balanceConfig.unitSellGold);
            }

            // Remove from grid
            if (GridManager.Instance != null)
            {
                GridManager.Instance.RemoveUnit(selectedUnit.GridPosition);
            }

            // Destroy unit GameObject
            Destroy(selectedUnit.gameObject);

            // Hide UI
            HideUI();
        }

        /// <summary>
        /// Handle synthesize button click.
        /// Only allowed during Preparation phase.
        /// </summary>
        private void OnSynthesizeButtonClicked()
        {
            if (selectedUnit == null || balanceConfig == null) return;

            // Check if we're in Preparation phase
            if (!CanManageUnits())
            {
                Debug.LogWarning("[UnitSelectionUI] Cannot synthesize units outside Preparation phase!");
                return;
            }

            // Get synthesis recipe
            var recipe = balanceConfig.GetSynthesisRecipe(selectedUnit.Data.unitName);
            if (recipe == null)
            {
                Debug.LogWarning($"[UnitSelectionUI] No synthesis recipe for {selectedUnit.Data.unitName}");
                return;
            }

            // Find SynthesisManager
            SynthesisManager synthesisManager = FindFirstObjectByType<SynthesisManager>();
            if (synthesisManager == null)
            {
                Debug.LogError("[UnitSelectionUI] SynthesisManager not found!");
                return;
            }

            // Try to synthesize
            bool success = synthesisManager.TrySynthesize(selectedUnit);

            if (success)
            {
                Debug.Log($"[UnitSelectionUI] Synthesis successful: {selectedUnit.Data.unitName} → {recipe.resultUnitName}");
            }
            else
            {
                Debug.LogWarning($"[UnitSelectionUI] Synthesis failed for {selectedUnit.Data.unitName}");
            }

            // Hide UI regardless of result
            HideUI();
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Check if units can be managed (sold/synthesized).
        /// Only allowed during Preparation phase.
        /// </summary>
        private bool CanManageUnits()
        {
            return GameplayManager.Instance != null &&
                   GameplayManager.Instance.CurrentState == GameState.Preparation;
        }

        /// <summary>
        /// Check if selected unit can be synthesized.
        /// </summary>
        private bool CanSynthesizeSelectedUnit()
        {
            if (selectedUnit == null || balanceConfig == null) return false;

            // Must be in Preparation phase
            if (!CanManageUnits()) return false;

            // Check if recipe exists
            var recipe = balanceConfig.GetSynthesisRecipe(selectedUnit.Data.unitName);
            if (recipe == null) return false;

            // Count same units
            int sameUnitCount = CountSameUnits(selectedUnit.Data.unitName);

            return sameUnitCount >= 3;
        }

        /// <summary>
        /// Count units with the same name.
        /// </summary>
        private int CountSameUnits(string unitName)
        {
            if (UnitManager.Instance == null) return 0;

            var placedUnits = UnitManager.Instance.GetPlacedUnits();
            int count = 0;

            foreach (var unit in placedUnits)
            {
                if (unit != null && unit.Data != null && unit.Data.unitName == unitName)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Position panel near the selected unit.
        /// </summary>
        private void PositionPanelNearUnit(Unit unit)
        {
            if (selectionPanel == null || unit == null) return;

            // Convert unit world position to screen position
            Vector3 worldPos = unit.transform.position + Vector3.up * 0.5f; // Slightly above unit
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            // Set panel position
            RectTransform panelRect = selectionPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                panelRect.position = screenPos;
            }
        }
        #endregion
    }
}
