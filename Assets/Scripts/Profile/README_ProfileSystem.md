# Profile System - User Nickname & Avatar System

Complete user profile system with nickname customization, avatar selection, and hidden avatar unlocks.

## Overview

The Profile System allows users to:
- **Set a custom nickname** (2-12 characters)
- **Select from multiple avatar options** (unlocked by default or via quests/achievements)
- **Unlock hidden avatars** by completing specific quests
- **Display profile** (avatar + nickname) in game HUD header

## System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Profile System                           │
├─────────────────────────────────────────────────────────────┤
│  Data Layer                                                  │
│  ├─ ProfileAvatarData (ScriptableObject)                    │
│  └─ UserProfile (Serializable Model)                        │
├─────────────────────────────────────────────────────────────┤
│  Manager Layer                                               │
│  ├─ UserProfileManager (Singleton)                          │
│  └─ ProfileUnlockManager (Quest Integration)                │
├─────────────────────────────────────────────────────────────┤
│  UI Layer                                                    │
│  ├─ ProfileSelectionUI (Avatar Selection + Nickname Input)  │
│  └─ ProfileHeaderDisplay (HUD Display Component)            │
└─────────────────────────────────────────────────────────────┘
```

## Quick Start

### 1. Create Avatar Assets

Create avatar ScriptableObjects in Unity:

**Path**: `Assets/Resources/Avatars/`

```
Right-click → Create → LottoDefense → Profile → Avatar Data
```

**Example Avatar Configuration**:
```
Avatar ID: avatar_warrior
Avatar Name: 전사
Avatar Sprite: [Assign 128x128 sprite]
Border Color: Red (255, 100, 100)
Is Default Unlocked: true
```

**Example Hidden Avatar**:
```
Avatar ID: avatar_dragon_master
Avatar Name: 드래곤 마스터
Avatar Sprite: [Assign 128x128 sprite]
Border Color: Gold (255, 215, 0)
Is Default Unlocked: false
Required Quest ID: quest_slay_100_monsters
Unlock Hint: "처치한 몬스터 100마리 달성"
Rarity: Legendary
```

### 2. Initialize System in Game

The system auto-initializes in `GameSceneBootstrapper.cs`:

```csharp
private void Awake()
{
    // ... other managers
    EnsureProfileManagers(); // Initializes UserProfileManager + ProfileUnlockManager
}
```

### 3. Display Profile in HUD

Profile header is automatically added to GameHUD:

```csharp
// In GameSceneBootstrapper.UI.cs
private void EnsureGameHUD()
{
    // ... HUD setup
    GameObject profileHeaderRow = CreateProfileHeader(hudObj.transform);
    // ... rest of HUD
}
```

## Usage Examples

### Accessing User Profile

```csharp
using LottoDefense.Profile;

// Get current nickname
string nickname = UserProfileManager.Instance.Nickname;

// Get selected avatar sprite
Sprite avatarSprite = UserProfileManager.Instance.GetCurrentAvatarSprite();

// Check if avatar is unlocked
bool isUnlocked = UserProfileManager.Instance.CurrentProfile.HasUnlockedAvatar("avatar_dragon_master");
```

### Changing Nickname

```csharp
// From code
bool success = UserProfileManager.Instance.SetNickname("DragonSlayer");
if (success)
{
    Debug.Log("Nickname changed successfully!");
}

// Via UI
// User opens ProfileSelectionUI and enters nickname
// System validates (2-12 characters) and saves automatically
```

### Selecting Avatar

```csharp
// From code
bool success = UserProfileManager.Instance.SelectAvatar("avatar_warrior");
if (success)
{
    Debug.Log("Avatar changed successfully!");
}

// Via UI
// User clicks ProfileHeaderDisplay → Opens ProfileSelectionUI
// User clicks avatar icon → Automatically saved
```

### Unlocking Hidden Avatars

**Via Quest Completion (Automatic)**:
```csharp
// ProfileUnlockManager automatically listens to QuestManager.OnQuestCompleted
// When quest "quest_slay_100_monsters" completes, avatar_dragon_master unlocks

// No code needed - works automatically!
```

**Manual Unlock (Testing/Dev)**:
```csharp
ProfileUnlockManager.Instance.ManualUnlockAvatar("avatar_dragon_master");
```

**Via Achievement (Future)**:
```csharp
// When achievement system is implemented
ProfileUnlockManager.Instance.CheckAchievementUnlock("achievement_speedrunner");
```

## Quest Integration

### Setting Up Hidden Avatar Quests

1. **Create ProfileAvatarData** with quest requirement:
```
Avatar ID: avatar_legendary_warrior
Required Quest ID: quest_reach_round_50
Is Default Unlocked: false
```

2. **Create corresponding quest** in QuestConfig:
```csharp
new QuestDefinition
{
    id = "quest_reach_round_50",
    title = "라운드 50 도달",
    description = "생존하여 라운드 50에 도달하세요",
    type = QuestType.Combat,
    // ... rest of quest config
}
```

3. **Done!** - System automatically unlocks avatar when quest completes.

### Retroactive Unlock Check

If you add new hidden avatars to an existing save:

```csharp
// Check all completed quests and unlock eligible avatars
ProfileUnlockManager.Instance.CheckAllQuestsForUnlocks();
```

## UI Components

### ProfileSelectionUI

**What it does**:
- Display all available avatars in scrollable grid
- Show locked/unlocked state with lock icon
- Allow nickname input with validation
- Live preview of selected avatar + nickname

**How to open**:
```csharp
ProfileSelectionUI profileUI = FindFirstObjectByType<ProfileSelectionUI>();
profileUI.Show();
```

**User clicks profile header** → Opens ProfileSelectionUI automatically.

### ProfileHeaderDisplay

**What it does**:
- Display current avatar + nickname in HUD
- Clickable to open ProfileSelectionUI
- Auto-updates when profile changes

**Location**: Top of GameHUD (after ROUND/PHASE/TIME row)

## Data Persistence

### Save/Load

Profile data saved automatically to **PlayerPrefs**:
```
Key: "UserProfile"
Format: JSON (JsonUtility)
```

**Saved data includes**:
- Nickname
- Selected avatar ID
- List of unlocked avatar IDs

**When data is saved**:
- Nickname change
- Avatar selection
- Avatar unlock

### Reset Profile (Testing)

```csharp
UserProfileManager.Instance.ResetProfile();
// Clears all data, resets to default
```

## Avatar Rarity System

**Rarities**:
- Common (기본)
- Uncommon (고급)
- Rare (희귀)
- Epic (영웅)
- Legendary (전설)
- Hidden (히든)

**Rarity affects**:
- Visual border color
- Unlock difficulty
- Player prestige

## Events

Subscribe to profile change events:

```csharp
using LottoDefense.Profile;

void OnEnable()
{
    UserProfileManager.Instance.OnNicknameChanged += HandleNicknameChanged;
    UserProfileManager.Instance.OnAvatarChanged += HandleAvatarChanged;
    UserProfileManager.Instance.OnAvatarUnlocked += HandleAvatarUnlocked;
}

void HandleNicknameChanged(string newNickname)
{
    Debug.Log($"Nickname changed to: {newNickname}");
}

void HandleAvatarChanged(string newAvatarId)
{
    Debug.Log($"Avatar changed to: {newAvatarId}");
}

void HandleAvatarUnlocked(string avatarId)
{
    Debug.Log($"Avatar unlocked: {avatarId}");
    // Show celebration animation
}
```

## Validation Rules

### Nickname Validation
- **Length**: 2-12 characters
- **Whitespace**: Trimmed automatically
- **Empty**: Not allowed

### Avatar Selection
- Must be unlocked
- Must exist in avatar database

## Troubleshooting

### "No avatars available"
**Cause**: No ProfileAvatarData in `Assets/Resources/Avatars/`
**Fix**: Create at least one default avatar asset

### "Avatar not unlocking after quest completion"
**Cause**: Quest ID mismatch
**Fix**: Verify `avatar.requiredQuestId` matches `quest.id` exactly

### "Profile button doesn't open UI"
**Cause**: ProfileSelectionUI not in scene
**Fix**: Ensure ProfileSelectionUI is created (or create programmatically)

### "Avatar sprite is null"
**Cause**: Sprite not assigned in ProfileAvatarData
**Fix**: Assign sprite in avatar asset inspector

## Performance Notes

- Avatar database loaded once on startup (Resources.LoadAll)
- Profile data cached in memory (no repeated PlayerPrefs reads)
- UI refreshed only on profile changes (event-driven)
- Grid uses object pooling for avatar buttons

## Future Enhancements

- [ ] Backend API integration for cross-device sync
- [ ] Achievement-based unlocks
- [ ] Seasonal/event exclusive avatars
- [ ] Avatar animation support
- [ ] Nickname profanity filter
- [ ] Social features (view friend profiles)
- [ ] Avatar trading/gifting system

## Credits

Designed and implemented by Senior Frontend Development System.
For questions or support, check project documentation.
