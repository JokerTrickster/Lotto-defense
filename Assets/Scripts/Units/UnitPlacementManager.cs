using UnityEngine;
using System;
using System.Collections;
using LottoDefense.Grid;
using LottoDefense.Gameplay;
using LottoDefense.UI;
using LottoDefense.Utils;
using LottoDefense.VFX;

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
                if (GameplayManager.IsCleaningUp) return null;

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

        /// <summary>
        /// Empty cell selected as a move destination (tap empty cell first, then tap unit).
        /// </summary>
        private Vector2Int? pendingEmptyCell;
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

            // Validate GameplayManager is available
            if (!CanPlaceUnits())
            {
                string reason = "GameplayManager not available";
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
            // Case 1: Unit already selected → move/swap to clicked cell
            if (SelectedPlacedUnit != null && GridManager.Instance != null)
            {
                Unit occupant = GridManager.Instance.GetUnitAt(gridPos);
                if (occupant == null)
                {
                    // Move selected unit to empty cell
                    MoveUnitToPosition(SelectedPlacedUnit, gridPos);
                    DeselectPlacedUnit();
                    ClearPendingEmptyCell();
                    return;
                }
                else if (occupant != SelectedPlacedUnit)
                {
                    // Swap with occupant unit
                    SwapUnits(SelectedPlacedUnit, occupant);
                    ClearPendingEmptyCell();
                    return;
                }
                else
                {
                    // Clicked same unit's cell - deselect
                    DeselectPlacedUnit();
                    ClearPendingEmptyCell();
                    return;
                }
            }

            // Case 2: Empty cell clicked with no unit selected → remember as pending destination
            if (GridManager.Instance != null && !GridManager.Instance.IsOccupied(gridPos)
                && GridManager.Instance.IsPlacementCell(gridPos))
            {
                // If not in placement mode, store as pending empty cell
                if (!IsPlacementMode || SelectedUnitData == null)
                {
                    SetPendingEmptyCell(gridPos);
                    return;
                }
            }

            // Case 3: New unit placement mode
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
            if (unit == null || GridManager.Instance == null) return;

            Vector2Int oldPos = unit.GridPosition;

            // Remove unit from old position in grid
            GridManager.Instance.RemoveUnit(oldPos);

            // Update unit's grid position
            unit.MoveTo(newPos);

            // Place unit at new position in grid
            GridManager.Instance.SetUnit(newPos, unit.gameObject);

        }

        /// <summary>
        /// Handle clicks on already-placed units.
        /// Shows UnitSelectionUI for sell/synthesize options.
        /// </summary>
        public void OnPlacedUnitClicked(Unit clickedUnit)
        {
            if (clickedUnit == null)
                return;

            // If an empty cell was pre-selected, move this unit there immediately
            if (pendingEmptyCell.HasValue)
            {
                Vector2Int target = pendingEmptyCell.Value;
                ClearPendingEmptyCell();
                MoveUnitToPosition(clickedUnit, target);
                DeselectPlacedUnit();
                return;
            }

            // If another unit is already selected for movement, swap them
            if (SelectedPlacedUnit != null && SelectedPlacedUnit != clickedUnit)
            {
                SwapUnits(SelectedPlacedUnit, clickedUnit);
                return;
            }

            // Toggle: clicking the same unit again deselects it
            if (SelectedPlacedUnit == clickedUnit)
            {
                DeselectPlacedUnit();

                UnitSelectionUI selectionUI2 = FindFirstObjectByType<UnitSelectionUI>();
                if (selectionUI2 != null)
                    selectionUI2.HideUI();

                GameBottomUI bottomUI2 = FindFirstObjectByType<GameBottomUI>();
                if (bottomUI2 != null)
                    bottomUI2.SetSelectedUnit(null);

                return;
            }

            // Select unit for movement (clicking empty cell will move it)
            SelectPlacedUnit(clickedUnit);

            // Also show unit selection UI (sell/synthesize/upgrade options)
            UnitSelectionUI selectionUI = FindFirstObjectByType<UnitSelectionUI>();
            if (selectionUI != null)
            {
                selectionUI.ShowForUnit(clickedUnit);
            }

            // Notify GameBottomUI of selected unit for upgrade buttons
            GameBottomUI bottomUI = FindFirstObjectByType<GameBottomUI>();
            if (bottomUI != null)
            {
                bottomUI.SetSelectedUnit(clickedUnit);
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

            // Apply existing rarity-wide upgrades to new unit
            if (UnitUpgradeManager.Instance != null)
            {
                UnitUpgradeManager.Instance.ApplyRarityUpgrades(unitComponent);
            }

            // Place on grid
            if (GridManager.Instance.SetUnit(gridPos, unitObject))
            {
                // Remove from inventory
                UnitManager.Instance.RemoveUnit(SelectedUnitData);

                // Spawn effect
                StartCoroutine(PlaySpawnEffect(unitObject.transform.position, SelectedUnitData.rarity));

                // Legendary summon VFX
                if (SelectedUnitData.rarity == Rarity.Legendary)
                {
                    VFXManager.Instance?.ShowLegendarySummonEffect(unitObject.transform.position);
                }

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
                Sprite loaded = GameSpriteLoader.LoadUnitSprite(data.unitName);
                if (loaded != null)
                {
                    sr.sprite = loaded;
                    sr.color = UnitData.GetRarityColor(data.rarity);
                }
                else
                {
                    sr.sprite = UnitData.CreateCircleSprite(32);
                    sr.color = UnitData.GetRarityColor(data.rarity);
                }
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
            // Allow unit selection and movement in all states (removed phase restriction)
            SelectedPlacedUnit = unit;
            unit.Select();
        }

        /// <summary>
        /// Store an empty cell as pending move destination and highlight it.
        /// </summary>
        private void SetPendingEmptyCell(Vector2Int pos)
        {
            ClearPendingEmptyCell();
            pendingEmptyCell = pos;

            GridCell cell = GridManager.Instance.GetCellAt(pos);
            if (cell != null)
            {
                cell.SetVisualState(CellState.Selected);
            }
        }

        /// <summary>
        /// Clear the pending empty cell selection and reset its highlight.
        /// </summary>
        private void ClearPendingEmptyCell()
        {
            if (pendingEmptyCell.HasValue && GridManager.Instance != null)
            {
                GridCell cell = GridManager.Instance.GetCellAt(pendingEmptyCell.Value);
                if (cell != null && !cell.IsOccupied)
                {
                    cell.SetVisualState(CellState.Normal);
                }
            }
            pendingEmptyCell = null;
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

            OnUnitsSwapped?.Invoke(unit1, unit2, pos1, pos2);

            // Deselect after swap
            DeselectPlacedUnit();
        }
        #endregion

        #region Placement Validation
        /// <summary>
        /// Check if units can currently be placed (any active game state).
        /// </summary>
        public bool CanPlaceUnits()
        {
            return GameplayManager.Instance != null;
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
                return "GameplayManager not available";

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
            }

            SelectedUnitData = null;
            IsPlacementMode = false;
            DeselectPlacedUnit();
            ClearPendingEmptyCell();

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

        #region Spawn Effect
        /// <summary>
        /// Play visual effect when a unit is spawned.
        /// Creates expanding circle particles with rarity color.
        /// </summary>
        private System.Collections.IEnumerator PlaySpawnEffect(Vector3 position, Rarity rarity)
        {
            // Create effect object
            GameObject effectObj = new GameObject("SpawnEffect");
            effectObj.transform.position = position;

            // Get rarity color
            Color effectColor = UnitData.GetRarityColor(rarity);
            effectColor.a = 0.8f;

            // Create expanding circle using LineRenderer
            LineRenderer lineRenderer = effectObj.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = false;
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            lineRenderer.positionCount = 32;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = effectColor;
            lineRenderer.endColor = effectColor;

            // Create circle points
            float angleStep = 360f / 32f;
            for (int i = 0; i < 32; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                lineRenderer.SetPosition(i, pos);
            }

            // Animate expansion and fade
            float duration = 0.6f;
            float elapsed = 0f;
            float startRadius = 0.1f;
            float endRadius = 1.5f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Expand radius
                float currentRadius = Mathf.Lerp(startRadius, endRadius, t);
                for (int i = 0; i < 32; i++)
                {
                    float angle = i * angleStep * Mathf.Deg2Rad;
                    Vector3 pos = new Vector3(
                        Mathf.Cos(angle) * currentRadius,
                        Mathf.Sin(angle) * currentRadius,
                        0f
                    );
                    lineRenderer.SetPosition(i, pos);
                }

                // Fade out
                Color color = effectColor;
                color.a = effectColor.a * (1f - t);
                lineRenderer.startColor = color;
                lineRenderer.endColor = color;

                yield return null;
            }

            // Cleanup
            Destroy(effectObj);
        }
        #endregion
    }
}
