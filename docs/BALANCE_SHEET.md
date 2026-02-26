# Lotto Defense - 밸런스 시트

> 코드에서 추출한 모든 수치. 수정 시 해당 소스 파일도 함께 변경해야 함.

---

## 1. 유닛 스탯

> 소스: `Assets/Scripts/Gameplay/GameBalanceConfig.cs`, `Assets/Scripts/Units/UnitData.cs`

| 유닛 | 등급 | 공격력 | 방어 | 공속 | 사거리 | 스플래시 반경 | 최대 타겟 | 감쇠(%) | 공격 패턴 |
|------|------|--------|------|------|--------|-------------|----------|---------|-----------|
| Warrior | Normal | 10 | 5 | 1.0 | 1.5 | 0 | 1 | 50 | SingleTarget |
| Archer | Rare | 15 | 3 | 1.2 | 3.0 | 0 | 2 | 50 | Pierce |
| Mage | Epic | 25 | 2 | 0.8 | 4.0 | 1.5 | 5 | 50 | AOE |
| DragonKnight | Legendary | 40 | 10 | 0.7 | 2.0 | 1.5 | 3 | 50 | Splash |
| Phoenix | Legendary | 50 | 5 | 0.6 | 5.0 | 2.5 | 5 | 40 | Chain |

---

## 2. 스킬 프리셋

> 소스: `Assets/Scripts/Gameplay/GameBalanceConfig.cs`

| skillId | 타입 | 쿨다운 | 데미지 배율 | 사거리 배율 | 공속 배율 | 지속시간 | 타겟수 | AOE 반경 | 슬로우 | 프리즈 | CC |
|---------|------|--------|------------|-----------|----------|---------|--------|---------|--------|--------|-----|
| critical_strike | OnHit | 3 | 2.0 | - | - | - | - | - | - | - | - |
| double_shot | OnHit | 5 | 0.5 | - | - | - | 1 | - | - | - | - |
| piercing_arrow | OnHit | 4 | 1.0 | - | - | - | 2 | - | - | - | - |
| chain_lightning | OnKill | 2 | 0.5 | - | - | - | 3 | 2.0 | - | - | - |
| battle_frenzy | OnKill | 1 | - | - | 1.5 | 3 | - | - | - | - | - |
| gold_rush | OnKill | 0 | 1.0 | - | - | - | - | - | - | - | - |
| sniper | Passive | 0 | - | 1.5 | - | - | - | - | - | - | - |
| berserker | Passive | 0 | 1.3 | - | - | - | - | - | - | - | - |
| rapid_fire | Passive | 0 | - | - | 1.5 | - | - | - | - | - | - |
| area_attack | Passive | 0 | - | - | - | - | 5 | 1.5 | - | - | - |
| war_cry | Active | 10 | 2.0 | - | - | 3 | - | - | - | - | - |
| arrow_rain | Active | 12 | - | - | 2.0 | 4 | - | - | 0.5 | - | 3 |
| meteor | Active | 15 | 3.0 | - | - | 5 | - | - | - | 2 | 2 |
| dragon_fury | Active | 15 | 2.5 | - | 1.5 | 5 | - | - | - | - | - |
| phoenix_flame | Active | 12 | 3.0 | - | 2.0 | 6 | - | - | - | - | - |

---

## 3. 몬스터 스탯

> 소스: `Assets/Scripts/Gameplay/GameBalanceConfig.cs`, `Assets/Scripts/Monsters/MonsterManager.cs`

| 몬스터 | 타입 | HP | 공격 | 방어 | 속도 | 골드 | HP 스케일링 | 방어 스케일링 |
|--------|------|-----|------|------|------|------|-------------|--------------|
| 기본 몬스터 | Normal | 100 | 10 | 5 | 2.0 | 10 | ×1.1/R | ×1.05/R |
| 빠른 몬스터 | Fast | 70 | 8 | 3 | 4.0 | 8 | ×1.08/R | ×1.03/R |
| 탱크 몬스터 | Tank | 200 | 15 | 10 | 1.5 | 15 | ×1.12/R | ×1.06/R |

### 보스 (5라운드)

| 항목 | 배율 |
|------|------|
| HP | ×10 |
| 공격 | ×5 |
| 방어 | ×3 |
| 속도 | ×0.7 |
| 골드 | ×20 |

### 스케일링 공식
```
라운드별 HP = 기본HP × pow(HP스케일링, 라운드 - 1)
라운드별 방어 = 기본방어 × pow(방어스케일링, 라운드 - 1)
```

### DifficultyConfig 추가 스케일링 (AnimationCurve)
- HP: 라운드 0→1 에서 1배→5배
- 방어: 라운드 0→1 에서 1배→3배

---

## 4. 난이도 배율

> 소스: `Assets/Scripts/Gameplay/GameDifficulty.cs`

| 난이도 | HP | 방어 | 골드 |
|--------|-----|------|------|
| Normal | 1.0 | 1.0 | 1.0 |
| Hard | 1.5 | 1.3 | 1.5 |
| VeryHard | 2.0 | 1.5 | 2.0 |

---

## 5. 전투 공식

> 소스: `Assets/Scripts/Monsters/Monster.cs`, `Assets/Scripts/Units/Unit.cs`

| 공식 | 수식 |
|------|------|
| **데미지** | `max(1, 공격력 - 방어력)` |
| **스플래시/AOE 감쇠** | `lerp(1, 감쇠%/100, 거리/반경)` |
| **체인 데미지** | `데미지 × pow(0.8, 바운스횟수)` |
| **최종 공격력** | `기본 × 업그레이드배율 × 로비레벨배율 × 스킬버프` |

---

## 6. 업그레이드

> 소스: `Assets/Scripts/Units/UnitUpgradeManager.cs`, `Assets/Scripts/Units/UnitData.cs`

### 레벨당 증가율

| 유닛 | 공격력 증가/레벨 | 공속 증가/레벨 | 기본 업그레이드 비용 |
|------|-----------------|---------------|---------------------|
| Warrior | +10% | +8% | 5 |
| Archer | +10% | +8% | 10 |
| Mage | +10% | +8% | 20 |
| DragonKnight | +10% | +8% | 50 |
| Phoenix | +12% | +10% | 60 |

### 공식
```
업그레이드 비용 = 기본비용 × (1 + 현재레벨 × 0.5)
공격력 배율 = 1 + 레벨 × (공격증가%/100)
공속 배율 = 1 + 레벨 × (공속증가%/100)
```

### 로비 레벨 시스템

| 등급 | 기본 비용 |
|------|----------|
| Normal | 20 |
| Rare | 50 |
| Epic | 100 |
| Legendary | 200 |

```
로비 레벨업 비용 = 기본비용 × max(1, 현재레벨)
로비 공격력 배율 = 1 + 0.1 × (레벨 - 1)
로비 공속 배율 = 1 + 0.05 × (레벨 - 1)
```

---

## 7. 경제

> 소스: `Assets/Scripts/Gameplay/GameBalanceConfig.cs`

### 수입

| 항목 | 값 |
|------|-----|
| 시작 골드 | 30 |
| Normal 몬스터 처치 | 10골드 |
| Fast 몬스터 처치 | 8골드 |
| Tank 몬스터 처치 | 15골드 |
| Boss 처치 | 기본×20 |

### 지출

| 항목 | 값 |
|------|-----|
| 유닛 소환 | 5골드 |
| 조합 | 0골드 |
| 업그레이드 | 등급별 (위 표 참조) |

### 판매 가격

| 등급 | 판매 가격 |
|------|----------|
| Normal | 3골드 |
| Rare | 8골드 |
| Epic | 20골드 |
| Legendary | 50골드 |

### 소환 확률

| 등급 | 확률 |
|------|------|
| Normal | 25% |
| Rare | 25% |
| Epic | 25% |
| Legendary | 25% |

---

## 8. 라운드 구성

> 소스: `Assets/Scripts/Gameplay/RoundConfig.cs`, `Assets/Scripts/Monsters/MonsterManager.cs`

| 항목 | 값 |
|------|-----|
| 준비 시간 | 15초 |
| 전투 시간 | 30초 |
| 라운드당 몬스터 | 30마리 |
| 스폰 간격 | 0.5초 |
| 스폰 지속 | 15초 |
| 최대 동시 몬스터 | 100마리 |
| 보스 라운드 | 5 |
| 보스 스폰 딜레이 | 2초 |

---

## 9. 상점 가격

> 소스: `Assets/Scripts/Gameplay/GameBalanceConfig.cs`

| 유닛 | 해금 가격 |
|------|----------|
| Warrior | 무료 |
| Archer | 100골드 |
| Mage | 500골드 |
| DragonKnight | 2,000골드 |
| Phoenix | 3,000골드 |

---

## 10. 보상

### 게임 결과 보상

| 도달 라운드 | 골드 보상 |
|------------|----------|
| 0~3 | 10골드 |
| 4~6 | 30골드 |
| 7~9 | 60골드 |
| 10+ | 100골드 |

### 일일 보상

| 클리어 횟수 | 골드 | 티켓 |
|------------|------|------|
| 1회 | 50 | 0 |
| 3회 | 0 | 1 |
| 5회 | 150 | 0 |
| 10회 | 200 | 2 |

---

## 11. 유닛 마나/스킬

> 소스: `Assets/Scripts/Units/Unit.cs`

| 항목 | 값 |
|------|-----|
| 최대 마나 | 100 |
| 마나 회복 | MaxMana / 스킬쿨다운 (초당) |
| 스킬 발동 | 마나 100 충전 시 |

---

## 12. 조합 레시피

> 소스: `Assets/Scripts/Gameplay/GameBalanceConfig.cs`

| 재료 (×2) | 결과 (×1) | 비용 |
|-----------|-----------|------|
| Warrior | Archer | 0골드 |
| Archer | Mage | 0골드 |
| Mage | DragonKnight | 0골드 |
| DragonKnight | Phoenix | 0골드 |
