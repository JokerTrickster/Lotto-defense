# Synthesis Recipe Templates

This document provides detailed templates for creating the 10 recommended synthesis recipes.

## Prerequisites

Ensure all 12 UnitData assets exist in `Assets/Data/Units/`:

### Melee Units
- `RecruitWarrior.asset` (Normal) - 12 ATK, 8 DEF, 1.2 Range, 0.9 Speed
- `VeteranKnight.asset` (Rare) - 25 ATK, 15 DEF, 1.3 Range, 1.1 Speed
- `EliteGuardian.asset` (Epic) - 42 ATK, 28 DEF, 1.4 Range, 1.4 Speed
- `DragonSlayer.asset` (Legendary) - 70 ATK, 45 DEF, 1.5 Range, 1.8 Speed

### Ranged Units
- `RecruitArcher.asset` (Normal) - 15 ATK, 5 DEF, 3.5 Range, 1.0 Speed
- `SkilledMarksman.asset` (Rare) - 28 ATK, 10 DEF, 4.0 Range, 1.2 Speed
- `MasterSniper.asset` (Epic) - 48 ATK, 18 DEF, 4.5 Range, 1.5 Speed
- `CelestialBowman.asset` (Legendary) - 75 ATK, 30 DEF, 5.0 Range, 1.9 Speed

### Debuffer Units
- `ApprenticeMage.asset` (Normal) - 8 ATK, 6 DEF, 2.5 Range, 0.8 Speed
- `FrostWizard.asset` (Rare) - 18 ATK, 12 DEF, 3.0 Range, 1.0 Speed
- `ShadowSorcerer.asset` (Epic) - 32 ATK, 22 DEF, 3.5 Range, 1.3 Speed
- `Archmage.asset` (Legendary) - 55 ATK, 38 DEF, 4.0 Range, 1.7 Speed

## Recipe Templates

### Tier 1: Basic Upgrades (Normal → Rare)

#### Recipe 1: Warrior Training
**File**: `WarriorTraining.asset`
```
Recipe Name: Warrior Training
Starts Discovered: true

Ingredients (3 total):
  Slot 1: Recruit Warrior (Normal) - Quantity: 3

Result Unit: Veteran Knight (Rare)

Description:
"Three recruit warriors train intensively together to become a veteran knight with superior combat skills."
```

**Purpose**: Basic melee upgrade path
**Economy**: Converts 3 common units → 1 uncommon
**Strategic Value**: Core upgrade for melee-focused strategies

---

#### Recipe 2: Archery Academy
**File**: `ArcheryAcademy.asset`
```
Recipe Name: Archery Academy
Starts Discovered: true

Ingredients (3 total):
  Slot 1: Recruit Archer (Normal) - Quantity: 3

Result Unit: Skilled Marksman (Rare)

Description:
"Three novice archers complete advanced training to become skilled marksmen with deadly accuracy."
```

**Purpose**: Basic ranged upgrade path
**Economy**: Converts 3 common units → 1 uncommon
**Strategic Value**: Essential for ranged defense strategies

---

#### Recipe 3: Magical Studies
**File**: `MagicalStudies.asset`
```
Recipe Name: Magical Studies
Starts Discovered: true

Ingredients (5 total):
  Slot 1: Apprentice Mage (Normal) - Quantity: 5

Result Unit: Frost Wizard (Rare)

Description:
"Five apprentice mages pool their knowledge to master frost magic and become a frost wizard."
```

**Purpose**: Basic debuffer upgrade path
**Economy**: Converts 5 common units → 1 uncommon (higher cost due to support role)
**Strategic Value**: Unlock debuff capabilities

---

#### Recipe 4: Mixed Tactics
**File**: `MixedTactics.asset`
```
Recipe Name: Mixed Tactics
Starts Discovered: true

Ingredients (4 total):
  Slot 1: Recruit Warrior (Normal) - Quantity: 2
  Slot 2: Recruit Archer (Normal) - Quantity: 1
  Slot 3: Apprentice Mage (Normal) - Quantity: 1

Result Unit: Veteran Knight (Rare)

Description:
"Warriors learn from archers and mages, combining melee prowess with tactical knowledge."
```

**Purpose**: Alternative upgrade using mixed units
**Economy**: Converts 4 mixed common units → 1 uncommon
**Strategic Value**: Flexibility for varied inventories

---

### Tier 2: Advanced Combinations (Rare → Epic)

#### Recipe 5: Elite Guardian Forge
**File**: `EliteGuardianForge.asset`
```
Recipe Name: Elite Guardian Forge
Starts Discovered: false

Ingredients (4 total):
  Slot 1: Veteran Knight (Rare) - Quantity: 2
  Slot 2: Frost Wizard (Rare) - Quantity: 1
  Slot 3: Skilled Marksman (Rare) - Quantity: 1

Result Unit: Elite Guardian (Epic)

Description:
"Knights trained by wizards and archers become elite guardians with enhanced combat awareness and magical fortification."
```

**Purpose**: Premium melee unit creation
**Economy**: Converts 4 rare units → 1 epic
**Strategic Value**: High-investment defensive powerhouse
**Discovery**: Unlocks on first successful synthesis

---

#### Recipe 6: Sniper Mastery
**File**: `SniperMastery.asset`
```
Recipe Name: Sniper Mastery
Starts Discovered: false

Ingredients (3 total):
  Slot 1: Skilled Marksman (Rare) - Quantity: 3

Result Unit: Master Sniper (Epic)

Description:
"Three skilled marksmen combine their expertise to achieve the pinnacle of precision shooting."
```

**Purpose**: Pure ranged upgrade path
**Economy**: Converts 3 rare units → 1 epic
**Strategic Value**: Maximize ranged damage output
**Discovery**: Unlocks after creating 2+ rare units

---

#### Recipe 7: Shadow Ritual
**File**: `ShadowRitual.asset`
```
Recipe Name: Shadow Ritual
Starts Discovered: false

Ingredients (4 total):
  Slot 1: Frost Wizard (Rare) - Quantity: 2
  Slot 2: Veteran Knight (Rare) - Quantity: 1
  Slot 3: Skilled Marksman (Rare) - Quantity: 1

Result Unit: Shadow Sorcerer (Epic)

Description:
"Wizards channel the strength of warriors and precision of archers into devastating shadow magic."
```

**Purpose**: Premium debuffer creation
**Economy**: Converts 4 mixed rare units → 1 epic debuffer
**Strategic Value**: Powerful crowd control specialist
**Discovery**: Unlocks through experimentation

---

### Tier 3: Legendary Ascensions (Epic → Legendary)

#### Recipe 8: Dragon Slayer Ascension
**File**: `DragonSlayerAscension.asset`
```
Recipe Name: Dragon Slayer Ascension
Starts Discovered: false

Ingredients (3 total):
  Slot 1: Elite Guardian (Epic) - Quantity: 1
  Slot 2: Master Sniper (Epic) - Quantity: 1
  Slot 3: Shadow Sorcerer (Epic) - Quantity: 1

Result Unit: Dragon Slayer (Legendary)

Description:
"The ultimate warrior emerges when guardian, sniper, and sorcerer unite their legendary powers to become the Dragon Slayer."
```

**Purpose**: Ultimate melee unit
**Economy**: Converts 3 epic units (mixed) → 1 legendary
**Strategic Value**: Game-changing endgame unit
**Discovery**: Only unlocks after possessing 3+ epic units
**Balance Note**: Requires massive investment (roughly 9-12 rare units worth)

---

#### Recipe 9: Celestial Ascension
**File**: `CelestialAscension.asset`
```
Recipe Name: Celestial Ascension
Starts Discovered: false

Ingredients (4 total):
  Slot 1: Master Sniper (Epic) - Quantity: 2
  Slot 2: Shadow Sorcerer (Epic) - Quantity: 1
  Slot 3: Elite Guardian (Epic) - Quantity: 1

Result Unit: Celestial Bowman (Legendary)

Description:
"Divine blessing descends upon the greatest marksmen, granting celestial arrows that never miss and pierce all defenses."
```

**Purpose**: Ultimate ranged unit
**Economy**: Converts 4 epic units → 1 legendary
**Strategic Value**: Unmatched ranged damage and range
**Discovery**: Unlocks after synthesizing 5+ epic units
**Balance Note**: Sniper-focused requiring 2x Master Snipers

---

#### Recipe 10: Arcane Apotheosis
**File**: `ArcaneApotheosis.asset`
```
Recipe Name: Arcane Apotheosis
Starts Discovered: false

Ingredients (5 total):
  Slot 1: Shadow Sorcerer (Epic) - Quantity: 3
  Slot 2: Elite Guardian (Epic) - Quantity: 1
  Slot 3: Master Sniper (Epic) - Quantity: 1

Result Unit: Archmage (Legendary)

Description:
"The supreme master of magic ascends, commanding reality itself through the combined wisdom of three shadow sorcerers."
```

**Purpose**: Ultimate support/debuffer unit
**Economy**: Converts 5 epic units → 1 legendary
**Strategic Value**: Army-wide debuffs and control
**Discovery**: Final recipe, unlocks after creating other legendary units
**Balance Note**: Highest cost (5 epic units) for support role power

---

## Creation Workflow

### In Unity Editor

1. **Open Project**: `epic-lotto-defense`

2. **Navigate**: `Assets/Data/Recipes/`

3. **Create Recipe**:
   - Right-click in folder
   - Create > Lotto Defense > Synthesis Recipe
   - Name file (e.g., `WarriorTraining.asset`)

4. **Configure Recipe**:
   ```
   Recipe Name: [From template]
   Starts Discovered: [true/false]

   Ingredients (Size: [count]):
     Element 0:
       Unit Data: [Drag UnitData asset]
       Quantity: [number]
     Element 1: ... (if needed)

   Result Unit: [Drag UnitData asset]

   Recipe Icon: [Optional sprite]
   Recipe Description: [From template]
   ```

5. **Verify**:
   - Check `GetTotalIngredientCount()` in inspector
   - Ensure result unit is not null
   - Validate ingredient references are correct

6. **Test**:
   - Add to SynthesisTester.testRecipes array
   - Play mode > Run validation tests

### Batch Creation Script

For faster creation, use this Editor script:

```csharp
// Place in: Assets/Editor/RecipeCreator.cs

using UnityEngine;
using UnityEditor;
using LottoDefense.Units;

public class RecipeCreator : EditorWindow
{
    [MenuItem("Tools/Create All Recipes")]
    static void CreateAllRecipes()
    {
        // Load all unit assets first
        var units = LoadAllUnits();

        // Create each recipe
        CreateWarriorTraining(units);
        CreateArcheryAcademy(units);
        // ... etc

        AssetDatabase.SaveAssets();
        Debug.Log("All recipes created!");
    }

    static UnitData[] LoadAllUnits()
    {
        return Resources.LoadAll<UnitData>("Units");
    }

    static void CreateWarriorTraining(UnitData[] units)
    {
        var recipe = ScriptableObject.CreateInstance<SynthesisRecipe>();
        // Configure recipe...
        AssetDatabase.CreateAsset(recipe, "Assets/Data/Recipes/WarriorTraining.asset");
    }
}
```

## Balance Considerations

### Conversion Ratios
- **Normal → Rare**: 3:1 standard, 4:1 mixed, 5:1 for support
- **Rare → Epic**: 3:1 pure, 4:1 mixed
- **Epic → Legendary**: 3:1 best, 4-5:1 for premium

### Economy Math
```
To create 1 Dragon Slayer (Legendary):
  3 Epic units required

  Each Epic from Rares:
    Elite Guardian: 4 Rares
    Master Sniper: 3 Rares
    Shadow Sorcerer: 4 Rares
    Total: 11 Rares

  Each Rare from Normals:
    ~3.5 Normals average

  Total investment: ~38-40 Normal units → 1 Legendary
```

### Discovery Progression
1. **Always Available**: Basic Normal → Rare recipes (1-4)
2. **Early Unlock**: Rare → Epic recipes (5-7) after creating rares
3. **Late Game**: Legendary recipes (8-10) after extensive epic creation

## Testing Checklist

For each recipe:
- [ ] File created with correct name
- [ ] Recipe name is descriptive
- [ ] All ingredient slots filled with correct UnitData
- [ ] Quantities are accurate
- [ ] Result unit is correct rarity tier
- [ ] Description is flavorful and clear
- [ ] Discovery state matches tier
- [ ] Recipe validates with correct ingredients
- [ ] Recipe rejects incorrect ingredients
- [ ] Synthesis executes successfully
- [ ] Ingredients removed from inventory
- [ ] Result added to inventory
- [ ] Icon assigned (if using custom sprites)

## Common Mistakes

1. **Wrong unit references**: Double-check ingredient UnitData matches intended unit
2. **Quantity mismatch**: Verify total ingredients matches template
3. **Missing result**: Always assign result unit
4. **Wrong rarity tier**: Result should be higher rarity than ingredients
5. **Discovery state**: Legendary recipes should start undiscovered
6. **Circular dependencies**: Don't create recipes that require their own result

## Integration with UnitManager

Once recipes are created:

1. **Assign to SynthesisManager**:
   - Inspector: Drag recipes to `All Recipes` array
   - OR place in `Resources/Recipes/` folder

2. **Verify in Play Mode**:
   ```
   SynthesisManager.Instance.LogAllRecipes();
   ```

3. **Test Discovery**:
   ```
   // All recipes should be loaded
   Debug.Log(SynthesisManager.Instance.GetSystemSummary());
   ```

## Future Recipe Ideas

Additional recipes for expansions:
- Cross-type fusions (2 different types → hybrid)
- Sacrifice recipes (4 units → 1 same-tier with bonus stats)
- Specialty recipes (specific combinations → unique units)
- Event recipes (limited-time seasonal units)
- Prestige recipes (legendary → enhanced legendary)
