using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace LottoDefense.Gameplay
{
    /// <summary>
    /// Displays animated countdown sequence (3-2-1) with visual and audio feedback.
    /// Auto-destroys after completion and notifies callback.
    /// </summary>
    public class CountdownUI : MonoBehaviour
    {
        #region Serialized Fields
        [Header("UI References")]
        [SerializeField] private Text countdownText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Animation Settings")]
        [SerializeField] private float countdownInterval = 1f;
        [SerializeField] private float scaleAnimationDuration = 0.8f;
        [SerializeField] private float startScale = 0.5f;
        [SerializeField] private float peakScale = 1.5f;
        [SerializeField] private float endScale = 1.0f;

        [Header("Audio Settings")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip countdownBeep;
        #endregion

        #region Private Fields
        private Action _onCompleteCallback;
        private Coroutine _countdownCoroutine;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Auto-setup references if not assigned
            if (countdownText == null)
            {
                countdownText = GetComponentInChildren<Text>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            // Start hidden but keep GameObject active so FindFirstObjectByType can find it
            canvasGroup.alpha = 0f;
            // NOTE: Do NOT call gameObject.SetActive(false) here as it prevents FindFirstObjectByType from finding this component
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Start the countdown animation sequence.
        /// </summary>
        /// <param name="onComplete">Callback invoked when countdown finishes</param>
        public void StartCountdown(Action onComplete = null)
        {
            Debug.Log("[CountdownUI] StartCountdown called");
            _onCompleteCallback = onComplete;

            gameObject.SetActive(true);
            canvasGroup.alpha = 1f;

            if (_countdownCoroutine != null)
            {
                StopCoroutine(_countdownCoroutine);
            }

            _countdownCoroutine = StartCoroutine(CountdownSequence());
            Debug.Log("[CountdownUI] Countdown coroutine started");
        }
        #endregion

        #region Coroutines
        /// <summary>
        /// Main countdown sequence coroutine.
        /// </summary>
        private IEnumerator CountdownSequence()
        {
            Debug.Log("[CountdownUI] CountdownSequence started");
            // Countdown from 3 to 1
            for (int i = 3; i >= 1; i--)
            {
                Debug.Log($"[CountdownUI] Showing number: {i}");
                yield return StartCoroutine(ShowNumber(i));
                yield return new WaitForSeconds(countdownInterval);
            }

            Debug.Log("[CountdownUI] Countdown complete, fading out");
            // Fade out and complete
            yield return StartCoroutine(FadeOut());

            // Notify completion
            Debug.Log("[CountdownUI] Invoking completion callback");
            _onCompleteCallback?.Invoke();

            // Auto-destroy this UI element
            Destroy(gameObject);
        }

        /// <summary>
        /// Display and animate a single countdown number.
        /// </summary>
        private IEnumerator ShowNumber(int number)
        {
            if (countdownText != null)
            {
                countdownText.text = number.ToString();
            }

            // Play audio feedback
            PlayCountdownSound();

            // Animate scale: start small -> peak large -> settle to normal
            yield return StartCoroutine(AnimateScale());
        }

        /// <summary>
        /// Scale animation: 0.5 -> 1.5 -> 1.0
        /// </summary>
        private IEnumerator AnimateScale()
        {
            if (countdownText == null)
                yield break;

            Transform textTransform = countdownText.transform;
            float elapsed = 0f;
            float halfDuration = scaleAnimationDuration * 0.5f;

            // Phase 1: Scale up from startScale to peakScale
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                float scale = Mathf.Lerp(startScale, peakScale, t);
                textTransform.localScale = Vector3.one * scale;
                yield return null;
            }

            // Phase 2: Scale down from peakScale to endScale
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                float scale = Mathf.Lerp(peakScale, endScale, t);
                textTransform.localScale = Vector3.one * scale;
                yield return null;
            }

            // Ensure final scale is exact
            textTransform.localScale = Vector3.one * endScale;
        }

        /// <summary>
        /// Fade out the countdown UI.
        /// </summary>
        private IEnumerator FadeOut()
        {
            float elapsed = 0f;
            float fadeDuration = 0.3f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = 1f - (elapsed / fadeDuration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
        }
        #endregion

        #region Audio
        /// <summary>
        /// Play countdown beep sound effect.
        /// </summary>
        private void PlayCountdownSound()
        {
            if (audioSource != null && countdownBeep != null)
            {
                audioSource.PlayOneShot(countdownBeep);
            }
        }
        #endregion
    }
}
