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
            Debug.Log("[GameSceneBootstrapper] Starting game scene initialization...");

            // Load built-in Arial font — required for legacy Text to render
            defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (defaultFont == null)
                Debug.LogError("[GameSceneBootstrapper] Failed to load Arial font!");

            EnsureMainCanvas();

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

            GameObject countdownObj = new GameObject("CountdownUI");
            countdownObj.transform.SetParent(mainCanvas.transform, false);

            RectTransform rect = countdownObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            CanvasGroup canvasGroup = countdownObj.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            GameObject textObj = new GameObject("CountdownText");
            textObj.transform.SetParent(countdownObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = new Vector2(400, 300);

            Text countdownText = CreateText(textObj, "3", 144, Color.white);

            countdown = countdownObj.AddComponent<CountdownUI>();
            SetField(countdown, "countdownText", countdownText);
            SetField(countdown, "canvasGroup", canvasGroup);
        }
        #endregion

        #region HUD
        private void EnsureGameHUD()
        {
            if (FindFirstObjectByType<GameHUD>() != null) return;

            // Root container anchored to top
            GameObject hudObj = new GameObject("GameHUD");
            hudObj.transform.SetParent(mainCanvas.transform, false);

            RectTransform hudRect = hudObj.AddComponent<RectTransform>();
            hudRect.anchorMin = new Vector2(0, 1);
            hudRect.anchorMax = new Vector2(1, 1);
            hudRect.pivot = new Vector2(0.5f, 1);
            hudRect.anchoredPosition = Vector2.zero;
            hudRect.sizeDelta = new Vector2(0, 130);

            var vlayout = hudObj.AddComponent<VerticalLayoutGroup>();
            vlayout.padding = new RectOffset(20, 20, 10, 6);
            vlayout.spacing = 4;
            vlayout.childForceExpandWidth = true;
            vlayout.childForceExpandHeight = false;
            vlayout.childControlWidth = true;
            vlayout.childControlHeight = true;

            var bg = hudObj.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.75f);

            // Row 1: Round | Phase | Time
            GameObject row1 = CreateHUDRow(hudObj.transform, "Row1");
            Text roundText = CreateHUDLabel(row1.transform, "RoundText", "R1", 28);
            Text phaseText = CreateHUDLabel(row1.transform, "PhaseText", "COUNTDOWN", 24);
            Text timeText = CreateHUDLabel(row1.transform, "TimeText", "00:00", 28);

            // Row 2: Life | Gold | Monsters | Units
            GameObject row2 = CreateHUDRow(hudObj.transform, "Row2");
            Text lifeText = CreateHUDLabel(row2.transform, "LifeText", "♥10", 24, new Color(1f, 0.3f, 0.3f));
            Text goldText = CreateHUDLabel(row2.transform, "GoldText", "G:30", 24, new Color(1f, 0.84f, 0f));
            Text monsterText = CreateHUDLabel(row2.transform, "MonsterText", "M:0", 24);
            Text unitText = CreateHUDLabel(row2.transform, "UnitText", "U:0", 24);

            GameHUD hud = hudObj.AddComponent<GameHUD>();
            SetField(hud, "roundText", roundText);
            SetField(hud, "phaseText", phaseText);
            SetField(hud, "timeText", timeText);
            SetField(hud, "monsterText", monsterText);
            SetField(hud, "goldText", goldText);
            SetField(hud, "unitText", unitText);
            SetField(hud, "lifeText", lifeText);
        }

        private GameObject CreateHUDRow(Transform parent, string name)
        {
            GameObject row = new GameObject(name);
            row.transform.SetParent(parent, false);

            RectTransform rect = row.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 40);

            var layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 12;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childAlignment = TextAnchor.MiddleCenter;

            return row;
        }

        private Text CreateHUDLabel(Transform parent, string name, string text, int fontSize, Color? color = null)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            obj.AddComponent<RectTransform>();

            Text t = CreateText(obj, text, fontSize, color ?? Color.white);
            t.fontStyle = FontStyle.Bold;

            return t;
        }
        #endregion

        #region Buttons
        private void EnsureSummonButton()
        {
            GameObject btnObj = new GameObject("SummonButton");
            btnObj.transform.SetParent(mainCanvas.transform, false);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.05f, 0);
            btnRect.anchorMax = new Vector2(0.95f, 0);
            btnRect.pivot = new Vector2(0.5f, 0);
            btnRect.anchoredPosition = new Vector2(0, 130);
            btnRect.sizeDelta = new Vector2(0, 90);

            var btnImage = btnObj.AddComponent<Image>();
            btnImage.color = new Color(0.1f, 0.6f, 0.1f, 0.95f);

            var button = btnObj.AddComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = new Color(0.15f, 0.7f, 0.15f);
            colors.pressedColor = new Color(0.08f, 0.4f, 0.08f);
            button.colors = colors;

            Text btnText = CreateButtonText(btnObj, "소환 (5G)", 36);
            button.onClick.AddListener(() => OnSummonButtonClicked(btnText));
        }

        private void EnsureBackToMenuButton()
        {
            GameObject btnObj = new GameObject("BackToMenuButton");
            btnObj.transform.SetParent(mainCanvas.transform, false);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.15f, 0);
            btnRect.anchorMax = new Vector2(0.85f, 0);
            btnRect.pivot = new Vector2(0.5f, 0);
            btnRect.anchoredPosition = new Vector2(0, 30);
            btnRect.sizeDelta = new Vector2(0, 70);

            var btnImage = btnObj.AddComponent<Image>();
            btnImage.color = new Color(0.3f, 0.3f, 0.55f, 0.9f);

            var button = btnObj.AddComponent<Button>();
            var navigator = btnObj.AddComponent<SceneNavigator>();
            button.onClick.AddListener(navigator.LoadMainGame);

            CreateButtonText(btnObj, "메인 메뉴로", 28);
        }

        private Text CreateButtonText(GameObject parent, string text, int fontSize)
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(parent.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text t = CreateText(textObj, text, fontSize, Color.white);
            t.fontStyle = FontStyle.Bold;
            return t;
        }
        #endregion

        #region Button Logic
        private void OnSummonButtonClicked(Text btnText)
        {
            if (GameplayManager.Instance == null) return;

            if (GameplayManager.Instance.CurrentState != GameState.Preparation)
            {
                StartCoroutine(FlashButtonText(btnText, "준비 단계에서만!"));
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
                StartCoroutine(FlashButtonText(btnText, "골드 부족!"));
                return;
            }

            UnitData drawnUnit = unitMgr.DrawUnit();
            if (drawnUnit != null)
            {
                UnitPlacementManager placementMgr = FindFirstObjectByType<UnitPlacementManager>();
                if (placementMgr != null)
                {
                    placementMgr.SelectUnitForPlacement(drawnUnit);
                    StartCoroutine(FlashButtonText(btnText, $"{drawnUnit.unitName} 배치하세요!"));
                }
            }
        }

        private System.Collections.IEnumerator FlashButtonText(Text btnText, string message)
        {
            string original = "소환 (5G)";
            btnText.text = message;
            yield return new WaitForSeconds(1.5f);
            btnText.text = original;
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
