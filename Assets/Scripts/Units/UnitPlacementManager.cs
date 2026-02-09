using UnityEngine;
using System;
using LottoDefense.Grid;
using LottoDefense.Gameplay;

namespace LottoDefense.Units
{
    /// <summary>
    /// Singleton manager responsible for unit placement and swapping mechanics.
    /// Handles click-to-select from inventory, click-to-place on grid, and click-to-swap between placed units.
    /// Enforces placement rules (preparation phase only, valid cells, occupancy).
    /// </summary>
    public class UnitPlacementManager : MonoBehaviour
    {
        #region Singleton
        private static UnitPlacementManager _instance;

        /// <summary>
        /// Global access point for the UnitPlacementManager singleton.
        /// </summary>
        public static UnitPlacementManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<UnitPlacementManager>();

                    if (_instance == null)
                    {
                        GameObject go = new GameObject("UnitPlacementManager");
                        _instance = go.AddComponent<UnitPlacementManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Inspector Fields
        [Header("Visual Feedback Colors")]
        [SerializeField] private Color validPlacementColor = new Color(0f, 1f, 0f, 0.3f);
        [SerializeField] private Color invalidPlacementColor = new Color(1f, 0f, 0f, 0.3f);
        [SerializeField] private Color swappableColor = new Color(1f, 1f, 0f, 0.3f);
        #endregion

        #region Properties
        /// <summary>
        /// Whether placement mode is currently active.
        /// </summary>
        public bool IsPlacementMode { get; private set; }

        /// <summary>
        /// The unit data currently selected from inventory for placement.
        /// </summary>
        public UnitData SelectedUnitData { get; private set; }

        /// <summary>
        /// The placed unit currently selected for swapping.
        /// </summary>
        public Unit SelectedPlacedUnit { get; private set; }
        #endregion

        #region Events
        /// <summary>
        /// Fired when a unit is successfully placed on the grid.
        /// Parameters: placedUnit, gridPosition
        /// </summary>
        public event Action<Unit, Vector2Int> OnUnitPlaced;

        /// <summary>
        /// Fired when two units are successfully swapped.
        /// Parameters: unit1, unit2, pos1, pos2
        /// </summary>
        public event Action<Unit, Unit, Vector2Int, Vector2Int> OnUnitsSwapped;

        /// <summary>
        /// Fired when placement mode is entered.
        /// Parameters: selectedUnitData
        /// </summary>
        public event Action<UnitData> OnPlacementModeEntered;

        /// <summary>
        /// Fired when placement mode is exited.
        /// </summary>
        public event Action OnPlacementModeExited;

        /// <summary>
        /// Fired when a placement action fails.
        /// Parameters: failureReason
        /// </summary>
        public event Action<string> OnPlacementFailed;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            // Cancel placement on ESC key
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelPlacement();
            }

            // Update grid cell highlights in placement mode
            if (IsPlacementMode && SelectedUnitData != null)
            {
                UpdateGridHighlights();
            }
        }

        private void OnEnable()
        {
            // Subscribe to grid cell clicks
            if (GridManager.Instance != null)
            {
                GridManager.Instance.OnCellSelected += OnGridCellClicked;
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from grid cell clicks
            if (GridManager.Instance != null)
            {
                GridManager.Instance.OnCellSelected -= OnGridCellClicked;
            }
        }
        #endregion

        #region Inventory Selection
        /// <summary>
        /// Select a unit from inventory and auto-place in first empty cell.
        /// </summary>
        /// <param name="unitData">Unit data to place</param>
        public void SelectUnitForPlacement(UnitData unitData)
        {
            if (unitData == null)
            {
                Debug.LogWarning("[UnitPlacementManager] Cannot select null unit data");
                return;
            }

            // Validate phase - only allow placement during Preparation
            if (!CanPlaceUnits())
            {
                string reason = "Can only place units during Preparation phase";
                Debug.LogWarning($"[UnitPlacementManager] {reason}");
                OnPlacementFailed?.Invoke(reason);
                return;
            }

            // Clear any previous selection
            CancelPlacement();

            // Auto-place in first empty cell
            Vector2Int? emptyCell = FindFirstEmptyCell();
            if (emptyCell.HasValue)
            {
                SelectedUnitData = unitData;
                IsPlacementMode = true;
                OnPlacementModeEntered?.Invoke(unitData);

                // Immediately place the unit
                PlaceUnit(emptyCell.Value);

                Debug.Log($"[UnitPlacementManager] Auto-placed {unitData.GetDisplayName()} at {emptyCell.Value}");
            }
            else
            {
                string reason = "No empty cells available!";
                Debug.LogWarning($"[UnitPlacementManager] {reason}");
                OnPlacementFailed?.Invoke(reason);
            }
        }

        /// <summary>
        /// Find the first empty cell in the grid (scanning left to right, top to bottom).
        /// </summary>
        private Vector2Int? FindFirstEmptyCell()
        {
            if (GridManager.Instance == null) return null;

            // Scan top to bottom, left to right
            for (int y = GridManager.GRID_HEIGHT - 1; y >= 0; y--)
            {
                for (int x = 0; x < GridManager.GRID_WIDTH; x++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (IsValidPlacement(pos))
                    {
                        return pos;
                    }
                }
            }

            return null; // No empty cells found
        }
        #endregion

        #region Grid Cell Interaction
        /// <summary>
        /// Handle grid cell click events - supports placement and unit movement.
        /// </summary>
        private void OnGridCellClicked(Vector2Int gridPos)
        {
            // Case 1: Unit selected for movement - move to empty cell
            if (SelectedPlacedUnit != null)
            {
                if (UnitManager.Instance != null && UnitManager.Instance.GetUnitAtPosition(gridPos) == null)
                {
                    // Move selected unit to empty cell
                    MoveUnitToPosition(SelectedPlacedUnit, gridPos);
                    DeselectPlacedUnit();
                    return;
                }
            }

            // Case 2: New unit placement mode
            if (!IsPlacementMode || SelectedUnitData == null)
                return;

            // Validate placement
            if (!IsValidPlacement(gridPos))
            {
                string reason = GetPlacementFailureReason(gridPos);
                Debug.LogWarning($"[UnitPlacementManager] Invalid placement at {gridPos}: {reason}");
                OnPlacementFailed?.Invoke(reason);
                return;
            }

            // Place the unit
            PlaceUnit(gridPos);
        }

        /// <summary>
        /// Move a placed unit to a new empty position.
        /// </summary>
        private void MoveUnitToPosition(Unit unit, Vector2Int newPos)
        {
            if (unit == null || UnitManager.Instance == null || GridManager.Instance == null) return;

            Vector2Int oldPos = unit.GridPosition;

            // Update old cell - mark as empty
            GridCell oldCell = GridManager.Instance.GetCell(oldPos);
            if (oldCell != null)
            {
                oldCell.IsOccupied = false;
                oldCell.OccupyingUnit = null;
            }

            // Update new cell - mark as occupied
            GridCell newCell = GridManager.Instance.GetCell(newPos);
            if (newCell != null)
            {
                newCell.IsOccupied = true;
                newCell.OccupyingUnit = unit.gameObject;
            }

            // Update unit position
            Vector3 worldPos = GridManager.Instance.GetCellWorldCenter(newPos);
            unit.transform.position = worldPos;
            unit.SetGridPosition(newPos);

            Debug.Log($"[UnitPlacementManager] Moved {unit.Data.unitName} from {oldPos} to {newPos}");
        }

        /// <summary>
        /// Handle clicks on already-placed units for swapping.
        /// </summary>
        public void OnPlacedUnitClicked(Unit clickedUnit)
        {
            if (clickedUnit == null)
                return;

            // If no unit selected, select this one
            if (SelectedPlacedUnit == null)
            {
                SelectPlacedUnit(clickedUnit);
            }
            // If a unit is already selected, swap them
            else if (SelectedPlacedUnit != clickedUnit)
            {
                SwapUnits(SelectedPlacedUnit, clickedUnit);
            }
            // If same unit clicked, deselect
            else
            {
                DeselectPlacedUnit();
            }
        }
        #endregion

        #region Unit Placement
        /// <summary>
        /// Place the selected unit at the specified grid position.
        /// </summary>
        private void PlaceUnit(Vector2Int gridPos)
        {
            if (SelectedUnitData == null)
                return;

            // Create unit prefab instance
            GameObject unitPrefab = SelectedUnitData.prefab;
            bool isTemplatePrefab = false;
            if (unitPrefab == null)
            {
                // Create default unit GameObject if no prefab
                unitPrefab = CreateDefaultUnitPrefab(SelectedUnitData);
                isTemplatePrefab = true;
            }

            GameObject unitObject = Instantiate(unitPrefab);

            // Destroy the temporary template to avoid memory leak
            if (isTemplatePrefab)
                Destroy(unitPrefab);

            // Add Unit component if not present
            Unit unitComponent = unitObject.GetComponent<Unit>();
            if (unitComponent == null)
            {
                unitComponent = unitObject.AddComponent<Unit>();
            }

            // Initialize unit
            unitComponent.Initialize(SelectedUnitData, gridPos);

            // Place on grid
            if (GridManager.Instance.SetUnit(gridPos, unitObject))
            {
                // Remove from inventory
                UnitManager.Instance.RemoveUnit(SelectedUnitData);

                Debug.Log($"[UnitPlacementManager] Placed {SelectedUnitData.GetDisplayName()} at {gridPos}");
                OnUnitPlaced?.Invoke(unitComponent, gridPos);

                // Exit placement mode
                CancelPlacement();
            }
            else
            {
                // Failed to place - cleanup
                Destroy(unitObject);
                string reason = "Failed to place unit on grid";
                Debug.LogError($"[UnitPlacementManager] {reason}");
                OnPlacementFailed?.Invoke(reason);
            }
        }

        /// <summary>
        /// Create a default unit prefab when none is provided.
        /// Generates a colored circle sprite based on rarity when no icon exists.
        /// </summary>
        private GameObject CreateDefaultUnitPrefab(UnitData data)
        {
            GameObject obj = new GameObject();

            // Add sprite renderer
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            if (data.icon != null)
            {
                sr.sprite = data.icon;
            }
            else
            {
                // Generate circle sprite as placeholder
                sr.sprite = UnitData.CreateCircleSprite(32);
                sr.color = UnitData.GetRarityColor(data.rarity);
            }
            sr.sortingOrder = 10;

            // Add collider for mouse interaction
            BoxCollider2D collider = obj.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one * 0.8f;

            obj.name = $"Unit_{data.unitName}";
            return obj;
        }

        #endregion

        #region Unit Swapping
        /// <summary>
        /// Select a placed unit for swapping.
        /// </summary>
        private void SelectPlacedUnit(Unit unit)
        {
            // Validate phase
            if (!CanPlaceUnits())
            {
                string reason = "Can only swap units during Preparation phase";
                Debug.LogWarning($"[UnitPlacementManager] {reason}");
                OnPlacementFailed?.Invoke(reason);
                return;
            }

            SelectedPlacedUnit = unit;
            unit.Select();
            Debug.Log($"[UnitPlacementManager] Selected {unit.Data.GetDisplayName()} for swapping");
        }

        /// <summary>
        /// Deselect the currently selected placed unit.
        /// </summary>
        private void DeselectPlacedUnit()
        {
            if (SelectedPlacedUnit != null)
            {
                SelectedPlacedUnit.Deselect();
                SelectedPlacedUnit = null;
                Debug.Log("[UnitPlacementManager] Deselected placed unit");
            }
        }

        /// <summary>
        /// Swap positions of two placed units.
        /// </summary>
        private void SwapUnits(Unit unit1, Unit unit2)
        {
            if (unit1 == null || unit2 == null)
                return;

            Vector2Int pos1 = unit1.GridPosition;
            Vector2Int pos2 = unit2.GridPosition;

            // Remove both units from grid
            GridManager.Instance.RemoveUnit(pos1);
            GridManager.Instance.RemoveUnit(pos2);

            // Update unit positions
            unit1.MoveTo(pos2);
            unit2.MoveTo(pos1);

            // Place units at swapped positions
            GridManager.Instance.SetUnit(pos2, unit1.gameObject);
            GridManager.Instance.SetUnit(pos1, unit2.gameObject);

            Debug.Log($"[UnitPlacementManager] Swapped {unit1.Data.GetDisplayName()} at {pos1} with {unit2.Data.GetDisplayName()} at {pos2}");
            OnUnitsSwapped?.Invoke(unit1, unit2, pos1, pos2);

            // Deselect after swap
            DeselectPlacedUnit();
        }
        #endregion

        #region Placement Validation
        /// <summary>
        /// Check if units can currently be placed (must be in Preparation phase).
        /// </summary>
        public bool CanPlaceUnits()
        {
            return GameplayManager.Instance.CurrentState == GameState.Preparation;
        }

        /// <summary>
        /// Validate if a unit can be placed at the specified position.
        /// </summary>
        public bool IsValidPlacement(Vector2Int gridPos)
        {
            // Must be in valid phase
            if (!CanPlaceUnits())
                return false;

            // Must be valid grid position
            if (!GridManager.Instance.IsValidPosition(gridPos))
                return false;

            // Must be a placement cell (not on path)
            if (!GridManager.Instance.IsPlacementCell(gridPos))
                return false;

            // Must not be occupied
            if (GridManager.Instance.IsOccupied(gridPos))
                return false;

            return true;
        }

        /// <summary>
        /// Get a descriptive reason why placement failed at the specified position.
        /// </summary>
        private string GetPlacementFailureReason(Vector2Int gridPos)
        {
            if (!CanPlaceUnits())
                return "Can only place units during Preparation phase";

            if (!GridManager.Instance.IsValidPosition(gridPos))
                return "Position is outside grid bounds";

            if (!GridManager.Instance.IsPlacementCell(gridPos))
                return "Cannot place units on monster paths";

            if (GridManager.Instance.IsOccupied(gridPos))
                return "Cell is already occupied";

            return "Unknown error";
        }
        #endregion

        #region Visual Feedback
        /// <summary>
        /// Update grid cell highlights based on current placement mode.
        /// </summary>
        private void UpdateGridHighlights()
        {
            // This would highlight all valid/invalid cells
            // For now, individual cells handle highlights on hover via GridCell.OnMouseEnter
            // Future enhancement: Show all valid cells when in placement mode
        }

        /// <summary>
        /// Highlight a specific cell with a color based on placement validity.
        /// </summary>
        public void HighlightCell(Vector2Int gridPos, bool isValid)
        {
            GridCell cell = GridManager.Instance.GetCellAt(gridPos);
            if (cell != null)
            {
                Color highlightColor = isValid ? validPlacementColor : invalidPlacementColor;
                cell.Highlight(highlightColor);
            }
        }

        /// <summary>
        /// Clear highlights from a specific cell.
        /// </summary>
        public void ClearCellHighlight(Vector2Int gridPos)
        {
            GridCell cell = GridManager.Instance.GetCellAt(gridPos);
            if (cell != null)
            {
                cell.ResetHighlight();
            }
        }
        #endregion

        #region Placement Mode Control
        /// <summary>
        /// Cancel placement mode and clear selections.
        /// </summary>
        public void CancelPlacement()
        {
            if (IsPlacementMode)
            {
                Debug.Log("[UnitPlacementManager] Placement cancelled");
            }

            SelectedUnitData = null;
            IsPlacementMode = false;
            DeselectPlacedUnit();

            OnPlacementModeExited?.Invoke();
        }
        #endregion

        #region Debug Utilities
        /// <summary>
        /// Get current placement state for debugging.
        /// </summary>
        public string GetPlacementStateInfo()
        {
            if (IsPlacementMode && SelectedUnitData != null)
            {
                return $"Placement Mode: Active - {SelectedUnitData.GetDisplayName()}";
            }
            else if (SelectedPlacedUnit != null)
            {
                return $"Swap Mode: Selected {SelectedPlacedUnit.Data.GetDisplayName()}";
            }
            else
            {
                return "Placement Mode: Inactive";
            }
        }
        #endregion
    }
}
