# 로그인 화면 구현 가이드

## 개요

피그마 디자인을 기반으로 한 **커플 보드게임** 로그인 화면 Unity 구현.

---

## 디자인 스펙

### 📱 레이아웃

```
┌─────────────────────┐
│                     │
│   [캐릭터 로고]     │  ← 핑크 배경 (200×200)
│   커플 보드게임     │  ← 제목 (32pt)
│   BOARD GAME        │  ← 서브타이틀 (18pt)
│                     │
│  [구글 연결] G      │  ← 흰색 버튼 (500×80)
│                     │
│      또는           │  ← 구분선
│                     │
│    [Apple] 🍎       │  ← 검정 버튼 (500×80)
│    [Guest] 🔒       │  ← 흰색 버튼 (500×80)
│                     │
│ 로그인하면 I 동의   │  ← 이용약관 (16pt)
│                     │
│    [   시작   ]     │  ← 핑크 큰 버튼 (500×100)
└─────────────────────┘
```

---

### 🎨 색상 팔레트

```csharp
배경 그라데이션 (상단): #FFF5F0 (연한 핑크)
배경 그라데이션 (하단): #FFE8E0 (복숭아색)
로고 배경: #FFB6C1 (핑크)
주요 버튼: #FFB6C1 (핑크)
텍스트: #000000 (검정)
구분선: #808080 (회색)
```

---

## 구현 파일

### 1. LoginUI.cs

**위치:** `Assets/Scripts/UI/LoginUI.cs`

**기능:**
- ✅ 그라데이션 배경 (핑크/복숭아색)
- ✅ 로고 영역 (캐릭터 + 타이틀)
- ✅ 소셜 로그인 버튼 (구글/애플/게스트)
- ✅ 이용약관 텍스트
- ✅ 시작 버튼
- ✅ 로딩 오버레이
- ✅ Safe Area 지원

**자동 생성:**
- Canvas
- UI 컴포넌트 (버튼, 텍스트, 이미지)
- 이벤트 핸들러

---

### 2. LoginSceneBootstrapper.cs

**위치:** `Assets/Scripts/Login/LoginSceneBootstrapper.cs`

**기능:**
- ✅ RuntimeInitializeOnLoadMethod로 자동 실행
- ✅ LoginScene 로드 시 LoginUI 자동 생성
- ✅ 수동 설정 불필요

---

## 사용 방법

### ✅ 자동 설정 (권장)

**아무것도 할 필요 없습니다!**

1. Unity Play 버튼 클릭
2. LoginScene이 첫 번째 씬이면 자동으로 UI 생성
3. 끝!

---

### 🎮 수동 설정 (선택사항)

#### 1. 새 씬 생성

```
File → New Scene
File → Save As → "LoginScene"
```

#### 2. Build Settings 설정

```
File → Build Settings
Scenes in Build:
  0. LoginScene  ← 첫 번째
  1. MainGame
```

#### 3. Play 버튼 클릭

LoginUI가 자동으로 생성됩니다!

---

## 로그인 버튼 동작

### 현재 구현 (Mock)

```csharp
✅ 구글 로그인 → 1.5초 로딩 → MainGame 씬
✅ 애플 로그인 → 1.5초 로딩 → MainGame 씬
✅ 게스트 로그인 → 1.5초 로딩 → MainGame 씬
✅ 시작 버튼 → 구글 로그인과 동일
```

### TODO: 백엔드 연동

```csharp
private void OnGoogleLoginClicked()
{
    // Google OAuth 구현
    // POST /auth/google/signin
    
    GoogleSignIn.DefaultInstance.SignIn()
        .ContinueWith(task => {
            string idToken = task.Result.IdToken;
            
            StartCoroutine(APIManager.Instance.Client.Post(
                "/auth/google/signin",
                new { id_token = idToken },
                (AuthResponse response) => {
                    PlayerPrefs.SetString("JWT_TOKEN", response.token);
                    OnLoginSuccess();
                },
                (error) => ShowError(error)
            ));
        });
}
```

---

## 커스터마이징

### 색상 변경

**LoginUI.cs (25-29줄):**

```csharp
// 기본 핑크
private static readonly Color PINK_PRIMARY = new Color(1f, 0.71f, 0.76f, 1f);

// 파란색으로 변경
private static readonly Color PINK_PRIMARY = new Color(0.4f, 0.7f, 1f, 1f);
```

---

### 로고 이미지 추가

**1. 이미지 준비:**
```
Assets/Resources/UI/LoginCharacter.png
```

**2. LoginUI.cs (154줄) 수정:**

```csharp
// TODO 주석 제거하고 활성화
Sprite characterSprite = Resources.Load<Sprite>("UI/LoginCharacter");
if (characterSprite != null)
{
    logoBackground.sprite = characterSprite;
    logoBackground.type = Image.Type.Simple;
}
```

---

### 버튼 크기 변경

**LoginUI.cs (188줄):**

```csharp
// 기본 크기
float buttonWidth = 500f;
float buttonHeight = 80f;

// 더 크게
float buttonWidth = 600f;
float buttonHeight = 100f;
```

---

### 폰트 변경

**1. 커스텀 폰트 추가:**
```
Assets/Fonts/YourCustomFont.ttf
```

**2. LoginUI.cs (95줄) 수정:**

```csharp
Font defaultFont = Resources.Load<Font>("Fonts/YourCustomFont");
if (defaultFont == null)
    defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
```

---

## 백엔드 API 연동

### Google OAuth

**Endpoint:** `POST /auth/google/signin`

**Request:**
```json
{
  "id_token": "eyJhbGciOiJSUzI1NiIsImtpZCI..."
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
      "id": 1,
      "email": "user@gmail.com",
      "name": "홍길동"
    }
  }
}
```

---

### Apple Sign In

**Endpoint:** `POST /auth/apple/signin` (예정)

**Request:**
```json
{
  "identity_token": "eyJraWQiOiJlWGF1...",
  "authorization_code": "c8e8cd..."
}
```

---

### Guest Login

**Endpoint:** `POST /auth/guest` (예정)

**Request:**
```json
{
  "device_id": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "guest_id": "guest_123456"
  }
}
```

---

## 테스트

### Unity Editor

```
1. Play 버튼 클릭
2. 로그인 화면 표시 확인
3. 버튼 클릭 테스트
4. 로딩 오버레이 확인
5. MainGame 씬 전환 확인
```

---

### 모바일 빌드

```
1. Build Settings → Android/iOS
2. Build & Run
3. 실제 디바이스에서 테스트
4. Safe Area 확인 (노치 대응)
5. 터치 반응 확인
```

---

## UI 계층 구조

```
LoginCanvas
└── LoginPanel
    ├── GradientBackground
    │   ├── BottomColor (#FFE8E0)
    │   └── TopColor (#FFF5F0)
    └── SafeArea
        ├── LogoContainer (상단)
        │   ├── LogoBackground (핑크 원)
        │   ├── Title (커플 보드게임)
        │   └── Subtitle (BOARD GAME)
        ├── GoogleLoginButton (중앙)
        ├── Divider (또는)
        ├── AppleLoginButton
        ├── GuestLoginButton
        ├── TermsText (이용약관)
        ├── StartButton (큰 핑크)
        └── LoadingOverlay (숨김)
```

---

## 애니메이션 추가 (선택사항)

### DOTween 설치 후:

```csharp
using DG.Tweening;

private void Start()
{
    CreateLoginUI();
    CheckAutoLogin();
    
    // 페이드인 애니메이션
    CanvasGroup canvasGroup = loginPanel.AddComponent<CanvasGroup>();
    canvasGroup.alpha = 0f;
    canvasGroup.DOFade(1f, 0.5f);
    
    // 로고 펄스 효과
    logoBackground.transform.DOScale(1.1f, 1f)
        .SetLoops(-1, LoopType.Yoyo)
        .SetEase(Ease.InOutSine);
}
```

---

## 문제 해결

### Q: 로그인 화면이 안 나타남
**A:** Build Settings에서 LoginScene이 첫 번째인지 확인

---

### Q: 버튼이 안 눌림
**A:** EventSystem 확인:
```csharp
if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
{
    GameObject eventSystem = new GameObject("EventSystem");
    eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
    eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
}
```

---

### Q: Safe Area가 안 맞음
**A:** SafeAreaHelper 추가:
```csharp
public class SafeAreaHelper : MonoBehaviour
{
    void Awake()
    {
        RectTransform rect = GetComponent<RectTransform>();
        Rect safeArea = Screen.safeArea;
        
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;
        
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;
        
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
    }
}
```

---

## 향후 개선 사항

```
✅ 완료:
- 기본 UI 레이아웃
- 소셜 로그인 버튼
- 그라데이션 배경
- 로딩 오버레이

📋 TODO:
- Google OAuth 연동
- Apple Sign In 연동
- 게스트 로그인 구현
- 이용약관 팝업
- 비밀번호 찾기
- 회원가입 화면
- 애니메이션 효과
- 다국어 지원
```

---

## 참고

- **피그마 디자인:** https://www.figma.com/design/XoXEKlH3AEk3k5EyH7I53U/Untitled?node-id=153-2003
- **백엔드 API:** `~/project/joker_backend`
- **Unity 프로젝트:** `~/project/Lotto-defense`

---

**이제 Unity에서 Play 버튼만 누르면 로그인 화면이 자동으로 생성됩니다!** 🎉
