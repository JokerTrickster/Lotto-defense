# Monster Path System

## 📍 경로 시스템 개요

몬스터들은 미리 정의된 경로(Path)를 따라 이동합니다. 현재 3가지 경로 타입이 있습니다:

### PathType 종류

```csharp
public enum PathType
{
    Top,         // 상단 경로 (왼쪽 → 오른쪽)
    Bottom,      // 하단 경로 (왼쪽 → 오른쪽)
    SquareLoop   // 사각형 순환 경로 (그리드 주변을 시계방향으로 돌기)
}
```

## 🗺️ 경로 생성 방식

경로는 `GridManager.GeneratePathWaypoints(PathType pathType)` 메서드에서 생성됩니다.

### SquareLoop (현재 기본 사용)

그리드 바깥쪽을 시계방향으로 도는 경로:

```
시작 (왼쪽 상단)
    ↓
오른쪽으로 이동 →→→→
    ↓
아래로 이동 ↓↓↓↓
    ↓
왼쪽으로 이동 ←←←←
    ↓
위로 이동 ↑↑↑↑
    ↓
다시 시작점 (루프)
```

**코드:**
```csharp
// GridManager.cs의 GeneratePathWaypoints()
case PathType.SquareLoop:
    // 1. 왼쪽 상단 시작
    waypoints.Add(gridTopLeft + Vector3.left * pathMargin);

    // 2. 오른쪽으로 이동 (상단)
    for (int i = 0; i <= PATH_POINTS_PER_SIDE; i++) {
        float t = (float)i / PATH_POINTS_PER_SIDE;
        waypoints.Add(Vector3.Lerp(gridTopLeft, gridTopRight, t) + ...);
    }

    // 3. 아래로 이동 (오른쪽)
    // 4. 왼쪽으로 이동 (하단)
    // 5. 위로 이동 (왼쪽) → 처음으로 돌아감
```

## 🔄 경로 변경 방법

### 1. 기존 경로 수정

`GridManager.cs`의 `GeneratePathWaypoints()` 메서드를 수정:

```csharp
case PathType.SquareLoop:
    // PATH_MARGIN을 조정하면 경로가 그리드로부터 떨어지는 거리 변경
    private const float PATH_MARGIN = 0.3f; // 현재 값

    // PATH_POINTS_PER_SIDE를 조정하면 경로의 부드러움 변경
    private const int PATH_POINTS_PER_SIDE = 6; // 6개 웨이포인트
```

### 2. 새로운 경로 타입 추가

**Step 1:** PathType enum에 새 타입 추가
```csharp
public enum PathType
{
    Top,
    Bottom,
    SquareLoop,
    Zigzag,        // 새로운 경로!
    Spiral         // 또 다른 경로!
}
```

**Step 2:** GridManager의 GeneratePathWaypoints()에 case 추가
```csharp
case PathType.Zigzag:
    // 지그재그 경로 생성 로직
    waypoints.Add(gridTopLeft);
    waypoints.Add(gridBottomRight);
    waypoints.Add(gridTopRight);
    waypoints.Add(gridBottomLeft);
    break;
```

**Step 3:** MonsterManager에서 사용
```csharp
SpawnMonster(currentRoundMonsterType, PathType.Zigzag);
```

### 3. 동적 경로 변경 (런타임)

몬스터가 이미 스폰된 후 경로를 변경하려면:

```csharp
// Monster.cs에 새 메서드 추가
public void ChangePathWaypoints(List<Vector3> newWaypoints)
{
    if (pathFollower != null)
    {
        pathFollower.SetWaypoints(newWaypoints, isLooping);
    }
}

// MonsterManager에서 호출
Monster monster = GetMonsterById(monsterId);
List<Vector3> newPath = GridManager.Instance.GeneratePathWaypoints(PathType.Top);
monster.ChangePathWaypoints(newPath);
```

## 📊 현재 사용 중인 경로

**MonsterManager.cs - StartMonsterSpawning():**
```csharp
// Line 286: 모든 몬스터가 SquareLoop 경로 사용
SpawnMonster(currentRoundMonsterType, PathType.SquareLoop);
```

**변경 예시:**
```csharp
// 짝수 라운드는 Top, 홀수 라운드는 Bottom
PathType path = (currentRound % 2 == 0) ? PathType.Top : PathType.Bottom;
SpawnMonster(currentRoundMonsterType, path);
```

## 🎯 보스 라운드 (5라운드)

5라운드에서는 보스가 등장하며, 특별한 연출이 추가됩니다:

### 보스 스폰 순서

1. **경고 이펙트** (2초)
   - 화면 흔들림 (Screen Shake)
   - "⚔️ BOSS INCOMING! ⚔️" 경고 텍스트 (빨간색)

2. **보스 등장**
   - 보스 몬스터 스폰 (HP x10, Gold x20)
   - 금색 원형 이펙트 + "👑 BOSS 👑" 텍스트

### 보스 스탯

```csharp
// MonsterManager.CreateBossData()
HP:     baseHP × 10      (10배)
Speed:  baseSpeed × 0.7  (30% 느림 - 더 위압적)
Damage: baseDamage × 5   (5배)
Gold:   baseGold × 20    (20배 보상!)
```

## 💡 추가 기능 아이디어

### 1. 라운드별 경로 로테이션
```csharp
PathType GetPathForRound(int round)
{
    PathType[] rotation = { PathType.Top, PathType.SquareLoop, PathType.Bottom };
    return rotation[round % rotation.Length];
}
```

### 2. 몬스터 타입별 경로
```csharp
if (monsterData.monsterName == "FastMonster")
    pathType = PathType.Top; // 빠른 몬스터는 짧은 경로
else if (monsterData.monsterName == "TankMonster")
    pathType = PathType.SquareLoop; // 탱크는 긴 경로
```

### 3. 난이도별 경로
```csharp
if (difficultyLevel >= 5)
    pathType = PathType.RandomZigzag; // 어려워지면 예측 불가능한 경로
```

---

**작성일:** 2026-02-11
**버전:** 1.0
