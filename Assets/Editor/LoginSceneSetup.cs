using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using LottoDefense.UI;
using LottoDefense.Controllers;

public class LoginSceneSetup : EditorWindow
{
    [MenuItem("Lotto Defense/Setup Login Scene")]
    public static void SetupLoginScene()
    {
        // Create new scene
        var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Setup Main Camera for 2D UI
        var mainCamera = GameObject.Find("Main Camera");
        if (mainCamera != null)
        {
            Camera cam = mainCamera.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 1f);
            cam.orthographic = true;
            cam.orthographicSize = 5;
        }

        // Create Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Create EventSystem if not exists
        if (GameObject.Find("EventSystem") == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // Create Background Panel
        GameObject bgPanel = new GameObject("Background");
        bgPanel.transform.SetParent(canvasObj.transform, false);
        Image bgImage = bgPanel.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.15f, 1f);
        RectTransform bgRect = bgPanel.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // Create Logo/Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(canvasObj.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "LOTTO DEFENSE";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 72;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.7f);
        titleRect.anchorMax = new Vector2(0.5f, 0.7f);
        titleRect.sizeDelta = new Vector2(800, 150);
        titleRect.anchoredPosition = Vector2.zero;

        // Create Google Login Button
        GameObject loginButtonObj = new GameObject("GoogleLoginButton");
        loginButtonObj.transform.SetParent(canvasObj.transform, false);
        Image loginButtonImage = loginButtonObj.AddComponent<Image>();
        loginButtonImage.color = Color.white;
        Button loginButton = loginButtonObj.AddComponent<Button>();
        GoogleLoginButton googleLogin = loginButtonObj.AddComponent<GoogleLoginButton>();

        RectTransform loginButtonRect = loginButtonObj.GetComponent<RectTransform>();
        loginButtonRect.anchorMin = new Vector2(0.5f, 0.4f);
        loginButtonRect.anchorMax = new Vector2(0.5f, 0.4f);
        loginButtonRect.sizeDelta = new Vector2(600, 120);
        loginButtonRect.anchoredPosition = Vector2.zero;

        // Add button text
        GameObject buttonTextObj = new GameObject("Text");
        buttonTextObj.transform.SetParent(loginButtonObj.transform, false);
        Text buttonText = buttonTextObj.AddComponent<Text>();
        buttonText.text = "Sign in with Google";
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 36;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = Color.black;
        RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.sizeDelta = Vector2.zero;

        // Create Loading Overlay
        GameObject loadingOverlayObj = new GameObject("LoadingOverlay");
        loadingOverlayObj.transform.SetParent(canvasObj.transform, false);
        Image overlayImage = loadingOverlayObj.AddComponent<Image>();
        overlayImage.color = new Color(0, 0, 0, 0.8f);
        CanvasGroup overlayCanvasGroup = loadingOverlayObj.AddComponent<CanvasGroup>();
        LoadingOverlay loadingOverlay = loadingOverlayObj.AddComponent<LoadingOverlay>();

        RectTransform overlayRect = loadingOverlayObj.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.sizeDelta = Vector2.zero;

        // Add loading spinner
        GameObject spinnerObj = new GameObject("Spinner");
        spinnerObj.transform.SetParent(loadingOverlayObj.transform, false);
        Image spinnerImage = spinnerObj.AddComponent<Image>();
        spinnerImage.color = Color.white;
        RectTransform spinnerRect = spinnerObj.GetComponent<RectTransform>();
        spinnerRect.sizeDelta = new Vector2(100, 100);
        spinnerRect.anchoredPosition = new Vector2(0, 50);

        // Add loading text
        GameObject loadingTextObj = new GameObject("LoadingText");
        loadingTextObj.transform.SetParent(loadingOverlayObj.transform, false);
        Text loadingText = loadingTextObj.AddComponent<Text>();
        loadingText.text = "Loading...";
        loadingText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        loadingText.fontSize = 48;
        loadingText.alignment = TextAnchor.MiddleCenter;
        loadingText.color = Color.white;
        RectTransform loadingTextRect = loadingTextObj.GetComponent<RectTransform>();
        loadingTextRect.sizeDelta = new Vector2(600, 100);
        loadingTextRect.anchoredPosition = new Vector2(0, -50);

        // Set references in LoadingOverlay
        SerializedObject loadingOverlaySO = new SerializedObject(loadingOverlay);
        loadingOverlaySO.FindProperty("canvasGroup").objectReferenceValue = overlayCanvasGroup;
        loadingOverlaySO.FindProperty("loadingSpinner").objectReferenceValue = spinnerImage;
        loadingOverlaySO.FindProperty("loadingText").objectReferenceValue = loadingText;
        loadingOverlaySO.ApplyModifiedProperties();

        // Create LoginSceneController
        GameObject controllerObj = new GameObject("LoginSceneController");
        LoginSceneController controller = controllerObj.AddComponent<LoginSceneController>();

        // Set references in controller
        SerializedObject controllerSO = new SerializedObject(controller);
        controllerSO.FindProperty("loginButton").objectReferenceValue = googleLogin;
        controllerSO.FindProperty("loadingOverlay").objectReferenceValue = loadingOverlay;
        controllerSO.ApplyModifiedProperties();

        // Create Legal Links Panel
        GameObject legalPanel = new GameObject("LegalLinksPanel");
        legalPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform legalRect = legalPanel.AddComponent<RectTransform>();
        legalRect.anchorMin = new Vector2(0.5f, 0.1f);
        legalRect.anchorMax = new Vector2(0.5f, 0.1f);
        legalRect.sizeDelta = new Vector2(800, 100);
        legalRect.anchoredPosition = Vector2.zero;

        LegalLinksHandler legalHandler = legalPanel.AddComponent<LegalLinksHandler>();

        // Privacy Policy Button
        GameObject privacyButtonObj = new GameObject("PrivacyPolicyButton");
        privacyButtonObj.transform.SetParent(legalPanel.transform, false);
        RectTransform privacyRect = privacyButtonObj.AddComponent<RectTransform>();
        Button privacyButton = privacyButtonObj.AddComponent<Button>();
        privacyButton.onClick.AddListener(() => legalHandler.OpenPrivacyPolicy());

        GameObject privacyTextObj = new GameObject("Text");
        privacyTextObj.transform.SetParent(privacyButtonObj.transform, false);
        Text privacyText = privacyTextObj.AddComponent<Text>();
        privacyText.text = "Privacy Policy";
        privacyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        privacyText.fontSize = 28;
        privacyText.alignment = TextAnchor.MiddleCenter;
        privacyText.color = new Color(0.6f, 0.6f, 1f);
        privacyRect.anchorMin = new Vector2(0, 0.5f);
        privacyRect.anchorMax = new Vector2(0, 0.5f);
        privacyRect.sizeDelta = new Vector2(350, 80);
        privacyRect.anchoredPosition = new Vector2(175, 0);

        RectTransform privacyTextRect = privacyTextObj.GetComponent<RectTransform>();
        privacyTextRect.anchorMin = Vector2.zero;
        privacyTextRect.anchorMax = Vector2.one;
        privacyTextRect.sizeDelta = Vector2.zero;

        // Terms of Service Button
        GameObject termsButtonObj = new GameObject("TermsButton");
        termsButtonObj.transform.SetParent(legalPanel.transform, false);
        RectTransform termsRect = termsButtonObj.AddComponent<RectTransform>();
        Button termsButton = termsButtonObj.AddComponent<Button>();
        termsButton.onClick.AddListener(() => legalHandler.OpenTermsOfService());

        GameObject termsTextObj = new GameObject("Text");
        termsTextObj.transform.SetParent(termsButtonObj.transform, false);
        Text termsText = termsTextObj.AddComponent<Text>();
        termsText.text = "Terms of Service";
        termsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        termsText.fontSize = 28;
        termsText.alignment = TextAnchor.MiddleCenter;
        termsText.color = new Color(0.6f, 0.6f, 1f);
        termsRect.anchorMin = new Vector2(1, 0.5f);
        termsRect.anchorMax = new Vector2(1, 0.5f);
        termsRect.sizeDelta = new Vector2(350, 80);
        termsRect.anchoredPosition = new Vector2(-175, 0);

        RectTransform termsTextRect = termsTextObj.GetComponent<RectTransform>();
        termsTextRect.anchorMin = Vector2.zero;
        termsTextRect.anchorMax = Vector2.one;
        termsTextRect.sizeDelta = Vector2.zero;

        // Save scene
        EditorSceneManager.SaveScene(newScene, "Assets/Scenes/LoginScene.unity");

        Debug.Log("Login scene created successfully at Assets/Scenes/LoginScene.unity");
        EditorUtility.DisplayDialog("Success", "Login scene has been created!\n\nNext steps:\n1. Create a LoginConfig asset (Right-click in Project > Create > Lotto Defense > Login Configuration)\n2. Assign it to LoginSceneController\n3. Create a MainGame scene", "OK");
    }
}
