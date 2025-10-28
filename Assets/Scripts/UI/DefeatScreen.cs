using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using LottoDefense.Gameplay;

namespace LottoDefense.UI
{
    /// <summary>
    /// Defeat screen displayed when player's life reaches zero.
    /// Shows survival stats and provides restart/menu options.
    /// </summary>
    public class DefeatScreen : MonoBehaviour
    {
        #region Inspector Fields
        [Header("UI Components")]
        [Tooltip("Defeat panel container (activated when shown)")]
        [SerializeField] private GameObject defeatPanel;

        [Tooltip("Text displaying rounds survived")]
        [SerializeField] private TextMeshProUGUI survivedRoundsText;

        [Tooltip("Text displaying total gold earned")]
        [SerializeField] private TextMeshProUGUI totalGoldText;

        [Tooltip("Button to restart the game")]
        [SerializeField] private Button restartButton;

        [Tooltip("Button to return to main menu")]
        [SerializeField] private Button returnToMenuButton;

        [Header("Settings")]
        [Tooltip("Scene name to reload for restart")]
        [SerializeField] private string gameSceneName = "GameScene";

        [Tooltip("Scene name to load for main menu")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        [Header("Display Formats")]
        [SerializeField] private string survivedFormat = "Survived {0} Rounds";
        [SerializeField] private string goldFormat = "Total Gold: {0}";
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Hide panel initially
            if (defeatPanel != null)
            {
                defeatPanel.SetActive(false);
            }

            // Setup button listeners
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(RestartGame);
            }

            if (returnToMenuButton != null)
            {
                returnToMenuButton.onClick.AddListener(ReturnToMainMenu);
            }
        }

        private void OnEnable()
        {
            // Subscribe to GameplayManager state changes
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnStateChanged += HandleStateChanged;
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from GameplayManager
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnStateChanged -= HandleStateChanged;
            }

            // Cleanup button listeners
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(RestartGame);
            }

            if (returnToMenuButton != null)
            {
                returnToMenuButton.onClick.RemoveListener(ReturnToMainMenu);
            }
        }
        #endregion

        #region State Management
        /// <summary>
        /// Handle game state changes.
        /// </summary>
        private void HandleStateChanged(GameState oldState, GameState newState)
        {
            if (newState == GameState.Defeat)
            {
                ShowDefeat();
            }
        }
        #endregion

        #region Display
        /// <summary>
        /// Show defeat screen with current game stats.
        /// </summary>
        public void ShowDefeat()
        {
            if (GameplayManager.Instance == null)
            {
                Debug.LogError("[DefeatScreen] GameplayManager not found!");
                return;
            }

            int survivedRounds = GameplayManager.Instance.CurrentRound;
            int totalGold = GameplayManager.Instance.CurrentGold;

            ShowDefeat(survivedRounds, totalGold);
        }

        /// <summary>
        /// Show defeat screen with specific stats.
        /// </summary>
        /// <param name="survivedRounds">Number of rounds survived</param>
        /// <param name="totalGold">Total gold earned</param>
        public void ShowDefeat(int survivedRounds, int totalGold)
        {
            Debug.Log($"[DefeatScreen] Showing defeat - Survived: {survivedRounds} rounds, Gold: {totalGold}");

            // Update text displays
            if (survivedRoundsText != null)
            {
                survivedRoundsText.text = string.Format(survivedFormat, survivedRounds);
            }

            if (totalGoldText != null)
            {
                totalGoldText.text = string.Format(goldFormat, totalGold);
            }

            // Show defeat panel
            if (defeatPanel != null)
            {
                defeatPanel.SetActive(true);
            }

            // Pause game
            Time.timeScale = 0f;

            Debug.Log("[DefeatScreen] Defeat screen displayed");
        }

        /// <summary>
        /// Hide defeat screen.
        /// </summary>
        public void Hide()
        {
            if (defeatPanel != null)
            {
                defeatPanel.SetActive(false);
            }

            // Resume game
            Time.timeScale = 1f;

            Debug.Log("[DefeatScreen] Defeat screen hidden");
        }
        #endregion

        #region Navigation
        /// <summary>
        /// Restart the game from round 1.
        /// </summary>
        private void RestartGame()
        {
            Debug.Log("[DefeatScreen] Restarting game");

            // Resume time before scene transition
            Time.timeScale = 1f;

            // Reload current scene or game scene
            if (!string.IsNullOrEmpty(gameSceneName))
            {
                SceneManager.LoadScene(gameSceneName);
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        /// <summary>
        /// Return to main menu scene.
        /// </summary>
        private void ReturnToMainMenu()
        {
            Debug.Log("[DefeatScreen] Returning to main menu");

            // Resume time before scene transition
            Time.timeScale = 1f;

            // Load main menu scene
            SceneManager.LoadScene(mainMenuSceneName);
        }
        #endregion

        #region Public API
        /// <summary>
        /// Check if defeat screen is currently visible.
        /// </summary>
        public bool IsVisible()
        {
            return defeatPanel != null && defeatPanel.activeSelf;
        }
        #endregion
    }
}
