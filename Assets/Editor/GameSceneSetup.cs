using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using LottoDefense.UI;

public class GameSceneSetup : EditorWindow
{
    [MenuItem("Lotto Defense/Setup GameScene (Actual Gameplay)")]
    public static void SetupGameScene()
    {
        // Create new scene
        var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Setup Main Camera for 2D
        var mainCamera = GameObject.Find("Main Camera");
        if (mainCamera != null)
        {
            Camera cam = mainCamera.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.1f, 1f);
            cam.orthographic = true;
            cam.orthographicSize = 5;
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

        // Create Game UI Title
        GameObject titleObj = new GameObject("GameTitle");
        titleObj.transform.SetParent(canvasObj.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "GAME SCREEN";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 60;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.9f);
        titleRect.anchorMax = new Vector2(0.5f, 0.9f);
        titleRect.sizeDelta = new Vector2(800, 100);
        titleRect.anchoredPosition = Vector2.zero;

        // Create gameplay area placeholder
        GameObject gameAreaObj = new GameObject("GameplayArea");
        gameAreaObj.transform.SetParent(canvasObj.transform, false);
        Text gameAreaText = gameAreaObj.AddComponent<Text>();
        gameAreaText.text = "Your game content goes here\n\n- Player\n- Enemies\n- Towers\n- etc...";
        gameAreaText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        gameAreaText.fontSize = 40;
        gameAreaText.alignment = TextAnchor.MiddleCenter;
        gameAreaText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        RectTransform gameAreaRect = gameAreaObj.GetComponent<RectTransform>();
        gameAreaRect.anchorMin = new Vector2(0.5f, 0.5f);
        gameAreaRect.anchorMax = new Vector2(0.5f, 0.5f);
        gameAreaRect.sizeDelta = new Vector2(800, 400);
        gameAreaRect.anchoredPosition = Vector2.zero;

        // Create Back to Menu Button
        GameObject backButtonObj = new GameObject("BackToMenuButton");
        backButtonObj.transform.SetParent(canvasObj.transform, false);
        Image backButtonImage = backButtonObj.AddComponent<Image>();
        backButtonImage.color = new Color(0.3f, 0.3f, 0.7f, 1f);
        Button backButton = backButtonObj.AddComponent<Button>();

        RectTransform backButtonRect = backButtonObj.GetComponent<RectTransform>();
        backButtonRect.anchorMin = new Vector2(0.5f, 0.1f);
        backButtonRect.anchorMax = new Vector2(0.5f, 0.1f);
        backButtonRect.sizeDelta = new Vector2(500, 100);
        backButtonRect.anchoredPosition = Vector2.zero;

        // Add back button text
        GameObject backTextObj = new GameObject("Text");
        backTextObj.transform.SetParent(backButtonObj.transform, false);
        Text backText = backTextObj.AddComponent<Text>();
        backText.text = "메인 메뉴로";
        backText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        backText.fontSize = 40;
        backText.alignment = TextAnchor.MiddleCenter;
        backText.color = Color.white;
        RectTransform backTextRect = backTextObj.GetComponent<RectTransform>();
        backTextRect.anchorMin = Vector2.zero;
        backTextRect.anchorMax = Vector2.one;
        backTextRect.sizeDelta = Vector2.zero;

        // Create SceneNavigator
        GameObject navigatorObj = new GameObject("SceneNavigator");
        SceneNavigator navigator = navigatorObj.AddComponent<SceneNavigator>();

        // Add back to menu functionality using persistent listener
        UnityEditor.Events.UnityEventTools.AddPersistentListener(backButton.onClick, navigator.LoadMainGame);

        // Create GameplayManager placeholder
        GameObject gameplayManagerObj = new GameObject("GameplayManager");
        // Add your game manager script here when ready

        // Save scene
        EditorSceneManager.SaveScene(newScene, "Assets/Scenes/GameScene.unity");

        Debug.Log("GameScene created successfully at Assets/Scenes/GameScene.unity");
        EditorUtility.DisplayDialog("Success", "GameScene has been created!\n\nThis is where your actual gameplay will happen.\n\nButton:\n- 메인 메뉴로: Back to MainGame", "OK");
    }
}
