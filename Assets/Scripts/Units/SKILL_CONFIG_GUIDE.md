# 스킬 Config 관리 가이드

## 개요
모든 스킬은 `GameBalanceConfig`에서 중앙 관리됩니다. 코드 변경 없이 Unity Inspector에서 스킬을 수정하고 새로운 스킬을 추가할 수 있습니다.

## 스킬 프리셋 시스템

### 구조
```
GameBalanceConfig
├─ skillPresets (List<SkillPreset>)
│  ├─ skillId: "critical_strike"    ← 스킬 고유 ID
│  └─ skill: SkillBalance            ← 스킬 밸런스 데이터
└─ units (List<UnitBalance>)
   └─ skillIds: ["critical_strike"]  ← 유닛이 가질 스킬 ID들
```

### 장점
1. **중앙 관리**: 모든 스킬을 한 곳에서 관리
2. **재사용**: 여러 유닛이 같은 스킬을 공유
3. **실시간 조정**: Inspector에서 즉시 밸런스 조정
4. **코드 불필요**: 새 스킬 추가 시 코드 변경 없음

## 기본 제공 스킬

### OnHit 스킬 (공격 시 발동)
| ID | 이름 | 설명 | 쿨다운 | 효과 |
|----|------|------|--------|------|
| `critical_strike` | 크리티컬 스트라이크 | 2배 데미지 | 3초 | 데미지 x2 |
| `double_shot` | 더블 샷 | 추가 1회 공격 | 5초 | 0.5배 데미지 추가 |
| `piercing_arrow` | 관통 화살 | 2명 관통 | 4초 | 2명 명중 |

### OnKill 스킬 (처치 시 발동)
| ID | 이름 | 설명 | 쿨다운 | 효과 |
|----|------|------|--------|------|
| `chain_lightning` | 연쇄 번개 | 주변 적 데미지 | 2초 | 3명, 0.5배 데미지, 범위 2 |
| `battle_frenzy` | 전투 광기 | 공속 증가 | 1초 | 1.5배 공속, 3초 지속 |
| `gold_rush` | 골드 러시 | 추가 골드 | 없음 | 골드 +3 (구현 필요) |

### Passive 스킬 (항상 활성)
| ID | 이름 | 설명 | 효과 |
|----|------|------|------|
| `sniper` | 저격수 | 사거리 증가 | 범위 x1.5 |
| `berserker` | 버서커 | 공격력 증가 | 데미지 x1.3 |
| `rapid_fire` | 속사 | 공격속도 증가 | 공속 x1.5 |
| `area_attack` | 광역 공격 | AOE 공격 | 범위 1.5, 최대 5명 |

## Unity Inspector 사용법

### 1. GameBalanceConfig 에셋 찾기
1. Project 창에서 `Resources/GameBalanceConfig` 검색
2. 없으면 생성: `Create > LottoDefense > Game Balance Config`

### 2. 새 스킬 추가하기
1. Inspector에서 `Skill Presets` 섹션으로 스크롤
2. 배열 크기 +1
3. 새 스킬 설정:
   ```
   Skill Id: "my_new_skill"
   Skill Name: "내 스킬"
   Description: "스킬 설명"
   Skill Type: OnHit / OnKill / Passive
   Cooldown Duration: 5
   Damage Multiplier: 1.5
   ... (기타 파라미터)
   ```

### 3. 유닛에 스킬 할당하기
1. `Units` 섹션에서 원하는 유닛 찾기
2. `Skill Ids` 배열에 스킬 ID 추가
   ```
   Skill Ids:
   - critical_strike
   - rapid_fire
   ```

### 4. 스킬 밸런스 조정
1. `Skill Presets`에서 스킬 찾기
2. 원하는 파라미터 수정:
   - `Damage Multiplier`: 데미지 배율
   - `Cooldown Duration`: 쿨다운 시간
   - `Effect Duration`: 버프 지속시간
   - `Target Count`: 영향받는 대상 수
   - `Aoe Radius`: 범위
3. 저장하면 해당 스킬을 가진 모든 유닛에 즉시 반영

## 코드에서 사용하기

### 1. 유닛 초기화 시 스킬 로드
```csharp
// UnitManager.cs 또는 Unit.cs에서
public void InitializeUnit(GameBalanceConfig.UnitBalance unitBalance, GameBalanceConfig config)
{
    // 유닛의 스킬 가져오기
    List<GameBalanceConfig.SkillBalance> skillBalances = config.GetUnitSkills(unitBalance);

    // UnitSkill 인스턴스 생성
    List<UnitSkill> skills = new List<UnitSkill>();
    foreach (var balance in skillBalances)
    {
        UnitSkill skill = UnitSkill.FromBalance(balance);
        skill.Initialize();
        skills.Add(skill);
    }

    // 스킬 저장
    this.skills = skills;
}
```

### 2. 스킬 쿨다운 업데이트
```csharp
// Unit.cs Update()
private void Update()
{
    if (skills != null)
    {
        foreach (var skill in skills)
        {
            skill.UpdateCooldown(Time.deltaTime);
        }
    }
}
```

### 3. OnHit 스킬 발동
```csharp
// Unit.cs 공격 메서드
private void Attack(Monster target)
{
    // 기본 공격
    int baseDamage = attack;

    // OnHit 스킬 체크 및 발동
    if (skills != null)
    {
        foreach (var skill in skills)
        {
            if (skill.ShouldTriggerOnHit() && skill.TryActivate())
            {
                // 데미지 배율 적용
                baseDamage = Mathf.RoundToInt(baseDamage * skill.damageMultiplier);

                Debug.Log($"[Unit] {skill.skillName} 발동! 데미지: {baseDamage}");
            }
        }
    }

    target.TakeDamage(baseDamage);
}
```

### 4. OnKill 스킬 발동
```csharp
// Unit.cs 또는 CombatManager에서
private void OnMonsterKilled(Monster killedMonster)
{
    if (skills != null)
    {
        foreach (var skill in skills)
        {
            if (skill.ShouldTriggerOnKill() && skill.TryActivate())
            {
                Debug.Log($"[Unit] {skill.skillName} 발동! (OnKill)");

                // AOE 효과
                if (skill.aoeRadius > 0f && skill.targetCount > 0)
                {
                    var nearbyMonsters = FindMonstersInRadius(
                        killedMonster.transform.position,
                        skill.aoeRadius
                    );

                    int count = Mathf.Min(nearbyMonsters.Count, skill.targetCount);
                    for (int i = 0; i < count; i++)
                    {
                        int damage = Mathf.RoundToInt(attack * skill.damageMultiplier);
                        nearbyMonsters[i].TakeDamage(damage);
                    }
                }

                // 버프 효과 (공속 증가 등)
                if (skill.attackSpeedMultiplier > 1f && skill.effectDuration > 0f)
                {
                    StartCoroutine(ApplyTemporaryBuff(skill));
                }
            }
        }
    }
}
```

### 5. Passive 스킬 적용
```csharp
// Unit.cs 초기화 시
private void ApplyPassiveSkills()
{
    if (skills == null) return;

    float totalDamageMultiplier = 1f;
    float totalRangeMultiplier = 1f;
    float totalAttackSpeedMultiplier = 1f;

    foreach (var skill in skills)
    {
        if (skill.IsPassive())
        {
            totalDamageMultiplier *= skill.damageMultiplier;
            totalRangeMultiplier *= skill.rangeMultiplier;
            totalAttackSpeedMultiplier *= skill.attackSpeedMultiplier;

            Debug.Log($"[Unit] Passive 스킬 적용: {skill.skillName}");
        }
    }

    // 최종 스탯에 패시브 효과 적용
    this.attack = Mathf.RoundToInt(baseAttack * totalDamageMultiplier);
    this.attackRange = baseAttackRange * totalRangeMultiplier;
    this.attackSpeed = baseAttackSpeed * totalAttackSpeedMultiplier;
}
```

## 스킬 밸런싱 팁

### 1. 쿨다운 설정
- **OnHit 스킬**: 3~5초 (너무 짧으면 OP)
- **OnKill 스킬**: 1~2초 (처치가 조건이므로 짧아도 OK)
- **Passive**: 0초 (항상 활성)

### 2. 데미지 배율
- **크리티컬**: 2.0x (강력하지만 쿨다운 김)
- **추가 공격**: 0.5x (여러 번이므로 낮게)
- **패시브 증가**: 1.3x (항상 적용이므로 적당히)

### 3. AOE 범위
- **소형**: 1.0~1.5 (인접한 적 1~2명)
- **중형**: 2.0~3.0 (주변 여러 명)
- **대형**: 4.0+ (화면 절반)

### 4. 레어리티별 스킬 수
- **Normal**: 1개 (단순 스킬)
- **Rare**: 1~2개 (조합 가능)
- **Epic**: 2개 (강력한 조합)
- **Legendary**: 3개 (최강 조합)

## 새 스킬 아이디어

### OnHit 스킬
```yaml
lifesteal:
  name: "흡혈"
  description: "데미지의 30% 회복 (구현 필요)"
  cooldown: 5초
  damageMultiplier: 1.0

slow:
  name: "둔화"
  description: "적 이동속도 -50%, 2초 (구현 필요)"
  cooldown: 4초
  effectDuration: 2초
```

### OnKill 스킬
```yaml
explosive_death:
  name: "폭발 죽음"
  description: "처치 시 범위 3에 100% 데미지"
  cooldown: 3초
  damageMultiplier: 1.0
  aoeRadius: 3.0
  targetCount: 10

heal_on_kill:
  name: "승리의 치유"
  description: "처치 시 체력 회복 (구현 필요)"
  cooldown: 0초
```

### Passive 스킬
```yaml
armor_pierce:
  name: "방어 관통"
  description: "적 방어력 무시 50% (구현 필요)"
  cooldown: 0초

vampire_aura:
  name: "흡혈 오라"
  description: "모든 공격에 10% 흡혈 (구현 필요)"
  cooldown: 0초
```

## 문제 해결

### Q: 스킬이 발동하지 않아요
A: 다음을 확인하세요:
1. `skill.Initialize()` 호출했나요?
2. `skill.UpdateCooldown()` 매 프레임 호출하나요?
3. `skill.TryActivate()` 반환값이 true인가요?
4. 스킬 타입이 올바른 시점에 체크되나요?

### Q: 스킬 ID를 잘못 입력했어요
A: `GetUnitSkills()` 메서드가 경고를 출력합니다. Unity Console 확인하세요.

### Q: 스킬 밸런스를 변경했는데 반영 안 돼요
A: GameBalanceConfig 에셋을 저장했는지 확인하세요 (Ctrl+S).

### Q: 같은 스킬을 여러 유닛이 쓸 수 있나요?
A: 네! 그게 프리셋 시스템의 장점입니다. skillIds에 같은 ID 추가하면 됩니다.

## 참고 자료
- `Assets/Scripts/Gameplay/GameBalanceConfig.cs` - 스킬 밸런스 정의
- `Assets/Scripts/Units/UnitSkill.cs` - 스킬 런타임 로직
- `Assets/Scripts/Units/SKILL_SYSTEM_GUIDE.md` - 스킬 시스템 전체 가이드
