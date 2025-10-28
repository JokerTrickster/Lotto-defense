# 🚀 Lotto Defense - 빠른 설정 가이드

## 1️⃣ 현재 실행 가능한 것

✅ **LoginScene** - 로그인 화면 (완전히 작동)
✅ **MainGame** - 메인 메뉴 (완전히 작동)
⚠️ **GameScene** - 게임 플레이 (코드 완성, Unity 설정 필요)

## 2️⃣ 바로 실행해보기

### Unity Editor에서:
1. **Unity Hub** 실행
2. **Lotto-defense** 프로젝트 열기
3. **Project 창** → `Assets/Scenes/LoginScene.unity` 더블클릭
4. **▶️ Play 버튼** 클릭
5. "Login with Google" 클릭 → 자동 로그인
6. "게임 시작" 클릭 → GameScene으로 이동

**현재 상태:** GameScene에 "GAME SCREEN" 플레이스홀더가 표시됩니다.

## 3️⃣ 완전한 게임플레이 활성화 (선택사항)

새로 구현된 10개 시스템을 활성화하려면 GameScene 설정이 필요합니다.

### 필수 설정 파일 위치:

모든 설정 가이드가 준비되어 있습니다:

**핵심 시스템:**
- `Assets/Scripts/Gameplay/SETUP_INSTRUCTIONS.md` - 게임 상태 머신
- `Assets/Scripts/Grid/GridSystemSetup.md` - 그리드 시스템
- `Assets/Scripts/Units/SETUP_INSTRUCTIONS.md` - 유닛 시스템

**고급 기능:**
- `Assets/Scripts/Units/SYNTHESIS_SYSTEM_README.md` - 합성 시스템
- `Assets/Scripts/Units/README_Upgrade_System.md` - 업그레이드 시스템

### 설정 우선순위:

**최소 동작을 위한 필수 설정 (30분):**
1. GameplayManager 컴포넌트 추가
2. GridManager 컴포넌트 추가
3. GameHUD 텍스트 연결

**완전한 게임플레이 (2-3시간):**
1. 위 필수 설정 완료
2. ScriptableObject 에셋 12개 생성 (Units)
3. UI 프리팹 생성 및 연결
4. 테스트 및 밸런스 조정

## 4️⃣ 테스트 도구 사용

각 시스템에는 테스트 도구가 포함되어 있습니다:

**게임 실행 중 키보드 단축키:**
- **[T]** - 그리드 시스템 테스트
- **[1-6]** - 게임 상태 전환 테스트
- **[Space]** - 현재 상태 표시
- **[+/-]** - 골드/라이프 조정

## 5️⃣ 도움말

**설정 중 문제가 발생하면:**
1. Console 창에서 에러 메시지 확인
2. 해당 시스템의 README.md 파일 참조
3. SETUP_INSTRUCTIONS.md의 Troubleshooting 섹션 확인

**주요 문서:**
- `/Assets/Scripts/Gameplay/` - 게임 로직
- `/Assets/Scripts/Grid/` - 그리드 시스템
- `/Assets/Scripts/Units/` - 유닛 관리
- `/Assets/Scripts/Monsters/` - 몬스터 시스템
- `/Assets/Scripts/Combat/` - 전투 시스템
- `/Assets/Scripts/VFX/` - 비주얼 이펙트

## 📊 시스템 현황

✅ **완료된 코드:** ~15,000 라인, 60개 파일
⏳ **Unity 설정 필요:** ScriptableObject 생성, UI 연결
🎮 **테스트 가능:** 각 시스템별 테스터 포함

---

**빠른 시작:** LoginScene → Play로 지금 바로 테스트 가능!
**완전한 게임:** 위 설정 가이드 따라 2-3시간 작업 후 전체 게임플레이 가능!
