---
name: mobile-login-screen
description: Unity 2D mobile defense game login screen with Google Sign-In button and visual branding
status: backlog
created: 2025-10-27T02:54:51Z
---

# PRD: Mobile Login Screen

## Executive Summary

Create an engaging login screen for a Unity 2D mobile defense game featuring Google Sign-In authentication. The screen will serve as the entry point to the game, displaying game branding and a Google login button. Initially, the button will simulate login for UI/UX validation, with the architecture prepared for future Google OAuth integration.

**Target Platform**: Mobile (iOS/Android)
**Orientation**: Portrait (locked)
**Resolution**: 16:9 aspect ratio
**Timeline**: Phase 1 (Visual-only) → Phase 2 (OAuth integration)

## Problem Statement

### What problem are we solving?
Players need a welcoming, professional entry point to the game that:
- Clearly communicates the game's identity through visual branding
- Provides a frictionless authentication path via Google Sign-In
- Maintains modern mobile gaming UX standards
- Prepares infrastructure for future social features (leaderboards, achievements)

### Why is this important now?
- Establishes first impression of game quality and polish
- Validates UI/UX flow before investing in backend OAuth implementation
- Allows parallel development of core gameplay while auth infrastructure matures
- Meets compliance requirements (ToS/Privacy Policy visibility)

## User Stories

### Primary Persona: Mobile Gamer (Casual to Mid-Core)
**Demographics**: Ages 18-45, mobile-first gamers, familiar with Google Sign-In
**Behavior**: Expects quick onboarding, minimal friction, professional presentation

### User Journeys

#### Journey 1: First-Time User
```
1. User launches app
2. Sees branded login screen with game logo and animated background
3. Recognizes familiar Google Sign-In button at bottom
4. Taps button → sees loading feedback
5. Transitions smoothly to main menu
6. Future launches skip login (session persisted)
```

**Pain Points Being Addressed**:
- ❌ No confusing multi-step registration
- ❌ No manual form filling
- ❌ No unclear navigation
- ✅ One-tap authentication
- ✅ Clear visual hierarchy
- ✅ Persistent session

#### Journey 2: Returning User
```
1. User launches app
2. App detects persisted login state
3. Automatically skips to main menu (or shows brief splash)
```

## Requirements

### Functional Requirements

#### FR1: Scene Structure
- **FR1.1**: Separate `LoginScene` as game's initial scene
- **FR1.2**: Scene loads independently with no external dependencies
- **FR1.3**: Scene transition to `MainMenuScene` with fade/slide effect
- **FR1.4**: Support back button behavior (Android: exit app, iOS: N/A)

#### FR2: Visual Elements
- **FR2.1**: Game logo/title prominently displayed in upper 40% of screen
- **FR2.2**: Animated or static background image (full screen)
- **FR2.3**: Google Sign-In button positioned in lower 20% of screen (horizontally centered)
- **FR2.4**: Terms of Service and Privacy Policy links at bottom (small text)

#### FR3: Google Sign-In Button
- **FR3.1**: Follow Google Brand Guidelines (official colors, iconography)
  - Use official "Sign in with Google" asset package
  - Standard button dimensions: 280dp width (scalable)
  - Proper spacing and padding per guidelines
- **FR3.2**: Button displays Google "G" logo + "Sign in with Google" text
- **FR3.3**: Button idle state: standard appearance
- **FR3.4**: Button pressed state: visual feedback (scale/color shift)
- **FR3.5**: Button disabled state during loading

#### FR4: Temporary Login Simulation (Phase 1)
- **FR4.1**: On button press, show loading indicator (spinner overlay or button state)
- **FR4.2**: Simulate 1-2 second delay to mimic network request
- **FR4.3**: Store mock "logged in" flag in PlayerPrefs
- **FR4.4**: Navigate to MainMenuScene after successful "login"
- **FR4.5**: Handle simulated failure scenario (rare, for testing error handling)

#### FR5: Session Persistence
- **FR5.1**: On subsequent app launches, check PlayerPrefs for login state
- **FR5.2**: If logged in, skip LoginScene and load MainMenuScene directly
- **FR5.3**: Provide debug option to clear session (developer menu)

#### FR6: Legal Compliance
- **FR6.1**: Display "Terms of Service" and "Privacy Policy" links at screen bottom
- **FR6.2**: Links open system browser (or WebView) with placeholder URLs
- **FR6.3**: Links styled clearly but non-intrusively (small gray text)

#### FR7: Future OAuth Integration Readiness
- **FR7.1**: Code architecture separates UI logic from authentication logic
- **FR7.2**: Authentication interface/abstract class ready for Google Sign-In SDK injection
- **FR7.3**: Scene logic uses async/await pattern for future network calls
- **FR7.4**: Error handling structure supports OAuth error codes
- **FR7.5**: Prepared for Google Play Games Services integration (leaderboards, achievements)

### Non-Functional Requirements

#### NFR1: Performance
- **NFR1.1**: Scene loads in <1 second on mid-range devices (Snapdragon 660 equivalent)
- **NFR1.2**: UI remains responsive (60 FPS) during animations
- **NFR1.3**: Background image optimized (compressed, appropriate resolution)
- **NFR1.4**: Total scene memory footprint <50MB

#### NFR2: Visual Quality
- **NFR2.1**: UI scales properly across 16:9 devices (5.5" to 6.7" displays)
- **NFR2.2**: All text remains readable at minimum supported resolution (720x1280)
- **NFR2.3**: No visual artifacts or clipping on safe area boundaries
- **NFR2.4**: Smooth transitions (no stuttering or frame drops)

#### NFR3: Usability
- **NFR3.1**: Google button tap target minimum 48dp (Google Material Design guideline)
- **NFR3.2**: Loading feedback appears within 100ms of button press
- **NFR3.3**: Clear visual indication when button is tappable vs disabled
- **NFR3.4**: Legal links accessible but not distracting

#### NFR4: Accessibility
- **NFR4.1**: Sufficient contrast ratio for text elements (WCAG AA: 4.5:1 minimum)
- **NFR4.2**: Haptic feedback on button press (iOS/Android)
- **NFR4.3**: Support for system font scaling (within reasonable limits)

#### NFR5: Platform Compliance
- **NFR5.1**: Follows Google Sign-In Branding Guidelines
- **NFR5.2**: Compliant with App Store / Play Store policies on authentication UI
- **NFR5.3**: Portrait orientation locked in Unity settings and manifest

#### NFR6: Code Quality
- **NFR6.1**: Clean separation of concerns (UI, logic, data persistence)
- **NFR6.2**: No hardcoded values (use ScriptableObjects or config files)
- **NFR6.3**: Comprehensive XML documentation for public APIs
- **NFR6.4**: Unit tests for authentication state logic
- **NFR6.5**: Scene playable in Unity Editor without device build

## Success Criteria

### Measurable Outcomes

#### Development Metrics
- ✅ LoginScene builds and runs on Android/iOS test devices
- ✅ Scene transition completes in <500ms (measured)
- ✅ Zero console errors or warnings during normal flow
- ✅ Passes all unit tests (>90% code coverage for auth logic)

#### User Experience Metrics
- ✅ Testers can complete login flow in <5 seconds
- ✅ 100% of testers successfully understand how to login (no confusion)
- ✅ 0 crashes during login flow across 50+ test runs
- ✅ UI renders correctly on 5+ test devices (various screen sizes)

#### Quality Metrics
- ✅ Google Brand Guidelines compliance verified by checklist
- ✅ Legal links functional and accessible
- ✅ Session persistence works across app restarts (10/10 attempts)
- ✅ Code review approval from 2+ team members

#### Future Readiness
- ✅ Google Sign-In SDK integration requires <4 hours work
- ✅ No breaking changes to scene UI when adding real OAuth
- ✅ Authentication interface properly abstracts mock vs real implementation

## Technical Architecture

### Components

#### 1. LoginSceneController (MonoBehaviour)
- Orchestrates scene flow and state management
- Handles scene transitions
- Manages UI element references

#### 2. AuthenticationManager (Singleton)
- Interface: `IAuthenticationService`
- Phase 1 Implementation: `MockAuthenticationService`
- Phase 2 Implementation: `GoogleAuthenticationService`
- Persists login state via PlayerPrefs
- Provides async login methods

#### 3. UI Components
- `LoginButton`: Custom button with Google branding
- `LoadingOverlay`: Spinner/indicator during auth
- `LegalLinksPanel`: ToS/Privacy Policy buttons

#### 4. Scene Transition Manager
- Handles fade/slide effects between scenes
- Reusable across all scene transitions

### Data Flow
```
User Tap → LoginButton.OnClick()
  → LoginSceneController.HandleLogin()
  → AuthenticationManager.LoginAsync()
  → MockAuthenticationService.SimulateLogin() [Phase 1]
  → PlayerPrefs.SetString("isLoggedIn", "true")
  → SceneTransitionManager.LoadScene("MainMenu")
```

### Asset Requirements
- **Logo**: PNG with transparency, 512x512 minimum
- **Background**: 1080x1920 (portrait), compressed
- **Google Button**: Official asset from Google brand resources
- **Loading Spinner**: Unity UI sprite or animation

## Constraints & Assumptions

### Technical Constraints
- Unity version: 2D project (assumed LTS version: 2021.3+)
- Canvas Scaler: Screen Space - Overlay mode
- Target devices: Android 7.0+, iOS 12.0+
- No third-party UI frameworks (Unity UI only)

### Resource Constraints
- Development time: Phase 1 estimated 8-12 hours
- Single developer implementation
- No dedicated UI/UX designer (use templates/guidelines)
- Art assets: placeholder or free/purchased assets acceptable

### Assumptions
- MainMenuScene exists or will be created in parallel
- PlayerPrefs sufficient for session persistence (no encryption needed yet)
- Mock login acceptable for initial testing/demo builds
- Internet connection not required for Phase 1
- Real OAuth will be Priority 2+ milestone

## Out of Scope

### Explicitly NOT Building
- ❌ Real Google OAuth implementation (Phase 2)
- ❌ Email/password authentication
- ❌ Social media logins (Facebook, Apple)
- ❌ Guest login / Skip login option
- ❌ Username/password form fields
- ❌ Account creation flow
- ❌ Password reset functionality
- ❌ Sound effects (SFX) on button press
- ❌ Multi-language support (English only Phase 1)
- ❌ Landscape orientation support
- ❌ Tablet-specific layouts
- ❌ Accessibility features beyond basic contrast/tap targets
- ❌ Analytics tracking integration
- ❌ A/B testing infrastructure

## Dependencies

### External Dependencies
- **Google Brand Assets**: Download official button assets from Google
  - URL: https://developers.google.com/identity/branding-guidelines
  - Risk: Low (freely available)

- **Unity Asset Store**: Scene transition effects (optional)
  - Alternative: Custom fade implementation
  - Risk: Low (can implement manually)

### Internal Dependencies
- **MainMenuScene**: Must exist before integration testing
  - Owner: [Team member TBD]
  - Timeline: Parallel development acceptable
  - Blocker: Can use placeholder scene for testing

- **Art Assets**: Logo and background image
  - Owner: [Art team / Designer]
  - Timeline: Need before visual testing
  - Blocker: Can use placeholder graphics initially

### Future Dependencies (Phase 2)
- Google Sign-In Unity Plugin
- Backend authentication service
- Google Play Games Services SDK (for leaderboards/achievements)

## Implementation Phases

### Phase 1: Visual-Only Login (Current Scope)
**Timeline**: Sprint 1
**Deliverables**:
- Functional LoginScene with all visual elements
- Mock authentication working
- Scene transitions implemented
- Session persistence functional
- Legal links operational

### Phase 2: Google OAuth Integration (Future)
**Timeline**: Sprint 3-4 (after core gameplay)
**Deliverables**:
- Google Sign-In SDK integrated
- Real authentication flow
- Backend user management
- Error handling for network failures
- Token refresh logic

### Phase 3: Social Features (Future)
**Timeline**: Post-MVP
**Deliverables**:
- Google Play Games Services
- Leaderboards
- Achievements
- Friend invites

## Risk Assessment

### High Risk
- **Google Brand Compliance**: Incorrect button implementation could require rework
  - Mitigation: Use official assets, review guidelines checklist

### Medium Risk
- **Session Persistence Issues**: PlayerPrefs could be cleared by system
  - Mitigation: Add fallback logic, test on multiple devices

- **Scene Transition Performance**: Could cause stuttering on low-end devices
  - Mitigation: Profile early, optimize asset loading

### Low Risk
- **UI Scaling Issues**: Canvas might not scale perfectly across all devices
  - Mitigation: Test on multiple resolutions early, adjust anchors

## Appendix

### Google Sign-In Branding Guidelines Checklist
- [ ] Use official "Sign in with Google" button
- [ ] Correct color scheme (white background, Google blue text)
- [ ] Proper "G" logo positioning
- [ ] Minimum button size (280dp width)
- [ ] No modifications to official assets
- [ ] Proper padding/spacing around button

### Reference Materials
- Google Identity Branding Guidelines: https://developers.google.com/identity/branding-guidelines
- Unity Canvas Scaler Documentation: https://docs.unity3d.com/Manual/script-CanvasScaler.html
- Material Design Touch Targets: https://material.io/design/usability/accessibility.html

### Open Questions
1. What is the game's official name/logo? (Need from art team)
2. Should we show a splash screen before login? (Product decision)
3. Do we need server-side session validation eventually? (Architecture decision)
4. Should session expire after X days? (Security decision)

---

**Document Version**: 1.0
**Last Updated**: 2025-10-27
**Owner**: Development Team
**Reviewers**: Product, Engineering, Design
