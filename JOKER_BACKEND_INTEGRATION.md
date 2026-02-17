# joker_backend 통합 가이드 (Cursor AI)

**목표:** lotto-defense-backend → joker_backend 통합  
**방식:** Cursor AI 사용  
**예상 시간:** 2-3시간

---

## 📋 준비 사항

### 1. 파일 복사 준비

**복사할 파일 위치:**
```
~/project/lotto-defense-backend/
├── internal/models/           ← 복사
│   ├── user.go
│   ├── game.go
│   ├── quest.go
│   ├── room.go
│   └── friendship.go
└── internal/repository/       ← 복사
    ├── user_repository.go
    ├── game_repository.go
    └── room_repository.go
```

---

## 🚀 Cursor AI 통합 단계

### Step 1: 프로젝트 준비 (1분)

**Cursor IDE에서:**

1. joker_backend 프로젝트 열기
2. 다음 문서 추가:
   - `~/project/Lotto-defense/BACKEND_SPEC.md`
   - `~/project/Lotto-defense/JOKER_BACKEND_INTEGRATION.md` (이 파일)

---

### Step 2: 아키텍처 분석 (Cursor AI)

**Cursor AI에게 보낼 프롬프트:**

```
joker_backend 프로젝트의 아키텍처를 분석해줘.

확인할 것:
1. 디렉토리 구조 (models, repository, service, handlers 등)
2. Echo 프레임워크 사용 방식
3. GORM 모델 정의 패턴
4. Repository 인터페이스/구현 패턴
5. Service 레이어 패턴
6. 핸들러 작성 스타일
7. 라우터 설정 방식
8. 미들웨어 적용 방식
9. 에러 핸들링 방식
10. JSON 응답 포맷

기존 패턴을 완벽히 따라서 Lotto Defense 기능을 추가할 거야.
기존 코드 예제를 보여줘.
```

**Cursor AI 응답 확인:**
- 기존 모델 예제
- 기존 Repository 예제
- 기존 Service 예제
- 기존 Handler 예제

---

### Step 3: 모델 통합 (Cursor AI)

**Cursor AI에게 보낼 프롬프트:**

```
Lotto Defense 게임용 GORM 모델을 추가해줘.

기존 모델 스타일 참조:
@models/existing_model.go

추가할 모델 (총 8개):

1. LottoUser (테이블: lotto_users)
   - ID, Username, Email, PasswordHash
   - CreatedAt, UpdatedAt, LastLogin
   - IsActive

2. LottoUserStats (테이블: lotto_user_stats)
   - UserID (FK)
   - SingleHighestRound, SingleTotalGames, SingleTotalKills
   - CoopHighestRound, CoopTotalGames, CoopTotalKills, CoopWins
   - TotalGoldEarned, CurrentGold
   - QuestsCompleted

3. LottoGameResult (테이블: lotto_game_results)
   - ID, UserID, GameMode (single/coop)
   - RoundsReached, MonstersKilled, GoldEarned
   - SurvivalTimeSeconds, FinalArmyValue
   - Result (victory/defeat/disconnect)
   - PlayedAt

4. LottoQuest (테이블: lotto_quests)
   - ID, UserID
   - QuestType, QuestName, QuestDescription
   - TargetCount, CurrentCount
   - RewardGold, RewardItem
   - Status (active/completed/claimed)
   - CreatedAt, CompletedAt, ClaimedAt

5. LottoReward (테이블: lotto_rewards)
   - ID, UserID
   - RewardType, RewardSourceID
   - GoldAmount, ItemID, ItemCount
   - Claimed, ClaimedAt

6. LottoRoom (테이블: lotto_rooms)
   - ID, RoomCode (4자리)
   - HostUserID
   - RoomType (random/private)
   - MaxPlayers, CurrentPlayers
   - Status (waiting/playing/finished)
   - CurrentRound, SharedGold
   - CreatedAt, StartedAt, FinishedAt, ExpiresAt

7. LottoRoomPlayer (테이블: lotto_room_players)
   - ID, RoomID, UserID
   - PlayerSlot (0/1)
   - IsReady, IsConnected
   - Kills, GoldContributed
   - JoinedAt, LeftAt

8. LottoFriendship (테이블: lotto_friendships)
   - ID, UserID, FriendID
   - Status (pending/accepted/blocked)
   - CreatedAt, AcceptedAt

요구사항:
- 기존 모델 네이밍 컨벤션 따르기
- GORM 태그 정확하게
- JSON 태그 추가
- 관계(Association) 정의
- TableName() 메서드

파일 위치: 기존 models/ 디렉토리에 생성
파일명: 
- models/lotto_user.go
- models/lotto_game.go
- models/lotto_quest.go
- models/lotto_room.go
- models/lotto_friendship.go
```

**Cursor AI가 생성할 것:**
- 5개 파일 (models/)
- 8개 모델 정의
- 기존 패턴 완벽 매칭

---

### Step 4: Repository 통합 (Cursor AI)

**Cursor AI에게 보낼 프롬프트:**

```
Lotto Defense용 Repository를 추가해줘.

기존 Repository 스타일 참조:
@repository/existing_repository.go

추가할 Repository (총 5개):

1. LottoUserRepository
   인터페이스:
   - Create(user *LottoUser)
   - GetByID(id)
   - GetByEmail(email)
   - GetByUsername(username)
   - Update(user)
   - UpdateLastLogin(id)
   - GetStats(userID)
   - CreateStats(stats)
   - UpdateStats(stats)

2. LottoGameRepository
   인터페이스:
   - Create(result)
   - GetByID(id)
   - GetHistory(userID, gameMode, limit, offset)
   - GetHighestRound(userID, gameMode)
   - GetTotalKills(userID, gameMode)

3. LottoQuestRepository
   인터페이스:
   - Create(quest)
   - GetByID(id)
   - GetActiveQuests(userID)
   - GetCompletedQuests(userID)
   - UpdateProgress(questID, increment)
   - CompleteQuest(questID)
   - ClaimQuest(questID)

4. LottoRoomRepository
   인터페이스:
   - Create(room)
   - GetByID(id)
   - GetByCode(code)
   - Update(room)
   - Delete(id)
   - AddPlayer(player)
   - RemovePlayer(roomID, userID)
   - GetPlayers(roomID)
   - UpdatePlayerReady(roomID, userID, isReady)
   - GetActiveRooms()

5. LottoFriendRepository
   인터페이스:
   - Create(friendship)
   - GetFriends(userID)
   - GetPendingRequests(userID)
   - AcceptRequest(friendshipID)
   - RejectRequest(friendshipID)
   - BlockUser(userID, friendID)

요구사항:
- 기존 Repository 패턴 따르기
- 인터페이스 + 구현체 패턴
- GORM 사용
- 에러 핸들링 일관성
- Preload 사용 (관계 조회 시)

파일 위치: 기존 repository/ 디렉토리
파일명:
- repository/lotto_user_repository.go
- repository/lotto_game_repository.go
- repository/lotto_quest_repository.go
- repository/lotto_room_repository.go
- repository/lotto_friend_repository.go
```

---

### Step 5: Service 레이어 (Cursor AI)

**Cursor AI에게 보낼 프롬프트:**

```
Lotto Defense용 Service를 추가해줘.

기존 Service 스타일 참조:
@service/existing_service.go

추가할 Service (총 5개):

1. LottoAuthService
   메서드:
   - Register(username, email, password) (*LottoUser, string, error)
     → 회원가입, JWT 토큰 반환
   - Login(email, password) (*LottoUser, string, error)
     → 로그인, JWT 토큰 반환
   - ValidateToken(token) (int64, error)
     → 토큰 검증, UserID 반환
   - HashPassword(password) (string, error)
   - ComparePassword(hash, password) bool

2. LottoGameService
   메서드:
   - SaveSingleResult(userID, result) error
     → 게임 결과 저장 + UserStats 업데이트
   - GetGameHistory(userID, gameMode, limit, offset)
   - GetUserStats(userID) (*LottoUserStats, error)

3. LottoQuestService
   메서드:
   - GetActiveQuests(userID) ([]LottoQuest, error)
   - UpdateQuestProgress(questID, increment) error
   - CompleteQuest(questID) error
   - ClaimReward(questID) (*LottoReward, error)
     → 보상 생성 + CurrentGold 업데이트

4. LottoRoomService
   메서드:
   - CreateRoom(hostUserID, roomType) (*LottoRoom, error)
     → 4자리 코드 생성
   - JoinRoom(userID, roomCode) (*LottoRoom, error)
   - LeaveRoom(userID, roomID) error
   - SetReady(userID, roomID, isReady) error
   - GetRoomInfo(roomID) (*LottoRoom, error)
   - DeleteExpiredRooms() error

5. LottoFriendService
   메서드:
   - GetFriends(userID) ([]LottoFriendship, error)
   - SendFriendRequest(userID, friendUsername) error
   - AcceptFriendRequest(userID, friendshipID) error
   - RejectFriendRequest(userID, friendshipID) error

요구사항:
- 기존 Service 패턴 따르기
- Repository 의존성 주입
- 트랜잭션 처리 (필요 시)
- 비즈니스 로직 검증
- 에러 반환 명확히

파일 위치: 기존 service/ 디렉토리
파일명:
- service/lotto_auth_service.go
- service/lotto_game_service.go
- service/lotto_quest_service.go
- service/lotto_room_service.go
- service/lotto_friend_service.go

JWT 설정:
- Secret: 기존 프로젝트 설정 사용
- Expire: 24시간
```

---

### Step 6: HTTP 핸들러 (Cursor AI)

**Cursor AI에게 보낼 프롬프트:**

```
Lotto Defense용 HTTP 핸들러를 추가해줘.

기존 Handler 스타일 참조:
@handlers/existing_handler.go

추가할 Handler (총 5개):

1. lotto_auth_handler.go
   - POST /api/v1/lotto/auth/register
     Request: {username, email, password}
     Response: {success, data: {user, token}}
   
   - POST /api/v1/lotto/auth/login
     Request: {email, password}
     Response: {success, data: {user, token}}
   
   - POST /api/v1/lotto/auth/logout
     Response: {success, message}

2. lotto_user_handler.go
   - GET /api/v1/lotto/users/me
     Response: {success, data: {user, stats}}
   
   - GET /api/v1/lotto/users/me/stats
     Response: {success, data: {stats}}

3. lotto_game_handler.go
   - POST /api/v1/lotto/game/single/result
     Request: {rounds_reached, monsters_killed, gold_earned, ...}
     Response: {success, data: {game_id, new_highest_round, rewards}}
   
   - GET /api/v1/lotto/game/history?mode=single&limit=10&offset=0
     Response: {success, data: {total, games}}

4. lotto_quest_handler.go
   - GET /api/v1/lotto/quests?status=active
     Response: {success, data: {quests}}
   
   - POST /api/v1/lotto/quests/:id/progress
     Request: {increment: 1}
     Response: {success, data: {quest}}
   
   - POST /api/v1/lotto/quests/:id/claim
     Response: {success, data: {quest, rewards, new_gold}}

5. lotto_coop_handler.go
   - POST /api/v1/lotto/coop/rooms
     Request: {room_type: "private"}
     Response: {success, data: {room_id, room_code}}
   
   - POST /api/v1/lotto/coop/rooms/join
     Request: {room_code}
     Response: {success, data: {room, player_slot, ws_url}}
   
   - POST /api/v1/lotto/coop/matchmaking/random
     Response: {success, data: {room, matched}}
   
   - GET /api/v1/lotto/coop/rooms/:id
     Response: {success, data: {room, players}}
   
   - POST /api/v1/lotto/coop/rooms/:id/leave
     Response: {success, message}
   
   - POST /api/v1/lotto/coop/rooms/:id/ready
     Request: {is_ready}
     Response: {success, data: {is_ready, all_ready}}

요구사항:
- Echo Context 사용 (c echo.Context)
- c.Bind() 요청 바인딩
- c.JSON() 응답
- echo.NewHTTPError() 에러 처리
- Service 레이어 호출
- JWT 미들웨어 적용 (필요 시)
- 기존 핸들러 응답 포맷 따르기

파일 위치: 기존 handlers/ 디렉토리
파일명:
- handlers/lotto_auth_handler.go
- handlers/lotto_user_handler.go
- handlers/lotto_game_handler.go
- handlers/lotto_quest_handler.go
- handlers/lotto_coop_handler.go
```

---

### Step 7: 라우터 통합 (Cursor AI)

**Cursor AI에게 보낼 프롬프트:**

```
기존 라우터에 Lotto Defense 라우트를 추가해줘.

기존 라우터 파일:
@router/router.go (또는 기존 라우터 파일)

추가할 라우트:

// Lotto Defense API 그룹
lotto := api.Group("/lotto")

// 인증 (공개)
lottoAuth := lotto.Group("/auth")
lottoAuth.POST("/register", lottoAuthHandler.Register)
lottoAuth.POST("/login", lottoAuthHandler.Login)
lottoAuth.POST("/logout", lottoAuthHandler.Logout) // JWT 필요

// 유저 (JWT 필수)
lottoUsers := lotto.Group("/users")
lottoUsers.Use(jwtMiddleware) // 기존 JWT 미들웨어 사용
lottoUsers.GET("/me", lottoUserHandler.GetMe)
lottoUsers.GET("/me/stats", lottoUserHandler.GetStats)

// 게임 (JWT 필수)
lottoGame := lotto.Group("/game")
lottoGame.Use(jwtMiddleware)
lottoGame.POST("/single/result", lottoGameHandler.SaveSingleResult)
lottoGame.GET("/history", lottoGameHandler.GetHistory)

// 퀘스트 (JWT 필수)
lottoQuests := lotto.Group("/quests")
lottoQuests.Use(jwtMiddleware)
lottoQuests.GET("", lottoQuestHandler.GetQuests)
lottoQuests.POST("/:id/progress", lottoQuestHandler.UpdateProgress)
lottoQuests.POST("/:id/claim", lottoQuestHandler.ClaimReward)

// 협동 플레이 (JWT 필수)
lottoCoop := lotto.Group("/coop")
lottoCoop.Use(jwtMiddleware)
lottoCoop.POST("/rooms", lottoCoopHandler.CreateRoom)
lottoCoop.POST("/rooms/join", lottoCoopHandler.JoinRoom)
lottoCoop.POST("/matchmaking/random", lottoCoopHandler.RandomMatchmaking)
lottoCoop.GET("/rooms/:id", lottoCoopHandler.GetRoom)
lottoCoop.POST("/rooms/:id/leave", lottoCoopHandler.LeaveRoom)
lottoCoop.POST("/rooms/:id/ready", lottoCoopHandler.SetReady)

요구사항:
- 기존 라우터 구조 유지
- 기존 JWT 미들웨어 재사용
- /api/v1/lotto 경로 사용
- Handler 인스턴스 생성 및 주입
```

---

### Step 8: Auto Migration 추가 (Cursor AI)

**Cursor AI에게 보낼 프롬프트:**

```
main.go (또는 migration 파일)에 Lotto Defense 모델 추가해줘.

기존 Auto Migration 코드 찾기:
@main.go (또는 @migrations/)

추가할 모델:
db.AutoMigrate(
    &models.LottoUser{},
    &models.LottoUserStats{},
    &models.LottoGameResult{},
    &models.LottoQuest{},
    &models.LottoReward{},
    &models.LottoRoom{},
    &models.LottoRoomPlayer{},
    &models.LottoFriendship{},
)

기존 패턴 그대로 추가.
```

---

### Step 9: 테스트 (수동)

**컴파일 확인:**
```bash
cd joker_backend
go build ./...
```

**서버 실행:**
```bash
go run main.go
```

**API 테스트:**
```bash
# 회원가입
curl -X POST http://localhost:8080/api/v1/lotto/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"player1","email":"player1@test.com","password":"test123"}'

# 로그인
curl -X POST http://localhost:8080/api/v1/lotto/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"player1@test.com","password":"test123"}'

# 내 정보 (토큰 필요)
curl http://localhost:8080/api/v1/lotto/users/me \
  -H "Authorization: Bearer <token>"
```

---

## ✅ 체크리스트

- [ ] Step 1: 프로젝트 준비 (문서 추가)
- [ ] Step 2: 아키텍처 분석 (Cursor AI)
- [ ] Step 3: 모델 통합 (5개 파일)
- [ ] Step 4: Repository 통합 (5개 파일)
- [ ] Step 5: Service 레이어 (5개 파일)
- [ ] Step 6: HTTP 핸들러 (5개 파일)
- [ ] Step 7: 라우터 통합
- [ ] Step 8: Auto Migration 추가
- [ ] Step 9: 컴파일 성공
- [ ] Step 10: 서버 실행
- [ ] Step 11: API 테스트

---

## 🎯 완료 후

**joker_backend에 추가된 것:**
- ✅ 모델 5개 파일 (8개 테이블)
- ✅ Repository 5개 파일
- ✅ Service 5개 파일
- ✅ Handler 5개 파일
- ✅ 라우터 업데이트
- ✅ Auto Migration 업데이트

**총:** 25개 파일 추가

---

## 💡 팁

### Cursor AI 활용
- @파일명으로 기존 파일 참조
- "이 스타일로..." 패턴 복사 요청
- 단계별로 진행 (한 번에 하나씩)

### 에러 발생 시
```
Cursor AI에게:

컴파일 에러:
[에러 메시지]

기존 코드 스타일 따라서 수정해줘.
```

### 검증
각 단계마다:
```bash
go build ./...
```

---

## 📞 문제 발생 시

**Cursor AI 프롬프트:**
```
에러 발생:
[에러 메시지]

원인 분석하고 기존 코드 패턴에 맞게 수정해줘.
```

또는 저에게 물어보세요!

---

**이제 Cursor IDE에서 Step 1부터 시작하세요!** 🚀

**joker_backend 프로젝트 열고 → Step 2 프롬프트 복사 → Cursor AI에게 전달!** 💪
