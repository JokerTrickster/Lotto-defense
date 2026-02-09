using UnityEngine;
using UnityEngine.UI;
using LottoDefense.Grid;
using LottoDefense.Monsters;
using LottoDefense.Units;
using LottoDefense.UI;

namespace LottoDefense.Gameplay
{
    public class GameSceneBootstrapper : MonoBehaviour
    {
        [Header("References (Auto-created if null)")]
        [SerializeField] private Canvas mainCanvas;

        private Font defaultFont;

        private void Awake()
        {
            Debug.Log($"[GameSceneBootstrapper] Awake called on GameObject: {gameObject.name}");
            Debug.Log("[GameSceneBootstrapper] Starting game scene initialization...");

            defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (defaultFont == null)
                Debug.LogError("[GameSceneBootstrapper] Failed to load Arial font!");
            else
                Debug.Log("[GameSceneBootstrapper] Arial font loaded successfully");

            Debug.Log("[GameSceneBootstrapper] Creating main canvas...");
            EnsureMainCanvas();
            Debug.Log("[GameSceneBootstrapper] Main canvas created");

            EnsureGridManager();
            EnsureMonsterManager();
            EnsureRoundManager();
            EnsureUnitManager();
            EnsureUnitPlacementManager();

            EnsureCountdownUI();
            EnsureGameHUD();
            EnsureSummonButton();
            EnsureBackToMenuButton();

            Debug.Log("[GameSceneBootstrapper] Game scene initialization complete");
        }

        private void Start()
        {
            // Safety net: ensure countdown starts even if GameplayManager.Start() has timing issues
            // with DontDestroyOnLoad objects created during sceneLoaded callbacks
            StartCoroutine(EnsureCountdownStarted());
        }

        private System.Collections.IEnumerator EnsureCountdownStarted()
        {
            // Wait 3 frames to give GameplayManager.Start() a chance to handle it first
            yield return null;
            yield return null;
            yield return null;

            if (GameplayManager.Instance != null &&
                GameplayManager.Instance.CurrentState == GameState.Countdown)
            {
                Debug.Log("[GameSceneBootstrapper] Safety net: triggering countdown");
                GameplayManager.Instance.StartCountdown();
            }
        }

        #region Canvas
        private void EnsureMainCanvas()
        {
            Canvas existingCanvas = FindFirstObjectByType<Canvas>();
            if (existingCanvas != null && existingCanvas.gameObject.name != "GameCanvas")
            {
                Destroy(existingCanvas.gameObject);
                mainCanvas = null;
            }

            if (mainCanvas == null)
            {
                GameObject canvasObj = new GameObject("GameCanvas");
                mainCanvas = canvasObj.AddComponent<Canvas>();
                mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                mainCanvas.sortingOrder = 100;

                var scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;

                canvasObj.AddComponent<GraphicRaycaster>();
            }
        }
        #endregion

        #region Managers
        private void EnsureGridManager()
        {
            if (FindFirstObjectByType<GridManager>() == null)
                new GameObject("GridManager").AddComponent<GridManager>();
        }

        private void EnsureMonsterManager()
        {
            MonsterManager manager = FindFirstObjectByType<MonsterManager>();
            if (manager == null)
            {
                GameObject obj = new GameObject("MonsterManager");
                manager = obj.AddComponent<MonsterManager>();
            }

            MonsterData[] monsterData = Resources.LoadAll<MonsterData>("Monsters");
            if (monsterData != null && monsterData.Length > 0)
            {
                SetField(manager, "monsterDataPool", monsterData);
                Debug.Log($"[GameSceneBootstrapper] Loaded {monsterData.Length} monster types");
            }
            else
            {
                Debug.LogWarning("[GameSceneBootstrapper] No MonsterData found in Resources/Monsters!");
            }
        }

        private void EnsureRoundManager()
        {
            RoundManager manager = FindFirstObjectByType<RoundManager>();
            if (manager == null)
            {
                GameObject obj = new GameObject("RoundManager");
                manager = obj.AddComponent<RoundManager>();
            }

            DifficultyConfig config = Resources.Load<DifficultyConfig>("DifficultyConfig");
            if (config != null)
            {
                SetField(manager, "difficultyConfig", config);
            }
        }

        private void EnsureUnitManager()
        {
            if (FindFirstObjectByType<UnitManager>() == null)
                new GameObject("UnitManager").AddComponent<UnitManager>();
        }

        private void EnsureUnitPlacementManager()
        {
            if (FindFirstObjectByType<UnitPlacementManager>() == null)
                new GameObject("UnitPlacementManager").AddComponent<UnitPlacementManager>();
        }
        #endregion

        #region Countdown UI
        private void EnsureCountdownUI()
        {
            CountdownUI countdown = FindFirstObjectByType<CountdownUI>();
            if (countdown != null) return;

            // Full-screen overlay
            GameObject countdownObj = new GameObject("CountdownUI");
            countdownObj.transform.SetParent(mainCanvas.transform, false);

            RectTransform rect = countdownObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Semi-transparent dark overlay background
            Image overlayBg = countdownObj.AddComponent<Image>();
            overlayBg.color = GameSceneDesignTokens.CountdownOverlay;
            overlayBg.raycastTarget = false;

            CanvasGroup canvasGroup = countdownObj.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            // Countdown number text
            GameObject textObj = new GameObject("CountdownText");
            textObj.transform.SetParent(countdownObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = new Vector2(400, 300);

            // Drop shadow for countdown number
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

            // Add Outline component for extra visibility
            Outline outline = textObj.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.5f);
            outline.effectDistance = new Vector2(3, -3);

            countdown = countdownObj.AddComponent<CountdownUI>();
            SetField(countdown, "countdownText", countdownText);
            SetField(countdown, "canvasGroup", canvasGroup);
        }
        #endregion

        #region HUD
        private void EnsureGameHUD()
        {
            if (FindFirstObjectByType<GameHUD>() != null) return;

            // ---- Root HUD panel (top of screen) ----
            GameObject hudObj = new GameObject("GameHUD");
            hudObj.transform.SetParent(mainCanvas.transform, false);

            RectTransform hudRect = hudObj.AddComponent<RectTransform>();
            hudRect.anchorMin = new Vector2(0, 1);
            hudRect.anchorMax = new Vector2(1, 1);
            hudRect.pivot = new Vector2(0.5f, 1);
            hudRect.anchoredPosition = Vector2.zero;
            hudRect.sizeDelta = new Vector2(0, GameSceneDesignTokens.HudHeight);

            Image hudBg = hudObj.AddComponent<Image>();
            hudBg.color = GameSceneDesignTokens.HudBackground;
            hudBg.raycastTarget = false;

            VerticalLayoutGroup vlayout = hudObj.AddComponent<VerticalLayoutGroup>();
            int padH = Mathf.RoundToInt(GameSceneDesignTokens.HudPaddingH);
            int padV = Mathf.RoundToInt(GameSceneDesignTokens.HudPaddingV);
            vlayout.padding = new RectOffset(padH, padH, padV, padV);
            vlayout.spacing = 6;
            vlayout.childForceExpandWidth = true;
            vlayout.childForceExpandHeight = false;
            vlayout.childControlWidth = true;
            vlayout.childControlHeight = true;

            // ---- Row 1: ROUND | PHASE | TIMER ----
            GameObject row1 = CreateHUDRow(hudObj.transform, "Row_Top", 48);
            Text roundText = CreateStatWithLabel(row1.transform, "Round", "ROUND", "R1",
                GameSceneDesignTokens.StatLabel, GameSceneDesignTokens.RoundColor,
                GameSceneDesignTokens.StatLabelSize, GameSceneDesignTokens.StatValueSize);

            Text phaseText = CreateStatWithLabel(row1.transform, "Phase", "PHASE", "COUNTDOWN",
                GameSceneDesignTokens.StatLabel, GameSceneDesignTokens.PhaseColor,
                GameSceneDesignTokens.StatLabelSize, GameSceneDesignTokens.PhaseTextSize);

            Text timeText = CreateStatWithLabel(row1.transform, "Time", "TIME", "00:00",
                GameSceneDesignTokens.StatLabel, GameSceneDesignTokens.TimeColor,
                GameSceneDesignTokens.StatLabelSize, GameSceneDesignTokens.StatValueSize);

            // ---- Divider line ----
            CreateDivider(hudObj.transform);

            // ---- Row 2: LIFE | GOLD | MONSTERS | UNITS ----
            GameObject row2 = CreateHUDRow(hudObj.transform, "Row_Stats", 52);
            Text lifeText = CreateStatCard(row2.transform, "Life", "\u2665", "10",
                GameSceneDesignTokens.LifeColor);
            Text goldText = CreateStatCard(row2.transform, "Gold", "\u2666", "30",
                GameSceneDesignTokens.GoldColor);
            Text monsterText = CreateStatCard(row2.transform, "Monster", "\u25C6", "0",
                GameSceneDesignTokens.MonsterColor);
            Text unitText = CreateStatCard(row2.transform, "Unit", "\u25A0", "0",
                GameSceneDesignTokens.UnitColor);

            GameHUD hud = hudObj.AddComponent<GameHUD>();
            SetField(hud, "roundText", roundText);
            SetField(hud, "phaseText", phaseText);
            SetField(hud, "timeText", timeText);
            SetField(hud, "monsterText", monsterText);
            SetField(hud, "goldText", goldText);
            SetField(hud, "unitText", unitText);
            SetField(hud, "lifeText", lifeText);
        }

        /// <summary>
        /// Create a horizontal row container for HUD.
        /// </summary>
        private GameObject CreateHUDRow(Transform parent, string name, float height)
        {
            GameObject row = new GameObject(name);
            row.transform.SetParent(parent, false);

            RectTransform rect = row.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, height);

            HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childAlignment = TextAnchor.MiddleCenter;

            return row;
        }

        /// <summary>
        /// Create a stat display with small label above and large value below.
        /// Returns the value Text (used for HUD binding).
        /// </summary>
        private Text CreateStatWithLabel(Transform parent, string name, string label, string value,
            Color labelColor, Color valueColor, int labelSize, int valueSize)
        {
            GameObject container = new GameObject(name);
            container.transform.SetParent(parent, false);
            container.AddComponent<RectTransform>();

            VerticalLayoutGroup vl = container.AddComponent<VerticalLayoutGroup>();
            vl.spacing = 0;
            vl.childForceExpandWidth = true;
            vl.childForceExpandHeight = true;
            vl.childControlWidth = true;
            vl.childControlHeight = true;
            vl.childAlignment = TextAnchor.MiddleCenter;

            // Label text (small, dimmed)
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(container.transform, false);
            labelObj.AddComponent<RectTransform>();
            Text labelText = CreateText(labelObj, label, labelSize, labelColor);
            labelText.fontStyle = FontStyle.Normal;
            labelText.raycastTarget = false;

            // Value text (large, vivid)
            GameObject valueObj = new GameObject("Value");
            valueObj.transform.SetParent(container.transform, false);
            valueObj.AddComponent<RectTransform>();
            Text valueText = CreateText(valueObj, value, valueSize, valueColor);
            valueText.fontStyle = FontStyle.Bold;
            valueText.raycastTarget = false;

            return valueText;
        }

        /// <summary>
        /// Create a stat card with icon symbol + value.
        /// Used for Life/Gold/Monster/Unit row.
        /// </summary>
        private Text CreateStatCard(Transform parent, string name, string icon, string value, Color accentColor)
        {
            GameObject card = new GameObject(name);
            card.transform.SetParent(parent, false);

            RectTransform cardRect = card.AddComponent<RectTransform>();

            // Card background
            Image cardBg = card.AddComponent<Image>();
            cardBg.color = GameSceneDesignTokens.HudStatCardBg;
            cardBg.raycastTarget = false;

            HorizontalLayoutGroup hl = card.AddComponent<HorizontalLayoutGroup>();
            int pad = Mathf.RoundToInt(GameSceneDesignTokens.StatCardPadding);
            hl.padding = new RectOffset(pad + 4, pad + 4, pad, pad);
            hl.spacing = 4;
            hl.childForceExpandWidth = false;
            hl.childForceExpandHeight = true;
            hl.childControlWidth = true;
            hl.childControlHeight = true;
            hl.childAlignment = TextAnchor.MiddleCenter;

            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(card.transform, false);
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            LayoutElement iconLE = iconObj.AddComponent<LayoutElement>();
            iconLE.preferredWidth = 32;
            Text iconText = CreateText(iconObj, icon, 26, accentColor);
            iconText.fontStyle = FontStyle.Bold;
            iconText.raycastTarget = false;

            // Value
            GameObject valueObj = new GameObject("Value");
            valueObj.transform.SetParent(card.transform, false);
            valueObj.AddComponent<RectTransform>();
            LayoutElement valueLE = valueObj.AddComponent<LayoutElement>();
            valueLE.flexibleWidth = 1;
            Text valueText = CreateText(valueObj, value, GameSceneDesignTokens.StatValueSize, Color.white);
            valueText.fontStyle = FontStyle.Bold;
            valueText.raycastTarget = false;

            return valueText;
        }

        /// <summary>
        /// Create a thin horizontal divider line.
        /// </summary>
        private void CreateDivider(Transform parent)
        {
            GameObject divider = new GameObject("Divider");
            divider.transform.SetParent(parent, false);

            RectTransform divRect = divider.AddComponent<RectTransform>();
            divRect.sizeDelta = new Vector2(0, 1);

            LayoutElement le = divider.AddComponent<LayoutElement>();
            le.preferredHeight = 1;
            le.flexibleWidth = 1;

            Image divImg = divider.AddComponent<Image>();
            divImg.color = GameSceneDesignTokens.HudBorder;
            divImg.raycastTarget = false;
        }
        #endregion

        #region Buttons
        private void EnsureSummonButton()
        {
            GameObject btnObj = new GameObject("SummonButton");
            btnObj.transform.SetParent(mainCanvas.transform, false);

            float marginH = GameSceneDesignTokens.ButtonMarginH;
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(marginH, 0);
            btnRect.anchorMax = new Vector2(1f - marginH, 0);
            btnRect.pivot = new Vector2(0.5f, 0);
            btnRect.anchoredPosition = new Vector2(0, GameSceneDesignTokens.MenuButtonHeight + GameSceneDesignTokens.ButtonGap * 2 + 16);
            btnRect.sizeDelta = new Vector2(0, GameSceneDesignTokens.SummonButtonHeight);

            // Button background
            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = GameSceneDesignTokens.SummonButtonBg;

            // Button component with color states
            Button button = btnObj.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
            colors.fadeDuration = 0.1f;
            button.colors = colors;

            // Inner layout: vertical stack for main text + cost subtext
            VerticalLayoutGroup vl = btnObj.AddComponent<VerticalLayoutGroup>();
            vl.padding = new RectOffset(0, 0, 8, 8);
            vl.spacing = 0;
            vl.childForceExpandWidth = true;
            vl.childForceExpandHeight = true;
            vl.childControlWidth = true;
            vl.childControlHeight = true;
            vl.childAlignment = TextAnchor.MiddleCenter;

            // Main text: "소환"
            GameObject mainTextObj = new GameObject("MainText");
            mainTextObj.transform.SetParent(btnObj.transform, false);
            mainTextObj.AddComponent<RectTransform>();
            Text mainText = CreateText(mainTextObj, "\uC18C\uD658", GameSceneDesignTokens.SummonTextSize, GameSceneDesignTokens.ButtonText);
            mainText.fontStyle = FontStyle.Bold;
            Outline mainOutline = mainTextObj.AddComponent<Outline>();
            mainOutline.effectColor = new Color(0, 0, 0, 0.4f);
            mainOutline.effectDistance = new Vector2(2, -2);

            // Cost subtext: "5 Gold"
            GameObject costTextObj = new GameObject("CostText");
            costTextObj.transform.SetParent(btnObj.transform, false);
            costTextObj.AddComponent<RectTransform>();
            Text costText = CreateText(costTextObj, "- 5 Gold -", GameSceneDesignTokens.SummonCostSize, GameSceneDesignTokens.ButtonCostText);
            costText.fontStyle = FontStyle.Bold;

            button.onClick.AddListener(() => OnSummonButtonClicked(mainText, costText));
        }

        private void EnsureBackToMenuButton()
        {
            GameObject btnObj = new GameObject("BackToMenuButton");
            btnObj.transform.SetParent(mainCanvas.transform, false);

            float marginH = GameSceneDesignTokens.ButtonMarginH;
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(marginH + 0.1f, 0);
            btnRect.anchorMax = new Vector2(1f - marginH - 0.1f, 0);
            btnRect.pivot = new Vector2(0.5f, 0);
            btnRect.anchoredPosition = new Vector2(0, 16);
            btnRect.sizeDelta = new Vector2(0, GameSceneDesignTokens.MenuButtonHeight);

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = GameSceneDesignTokens.MenuButtonBg;

            Button button = btnObj.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            colors.fadeDuration = 0.1f;
            button.colors = colors;

            SceneNavigator navigator = btnObj.AddComponent<SceneNavigator>();
            button.onClick.AddListener(navigator.LoadMainGame);

            // Button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text btnText = CreateText(textObj, "\u25C0 \uBA54\uC778 \uBA54\uB274", GameSceneDesignTokens.MenuTextSize, GameSceneDesignTokens.ButtonText);
            btnText.fontStyle = FontStyle.Bold;

            Outline outline = textObj.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.3f);
            outline.effectDistance = new Vector2(1, -1);
        }
        #endregion

        #region Button Logic
        private void OnSummonButtonClicked(Text mainText, Text costText)
        {
            if (GameplayManager.Instance == null) return;

            if (GameplayManager.Instance.CurrentState != GameState.Preparation)
            {
                StartCoroutine(FlashButtonText(mainText, costText, "\uC804\uD22C \uC911!", ""));
                return;
            }

            UnitManager unitMgr = FindFirstObjectByType<UnitManager>();
            if (unitMgr == null)
            {
                Debug.LogError("[GameSceneBootstrapper] UnitManager not found");
                return;
            }

            if (!unitMgr.CanDraw())
            {
                StartCoroutine(FlashButtonText(mainText, costText, "\uBD80\uC871!", "\uACE8\uB4DC\uAC00 \uBD80\uC871\uD569\uB2C8\uB2E4"));
                return;
            }

            UnitData drawnUnit = unitMgr.DrawUnit();
            if (drawnUnit != null)
            {
                UnitPlacementManager placementMgr = FindFirstObjectByType<UnitPlacementManager>();
                if (placementMgr != null)
                {
                    placementMgr.SelectUnitForPlacement(drawnUnit);
                    StartCoroutine(FlashButtonText(mainText, costText,
                        drawnUnit.unitName, "\uBC30\uCE58\uD560 \uC704\uCE58\uB97C \uD130\uCE58\uD558\uC138\uC694!"));
                }
            }
        }

        private System.Collections.IEnumerator FlashButtonText(Text mainText, Text costText, string mainMsg, string costMsg)
        {
            string origMain = "\uC18C\uD658";
            string origCost = "- 5 Gold -";
            Color origMainColor = GameSceneDesignTokens.ButtonText;
            Color origCostColor = GameSceneDesignTokens.ButtonCostText;

            mainText.text = mainMsg;
            mainText.color = Color.white;
            costText.text = costMsg;
            costText.color = new Color(1f, 1f, 1f, 0.8f);

            yield return new WaitForSeconds(1.5f);

            mainText.text = origMain;
            mainText.color = origMainColor;
            costText.text = origCost;
            costText.color = origCostColor;
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Create a Text component with font properly assigned.
        /// This is the ONLY place Text components should be created.
        /// </summary>
        private Text CreateText(GameObject obj, string text, int fontSize, Color color)
        {
            Text t = obj.AddComponent<Text>();
            t.font = defaultFont;
            t.text = text;
            t.fontSize = fontSize;
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        private void SetField<T>(object target, string fieldName, T value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(target, value);
        }
        #endregion
    }
}
