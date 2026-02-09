# 게임 밸런스 수정 가이드

## 📋 개요

모든 게임 밸런스 값이 **하나의 파일**에 통합되었습니다!

더 이상 여러 ScriptableObject 에셋을 찾아다니지 않아도 됩니다.

## 🎯 중앙 설정 파일 위치

```
Assets/Resources/GameBalanceConfig.asset
```

Unity 에디터에서 이 파일을 선택하면 **Inspector**에서 모든 값을 수정할 수 있습니다.

## 📝 수정 가능한 항목

### 1️⃣ **유닛 밸런스** (Units)

각 유닛의 스탯을 개별적으로 설정할 수 있습니다.

#### 설정 항목:
- **Unit Name**: 유닛 이름
- **Rarity**: 등급 (Normal=0, Rare=1, Epic=2, Legendary=3)
- **Attack**: 기본 공격력
- **Attack Speed**: 초당 공격 횟수 (1.0 = 1초에 1번)
- **Attack Range**: 공격 사거리 (유닛 단위)
- **Upgrade Cost**: 업그레이드 비용 (골드)

#### 예시:
```
기본 궁수:
  - Rarity: Normal
  - Attack: 10
  - Attack Speed: 1.0 (1초당 1번 공격)
  - Attack Range: 3.0
  - Upgrade Cost: 5

드래곤 아처:
  - Rarity: Legendary
  - Attack: 100
  - Attack Speed: 1.5 (1초당 1.5번 공격)
  - Attack Range: 7.0
  - Upgrade Cost: 50
```

---

### 2️⃣ **몬스터 밸런스** (Monsters)

3가지 몬스터 타입의 스탯을 설정합니다.

#### 설정 항목:
- **Monster Name**: 몬스터 이름
- **Type**: 타입 (Normal=0, Fast=1, Tank=2)
- **Max Health**: 기본 최대 체력
- **Attack**: 공격력 (현재 미사용)
- **Defense**: 방어력 (피해 감소)
- **Move Speed**: 이동 속도 (유닛/초)
- **Gold Reward**: 처치 시 획득 골드
- **Health Scaling**: 라운드당 체력 증가율 (1.1 = 10% 증가)
- **Defense Scaling**: 라운드당 방어력 증가율 (1.05 = 5% 증가)

#### 예시:
```
기본 몬스터:
  - Max Health: 100
  - Defense: 5
  - Move Speed: 2.0
  - Gold Reward: 10
  - Health Scaling: 1.1 (라운드마다 10% 증가)

탱크 몬스터:
  - Max Health: 200
  - Defense: 10
  - Move Speed: 1.5
  - Gold Reward: 15
  - Health Scaling: 1.12 (라운드마다 12% 증가)
```

---

### 3️⃣ **난이도 설정** (Difficulty)

라운드별 스케일링을 설정합니다.

#### 설정 항목:
- **Max Rounds**: 최대 라운드 수 (기본: 5)
- **Health Scaling**: 라운드별 체력 배율 커브
  - X축: 0~1 (정규화된 라운드 진행도)
  - Y축: 배율 (1 = 1배, 5 = 5배)
- **Defense Scaling**: 라운드별 방어력 배율 커브

#### 예시:
```
Max Rounds: 5

Health Scaling (커브):
  - Round 1 (0.0): 1.0배
  - Round 5 (1.0): 5.0배
  → 선형 증가 (1라운드 100 HP → 5라운드 500 HP)

Defense Scaling (커브):
  - Round 1 (0.0): 1.0배
  - Round 5 (1.0): 3.0배
  → 선형 증가 (1라운드 5 방어 → 5라운드 15 방어)
```

---

### 4️⃣ **소환 확률** (Spawn Rates)

유닛 등급별 소환 확률을 설정합니다. **합계는 반드시 100%**여야 합니다.

#### 설정 항목:
- **Normal Rate**: 일반 등급 확률 (%)
- **Rare Rate**: 희귀 등급 확률 (%)
- **Epic Rate**: 영웅 등급 확률 (%)
- **Legendary Rate**: 전설 등급 확률 (%)

#### 예시:
```
현재 설정 (테스트 모드):
  - Normal: 25%
  - Rare: 25%
  - Epic: 25%
  - Legendary: 25%
  → 합계: 100% ✓

실제 게임 권장:
  - Normal: 50%
  - Rare: 30%
  - Epic: 15%
  - Legendary: 5%
  → 합계: 100% ✓
```

**⚠️ 주의**: 합계가 100%가 아니면 Unity Console에 경고가 표시됩니다!

---

### 5️⃣ **게임 룰** (Game Rules)

게임 플레이 타이밍과 경제를 설정합니다.

#### 설정 항목:
- **Preparation Time**: 준비 시간 (초) - 기본: 15초
- **Combat Time**: 전투 시간 (초) - 기본: 30초
- **Starting Gold**: 시작 골드 - 기본: 30
- **Summon Cost**: 유닛 소환 비용 - 기본: 5 골드
- **Max Monster Count**: 게임 오버 몬스터 수 - 기본: 100마리
- **Spawn Rate**: 스폰 속도 (초당 몬스터 수) - 기본: 2.0 (초당 2마리)

#### 예시:
```
준비 시간: 15초
  → 플레이어가 유닛을 배치할 수 있는 시간

전투 시간: 30초
  → 몬스터가 스폰되고 유닛이 공격하는 시간

시작 골드: 30
소환 비용: 5
  → 게임 시작 시 유닛을 6번 뽑을 수 있음

게임 오버 조건: 100마리
  → 100마리가 끝까지 도달하면 게임 오버

스폰 속도: 2.0
  → 1초에 2마리씩 스폰 (0.5초마다 1마리)
```

---

## 🔧 수정 방법

### Unity 에디터에서 수정:

1. **Project 탭**에서 `Assets/Resources/GameBalanceConfig` 검색
2. 파일 선택
3. **Inspector 탭**에서 원하는 값 수정
4. **Ctrl+S** 또는 **Cmd+S**로 저장
5. 플레이 모드로 테스트

### 코드에서 접근:

```csharp
// GameBalanceConfig 로드
GameBalanceConfig config = Resources.Load<GameBalanceConfig>("GameBalanceConfig");

// 유닛 스탯 확인
foreach (var unit in config.units)
{
    Debug.Log($"{unit.unitName}: ATK {unit.attack}");
}

// 확률 확인
Debug.Log($"Legendary Rate: {config.spawnRates.legendaryRate}%");

// 게임 룰 확인
Debug.Log($"Summon Cost: {config.gameRules.summonCost} Gold");
```

---

## 📊 밸런스 조정 팁

### 유닛 공격력 조정:
- **너무 약함** → 공격력 10~20% 증가
- **너무 강함** → 공격력 10~20% 감소
- 공격 속도는 0.5~2.0 사이가 적당

### 몬스터 체력 조정:
- **너무 빨리 죽음** → 체력 20~50% 증가
- **너무 안 죽음** → 체력 20~50% 감소
- Health Scaling은 1.05~1.15 사이 권장

### 확률 조정:
- **전설 유닛이 너무 많이 나옴** → Legendary Rate 낮추기 (5% 권장)
- **일반 유닛만 나옴** → Normal Rate 낮추고 Rare/Epic 올리기

### 난이도 조정:
- **너무 쉬움** → 준비 시간 줄이기 / 몬스터 체력 증가
- **너무 어려움** → 준비 시간 늘리기 / 시작 골드 증가

---

## ✅ 체크리스트

밸런스 수정 후 확인할 사항:

- [ ] 소환 확률 합계가 100%인가?
- [ ] 유닛 공격력이 0보다 큰가?
- [ ] 몬스터 체력이 0보다 큰가?
- [ ] 게임 룰 값들이 양수인가?
- [ ] 플레이 테스트로 재미있는가?

---

## 🎮 실시간 리로드

Unity 에디터에서 값을 수정하면:
- **플레이 모드 재시작** 시 자동으로 새 값 적용
- **씬 리로드** 필요 없음
- **빌드 필요 없음** (에디터 내에서)

---

## 📌 주의사항

1. **저장 필수**: 수정 후 반드시 저장! (Ctrl+S / Cmd+S)
2. **백업 권장**: 큰 변경 전에 파일 백업
3. **Git 커밋**: 밸런스 변경 후 커밋으로 기록
4. **플레이 테스트**: 항상 직접 플레이해서 확인!

---

## 🔄 기존 에셋 제거

이제 다음 파일들은 사용하지 않습니다:

- ~~`Assets/Resources/DifficultyConfig.asset`~~ (삭제됨)
- ~~`Assets/Resources/Units/*.asset`~~ (필요 시 유지 가능, 무시됨)
- ~~`Assets/Resources/Monsters/*.asset`~~ (필요 시 유지 가능, 무시됨)

모든 밸런스는 **GameBalanceConfig.asset**에서만 관리됩니다!

---

## 💡 문제 해결

### Q: 설정 파일이 로드 안 됨
**A**: `Assets/Resources/GameBalanceConfig.asset` 파일이 존재하는지 확인

### Q: 확률이 제대로 작동 안 함
**A**: 확률 합계가 정확히 100%인지 확인 (Inspector에서 경고 메시지 확인)

### Q: 변경사항이 적용 안 됨
**A**: Unity 에디터를 재시작하거나 씬을 리로드

---

**🎯 이제 한 파일만 열면 모든 밸런스를 조정할 수 있습니다!**
