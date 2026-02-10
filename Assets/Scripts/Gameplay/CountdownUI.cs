using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using LottoDefense.UI;

namespace LottoDefense.Gameplay
{
    /// <summary>
    /// Displays animated countdown sequence (3-2-1-START!) with visual and audio feedback.
    /// Auto-destroys after completion and notifies callback.
    /// </summary>
    public class CountdownUI : MonoBehaviour
    {
        #region Serialized Fields
        [Header("UI References")]
        [SerializeField] private Text countdownText;
        [SerializeField] private Text startText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Animation Settings")]
        [SerializeField] private float countdownInterval = 1f;
        [SerializeField] private float scaleAnimationDuration = 0.8f;
        [SerializeField] private float startScale = 0.3f;
        [SerializeField] private float peakScale = 2.0f;
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
        /// Uses WaitForSecondsRealtime to work regardless of Time.timeScale.
        /// </summary>
        private IEnumerator CountdownSequence()
        {
            Debug.Log($"[CountdownUI] CountdownSequence started - Time.timeScale: {Time.timeScale}");

            // Ensure time is running during countdown
            Time.timeScale = 1f;

            // Countdown from 3 to 1
            for (int i = 3; i >= 1; i--)
            {
                Debug.Log($"[CountdownUI] Showing number: {i}");
                yield return StartCoroutine(ShowNumber(i));
                yield return new WaitForSecondsRealtime(countdownInterval);
            }

            // Show "START!" text
            Debug.Log("[CountdownUI] Showing START!");
            yield return StartCoroutine(ShowStartText());

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
        /// Display and animate a single countdown number with per-number color.
        /// </summary>
        private IEnumerator ShowNumber(int number)
        {
            if (countdownText != null)
            {
                countdownText.text = number.ToString();

                // Apply color per number: 3=red, 2=yellow, 1=green
                switch (number)
                {
                    case 3:
                        countdownText.color = GameSceneDesignTokens.Countdown3Color;
                        break;
                    case 2:
                        countdownText.color = GameSceneDesignTokens.Countdown2Color;
                        break;
                    case 1:
                        countdownText.color = GameSceneDesignTokens.Countdown1Color;
                        break;
                    default:
                        countdownText.color = GameSceneDesignTokens.CountdownText;
                        break;
                }
            }

            // Play audio feedback
            PlayCountdownSound();

            // Animate scale: start small -> peak large -> settle to normal
            yield return StartCoroutine(AnimateScale());
        }

        /// <summary>
        /// Show "START!" text briefly after countdown finishes.
        /// </summary>
        private IEnumerator ShowStartText()
        {
            // Hide countdown number
            if (countdownText != null)
            {
                countdownText.gameObject.SetActive(false);
            }

            // Show START! text
            if (startText != null)
            {
                startText.gameObject.SetActive(true);
                startText.color = GameSceneDesignTokens.CountdownStartColor;
                startText.transform.localScale = Vector3.one * startScale;

                // Quick scale-up animation
                float elapsed = 0f;
                float duration = 0.3f;
                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = elapsed / duration;
                    float scale = Mathf.Lerp(startScale, 1.2f, t);
                    startText.transform.localScale = Vector3.one * scale;
                    yield return null;
                }
                startText.transform.localScale = Vector3.one * 1.2f;

                yield return new WaitForSecondsRealtime(0.5f);
            }
            else
            {
                // Fallback: show START! in countdown text if startText not assigned
                if (countdownText != null)
                {
                    countdownText.gameObject.SetActive(true);
                    countdownText.text = "START!";
                    countdownText.color = GameSceneDesignTokens.CountdownStartColor;
                    yield return StartCoroutine(AnimateScale());
                    yield return new WaitForSecondsRealtime(0.5f);
                }
            }
        }

        /// <summary>
        /// Scale animation: startScale -> peakScale -> endScale
        /// Uses unscaledDeltaTime to work regardless of Time.timeScale.
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
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / halfDuration;
                float scale = Mathf.Lerp(startScale, peakScale, t);
                textTransform.localScale = Vector3.one * scale;
                yield return null;
            }

            // Phase 2: Scale down from peakScale to endScale
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
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
        /// Uses unscaledDeltaTime to work regardless of Time.timeScale.
        /// </summary>
        private IEnumerator FadeOut()
        {
            float elapsed = 0f;
            float fadeDuration = 0.3f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
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
