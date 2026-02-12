using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LottoDefense.Authentication;

namespace LottoDefense.Controllers
{
    /// <summary>
    /// Dynamically creates the login UI for LoginScene.
    /// Auto-instantiated via [RuntimeInitializeOnLoadMethod] — no scene setup needed.
    /// </summary>
    public class LoginSceneBootstrapper : MonoBehaviour
    {
        private Canvas mainCanvas;
        private Font defaultFont;
        private Button loginButton;
        private Button guestButton;
        private Text statusText;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneCallback()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "LoginScene")
            {
                // Avoid duplicates
                if (Object.FindFirstObjectByType<LoginSceneBootstrapper>() != null)
                    return;

                GameObject obj = new GameObject("LoginSceneBootstrapper");
                obj.AddComponent<LoginSceneBootstrapper>();
                Debug.Log("[LoginSceneBootstrapper] Auto-created in LoginScene");
            }
        }

        private void Awake()
        {
            defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null)
                defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            if (defaultFont == null)
                Debug.LogError("[LoginSceneBootstrapper] Failed to load default font!");

            DestroyBrokenCanvas();
            SetupCanvas();
            CreateLoginUI();

            AuthenticationManager.Instance.OnAuthenticationComplete += OnAuthComplete;
            Debug.Log("[LoginSceneBootstrapper] Login UI initialized");
        }

        private void OnDestroy()
        {
            // Use FindFirstObjectByType instead of Instance getter to avoid
            // auto-creating a new singleton during cleanup/scene unload
            var auth = FindFirstObjectByType<AuthenticationManager>();
            if (auth != null)
                auth.OnAuthenticationComplete -= OnAuthComplete;
        }

        private void DestroyBrokenCanvas()
        {
            // Destroy any existing Canvas in the scene (the scene has a broken one with scale 0,0,0)
            Canvas[] existingCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas c in existingCanvases)
            {
                Debug.Log($"[LoginSceneBootstrapper] Destroying existing Canvas: {c.gameObject.name}");
                Destroy(c.gameObject);
            }
        }

        private void SetupCanvas()
        {
            GameObject canvasObj = new GameObject("LoginCanvas");
            mainCanvas = canvasObj.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mainCanvas.sortingOrder = 0;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();
        }

        private void CreateLoginUI()
        {
            CreateBackground();
            CreateTitle();
            CreateLoginButton();
            CreateGuestButton();
            CreateStatusText();
        }

        private void CreateBackground()
        {
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(mainCanvas.transform, false);
            Image bg = bgObj.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.08f, 0.16f, 1f);

            RectTransform rect = bgObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
        }

        private void CreateTitle()
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(mainCanvas.transform, false);
            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = "LOTTO DEFENSE";
            titleText.font = defaultFont;
            titleText.fontSize = 72;
            titleText.color = Color.white;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.fontStyle = FontStyle.Bold;

            RectTransform rect = titleObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.05f, 0.6f);
            rect.anchorMax = new Vector2(0.95f, 0.75f);
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
        }

        private void CreateLoginButton()
        {
            GameObject btnObj = CreateButton(
                "LoginButton",
                "Google 로그인",
                new Color(0.26f, 0.52f, 0.96f, 1f),
                new Vector2(0.15f, 0.38f),
                new Vector2(0.85f, 0.44f),
                42
            );
            loginButton = btnObj.GetComponent<Button>();
            loginButton.onClick.AddListener(OnLoginClicked);
        }

        private void CreateGuestButton()
        {
            GameObject btnObj = CreateButton(
                "GuestButton",
                "게스트 로그인",
                new Color(0.35f, 0.35f, 0.45f, 1f),
                new Vector2(0.15f, 0.28f),
                new Vector2(0.85f, 0.34f),
                36
            );
            guestButton = btnObj.GetComponent<Button>();
            guestButton.onClick.AddListener(OnGuestLoginClicked);
        }

        private void CreateStatusText()
        {
            GameObject textObj = new GameObject("StatusText");
            textObj.transform.SetParent(mainCanvas.transform, false);
            statusText = textObj.AddComponent<Text>();
            statusText.text = "";
            statusText.font = defaultFont;
            statusText.fontSize = 28;
            statusText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            statusText.alignment = TextAnchor.MiddleCenter;

            RectTransform rect = textObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.1f, 0.2f);
            rect.anchorMax = new Vector2(0.9f, 0.25f);
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
        }

        private GameObject CreateButton(string name, string label, Color bgColor,
            Vector2 anchorMin, Vector2 anchorMax, int fontSize)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(mainCanvas.transform, false);

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = bgColor;

            btnObj.AddComponent<Button>();

            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = anchorMin;
            btnRect.anchorMax = anchorMax;
            btnRect.sizeDelta = Vector2.zero;
            btnRect.anchoredPosition = Vector2.zero;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            Text text = textObj.AddComponent<Text>();
            text.text = label;
            text.font = defaultFont;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            return btnObj;
        }

        private void OnLoginClicked()
        {
            Debug.Log("[LoginSceneBootstrapper] Google login clicked");
            SetButtonsInteractable(false);
            statusText.text = "로그인 중...";
            AuthenticationManager.Instance.AuthenticateWithGoogle();
        }

        private void OnGuestLoginClicked()
        {
            Debug.Log("[LoginSceneBootstrapper] Guest login clicked");
            SetButtonsInteractable(false);
            statusText.text = "게스트 로그인 중...";
            string guestId = "guest_" + Random.Range(1000, 9999);
            AuthenticationManager.Instance.LoginAsGuest(guestId);
        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (loginButton != null) loginButton.interactable = interactable;
            if (guestButton != null) guestButton.interactable = interactable;
        }

        private void OnAuthComplete(bool success)
        {
            if (success)
            {
                Debug.Log("[LoginSceneBootstrapper] Auth success, loading MainGame");
                statusText.text = "게임 로딩 중...";
                SceneManager.LoadScene("MainGame");
            }
            else
            {
                Debug.Log("[LoginSceneBootstrapper] Auth failed");
                statusText.text = "로그인 실패. 다시 시도해주세요.";
                SetButtonsInteractable(true);
            }
        }
    }
}
