# 📊 Lotto-Defense 데이터 연동 가이드 (구글 시트/노션)

**목표:** 구글 시트나 노션에서 게임 밸런스 데이터를 관리하고, CSV/JSON으로 export해서 Unity에 자동 적용!

---

## 🎯 **왜 필요한가?**

### **문제:**
- Unity 에디터에서 일일이 수정하기 번거로움
- 여러 사람이 동시에 밸런스 조정 어려움
- 변경 이력 추적 힘듦
- 모바일에서 수정 불가능

### **해결:**
- ✅ 구글 시트/노션에서 편하게 편집
- ✅ 여러 명이 동시에 작업 가능
- ✅ 변경 이력 자동 저장
- ✅ 모바일에서도 수정 가능
- ✅ CSV/JSON export → Unity 자동 import

---

## 📋 **구글 시트 템플릿**

### **1. 유닛 데이터 시트**

**시트 이름:** `Units`

| unitName | rarity | attack | attackSpeed | attackRange | attackPattern | upgradeCost | skills |
|----------|--------|--------|-------------|-------------|---------------|-------------|--------|
| Warrior | Normal | 10 | 1.0 | 1.5 | SingleTarget | 5 | battle_frenzy,critical_strike,war_cry |
| Archer | Rare | 15 | 1.2 | 3.0 | Pierce | 10 | double_shot,sniper,arrow_rain |
| Mage | Epic | 25 | 0.8 | 4.0 | AOE | 20 | area_attack,chain_lightning,meteor |
| Dragon Knight | Legendary | 40 | 0.7 | 2.0 | Splash | 50 | berserker,area_attack,rapid_fire,dragon_fury |
| Phoenix | Legendary | 50 | 0.6 | 5.0 | Chain | 60 | area_attack,chain_lightning,critical_strike,phoenix_flame |

**컬럼 설명:**
- `unitName`: 유닛 이름 (영어)
- `rarity`: Normal, Rare, Epic, Legendary
- `attack`: 공격력 (정수)
- `attackSpeed`: 공격속도 (초당 횟수)
- `attackRange`: 사거리 (유닛 단위)
- `attackPattern`: SingleTarget, Splash, AOE, Pierce, Chain
- `upgradeCost`: 업그레이드 비용 (골드)
- `skills`: 스킬 ID 목록 (콤마로 구분)

---

### **2. 스킬 데이터 시트**

**시트 이름:** `Skills`

| skillId | skillName | skillType | cooldownDuration | damageMultiplier | attackSpeedMultiplier | effectDuration | slowMultiplier | freezeDuration |
|---------|-----------|-----------|------------------|------------------|-----------------------|----------------|----------------|----------------|
| war_cry | 전사의 함성 | Active | 10 | 2.0 | 1.0 | 3 | 0 | 0 |
| arrow_rain | 화살 비 | Active | 12 | 1.0 | 2.0 | 4 | 0.5 | 0 |
| meteor | 메테오 | Active | 15 | 3.0 | 1.0 | 5 | 0 | 2 |
| critical_strike | 크리티컬 | OnHit | 3 | 2.0 | 1.0 | 0 | 0 | 0 |
| sniper | 저격수 | Passive | 0 | 1.0 | 1.0 | 0 | 0 | 0 |

**컬럼 설명:**
- `skillId`: 스킬 고유 ID (유닛이 참조)
- `skillName`: 스킬 표시 이름 (한글 가능)
- `skillType`: Active, Passive, OnHit, OnKill
- `cooldownDuration`: 쿨다운 시간 (초)
- `damageMultiplier`: 데미지 배율 (1.5 = 150%)
- `attackSpeedMultiplier`: 공속 배율
- `effectDuration`: 효과 지속시간 (초)
- `slowMultiplier`: 슬로우 배율 (0.5 = 50% 느리게)
- `freezeDuration`: 동결 시간 (초)

---

### **3. 몬스터 데이터 시트**

**시트 이름:** `Monsters`

| monsterName | type | maxHealth | attack | defense | moveSpeed | goldReward | healthScaling | defenseScaling |
|-------------|------|-----------|--------|---------|-----------|------------|---------------|----------------|
| 기본 몬스터 | Normal | 100 | 10 | 5 | 2.0 | 10 | 1.1 | 1.05 |
| 빠른 몬스터 | Fast | 70 | 8 | 3 | 4.0 | 8 | 1.08 | 1.03 |
| 탱크 몬스터 | Tank | 200 | 15 | 10 | 1.5 | 15 | 1.12 | 1.06 |
| 드래곤 보스 | Boss | 500 | 30 | 20 | 1.0 | 50 | 1.15 | 1.1 |

---

### **4. 라운드 데이터 시트**

**시트 이름:** `Rounds`

| roundNumber | monsterName | totalMonsters | spawnInterval | spawnDuration |
|-------------|-------------|---------------|---------------|---------------|
| 1 | 기본 몬스터 | 30 | 0.5 | 15 |
| 2 | 기본 몬스터 | 40 | 0.4 | 15 |
| 3 | 빠른 몬스터 | 35 | 0.5 | 15 |
| 5 | 탱크 몬스터 | 20 | 0.6 | 15 |
| 10 | 드래곤 보스 | 1 | 0 | 1 |

---

### **5. 게임 설정 시트**

**시트 이름:** `GameSettings`

| setting | value |
|---------|-------|
| preparationTime | 15 |
| combatTime | 30 |
| startingGold | 30 |
| summonCost | 5 |
| maxMonsterCount | 100 |
| normalRate | 25 |
| rareRate | 25 |
| epicRate | 25 |
| legendaryRate | 25 |

---

## 🔗 **구글 시트 사용법**

### **1단계: 구글 시트 생성**

1. [Google Sheets](https://sheets.google.com) 접속
2. 새 스프레드시트 생성
3. 이름: "Lotto Defense 게임 밸런스"
4. 위의 템플릿대로 시트 5개 만들기:
   - `Units`
   - `Skills`
   - `Monsters`
   - `Rounds`
   - `GameSettings`

### **2단계: 데이터 입력**

각 시트에 위의 예시처럼 데이터 입력

### **3단계: CSV로 Export**

**각 시트별로:**
```
1. 시트 선택 (예: Units)
2. File → Download → Comma Separated Values (.csv)
3. 파일명: Units.csv
```

**모든 시트 export:**
```
Units.csv
Skills.csv
Monsters.csv
Rounds.csv
GameSettings.csv
```

### **4단계: Unity에 Import**

CSV 파일들을 Unity 프로젝트에 복사:
```
Assets/Data/CSV/
├── Units.csv
├── Skills.csv
├── Monsters.csv
├── Rounds.csv
└── GameSettings.csv
```

### **5단계: 자동 변환 스크립트 실행**

Unity 메뉴에서:
```
Tools → Lotto Defense → Import CSV Data
```

→ ScriptableObject 자동 생성!

---

## 📝 **노션 데이터베이스 사용법**

### **1단계: 노션 데이터베이스 생성**

1. 노션 페이지 생성
2. `/database` 입력 → Table 선택
3. 데이터베이스 이름: "유닛 데이터"

### **2단계: 컬럼 추가**

구글 시트와 동일한 컬럼 추가:
- Unit Name (Title)
- Rarity (Select: Normal, Rare, Epic, Legendary)
- Attack (Number)
- Attack Speed (Number)
- Attack Range (Number)
- Attack Pattern (Select: SingleTarget, Splash, AOE, Pierce, Chain)
- Upgrade Cost (Number)
- Skills (Multi-select)

### **3단계: 데이터 입력**

행(Row)을 추가하며 데이터 입력

### **4단계: CSV로 Export**

```
1. 데이터베이스 우측 상단 ⋮ (더보기)
2. Export
3. Format: CSV
4. Download
```

### **5단계: Unity에 Import**

구글 시트와 동일

---

## 🤖 **자동 Import 스크립트 (Unity)**

프로젝트에 자동 import 스크립트를 추가하겠습니다:

```csharp
// Assets/Editor/CSVImporter.cs
// Unity 메뉴: Tools → Lotto Defense → Import CSV Data

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class CSVImporter : EditorWindow
{
    [MenuItem("Tools/Lotto Defense/Import CSV Data")]
    public static void ImportCSV()
    {
        // Units.csv → GameBalanceConfig 업데이트
        // Skills.csv → GameBalanceConfig 업데이트
        // Monsters.csv → GameBalanceConfig 업데이트
        // Rounds.csv → RoundConfig 업데이트
        
        Debug.Log("✅ CSV 데이터 import 완료!");
    }
}
```

---

## 🔄 **워크플로우**

### **일상적인 밸런스 조정:**

```
1. 구글 시트 열기 (모바일/PC 어디서든)
   ↓
2. 수치 수정 (예: Warrior 공격력 10 → 15)
   ↓
3. File → Download → CSV
   ↓
4. Unity 프로젝트의 Assets/Data/CSV/에 덮어쓰기
   ↓
5. Unity 메뉴: Tools → Import CSV Data
   ↓
6. 플레이 모드로 테스트!
```

### **협업 워크플로우:**

```
기획자: 구글 시트에서 밸런스 조정
  ↓
개발자: CSV export → Unity import
  ↓
테스터: 게임 플레이 테스트
  ↓
피드백 → 구글 시트 수정 → 반복
```

---

## 📊 **구글 시트 템플릿 링크 (예정)**

완성되면 다음과 같은 템플릿을 공유할 수 있습니다:

```
[Lotto Defense 게임 밸런스 템플릿]
https://docs.google.com/spreadsheets/d/...

→ File → Make a copy로 복사해서 사용
```

---

## 🎯 **장점**

✅ **편의성**
- 모바일에서도 수정 가능
- Unity 없이도 밸런스 조정
- 여러 명이 동시에 작업

✅ **협업**
- 구글 시트 공유로 팀 작업
- 댓글로 의견 교환
- 변경 이력 자동 추적

✅ **백업**
- 구글/노션 클라우드에 자동 저장
- 버전 히스토리 확인 가능
- 언제든 이전 버전으로 복구

✅ **효율성**
- 대량 데이터 편집 쉬움
- Excel 함수 사용 가능
- 복사/붙여넣기로 빠른 작업

---

## 🚀 **다음 단계**

### **지금 할 수 있는 것:**
1. 구글 시트 템플릿 생성
2. 현재 데이터 수동 입력
3. CSV export → Unity에 복사

### **스크립트 개발 필요:**
1. CSV → ScriptableObject 자동 변환
2. Unity 메뉴 추가
3. 데이터 검증 시스템

**CSV Import 스크립트를 지금 만들어드릴까요?** 🤔

---

## 💡 **추천 구성**

**구글 시트:**
- ✅ 편집 간편
- ✅ 모바일 앱 좋음
- ✅ 공유 쉬움
- ❌ 복잡한 데이터 구조는 어려움

**노션:**
- ✅ 강력한 데이터베이스
- ✅ 관계형 데이터 지원
- ✅ 예쁜 UI
- ❌ CSV export 약간 번거로움

**추천:** 구글 시트로 시작! (간단하고 빠름)

---

**구글 시트 템플릿을 만들어드릴까요?** 📊✨
