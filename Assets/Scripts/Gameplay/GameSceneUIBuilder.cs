using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using LottoDefense.UI;

namespace LottoDefense.Gameplay
{
    /// <summary>
    /// Builds all UI elements for GameScene including HUD, buttons, countdown, and result screens.
    /// Extracted from GameSceneBootstrapper to reduce file size and improve maintainability.
    /// </summary>
    public class GameSceneUIBuilder
    {
        private Canvas mainCanvas;
        private Font defaultFont;
        private GameBalanceConfig balanceConfig;

        public GameSceneUIBuilder(Font font, GameBalanceConfig config)
        {
            defaultFont = font;
            balanceConfig = config;
        }

        #region Canvas
        public Canvas EnsureMainCanvas()
        {
            Canvas existingCanvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if (existingCanvas != null && existingCanvas.gameObject.name != "GameCanvas")
            {
                UnityEngine.Object.Destroy(existingCanvas.gameObject);
                mainCanvas = null;
            }

            if (mainCanvas == null)
            {
                GameObject canvasObj = new GameObject("GameCanvas");
                mainCanvas = canvasObj.AddComponent<Canvas>();
                mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                mainCanvas.sortingOrder = 100; // Above game objects

                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920); // Mobile portrait

                canvasObj.AddComponent<GraphicRaycaster>();
            }

            return mainCanvas;
        }
        #endregion

        #region Countdown UI
        public void EnsureCountdownUI()
        {
            CountdownUI existing = UnityEngine.Object.FindFirstObjectByType<CountdownUI>();
            if (existing != null)
            {
                Debug.Log("[GameSceneUIBuilder] CountdownUI already exists");
                return;
            }

            GameObject countdownObj = new GameObject("CountdownUI");
            countdownObj.transform.SetParent(mainCanvas.transform, false);

            CanvasGroup canvasGroup = countdownObj.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            RectTransform rect = countdownObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            GameObject textObj = new GameObject("CountdownText");
            textObj.transform.SetParent(countdownObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = new Vector2(400, 300);

            GameObject shadowObj = new GameObject("Shadow");
            shadowObj.transform.SetParent(textObj.transform, false);
            RectTransform shadowRect = shadowObj.AddComponent<RectTransform>();
            shadowRect.anchorMin = Vector2.zero;
            shadowRect.anchorMax = Vector2.one;
            shadowRect.offsetMin = new Vector2(4, -4);
            shadowRect.offsetMax = new Vector2(4, -4);
            Text shadowText = CreateText(shadowObj, "3", GameSceneDesignTokens.CountdownSize, new Color(0, 0, 0, 0.6f));
            shadowText.fontStyle = FontStyle.Bold;
            shadowText.raycastTarget = false;

            Text countdownText = CreateText(textObj, "3", GameSceneDesignTokens.CountdownSize, GameSceneDesignTokens.CountdownText);
            countdownText.fontStyle = FontStyle.Bold;

            Outline outline = textObj.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.5f);
            outline.effectDistance = new Vector2(3, -3);

            CountdownUI countdown = countdownObj.AddComponent<CountdownUI>();
            SetField(countdown, "countdownText", countdownText);
            SetField(countdown, "canvasGroup", canvasGroup);

            Debug.Log("[GameSceneUIBuilder] CountdownUI created");
        }
        #endregion

        #region Game HUD
        public void EnsureGameHUD()
        {
            GameHUD existing = UnityEngine.Object.FindFirstObjectByType<GameHUD>();
            if (existing != null)
            {
                Debug.Log("[GameSceneUIBuilder] GameHUD already exists");
                return;
            }

            GameObject hudObj = new GameObject("GameHUD");
            hudObj.transform.SetParent(mainCanvas.transform, false);

            RectTransform hudRect = hudObj.AddComponent<RectTransform>();
            hudRect.anchorMin = new Vector2(0.5f, 1f);
            hudRect.anchorMax = new Vector2(0.5f, 1f);
            hudRect.pivot = new Vector2(0.5f, 1f);
            hudRect.anchoredPosition = new Vector2(0, -GameSceneDesignTokens.HudTopMargin);
            hudRect.sizeDelta = new Vector2(GameSceneDesignTokens.HudWidth, GameSceneDesignTokens.HudHeight);

            Image bg = hudObj.AddComponent<Image>();
            bg.color = GameSceneDesignTokens.HudBackground;

            GameObject roundObj = CreateHUDElement(hudObj, "RoundText", new Vector2(-200, -35), new Vector2(150, 50));
            Text roundText = CreateText(roundObj, "R1", GameSceneDesignTokens.HudFontSize, GameSceneDesignTokens.HudText);
            roundText.fontStyle = FontStyle.Bold;

            GameObject stateObj = CreateHUDElement(hudObj, "StateText", new Vector2(0, -35), new Vector2(250, 50));
            Text stateText = CreateText(stateObj, "PREPARATION", GameSceneDesignTokens.HudFontSize, GameSceneDesignTokens.HudText);

            GameObject timerObj = CreateHUDElement(hudObj, "TimerText", new Vector2(200, -35), new Vector2(150, 50));
            Text timerText = CreateText(timerObj, "00:00", GameSceneDesignTokens.HudFontSize, GameSceneDesignTokens.HudText);

            GameObject statsObj = CreateHUDElement(hudObj, "Stats", new Vector2(0, -80), new Vector2(600, 50));
            RectTransform statsRect = statsObj.GetComponent<RectTransform>();

            GameObject lifeObj = CreateStatsElement(statsObj, "Life", new Vector2(-250, 0), "♥10", GameSceneDesignTokens.LifeColor);
            Text lifeText = lifeObj.GetComponent<Text>();

            GameObject goldObj = CreateStatsElement(statsObj, "Gold", new Vector2(-80, 0), "G:30", GameSceneDesignTokens.GoldColor);
            Text goldText = goldObj.GetComponent<Text>();

            GameObject monsterCountObj = CreateStatsElement(statsObj, "MonsterCount", new Vector2(80, 0), "M:0", GameSceneDesignTokens.MonsterCountColor);
            Text monsterCountText = monsterCountObj.GetComponent<Text>();

            GameObject unitCountObj = CreateStatsElement(statsObj, "UnitCount", new Vector2(250, 0), "U:0", GameSceneDesignTokens.UnitCountColor);
            Text unitCountText = unitCountObj.GetComponent<Text>();

            GameHUD hud = hudObj.AddComponent<GameHUD>();
            SetField(hud, "roundText", roundText);
            SetField(hud, "stateText", stateText);
            SetField(hud, "timerText", timerText);
            SetField(hud, "lifeText", lifeText);
            SetField(hud, "goldText", goldText);
            SetField(hud, "monsterCountText", monsterCountText);
            SetField(hud, "unitCountText", unitCountText);

            Debug.Log("[GameSceneUIBuilder] GameHUD created");
        }

        private GameObject CreateHUDElement(GameObject parent, string name, Vector2 position, Vector2 size)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return obj;
        }

        private GameObject CreateStatsElement(GameObject parent, string name, Vector2 position, string text, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(100, 40);

            Text textComp = CreateText(obj, text, GameSceneDesignTokens.HudFontSize, color);
            textComp.fontStyle = FontStyle.Bold;
            return obj;
        }
        #endregion

        #region Buttons
        public void EnsureSummonButton()
        {
            if (UnityEngine.Object.FindFirstObjectByType<SummonButton>() != null)
            {
                Debug.Log("[GameSceneUIBuilder] SummonButton already exists");
                return;
            }

            GameObject btnObj = new GameObject("SummonButton");
            btnObj.transform.SetParent(mainCanvas.transform, false);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0f);
            btnRect.anchorMax = new Vector2(0.5f, 0f);
            btnRect.pivot = new Vector2(0.5f, 0f);
            btnRect.anchoredPosition = new Vector2(0, GameSceneDesignTokens.SummonButtonBottomMargin);
            btnRect.sizeDelta = new Vector2(GameSceneDesignTokens.SummonButtonWidth, GameSceneDesignTokens.SummonButtonHeight);

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = GameSceneDesignTokens.SummonButtonNormal;

            Button button = btnObj.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            button.colors = colors;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            int cost = balanceConfig != null ? balanceConfig.summonCostGold : 5;
            Text btnText = CreateText(textObj, $"소환 ({cost}G)", GameSceneDesignTokens.SummonButtonFontSize, GameSceneDesignTokens.SummonButtonText);
            btnText.fontStyle = FontStyle.Bold;

            SummonButton summonBtn = btnObj.AddComponent<SummonButton>();
            SetField(summonBtn, "summonText", btnText);

            Debug.Log("[GameSceneUIBuilder] SummonButton created");
        }

        public void EnsureBackToMenuButton()
        {
            GameObject btnObj = new GameObject("BackToMenuButton");
            btnObj.transform.SetParent(mainCanvas.transform, false);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0f);
            btnRect.anchorMax = new Vector2(0.5f, 0f);
            btnRect.pivot = new Vector2(0.5f, 0f);
            btnRect.anchoredPosition = new Vector2(0, GameSceneDesignTokens.BackToMenuButtonBottomMargin);
            btnRect.sizeDelta = new Vector2(GameSceneDesignTokens.BackToMenuButtonWidth, GameSceneDesignTokens.BackToMenuButtonHeight);

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = GameSceneDesignTokens.BackToMenuButtonNormal;

            Button button = btnObj.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
            button.colors = colors;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            Text btnText = CreateText(textObj, "메인 메뉴로", GameSceneDesignTokens.BackToMenuButtonFontSize, GameSceneDesignTokens.BackToMenuButtonText);

            SceneNavigator navigator = btnObj.AddComponent<SceneNavigator>();
            button.onClick.AddListener(() => navigator.LoadMainGame());

            Debug.Log("[GameSceneUIBuilder] BackToMenuButton created");
        }
        #endregion

        #region Game Result UI
        public void EnsureGameResultUI()
        {
            if (UnityEngine.Object.FindFirstObjectByType<GameResultUI>() != null)
            {
                Debug.Log("[GameSceneUIBuilder] GameResultUI already exists");
                return;
            }

            GameObject resultObj = new GameObject("GameResultUI");
            resultObj.transform.SetParent(mainCanvas.transform, false);
            resultObj.SetActive(false);

            RectTransform resultRect = resultObj.AddComponent<RectTransform>();
            resultRect.anchorMin = Vector2.zero;
            resultRect.anchorMax = Vector2.one;
            resultRect.sizeDelta = Vector2.zero;

            CanvasGroup canvasGroup = resultObj.AddComponent<CanvasGroup>();

            GameObject dimObj = new GameObject("DimBackground");
            dimObj.transform.SetParent(resultObj.transform, false);
            RectTransform dimRect = dimObj.AddComponent<RectTransform>();
            dimRect.anchorMin = Vector2.zero;
            dimRect.anchorMax = Vector2.one;
            dimRect.sizeDelta = Vector2.zero;
            Image dimImage = dimObj.AddComponent<Image>();
            dimImage.color = new Color(0, 0, 0, 0.8f);

            GameObject panelObj = new GameObject("ResultPanel");
            panelObj.transform.SetParent(resultObj.transform, false);
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(700, 500);
            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0.15f, 0.15f, 0.2f);

            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(panelObj.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0, -40);
            titleRect.sizeDelta = new Vector2(600, 80);
            Text titleText = CreateText(titleObj, "VICTORY", 60, new Color(1f, 0.84f, 0f));
            titleText.fontStyle = FontStyle.Bold;

            GameObject statsTextObj = new GameObject("StatsText");
            statsTextObj.transform.SetParent(panelObj.transform, false);
            RectTransform statsRect = statsTextObj.AddComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0.5f, 0.5f);
            statsRect.anchorMax = new Vector2(0.5f, 0.5f);
            statsRect.anchoredPosition = new Vector2(0, 30);
            statsRect.sizeDelta = new Vector2(600, 250);
            Text statsText = CreateText(statsTextObj, "Stats", 28, Color.white);
            statsText.alignment = TextAnchor.UpperLeft;

            GameObject confirmBtnObj = new GameObject("ConfirmButton");
            confirmBtnObj.transform.SetParent(panelObj.transform, false);
            RectTransform confirmRect = confirmBtnObj.AddComponent<RectTransform>();
            confirmRect.anchorMin = new Vector2(0.5f, 0f);
            confirmRect.anchorMax = new Vector2(0.5f, 0f);
            confirmRect.pivot = new Vector2(0.5f, 0f);
            confirmRect.anchoredPosition = new Vector2(0, 40);
            confirmRect.sizeDelta = new Vector2(300, 80);
            Image confirmImage = confirmBtnObj.AddComponent<Image>();
            confirmImage.color = new Color(0.2f, 0.6f, 0.2f);

            Button confirmButton = confirmBtnObj.AddComponent<Button>();
            var colors = confirmButton.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
            confirmButton.colors = colors;

            GameObject confirmTextObj = new GameObject("Text");
            confirmTextObj.transform.SetParent(confirmBtnObj.transform, false);
            RectTransform confirmTextRect = confirmTextObj.AddComponent<RectTransform>();
            confirmTextRect.anchorMin = Vector2.zero;
            confirmTextRect.anchorMax = Vector2.one;
            confirmTextRect.sizeDelta = Vector2.zero;
            Text confirmText = CreateText(confirmTextObj, "확인", 32, Color.white);
            confirmText.fontStyle = FontStyle.Bold;

            GameResultUI resultUI = resultObj.AddComponent<GameResultUI>();
            SetField(resultUI, "canvasGroup", canvasGroup);
            SetField(resultUI, "titleText", titleText);
            SetField(resultUI, "statsText", statsText);
            SetField(resultUI, "confirmButton", confirmButton);

            Debug.Log("[GameSceneUIBuilder] GameResultUI created");
        }
        #endregion

        #region Helpers
        private Text CreateText(GameObject parent, string content, int fontSize, Color color)
        {
            Text text = parent.AddComponent<Text>();
            text.text = content;
            text.font = defaultFont;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.resizeTextForBestFit = false;
            text.raycastTarget = true;
            return text;
        }

        private void SetField(object obj, string fieldName, object value)
        {
            FieldInfo field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(obj, value);
            }
            else
            {
                Debug.LogWarning($"[GameSceneUIBuilder] Field '{fieldName}' not found on {obj.GetType().Name}");
            }
        }
        #endregion
    }
}
