using UnityEngine;
using System;
using System.Collections.Generic;
using LottoDefense.Grid;

namespace LottoDefense.Units
{
    /// <summary>
    /// Singleton manager controlling unit lifecycle, placement, and inventory management.
    /// Minimal implementation for combat system testing.
    /// </summary>
    public class UnitManager : MonoBehaviour
    {
        #region Singleton
        private static UnitManager _instance;

        /// <summary>
        /// Global access point for the UnitManager singleton.
        /// </summary>
        public static UnitManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<UnitManager>();

                    if (_instance == null)
                    {
                        GameObject go = new GameObject("UnitManager");
                        _instance = go.AddComponent<UnitManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Private Fields
        private Dictionary<Vector2Int, Unit> placedUnits = new Dictionary<Vector2Int, Unit>();
        private Transform unitsContainer;
        #endregion

        #region Properties
        /// <summary>
        /// Number of units currently placed on the grid.
        /// </summary>
        public int PlacedUnitCount => placedUnits.Count;
        #endregion

        #region Events
        /// <summary>
        /// Fired when a unit is placed on the grid.
        /// </summary>
        public event Action<Unit, Vector2Int> OnUnitPlaced;

        /// <summary>
        /// Fired when a unit is removed from the grid.
        /// </summary>
        public event Action<Unit, Vector2Int> OnUnitRemoved;
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

            Initialize();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize unit manager.
        /// </summary>
        private void Initialize()
        {
            // Create container for units
            unitsContainer = new GameObject("Units").transform;
            unitsContainer.SetParent(transform);

            Debug.Log("[UnitManager] Initialized");
        }
        #endregion

        #region Unit Placement
        /// <summary>
        /// Place a unit on the grid at the specified position.
        /// </summary>
        /// <param name="unitData">Unit data template</param>
        /// <param name="gridPosition">Grid coordinates</param>
        /// <returns>Placed unit instance, or null if placement failed</returns>
        public Unit PlaceUnit(UnitData unitData, Vector2Int gridPosition)
        {
            if (unitData == null)
            {
                Debug.LogError("[UnitManager] Cannot place unit with null data!");
                return null;
            }

            if (GridManager.Instance == null)
            {
                Debug.LogError("[UnitManager] GridManager not found!");
                return null;
            }

            // Validate position
            if (!GridManager.Instance.IsValidPosition(gridPosition))
            {
                Debug.LogError($"[UnitManager] Invalid grid position: {gridPosition}");
                return null;
            }

            // Check if position already occupied
            if (placedUnits.ContainsKey(gridPosition))
            {
                Debug.LogWarning($"[UnitManager] Position {gridPosition} already occupied!");
                return null;
            }

            // Create unit instance
            GameObject unitObj;
            if (unitData.prefab != null)
            {
                unitObj = Instantiate(unitData.prefab, unitsContainer);
            }
            else
            {
                // Create default unit GameObject
                unitObj = CreateDefaultUnit(unitData);
            }

            // Position unit in world space
            Vector3 worldPos = GridManager.Instance.GridToWorld(gridPosition);
            unitObj.transform.position = worldPos;

            // Setup Unit component
            Unit unit = unitObj.GetComponent<Unit>();
            if (unit == null)
            {
                unit = unitObj.AddComponent<Unit>();
            }

            unit.Initialize(unitData, gridPosition);

            // Store reference
            placedUnits[gridPosition] = unit;

            // Update grid cell state
            GridCell cell = GridManager.Instance.GetCellAt(gridPosition);
            if (cell != null)
            {
                cell.SetOccupied(unitObj);
            }

            OnUnitPlaced?.Invoke(unit, gridPosition);

            Debug.Log($"[UnitManager] Placed {unitData.unitName} at {gridPosition}");

            return unit;
        }

        /// <summary>
        /// Remove a unit from the grid.
        /// </summary>
        /// <param name="gridPosition">Grid coordinates</param>
        /// <returns>True if unit was removed</returns>
        public bool RemoveUnit(Vector2Int gridPosition)
        {
            if (!placedUnits.ContainsKey(gridPosition))
            {
                Debug.LogWarning($"[UnitManager] No unit at position {gridPosition}");
                return false;
            }

            Unit unit = placedUnits[gridPosition];
            placedUnits.Remove(gridPosition);

            // Update grid cell state
            if (GridManager.Instance != null)
            {
                GridCell cell = GridManager.Instance.GetCellAt(gridPosition);
                if (cell != null)
                {
                    cell.ClearOccupancy();
                }
            }

            OnUnitRemoved?.Invoke(unit, gridPosition);

            // Destroy unit GameObject
            if (unit != null)
            {
                Destroy(unit.gameObject);
            }

            Debug.Log($"[UnitManager] Removed unit at {gridPosition}");

            return true;
        }

        /// <summary>
        /// Get unit at the specified grid position.
        /// </summary>
        /// <param name="gridPosition">Grid coordinates</param>
        /// <returns>Unit at position, or null if none</returns>
        public Unit GetUnitAt(Vector2Int gridPosition)
        {
            if (placedUnits.TryGetValue(gridPosition, out Unit unit))
            {
                return unit;
            }
            return null;
        }

        /// <summary>
        /// Get all placed units.
        /// </summary>
        /// <returns>List of all placed units</returns>
        public List<Unit> GetPlacedUnits()
        {
            return new List<Unit>(placedUnits.Values);
        }

        /// <summary>
        /// Clear all placed units from the grid.
        /// </summary>
        public void ClearAllUnits()
        {
            List<Vector2Int> positions = new List<Vector2Int>(placedUnits.Keys);

            foreach (Vector2Int pos in positions)
            {
                RemoveUnit(pos);
            }

            Debug.Log("[UnitManager] All units cleared");
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Create a default unit GameObject when no prefab is provided.
        /// </summary>
        private GameObject CreateDefaultUnit(UnitData unitData)
        {
            GameObject unitObj = new GameObject($"Unit_{unitData.unitName}");

            // Add SpriteRenderer
            SpriteRenderer sr = unitObj.AddComponent<SpriteRenderer>();
            if (unitData.icon != null)
            {
                sr.sprite = unitData.icon;
            }
            sr.sortingOrder = 10; // Above grid cells

            // Scale to fit cell
            if (GridManager.Instance != null)
            {
                float cellSize = GridManager.Instance.CellSize;
                unitObj.transform.localScale = new Vector3(cellSize * 0.8f, cellSize * 0.8f, 1f);
            }

            return unitObj;
        }
        #endregion

        #region Debugging
        /// <summary>
        /// Get unit manager statistics for debugging.
        /// </summary>
        public string GetStats()
        {
            return $"Placed units: {placedUnits.Count}";
        }
        #endregion
    }
}
