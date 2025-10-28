# Grid System - Lotto Defense

## Overview

The Grid System provides a robust 6x10 grid optimized for portrait orientation (9:16 aspect ratio) that serves as the battlefield for unit placement and monster movement. It includes coordinate conversion, cell selection, neighbor queries, and visual feedback.

## Quick Start

### 1. Add to Scene
```
1. Open GameScene.unity in Unity Editor
2. Create empty GameObject named "GridManager"
3. Add GridManager component (LottoDefense.Grid.GridManager)
4. Press Play - grid generates automatically
```

### 2. Test the System
```
1. Add GridSystemTester component to any GameObject
2. Press Play
3. Tests run automatically and show results in Console
4. Press T to re-run tests
5. Press G to toggle grid visualization
```

### 3. Use in Code
```csharp
using LottoDefense.Grid;

// Get cell at grid position
GridCell cell = GridManager.Instance.GetCellAt(2, 5);

// Convert coordinates
Vector3 worldPos = GridManager.Instance.GridToWorld(new Vector2Int(3, 4));
Vector2Int? gridPos = GridManager.Instance.WorldToGrid(worldPosition);

// Check occupancy
if (!GridManager.Instance.IsOccupied(x, y))
{
    // Place unit
}

// Get neighbors
List<GridCell> neighbors = GridManager.Instance.GetNeighbors(position, includeDiagonal: false);

// Select cell
GridManager.Instance.SelectCell(new Vector2Int(2, 3));
```

## Architecture

### Components

**GridManager** (Singleton)
- Grid generation and management
- Coordinate conversion
- Cell queries (neighbors, occupancy, validation)
- Cell selection system
- Events: OnCellSelected, OnCellDeselected

**GridCell** (Component)
- Individual cell behavior
- Visual state management (Normal, Hover, Selected, Invalid, Occupied)
- Mouse interaction (hover, click)
- Occupancy tracking

**CellState** (Enum)
- 5 visual states for cell appearance

### Grid Layout

```
Portrait Mode (9:16):
- Screen usage: 95% width
- Bottom margin: 10% height
- Top reserved: 20% for HUD
- Cell size: Dynamic (calculated from screen width)

Grid Coordinates:
(0,9)  (1,9)  (2,9)  (3,9)  (4,9)  (5,9)  ← Top
(0,8)  (1,8)  (2,8)  (3,8)  (4,8)  (5,8)
...
(0,1)  (1,1)  (2,1)  (3,1)  (4,1)  (5,1)
(0,0)  (1,0)  (2,0)  (3,0)  (4,0)  (5,0)  ← Bottom
   ↑
 Origin
```

## Visual States

- **Normal**: Semi-transparent white (α = 0.2)
- **Hover**: Yellow highlight (α = 0.4)
- **Selected**: Green highlight (α = 0.6)
- **Invalid**: Red highlight (α = 0.4)
- **Occupied**: Blue tint (α = 0.3)

## API Reference

### GridManager Public Methods

```csharp
// Coordinate Conversion
Vector2Int? WorldToGrid(Vector3 worldPos)
Vector3 GridToWorld(Vector2Int gridPos)

// Cell Queries
GridCell GetCellAt(int x, int y)
GridCell GetCellAt(Vector2Int pos)
List<GridCell> GetNeighbors(Vector2Int pos, bool includeDiagonal = false)
bool IsValidPosition(int x, int y)
bool IsValidPosition(Vector2Int pos)
bool IsOccupied(int x, int y)
bool IsOccupied(Vector2Int pos)

// Cell Selection
void SelectCell(Vector2Int pos)
void DeselectAll()

// Properties
float CellSize { get; }
Vector3 GridOrigin { get; }
Vector2Int? SelectedCell { get; }

// Events
event Action<Vector2Int> OnCellSelected
event Action<Vector2Int> OnCellDeselected

// Constants
const int GRID_WIDTH = 6
const int GRID_HEIGHT = 10
```

### GridCell Public Methods

```csharp
// Visual State
void SetVisualState(CellState state)
void Highlight(Color color)
void ResetHighlight()

// Occupancy
void SetOccupied(GameObject unit)
void ClearOccupancy()

// Properties
Vector2Int Coordinates { get; set; }
bool IsOccupied { get; set; }
GameObject OccupyingUnit { get; set; }
CellState CurrentState { get; }
```

## Usage Examples

### Unit Placement
```csharp
// Subscribe to cell selection
GridManager.Instance.OnCellSelected += HandleCellSelected;

void HandleCellSelected(Vector2Int pos)
{
    GridCell cell = GridManager.Instance.GetCellAt(pos);

    if (!cell.IsOccupied && HasEnoughGold())
    {
        // Place unit
        GameObject unit = SpawnUnit();
        Vector3 worldPos = GridManager.Instance.GridToWorld(pos);
        unit.transform.position = worldPos;

        // Mark cell occupied
        cell.SetOccupied(unit);
    }
}
```

### Monster Pathfinding
```csharp
// Get path from top to bottom
List<Vector2Int> path = new List<Vector2Int>();

for (int y = GRID_HEIGHT - 1; y >= 0; y--)
{
    path.Add(new Vector2Int(columnIndex, y));
}

// Convert to world positions
foreach (var gridPos in path)
{
    Vector3 worldPos = GridManager.Instance.GridToWorld(gridPos);
    waypoints.Add(worldPos);
}
```

### Area Effect Targeting
```csharp
// Get all cells within range
Vector2Int centerPos = new Vector2Int(2, 5);
List<GridCell> affectedCells = new List<GridCell>();

for (int x = -range; x <= range; x++)
{
    for (int y = -range; y <= range; y++)
    {
        Vector2Int targetPos = centerPos + new Vector2Int(x, y);

        if (GridManager.Instance.IsValidPosition(targetPos))
        {
            GridCell cell = GridManager.Instance.GetCellAt(targetPos);
            affectedCells.Add(cell);
        }
    }
}
```

## Testing

### Automated Tests (GridSystemTester)

Run comprehensive test suite:
1. Add GridSystemTester component
2. Press Play
3. View results in Console

Tests include:
- Grid generation (60 cells)
- Coordinate conversion accuracy
- Cell query operations
- Neighbor system (cardinal + diagonal)
- Cell selection system
- Performance metrics

### Manual Testing

1. **Grid Visualization**: Press G in Play mode to see grid overlay
2. **Cell Selection**: Click cells to test selection
3. **Hover Effect**: Move mouse over cells to see hover state
4. **Performance**: Grid generates in <100ms

## Performance

- **Grid Generation**: <100ms (60 cells)
- **Coordinate Conversion**: <0.001ms per operation
- **Memory**: ~5KB for grid data structure
- **Update Cost**: Zero (event-driven, no Update() polling)

## Integration

### GameHUD
- Located in Assets/Scripts/UI/GameHUD.cs
- Displays 6 game stats at top of screen
- Auto-updates from GameplayManager events
- See GameHUD.cs for details

### Dependencies
- Unity 2022.3 LTS+
- GameplayManager (for HUD integration)

### Used By
- Unit Placement System (Issue #12)
- Monster Spawning System (Issue #13)
- Combat System (Issue #14)

## Troubleshooting

### Grid not visible
- Check Main Camera is orthographic
- Verify cell sprite/material is assigned
- Check GridManager exists in scene

### Cell clicks not working
- Ensure BoxCollider2D attached to cells
- Check EventSystem exists in scene
- Verify camera has Physics2DRaycaster

### HUD not updating
- Check GameplayManager exists
- Verify GameHUD subscribed to events
- Check TextMeshPro components assigned

## Documentation

- **Setup Guide**: GridSystemSetup.md (detailed Unity Editor setup)
- **Completion Report**: .claude/epics/lotto-defense/updates/19-completion.md
- **Issue Specification**: .claude/epics/lotto-defense/19.md

## License

Part of Lotto Defense game project.
