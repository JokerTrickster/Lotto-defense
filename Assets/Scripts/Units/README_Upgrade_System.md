# Unit Upgrade System - Implementation Documentation

## Overview
The Unit Upgrade System allows players to spend gold to incrementally improve individual unit stats. This provides an alternative progression path to the synthesis system and enables tactical optimization of placed units.

## Core Components

### 1. UpgradeManager.cs
**Namespace:** `LottoDefense.Units`
**Type:** Singleton MonoBehaviour

**Responsibilities:**
- Calculate upgrade costs using formula: `10 * level^1.5`
- Calculate attack multipliers using formula: `1.0 + (0.1 * (level - 1))`
- Validate gold availability before upgrades
- Apply stat modifications to units
- Track upgrade events and notify listeners

**Key Methods:**
```csharp
bool CanUpgradeUnit(Unit unit)              // Check if upgrade is possible
bool TryUpgradeUnit(Unit unit)              // Attempt to upgrade a unit
int GetUpgradeCost(int currentLevel)        // Calculate cost for next level
float CalculateAttackMultiplier(int level)  // Get attack multiplier for level
UpgradeStats GetUpgradeStats(Unit unit)     // Get comprehensive upgrade info
```

**Events:**
- `OnUnitUpgraded(Unit, int newLevel, int newAttack)` - Fired on successful upgrade
- `OnUpgradeFailed(string reason)` - Fired when upgrade fails
- `OnUnitSelected(Unit)` - Fired when unit selected for upgrade

### 2. Unit.cs Extensions
**Added Properties:**
```csharp
int UpgradeLevel { get; private set; } = 1    // Current upgrade level (1-10)
int CurrentAttack { get; private set; }        // Attack including multiplier
```

**Added Methods:**
```csharp
void ApplyUpgrade(int newLevel, float attackMultiplier)  // Apply upgrade
float GetAttackMultiplier()                               // Get current multiplier
```

### 3. UpgradeUI.cs
**Namespace:** `LottoDefense.UI`
**Type:** MonoBehaviour

**Responsibilities:**
- Display unit information and current stats
- Show upgrade cost and next level preview
- Handle upgrade button interaction
- Provide visual feedback for success/failure
- Update display when gold changes

**Key Methods:**
```csharp
void Show(Unit unit)      // Display upgrade panel for unit
void Hide()               // Hide upgrade panel
void RefreshDisplay()     // Update all UI elements
```

**UI Elements:**
- Unit icon and name display
- Current level indicator
- Current/next attack values
- Upgrade cost with affordability coloring
- Upgrade button with state management
- Max level indicator

### 4. UpgradeManagerTester.cs
**Namespace:** `LottoDefense.Units`
**Type:** MonoBehaviour Test Suite

**Test Coverage:**
- Cost formula validation at all levels
- Attack multiplier calculations
- Gold deduction integration
- Max level enforcement
- Insufficient gold handling
- Multiple sequential upgrades
- Null unit handling
- Stats calculation accuracy

## Upgrade Formulas

### Cost Formula
```
Cost = 10 * level^1.5
```

**Cost Progression:**
- L1→L2: 10 gold
- L2→L3: 28 gold
- L3→L4: 52 gold
- L5→L6: 112 gold
- L9→L10: 270 gold
- **Total L1→L10: ~1,000 gold**

### Attack Multiplier Formula
```
Multiplier = 1.0 + (0.1 * (level - 1))
```

**Attack Progression (base attack 100):**
- Level 1: 100 ATK (1.0x)
- Level 2: 110 ATK (1.1x) +10%
- Level 5: 140 ATK (1.4x) +40%
- Level 10: 190 ATK (1.9x) +90%

## Integration Points

### GameplayManager Integration
- **Gold Validation:** Checks `GameplayManager.Instance.CurrentGold` before upgrades
- **Gold Deduction:** Calls `GameplayManager.Instance.ModifyGold(-cost)` on success
- **Event Subscription:** Listens to `OnGameValueChanged` for gold updates

### Unit System Integration
- **Unit Selection:** Works with placed Unit instances on grid
- **Stat Persistence:** Upgrade level and current attack stored per unit
- **Visual Updates:** ToString() includes upgrade level for debugging

### Grid System Integration
- **Placed Units Only:** Upgrades apply to units placed on grid cells
- **Position Tracking:** Unit grid position maintained during upgrades
- **Selection Flow:** Player selects unit → UpgradeUI shows → upgrade applied

## Usage Flow

### Basic Upgrade Flow
1. Player clicks on placed unit on grid
2. UpgradeUI shows with current stats
3. UI displays upgrade cost and next level stats
4. Player clicks "UPGRADE" button
5. UpgradeManager validates gold and level
6. Gold deducted, stats increased
7. UI refreshes with new stats
8. Success feedback shown (particles, sound)

### Error Handling
- **Insufficient Gold:** Button disabled, cost shown in red
- **Max Level:** Button shows "MAX LEVEL", upgrade disabled
- **Null Unit:** Graceful failure with warning logs

## Testing

### Running Tests
1. Attach `UpgradeManagerTester` to GameObject in scene
2. Assign test `UnitData` and optional `unitPrefab`
3. Check "Run Tests On Start" or use Context Menu
4. View results in Console

### Test Suites
- **Formula Tests:** Validate cost and multiplier calculations
- **Logic Tests:** Test upgrade success/failure conditions
- **Integration Tests:** Verify gold system integration
- **Edge Cases:** Handle null units, max level, etc.

### Manual Testing
Context Menu options:
- "Run All Tests" - Execute full test suite
- "Print Cost Curve" - Display upgrade progression
- "Test Upgrade UI Integration" - Test with live UI

## Configuration

### UpgradeManager Settings
```csharp
MAX_UPGRADE_LEVEL = 10           // Maximum upgrade level
BASE_COST = 10f                  // Base cost multiplier
COST_EXPONENT = 1.5f             // Cost curve steepness
ATTACK_INCREASE_PER_LEVEL = 0.1f // 10% per level
```

### UpgradeUI Settings (Inspector)
- **Panel References:** Assign panel GameObject
- **UI Elements:** Assign text and button components
- **Visual Feedback:** Configure colors for affordability
- **Audio:** Optional success sound and particles

## Performance Considerations

### Optimization
- Cost calculations: < 1ms (simple math operations)
- UI updates: < 16ms (targeted element updates)
- No per-frame updates (event-driven architecture)
- Singleton pattern for single manager instance

### Memory
- Minimal overhead: 2 ints per unit (level, attack)
- No persistent storage between sessions (room for future save system)
- Event cleanup on destroy to prevent leaks

## Future Enhancements

### Potential Features
1. **Save/Load System:** Persist upgrade levels across sessions
2. **Multiple Stats:** Add HP, Defense, Speed upgrades
3. **Upgrade Confirmation:** Confirm expensive upgrades
4. **Bulk Upgrades:** Hold button to rapidly upgrade
5. **Discount System:** Bulk upgrade discounts
6. **Visual Indicators:** Stars/glow on upgraded units
7. **Undo System:** Refund recent upgrades

### Balance Tuning
Easily adjustable parameters:
- Cost exponent (1.5) - adjust progression steepness
- Attack increase (10%) - adjust power scaling
- Max level (10) - extend upgrade ceiling
- Base cost (10) - adjust overall cost scale

## Dependencies

### Required Systems
- ✅ GameplayManager - Gold management
- ✅ Unit System - Unit data and instances
- ✅ Grid System - Unit placement

### Optional Systems
- UI System - Upgrade panel display
- Audio System - Success sound effects
- Particle System - Visual feedback

## API Reference

### UpgradeManager Public API
```csharp
// Core Methods
bool CanUpgradeUnit(Unit unit)
bool TryUpgradeUnit(Unit unit)
int GetUpgradeCost(int currentLevel)
float CalculateAttackMultiplier(int level)
UpgradeStats GetUpgradeStats(Unit unit)
int GetMaxLevel()

// Unit Selection
void SelectUnit(Unit unit)
void DeselectUnit()

// Debug Utilities
string GetUpgradeCostCurve()
```

### UpgradeUI Public API
```csharp
void Show(Unit unit)
void Hide()
bool IsVisible()
Unit GetCurrentUnit()
void RefreshDisplay()
```

### UpgradeStats Structure
```csharp
struct UpgradeStats
{
    int currentLevel;
    int maxLevel;
    bool isMaxLevel;
    int upgradeCost;
    int currentAttack;
    int nextAttack;
    int attackGain;
    bool canAfford;
}
```

## Troubleshooting

### Common Issues

**Issue:** Upgrade button not working
**Solution:** Verify GameplayManager and UpgradeManager exist in scene

**Issue:** Gold not deducted
**Solution:** Check GameplayManager.Instance is not null

**Issue:** Stats not updating
**Solution:** Ensure UpgradeUI subscribes to events in Start()

**Issue:** Tests failing
**Solution:** Assign testUnitData in UpgradeManagerTester inspector

## Example Usage

### Setup Scene
```csharp
// 1. Add UpgradeManager to scene (auto-creates as singleton)
// 2. Add UpgradeUI to Canvas
// 3. Configure UpgradeUI inspector references
// 4. Add UpgradeManagerTester for testing (optional)
```

### Programmatic Usage
```csharp
// Select unit for upgrade
UpgradeManager.Instance.SelectUnit(myUnit);

// Check if upgrade possible
if (UpgradeManager.Instance.CanUpgradeUnit(myUnit))
{
    // Attempt upgrade
    bool success = UpgradeManager.Instance.TryUpgradeUnit(myUnit);

    if (success)
    {
        Debug.Log($"Upgraded to level {myUnit.UpgradeLevel}");
    }
}

// Get upgrade info for display
UpgradeStats stats = UpgradeManager.Instance.GetUpgradeStats(myUnit);
Debug.Log($"Next upgrade costs {stats.upgradeCost} gold");
```

---

**Implementation Date:** 2025-10-28
**Issue:** #18
**Author:** Unity Game Developer Agent
**Status:** Complete ✅
