# Lotto Defense - 개발환경 셋업 가이드

---

## 1. 필수 소프트웨어

| 소프트웨어 | 버전 | 비고 |
|-----------|------|------|
| Unity Editor | 6000.3.9f1 | Unity Hub에서 설치 |
| Android SDK | API 25+ (MinSDK), Target: 최신 | Unity 설치 시 Android Build Support 포함 |
| JDK | Unity 내장 OpenJDK | 별도 설치 불필요 |
| Git | 최신 | 버전 관리 |

---

## 2. 프로젝트 클론 및 열기

```bash
git clone https://github.com/JokerTrickster/lotto-defense.git
cd lotto-defense
```

Unity Hub → `Open` → 클론한 폴더 선택 → Unity 6000.3.9f1로 열기.

---

## 3. Unity 패키지 의존성

> 소스: `Packages/manifest.json`

### 주요 패키지

| 패키지 | 버전 | 용도 |
|--------|------|------|
| com.unity.render-pipelines.universal | 17.3.0 | URP 2D 렌더링 |
| com.unity.ugui | 2.0.0 | UI 시스템 |
| com.unity.2d.animation | 13.0.4 | 2D 애니메이션 |
| com.unity.2d.sprite | 1.0.0 | 스프라이트 |
| com.unity.inputsystem | 1.18.0 | 입력 시스템 (패키지 설치됨, 사용 안 함) |
| com.endel.nativewebsocket | GitHub | WebSocket 통신 |
| com.unity.test-framework | 1.6.0 | 테스트 |

패키지는 프로젝트를 열면 자동으로 Resolve됨.

NativeWebSocket은 GitHub URL 참조:
```
"com.endel.nativewebsocket": "https://github.com/endel/NativeWebSocket.git#upm"
```

---

## 4. 프로젝트 설정

### 4.1 Input Handler

> 소스: `ProjectSettings/ProjectSettings.asset`

```
activeInputHandler: 0  (Old Input Manager)
```

**주의:** `activeInputHandler: 2` (Both)로 설정하면 Android 빌드 시 경고 발생:
```
PlayerSettings->Active Input Handling is set to Both, this is unsupported on Android
```

`com.unity.inputsystem` 패키지가 설치되어 있지만 실제로 Old Input Manager만 사용.

### 4.2 Rendering

- URP 2D (Universal Render Pipeline)
- `URP_COMPATIBILITY_MODE` 스크립팅 심볼 정의됨 (Android)

### 4.3 CanvasScaler

- 모든 UI는 코드로 동적 생성 (프리팹 없음)
- `GameSceneBootstrapper.cs`와 `MainGameBootstrapper.cs`에서 Canvas 생성
- 기본 해상도: 1920×1080

### 4.4 Unity Services

- Unity Connect: 비활성화 (`m_Enabled: 0` in `UnityConnectSettings.asset`)
- Cloud 서비스 사용하지 않음
- **주의:** Unity 업그레이드 시 `m_Enabled`이 `1`로 리셋될 수 있음 → "Missing Project ID" 에러 발생

---

## 5. Android 빌드

### 5.1 빌드 설정

| 항목 | 값 |
|------|-----|
| Scripting Backend | IL2CPP |
| Target Architecture | ARM64 |
| Min SDK | 25 (Android 7.1) |
| Target SDK | 0 (최신) |
| Bundle Identifier | com.DefaultCompany.2D-URP |
| Bundle Version | 1.0 |

### 5.2 빌드 방법

1. `File` → `Build Settings`
2. Platform: `Android` 선택 → `Switch Platform`
3. 기기 USB 연결 + 개발자 모드 + USB 디버깅 활성화
4. `Build and Run`

### 5.3 알려진 이슈

| 이슈 | 원인 | 해결 |
|------|------|------|
| "Missing Project ID" | `UnityConnectSettings.asset`의 `m_Enabled: 1` | `m_Enabled: 0`으로 변경 |
| "Unsupported input handling" | `activeInputHandler: 2` (Both) | `activeInputHandler: 0`으로 변경 |
| Build & Run 실패 | USB 드라이버, ADB 인식 문제 | ADB 재시작, USB 재연결 |

---

## 6. 백엔드 서버 연결

### 6.1 REST API

> 소스: `Assets/Scripts/Backend/APIClient.cs`

```csharp
private const string BASE_URL = "http://localhost:18082/api/v1/td";
```

서버가 로컬에서 실행 중이어야 로그인, 통계, 랭킹 등 사용 가능.
서버 미실행 시에도 게임 자체는 플레이 가능 (오프라인 모드).

### 6.2 WebSocket (멀티플레이어)

> 소스: `Assets/Scripts/UI/MultiplayerLobbyUI.cs`

```
ws://localhost:8080/ws
```

멀티플레이어 기능을 사용하려면 WebSocket 서버 실행 필요.

### 6.3 서버 변경

API URL을 변경하려면:
- REST: `Assets/Scripts/Backend/APIClient.cs` → `BASE_URL` 상수 수정
- WebSocket: `Assets/Scripts/UI/MultiplayerLobbyUI.cs` → URL 하드코딩 부분 수정

---

## 7. 디렉토리 구조

```
Assets/Scripts/
├── Authentication/     # 로그인/회원가입 UI
├── Backend/            # REST API 클라이언트, 모델
│   ├── Models/         # 요청/응답 데이터 클래스
│   └── UI/             # 로그인, 통계, 랭킹 UI
├── Combat/             # 전투 매니저
├── Controllers/        # 입력/게임 컨트롤러
├── Gameplay/           # 게임 루프, 밸런스, 라운드, 부트스트래퍼
├── Grid/               # 4x5 그리드 시스템
├── Lobby/              # 메인 로비 부트스트래퍼, UI
├── Monsters/           # 몬스터 스폰, 이동, AI
├── Networking/         # WebSocket, 멀티플레이어
├── Profile/            # 프로필 아바타, 해금
├── Quests/             # 퀘스트 시스템
├── ScriptableObjects/  # 데이터 에셋 정의
├── UI/                 # 공통 UI 컴포넌트
├── Units/              # 유닛, 스킬, 업그레이드, 배치
├── Utils/              # 유틸리티 (폰트, 반사, 헬퍼)
└── VFX/                # 시각 효과 (프로시저럴 생성)
```

---

## 8. 주요 아키텍처 패턴

| 패턴 | 사용 위치 |
|------|----------|
| 싱글톤 | 대부분의 매니저 (GameplayManager, GridManager 등) |
| 프로시저럴 UI | 모든 UI가 코드로 동적 생성 (프리팹 없음) |
| 디자인 토큰 | `GameSceneDesignTokens.cs`, `LobbyDesignTokens.cs` |
| ScriptableObject | 게임 데이터 (`UnitData`, `MonsterData`, `GameBalanceConfig` 등) |
| 상태 머신 | `GameplayManager` (Countdown → Preparation → Combat → RoundResult) |

---

## 9. 씬 구조

| 씬 | 용도 | 부트스트래퍼 |
|----|------|-------------|
| MainGame | 로비/메인 화면 | `MainGameBootstrapper.cs` |
| GameScene | 인게임 플레이 | `GameSceneBootstrapper.cs` |
