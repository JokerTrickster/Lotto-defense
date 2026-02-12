# Lotto Defense - 백엔드 서버 명세

## 개요

Lotto Defense 멀티플레이어 서버. 2인 "병렬 개인전" 타워디펜스.
- 각자 독립된 맵에서 디펜스 진행
- 같은 웨이브 동기화
- 한 명이 죽어도 상대는 계속 진행
- 마지막까지 살아남은 사람 또는 더 높은 라운드 도달자가 승리

**기술 스택**: Go + gorilla/websocket (또는 nhooyr/websocket)

---

## 1. WebSocket 메시지 프로토콜

### 메시지 포맷

모든 메시지는 JSON으로 envelope 구조:

```json
{
  "type": "message_type_string",
  "payload": "{\"field\":\"value\"}"   // JSON 문자열 (이중 직렬화)
}
```

> **주의**: `payload`는 JSON 객체가 아니라 **JSON 문자열**. Unity의 `JsonUtility`가 이중 직렬화하기 때문.
> 서버에서 파싱할 때: `json.Unmarshal(msg.Payload)` → string → 다시 `json.Unmarshal`

### 1-1. Client → Server 메시지

| Type | Payload | 설명 |
|------|---------|------|
| `room_create` | `RoomCreatePayload` | 방 생성 요청 |
| `room_join` | `RoomJoinPayload` | 방 코드로 참가 |
| `room_auto_match` | `RoomAutoMatchPayload` | 자동 매칭 요청 |
| `player_ready` | `PlayerReadyPayload` | 준비 완료 |
| `game_state_update` | `GameStateUpdatePayload` | 게임 상태 리포트 (3초 간격) |
| `player_dead` | `PlayerDeadPayload` | 플레이어 사망 |
| `heartbeat` | `HeartbeatPayload` | 접속 유지 (15초 간격) |

#### Payload 상세

```go
// room_create
type RoomCreatePayload struct {
    PlayerName string `json:"playerName"`
}

// room_join
type RoomJoinPayload struct {
    RoomCode   string `json:"roomCode"`
    PlayerName string `json:"playerName"`
}

// room_auto_match
type RoomAutoMatchPayload struct {
    PlayerName string `json:"playerName"`
}

// player_ready
type PlayerReadyPayload struct {
    RoomCode string `json:"roomCode"`
}

// game_state_update (클라이언트가 3초마다 전송)
type GameStateUpdatePayload struct {
    Life           int `json:"life"`
    Round          int `json:"round"`
    Gold           int `json:"gold"`
    MonstersKilled int `json:"monstersKilled"`
    UnitCount      int `json:"unitCount"`
}

// player_dead
type PlayerDeadPayload struct {
    FinalRound   int `json:"finalRound"`
    Contribution int `json:"contribution"`
}

// heartbeat
type HeartbeatPayload struct {
    Timestamp int64 `json:"timestamp"`  // Unix millis
}
```

### 1-2. Server → Client 메시지

| Type | Payload | 설명 |
|------|---------|------|
| `room_created` | `RoomCreatedPayload` | 방 생성 완료 |
| `player_joined` | `PlayerJoinedPayload` | 상대방 입장 |
| `match_start` | `MatchStartPayload` | 게임 시작 |
| `wave_sync` | `WaveSyncPayload` | 웨이브 동기화 신호 |
| `opponent_state` | `OpponentStatePayload` | 상대 상태 업데이트 |
| `opponent_dead` | `OpponentDeadPayload` | 상대 사망 |
| `match_result` | `MatchResultPayload` | 최종 결과 |
| `error` | `ErrorPayload` | 에러 |

#### Payload 상세

```go
// room_created
type RoomCreatedPayload struct {
    RoomCode string `json:"roomCode"`
    RoomID   string `json:"roomId"`
}

// player_joined
type PlayerJoinedPayload struct {
    PlayerName  string `json:"playerName"`
    PlayerCount int    `json:"playerCount"`
}

// match_start
type MatchStartPayload struct {
    TotalPlayers int `json:"totalPlayers"`
    StartRound   int `json:"startRound"`
}

// wave_sync
type WaveSyncPayload struct {
    Round int `json:"round"`
}

// opponent_state (상대 game_state_update를 변환해서 전달)
type OpponentStatePayload struct {
    PlayerName     string `json:"playerName"`
    Life           int    `json:"life"`
    Round          int    `json:"round"`
    Gold           int    `json:"gold"`
    MonstersKilled int    `json:"monstersKilled"`
    UnitCount      int    `json:"unitCount"`
    IsAlive        bool   `json:"isAlive"`
}

// opponent_dead
type OpponentDeadPayload struct {
    PlayerName string `json:"playerName"`
    FinalRound int    `json:"finalRound"`
}

// match_result
type MatchResultPayload struct {
    IsWinner             bool `json:"isWinner"`
    MyRound              int  `json:"myRound"`
    OpponentRound        int  `json:"opponentRound"`
    MyContribution       int  `json:"myContribution"`
    OpponentContribution int  `json:"opponentContribution"`
}

// error
type ErrorPayload struct {
    Code    string `json:"code"`
    Message string `json:"message"`
}
```

---

## 2. 시퀀스 다이어그램

### 2-1. 방 생성 + 매칭 플로우

```
Player A (방장)                Server                  Player B
    |                            |                        |
    |-- room_create ------------>|                        |
    |<-- room_created -----------|  (roomCode: "ABC123")  |
    |                            |                        |
    |                            |<-- room_join ----------|  (roomCode: "ABC123")
    |<-- player_joined ----------|-- player_joined ------>|  (playerCount: 2)
    |                            |                        |
    |-- player_ready ----------->|                        |
    |                            |<-- player_ready -------|
    |                            |                        |
    |<-- match_start ------------|-- match_start -------->|  (totalPlayers: 2)
    |                            |                        |
```

### 2-2. 자동 매칭 플로우

```
Player A                     Server                  Player B
    |                            |                        |
    |-- room_auto_match -------->|                        |
    |   (대기열 진입)              |                        |
    |                            |<-- room_auto_match ----|
    |                            |   (매칭 성사!)           |
    |<-- room_created -----------|-- room_created ------->|
    |<-- player_joined ----------|-- player_joined ------>|
    |                            |                        |
    |   (자동 매칭은 ready 자동)    |                        |
    |<-- match_start ------------|-- match_start -------->|
```

### 2-3. 인게임 플로우

```
Player A                     Server                  Player B
    |                            |                        |
    |<-- wave_sync (round 1) ----|-- wave_sync (round 1)->|
    |                            |                        |
    |-- game_state_update ------>|                        |
    |                            |-- opponent_state ----->|  (A의 상태)
    |                            |                        |
    |                            |<-- game_state_update --|
    |<-- opponent_state ---------|                        |  (B의 상태)
    |                            |                        |
    |   ... (3초 간격 반복) ...     |                        |
    |                            |                        |
    |-- player_dead ------------>|                        |
    |                            |-- opponent_dead ------>|
    |                            |                        |
    |                            |  (B가 계속 진행...)       |
    |                            |<-- player_dead --------|  (or B도 죽거나 클리어)
    |                            |                        |
    |<-- match_result -----------|-- match_result ------->|
```

---

## 3. 서버 아키텍처 (Go)

### 3-1. 핵심 구조체

```go
// 서버 메인 구조체
type Server struct {
    rooms      map[string]*Room    // roomCode -> Room
    matchQueue chan *Player         // 자동 매칭 대기열
    mu         sync.RWMutex        // rooms 맵 보호
}

// 방
type Room struct {
    Code       string
    ID         string
    Players    [2]*Player          // 최대 2인
    State      RoomState           // Waiting, Playing, Finished
    CurrentRound int

    msgCh      chan RoomMessage     // 방 전용 메시지 채널
    done       chan struct{}        // 방 종료 신호
    mu         sync.Mutex          // Players 접근 보호
}

// 플레이어
type Player struct {
    Name       string
    Conn       *websocket.Conn
    Room       *Room
    GameState  GameStateUpdatePayload
    IsAlive    bool
    IsReady    bool

    sendCh     chan []byte          // 이 플레이어에게 보낼 메시지 큐
}

// 방 상태
type RoomState int
const (
    RoomWaiting  RoomState = iota  // 대기 중 (플레이어 모집)
    RoomPlaying                     // 게임 진행 중
    RoomFinished                    // 게임 종료
)
```

### 3-2. Goroutine 구조 (방마다 goroutine)

```
메인 goroutine
├── HTTP/WebSocket 서버 (net/http)
├── matchmaker goroutine (자동 매칭 루프)
│
└── 클라이언트 접속 시:
    ├── readPump goroutine  (클라이언트 → 서버 메시지 읽기)
    └── writePump goroutine (서버 → 클라이언트 메시지 쓰기)

방 생성 시:
└── Room.Run() goroutine (방 전용 게임 루프)
    ├── 메시지 라우팅
    ├── 웨이브 동기화 타이머
    └── 매치 결과 판정
```

**권장: 방마다 1개의 goroutine (`Room.Run()`)을 생성**하는 것이 맞습니다.

이유:
- 각 방의 상태는 독립적 → goroutine 분리가 자연스러움
- 채널 기반으로 메시지를 받으면 **lock 없이** 방 내부 상태를 안전하게 처리
- 방 종료 시 goroutine도 자연스럽게 종료

### 3-3. Goroutine 상세 설계

```go
// 클라이언트 접속 처리
func (s *Server) HandleWebSocket(w http.ResponseWriter, r *http.Request) {
    conn, _ := upgrader.Upgrade(w, r, nil)
    player := &Player{
        Conn:   conn,
        sendCh: make(chan []byte, 256),
    }

    go player.readPump(s)   // 클라이언트 메시지 읽기
    go player.writePump()   // 클라이언트에게 메시지 보내기
}

// readPump: 클라이언트로부터 메시지를 읽어서 적절한 곳으로 라우팅
func (p *Player) readPump(s *Server) {
    defer p.cleanup()

    for {
        _, message, err := p.Conn.ReadMessage()
        if err != nil { break }

        var msg NetworkMessage
        json.Unmarshal(message, &msg)

        switch msg.Type {
        case "room_create", "room_join", "room_auto_match":
            s.handleLobbyMessage(p, msg)  // 서버 레벨 처리
        default:
            if p.Room != nil {
                p.Room.msgCh <- RoomMessage{Player: p, Msg: msg}  // 방으로 전달
            }
        }
    }
}

// writePump: sendCh에서 메시지를 꺼내 클라이언트에 전송
func (p *Player) writePump() {
    for msg := range p.sendCh {
        p.Conn.WriteMessage(websocket.TextMessage, msg)
    }
}

// Room.Run: 방 전용 게임 루프 goroutine
func (r *Room) Run() {
    defer r.cleanup()

    waveTicker := time.NewTicker(0) // 시작 시 즉시 첫 웨이브
    waveTicker.Stop()               // Playing 상태가 될 때 시작

    for {
        select {
        case msg := <-r.msgCh:
            r.handleMessage(msg)     // lock 불필요 - 이 goroutine만 접근

        case <-waveTicker.C:
            r.broadcastWaveSync()

        case <-r.done:
            return
        }
    }
}
```

### 3-4. 동시성 문제 해결 전략

| 공유 자원 | 접근 패턴 | 해결 방법 |
|-----------|-----------|-----------|
| `Server.rooms` 맵 | 여러 goroutine이 방 생성/삭제/조회 | `sync.RWMutex` |
| `Room` 내부 상태 | Room.Run() goroutine만 변경 | **채널** (lock 불필요) |
| `Player.sendCh` | readPump이 넣고, writePump이 꺼냄 | **Go 채널** (thread-safe) |
| 매칭 큐 | 여러 플레이어가 동시 진입 | **Go 채널** (buffered) |
| `Player.Conn` (WebSocket) | readPump은 Read, writePump은 Write | 분리되어 있으므로 안전 |

**핵심 원칙: "공유 메모리로 통신하지 말고, 통신으로 메모리를 공유하라"**

```go
// 나쁜 예: mutex로 방 상태 직접 수정
room.mu.Lock()
room.Players[0].GameState = newState
room.mu.Unlock()

// 좋은 예: 채널로 메시지 전달 → Room.Run()이 처리
room.msgCh <- RoomMessage{Player: p, Msg: msg}
// Room.Run() 내부에서 lock 없이 안전하게 처리
```

---

## 4. 매칭 시스템

### 4-1. 방 코드 매칭 (room_create / room_join)

```go
func (s *Server) handleRoomCreate(player *Player, payload RoomCreatePayload) {
    code := generateRoomCode()  // 예: 6자리 영숫자 "ABC123"

    room := &Room{
        Code:    code,
        ID:      uuid.New().String(),
        State:   RoomWaiting,
        msgCh:   make(chan RoomMessage, 64),
        done:    make(chan struct{}),
    }
    room.Players[0] = player
    player.Room = room

    s.mu.Lock()
    s.rooms[code] = room
    s.mu.Unlock()

    go room.Run()  // 방 goroutine 시작

    player.Send("room_created", RoomCreatedPayload{
        RoomCode: code,
        RoomID:   room.ID,
    })
}

func (s *Server) handleRoomJoin(player *Player, payload RoomJoinPayload) {
    s.mu.RLock()
    room, exists := s.rooms[payload.RoomCode]
    s.mu.RUnlock()

    if !exists || room.State != RoomWaiting {
        player.Send("error", ErrorPayload{Code: "ROOM_NOT_FOUND", Message: "방을 찾을 수 없습니다"})
        return
    }

    // 채널로 입장 요청 전달 (Room.Run()이 처리)
    room.msgCh <- RoomMessage{Player: player, Msg: joinMsg}
}
```

### 4-2. 자동 매칭 (room_auto_match)

```go
// matchmaker goroutine - 서버 시작 시 1개만 실행
func (s *Server) matchmaker() {
    waiting := make([]*Player, 0)

    for {
        select {
        case player := <-s.matchQueue:
            waiting = append(waiting, player)

            // 2명 모이면 매칭
            if len(waiting) >= 2 {
                p1 := waiting[0]
                p2 := waiting[1]
                waiting = waiting[2:]

                s.createMatchedRoom(p1, p2)
            }

        case <-time.After(30 * time.Second):
            // 30초 타임아웃: 대기 중인 플레이어에게 알림
            for _, p := range waiting {
                p.Send("error", ErrorPayload{
                    Code:    "MATCH_TIMEOUT",
                    Message: "매칭 상대를 찾지 못했습니다",
                })
            }
            waiting = waiting[:0]
        }
    }
}

func (s *Server) createMatchedRoom(p1, p2 *Player) {
    // 방 생성 + 양쪽에 room_created, player_joined 전송
    // 자동 매칭은 ready 스킵 → 바로 match_start
    room := s.createRoom(p1)
    room.addPlayer(p2)
    room.startMatch()  // 바로 시작
}
```

---

## 5. 웨이브 동기화

서버가 웨이브 진행을 제어합니다. 클라이언트는 `wave_sync`를 받으면 다음 라운드를 시작합니다.

### 동기화 규칙

```go
func (r *Room) broadcastWaveSync() {
    r.CurrentRound++

    // 양쪽 모두에게 동시에 전송
    r.broadcast("wave_sync", WaveSyncPayload{
        Round: r.CurrentRound,
    })
}
```

**웨이브 타이밍 옵션** (선택 필요):

| 방식 | 설명 | 장점 | 단점 |
|------|------|------|------|
| **A. 서버 타이머** | 서버가 고정 간격으로 wave_sync 전송 | 간단, 동기화 보장 | 빠른 사람이 대기해야 함 |
| **B. 양쪽 모두 준비** | 양쪽 모두 라운드 완료 보고 시 다음 wave_sync | 공정 | 한쪽이 느리면 다른쪽이 대기 |
| **C. 독립 진행** | 각자 진행, 상태만 공유 | 대기 없음 | 라운드 차이 발생 |

현재 클라이언트 구현 기준으로 **B 방식 (양쪽 준비 시 진행)** 이 가장 적합합니다.
클라이언트의 `RoundManager`는 `wave_sync`가 올 때까지 30초 타임아웃으로 대기하므로,
서버는 양쪽 라운드 완료를 확인한 후 `wave_sync`를 보내면 됩니다.

```go
func (r *Room) handleGameStateUpdate(player *Player, state GameStateUpdatePayload) {
    playerIdx := r.getPlayerIndex(player)
    r.playerStates[playerIdx] = state

    // 상대에게 opponent_state 전달
    opponent := r.getOpponent(player)
    if opponent != nil {
        opponent.Send("opponent_state", OpponentStatePayload{
            PlayerName:     player.Name,
            Life:           state.Life,
            Round:          state.Round,
            Gold:           state.Gold,
            MonstersKilled: state.MonstersKilled,
            UnitCount:      state.UnitCount,
            IsAlive:        player.IsAlive,
        })
    }

    // 양쪽 라운드가 같으면 다음 웨이브 동기화
    if r.bothPlayersAtRound(r.CurrentRound) {
        r.broadcastWaveSync()
    }
}
```

---

## 6. 매치 결과 판정

```go
func (r *Room) handlePlayerDead(player *Player, payload PlayerDeadPayload) {
    player.IsAlive = false
    player.FinalRound = payload.FinalRound
    player.Contribution = payload.Contribution

    opponent := r.getOpponent(player)

    // 상대에게 사망 알림
    if opponent != nil {
        opponent.Send("opponent_dead", OpponentDeadPayload{
            PlayerName: player.Name,
            FinalRound: payload.FinalRound,
        })
    }

    // 양쪽 모두 죽었거나 한쪽이 전체 라운드(30) 클리어 → 결과 판정
    if r.shouldEndMatch() {
        r.sendMatchResults()
    }
}

func (r *Room) shouldEndMatch() bool {
    alive := 0
    for _, p := range r.Players {
        if p != nil && p.IsAlive {
            alive++
        }
    }
    // 둘 다 죽었거나, 살아있는 사람이 30라운드 클리어
    return alive == 0 || r.someoneCleared()
}

func (r *Room) sendMatchResults() {
    r.State = RoomFinished

    p1 := r.Players[0]
    p2 := r.Players[1]

    // 승자 판정: 더 높은 라운드 도달자, 같으면 기여도 비교
    p1Wins := p1.FinalRound > p2.FinalRound ||
              (p1.FinalRound == p2.FinalRound && p1.Contribution >= p2.Contribution)

    p1.Send("match_result", MatchResultPayload{
        IsWinner:             p1Wins,
        MyRound:              p1.FinalRound,
        OpponentRound:        p2.FinalRound,
        MyContribution:       p1.Contribution,
        OpponentContribution: p2.Contribution,
    })

    p2.Send("match_result", MatchResultPayload{
        IsWinner:             !p1Wins,
        MyRound:              p2.FinalRound,
        OpponentRound:        p1.FinalRound,
        MyContribution:       p2.Contribution,
        OpponentContribution: p1.Contribution,
    })

    close(r.done)  // Room goroutine 종료
}
```

---

## 7. 연결 관리 + 강제종료 처리

### 7-1. Heartbeat

- 클라이언트: 15초 간격으로 `heartbeat` 전송
- 서버: 30초 이상 메시지 없으면 연결 끊김으로 판단

```go
func (p *Player) readPump(s *Server) {
    p.Conn.SetReadDeadline(time.Now().Add(30 * time.Second))
    p.Conn.SetPongHandler(func(string) error {
        p.Conn.SetReadDeadline(time.Now().Add(30 * time.Second))
        return nil
    })
    // ...
}
```

### 7-2. 끊김 감지 방법

| 상황 | 감지 방법 | 감지 시간 |
|------|-----------|-----------|
| 앱 정상 종료 (뒤로가기 등) | OS가 TCP FIN 전송 → `ReadMessage()` 즉시 에러 | 즉시 (~1초) |
| 앱 강제 종료 (kill, 태스크 킬) | OS가 TCP RST 전송 → `ReadMessage()` 에러 | 즉시 (~1초) |
| 네트워크 끊김 (비행기 모드, WiFi 꺼짐) | heartbeat 타임아웃 → `ReadDeadline` 초과 | **최대 30초** |
| 배터리 방전 | heartbeat 타임아웃 | **최대 30초** |

### 7-3. 끊김 감지 코드

```go
func (p *Player) readPump(s *Server) {
    // readPump이 끝나면 반드시 cleanup 호출
    defer p.handleDisconnect(s)

    p.Conn.SetReadDeadline(time.Now().Add(30 * time.Second))

    for {
        _, message, err := p.Conn.ReadMessage()
        if err != nil {
            // 정상 종료, 강제 종료, 타임아웃 모두 여기서 걸림
            log.Printf("[Player %s] Connection lost: %v", p.Name, err)
            break
        }

        // 메시지가 오면 deadline 갱신 (heartbeat 포함)
        p.Conn.SetReadDeadline(time.Now().Add(30 * time.Second))

        // ... 메시지 처리
    }
}

func (p *Player) handleDisconnect(s *Server) {
    p.Conn.Close()
    close(p.sendCh)

    if p.Room != nil {
        // Room goroutine에게 끊김 통보 (채널로 전달)
        p.Room.msgCh <- RoomMessage{
            Player: p,
            Type:   "player_disconnect",
        }
    }

    log.Printf("[Player %s] Disconnected and cleaned up", p.Name)
}
```

### 7-4. 상황별 처리 로직

Room.Run() goroutine 내부에서 `player_disconnect`를 받았을 때:

```go
func (r *Room) handlePlayerDisconnect(player *Player) {
    opponent := r.getOpponent(player)

    switch r.State {

    // ─── 대기실에서 끊김: 방 폭파 ───
    case RoomWaiting:
        if opponent != nil {
            opponent.Send("error", ErrorPayload{
                Code:    "OPPONENT_LEFT",
                Message: "상대가 나갔습니다",
            })
        }
        // 방에서 플레이어 제거
        r.removePlayer(player)
        player.Room = nil

        // 남은 사람이 없으면 방 종료
        if r.playerCount() == 0 {
            close(r.done)
        }

    // ─── 게임 중 끊김: 즉시 사망 처리 ───
    case RoomPlaying:
        player.IsAlive = false
        player.FinalRound = r.CurrentRound
        player.Contribution = 0  // 강종이므로 기여도 0
        player.Room = nil

        if opponent != nil {
            // 상대에게 "상대 사망" 알림
            opponent.Send("opponent_dead", OpponentDeadPayload{
                PlayerName: player.Name,
                FinalRound: r.CurrentRound,
            })

            if !opponent.IsAlive {
                // 양쪽 다 죽음 → 결과 판정
                r.sendMatchResults()
            }
            // 상대가 살아있으면 → 상대는 계속 진행
            // 상대가 나중에 죽거나 30라운드 클리어하면 그때 결과 판정
        } else {
            // 상대도 이미 없음 → 방 종료
            close(r.done)
        }

    // ─── 이미 끝난 게임에서 끊김: 무시 ───
    case RoomFinished:
        player.Room = nil
        // cleanup만, 별도 처리 불필요
    }
}
```

### 7-5. 처리 흐름 요약

```
클라이언트 강제종료
│
├── readPump() → ReadMessage() 에러 → handleDisconnect()
│   └── Room.msgCh ← "player_disconnect"
│
└── Room.Run()이 수신
    │
    ├── [대기실] 방 폭파
    │   └── 상대에게 error("OPPONENT_LEFT") → 방 삭제
    │
    ├── [게임 중] 즉시 사망 처리
    │   ├── 끊긴 사람: isAlive=false, finalRound=현재라운드
    │   ├── 상대에게 opponent_dead 전송
    │   ├── 상대 살아있음 → 계속 진행
    │   └── 양쪽 다 죽음 → match_result 전송 → 방 종료
    │
    └── [게임 끝] 무시 (cleanup만)
```

### 7-6. 클라이언트 쪽 동작

끊긴 본인 (클라이언트):
- `WebSocketClient.OnDisconnected` 이벤트 발생
- `MultiplayerManager` 상태 → `Disconnected`
- 게임은 싱글플레이처럼 계속 진행 가능 (서버 없이)
- 결과 화면에서 멀티 결과 없이 싱글 결과만 표시

상대 (클라이언트):
- `opponent_dead` 수신 → `OpponentStatusUI`에 "상대 탈락!" 표시
- 자기 게임은 계속 진행
- 자기도 죽거나 클리어하면 `match_result` 수신 → 결과 화면

### 7-7. 방 리소스 정리

강제종료 시 서버 리소스 누수를 방지해야 합니다:

```go
func (r *Room) cleanup() {
    // 서버의 rooms 맵에서 제거
    r.server.mu.Lock()
    delete(r.server.rooms, r.Code)
    r.server.mu.Unlock()

    // 남아있는 플레이어의 Room 참조 해제
    for _, p := range r.Players {
        if p != nil {
            p.Room = nil
        }
    }

    log.Printf("[Room %s] Cleaned up", r.Code)
}
```

### 7-8. Edge Cases

| 상황 | 처리 |
|------|------|
| 양쪽 동시 강종 | 두 readPump이 각각 `player_disconnect` 전송 → Room.Run()이 순차 처리 → 두 번째에서 방 종료 |
| 게임 시작 직전 강종 (ready 후, match_start 전) | 대기실 로직 적용 → 방 폭파 |
| 강종 후 즉시 재접속 시도 | 현재는 새 연결로 처리 (기존 방 복구 불가). 재접속은 향후 확장 |
| sendCh가 가득 찬 상태에서 끊김 | `close(p.sendCh)` → writePump의 `range`가 종료 → goroutine 정리 |

---

## 8. 에러 코드

| Code | Message | 상황 |
|------|---------|------|
| `ROOM_NOT_FOUND` | 방을 찾을 수 없습니다 | 잘못된 방 코드 |
| `ROOM_FULL` | 방이 가득 찼습니다 | 이미 2인 |
| `ALREADY_IN_ROOM` | 이미 방에 참가 중입니다 | 중복 참가 |
| `NOT_IN_ROOM` | 방에 참가하지 않았습니다 | 방 없이 ready 전송 |
| `MATCH_TIMEOUT` | 매칭 상대를 찾지 못했습니다 | 자동 매칭 30초 초과 |
| `OPPONENT_LEFT` | 상대가 나갔습니다 | 대기실에서 상대 강제종료/퇴장 |
| `SERVER_ERROR` | 서버 오류가 발생했습니다 | 내부 에러 |

---

## 9. 게임 규칙 (서버가 알아야 할 값)

| 항목 | 값 |
|------|-----|
| 최대 라운드 | 30 |
| 초기 라이프 | 10 |
| 초기 골드 | 30 |
| 최대 플레이어 | 2 |
| 상태 리포트 간격 | 3초 |
| 하트비트 간격 | 15초 |
| 하트비트 타임아웃 | 30초 |
| 방 코드 형식 | 6자리 영숫자 (예: "ABC123") |

---

## 10. 프로젝트 구조 (권장)

```
lotto-defense-server/
├── main.go                 // 엔트리포인트
├── go.mod
├── server/
│   ├── server.go           // Server 구조체, HTTP 핸들러
│   ├── room.go             // Room 구조체, Run() goroutine
│   ├── player.go           // Player 구조체, readPump/writePump
│   ├── matchmaker.go       // 자동 매칭 로직
│   └── messages.go         // 메시지 타입 + Payload 구조체
└── config/
    └── config.go           // 설정값 (포트, 타임아웃 등)
```

---

## 11. 구현 우선순위

1. **메시지 파싱 + WebSocket 연결** - 기본 연결, heartbeat
2. **방 생성/참가** - room_create, room_join, room_created, player_joined
3. **매치 시작** - player_ready → match_start
4. **인게임 상태 교환** - game_state_update → opponent_state
5. **사망/결과** - player_dead → opponent_dead → match_result
6. **웨이브 동기화** - wave_sync
7. **자동 매칭** - room_auto_match + matchmaker goroutine
8. **연결 끊김 처리** - 타임아웃, 재연결
