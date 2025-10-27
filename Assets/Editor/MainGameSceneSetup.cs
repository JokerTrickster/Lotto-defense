using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using LottoDefense.Controllers;
using LottoDefense.UI;

public class MainGameSceneSetup : EditorWindow
{
    [MenuItem("Lotto Defense/Setup MainGame Scene")]
    public static void SetupMainGameScene()
    {
        // Create new scene
        var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Setup Main Camera for 2D
        var mainCamera = GameObject.Find("Main Camera");
        if (mainCamera != null)
        {
            Camera cam = mainCamera.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5;
            cam.backgroundColor = new Color(0.1f, 0.15f, 0.2f, 1f);
        }

        // Create Canvas for UI
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Create EventSystem
        if (GameObject.Find("EventSystem") == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // Create Game Title Text
        GameObject titleObj = new GameObject("GameTitle");
        titleObj.transform.SetParent(canvasObj.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "LOTTO DEFENSE";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 80;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.75f);
        titleRect.anchorMax = new Vector2(0.5f, 0.75f);
        titleRect.sizeDelta = new Vector2(900, 150);
        titleRect.anchoredPosition = Vector2.zero;

        // Create Start Game Button
        GameObject startButtonObj = new GameObject("StartGameButton");
        startButtonObj.transform.SetParent(canvasObj.transform, false);
        Image startButtonImage = startButtonObj.AddComponent<Image>();
        startButtonImage.color = new Color(0.2f, 0.7f, 0.3f, 1f);
        Button startButton = startButtonObj.AddComponent<Button>();

        RectTransform startButtonRect = startButtonObj.GetComponent<RectTransform>();
        startButtonRect.anchorMin = new Vector2(0.5f, 0.5f);
        startButtonRect.anchorMax = new Vector2(0.5f, 0.5f);
        startButtonRect.sizeDelta = new Vector2(500, 120);
        startButtonRect.anchoredPosition = Vector2.zero;

        // Add start button text
        GameObject startTextObj = new GameObject("Text");
        startTextObj.transform.SetParent(startButtonObj.transform, false);
        Text startText = startTextObj.AddComponent<Text>();
        startText.text = "게임 시작";
        startText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        startText.fontSize = 48;
        startText.alignment = TextAnchor.MiddleCenter;
        startText.color = Color.white;
        RectTransform startTextRect = startTextObj.GetComponent<RectTransform>();
        startTextRect.anchorMin = Vector2.zero;
        startTextRect.anchorMax = Vector2.one;
        startTextRect.sizeDelta = Vector2.zero;

        // Scene Navigator will be added after buttons
        // Button events will be connected programmatically

        // Create Logout Button
        GameObject logoutButtonObj = new GameObject("LogoutButton");
        logoutButtonObj.transform.SetParent(canvasObj.transform, false);
        Image logoutButtonImage = logoutButtonObj.AddComponent<Image>();
        logoutButtonImage.color = new Color(0.7f, 0.3f, 0.3f, 1f);
        Button logoutButton = logoutButtonObj.AddComponent<Button>();

        RectTransform logoutButtonRect = logoutButtonObj.GetComponent<RectTransform>();
        logoutButtonRect.anchorMin = new Vector2(0.5f, 0.3f);
        logoutButtonRect.anchorMax = new Vector2(0.5f, 0.3f);
        logoutButtonRect.sizeDelta = new Vector2(500, 120);
        logoutButtonRect.anchoredPosition = Vector2.zero;

        // Add logout button text
        GameObject logoutTextObj = new GameObject("Text");
        logoutTextObj.transform.SetParent(logoutButtonObj.transform, false);
        Text logoutText = logoutTextObj.AddComponent<Text>();
        logoutText.text = "로그아웃";
        logoutText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        logoutText.fontSize = 48;
        logoutText.alignment = TextAnchor.MiddleCenter;
        logoutText.color = Color.white;
        RectTransform logoutTextRect = logoutTextObj.GetComponent<RectTransform>();
        logoutTextRect.anchorMin = Vector2.zero;
        logoutTextRect.anchorMax = Vector2.one;
        logoutTextRect.sizeDelta = Vector2.zero;

        // Create SceneNavigator
        GameObject navigatorObj = new GameObject("SceneNavigator");
        SceneNavigator navigator = navigatorObj.AddComponent<SceneNavigator>();

        // Connect button events using persistent listener (will be saved)
        UnityEditor.Events.UnityEventTools.AddPersistentListener(startButton.onClick, navigator.LoadGameScene);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(logoutButton.onClick, navigator.LoadLoginScene);

        // Create GameManager placeholder
        GameObject gameManagerObj = new GameObject("GameManager");
        gameManagerObj.AddComponent<MainGameManager>();

        // Save scene
        EditorSceneManager.SaveScene(newScene, "Assets/Scenes/MainGame.unity");

        Debug.Log("MainGame scene created successfully at Assets/Scenes/MainGame.unity");
        EditorUtility.DisplayDialog("Success", "MainGame (Menu) scene has been created!\n\nButtons:\n- 게임 시작: Go to GameScene\n- 로그아웃: Back to LoginScene\n\nNext: Create GameScene for actual gameplay", "OK");
    }
}
