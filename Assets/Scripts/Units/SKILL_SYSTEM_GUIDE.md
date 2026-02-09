# 유닛 스킬 시스템 가이드

## 개요
각 유닛은 특별한 스킬을 가질 수 있습니다. 스킬은 4가지 타입으로 분류됩니다:
- **Active**: 수동으로 발동하는 스킬 (미구현 - UI 필요)
- **Passive**: 항상 활성화된 효과
- **OnHit**: 유닛이 공격할 때마다 발동
- **OnKill**: 유닛이 몬스터를 처치할 때 발동

## UnitSkill 클래스 구조

### 주요 필드
```csharp
public string skillName;              // 스킬 이름
public string description;            // 스킬 설명
public SkillType skillType;          // 스킬 타입
public float cooldownDuration;       // 쿨다운 시간 (초)
public float damageMultiplier;       // 데미지 배율 (1.5 = 150%)
public float rangeMultiplier;        // 범위 배율
public float attackSpeedMultiplier;  // 공속 배율
public float effectDuration;         // 효과 지속시간
public int targetCount;              // 영향받는 대상 수
public float aoeRadius;              // AOE 범위
```

### 런타임 상태
```csharp
public float currentCooldown;        // 현재 쿨다운 (초)
public bool IsOnCooldown;            // 쿨다운 중인지 여부
public float CooldownProgress;       // 쿨다운 진행도 (0~1)
```

### 주요 메서드
```csharp
Initialize()                         // 스킬 초기화
TryActivate()                        // 스킬 발동 시도 (성공 시 true 반환)
UpdateCooldown(float deltaTime)      // 쿨다운 업데이트 (매 프레임 호출)
ResetCooldown()                      // 쿨다운 즉시 초기화
ShouldTriggerOnHit()                // OnHit 스킬이 발동해야 하는지 체크
ShouldTriggerOnKill()               // OnKill 스킬이 발동해야 하는지 체크
IsPassive()                          // Passive 스킬인지 체크
```

## 스킬 예제

### 1. 크리티컬 스트라이크 (OnHit)
```csharp
{
    skillName = "크리티컬 스트라이크",
    description = "공격 시 20% 확률로 2배 데미지",
    skillType = SkillType.OnHit,
    cooldownDuration = 3f,           // 3초 쿨다운
    damageMultiplier = 2.0f,         // 2배 데미지
}
```

### 2. 흡혈 (OnHit)
```csharp
{
    skillName = "흡혈",
    description = "공격 시 데미지의 30%만큼 체력 회복",
    skillType = SkillType.OnHit,
    cooldownDuration = 5f,
    damageMultiplier = 1.0f,
    // 흡혈 효과는 Unit 클래스에서 별도 구현 필요
}
```

### 3. 버서커 (Passive)
```csharp
{
    skillName = "버서커",
    description = "체력이 낮을수록 공격력 증가 (최대 50%)",
    skillType = SkillType.Passive,
    cooldownDuration = 0f,           // 패시브는 쿨다운 없음
    damageMultiplier = 1.5f,         // 최대 1.5배
}
```

### 4. 연쇄 공격 (OnKill)
```csharp
{
    skillName = "연쇄 공격",
    description = "적 처치 시 주변 적들에게 데미지",
    skillType = SkillType.OnKill,
    cooldownDuration = 2f,
    damageMultiplier = 0.5f,         // 50% 데미지
    targetCount = 3,                 // 최대 3명
    aoeRadius = 2.0f,                // 범위 2 유닛
}
```

### 5. 공격 속도 버프 (OnKill)
```csharp
{
    skillName = "전투 광기",
    description = "적 처치 시 3초간 공격속도 50% 증가",
    skillType = SkillType.OnKill,
    cooldownDuration = 1f,
    attackSpeedMultiplier = 1.5f,    // 1.5배 공속
    effectDuration = 3f,             // 3초 지속
}
```

## Unity Editor에서 스킬 설정하기

### 1. UnitData 에셋에 스킬 추가
1. Project 창에서 UnitData 에셋 선택
2. Inspector에서 "Skills" 배열 크기 설정
3. 각 스킬의 필드 값 입력
4. 스킬 아이콘, VFX 프리팹 할당 (선택 사항)

### 2. 스킬 효과 구현
스킬 효과는 `Unit` 클래스나 `CombatManager`에서 구현해야 합니다:

```csharp
// Unit.cs에서 스킬 초기화
private void InitializeSkills()
{
    if (unitData.skills != null)
    {
        foreach (var skill in unitData.skills)
        {
            skill.Initialize();

            // 스킬 발동 이벤트 구독
            skill.OnSkillActivated += HandleSkillActivated;
        }
    }
}

// Update에서 스킬 쿨다운 업데이트
private void Update()
{
    if (unitData.skills != null)
    {
        foreach (var skill in unitData.skills)
        {
            skill.UpdateCooldown(Time.deltaTime);
        }
    }
}

// 공격 시 OnHit 스킬 체크
private void Attack(Monster target)
{
    // 기본 공격 로직...

    // OnHit 스킬 발동
    if (unitData.skills != null)
    {
        foreach (var skill in unitData.skills)
        {
            if (skill.ShouldTriggerOnHit() && skill.TryActivate())
            {
                ApplySkillEffect(skill, target);
            }
        }
    }
}

// 스킬 효과 적용
private void ApplySkillEffect(UnitSkill skill, Monster target)
{
    // 데미지 배율 적용
    int bonusDamage = Mathf.RoundToInt(attack * (skill.damageMultiplier - 1f));

    // VFX 재생
    if (skill.vfxPrefab != null && VFXManager.Instance != null)
    {
        VFXManager.Instance.PlaySkillEffect(skill.vfxPrefab, target.transform.position);
    }

    // AOE 효과
    if (skill.aoeRadius > 0f && skill.targetCount > 0)
    {
        // 범위 내 적들 찾기
        var nearbyEnemies = FindNearbyMonsters(target.transform.position, skill.aoeRadius);
        for (int i = 0; i < Mathf.Min(nearbyEnemies.Count, skill.targetCount); i++)
        {
            nearbyEnemies[i].TakeDamage(bonusDamage);
        }
    }
}
```

## 다음 구현 단계

### 1. Unit 클래스에 스킬 통합 (필수)
- [ ] `Unit.cs`에 스킬 초기화 메서드 추가
- [ ] Update에서 스킬 쿨다운 업데이트
- [ ] 공격 시 OnHit 스킬 발동 로직
- [ ] 처치 시 OnKill 스킬 발동 로직
- [ ] Passive 스킬 효과 적용

### 2. UI 표시 (선택 사항)
- [ ] 스킬 아이콘 표시
- [ ] 쿨다운 진행바
- [ ] 스킬 설명 툴팁

### 3. VFX 연동 (선택 사항)
- [ ] 스킬 발동 시 이펙트 재생
- [ ] AOE 범위 표시
- [ ] 버프 이펙트

### 4. 밸런싱
- [ ] 스킬 쿨다운 조정
- [ ] 데미지 배율 밸런싱
- [ ] 효과 지속시간 조정

## 주의사항

1. **쿨다운 업데이트**: 반드시 매 프레임 `UpdateCooldown()`을 호출해야 합니다.
2. **초기화**: 유닛 생성 시 모든 스킬의 `Initialize()`를 호출해야 합니다.
3. **메모리 관리**: 스킬 이벤트 구독 시 적절히 해제해야 메모리 누수를 방지할 수 있습니다.
4. **밸런싱**: 스킬이 너무 강력하면 게임이 쉬워질 수 있으니 적절한 쿨다운과 효과를 설정하세요.

## 참고 자료
- `Assets/Scripts/Units/UnitSkill.cs` - 스킬 클래스 정의
- `Assets/Scripts/Units/UnitData.cs` - 유닛 데이터에 스킬 배열 추가됨
