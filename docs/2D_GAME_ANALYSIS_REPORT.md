# Lotto-defense 2D 게임 코드 분석 보고서

**엔진**: Unity (C#)  
**장르**: 톱다운 타워 디펜스 (그리드 배치, 몬스터 경로 이동)  
**분석 기준**: 2D 게임 스킬 (스프라이트, 타일맵, 물리, 카메라, 장르 패턴, 안티패턴)

---

## 1. 프로젝트 구조 및 2D 관련 모듈

| 영역 | 파일/모듈 | 역할 |
|------|------------|------|
| 스프라이트/애니메이션 | `GameSpriteLoader.cs`, `Unit.cs`, `Monster.cs`, `UnitData.cs`, `AnimationHelper.cs`, `VFXManager.cs` | 유닛/몬스터 스프라이트 로드, 프로시저럴 스프라이트, 코루틴 애니메이션 |
| 그리드/타일맵 | `GridManager.cs`, `GridCell.cs`, `CellState.cs` | 8×4 그리드, 셀 생성/상태, 월드 좌표 변환 (Unity Tilemap 미사용) |
| 물리/충돌 | `GridCell.cs`, `UnitManager.cs`, `UnitPlacementManager.cs`, `MonsterPool.cs`, `UnitSelectionUI.cs` | BoxCollider2D/CircleCollider2D 클릭·레이캐스트용, Rigidbody2D 없음 |
| 카메라 | `GridManager.cs`, `GameSceneValidator.cs`, `VFXManager.cs`, `FloatingTextController.cs`, `DamageNumberController.cs` | Orthographic, 고정 카메라 + VFX용 스크린 셰이크 |
| 입력 | `UnitSelectionUI.cs`, `UnitPlacementManager.cs`, `GameHUD.cs`, `MobileOptimizationManager.cs` | 터치/마우스, 터치 타겟 검증 |

---

## 2. 스프라이트 시스템

### 2.1 아틀라스 vs 개별 텍스처 (안티패턴)

- **`GameSpriteLoader.cs`**: `Resources/Sprites/Units`, `Resources/Sprites/Monsters`에서 PNG별로 `Resources.Load<Texture2D>` 후 `Sprite.Create`로 개별 스프라이트 생성. **아틀라스 미사용** → 드로우콜·메모리 비효율.
- **권장**: Unity Sprite Atlas로 묶거나, 런타임에 아틀라스에서 slice 로드.

### 2.2 애니메이션

- **프레임 애니메이션 없음**: Animator/AnimationClip 미사용. 모든 연출은 `AnimationHelper` + 코루틴(Scale, Fade, Flash, Move).
- **프레임 레이트**: 스킬 권장 8–24 FPS와 무관. 스크립트 기반 트윈만 존재.
- **피벗**: `new Vector2(0.5f, 0.5f)`로 통일 → 일관적.

### 2.3 스프라이트 생성 방식 이중화

- **`UnitData.CreateCircleSprite(int size)`**: 매 호출마다 `new Texture2D` 생성 후 `Sprite.Create` 반환. **캐시 없음** → 호출마다 텍스처 누수.
  - 사용처: `GameSceneBootstrapper.cs:549`, `UnitManager.cs:586`, `UnitPlacementManager.cs:466`, `SynthesisGuideUI.cs:199,222,305`
- **`Unit.GetOrCreateCircleSprite()`**: static 캐시 사용 → 올바른 패턴.
- **권장**: `UnitData.CreateCircleSprite`도 size별 static 캐시 도입하거나, `Unit.GetOrCreateCircleSprite()`와 통합.

---

## 3. 타일맵/그리드 설계

### 3.1 타일맵

- **Unity Tilemap 미사용**: 타일맵 컴포넌트 없음. `GridManager`가 셀당 GameObject + SpriteRenderer + BoxCollider2D 생성.
- **타일 크기**: 고정 픽셀(64×64)이 아니라 **화면 비율 기반** (`CellSize = Mathf.Min(cellByWidth, cellByHeight)`). 2D 스킬 권장(16/32/64)과는 “논리적 셀” 개념으로 일치 가능하나, 실제 셀 스프라이트는 64×64 프로시저럴 또는 `cellSprite` 할당에 의존.

### 3.2 레이어 (Background / Terrain / Props / Foreground)

- **SortingOrder만 사용**:  
  Grid 0, Unit 10, Ring/VFX 50, HP bar 100–101, UI 100 등.  
  **SortingLayer**는 미사용 → 레이어 이름으로 구분하는 구조 없음.

### 3.3 충돌

- **GridCell**: BoxCollider2D, 클릭 감지용. 충돌 레이어/필터링 없음.
- **유닛**: BoxCollider2D (UnitManager, UnitPlacementManager에서 추가). 클릭/배치용.
- **몬스터**: CircleCollider2D (MonsterPool). 형태에 맞춤 → 적절.

---

## 4. 2D 물리

### 4.1 물리 엔진 사용 여부

- **Rigidbody2D 없음**: 이동은 전부 `Transform`/경로 보간. 타워디펜스에 맞는 선택.
- **FixedUpdate / fixedDeltaTime 미사용**: 물리 시뮬레이션 없으므로 문제 없음.
- **Physics2D 사용처**: `UnitSelectionUI.cs:237` — `Physics2D.Raycast(worldPos, Vector2.zero)` 터치로 유닛 피킹. 레이어 마스크 미지정 → 모든 레이어에 히트 가능 (의도일 수 있으나, 필요 시 레이어 필터 권장).

### 4.2 픽셀 퍼펙트 vs 물리

- 픽셀 퍼펙트와 물리를 섞는 안티패턴 없음. 그리드/월드 좌표 기반으로만 동작.

---

## 5. 카메라

### 5.1 타입

- **고정 Orthographic**: 플레이어 추적/룸 전환 없음. `GridManager.InitializeGrid()`에서 `Camera.main.orthographicSize`·aspect로 그리드 영역 계산.

### 5.2 스크린 셰이크 (안티패턴)

- **위치**: `VFXManager.cs` — 보스 경고(~560–622), 배너(~771–797), 레전더리 연출(~1050–1130).
- **지속 시간**: 1.0s, 1.2s 등. 스킬 권장 **50–200ms**보다 훨씬 김 → 시각적 피로·멀미 가능성.
- **강도**: 0.05f ~ 0.15f. 감쇠 `(1f - t * 0.5f)` 적용 → 방향은 적절.
- **구현**: 매 프레임 `mainCamera.transform.position = originalCamPos + offset`. 별도 스무딩 없음. 복수 VFX 동시 재생 시 카메라가 한 프레임에 여러 번 덮어쓸 수 있음 → **잠재적 지터**.

---

## 6. 장르 패턴 (타워 디펜스)

- **플랫포머 패턴(코요테 타임, 점프 버퍼 등)**: 해당 없음.
- **톱다운**: 그리드 배치, 경로 기반 몬스터 이동, 유닛 타겟팅/스킬. 이동·조준은 “유닛 배치 + 자동 타겟”으로 처리되어 별도 조준/회전 패턴은 제한적.

---

## 7. 리소스 누수 및 정리

### 7.1 Texture2D / Sprite

| 위치 | 내용 | 위험도 |
|------|------|--------|
| `UnitData.CreateCircleSprite` | 호출마다 새 Texture2D, 캐시 없음 | High |
| `GridManager.CreateSquareSprite()` | prefab 없을 때 셀마다 새 Texture2D(64×64). 8×4 = 32개, 해제 없음 | High |
| `Monster.CreateDefaultSprite()` | 몬스터 인스턴스마다 새 Texture2D(32×32) | High |
| `Monster.CreateBarSprite()` | 몬스터당 2회 (HP bar bg/fill). Texture2D(1×1) | Medium |
| `MonsterPool` placeholder | 풀 오브젝트당 새 Texture2D(32×32) | High |
| `VFXManager` ring Texture2D | BossSpawnRing, 레전더리 링 등 `new Texture2D(64,64)` 후 Sprite.Create, ring Destroy 시 Texture 미해제 | Medium |

### 7.2 Material

- **LineRenderer에 `new Material(Shader.Find("Sprites/Default"))`** 할당 후, GameObject만 `Destroy`하는 경우 Unity가 Material을 자동 파괴하지 않음 → **Material 누수**.
  - `Unit.cs`: 650, 678, 711, 1055 — SplashEffect, ChainEffect, MissileEffect, range indicator border.
  - `UnitPlacementManager.cs`: 723.
  - `GridManager.cs`: 703 — DrawSquareLoopPath.
- **권장**: 공용 Material 한 번 생성해 재사용하거나, 오브젝트 파괴 전 `Destroy(lr.material)` 호출.

---

## 8. 기타 이슈

### 8.1 네이밍/일관성

- `GameSpriteLoader` vs `UnitData.CreateCircleSprite` vs `Unit.GetOrCreateCircleSprite`: “원형 스프라이트 생성”이 세 곳에 흩어져 있고, 캐시 유무·사이즈도 제각각.

### 8.2 Dead code / 과한 로그

- `GridManager.cs:59`: `// [SerializeField] private float borderWidth` 주석 처리된 필드 제거 권장 (이미 제거된 것으로 보임).
- `GridManager` Awake/Start에 `Debug.Log` 다수 — 릴리즈 시 제거 또는 조건부 컴파일 권장.

### 8.3 과한 엔지니어링

- 없음. 그리드/유닛/몬스터/VFX 구조는 단순한 편.

---

## 9. 요약 및 우선 조치

| 우선순위 | 항목 | 파일:라인 | 조치 |
|----------|------|-----------|------|
| Critical | Texture2D/Sprite 누수 | UnitData.cs:170–189, GridManager.cs:274–298, Monster.cs:302–349, 446–451, MonsterPool | CreateCircleSprite 등 size별 캐시; Grid/몬스터/풀은 공용 스프라이트 또는 명시적 Destroy |
| Critical | Material 누수 (LineRenderer) | Unit.cs 650,678,711,1055; UnitPlacementManager 723; GridManager 703 | 공용 Material 사용 또는 Destroy(lineObj) 전 Destroy(lr.material) |
| High | 스크린 셰이크 지속 시간 | VFXManager.cs 576–606, 776–797, 1066–1085 | 50–200ms 수준으로 단축, 강도 유지 또는 소폭 상향 |
| Medium | 스프라이트 아틀라스 미사용 | GameSpriteLoader.cs | Sprite Atlas 도입 검토 (에셋 파이프라인 변경 필요) |
| Medium | VFX 링 Texture2D 정리 | VFXManager.cs 645, 900, 1149 | 링용 Texture/Sprite 캐시 또는 Destroy(ring) 시 sprite/texture 명시 정리 |
| Low | SortingLayer 미사용 | 전역 | Background/Terrain/Props/Foreground 등 SortingLayer로 정리 검토 |
| Low | Physics2D 레이어 필터 | UnitSelectionUI.cs:237 | 필요 시 Raycast에 layerMask 추가 |

---

*분석일: 2025-02-23. Lotto-defense 코드베이스 및 2d-games SKILL.md 기준.*
