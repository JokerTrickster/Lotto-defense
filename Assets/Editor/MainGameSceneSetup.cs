using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using LottoDefense.Controllers;
using LottoDefense.UI;

/// <summary>
/// Builds MainGame scene with 협동타워디팬스-style main menu:
/// Title, Solo / Co-op / Boss Rush buttons, Logout.
/// </summary>
public class MainGameSceneSetup : EditorWindow
{
    [MenuItem("Lotto Defense/Setup MainGame Scene")]
    public static void SetupMainGameScene()
    {
        var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Camera: 2D portrait
        var mainCamera = GameObject.Find("Main Camera");
        if (mainCamera != null)
        {
            var cam = mainCamera.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5;
            cam.backgroundColor = MainMenuDesignTokens.BackgroundDark;
        }

        // Canvas
        GameObject canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(MainMenuDesignTokens.RefWidth, MainMenuDesignTokens.RefHeight);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // EventSystem
        if (GameObject.Find("EventSystem") == null)
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // ---- Title ----
        var titleObj = CreateText("GameTitle", "LOTTO DEFENSE", font, MainMenuDesignTokens.TitleFontSize, MainMenuDesignTokens.TextPrimary);
        titleObj.transform.SetParent(canvasObj.transform, false);
        var titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.82f);
        titleRect.anchorMax = new Vector2(0.5f, 0.82f);
        titleRect.sizeDelta = new Vector2(900, 140);
        titleRect.anchoredPosition = Vector2.zero;

        // ---- Mode buttons (Solo, Co-op, Boss Rush) ----
        float buttonY = 0.52f;
        float step = (MainMenuDesignTokens.ButtonHeight + MainMenuDesignTokens.ButtonSpacing) / MainMenuDesignTokens.RefHeight;

        var soloBtn = CreateMenuButton("SoloButton", "솔로 플레이", MainMenuDesignTokens.ButtonPrimary, font);
        soloBtn.transform.SetParent(canvasObj.transform, false);
        SetButtonRect(soloBtn.GetComponent<RectTransform>(), 0.5f, buttonY);
        buttonY -= step;

        var coopBtn = CreateMenuButton("CoopButton", "협동 플레이", MainMenuDesignTokens.ButtonCoop, font);
        coopBtn.transform.SetParent(canvasObj.transform, false);
        SetButtonRect(coopBtn.GetComponent<RectTransform>(), 0.5f, buttonY);
        buttonY -= step;

        var bossBtn = CreateMenuButton("BossRushButton", "보스 러시", MainMenuDesignTokens.ButtonBossRush, font);
        bossBtn.transform.SetParent(canvasObj.transform, false);
        SetButtonRect(bossBtn.GetComponent<RectTransform>(), 0.5f, buttonY);

        // ---- Logout ----
        var logoutBtn = CreateMenuButton("LogoutButton", "로그아웃", MainMenuDesignTokens.ButtonLogout, font);
        logoutBtn.transform.SetParent(canvasObj.transform, false);
        SetButtonRect(logoutBtn.GetComponent<RectTransform>(), 0.5f, 0.18f);

        // ---- SceneNavigator + wiring ----
        var navigatorObj = new GameObject("SceneNavigator");
        var navigator = navigatorObj.AddComponent<SceneNavigator>();

        UnityEditor.Events.UnityEventTools.AddPersistentListener(soloBtn.GetComponent<Button>().onClick, navigator.LoadGameScene);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(coopBtn.GetComponent<Button>().onClick, navigator.ShowComingSoonCoop);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(bossBtn.GetComponent<Button>().onClick, navigator.ShowComingSoonBossRush);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(logoutBtn.GetComponent<Button>().onClick, navigator.LoadLoginScene);

        var gameManagerObj = new GameObject("GameManager");
        gameManagerObj.AddComponent<MainGameManager>();

        EditorSceneManager.SaveScene(newScene, "Assets/Scenes/MainGame.unity");
        Debug.Log("MainGame scene (협동타워디팬스-style) created at Assets/Scenes/MainGame.unity");
        EditorUtility.DisplayDialog("Success", "MainGame 메인 메뉴가 생성되었습니다.\n\n• 솔로 플레이 → GameScene\n• 협동 플레이 / 보스 러시 → 준비중\n• 로그아웃 → LoginScene", "OK");
    }

    static GameObject CreateText(string goName, string text, Font font, int fontSize, Color color)
    {
        var go = new GameObject(goName);
        go.AddComponent<RectTransform>();
        var t = go.AddComponent<Text>();
        t.text = text;
        t.font = font;
        t.fontSize = fontSize;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = color;
        go.AddComponent<CanvasRenderer>();
        return go;
    }

    static GameObject CreateMenuButton(string goName, string label, Color color, Font font)
    {
        var go = new GameObject(goName);
        var img = go.AddComponent<Image>();
        img.color = color;
        go.AddComponent<Button>();

        var textObj = new GameObject("Text");
        textObj.transform.SetParent(go.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        var t = textObj.AddComponent<Text>();
        t.text = label;
        t.font = font;
        t.fontSize = MainMenuDesignTokens.ButtonFontSize;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = MainMenuDesignTokens.TextPrimary;
        textObj.AddComponent<CanvasRenderer>();

        return go;
    }

    static void SetButtonRect(RectTransform rect, float anchorX, float anchorY)
    {
        rect.anchorMin = new Vector2(anchorX, anchorY);
        rect.anchorMax = new Vector2(anchorX, anchorY);
        rect.sizeDelta = new Vector2(MainMenuDesignTokens.ButtonWidth, MainMenuDesignTokens.ButtonHeight);
        rect.anchoredPosition = Vector2.zero;
    }
}
