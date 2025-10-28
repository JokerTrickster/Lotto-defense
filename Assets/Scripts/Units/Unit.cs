using UnityEngine;
using LottoDefense.Grid;

namespace LottoDefense.Units
{
    /// <summary>
    /// MonoBehaviour component attached to placed unit instances on the grid.
    /// Handles visual representation, grid position tracking, and selection state.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class Unit : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Visual Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = Color.yellow;
        [SerializeField] private float selectionGlowIntensity = 1.5f;
        #endregion

        #region Components
        private SpriteRenderer spriteRenderer;
        #endregion

        #region Properties
        /// <summary>
        /// Reference to the UnitData defining this unit's stats and behavior.
        /// </summary>
        public UnitData Data { get; private set; }

        /// <summary>
        /// Current grid position of this unit.
        /// </summary>
        public Vector2Int GridPosition { get; set; }

        /// <summary>
        /// Whether this unit is currently selected for placement/swapping.
        /// </summary>
        public bool IsSelected { get; private set; }

        /// <summary>
        /// Current upgrade level of this unit (1-10).
        /// Level 1 is base stats, each level increases effectiveness.
        /// </summary>
        public int UpgradeLevel { get; private set; } = 1;

        /// <summary>
        /// Current attack value including upgrade multiplier.
        /// </summary>
        public int CurrentAttack { get; private set; }
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnMouseDown()
        {
            // Notify placement manager when this placed unit is clicked
            if (UnitPlacementManager.Instance != null)
            {
                UnitPlacementManager.Instance.OnPlacedUnitClicked(this);
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize this unit instance with its data and grid position.
        /// </summary>
        /// <param name="unitData">Unit data template</param>
        /// <param name="gridPos">Grid coordinates</param>
        public void Initialize(UnitData unitData, Vector2Int gridPos)
        {
            Data = unitData;
            GridPosition = gridPos;
            UpgradeLevel = 1;
            CurrentAttack = unitData.attack; // Start with base attack

            // Setup visual representation
            if (spriteRenderer != null && unitData.icon != null)
            {
                spriteRenderer.sprite = unitData.icon;
                spriteRenderer.color = normalColor;
                spriteRenderer.sortingOrder = 10; // Above grid cells
            }

            // Position at grid cell center
            if (GridManager.Instance != null)
            {
                transform.position = GridManager.Instance.GridToWorld(gridPos);
            }

            gameObject.name = $"Unit_{unitData.unitName}_{gridPos.x}_{gridPos.y}";
            Debug.Log($"[Unit] Initialized {Data.GetDisplayName()} at {GridPosition}");
        }
        #endregion

        #region Selection Management
        /// <summary>
        /// Mark this unit as selected with visual feedback.
        /// </summary>
        public void Select()
        {
            IsSelected = true;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = selectedColor * selectionGlowIntensity;
            }

            Debug.Log($"[Unit] Selected {Data.GetDisplayName()} at {GridPosition}");
        }

        /// <summary>
        /// Deselect this unit and restore normal visuals.
        /// </summary>
        public void Deselect()
        {
            IsSelected = false;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = normalColor;
            }

            Debug.Log($"[Unit] Deselected {Data.GetDisplayName()} at {GridPosition}");
        }
        #endregion

        #region Position Management
        /// <summary>
        /// Move this unit to a new grid position with visual update.
        /// </summary>
        /// <param name="newGridPos">Target grid coordinates</param>
        public void MoveTo(Vector2Int newGridPos)
        {
            Vector2Int oldPos = GridPosition;
            GridPosition = newGridPos;

            // Update world position
            if (GridManager.Instance != null)
            {
                transform.position = GridManager.Instance.GridToWorld(newGridPos);
            }

            // Update name for debugging
            gameObject.name = $"Unit_{Data.unitName}_{newGridPos.x}_{newGridPos.y}";

            Debug.Log($"[Unit] Moved {Data.GetDisplayName()} from {oldPos} to {newGridPos}");
        }
        #endregion

        #region Upgrade Management
        /// <summary>
        /// Apply an upgrade to this unit, increasing its level and stats.
        /// </summary>
        /// <param name="newLevel">New upgrade level (must be greater than current)</param>
        /// <param name="attackMultiplier">Multiplier to apply to base attack</param>
        public void ApplyUpgrade(int newLevel, float attackMultiplier)
        {
            if (newLevel <= UpgradeLevel)
            {
                Debug.LogWarning($"[Unit] Cannot apply upgrade - new level {newLevel} <= current level {UpgradeLevel}");
                return;
            }

            if (newLevel > 10)
            {
                Debug.LogWarning($"[Unit] Cannot apply upgrade - level {newLevel} exceeds maximum (10)");
                return;
            }

            int oldLevel = UpgradeLevel;
            int oldAttack = CurrentAttack;

            UpgradeLevel = newLevel;
            CurrentAttack = Mathf.RoundToInt(Data.attack * attackMultiplier);

            Debug.Log($"[Unit] {Data.GetDisplayName()} upgraded: L{oldLevel}→L{newLevel}, ATK {oldAttack}→{CurrentAttack}");
        }

        /// <summary>
        /// Get the current attack multiplier based on upgrade level.
        /// </summary>
        public float GetAttackMultiplier()
        {
            return 1.0f + (0.1f * (UpgradeLevel - 1));
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get a summary string for debugging.
        /// </summary>
        public override string ToString()
        {
            return $"{Data?.GetDisplayName() ?? "Unknown"} at {GridPosition} (L{UpgradeLevel}, ATK:{CurrentAttack})";
        }
        #endregion
    }
}
