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
    /// Floating name label above selected unit + floating synthesis buttons over compatible units on grid.
    /// Sell and synthesis actions are handled by GameBottomUI.
    /// </summary>
    public class UnitSelectionUI : MonoBehaviour
    {
        #region Serialized Fields
        [Header("UI References")]
        [SerializeField] private GameObject selectionPanel;
        [SerializeField] private Text unitNameText;
        #endregion

        #region Private Fields
        private Unit selectedUnit;
        private GameBalanceConfig balanceConfig;
        private readonly List<SynthesisButtonController> activeSynthesisButtons = new List<SynthesisButtonController>();
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            balanceConfig = Resources.Load<GameBalanceConfig>("GameBalanceConfig");
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
                // Check if pointer is over any UI element (mobile-safe)
                if (IsPointerOverUI())
                    return;

                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

                if (hit.collider != null)
                {
                    Unit clickedUnit = hit.collider.GetComponent<Unit>();
                    if (clickedUnit != null)
                        return;
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

        #region Public Methods
        public void ShowForUnit(Unit unit)
        {
            if (unit == null) return;

            if (balanceConfig == null)
                balanceConfig = Resources.Load<GameBalanceConfig>("GameBalanceConfig");

            selectedUnit = unit;

            // Update unit name with rarity color
            if (unitNameText != null)
            {
                unitNameText.text = unit.Data.GetDisplayName();
                unitNameText.color = UnitData.GetRarityColor(unit.Data.rarity);
            }

            PositionPanelNearUnit(unit);

            if (selectionPanel != null)
                selectionPanel.SetActive(true);

            // Notify bottom UI
            GameBottomUI bottomUI = FindFirstObjectByType<GameBottomUI>();
            if (bottomUI != null)
            {
                bottomUI.SetSelectedUnit(unit);
            }

            // Show floating synthesis buttons over compatible units
            ClearSynthesisButtons();
            if (balanceConfig != null)
            {
                var recipe = balanceConfig.GetSynthesisRecipe(unit.Data.unitName);
                if (recipe != null)
                {
                    ShowSynthesisButtons(unit);
                }
            }

            Debug.Log($"[UnitSelectionUI] Showing UI for {unit.Data.GetDisplayName()}");
        }

        public void HideUI()
        {
            if (selectionPanel != null)
                selectionPanel.SetActive(false);

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
                Debug.Log($"[UnitSelectionUI] Showing {activeSynthesisButtons.Count} synthesis button(s)");
            }
        }

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
                    return canvas;
            }
            return null;
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Mobile-safe check for pointer over UI.
        /// IsPointerOverGameObject() without fingerId is unreliable on mobile.
        /// </summary>
        private bool IsPointerOverUI()
        {
            if (EventSystem.current == null) return false;

            // Mobile touch: use fingerId for reliable detection
            if (Input.touchCount > 0)
            {
                return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
            }

            // Desktop mouse
            return EventSystem.current.IsPointerOverGameObject();
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
