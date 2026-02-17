# Lotto Defense - 백엔드 개발 명세서

**버전:** 1.0  
**작성일:** 2026-02-17  
**백엔드 언어:** Go (Golang)  
**프로토콜:** REST API + WebSocket

---

## 📋 목차

1. [게임 모드 개요](#게임-모드-개요)
2. [데이터베이스 스키마](#데이터베이스-스키마)
3. [REST API 명세](#rest-api-명세)
4. [WebSocket 프로토콜](#websocket-프로토콜)
5. [방 관리 시스템](#방-관리-시스템)
6. [게임 로직](#게임-로직)
7. [보안 및 인증](#보안-및-인증)

---

## 게임 모드 개요

### 1. 싱글 플레이 (Single Player)

**특징:**
- 혼자 플레이
- 로컬에서 게임 진행
- 서버는 결과 저장만 담당

**격자:**
- 5줄 x 4열 (20칸)
- 몬스터: 격자 주변 순환

**백엔드 역할:**
- ✅ 게임 결과 저장 (라운드 도달 기록)
- ✅ 퀘스트 완료 처리
- ✅ 보상 지급
- ❌ 실시간 통신 불필요

**통신:**
- REST API만 사용

---

### 2. 협동 플레이 (Co-op Mode)

**특징:**
- 2인 협동 플레이
- 실시간 동기화 필요
- 서버에서 게임 상태 관리

**격자:**
- 각 플레이어: 5줄 x 4열 (20칸)
- 레이아웃: 위아래 배치
- 가운데 몬스터 영역 공유 (양쪽 유닛이 공격 가능)

**매칭 방식:**
1. **랜덤 매칭:** 대기 중인 플레이어와 자동 매칭
2. **친구와 하기:**
   - 방 생성 → 4자리 코드 발급
   - 코드 입력 → 방 참가

**백엔드 역할:**
- ✅ 방 생성/관리
- ✅ 매칭 시스템
- ✅ 실시간 게임 상태 동기화
- ✅ 양쪽 플레이어 입력 처리
- ✅ 게임 결과 저장

**통신:**
- REST API (방 생성, 매칭)
- WebSocket (실시간 게임 플레이)

---

## 데이터베이스 스키마

### ERD 개요

```
Users (유저)
  ↓
UserStats (통계)
  ↓
GameResults (게임 기록)
  ↓
Quests (퀘스트)
  ↓
Rewards (보상)

Rooms (방)
  ↓
RoomPlayers (방 참가자)
```

---

### 1. `users` - 유저 정보

```sql
CREATE TABLE users (
    id BIGSERIAL PRIMARY KEY,
    username VARCHAR(50) UNIQUE NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    last_login TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE
);

CREATE INDEX idx_users_username ON users(username);
CREATE INDEX idx_users_email ON users(email);
```

---

### 2. `user_stats` - 유저 통계

```sql
CREATE TABLE user_stats (
    user_id BIGINT PRIMARY KEY REFERENCES users(id) ON DELETE CASCADE,
    
    -- 싱글 플레이
    single_highest_round INT DEFAULT 0,
    single_total_games INT DEFAULT 0,
    single_total_kills INT DEFAULT 0,
    
    -- 협동 플레이
    coop_highest_round INT DEFAULT 0,
    coop_total_games INT DEFAULT 0,
    coop_total_kills INT DEFAULT 0,
    coop_wins INT DEFAULT 0,
    
    -- 경제
    total_gold_earned BIGINT DEFAULT 0,
    current_gold INT DEFAULT 0,
    
    -- 퀘스트
    quests_completed INT DEFAULT 0,
    
    updated_at TIMESTAMP DEFAULT NOW()
);
```

---

### 3. `game_results` - 게임 결과

```sql
CREATE TABLE game_results (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    
    game_mode VARCHAR(20) NOT NULL, -- 'single' | 'coop'
    room_id BIGINT, -- NULL for single, FK for coop
    
    rounds_reached INT NOT NULL,
    monsters_killed INT NOT NULL,
    gold_earned INT NOT NULL,
    
    survival_time_seconds INT, -- 생존 시간
    final_army_value INT, -- 최종 유닛 가치
    
    result VARCHAR(20), -- 'victory' | 'defeat' | 'disconnect'
    
    played_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_game_results_user ON game_results(user_id);
CREATE INDEX idx_game_results_mode ON game_results(game_mode);
CREATE INDEX idx_game_results_room ON game_results(room_id);
```

---

### 4. `quests` - 퀘스트

```sql
CREATE TABLE quests (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    
    quest_type VARCHAR(50) NOT NULL, -- 'collect_archer_3', 'reach_round_10' 등
    quest_name VARCHAR(100) NOT NULL,
    quest_description TEXT,
    
    target_count INT NOT NULL, -- 목표 개수
    current_count INT DEFAULT 0, -- 현재 진행도
    
    reward_gold INT DEFAULT 0,
    reward_item VARCHAR(50),
    
    status VARCHAR(20) DEFAULT 'active', -- 'active' | 'completed' | 'claimed'
    
    created_at TIMESTAMP DEFAULT NOW(),
    completed_at TIMESTAMP,
    claimed_at TIMESTAMP
);

CREATE INDEX idx_quests_user ON quests(user_id);
CREATE INDEX idx_quests_status ON quests(status);
```

---

### 5. `rewards` - 보상 기록

```sql
CREATE TABLE rewards (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    
    reward_type VARCHAR(50) NOT NULL, -- 'quest' | 'achievement' | 'daily'
    reward_source_id BIGINT, -- quest_id or achievement_id
    
    gold_amount INT DEFAULT 0,
    item_id VARCHAR(50),
    item_count INT DEFAULT 1,
    
    claimed BOOLEAN DEFAULT FALSE,
    claimed_at TIMESTAMP,
    
    created_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_rewards_user ON rewards(user_id);
CREATE INDEX idx_rewards_claimed ON rewards(claimed);
```

---

### 6. `rooms` - 협동 플레이 방

```sql
CREATE TABLE rooms (
    id BIGSERIAL PRIMARY KEY,
    room_code CHAR(4) UNIQUE NOT NULL, -- 4자리 랜덤 코드
    
    host_user_id BIGINT NOT NULL REFERENCES users(id),
    
    room_type VARCHAR(20) NOT NULL, -- 'random' | 'private'
    max_players INT DEFAULT 2,
    current_players INT DEFAULT 1,
    
    status VARCHAR(20) DEFAULT 'waiting', -- 'waiting' | 'playing' | 'finished'
    
    current_round INT DEFAULT 1,
    shared_gold INT DEFAULT 100, -- 공유 골드
    
    created_at TIMESTAMP DEFAULT NOW(),
    started_at TIMESTAMP,
    finished_at TIMESTAMP,
    
    -- 방 삭제 시각 (게임 종료 후 30분)
    expires_at TIMESTAMP
);

CREATE INDEX idx_rooms_code ON rooms(room_code);
CREATE INDEX idx_rooms_status ON rooms(status);
CREATE INDEX idx_rooms_host ON rooms(host_user_id);
```

---

### 7. `room_players` - 방 참가자

```sql
CREATE TABLE room_players (
    id BIGSERIAL PRIMARY KEY,
    room_id BIGINT NOT NULL REFERENCES rooms(id) ON DELETE CASCADE,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    
    player_slot INT NOT NULL, -- 0 (위쪽) | 1 (아래쪽)
    
    is_ready BOOLEAN DEFAULT FALSE,
    is_connected BOOLEAN DEFAULT TRUE,
    
    kills INT DEFAULT 0,
    gold_contributed INT DEFAULT 0,
    
    joined_at TIMESTAMP DEFAULT NOW(),
    left_at TIMESTAMP,
    
    UNIQUE(room_id, player_slot)
);

CREATE INDEX idx_room_players_room ON room_players(room_id);
CREATE INDEX idx_room_players_user ON room_players(user_id);
```

---

### 8. `friendships` - 친구 관계

```sql
CREATE TABLE friendships (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    friend_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    
    status VARCHAR(20) DEFAULT 'pending', -- 'pending' | 'accepted' | 'blocked'
    
    created_at TIMESTAMP DEFAULT NOW(),
    accepted_at TIMESTAMP,
    
    UNIQUE(user_id, friend_id)
);

CREATE INDEX idx_friendships_user ON friendships(user_id);
CREATE INDEX idx_friendships_status ON friendships(status);
```

---

## REST API 명세

### Base URL
```
https://api.lottodefense.com/v1
```

### 인증
- **방식:** JWT (JSON Web Token)
- **헤더:** `Authorization: Bearer <token>`

---

### 1. 인증 (Authentication)

#### 1.1. 회원가입

```http
POST /auth/register
```

**Request Body:**
```json
{
  "username": "player123",
  "email": "player@example.com",
  "password": "securepassword"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "user_id": 12345,
    "username": "player123",
    "email": "player@example.com",
    "token": "eyJhbGciOiJIUzI1NiIs..."
  }
}
```

---

#### 1.2. 로그인

```http
POST /auth/login
```

**Request Body:**
```json
{
  "email": "player@example.com",
  "password": "securepassword"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "user_id": 12345,
    "username": "player123",
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "expires_at": "2026-02-18T10:00:00Z"
  }
}
```

---

#### 1.3. 로그아웃

```http
POST /auth/logout
```

**Headers:**
```
Authorization: Bearer <token>
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Logged out successfully"
}
```

---

### 2. 유저 정보 (Users)

#### 2.1. 내 정보 조회

```http
GET /users/me
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "user_id": 12345,
    "username": "player123",
    "email": "player@example.com",
    "created_at": "2026-01-01T00:00:00Z",
    "stats": {
      "single_highest_round": 25,
      "single_total_games": 50,
      "coop_highest_round": 30,
      "coop_total_games": 20,
      "current_gold": 5000
    }
  }
}
```

---

#### 2.2. 통계 조회

```http
GET /users/me/stats
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "single": {
      "highest_round": 25,
      "total_games": 50,
      "total_kills": 1500,
      "average_round": 18.5
    },
    "coop": {
      "highest_round": 30,
      "total_games": 20,
      "total_kills": 2000,
      "wins": 15,
      "win_rate": 0.75
    },
    "economy": {
      "total_gold_earned": 50000,
      "current_gold": 5000
    },
    "quests_completed": 35
  }
}
```

---

### 3. 게임 결과 (Game Results)

#### 3.1. 싱글 플레이 결과 저장

```http
POST /game/single/result
```

**Request Body:**
```json
{
  "rounds_reached": 25,
  "monsters_killed": 150,
  "gold_earned": 500,
  "survival_time_seconds": 1200,
  "final_army_value": 2000,
  "result": "defeat"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "game_id": 98765,
    "new_highest_round": 25,
    "rewards": [
      {
        "type": "gold",
        "amount": 500
      },
      {
        "type": "quest_progress",
        "quest_id": 123,
        "progress": "3/5"
      }
    ]
  }
}
```

---

#### 3.2. 게임 기록 조회

```http
GET /game/history?mode=single&limit=10&offset=0
```

**Query Parameters:**
- `mode`: `single` | `coop` | `all` (default: all)
- `limit`: int (default: 10, max: 50)
- `offset`: int (default: 0)

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "total": 50,
    "games": [
      {
        "game_id": 98765,
        "mode": "single",
        "rounds_reached": 25,
        "monsters_killed": 150,
        "gold_earned": 500,
        "result": "defeat",
        "played_at": "2026-02-17T18:30:00Z"
      }
    ]
  }
}
```

---

### 4. 퀘스트 (Quests)

#### 4.1. 퀘스트 목록 조회

```http
GET /quests?status=active
```

**Query Parameters:**
- `status`: `active` | `completed` | `claimed` | `all` (default: active)

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "quests": [
      {
        "quest_id": 123,
        "quest_type": "collect_archer_3",
        "quest_name": "궁수 3개 모으기",
        "description": "Rare 등급 이상 궁수 3개를 보유하세요",
        "target_count": 3,
        "current_count": 2,
        "progress": 0.66,
        "rewards": {
          "gold": 100
        },
        "status": "active"
      }
    ]
  }
}
```

---

#### 4.2. 퀘스트 진행도 업데이트

```http
POST /quests/{quest_id}/progress
```

**Request Body:**
```json
{
  "increment": 1
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "quest_id": 123,
    "current_count": 3,
    "target_count": 3,
    "completed": true,
    "completed_at": "2026-02-17T18:35:00Z"
  }
}
```

---

#### 4.3. 퀘스트 보상 수령

```http
POST /quests/{quest_id}/claim
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "quest_id": 123,
    "rewards": {
      "gold": 100
    },
    "new_gold_balance": 5100,
    "claimed_at": "2026-02-17T18:36:00Z"
  }
}
```

---

### 5. 협동 플레이 - 방 관리 (Co-op Rooms)

#### 5.1. 방 생성

```http
POST /coop/rooms
```

**Request Body:**
```json
{
  "room_type": "private"  // "random" | "private"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "room_id": 5001,
    "room_code": "A3F7",
    "host_user_id": 12345,
    "room_type": "private",
    "status": "waiting",
    "current_players": 1,
    "max_players": 2,
    "created_at": "2026-02-17T19:00:00Z"
  }
}
```

---

#### 5.2. 코드로 방 참가

```http
POST /coop/rooms/join
```

**Request Body:**
```json
{
  "room_code": "A3F7"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "room_id": 5001,
    "room_code": "A3F7",
    "player_slot": 1,  // 0 (위쪽) | 1 (아래쪽)
    "ws_url": "wss://ws.lottodefense.com/coop/5001?token=<jwt>"
  }
}
```

**Error (404 Not Found):**
```json
{
  "success": false,
  "error": {
    "code": "ROOM_NOT_FOUND",
    "message": "방을 찾을 수 없습니다"
  }
}
```

**Error (409 Conflict):**
```json
{
  "success": false,
  "error": {
    "code": "ROOM_FULL",
    "message": "방이 가득 찼습니다"
  }
}
```

---

#### 5.3. 랜덤 매칭

```http
POST /coop/matchmaking/random
```

**Request Body:**
```json
{}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "room_id": 5002,
    "room_code": "B9K2",
    "player_slot": 1,
    "matched": true,
    "ws_url": "wss://ws.lottodefense.com/coop/5002?token=<jwt>"
  }
}
```

**Response (202 Accepted) - 대기 중:**
```json
{
  "success": true,
  "data": {
    "room_id": 5003,
    "room_code": "C1D5",
    "player_slot": 0,
    "matched": false,
    "status": "waiting",
    "message": "상대를 찾는 중입니다...",
    "ws_url": "wss://ws.lottodefense.com/coop/5003?token=<jwt>"
  }
}
```

---

#### 5.4. 방 정보 조회

```http
GET /coop/rooms/{room_id}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "room_id": 5001,
    "room_code": "A3F7",
    "host_user_id": 12345,
    "room_type": "private",
    "status": "playing",
    "current_players": 2,
    "max_players": 2,
    "current_round": 5,
    "shared_gold": 250,
    "players": [
      {
        "user_id": 12345,
        "username": "player123",
        "slot": 0,
        "is_ready": true,
        "is_connected": true,
        "kills": 25,
        "gold_contributed": 100
      },
      {
        "user_id": 67890,
        "username": "player456",
        "slot": 1,
        "is_ready": true,
        "is_connected": true,
        "kills": 30,
        "gold_contributed": 150
      }
    ]
  }
}
```

---

#### 5.5. 방 나가기

```http
POST /coop/rooms/{room_id}/leave
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "방에서 나갔습니다"
}
```

---

#### 5.6. 준비 상태 변경

```http
POST /coop/rooms/{room_id}/ready
```

**Request Body:**
```json
{
  "is_ready": true
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "is_ready": true,
    "all_ready": true,
    "can_start": true
  }
}
```

---

### 6. 친구 (Friends)

#### 6.1. 친구 목록 조회

```http
GET /friends?status=accepted
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "friends": [
      {
        "user_id": 67890,
        "username": "player456",
        "status": "online",
        "is_in_game": false,
        "friendship_since": "2026-01-15T10:00:00Z"
      }
    ]
  }
}
```

---

#### 6.2. 친구 요청 보내기

```http
POST /friends/request
```

**Request Body:**
```json
{
  "username": "player456"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "friendship_id": 999,
    "friend_id": 67890,
    "status": "pending"
  }
}
```

---

#### 6.3. 친구 요청 수락

```http
POST /friends/{friendship_id}/accept
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "friendship_id": 999,
    "friend_id": 67890,
    "status": "accepted",
    "accepted_at": "2026-02-17T19:30:00Z"
  }
}
```

---

## WebSocket 프로토콜

### 연결 (Connection)

**URL:**
```
wss://ws.lottodefense.com/coop/{room_id}?token=<jwt>
```

**인증:**
- Query parameter로 JWT 전달
- 서버가 토큰 검증 후 연결 수락/거부

**연결 성공 메시지:**
```json
{
  "type": "connected",
  "data": {
    "room_id": 5001,
    "player_slot": 0,
    "message": "Connected to room A3F7"
  }
}
```

---

### 메시지 형식

모든 WebSocket 메시지는 다음 구조를 따름:

```json
{
  "type": "message_type",
  "timestamp": "2026-02-17T19:00:00Z",
  "data": { ... }
}
```

---

### 클라이언트 → 서버 메시지

#### 1. 준비 완료

```json
{
  "type": "player_ready",
  "data": {
    "is_ready": true
  }
}
```

---

#### 2. 유닛 배치

```json
{
  "type": "unit_placed",
  "data": {
    "unit_id": "archer_001",
    "unit_type": "Archer",
    "rarity": "Rare",
    "grid_x": 2,
    "grid_y": 3,
    "cost": 50
  }
}
```

---

#### 3. 유닛 합성

```json
{
  "type": "unit_synthesized",
  "data": {
    "source_unit_id": "archer_001",
    "target_unit_id": "archer_002",
    "result_unit_id": "archer_003",
    "result_rarity": "Epic"
  }
}
```

---

#### 4. 유닛 판매

```json
{
  "type": "unit_sold",
  "data": {
    "unit_id": "warrior_005",
    "sell_price": 30
  }
}
```

---

#### 5. 라운드 시작 준비

```json
{
  "type": "round_ready",
  "data": {
    "round": 1
  }
}
```

---

#### 6. 채팅 메시지

```json
{
  "type": "chat_message",
  "data": {
    "message": "Let's do this!"
  }
}
```

---

### 서버 → 클라이언트 메시지

#### 1. 게임 상태 동기화

```json
{
  "type": "game_state",
  "timestamp": "2026-02-17T19:05:00Z",
  "data": {
    "round": 5,
    "shared_gold": 250,
    "players": [
      {
        "slot": 0,
        "user_id": 12345,
        "username": "player123",
        "is_ready": true,
        "kills": 25,
        "units": [
          {
            "unit_id": "archer_001",
            "unit_type": "Archer",
            "rarity": "Rare",
            "grid_x": 2,
            "grid_y": 3,
            "level": 1,
            "health": 100,
            "mana": 50
          }
        ]
      },
      {
        "slot": 1,
        "user_id": 67890,
        "username": "player456",
        "is_ready": true,
        "kills": 30,
        "units": [ ... ]
      }
    ],
    "monsters": [
      {
        "monster_id": "goblin_001",
        "monster_type": "Goblin",
        "health": 80,
        "max_health": 100,
        "position": {
          "x": 2.5,
          "y": 4.0
        },
        "waypoint_index": 5
      }
    ]
  }
}
```

---

#### 2. 상대 플레이어 행동 알림

```json
{
  "type": "opponent_action",
  "timestamp": "2026-02-17T19:05:15Z",
  "data": {
    "action": "unit_placed",
    "player_slot": 1,
    "details": {
      "unit_id": "mage_005",
      "unit_type": "Mage",
      "rarity": "Epic",
      "grid_x": 1,
      "grid_y": 2
    }
  }
}
```

---

#### 3. 라운드 시작

```json
{
  "type": "round_started",
  "timestamp": "2026-02-17T19:05:30Z",
  "data": {
    "round": 6,
    "monster_count": 15,
    "monster_type": "Orc",
    "boss": false
  }
}
```

---

#### 4. 라운드 완료

```json
{
  "type": "round_completed",
  "timestamp": "2026-02-17T19:06:00Z",
  "data": {
    "round": 6,
    "success": true,
    "rewards": {
      "gold": 100,
      "bonus_gold": 20
    },
    "next_round": 7
  }
}
```

---

#### 5. 몬스터 스폰

```json
{
  "type": "monster_spawned",
  "timestamp": "2026-02-17T19:05:35Z",
  "data": {
    "monster_id": "orc_012",
    "monster_type": "Orc",
    "health": 150,
    "max_health": 150,
    "speed": 1.5,
    "position": {
      "x": 0.0,
      "y": 0.0
    }
  }
}
```

---

#### 6. 몬스터 피해

```json
{
  "type": "monster_damaged",
  "timestamp": "2026-02-17T19:05:40Z",
  "data": {
    "monster_id": "orc_012",
    "damage": 25,
    "current_health": 125,
    "max_health": 150,
    "attacker_slot": 0,
    "attacker_unit_id": "archer_001"
  }
}
```

---

#### 7. 몬스터 사망

```json
{
  "type": "monster_killed",
  "timestamp": "2026-02-17T19:05:45Z",
  "data": {
    "monster_id": "orc_012",
    "killer_slot": 0,
    "killer_unit_id": "archer_001",
    "gold_reward": 10
  }
}
```

---

#### 8. 골드 변경

```json
{
  "type": "gold_changed",
  "timestamp": "2026-02-17T19:05:46Z",
  "data": {
    "shared_gold": 260,
    "change": 10,
    "reason": "monster_kill"
  }
}
```

---

#### 9. 게임 종료

```json
{
  "type": "game_ended",
  "timestamp": "2026-02-17T19:10:00Z",
  "data": {
    "result": "victory",
    "final_round": 30,
    "reason": "boss_defeated",
    "stats": {
      "total_kills": 250,
      "total_gold_earned": 5000,
      "survival_time_seconds": 1800,
      "players": [
        {
          "slot": 0,
          "user_id": 12345,
          "kills": 120,
          "gold_contributed": 2500
        },
        {
          "slot": 1,
          "user_id": 67890,
          "kills": 130,
          "gold_contributed": 2500
        }
      ]
    }
  }
}
```

---

#### 10. 플레이어 연결 끊김

```json
{
  "type": "player_disconnected",
  "timestamp": "2026-02-17T19:08:00Z",
  "data": {
    "player_slot": 1,
    "user_id": 67890,
    "reason": "connection_lost",
    "grace_period_seconds": 30
  }
}
```

---

#### 11. 플레이어 재연결

```json
{
  "type": "player_reconnected",
  "timestamp": "2026-02-17T19:08:20Z",
  "data": {
    "player_slot": 1,
    "user_id": 67890
  }
}
```

---

#### 12. 채팅 메시지

```json
{
  "type": "chat_message",
  "timestamp": "2026-02-17T19:06:00Z",
  "data": {
    "player_slot": 0,
    "user_id": 12345,
    "username": "player123",
    "message": "Nice shot!"
  }
}
```

---

#### 13. 에러

```json
{
  "type": "error",
  "timestamp": "2026-02-17T19:05:50Z",
  "data": {
    "code": "INSUFFICIENT_GOLD",
    "message": "골드가 부족합니다",
    "details": {
      "required": 100,
      "available": 50
    }
  }
}
```

---

## 방 관리 시스템

### 고루틴 기반 독립 관리

각 방은 **독립적인 고루틴**에서 실행되어 서로 영향을 주지 않음.

```go
// 방 구조체
type Room struct {
    ID          int64
    Code        string
    HostUserID  int64
    RoomType    string // "random" | "private"
    Status      string // "waiting" | "playing" | "finished"
    
    Players     [2]*Player
    MaxPlayers  int
    
    CurrentRound int
    SharedGold   int
    
    Monsters    []*Monster
    
    // 동기화
    mu          sync.RWMutex
    
    // 채널
    actionChan  chan Action
    doneChan    chan struct{}
    
    CreatedAt   time.Time
    StartedAt   *time.Time
    FinishedAt  *time.Time
    ExpiresAt   time.Time
}

// 방 관리자
type RoomManager struct {
    rooms       map[int64]*Room
    roomsByCode map[string]*Room
    mu          sync.RWMutex
    
    // 랜덤 매칭 큐
    matchmakingQueue chan *MatchmakingRequest
}
```

---

### 방 생명주기

```go
func (rm *RoomManager) CreateRoom(hostUserID int64, roomType string) (*Room, error) {
    room := &Room{
        ID:         generateRoomID(),
        Code:       generateRoomCode(), // 4자리 랜덤
        HostUserID: hostUserID,
        RoomType:   roomType,
        Status:     "waiting",
        MaxPlayers: 2,
        SharedGold: 100,
        actionChan: make(chan Action, 100),
        doneChan:   make(chan struct{}),
        CreatedAt:  time.Now(),
        ExpiresAt:  time.Now().Add(30 * time.Minute),
    }
    
    rm.mu.Lock()
    rm.rooms[room.ID] = room
    rm.roomsByCode[room.Code] = room
    rm.mu.Unlock()
    
    // 방 전용 고루틴 시작
    go room.Run()
    
    return room, nil
}

func (r *Room) Run() {
    ticker := time.NewTicker(100 * time.Millisecond) // 10 FPS
    defer ticker.Stop()
    
    for {
        select {
        case <-r.doneChan:
            // 방 종료
            return
            
        case action := <-r.actionChan:
            // 플레이어 액션 처리
            r.handleAction(action)
            
        case <-ticker.C:
            if r.Status == "playing" {
                // 게임 틱 업데이트
                r.updateGameState()
                r.broadcastGameState()
            }
        }
    }
}
```

---

### 액션 처리

```go
type Action struct {
    Type       string      // "unit_placed", "unit_sold", etc.
    PlayerSlot int         // 0 | 1
    Data       interface{}
}

func (r *Room) handleAction(action Action) {
    r.mu.Lock()
    defer r.mu.Unlock()
    
    switch action.Type {
    case "unit_placed":
        r.handleUnitPlaced(action)
    case "unit_sold":
        r.handleUnitSold(action)
    case "unit_synthesized":
        r.handleUnitSynthesized(action)
    case "round_ready":
        r.handleRoundReady(action)
    }
}
```

---

### 게임 상태 동기화

```go
func (r *Room) updateGameState() {
    // 몬스터 이동
    for _, monster := range r.Monsters {
        monster.Move(0.1) // deltaTime
    }
    
    // 유닛 공격
    for _, player := range r.Players {
        if player == nil {
            continue
        }
        for _, unit := range player.Units {
            target := r.findTarget(unit)
            if target != nil {
                damage := unit.Attack(target)
                if damage > 0 {
                    r.broadcastMonsterDamaged(target, damage, player.Slot, unit.ID)
                    
                    if target.Health <= 0 {
                        r.handleMonsterKilled(target, player.Slot, unit.ID)
                    }
                }
            }
        }
    }
    
    // 유닛 마나 재생
    for _, player := range r.Players {
        if player == nil {
            continue
        }
        for _, unit := range player.Units {
            unit.RegenerateMana(0.1)
            if unit.Mana >= unit.MaxMana {
                unit.ActivateSkill()
                r.broadcastSkillActivated(player.Slot, unit.ID)
            }
        }
    }
    
    // 라운드 완료 체크
    if len(r.Monsters) == 0 && r.Status == "playing" {
        r.completeRound()
    }
}
```

---

### 랜덤 매칭

```go
type MatchmakingRequest struct {
    UserID     int64
    ResponseCh chan *Room
}

func (rm *RoomManager) StartMatchmaking(userID int64) (*Room, error) {
    req := &MatchmakingRequest{
        UserID:     userID,
        ResponseCh: make(chan *Room),
    }
    
    // 매칭 큐에 추가
    rm.matchmakingQueue <- req
    
    // 매칭 완료 대기 (타임아웃 30초)
    select {
    case room := <-req.ResponseCh:
        return room, nil
    case <-time.After(30 * time.Second):
        return nil, errors.New("matchmaking timeout")
    }
}

func (rm *RoomManager) RunMatchmaker() {
    var waitingRequest *MatchmakingRequest
    
    for req := range rm.matchmakingQueue {
        if waitingRequest == nil {
            // 첫 번째 플레이어 - 방 생성
            room, _ := rm.CreateRoom(req.UserID, "random")
            
            // 두 번째 플레이어 대기
            waitingRequest = req
            
        } else {
            // 두 번째 플레이어 - 방 참가
            room, _ := rm.getRoomByHostID(waitingRequest.UserID)
            room.AddPlayer(req.UserID, 1)
            
            // 양쪽 플레이어에게 알림
            waitingRequest.ResponseCh <- room
            req.ResponseCh <- room
            
            waitingRequest = nil
        }
    }
}
```

---

## 게임 로직

### 협동 플레이 격자 레이아웃

```
┌─────────────────────────┐
│   Player 1 (Slot 0)     │  ← 위쪽 플레이어
│   5줄 x 4열             │
│ ┌─┬─┬─┬─┐              │
│ ├─┼─┼─┼─┤              │
│ ├─┼─┼─┼─┤              │
│ ├─┼─┼─┼─┤              │
│ └─┴─┴─┴─┘              │
├─────────────────────────┤
│   Shared Monster Zone   │  ← 공유 몬스터 영역
│   [👹] [👹] [👹]       │  (양쪽 유닛이 공격 가능)
├─────────────────────────┤
│ ┌─┬─┬─┬─┐              │
│ ├─┼─┼─┼─┤              │
│ ├─┼─┼─┼─┤              │
│ ├─┼─┼─┼─┤              │
│ └─┴─┴─┴─┘              │
│   5줄 x 4열             │
│   Player 2 (Slot 1)     │  ← 아래쪽 플레이어
└─────────────────────────┘
```

---

### 공유 골드 시스템

- **초기 골드:** 각 플레이어 100 (총 200)
- **골드 획득:** 몬스터 처치 시 **공유 풀**에 추가
- **골드 사용:** 공유 풀에서 차감
- **동기화:** 양쪽 플레이어가 실시간으로 같은 골드 잔액 확인

```go
func (r *Room) SpendGold(amount int) error {
    r.mu.Lock()
    defer r.mu.Unlock()
    
    if r.SharedGold < amount {
        return errors.New("insufficient gold")
    }
    
    r.SharedGold -= amount
    r.broadcastGoldChanged(-amount, "unit_purchase")
    return nil
}

func (r *Room) EarnGold(amount int, reason string) {
    r.mu.Lock()
    defer r.mu.Unlock()
    
    r.SharedGold += amount
    r.broadcastGoldChanged(amount, reason)
}
```

---

### 몬스터 타게팅

- 각 유닛은 **가장 가까운 몬스터**를 공격
- 거리 계산: 유닛 위치 ↔ 몬스터 위치
- 범위: 유닛의 `attackRange` 내에 있는 몬스터만

```go
func (r *Room) findTarget(unit *Unit) *Monster {
    var closestMonster *Monster
    closestDistance := math.MaxFloat64
    
    for _, monster := range r.Monsters {
        if monster.Health <= 0 {
            continue
        }
        
        distance := calculateDistance(unit.Position, monster.Position)
        
        if distance <= unit.AttackRange && distance < closestDistance {
            closestMonster = monster
            closestDistance = distance
        }
    }
    
    return closestMonster
}
```

---

### 라운드 진행

1. **준비 단계 (Preparation)**
   - 양쪽 플레이어 "준비 완료" 클릭
   - 모두 준비 완료 시 라운드 시작

2. **전투 단계 (Combat)**
   - 몬스터 스폰
   - 유닛 자동 공격
   - 몬스터 이동
   - 스킬 자동 발동

3. **완료 단계 (Completion)**
   - 모든 몬스터 처치 → 라운드 성공
   - 보상 지급 (골드)
   - 다음 라운드 준비

4. **실패 조건**
   - 몬스터가 끝까지 도달 (예: 순환 경로 10바퀴)
   - 플레이어 연결 끊김 (30초 grace period)

---

## 보안 및 인증

### JWT (JSON Web Token)

**토큰 구조:**
```json
{
  "user_id": 12345,
  "username": "player123",
  "exp": 1708387200,
  "iat": 1708300800
}
```

**발급:**
- 로그인 성공 시
- 만료 시간: 24시간

**검증:**
- 모든 REST API 요청: `Authorization: Bearer <token>`
- WebSocket 연결: Query parameter `?token=<jwt>`

---

### Rate Limiting

**제한:**
- REST API: 60 requests/min per IP
- WebSocket 메시지: 30 messages/sec per connection

**초과 시:**
```json
{
  "success": false,
  "error": {
    "code": "RATE_LIMIT_EXCEEDED",
    "message": "요청이 너무 많습니다. 잠시 후 다시 시도하세요.",
    "retry_after": 30
  }
}
```

---

### 입력 검증

**클라이언트 입력:**
- 모든 좌표, ID, 수량 검증
- 격자 범위 체크 (0 ≤ x < 4, 0 ≤ y < 5)
- 골드 잔액 체크
- 유닛 소유권 검증

**SQL Injection 방지:**
- Prepared statements 사용
- ORM (GORM) 사용 권장

---

### 치팅 방지

**서버 권위 (Server Authority):**
- 모든 게임 로직은 **서버에서 실행**
- 클라이언트는 입력만 전송, 결과는 서버에서 수신
- 유닛 배치, 합성, 공격, 골드 변경 모두 서버 검증

**예시:**
```go
// 클라이언트가 "유닛 배치" 요청
// 서버가 검증:
func (r *Room) handleUnitPlaced(action Action) {
    player := r.Players[action.PlayerSlot]
    
    // 1. 골드 충분한가?
    if r.SharedGold < action.Data.Cost {
        r.sendError(action.PlayerSlot, "INSUFFICIENT_GOLD")
        return
    }
    
    // 2. 격자 칸이 비어있는가?
    if r.isCellOccupied(action.PlayerSlot, action.Data.GridX, action.Data.GridY) {
        r.sendError(action.PlayerSlot, "CELL_OCCUPIED")
        return
    }
    
    // 3. 유닛 타입이 유효한가?
    if !isValidUnitType(action.Data.UnitType) {
        r.sendError(action.PlayerSlot, "INVALID_UNIT_TYPE")
        return
    }
    
    // 검증 통과 → 실행
    r.SharedGold -= action.Data.Cost
    player.PlaceUnit(action.Data)
    r.broadcastUnitPlaced(action)
}
```

---

## 기술 스택 (Go)

### 추천 라이브러리

```go
// HTTP 프레임워크
"github.com/gin-gonic/gin"

// WebSocket
"github.com/gorilla/websocket"

// 데이터베이스
"gorm.io/gorm"
"gorm.io/driver/postgres"

// JWT
"github.com/golang-jwt/jwt/v5"

// 암호화
"golang.org/x/crypto/bcrypt"

// 환경 변수
"github.com/joho/godotenv"

// Redis (세션, 매칭 큐)
"github.com/go-redis/redis/v8"

// 로깅
"github.com/sirupsen/logrus"
```

---

### 프로젝트 구조

```
lotto-defense-backend/
├── cmd/
│   └── server/
│       └── main.go
├── internal/
│   ├── api/
│   │   ├── handlers/
│   │   │   ├── auth.go
│   │   │   ├── users.go
│   │   │   ├── game.go
│   │   │   ├── quests.go
│   │   │   ├── coop.go
│   │   │   └── friends.go
│   │   ├── middleware/
│   │   │   ├── auth.go
│   │   │   └── ratelimit.go
│   │   └── router.go
│   ├── websocket/
│   │   ├── client.go
│   │   ├── hub.go
│   │   └── message.go
│   ├── room/
│   │   ├── room.go
│   │   ├── manager.go
│   │   ├── matchmaking.go
│   │   └── game_logic.go
│   ├── models/
│   │   ├── user.go
│   │   ├── room.go
│   │   ├── quest.go
│   │   └── game_result.go
│   ├── repository/
│   │   ├── user_repo.go
│   │   ├── room_repo.go
│   │   └── quest_repo.go
│   ├── service/
│   │   ├── auth_service.go
│   │   ├── game_service.go
│   │   └── quest_service.go
│   └── config/
│       └── config.go
├── migrations/
│   ├── 001_create_users.sql
│   ├── 002_create_rooms.sql
│   └── ...
├── go.mod
├── go.sum
├── .env.example
└── README.md
```

---

## 배포 및 스케일링

### Docker Compose

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:15
    environment:
      POSTGRES_DB: lottodefense
      POSTGRES_USER: admin
      POSTGRES_PASSWORD: secret
    volumes:
      - postgres_data:/var/lib/postgresql/data
    ports:
      - "5432:5432"
  
  redis:
    image: redis:7
    ports:
      - "6379:6379"
  
  backend:
    build: .
    ports:
      - "8080:8080"
    environment:
      DATABASE_URL: postgres://admin:secret@postgres:5432/lottodefense
      REDIS_URL: redis://redis:6379
      JWT_SECRET: your-secret-key
    depends_on:
      - postgres
      - redis

volumes:
  postgres_data:
```

---

### 스케일링 고려사항

**수평 확장 (Horizontal Scaling):**
- API 서버: 여러 인스턴스 (Load Balancer)
- WebSocket 서버: Sticky sessions 필요
- Redis: 매칭 큐, 세션 관리 공유

**방 분산:**
- 각 방은 독립적인 고루틴
- 서버 간 방 분산 가능
- Redis Pub/Sub로 서버 간 통신

---

## 테스트 시나리오

### 1. 싱글 플레이
1. 회원가입/로그인
2. 게임 시작 (로컬)
3. 라운드 플레이
4. 게임 종료
5. 결과 저장 (POST /game/single/result)
6. 퀘스트 진행도 확인
7. 보상 수령

### 2. 협동 플레이 - 친구와 하기
1. Player1: 방 생성 (POST /coop/rooms)
2. Player1: 4자리 코드 확인 (A3F7)
3. Player2: 코드 입력 참가 (POST /coop/rooms/join)
4. 양쪽: WebSocket 연결
5. 양쪽: 준비 완료
6. 게임 시작
7. 실시간 동기화 확인
8. 게임 종료
9. 결과 저장

### 3. 협동 플레이 - 랜덤 매칭
1. Player1: 랜덤 매칭 (POST /coop/matchmaking/random)
2. Player2: 랜덤 매칭
3. 자동 매칭 성공
4. 게임 진행
5. 결과 저장

---

## 다음 단계

1. **데이터베이스 마이그레이션 작성**
2. **Go 프로젝트 초기화**
3. **REST API 구현 (인증부터)**
4. **WebSocket 서버 구현**
5. **방 관리 시스템 구현**
6. **게임 로직 구현**
7. **Unity 클라이언트 연동**
8. **테스트 및 디버깅**

---

**문서 버전:** 1.0  
**최종 수정:** 2026-02-17  
**작성자:** AI Assistant

---

이 문서는 백엔드 개발의 기준점입니다. 구현 과정에서 추가/수정이 필요하면 업데이트하세요.
