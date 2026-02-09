using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using LottoDefense.Gameplay;

namespace LottoDefense.UI
{
    /// <summary>
    /// Displays game result screen (Victory or Defeat) with stats and options.
    /// Shows round reached, contribution score, and provides button to return to main menu.
    /// </summary>
    public class GameResultUI : MonoBehaviour
    {
        #region Inspector Fields
        [Header("UI References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text roundText;
        [SerializeField] private Text contributionText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Text confirmButtonText;
        #endregion

        #region Private Fields
        private bool isShowing = false;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Start hidden
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            if (resultPanel != null)
            {
                resultPanel.SetActive(false);
            }

            // Setup button
            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            }
        }

        private void OnEnable()
        {
            // Subscribe to game state changes
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnStateChanged += HandleStateChanged;
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from game state changes
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnStateChanged -= HandleStateChanged;
            }
        }
        #endregion

        #region State Management
        /// <summary>
        /// Handle game state changes to show/hide result screen.
        /// </summary>
        private void HandleStateChanged(GameState oldState, GameState newState)
        {
            if (newState == GameState.Victory)
            {
                ShowResult(true);
            }
            else if (newState == GameState.Defeat)
            {
                ShowResult(false);
            }
        }
        #endregion

        #region Display Logic
        /// <summary>
        /// Show result screen with victory or defeat message.
        /// </summary>
        /// <param name="isVictory">True for victory, false for defeat</param>
        public void ShowResult(bool isVictory)
        {
            if (isShowing) return;

            isShowing = true;

            // Get game stats
            int roundReached = GameplayManager.Instance != null ? GameplayManager.Instance.CurrentRound : 1;
            int monstersKilled = CalculateMonstersKilled();
            int contribution = CalculateContribution(roundReached, monstersKilled);

            // Update UI
            if (titleText != null)
            {
                titleText.text = isVictory ? "🎉 승리!" : "💀 게임 오버";
                titleText.color = isVictory ? new Color(0.2f, 1f, 0.3f) : new Color(1f, 0.2f, 0.2f);
            }

            if (roundText != null)
            {
                roundText.text = $"도달 라운드: {roundReached}";
            }

            if (contributionText != null)
            {
                contributionText.text = $"기여도: {contribution}점";
            }

            if (confirmButtonText != null)
            {
                confirmButtonText.text = "확인";
            }

            // Show panel with fade in
            if (resultPanel != null)
            {
                resultPanel.SetActive(true);
            }

            if (canvasGroup != null)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                StartCoroutine(FadeIn());
            }

            Debug.Log($"[GameResultUI] Showing result: {(isVictory ? "Victory" : "Defeat")}, Round: {roundReached}, Contribution: {contribution}");
        }

        /// <summary>
        /// Calculate total monsters killed (simplified).
        /// </summary>
        private int CalculateMonstersKilled()
        {
            // TODO: Track this properly in a statistics manager
            // For now, estimate based on rounds completed
            if (GameplayManager.Instance == null) return 0;

            int roundsCompleted = GameplayManager.Instance.CurrentRound - 1;
            return roundsCompleted * 30; // Assume 30 monsters per round
        }

        /// <summary>
        /// Calculate contribution score based on performance.
        /// </summary>
        private int CalculateContribution(int roundReached, int monstersKilled)
        {
            // Base score: rounds reached
            int baseScore = roundReached * 100;

            // Bonus score: monsters killed
            int monsterBonus = monstersKilled * 10;

            // Life remaining bonus
            int lifeBonus = 0;
            if (GameplayManager.Instance != null)
            {
                lifeBonus = GameplayManager.Instance.CurrentLife * 50;
            }

            return baseScore + monsterBonus + lifeBonus;
        }

        /// <summary>
        /// Fade in the result panel.
        /// </summary>
        private System.Collections.IEnumerator FadeIn()
        {
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = 1f;
        }
        #endregion

        #region Button Handlers
        /// <summary>
        /// Handle confirm button click - return to main menu.
        /// </summary>
        private void OnConfirmButtonClicked()
        {
            Debug.Log("[GameResultUI] Confirm button clicked - returning to main menu");

            // Load main menu scene (SceneNavigator.LoadMainGame will handle cleanup)
            SceneNavigator navigator = FindFirstObjectByType<SceneNavigator>();
            if (navigator != null)
            {
                navigator.LoadMainGame();
            }
            else
            {
                // Fallback: cleanup with coroutine
                StartCoroutine(CleanupAndLoadMainGame());
            }
        }

        private System.Collections.IEnumerator CleanupAndLoadMainGame()
        {
            GameplayManager.CleanupAllGameplaySingletons();
            yield return null; // Wait for Destroy() to take effect
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainGame");
        }
        #endregion
    }
}
