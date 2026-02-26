# Lotto Defense - API 명세서

> 클라이언트-서버 연동 관점의 REST API 및 WebSocket 명세.
> WebSocket 멀티플레이어 상세는 `docs/backend-server-spec.md` 참조.

---

## 1. 서버 정보

| 항목 | 값 |
|------|-----|
| REST Base URL | `http://localhost:18082/api/v1/td` |
| WebSocket URL | `ws://localhost:8080/ws` |
| 인증 방식 | JWT Bearer Token |
| 토큰 저장 | PlayerPrefs (`td_jwt_token`, `td_username`) |

> 소스: `Assets/Scripts/Backend/APIClient.cs`, `Assets/Scripts/UI/MultiplayerLobbyUI.cs`

---

## 2. REST API 엔드포인트

### 2.1 인증

#### POST `/auth/register` - 회원가입

**Request:**
```json
{
  "username": "string",
  "email": "string",
  "password": "string"
}
```

**Response:**
```json
{
  "user": {
    "id": 1,
    "username": "string",
    "email": "string",
    "created_at": "2025-01-01T00:00:00Z",
    "last_login": "2025-01-01T00:00:00Z",
    "nickname": "string",
    "avatar_id": "string"
  },
  "token": "jwt_token_string"
}
```

**호출 위치:** `Assets/Scripts/Backend/UI/LoginUI.cs`

---

#### POST `/auth/login` - 로그인

**Request:**
```json
{
  "email": "string",
  "password": "string"
}
```

**Response:** AuthResponse (위와 동일)

**호출 위치:** `Assets/Scripts/Backend/UI/LoginUI.cs`

---

### 2.2 사용자

#### GET `/users/me` - 유저 정보 조회

**Headers:** `Authorization: Bearer {token}`

**Response:**
```json
{
  "user": {
    "id": 1,
    "username": "string",
    "email": "string",
    "created_at": "string",
    "last_login": "string",
    "nickname": "string",
    "avatar_id": "string"
  },
  "stats": {
    "single_highest_round": 10,
    "single_total_games": 50,
    "single_total_kills": 1500,
    "coop_highest_round": 8,
    "coop_total_games": 20,
    "coop_total_kills": 600,
    "coop_wins": 12,
    "total_gold_earned": 5000,
    "current_gold": 1200,
    "quests_completed": 5
  }
}
```

**호출 위치:** `APIManager.cs` (정의됨, 현재 미사용)

---

#### GET `/users/me/stats` - 게임 통계

**Headers:** `Authorization: Bearer {token}`

**Response:**
```json
{
  "single": {
    "highest_round": 10,
    "total_games": 50,
    "total_kills": 1500,
    "average_round": 7.5
  },
  "coop": {
    "highest_round": 8,
    "total_games": 20,
    "total_kills": 600,
    "wins": 12,
    "win_rate": 0.6
  },
  "gold": {
    "total_earned": 5000,
    "current": 1200
  }
}
```

**호출 위치:** `Assets/Scripts/Backend/UI/StatsUI.cs`

---

### 2.3 게임

#### POST `/game/single/result` - 싱글 게임 결과 저장

**Headers:** `Authorization: Bearer {token}`

**Request:**
```json
{
  "game_mode": "single",
  "rounds_reached": 10,
  "monsters_killed": 250,
  "gold_earned": 500,
  "survival_time_seconds": 300,
  "final_army_value": 1500,
  "result": "victory"
}
```

**Response:**
```json
{
  "game_id": 123,
  "new_highest_round": 10,
  "rewards": [
    {
      "type": "gold",
      "amount": 100,
      "item_id": null
    }
  ]
}
```

**호출 위치:** `Assets/Scripts/Gameplay/GameplayManager.cs`

---

#### GET `/game/history` - 게임 히스토리

**Headers:** `Authorization: Bearer {token}`

**Query params:** `mode=single&limit=10&offset=0`

**Response:**
```json
{
  "total": 50,
  "games": [
    {
      "game_id": 123,
      "game_mode": "single",
      "rounds_reached": 10,
      "monsters_killed": 250,
      "gold_earned": 500,
      "survival_time_seconds": 300,
      "result": "victory",
      "played_at": "2025-01-01T12:00:00Z"
    }
  ]
}
```

**호출 위치:** `APIManager.cs` (정의됨, 현재 미사용)

---

### 2.4 랭킹

#### GET `/rankings/{gameMode}` - 주간 랭킹

**Path params:** `gameMode` = `"single"` 또는 `"coop"`

**Response:**
```json
{
  "game_mode": "single",
  "rankings": [
    {
      "rank": 1,
      "user_id": 42,
      "username": "player1",
      "rounds_reached": 10,
      "survival_time_seconds": 450,
      "survival_minutes": 7.5,
      "played_at": "2025-01-01T12:00:00Z",
      "player2_id": null,
      "player2_username": null
    }
  ]
}
```

**호출 위치:** `Assets/Scripts/Backend/UI/RankingUI.cs`

---

## 3. API 응답 래퍼

모든 REST 응답은 다음 형태로 감싸짐:

```json
{
  "success": true,
  "data": { ... }
}
```

- `success: true` → `data` 파싱 후 콜백
- `success: false` → 에러 콜백

---

## 4. 에러 처리

| 상황 | 처리 |
|------|------|
| 미인증 상태에서 API 호출 | `onError("Not logged in")` 즉시 반환 |
| 네트워크 에러 | UnityWebRequest.error 메시지 전달 |
| JSON 파싱 실패 | `"JSON parse error: {message}"` 전달 |
| 서버 에러 (success=false) | `"Request failed"` 전달 |

---

## 5. WebSocket 메시지 (요약)

> 상세: `docs/backend-server-spec.md`
> 소스: `Assets/Scripts/Networking/NetworkMessages.cs`

### 메시지 봉투
```json
{
  "type": "message_type",
  "payload": "json_string"
}
```

### 클라이언트 → 서버

| 타입 | 필드 |
|------|------|
| `room_create` | playerName |
| `room_join` | roomCode, playerName |
| `room_auto_match` | playerName |
| `player_ready` | roomCode |
| `game_state_update` | life, round, gold, monstersKilled, unitCount |
| `player_dead` | finalRound, contribution |
| `heartbeat` | timestamp (Unix ms) |

### 서버 → 클라이언트

| 타입 | 필드 |
|------|------|
| `room_created` | roomCode, roomId |
| `player_joined` | playerName, playerCount |
| `match_start` | totalPlayers, startRound |
| `wave_sync` | round |
| `opponent_state` | playerName, life, round, gold, monstersKilled, unitCount, isAlive |
| `opponent_dead` | playerName, finalRound |
| `match_result` | isWinner, myRound, opponentRound, myContribution, opponentContribution |
| `error` | code, message |

### 연결 설정

| 항목 | 값 |
|------|-----|
| 하트비트 간격 | 15초 |
| 재접속 딜레이 | 3초 |
| 최대 재접속 시도 | 5회 |
| 상태 동기화 간격 | 3초 (REST 폴링) |

---

## 6. 호출 위치 매핑

| 화면/스크립트 | 사용하는 API |
|-------------|-------------|
| LoginUI | Register, Login |
| StatsUI | GetStats |
| RankingUI | GetWeeklyRankings |
| GameplayManager | SaveGameResult |
| MultiplayerLobbyUI | WebSocket 전체 |
| (미사용) | GetUserInfo, GetGameHistory |
