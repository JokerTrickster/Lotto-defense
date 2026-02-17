# 🎮 Lotto-Defense Config 수정 가이드

**중요:** 이 게임은 **Config 기반 시스템**으로 설계되어 있습니다!  
**코드 수정 없이** Unity 에디터에서 `.asset` 파일만 수정하면 됩니다.

---

## 📁 Config 파일 위치

### 1️⃣  **라운드 & 몬스터 설정**
```
Assets/Resources/RoundConfig.asset
```
- 라운드 수
- 각 라운드별 몬스터 종류
- 스폰 수, 간격, 지속시간

### 2️⃣  **게임 밸런스 설정**
```
Assets/Data/GameBalanceConfig.asset
```
- 유닛 스탯 (공격력, 공속, 사거리)
- 스킬 밸런스 (쿨다운, 데미지 배율)
- 몬스터 스탯
- 소환 확률
- 합성 레시피
- 보상 설정

### 3️⃣  **유닛 데이터**
```
Assets/Resources/Units/
├── Warrior.asset
├── Archer.asset
├── Mage.asset
├── DragonKnight.asset
└── Phoenix.asset
```

### 4️⃣  **몬스터 데이터**
```
Assets/Resources/Monsters/
├── Goblin.asset
├── SlimeMonster.asset
├── ArmoredOgre.asset
├── SpeedDemon.asset
└── DragonBoss.asset
```

---

## ⚙️ Unity 에디터에서 수정하는 방법

### 📋 **1. 라운드 설정 변경**

1. Unity 프로젝트 열기
2. **Project 창** → `Assets/Resources/RoundConfig.asset` 클릭
3. **Inspector 창**에서 수정:

```
총 라운드 수 (Total Rounds): 30

Round Configs:
  - Round Number: 1
    Monster Data: Goblin ← 드래그 앤 드롭으로 변경 가능
    Total Monsters: 30
    Spawn Interval: 0.5
    Spawn Duration: 15
    
  - Round Number: 2
    Monster Data: SlimeMonster
    Total Monsters: 40
    ...
```

**우클릭 메뉴:**
- `Auto-Generate Round Configs` - 자동으로 라운드 생성
- `Sort Rounds by Number` - 라운드 번호 순으로 정렬

---

### 🎯 **2. 유닛 스탯 변경**

1. **Project 창** → `Assets/Data/GameBalanceConfig.asset` 클릭
2. **Inspector 창** → `Units` 섹션 펼치기
3. 원하는 유닛 선택 (예: Warrior)

```
Unit Name: Warrior
Rarity: Normal
Attack: 10          ← 공격력
Attack Speed: 1.0   ← 공격속도 (초당 공격 횟수)
Attack Range: 1.5   ← 사거리
Attack Pattern: SingleTarget  ← 공격 패턴
Upgrade Cost: 5     ← 업그레이드 비용
Skill Ids:          ← 스킬 ID 리스트
  - battle_frenzy
  - critical_strike
  - war_cry
```

**공격 패턴 종류:**
- `SingleTarget` - 단일 공격
- `Splash` - 범위 공격 (splashRadius 설정 필요)
- `AOE` - 광역 공격
- `Pierce` - 관통 공격 (maxTargets 설정 필요)
- `Chain` - 연쇄 공격

---

### ⚡ **3. 스킬 밸런스 변경**

**GameBalanceConfig.asset** → `Skill Presets` 섹션:

```
Skill Id: arrow_rain
Skill:
  Skill Name: 화살 비
  Skill Type: Active
  Cooldown Duration: 12       ← 쿨다운 (초)
  Damage Multiplier: 1.0      ← 데미지 배율
  Attack Speed Multiplier: 2.0 ← 공속 배율
  Effect Duration: 4          ← 효과 지속시간
  Slow Multiplier: 0.5        ← 슬로우 (0.5 = 50% 느리게)
  CC Duration: 3              ← CC 지속시간
```

**스킬 타입:**
- `Active` - 마나 충전 후 자동 발동
- `Passive` - 항상 적용
- `OnHit` - 공격 시 발동
- `OnKill` - 처치 시 발동

---

### 👾 **4. 몬스터 스탯 변경**

**GameBalanceConfig.asset** → `Monsters` 섹션:

```
Monster Name: 기본 몬스터
Type: Normal
Max Health: 100           ← 체력
Defense: 5                ← 방어력
Move Speed: 2.0           ← 이동속도
Gold Reward: 10           ← 처치 골드
Health Scaling: 1.1       ← 라운드당 체력 증가율 (10%)
Defense Scaling: 1.05     ← 라운드당 방어력 증가율 (5%)
```

---

### 🎲 **5. 소환 확률 변경**

**GameBalanceConfig.asset** → `Spawn Rates` 섹션:

```
Normal Rate: 25      ← Normal 등급 확률 (%)
Rare Rate: 25        ← Rare 등급 확률
Epic Rate: 25        ← Epic 등급 확률
Legendary Rate: 25   ← Legendary 등급 확률

※ 합계가 100%여야 함!
```

---

### 🔄 **6. 합성 레시피 변경**

**GameBalanceConfig.asset** → `Synthesis Recipes` 섹션:

```
Synthesis Recipes:
  - Source Unit Name: Warrior      ← 재료 유닛 (2개 필요)
    Result Unit Name: Archer       ← 결과 유닛
    Synthesis Gold Cost: 0         ← 합성 비용

  - Source Unit Name: Archer
    Result Unit Name: Mage
    Synthesis Gold Cost: 0
```

---

### 💰 **7. 보상 설정**

#### **게임 결과 보상**
**GameBalanceConfig.asset** → `Game Result Rewards`:

```
Min Round: 0
Max Round: 3
Gold Reward: 10   ← 라운드 0-3 도달 시 10골드

Min Round: 4
Max Round: 6
Gold Reward: 30   ← 라운드 4-6 도달 시 30골드
```

#### **일일 보상**
**GameBalanceConfig.asset** → `Daily Reward Stages`:

```
Required Clears: 1    ← 1회 클리어
Gold Reward: 50
Ticket Reward: 0

Required Clears: 3    ← 3회 클리어
Gold Reward: 0
Ticket Reward: 1      ← 입장권 1개
```

---

## 🎯 빠른 밸런스 조정 가이드

### **게임이 너무 쉬울 때:**
1. 몬스터 체력 증가 (`Max Health` ↑)
2. 몬스터 방어력 증가 (`Defense` ↑)
3. 유닛 공격력 감소 (`Attack` ↓)
4. 스킬 쿨다운 증가 (`Cooldown Duration` ↑)

### **게임이 너무 어려울 때:**
1. 유닛 공격력 증가 (`Attack` ↑)
2. 유닛 공격속도 증가 (`Attack Speed` ↑)
3. 스킬 데미지 배율 증가 (`Damage Multiplier` ↑)
4. 시작 골드 증가 (`Starting Gold` ↑)

### **특정 라운드 난이도 조정:**
1. `RoundConfig.asset` 열기
2. 해당 라운드의 `Total Monsters` 조정
3. `Spawn Interval` 조정 (짧을수록 빠르게 등장)

---

## 🆕 새 유닛/몬스터 추가하는 방법

### **새 유닛 추가:**

1. **유닛 데이터 생성**
   - Project 창 우클릭
   - `Create > Lotto Defense > Unit Data`
   - 이름 설정 (예: `Wizard.asset`)

2. **GameBalanceConfig에 추가**
   - `GameBalanceConfig.asset` 열기
   - `Units` 리스트에 새 항목 추가
   - 모든 스탯 설정

3. **스프라이트 추가** (선택)
   - `Assets/Resources/Sprites/Units/Wizard.png`

### **새 몬스터 추가:**

1. **몬스터 데이터 생성**
   - Project 창 우클릭
   - `Create > Lotto Defense > Monster Data`
   - 이름 설정

2. **GameBalanceConfig에 추가**
   - `Monsters` 리스트에 새 항목 추가

3. **RoundConfig에서 사용**
   - 원하는 라운드의 `Monster Data`에 드래그

---

## ⚠️ 주의사항

### ✅ **권장:**
- Unity 에디터에서 `.asset` 파일만 수정
- 수정 후 `Ctrl+S`로 저장
- 플레이 모드에서 바로 테스트
- Git으로 버전 관리

### ❌ **비추천:**
- 코드 직접 수정 (불필요함)
- 텍스트 에디터로 `.asset` 파일 수정
- 플레이 모드에서 값 수정 (저장 안 됨)

---

## 🚀 빠른 테스트 방법

1. Config 수정
2. Unity에서 `Ctrl+S` (저장)
3. 플레이 모드 시작 (Cmd+P 또는 ▶️)
4. 바로 게임에서 확인
5. 문제 있으면 다시 수정 → 반복

**실시간 적용됨!** 재시작 불필요!

---

## 📖 추가 정보

### **스킬 ID 목록 (재사용 가능)**

**Active (마나 충전 스킬):**
- `war_cry` - 전사의 함성 (공격력 2배, 3초)
- `arrow_rain` - 화살 비 (공속 2배 + 슬로우, 4초)
- `meteor` - 메테오 (공격력 3배 + 동결, 5초)
- `dragon_fury` - 용의 분노 (공격력 2.5배 + 공속 1.5배, 5초)
- `phoenix_flame` - 불사조의 불꽃 (공격력 3배 + 공속 2배, 6초)

**Passive (항상 적용):**
- `sniper` - 저격수 (사거리 +50%)
- `berserker` - 버서커 (공격력 +30%)
- `rapid_fire` - 속사 (공속 +50%)
- `area_attack` - 광역 공격 (범위 1.5)

**OnHit (공격 시):**
- `critical_strike` - 크리티컬 (2배 데미지)
- `double_shot` - 더블 샷 (추가 공격)
- `piercing_arrow` - 관통 화살 (2명 관통)

**OnKill (처치 시):**
- `chain_lightning` - 연쇄 번개 (주변 3명에게 50% 데미지)
- `battle_frenzy` - 전투 광기 (공속 +50%, 3초)
- `gold_rush` - 골드 러시 (+3골드)

---

## 🎓 예제: 강력한 커스텀 유닛 만들기

**GameBalanceConfig.asset** → Units에 추가:

```yaml
Unit Name: Super Mage
Rarity: Legendary
Attack: 100              # 엄청 강함
Attack Speed: 2.0        # 빠른 공격
Attack Range: 6.0        # 긴 사거리
Attack Pattern: AOE      # 광역 공격
Splash Radius: 3.0       # 넓은 범위
Upgrade Cost: 100
Skill Ids:
  - area_attack          # 광역 공격 패시브
  - rapid_fire           # 공속 증가 패시브
  - meteor               # 메테오 액티브
```

이렇게 하면 **코드 수정 없이** 새로운 강력한 유닛 완성! 🎉

---

## 📞 문제 해결

### "수정했는데 게임에 반영 안 돼요"
→ Unity에서 `Ctrl+S` 눌렀는지 확인

### "Asset 파일을 찾을 수 없어요"
→ Project 창에서 검색 (돋보기 아이콘)

### "합성이 안 돼요"
→ `Synthesis Recipes`에 레시피가 있는지 확인

### "새 스킬을 만들고 싶어요"
→ `Skill Presets`에 새 항목 추가 → `skillId` 부여 → 유닛의 `Skill Ids`에 추가

---

**✨ 코드 없이 게임 밸런스를 자유롭게 조정하세요! ✨**
