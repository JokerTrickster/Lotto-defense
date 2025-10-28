# Unit Placement & Swap System

Complete implementation of the click-to-place and click-to-swap mechanics for the Lotto Defense game.

## Overview

The placement system allows players to:
1. **Select** units from their inventory by clicking unit cards
2. **Place** units on empty grid cells during the Preparation phase
3. **Swap** positions of already-placed units
4. Receive **visual feedback** for all interactions
5. Automatic **validation** of placement rules

## Architecture

### Component Hierarchy
```
UnitPlacementManager (Singleton)
├── Placement Mode Management
├── Validation Logic
└── Event Coordination
    ├── Unit.cs (MonoBehaviour per placed unit)
    ├── GridManager (Grid occupancy tracking)
    ├── UnitManager (Inventory integration)
    └── GameplayManager (Phase validation)
```

### Key Components

#### 1. Unit.cs
**Purpose**: MonoBehaviour attached to each placed unit instance

**Responsibilities**:
- Track grid position
- Handle visual selection state
- Detect mouse clicks for swap mode
- Sync world position with grid coordinates

**Properties**:
- `Data` - Reference to UnitData ScriptableObject
- `GridPosition` - Current grid coordinates (Vector2Int)
- `IsSelected` - Selection state for visual feedback

#### 2. UnitPlacementManager.cs
**Purpose**: Central coordinator for all placement and swap logic

**Responsibilities**:
- Manage placement mode state
- Validate placement rules
- Handle inventory → grid placement flow
- Coordinate unit swapping
- Fire events for UI updates

**Key States**:
- `IsPlacementMode` - Active when unit selected from inventory
- `SelectedUnitData` - Unit waiting to be placed
- `SelectedPlacedUnit` - Unit selected for swapping

#### 3. InventoryUI.cs
**Purpose**: Display available units and enable selection

**Responsibilities**:
- Render unit cards from inventory
- Handle card click events
- Refresh on inventory changes
- Trigger placement mode

#### 4. GridManager Extensions
**Purpose**: Add unit management to existing grid system

**New Methods**:
```csharp
GetUnitAt(x, y) → Unit          // Retrieve unit at position
SetUnit(x, y, GameObject)       // Place unit on grid
RemoveUnit(x, y) → GameObject   // Clear unit from cell
IsPlacementCell(x, y) → bool    // Validate placement location
```

## Placement Flow

### Initial Placement
```
┌─────────────────────────────────────┐
│ 1. Player draws unit from gacha    │
└────────────┬────────────────────────┘
             ↓
┌─────────────────────────────────────┐
│ 2. Unit added to inventory          │
└────────────┬────────────────────────┘
             ↓
┌─────────────────────────────────────┐
│ 3. InventoryUI displays unit card   │
└────────────┬────────────────────────┘
             ↓
┌─────────────────────────────────────┐
│ 4. Player clicks unit card          │
└────────────┬────────────────────────┘
             ↓
┌─────────────────────────────────────┐
│ 5. UnitPlacementManager              │
│    .SelectUnitForPlacement()        │
│    - Validates phase (Preparation)  │
│    - Enters placement mode          │
└────────────┬────────────────────────┘
             ↓
┌─────────────────────────────────────┐
│ 6. Player clicks grid cell          │
└────────────┬────────────────────────┘
             ↓
┌─────────────────────────────────────┐
│ 7. GridCell.OnMouseDown()           │
│    → GridManager.SelectCell()       │
└────────────┬────────────────────────┘
             ↓
┌─────────────────────────────────────┐
│ 8. UnitPlacementManager              │
│    .OnGridCellClicked(pos)          │
│    - Validates placement rules      │
└────────────┬────────────────────────┘
             ↓
┌─────────────────────────────────────┐
│ 9. PlaceUnit(pos)                   │
│    - Instantiate prefab             │
│    - Add Unit component             │
│    - Initialize with data + pos     │
│    - Update grid occupancy          │
│    - Remove from inventory          │
└────────────┬────────────────────────┘
             ↓
┌─────────────────────────────────────┐
│ 10. Fire OnUnitPlaced event         │
│     Exit placement mode             │
└─────────────────────────────────────┘
```

### Unit Swap Flow
```
┌─────────────────────────────────────┐
│ 1. Player clicks placed unit A      │
└────────────┬────────────────────────┘
             ↓
┌─────────────────────────────────────┐
│ 2. Unit.OnMouseDown()                │
│    → UnitPlacementManager            │
│      .OnPlacedUnitClicked(A)        │
│    - Validates phase (Preparation)  │
│    - Selects unitA                  │
│    - Highlights unitA               │
└────────────┬────────────────────────┘
             ↓
┌─────────────────────────────────────┐
│ 3. Player clicks different unit B   │
└────────────┬────────────────────────┘
             ↓
┌─────────────────────────────────────┐
│ 4. UnitPlacementManager              │
│    .OnPlacedUnitClicked(B)          │
│    - Detects swap intent            │
└────────────┬────────────────────────┘
             ↓
┌─────────────────────────────────────┐
│ 5. SwapUnits(A, B)                  │
│    - Store pos1 = A.GridPosition    │
│    - Store pos2 = B.GridPosition    │
│    - Remove both from grid          │
│    - Update positions               │
│    - Place at swapped positions     │
└────────────┬────────────────────────┘
             ↓
┌─────────────────────────────────────┐
│ 6. Fire OnUnitsSwapped event        │
│    Deselect both units              │
└─────────────────────────────────────┘
```

## Validation Rules

### Placement Validation Hierarchy
```csharp
IsValidPlacement(gridPos) returns true if ALL of:
1. CanPlaceUnits() == true
   → GameplayManager.CurrentState == GameState.Preparation

2. IsValidPosition(gridPos) == true
   → x >= 0 && x < 6 && y >= 0 && y < 10

3. IsPlacementCell(gridPos) == true
   → x != 2 && x != 3 (not on monster paths)

4. !IsOccupied(gridPos)
   → GridCell.IsOccupied == false
```

### Phase Restrictions
| Game Phase     | Placement | Swap |
|----------------|-----------|------|
| Countdown      | ❌        | ❌   |
| **Preparation**| ✅        | ✅   |
| Combat         | ❌        | ❌   |
| RoundResult    | ❌        | ❌   |
| Victory        | ❌        | ❌   |
| Defeat         | ❌        | ❌   |

Only the **Preparation** phase allows placement and swapping.

### Grid Layout
```
     x: 0   1   2   3   4   5
        ┌───┬───┬───┬───┬───┬───┐
y: 0   │ ✓ │ ✓ │ ✗ │ ✗ │ ✓ │ ✓ │
       ├───┼───┼───┼───┼───┼───┤
   1   │ ✓ │ ✓ │ ✗ │ ✗ │ ✓ │ ✓ │
       ├───┼───┼───┼───┼───┼───┤
   2   │ ✓ │ ✓ │ ✗ │ ✗ │ ✓ │ ✓ │
       ├───┼───┼───┼───┼───┼───┤
   ... │...│...│ ↓ │ ↑ │...│...│
       ├───┼───┼───┼───┼───┼───┤
   9   │ ✓ │ ✓ │ ✗ │ ✗ │ ✓ │ ✓ │
       └───┴───┴───┴───┴───┴───┘

Legend:
✓ = Valid placement cell
✗ = Monster path (no placement)
↓ = Top path (x=3, monsters move down)
↑ = Bottom path (x=2, monsters move up)

Valid columns: x ∈ {0, 1, 4, 5}
Path columns:  x ∈ {2, 3}
```

## Event System

### Events Provided

#### UnitPlacementManager Events
```csharp
// Placement events
OnUnitPlaced(Unit unit, Vector2Int pos)
OnPlacementModeEntered(UnitData data)
OnPlacementModeExited()
OnPlacementFailed(string reason)

// Swap events
OnUnitsSwapped(Unit u1, Unit u2, Vector2Int p1, Vector2Int p2)
```

#### Integration Events
```csharp
// UnitManager
OnInventoryChanged(List<UnitData> inv, string op, UnitData unit)

// GridManager
OnCellSelected(Vector2Int pos)
OnCellDeselected(Vector2Int pos)
```

### Event Usage Examples
```csharp
// Listen for placement
UnitPlacementManager.Instance.OnUnitPlaced += (unit, pos) => {
    Debug.Log($"Unit {unit.Data.unitName} placed at {pos}");
    PlayPlacementSound();
    ShowPlacementEffect(pos);
};

// Listen for swap
UnitPlacementManager.Instance.OnUnitsSwapped += (u1, u2, p1, p2) => {
    Debug.Log($"Swapped {u1.Data.unitName} and {u2.Data.unitName}");
    PlaySwapAnimation(u1, u2);
};

// Listen for failures
UnitPlacementManager.Instance.OnPlacementFailed += (reason) => {
    ShowErrorMessage(reason);
};
```

## Visual Feedback

### Colors
```csharp
// Unit selection
Color normalColor = Color.white;
Color selectedColor = Color.yellow * 1.5f;

// Grid cell highlights
Color validPlacementColor = new Color(0f, 1f, 0f, 0.3f);   // Green
Color invalidPlacementColor = new Color(1f, 0f, 0f, 0.3f); // Red
Color swappableColor = new Color(1f, 1f, 0f, 0.3f);        // Yellow
```

### Visual States
| State                | Visual Effect                    |
|----------------------|----------------------------------|
| Unit Normal          | White sprite                     |
| Unit Selected        | Yellow glow (1.5x brightness)    |
| Cell Hover (Valid)   | Green semi-transparent overlay   |
| Cell Hover (Invalid) | Red semi-transparent overlay     |
| Cell Occupied        | Unit sprite visible              |

### Input Feedback
- **Placement Mode**: Cursor changes, valid cells highlighted
- **Swap Mode**: Selected unit glows yellow
- **ESC Key**: Cancels placement/swap, clears highlights
- **Invalid Click**: Red flash on cell, error message

## API Reference

### UnitPlacementManager

#### Public Methods
```csharp
// Mode control
void SelectUnitForPlacement(UnitData unitData)
void CancelPlacement()

// Validation
bool CanPlaceUnits()
bool IsValidPlacement(Vector2Int gridPos)

// Visual feedback
void HighlightCell(Vector2Int gridPos, bool isValid)
void ClearCellHighlight(Vector2Int gridPos)

// Debug
string GetPlacementStateInfo()
```

#### Public Properties
```csharp
bool IsPlacementMode { get; }
UnitData SelectedUnitData { get; }
Unit SelectedPlacedUnit { get; }
```

### Unit

#### Public Methods
```csharp
void Initialize(UnitData unitData, Vector2Int gridPos)
void Select()
void Deselect()
void MoveTo(Vector2Int newGridPos)
```

#### Public Properties
```csharp
UnitData Data { get; }
Vector2Int GridPosition { get; set; }
bool IsSelected { get; }
```

### GridManager Extensions

#### New Methods
```csharp
Unit GetUnitAt(int x, int y)
Unit GetUnitAt(Vector2Int pos)

bool SetUnit(int x, int y, GameObject unitObject)
bool SetUnit(Vector2Int pos, GameObject unitObject)

GameObject RemoveUnit(int x, int y)
GameObject RemoveUnit(Vector2Int pos)

bool IsPlacementCell(int x, int y)
bool IsPlacementCell(Vector2Int pos)

Vector3 GetCellWorldPosition(int x, int y)
Vector3 GetCellWorldPosition(Vector2Int pos)
```

## Usage Examples

### Basic Setup
```csharp
// In scene setup (auto-initialized):
// - UnitPlacementManager (singleton)
// - GridManager (from Issue #19)
// - UnitManager (from Issue #21)
// - GameplayManager (from Issue #17)

// In UI setup:
// 1. Create Canvas
// 2. Add InventoryUI component
// 3. Assign inventoryContainer (parent for unit cards)
// 4. Optionally assign unitCardPrefab (or uses default)
```

### Programmatic Placement
```csharp
// Get unit from inventory
UnitData myUnit = UnitManager.Instance.Inventory[0];

// Enter placement mode
UnitPlacementManager.Instance.SelectUnitForPlacement(myUnit);

// Check if position is valid
Vector2Int pos = new Vector2Int(0, 5);
if (UnitPlacementManager.Instance.IsValidPlacement(pos))
{
    // Position is valid - wait for player click
    // Or manually trigger placement via grid cell selection
    GridManager.Instance.SelectCell(pos);
}

// Cancel placement
UnitPlacementManager.Instance.CancelPlacement();
```

### Programmatic Swap
```csharp
// Get placed units
Unit unitA = GridManager.Instance.GetUnitAt(0, 5);
Unit unitB = GridManager.Instance.GetUnitAt(1, 5);

// Simulate swap clicks
UnitPlacementManager.Instance.OnPlacedUnitClicked(unitA); // Select A
UnitPlacementManager.Instance.OnPlacedUnitClicked(unitB); // Swap with B
```

## Testing

### Test Suite: UnitPlacementTester.cs
**13 automated test scenarios** covering:
- Manager initialization
- Placement cell validation
- Path exclusion
- Bounds checking
- Phase restrictions
- Placement flow
- Inventory integration
- Swap mechanics
- Visual feedback

### Running Tests
```csharp
// Method 1: Keyboard
// - Press Play in Unity
// - Press [T] key
// - Check Console for results

// Method 2: Context Menu
// - Right-click UnitPlacementTester component
// - Select "Run All Tests"

// Method 3: Script
UnitPlacementTester tester = FindFirstObjectByType<UnitPlacementTester>();
tester.RunAllTests();
```

### Test Results Format
```
=== UNIT PLACEMENT SYSTEM TEST SUITE ===
[PASS] Managers Initialized
[PASS] Valid Placement Cells
[PASS] Invalid Placement on Paths
[PASS] Placement Out of Bounds Validation
[PASS] Placement Allowed During Preparation
[PASS] Placement Blocked During Combat
[PASS] Basic Unit Placement Flow
[PASS] Placement Removes from Inventory
[PASS] Cannot Place on Occupied Cell
[PASS] Unit Swap Mechanics
[PASS] Swap Same Position Validation
[PASS] Selection Highlight System

=== TEST RESULTS ===
PASSED: 13
FAILED: 0
TOTAL:  13
==================
```

## Troubleshooting

### Issue: Units not placing
**Symptoms**: Clicks don't place units
**Solutions**:
1. Check `GameplayManager.CurrentState == Preparation`
2. Verify `UnitPlacementManager.Instance.CanPlaceUnits()` returns true
3. Ensure clicked cell is not on path (x != 2, x != 3)
4. Check cell is not already occupied

### Issue: Swap not working
**Symptoms**: Clicking units doesn't swap
**Solutions**:
1. Must be in Preparation phase
2. Click first unit to select (yellow highlight)
3. Click second unit to swap
4. Ensure both units have Unit component attached

### Issue: Visual feedback missing
**Symptoms**: No highlights or selection indicators
**Solutions**:
1. Check GridCell has SpriteRenderer
2. Verify colors are assigned in UnitPlacementManager
3. Ensure Unit has SpriteRenderer component
4. Check sprite renderer sorting order (units = 10, cells = 0)

### Issue: Inventory UI not showing units
**Symptoms**: Empty inventory display
**Solutions**:
1. Verify InventoryUI.inventoryContainer is assigned
2. Check UnitManager.Instance.Inventory has units
3. Ensure InventoryUI.Start() was called
4. Try manual refresh: `InventoryUI.RefreshInventoryDisplay()`

## Performance

### Optimization Strategies
1. **Object Pooling**: Reuse unit GameObjects instead of Destroy/Instantiate
2. **Event Batching**: Batch inventory changes to reduce UI updates
3. **Lazy Initialization**: Create unit cards on-demand
4. **Culling**: Hide off-screen inventory cards

### Current Performance
- **Placement**: O(1) grid cell lookup
- **Validation**: O(1) bounds checking
- **Swap**: O(1) position exchange
- **Inventory Display**: O(n) where n = number of units (limited to 10)

## Dependencies

### Required Components
- ✅ GridManager (Issue #19) - Grid system and cell management
- ✅ UnitManager (Issue #21) - Unit data and inventory
- ✅ GameplayManager (Issue #17) - Game state and phase management
- ✅ GridCell - Cell occupancy tracking and visuals

### Optional Components
- Unit prefabs (creates default if missing)
- Unit card prefab (creates default if missing)
- Custom sprites and icons

## Future Enhancements

### Phase 2 Features
- [ ] Drag-and-drop placement alternative
- [ ] Unit rotation for directional facing
- [ ] Batch placement mode (multiple units)
- [ ] Placement preview (ghost unit)
- [ ] Undo/redo stack for swaps

### UI Improvements
- [ ] Grid overlay toggle
- [ ] Keybindings for quick unit selection (1-9)
- [ ] Touch gesture support (mobile)
- [ ] Unit tooltip on hover

### Visual Polish
- [ ] Placement animations (fade in, scale)
- [ ] Swap animations (arc movement)
- [ ] Particle effects on placement
- [ ] Sound effects for placement/swap
- [ ] Haptic feedback (mobile)

## File Structure
```
Assets/Scripts/Units/
├── Unit.cs                      // Placed unit MonoBehaviour
├── UnitData.cs                  // Unit ScriptableObject (Issue #21)
├── UnitManager.cs               // Inventory & gacha (Issue #21)
├── UnitPlacementManager.cs      // Placement & swap coordinator
├── UnitPlacementTester.cs       // Automated test suite
└── PLACEMENT_README.md          // This file

Assets/Scripts/UI/
└── InventoryUI.cs               // Inventory display & selection

Assets/Scripts/Grid/
├── GridManager.cs               // Extended with unit management
└── GridCell.cs                  // Cell occupancy tracking
```

## Credits

**Developed**: 2025-10-28
**Issue**: #12 - Unit Placement & Swap System
**Dependencies**: Issues #17, #19, #21
**Unblocks**: Issues #14, #16, #18

**Total Lines**: ~1866 lines of new code
- Unit.cs: 151 lines
- UnitPlacementManager.cs: 436 lines
- InventoryUI.cs: 280 lines
- UnitPlacementTester.cs: 665 lines
- GridManager extensions: 167 lines
- Documentation: 167 lines

---

For questions or issues, see the test suite or completion documentation in:
`.claude/epics/lotto-defense/updates/12-completion.md`
