# Lotto Defense — 전체 코드 분석 보고서

> 작업 준비를 위한 프로젝트 이해 문서. Code Reviewer 관점에서 구조, 아키텍처, 규칙을 정리함.

---

## 1. 프로젝트 정체

| 항목 | 내용 |
|------|------|
| **이름** | Lotto Defense |
| **장르** | 2D 타워 디펜스 + 가챠(뽑기) |
| **플랫폼** | 모바일 (iOS/Android) |
| **엔진** | Unity 6 (6000.0.60f1) |
| **렌더** | URP 2D, Portrait 9:16 |
| **코드량** | 약 15,000 라인 C# |
| **목표 성능** | 60 FPS, 메모리 <135 MB |

**한줄 요약:** 로그인 → 메인메뉴 → 게임씬에서, 골드로 유닛을 뽑고 6×10 그리드에 배치·합성·업그레이드하며 30라운드 몬스터 웨이브를 방어하는 2D 타워 디펜스 게임.

---

## 2. 기술 스택

- **언어**: C#
- **Unity 패키지**: 2D Animation, Sprite, Tilemap, Input System, URP, TextMeshPro, Test Framework
- **아키텍처**: 싱글톤 매니저 + 이벤트 기반 연동, ScriptableObject 데이터
- **테스트**: Unity Test Framework (Editor 테스트: `DifficultyConfigTests`, `RoundManagerTests`)

---

## 3. 씬 흐름

```
LoginScene (Google 로그인 시뮬레이션)
    → MainGame (메인 메뉴, 인증 체크)
        → GameScene (실제 플레이: 카운트다운 → 준비 → 전투 → 라운드결과 → 승리/패배)
```

- **SceneNavigator**: `LoadGameScene()`, `LoadMainGame()`, `LoadLoginScene()`, `QuitGame()` 제공.
- **Build Settings** 에 LoginScene, MainGame, GameScene 순서로 등록 필요.

---

## 4. 네임스페이스 및 모듈

| 네임스페이스 | 역할 | 대표 타입 |
|-------------|------|-----------|
| `LottoDefense.Authentication` | 로그인/로그아웃 | `AuthenticationManager` |
| `LottoDefense.Controllers` | 씬별 진입점 | `AppStartupManager`, `LoginSceneController`, `MainGameManager` |
| `LottoDefense.Gameplay` | 라운드·골드·생명·상태머신 | `GameplayManager`, `RoundManager`, `GameState`, `DifficultyConfig`, `BalanceConfig` |
| `LottoDefense.Grid` | 6×10 그리드 | `GridManager`, `GridCell`, `CellState` |
| `LottoDefense.Units` | 유닛 데이터·가챠·배치·합성·업그레이드 | `UnitManager`, `UnitData`, `Unit`, `UnitPlacementManager`, `SynthesisManager`, `UpgradeManager`, `Rarity`, `UnitType` |
| `LottoDefense.Monsters` | 몬스터 스폰·풀·데이터 | `MonsterManager`, `Monster`, `MonsterData`, `MonsterPool`, `PathType` |
| `LottoDefense.Combat` | 유닛↔몬스터 자동 전투 | `CombatManager` (틱 기반 타겟팅/공격) |
| `LottoDefense.UI` | HUD·인벤토리·합성·업그레이드·승패·씬전환 | `GameHUD`, `InventoryUI`, `SynthesisPanel`, `UpgradeUI`, `VictoryScreen`, `DefeatScreen`, `SceneNavigator` |
| `LottoDefense.VFX` | 데미지 숫자·플로팅 텍스트·이펙트 | `VFXManager`, `DamageNumberController`, `FloatingTextController` |
| `LottoDefense.Configuration` | 설정 SO | `LoginConfig` (ScriptableObjects 폴더) |

---

## 5. 핵심 시스템 (10개)

1. **Game State Machine**  
   `GameplayManager`: `GameState` (Countdown → Preparation → Combat → RoundResult → Victory/Defeat).  
   `OnStateChanged`, `OnGameValueChanged` 로 UI/다른 매니저 연동.

2. **Grid System**  
   `GridManager`: 6×10, 세로 비율 9:16, `CellSize`/`GridOrigin` 계산, 셀 선택/해제 이벤트.

3. **Unit Data & Gacha**  
   `UnitManager`: 골드 5로 뽑기, 레어도 가중치 (Normal 50%, Rare 30%, Epic 15%, Legendary 5%).  
   `UnitData` (ScriptableObject): type(Melee/Ranged/Debuffer), rarity, attack/defense/attackRange/attackSpeed, prefab/icon.

4. **Monster Spawning**  
   `MonsterManager`: 라운드당 스폰 수·스폰 간격, Top/Bottom 경로 번갈아 사용, `MonsterPool` 오브젝트 풀, `DifficultyConfig`로 라운드별 HP/방어력 스케일.

5. **Unit Placement**  
   `UnitPlacementManager`: 그리드 셀에 유닛 배치/교체, `GridManager`/`UnitManager`와 연동.

6. **Combat System**  
   `CombatManager`: 0.1초 틱, 유닛→몬스터 자동 타겟팅 및 데미지 적용, `OnUnitAttack`/`OnMonsterDamaged` 이벤트.

7. **Unit Synthesis**  
   `SynthesisManager`: 레시피(`SynthesisRecipe`) 기반 2유닛→1유닛 합성, 발견 레시피 저장.

8. **Unit Upgrade**  
   `UpgradeManager`: 유닛 강화 (데이터/레벨 등).

9. **Round Management**  
   `RoundManager`: Preparation/Combat 페이즈, 타이머, 라운드 종료 시 생명 감소(남은 몬스터 수), 30라운드까지.  
   `DifficultyConfig`: 라운드별 HP/Defense 곡선(AnimationCurve).

10. **Polish & Balance**  
    VFX, `BalanceConfig`, `MobileOptimizationManager`, 난이도/밸런스 튜닝.

---

## 6. 데이터 에셋

- **경로**: `Assets/Data/`
  - `DifficultyConfig.asset`: 최대 라운드 30, HP/Defense 곡선.
  - `Monsters/*.asset`: SlimeMonster, Goblin, ArmoredOgre, SpeedDemon, DragonBoss (MonsterData).
  - `Units/`: 유닛 에셋 가이드(README, UNIT_CREATION_GUIDE, unit_stats_reference.json).
  - `Recipes/`: 합성 레시피 템플릿.
- **ScriptableObject**: `UnitData`, `MonsterData`, `DifficultyConfig`, `BalanceConfig`, `SynthesisRecipe`, `LoginConfig`.

---

## 7. 싱글톤 패턴

대부분의 매니저가 싱글톤 + `DontDestroyOnLoad` (해당 씬에서 유지되는 경우):

- `GameplayManager`, `RoundManager`, `GridManager`, `UnitManager`, `MonsterManager`, `CombatManager`, `SynthesisManager`, `UpgradeManager`, `UnitPlacementManager`
- `AuthenticationManager`: `DontDestroyOnLoad`로 씬 전환 후에도 유지.

접근: `XxxManager.Instance` (없으면 `FindFirstObjectByType` 또는 런타임 생성).

---

## 8. 이벤트 사용

- **GameplayManager**: `OnStateChanged(GameState, GameState)`, `OnGameValueChanged(string, int)` (Round/Life/Gold).
- **RoundManager**: `OnPhaseChanged`, `OnTimerUpdated`.
- **CombatManager**: `OnCombatStarted/Stopped/Tick`, `OnUnitAttack`, `OnMonsterDamaged`.
- **MonsterManager**: `OnMonsterSpawned` 등.
- **UnitManager**: `OnUnitDrawn`, `OnInventoryChanged`, `OnDrawFailed`.

UI는 위 이벤트 구독으로 갱신 (예: `GameHUD` ↔ `GameplayManager.OnGameValueChanged`).

---

## 9. 테스트

- **위치**: `Assets/Tests/Editor/`
- **어셈블리**: `LottoDefense.Tests.Editor.asmdef`
- **파일**: `DifficultyConfigTests.cs`, `RoundManagerTests.cs`
- **기타**: 각 모듈별 `*Tester.cs` (예: `GameplayManagerTester`, `CombatSystemTester`) — 키보드 단축키로 Play Mode에서 동작.

CLAUDE.md 요구사항: 모든 함수에 테스트, 치팅/허수 테스트 금지, 실사용 반영·디버깅에 쓸 수 있도록 설계.

---

## 10. 프로젝트 규칙 (CLAUDE.md)

- **부분 구현 금지**, **단순화 주석으로 대체 금지**.
- **코드 중복 금지**: 기존 코드 검색 후 재사용, 상식적인 함수명 사용.
- **데드 코드 금지**: 사용하거나 제거.
- **모든 함수에 테스트** 작성, 치팅 테스트 금지.
- **네이밍 일관성**: 기존 코드베이스 패턴 따르기.
- **과도한 추상화/엔터프라이즈 패턴 지양**.
- **관심사 분리**: 검증 로직을 API 핸들러 안에, DB 쿼리를 UI 안에 넣지 않기.
- **리소스 누수 방지**: 연결/타이머/이벤트 리스너 정리.

---

## 11. 작업 시 참고 포인트

- **새 유닛/몬스터 추가**: `UnitData`/`MonsterData` ScriptableObject 생성 후 매니저 풀에 등록.
- **밸런스/난이도**: `DifficultyConfig`, `BalanceConfig` 및 `MonsterData` 스케일링 필드.
- **씬/버튼 연결**: EventSystem, Build Settings 씬 순서, `SceneNavigator` 호출부.
- **실제 Google 로그인**: `AuthenticationManager.AuthenticateWithGoogle()` 현재 시뮬레이션만 구현됨 — TODO.
- **GameScene 풀 셋업**: `HOW_TO_PLAY.md` 및 `QUICK_SETUP_GUIDE.md`, `Assets/Scripts/Gameplay/SETUP_INSTRUCTIONS.md` 등 참고.

---

## 12. 디렉터리 요약

```
npz/
├── Assets/
│   ├── Data/           # DifficultyConfig, Monsters/*.asset, Units, Recipes
│   ├── Editor/         # 씬/빌드/유닛 데이터 생성 헬퍼
│   ├── Scenes/         # LoginScene, MainGame, GameScene, SampleScene
│   ├── Scripts/       # Authentication, Combat, Controllers, Gameplay, Grid, Monsters, ScriptableObjects, UI, Units, VFX
│   ├── Settings/      # URP 2D, Lit2D
│   └── Tests/Editor/  # DifficultyConfigTests, RoundManagerTests
├── Packages/           # manifest.json (Unity 2D, URP, InputSystem, Test Framework 등)
├── ProjectSettings/
├── CLAUDE.md           # 절대 규칙 및 톤
├── HOW_TO_PLAY.md      # 실행·플레이·설정 가이드
├── QUICK_SETUP_GUIDE.md
└── README.md
```

---

이 문서를 기준으로 특정 시스템 수정·기능 추가·리팩터링 시 네임스페이스·매니저 역할·이벤트·데이터 경로를 빠르게 참조할 수 있다.
