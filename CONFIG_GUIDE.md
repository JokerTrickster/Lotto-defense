# Lotto Defense - Config 설정 가이드

게임 밸런스를 조정하고 라운드를 설정하는 방법을 설명합니다.

---

## 📁 Config 파일 위치

모든 Config 에셋 파일은 `Assets/Resources/` 폴더에 위치해야 합니다:

```
Assets/Resources/
├── GameBalanceConfig.asset     ✅ 이미 생성됨 (업데이트됨)
├── RoundConfig.asset            ✅ 이미 생성됨
├── Units/                       (유닛 데이터)
│   ├── Archer.asset
│   ├── Warrior.asset
│   └── ...
└── Monsters/                    (몬스터 데이터)
    ├── SlimeMonster.asset
    ├── Goblin.asset
    └── ...
```

---

## 1️⃣ UnitData - 유닛별 스탯 및 업그레이드 설정

### 📍 경로
- **파일**: `Assets/Resources/Units/*.asset` (각 유닛별 에셋)
- **스크립트**: `Assets/Scripts/Units/UnitData.cs`

### 🎮 설정 가능한 항목

#### ✅ 기본 스탯
```yaml
attack: 10              # 기본 공격력
attackSpeed: 1.0        # 초당 공격 횟수
attackRange: 1.5        # 공격 사거리 (그리드 단위)
```

#### ✅ 공격 패턴 (NEW!)
```yaml
attackPattern: SingleTarget    # 공격 유형
  - SingleTarget: 단일 대상 공격 (기본)
  - Splash: 범위 공격 (주 대상 + 주변 적)
  - AOE: 광역 공격 (범위 내 모든 적)
  - Pierce: 관통 공격 (일직선상 적 관통)
  - Chain: 연쇄 공격 (적에서 적으로 튕김)

splashRadius: 1.5             # 스플래시/AOE 반경 (0 = 없음)
maxTargets: 3                 # 최대 타겟 수 (Pierce/Chain용, 0 = 무제한)
splashDamageFalloff: 50       # 범위 끝 데미지 비율 (%, 100 = 감쇠 없음)
```

**공격 패턴 예시:**

1. **단일 공격 궁수**
   ```yaml
   attackPattern: SingleTarget
   maxTargets: 1
   splashRadius: 0
   ```

2. **스플래시 공격 법사**
   ```yaml
   attackPattern: Splash
   splashRadius: 2.0           # 2.0 범위 내 추가 피해
   splashDamageFalloff: 50     # 범위 끝에서 50% 데미지
   ```

3. **광역 공격 포병**
   ```yaml
   attackPattern: AOE
   splashRadius: 3.0           # 3.0 범위 내 모든 적
   splashDamageFalloff: 30     # 범위 끝에서 30% 데미지
   ```

4. **관통 공격 저격수**
   ```yaml
   attackPattern: Pierce
   maxTargets: 5               # 최대 5명 관통
   attackRange: 5.0            # 긴 사거리
   ```

5. **연쇄 공격 번개 마법사**
   ```yaml
   attackPattern: Chain
   maxTargets: 4               # 4번 튕김
   splashRadius: 2.5           # 튕김 범위 2.5
   ```

#### ✅ 업그레이드 설정 (NEW!)
```yaml
baseUpgradeCost: 5               # 첫 업그레이드 기본 비용
attackUpgradePercent: 10         # 업그레이드당 공격력 증가율 (%)
attackSpeedUpgradePercent: 8     # 업그레이드당 공격속도 증가율 (%)
maxUpgradeLevel: 10              # 최대 업그레이드 레벨
```

**예시: Normal 등급 유닛**
- 기본 비용: 5 골드
- 레벨 1 업그레이드: 5 * (1 + 0 * 0.5) = 5 골드
- 레벨 2 업그레이드: 5 * (1 + 1 * 0.5) = 7 골드
- 레벨 3 업그레이드: 5 * (1 + 2 * 0.5) = 10 골드

**예시: Legendary 등급 유닛**
- 기본 비용: 50 골드로 설정하면
- 레벨 1: 50 골드
- 레벨 2: 75 골드
- 레벨 3: 100 골드

### 🔧 Unity 에디터에서 수정하는 방법

1. `Assets/Resources/Units/` 폴더에서 유닛 에셋 선택 (예: Archer.asset)
2. Inspector 창에서 수정:
   - **Combat Stats**: 공격력, 공격속도, 사거리
   - **Upgrade Settings**: 업그레이드 비용, 증가율, 최대 레벨
3. Ctrl+S (Cmd+S) 저장

---

## 2️⃣ GameBalanceConfig - 게임 밸런스 설정

### 📍 경로
- **파일**: `Assets/Resources/GameBalanceConfig.asset`
- **스크립트**: `Assets/Scripts/Gameplay/GameBalanceConfig.cs`

### 🎮 설정 가능한 항목

#### ✅ 유닛 판매 (이미 설정됨)
```yaml
unitSellGold: 3  # 유닛 판매 시 획득하는 골드
```

#### ✅ 유닛 조합 레시피 (이미 설정됨)
```yaml
synthesisRecipes:
  - sourceUnitName: "기본 궁수"       # 재료 유닛 이름
    resultUnitName: "강화 궁수"       # 결과 유닛 이름
    synthesisGoldCost: 0              # 조합 비용 (골드)
```

**현재 설정된 조합 체인:**
```
Normal (0성) → Rare (1성)
├─ 기본 궁수 x3 → 강화 궁수
└─ 검사 x3 → 마법사

Rare (1성) → Epic (2성)
├─ 강화 궁수 x3 → 저격수
└─ 마법사 x3 → 대마법사

Epic (2성) → Legendary (3성)
├─ 저격수 x3 → 드래곤 아처
└─ 대마법사 x3 → 대현자
```

### 🔧 Unity 에디터에서 수정하는 방법

1. Unity 에디터에서 `Assets/Resources/GameBalanceConfig.asset` 선택
2. Inspector 창에서 수정:
   - **유닛 판매 골드**: `Unit Sell Gold` 값 변경
   - **조합 레시피 추가**:
     1. `Synthesis Recipes` 펼치기
     2. `+` 버튼 클릭
     3. 재료/결과 유닛 이름 입력
     4. 조합 비용 설정
3. Ctrl+S (Cmd+S) 저장

### 💻 코드에서 사용하는 방법

```csharp
// 1. 자동 로드 (UnitSelectionUI, SynthesisManager에서 이미 사용 중)
balanceConfig = Resources.Load<GameBalanceConfig>("GameBalanceConfig");

// 2. 조합 레시피 확인
var recipe = balanceConfig.GetSynthesisRecipe("기본 궁수");
if (recipe != null) {
    Debug.Log($"{recipe.sourceUnitName} x3 → {recipe.resultUnitName}");
}

// 3. 판매 골드 확인
int sellGold = balanceConfig.unitSellGold; // 3
```

---

## 3️⃣ 조합 가이드 UI (NEW!)

### 📍 기능
- 게임 중 **책 모양 버튼**(왼쪽 하단)을 클릭하면 조합 가이드 열람
- 페이지를 넘기며 모든 조합 레시피 확인 가능
- 각 페이지에 표시되는 정보:
  - 소스 유닛 (3개 필요)
  - 결과 유닛
  - 각 유닛의 스탯 (공격력, 공격속도, 사거리, DPS)
  - 조합 비용

### 🎮 사용 방법
1. 게임 플레이 중 왼쪽 하단의 📖 버튼 클릭
2. ◀/▶ 버튼으로 페이지 넘기기
3. X 버튼으로 닫기

---

## 4️⃣ RoundConfig - 라운드별 몬스터 설정

### 📍 경로
- **파일**: `Assets/Resources/RoundConfig.asset`
- **스크립트**: `Assets/Scripts/Gameplay/RoundConfig.cs`

### 🎮 설정 가능한 항목

```yaml
totalRounds: 30  # 총 라운드 수

roundConfigs:    # 각 라운드 설정
  - roundNumber: 1               # 라운드 번호
    monsterData: SlimeMonster    # 이 라운드에 나올 몬스터
    totalMonsters: 10            # 스폰할 총 몬스터 수
    spawnInterval: 1.0           # 스폰 간격 (초)
    spawnDuration: 10.0          # 스폰 지속 시간 (초)
```

**현재 설정된 라운드 진행:**
```
Round 1-2:  SlimeMonster (슬라임)    - 10~15마리
Round 3-4:  Goblin (고블린)          - 20~25마리
Round 5-6:  SpeedDemon (빠른 악마)   - 30마리
Round 7-9:  ArmoredOgre (방어 오우거) - 30마리
Round 10+:  DragonBoss (드래곤 보스)  - 30마리 (라운드 11~30은 기본값 사용)
```

### 🔧 Unity 에디터에서 수정하는 방법

#### 방법 1: Inspector에서 직접 수정
1. `Assets/Resources/RoundConfig.asset` 선택
2. Inspector에서 수정:
   - `Total Rounds`: 총 라운드 수
   - `Round Configs` 펼치기
   - 각 라운드별로:
     - Round Number: 라운드 번호
     - Monster Data: 드래그 앤 드롭으로 몬스터 선택
     - Total Monsters: 스폰할 수
     - Spawn Interval: 스폰 간격
     - Spawn Duration: 스폰 지속 시간

#### 방법 2: Context Menu 사용
1. RoundConfig 에셋 우클릭
2. `Auto-Generate Round Configs` 선택 → 기본 설정으로 30라운드 자동 생성
3. `Sort Rounds by Number` 선택 → 라운드 번호 순으로 정렬

### 📋 Manager에 연결하는 방법

RoundConfig를 사용하려면 Manager 오브젝트에 연결해야 합니다:

1. **Hierarchy**에서 `RoundManager` 오브젝트 선택
2. **Inspector**에서 `Round Config` 필드에 `RoundConfig.asset` 드래그 앤 드롭
3. **Hierarchy**에서 `MonsterManager` 오브젝트 선택
4. **Inspector**에서 `Round Config` 필드에 `RoundConfig.asset` 드래그 앤 드롭

### 💻 코드에서 사용하는 방법

```csharp
// 1. RoundManager/MonsterManager의 Inspector에서 할당
[SerializeField] private RoundConfig roundConfig;

// 2. 특정 라운드 설정 가져오기
RoundMonsterConfig config = roundConfig.GetRoundConfig(5); // 5라운드
Debug.Log($"Round 5: {config.monsterData.monsterName}");

// 3. 스폰 설정 확인
int count = config.totalMonsters;      // 30
float interval = config.spawnInterval; // 0.5
float duration = config.spawnDuration; // 15

// 4. 총 라운드 수 확인
int maxRounds = roundConfig.TotalRounds; // 30
```

---

## 5️⃣ 실제 동작 예시

### ✅ 유닛 조합 시나리오

1. 플레이어가 "기본 궁수" 3개를 그리드에 배치
2. 준비 페이즈에서 "기본 궁수" 중 하나 클릭
3. UI에 "조합 → 강화 궁수" 버튼 활성화
4. 버튼 클릭 → SynthesisManager가 작동:
   - "기본 궁수" 3개 제거
   - "강화 궁수" 1개 생성 (랜덤 빈 칸에 배치)
   - 조합 비용 차감 (현재 0골드)

### ✅ 라운드 진행 시나리오

1. **Round 1 시작** (RoundConfig 확인)
   - MonsterManager: "SlimeMonster 10마리, 1초 간격, 10초 동안"
   - 10초 동안 슬라임 10마리 스폰
   - 모두 처치 → Round 2로 진행

2. **Round 5 시작**
   - RoundConfig: SpeedDemon 설정 확인
   - 15초 동안 30마리 스폰 (0.5초 간격)
   - 더 빠르고 강한 몬스터 등장

3. **Round 11+ (설정 없음)**
   - RoundConfig에 11라운드 설정이 없음
   - defaultMonster (SlimeMonster) 사용
   - defaultTotalMonsters (30마리) 사용
   - 자동으로 fallback 동작

---

## 🔍 동작 확인 방법

### 1. 컴파일 에러 없는지 확인
```bash
# Unity 에디터에서 Console 확인
0 errors, 0 warnings ✅
```

### 2. 로딩 확인
```csharp
// Unity 실행 시 Console 로그 확인
[UnitSelectionUI] GameBalanceConfig loaded ✅
[MonsterManager] Round 1 from config: SlimeMonster (x10, 1s interval) ✅
[RoundManager] Round 1/30 ✅
```

### 3. 실제 플레이 테스트
1. GameScene 실행
2. 준비 페이즈에서 유닛 배치
3. 같은 유닛 3개 → 조합 버튼 활성화 확인
4. 전투 페이즈 시작 → RoundConfig에 설정한 몬스터 스폰 확인
5. 라운드 진행 → 설정한 대로 몬스터 변경 확인

---

## ⚙️ 고급 설정

### 라운드별 다른 난이도 설정

```yaml
# Round 1-5: Easy (슬라임, 고블린)
- roundNumber: 1
  totalMonsters: 10
  spawnInterval: 1.0

# Round 6-10: Medium (빠른 악마, 오우거)
- roundNumber: 6
  totalMonsters: 20
  spawnInterval: 0.7

# Round 11+: Hard (드래곤 보스)
- roundNumber: 11
  totalMonsters: 30
  spawnInterval: 0.5
```

### 보스 라운드 설정

```yaml
# Round 10, 20, 30: 보스 등장
- roundNumber: 10
  monsterData: DragonBoss
  totalMonsters: 1      # 보스 1마리만
  spawnInterval: 0
  spawnDuration: 0.1

- roundNumber: 20
  monsterData: DragonBoss
  totalMonsters: 2      # 보스 2마리
  spawnInterval: 5      # 5초 간격
  spawnDuration: 10
```

### 조합 비용 추가

```yaml
synthesisRecipes:
  - sourceUnitName: "드래곤 아처"
    resultUnitName: "궁극 드래곤"
    synthesisGoldCost: 50  # 조합 시 50골드 필요
```

---

## 🚨 주의사항

### ❌ 하면 안 되는 것

1. **Resources 폴더 밖에 Config 파일 생성**
   - `Resources.Load()`는 Resources 폴더만 검색합니다

2. **유닛/몬스터 이름 오타**
   - 조합 레시피의 이름이 실제 UnitData 이름과 정확히 일치해야 합니다

3. **RoundConfig를 Manager에 연결하지 않음**
   - Inspector에서 드래그 앤 드롭으로 연결 필수

4. **라운드 번호 중복**
   - 같은 라운드 번호를 2번 설정하면 경고 발생

### ✅ 권장 사항

1. **설정 후 Unity 재시작**
   - Config 변경 후 Play 모드 재시작 권장

2. **Git 커밋**
   - Config 에셋 파일도 버전 관리에 포함

3. **백업**
   - 중요한 밸런스 설정은 별도 백업 권장

---

## 📚 참고 파일

- **GameBalanceConfig.cs**: `Assets/Scripts/Gameplay/GameBalanceConfig.cs`
- **RoundConfig.cs**: `Assets/Scripts/Gameplay/RoundConfig.cs`
- **SynthesisManager.cs**: `Assets/Scripts/Units/SynthesisManager.cs`
- **MonsterManager.cs**: `Assets/Scripts/Monsters/MonsterManager.cs`
- **RoundManager.cs**: `Assets/Scripts/Gameplay/RoundManager.cs`

---

## ✅ 완료!

이제 Unity 에디터에서 Config 파일을 열고 원하는 대로 수정하면 됩니다.
모든 설정은 실시간으로 반영되며, 코드 수정 없이 밸런스 조정이 가능합니다! 🎮
