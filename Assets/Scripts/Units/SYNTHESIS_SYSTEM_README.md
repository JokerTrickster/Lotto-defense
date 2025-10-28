# Unit Synthesis System

## Overview
The synthesis system allows players to combine multiple units into higher-tier units through predefined recipes during the Preparation phase.

## Core Components

### 1. SynthesisRecipe (ScriptableObject)
Defines a single synthesis recipe with ingredients and result.

**Location**: `Assets/Scripts/Units/SynthesisRecipe.cs`

**Key Features**:
- Ingredient requirements (unit type + quantity)
- Result unit specification
- Recipe discovery system support
- Validation logic for ingredient matching

**Usage**:
```csharp
// Create via: Assets > Create > Lotto Defense > Synthesis Recipe
SynthesisRecipe recipe = ...; // From inspector
List<UnitData> selectedUnits = GetPlayerSelection();
bool isValid = recipe.ValidateIngredients(selectedUnits);
```

### 2. SynthesisManager (Singleton)
Manages recipe execution, validation, and discovery state.

**Location**: `Assets/Scripts/Units/SynthesisManager.cs`

**Key Responsibilities**:
- Recipe validation (< 16ms performance requirement)
- Unit transformation (remove ingredients, add result)
- Discovery state persistence
- Phase validation (Preparation only)
- Visual effects coordination

**Usage**:
```csharp
// Execute synthesis
bool success = SynthesisManager.Instance.TrySynthesize(
    selectedUnits,
    recipe,
    synthesisPosition // optional
);

// Query valid recipes
List<SynthesisRecipe> valid = SynthesisManager.Instance.GetValidRecipes(selectedUnits);

// Discovery management
bool discovered = SynthesisManager.Instance.IsRecipeDiscovered(recipe);
SynthesisManager.Instance.DiscoverRecipe(recipe);
```

### 3. SynthesisPanel (UI)
Manages the synthesis UI panel for recipe selection and execution.

**Location**: `Assets/Scripts/UI/SynthesisPanel.cs`

**Features**:
- Recipe list display (discovered only)
- Unit selection interface
- Real-time validation feedback
- Auto-selection for recipes
- Success/error animations

**Usage**:
```csharp
// Show/hide panel
SynthesisPanel panel = FindObjectOfType<SynthesisPanel>();
panel.Show();
panel.Hide();

// Manual unit selection
panel.AddSelectedUnit(unitData);
panel.RemoveSelectedUnit(unitData);
panel.ClearSelection();
```

## Recipe Creation Guide

### Step 1: Create UnitData Assets
Before creating recipes, ensure all required UnitData assets exist:

```
Assets/Data/Units/
├── Melee/
│   ├── RecruitWarrior.asset     (Normal)
│   ├── VeteranKnight.asset       (Rare)
│   ├── EliteGuardian.asset       (Epic)
│   └── DragonSlayer.asset        (Legendary)
├── Ranged/
│   ├── RecruitArcher.asset       (Normal)
│   ├── SkilledMarksman.asset     (Rare)
│   ├── MasterSniper.asset        (Epic)
│   └── CelestialBowman.asset     (Legendary)
└── Debuffer/
    ├── ApprenticeMage.asset      (Normal)
    ├── FrostWizard.asset         (Rare)
    ├── ShadowSorcerer.asset      (Epic)
    └── Archmage.asset            (Legendary)
```

### Step 2: Create Recipe Assets

**Location**: `Assets/Data/Recipes/`

**Menu**: `Assets > Create > Lotto Defense > Synthesis Recipe`

#### Example Recipes

##### 1. Basic Archer (3 Normal → 1 Rare)
```
Recipe Name: Basic Archer Upgrade
Ingredients:
  - Recruit Warrior (Normal) x3
Result: Veteran Knight (Rare)
Description: "Three recruits train together to become a veteran knight"
Starts Discovered: true
```

##### 2. Elite Guardian (2 Rare + 1 Epic → 1 Epic)
```
Recipe Name: Elite Guardian Forge
Ingredients:
  - Veteran Knight (Rare) x2
  - Skilled Marksman (Rare) x1
Result: Elite Guardian (Epic)
Description: "Combined training creates an elite guardian"
Starts Discovered: false
```

##### 3. Dragon Slayer (3 Epic → 1 Legendary)
```
Recipe Name: Dragon Slayer Ascension
Ingredients:
  - Elite Guardian (Epic) x1
  - Master Sniper (Epic) x1
  - Shadow Sorcerer (Epic) x1
Result: Dragon Slayer (Legendary)
Description: "Three epic heroes unite to become a legendary dragon slayer"
Starts Discovered: false
```

### Recommended Recipe Progression

**Tier 1: Normal → Rare** (Easy recipes, always discovered)
1. 3x Normal Melee → 1x Rare Melee
2. 3x Normal Ranged → 1x Rare Ranged
3. 3x Normal Debuffer → 1x Rare Debuffer
4. 2x Normal Melee + 1x Normal Ranged → 1x Rare Melee

**Tier 2: Rare → Epic** (Medium recipes, discovered on first use)
5. 2x Rare Melee + 1x Rare Debuffer → 1x Epic Melee
6. 2x Rare Ranged + 1x Rare Melee → 1x Epic Ranged
7. 3x Rare Debuffer → 1x Epic Debuffer

**Tier 3: Epic → Legendary** (Hard recipes, hidden until discovered)
8. 3x Epic (any type) → 1x Legendary Dragon Slayer
9. 2x Epic Ranged + 1x Epic Debuffer → 1x Legendary Celestial Bowman
10. 2x Epic Debuffer + 1x Epic Melee → 1x Legendary Archmage

## Unity Setup

### Scene Setup

1. **Create SynthesisManager**:
   - Create empty GameObject: `SynthesisManager`
   - Add `SynthesisManager` component
   - Assign recipes in inspector OR place recipes in `Resources/Recipes/`
   - Optional: Assign synthesis effect prefab

2. **Create Synthesis UI Panel**:
   - Create Canvas > Panel: `SynthesisPanel`
   - Add `SynthesisPanel` component
   - Setup UI elements:
     - Recipe list container (vertical layout)
     - Selected units container (horizontal layout)
     - Synthesize button
     - Status text (TextMeshPro)
     - Close button
   - Assign prefabs:
     - RecipeSlotUI prefab
     - SelectedUnitSlotUI prefab

3. **Connect to Game Flow**:
   ```csharp
   // In your game UI manager
   public Button synthesisButton;
   public SynthesisPanel synthesisPanel;

   void Start() {
       synthesisButton.onClick.AddListener(() => synthesisPanel.Show());
   }
   ```

### Prefab Structure

#### RecipeSlotUI Prefab
```
RecipeSlot (RecipeSlotUI component)
├── Background (Image)
├── RecipeIcon (Image)
├── RecipeName (TextMeshProUGUI)
├── Ingredients (TextMeshProUGUI)
├── Result (TextMeshProUGUI)
├── DiscoveredIndicator (GameObject with Image)
├── NewIndicator (GameObject with Image)
└── SelectButton (Button)
```

#### SelectedUnitSlotUI Prefab
```
SelectedUnitSlot (SelectedUnitSlotUI component)
├── Background (Image)
├── UnitIcon (Image)
├── UnitName (TextMeshProUGUI)
└── RemoveButton (Button)
```

## Event System

### SynthesisManager Events

```csharp
// Recipe discovered
SynthesisManager.Instance.OnRecipeDiscovered += (recipe) => {
    Debug.Log($"New recipe: {recipe.RecipeName}");
    ShowNotification($"Recipe Discovered: {recipe.RecipeName}!");
};

// Synthesis complete
SynthesisManager.Instance.OnSynthesisComplete += (resultUnit, position) => {
    Debug.Log($"Created: {resultUnit.GetDisplayName()}");
    PlaySuccessAnimation(position);
};

// Synthesis failed
SynthesisManager.Instance.OnSynthesisFailed += (reason) => {
    Debug.LogWarning($"Synthesis failed: {reason}");
    ShowErrorMessage(reason);
};
```

### SynthesisPanel Events

```csharp
// Units selected/deselected
synthesisPanel.OnUnitsSelected += (units) => {
    Debug.Log($"Selected {units.Count} units");
};
```

## Testing

### Using SynthesisTester

1. **Add to scene**:
   - Create GameObject: `SynthesisTester`
   - Add `SynthesisTester` component

2. **Assign test recipes** in inspector

3. **Run tests**:
   - Play mode: Click buttons in GUI overlay
   - Context menu: Right-click component > Run All Tests
   - Inspector: Check "Run Tests On Start"

### Test Methods

```csharp
SynthesisTester tester = FindObjectOfType<SynthesisTester>();

// Run all tests
tester.RunAllTests();

// Individual tests
tester.TestManagerInitialization();
tester.TestRecipeValidation();
tester.TestRecipeDiscovery();
tester.TestSynthesisExecution();

// Helper utilities
tester.ClearTestInventory();
tester.ResetDiscoveryState();
tester.AddTestGold();
tester.DrawRandomUnits();
```

### Manual Testing Checklist

- [ ] All recipes validate correctly with proper ingredients
- [ ] Invalid combinations are rejected
- [ ] Synthesis removes ingredients from inventory
- [ ] Synthesis adds result unit to inventory
- [ ] Only works during Preparation phase
- [ ] Discovery state persists across sessions
- [ ] UI updates properly on synthesis
- [ ] Visual effects play correctly
- [ ] Performance < 16ms for validation/execution

## Performance Optimization

### Validation Performance
The system is designed to validate recipes in < 16ms:

- Recipe validation uses LINQ for efficiency
- Validation is only performed when selection changes
- UI updates are batched

### Memory Management
- Recipe assets are loaded once and cached
- UI elements use object pooling (via prefabs)
- Discovery state uses HashSet for O(1) lookup

## Integration Points

### With UnitManager
```csharp
// Synthesis consumes units from inventory
UnitManager.Instance.RemoveUnit(ingredient);

// Adds result to inventory
UnitManager.Instance.AddUnit(resultUnit);

// Checks inventory for available units
var inventory = UnitManager.Instance.GetInventory();
```

### With GameplayManager
```csharp
// Only allow synthesis during Preparation phase
bool canSynthesize = GameplayManager.Instance.CurrentState == GameState.Preparation;
```

### With GridManager
```csharp
// Optional: Spawn synthesis effect at grid position
Vector3 synthesisPos = GridManager.Instance.GridToWorld(gridCell);
SynthesisManager.Instance.TrySynthesize(units, recipe, synthesisPos);
```

## Save/Load System

### Discovery State Persistence

```csharp
// Save (automatic on discovery)
SynthesisManager.Instance.SaveDiscoveryState();

// Load (automatic on initialization)
SynthesisManager.Instance.LoadDiscoveryState();

// Reset (for testing or new game)
SynthesisManager.Instance.ResetDiscoveryState();
```

**Storage**: PlayerPrefs key: `"SynthesisDiscoveryState"`
**Format**: Comma-separated recipe names

## Common Issues & Solutions

### "No recipes found"
- Ensure recipes are in `Assets/Data/Recipes/` or `Resources/Recipes/`
- Check SynthesisManager has recipes assigned in inspector
- Verify recipe assets are properly created

### "Synthesis not working during Combat"
- Synthesis only works in Preparation phase
- Check `GameplayManager.Instance.CurrentState == GameState.Preparation`

### "Recipe validation always fails"
- Verify UnitData references in recipe match actual inventory units
- Check ingredient quantities match selection exactly
- Use SynthesisTester to debug validation

### "UI not updating"
- Ensure SynthesisPanel subscribed to events in OnEnable
- Check prefabs are assigned in SynthesisPanel inspector
- Verify event handlers are not throwing exceptions

## Future Enhancements

Potential additions for future iterations:
- Synthesis gold cost (beyond ingredient cost)
- Synthesis cooldown timer
- Batch synthesis (multiple recipes at once)
- Recipe hints/tooltips
- Synthesis success rate (with chance of failure)
- Pity system for rare recipe results
- Synthesis history log
- Recipe unlock via achievements
- Visual merge animations for units

## Architecture Notes

### Design Decisions

1. **ScriptableObject recipes**: Designer-friendly, no code required
2. **Singleton managers**: Global access, easy integration
3. **Event-driven UI**: Loose coupling, easy to extend
4. **Preparation phase only**: Strategic planning without combat pressure
5. **Discovery system**: Progression incentive, exploration reward

### Code Standards

- All public methods have XML documentation
- Performance-critical code has timing measurements
- Event naming: `On[Action][Result]` (e.g., OnSynthesisComplete)
- Validation methods return bool + error reason via event
- Debug logging for all major operations

### Namespace Organization

```
LottoDefense.Units
├── SynthesisRecipe      (Data)
├── SynthesisManager     (Logic)
└── SynthesisTester      (Testing)

LottoDefense.UI
└── SynthesisPanel       (Presentation)
    ├── RecipeSlotUI
    └── SelectedUnitSlotUI
```
