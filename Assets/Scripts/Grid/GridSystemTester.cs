using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace LottoDefense.Grid
{
    /// <summary>
    /// Test and validation script for the grid system.
    /// Provides runtime tests for grid generation, coordinate conversion, and cell queries.
    /// </summary>
    public class GridSystemTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool runTestsOnStart = true;
        [SerializeField] private bool showDebugVisualization = true;
        [SerializeField] private KeyCode runTestsKey = KeyCode.T;
        [SerializeField] private KeyCode visualizeGridKey = KeyCode.G;

        // private bool testsCompleted = false; // Unused - removed to avoid CS0414 warning

        #region Unity Lifecycle
        private void Start()
        {
            if (runTestsOnStart)
            {
                RunAllTests();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(runTestsKey))
            {
                RunAllTests();
            }

            if (Input.GetKeyDown(visualizeGridKey))
            {
                showDebugVisualization = !showDebugVisualization;
                Debug.Log($"[GridSystemTester] Debug visualization: {showDebugVisualization}");
            }
        }

        private void OnDrawGizmos()
        {
            if (!showDebugVisualization || GridManager.Instance == null)
                return;

            DrawGridVisualization();
        }
        #endregion

        #region Test Suite
        /// <summary>
        /// Run all grid system tests.
        /// </summary>
        [ContextMenu("Run All Tests")]
        public void RunAllTests()
        {
            Debug.Log("========== GRID SYSTEM TEST SUITE ==========");
            Stopwatch totalTimer = Stopwatch.StartNew();

            int passedTests = 0;
            int totalTests = 0;

            // Test 1: Grid Manager Singleton
            totalTests++;
            if (TestGridManagerSingleton())
                passedTests++;

            // Test 2: Grid Generation
            totalTests++;
            if (TestGridGeneration())
                passedTests++;

            // Test 3: Coordinate Conversion
            totalTests++;
            if (TestCoordinateConversion())
                passedTests++;

            // Test 4: Cell Queries
            totalTests++;
            if (TestCellQueries())
                passedTests++;

            // Test 5: Neighbor System
            totalTests++;
            if (TestNeighborSystem())
                passedTests++;

            // Test 6: Cell Selection
            totalTests++;
            if (TestCellSelection())
                passedTests++;

            // Test 7: Performance
            totalTests++;
            if (TestPerformance())
                passedTests++;

            totalTimer.Stop();

            Debug.Log($"========== TEST RESULTS ==========");
            Debug.Log($"Passed: {passedTests}/{totalTests} tests");
            Debug.Log($"Success Rate: {(passedTests * 100f / totalTests):F1}%");
            Debug.Log($"Total Time: {totalTimer.ElapsedMilliseconds}ms");
            Debug.Log($"====================================");

            // testsCompleted = true; // Commented out - variable removed to avoid CS0414 warning
        }

        /// <summary>
        /// Test 1: GridManager singleton initialization.
        /// </summary>
        private bool TestGridManagerSingleton()
        {
            Debug.Log("[Test 1] GridManager Singleton");

            if (GridManager.Instance == null)
            {
                Debug.LogError("[Test 1] FAILED: GridManager.Instance is null");
                return false;
            }

            // Test singleton property (should return same instance)
            var instance1 = GridManager.Instance;
            var instance2 = GridManager.Instance;

            if (instance1 != instance2)
            {
                Debug.LogError("[Test 1] FAILED: Singleton returns different instances");
                return false;
            }

            Debug.Log("[Test 1] PASSED: GridManager singleton working correctly");
            return true;
        }

        /// <summary>
        /// Test 2: Grid generation and cell count.
        /// </summary>
        private bool TestGridGeneration()
        {
            Debug.Log("[Test 2] Grid Generation");

            int expectedCellCount = GridManager.GRID_WIDTH * GridManager.GRID_HEIGHT;
            int actualCellCount = 0;

            // Count valid cells
            for (int x = 0; x < GridManager.GRID_WIDTH; x++)
            {
                for (int y = 0; y < GridManager.GRID_HEIGHT; y++)
                {
                    if (GridManager.Instance.GetCellAt(x, y) != null)
                    {
                        actualCellCount++;
                    }
                }
            }

            if (actualCellCount != expectedCellCount)
            {
                Debug.LogError($"[Test 2] FAILED: Expected {expectedCellCount} cells, found {actualCellCount}");
                return false;
            }

            // Verify cell size is positive
            if (GridManager.Instance.CellSize <= 0)
            {
                Debug.LogError($"[Test 2] FAILED: Invalid cell size: {GridManager.Instance.CellSize}");
                return false;
            }

            Debug.Log($"[Test 2] PASSED: Grid generated with {actualCellCount} cells, size {GridManager.Instance.CellSize:F2}");
            return true;
        }

        /// <summary>
        /// Test 3: Coordinate conversion accuracy.
        /// </summary>
        private bool TestCoordinateConversion()
        {
            Debug.Log("[Test 3] Coordinate Conversion");

            int testCases = 0;
            int successfulConversions = 0;

            // Test all grid positions
            for (int x = 0; x < GridManager.GRID_WIDTH; x++)
            {
                for (int y = 0; y < GridManager.GRID_HEIGHT; y++)
                {
                    testCases++;
                    Vector2Int gridPos = new Vector2Int(x, y);

                    // Convert grid to world and back
                    Vector3 worldPos = GridManager.Instance.GridToWorld(gridPos);
                    Vector2Int? convertedBack = GridManager.Instance.WorldToGrid(worldPos);

                    if (convertedBack.HasValue && convertedBack.Value == gridPos)
                    {
                        successfulConversions++;
                    }
                    else
                    {
                        Debug.LogError($"[Test 3] Conversion failed for {gridPos}: got {convertedBack}");
                    }
                }
            }

            if (successfulConversions != testCases)
            {
                Debug.LogError($"[Test 3] FAILED: {successfulConversions}/{testCases} conversions successful");
                return false;
            }

            Debug.Log($"[Test 3] PASSED: {successfulConversions}/{testCases} coordinate conversions accurate");
            return true;
        }

        /// <summary>
        /// Test 4: Cell query operations.
        /// </summary>
        private bool TestCellQueries()
        {
            Debug.Log("[Test 4] Cell Queries");

            // Test valid position checks
            if (!GridManager.Instance.IsValidPosition(0, 0))
            {
                Debug.LogError("[Test 4] FAILED: (0,0) should be valid");
                return false;
            }

            if (!GridManager.Instance.IsValidPosition(GridManager.GRID_WIDTH - 1, GridManager.GRID_HEIGHT - 1))
            {
                Debug.LogError("[Test 4] FAILED: Max position should be valid");
                return false;
            }

            // Test invalid positions
            if (GridManager.Instance.IsValidPosition(-1, 0))
            {
                Debug.LogError("[Test 4] FAILED: Negative X should be invalid");
                return false;
            }

            if (GridManager.Instance.IsValidPosition(GridManager.GRID_WIDTH, 0))
            {
                Debug.LogError("[Test 4] FAILED: X >= GRID_WIDTH should be invalid");
                return false;
            }

            // Test GetCellAt
            GridCell cell = GridManager.Instance.GetCellAt(0, 0);
            if (cell == null)
            {
                Debug.LogError("[Test 4] FAILED: GetCellAt(0,0) returned null");
                return false;
            }

            if (cell.Coordinates != new Vector2Int(0, 0))
            {
                Debug.LogError($"[Test 4] FAILED: Cell coordinates mismatch: {cell.Coordinates}");
                return false;
            }

            Debug.Log("[Test 4] PASSED: Cell query operations working correctly");
            return true;
        }

        /// <summary>
        /// Test 5: Neighbor query system.
        /// </summary>
        private bool TestNeighborSystem()
        {
            Debug.Log("[Test 5] Neighbor System");

            // Test center cell (should have 4 cardinal neighbors)
            Vector2Int centerPos = new Vector2Int(2, 5);
            var cardinalNeighbors = GridManager.Instance.GetNeighbors(centerPos, false);

            if (cardinalNeighbors.Count != 4)
            {
                Debug.LogError($"[Test 5] FAILED: Center cell should have 4 cardinal neighbors, found {cardinalNeighbors.Count}");
                return false;
            }

            // Test with diagonal (should have 8 neighbors)
            var allNeighbors = GridManager.Instance.GetNeighbors(centerPos, true);

            if (allNeighbors.Count != 8)
            {
                Debug.LogError($"[Test 5] FAILED: Center cell should have 8 total neighbors, found {allNeighbors.Count}");
                return false;
            }

            // Test corner cell (0, 0) - should have 2 cardinal neighbors
            Vector2Int cornerPos = new Vector2Int(0, 0);
            var cornerNeighbors = GridManager.Instance.GetNeighbors(cornerPos, false);

            if (cornerNeighbors.Count != 2)
            {
                Debug.LogError($"[Test 5] FAILED: Corner cell should have 2 cardinal neighbors, found {cornerNeighbors.Count}");
                return false;
            }

            // Test edge cell - should have 3 cardinal neighbors
            Vector2Int edgePos = new Vector2Int(0, 5);
            var edgeNeighbors = GridManager.Instance.GetNeighbors(edgePos, false);

            if (edgeNeighbors.Count != 3)
            {
                Debug.LogError($"[Test 5] FAILED: Edge cell should have 3 cardinal neighbors, found {edgeNeighbors.Count}");
                return false;
            }

            Debug.Log("[Test 5] PASSED: Neighbor system working correctly");
            return true;
        }

        /// <summary>
        /// Test 6: Cell selection system.
        /// </summary>
        private bool TestCellSelection()
        {
            Debug.Log("[Test 6] Cell Selection");

            // Ensure no cell is selected initially
            GridManager.Instance.DeselectAll();

            if (GridManager.Instance.SelectedCell.HasValue)
            {
                Debug.LogError("[Test 6] FAILED: Cell should be deselected after DeselectAll");
                return false;
            }

            // Select a cell
            Vector2Int testPos = new Vector2Int(2, 3);
            GridManager.Instance.SelectCell(testPos);

            if (!GridManager.Instance.SelectedCell.HasValue)
            {
                Debug.LogError("[Test 6] FAILED: Cell should be selected");
                return false;
            }

            if (GridManager.Instance.SelectedCell.Value != testPos)
            {
                Debug.LogError($"[Test 6] FAILED: Selected cell mismatch: {GridManager.Instance.SelectedCell.Value} != {testPos}");
                return false;
            }

            // Verify visual state
            GridCell cell = GridManager.Instance.GetCellAt(testPos);
            if (cell.CurrentState != CellState.Selected)
            {
                Debug.LogError($"[Test 6] FAILED: Cell visual state should be Selected, is {cell.CurrentState}");
                return false;
            }

            // Select another cell (should deselect first)
            Vector2Int testPos2 = new Vector2Int(3, 4);
            GridManager.Instance.SelectCell(testPos2);

            if (GridManager.Instance.SelectedCell.Value != testPos2)
            {
                Debug.LogError("[Test 6] FAILED: Second cell should be selected");
                return false;
            }

            // Verify first cell is deselected
            if (cell.CurrentState == CellState.Selected)
            {
                Debug.LogError("[Test 6] FAILED: First cell should be deselected");
                return false;
            }

            // Cleanup
            GridManager.Instance.DeselectAll();

            Debug.Log("[Test 6] PASSED: Cell selection system working correctly");
            return true;
        }

        /// <summary>
        /// Test 7: Performance metrics.
        /// </summary>
        private bool TestPerformance()
        {
            Debug.Log("[Test 7] Performance");

            // Test coordinate conversion speed (1000 conversions)
            Stopwatch timer = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                int x = i % GridManager.GRID_WIDTH;
                int y = (i / GridManager.GRID_WIDTH) % GridManager.GRID_HEIGHT;
                Vector3 worldPos = GridManager.Instance.GridToWorld(new Vector2Int(x, y));
                GridManager.Instance.WorldToGrid(worldPos);
            }
            timer.Stop();

            float avgConversionTime = timer.ElapsedMilliseconds / 1000f;

            if (avgConversionTime > 1f)
            {
                Debug.LogWarning($"[Test 7] WARNING: Coordinate conversion slow: {avgConversionTime:F3}ms average");
            }

            Debug.Log($"[Test 7] PASSED: Performance metrics - Coordinate conversion: {avgConversionTime:F3}ms avg");
            return true;
        }
        #endregion

        #region Debug Visualization
        /// <summary>
        /// Draw debug visualization of the grid in Scene view.
        /// </summary>
        private void DrawGridVisualization()
        {
            // Draw grid cells
            for (int x = 0; x < GridManager.GRID_WIDTH; x++)
            {
                for (int y = 0; y < GridManager.GRID_HEIGHT; y++)
                {
                    Vector3 worldPos = GridManager.Instance.GridToWorld(new Vector2Int(x, y));
                    float size = GridManager.Instance.CellSize;

                    // Draw cell outline
                    Gizmos.color = Color.white;
                    DrawWireCube(worldPos, new Vector3(size, size, 0));

                    // Highlight selected cell
                    if (GridManager.Instance.SelectedCell.HasValue &&
                        GridManager.Instance.SelectedCell.Value == new Vector2Int(x, y))
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawCube(worldPos, new Vector3(size * 0.8f, size * 0.8f, 0));
                    }
                }
            }

            // Draw grid origin
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(GridManager.Instance.GridOrigin, 0.2f);

            // Draw coordinate labels (only in editor)
#if UNITY_EDITOR
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.yellow;
            style.fontSize = 10;

            // Label corners
            Vector3 origin = GridManager.Instance.GridOrigin;
            UnityEditor.Handles.Label(origin, "(0,0)", style);

            Vector3 maxCorner = GridManager.Instance.GridToWorld(
                new Vector2Int(GridManager.GRID_WIDTH - 1, GridManager.GRID_HEIGHT - 1));
            UnityEditor.Handles.Label(maxCorner, $"({GridManager.GRID_WIDTH - 1},{GridManager.GRID_HEIGHT - 1})", style);
#endif
        }

        /// <summary>
        /// Helper to draw wire cube.
        /// </summary>
        private void DrawWireCube(Vector3 center, Vector3 size)
        {
            Vector3 halfSize = size * 0.5f;

            // Bottom face
            Gizmos.DrawLine(center + new Vector3(-halfSize.x, -halfSize.y, 0), center + new Vector3(halfSize.x, -halfSize.y, 0));
            Gizmos.DrawLine(center + new Vector3(halfSize.x, -halfSize.y, 0), center + new Vector3(halfSize.x, halfSize.y, 0));
            Gizmos.DrawLine(center + new Vector3(halfSize.x, halfSize.y, 0), center + new Vector3(-halfSize.x, halfSize.y, 0));
            Gizmos.DrawLine(center + new Vector3(-halfSize.x, halfSize.y, 0), center + new Vector3(-halfSize.x, -halfSize.y, 0));
        }
        #endregion
    }
}
