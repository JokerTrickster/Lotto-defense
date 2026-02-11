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
    /// Handles unit selling (+3 gold) and synthesis (2 same units → 1 upgraded unit).
    /// Two-step selection: select unit 1, then unit 2, then press synthesize.
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
        [SerializeField] private Text slot1Text;
        [SerializeField] private Text slot2Text;
        [SerializeField] private Button slot1Button;
        [SerializeField] private Button slot2Button;
        #endregion

        #region Private Fields
        private Unit selectedUnit1;
        private Unit selectedUnit2;
        private bool isSelectingSlot2;
        private GameBalanceConfig balanceConfig;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            balanceConfig = Resources.Load<GameBalanceConfig>("GameBalanceConfig");
            if (balanceConfig == null)
            {
                Debug.LogError("[UnitSelectionUI] GameBalanceConfig not found in Resources!");
            }

            if (sellButton != null)
            {
                sellButton.onClick.AddListener(OnSellButtonClicked);
            }

            if (synthesizeButton != null)
            {
                synthesizeButton.onClick.AddListener(OnSynthesizeButtonClicked);
            }

            if (slot1Button != null)
            {
                slot1Button.onClick.AddListener(OnSlot1Clicked);
            }

            if (slot2Button != null)
            {
                slot2Button.onClick.AddListener(OnSlot2Clicked);
            }

            if (selectionPanel != null)
            {
                selectionPanel.SetActive(false);
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && selectionPanel != null && selectionPanel.activeSelf)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

                if (hit.collider != null)
                {
                    Unit clickedUnit = hit.collider.GetComponent<Unit>();
                    if (clickedUnit != null)
                    {
                        return;
                    }
                }

                HideUI();
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Show selection UI for a unit. If in slot2 selection mode, assign to slot2.
        /// </summary>
        public void ShowForUnit(Unit unit)
        {
            if (unit == null || balanceConfig == null) return;

            // If selecting second unit for synthesis
            if (isSelectingSlot2 && selectedUnit1 != null)
            {
                if (unit == selectedUnit1)
                {
                    Debug.LogWarning("[UnitSelectionUI] Cannot select same unit for both slots");
                    return;
                }

                selectedUnit2 = unit;
                isSelectingSlot2 = false;
                UpdateUI();
                return;
            }

            // First selection - set as slot 1
            selectedUnit1 = unit;
            selectedUnit2 = null;
            isSelectingSlot2 = false;

            bool canManage = CanManageUnits();

            if (unitNameText != null)
            {
                unitNameText.text = unit.Data.GetDisplayName();
            }

            if (sellButton != null)
            {
                sellButton.interactable = canManage;
            }

            if (sellButtonText != null)
            {
                sellButtonText.text = canManage
                    ? $"판매 (+{balanceConfig.unitSellGold} 골드)"
                    : "준비 시간에만 가능";
            }

            UpdateUI();
            PositionPanelNearUnit(unit);

            if (selectionPanel != null)
            {
                selectionPanel.SetActive(true);
            }

            GameBottomUI bottomUI = FindFirstObjectByType<GameBottomUI>();
            if (bottomUI != null)
            {
                bottomUI.SetSelectedUnit(unit);
                bottomUI.Show();
            }

            Debug.Log($"[UnitSelectionUI] Showing UI for {unit.Data.GetDisplayName()}");
        }

        /// <summary>
        /// Hide selection UI and reset state.
        /// </summary>
        public void HideUI()
        {
            if (selectionPanel != null)
            {
                selectionPanel.SetActive(false);
            }

            GameBottomUI bottomUI = FindFirstObjectByType<GameBottomUI>();
            if (bottomUI != null)
            {
                bottomUI.SetSelectedUnit(null);
            }

            selectedUnit1 = null;
            selectedUnit2 = null;
            isSelectingSlot2 = false;
        }
        #endregion

        #region Button Handlers
        private void OnSlot1Clicked()
        {
            // Slot 1 is already selected when ShowForUnit is called
            // Clicking slot 1 clears it
            if (selectedUnit1 != null)
            {
                selectedUnit1 = null;
                selectedUnit2 = null;
                isSelectingSlot2 = false;
                UpdateUI();
            }
        }

        private void OnSlot2Clicked()
        {
            if (selectedUnit1 == null) return;

            if (selectedUnit2 != null)
            {
                // Clear slot 2
                selectedUnit2 = null;
                isSelectingSlot2 = false;
                UpdateUI();
            }
            else
            {
                // Enter slot 2 selection mode
                isSelectingSlot2 = true;
                UpdateUI();
            }
        }

        private void OnSellButtonClicked()
        {
            if (selectedUnit1 == null || balanceConfig == null) return;

            if (!CanManageUnits())
            {
                Debug.LogWarning("[UnitSelectionUI] Cannot sell units outside Preparation phase!");
                return;
            }

            Debug.Log($"[UnitSelectionUI] Selling {selectedUnit1.Data.GetDisplayName()} for {balanceConfig.unitSellGold} gold");

            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.ModifyGold(balanceConfig.unitSellGold);
            }

            if (GridManager.Instance != null)
            {
                GridManager.Instance.RemoveUnit(selectedUnit1.GridPosition);
            }

            Destroy(selectedUnit1.gameObject);
            HideUI();
        }

        private void OnSynthesizeButtonClicked()
        {
            if (selectedUnit1 == null || selectedUnit2 == null || balanceConfig == null) return;

            if (!CanManageUnits())
            {
                Debug.LogWarning("[UnitSelectionUI] Cannot synthesize units outside Preparation phase!");
                return;
            }

            if (SynthesisManager.Instance == null)
            {
                Debug.LogError("[UnitSelectionUI] SynthesisManager not found!");
                return;
            }

            bool success = SynthesisManager.Instance.TrySynthesize(selectedUnit1, selectedUnit2);

            if (success)
            {
                Debug.Log($"[UnitSelectionUI] Synthesis successful!");
            }
            else
            {
                Debug.LogWarning($"[UnitSelectionUI] Synthesis failed");
            }

            HideUI();
        }
        #endregion

        #region Helper Methods
        private bool CanManageUnits()
        {
            return GameplayManager.Instance != null &&
                   GameplayManager.Instance.CurrentState == GameState.Preparation;
        }

        /// <summary>
        /// Update slot texts and synthesize button state.
        /// </summary>
        private void UpdateUI()
        {
            bool canManage = CanManageUnits();

            // Slot 1 text
            if (slot1Text != null)
            {
                slot1Text.text = selectedUnit1 != null
                    ? $"유닛 1: {selectedUnit1.Data.unitName}"
                    : "유닛 선택 1: 비어있음";
            }

            // Slot 2 text
            if (slot2Text != null)
            {
                if (isSelectingSlot2)
                {
                    slot2Text.text = "유닛 선택 2: 유닛을 터치하세요";
                }
                else if (selectedUnit2 != null)
                {
                    slot2Text.text = $"유닛 2: {selectedUnit2.Data.unitName}";
                }
                else
                {
                    slot2Text.text = "유닛 선택 2: 비어있음";
                }
            }

            // Synthesize button
            bool canSynthesize = CanSynthesizeCurrentSelection();
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
                else if (selectedUnit1 != null && selectedUnit2 != null)
                {
                    if (selectedUnit1.Data.unitName != selectedUnit2.Data.unitName)
                    {
                        synthesizeButtonText.text = "같은 유닛만 조합 가능";
                    }
                    else if (canSynthesize)
                    {
                        var recipe = balanceConfig.GetSynthesisRecipe(selectedUnit1.Data.unitName);
                        UnitData resultData = Resources.Load<UnitData>($"Units/{recipe.resultUnitName}");
                        string displayName = resultData != null ? resultData.unitName : recipe.resultUnitName;
                        synthesizeButtonText.text = $"조합 → {displayName}";
                    }
                    else
                    {
                        synthesizeButtonText.text = "조합 불가";
                    }
                }
                else
                {
                    synthesizeButtonText.text = "유닛 2개를 선택하세요";
                }
            }
        }

        private bool CanSynthesizeCurrentSelection()
        {
            if (selectedUnit1 == null || selectedUnit2 == null || balanceConfig == null) return false;
            if (!CanManageUnits()) return false;
            if (selectedUnit1.Data.unitName != selectedUnit2.Data.unitName) return false;

            var recipe = balanceConfig.GetSynthesisRecipe(selectedUnit1.Data.unitName);
            return recipe != null;
        }

        private void PositionPanelNearUnit(Unit unit)
        {
            if (selectionPanel == null || unit == null) return;

            Vector3 worldPos = unit.transform.position + Vector3.up * 0.5f;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            RectTransform panelRect = selectionPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                panelRect.position = screenPos;
            }
        }
        #endregion
    }
}
