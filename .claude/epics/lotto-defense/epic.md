---
name: lotto-defense
status: backlog
created: 2025-10-28T02:09:08Z
progress: 0%
prd: .claude/prds/lotto-defense.md
github: [Will be updated when synced to GitHub]
---

# Epic: Lotto Defense Game Implementation

## Overview

Implement a solo mobile tower defense game with gacha mechanics, combining strategic grid-based unit placement with randomized unit draws. The game features 30 progressive rounds alternating between 30-second preparation phases and 60-second combat phases. Players manage limited gold resources to draw random units (Normal/Rare/Epic/Legendary), place them on a fixed grid, synthesize combinations, and defend against waves of 20 monsters per round spawning from top and bottom paths.

**Technical Approach:** Leverage Unity 2D engine with existing scene navigation infrastructure. Implement state machine pattern for round management, observer pattern for UI updates, and object pooling for monster spawning to maintain 60 FPS performance.

## Architecture Decisions

### Core Design Patterns

1. **State Machine for Game Flow**
   - **Rationale:** Clean separation of Countdown → Preparation → Combat → Result states
   - **Implementation:** GameplayManager with enum-based state switching
   - **Benefit:** Easy to extend (e.g., add pause state later), predictable transitions

2. **Component-Based Architecture**
   - **Rationale:** Unity's GameObject/Component model fits tower defense perfectly
   - **Components:** GridCell, Unit (base), Monster (base), with specific type implementations
   - **Benefit:** Modular, testable, and supports future unit/monster variety

3. **ScriptableObject for Data**
   - **Rationale:** Designer-friendly, hot-reloadable stats without recompiling
   - **Data:** UnitStats, MonsterStats, SynthesisRecipes, DifficultyScaling
   - **Benefit:** Easy balance tuning, version control friendly

4. **Observer Pattern for UI**
   - **Rationale:** Decouple game logic from presentation
   - **Implementation:** C# events for gold changes, monster deaths, round transitions
   - **Benefit:** UI reactivity without tight coupling, easier testing

5. **Object Pooling for Performance**
   - **Rationale:** 20 monsters spawning per round = 600 instances over 30 rounds
   - **Implementation:** Simple pool for Monster GameObjects
   - **Benefit:** Eliminate GC spikes, maintain 60 FPS

### Technology Stack

- **Engine:** Unity 6000.0.60f1 (2D)
- **Scripting:** C# with namespace organization (`LottoDefense.Gameplay`, `.Units`, `.Monsters`, `.UI`)
- **UI Framework:** uGUI (Canvas + RectTransform)
- **Input:** Unity Input System (existing package)
- **Animation:** Unity Animator for countdown, simple tweening for combat feedback
- **Data Format:** ScriptableObjects (stats, recipes), JSON serialization (game state)

### Key Technical Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Grid Storage | 2D Array `GridCell[,]` | Fast indexed access, fixed size |
| Unit Placement | Click-to-select + Click-to-place | Simpler than drag-drop, mobile-friendly |
| Monster Pathing | Waypoint-based linear | Sufficient for straight paths, easy to visualize |
| Combat Resolution | Coroutine-based tick system | Unity-native, easy damage over time |
| Gacha System | Weighted random with seed | Reproducible for testing, balanced probabilities |

## Technical Approach

### Frontend Components (Unity GameObjects)

#### Core Managers (Single Instances)
1. **GameplayManager** (`LottoDefense.Gameplay`)
   - State machine: `enum GameState { Countdown, Preparation, Combat, RoundResult, Victory, Defeat }`
   - Round tracking, timer management
   - Victory/defeat condition checking
   - Coordinates other managers

2. **GridManager** (`LottoDefense.Grid`)
   - Instantiate grid cells (e.g., 6x10 for portrait)
   - Handle cell selection, unit placement validation
   - Provide grid-to-world position conversion

3. **UnitManager** (`LottoDefense.Units`)
   - Gacha system: Weighted random unit generation
   - Unit inventory management (available slots)
   - Unit placement/swap logic
   - Upgrade and synthesis coordination

4. **MonsterManager** (`LottoDefense.Monsters`)
   - Spawn scheduling (2 monsters/sec for 10 seconds)
   - Path management (top path, bottom path)
   - Monster pooling
   - Difficulty scaling (HP/Defense per round)

5. **CombatManager** (`LottoDefense.Combat`)
   - Target acquisition for units
   - Damage calculation (attack - defense)
   - Gold reward on monster death
   - Remaining monster tracking

6. **UIManager** (`LottoDefense.UI`)
   - HUD updates (round, timer, gold, life, monsters, unit slots)
   - Button state management (draw button enable/disable)
   - Synthesis/upgrade panel display
   - Countdown animation control

#### Game Entities

**Units** (Prefabs with ScriptableObject data)
- Base: `Unit.cs` (attack, attackSpeed, range, rarity, level)
- Types: `MeleeUnit.cs`, `RangedUnit.cs`, `DebufferUnit.cs`
- Visual: SpriteRenderer + Animator (attack animation)

**Monsters** (Pooled prefabs with ScriptableObject data)
- Base: `Monster.cs` (HP, defense, speed, goldReward)
- Visual: SpriteRenderer + health bar
- Movement: Waypoint follower along path

**Grid Cells**
- `GridCell.cs`: Occupancy state, unit reference, visual highlight

### Backend Services (Not Applicable - Single Player)

No backend services required. All game logic runs client-side in Unity. Future considerations:
- Cloud save via PlayerPrefs or Unity Gaming Services (out of scope v1)
- Analytics events (optional, post-launch)

### Data Models

#### Core Data Structures

```csharp
// ScriptableObjects (Designer-editable)
[CreateAssetMenu] public class UnitData : ScriptableObject {
    public string unitName;
    public UnitType type; // Melee, Ranged, Debuffer
    public Rarity rarity; // Normal, Rare, Epic, Legendary
    public int baseAttack, baseHP, attackSpeed;
    public float range;
    public Sprite sprite;
}

[CreateAssetMenu] public class MonsterData : ScriptableObject {
    public string monsterName;
    public int baseHP, baseDefense;
    public float moveSpeed;
    public int goldReward;
    public Sprite sprite;
}

[CreateAssetMenu] public class SynthesisRecipe : ScriptableObject {
    public UnitData[] ingredients; // e.g., [MeleeNormal, MeleeNormal]
    public UnitData result;         // e.g., MeleeRare
}

[CreateAssetMenu] public class DifficultyConfig : ScriptableObject {
    public float[] hpMultiplierPerRound;   // [1.0, 1.1, 1.2, ...]
    public float[] defenseMultiplierPerRound;
}

// Runtime State (C# Classes)
public class GameState {
    public int currentRound;
    public int life;
    public int gold;
    public List<Unit> placedUnits;
    public List<Unit> availableUnits;
}

public class Unit {
    public UnitData data;
    public int currentLevel;
    public int currentAttack; // Upgradeable
    public GridCell occupiedCell;
}

public class Monster {
    public MonsterData data;
    public int currentHP;
    public int currentDefense;
    public Path assignedPath; // Top or Bottom
}
```

### Infrastructure

#### Deployment
- **Platform:** iOS and Android (Unity Build)
- **Build:** IL2CPP for performance on mobile
- **Testing:** Unity Editor + Android/iOS test builds

#### Performance Optimization
- **Object Pooling:** Monster and projectile pools
- **Sprite Atlases:** Batch UI and unit sprites
- **LOD:** Single sprite per unit (no need for LOD in 2D)
- **Physics:** Use 2D Trigger checks only for range detection (no rigidbodies)

#### Monitoring (Optional)
- **Unity Profiler:** Frame time, GC alloc tracking during development
- **Debug Overlay:** Show FPS, active monsters, pool usage (dev builds only)

## Implementation Strategy

### Development Phases

**Phase 1: Core Framework (Days 1-3)**
- Setup GameScene hierarchy (Managers, Grid, UI)
- Implement GameplayManager state machine
- Create countdown animation (3-2-1)
- Build basic HUD layout (all 6 stat displays)

**Phase 2: Grid & Unit Foundation (Days 4-6)**
- GridManager: Generate grid cells, handle clicks
- Create UnitData ScriptableObjects (3 types × 4 rarities = 12 units)
- Implement gacha system with weighted random
- Unit placement and swapping logic

**Phase 3: Monster & Combat (Days 7-10)**
- MonsterManager: Spawning, path following, pooling
- Create MonsterData ScriptableObjects (3-5 types)
- CombatManager: Target acquisition, damage calculation
- Gold rewards on death

**Phase 4: Advanced Systems (Days 11-14)**
- Synthesis system: Recipe validation, unit transformation
- Upgrade system: Cost calculation, stat modification
- Round progression: Difficulty scaling, win/loss checks
- Victory/defeat screens

**Phase 5: Polish & Balance (Days 15-17)**
- Animations: Unit attacks, monster deaths, UI feedback
- Balance tuning: Gold economy, difficulty curve
- Bug fixes and optimization
- Mobile build testing

### Risk Mitigation

| Risk | Impact | Mitigation |
|------|--------|------------|
| Performance drops with 20 monsters | High | Object pooling, profiling early |
| Gacha balance (too easy/hard) | Medium | ScriptableObject tuning, playtesting |
| Grid size unclear for portrait | Medium | Prototype with 6x10, adjust based on device test |
| Synthesis complexity confusing | Low | Simple recipes first, tooltips later |

### Testing Approach

1. **Unit Tests** (C# Test Framework)
   - Gacha weighted probabilities
   - Damage calculation formulas
   - Gold economy math

2. **Integration Tests**
   - Round state transitions
   - Monster spawning timing
   - Unit placement validation

3. **Playtesting**
   - Full 30-round runs
   - Balance validation (can afford units, life buffer)
   - Edge cases (0 gold, grid full, no monsters killed)

## Task Breakdown Preview

### High-Level Task Categories (10 Tasks Max)

- [ ] **Task 1: Game State Machine & Countdown**
  - GameplayManager with state enum
  - Countdown animation (3-2-1)
  - State transition to Preparation
  - **Estimated:** 1 day

- [ ] **Task 2: Grid System & UI Layout**
  - GridManager: Generate cells, handle clicks
  - HUD layout (6 stats: round, time, monsters, gold, units, life)
  - Basic styling and positioning
  - **Estimated:** 1 day

- [ ] **Task 3: Unit Data & Gacha System**
  - Create 12 UnitData ScriptableObjects (3 types × 4 rarities)
  - Weighted random gacha (5% Legendary, 15% Epic, 30% Rare, 50% Normal)
  - Unit draw button with gold cost check
  - **Estimated:** 1 day

- [ ] **Task 4: Unit Placement & Swap**
  - Click-to-select unit logic
  - Click-to-place/swap on grid
  - Visual feedback (highlight, selection indicator)
  - **Estimated:** 1 day

- [ ] **Task 5: Monster Spawning & Movement**
  - MonsterManager: Spawn 2/sec from top/bottom
  - Waypoint-based movement along straight paths
  - Object pooling for 20 monsters
  - Create 3-5 MonsterData ScriptableObjects
  - **Estimated:** 2 days

- [ ] **Task 6: Combat System**
  - CombatManager: Unit target acquisition in range
  - Damage calculation (attack - defense)
  - Monster death: Award gold, decrease counter
  - Auto-combat coroutine tick
  - **Estimated:** 2 days

- [ ] **Task 7: Round Management & Difficulty**
  - Preparation timer (30s) → Combat timer (60s)
  - Round completion logic (success/life loss)
  - Difficulty scaling ScriptableObject (HP/Defense per round)
  - Victory (round 30) / Defeat (life 0) screens
  - **Estimated:** 2 days

- [ ] **Task 8: Unit Synthesis System**
  - SynthesisRecipe ScriptableObjects (define 5-10 recipes)
  - Synthesis UI panel (drag-to-add, validate, transform)
  - Consumed units removed, new unit spawned
  - **Estimated:** 2 days

- [ ] **Task 9: Unit Upgrade System**
  - Upgrade UI panel (show stats, cost, confirm)
  - Upgrade cost formula (e.g., 10 * level^1.5)
  - Modify unit attack stat, save level
  - **Estimated:** 1 day

- [ ] **Task 10: Polish & Balance**
  - Attack animations (simple fade/scale tweens)
  - UI feedback (gold gain pop-up, damage numbers)
  - Balance tuning: Gold rewards, upgrade costs, difficulty curve
  - Mobile build testing (portrait orientation, touch targets)
  - Bug fixes from playtesting
  - **Estimated:** 3 days

**Total Estimated Effort:** 17 days (~3.5 weeks with buffer)

## Dependencies

### External Dependencies
- ✅ Unity Engine 6000.0.60f1 (installed)
- ✅ Unity Input System package (installed)
- ✅ Unity 2D Sprite package (installed)
- ⚠️ **Placeholder sprites required:** Units (12), Monsters (3-5), Grid tiles, UI elements
- ⚠️ **Animator setup needed:** Countdown animation, attack effects

### Internal Dependencies
- ✅ **LoginScene** (complete)
- ✅ **MainGame menu scene** (complete)
- ✅ **SceneNavigator** (complete)
- ✅ **GameScene** (placeholder exists, needs full implementation)
- ⚠️ **Build Settings:** GameScene must be in build (already added)

### Prerequisite Work
1. **Asset Creation:** Placeholder sprites for units/monsters (can use colored squares)
2. **Grid Dimensions:** Finalize grid size (recommend 6x10 for portrait)
3. **Balance Spreadsheet:** Define exact stats for 12 units, 5 monsters, gold rewards, upgrade costs

## Success Criteria (Technical)

### Performance Benchmarks
- ✅ Maintain 60 FPS during combat with 20 active monsters and 10 units
- ✅ Scene load time < 2 seconds
- ✅ Memory usage < 200MB on mid-range mobile devices
- ✅ No GC spikes > 10ms during combat

### Quality Gates
- ✅ Zero critical bugs preventing round completion
- ✅ All unit types (Melee, Ranged, Debuffer) functional
- ✅ Monster spawning timing accurate (2/sec for 10 seconds)
- ✅ Gold economy allows unit draws throughout 30 rounds
- ✅ Difficulty scaling makes round 30 significantly harder than round 1

### Acceptance Criteria
1. **Countdown Animation:** Smooth 3-2-1 countdown with visual feedback
2. **Preparation Phase:** Can draw units (5g), place on grid, synthesize, upgrade within 30s timer
3. **Combat Phase:** Units auto-attack monsters in range, gold awarded on death, timer counts down
4. **Round Progression:** Successfully transition through 30 rounds OR trigger defeat at life 0
5. **Victory Screen:** Display "Victory" when completing round 30
6. **Defeat Screen:** Display "Game Over" when life reaches 0, return to MainGame

### Code Quality
- Namespace organization: `LottoDefense.Gameplay`, `.Units`, `.Monsters`, `.UI`, `.Data`
- XML documentation on public methods
- No hardcoded magic numbers (use const or ScriptableObject)
- Consistent naming: PascalCase classes, camelCase variables, _privateFields

## Estimated Effort

### Overall Timeline
- **Implementation:** 17 days (3.5 weeks)
- **Testing & Polish:** 3 days (included in Task 10)
- **Total:** ~4 weeks for MVP

### Resource Requirements
- **Developer:** 1 Unity developer (full-time)
- **Designer:** 1 game designer (part-time for balance tuning)
- **Artist:** 1 2D artist (part-time for placeholder sprites → final art)

### Critical Path Items
1. **State Machine (Task 1):** Blocks all other tasks
2. **Grid System (Task 2):** Blocks unit placement, combat
3. **Unit Gacha (Task 3):** Blocks placement, synthesis, upgrade
4. **Monster Spawning (Task 5):** Blocks combat system
5. **Combat System (Task 6):** Core gameplay loop
6. **Round Management (Task 7):** Victory/defeat conditions

**Parallel Work Opportunities:**
- Unit Data (Task 3) can happen while Grid (Task 2) is being built
- Synthesis (Task 8) and Upgrade (Task 9) can be developed independently after Tasks 1-6

## Open Technical Questions

1. **Grid Dimensions:** Recommend 6 columns × 10 rows for portrait. Need device testing to confirm readability.
2. **Monster Path Waypoints:** Define exact waypoint positions once grid is finalized.
3. **Gacha Seed:** Use Unity's Random.InitState for reproducible testing?
4. **Gold Reward Formula:** Start with 2-5 gold per monster, scale with round?
5. **Upgrade Cost Formula:** Suggest `10 * level^1.5` capped at level 10.
6. **Synthesis Recipe Count:** 5-10 recipes reasonable for MVP? More can be data-driven later.
7. **Debuffer Mechanics:** Define exact debuff effects (slow? armor reduction?).
8. **Art Style:** Placeholder colored shapes OK for prototype?

---

**Created:** 2025-10-28T02:09:08Z
**Status:** Ready for Task Decomposition
**Next Step:** Run `/pm:epic-decompose lotto-defense` to create GitHub issues
