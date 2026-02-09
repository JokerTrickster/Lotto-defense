using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace LottoDefense.UI
{
    /// <summary>
    /// Displays round start notification (e.g., "라운드 1", "라운드 2").
    /// Shows briefly then fades out automatically.
    /// </summary>
    public class RoundStartUI : MonoBehaviour
    {
        #region Serialized Fields
        [Header("UI References")]
        [SerializeField] private Text roundText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Animation Settings")]
        [SerializeField] private float displayDuration = 2f;
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private float scaleStart = 0.5f;
        [SerializeField] private float scalePeak = 1.2f;
        [SerializeField] private float scaleEnd = 1.0f;
        #endregion

        #region Private Fields
        private Coroutine _displayCoroutine;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Auto-setup references if not assigned
            if (roundText == null)
            {
                roundText = GetComponentInChildren<Text>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            // Start hidden
            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Show round start notification.
        /// </summary>
        /// <param name="roundNumber">Round number to display</param>
        public void ShowRoundStart(int roundNumber)
        {
            // CRITICAL: Must activate GameObject BEFORE starting coroutine
            gameObject.SetActive(true);

            if (_displayCoroutine != null)
            {
                StopCoroutine(_displayCoroutine);
            }

            _displayCoroutine = StartCoroutine(DisplayRoundStart(roundNumber));
        }
        #endregion

        #region Coroutines
        /// <summary>
        /// Display round start sequence with animation.
        /// </summary>
        private IEnumerator DisplayRoundStart(int roundNumber)
        {
            gameObject.SetActive(true);

            // Set text
            if (roundText != null)
            {
                roundText.text = $"라운드 {roundNumber}";
            }

            // Reset transform
            transform.localScale = Vector3.one * scaleStart;

            // Fade in + scale up
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / fadeInDuration;

                // Fade in
                canvasGroup.alpha = t;

                // Scale: start -> peak
                float scale = Mathf.Lerp(scaleStart, scalePeak, t);
                transform.localScale = Vector3.one * scale;

                yield return null;
            }

            // Hold at peak scale
            canvasGroup.alpha = 1f;
            transform.localScale = Vector3.one * scalePeak;

            // Scale down to normal size
            elapsed = 0f;
            float scaleDownDuration = 0.2f;
            while (elapsed < scaleDownDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / scaleDownDuration;

                float scale = Mathf.Lerp(scalePeak, scaleEnd, t);
                transform.localScale = Vector3.one * scale;

                yield return null;
            }

            transform.localScale = Vector3.one * scaleEnd;

            // Hold display
            yield return new WaitForSecondsRealtime(displayDuration);

            // Fade out
            elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / fadeOutDuration;

                canvasGroup.alpha = 1f - t;

                yield return null;
            }

            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }
        #endregion
    }
}
