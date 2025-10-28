# Grid System Setup Guide

## Overview
This guide explains how to set up the 6x10 grid system and GameHUD in GameScene.unity.

## Prerequisites
- Unity 2022.3 LTS or later
- TextMeshPro package installed
- GameScene.unity open in Unity Editor

## Setup Steps

### 1. Add GridManager Component

1. In the Hierarchy, find or create an empty GameObject named "GridManager"
2. Add the `GridManager` component (LottoDefense.Grid.GridManager)
3. The GridManager will automatically generate the grid on start
4. **Optional**: Create a cell prefab for custom visual styling
   - Create a new GameObject with SpriteRenderer and BoxCollider2D
   - Add GridCell component
   - Assign as the Cell Prefab in GridManager inspector

### 2. Setup GameHUD

1. In the Hierarchy, expand Canvas
2. Create a new empty GameObject named "HUD" as a child of Canvas
3. Add the `GameHUD` component to the HUD GameObject
4. Create TextMeshPro UI text components (6 total):
   - **RoundText**: Display current round
   - **TimeText**: Display remaining time
   - **MonsterText**: Display active monsters
   - **GoldText**: Display current gold
   - **UnitText**: Display placed units
   - **LifeText**: Display remaining life

### 3. HUD Layout (Portrait Mode)

Recommended layout for 9:16 aspect ratio:

```
Canvas (1080x1920)
└── HUD (Top 20% of screen)
    ├── Row 1 (Top)
    │   ├── RoundText (Left)
    │   ├── TimeText (Center)
    │   └── MonsterText (Right)
    └── Row 2 (Below Row 1)
        ├── GoldText (Left)
        ├── UnitText (Center)
        └── LifeText (Right)
```

#### HUD RectTransform Settings:
- **HUD Container**:
  - Anchor: Top-Center (x: 0.5, y: 1)
  - Position: (0, -100)
  - Size: (1000, 200)

- **Text Components** (all):
  - Font Size: 32-40
  - Alignment: Center
  - Auto Size: Enabled
  - Color: White

#### Specific Text Positions:
- **RoundText**: Anchor (0, 1), Position (-350, -50)
- **TimeText**: Anchor (0.5, 1), Position (0, -50)
- **MonsterText**: Anchor (1, 1), Position (350, -50)
- **GoldText**: Anchor (0, 1), Position (-350, -130)
- **UnitText**: Anchor (0.5, 1), Position (0, -130)
- **LifeText**: Anchor (1, 1), Position (350, -130)

### 4. Link Components

1. Select the HUD GameObject
2. In the GameHUD component inspector:
   - Drag each TextMeshPro component to its corresponding field
   - Round Text → roundText
   - Time Text → timeText
   - Monster Text → monsterText
   - Gold Text → goldText
   - Unit Text → unitText
   - Life Text → lifeText

### 5. Camera Settings

Ensure Main Camera is configured for portrait orientation:
- Projection: Orthographic
- Orthographic Size: 5 (adjustable)
- Position: (0, 0, -10)
- Clear Flags: Solid Color
- Background: Dark color (e.g., #0D0D1A)

### 6. Testing

1. Enter Play Mode
2. Verify grid generates correctly:
   - 6 columns × 10 rows (60 cells)
   - Cells are evenly spaced
   - Grid is centered on screen
3. Test cell interaction:
   - Hover over cells (should show yellow tint)
   - Click cells (should show green selection)
   - Only one cell selected at a time
4. Verify HUD displays:
   - Round: 1
   - Time: 00:00
   - Monsters: 0
   - Gold: 50 (from GameplayManager)
   - Units: 0
   - Life: 10 (from GameplayManager)

## Visual Specifications

### Grid Cell States:
- **Normal**: Semi-transparent white (α = 0.2)
- **Hover**: Yellow tint (α = 0.4)
- **Selected**: Green tint (α = 0.6)
- **Invalid**: Red tint (α = 0.4)
- **Occupied**: Blue tint (α = 0.3)

### Cell Dimensions:
- Size: Calculated dynamically (screen width * 0.95 / 6)
- Border: 2px white outline
- Total Grid: 6 columns × 10 rows

### Grid Position:
- Center-bottom of screen
- Bottom margin: 10% of screen height
- Leaves top 20% for HUD

## Common Issues

### Issue: Grid cells not visible
**Solution**: Check camera orthographic size and grid cell sprite renderer settings

### Issue: Cell clicks not working
**Solution**: Ensure BoxCollider2D is attached and Main Camera has Physics2DRaycaster

### Issue: HUD not updating
**Solution**: Verify GameplayManager exists in scene and GameHUD is subscribed to events

### Issue: Grid cells overlap or misaligned
**Solution**: Check CellSize calculation and GridOrigin position

## API Reference

### GridManager Public Methods:
```csharp
// Coordinate conversion
Vector2Int? WorldToGrid(Vector3 worldPos)
Vector3 GridToWorld(Vector2Int gridPos)

// Cell queries
GridCell GetCellAt(int x, int y)
List<GridCell> GetNeighbors(Vector2Int pos, bool includeDiagonal = false)
bool IsValidPosition(int x, int y)
bool IsOccupied(int x, int y)

// Cell selection
void SelectCell(Vector2Int pos)
void DeselectAll()
```

### GameHUD Public Methods:
```csharp
// Update individual stats
void UpdateRound(int round)
void UpdateTime(float seconds)
void UpdateMonsterCount(int count)
void UpdateGold(int gold)
void UpdateUnitCount(int count)
void UpdateLife(int life)

// Manual refresh
void RefreshAllValues()
```

## Performance Notes

- Grid generation completes in <100ms on modern hardware
- Cell interaction uses Unity's built-in OnMouse events (efficient)
- HUD updates only when values change (event-driven, no polling)
- Singleton pattern ensures single instance of GridManager

## Next Steps

After setup:
1. Test grid coordinate conversion accuracy
2. Implement unit placement on grid cells
3. Add monster pathfinding using grid
4. Connect HUD to round management system
