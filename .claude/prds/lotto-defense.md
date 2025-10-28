---
name: lotto-defense
description: Solo tower defense game with random unit gacha system and 30-round progressive difficulty
status: backlog
created: 2025-10-28T02:05:45Z
---

# PRD: Lotto Defense Game

## Executive Summary

Lotto Defense is a solo tower defense mobile game where players strategically place randomly-drawn units on a grid to defend against waves of monsters over 30 increasingly difficult rounds. The core gameplay loop combines gacha mechanics for unit acquisition, strategic grid-based tower placement, and unit synthesis to create powerful combinations. Players must survive 60-second combat rounds with 30-second preparation phases, managing limited gold resources while defending against 20 monsters per round.

## Problem Statement

### What Problem Are We Solving?

Players want an engaging tower defense experience that combines:
- Strategic depth through unit placement and synthesis
- Exciting randomness through gacha mechanics
- Progressive challenge over extended gameplay sessions
- Quick, mobile-friendly round-based gameplay

### Why Is This Important Now?

- Tower defense genre remains popular but needs fresh mechanics
- Gacha + strategy hybrid appeals to both casual and hardcore mobile gamers
- Current implementation has login and menu screens complete
- Game screen needs core gameplay loop implementation

## User Stories

### Primary Persona: Mobile Strategy Gamer

**Demographics:**
- Age: 18-35
- Platform: Mobile (portrait orientation)
- Session Length: 5-30 minutes
- Skill Level: Casual to intermediate strategy gamers

### User Journey

1. **Game Start**
   - User clicks "게임 시작" from main menu
   - Sees countdown animation (3, 2, 1)
   - Enters first preparation phase

2. **Preparation Phase (30 seconds)**
   - Draws random units using gold (5 gold per draw)
   - Places units on grid by clicking
   - Combines units using synthesis table
   - Upgrades unit attack power
   - Prepares for incoming monster wave

3. **Combat Phase (60 seconds)**
   - Watches units automatically attack monsters
   - Monsters spawn from top and bottom (2 monsters/second, 20 total)
   - Earns gold for each monster killed
   - Monitors remaining monsters, time, life, and gold

4. **Round Transition**
   - If all monsters defeated: Progress to next round
   - If time expires: Lose life equal to remaining monsters
   - If life reaches 0: Game Over (no restart)
   - If survived: Enter next preparation phase

5. **Victory**
   - Successfully complete all 30 rounds
   - Final round cleared = Victory

### Pain Points Being Addressed

- **Boring static tower defense:** Random unit draws add excitement
- **No progression variety:** Unit synthesis creates unique combinations
- **Unclear game state:** Clear UI showing all critical information
- **Frustrating difficulty spikes:** Progressive 30-round difficulty curve

## Requirements

### Functional Requirements

#### FR1: Game Initialization
- **FR1.1:** Display "게임 준비" animation on scene load
- **FR1.2:** Show 3-2-1 countdown with visual feedback
- **FR1.3:** Initialize game state: Round 1, Life 10, Gold 50
- **FR1.4:** Transition to first preparation phase after countdown

#### FR2: Grid System
- **FR2.1:** Display grid in portrait orientation (height > width)
- **FR2.2:** Each grid cell holds maximum 1 unit
- **FR2.3:** Grid size is fixed throughout game
- **FR2.4:** Grid placement area clearly distinguished from monster path

#### FR3: Unit System
- **FR3.1:** Unit Draw
  - Cost: 5 gold per draw
  - Random unit from rarity pool (Normal/Rare/Epic/Legendary)
  - Unit automatically appears in available slot

- **FR3.2:** Unit Placement
  - Click unit to select
  - Click another unit to swap positions
  - Click empty grid cell to place selected unit

- **FR3.3:** Unit Types
  - **Melee:** Short range, high HP
  - **Ranged:** Long range, moderate damage
  - **Debuffer:** Apply status effects to monsters

- **FR3.4:** Unit Stats
  - Attack damage
  - Attack speed
  - Attack range (based on type)
  - Rarity tier (affects base stats)

- **FR3.5:** Unit Upgrade
  - Upgrade attack power using gold
  - Upgrade cost increases per level
  - Available during preparation phase only

#### FR4: Unit Synthesis
- **FR4.1:** Synthesis table showing combination recipes
- **FR4.2:** Drag units to synthesis area
- **FR4.3:** Click "조합" button to create new unit
- **FR4.4:** Consumed units removed, new unit appears
- **FR4.5:** Synthesis follows predefined recipe rules

#### FR5: Monster System
- **FR5.1:** Monster Spawning
  - Spawn from TOP and BOTTOM of map
  - 2 monsters per second
  - 20 monsters total per round
  - Spawn stops after 10 seconds

- **FR5.2:** Monster Movement
  - Follow predefined straight path
  - Top and bottom paths meet at center point
  - Monsters loop back to start if reaching end
  - Movement speed consistent within round

- **FR5.3:** Monster Stats
  - HP (increases each round)
  - Defense (increases each round)
  - Multiple monster types possible

- **FR5.4:** Monster Death
  - Award gold to player
  - Remove from field
  - Decrease "remaining monsters" counter

#### FR6: Combat System
- **FR6.1:** Auto-combat (no manual control during combat)
- **FR6.2:** Units automatically target monsters in range
- **FR6.3:** Damage calculation considers:
  - Unit attack power
  - Monster defense
  - Critical hits (if applicable)
- **FR6.4:** Range-based targeting:
  - Melee: Adjacent grid cells
  - Ranged: Extended radius
  - Debuffer: Area effects

#### FR7: Round System
- **FR7.1:** Preparation Phase (30 seconds)
  - Timer displays countdown
  - Allow: Unit draw, placement, synthesis, upgrade
  - Auto-start combat when timer expires

- **FR7.2:** Combat Phase (60 seconds)
  - Timer displays countdown
  - Monster spawning active for first 10 seconds
  - Combat continues until time expires OR all monsters killed

- **FR7.3:** Round Completion
  - All monsters killed before time: SUCCESS
  - Time expires with monsters remaining: Life -= remaining monsters
  - Transition to next preparation phase if life > 0

- **FR7.4:** Progressive Difficulty
  - Each round: Monster HP +X%, Defense +Y%
  - Round 30 = maximum difficulty

#### FR8: Game State Management
- **FR8.1:** Life System
  - Start with 10 life
  - Lose life equal to remaining monsters when round time expires
  - Game Over when life reaches 0

- **FR8.2:** Gold Economy
  - Starting gold: 50
  - Earn gold per monster kill (amount TBD)
  - Spend gold on: Unit draws (5g), upgrades, synthesis

- **FR8.3:** Victory Condition
  - Complete all 30 rounds with life > 0

- **FR8.4:** Defeat Condition
  - Life reaches 0
  - No restart available (return to main menu)

#### FR9: UI Requirements
- **FR9.1:** HUD Display
  - Current Round (e.g., "Round 5/30")
  - Remaining Time (countdown timer)
  - Remaining Monsters (e.g., "15/20")
  - Current Gold (e.g., "45G")
  - Available Unit Slots (e.g., "3/10")
  - Life Points (e.g., "7/10")

- **FR9.2:** Unit Draw Button
  - Shows cost (5 gold)
  - Disabled if insufficient gold
  - Visual feedback on click

- **FR9.3:** Synthesis Interface
  - Dedicated area showing available recipes
  - Drag-and-drop or click-to-add units
  - Clear visual feedback for valid/invalid combinations

- **FR9.4:** Upgrade Interface
  - Shows current unit stats
  - Upgrade cost and new stats preview
  - Confirm button

### Non-Functional Requirements

#### NFR1: Performance
- Maintain 60 FPS during combat with 20 monsters and 10+ units
- Scene load time < 2 seconds
- Smooth animations (countdown, unit placement, combat)

#### NFR2: Usability
- Portrait orientation optimized for one-handed play
- Touch targets minimum 44x44 points
- Clear visual hierarchy for important information
- Intuitive drag-and-drop or tap interactions

#### NFR3: Scalability
- Support multiple device resolutions (iOS and Android)
- Grid scales proportionally to screen size
- Text remains readable on small screens

#### NFR4: Reliability
- No crashes during 30-round playthrough
- Save game state between preparation/combat phases
- Graceful handling of edge cases (insufficient gold, invalid placements)

## Success Criteria

### Measurable Outcomes

1. **Gameplay Completion**
   - Players can complete full 30-round game
   - All core mechanics (draw, place, synthesize, upgrade, combat) functional
   - Win/loss conditions trigger correctly

2. **Performance Metrics**
   - 60 FPS maintained during combat
   - No memory leaks during extended play
   - Scene transitions < 0.5 seconds

3. **User Experience**
   - Countdown animation plays smoothly
   - UI elements clearly visible and responsive
   - Gold economy balanced (can afford units throughout game)

4. **Technical Quality**
   - Zero critical bugs in core gameplay loop
   - All unit types function as designed
   - Monster spawning/pathing works correctly

## Constraints & Assumptions

### Technical Constraints
- Unity 2D engine
- Portrait mobile orientation
- Current codebase has login/menu complete
- Must integrate with existing scene navigation

### Design Constraints
- Straight-line monster paths (may evolve to curves later)
- Fixed grid size (no dynamic expansion)
- No restart on game over (design decision)
- No pause functionality during rounds

### Timeline Constraints
- Implement core gameplay loop first
- Polish and balance can iterate later
- Art assets can be placeholder initially

### Assumptions
- Players understand basic tower defense concepts
- Touch controls are primary input method
- Game sessions complete in 15-30 minutes
- Players accept gacha randomness as core mechanic

## Out of Scope

### Explicitly NOT Building

1. **Multiplayer Features**
   - No co-op or competitive modes
   - No leaderboards or social features

2. **Meta Progression**
   - No persistent upgrades between games
   - No player leveling system
   - No unlock system for units

3. **Advanced Features**
   - No skill trees or talent systems
   - No equipment system
   - No multiple game modes (endless, challenge, etc.)

4. **Monetization**
   - No in-app purchases
   - No ads integration
   - No premium currency

5. **Save System**
   - No mid-game save/load
   - Game state resets on app close
   - No replay functionality

6. **Tutorial System**
   - No interactive tutorial (may add later)
   - No contextual tooltips
   - Players learn through experimentation

## Dependencies

### Internal Dependencies
- **Existing Systems**
  - LoginScene (complete)
  - MainGame menu scene (complete)
  - SceneNavigator (complete)
  - AuthenticationManager (complete)

- **New Systems Required**
  - GameplayManager: Round state machine
  - UnitManager: Gacha, placement, synthesis
  - MonsterManager: Spawning, movement, AI
  - GridManager: Grid logic and validation
  - CombatManager: Damage calculation and resolution
  - UIManager: HUD updates and user feedback

### External Dependencies
- Unity Engine 6000.0.60f1
- Unity Input System package
- Unity 2D sprite rendering
- Unity UI (uGUI)

### Asset Dependencies
- Unit sprites (4 rarities × multiple types)
- Monster sprites (multiple types)
- Grid background/tile sprites
- UI elements (buttons, panels, icons)
- Animation assets (countdown, attack effects)
- Audio (optional for v1)

### Technical Dependencies
- Scene transition system working
- Build settings include GameScene
- Performance profiling tools
- Mobile testing devices (iOS/Android)

## Implementation Phases

### Phase 1: Core Framework (Week 1)
- GameplayManager state machine (Countdown → Prep → Combat → Result)
- Grid system with click/placement logic
- Basic UI layout (HUD with all required info)
- Scene transitions from MainGame to GameScene

### Phase 2: Unit System (Week 2)
- Unit gacha with rarity system
- Unit placement and swapping
- Basic unit stats and rendering
- Placeholder unit sprites

### Phase 3: Combat System (Week 3)
- Monster spawning system
- Monster movement along straight paths
- Auto-combat (unit attacks, damage calculation)
- Monster death and gold rewards

### Phase 4: Advanced Features (Week 4)
- Unit synthesis system with recipe table
- Unit upgrade functionality
- Round progression (difficulty scaling)
- Win/loss conditions

### Phase 5: Polish & Balance (Week 5)
- Animation polish (countdown, combat effects)
- UI feedback improvements
- Game balance tuning (gold economy, difficulty curve)
- Bug fixes and optimization

## Open Questions

1. **Grid Dimensions:** Exact grid size (e.g., 5x8, 6x10)?
2. **Gold Rewards:** How much gold per monster kill?
3. **Upgrade Costs:** Scaling formula for upgrade prices?
4. **Synthesis Recipes:** Specific unit combinations and results?
5. **Monster Variety:** How many different monster types per round range?
6. **Difficulty Scaling:** Exact HP/Defense increase percentages per round?
7. **Unit Balance:** Damage/speed values for each rarity and type?
8. **Art Style:** Visual aesthetic for units, monsters, and UI?

## Next Steps

1. **Design Review:** Validate PRD with stakeholders
2. **Create Epic:** Run `/pm:prd-parse lotto-defense` to generate implementation epic
3. **Art Direction:** Define visual style and create asset list
4. **Prototyping:** Build core loop (grid + spawning + combat) for gameplay validation
5. **Balance Spreadsheet:** Create detailed stats for units, monsters, and economy

---

**Prepared by:** Claude Code
**Date:** 2025-10-28
**Version:** 1.0
**Status:** Ready for Epic Creation
