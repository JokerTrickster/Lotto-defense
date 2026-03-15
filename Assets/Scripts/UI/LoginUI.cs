using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LottoDefense.Backend;
using System.Collections;

namespace LottoDefense.UI
{
    /// <summary>
    /// 커플 보드게임 로그인 UI
    /// 피그마 디자인: 구글/애플/게스트 로그인
    /// </summary>
    public class LoginUI : MonoBehaviour
    {
        #region UI Elements
        private GameObject loginPanel;
        private GameObject logoContainer;
        private Image logoBackground;
        private Text titleText;
        private Text subtitleText;
        
        private Button googleLoginButton;
        private Button appleLoginButton;
        private Button guestLoginButton;
        private Button startButton;
        
        private Text termsText;
        private GameObject loadingOverlay;
        private Text errorText;
        #endregion

        #region Constants
        private static readonly Color BG_GRADIENT_TOP = new Color(1f, 0.96f, 0.94f, 1f);    // #FFF5F0
        private static readonly Color BG_GRADIENT_BOTTOM = new Color(1f, 0.91f, 0.88f, 1f); // #FFE8E0
        private static readonly Color PINK_PRIMARY = new Color(1f, 0.71f, 0.76f, 1f);       // #FFB6C1
        private static readonly Color BLACK = new Color(0f, 0f, 0f, 1f);
        private static readonly Color WHITE = new Color(1f, 1f, 1f, 1f);
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            CreateLoginUI();
            CheckAutoLogin();
        }
        #endregion

        #region UI Creation
        private void CreateLoginUI()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("LoginCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080, 1920);
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null)
                defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // 메인 패널
            loginPanel = new GameObject("LoginPanel");
            loginPanel.transform.SetParent(canvas.transform, false);
            RectTransform panelRect = loginPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;

            // 그라데이션 배경
            CreateGradientBackground(loginPanel);

            // Safe Area
            GameObject safeAreaObj = new GameObject("SafeArea");
            safeAreaObj.transform.SetParent(loginPanel.transform, false);
            RectTransform safeAreaRect = safeAreaObj.AddComponent<RectTransform>();
            safeAreaRect.anchorMin = Vector2.zero;
            safeAreaRect.anchorMax = Vector2.one;
            safeAreaRect.sizeDelta = Vector2.zero;

            // 로고 영역 (상단)
            CreateLogoSection(safeAreaObj, defaultFont);

            // 로그인 버튼들 (중앙)
            CreateLoginButtons(safeAreaObj, defaultFont);

            // 하단 이용약관 + 시작 버튼
            CreateBottomSection(safeAreaObj, defaultFont);

            // 로딩 오버레이
            CreateLoadingOverlay(loginPanel, defaultFont);

            Debug.Log("[LoginUI] UI created");
        }

        private void CreateGradientBackground(GameObject parent)
        {
            GameObject bgObj = new GameObject("GradientBackground");
            bgObj.transform.SetParent(parent.transform, false);
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            // 간단한 그라데이션 (2개 이미지 오버레이)
            Image bottomBg = bgObj.AddComponent<Image>();
            bottomBg.color = BG_GRADIENT_BOTTOM;

            GameObject topBgObj = new GameObject("GradientTop");
            topBgObj.transform.SetParent(bgObj.transform, false);
            RectTransform topRect = topBgObj.AddComponent<RectTransform>();
            topRect.anchorMin = new Vector2(0, 0.5f);
            topRect.anchorMax = Vector2.one;
            topRect.sizeDelta = Vector2.zero;

            Image topBg = topBgObj.AddComponent<Image>();
            topBg.color = BG_GRADIENT_TOP;
            topBg.raycastTarget = false;
        }

        private void CreateLogoSection(GameObject parent, Font font)
        {
            // 로고 컨테이너 (상단 중앙)
            logoContainer = new GameObject("LogoContainer");
            logoContainer.transform.SetParent(parent.transform, false);
            RectTransform logoRect = logoContainer.AddComponent<RectTransform>();
            logoRect.anchorMin = new Vector2(0.5f, 1f);
            logoRect.anchorMax = new Vector2(0.5f, 1f);
            logoRect.pivot = new Vector2(0.5f, 1f);
            logoRect.anchoredPosition = new Vector2(0, -100);
            logoRect.sizeDelta = new Vector2(300, 350);

            // 로고 배경 (핑크 둥근 사각형)
            GameObject logoBgObj = new GameObject("LogoBackground");
            logoBgObj.transform.SetParent(logoContainer.transform, false);
            RectTransform logoBgRect = logoBgObj.AddComponent<RectTransform>();
            logoBgRect.anchorMin = new Vector2(0.5f, 1f);
            logoBgRect.anchorMax = new Vector2(0.5f, 1f);
            logoBgRect.pivot = new Vector2(0.5f, 1f);
            logoBgRect.anchoredPosition = Vector2.zero;
            logoBgRect.sizeDelta = new Vector2(200, 200);

            logoBackground = logoBgObj.AddComponent<Image>();
            logoBackground.color = PINK_PRIMARY;

            // TODO: 캐릭터 이미지 추가
            // Sprite characterSprite = Resources.Load<Sprite>("UI/LoginCharacter");
            // logoBackground.sprite = characterSprite;

            // 제목 "커플 보드게임"
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(logoContainer.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0f);
            titleRect.anchorMax = new Vector2(0.5f, 0f);
            titleRect.pivot = new Vector2(0.5f, 0f);
            titleRect.anchoredPosition = new Vector2(0, 80);
            titleRect.sizeDelta = new Vector2(300, 60);

            titleText = titleObj.AddComponent<Text>();
            titleText.text = "커플\n보드게임";
            titleText.font = font;
            titleText.fontSize = 32;
            titleText.fontStyle = FontStyle.Bold;
            titleText.color = BLACK;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.lineSpacing = 1.2f;

            // 서브타이틀 "BOARD GAME"
            GameObject subtitleObj = new GameObject("Subtitle");
            subtitleObj.transform.SetParent(logoContainer.transform, false);
            RectTransform subtitleRect = subtitleObj.AddComponent<RectTransform>();
            subtitleRect.anchorMin = new Vector2(0.5f, 0f);
            subtitleRect.anchorMax = new Vector2(0.5f, 0f);
            subtitleRect.pivot = new Vector2(0.5f, 0f);
            subtitleRect.anchoredPosition = new Vector2(0, 40);
            subtitleRect.sizeDelta = new Vector2(300, 30);

            subtitleText = subtitleObj.AddComponent<Text>();
            subtitleText.text = "BOARD GAME";
            subtitleText.font = font;
            subtitleText.fontSize = 18;
            subtitleText.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            subtitleText.alignment = TextAnchor.MiddleCenter;
        }

        private void CreateLoginButtons(GameObject parent, Font font)
        {
            float buttonWidth = 500f;
            float buttonHeight = 80f;
            float spacing = 20f;
            float startY = -500f;

            // 구글 로그인 버튼
            googleLoginButton = CreateSocialButton(
                parent,
                "GoogleLoginButton",
                "구글 연결",
                WHITE,
                BLACK,
                new Vector2(0, startY),
                new Vector2(buttonWidth, buttonHeight),
                font
            );
            googleLoginButton.onClick.AddListener(OnGoogleLoginClicked);

            // "또는" 구분선
            CreateDivider(parent, "또는", new Vector2(0, startY - buttonHeight - spacing - 30), font);

            // Apple 로그인 버튼
            appleLoginButton = CreateSocialButton(
                parent,
                "AppleLoginButton",
                " 로그인",
                BLACK,
                WHITE,
                new Vector2(0, startY - buttonHeight - spacing - 90),
                new Vector2(buttonWidth, buttonHeight),
                font
            );
            appleLoginButton.onClick.AddListener(OnAppleLoginClicked);

            // 게스트 로그인 버튼
            guestLoginButton = CreateSocialButton(
                parent,
                "GuestLoginButton",
                "🔒 게스트로 시작",
                WHITE,
                BLACK,
                new Vector2(0, startY - (buttonHeight + spacing) * 2 - 90),
                new Vector2(buttonWidth, buttonHeight),
                font
            );
            guestLoginButton.onClick.AddListener(OnGuestLoginClicked);
        }

        private Button CreateSocialButton(GameObject parent, string name, string text, Color bgColor, Color textColor, Vector2 position, Vector2 size, Font font)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent.transform, false);
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 1f);
            btnRect.anchorMax = new Vector2(0.5f, 1f);
            btnRect.pivot = new Vector2(0.5f, 0.5f);
            btnRect.anchoredPosition = position;
            btnRect.sizeDelta = size;

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = bgColor;

            // 둥근 모서리 효과 (간단한 외곽선)
            Outline outline = btnObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);
            outline.effectDistance = new Vector2(2, -2);

            Button button = btnObj.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.95f, 0.95f, 0.95f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            button.colors = colors;

            // 텍스트
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            Text btnText = textObj.AddComponent<Text>();
            btnText.text = text;
            btnText.font = font;
            btnText.fontSize = 24;
            btnText.color = textColor;
            btnText.alignment = TextAnchor.MiddleCenter;

            return button;
        }

        private void CreateDivider(GameObject parent, string text, Vector2 position, Font font)
        {
            GameObject dividerObj = new GameObject("Divider");
            dividerObj.transform.SetParent(parent.transform, false);
            RectTransform dividerRect = dividerObj.AddComponent<RectTransform>();
            dividerRect.anchorMin = new Vector2(0.5f, 1f);
            dividerRect.anchorMax = new Vector2(0.5f, 1f);
            dividerRect.pivot = new Vector2(0.5f, 0.5f);
            dividerRect.anchoredPosition = position;
            dividerRect.sizeDelta = new Vector2(500, 40);

            Text dividerText = dividerObj.AddComponent<Text>();
            dividerText.text = text;
            dividerText.font = font;
            dividerText.fontSize = 18;
            dividerText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            dividerText.alignment = TextAnchor.MiddleCenter;
        }

        private void CreateBottomSection(GameObject parent, Font font)
        {
            // 이용약관 텍스트
            GameObject termsObj = new GameObject("TermsText");
            termsObj.transform.SetParent(parent.transform, false);
            RectTransform termsRect = termsObj.AddComponent<RectTransform>();
            termsRect.anchorMin = new Vector2(0.5f, 0f);
            termsRect.anchorMax = new Vector2(0.5f, 0f);
            termsRect.pivot = new Vector2(0.5f, 0f);
            termsRect.anchoredPosition = new Vector2(0, 250);
            termsRect.sizeDelta = new Vector2(500, 40);

            termsText = termsObj.AddComponent<Text>();
            termsText.text = "로그인하면  I  이용약관 동의";
            termsText.font = font;
            termsText.fontSize = 16;
            termsText.color = new Color(0.4f, 0.4f, 0.4f, 1f);
            termsText.alignment = TextAnchor.MiddleCenter;

            // 시작 버튼 (큰 핑크 버튼)
            GameObject startBtnObj = new GameObject("StartButton");
            startBtnObj.transform.SetParent(parent.transform, false);
            RectTransform startBtnRect = startBtnObj.AddComponent<RectTransform>();
            startBtnRect.anchorMin = new Vector2(0.5f, 0f);
            startBtnRect.anchorMax = new Vector2(0.5f, 0f);
            startBtnRect.pivot = new Vector2(0.5f, 0f);
            startBtnRect.anchoredPosition = new Vector2(0, 120);
            startBtnRect.sizeDelta = new Vector2(500, 100);

            Image startBtnImage = startBtnObj.AddComponent<Image>();
            startBtnImage.color = PINK_PRIMARY;

            startButton = startBtnObj.AddComponent<Button>();
            ColorBlock startColors = startButton.colors;
            startColors.normalColor = Color.white;
            startColors.highlightedColor = new Color(1f, 0.85f, 0.88f, 1f);
            startColors.pressedColor = new Color(1f, 0.65f, 0.70f, 1f);
            startButton.colors = startColors;
            startButton.onClick.AddListener(OnStartClicked);

            // 시작 버튼 텍스트
            GameObject startTextObj = new GameObject("Text");
            startTextObj.transform.SetParent(startBtnObj.transform, false);
            RectTransform startTextRect = startTextObj.AddComponent<RectTransform>();
            startTextRect.anchorMin = Vector2.zero;
            startTextRect.anchorMax = Vector2.one;
            startTextRect.sizeDelta = Vector2.zero;

            Text startText = startTextObj.AddComponent<Text>();
            startText.text = "시작";
            startText.font = font;
            startText.fontSize = 32;
            startText.fontStyle = FontStyle.Bold;
            startText.color = WHITE;
            startText.alignment = TextAnchor.MiddleCenter;
        }

        private void CreateLoadingOverlay(GameObject parent, Font font)
        {
            loadingOverlay = new GameObject("LoadingOverlay");
            loadingOverlay.transform.SetParent(parent.transform, false);
            RectTransform loadingRect = loadingOverlay.AddComponent<RectTransform>();
            loadingRect.anchorMin = Vector2.zero;
            loadingRect.anchorMax = Vector2.one;
            loadingRect.sizeDelta = Vector2.zero;

            Image loadingBg = loadingOverlay.AddComponent<Image>();
            loadingBg.color = new Color(0f, 0f, 0f, 0.7f);

            GameObject textObj = new GameObject("LoadingText");
            textObj.transform.SetParent(loadingOverlay.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = new Vector2(400, 100);

            Text loadingText = textObj.AddComponent<Text>();
            loadingText.text = "로그인 중...";
            loadingText.font = font;
            loadingText.fontSize = 28;
            loadingText.color = WHITE;
            loadingText.alignment = TextAnchor.MiddleCenter;

            loadingOverlay.SetActive(false);
        }
        #endregion

        #region Login Logic
        private void CheckAutoLogin()
        {
            string token = PlayerPrefs.GetString("JWT_TOKEN", "");
            if (!string.IsNullOrEmpty(token))
            {
                Debug.Log("[LoginUI] Auto-login with saved token");
                // TODO: Validate token with backend
                // OnLoginSuccess();
            }
        }

        private void OnGoogleLoginClicked()
        {
            Debug.Log("[LoginUI] Google login clicked");
            ShowLoading(true);

            // TODO: Implement Google OAuth
            StartCoroutine(MockLogin("Google"));
        }

        private void OnAppleLoginClicked()
        {
            Debug.Log("[LoginUI] Apple login clicked");
            ShowLoading(true);

            // TODO: Implement Apple Sign In
            StartCoroutine(MockLogin("Apple"));
        }

        private void OnGuestLoginClicked()
        {
            Debug.Log("[LoginUI] Guest login clicked");
            ShowLoading(true);

            // TODO: Implement guest login
            StartCoroutine(MockLogin("Guest"));
        }

        private void OnStartClicked()
        {
            Debug.Log("[LoginUI] Start button clicked");
            // Same as Google login for now
            OnGoogleLoginClicked();
        }

        private IEnumerator MockLogin(string provider)
        {
            yield return new WaitForSeconds(1.5f);
            
            ShowLoading(false);
            OnLoginSuccess();
        }

        private void OnLoginSuccess()
        {
            Debug.Log("[LoginUI] Login success - Loading main scene");
            SceneManager.LoadScene("MainGame");
        }

        private void ShowLoading(bool show)
        {
            if (loadingOverlay != null)
            {
                loadingOverlay.SetActive(show);
            }
        }

        private void ShowError(string message)
        {
            Debug.LogError($"[LoginUI] Error: {message}");
            ShowLoading(false);
            // TODO: Show error popup
        }
        #endregion
    }
}
