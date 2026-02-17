# Cursor AI 개발 가이드 - Lotto Defense 백엔드

**프로젝트:** joker_backend (기존 프로젝트에 추가)  
**작업:** Lotto Defense 게임 백엔드 개발  
**기반:** BACKEND_SPEC.md  
**방식:** 기존 아키텍처 스타일 유지

---

## 🎯 Cursor AI 사용 전략

### 1단계: 프로젝트 준비

**Cursor IDE에서:**
1. `joker_backend` 프로젝트 열기 (이미 열려 있음 ✅)
2. BACKEND_SPEC.md 파일 추가하기
3. 기존 코드 아키텍처 파악되어 있음 ✅

---

## 📝 Cursor AI 프롬프트 템플릿

### Step 1: 아키텍처 분석 요청

```
Cursor AI에게 보낼 프롬프트:

---
이 프로젝트의 기존 백엔드 아키텍처를 분석해줘.

확인할 것:
1. 디렉토리 구조 (internal/, cmd/, models/ 등)
2. Echo/Gin 프레임워크 사용 여부
3. GORM 모델 정의 방식
4. 라우터 설정 방식 (router.go 패턴)
5. 핸들러 작성 스타일 (handlers/)
6. 미들웨어 패턴 (middleware/)
7. 에러 핸들링 방식
8. 응답 포맷 (JSON 구조)
9. 데이터베이스 연결 방식
10. 환경 변수 관리 (.env, config/)

기존 프로젝트 스타일을 그대로 따라서 새로운 Lotto Defense 백엔드를 개발할 거야.
---
```

**Cursor AI 응답 예상:**
- 기존 프로젝트 구조 요약
- 사용 중인 패턴 정리
- 코딩 컨벤션 파악

---

### Step 2: BACKEND_SPEC.md 전달

```
Cursor AI에게 보낼 프롬프트:

---
@BACKEND_SPEC.md 

이 명세서를 읽고 Lotto Defense 백엔드를 개발해야 해.

요구사항:
1. 기존 joker_backend 프로젝트 아키텍처 스타일 유지
2. Echo 프레임워크 + GORM 사용
3. 기존 디렉토리 구조 따르기
4. 기존 에러 핸들링 패턴 사용
5. 기존 응답 포맷 사용

명세서 주요 내용:
- 싱글 플레이 (5줄 4열)
- 협동 플레이 (각 5줄 4열, 2인)
- REST API + WebSocket
- 8개 데이터베이스 테이블
- 방 관리 시스템 (고루틴 기반)

먼저 전체 구조를 제안해줘. 어떤 파일들을 만들어야 하는지 리스트업 해줘.
---
```

---

### Step 3: 모델(Model) 생성

```
Cursor AI에게 보낼 프롬프트:

---
BACKEND_SPEC.md의 데이터베이스 스키마를 기반으로 GORM 모델을 만들어줘.

생성할 모델:
1. User (users 테이블)
2. UserStats (user_stats 테이블)
3. GameResult (game_results 테이블)
4. Quest (quests 테이블)
5. Reward (rewards 테이블)
6. Room (rooms 테이블)
7. RoomPlayer (room_players 테이블)
8. Friendship (friendships 테이블)

요구사항:
- 기존 프로젝트의 모델 정의 스타일 따르기
- GORM 태그 정확하게 (gorm:"column:xxx;type:xxx")
- JSON 태그 추가 (json:"xxx")
- Association 관계 정의 (hasMany, belongsTo 등)
- TableName() 메서드 정의

파일 위치: internal/models/lottodefense/ 에 생성
(또는 기존 프로젝트 모델 디렉토리 구조 따르기)
---
```

---

### Step 4: 마이그레이션 파일 생성

```
Cursor AI에게 보낼 프롬프트:

---
BACKEND_SPEC.md의 SQL 스키마를 PostgreSQL 마이그레이션 파일로 만들어줘.

파일 생성:
- migrations/lottodefense/001_create_users.sql
- migrations/lottodefense/002_create_user_stats.sql
- migrations/lottodefense/003_create_game_results.sql
- migrations/lottodefense/004_create_quests.sql
- migrations/lottodefense/005_create_rewards.sql
- migrations/lottodefense/006_create_rooms.sql
- migrations/lottodefense/007_create_room_players.sql
- migrations/lottodefense/008_create_friendships.sql

각 파일에 UP/DOWN 마이그레이션 포함.
기존 프로젝트 마이그레이션 스타일 따르기.
---
```

---

### Step 5: Repository 레이어 생성

```
Cursor AI에게 보낼 프롬프트:

---
Repository 패턴으로 데이터베이스 접근 레이어를 만들어줘.

생성할 Repository:
1. UserRepository (user_repo.go)
   - CreateUser
   - GetUserByID
   - GetUserByEmail
   - UpdateUser
   - GetUserStats

2. GameRepository (game_repo.go)
   - SaveGameResult
   - GetGameHistory
   - GetHighestRound

3. QuestRepository (quest_repo.go)
   - GetActiveQuests
   - UpdateQuestProgress
   - CompleteQuest
   - ClaimReward

4. RoomRepository (room_repo.go)
   - CreateRoom
   - GetRoomByID
   - GetRoomByCode
   - UpdateRoom
   - DeleteRoom
   - AddPlayerToRoom
   - RemovePlayerFromRoom

5. FriendRepository (friend_repo.go)
   - GetFriends
   - SendFriendRequest
   - AcceptFriendRequest

요구사항:
- 기존 프로젝트 Repository 패턴 따르기
- 인터페이스 정의 (interface + struct)
- GORM 사용
- 에러 핸들링 일관성

파일 위치: internal/repository/lottodefense/
---
```

---

### Step 6: Service 레이어 생성

```
Cursor AI에게 보낼 프롬프트:

---
비즈니스 로직을 담당하는 Service 레이어를 만들어줘.

생성할 Service:
1. AuthService (auth_service.go)
   - Register(username, email, password)
   - Login(email, password)
   - GenerateJWT(userID)
   - ValidateToken(token)

2. GameService (game_service.go)
   - SaveSinglePlayResult
   - GetGameHistory
   - UpdateStatistics

3. QuestService (quest_service.go)
   - GetQuests(userID, status)
   - UpdateProgress(questID, increment)
   - ClaimReward(questID)

4. RoomService (room_service.go)
   - CreateRoom(hostUserID, roomType)
   - JoinRoom(userID, roomCode)
   - LeaveRoom(userID, roomID)
   - SetReady(userID, roomID)

요구사항:
- Repository 사용
- 트랜잭션 처리 (필요 시)
- 비즈니스 검증 로직
- 에러 반환 명확히

파일 위치: internal/service/lottodefense/
---
```

---

### Step 7: HTTP 핸들러 생성

```
Cursor AI에게 보낼 프롬프트:

---
Echo 프레임워크로 HTTP 핸들러를 만들어줘.

BACKEND_SPEC.md의 REST API 명세 기반:

1. auth_handler.go
   - POST /auth/register
   - POST /auth/login
   - POST /auth/logout

2. user_handler.go
   - GET /users/me
   - GET /users/me/stats

3. game_handler.go
   - POST /game/single/result
   - GET /game/history

4. quest_handler.go
   - GET /quests
   - POST /quests/:id/progress
   - POST /quests/:id/claim

5. coop_handler.go
   - POST /coop/rooms
   - POST /coop/rooms/join
   - POST /coop/matchmaking/random
   - GET /coop/rooms/:id
   - POST /coop/rooms/:id/leave
   - POST /coop/rooms/:id/ready

요구사항:
- Echo Context 사용 (c echo.Context)
- c.Bind() 로 요청 바인딩
- c.JSON() 로 응답
- echo.NewHTTPError() 로 에러 처리
- Service 레이어 호출
- 기존 프로젝트 핸들러 스타일 따르기

파일 위치: internal/api/handlers/lottodefense/
---
```

---

### Step 8: 라우터 설정

```
Cursor AI에게 보낼 프롬프트:

---
Echo 라우터를 설정해줘.

파일: internal/api/router/lottodefense_router.go

요구사항:
1. 모든 핸들러 연결
2. 미들웨어 적용:
   - JWT 인증 (필요한 엔드포인트에만)
   - CORS
   - Logger
   - Rate Limiting
3. API 그룹핑 (/v1/auth, /v1/users 등)
4. 기존 프로젝트 라우터 패턴 따르기

함수 시그니처:
func SetupLottoDefenseRoutes(e *echo.Echo, db *gorm.DB)
---
```

---

### Step 9: WebSocket 핸들러

```
Cursor AI에게 보낼 프롬프트:

---
협동 플레이용 WebSocket 핸들러를 만들어줘.

BACKEND_SPEC.md의 WebSocket 프로토콜 기반:

파일 구조:
1. websocket/client.go
   - Client 구조체
   - Read/Write 고루틴
   - 메시지 송수신

2. websocket/hub.go
   - Hub 구조체 (방 관리)
   - 브로드캐스트
   - 클라이언트 등록/해제

3. websocket/message.go
   - 메시지 타입 정의
   - 메시지 파싱/직렬화

4. websocket/handler.go
   - Echo WebSocket 핸들러
   - JWT 검증
   - Client 생성 및 Hub 연결

요구사항:
- gorilla/websocket 사용
- JSON 메시지 포맷
- 에러 핸들링
- 연결 종료 처리

파일 위치: internal/websocket/lottodefense/
---
```

---

### Step 10: 방 관리 시스템

```
Cursor AI에게 보낼 프롬프트:

---
협동 플레이 방 관리 시스템을 만들어줘.

BACKEND_SPEC.md의 방 관리 시스템 기반:

파일 구조:
1. room/room.go
   - Room 구조체
   - Run() 고루틴 (게임 루프)
   - handleAction()
   - updateGameState()
   - broadcastGameState()

2. room/manager.go
   - RoomManager 구조체
   - CreateRoom()
   - GetRoom()
   - DeleteRoom()
   - rooms map 관리

3. room/matchmaking.go
   - MatchmakingQueue
   - StartMatchmaking()
   - RunMatchmaker() 고루틴

4. room/game_logic.go
   - findTarget() (몬스터 타게팅)
   - handleMonsterKilled()
   - completeRound()
   - SpendGold() / EarnGold()

요구사항:
- 각 방은 독립 고루틴
- sync.RWMutex 동기화
- Channel 기반 통신 (actionChan, doneChan)
- 4자리 방 코드 생성 (A3F7 형식)

파일 위치: internal/room/lottodefense/
---
```

---

### Step 11: 미들웨어

```
Cursor AI에게 보낼 프롬프트:

---
필요한 미들웨어를 만들어줘.

1. JWT 인증 미들웨어 (auth.go)
   - 토큰 검증
   - UserID 추출 → Context에 저장
   - echo.MiddlewareFunc 반환

2. Rate Limiting 미들웨어 (ratelimit.go)
   - Redis 기반 (선택)
   - 60 req/min per IP
   - 초과 시 429 에러

3. Error Handler (error.go)
   - Echo 글로벌 에러 핸들러
   - 일관된 에러 응답 포맷

요구사항:
- 기존 프로젝트 미들웨어 스타일 따르기
- Echo 미들웨어 패턴

파일 위치: internal/api/middleware/lottodefense/
---
```

---

### Step 12: 설정 파일

```
Cursor AI에게 보낼 프롬프트:

---
환경 변수 및 설정 파일을 만들어줘.

1. .env.example
   - DATABASE_URL
   - REDIS_URL
   - JWT_SECRET
   - SERVER_PORT
   - CORS_ORIGINS

2. config/config.go
   - Config 구조체
   - LoadConfig() 함수
   - godotenv 사용

요구사항:
- 기존 프로젝트 설정 방식 따르기
- 환경별 설정 지원 (dev/prod)

파일 위치: 
- .env.example (프로젝트 루트)
- internal/config/lottodefense/
---
```

---

### Step 13: main.go 통합

```
Cursor AI에게 보낼 프롬프트:

---
기존 main.go에 Lotto Defense 라우터를 추가해줘.

또는 새로운 서버 파일 생성:
- cmd/lottodefense/main.go

요구사항:
1. Echo 인스턴스 생성
2. GORM DB 연결
3. 미들웨어 등록
4. Lotto Defense 라우터 등록
5. WebSocket 핸들러 연결
6. RoomManager 시작
7. 서버 시작 (:8080)

기존 프로젝트와 통합하는 방식으로.
---
```

---

## 🔥 단계별 실행 순서

Cursor AI에게 **한 번에 하나씩** 요청하세요:

```
1. 아키텍처 분석
   ↓
2. BACKEND_SPEC.md 읽기 + 구조 제안
   ↓
3. 모델 생성 (8개)
   ↓
4. 마이그레이션 파일 (8개)
   ↓
5. Repository (5개)
   ↓
6. Service (4개)
   ↓
7. HTTP 핸들러 (5개)
   ↓
8. 라우터 설정
   ↓
9. WebSocket (4개 파일)
   ↓
10. 방 관리 시스템 (4개 파일)
   ↓
11. 미들웨어 (3개)
   ↓
12. 설정 파일
   ↓
13. main.go 통합
```

---

## ✅ 각 단계 완료 후 확인사항

Cursor AI가 코드 생성 후:

1. **컴파일 확인**
   ```bash
   go build ./...
   ```

2. **포맷 확인**
   ```bash
   go fmt ./...
   ```

3. **Lint 확인**
   ```bash
   golangci-lint run
   ```

4. **테스트 (옵션)**
   ```bash
   go test ./...
   ```

---

## 💡 Cursor AI 활용 팁

### 1. @파일명 참조 사용

```
Cursor AI 프롬프트에서:

@BACKEND_SPEC.md 이 명세서를 보고...
@internal/models/existing_model.go 이 스타일로...
```

### 2. 기존 코드 스타일 참조

```
@internal/api/handlers/기존핸들러.go 

이 핸들러 스타일 그대로 따라서 Lotto Defense 핸들러 만들어줘.
```

### 3. 여러 파일 동시 참조

```
@internal/models/user.go
@internal/repository/user_repo.go
@internal/service/user_service.go

이 3개 파일 패턴 그대로 Quest 기능도 만들어줘.
```

### 4. 단계별 검증

```
방금 만든 코드 검증해줘:
1. 컴파일 에러 없는지
2. import 누락 없는지
3. 타입 매칭 맞는지
4. 에러 핸들링 빠진 곳 없는지
```

### 5. 리팩토링 요청

```
방금 만든 코드에서:
1. 중복 코드 제거해줘
2. 에러 메시지 일관성 맞춰줘
3. 주석 추가해줘
4. 변수명 더 명확하게 해줘
```

---

## 🚨 주의사항

### 1. 한 번에 너무 많이 요청하지 말기

❌ 나쁜 예:
```
모델, 레포지토리, 서비스, 핸들러 다 한번에 만들어줘.
```

✅ 좋은 예:
```
먼저 User 모델만 만들어줘.
(완료 후)
User 레포지토리 만들어줘.
(완료 후)
User 서비스 만들어줘.
```

### 2. 기존 코드 참조 필수

항상 기존 프로젝트 파일 참조:
```
@기존파일.go 이 스타일로...
```

### 3. 컴파일 에러 즉시 수정

코드 생성 후 바로:
```bash
go build ./...
```

에러 나면 Cursor AI에게:
```
컴파일 에러 발생:
[에러 메시지 붙여넣기]

수정해줘.
```

---

## 📋 체크리스트

각 단계 완료 후 체크:

- [ ] Step 1: 아키텍처 분석 완료
- [ ] Step 2: BACKEND_SPEC.md 전달 완료
- [ ] Step 3: 모델 8개 생성 완료
- [ ] Step 4: 마이그레이션 8개 생성 완료
- [ ] Step 5: Repository 5개 생성 완료
- [ ] Step 6: Service 4개 생성 완료
- [ ] Step 7: HTTP 핸들러 5개 생성 완료
- [ ] Step 8: 라우터 설정 완료
- [ ] Step 9: WebSocket 4개 파일 생성 완료
- [ ] Step 10: 방 관리 시스템 4개 파일 생성 완료
- [ ] Step 11: 미들웨어 3개 생성 완료
- [ ] Step 12: 설정 파일 생성 완료
- [ ] Step 13: main.go 통합 완료
- [ ] 전체 컴파일 성공
- [ ] 서버 실행 테스트

---

## 🎯 최종 목표

**완성되면:**

```bash
# 서버 실행
go run cmd/lottodefense/main.go

# 또는 기존 main.go에 통합된 경우
go run cmd/server/main.go
```

**API 테스트:**
```bash
# 회원가입
curl -X POST http://localhost:8080/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"player1","email":"player1@test.com","password":"test123"}'

# 로그인
curl -X POST http://localhost:8080/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"player1@test.com","password":"test123"}'

# 방 생성
curl -X POST http://localhost:8080/v1/coop/rooms \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"room_type":"private"}'
```

---

## 📞 문제 발생 시

Cursor AI에게:
```
에러 발생:
[에러 메시지]

원인 분석하고 수정해줘.
```

또는 저에게 물어보세요!

---

**이제 Cursor IDE에서 위 프롬프트들을 하나씩 실행하세요!** 🚀

**Step 1부터 시작하세요!** 💪
