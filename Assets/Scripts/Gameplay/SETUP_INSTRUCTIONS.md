# GameScene Setup Instructions for Issue #17

## Overview
This document provides step-by-step instructions for setting up the Game State Machine and Countdown Animation in the GameScene.

## Prerequisites
- All scripts in `Assets/Scripts/Gameplay/` have been created
- Unity Editor is open with the project loaded

## Step 1: Add GameplayManager Component

1. Open `GameScene.unity` in Unity Editor
2. In the Hierarchy, select the existing `GameplayManager` GameObject
3. In the Inspector, click "Add Component"
4. Search for "GameplayManager" and add it
5. The component requires no manual configuration (all initialization is automatic)

## Step 2: Create CountdownUI Hierarchy

1. In the Hierarchy, right-click on `Canvas` and select `UI → Panel`
2. Rename the new panel to `CountdownUI`
3. Select `CountdownUI` in the Hierarchy
4. In the Inspector, configure the RectTransform:
   - Anchor Presets: Center/Middle (stretch)
   - Left: 0, Top: 0, Right: 0, Bottom: 0
   - Width: 0, Height: 0
   - Scale: (1, 1, 1)

## Step 3: Add CountdownText

1. Right-click on `CountdownUI` and select `UI → Text - TextMeshPro`
   - If prompted to import TMP Essentials, click "Import TMP Essentials"
2. Rename the text object to `CountdownText`
3. Configure the RectTransform:
   - Anchor Presets: Center/Middle
   - Width: 300, Height: 300
   - Pos X: 0, Pos Y: 0
4. Configure the TextMeshProUGUI component:
   - Font Size: 120
   - Alignment: Center (both horizontal and vertical)
   - Color: White (#FFFFFF)
   - Font Style: Bold
   - Text: "3" (placeholder)

## Step 4: Add CountdownUI Component

1. Select `CountdownUI` GameObject in the Hierarchy
2. In the Inspector, click "Add Component"
3. Search for "CountdownUI" and add it
4. Configure the component references:
   - **Countdown Text**: Drag the `CountdownText` child object here
   - **Canvas Group**: Should auto-populate (or drag CountdownUI itself)
   - Leave other fields at default values:
     - Countdown Interval: 1
     - Scale Animation Duration: 0.8
     - Start Scale: 0.5
     - Peak Scale: 1.5
     - End Scale: 1.0

## Step 5: Optional - Add Audio

If you have a countdown beep sound:

1. Select `CountdownUI` GameObject
2. Add Component → Audio Source
3. Configure Audio Source:
   - Play On Awake: OFF
   - Loop: OFF
4. In the CountdownUI component:
   - **Audio Source**: Auto-populated
   - **Countdown Beep**: Drag your audio clip here

## Step 6: Deactivate CountdownUI by Default

1. Select `CountdownUI` GameObject in the Hierarchy
2. In the Inspector, uncheck the checkbox next to the GameObject name at the top
3. This ensures it starts hidden and is activated by GameplayManager

## Step 7: Verify Setup

1. Save the scene (Ctrl+S / Cmd+S)
2. Enter Play Mode
3. Expected behavior:
   - Countdown displays "3" → "2" → "1" with scaling animation
   - Each number appears for 1 second
   - After "1", countdown fades out
   - Console shows state transition: Countdown → Preparation
   - Console shows: "Round: 1, Life: 10, Gold: 50"

## Troubleshooting

### Countdown doesn't appear
- Check that CountdownUI is deactivated in the scene (not active by default)
- Verify GameplayManager component is on the GameplayManager GameObject
- Check Console for any error messages

### No state transition
- Verify CountdownUI component has proper references
- Check that the callback is properly wired in CountdownUI.StartCountdown()

### Animation doesn't work
- Ensure CountdownText is assigned in CountdownUI component
- Check that the text has a RectTransform (all UI elements should)

## Testing Checklist

- [ ] GameplayManager component is on GameplayManager GameObject
- [ ] CountdownUI GameObject exists as child of Canvas
- [ ] CountdownText uses TextMeshProUGUI
- [ ] CountdownUI component has all references assigned
- [ ] CountdownUI GameObject is inactive in the scene
- [ ] Countdown plays on scene start in Play Mode
- [ ] Console shows proper state transitions
- [ ] Initial game values are correct (Round=1, Life=10, Gold=50)
