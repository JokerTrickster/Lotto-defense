using UnityEngine;
using System;
using System.Collections.Generic;
using LottoDefense.Grid;
using LottoDefense.Gameplay;
using LottoDefense.Units;

namespace LottoDefense.Multiplayer
{
    /// <summary>
    /// Manages the cooperative play map layout for 2 players.
    /// Handles player-specific grid areas and shared monster paths.
    /// </summary>
    public class CoopMapManager : MonoBehaviour
    {
        #region Constants
        // Player 1 (left side) grid area
        private const int PLAYER1_GRID_START_X = 0;
        private const int PLAYER1_GRID_END_X = 1;

        // Player 2 (right side) grid area
        private const int PLAYER2_GRID_START_X = 2;
        private const int PLAYER2_GRID_END_X = 3;

        // Shared rows (all 5 rows available to both players)
        private const int SHARED_GRID_START_Y = 0;
        private const int SHARED_GRID_END_Y = 4;

        // Visual indicators
        private const float AREA_HIGHLIGHT_ALPHA = 0.3f;
        private readonly Color PLAYER1_COLOR = new Color(0.2f, 0.5f, 1f); // Blue
        private readonly Color PLAYER2_COLOR = new Color(1f, 0.5f, 0.2f); // Orange
        #endregion

        #region Singleton
        private static CoopMapManager _instance;
        public static CoopMapManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<CoopMapManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("CoopMapManager");
                        _instance = go.AddComponent<CoopMapManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Private Fields
        private Dictionary<int, PlayerArea> playerAreas;
        private GameObject player1AreaVisual;
        private GameObject player2AreaVisual;
        private bool isCoopMode = false;
        #endregion

        #region Public Properties
        /// <summary>
        /// Whether cooperative play mode is active
        /// </summary>
        public bool IsCoopMode => isCoopMode;

        /// <summary>
        /// Player 1's grid area boundaries
        /// </summary>
        public PlayerArea Player1Area => playerAreas?[1];

        /// <summary>
        /// Player 2's grid area boundaries
        /// </summary>
        public PlayerArea Player2Area => playerAreas?[2];
        #endregion

        #region Data Classes
        [Serializable]
        public class PlayerArea
        {
            public int playerNumber;
            public int startX;
            public int endX;
            public int startY;
            public int endY;
            public Color areaColor;

            public PlayerArea(int player, int sx, int ex, int sy, int ey, Color color)
            {
                playerNumber = player;
                startX = sx;
                endX = ex;
                startY = sy;
                endY = ey;
                areaColor = color;
            }

            /// <summary>
            /// Check if a grid position is within this player's area
            /// </summary>
            public bool ContainsPosition(Vector2Int pos)
            {
                return pos.x >= startX && pos.x <= endX &&
                       pos.y >= startY && pos.y <= endY;
            }

            /// <summary>
            /// Get all valid grid positions in this area
            /// </summary>
            public List<Vector2Int> GetAllPositions()
            {
                List<Vector2Int> positions = new List<Vector2Int>();
                for (int x = startX; x <= endX; x++)
                {
                    for (int y = startY; y <= endY; y++)
                    {
                        positions.Add(new Vector2Int(x, y));
                    }
                }
                return positions;
            }
        }
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

            InitializePlayerAreas();
        }

        private void Start()
        {
            // Subscribe to game mode changes if available
            if (GameplayManager.Instance != null)
            {
                // Listen for coop mode activation
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize player area definitions
        /// </summary>
        private void InitializePlayerAreas()
        {
            playerAreas = new Dictionary<int, PlayerArea>();

            // Player 1: Left side (columns 0-1)
            playerAreas[1] = new PlayerArea(
                1,
                PLAYER1_GRID_START_X,
                PLAYER1_GRID_END_X,
                SHARED_GRID_START_Y,
                SHARED_GRID_END_Y,
                PLAYER1_COLOR
            );

            // Player 2: Right side (columns 2-3)
            playerAreas[2] = new PlayerArea(
                2,
                PLAYER2_GRID_START_X,
                PLAYER2_GRID_END_X,
                SHARED_GRID_START_Y,
                SHARED_GRID_END_Y,
                PLAYER2_COLOR
            );
        }
        #endregion

        #region Coop Mode Management
        /// <summary>
        /// Enable cooperative play mode
        /// </summary>
        public void EnableCoopMode()
        {
            if (isCoopMode) return;

            isCoopMode = true;
            CreateAreaVisuals();
            HighlightPlayerAreas();

            Debug.Log("[CoopMapManager] Cooperative play mode enabled - 2 player map active");
        }

        /// <summary>
        /// Disable cooperative play mode
        /// </summary>
        public void DisableCoopMode()
        {
            if (!isCoopMode) return;

            isCoopMode = false;
            ClearAreaVisuals();

            Debug.Log("[CoopMapManager] Cooperative play mode disabled - single player map active");
        }
        #endregion

        #region Visual Feedback
        /// <summary>
        /// Create visual indicators for player areas
        /// </summary>
        private void CreateAreaVisuals()
        {
            if (GridManager.Instance == null) return;

            // Create Player 1 area visual
            player1AreaVisual = CreateAreaHighlight(playerAreas[1], "Player1_Area");

            // Create Player 2 area visual
            player2AreaVisual = CreateAreaHighlight(playerAreas[2], "Player2_Area");
        }

        /// <summary>
        /// Create a visual highlight for a player area
        /// </summary>
        private GameObject CreateAreaHighlight(PlayerArea area, string name)
        {
            GameObject highlight = new GameObject(name);
            highlight.transform.SetParent(transform);

            // Calculate area bounds
            Vector3 bottomLeft = GridManager.Instance.GridToWorld(new Vector2Int(area.startX, area.startY));
            Vector3 topRight = GridManager.Instance.GridToWorld(new Vector2Int(area.endX, area.endY));

            // Adjust for cell size
            float cellSize = GridManager.Instance.CellSize;
            bottomLeft.x -= cellSize * 0.5f;
            bottomLeft.y -= cellSize * 0.5f;
            topRight.x += cellSize * 0.5f;
            topRight.y += cellSize * 0.5f;

            // Create mesh for area
            MeshFilter meshFilter = highlight.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = highlight.AddComponent<MeshRenderer>();

            // Create simple quad mesh
            Mesh mesh = new Mesh();
            Vector3[] vertices = new Vector3[]
            {
                new Vector3(bottomLeft.x, bottomLeft.y, 0),
                new Vector3(topRight.x, bottomLeft.y, 0),
                new Vector3(topRight.x, topRight.y, 0),
                new Vector3(bottomLeft.x, topRight.y, 0)
            };

            int[] triangles = new int[] { 0, 2, 1, 0, 3, 2 };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            meshFilter.mesh = mesh;

            // Set material with transparent color
            Material mat = new Material(Shader.Find("Sprites/Default"));
            Color highlightColor = area.areaColor;
            highlightColor.a = AREA_HIGHLIGHT_ALPHA;
            mat.color = highlightColor;
            meshRenderer.material = mat;
            meshRenderer.sortingOrder = -1; // Behind grid cells

            return highlight;
        }

        /// <summary>
        /// Highlight grid cells in player areas
        /// </summary>
        private void HighlightPlayerAreas()
        {
            if (GridManager.Instance == null) return;

            // Highlight Player 1 cells
            foreach (var pos in playerAreas[1].GetAllPositions())
            {
                GridCell cell = GridManager.Instance.GetCellAt(pos);
                if (cell != null)
                {
                    ApplyPlayerHighlight(cell, 1);
                }
            }

            // Highlight Player 2 cells
            foreach (var pos in playerAreas[2].GetAllPositions())
            {
                GridCell cell = GridManager.Instance.GetCellAt(pos);
                if (cell != null)
                {
                    ApplyPlayerHighlight(cell, 2);
                }
            }
        }

        /// <summary>
        /// Apply visual highlight to a cell for a specific player
        /// </summary>
        private void ApplyPlayerHighlight(GridCell cell, int playerNumber)
        {
            SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color tint = playerNumber == 1 ? PLAYER1_COLOR : PLAYER2_COLOR;
                tint.a = 0.2f; // Subtle tint
                sr.color = Color.white * 0.8f + tint * 0.2f;
            }
        }

        /// <summary>
        /// Clear all visual indicators
        /// </summary>
        private void ClearAreaVisuals()
        {
            if (player1AreaVisual != null)
            {
                Destroy(player1AreaVisual);
                player1AreaVisual = null;
            }

            if (player2AreaVisual != null)
            {
                Destroy(player2AreaVisual);
                player2AreaVisual = null;
            }

            // Reset cell colors
            if (GridManager.Instance != null)
            {
                for (int x = 0; x < GridManager.GRID_WIDTH; x++)
                {
                    for (int y = 0; y < GridManager.GRID_HEIGHT; y++)
                    {
                        GridCell cell = GridManager.Instance.GetCellAt(x, y);
                        if (cell != null)
                        {
                            SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
                            if (sr != null)
                            {
                                sr.color = Color.white;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Player Area Queries
        /// <summary>
        /// Get which player's area a position belongs to
        /// </summary>
        public int GetPlayerForPosition(Vector2Int pos)
        {
            if (!isCoopMode) return 0; // Single player mode

            if (playerAreas[1].ContainsPosition(pos)) return 1;
            if (playerAreas[2].ContainsPosition(pos)) return 2;

            return 0; // Invalid position
        }

        /// <summary>
        /// Check if a player can place units at a position
        /// </summary>
        public bool CanPlayerPlaceAt(int playerNumber, Vector2Int pos)
        {
            if (!isCoopMode) return true; // No restrictions in single player

            if (!playerAreas.ContainsKey(playerNumber)) return false;

            return playerAreas[playerNumber].ContainsPosition(pos);
        }

        /// <summary>
        /// Get all valid placement positions for a player
        /// </summary>
        public List<Vector2Int> GetPlayerPlacementPositions(int playerNumber)
        {
            if (!isCoopMode || !playerAreas.ContainsKey(playerNumber))
            {
                // Return all positions in single player mode
                List<Vector2Int> allPositions = new List<Vector2Int>();
                for (int x = 0; x < GridManager.GRID_WIDTH; x++)
                {
                    for (int y = 0; y < GridManager.GRID_HEIGHT; y++)
                    {
                        allPositions.Add(new Vector2Int(x, y));
                    }
                }
                return allPositions;
            }

            return playerAreas[playerNumber].GetAllPositions();
        }

        /// <summary>
        /// Get unit count for a specific player
        /// </summary>
        public int GetPlayerUnitCount(int playerNumber)
        {
            if (GridManager.Instance == null) return 0;

            int count = 0;
            List<Vector2Int> positions = GetPlayerPlacementPositions(playerNumber);

            foreach (var pos in positions)
            {
                if (GridManager.Instance.IsOccupied(pos))
                {
                    count++;
                }
            }

            return count;
        }
        #endregion

        #region Shared Monster Path
        /// <summary>
        /// Get waypoints for the shared monster path in coop mode
        /// </summary>
        public List<Vector3> GetCoopMonsterPath()
        {
            // In coop mode, monsters still follow the same outer loop path
            // Both players defend against the same monster wave
            return GridManager.Instance?.GetSquareLoopWaypoints() ?? new List<Vector3>();
        }
        #endregion
    }
}