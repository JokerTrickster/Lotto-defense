# 📊 구글 시트 연동 설정 가이드

**이제 구글 시트에서 게임 밸런스를 편집할 수 있습니다!** 🎉

---

## 🚀 **빠른 시작 (5분 설정)**

### **1단계: Unity에서 CSV Export**

```
1. Unity 프로젝트 열기
2. 메뉴: Tools → Lotto Defense → Export Data to CSV
3. Assets/Data/CSV/ 폴더에 5개 CSV 파일 생성됨:
   ✅ Units.csv
   ✅ Skills.csv
   ✅ Monsters.csv
   ✅ Rounds.csv
   ✅ GameSettings.csv
```

### **2단계: 구글 시트 생성**

```
1. https://sheets.google.com 접속
2. 새 스프레드시트 생성
3. 이름: "Lotto Defense 게임 밸런스"
```

### **3단계: CSV Import**

**각 CSV 파일마다:**

```
1. 구글 시트에서 새 시트 추가 (하단 + 버튼)
2. 시트 이름 변경:
   - Sheet1 → Units
   - Sheet2 → Skills
   - Sheet3 → Monsters
   - Sheet4 → Rounds
   - Sheet5 → GameSettings

3. 각 시트에 CSV 데이터 import:
   a. 시트 선택 (예: Units)
   b. File → Import
   c. Upload 탭 → Browse
   d. Units.csv 선택
   e. Import location: "Replace current sheet"
   f. Separator type: "Comma"
   g. "Import data" 클릭
   
4. Skills, Monsters, Rounds, GameSettings도 반복
```

### **4단계: 완료!** ✅

이제 구글 시트에서 자유롭게 편집하세요!

---

## 📝 **시트별 설명**

### **Units (유닛 데이터)**

| 컬럼 | 설명 | 예시 |
|------|------|------|
| unitName | 유닛 이름 (영어) | Warrior, Archer |
| rarity | 등급 | Normal, Rare, Epic, Legendary |
| attack | 공격력 | 10, 15, 25 |
| attackSpeed | 공격속도 (초당 횟수) | 1.0, 1.2 |
| attackRange | 사거리 (유닛 단위) | 1.5, 3.0 |
| attackPattern | 공격 패턴 | SingleTarget, Splash, AOE, Pierce, Chain |
| splashRadius | 스플래시 범위 (0 = 없음) | 0, 1.0, 1.5 |
| maxTargets | 최대 타겟 수 (Pierce/Chain용) | 1, 2, 4 |
| upgradeCost | 업그레이드 비용 | 5, 10, 20 |
| skillIds | 스킬 ID (;로 구분) | war_cry;critical_strike |

**예시 데이터:**
```
Warrior,Normal,10,1.0,1.5,SingleTarget,0,1,5,"battle_frenzy;critical_strike;war_cry"
```

---

### **Skills (스킬 밸런스)**

| 컬럼 | 설명 | 예시 |
|------|------|------|
| skillId | 스킬 고유 ID | war_cry, arrow_rain |
| skillName | 표시 이름 (한글 가능) | 전사의 함성, 화살 비 |
| skillType | 스킬 타입 | Active, Passive, OnHit, OnKill |
| cooldownDuration | 쿨다운 (초) | 10, 12, 0 |
| damageMultiplier | 데미지 배율 | 1.0, 2.0, 3.0 |
| rangeMultiplier | 사거리 배율 | 1.0, 1.5 |
| attackSpeedMultiplier | 공속 배율 | 1.0, 1.5, 2.0 |
| effectDuration | 효과 지속시간 (초) | 0, 3, 5 |
| targetCount | 영향받는 대상 수 | 0, 1, 3 |
| aoeRadius | AOE 범위 | 0, 1.5, 2.0 |
| slowMultiplier | 슬로우 배율 (0.5 = 50% 느림) | 0, 0.5 |
| freezeDuration | 동결 시간 (초) | 0, 2 |
| ccDuration | CC 지속시간 (초) | 0, 2, 3 |

**예시 데이터:**
```
war_cry,"전사의 함성",Active,10,2.0,1.0,1.0,3,0,0,0,0,0
```

---

### **Monsters (몬스터 스탯)**

| 컬럼 | 설명 | 예시 |
|------|------|------|
| monsterName | 몬스터 이름 | 기본 몬스터, 고블린 |
| type | 타입 | Normal, Fast, Tank, Boss |
| maxHealth | 최대 체력 | 100, 200, 500 |
| attack | 공격력 (미사용) | 10 |
| defense | 방어력 | 5, 10, 20 |
| moveSpeed | 이동속도 | 2.0, 4.0, 1.0 |
| goldReward | 처치 골드 | 10, 15, 50 |
| healthScaling | 라운드당 체력 증가율 | 1.1 (10% 증가) |
| defenseScaling | 라운드당 방어력 증가율 | 1.05 (5% 증가) |

**예시 데이터:**
```
"기본 몬스터",Normal,100,10,5,2.0,10,1.1,1.05
```

---

### **Rounds (라운드 설정)**

| 컬럼 | 설명 | 예시 |
|------|------|------|
| roundNumber | 라운드 번호 | 1, 2, 3, 10 |
| monsterName | 등장 몬스터 | 기본 몬스터, 드래곤 보스 |
| totalMonsters | 스폰 수 | 30, 40, 1 |
| spawnInterval | 스폰 간격 (초) | 0.5, 0.4, 0 |
| spawnDuration | 스폰 지속시간 (초) | 15, 1 |

**예시 데이터:**
```
1,"기본 몬스터",30,0.5,15
10,"드래곤 보스",1,0,1
```

---

### **GameSettings (게임 설정)**

| 설정 | 설명 | 기본값 |
|------|------|--------|
| preparationTime | 준비 시간 (초) | 15 |
| combatTime | 전투 시간 (초) | 30 |
| startingGold | 시작 골드 | 30 |
| summonCost | 소환 비용 | 5 |
| maxMonsterCount | 최대 몬스터 수 | 100 |
| spawnRate | 스폰 속도 | 2.0 |
| normalRate | Normal 확률 (%) | 25 |
| rareRate | Rare 확률 (%) | 25 |
| epicRate | Epic 확률 (%) | 25 |
| legendaryRate | Legendary 확률 (%) | 25 |
| sellGoldNormal | Normal 판매 골드 | 3 |
| sellGoldRare | Rare 판매 골드 | 8 |
| sellGoldEpic | Epic 판매 골드 | 20 |
| sellGoldLegendary | Legendary 판매 골드 | 50 |

**형식:**
```
setting,value
preparationTime,15
startingGold,30
```

---

## ✏️ **데이터 편집 방법**

### **예시 1: Warrior 공격력 증가**

```
1. Units 시트 열기
2. Warrior 행 찾기
3. attack 컬럼 (C열): 10 → 20 변경
4. 저장 (자동)
```

### **예시 2: 새 스킬 추가**

```
1. Skills 시트 열기
2. 맨 아래 새 행 추가
3. 데이터 입력:
   skillId: super_attack
   skillName: "슈퍼 어택"
   skillType: Active
   cooldownDuration: 8
   damageMultiplier: 5.0
   ...
4. Units 시트에서 skillIds에 추가:
   "war_cry;critical_strike;super_attack"
```

### **예시 3: 라운드 10 보스 강화**

```
1. Rounds 시트 열기
2. roundNumber 10 행 찾기
3. totalMonsters: 1 → 3 (보스 3마리)
4. Monsters 시트에서 드래곤 보스 체력 증가
```

---

## 🔄 **Unity로 다시 가져오기**

### **방법 1: 전체 시트 Export**

```
1. 구글 시트에서 File → Download → Comma Separated Values (.csv)
   ⚠️ 각 시트별로 따로 다운로드해야 함!
   
2. Units 시트 → Units.csv
3. Skills 시트 → Skills.csv
4. Monsters 시트 → Monsters.csv
5. Rounds 시트 → Rounds.csv
6. GameSettings 시트 → GameSettings.csv

7. CSV 파일들을 Unity 프로젝트에 복사:
   ~/project/Lotto-defense/Assets/Data/CSV/

8. Unity 메뉴: Tools → Lotto Defense → Import Data from CSV

9. "Yes, Import" 클릭

10. ✅ 완료! 게임에 즉시 반영됨
```

### **방법 2: 개별 시트 Export**

```
1. 편집한 시트만 선택 (예: Units)
2. File → Download → CSV
3. Assets/Data/CSV/Units.csv 덮어쓰기
4. Unity에서 Import
```

---

## 🎯 **워크플로우 예시**

### **일일 밸런스 조정:**

```
아침:
  구글 시트 열기 (모바일/PC)
  ↓
  밸런스 수정 (Warrior 공격력 +5)
  ↓
  File → Download → CSV
  ↓
점심:
  CSV를 Unity에 복사
  ↓
  Tools → Import Data from CSV
  ↓
  플레이 모드로 테스트
  ↓
오후:
  피드백 반영 → 구글 시트 재수정
  ↓
  반복
```

### **협업 워크플로우:**

```
기획자:
  구글 시트 공유 받음
  ↓
  밸런스 조정 (여러 명이 동시에)
  ↓
  댓글로 의견 교환
  ↓
개발자:
  CSV export
  ↓
  Unity import
  ↓
  빌드 & 배포
```

---

## ⚠️ **주의사항**

### **DO (해도 됨):**
✅ 숫자 값 변경 (공격력, 체력 등)
✅ 문자열 수정 (스킬 이름 등)
✅ 행 추가 (새 유닛, 스킬 등)
✅ 행 삭제 (불필요한 데이터)

### **DON'T (하지 말 것):**
❌ **컬럼 이름 변경 금지!** (import 실패)
❌ **컬럼 순서 변경 금지!**
❌ **Enum 값 오타** (Normal ✅, normal ❌)
❌ **skillIds를 ,로 구분** (;를 사용해야 함)
❌ **빈 셀 많이 만들기** (0 또는 기본값 입력)

### **Enum 값 (대소문자 정확히!):**

**Rarity:**
- Normal
- Rare
- Epic
- Legendary

**AttackPattern:**
- SingleTarget
- Splash
- AOE
- Pierce
- Chain

**SkillType:**
- Active
- Passive
- OnHit
- OnKill

**MonsterType:**
- Normal
- Fast
- Tank
- Boss

---

## 🐛 **문제 해결**

### **"Import 실패" 오류:**
→ 컬럼 이름과 순서가 정확한지 확인
→ Enum 값 대소문자 확인

### **"데이터가 반영 안 됨":**
→ Unity에서 플레이 모드 재시작
→ Assets → Reimport All

### **"CSV 파일을 찾을 수 없음":**
→ 파일 위치 확인: Assets/Data/CSV/
→ 파일명 확인 (대소문자 구분)

### **"Skill ID not found" 경고:**
→ Skills 시트에 해당 skillId가 있는지 확인
→ skillIds 컬럼에서 오타 확인

---

## 📱 **모바일에서 편집하기**

### **구글 시트 앱 (Android/iOS):**

```
1. Google Sheets 앱 설치
2. "Lotto Defense 게임 밸런스" 시트 열기
3. 편집 (탭으로 이동 가능)
4. 자동 저장됨
5. 나중에 PC에서 CSV export
```

**장점:**
- ✅ 어디서든 수정 가능
- ✅ 실시간 동기화
- ✅ 여러 명이 동시에 작업 가능

---

## 🎉 **완료!**

이제 구글 시트로 게임 밸런스를 편하게 관리할 수 있습니다!

**질문이나 문제가 있으면 말씀해주세요!** 🚀

---

## 📖 **추가 자료**

- `CONFIG_GUIDE.md` - Unity 에디터 직접 수정 가이드
- `DATA_SYNC_GUIDE.md` - 데이터 연동 상세 설명
- `IMAGE_GUIDE.md` - 이미지 규격 가이드
