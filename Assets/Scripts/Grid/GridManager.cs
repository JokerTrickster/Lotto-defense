using UnityEngine;
using System;
using System.Collections.Generic;

namespace LottoDefense.Grid
{
    /// <summary>
    /// Singleton manager that handles grid generation, cell management, and coordinate conversion.
    /// Manages the 6x10 grid system optimized for portrait orientation (9:16 aspect ratio).
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        #region Constants
        public const int GRID_WIDTH = 6;
        public const int GRID_HEIGHT = 10;
        private const float SCREEN_WIDTH_USAGE = 0.95f; // Use 95% of screen width
        private const float SCREEN_BOTTOM_MARGIN = 0.1f; // 10% from bottom
        private const float CELL_BORDER_WIDTH = 0.02f; // Border width in world units
        #endregion

        #region Singleton
        private static GridManager _instance;

        /// <summary>
        /// Global access point for the GridManager singleton.
        /// </summary>
        public static GridManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<GridManager>();

                    if (_instance == null)
                    {
                        GameObject go = new GameObject("GridManager");
                        _instance = go.AddComponent<GridManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Inspector Fields
        [Header("Grid Cell Prefab")]
        [SerializeField] private GameObject cellPrefab;

        [Header("Grid Visual Settings")]
        [SerializeField] private Sprite cellSprite;
        [SerializeField] private float borderWidth = 0.02f;
        #endregion

        #region Private Fields
        private GridCell[,] gridCells;
        private Vector2Int? selectedCell = null;
        private Transform gridContainer;
        #endregion

        #region Properties
        /// <summary>
        /// Size of each grid cell in world units.
        /// </summary>
        public float CellSize { get; private set; }

        /// <summary>
        /// Origin point of the grid (bottom-left corner in world space).
        /// </summary>
        public Vector3 GridOrigin { get; private set; }

        /// <summary>
        /// Currently selected cell coordinates (null if none selected).
        /// </summary>
        public Vector2Int? SelectedCell => selectedCell;
        #endregion

        #region Events
        /// <summary>
        /// Fired when a cell is selected.
        /// </summary>
        public event Action<Vector2Int> OnCellSelected;

        /// <summary>
        /// Fired when a cell is deselected.
        /// </summary>
        public event Action<Vector2Int> OnCellDeselected;
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

            InitializeGrid();
        }

        private void Start()
        {
            GenerateGrid();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize grid parameters based on screen dimensions.
        /// </summary>
        private void InitializeGrid()
        {
            // Calculate cell size based on screen width
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("[GridManager] Main camera not found!");
                return;
            }

            // Get screen bounds in world space
            float screenHeight = mainCamera.orthographicSize * 2f;
            float screenWidth = screenHeight * mainCamera.aspect;

            // Calculate cell size to fit width
            CellSize = (screenWidth * SCREEN_WIDTH_USAGE) / GRID_WIDTH;

            // Calculate grid dimensions
            float gridWidth = CellSize * GRID_WIDTH;
            float gridHeight = CellSize * GRID_HEIGHT;

            // Position grid: centered horizontally, positioned from bottom
            float gridX = -gridWidth / 2f;
            float gridY = -mainCamera.orthographicSize + (screenHeight * SCREEN_BOTTOM_MARGIN);

            GridOrigin = new Vector3(gridX, gridY, 0f);

            Debug.Log($"[GridManager] Grid initialized - CellSize: {CellSize:F2}, Origin: {GridOrigin}, ScreenSize: {screenWidth:F2}x{screenHeight:F2}");
        }
        #endregion

        #region Grid Generation
        /// <summary>
        /// Generate the grid cell objects.
        /// </summary>
        public void GenerateGrid()
        {
            long startTime = System.Diagnostics.Stopwatch.GetTimestamp();

            // Create container for grid cells
            if (gridContainer == null)
            {
                gridContainer = new GameObject("GridCells").transform;
                gridContainer.SetParent(transform);
                gridContainer.localPosition = Vector3.zero;
            }

            // Initialize grid array
            gridCells = new GridCell[GRID_WIDTH, GRID_HEIGHT];

            // Generate cells
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                for (int y = 0; y < GRID_HEIGHT; y++)
                {
                    CreateCell(x, y);
                }
            }

            long endTime = System.Diagnostics.Stopwatch.GetTimestamp();
            float elapsedMs = (endTime - startTime) * 1000f / System.Diagnostics.Stopwatch.Frequency;

            Debug.Log($"[GridManager] Grid generation complete - {GRID_WIDTH}x{GRID_HEIGHT} cells created in {elapsedMs:F2}ms");
        }

        /// <summary>
        /// Create a single grid cell at the specified coordinates.
        /// </summary>
        private void CreateCell(int x, int y)
        {
            // Create cell GameObject
            GameObject cellObj;

            if (cellPrefab != null)
            {
                cellObj = Instantiate(cellPrefab, gridContainer);
            }
            else
            {
                // Create default cell if no prefab provided
                cellObj = CreateDefaultCell();
            }

            cellObj.name = $"Cell_{x}_{y}";

            // Calculate world position
            Vector3 worldPos = GridToWorld(new Vector2Int(x, y));
            cellObj.transform.position = worldPos;

            // Setup GridCell component
            GridCell gridCell = cellObj.GetComponent<GridCell>();
            if (gridCell == null)
            {
                gridCell = cellObj.AddComponent<GridCell>();
            }

            gridCell.Coordinates = new Vector2Int(x, y);

            // Store reference
            gridCells[x, y] = gridCell;
        }

        /// <summary>
        /// Create a default cell when no prefab is provided.
        /// </summary>
        private GameObject CreateDefaultCell()
        {
            GameObject cellObj = new GameObject();

            // Add SpriteRenderer
            SpriteRenderer sr = cellObj.AddComponent<SpriteRenderer>();

            if (cellSprite != null)
            {
                sr.sprite = cellSprite;
            }
            else
            {
                // Create default square sprite
                sr.sprite = CreateSquareSprite();
            }

            sr.sortingOrder = 0;

            // Scale sprite to cell size
            cellObj.transform.localScale = new Vector3(CellSize, CellSize, 1f);

            // Add BoxCollider2D for mouse interaction
            BoxCollider2D collider = cellObj.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;

            return cellObj;
        }

        /// <summary>
        /// Create a simple square sprite for default cells.
        /// </summary>
        private Sprite CreateSquareSprite()
        {
            Texture2D texture = new Texture2D(64, 64);
            Color borderColor = Color.white;
            Color fillColor = new Color(1f, 1f, 1f, 0.1f);

            // Fill with semi-transparent white
            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    // Draw border (2px)
                    if (x < 2 || x >= 62 || y < 2 || y >= 62)
                    {
                        texture.SetPixel(x, y, borderColor);
                    }
                    else
                    {
                        texture.SetPixel(x, y, fillColor);
                    }
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64f);
        }
        #endregion

        #region Coordinate Conversion
        /// <summary>
        /// Convert world position to grid coordinates.
        /// </summary>
        /// <param name="worldPos">World space position</param>
        /// <returns>Grid coordinates, or null if outside grid bounds</returns>
        public Vector2Int? WorldToGrid(Vector3 worldPos)
        {
            // Calculate relative position from grid origin
            Vector3 relativePos = worldPos - GridOrigin;

            // Convert to grid coordinates
            int x = Mathf.FloorToInt(relativePos.x / CellSize);
            int y = Mathf.FloorToInt(relativePos.y / CellSize);

            // Validate bounds
            if (IsValidPosition(x, y))
            {
                return new Vector2Int(x, y);
            }

            return null;
        }

        /// <summary>
        /// Convert grid coordinates to world position (cell center).
        /// </summary>
        /// <param name="gridPos">Grid coordinates</param>
        /// <returns>World space position at cell center</returns>
        public Vector3 GridToWorld(Vector2Int gridPos)
        {
            float worldX = GridOrigin.x + (gridPos.x * CellSize) + (CellSize * 0.5f);
            float worldY = GridOrigin.y + (gridPos.y * CellSize) + (CellSize * 0.5f);

            return new Vector3(worldX, worldY, 0f);
        }
        #endregion

        #region Cell Queries
        /// <summary>
        /// Get the GridCell component at the specified coordinates.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>GridCell component, or null if invalid position</returns>
        public GridCell GetCellAt(int x, int y)
        {
            if (IsValidPosition(x, y))
            {
                return gridCells[x, y];
            }
            return null;
        }

        /// <summary>
        /// Get the GridCell component at the specified coordinates.
        /// </summary>
        /// <param name="pos">Grid coordinates</param>
        /// <returns>GridCell component, or null if invalid position</returns>
        public GridCell GetCellAt(Vector2Int pos)
        {
            return GetCellAt(pos.x, pos.y);
        }

        /// <summary>
        /// Get all neighboring cells of the specified position.
        /// </summary>
        /// <param name="pos">Center grid coordinates</param>
        /// <param name="includeDiagonal">Whether to include diagonal neighbors</param>
        /// <returns>List of neighboring GridCell components</returns>
        public List<GridCell> GetNeighbors(Vector2Int pos, bool includeDiagonal = false)
        {
            List<GridCell> neighbors = new List<GridCell>();

            // Cardinal directions
            Vector2Int[] cardinalOffsets = new Vector2Int[]
            {
                new Vector2Int(0, 1),   // North
                new Vector2Int(1, 0),   // East
                new Vector2Int(0, -1),  // South
                new Vector2Int(-1, 0)   // West
            };

            // Diagonal directions
            Vector2Int[] diagonalOffsets = new Vector2Int[]
            {
                new Vector2Int(1, 1),   // NE
                new Vector2Int(1, -1),  // SE
                new Vector2Int(-1, -1), // SW
                new Vector2Int(-1, 1)   // NW
            };

            // Add cardinal neighbors
            foreach (var offset in cardinalOffsets)
            {
                Vector2Int neighborPos = pos + offset;
                GridCell cell = GetCellAt(neighborPos);
                if (cell != null)
                {
                    neighbors.Add(cell);
                }
            }

            // Add diagonal neighbors if requested
            if (includeDiagonal)
            {
                foreach (var offset in diagonalOffsets)
                {
                    Vector2Int neighborPos = pos + offset;
                    GridCell cell = GetCellAt(neighborPos);
                    if (cell != null)
                    {
                        neighbors.Add(cell);
                    }
                }
            }

            return neighbors;
        }

        /// <summary>
        /// Check if the specified position is within grid bounds.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>True if position is valid</returns>
        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < GRID_WIDTH && y >= 0 && y < GRID_HEIGHT;
        }

        /// <summary>
        /// Check if the specified position is within grid bounds.
        /// </summary>
        /// <param name="pos">Grid coordinates</param>
        /// <returns>True if position is valid</returns>
        public bool IsValidPosition(Vector2Int pos)
        {
            return IsValidPosition(pos.x, pos.y);
        }

        /// <summary>
        /// Check if a cell is occupied by a unit.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>True if cell is occupied</returns>
        public bool IsOccupied(int x, int y)
        {
            GridCell cell = GetCellAt(x, y);
            return cell != null && cell.IsOccupied;
        }

        /// <summary>
        /// Check if a cell is occupied by a unit.
        /// </summary>
        /// <param name="pos">Grid coordinates</param>
        /// <returns>True if cell is occupied</returns>
        public bool IsOccupied(Vector2Int pos)
        {
            return IsOccupied(pos.x, pos.y);
        }
        #endregion

        #region Cell Selection
        /// <summary>
        /// Select a cell at the specified grid coordinates.
        /// </summary>
        /// <param name="pos">Grid coordinates to select</param>
        public void SelectCell(Vector2Int pos)
        {
            if (!IsValidPosition(pos))
            {
                Debug.LogWarning($"[GridManager] Invalid cell selection: {pos}");
                return;
            }

            // Deselect previous cell
            if (selectedCell.HasValue)
            {
                GridCell prevCell = GetCellAt(selectedCell.Value);
                if (prevCell != null && !prevCell.IsOccupied)
                {
                    prevCell.SetVisualState(CellState.Normal);
                }

                OnCellDeselected?.Invoke(selectedCell.Value);
            }

            // Select new cell
            selectedCell = pos;
            GridCell cell = GetCellAt(pos);

            if (cell != null && !cell.IsOccupied)
            {
                cell.SetVisualState(CellState.Selected);
            }

            OnCellSelected?.Invoke(pos);

            Debug.Log($"[GridManager] Cell selected: {pos}");
        }

        /// <summary>
        /// Deselect all cells.
        /// </summary>
        public void DeselectAll()
        {
            if (selectedCell.HasValue)
            {
                GridCell cell = GetCellAt(selectedCell.Value);
                if (cell != null && !cell.IsOccupied)
                {
                    cell.SetVisualState(CellState.Normal);
                }

                OnCellDeselected?.Invoke(selectedCell.Value);
                selectedCell = null;

                Debug.Log("[GridManager] All cells deselected");
            }
        }
        #endregion
    }
}
