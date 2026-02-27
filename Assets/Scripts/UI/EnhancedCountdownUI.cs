using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using LottoDefense.Gameplay;
using LottoDefense.Profile;
using LottoDefense.Multiplayer;

namespace LottoDefense.UI
{
    /// <summary>
    /// Enhanced countdown UI that displays player profiles before the countdown starts.
    /// Shows profile images and nicknames for both single and cooperative play.
    /// </summary>
    public class EnhancedCountdownUI : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Profile Display")]
        [SerializeField] private GameObject profilePanel;
        [SerializeField] private Image player1Avatar;
        [SerializeField] private Text player1Nickname;
        [SerializeField] private GameObject player1Container;

        [SerializeField] private Image player2Avatar;
        [SerializeField] private Text player2Nickname;
        [SerializeField] private GameObject player2Container;

        [SerializeField] private Text vsText;

        [Header("Countdown Display")]
        [SerializeField] private Text countdownText;
        [SerializeField] private Text startText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Animation Settings")]
        [SerializeField] private float profileDisplayDuration = 2f;
        [SerializeField] private float profileFadeInTime = 0.5f;
        [SerializeField] private float profileFadeOutTime = 0.3f;
        [SerializeField] private float countdownInterval = 1f;
        [SerializeField] private float scaleAnimationDuration = 0.8f;
        [SerializeField] private float startScale = 0.3f;
        [SerializeField] private float peakScale = 2.0f;
        [SerializeField] private float endScale = 1.0f;

        [Header("Audio Settings")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip countdownBeep;
        [SerializeField] private AudioClip profileShowSound;
        [SerializeField] private AudioClip battleStartSound;
        #endregion

        #region Private Fields
        private Action _onCompleteCallback;
        private Coroutine _countdownCoroutine;
        private bool _isCoopMode = false;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
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
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            // Start hidden
            canvasGroup.alpha = 0f;
            if (profilePanel != null) profilePanel.SetActive(false);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Start the enhanced countdown sequence with profile display
        /// </summary>
        public void StartEnhancedCountdown(bool coopMode, Action onComplete = null)
        {
            _onCompleteCallback = onComplete;
            _isCoopMode = coopMode;

            gameObject.SetActive(true);

            if (_countdownCoroutine != null)
            {
                StopCoroutine(_countdownCoroutine);
            }

            _countdownCoroutine = StartCoroutine(EnhancedCountdownSequence());
        }

        /// <summary>
        /// Set player profile data for display
        /// </summary>
        public void SetPlayerProfiles(UserProfile player1Profile, UserProfile player2Profile = null)
        {
            // Set Player 1 profile
            if (player1Profile != null && player1Container != null)
            {
                if (player1Nickname != null)
                    player1Nickname.text = player1Profile.nickname;

                // Get avatar sprite through UserProfileManager
                if (player1Avatar != null && UserProfileManager.Instance != null)
                {
                    var avatarData = UserProfileManager.Instance.GetAvatarData(player1Profile.selectedAvatarId);
                    if (avatarData != null && avatarData.avatarSprite != null)
                        player1Avatar.sprite = avatarData.avatarSprite;
                }
            }

            // Set Player 2 profile (for coop mode)
            if (player2Profile != null && player2Container != null)
            {
                if (player2Nickname != null)
                    player2Nickname.text = player2Profile.nickname;

                // Get avatar sprite through UserProfileManager
                if (player2Avatar != null && UserProfileManager.Instance != null)
                {
                    var avatarData = UserProfileManager.Instance.GetAvatarData(player2Profile.selectedAvatarId);
                    if (avatarData != null && avatarData.avatarSprite != null)
                        player2Avatar.sprite = avatarData.avatarSprite;
                }
            }
        }
        #endregion

        #region Coroutines
        /// <summary>
        /// Enhanced countdown sequence with profile display
        /// </summary>
        private IEnumerator EnhancedCountdownSequence()
        {
            // Ensure time is running
            Time.timeScale = 1f;

            // Phase 1: Show player profiles
            yield return StartCoroutine(ShowPlayerProfiles());

            // Phase 2: Countdown from 3 to 1
            for (int i = 3; i >= 1; i--)
            {
                yield return StartCoroutine(ShowNumber(i));
                yield return new WaitForSecondsRealtime(countdownInterval);
            }

            // Phase 3: Show "START!" text
            yield return StartCoroutine(ShowStartText());

            // Phase 4: Fade out and complete
            yield return StartCoroutine(FadeOut());

            // Notify completion
            _onCompleteCallback?.Invoke();

            // Clean up
            Cleanup();
        }

        /// <summary>
        /// Display player profiles with animation
        /// </summary>
        private IEnumerator ShowPlayerProfiles()
        {
            if (profilePanel == null) yield break;

            // Setup profile display based on mode
            SetupProfileDisplay();

            // Fade in profile panel
            profilePanel.SetActive(true);
            CanvasGroup profileCanvasGroup = profilePanel.GetComponent<CanvasGroup>();
            if (profileCanvasGroup == null)
            {
                profileCanvasGroup = profilePanel.AddComponent<CanvasGroup>();
            }

            // Play profile show sound
            if (profileShowSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(profileShowSound);
            }

            // Animate profile fade in
            float elapsed = 0f;
            while (elapsed < profileFadeInTime)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / profileFadeInTime;
                profileCanvasGroup.alpha = t;

                // Scale animation for profile containers
                if (player1Container != null)
                {
                    float scale = Mathf.Lerp(0.5f, 1f, t);
                    player1Container.transform.localScale = Vector3.one * scale;
                }

                if (_isCoopMode && player2Container != null)
                {
                    float scale = Mathf.Lerp(0.5f, 1f, t);
                    player2Container.transform.localScale = Vector3.one * scale;
                }

                yield return null;
            }

            profileCanvasGroup.alpha = 1f;

            // Display profiles for specified duration
            yield return new WaitForSecondsRealtime(profileDisplayDuration);

            // Animate profile fade out
            elapsed = 0f;
            while (elapsed < profileFadeOutTime)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / profileFadeOutTime;
                profileCanvasGroup.alpha = 1f - t;
                yield return null;
            }

            profileCanvasGroup.alpha = 0f;
            profilePanel.SetActive(false);

            // Fade in main countdown UI
            canvasGroup.alpha = 1f;
        }

        /// <summary>
        /// Setup profile display based on game mode
        /// </summary>
        private void SetupProfileDisplay()
        {
            // Get current user profile through UserProfileManager
            UserProfileManager profileManager = UserProfileManager.Instance;

            if (_isCoopMode)
            {
                // Cooperative mode - show both players
                if (player1Container != null) player1Container.SetActive(true);
                if (player2Container != null) player2Container.SetActive(true);
                if (vsText != null)
                {
                    vsText.gameObject.SetActive(true);
                    vsText.text = "&";  // Use "&" for cooperative play instead of "VS"
                }

                // Position containers for 2-player display
                if (player1Container != null)
                {
                    RectTransform rt1 = player1Container.GetComponent<RectTransform>();
                    rt1.anchoredPosition = new Vector2(-200, 0);
                }

                if (player2Container != null)
                {
                    RectTransform rt2 = player2Container.GetComponent<RectTransform>();
                    rt2.anchoredPosition = new Vector2(200, 0);
                }

                // Set profiles
                if (profileManager != null && profileManager.CurrentProfile != null)
                {
                    SetPlayerProfiles(profileManager.CurrentProfile, CreateGuestProfile());
                }
            }
            else
            {
                // Single player mode - show only player 1
                if (player1Container != null)
                {
                    player1Container.SetActive(true);
                    RectTransform rt1 = player1Container.GetComponent<RectTransform>();
                    rt1.anchoredPosition = Vector2.zero;
                }

                if (player2Container != null) player2Container.SetActive(false);
                if (vsText != null) vsText.gameObject.SetActive(false);

                // Set profile
                if (profileManager != null && profileManager.CurrentProfile != null)
                {
                    SetPlayerProfiles(profileManager.CurrentProfile);
                }
            }
        }

        /// <summary>
        /// Create a guest profile for Player 2 in coop mode
        /// </summary>
        private UserProfile CreateGuestProfile()
        {
            UserProfile guestProfile = new UserProfile();
            guestProfile.nickname = "Player 2";
            guestProfile.selectedAvatarId = "avatar_default";
            // Guest uses default avatar or random avatar
            return guestProfile;
        }

        /// <summary>
        /// Display and animate a countdown number
        /// </summary>
        private IEnumerator ShowNumber(int number)
        {
            if (countdownText != null)
            {
                countdownText.gameObject.SetActive(true);
                countdownText.text = number.ToString();

                // Apply color per number
                switch (number)
                {
                    case 3:
                        countdownText.color = new Color(1f, 0.3f, 0.3f); // Red
                        break;
                    case 2:
                        countdownText.color = new Color(1f, 1f, 0.3f); // Yellow
                        break;
                    case 1:
                        countdownText.color = new Color(0.3f, 1f, 0.3f); // Green
                        break;
                }
            }

            // Play countdown sound
            PlayCountdownSound();

            // Animate scale
            yield return StartCoroutine(AnimateScale(countdownText.transform));
        }

        /// <summary>
        /// Show "START!" text with animation
        /// </summary>
        private IEnumerator ShowStartText()
        {
            if (countdownText != null)
            {
                countdownText.gameObject.SetActive(false);
            }

            if (startText != null)
            {
                startText.gameObject.SetActive(true);
                startText.color = new Color(0.3f, 1f, 0.3f); // Green for start
                startText.transform.localScale = Vector3.one * startScale;

                // Play battle start sound
                if (battleStartSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(battleStartSound);
                }

                // Quick scale-up animation
                float elapsed = 0f;
                float duration = 0.3f;
                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = elapsed / duration;
                    float scale = Mathf.Lerp(startScale, 1.5f, t);
                    startText.transform.localScale = Vector3.one * scale;
                    yield return null;
                }

                yield return new WaitForSecondsRealtime(0.5f);
            }
        }

        /// <summary>
        /// Animate scale of a transform
        /// </summary>
        private IEnumerator AnimateScale(Transform target)
        {
            if (target == null) yield break;

            float elapsed = 0f;
            float halfDuration = scaleAnimationDuration * 0.5f;

            // Scale up
            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / halfDuration;
                float scale = Mathf.Lerp(startScale, peakScale, t);
                target.localScale = Vector3.one * scale;
                yield return null;
            }

            // Scale down
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / halfDuration;
                float scale = Mathf.Lerp(peakScale, endScale, t);
                target.localScale = Vector3.one * scale;
                yield return null;
            }

            target.localScale = Vector3.one * endScale;
        }

        /// <summary>
        /// Fade out the UI
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
        /// Play countdown beep sound
        /// </summary>
        private void PlayCountdownSound()
        {
            if (audioSource != null && countdownBeep != null)
            {
                audioSource.PlayOneShot(countdownBeep);
            }
        }
        #endregion

        #region Cleanup
        /// <summary>
        /// Clean up the UI after countdown
        /// </summary>
        private void Cleanup()
        {
            if (profilePanel != null) profilePanel.SetActive(false);
            if (countdownText != null) countdownText.gameObject.SetActive(false);
            if (startText != null) startText.gameObject.SetActive(false);

            // Reset to single player defaults
            _isCoopMode = false;
        }
        #endregion
    }
}