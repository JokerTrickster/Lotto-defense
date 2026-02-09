using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using LottoDefense.Units;
using LottoDefense.Monsters;

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
            prefab.AddComponent<CanvasGroup>();

            // Add RectTransform
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
            prefab.AddComponent<CanvasGroup>();

            // Add RectTransform
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

            prefab.SetActive(false);
            return prefab;
        }
        #endregion
    }
}
