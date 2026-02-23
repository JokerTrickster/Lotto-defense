using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace LottoDefense.UI
{
    [DefaultExecutionOrder(-100)]
    public class AutoCreateMainMenu : MonoBehaviour
    {
        private void Awake()
        {
            if (SceneManager.GetActiveScene().name != "MainGame")
            {
                Destroy(gameObject);
                return;
            }

            CleanupOldUI();
            CreateMainMenuButtons();
            Destroy(gameObject);
        }
        
        private void CleanupOldUI()
        {
            string[] buttonsToRemove = new string[]
            {
                "SinglePlayButton", "CoopPlayButton",
                "RankingButton", "MyStatsButton",
                "StartGameButton", "게임 시작",
                "MainMenuContainer"
            };
            
            foreach (string btnName in buttonsToRemove)
            {
                GameObject btnObj = GameObject.Find(btnName);
                if (btnObj != null)
                    Destroy(btnObj);
            }
        }

        private void CreateMainMenuButtons()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
                
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            else
            {
                CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1080, 1920);
                    scaler.matchWidthOrHeight = 0.5f;
                }
            }

            SceneNavigator navigator = FindFirstObjectByType<SceneNavigator>();
            if (navigator == null)
            {
                GameObject navObj = new GameObject("SceneNavigator");
                navigator = navObj.AddComponent<SceneNavigator>();
                DontDestroyOnLoad(navObj);
            }

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null)
                defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // Vertical container centered on screen
            GameObject container = new GameObject("MainMenuContainer");
            container.transform.SetParent(canvas.transform, false);

            RectTransform containerRect = container.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.pivot = new Vector2(0.5f, 0.5f);
            containerRect.anchoredPosition = new Vector2(0, -60);
            containerRect.sizeDelta = new Vector2(720, 500);

            VerticalLayoutGroup vlg = container.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 24;
            vlg.padding = new RectOffset(0, 0, 0, 0);
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childAlignment = TextAnchor.MiddleCenter;

            // Row 1: Single + Coop side by side
            GameObject topRow = new GameObject("TopRow");
            topRow.transform.SetParent(container.transform, false);
            LayoutElement topRowLE = topRow.AddComponent<LayoutElement>();
            topRowLE.preferredHeight = 140;

            HorizontalLayoutGroup hlg = topRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 24;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            SceneNavigator nav = navigator;

            Button singleButton = CreateMenuButton(topRow.transform, "SinglePlayButton",
                "싱글 플레이", new Color(0.5f, 0.72f, 0.95f), defaultFont, 42);
            singleButton.onClick.AddListener(() => nav.LoadGameScene());

            Button coopButton = CreateMenuButton(topRow.transform, "CoopPlayButton",
                "협동 플레이", new Color(0.95f, 0.65f, 0.4f), defaultFont, 42);
            coopButton.onClick.AddListener(() => nav.ShowMultiplayerLobby());

            // Row 2: Ranking (full width, slightly smaller height)
            Button rankingButton = CreateMenuButton(container.transform, "RankingButton",
                "랭킹", new Color(0.45f, 0.8f, 0.55f), defaultFont, 40);
            LayoutElement rankLE = rankingButton.gameObject.AddComponent<LayoutElement>();
            rankLE.preferredHeight = 110;
            rankingButton.onClick.AddListener(() => nav.ShowRankings());
        }

        private Button CreateMenuButton(Transform parent, string name, string text, Color color, Font font, int fontSize)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            btnObj.AddComponent<RectTransform>();

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = color;
            Sprite rounded = CuteUIHelper.GetRoundedRectSprite(20);
            if (rounded != null)
            {
                btnImage.sprite = rounded;
                btnImage.type = Image.Type.Sliced;
            }

            Shadow btnShadow = btnObj.AddComponent<Shadow>();
            btnShadow.effectColor = CuteUIHelper.SoftShadow;
            btnShadow.effectDistance = new Vector2(2, -3);

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.98f, 0.95f, 1f);
            colors.pressedColor = new Color(0.85f, 0.82f, 0.78f, 1f);
            colors.fadeDuration = 0.08f;
            btn.colors = colors;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(12, 8);
            textRect.offsetMax = new Vector2(-12, -8);

            Text btnText = textObj.AddComponent<Text>();
            btnText.text = text;
            btnText.font = font;
            btnText.fontSize = fontSize;
            btnText.color = CuteUIHelper.DarkText;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.fontStyle = FontStyle.Bold;
            btnText.resizeTextForBestFit = true;
            btnText.resizeTextMinSize = 24;
            btnText.resizeTextMaxSize = fontSize;

            return btn;
        }
    }
}
