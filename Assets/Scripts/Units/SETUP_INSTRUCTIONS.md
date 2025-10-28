# Unit System Setup Instructions

Step-by-step guide to configure the Unit system in Unity Editor.

## Prerequisites

✅ GameplayManager scene setup (Issue #17)
✅ Grid System implemented (Issue #19)

## Setup Steps

### Step 1: Create UnitManager GameObject

1. **In Hierarchy** → Right-click → Create Empty
2. **Rename** to "UnitManager"
3. **Add Component** → Scripts → Units → UnitManager
4. **Note**: UnitManager will persist across scenes (DontDestroyOnLoad)

### Step 2: Create Unit Data Assets

You need to create **12 UnitData ScriptableObject assets** (3 types × 4 rarities).

#### Create Normal Units (4)

**Melee Normal:**
1. **Navigate to**: Assets/Data/Units/Melee/
2. **Right-click** → Create → Lotto Defense → Unit Data
3. **Configure**:
   - Name: "MeleeNormal"
   - Unit Name: "Recruit Warrior"
   - Type: Melee
   - Rarity: Normal
   - Attack: 12
   - Defense: 8
   - Attack Range: 1.2
   - Attack Speed: 0.9

**Ranged Normal:**
1. **Navigate to**: Assets/Data/Units/Ranged/
2. **Create Unit Data**
3. **Configure**:
   - Name: "RangedNormal"
   - Unit Name: "Recruit Archer"
   - Type: Ranged
   - Rarity: Normal
   - Attack: 15
   - Defense: 5
   - Attack Range: 3.5
   - Attack Speed: 1.0

**Debuffer Normal:**
1. **Navigate to**: Assets/Data/Units/Debuffer/
2. **Create Unit Data**
3. **Configure**:
   - Name: "DebufferNormal"
   - Unit Name: "Apprentice Mage"
   - Type: Debuffer
   - Rarity: Normal
   - Attack: 8
   - Defense: 6
   - Attack Range: 2.5
   - Attack Speed: 0.8

#### Create Rare Units (4)

**Melee Rare:**
- Name: "MeleeRare"
- Unit Name: "Veteran Knight"
- Type: Melee, Rarity: Rare
- Attack: 25, Defense: 15, Range: 1.3, Speed: 1.1

**Ranged Rare:**
- Name: "RangedRare"
- Unit Name: "Skilled Marksman"
- Type: Ranged, Rarity: Rare
- Attack: 28, Defense: 10, Range: 4.0, Speed: 1.2

**Debuffer Rare:**
- Name: "DebufferRare"
- Unit Name: "Frost Wizard"
- Type: Debuffer, Rarity: Rare
- Attack: 18, Defense: 12, Range: 3.0, Speed: 1.0

#### Create Epic Units (4)

**Melee Epic:**
- Name: "MeleeEpic"
- Unit Name: "Elite Guardian"
- Type: Melee, Rarity: Epic
- Attack: 42, Defense: 28, Range: 1.4, Speed: 1.4

**Ranged Epic:**
- Name: "RangedEpic"
- Unit Name: "Master Sniper"
- Type: Ranged, Rarity: Epic
- Attack: 48, Defense: 18, Range: 4.5, Speed: 1.5

**Debuffer Epic:**
- Name: "DebufferEpic"
- Unit Name: "Shadow Sorcerer"
- Type: Debuffer, Rarity: Epic
- Attack: 32, Defense: 22, Range: 3.5, Speed: 1.3

#### Create Legendary Units (4)

**Melee Legendary:**
- Name: "MeleeLegendary"
- Unit Name: "Dragon Slayer"
- Type: Melee, Rarity: Legendary
- Attack: 70, Defense: 45, Range: 1.5, Speed: 1.8

**Ranged Legendary:**
- Name: "RangedLegendary"
- Unit Name: "Celestial Bowman"
- Type: Ranged, Rarity: Legendary
- Attack: 75, Defense: 30, Range: 5.0, Speed: 1.9

**Debuffer Legendary:**
- Name: "DebufferLegendary"
- Unit Name: "Archmage"
- Type: Debuffer, Rarity: Legendary
- Attack: 55, Defense: 38, Range: 4.0, Speed: 1.7

### Step 3: Assign Units to UnitManager

1. **Select UnitManager** in Hierarchy
2. **In Inspector**, find the unit pool lists:

**Normal Units (Size: 4)**
- Element 0: Drag MeleeNormal
- Element 1: Drag RangedNormal
- Element 2: Drag DebufferNormal
- Element 3: (Optional 4th Normal unit)

**Rare Units (Size: 4)**
- Element 0: Drag MeleeRare
- Element 1: Drag RangedRare
- Element 2: Drag DebufferRare
- Element 3: (Optional 4th Rare unit)

**Epic Units (Size: 4)**
- Element 0: Drag MeleeEpic
- Element 1: Drag RangedEpic
- Element 2: Drag DebufferEpic
- Element 3: (Optional 4th Epic unit)

**Legendary Units (Size: 4)**
- Element 0: Drag MeleeLegendary
- Element 1: Drag RangedLegendary
- Element 2: Drag DebufferLegendary
- Element 3: (Optional 4th Legendary unit)

**Max Inventory Size**: Leave at 50 (default)

### Step 4: Add Test Component (Optional)

For testing gacha probabilities:

1. **Select UnitManager** GameObject
2. **Add Component** → Scripts → Units → UnitManagerTester
3. **Configure**:
   - Probability Test Count: 10000
   - Run Tests On Start: False (manual trigger)

### Step 5: Verify Setup

1. **Enter Play Mode**
2. **Check Console** for initialization logs:
   ```
   [UnitManager] Initialized - Pool sizes: N=4, R=4, E=4, L=4
   ```
3. **Test gacha**:
   - Select UnitManager in Hierarchy
   - In UnitManagerTester component
   - Right-click → Run All Tests

Expected output:
```
========== UNIT MANAGER TEST SUITE ==========
[TEST 1] Unit Pool Validation
Normal pool: 4 units
Rare pool: 4 units
Epic pool: 4 units
Legendary pool: 4 units
✅ PASSED: All rarity pools populated
...
```

## Testing Workflow

### Manual Single Draw Test

1. **Ensure GameplayManager** has sufficient gold
2. **Open Console** (Ctrl/Cmd + Shift + C)
3. **In UnitManagerTester context menu**:
   - Test Single Draw
4. **Observe output**:
   ```
   [UnitManager] Gacha roll: 23.45 -> Rare (pool size: 4)
   [UnitManager] Drew unit: [Rare] Skilled Marksman (Gold remaining: 95)
   ```

### Probability Distribution Test

1. **In UnitManagerTester component**
2. **Set Probability Test Count**: 10000
3. **Right-click component** → Test 4: Gacha Probability Distribution
4. **Wait ~5 seconds** for test completion
5. **Check results** (should be within ±2% margin):
   ```
   Normal: 5023 (50.23% | Expected: 50%)
   Rare: 2998 (29.98% | Expected: 30%)
   Epic: 1501 (15.01% | Expected: 15%)
   Legendary: 478 (4.78% | Expected: 5%)
   ✅ PASSED: All probabilities within 2% margin
   ```

### Event Testing

1. **Create test script**:
```csharp
using LottoDefense.Units;

public class EventTest : MonoBehaviour
{
    void Start()
    {
        UnitManager.Instance.OnUnitDrawn += OnUnitDrawn;
        UnitManager.Instance.OnDrawFailed += OnDrawFailed;
    }

    void OnUnitDrawn(UnitData unit, int gold)
    {
        Debug.Log($"UI Update: Show {unit.GetDisplayName()}!");
    }

    void OnDrawFailed(string reason)
    {
        Debug.LogWarning($"UI Update: {reason}");
    }
}
```

## Troubleshooting

### "Draw failed: Insufficient gold"
**Solution**:
```csharp
GameplayManager.Instance.SetGold(100);
```

### "Legendary pool is empty!"
**Solution**:
- Verify 4 legendary units created
- Check units assigned to UnitManager Inspector
- Ensure rarity field set to "Legendary"

### "No units available in selected rarity pool"
**Solution**:
- Check all 4 rarity lists have at least 1 unit
- Verify UnitData assets exist in Project
- Try `LoadUnitsFromResources()` if using Resources folder

### Probabilities seem incorrect
**Solution**:
- Run 10,000+ draw test (not just 10-100)
- Small samples show natural variance
- Legendary 5% = expect 1 per 20 draws (average)

## Integration Checklist

- [ ] UnitManager GameObject created
- [ ] 12 UnitData assets created (3 types × 4 rarities)
- [ ] All units assigned to UnitManager Inspector
- [ ] GameplayManager exists in scene
- [ ] Unit pool validation test passes
- [ ] Gacha probability test passes (±2% margin)
- [ ] Gold integration test passes
- [ ] Event notifications working

## Next Steps

After completing setup:

1. **Issue #12**: Implement unit placement on grid
2. **Issue #16**: Add unit synthesis system
3. **Issue #18**: Implement unit upgrade mechanics

## Quick Reference

**Create Unit Data**: Right-click → Create → Lotto Defense → Unit Data
**Test Gacha**: UnitManagerTester → Run All Tests
**Check Status**: UnitManagerTester → Display Manager Status
**Single Draw**: UnitManagerTester → Test Single Draw

---

**Setup Time**: ~30 minutes
**Difficulty**: Beginner-friendly
**Last Updated**: 2025-10-28
