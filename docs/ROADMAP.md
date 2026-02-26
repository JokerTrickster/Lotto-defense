# Lotto Defense - 개발 로드맵

---

## 1. 완료된 기능

### 핵심 게임플레이
- [x] 유닛 소환 (가챠) 시스템
- [x] 4x5 그리드 배치 시스템
- [x] 유닛 5종 (Warrior, Archer, Mage, DragonKnight, Phoenix)
- [x] 공격 패턴 5종 (SingleTarget, Pierce, AOE, Splash, Chain)
- [x] 스킬 시스템 (Active, Passive, OnHit, OnKill)
- [x] 몬스터 3종 + 보스
- [x] 몬스터 경로 이동
- [x] CC 시스템 (슬로우, 프리즈)
- [x] 조합 시스템 (같은 유닛 2개 → 상위)
- [x] 자동 조합
- [x] 유닛 업그레이드 (공격력/공속)
- [x] 유닛 판매
- [x] 라운드 시스템 (준비 → 전투 → 결과)
- [x] 난이도 3단계 (Normal, Hard, VeryHard)
- [x] 게임 속도 조절 (x1, x2, x3)
- [x] 전투 데미지 공식

### UI/UX
- [x] 코드 기반 UI 동적 생성 (프리팹 없음)
- [x] 디자인 토큰 시스템
- [x] 프로시저럴 스프라이트 생성
- [x] 유닛 정보 패널 (스탯, 스킬 설명)
- [x] 마나 바
- [x] 사거리 인디케이터
- [x] 프로시저럴 VFX (공격, 스킬, 피격, 소환, 업그레이드)
- [x] 난이도 선택 팝업
- [x] 게임 결과 화면

### 메타 시스템
- [x] 프로필 시스템 (아바타 7종, 닉네임)
- [x] 프로필 선택 모달 (4열 그리드)
- [x] 퀘스트 시스템 (CollectUnits, PositionUnits)
- [x] 퀘스트 기반 프로필 해금
- [x] 유닛 상점 (해금)

### 네트워크
- [x] REST API 클라이언트 (로그인, 회원가입, 결과 저장, 랭킹)
- [x] WebSocket 멀티플레이어 기본 구조
- [x] 방 생성/참가/자동 매칭
- [x] Co-op 상태 동기화

### 빌드
- [x] Android 빌드 설정 (IL2CPP, ARM64)
- [x] Input Handler 설정 (Old Input Manager)
- [x] Unity Services 비활성화

---

## 2. 미완성 기능

### 높은 우선순위

| 항목 | 상태 | 위치 | 설명 |
|------|------|------|------|
| Google Sign-In | 플레이스홀더 | `AuthenticationManager.cs:50` | `SimulateAuthentication()` 호출만 함. Google OAuth SDK 연동 필요 |
| 통계 UI (StatsUI) | 비어있음 | `Backend/UI/StatsUI.cs:148` | API는 준비됨, UI 구현 필요 |
| 랭킹 UI (RankingUI) | 비어있음 | `Backend/UI/RankingUI.cs:180` | API는 준비됨, UI 구현 필요 |
| 몬스터 처치수 추적 | 추정치 사용 | `UI/GameResultUI.cs:198` | `roundsCompleted × 30`으로 추정. 실제 통계 매니저 필요 |

### 중간 우선순위

| 항목 | 상태 | 위치 | 설명 |
|------|------|------|------|
| 프로필 해금 알림 | Debug.Log만 | `ProfileUnlockManager.cs:142` | 팝업 UI 필요 |
| QuestManager 연동 | TODO | `UserProfileManager.cs:359` | 퀘스트 완료 → 아바타 해금 연결 미완 |
| AchievementManager | 미존재 | `UserProfileManager.cs:367` | 클래스 자체가 없음 |
| 로비 조합 버튼 | 로그만 | `MainGameBootstrapper.cs:441` | 클릭 시 조합 UI 표시 안 됨 |
| 게임 히스토리 UI | API만 존재 | `APIManager.cs` | `GetGameHistory` 정의됨, 호출하는 UI 없음 |
| 유저 정보 조회 | API만 존재 | `APIManager.cs` | `GetUserInfo` 정의됨, 호출하는 UI 없음 |

---

## 3. 기술 부채

| 항목 | 심각도 | 위치 | 설명 |
|------|--------|------|------|
| 리플렉션 기반 UI 와이어링 | 중 | `GameSceneBootstrapper.UI.cs` 전반 | `SetField()` 사용. 타입 안전하지 않고 유지보수 어려움 |
| 프리팹 없는 전체 UI | 중 | 부트스트래퍼 전체 | 1000줄+ UI 코드. 프리팹 전환 검토 필요 |
| 하드코딩된 서버 URL | 중 | `APIClient.cs:15`, `MultiplayerLobbyUI.cs` | 환경별 설정 시스템 필요 |
| 싱글톤 과다 사용 | 낮 | 매니저 클래스 전반 | 테스트 어렵고 의존성 불명확 |
| 주석 처리된 코드 | 낮 | `CombatManager.cs:93`, `GridManager.cs:59` 등 | CS0067/CS0414 방지용. 삭제 또는 활용 |
| Bundle Identifier | 낮 | `ProjectSettings.asset` | `com.DefaultCompany.2D-URP` → 실제 ID로 변경 필요 |
| 테스트 부족 | 높 | 프로젝트 전반 | Editor 테스트 2개, In-game 테스터 3개만 존재 |
| CoopStateSync 안정성 | 중 | `Networking/CoopStateSync.cs` | 서버 응답 파싱이 취약할 수 있음 |

---

## 4. 개선 필요 항목

### VFX
- [ ] 프로시저럴 VFX → 실제 이미지 에셋으로 교체
- [ ] 파티클 시스템 도입 (Unity Particle System)
- [ ] 스킬별 전용 이펙트 에셋

### UI/UX
- [ ] 프리팹 기반 UI 전환 검토 (유지보수성 향상)
- [ ] UI 에셋 (버튼, 패널 스프라이트) 도입
- [ ] 튜토리얼 시스템
- [ ] 설정 화면 (사운드, 진동 등)

### 오디오
- [ ] BGM 시스템
- [ ] 효과음 (공격, 스킬, 몬스터 피격, UI)
- [ ] 오디오 매니저

---

## 5. 향후 기능 제안

### Phase 1: 핵심 완성
- [ ] Google/Apple 소셜 로그인
- [ ] 통계/랭킹 UI 완성
- [ ] 실제 몬스터 처치 통계
- [ ] 프로필 해금 알림 팝업
- [ ] 일일 보상 UI 완성
- [ ] 우편함 UI 완성

### Phase 2: 콘텐츠 확장
- [ ] 새 유닛 추가 (Support, Assassin 등)
- [ ] 새 몬스터 타입 (비행, 스텔스, 힐러)
- [ ] 추가 라운드 (11~30라운드 콘텐츠)
- [ ] 보스 다양화 (라운드별 다른 보스)
- [ ] 새 스킬 프리셋
- [ ] 이벤트 퀘스트

### Phase 3: 시스템 고도화
- [ ] 서버 URL 환경 설정 (Dev/Staging/Production)
- [ ] 에러 리포팅 (Sentry, Firebase Crashlytics)
- [ ] 앱스토어 배포 (Google Play, App Store)
- [ ] 인앱 결제
- [ ] 푸시 알림
- [ ] AB 테스트 프레임워크

### Phase 4: 라이브 운영
- [ ] 시즌 시스템
- [ ] 길드/클랜
- [ ] PvP 랭킹전
- [ ] 리플레이 시스템
- [ ] 분석 대시보드

---

## 6. 현재 테스트 현황

| 파일 | 종류 | 대상 |
|------|------|------|
| `Assets/Tests/Editor/DifficultyConfigTests.cs` | NUnit Editor | DifficultyConfig |
| `Assets/Tests/Editor/RoundManagerTests.cs` | NUnit Editor | RoundManager |
| `Assets/Scripts/Grid/GridSystemTester.cs` | In-game | 그리드 시스템 |
| `Assets/Scripts/VFX/VFXManagerTester.cs` | In-game | VFX |
| `Assets/Scripts/Gameplay/GameplayManagerTester.cs` | In-game | 게임플레이 루프 |

**필요한 테스트:**
- 전투 데미지 계산 유닛 테스트
- 조합 로직 유닛 테스트
- 업그레이드 공식 유닛 테스트
- 스킬 발동/쿨다운 테스트
- API 응답 파싱 테스트
- 몬스터 스케일링 테스트
