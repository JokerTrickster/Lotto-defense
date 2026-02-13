using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using LottoDefense.Grid;
using LottoDefense.Monsters;
using LottoDefense.Units;
using LottoDefense.UI;
using LottoDefense.Combat;
using LottoDefense.VFX;
using LottoDefense.Quests;
using LottoDefense.Networking;
using UnityEngine.EventSystems;

namespace LottoDefense.Gameplay
{
    public class GameSceneBootstrapper : MonoBehaviour
    {
        [Header("References (Auto-created if null)")]
        [SerializeField] private Canvas mainCanvas;

        private Font defaultFont;
        private GameBalanceConfig balanceConfig;

        private void Awake()
        {
            Debug.Log($"[GameSceneBootstrapper] Awake called on GameObject: {gameObject.name}");
            Debug.Log("[GameSceneBootstrapper] Starting game scene initialization...");

            // Unity 2022+ uses LegacyRuntime.ttf instead of Arial.ttf
            defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null)
            {
                // Fallback to Arial.ttf for older Unity versions
                defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            if (defaultFont == null)
                Debug.LogError("[GameSceneBootstrapper] Failed to load default font!");
            else
                Debug.Log($"[GameSceneBootstrapper] Font loaded successfully: {defaultFont.name}");

            // Load or create game balance config (CENTRAL CONFIG)
            balanceConfig = LoadOrCreateGameBalanceConfig();
            Debug.Log("[GameSceneBootstrapper] Game balance config loaded");

            Debug.Log("[GameSceneBootstrapper] Creating main canvas...");
            EnsureMainCanvas();
            EnsureEventSystem();
            Debug.Log("[GameSceneBootstrapper] Main canvas created");

            EnsureGridManager();
            EnsureMonsterManager();
            EnsureRoundManager();
            EnsureUnitManager();
            EnsureUnitPlacementManager();
            EnsureSynthesisManager();
            EnsureCombatManager();
            EnsureVFXManager();

            EnsureCountdownUI();
            EnsureRoundStartUI();
            EnsureUnitSelectionUI();
            EnsureGameBottomUI();
            EnsureGameHUD();
            EnsureSummonButton();
            EnsureSynthesisGuideButton();
            EnsureQuestButton();
            EnsureGameResultUI();
            EnsureOpponentStatusUI();

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

        private void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<EventSystem>();
                esObj.AddComponent<StandaloneInputModule>();
                Debug.Log("[GameSceneBootstrapper] Created EventSystem");
            }
        }

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

            // Create MonsterData from central balance config
            MonsterData[] monsterData = new MonsterData[balanceConfig.monsters.Count];
            for (int i = 0; i < balanceConfig.monsters.Count; i++)
            {
                monsterData[i] = CreateMonsterDataFromConfig(balanceConfig.monsters[i]);
            }

            SetField(manager, "monsterDataPool", monsterData);
            Debug.Log($"[GameSceneBootstrapper] Created {monsterData.Length} monster types from GameBalanceConfig");

            // Set spawn rate from balance config
            float spawnInterval = 1f / balanceConfig.gameRules.spawnRate;
            SetField(manager, "spawnInterval", spawnInterval);
            Debug.Log($"[GameSceneBootstrapper] Set spawn interval to {spawnInterval:F2}s ({balanceConfig.gameRules.spawnRate} monsters/sec)");

            // Load and pass RoundConfig so MonsterManager can use per-round monster configs
            RoundConfig roundConfig = Resources.Load<RoundConfig>("RoundConfig");
            if (roundConfig != null)
            {
                SetField(manager, "roundConfig", roundConfig);
                Debug.Log("[GameSceneBootstrapper] Passed RoundConfig to MonsterManager");
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

            // Create DifficultyConfig from central balance config
            DifficultyConfig config = CreateDifficultyConfigFromConfig(balanceConfig.difficulty);
            SetField(manager, "difficultyConfig", config);
            Debug.Log("[GameSceneBootstrapper] Created DifficultyConfig from GameBalanceConfig");

            // Pass phase timing from GameBalanceConfig game rules
            SetField(manager, "preparationDuration", (float)balanceConfig.gameRules.preparationTime);
            SetField(manager, "combatDuration", (float)balanceConfig.gameRules.combatTime);
            Debug.Log($"[GameSceneBootstrapper] Set RoundManager timing - Prep: {balanceConfig.gameRules.preparationTime}s, Combat: {balanceConfig.gameRules.combatTime}s");

            // Load and pass RoundConfig so RoundManager can use per-round definitions
            RoundConfig roundConfig = Resources.Load<RoundConfig>("RoundConfig");
            if (roundConfig != null)
            {
                SetField(manager, "roundConfig", roundConfig);
                Debug.Log($"[GameSceneBootstrapper] Passed RoundConfig to RoundManager (totalRounds={roundConfig.TotalRounds})");
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

        private void EnsureSynthesisManager()
        {
            if (FindFirstObjectByType<LottoDefense.Units.SynthesisManager>() == null)
                new GameObject("SynthesisManager").AddComponent<LottoDefense.Units.SynthesisManager>();
        }

        private void EnsureCombatManager()
        {
            if (FindFirstObjectByType<LottoDefense.Combat.CombatManager>() == null)
            {
                GameObject obj = new GameObject("CombatManager");
                obj.AddComponent<LottoDefense.Combat.CombatManager>();
                Debug.Log("[GameSceneBootstrapper] Created CombatManager");
            }
        }

        private void EnsureVFXManager()
        {
            if (FindFirstObjectByType<LottoDefense.VFX.VFXManager>() == null)
            {
                GameObject obj = new GameObject("VFXManager");
                obj.AddComponent<LottoDefense.VFX.VFXManager>();
                Debug.Log("[GameSceneBootstrapper] Created VFXManager");
            }
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

            // "START!" text (hidden initially, shown after "1")
            GameObject startTextObj = new GameObject("StartText");
            startTextObj.transform.SetParent(countdownObj.transform, false);

            RectTransform startTextRect = startTextObj.AddComponent<RectTransform>();
            startTextRect.anchorMin = new Vector2(0.5f, 0.5f);
            startTextRect.anchorMax = new Vector2(0.5f, 0.5f);
            startTextRect.anchoredPosition = Vector2.zero;
            startTextRect.sizeDelta = new Vector2(600, 300);

            Text startText = CreateText(startTextObj, "START!", 160, GameSceneDesignTokens.CountdownStartColor);
            startText.fontStyle = FontStyle.Bold;
            startText.raycastTarget = false;

            Outline startOutline = startTextObj.AddComponent<Outline>();
            startOutline.effectColor = new Color(0, 0, 0, 0.5f);
            startOutline.effectDistance = new Vector2(3, -3);

            startTextObj.SetActive(false); // Hidden initially

            countdown = countdownObj.AddComponent<CountdownUI>();
            SetField(countdown, "countdownText", countdownText);
            SetField(countdown, "startText", startText);
            SetField(countdown, "canvasGroup", canvasGroup);
        }
        #endregion

        #region Round Start UI
        private void EnsureRoundStartUI()
        {
            RoundStartUI roundStartUI = FindFirstObjectByType<RoundStartUI>();
            if (roundStartUI != null) return;

            // Full-screen overlay for round start notification
            GameObject roundStartObj = new GameObject("RoundStartUI");
            roundStartObj.transform.SetParent(mainCanvas.transform, false);

            RectTransform rect = roundStartObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            CanvasGroup canvasGroup = roundStartObj.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;

            // Round text container
            GameObject textObj = new GameObject("RoundText");
            textObj.transform.SetParent(roundStartObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = new Vector2(600, 200);

            // Drop shadow for text
            GameObject shadowObj = new GameObject("Shadow");
            shadowObj.transform.SetParent(textObj.transform, false);
            RectTransform shadowRect = shadowObj.AddComponent<RectTransform>();
            shadowRect.anchorMin = Vector2.zero;
            shadowRect.anchorMax = Vector2.one;
            shadowRect.offsetMin = new Vector2(4, -4);
            shadowRect.offsetMax = new Vector2(4, -4);
            Text shadowText = CreateText(shadowObj, "라운드 1", 80, new Color(0, 0, 0, 0.6f));
            shadowText.fontStyle = FontStyle.Bold;
            shadowText.raycastTarget = false;

            Text roundText = CreateText(textObj, "라운드 1", 80, new Color(1f, 0.9f, 0.3f));
            roundText.fontStyle = FontStyle.Bold;

            // Add Outline component for extra visibility
            Outline outline = textObj.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.5f);
            outline.effectDistance = new Vector2(4, -4);

            RoundStartUI component = roundStartObj.AddComponent<RoundStartUI>();
            SetField(component, "roundText", roundText);
            SetField(component, "canvasGroup", canvasGroup);

            Debug.Log("[GameSceneBootstrapper] Created RoundStartUI");
        }
        #endregion

        #region Unit Selection UI
        private void EnsureUnitSelectionUI()
        {
            UnitSelectionUI selectionUI = FindFirstObjectByType<UnitSelectionUI>();
            if (selectionUI != null) return;

            // Always-active container for UnitSelectionUI component
            GameObject containerObj = new GameObject("UnitSelectionUIContainer");
            containerObj.transform.SetParent(mainCanvas.transform, false);

            // Simple floating name label panel (sell/synthesis moved to GameBottomUI)
            GameObject selectionPanelObj = new GameObject("UnitSelectionPanel");
            selectionPanelObj.transform.SetParent(mainCanvas.transform, false);

            RectTransform panelRect = selectionPanelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.zero;
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.sizeDelta = new Vector2(220f, 50f);

            // Dark background with gold border
            Image panelBg = selectionPanelObj.AddComponent<Image>();
            panelBg.color = GameSceneDesignTokens.SelectionPanelBg;

            Outline panelOutline = selectionPanelObj.AddComponent<Outline>();
            panelOutline.effectColor = GameSceneDesignTokens.SelectionPanelBorder;
            panelOutline.effectDistance = new Vector2(2, -2);

            // Unit name text
            GameObject unitNameObj = new GameObject("UnitNameText");
            unitNameObj.transform.SetParent(selectionPanelObj.transform, false);

            RectTransform nameRect = unitNameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = Vector2.zero;
            nameRect.anchorMax = Vector2.one;
            nameRect.offsetMin = new Vector2(8, 4);
            nameRect.offsetMax = new Vector2(-8, -4);

            Text unitNameText = CreateText(unitNameObj, "\uC720\uB2DB", 24, GameSceneDesignTokens.GoldColor);
            unitNameText.alignment = TextAnchor.MiddleCenter;
            unitNameText.fontStyle = FontStyle.Bold;
            unitNameText.resizeTextForBestFit = true;
            unitNameText.resizeTextMinSize = 18;
            unitNameText.resizeTextMaxSize = 24;

            Outline nameOutline = unitNameObj.AddComponent<Outline>();
            nameOutline.effectColor = new Color(0f, 0f, 0f, 0.5f);
            nameOutline.effectDistance = new Vector2(1, -1);

            // Add UnitSelectionUI component on the always-active container
            UnitSelectionUI component = containerObj.AddComponent<UnitSelectionUI>();
            SetField(component, "selectionPanel", selectionPanelObj);
            SetField(component, "unitNameText", unitNameText);

            // Start panel hidden (container stays active)
            selectionPanelObj.SetActive(false);

            Debug.Log("[GameSceneBootstrapper] Created UnitSelectionUI with name label");
        }
        #endregion

        #region Game Bottom UI
        private void EnsureGameBottomUI()
        {
            GameBottomUI bottomUI = FindFirstObjectByType<GameBottomUI>();
            if (bottomUI != null) return;

            // Bottom panel with 2-row layout: top row (sell/synthesis), bottom row (auto-synth/upgrades)
            GameObject bottomPanelObj = new GameObject("GameBottomUI");
            bottomPanelObj.transform.SetParent(mainCanvas.transform, false);

            RectTransform panelRect = bottomPanelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(1, 0);
            panelRect.pivot = new Vector2(0.5f, 0);
            panelRect.anchoredPosition = new Vector2(0, 16 + GameSceneDesignTokens.SummonButtonHeight + 12);
            panelRect.sizeDelta = new Vector2(-20, 0);

            // Dark panel background
            Image panelBg = bottomPanelObj.AddComponent<Image>();
            panelBg.color = new Color(0.04f, 0.05f, 0.08f, 0.94f);

            // Subtle border
            Outline panelOutline = bottomPanelObj.AddComponent<Outline>();
            panelOutline.effectColor = new Color(0.3f, 0.35f, 0.45f, 0.5f);
            panelOutline.effectDistance = new Vector2(2, -2);

            Shadow panelShadow = bottomPanelObj.AddComponent<Shadow>();
            panelShadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
            panelShadow.effectDistance = new Vector2(0, -3);

            // Auto-size height based on visible rows
            ContentSizeFitter csf = bottomPanelObj.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Vertical layout for 2 rows
            VerticalLayoutGroup vlayout = bottomPanelObj.AddComponent<VerticalLayoutGroup>();
            vlayout.padding = new RectOffset(12, 12, 8, 8);
            vlayout.spacing = 8;
            vlayout.childControlWidth = true;
            vlayout.childControlHeight = true;
            vlayout.childForceExpandWidth = true;
            vlayout.childForceExpandHeight = false;

            // --- Top row: Sell + Synthesis (hidden until unit selected) ---
            GameObject topRowObj = new GameObject("TopRow");
            topRowObj.transform.SetParent(bottomPanelObj.transform, false);

            LayoutElement topRowLayout = topRowObj.AddComponent<LayoutElement>();
            topRowLayout.preferredHeight = 80;

            HorizontalLayoutGroup topHLayout = topRowObj.AddComponent<HorizontalLayoutGroup>();
            topHLayout.spacing = 12;
            topHLayout.childControlWidth = true;
            topHLayout.childControlHeight = true;
            topHLayout.childForceExpandWidth = true;
            topHLayout.childForceExpandHeight = true;

            // Sell button (red)
            GameObject sellBtnObj = CreateGameButton(topRowObj.transform, "SellButton",
                "\uD310\uB9E4", GameSceneDesignTokens.SellBtnBg, GameSceneDesignTokens.SellBtnBorder);
            Button sellBtn = sellBtnObj.GetComponent<Button>();
            Text sellBtnText = sellBtnObj.GetComponentInChildren<Text>();

            // Synthesis button (gold)
            GameObject synthBtnObj = CreateGameButton(topRowObj.transform, "SynthesisButton",
                "\uC870\uD569", GameSceneDesignTokens.SynthFloatBtnBg, GameSceneDesignTokens.SynthFloatBtnBorder);
            Button synthBtn = synthBtnObj.GetComponent<Button>();
            Text synthBtnText = synthBtnObj.GetComponentInChildren<Text>();
            synthBtnText.color = GameSceneDesignTokens.SynthFloatBtnText;

            topRowObj.SetActive(false); // Hidden until unit selected

            // --- Bottom row: Auto Synthesis + Attack UP + Speed UP (always visible) ---
            GameObject bottomRowObj = new GameObject("BottomRow");
            bottomRowObj.transform.SetParent(bottomPanelObj.transform, false);

            LayoutElement bottomRowLayout = bottomRowObj.AddComponent<LayoutElement>();
            bottomRowLayout.preferredHeight = 90;

            HorizontalLayoutGroup bottomHLayout = bottomRowObj.AddComponent<HorizontalLayoutGroup>();
            bottomHLayout.spacing = 12;
            bottomHLayout.childControlWidth = true;
            bottomHLayout.childControlHeight = true;
            bottomHLayout.childForceExpandWidth = true;
            bottomHLayout.childForceExpandHeight = true;

            // Auto Synthesis button
            GameObject autoSynthButtonObj = CreateGameButton(bottomRowObj.transform, "AutoSynthButton",
                "\uC790\uB3D9 \uC870\uD569", GameSceneDesignTokens.AutoSynthBtnBg, GameSceneDesignTokens.AutoSynthBtnBorder);
            Button autoSynthButton = autoSynthButtonObj.GetComponent<Button>();
            Text autoSynthText = autoSynthButtonObj.GetComponentInChildren<Text>();

            // Attack Upgrade button
            GameObject attackUpButtonObj = CreateGameButton(bottomRowObj.transform, "AttackUpgradeButton",
                "\uACF5\uACA9\uB825 UP", GameSceneDesignTokens.AttackUpBtnBg, GameSceneDesignTokens.AttackUpBtnBorder);
            Button attackUpButton = attackUpButtonObj.GetComponent<Button>();
            Text attackUpText = attackUpButtonObj.GetComponentInChildren<Text>();

            // Attack Speed Upgrade button
            GameObject speedUpButtonObj = CreateGameButton(bottomRowObj.transform, "AttackSpeedUpgradeButton",
                "\uACF5\uC18D UP", GameSceneDesignTokens.SpeedUpBtnBg, GameSceneDesignTokens.SpeedUpBtnBorder);
            Button speedUpButton = speedUpButtonObj.GetComponent<Button>();
            Text speedUpText = speedUpButtonObj.GetComponentInChildren<Text>();

            // Add GameBottomUI component with all references
            GameBottomUI component = bottomPanelObj.AddComponent<GameBottomUI>();
            SetField(component, "panel", bottomPanelObj);
            SetField(component, "topRow", topRowObj);
            SetField(component, "autoSynthesisButton", autoSynthButton);
            SetField(component, "attackUpgradeButton", attackUpButton);
            SetField(component, "attackSpeedUpgradeButton", speedUpButton);
            SetField(component, "sellButton", sellBtn);
            SetField(component, "synthesisButton", synthBtn);
            SetField(component, "autoSynthesisButtonText", autoSynthText);
            SetField(component, "attackUpgradeButtonText", attackUpText);
            SetField(component, "attackSpeedUpgradeButtonText", speedUpText);
            SetField(component, "sellButtonText", sellBtnText);
            SetField(component, "synthesisButtonText", synthBtnText);

            bottomPanelObj.SetActive(true);

            Debug.Log("[GameSceneBootstrapper] Created GameBottomUI with 2-row layout");
        }

        private GameObject CreateGameButton(Transform parent, string name, string text, Color bgColor, Color borderColor)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);

            LayoutElement layoutElement = buttonObj.AddComponent<LayoutElement>();
            layoutElement.minHeight = 90;
            layoutElement.preferredHeight = 100;

            // Button background
            Image buttonBg = buttonObj.AddComponent<Image>();
            buttonBg.color = bgColor;

            // Colored border for identity
            Outline buttonOutline = buttonObj.AddComponent<Outline>();
            buttonOutline.effectColor = borderColor;
            buttonOutline.effectDistance = new Vector2(3, -3);

            // Drop shadow for depth
            Shadow buttonShadow = buttonObj.AddComponent<Shadow>();
            buttonShadow.effectColor = new Color(0, 0, 0, 0.5f);
            buttonShadow.effectDistance = new Vector2(3, -3);

            // Button component
            Button button = buttonObj.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            // Inner highlight strip (top-half lighter for pseudo-3D)
            GameObject highlightObj = new GameObject("Highlight");
            highlightObj.transform.SetParent(buttonObj.transform, false);
            RectTransform hlRect = highlightObj.AddComponent<RectTransform>();
            hlRect.anchorMin = new Vector2(0, 0.5f);
            hlRect.anchorMax = Vector2.one;
            hlRect.offsetMin = new Vector2(2, 0);
            hlRect.offsetMax = new Vector2(-2, -2);
            Image hlImg = highlightObj.AddComponent<Image>();
            hlImg.color = new Color(1f, 1f, 1f, 0.12f);
            hlImg.raycastTarget = false;

            // Button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(6, 6);
            textRect.offsetMax = new Vector2(-6, -6);

            Text buttonText = CreateText(textObj, text, 20, Color.white);
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.fontStyle = FontStyle.Bold;
            buttonText.resizeTextForBestFit = true;
            buttonText.resizeTextMinSize = 14;
            buttonText.resizeTextMaxSize = 20;

            Outline textOutline = textObj.AddComponent<Outline>();
            textOutline.effectColor = new Color(0, 0, 0, 0.8f);
            textOutline.effectDistance = new Vector2(1, -1);

            return buttonObj;
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
            hudBg.raycastTarget = true;

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

            // Stat containers for tooltip click detection
            RectTransform[] statContainers = new RectTransform[]
            {
                roundText.transform.parent.GetComponent<RectTransform>(),
                phaseText.transform.parent.GetComponent<RectTransform>(),
                timeText.transform.parent.GetComponent<RectTransform>(),
                lifeText.transform.parent.GetComponent<RectTransform>(),
                goldText.transform.parent.GetComponent<RectTransform>(),
                monsterText.transform.parent.GetComponent<RectTransform>(),
                unitText.transform.parent.GetComponent<RectTransform>()
            };
            SetField(hud, "statContainers", statContainers);

            // Tooltip panel (full-width bar below HUD)
            GameObject tooltipObj = new GameObject("TooltipPanel");
            tooltipObj.transform.SetParent(mainCanvas.transform, false);

            RectTransform tooltipRect = tooltipObj.AddComponent<RectTransform>();
            tooltipRect.anchorMin = new Vector2(0, 1);
            tooltipRect.anchorMax = new Vector2(1, 1);
            tooltipRect.pivot = new Vector2(0.5f, 1);
            tooltipRect.anchoredPosition = new Vector2(0, -GameSceneDesignTokens.HudHeight);
            tooltipRect.sizeDelta = new Vector2(0, 44);

            Image tooltipBg = tooltipObj.AddComponent<Image>();
            tooltipBg.color = new Color(0.08f, 0.1f, 0.15f, 0.95f);
            tooltipBg.raycastTarget = true;

            GameObject tooltipTextObj = new GameObject("Text");
            tooltipTextObj.transform.SetParent(tooltipObj.transform, false);
            RectTransform textRect = tooltipTextObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(16, 4);
            textRect.offsetMax = new Vector2(-16, -4);

            Text tooltipTextComp = CreateText(tooltipTextObj, "", 24, Color.white);
            tooltipTextComp.alignment = TextAnchor.MiddleCenter;

            tooltipObj.SetActive(false);

            SetField(hud, "tooltipPanel", tooltipObj);
            SetField(hud, "tooltipText", tooltipTextComp);
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
            btnRect.anchoredPosition = new Vector2(0, 16);
            btnRect.sizeDelta = new Vector2(0, GameSceneDesignTokens.SummonButtonHeight);

            // Vibrant green background
            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = GameSceneDesignTokens.SummonButtonBg;

            // Bright green border glow
            Outline btnOutline = btnObj.AddComponent<Outline>();
            btnOutline.effectColor = GameSceneDesignTokens.SummonButtonBorder;
            btnOutline.effectDistance = new Vector2(3, -3);

            // Drop shadow for depth
            Shadow btnShadow = btnObj.AddComponent<Shadow>();
            btnShadow.effectColor = new Color(0f, 0.2f, 0f, 0.5f);
            btnShadow.effectDistance = new Vector2(0, -4);

            // Button component
            Button button = btnObj.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            // Inner highlight strip (top portion lighter for 3D depth)
            GameObject highlightObj = new GameObject("Highlight");
            highlightObj.transform.SetParent(btnObj.transform, false);
            RectTransform hlRect = highlightObj.AddComponent<RectTransform>();
            hlRect.anchorMin = new Vector2(0, 0.5f);
            hlRect.anchorMax = Vector2.one;
            hlRect.offsetMin = new Vector2(3, 0);
            hlRect.offsetMax = new Vector2(-3, -3);
            Image hlImg = highlightObj.AddComponent<Image>();
            hlImg.color = new Color(1f, 1f, 1f, 0.15f);
            hlImg.raycastTarget = false;

            // Inner layout: vertical stack
            VerticalLayoutGroup vl = btnObj.AddComponent<VerticalLayoutGroup>();
            vl.padding = new RectOffset(0, 0, 8, 8);
            vl.spacing = 0;
            vl.childForceExpandWidth = true;
            vl.childForceExpandHeight = true;
            vl.childControlWidth = true;
            vl.childControlHeight = true;
            vl.childAlignment = TextAnchor.MiddleCenter;

            // Main text
            GameObject mainTextObj = new GameObject("MainText");
            mainTextObj.transform.SetParent(btnObj.transform, false);
            mainTextObj.AddComponent<RectTransform>();
            Text mainText = CreateText(mainTextObj, "\uC18C\uD658", GameSceneDesignTokens.SummonTextSize, GameSceneDesignTokens.ButtonText);
            mainText.fontStyle = FontStyle.Bold;
            Outline mainOutline = mainTextObj.AddComponent<Outline>();
            mainOutline.effectColor = new Color(0, 0, 0, 0.5f);
            mainOutline.effectDistance = new Vector2(2, -2);

            // Cost subtext
            GameObject costTextObj = new GameObject("CostText");
            costTextObj.transform.SetParent(btnObj.transform, false);
            costTextObj.AddComponent<RectTransform>();
            Text costText = CreateText(costTextObj, "- 5 Gold -", GameSceneDesignTokens.SummonCostSize, GameSceneDesignTokens.ButtonCostText);
            costText.fontStyle = FontStyle.Bold;
            Outline costOutline = costTextObj.AddComponent<Outline>();
            costOutline.effectColor = new Color(0, 0, 0, 0.4f);
            costOutline.effectDistance = new Vector2(1, -1);

            button.onClick.AddListener(() => OnSummonButtonClicked(mainText, costText));
        }

        private void EnsureSynthesisGuideButton()
        {
            // Top-right corner icon button (52x52) positioned below HUD
            GameObject btnObj = new GameObject("SynthesisGuideButton");
            btnObj.transform.SetParent(mainCanvas.transform, false);

            float btnSize = GameSceneDesignTokens.UtilityButtonSize;
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(1, 1);
            btnRect.anchorMax = new Vector2(1, 1);
            btnRect.pivot = new Vector2(1, 1);
            btnRect.anchoredPosition = new Vector2(-12, -(GameSceneDesignTokens.HudHeight + 8));
            btnRect.sizeDelta = new Vector2(btnSize, btnSize);

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = new Color(0.3f, 0.6f, 0.9f, 1f); // Blue

            Button button = btnObj.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            colors.fadeDuration = 0.1f;
            button.colors = colors;

            // Create SynthesisGuideUI first (before it gets deactivated)
            EnsureSynthesisGuideUI();

            // Use lambda that finds the guide at click-time, since it may be inactive
            button.onClick.AddListener(() => {
                SynthesisGuideUI guide = FindFirstObjectByType<SynthesisGuideUI>(FindObjectsInactive.Include);
                if (guide != null)
                {
                    guide.Show();
                }
            });

            // Button icon ("?" as fallback - emoji may not render in legacy Text)
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text btnText = CreateText(textObj, "?", 28, GameSceneDesignTokens.ButtonText);
            btnText.fontStyle = FontStyle.Bold;

            Outline outline = textObj.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.5f);
            outline.effectDistance = new Vector2(2, -2);
        }

        private void EnsureSynthesisGuideUI()
        {
            SynthesisGuideUI existingUI = FindFirstObjectByType<SynthesisGuideUI>();
            if (existingUI != null) return;

            // Create guide panel
            GameObject guideObj = new GameObject("SynthesisGuideUI");
            guideObj.transform.SetParent(mainCanvas.transform, false);

            RectTransform rect = guideObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Semi-transparent background
            Image bgImage = guideObj.AddComponent<Image>();
            bgImage.color = new Color(0f, 0f, 0f, 0.7f);

            // Main panel
            GameObject panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(guideObj.transform, false);

            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(700, 800);

            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

            // Close button (top right)
            GameObject closeBtn = new GameObject("CloseButton");
            closeBtn.transform.SetParent(panelObj.transform, false);
            RectTransform closeBtnRect = closeBtn.AddComponent<RectTransform>();
            closeBtnRect.anchorMin = new Vector2(1f, 1f);
            closeBtnRect.anchorMax = new Vector2(1f, 1f);
            closeBtnRect.pivot = new Vector2(1f, 1f);
            closeBtnRect.anchoredPosition = new Vector2(-20, -20);
            closeBtnRect.sizeDelta = new Vector2(60, 60);

            Image closeBtnImage = closeBtn.AddComponent<Image>();
            closeBtnImage.color = new Color(0.8f, 0.2f, 0.2f, 1f);

            Button closeBtnButton = closeBtn.AddComponent<Button>();

            GameObject closeBtnText = new GameObject("Text");
            closeBtnText.transform.SetParent(closeBtn.transform, false);
            RectTransform closeBtnTextRect = closeBtnText.AddComponent<RectTransform>();
            closeBtnTextRect.anchorMin = Vector2.zero;
            closeBtnTextRect.anchorMax = Vector2.one;
            closeBtnTextRect.offsetMin = Vector2.zero;
            closeBtnTextRect.offsetMax = Vector2.zero;

            Text closeBtnTextComponent = CreateText(closeBtnText, "X", 32, Color.white);
            closeBtnTextComponent.fontStyle = FontStyle.Bold;

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panelObj.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0, -30);
            titleRect.sizeDelta = new Vector2(600, 50);

            Text titleText = CreateText(titleObj, "합성 레시피", 40, new Color(1f, 0.9f, 0.5f));
            titleText.fontStyle = FontStyle.Bold;

            // Page number
            GameObject pageNumObj = new GameObject("PageNumber");
            pageNumObj.transform.SetParent(panelObj.transform, false);
            RectTransform pageNumRect = pageNumObj.AddComponent<RectTransform>();
            pageNumRect.anchorMin = new Vector2(0.5f, 1f);
            pageNumRect.anchorMax = new Vector2(0.5f, 1f);
            pageNumRect.pivot = new Vector2(0.5f, 1f);
            pageNumRect.anchoredPosition = new Vector2(0, -90);
            pageNumRect.sizeDelta = new Vector2(200, 30);

            Text pageNumText = CreateText(pageNumObj, "1 / 6", 24, Color.white);

            // Source unit section (left side)
            GameObject sourceSection = CreateUnitDisplaySection(panelObj.transform, "Source", new Vector2(-200, -250), "소스 유닛");

            // Arrow
            GameObject arrowObj = new GameObject("Arrow");
            arrowObj.transform.SetParent(panelObj.transform, false);
            RectTransform arrowRect = arrowObj.AddComponent<RectTransform>();
            arrowRect.anchorMin = new Vector2(0.5f, 0.5f);
            arrowRect.anchorMax = new Vector2(0.5f, 0.5f);
            arrowRect.pivot = new Vector2(0.5f, 0.5f);
            arrowRect.anchoredPosition = new Vector2(0, -50);
            arrowRect.sizeDelta = new Vector2(100, 50);

            Text arrowText = CreateText(arrowObj, "\u2192", 50, Color.yellow); // Right arrow

            // Result unit section (right side)
            GameObject resultSection = CreateUnitDisplaySection(panelObj.transform, "Result", new Vector2(200, -250), "결과 유닛");

            // Required count text
            GameObject reqCountObj = new GameObject("RequiredCount");
            reqCountObj.transform.SetParent(panelObj.transform, false);
            RectTransform reqCountRect = reqCountObj.AddComponent<RectTransform>();
            reqCountRect.anchorMin = new Vector2(0.5f, 0);
            reqCountRect.anchorMax = new Vector2(0.5f, 0);
            reqCountRect.pivot = new Vector2(0.5f, 0);
            reqCountRect.anchoredPosition = new Vector2(0, 150);
            reqCountRect.sizeDelta = new Vector2(600, 40);

            Text reqCountText = CreateText(reqCountObj, "2개 필요", 28, new Color(1f, 0.8f, 0.3f));
            reqCountText.fontStyle = FontStyle.Bold;

            // Synthesis info text
            GameObject infoObj = new GameObject("SynthesisInfo");
            infoObj.transform.SetParent(panelObj.transform, false);
            RectTransform infoRect = infoObj.AddComponent<RectTransform>();
            infoRect.anchorMin = new Vector2(0.5f, 0);
            infoRect.anchorMax = new Vector2(0.5f, 0);
            infoRect.pivot = new Vector2(0.5f, 0);
            infoRect.anchoredPosition = new Vector2(0, 100);
            infoRect.sizeDelta = new Vector2(600, 60);

            Text infoText = CreateText(infoObj, "같은 유닛 2개를 모아서 합성하세요!", 20, Color.white);

            // Previous page button (left arrow)
            GameObject prevBtn = CreatePageButton(panelObj.transform, "PrevButton", new Vector2(50, 400), "\u25C0");

            // Next page button (right arrow)
            GameObject nextBtn = CreatePageButton(panelObj.transform, "NextButton", new Vector2(650, 400), "\u25B6");

            // Add SynthesisGuideUI component
            SynthesisGuideUI guideUI = guideObj.AddComponent<SynthesisGuideUI>();

            // Wire up references via reflection (since fields are private)
            var guidePanelField = typeof(SynthesisGuideUI).GetField("guidePanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var closeButtonField = typeof(SynthesisGuideUI).GetField("closeButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var prevPageButtonField = typeof(SynthesisGuideUI).GetField("prevPageButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var nextPageButtonField = typeof(SynthesisGuideUI).GetField("nextPageButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var pageNumberTextField = typeof(SynthesisGuideUI).GetField("pageNumberText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var recipeTitleTextField = typeof(SynthesisGuideUI).GetField("recipeTitleText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var sourceUnitIconField = typeof(SynthesisGuideUI).GetField("sourceUnitIcon", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var sourceUnitNameTextField = typeof(SynthesisGuideUI).GetField("sourceUnitNameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var sourceUnitStatsTextField = typeof(SynthesisGuideUI).GetField("sourceUnitStatsText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var resultUnitIconField = typeof(SynthesisGuideUI).GetField("resultUnitIcon", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var resultUnitNameTextField = typeof(SynthesisGuideUI).GetField("resultUnitNameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var resultUnitStatsTextField = typeof(SynthesisGuideUI).GetField("resultUnitStatsText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var requiredCountTextField = typeof(SynthesisGuideUI).GetField("requiredCountText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var synthesisInfoTextField = typeof(SynthesisGuideUI).GetField("synthesisInfoText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            guidePanelField?.SetValue(guideUI, guideObj);
            closeButtonField?.SetValue(guideUI, closeBtnButton);
            prevPageButtonField?.SetValue(guideUI, prevBtn.GetComponent<Button>());
            nextPageButtonField?.SetValue(guideUI, nextBtn.GetComponent<Button>());
            pageNumberTextField?.SetValue(guideUI, pageNumText);
            recipeTitleTextField?.SetValue(guideUI, titleText);

            // Source unit references
            var sourceIcon = sourceSection.transform.Find("Icon")?.GetComponent<Image>();
            var sourceName = sourceSection.transform.Find("Name")?.GetComponent<Text>();
            var sourceStats = sourceSection.transform.Find("Stats")?.GetComponent<Text>();
            sourceUnitIconField?.SetValue(guideUI, sourceIcon);
            sourceUnitNameTextField?.SetValue(guideUI, sourceName);
            sourceUnitStatsTextField?.SetValue(guideUI, sourceStats);

            // Result unit references
            var resultIcon = resultSection.transform.Find("Icon")?.GetComponent<Image>();
            var resultName = resultSection.transform.Find("Name")?.GetComponent<Text>();
            var resultStats = resultSection.transform.Find("Stats")?.GetComponent<Text>();
            resultUnitIconField?.SetValue(guideUI, resultIcon);
            resultUnitNameTextField?.SetValue(guideUI, resultName);
            resultUnitStatsTextField?.SetValue(guideUI, resultStats);

            requiredCountTextField?.SetValue(guideUI, reqCountText);
            synthesisInfoTextField?.SetValue(guideUI, infoText);

            // IMPORTANT: Start with guide hidden
            guideObj.SetActive(false);
        }

        private GameObject CreateUnitDisplaySection(Transform parent, string name, Vector2 position, string label)
        {
            GameObject section = new GameObject(name + "Section");
            section.transform.SetParent(parent, false);

            RectTransform sectionRect = section.AddComponent<RectTransform>();
            sectionRect.anchorMin = new Vector2(0.5f, 0.5f);
            sectionRect.anchorMax = new Vector2(0.5f, 0.5f);
            sectionRect.pivot = new Vector2(0.5f, 0.5f);
            sectionRect.anchoredPosition = position;
            sectionRect.sizeDelta = new Vector2(250, 350);

            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(section.transform, false);
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 1f);
            iconRect.anchorMax = new Vector2(0.5f, 1f);
            iconRect.pivot = new Vector2(0.5f, 1f);
            iconRect.anchoredPosition = new Vector2(0, -10);
            iconRect.sizeDelta = new Vector2(120, 120);

            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.color = Color.white;

            // Name
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(section.transform, false);
            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.5f, 1f);
            nameRect.anchorMax = new Vector2(0.5f, 1f);
            nameRect.pivot = new Vector2(0.5f, 1f);
            nameRect.anchoredPosition = new Vector2(0, -145);
            nameRect.sizeDelta = new Vector2(240, 40);

            Text nameText = CreateText(nameObj, label, 22, Color.white);
            nameText.fontStyle = FontStyle.Bold;

            // Stats
            GameObject statsObj = new GameObject("Stats");
            statsObj.transform.SetParent(section.transform, false);
            RectTransform statsRect = statsObj.AddComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0.5f, 1f);
            statsRect.anchorMax = new Vector2(0.5f, 1f);
            statsRect.pivot = new Vector2(0.5f, 1f);
            statsRect.anchoredPosition = new Vector2(0, -195);
            statsRect.sizeDelta = new Vector2(240, 140);

            Text statsText = CreateText(statsObj, "공격력: ?\n공격속도: ?\n사거리: ?\nDPS: ?", 18, Color.white);

            return section;
        }

        private GameObject CreatePageButton(Transform parent, string name, Vector2 position, string text)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = Vector2.zero;
            btnRect.anchorMax = Vector2.zero;
            btnRect.pivot = new Vector2(0.5f, 0.5f);
            btnRect.anchoredPosition = position;
            btnRect.sizeDelta = new Vector2(80, 80);

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = new Color(0.3f, 0.3f, 0.4f, 1f);

            Button button = btnObj.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            button.colors = colors;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text btnText = CreateText(textObj, text, 36, Color.white);
            btnText.fontStyle = FontStyle.Bold;

            return btnObj;
        }

        private void EnsureGameResultUI()
        {
            GameResultUI resultUI = FindFirstObjectByType<GameResultUI>();
            if (resultUI != null) return;

            // Full-screen overlay for game result
            GameObject resultObj = new GameObject("GameResultUI");
            resultObj.transform.SetParent(mainCanvas.transform, false);

            RectTransform rect = resultObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            CanvasGroup canvasGroup = resultObj.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            // Semi-transparent background
            Image bgImage = resultObj.AddComponent<Image>();
            bgImage.color = new Color(0f, 0f, 0f, 0.85f);

            // Result panel
            GameObject panelObj = new GameObject("ResultPanel");
            panelObj.transform.SetParent(resultObj.transform, false);
            panelObj.SetActive(false);

            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(600, 500);

            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

            // Title text
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(panelObj.transform, false);

            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0, -80);
            titleRect.sizeDelta = new Vector2(500, 100);

            Text titleText = CreateText(titleObj, "게임 오버", 60, Color.white);
            titleText.fontStyle = FontStyle.Bold;

            // Round text
            GameObject roundObj = new GameObject("RoundText");
            roundObj.transform.SetParent(panelObj.transform, false);

            RectTransform roundRect = roundObj.AddComponent<RectTransform>();
            roundRect.anchorMin = new Vector2(0.5f, 0.5f);
            roundRect.anchorMax = new Vector2(0.5f, 0.5f);
            roundRect.anchoredPosition = new Vector2(0, 50);
            roundRect.sizeDelta = new Vector2(500, 60);

            Text roundText = CreateText(roundObj, "도달 라운드: 1", 36, new Color(0.9f, 0.9f, 0.9f));

            // Contribution text
            GameObject contribObj = new GameObject("ContributionText");
            contribObj.transform.SetParent(panelObj.transform, false);

            RectTransform contribRect = contribObj.AddComponent<RectTransform>();
            contribRect.anchorMin = new Vector2(0.5f, 0.5f);
            contribRect.anchorMax = new Vector2(0.5f, 0.5f);
            contribRect.anchoredPosition = new Vector2(0, -30);
            contribRect.sizeDelta = new Vector2(500, 60);

            Text contribText = CreateText(contribObj, "기여도: 0점", 36, new Color(1f, 0.8f, 0.2f));

            // Confirm button
            GameObject btnObj = new GameObject("ConfirmButton");
            btnObj.transform.SetParent(panelObj.transform, false);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0f);
            btnRect.anchorMax = new Vector2(0.5f, 0f);
            btnRect.anchoredPosition = new Vector2(0, 80);
            btnRect.sizeDelta = new Vector2(300, 80);

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = new Color(0.2f, 0.6f, 0.3f);

            Button button = btnObj.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            colors.fadeDuration = 0.1f;
            button.colors = colors;

            // Button text
            GameObject btnTextObj = new GameObject("Text");
            btnTextObj.transform.SetParent(btnObj.transform, false);

            RectTransform btnTextRect = btnTextObj.AddComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.offsetMin = Vector2.zero;
            btnTextRect.offsetMax = Vector2.zero;

            Text btnText = CreateText(btnTextObj, "확인", 40, Color.white);
            btnText.fontStyle = FontStyle.Bold;

            // Add GameResultUI component
            GameResultUI resultUIComponent = resultObj.AddComponent<GameResultUI>();

            // Assign references using reflection
            SetField(resultUIComponent, "canvasGroup", canvasGroup);
            SetField(resultUIComponent, "resultPanel", panelObj);
            SetField(resultUIComponent, "titleText", titleText);
            SetField(resultUIComponent, "roundText", roundText);
            SetField(resultUIComponent, "contributionText", contribText);
            SetField(resultUIComponent, "confirmButton", button);
            SetField(resultUIComponent, "confirmButtonText", btnText);

            Debug.Log("[GameSceneBootstrapper] Created GameResultUI");
        }
        #endregion

        #region Button Logic
        private void OnSummonButtonClicked(Text mainText, Text costText)
        {
            if (GameplayManager.Instance == null) return;

            // Summon allowed in all game states

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

        #region Game Balance Config
        /// <summary>
        /// Load GameBalanceConfig from Resources or create default runtime config.
        /// This is the CENTRAL configuration for all game balance.
        /// </summary>
        private GameBalanceConfig LoadOrCreateGameBalanceConfig()
        {
            GameBalanceConfig config = Resources.Load<GameBalanceConfig>("GameBalanceConfig");

            if (config == null)
            {
                Debug.LogWarning("[GameSceneBootstrapper] GameBalanceConfig not found in Resources! Creating default runtime config...");
                config = CreateDefaultGameBalanceConfig();
            }
            else
            {
                Debug.Log("[GameSceneBootstrapper] GameBalanceConfig loaded from Resources");
            }

            return config;
        }

        /// <summary>
        /// Create default GameBalanceConfig at runtime with default values.
        /// </summary>
        private GameBalanceConfig CreateDefaultGameBalanceConfig()
        {
            GameBalanceConfig config = ScriptableObject.CreateInstance<GameBalanceConfig>();

            // Default values are set in GameBalanceConfig class
            Debug.Log("[GameSceneBootstrapper] Created default GameBalanceConfig");

            return config;
        }

        /// <summary>
        /// Convert GameBalanceConfig.MonsterBalance to MonsterData ScriptableObject.
        /// </summary>
        private MonsterData CreateMonsterDataFromConfig(GameBalanceConfig.MonsterBalance balance)
        {
            MonsterData data = ScriptableObject.CreateInstance<MonsterData>();
            data.name = balance.monsterName;
            data.monsterName = balance.monsterName;
            data.type = balance.type;
            data.maxHealth = balance.maxHealth;
            data.attack = balance.attack;
            data.defense = balance.defense;
            data.moveSpeed = balance.moveSpeed;
            data.goldReward = balance.goldReward;
            data.healthScaling = balance.healthScaling;
            data.defenseScaling = balance.defenseScaling;
            return data;
        }

        /// <summary>
        /// Convert GameBalanceConfig.DifficultyBalance to DifficultyConfig ScriptableObject.
        /// </summary>
        private DifficultyConfig CreateDifficultyConfigFromConfig(GameBalanceConfig.DifficultyBalance balance)
        {
            DifficultyConfig config = ScriptableObject.CreateInstance<DifficultyConfig>();
            SetField(config, "hpCurve", balance.healthScaling);
            SetField(config, "defenseCurve", balance.defenseScaling);
            SetField(config, "baseHpMultiplier", 1f);
            SetField(config, "baseDefenseMultiplier", 1f);
            SetField(config, "maxRounds", balance.maxRounds);
            return config;
        }
        #endregion

        #region Quest System
        private void EnsureQuestManager()
        {
            if (FindFirstObjectByType<QuestManager>() == null)
                new GameObject("QuestManager").AddComponent<QuestManager>();

            // Initialize quests for this game session
            QuestManager.Instance?.InitializeQuests();
        }

        private void EnsureQuestUI()
        {
            QuestUI existingUI = FindFirstObjectByType<QuestUI>();
            if (existingUI != null) return;

            // Full-screen overlay (same pattern as SynthesisGuideUI)
            GameObject questObj = new GameObject("QuestUI");
            questObj.transform.SetParent(mainCanvas.transform, false);

            RectTransform rect = questObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image bgImage = questObj.AddComponent<Image>();
            bgImage.color = new Color(0f, 0f, 0f, 0.7f);

            // Main panel (centered)
            GameObject panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(questObj.transform, false);

            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(700, 900);

            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = GameSceneDesignTokens.QuestPanelBg;

            // Close button (top right)
            GameObject closeBtn = new GameObject("CloseButton");
            closeBtn.transform.SetParent(panelObj.transform, false);
            RectTransform closeBtnRect = closeBtn.AddComponent<RectTransform>();
            closeBtnRect.anchorMin = new Vector2(1f, 1f);
            closeBtnRect.anchorMax = new Vector2(1f, 1f);
            closeBtnRect.pivot = new Vector2(1f, 1f);
            closeBtnRect.anchoredPosition = new Vector2(-20, -20);
            closeBtnRect.sizeDelta = new Vector2(60, 60);

            Image closeBtnImage = closeBtn.AddComponent<Image>();
            closeBtnImage.color = new Color(0.8f, 0.2f, 0.2f, 1f);

            Button closeBtnButton = closeBtn.AddComponent<Button>();

            GameObject closeBtnText = new GameObject("Text");
            closeBtnText.transform.SetParent(closeBtn.transform, false);
            RectTransform closeBtnTextRect = closeBtnText.AddComponent<RectTransform>();
            closeBtnTextRect.anchorMin = Vector2.zero;
            closeBtnTextRect.anchorMax = Vector2.one;
            closeBtnTextRect.offsetMin = Vector2.zero;
            closeBtnTextRect.offsetMax = Vector2.zero;

            Text closeBtnTextComponent = CreateText(closeBtnText, "X", 32, Color.white);
            closeBtnTextComponent.fontStyle = FontStyle.Bold;

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panelObj.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0, -30);
            titleRect.sizeDelta = new Vector2(600, 50);

            Text titleText = CreateText(titleObj, "히든 퀘스트", 40, new Color(1f, 0.85f, 0.3f));
            titleText.fontStyle = FontStyle.Bold;

            // Scroll content area
            GameObject contentArea = new GameObject("ContentArea");
            contentArea.transform.SetParent(panelObj.transform, false);
            RectTransform contentRect = contentArea.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.offsetMin = new Vector2(20, 20);
            contentRect.offsetMax = new Vector2(-20, -100);

            // Vertical layout for quest items
            VerticalLayoutGroup vlg = contentArea.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childAlignment = TextAnchor.UpperCenter;

            // Content size fitter for scrolling
            ContentSizeFitter csf = contentArea.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Add QuestUI component and wire references
            QuestUI questUI = questObj.AddComponent<QuestUI>();
            SetField(questUI, "questPanel", questObj);
            SetField(questUI, "closeButton", closeBtnButton);
            SetField(questUI, "contentParent", contentArea.transform);

            // Start hidden
            questObj.SetActive(false);
        }

        private void EnsureQuestButton()
        {
            // Ensure quest manager and UI exist first
            EnsureQuestManager();
            EnsureQuestUI();

            // Button positioned below SynthesisGuideButton (top-right, below HUD)
            GameObject btnObj = new GameObject("QuestButton");
            btnObj.transform.SetParent(mainCanvas.transform, false);

            float btnSize = GameSceneDesignTokens.UtilityButtonSize;
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(1, 1);
            btnRect.anchorMax = new Vector2(1, 1);
            btnRect.pivot = new Vector2(1, 1);
            // Position below the synthesis guide button (guide is at HudHeight+8, this is at HudHeight+8+btnSize+8)
            btnRect.anchoredPosition = new Vector2(-12, -(GameSceneDesignTokens.HudHeight + 8 + btnSize + 8));
            btnRect.sizeDelta = new Vector2(btnSize, btnSize);

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = GameSceneDesignTokens.QuestBtnBg;

            Button button = btnObj.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            colors.fadeDuration = 0.1f;
            button.colors = colors;

            // Click handler - find QuestUI at click time (may be inactive)
            button.onClick.AddListener(() => {
                QuestUI questUI = FindFirstObjectByType<QuestUI>(FindObjectsInactive.Include);
                if (questUI != null)
                {
                    questUI.Show();
                }
            });

            // Button icon ("Q" for Quest)
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text btnText = CreateText(textObj, "Q", 28, GameSceneDesignTokens.ButtonText);
            btnText.fontStyle = FontStyle.Bold;

            Outline outline = textObj.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.5f);
            outline.effectDistance = new Vector2(2, -2);
        }
        #endregion

        #region Opponent Status UI (Multiplayer)
        private void EnsureOpponentStatusUI()
        {
            // Only create in multiplayer mode
            if (MultiplayerManager.Instance == null || !MultiplayerManager.Instance.IsMultiplayer)
                return;

            if (FindFirstObjectByType<OpponentStatusUI>() != null)
                return;

            OpponentStatusUI.CreateOnCanvas(mainCanvas, defaultFont);
            Debug.Log("[GameSceneBootstrapper] Created OpponentStatusUI for multiplayer");
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
            t.raycastTarget = false; // Prevent text from blocking clicks on game objects
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
