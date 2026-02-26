# Lotto Defense - Backend Integration Documentation

## 프로젝트 개요
Unity 기반 모바일 타워 디펜스 게임으로, 실시간 멀티플레이어 협동 모드를 지원합니다.

## 현재 백엔드 연동 상태 분석

### ⚠️ 긴급 수정 필요 사항

1. **하드코딩된 localhost URL**
   - 위치: `Assets/Scripts/Networking/APIClient.cs:15`
   - 현재: `BASE_URL = "http://localhost:18082/api/v1/td"`
   - 필요 조치: 환경 설정 기반 URL 관리 시스템 구현

2. **재시도 로직 부재**
   - 위치: `APIClient.cs`
   - 문제: HTTP 요청 실패 시 재시도 메커니즘 없음
   - 필요 조치: Exponential backoff 재시도 로직 구현

3. **Null 체크 누락**
   - 위치: `AuthenticationManager.cs:70`
   - 문제: 이벤트 호출 시 null 체크 없음
   - 필요 조치: Null-conditional 연산자 사용

### 🔒 보안 개선 필요

1. **토큰 평문 저장**
   - 현재: JWT 토큰이 PlayerPrefs에 암호화 없이 저장
   - 위험: 루팅/탈옥 기기에서 토큰 탈취 가능
   - 권장: Unity Keystore 또는 토큰 암호화

2. **WebSocket 재연결 시 인증 검증 부재**
   - 문제: 토큰 만료 시 재연결 실패 가능
   - 권장: 재연결 flow에 토큰 갱신 로직 추가

### 🧪 테스트 코드 현황

**현재 상태: 네트워크 레이어 테스트 코드 전무**

필요한 테스트:
- APIClient 단위 테스트
- WebSocket 연결 테스트
- 인증 플로우 통합 테스트
- 네트워크 실패 시뮬레이션 테스트

## 네트워크 아키텍처

### HTTP API 통신
```csharp
// 현재 구조
APIManager (Singleton)
    ├── APIClient (UnityWebRequest wrapper)
    ├── Authentication endpoints
    └── Game data endpoints
```

### WebSocket 실시간 통신
```csharp
// 현재 구조
WebSocketClient (NativeWebSocket)
    ├── Connection management
    ├── Heartbeat (15초 간격)
    ├── Message handling
    └── Auto-reconnect (최대 5회)
```

### 메시지 구조
```csharp
public class NetworkMessage
{
    public string type;      // 메시지 타입
    public string payload;    // JSON 페이로드
    public long timestamp;    // 타임스탬프
}
```

## API 엔드포인트 명세

### 인증 API

#### 1. 로그인
- **Endpoint**: `POST /api/v1/td/auth/login`
- **Request**:
```json
{
    "username": "string",
    "password": "string"
}
```
- **Response**:
```json
{
    "token": "jwt_token_string",
    "userId": "string",
    "profile": {
        "nickname": "string",
        "avatarId": "string",
        "level": 1,
        "experience": 0
    }
}
```

#### 2. 토큰 갱신
- **Endpoint**: `POST /api/v1/td/auth/refresh`
- **Headers**: `Authorization: Bearer {token}`
- **Response**:
```json
{
    "token": "new_jwt_token_string"
}
```

### 게임 데이터 API

#### 1. 게임 세션 생성
- **Endpoint**: `POST /api/v1/td/game/session`
- **Headers**: `Authorization: Bearer {token}`
- **Request**:
```json
{
    "mode": "coop" | "single",
    "playerCount": 1 | 2
}
```
- **Response**:
```json
{
    "sessionId": "string",
    "roomCode": "string",
    "webSocketUrl": "ws://server/game/{sessionId}"
}
```

#### 2. 게임 결과 저장
- **Endpoint**: `POST /api/v1/td/game/result`
- **Headers**: `Authorization: Bearer {token}`
- **Request**:
```json
{
    "sessionId": "string",
    "score": 0,
    "wavesCompleted": 0,
    "unitsPlaced": 0,
    "enemiesDefeated": 0,
    "victory": true | false
}
```

### 프로필 API

#### 1. 프로필 조회
- **Endpoint**: `GET /api/v1/td/profile/{userId}`
- **Headers**: `Authorization: Bearer {token}`
- **Response**:
```json
{
    "userId": "string",
    "nickname": "string",
    "avatarId": "string",
    "level": 1,
    "experience": 0,
    "stats": {
        "gamesPlayed": 0,
        "victories": 0,
        "totalScore": 0
    }
}
```

## WebSocket 실시간 통신 프로토콜

### 연결 플로우
1. HTTP API로 게임 세션 생성
2. 반환된 WebSocket URL로 연결
3. 연결 후 인증 메시지 전송
4. 서버 응답 확인 후 게임 시작

### 메시지 타입

#### 클라이언트 → 서버

```javascript
// 인증
{
    "type": "auth",
    "payload": {
        "token": "jwt_token",
        "sessionId": "session_id"
    }
}

// 유닛 배치
{
    "type": "unit_placed",
    "payload": {
        "playerNumber": 1,
        "position": { "x": 0, "y": 0 },
        "unitType": "tower_basic"
    }
}

// 유닛 제거
{
    "type": "unit_removed",
    "payload": {
        "playerNumber": 1,
        "position": { "x": 0, "y": 0 }
    }
}

// 하트비트
{
    "type": "heartbeat",
    "payload": {}
}
```

#### 서버 → 클라이언트

```javascript
// 게임 상태 동기화
{
    "type": "game_state",
    "payload": {
        "wave": 1,
        "enemies": [...],
        "players": [...]
    }
}

// 웨이브 시작
{
    "type": "wave_start",
    "payload": {
        "waveNumber": 1,
        "enemyCount": 10
    }
}

// 게임 종료
{
    "type": "game_over",
    "payload": {
        "victory": true,
        "scores": {...}
    }
}
```

## 협동 플레이 동기화

### 플레이어 영역 할당
- Player 1: 그리드 왼쪽 (columns 0-1)
- Player 2: 그리드 오른쪽 (columns 2-3)
- 공유: 모든 행 (rows 0-4)

### 동기화 필요 데이터
1. 유닛 배치/제거
2. 플레이어 자원 (골드, 점수)
3. 웨이브 진행 상황
4. 몬스터 위치 및 상태
5. 게임 일시정지/재개

## 개발 우선순위

### Phase 1: 기본 연동 (필수)
- [ ] 환경별 API URL 설정
- [ ] HTTP 재시도 로직 구현
- [ ] 에러 핸들링 개선
- [ ] 토큰 관리 시스템

### Phase 2: 실시간 통신 (협동 플레이)
- [ ] WebSocket 연결 안정화
- [ ] 상태 동기화 프로토콜
- [ ] 지연 보상 메커니즘
- [ ] 연결 복구 로직

### Phase 3: 보안 강화
- [ ] 토큰 암호화 저장
- [ ] Certificate pinning
- [ ] 입력 검증 강화
- [ ] Rate limiting 대응

### Phase 4: 성능 최적화
- [ ] 요청 큐잉 시스템
- [ ] 오프라인 모드 지원
- [ ] 데이터 압축
- [ ] 캐싱 전략

## 테스트 체크리스트

### 단위 테스트
- [ ] API 클라이언트 모든 메소드
- [ ] 메시지 직렬화/역직렬화
- [ ] 토큰 관리 로직
- [ ] 에러 처리 경로

### 통합 테스트
- [ ] 전체 인증 플로우
- [ ] 게임 세션 생애주기
- [ ] 협동 플레이 동기화
- [ ] 네트워크 장애 복구

### 부하 테스트
- [ ] 동시 접속 처리
- [ ] 대용량 메시지 처리
- [ ] 장시간 연결 유지
- [ ] 메모리 누수 체크

## 환경 설정

### 개발 환경
```json
{
    "apiUrl": "http://localhost:18082/api/v1/td",
    "wsUrl": "ws://localhost:18082/ws",
    "timeout": 10000,
    "retryCount": 3
}
```

### 스테이징 환경
```json
{
    "apiUrl": "https://staging.lotto-defense.com/api/v1/td",
    "wsUrl": "wss://staging.lotto-defense.com/ws",
    "timeout": 15000,
    "retryCount": 5
}
```

### 프로덕션 환경
```json
{
    "apiUrl": "https://api.lotto-defense.com/api/v1/td",
    "wsUrl": "wss://api.lotto-defense.com/ws",
    "timeout": 20000,
    "retryCount": 5
}
```

## 모니터링 및 로깅

### 추적 필요 메트릭
- API 응답 시간
- WebSocket 연결 상태
- 에러 발생률
- 재시도 횟수
- 동기화 지연 시간

### 로그 레벨
- ERROR: 시스템 에러, 연결 실패
- WARNING: 재시도, 타임아웃
- INFO: 연결 상태 변경, 주요 이벤트
- DEBUG: 모든 메시지, 상세 상태

## 문의사항

백엔드 팀에서 추가로 필요한 정보나 수정사항이 있으면 연락 바랍니다.

- Unity 버전: 2022.3 LTS
- Target Platform: iOS, Android
- Min API Level: Android 21, iOS 12.0
- Network Library: UnityWebRequest, NativeWebSocket