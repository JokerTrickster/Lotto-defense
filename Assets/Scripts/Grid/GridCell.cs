using UnityEngine;

namespace LottoDefense.Grid
{
    /// <summary>
    /// Represents a single cell in the game grid.
    /// Handles visual states, occupancy tracking, and interaction feedback.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class GridCell : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Visual Settings")]
        [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.2f);
        [SerializeField] private Color hoverColor = new Color(1f, 1f, 0f, 0.4f);
        [SerializeField] private Color selectedColor = new Color(0f, 1f, 0f, 0.6f);
        [SerializeField] private Color invalidColor = new Color(1f, 0f, 0f, 0.4f);
        [SerializeField] private Color occupiedColor = new Color(0f, 0.5f, 1f, 0.3f);
        #endregion

        #region Components
        private SpriteRenderer spriteRenderer;
        private BoxCollider2D boxCollider;
        #endregion

        #region Properties
        /// <summary>
        /// Grid coordinates of this cell (x, y).
        /// </summary>
        public Vector2Int Coordinates { get; set; }

        /// <summary>
        /// Whether this cell is currently occupied by a unit.
        /// </summary>
        public bool IsOccupied { get; set; }

        /// <summary>
        /// Reference to the unit occupying this cell (null if empty).
        /// </summary>
        public GameObject OccupyingUnit { get; set; }

        /// <summary>
        /// Current visual state of the cell.
        /// </summary>
        public CellState CurrentState { get; private set; }
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            boxCollider = GetComponent<BoxCollider2D>();

            // Initialize to normal state
            SetVisualState(CellState.Normal);
        }

        private void OnMouseEnter()
        {
            // Only show hover if not selected or occupied
            if (CurrentState == CellState.Normal)
            {
                SetVisualState(CellState.Hover);
            }
        }

        private void OnMouseExit()
        {
            // Return to normal if we were hovering
            if (CurrentState == CellState.Hover)
            {
                SetVisualState(CellState.Normal);
            }
        }

        private void OnMouseDown()
        {
            // Notify GridManager of cell click
            if (GridManager.Instance != null)
            {
                GridManager.Instance.SelectCell(Coordinates);
            }
        }
        #endregion

        #region Visual State Management
        /// <summary>
        /// Set the visual state of the cell.
        /// </summary>
        /// <param name="state">Target visual state</param>
        public void SetVisualState(CellState state)
        {
            CurrentState = state;

            Color targetColor = state switch
            {
                CellState.Normal => normalColor,
                CellState.Hover => hoverColor,
                CellState.Selected => selectedColor,
                CellState.Invalid => invalidColor,
                CellState.Occupied => occupiedColor,
                _ => normalColor
            };

            if (spriteRenderer != null)
            {
                spriteRenderer.color = targetColor;
            }
        }

        /// <summary>
        /// Apply a custom highlight color to the cell.
        /// </summary>
        /// <param name="color">Color to apply</param>
        public void Highlight(Color color)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
            }
        }

        /// <summary>
        /// Reset the cell to its current state's default color.
        /// </summary>
        public void ResetHighlight()
        {
            SetVisualState(CurrentState);
        }
        #endregion

        #region Occupancy Management
        /// <summary>
        /// Mark this cell as occupied by a unit.
        /// </summary>
        /// <param name="unit">The unit occupying this cell</param>
        public void SetOccupied(GameObject unit)
        {
            IsOccupied = true;
            OccupyingUnit = unit;
            SetVisualState(CellState.Occupied);

            // Disable collider so Unit's OnMouseDown wins click detection
            if (boxCollider != null)
                boxCollider.enabled = false;
        }

        /// <summary>
        /// Clear the occupancy of this cell.
        /// </summary>
        public void ClearOccupancy()
        {
            IsOccupied = false;
            OccupyingUnit = null;
            SetVisualState(CellState.Normal);

            // Re-enable collider so GridCell receives clicks on empty cells
            if (boxCollider != null)
                boxCollider.enabled = true;
        }
        #endregion
    }
}
