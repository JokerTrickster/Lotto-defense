# Unity-Backend 연동 가이드

**Tower Defense 싱글 플레이 백엔드 연동 완료!** ✅

---

## ✅ 완성된 기능

### 1. **인증 시스템**
- 회원가입 (이메일, 닉네임, 비밀번호)
- 로그인 (이메일, 비밀번호)
- JWT 토큰 자동 저장 (PlayerPrefs)
- 자동 로그인 (앱 재실행 시)
- 게스트 모드 (로그인 건너뛰기)

### 2. **게임 결과 자동 전송**
- 게임 종료 시 (승리/패배) 자동으로 결과 전송
- 전송 데이터:
  - 도달한 라운드
  - 처치한 몬스터 수
  - 획득한 골드
  - 결과 (victory/defeat)

### 3. **통계 조회**
- 최고 라운드
- 총 게임 수
- 총 처치 수
- 총 획득 골드

---

## 📁 추가된 파일 (7개)

```
Assets/Scripts/Backend/
├── APIClient.cs           - HTTP 통신 (UnityWebRequest)
├── APIManager.cs          - 싱글톤, 토큰 관리
├── Models/
│   ├── AuthModels.cs     - 인증 요청/응답 모델
│   └── GameModels.cs     - 게임 결과 모델
└── UI/
    ├── LoginUI.cs        - 로그인/회원가입 UI
    └── StatsUI.cs        - 통계 조회 UI
```

**수정된 파일:**
- `GameplayManager.cs` - 게임 종료 시 결과 전송
- `MonsterManager.cs` - 킬 카운트 추적

---

## 🚀 사용 방법

### 1. 백엔드 서버 실행

```bash
cd ~/project/joker_backend/services/lottoDefenseService

# 환경 변수 설정
export JWT_SECRET=your-secret-key
export IS_LOCAL=true
export PORT=18082

# 서버 실행
go run cmd/main.go
```

**서버 주소:** `http://localhost:18082/api/v1/td`

### 2. Unity에서 테스트

#### **A. MainGame 씬에 LoginUI 추가**

1. **Canvas 생성** (없으면)
2. **LoginUI GameObject 추가:**
   - Add Component → `LoginUI`
3. **UI 요소 연결** (Inspector에서):
   - `Login Panel` - 로그인 패널
   - `Register Panel` - 회원가입 패널
   - `Login Email` - 이메일 입력 필드
   - `Login Password` - 비밀번호 입력 필드
   - 버튼들 연결

#### **B. 게임 플레이**

1. Unity Play
2. LoginUI가 나타남 (로그인 안 되어있으면)
3. **회원가입** 또는 **로그인**
4. **게임 시작** (GameScene)
5. 게임 플레이...
6. **게임 종료** (승리/패배)
7. → 자동으로 결과가 서버로 전송됨! ✅

#### **C. 통계 확인**

```csharp
// 코드에서 호출
APIManager.Instance.GetStats(
    (stats) => {
        Debug.Log($"최고 라운드: {stats.single.highest_round}");
    },
    (error) => Debug.LogError(error)
);
```

---

## 🔧 코드 사용 예제

### 회원가입

```csharp
APIManager.Instance.Register(
    "player1",              // username
    "test@example.com",     // email
    "password123",          // password
    (response) => {
        Debug.Log($"가입 완료: {response.user.username}");
        Debug.Log($"토큰: {response.token}");
    },
    (error) => {
        Debug.LogError($"가입 실패: {error}");
    }
);
```

### 로그인

```csharp
APIManager.Instance.Login(
    "test@example.com",
    "password123",
    (response) => {
        Debug.Log($"로그인 성공: {response.user.username}");
    },
    (error) => {
        Debug.LogError($"로그인 실패: {error}");
    }
);
```

### 게임 결과 전송 (자동)

```csharp
// GameplayManager.cs에서 자동 호출됨
// Victory 또는 Defeat 상태가 되면 자동으로 전송
```

### 로그인 상태 확인

```csharp
if (APIManager.Instance.IsLoggedIn)
{
    Debug.Log($"로그인됨: {APIManager.Instance.Username}");
}
else
{
    Debug.Log("로그인 안 됨");
}
```

### 로그아웃

```csharp
APIManager.Instance.Logout();
```

---

## 📊 데이터 흐름

```
Unity → 백엔드 서버 → MySQL DB

1. 회원가입/로그인
   Unity LoginUI → POST /api/v1/td/auth/register
                → 200 OK + JWT 토큰
   Unity → PlayerPrefs.SetString("td_jwt_token", token)

2. 게임 플레이
   Unity → 로컬 게임 진행 (서버 통신 없음)

3. 게임 종료
   Unity GameplayManager.ChangeState(Victory/Defeat)
   → APIManager.SaveGameResult()
   → POST /api/v1/td/game/single/result
   → 200 OK + { game_id, new_highest_round, rewards }

4. 통계 조회
   Unity → GET /api/v1/td/users/me/stats
   → 200 OK + { single, coop, gold }
```

---

## 🔐 보안

### JWT 토큰 저장
- **위치:** `PlayerPrefs` (암호화되지 않음)
- **키:** `td_jwt_token`, `td_username`
- **유효 기간:** 24시간 (서버 설정)

### 주의사항
- **프로덕션 환경:**
  - BASE_URL을 실제 서버 주소로 변경
  - HTTPS 사용 필수
  - PlayerPrefs 대신 암호화된 저장소 사용 권장

---

## 🐛 디버깅

### 로그 확인

```csharp
// Unity Console에서 확인
[APIManager] Token saved, user: player1
[GameplayManager] Sending result: Round 10, Kills 50, Gold 200, Result victory
[GameplayManager] Result saved! Game ID: 1, New highest round: 10
```

### 서버 로그 확인

```bash
# Go 서버 콘솔에서
[INFO] POST /api/v1/td/auth/register 201
[INFO] POST /api/v1/td/game/single/result 201
```

### 일반적인 문제

**1. "Not logged in" 에러**
- 해결: `APIManager.Instance.IsLoggedIn` 확인
- 토큰이 만료되었을 수 있음 → 다시 로그인

**2. "Connection refused" 에러**
- 해결: 백엔드 서버가 실행 중인지 확인
- `http://localhost:18082/api/v1/td` 주소 확인

**3. "Invalid credentials" 에러**
- 해결: 이메일/비밀번호 확인

---

## 📝 TODO (향후 작업)

### 완료된 기능 ✅
- [x] HTTP 클라이언트
- [x] 인증 시스템 (회원가입/로그인)
- [x] JWT 토큰 관리
- [x] 게임 결과 자동 전송
- [x] 통계 조회

### 남은 작업 🚧
- [ ] LoginUI 프리팹 생성 (현재 스크립트만 존재)
- [ ] StatsUI 프리팹 생성
- [ ] MainGame 씬에 "내 기록" 버튼 추가
- [ ] 게임 히스토리 조회 (최근 10게임)
- [ ] 에러 처리 개선 (재시도 로직)
- [ ] 네트워크 타임아웃 설정
- [ ] 오프라인 모드 (결과 로컬 저장 → 온라인 시 업로드)

---

## 🎮 게임 모드별 상태

### ✅ 싱글 플레이 (완료)
- 로컬 게임 진행
- 결과만 서버 전송
- 통계 업데이트
- 순위 기록

### 🚧 협동 플레이 (미완성)
- REST API는 완성 (방 생성/참가)
- **WebSocket 필요** (실시간 동기화)
- 예상 작업: 3-4시간

---

## 🔗 관련 문서

- **백엔드 API 문서:** `~/project/joker_backend/services/lottoDefenseService/features/towerDefense/README.md`
- **백엔드 명세서:** `BACKEND_SPEC.md`
- **커서 AI 가이드:** `JOKER_BACKEND_INTEGRATION.md`

---

## 🚀 프로덕션 배포 체크리스트

### Unity 클라이언트
- [ ] BASE_URL을 실제 서버 주소로 변경
- [ ] HTTPS 사용
- [ ] 토큰 암호화 저장
- [ ] 에러 처리 강화
- [ ] 로딩 UI 추가

### 백엔드 서버
- [ ] JWT_SECRET 변경
- [ ] HTTPS 인증서 설정
- [ ] CORS 설정
- [ ] Rate limiting
- [ ] 로그 모니터링

---

**작성일:** 2026-02-18  
**상태:** 싱글 플레이 연동 완료 ✅  
**다음 단계:** LoginUI 프리팹 + MainGame 통합
