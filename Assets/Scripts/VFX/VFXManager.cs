using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using LottoDefense.Units;
using LottoDefense.Monsters;
using LottoDefense.Gameplay;
using LottoDefense.Grid;

namespace LottoDefense.VFX
{
    /// <summary>
    /// Singleton manager coordinating all visual effects in the game.
    /// Manages object pools for damage numbers and floating text.
    /// Provides attack animations, death effects, and UI feedback.
    /// </summary>
    public class VFXManager : MonoBehaviour
    {
        #region Singleton
        private static VFXManager _instance;

        /// <summary>
        /// Global access point for the VFXManager singleton.
        /// </summary>
        public static VFXManager Instance
        {
            get
            {
                if (GameplayManager.IsCleaningUp) return null;

                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<VFXManager>();

                    if (_instance == null)
                    {
                        GameObject go = new GameObject("VFXManager");
                        _instance = go.AddComponent<VFXManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Inspector Fields
        [Header("Prefab References")]
        [SerializeField] private GameObject damageNumberPrefab;
        [SerializeField] private GameObject floatingTextPrefab;

        [Header("Pool Settings")]
        [SerializeField] private int damageNumberPoolSize = 30;
        [SerializeField] private int floatingTextPoolSize = 20;

        [Header("Animation Settings")]
        [SerializeField] private float attackAnimationDuration = 0.3f;
        [SerializeField] private float attackPunchScale = 1.2f;
        [SerializeField] private Color attackFlashColor = Color.white;
        [SerializeField] private float monsterDeathFadeDuration = 0.5f;

        [Header("Gold Popup Settings")]
        [SerializeField] private Color goldColor = new Color(1f, 0.84f, 0f); // Gold color
        #endregion

        #region Private Fields
        private Queue<DamageNumberController> damageNumberPool = new Queue<DamageNumberController>();
        private Queue<FloatingTextController> floatingTextPool = new Queue<FloatingTextController>();
        private Transform poolParent;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize VFXManager and create object pools.
        /// </summary>
        private void Initialize()
        {
            // Create parent for pooled objects
            poolParent = new GameObject("VFX_Pool").transform;
            poolParent.SetParent(transform);

            // Create default prefabs if not assigned
            if (damageNumberPrefab == null)
            {
                damageNumberPrefab = CreateDefaultDamageNumberPrefab();
            }

            if (floatingTextPrefab == null)
            {
                floatingTextPrefab = CreateDefaultFloatingTextPrefab();
            }

            // Initialize pools
            InitializePools();

            Debug.Log("[VFXManager] Initialized with pools - DamageNumbers: " + damageNumberPoolSize + ", FloatingText: " + floatingTextPoolSize);
        }

        /// <summary>
        /// Initialize object pools for damage numbers and floating text.
        /// </summary>
        private void InitializePools()
        {
            // Create damage number pool
            for (int i = 0; i < damageNumberPoolSize; i++)
            {
                CreateDamageNumber();
            }

            // Create floating text pool
            for (int i = 0; i < floatingTextPoolSize; i++)
            {
                CreateFloatingText();
            }
        }

        /// <summary>
        /// Create a new damage number and add to pool.
        /// </summary>
        private void CreateDamageNumber()
        {
            GameObject obj = Instantiate(damageNumberPrefab, poolParent);
            DamageNumberController controller = obj.GetComponent<DamageNumberController>();

            if (controller == null)
            {
                controller = obj.AddComponent<DamageNumberController>();
            }

            controller.ResetForPool();
            damageNumberPool.Enqueue(controller);
        }

        /// <summary>
        /// Create a new floating text and add to pool.
        /// </summary>
        private void CreateFloatingText()
        {
            GameObject obj = Instantiate(floatingTextPrefab, poolParent);
            FloatingTextController controller = obj.GetComponent<FloatingTextController>();

            if (controller == null)
            {
                controller = obj.AddComponent<FloatingTextController>();
            }

            controller.ResetForPool();
            floatingTextPool.Enqueue(controller);
        }
        #endregion

        #region Damage Numbers
        /// <summary>
        /// Show damage number at specified position.
        /// </summary>
        /// <param name="worldPosition">World position to display at</param>
        /// <param name="damage">Damage amount</param>
        /// <param name="isCritical">Whether this is a critical hit</param>
        public void ShowDamageNumber(Vector3 worldPosition, int damage, bool isCritical = false)
        {
            DamageNumberController controller = GetDamageNumber();
            if (controller != null)
            {
                controller.Show(worldPosition, damage, isCritical);
            }
        }

        /// <summary>
        /// Get damage number from pool or create new one if pool is empty.
        /// </summary>
        private DamageNumberController GetDamageNumber()
        {
            if (damageNumberPool.Count > 0)
            {
                return damageNumberPool.Dequeue();
            }

            // Pool exhausted, create new one
            Debug.LogWarning("[VFXManager] Damage number pool exhausted, creating new instance");
            GameObject obj = Instantiate(damageNumberPrefab, poolParent);
            return obj.GetComponent<DamageNumberController>();
        }

        /// <summary>
        /// Return damage number to pool.
        /// </summary>
        public void ReturnDamageNumber(DamageNumberController controller)
        {
            if (controller != null)
            {
                controller.ResetForPool();
                damageNumberPool.Enqueue(controller);
            }
        }
        #endregion

        #region Floating Text
        /// <summary>
        /// Show floating text with custom message and color.
        /// </summary>
        /// <param name="worldPosition">World position to display at</param>
        /// <param name="message">Message to display</param>
        /// <param name="color">Text color</param>
        public void ShowFloatingText(Vector3 worldPosition, string message, Color color)
        {
            Debug.Log($"[VFXManager] 🎯 ShowFloatingText called: message='{message}'");
            
            FloatingTextController controller = GetFloatingText();
            if (controller != null)
            {
                Debug.Log($"[VFXManager] ✅ Got FloatingTextController, calling Show");
                controller.Show(worldPosition, message, color);
            }
            else
            {
                Debug.LogError($"[VFXManager] ❌ FloatingTextController is NULL! Cannot show message: '{message}'");
            }
        }

        /// <summary>
        /// Show gold gain popup.
        /// </summary>
        /// <param name="worldPosition">World position to display at</param>
        /// <param name="goldAmount">Amount of gold gained</param>
        public void ShowGoldPopup(Vector3 worldPosition, int goldAmount)
        {
            string message = $"+{goldAmount} Gold";
            ShowFloatingText(worldPosition, message, goldColor);
        }

        /// <summary>
        /// Get floating text from pool or create new one if pool is empty.
        /// </summary>
        private FloatingTextController GetFloatingText()
        {
            if (floatingTextPool.Count > 0)
            {
                return floatingTextPool.Dequeue();
            }

            // Pool exhausted, create new one
            Debug.LogWarning("[VFXManager] Floating text pool exhausted, creating new instance");
            GameObject obj = Instantiate(floatingTextPrefab, poolParent);
            return obj.GetComponent<FloatingTextController>();
        }

        /// <summary>
        /// Return floating text to pool.
        /// </summary>
        public void ReturnFloatingText(FloatingTextController controller)
        {
            if (controller != null)
            {
                controller.ResetForPool();
                floatingTextPool.Enqueue(controller);
            }
        }
        #endregion

        #region Attack Animations
        /// <summary>
        /// Play attack animation for unit attacking a monster.
        /// Combines scale punch and color flash effects.
        /// </summary>
        /// <param name="unit">Unit performing the attack</param>
        /// <param name="target">Target monster</param>
        public void PlayAttackAnimation(Unit unit, Monster target)
        {
            if (unit == null || target == null)
                return;

            StartCoroutine(AttackAnimationRoutine(unit, target));
        }

        /// <summary>
        /// Attack animation coroutine.
        /// </summary>
        private IEnumerator AttackAnimationRoutine(Unit unit, Monster target)
        {
            if (unit == null || target == null)
                yield break;

            // Get sprite renderer
            SpriteRenderer unitSprite = unit.GetComponent<SpriteRenderer>();
            if (unitSprite == null)
                yield break;

            // Store original values
            Color originalColor = unitSprite.color;

            // Start concurrent animations
            Coroutine scaleAnimation = StartCoroutine(AnimationHelper.ScalePunch(unit.transform, attackPunchScale, attackAnimationDuration));
            Coroutine flashAnimation = StartCoroutine(AnimationHelper.FlashColor(unitSprite, attackFlashColor, attackAnimationDuration * 0.5f, 2));

            // Wait for animations to complete
            yield return scaleAnimation;
        }
        #endregion

        #region Monster Death Effects
        /// <summary>
        /// Play death effect for monster.
        /// Combines fade and scale down animation.
        /// </summary>
        /// <param name="monster">Monster that died</param>
        public void PlayMonsterDeathEffect(Monster monster)
        {
            if (monster == null)
                return;

            StartCoroutine(MonsterDeathRoutine(monster));
        }

        /// <summary>
        /// Monster death animation coroutine.
        /// </summary>
        private IEnumerator MonsterDeathRoutine(Monster monster)
        {
            if (monster == null)
                yield break;

            SpriteRenderer sprite = monster.GetComponent<SpriteRenderer>();
            if (sprite == null)
                yield break;

            // Concurrent fade and scale down
            Coroutine fadeCoroutine = StartCoroutine(AnimationHelper.FadeTo(sprite, 0f, monsterDeathFadeDuration));
            Coroutine scaleCoroutine = StartCoroutine(AnimationHelper.ScaleTo(monster.transform, Vector3.zero, monsterDeathFadeDuration));

            // Wait for animations
            yield return fadeCoroutine;

            // Monster object will be returned to pool by MonsterManager
        }
        #endregion

        #region Unit Placement Effects
        /// <summary>
        /// Play placement animation for newly placed unit.
        /// </summary>
        /// <param name="unit">Unit that was placed</param>
        public void PlayUnitPlacementEffect(Unit unit)
        {
            if (unit == null)
                return;

            StartCoroutine(UnitPlacementRoutine(unit));
        }

        /// <summary>
        /// Unit placement animation coroutine.
        /// </summary>
        private IEnumerator UnitPlacementRoutine(Unit unit)
        {
            if (unit == null)
                yield break;

            // Start at smaller scale
            unit.transform.localScale = Vector3.zero;

            // Scale up with bounce
            yield return AnimationHelper.ScaleTo(unit.transform, Vector3.one, 0.3f, AnimationHelper.GetBounceCurve());
        }
        #endregion

        #region Default Prefab Creation
        /// <summary>
        /// Get built-in font with fallback for different Unity versions.
        /// </summary>
        private Font GetBuiltinFont()
        {
            // Unity 2022+ uses LegacyRuntime.ttf
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                // Fallback to Arial.ttf for older Unity versions
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            if (font == null)
            {
                Debug.LogError("[VFXManager] Failed to load built-in font!");
            }
            return font;
        }

        /// <summary>
        /// Create default damage number prefab if none is assigned.
        /// </summary>
        private GameObject CreateDefaultDamageNumberPrefab()
        {
            GameObject prefab = new GameObject("DamageNumber");

            // Add Canvas
            Canvas canvas = prefab.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // Add CanvasGroup
            CanvasGroup canvasGroup = prefab.AddComponent<CanvasGroup>();

            // Add RectTransform (automatically added with Canvas)
            RectTransform rectTransform = prefab.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(100, 50);

            // Add Text with font
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(prefab.transform);
            Text text = textObj.AddComponent<Text>();
            text.font = GetBuiltinFont();
            text.fontSize = 36;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            // Add controller
            DamageNumberController controller = prefab.AddComponent<DamageNumberController>();

            // Use reflection to set serialized fields (in real implementation, this would be done in editor)
            var textField = controller.GetType().GetField("damageText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (textField != null)
                textField.SetValue(controller, text);

            var canvasGroupField = controller.GetType().GetField("canvasGroup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (canvasGroupField != null)
                canvasGroupField.SetValue(controller, canvasGroup);

            prefab.SetActive(false);
            return prefab;
        }

        /// <summary>
        /// Create default floating text prefab if none is assigned.
        /// </summary>
        private GameObject CreateDefaultFloatingTextPrefab()
        {
            GameObject prefab = new GameObject("FloatingText");

            // Add Canvas
            Canvas canvas = prefab.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10000; // Very high to be above everything

            // Add CanvasGroup
            CanvasGroup canvasGroup = prefab.AddComponent<CanvasGroup>();

            // Add RectTransform (automatically added with Canvas)
            RectTransform rectTransform = prefab.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(150, 50);

            // Add Text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(prefab.transform);
            Text text = textObj.AddComponent<Text>();
            text.font = GetBuiltinFont();
            text.fontSize = 32;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            // Add controller
            FloatingTextController controller = prefab.AddComponent<FloatingTextController>();

            // Use reflection to set serialized fields
            var textField = controller.GetType().GetField("messageText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (textField != null)
                textField.SetValue(controller, text);

            var canvasGroupField = controller.GetType().GetField("canvasGroup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (canvasGroupField != null)
                canvasGroupField.SetValue(controller, canvasGroup);

            prefab.SetActive(false);
            return prefab;
        }
        #endregion

        #region Boss Effects
        /// <summary>
        /// Show boss warning: full-screen red flash overlay + large warning text + camera shake.
        /// </summary>
        public void ShowBossWarning()
        {
            StartCoroutine(BossWarningRoutine());
        }

        private IEnumerator BossWarningRoutine()
        {
            Debug.Log("[VFXManager] BOSS WARNING!");

            // Create full-screen overlay on GameCanvas
            Canvas gameCanvas = null;
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas c in canvases)
            {
                if (c.gameObject.name == "GameCanvas")
                {
                    gameCanvas = c;
                    break;
                }
            }

            GameObject overlay = null;
            UnityEngine.UI.Image overlayImg = null;
            UnityEngine.UI.Text warningText = null;

            if (gameCanvas != null)
            {
                // Red flash overlay
                overlay = new GameObject("BossWarningOverlay");
                overlay.transform.SetParent(gameCanvas.transform, false);
                RectTransform overlayRect = overlay.AddComponent<RectTransform>();
                overlayRect.anchorMin = Vector2.zero;
                overlayRect.anchorMax = Vector2.one;
                overlayRect.offsetMin = Vector2.zero;
                overlayRect.offsetMax = Vector2.zero;

                overlayImg = overlay.AddComponent<UnityEngine.UI.Image>();
                overlayImg.color = new Color(0.6f, 0f, 0f, 0f);
                overlayImg.raycastTarget = false;

                // Warning text
                GameObject textObj = new GameObject("WarningText");
                textObj.transform.SetParent(overlay.transform, false);
                RectTransform textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = new Vector2(0, 0.35f);
                textRect.anchorMax = new Vector2(1, 0.65f);
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;

                warningText = textObj.AddComponent<UnityEngine.UI.Text>();
                warningText.text = "BOSS";
                warningText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                warningText.fontSize = 120;
                warningText.fontStyle = FontStyle.Bold;
                warningText.color = new Color(1f, 0.2f, 0.1f, 0f);
                warningText.alignment = TextAnchor.MiddleCenter;
                warningText.horizontalOverflow = HorizontalWrapMode.Overflow;
                warningText.verticalOverflow = VerticalWrapMode.Overflow;
                warningText.raycastTarget = false;

                UnityEngine.UI.Outline textOutline = textObj.AddComponent<UnityEngine.UI.Outline>();
                textOutline.effectColor = new Color(0f, 0f, 0f, 0.8f);
                textOutline.effectDistance = new Vector2(3, -3);
            }

            Camera mainCamera = Camera.main;
            Vector3 originalCamPos = mainCamera != null ? mainCamera.transform.position : Vector3.zero;

            // Phase 1: Red flash in (0.3s) with text fade in
            float flashDuration = 0.3f;
            float elapsed = 0f;
            while (elapsed < flashDuration)
            {
                float t = elapsed / flashDuration;
                if (overlayImg != null) overlayImg.color = new Color(0.6f, 0f, 0f, t * 0.5f);
                if (warningText != null) warningText.color = new Color(1f, 0.2f, 0.1f, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Phase 2: Hold + pulsing text + camera shake (1.2s)
            float holdDuration = 1.2f;
            elapsed = 0f;
            while (elapsed < holdDuration)
            {
                float t = elapsed / holdDuration;

                // Pulsing text scale
                if (warningText != null)
                {
                    float pulse = 1f + Mathf.Sin(elapsed * 8f) * 0.15f;
                    warningText.transform.localScale = Vector3.one * pulse;
                }

                // Camera shake (stronger)
                if (mainCamera != null)
                {
                    float shakeIntensity = 0.15f * (1f - t * 0.5f);
                    float offsetX = Random.Range(-shakeIntensity, shakeIntensity);
                    float offsetY = Random.Range(-shakeIntensity, shakeIntensity);
                    mainCamera.transform.position = originalCamPos + new Vector3(offsetX, offsetY, 0f);
                }

                // Overlay red pulse
                if (overlayImg != null)
                {
                    float overlayAlpha = 0.5f + Mathf.Sin(elapsed * 6f) * 0.15f;
                    overlayImg.color = new Color(0.6f, 0f, 0f, overlayAlpha);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Phase 3: Fade out (0.4s)
            float fadeOutDuration = 0.4f;
            elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                float t = elapsed / fadeOutDuration;
                if (overlayImg != null) overlayImg.color = new Color(0.6f, 0f, 0f, 0.5f * (1f - t));
                if (warningText != null) warningText.color = new Color(1f, 0.2f, 0.1f, 1f - t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Restore camera and cleanup
            if (mainCamera != null) mainCamera.transform.position = originalCamPos;
            if (overlay != null) Destroy(overlay);
        }

        /// <summary>
        /// Show boss spawn effect: golden flash + expanding ring at spawn position.
        /// </summary>
        public void ShowBossSpawnEffect(Vector3 worldPosition)
        {
            StartCoroutine(BossSpawnEffectRoutine(worldPosition));
        }

        private IEnumerator BossSpawnEffectRoutine(Vector3 worldPosition)
        {
            Debug.Log($"[VFXManager] Boss spawn effect at {worldPosition}");

            // Create expanding ring using SpriteRenderer
            GameObject ring = new GameObject("BossSpawnRing");
            ring.transform.position = worldPosition;
            SpriteRenderer ringRenderer = ring.AddComponent<SpriteRenderer>();
            ringRenderer.sortingOrder = 50;

            // Create ring texture (circle outline)
            Texture2D ringTex = new Texture2D(64, 64);
            Color[] pixels = new Color[64 * 64];
            Vector2 center = new Vector2(32, 32);
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist > 26f && dist < 31f)
                        pixels[y * 64 + x] = Color.white;
                    else
                        pixels[y * 64 + x] = Color.clear;
                }
            }
            ringTex.SetPixels(pixels);
            ringTex.Apply();
            ringTex.filterMode = FilterMode.Bilinear;

            ringRenderer.sprite = Sprite.Create(ringTex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64f);
            ringRenderer.color = new Color(1f, 0.8f, 0.2f, 1f); // Gold

            // Show boss title
            ShowFloatingText(worldPosition + Vector3.up * 0.8f, "BOSS", new Color(1f, 0.8f, 0.2f));

            // Animate: expand ring and fade out (0.8s)
            float duration = 0.8f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float scale = 0.3f + t * 2f;
                ring.transform.localScale = Vector3.one * scale;
                ringRenderer.color = new Color(1f, 0.8f, 0.2f, 1f - t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(ring);
        }
        #endregion

        #region Quest Effects
        /// <summary>
        /// Show quest completed banner: slide down from top with gold background, pulse, then slide out.
        /// </summary>
        public void ShowQuestCompletedEffect(string hintText)
        {
            StartCoroutine(QuestCompletedRoutine(hintText));
        }

        private IEnumerator QuestCompletedRoutine(string hintText)
        {
            Debug.Log($"[VFXManager] Quest completed effect: {hintText}");

            // Find GameCanvas
            Canvas gameCanvas = null;
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas c in canvases)
            {
                if (c.gameObject.name == "GameCanvas")
                {
                    gameCanvas = c;
                    break;
                }
            }

            if (gameCanvas == null) yield break;

            // Banner container
            GameObject banner = new GameObject("QuestCompletedBanner");
            banner.transform.SetParent(gameCanvas.transform, false);
            RectTransform bannerRect = banner.AddComponent<RectTransform>();
            bannerRect.anchorMin = new Vector2(0, 1);
            bannerRect.anchorMax = new Vector2(1, 1);
            bannerRect.pivot = new Vector2(0.5f, 1f);
            bannerRect.sizeDelta = new Vector2(0, 120);
            bannerRect.anchoredPosition = new Vector2(0, 120); // Start above screen

            // Gold background
            Image bannerBg = banner.AddComponent<Image>();
            bannerBg.color = new Color(1f, 0.84f, 0f, 0.9f);
            bannerBg.raycastTarget = false;

            // Title text: "퀘스트 달성!"
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(banner.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.45f);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = "퀘스트 달성!";
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 42;
            titleText.fontStyle = FontStyle.Bold;
            titleText.color = new Color(0.3f, 0.15f, 0f, 1f);
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.horizontalOverflow = HorizontalWrapMode.Overflow;
            titleText.verticalOverflow = VerticalWrapMode.Overflow;
            titleText.raycastTarget = false;

            Outline titleOutline = titleObj.AddComponent<Outline>();
            titleOutline.effectColor = new Color(1f, 1f, 1f, 0.5f);
            titleOutline.effectDistance = new Vector2(2, -2);

            // Hint text
            GameObject hintObj = new GameObject("HintText");
            hintObj.transform.SetParent(banner.transform, false);
            RectTransform hintRect = hintObj.AddComponent<RectTransform>();
            hintRect.anchorMin = new Vector2(0, 0);
            hintRect.anchorMax = new Vector2(1, 0.45f);
            hintRect.offsetMin = new Vector2(10, 0);
            hintRect.offsetMax = new Vector2(-10, 0);

            Text hintTextComp = hintObj.AddComponent<Text>();
            hintTextComp.text = hintText;
            hintTextComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hintTextComp.fontSize = 26;
            hintTextComp.color = new Color(0.4f, 0.2f, 0f, 1f);
            hintTextComp.alignment = TextAnchor.MiddleCenter;
            hintTextComp.horizontalOverflow = HorizontalWrapMode.Wrap;
            hintTextComp.verticalOverflow = VerticalWrapMode.Overflow;
            hintTextComp.raycastTarget = false;

            // Camera shake setup
            Camera mainCamera = Camera.main;
            Vector3 originalCamPos = mainCamera != null ? mainCamera.transform.position : Vector3.zero;

            // Phase 1: Slide down (0.3s) + light camera shake
            float slideDuration = 0.3f;
            float elapsed = 0f;
            while (elapsed < slideDuration)
            {
                float t = elapsed / slideDuration;
                float easeOut = 1f - (1f - t) * (1f - t); // ease-out quad
                bannerRect.anchoredPosition = new Vector2(0, 120f * (1f - easeOut));

                // Light camera shake
                if (mainCamera != null)
                {
                    float shakeIntensity = 0.05f * (1f - t);
                    float offsetX = Random.Range(-shakeIntensity, shakeIntensity);
                    float offsetY = Random.Range(-shakeIntensity, shakeIntensity);
                    mainCamera.transform.position = originalCamPos + new Vector3(offsetX, offsetY, 0f);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }
            bannerRect.anchoredPosition = Vector2.zero;
            if (mainCamera != null) mainCamera.transform.position = originalCamPos;

            // Phase 2: Hold + pulse (1.4s)
            float holdDuration = 1.4f;
            elapsed = 0f;
            while (elapsed < holdDuration)
            {
                float pulse = 1f + Mathf.Sin(elapsed * 6f) * 0.05f;
                titleObj.transform.localScale = Vector3.one * pulse;
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Phase 3: Slide up out (0.3s)
            float slideOutDuration = 0.3f;
            elapsed = 0f;
            while (elapsed < slideOutDuration)
            {
                float t = elapsed / slideOutDuration;
                float easeIn = t * t; // ease-in quad
                bannerRect.anchoredPosition = new Vector2(0, 120f * easeIn);
                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(banner);
        }

        /// <summary>
        /// Show reward claimed effect: gold flash + large gold text + expanding ring.
        /// </summary>
        public void ShowRewardClaimedEffect(int goldAmount)
        {
            StartCoroutine(RewardClaimedRoutine(goldAmount));
        }

        private IEnumerator RewardClaimedRoutine(int goldAmount)
        {
            Debug.Log($"[VFXManager] Reward claimed effect: +{goldAmount} gold");

            // Find GameCanvas
            Canvas gameCanvas = null;
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas c in canvases)
            {
                if (c.gameObject.name == "GameCanvas")
                {
                    gameCanvas = c;
                    break;
                }
            }

            if (gameCanvas == null) yield break;

            // Full-screen overlay for gold flash
            GameObject overlay = new GameObject("RewardFlashOverlay");
            overlay.transform.SetParent(gameCanvas.transform, false);
            RectTransform overlayRect = overlay.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            Image overlayImg = overlay.AddComponent<Image>();
            overlayImg.color = new Color(1f, 0.84f, 0f, 0f);
            overlayImg.raycastTarget = false;

            // Gold text "+N Gold"
            GameObject textObj = new GameObject("RewardText");
            textObj.transform.SetParent(overlay.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0.35f);
            textRect.anchorMax = new Vector2(1, 0.65f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text rewardText = textObj.AddComponent<Text>();
            rewardText.text = $"+{goldAmount} Gold";
            rewardText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            rewardText.fontSize = 80;
            rewardText.fontStyle = FontStyle.Bold;
            rewardText.color = new Color(1f, 0.84f, 0f, 0f);
            rewardText.alignment = TextAnchor.MiddleCenter;
            rewardText.horizontalOverflow = HorizontalWrapMode.Overflow;
            rewardText.verticalOverflow = VerticalWrapMode.Overflow;
            rewardText.raycastTarget = false;

            Outline textOutline = textObj.AddComponent<Outline>();
            textOutline.effectColor = new Color(0.4f, 0.2f, 0f, 0.9f);
            textOutline.effectDistance = new Vector2(3, -3);

            // Expanding ring (UI-based)
            GameObject ringObj = new GameObject("RewardRing");
            ringObj.transform.SetParent(overlay.transform, false);
            RectTransform ringRect = ringObj.AddComponent<RectTransform>();
            ringRect.anchorMin = new Vector2(0.5f, 0.5f);
            ringRect.anchorMax = new Vector2(0.5f, 0.5f);
            ringRect.sizeDelta = new Vector2(100, 100);

            Image ringImg = ringObj.AddComponent<Image>();
            ringImg.raycastTarget = false;

            // Create ring texture
            Texture2D ringTex = new Texture2D(64, 64);
            Color[] pixels = new Color[64 * 64];
            Vector2 center = new Vector2(32, 32);
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist > 26f && dist < 31f)
                        pixels[y * 64 + x] = Color.white;
                    else
                        pixels[y * 64 + x] = Color.clear;
                }
            }
            ringTex.SetPixels(pixels);
            ringTex.Apply();
            ringTex.filterMode = FilterMode.Bilinear;

            ringImg.sprite = Sprite.Create(ringTex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64f);
            ringImg.color = new Color(1f, 0.84f, 0f, 1f);

            // Phase 1: Gold flash in (0.15s) + text scale up + ring start
            float flashInDuration = 0.15f;
            float elapsed = 0f;
            while (elapsed < flashInDuration)
            {
                float t = elapsed / flashInDuration;
                overlayImg.color = new Color(1f, 0.84f, 0f, t * 0.4f);
                rewardText.color = new Color(1f, 0.84f, 0f, t);
                float textScale = 0.3f + t * 0.7f;
                textObj.transform.localScale = Vector3.one * textScale;
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Phase 2: Flash out + text hold + ring expand (0.6s)
            float mainDuration = 0.6f;
            elapsed = 0f;
            Vector2 initialTextPos = textRect.anchoredPosition;
            while (elapsed < mainDuration)
            {
                float t = elapsed / mainDuration;

                // Flash fades out
                float flashAlpha = 0.4f * (1f - Mathf.Clamp01(t / 0.5f));
                overlayImg.color = new Color(1f, 0.84f, 0f, flashAlpha);

                // Text moves up slightly and starts fading in second half
                float moveUp = t * 60f;
                textRect.anchoredPosition = initialTextPos + new Vector2(0, moveUp);
                float textAlpha = t < 0.5f ? 1f : 1f - (t - 0.5f) * 2f;
                rewardText.color = new Color(1f, 0.84f, 0f, textAlpha);

                // Ring expands and fades
                float ringScale = 1f + t * 4f;
                ringObj.transform.localScale = Vector3.one * ringScale;
                ringImg.color = new Color(1f, 0.84f, 0f, 1f - t);

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Phase 3: Final fade out (0.15s)
            float fadeOutDuration = 0.15f;
            elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                float t = elapsed / fadeOutDuration;
                rewardText.color = new Color(1f, 0.84f, 0f, Mathf.Max(0, (1f - t) * 0.3f));
                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(overlay);
        }
        #endregion

        #region Legendary Summon Effect
        /// <summary>
        /// Show legendary unit summon effect: full-screen gold flash + expanding gold rings + "LEGENDARY!" text + camera shake.
        /// </summary>
        public void ShowLegendarySummonEffect(Vector3 worldPosition)
        {
            StartCoroutine(LegendarySummonEffectRoutine(worldPosition));
        }

        private IEnumerator LegendarySummonEffectRoutine(Vector3 worldPosition)
        {
            Debug.Log($"[VFXManager] Legendary summon effect at {worldPosition}");

            // Find GameCanvas for UI overlay
            Canvas gameCanvas = null;
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas c in canvases)
            {
                if (c.gameObject.name == "GameCanvas")
                {
                    gameCanvas = c;
                    break;
                }
            }

            GameObject overlay = null;
            Image overlayImg = null;
            Text legendaryText = null;

            if (gameCanvas != null)
            {
                // Gold flash overlay
                overlay = new GameObject("LegendarySummonOverlay");
                overlay.transform.SetParent(gameCanvas.transform, false);
                RectTransform overlayRect = overlay.AddComponent<RectTransform>();
                overlayRect.anchorMin = Vector2.zero;
                overlayRect.anchorMax = Vector2.one;
                overlayRect.offsetMin = Vector2.zero;
                overlayRect.offsetMax = Vector2.zero;

                overlayImg = overlay.AddComponent<Image>();
                overlayImg.color = new Color(1f, 0.84f, 0f, 0f);
                overlayImg.raycastTarget = false;

                // "LEGENDARY!" text
                GameObject textObj = new GameObject("LegendaryText");
                textObj.transform.SetParent(overlay.transform, false);
                RectTransform textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = new Vector2(0, 0.35f);
                textRect.anchorMax = new Vector2(1, 0.65f);
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;

                legendaryText = textObj.AddComponent<Text>();
                legendaryText.text = "LEGENDARY!";
                legendaryText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                legendaryText.fontSize = 100;
                legendaryText.fontStyle = FontStyle.Bold;
                legendaryText.color = new Color(1f, 0.84f, 0f, 0f);
                legendaryText.alignment = TextAnchor.MiddleCenter;
                legendaryText.horizontalOverflow = HorizontalWrapMode.Overflow;
                legendaryText.verticalOverflow = VerticalWrapMode.Overflow;
                legendaryText.raycastTarget = false;

                Outline textOutline = textObj.AddComponent<Outline>();
                textOutline.effectColor = new Color(0.4f, 0.2f, 0f, 0.9f);
                textOutline.effectDistance = new Vector2(3, -3);
            }

            // Create 2 expanding gold rings in world space
            GameObject ring1 = CreateWorldRing(worldPosition, new Color(1f, 0.84f, 0f, 1f));
            GameObject ring2 = CreateWorldRing(worldPosition, new Color(1f, 0.9f, 0.4f, 0.7f));

            Camera mainCamera = Camera.main;
            Vector3 originalCamPos = mainCamera != null ? mainCamera.transform.position : Vector3.zero;

            // Phase 1: Gold flash in (0.2s)
            float flashDuration = 0.2f;
            float elapsed = 0f;
            while (elapsed < flashDuration)
            {
                float t = elapsed / flashDuration;
                if (overlayImg != null) overlayImg.color = new Color(1f, 0.84f, 0f, t * 0.5f);
                if (legendaryText != null) legendaryText.color = new Color(1f, 0.84f, 0f, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Phase 2: Hold + pulse + camera shake + ring expand (1.0s)
            float holdDuration = 1.0f;
            elapsed = 0f;
            while (elapsed < holdDuration)
            {
                float t = elapsed / holdDuration;

                // Text pulsing
                if (legendaryText != null)
                {
                    float pulse = 1f + Mathf.Sin(elapsed * 10f) * 0.1f;
                    legendaryText.transform.localScale = Vector3.one * pulse;
                }

                // Camera shake
                if (mainCamera != null)
                {
                    float shakeIntensity = 0.12f * (1f - t * 0.5f);
                    float offsetX = Random.Range(-shakeIntensity, shakeIntensity);
                    float offsetY = Random.Range(-shakeIntensity, shakeIntensity);
                    mainCamera.transform.position = originalCamPos + new Vector3(offsetX, offsetY, 0f);
                }

                // Overlay pulse
                if (overlayImg != null)
                {
                    float overlayAlpha = 0.5f + Mathf.Sin(elapsed * 8f) * 0.15f;
                    overlayImg.color = new Color(1f, 0.84f, 0f, overlayAlpha);
                }

                // Ring 1: fast expand
                if (ring1 != null)
                {
                    float scale1 = 0.3f + t * 3f;
                    ring1.transform.localScale = Vector3.one * scale1;
                    SpriteRenderer sr1 = ring1.GetComponent<SpriteRenderer>();
                    if (sr1 != null) sr1.color = new Color(1f, 0.84f, 0f, 1f - t);
                }

                // Ring 2: slower expand
                if (ring2 != null)
                {
                    float scale2 = 0.2f + t * 2f;
                    ring2.transform.localScale = Vector3.one * scale2;
                    SpriteRenderer sr2 = ring2.GetComponent<SpriteRenderer>();
                    if (sr2 != null) sr2.color = new Color(1f, 0.9f, 0.4f, 0.7f * (1f - t));
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Phase 3: Fade out (0.3s)
            float fadeOutDuration = 0.3f;
            elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                float t = elapsed / fadeOutDuration;
                if (overlayImg != null) overlayImg.color = new Color(1f, 0.84f, 0f, 0.5f * (1f - t));
                if (legendaryText != null) legendaryText.color = new Color(1f, 0.84f, 0f, 1f - t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Cleanup
            if (mainCamera != null) mainCamera.transform.position = originalCamPos;
            if (overlay != null) Destroy(overlay);
            if (ring1 != null) Destroy(ring1);
            if (ring2 != null) Destroy(ring2);

            // Show floating text at unit position
            ShowFloatingText(worldPosition + Vector3.up * 0.5f, "LEGENDARY!", new Color(1f, 0.84f, 0f));
        }

        /// <summary>
        /// Create a world-space ring sprite at position.
        /// </summary>
        private GameObject CreateWorldRing(Vector3 position, Color color)
        {
            GameObject ring = new GameObject("Ring");
            ring.transform.position = position;
            SpriteRenderer ringRenderer = ring.AddComponent<SpriteRenderer>();
            ringRenderer.sortingOrder = 50;

            Texture2D ringTex = new Texture2D(64, 64);
            Color[] pixels = new Color[64 * 64];
            Vector2 center = new Vector2(32, 32);
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist > 26f && dist < 31f)
                        pixels[y * 64 + x] = Color.white;
                    else
                        pixels[y * 64 + x] = Color.clear;
                }
            }
            ringTex.SetPixels(pixels);
            ringTex.Apply();
            ringTex.filterMode = FilterMode.Bilinear;

            ringRenderer.sprite = Sprite.Create(ringTex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64f);
            ringRenderer.color = color;
            ring.transform.localScale = Vector3.one * 0.3f;

            return ring;
        }
        #endregion

        #region Combat Line Effects
        private static Material s_defaultSpriteMaterial;
        private static Material DefaultSpriteMaterial
        {
            get
            {
                if (s_defaultSpriteMaterial == null)
                    s_defaultSpriteMaterial = new Material(Shader.Find("Sprites/Default"));
                return s_defaultSpriteMaterial;
            }
        }

        /// <summary>
        /// Draw a missile/laser line effect from start to end with fade animation.
        /// </summary>
        public void PlayMissileEffect(Vector3 start, Vector3 end, Rarity rarity)
        {
            StartCoroutine(MissileEffectCoroutine(start, end, UnitData.GetRarityColor(rarity)));
        }

        private IEnumerator MissileEffectCoroutine(Vector3 start, Vector3 end, Color color)
        {
            GameObject lineObj = new GameObject("MissileEffect");
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.startWidth = 0.05f;
            lr.endWidth = 0.05f;
            lr.positionCount = 2;
            lr.material = DefaultSpriteMaterial;
            lr.startColor = color;
            lr.endColor = color;

            float duration = 0.15f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                lr.SetPosition(0, start);
                lr.SetPosition(1, Vector3.Lerp(start, end, t));
                yield return null;
            }

            float fadeDuration = 0.1f;
            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = 1f - (elapsed / fadeDuration);
                Color fadeColor = new Color(color.r, color.g, color.b, alpha);
                lr.startColor = fadeColor;
                lr.endColor = fadeColor;
                yield return null;
            }
            Destroy(lineObj);
        }

        /// <summary>
        /// Draw splash effect line from center to hit position.
        /// </summary>
        public void PlaySplashEffect(Vector3 center, Vector3 hitPos)
        {
            StartCoroutine(SplashEffectCoroutine(center, hitPos));
        }

        private IEnumerator SplashEffectCoroutine(Vector3 start, Vector3 end)
        {
            GameObject lineObj = new GameObject("SplashEffect");
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.startWidth = 0.03f;
            lr.endWidth = 0.03f;
            lr.positionCount = 2;
            lr.material = DefaultSpriteMaterial;
            lr.startColor = new Color(1f, 0.5f, 0f, 0.6f);
            lr.endColor = new Color(1f, 0.5f, 0f, 0f);
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);

            yield return new WaitForSeconds(0.1f);
            Destroy(lineObj);
        }

        /// <summary>
        /// Draw chain lightning effect between targets.
        /// </summary>
        public void PlayChainEffect(Vector3 from, Vector3 to)
        {
            StartCoroutine(ChainEffectCoroutine(from, to));
        }

        private IEnumerator ChainEffectCoroutine(Vector3 start, Vector3 end)
        {
            GameObject lineObj = new GameObject("ChainEffect");
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.startWidth = 0.04f;
            lr.endWidth = 0.04f;
            lr.positionCount = 2;
            lr.material = DefaultSpriteMaterial;
            lr.startColor = new Color(0.5f, 0.5f, 1f, 0.8f);
            lr.endColor = new Color(0.5f, 0.5f, 1f, 0.8f);
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);

            yield return new WaitForSeconds(0.15f);
            Destroy(lineObj);
        }
        #endregion

        #region Upgrade Effect
        /// <summary>
        /// Show upgrade effect: glow on upgraded unit + brief pulse on all same-rarity units.
        /// </summary>
        public void ShowUpgradeEffect(Vector3 unitPosition, Rarity rarity)
        {
            StartCoroutine(UpgradeEffectRoutine(unitPosition, rarity));
        }

        private IEnumerator UpgradeEffectRoutine(Vector3 unitPosition, Rarity rarity)
        {
            Debug.Log($"[VFXManager] Upgrade effect for {rarity} at {unitPosition}");

            Color rarityColor = UnitData.GetRarityColor(rarity);

            // Pulse all same-rarity units on grid and show "UP!" on each immediately
            if (GridManager.Instance != null)
            {
                for (int x = 0; x < GridManager.GRID_WIDTH; x++)
                {
                    for (int y = 0; y < GridManager.GRID_HEIGHT; y++)
                    {
                        Unit unit = GridManager.Instance.GetUnitAt(x, y);
                        if (unit != null && unit.Data != null && unit.Data.rarity == rarity)
                        {
                            StartCoroutine(PulseUnitColor(unit, rarityColor));
                            ShowFloatingText(unit.transform.position + Vector3.up * 0.5f, "UP!", rarityColor);
                        }
                    }
                }
            }

            // Create expanding ring at clicked unit position
            GameObject ring = CreateWorldRing(unitPosition, rarityColor);

            // Animate ring concurrently with pulses
            float duration = 0.5f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                if (ring != null)
                {
                    float scale = 0.3f + t * 2f;
                    ring.transform.localScale = Vector3.one * scale;
                    SpriteRenderer sr = ring.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.color = new Color(rarityColor.r, rarityColor.g, rarityColor.b, 1f - t);
                }
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (ring != null) Destroy(ring);
        }

        /// <summary>
        /// Brief color pulse effect on a single unit.
        /// </summary>
        private IEnumerator PulseUnitColor(Unit unit, Color pulseColor)
        {
            if (unit == null) yield break;

            SpriteRenderer sr = unit.GetComponent<SpriteRenderer>();
            if (sr == null) yield break;

            Color originalColor = sr.color;

            // Flash to rarity color (2 pulses, color only - no scale change)
            for (int pulse = 0; pulse < 2; pulse++)
            {
                float flashDuration = 0.2f;
                float elapsed = 0f;
                while (elapsed < flashDuration)
                {
                    if (unit == null || sr == null) yield break;
                    float t = elapsed / flashDuration;
                    sr.color = Color.Lerp(originalColor, pulseColor, t);
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                elapsed = 0f;
                while (elapsed < flashDuration)
                {
                    if (unit == null || sr == null) yield break;
                    float t = elapsed / flashDuration;
                    sr.color = Color.Lerp(pulseColor, originalColor, t);
                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }

            if (unit != null && sr != null)
                sr.color = originalColor;
        }
        #endregion
    }
}
