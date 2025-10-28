# Unit Data Creation Guide

This guide explains how to create the 12 required UnitData ScriptableObject assets.

## Required Units (12 Total)

### Melee Units (4)
1. **Normal Melee** - Basic warrior
2. **Rare Melee** - Veteran knight
3. **Epic Melee** - Elite guardian
4. **Legendary Melee** - Dragon slayer

### Ranged Units (4)
1. **Normal Ranged** - Recruit archer
2. **Rare Ranged** - Skilled marksman
3. **Epic Ranged** - Master sniper
4. **Legendary Ranged** - Celestial bowman

### Debuffer Units (4)
1. **Normal Debuffer** - Apprentice mage
2. **Rare Debuffer** - Frost wizard
3. **Epic Debuffer** - Shadow sorcerer
4. **Legendary Debuffer** - Archmage

## Creation Steps (Unity Editor)

1. **Navigate to Assets/Data/Units/[Type] folder**
   - Right-click in Project window
   - Select "Create > Lotto Defense > Unit Data"
   - Name the asset appropriately (e.g., "MeleeNormal")

2. **Configure the UnitData asset**
   - Set the Unit Name
   - Set Type (Melee/Ranged/Debuffer)
   - Set Rarity (Normal/Rare/Epic/Legendary)
   - Configure combat stats based on rarity

3. **Repeat for all 12 units**

## Stat Scaling Guidelines

### Attack Values
- Normal: 10-15
- Rare: 20-30
- Epic: 35-50
- Legendary: 60-80

### Defense Values
- Normal: 5-8
- Rare: 10-15
- Epic: 20-30
- Legendary: 35-50

### Attack Range (in grid units)
- Melee: 1.0-1.5
- Ranged: 3.0-5.0
- Debuffer: 2.0-3.5

### Attack Speed (attacks/second)
- Normal: 0.8-1.0
- Rare: 1.0-1.3
- Epic: 1.3-1.6
- Legendary: 1.6-2.0

## Stat Balance by Type

### Melee Type
- **Strengths**: High defense, moderate attack
- **Weaknesses**: Short range
- Stat priority: Defense > Attack > Attack Speed

### Ranged Type
- **Strengths**: Long range, high attack
- **Weaknesses**: Low defense
- Stat priority: Attack > Range > Attack Speed

### Debuffer Type
- **Strengths**: Medium range, support abilities
- **Weaknesses**: Lower direct damage
- Stat priority: Range > Attack Speed > Defense

## Testing Integration

After creating all 12 units:

1. Open UnitManager inspector
2. Assign units to appropriate rarity lists:
   - Normal Units list (4 units)
   - Rare Units list (4 units)
   - Epic Units list (4 units)
   - Legendary Units list (4 units)

3. Run UnitManagerTester to validate gacha probabilities

## Alternative: Resource Loading

You can also place all units in `Assets/Resources/Units/` and call:
```csharp
UnitManager.Instance.LoadUnitsFromResources();
```

This will automatically categorize units by their rarity field.
