# Units System

Complete implementation of the unit data structure and gacha system for Lotto Defense.

## Overview

The Units system provides:
- **Unit Data Management**: ScriptableObject-based unit templates
- **Gacha System**: Weighted random unit acquisition (50/30/15/5 drop rates)
- **Inventory Management**: Unit storage with capacity limits
- **Gold Integration**: 5 gold cost per draw with GameplayManager validation

## Architecture

### Core Components

#### 1. UnitData (ScriptableObject)
Designer-editable unit template with:
- **Basic Info**: Name, Type, Rarity
- **Combat Stats**: Attack, Defense, Attack Range, Attack Speed
- **Visual References**: Icon sprite, Unit prefab
- **Metadata**: Description, calculated DPS

```csharp
// Example usage
UnitData warrior = ScriptableObject.CreateInstance<UnitData>();
warrior.unitName = "Elite Guardian";
warrior.type = UnitType.Melee;
warrior.rarity = Rarity.Epic;
warrior.attack = 42;
warrior.defense = 28;
```

#### 2. UnitManager (Singleton)
Central manager for gacha and inventory operations:

**Gacha System:**
```csharp
// Draw a random unit (costs 5 gold)
UnitData drawnUnit = UnitManager.Instance.DrawUnit();

// Check if player can afford draw
bool canDraw = UnitManager.Instance.CanDraw();
```

**Inventory Operations:**
```csharp
// Add unit to inventory
UnitManager.Instance.AddUnit(unitData);

// Remove unit from inventory
UnitManager.Instance.RemoveUnit(unitData);

// Get all units
List<UnitData> allUnits = UnitManager.Instance.GetInventory();

// Query by type or rarity
List<UnitData> meleeUnits = UnitManager.Instance.GetUnitsByType(UnitType.Melee);
List<UnitData> legendaryUnits = UnitManager.Instance.GetUnitsByRarity(Rarity.Legendary);
```

#### 3. Enums

**UnitType:**
- `Melee`: Close-range combat units (high defense, short range)
- `Ranged`: Long-range attack units (high attack, low defense)
- `Debuffer`: Support units (moderate stats, debuff abilities)

**Rarity:**
- `Normal`: 50% drop rate
- `Rare`: 30% drop rate
- `Epic`: 15% drop rate
- `Legendary`: 5% drop rate

## Unit Configuration

### Creating Units (Unity Editor)

1. **Right-click in Project** → Create → Lotto Defense → Unit Data
2. **Configure the asset:**
   - Set Unit Name (e.g., "Dragon Slayer")
   - Set Type (Melee/Ranged/Debuffer)
   - Set Rarity (Normal/Rare/Epic/Legendary)
   - Configure combat stats
   - Assign icon and prefab (optional)

3. **Add to UnitManager:**
   - Select UnitManager in Hierarchy
   - Drag unit to appropriate rarity list in Inspector

### Stat Scaling Guidelines

| Rarity | Attack | Defense | Range (Melee) | Range (Ranged) | Attack Speed |
|--------|--------|---------|---------------|----------------|--------------|
| Normal | 10-15 | 5-8 | 1.0-1.5 | 3.0-4.0 | 0.8-1.0 |
| Rare | 20-30 | 10-15 | 1.0-1.5 | 3.5-4.5 | 1.0-1.3 |
| Epic | 35-50 | 20-30 | 1.0-1.5 | 4.0-5.0 | 1.3-1.6 |
| Legendary | 60-80 | 35-50 | 1.0-1.5 | 4.5-5.5 | 1.6-2.0 |

**DPS Scaling:**
- Each rarity tier should provide ~2x DPS increase
- Legendary units should feel powerful but balanced

## Event System

### Available Events

```csharp
// Fired when unit successfully drawn
UnitManager.Instance.OnUnitDrawn += (UnitData unit, int remainingGold) => {
    Debug.Log($"Drew {unit.GetDisplayName()}, Gold: {remainingGold}");
};

// Fired when inventory changes
UnitManager.Instance.OnInventoryChanged += (List<UnitData> inventory, string operation, UnitData unit) => {
    Debug.Log($"Inventory {operation}: {unit?.unitName}");
};

// Fired when draw fails
UnitManager.Instance.OnDrawFailed += (string reason) => {
    Debug.LogWarning($"Draw failed: {reason}");
};
```

## Gacha System Details

### Probability Distribution

The gacha system uses weighted random selection:

```
Roll 0-5:     Legendary (5%)
Roll 5-20:    Epic (15%)
Roll 20-50:   Rare (30%)
Roll 50-100:  Normal (50%)
```

**Implementation:**
1. Generate random value 0-100
2. Map to rarity tier based on cumulative probabilities
3. Select random unit from chosen rarity pool
4. Deduct 5 gold from player
5. Add unit to inventory

### Validation

Before each draw:
- **Gold Check**: Player must have ≥5 gold
- **Inventory Check**: Must have available slots (max 50)
- **Pool Check**: Selected rarity pool must have units

## Testing

### UnitManagerTester

Comprehensive test suite with 6 test categories:

```csharp
// Run all tests
UnitManagerTester tester = GetComponent<UnitManagerTester>();
tester.RunAllTests();
```

**Test Suites:**
1. **Unit Pool Validation**: Verify all rarity tiers populated
2. **Gold Integration**: Test gold deduction and insufficient funds
3. **Inventory Operations**: Test add/remove functionality
4. **Gacha Probability Distribution**: 10,000 draw statistical test
5. **Inventory Capacity**: Test max capacity enforcement
6. **Event Notifications**: Verify all events fire correctly

### Expected Results

Running 10,000 gacha draws should yield (±2% margin):
- Normal: ~5000 (50%)
- Rare: ~3000 (30%)
- Epic: ~1500 (15%)
- Legendary: ~500 (5%)

## Integration with Other Systems

### GameplayManager
```csharp
// UnitManager checks gold before draw
int currentGold = GameplayManager.Instance.CurrentGold;
bool canAfford = currentGold >= UnitManager.Instance.GetGachaCost();

// Automatic gold deduction on successful draw
UnitManager.Instance.DrawUnit(); // Deducts 5 gold
```

### Grid System (Future)
```csharp
// Place unit on grid (future implementation)
GridManager.Instance.PlaceUnit(gridX, gridY, unitData);
```

## File Structure

```
Assets/
├── Scripts/
│   └── Units/
│       ├── UnitData.cs              # ScriptableObject definition
│       ├── UnitManager.cs           # Singleton manager
│       ├── UnitType.cs              # Type enum
│       ├── Rarity.cs                # Rarity enum
│       ├── UnitManagerTester.cs     # Test suite
│       └── README.md                # This file
└── Data/
    └── Units/
        ├── Melee/                   # Melee unit assets
        ├── Ranged/                  # Ranged unit assets
        ├── Debuffer/                # Debuffer unit assets
        ├── UNIT_CREATION_GUIDE.md   # Designer guide
        └── unit_stats_reference.json # Stat templates
```

## Performance Considerations

- **Memory**: Each UnitData is lightweight (~1KB)
- **Gacha Performance**: O(1) weighted selection
- **Inventory**: List operations are O(n), but typical size <50
- **Events**: Use weak references to prevent memory leaks

## Future Enhancements

- **Pity System**: Guaranteed legendary after X draws
- **Multi-Draw**: Draw 10 units with discount
- **Unit Preview**: Inspect unit before drawing
- **Duplicate Handling**: Convert duplicates to upgrade materials
- **Save/Load**: Persist inventory between sessions
- **Unit Synthesis**: Combine units for upgrades

## Troubleshooting

### No units drawn
- Verify unit pools are assigned in UnitManager Inspector
- Check that each rarity list has at least 1 unit
- Call `LoadUnitsFromResources()` if using Resources folder

### Probability seems off
- Run 10,000+ draw test for statistical validity
- Small sample sizes (<1000) can show variance
- Expected margin: ±2% for large samples

### Events not firing
- Verify event subscriptions are active
- Check for null references
- Use `+=` to add listeners, not `=`

## API Reference

### UnitManager Methods

| Method | Description | Returns |
|--------|-------------|---------|
| `DrawUnit()` | Draw random unit (costs 5 gold) | `UnitData` or `null` |
| `CanDraw()` | Check if draw is possible | `bool` |
| `AddUnit(unit)` | Add unit to inventory | `bool` |
| `RemoveUnit(unit)` | Remove unit from inventory | `bool` |
| `GetInventory()` | Get all units | `List<UnitData>` |
| `GetUnitsByType(type)` | Query units by type | `List<UnitData>` |
| `GetUnitsByRarity(rarity)` | Query units by rarity | `List<UnitData>` |
| `ClearInventory()` | Remove all units | `void` |
| `GetGachaCost()` | Get current gacha cost | `int` |

### UnitData Properties

| Property | Type | Description |
|----------|------|-------------|
| `unitName` | `string` | Display name |
| `type` | `UnitType` | Combat type |
| `rarity` | `Rarity` | Rarity tier |
| `attack` | `int` | Damage per hit |
| `defense` | `int` | Damage reduction |
| `attackRange` | `float` | Max attack distance |
| `attackSpeed` | `float` | Attacks per second |
| `icon` | `Sprite` | UI icon |
| `prefab` | `GameObject` | Unit instance |
| `description` | `string` | Flavor text |

## Dependencies

- **Namespace**: `LottoDefense.Units`
- **Requires**: `LottoDefense.Gameplay` (GameplayManager)
- **Unity Version**: 2022.3+
- **C# Version**: 9.0+

## Authors

- Unit System: Issue #21 Implementation
- Gacha Algorithm: Weighted probability distribution
- Testing Suite: Statistical validation framework

---

**Last Updated**: 2025-10-28
**Issue**: #21 - Unit Data & Gacha System
