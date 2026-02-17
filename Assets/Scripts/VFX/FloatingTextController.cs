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
                canvas.sortingOrder = 10000; // Very high - above all UI
            }
            
            Debug.Log($"[FloatingText] Awake: canvas={canvas != null}, sortingOrder={canvas?.sortingOrder}");
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
            Debug.Log($"[FloatingText] 🎯 Show called: message='{message}', worldPos={worldPosition}, color={color}");
            
            if (messageText == null)
            {
                Debug.LogError("[FloatingTextController] Text component missing!");
                return;
            }

            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
                if (rectTransform == null)
                {
                    Debug.LogError("[FloatingTextController] RectTransform is null and couldn't be retrieved!");
                    return;
                }
            }

            if (Camera.main == null)
            {
                Debug.LogError("[FloatingTextController] Camera.main is null!");
                return;
            }

            // CRITICAL FIX: Detach from pool parent so Canvas works independently
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            // Position at world location (converted to screen space)
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
            rectTransform.position = screenPosition;
            
            Debug.Log($"[FloatingText] 📍 Position: world={worldPosition}, screen={screenPosition}, rectPos={rectTransform.position}");

            // Setup text
            messageText.text = message;
            messageText.color = color;
            
            // Make skill effects bigger and more visible
            if (message.Contains("⚡"))
            {
                messageText.fontSize = 48; // Larger for skill names
                messageText.fontStyle = FontStyle.Bold;
                
                // Add outline for better visibility
                var outline = messageText.gameObject.GetComponent<Outline>();
                if (outline == null)
                    outline = messageText.gameObject.AddComponent<Outline>();
                outline.effectColor = Color.black;
                outline.effectDistance = new Vector2(3, -3);
            }
            else
            {
                messageText.fontSize = 32; // Normal size for other text
                messageText.fontStyle = FontStyle.Normal;
                
                // Smaller outline for normal text
                var outline = messageText.gameObject.GetComponent<Outline>();
                if (outline == null)
                    outline = messageText.gameObject.AddComponent<Outline>();
                outline.effectColor = Color.black;
                outline.effectDistance = new Vector2(2, -2);
            }

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
            
            Debug.Log($"[FloatingText] ✅ Displayed: '{message}' fontSize={messageText.fontSize} active={gameObject.activeSelf} alpha={canvasGroup.alpha}");
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

            // Return to pool parent
            if (VFXManager.Instance != null)
            {
                transform.SetParent(VFXManager.Instance.transform.Find("VFX_Pool"));
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
