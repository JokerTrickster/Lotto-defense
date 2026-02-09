using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace LottoDefense.VFX
{
    /// <summary>
    /// Generic floating text controller for displaying messages like "+5 Gold".
    /// Floats upward and fades out over time.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    public class FloatingTextController : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Components")]
        [SerializeField] private Text messageText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Animation Settings")]
        [SerializeField] private float floatSpeed = 1.5f;
        // [SerializeField] private float fadeDuration = 1.2f; // Unused - removed to avoid CS0414 warning
        [SerializeField] private float lifetime = 2f;
        [SerializeField] private float horizontalDrift = 0.3f; // Random horizontal movement
        #endregion

        #region Private Fields
        private Canvas canvas;
        private RectTransform rectTransform;
        private Coroutine animationCoroutine;
        private Vector2 driftDirection;
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
                canvas.sortingOrder = 1001; // Above damage numbers
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Show floating text at world position with specified message and color.
        /// </summary>
        /// <param name="worldPosition">World space position to display at</param>
        /// <param name="message">Message to display</param>
        /// <param name="color">Text color</param>
        public void Show(Vector3 worldPosition, string message, Color color)
        {
            if (messageText == null)
            {
                Debug.LogError("[FloatingTextController] Text component missing!");
                return;
            }

            // Position at world location (converted to screen space)
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
            rectTransform.position = screenPosition;

            // Setup text
            messageText.text = message;
            messageText.color = color;

            // Reset alpha
            canvasGroup.alpha = 1f;

            // Random horizontal drift
            driftDirection = new Vector2(Random.Range(-1f, 1f), 1f).normalized;

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
        /// Animate floating text moving upward with drift and fading out.
        /// </summary>
        private IEnumerator AnimateCoroutine()
        {
            float elapsed = 0f;
            Vector3 startPosition = rectTransform.position;

            while (elapsed < lifetime)
            {
                elapsed += Time.deltaTime;

                // Float upward with horizontal drift
                float verticalOffset = floatSpeed * elapsed;
                float horizontalOffset = horizontalDrift * driftDirection.x * elapsed;
                rectTransform.position = startPosition + new Vector3(horizontalOffset, verticalOffset, 0f);

                // Fade out gradually
                float fadeProgress = elapsed / lifetime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, fadeProgress);

                yield return null;
            }

            // Return to pool
            ReturnToPool();
        }

        /// <summary>
        /// Return this floating text to the pool.
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
                VFXManager.Instance.ReturnFloatingText(this);
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
            gameObject.SetActive(false);
        }
        #endregion
    }
}
