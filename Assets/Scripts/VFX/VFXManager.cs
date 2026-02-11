using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using LottoDefense.Units;
using LottoDefense.Monsters;
using LottoDefense.Gameplay;

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
            FloatingTextController controller = GetFloatingText();
            if (controller != null)
            {
                controller.Show(worldPosition, message, color);
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
    }
}
