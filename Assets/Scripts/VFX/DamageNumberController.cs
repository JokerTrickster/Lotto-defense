using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace LottoDefense.VFX
{
    /// <summary>
    /// Individual damage number instance that displays and animates damage text.
    /// Floats upward and fades out, then returns to pool.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    public class DamageNumberController : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Components")]
        [SerializeField] private Text damageText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Animation Settings")]
        [SerializeField] private float floatSpeed = 80f; // Increased for better visibility
        // [SerializeField] private float fadeDuration = 1f; // Unused - removed to avoid CS0414 warning
        [SerializeField] private float lifetime = 1.2f; // Slightly shorter

        [Header("Colors")]
        [SerializeField] private Color normalColor = new Color(1f, 1f, 0.3f); // Bright yellow for better visibility
        [SerializeField] private Color criticalColor = new Color(1f, 0.3f, 0.3f); // Red for crits
        [SerializeField] private float criticalScale = 1.5f; // Larger for emphasis
        #endregion

        #region Private Fields
        private Canvas canvas;
        private RectTransform rectTransform;
        private Coroutine animationCoroutine;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            canvas = GetComponent<Canvas>();
            rectTransform = GetComponent<RectTransform>();

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            // Ensure canvas is in screen space overlay
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 1000; // Above game objects
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Show damage number at world position with specified damage amount.
        /// </summary>
        /// <param name="worldPosition">World space position to display at</param>
        /// <param name="damage">Damage amount to display</param>
        /// <param name="isCritical">Whether this is a critical hit</param>
        public void Show(Vector3 worldPosition, int damage, bool isCritical = false)
        {
            if (damageText == null)
            {
                Debug.LogError("[DamageNumberController] Text component missing!");
                return;
            }

            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
                if (rectTransform == null)
                {
                    Debug.LogError("[DamageNumberController] RectTransform is null and couldn't be retrieved!");
                    return;
                }
            }

            if (Camera.main == null)
            {
                Debug.LogError("[DamageNumberController] Camera.main is null!");
                return;
            }

            // Position at world location (converted to screen space)
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
            rectTransform.position = screenPosition;

            // Setup text with bold formatting
            damageText.text = damage.ToString();
            damageText.color = isCritical ? criticalColor : normalColor;
            damageText.fontStyle = FontStyle.Bold; // Make text bold
            damageText.fontSize = isCritical ? 48 : 40; // Larger font

            // Scale for critical hits
            float scale = isCritical ? criticalScale : 1.2f; // Slightly larger base scale
            transform.localScale = Vector3.one * scale;

            // Add slight random horizontal offset for visual variety
            float randomX = Random.Range(-20f, 20f);
            rectTransform.position = screenPosition + new Vector3(randomX, 30f, 0f); // Start slightly above

            // Reset alpha
            canvasGroup.alpha = 1f;

            // Make visible
            gameObject.SetActive(true);

            // Start animation
            if (animationCoroutine != null)
                StopCoroutine(animationCoroutine);

            animationCoroutine = StartCoroutine(AnimateCoroutine());
        }
        #endregion

        #region Animation
        /// <summary>
        /// Animate damage number floating upward and fading out.
        /// </summary>
        private IEnumerator AnimateCoroutine()
        {
            float elapsed = 0f;
            Vector3 startPosition = rectTransform.position;
            float startScale = transform.localScale.x;

            while (elapsed < lifetime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / lifetime;

                // Float upward with easing
                float verticalOffset = floatSpeed * elapsed;
                rectTransform.position = startPosition + new Vector3(0f, verticalOffset, 0f);

                // Scale animation: slightly grow then shrink
                float scaleMultiplier = 1f;
                if (t < 0.2f)
                {
                    // Quick pop-in
                    scaleMultiplier = Mathf.Lerp(0.5f, 1.1f, t / 0.2f);
                }
                else if (t < 0.4f)
                {
                    // Settle
                    scaleMultiplier = Mathf.Lerp(1.1f, 1f, (t - 0.2f) / 0.2f);
                }
                transform.localScale = Vector3.one * (startScale * scaleMultiplier);

                // Fade out (starts after half lifetime)
                if (elapsed > lifetime * 0.5f)
                {
                    float fadeProgress = (elapsed - lifetime * 0.5f) / (lifetime * 0.5f);
                    canvasGroup.alpha = Mathf.Lerp(1f, 0f, fadeProgress);
                }

                yield return null;
            }

            // Return to pool
            ReturnToPool();
        }

        /// <summary>
        /// Return this damage number to the pool.
        /// </summary>
        private void ReturnToPool()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }

            gameObject.SetActive(false);

            // Notify VFXManager to return this to pool
            if (VFXManager.Instance != null)
            {
                VFXManager.Instance.ReturnDamageNumber(this);
            }
        }
        #endregion

        #region Pool Management
        /// <summary>
        /// Reset state for pool reuse.
        /// </summary>
        public void ResetForPool()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }

            canvasGroup.alpha = 1f;
            transform.localScale = Vector3.one;
            gameObject.SetActive(false);
        }
        #endregion
    }
}
