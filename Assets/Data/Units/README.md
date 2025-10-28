# Unit Data Assets

This directory contains UnitData ScriptableObject assets for the game.

## Creating New Units

1. In Unity Editor: Right-click in this folder
2. Select: Create > Lotto Defense > Unit Data
3. Configure the following properties:
   - **Unit Name**: Display name
   - **Unit Type**: Melee, Ranged, or Debuffer
   - **Rarity**: Normal, Rare, Epic, or Legendary
   - **Attack**: Base damage value (recommended: 10-50)
   - **Defense**: Damage reduction (recommended: 0-10)
   - **Attack Range**: Range in cells (Melee: 1, Ranged: 3, Debuffer: 2)
   - **Attack Speed**: Attacks per second (recommended: 0.5-2.0)
   - **Icon**: Sprite for UI display
   - **Prefab**: GameObject prefab for grid placement

## Unit Type Guidelines

### Melee
- Attack Range: 1.0 (adjacent cells only)
- High attack, moderate defense
- Example stats: Attack 30, Defense 5, Speed 1.0

### Ranged
- Attack Range: 3.0 (extended range)
- Moderate attack, low defense
- Example stats: Attack 20, Defense 2, Speed 1.5

### Debuffer
- Attack Range: 2.0 (area effect)
- Low attack, high utility
- Example stats: Attack 15, Defense 3, Speed 0.8

## Rarity Balance

- **Normal (50% drop)**: Attack 10-20, Defense 2-5
- **Rare (30% drop)**: Attack 21-30, Defense 6-8
- **Epic (15% drop)**: Attack 31-40, Defense 9-12
- **Legendary (5% drop)**: Attack 41-50, Defense 13-15

## Testing

Use CombatSystemTester script to test units:
1. Assign unit data to tester
2. Click "Spawn Test Units" in inspector
3. Start combat to verify behavior
