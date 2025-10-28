using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using LottoDefense.Gameplay;

namespace LottoDefense.UI
{
    /// <summary>
    /// Victory screen displayed when player completes all rounds successfully.
    /// Shows final stats and provides option to return to main menu.
    /// </summary>
    public class VictoryScreen : MonoBehaviour
    {
        #region Inspector Fields
        [Header("UI Components")]
        [Tooltip("Victory panel container (activated when shown)")]
        [SerializeField] private GameObject victoryPanel;

        [Tooltip("Text displaying final round number")]
        [SerializeField] private TextMeshProUGUI finalRoundText;

        [Tooltip("Text displaying remaining life")]
        [SerializeField] private TextMeshProUGUI remainingLifeText;

        [Tooltip("Text displaying total gold earned")]
        [SerializeField] private TextMeshProUGUI totalGoldText;

        [Tooltip("Button to return to main menu")]
        [SerializeField] private Button returnToMenuButton;

        [Header("Settings")]
        [Tooltip("Scene name to load for main menu")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        [Header("Display Formats")]
        [SerializeField] private string roundFormat = "Round {0} Complete!";
        [SerializeField] private string lifeFormat = "Remaining Life: {0}";
        [SerializeField] private string goldFormat = "Total Gold: {0}";
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Hide panel initially
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(false);
            }

            // Setup button listeners
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
            if (newState == GameState.Victory)
            {
                ShowVictory();
            }
        }
        #endregion

        #region Display
        /// <summary>
        /// Show victory screen with current game stats.
        /// </summary>
        public void ShowVictory()
        {
            if (GameplayManager.Instance == null)
            {
                Debug.LogError("[VictoryScreen] GameplayManager not found!");
                return;
            }

            int finalRound = GameplayManager.Instance.CurrentRound;
            int remainingLife = GameplayManager.Instance.CurrentLife;
            int totalGold = GameplayManager.Instance.CurrentGold;

            ShowVictory(finalRound, remainingLife, totalGold);
        }

        /// <summary>
        /// Show victory screen with specific stats.
        /// </summary>
        /// <param name="finalRound">Final round completed</param>
        /// <param name="remainingLife">Life remaining</param>
        /// <param name="totalGold">Total gold earned</param>
        public void ShowVictory(int finalRound, int remainingLife, int totalGold)
        {
            Debug.Log($"[VictoryScreen] Showing victory - Round: {finalRound}, Life: {remainingLife}, Gold: {totalGold}");

            // Update text displays
            if (finalRoundText != null)
            {
                finalRoundText.text = string.Format(roundFormat, finalRound);
            }

            if (remainingLifeText != null)
            {
                remainingLifeText.text = string.Format(lifeFormat, remainingLife);
            }

            if (totalGoldText != null)
            {
                totalGoldText.text = string.Format(goldFormat, totalGold);
            }

            // Show victory panel
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
            }

            // Pause game
            Time.timeScale = 0f;

            Debug.Log("[VictoryScreen] Victory screen displayed");
        }

        /// <summary>
        /// Hide victory screen.
        /// </summary>
        public void Hide()
        {
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(false);
            }

            // Resume game
            Time.timeScale = 1f;

            Debug.Log("[VictoryScreen] Victory screen hidden");
        }
        #endregion

        #region Navigation
        /// <summary>
        /// Return to main menu scene.
        /// </summary>
        private void ReturnToMainMenu()
        {
            Debug.Log("[VictoryScreen] Returning to main menu");

            // Resume time before scene transition
            Time.timeScale = 1f;

            // Load main menu scene
            SceneManager.LoadScene(mainMenuSceneName);
        }
        #endregion

        #region Public API
        /// <summary>
        /// Check if victory screen is currently visible.
        /// </summary>
        public bool IsVisible()
        {
            return victoryPanel != null && victoryPanel.activeSelf;
        }
        #endregion
    }
}
