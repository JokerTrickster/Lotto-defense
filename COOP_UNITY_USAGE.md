# Unity 협동 플레이 사용 가이드

## 구현 완료 사항

### ✅ 새로 추가된 파일
```
Assets/Scripts/Networking/CoopStateSync.cs      (7.4KB)
Assets/Scripts/UI/OpponentStatusUI.cs            (10KB) - 전체 재작성
```

### ✅ 수정된 파일
```
Assets/Scripts/Gameplay/GameplayManager.cs
Assets/Scripts/Gameplay/GameSceneBootstrapper.cs
```

---

## 사용 방법

### 1. 협동 플레이 시작

**MainGameBootstrapper.cs에서:**

```csharp
using LottoDefense.Backend;
using LottoDefense.Gameplay;

private void OnCoopPlayClicked()
{
    // 난이도 선택
    DifficultySelectionUI.Show(true, (difficulty) => {
        StartCoroutine(CreateAndJoinRoom(difficulty));
    });
}

private IEnumerator CreateAndJoinRoom(GameDifficulty difficulty)
{
    // API 클라이언트 가져오기
    APIClient apiClient = APIManager.Instance.Client;
    
    // 방 생성
    bool roomCreated = false;
    uint roomID = 0;
    
    yield return apiClient.Post(
        "/coop/rooms",
        new { room_type = "private", max_players = 2 },
        (RoomResponse response) => {
            roomCreated = true;
            roomID = response.room_id;
            Debug.Log($"Room created: {response.room_code}");
        },
        (error) => {
            Debug.LogError($"Room creation failed: {error}");
        }
    );
    
    if (!roomCreated) yield break;
    
    // 게임 씬 로드
    SceneManager.LoadScene("GameScene");
    
    // 씬 로드 대기
    yield return new WaitForSeconds(0.5f);
    
    // 협동 플레이 시작
    if (GameplayManager.Instance != null)
    {
        GameplayManager.Instance.StartCoopGame(roomID, difficulty, apiClient);
    }
}
```

### 2. 방 참가 (두 번째 플레이어)

```csharp
private IEnumerator JoinExistingRoom(string roomCode, GameDifficulty difficulty)
{
    APIClient apiClient = APIManager.Instance.Client;
    
    // 방 참가
    bool joined = false;
    uint roomID = 0;
    
    yield return apiClient.Post(
        "/coop/rooms/join",
        new { room_code = roomCode },
        (RoomResponse response) => {
            joined = true;
            roomID = response.room_id;
            Debug.Log($"Joined room: {roomID}");
        },
        (error) => {
            Debug.LogError($"Join failed: {error}");
        }
    );
    
    if (!joined) yield break;
    
    // Ready 상태
    yield return apiClient.Post(
        $"/coop/rooms/{roomID}/ready",
        new { is_ready = true },
        (response) => {
            Debug.Log("Ready!");
        },
        (error) => {
            Debug.LogError($"Ready failed: {error}");
        }
    );
    
    // 게임 시작
    SceneManager.LoadScene("GameScene");
    yield return new WaitForSeconds(0.5f);
    
    if (GameplayManager.Instance != null)
    {
        GameplayManager.Instance.StartCoopGame(roomID, difficulty, apiClient);
    }
}
```

### 3. 상대방 상태 모니터링

**자동으로 동작하지만, 커스텀 로직 추가 가능:**

```csharp
using LottoDefense.Networking;

void Start()
{
    // 이벤트 구독
    CoopStateSync.Instance.OnOpponentStateUpdated += OnOpponentUpdate;
    CoopStateSync.Instance.OnOpponentDefeated += OnOpponentLost;
}

void OnOpponentUpdate(OpponentStateData state)
{
    Debug.Log($"Opponent: Round {state.round}, HP {state.hp}");
    
    // 커스텀 UI 업데이트
    if (state.hp < 3)
    {
        ShowWarning("상대방 위험!");
    }
}

void OnOpponentLost()
{
    Debug.Log("Opponent was defeated!");
    ShowMessage("상대방이 패배했습니다!");
}
```

### 4. 동기화 간격 조정

```csharp
// 중요 이벤트 발생 시 1초로 단축
void OnBossAppeared()
{
    CoopStateSync.Instance.SetSyncInterval(1f);
}

// 평상시 3초
void OnBossDefeated()
{
    CoopStateSync.Instance.SetSyncInterval(3f);
}
```

---

## 자동 동작 기능

### ✅ 자동으로 처리되는 것들

1. **상태 전송 (3초마다)**
   - 라운드, HP, 골드, 처치 수
   - 타임스탬프

2. **상대방 정보 수신**
   - 상대방 이름, 상태
   - 생존 여부

3. **UI 표시**
   - 좌상단에 상대방 패널 자동 생성
   - HP 바, 색상 변화
   - 패배 시 오버레이

4. **정리**
   - 게임 종료 시 자동 정리
   - 씬 전환 시 안전하게 정리

---

## API 응답 모델

### RoomResponse
```csharp
[Serializable]
public class RoomResponse
{
    public uint room_id;
    public string room_code;
    public string status;
}
```

### OpponentStateResponse
```csharp
[Serializable]
public class OpponentStateResponse
{
    public uint opponent_id;
    public string opponent_name;
    public int round;
    public int hp;
    public int gold;
    public int kills;
    public long last_update;
    public bool is_alive;
}
```

---

## 테스트 시나리오

### 로컬 테스트 (2개 Unity 실행)

**Player 1 (방 생성자):**
```
1. Play 클릭
2. 협동 플레이 선택
3. 난이도 선택
4. 방 코드 확인 (예: "AB12")
5. 게임 대기
```

**Player 2 (참가자):**
```
1. Play 클릭
2. 방 코드 입력 "AB12"
3. 참가 클릭
4. Ready 클릭
5. 게임 시작
```

**게임 중:**
```
- 좌상단에 상대방 상태 표시됨
- 3초마다 HP/라운드/골드 업데이트
- 한쪽이 패배하면 "패배" 오버레이
```

---

## 디버깅

### Console 로그 확인

```
[CoopStateSync] Started syncing for room 123
[CoopStateSync] State updated
[OpponentStatusUI] UI created
Opponent: Round 5, HP 87
[CoopStateSync] Opponent Player2 defeated!
[CoopStateSync] Stopped syncing
```

### 문제 해결

**1. 상대방 정보가 안 보임**
- 3초 대기 (첫 동기화까지)
- APIClient 토큰 확인
- roomID가 올바른지 확인

**2. UI가 안 보임**
- OpponentStatusUI 생성 확인
- Canvas 존재 확인
- CoopStateSync.IsEnabled 확인

**3. 동기화가 멈춤**
- 네트워크 연결 확인
- 백엔드 서버 상태 확인
- Console 에러 메시지 확인

---

## 성능 최적화

### 권장 설정
```csharp
// 평상시
CoopStateSync.Instance.SetSyncInterval(3f);  // 3초

// 라운드 시작 시
CoopStateSync.Instance.SetSyncInterval(2f);  // 2초

// 보스전
CoopStateSync.Instance.SetSyncInterval(1f);  // 1초 (실시간에 가깝게)
```

### 주의사항
- 1초 미만으로 설정하지 마세요 (서버 부하)
- WiFi 환경에서 테스트 권장
- 3초 간격으로도 충분히 작동합니다

---

## 다음 단계

### Phase 2: WebSocket 업그레이드 (선택)
- 실시간 동기화 (지연 < 100ms)
- 더 나은 사용자 경험
- 기존 코드 재사용 가능

**현재 REST polling으로 완벽히 작동합니다!** ✅
