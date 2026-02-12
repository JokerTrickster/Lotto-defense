using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using LottoDefense.Units;
using LottoDefense.Gameplay;
using LottoDefense.Grid;

namespace LottoDefense.UI
{
    /// <summary>
    /// Compact floating panel above selected unit with sell + synthesis buttons.
    /// Sell price is based on unit rarity.
    /// Synthesis button shown when a recipe exists; floating buttons appear over compatible units.
    /// </summary>
    public class UnitSelectionUI : MonoBehaviour
    {
        #region Serialized Fields
        [Header("UI References")]
        [SerializeField] private GameObject selectionPanel;
        [SerializeField] private Button sellButton;
        [SerializeField] private Button synthesisButton;
        [SerializeField] private Text unitNameText;
        [SerializeField] private Text sellButtonText;
        [SerializeField] private Text synthesisButtonText;
        #endregion

        #region Private Fields
        private Unit selectedUnit;
        private GameBalanceConfig balanceConfig;
        private readonly List<SynthesisButtonController> activeSynthesisButtons = new List<SynthesisButtonController>();
        private bool listenersInitialized;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            balanceConfig = Resources.Load<GameBalanceConfig>("GameBalanceConfig");
            if (balanceConfig == null)
            {
                Debug.LogError("[UnitSelectionUI] GameBalanceConfig not found in Resources!");
            }
        }

        private void OnDestroy()
        {
            ClearSynthesisButtons();
        }

        private void Update()
        {
            // Dismiss panel when clicking empty space
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

            // Track selected unit position each frame
            if (selectedUnit != null && selectionPanel != null && selectionPanel.activeSelf)
            {
                PositionPanelNearUnit(selectedUnit);
            }
        }
        #endregion

        #region Initialization
        private void EnsureListeners()
        {
            if (listenersInitialized) return;
            listenersInitialized = true;

            if (sellButton != null)
                sellButton.onClick.AddListener(OnSellButtonClicked);
            if (synthesisButton != null)
                synthesisButton.onClick.AddListener(OnSynthesisDirectClicked);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Show compact panel above the unit with sell + synthesis buttons.
        /// Also shows floating synthesis buttons over compatible units.
        /// </summary>
        public void ShowForUnit(Unit unit)
        {
            if (unit == null) return;

            if (balanceConfig == null)
            {
                balanceConfig = Resources.Load<GameBalanceConfig>("GameBalanceConfig");
            }
            if (balanceConfig == null) return;

            EnsureListeners();

            selectedUnit = unit;

            // Update unit name with rarity color
            if (unitNameText != null)
            {
                unitNameText.text = unit.Data.GetDisplayName();
                unitNameText.color = UnitData.GetRarityColor(unit.Data.rarity);
            }

            // Update sell button with rarity-based price
            int sellPrice = balanceConfig.GetSellGold(unit.Data.rarity);
            if (sellButtonText != null)
            {
                sellButtonText.text = $"\uD310\uB9E4 (+{sellPrice}G)";
            }

            // Update synthesis button - show if recipe exists
            var recipe = balanceConfig.GetSynthesisRecipe(unit.Data.unitName);
            bool hasRecipe = recipe != null;
            if (synthesisButton != null)
            {
                synthesisButton.gameObject.SetActive(hasRecipe);
            }
            if (synthesisButtonText != null && hasRecipe)
            {
                synthesisButtonText.text = "\uC870\uD569";
            }

            PositionPanelNearUnit(unit);

            if (selectionPanel != null)
            {
                selectionPanel.SetActive(true);
            }

            // Notify bottom UI
            GameBottomUI bottomUI = FindFirstObjectByType<GameBottomUI>();
            if (bottomUI != null)
            {
                bottomUI.SetSelectedUnit(unit);
                bottomUI.Show();
            }

            // Show floating synthesis buttons over compatible units
            ClearSynthesisButtons();
            if (hasRecipe)
            {
                ShowSynthesisButtons(unit);
            }

            Debug.Log($"[UnitSelectionUI] Showing UI for {unit.Data.GetDisplayName()} (sell: {sellPrice}G, recipe: {hasRecipe})");
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

            if (UnitPlacementManager.Instance != null)
            {
                UnitPlacementManager.Instance.CancelPlacement();
            }

            selectedUnit = null;
            ClearSynthesisButtons();
        }
        #endregion

        #region Synthesis Buttons
        private void ShowSynthesisButtons(Unit source)
        {
            if (GridManager.Instance == null || balanceConfig == null) return;

            var recipe = balanceConfig.GetSynthesisRecipe(source.Data.unitName);
            if (recipe == null) return;

            Canvas canvas = FindGameCanvas();
            if (canvas == null) return;

            for (int x = 0; x < GridManager.GRID_WIDTH; x++)
            {
                for (int y = 0; y < GridManager.GRID_HEIGHT; y++)
                {
                    Unit candidate = GridManager.Instance.GetUnitAt(x, y);
                    if (candidate == null) continue;
                    if (candidate == source) continue;
                    if (candidate.Data.unitName != source.Data.unitName) continue;

                    GameObject btnObj = new GameObject($"SynthesisBtn_{x}_{y}");
                    btnObj.transform.SetParent(canvas.transform, false);

                    SynthesisButtonController controller = btnObj.AddComponent<SynthesisButtonController>();
                    controller.Initialize(source, candidate, OnSynthesisFloatingClicked);
                    activeSynthesisButtons.Add(controller);
                }
            }

            if (activeSynthesisButtons.Count > 0)
            {
                Debug.Log($"[UnitSelectionUI] Showing {activeSynthesisButtons.Count} synthesis button(s) for {source.Data.unitName}");
            }
        }

        /// <summary>
        /// Handle floating synthesis button click (over a compatible unit).
        /// </summary>
        private void OnSynthesisFloatingClicked(Unit source, Unit target)
        {
            if (source == null || target == null) return;

            if (SynthesisManager.Instance == null)
            {
                Debug.LogError("[UnitSelectionUI] SynthesisManager not found!");
                return;
            }

            bool success = SynthesisManager.Instance.TrySynthesize(source, target);
            if (success)
            {
                Debug.Log("[UnitSelectionUI] 1-click synthesis successful!");
            }
            else
            {
                Debug.LogWarning("[UnitSelectionUI] 1-click synthesis failed");
            }

            HideUI();
        }

        private void ClearSynthesisButtons()
        {
            for (int i = activeSynthesisButtons.Count - 1; i >= 0; i--)
            {
                if (activeSynthesisButtons[i] != null)
                {
                    Destroy(activeSynthesisButtons[i].gameObject);
                }
            }
            activeSynthesisButtons.Clear();
        }

        private Canvas FindGameCanvas()
        {
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas canvas in canvases)
            {
                if (canvas.gameObject.name == "GameCanvas")
                {
                    return canvas;
                }
            }
            Debug.LogWarning("[UnitSelectionUI] GameCanvas not found for synthesis buttons");
            return null;
        }
        #endregion

        #region Button Handlers
        /// <summary>
        /// Synthesis button in the panel clicked - find first compatible unit and synthesize.
        /// </summary>
        private void OnSynthesisDirectClicked()
        {
            if (selectedUnit == null || balanceConfig == null) return;

            if (SynthesisManager.Instance == null)
            {
                Debug.LogError("[UnitSelectionUI] SynthesisManager not found!");
                return;
            }

            // Find a compatible unit on the grid
            Unit target = FindCompatibleUnit(selectedUnit);
            if (target == null)
            {
                Debug.LogWarning("[UnitSelectionUI] No compatible unit found for synthesis");
                return;
            }

            bool success = SynthesisManager.Instance.TrySynthesize(selectedUnit, target);
            if (success)
            {
                Debug.Log("[UnitSelectionUI] Panel synthesis successful!");
            }
            else
            {
                Debug.LogWarning("[UnitSelectionUI] Panel synthesis failed");
            }

            HideUI();
        }

        private void OnSellButtonClicked()
        {
            if (selectedUnit == null || balanceConfig == null) return;

            int sellPrice = balanceConfig.GetSellGold(selectedUnit.Data.rarity);
            Debug.Log($"[UnitSelectionUI] Selling {selectedUnit.Data.GetDisplayName()} ({selectedUnit.Data.rarity}) for {sellPrice} gold");

            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.ModifyGold(sellPrice);
            }

            if (GridManager.Instance != null)
            {
                GridManager.Instance.RemoveUnit(selectedUnit.GridPosition);
            }

            Destroy(selectedUnit.gameObject);
            HideUI();
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Find a compatible unit on the grid for synthesis (same name, different instance).
        /// </summary>
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

        private void PositionPanelNearUnit(Unit unit)
        {
            if (selectionPanel == null || unit == null) return;

            Vector3 worldPos = unit.transform.position + Vector3.up * 0.45f;
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
