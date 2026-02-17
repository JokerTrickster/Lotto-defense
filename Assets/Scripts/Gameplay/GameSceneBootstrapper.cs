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
    public partial class GameSceneBootstrapper : MonoBehaviour
    {
        [Header("References (Auto-created if null)")]
        [SerializeField] private Canvas mainCanvas;

        private Transform safeAreaRoot;
        private Font defaultFont;
        private GameBalanceConfig balanceConfig;

        private void Awake()
        {

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

            // Load or create game balance config (CENTRAL CONFIG)
            balanceConfig = LoadOrCreateGameBalanceConfig();

            EnsureMainCanvas();
            EnsureEventSystem();

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
            EnsureSynthesisGuideButton();
            EnsureQuestButton();
            EnsureGameResultUI();
            EnsureOpponentStatusUI();

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

            // Create SafeArea container inside canvas
            Transform existingSafe = mainCanvas.transform.Find("SafeArea");
            if (existingSafe != null)
            {
                safeAreaRoot = existingSafe;
            }
            else
            {
                GameObject safeAreaObj = new GameObject("SafeArea");
                safeAreaObj.transform.SetParent(mainCanvas.transform, false);

                RectTransform safeRect = safeAreaObj.AddComponent<RectTransform>();
                safeRect.anchorMin = Vector2.zero;
                safeRect.anchorMax = Vector2.one;
                safeRect.offsetMin = Vector2.zero;
                safeRect.offsetMax = Vector2.zero;

                safeAreaObj.AddComponent<LottoDefense.UI.SafeAreaAdapter>();
                safeAreaRoot = safeAreaObj.transform;
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

            // Set spawn rate from balance config
            float spawnInterval = 1f / balanceConfig.gameRules.spawnRate;
            SetField(manager, "spawnInterval", spawnInterval);

            // Load and pass RoundConfig so MonsterManager can use per-round monster configs
            RoundConfig roundConfig = Resources.Load<RoundConfig>("RoundConfig");
            if (roundConfig != null)
            {
                SetField(manager, "roundConfig", roundConfig);
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

            // Pass phase timing from GameBalanceConfig game rules
            SetField(manager, "preparationDuration", (float)balanceConfig.gameRules.preparationTime);
            SetField(manager, "combatDuration", (float)balanceConfig.gameRules.combatTime);

            // Load and pass RoundConfig so RoundManager can use per-round definitions
            RoundConfig roundConfig = Resources.Load<RoundConfig>("RoundConfig");
            if (roundConfig != null)
            {
                SetField(manager, "roundConfig", roundConfig);
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
            }
        }

        private void EnsureVFXManager()
        {
            if (FindFirstObjectByType<LottoDefense.VFX.VFXManager>() == null)
            {
                GameObject obj = new GameObject("VFXManager");
                obj.AddComponent<LottoDefense.VFX.VFXManager>();
            }
        }
        #endregion

        #region Game Balance Config
        private GameBalanceConfig LoadOrCreateGameBalanceConfig()
        {
            GameBalanceConfig config = Resources.Load<GameBalanceConfig>("GameBalanceConfig");

            if (config == null)
            {
                // Create default runtime config silently
                config = CreateDefaultGameBalanceConfig();
            }
            else
            {
            }

            return config;
        }

        private GameBalanceConfig CreateDefaultGameBalanceConfig()
        {
            GameBalanceConfig config = ScriptableObject.CreateInstance<GameBalanceConfig>();

            // Default values are set in GameBalanceConfig class

            return config;
        }

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

        #region Utilities
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
            t.raycastTarget = false;
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
