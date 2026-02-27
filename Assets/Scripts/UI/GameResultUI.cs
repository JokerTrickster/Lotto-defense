using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using LottoDefense.Gameplay;
using LottoDefense.Networking;
using LottoDefense.Lobby;
using LottoDefense.Units;
using LottoDefense.Profile;
using LottoDefense.Multiplayer;

namespace LottoDefense.UI
{
    /// <summary>
    /// Displays game result screen (Victory or Defeat) with stats and options.
    /// Shows round reached, contribution score, player profiles, and provides button to return to main menu.
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
        [SerializeField] private Text rewardText;

        [Header("Player Profile Display")]
        [SerializeField] private GameObject profileContainer;
        [SerializeField] private Image playerAvatar;
        [SerializeField] private Text playerNickname;
        [SerializeField] private Text playerLevel;

        [Header("Coop Profiles")]
        [SerializeField] private GameObject coopProfileContainer;
        [SerializeField] private Image player1Avatar;
        [SerializeField] private Text player1Nickname;
        [SerializeField] private Image player2Avatar;
        [SerializeField] private Text player2Nickname;
        [SerializeField] private GameObject mvpIndicator;

        [Header("Statistics")]
        [SerializeField] private Text enemiesDefeatedText;
        [SerializeField] private Text unitsPlacedText;
        [SerializeField] private Text survivalTimeText;
        [SerializeField] private Text accuracyText;
        #endregion

        #region Private Fields
        private bool isShowing = false;
        private MatchResultPayload multiplayerResult;
        private bool isCoopMode = false;
        private float gameStartTime;
        private int enemiesDefeated = 0;
        private int unitsPlaced = 0;
        private int shotsHit = 0;
        private int totalShots = 0;
        #endregion

        #region Unity Lifecycle
        /// <summary>
        /// Use Start instead of Awake because GameSceneBootstrapper sets
        /// serialized fields via reflection AFTER AddComponent triggers Awake.
        /// By Start(), all fields are assigned.
        /// </summary>
        private void Start()
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

            // Setup button (fields are now assigned via reflection)
            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            }
            else
            {
                Debug.LogWarning("[GameResultUI] confirmButton is null in Start!");
            }

            // Subscribe to game state changes
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnStateChanged += HandleStateChanged;
            }

            // Subscribe to multiplayer match results
            if (MultiplayerManager.Instance != null)
            {
                MultiplayerManager.Instance.OnMatchResult += HandleMatchResult;
            }

            // Check if in coop mode
            if (CoopGameManager.Instance != null)
            {
                isCoopMode = CoopGameManager.Instance.IsCoopActive;
            }

            // Record game start time
            gameStartTime = Time.time;
        }

        private void OnDestroy()
        {
            // Unsubscribe from game state changes
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnStateChanged -= HandleStateChanged;
            }

            // Unsubscribe from multiplayer match results
            if (MultiplayerManager.Instance != null)
            {
                MultiplayerManager.Instance.OnMatchResult -= HandleMatchResult;
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

        private void HandleMatchResult(MatchResultPayload result)
        {
            multiplayerResult = result;
            ShowResult(result.isWinner);
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
            float survivalTime = Time.time - gameStartTime;

            // Setup player profiles
            SetupPlayerProfiles();

            // Update UI
            if (titleText != null)
            {
                if (isCoopMode)
                {
                    titleText.text = isVictory ? "협동 승리!" : "협동 패배";
                }
                else
                {
                    titleText.text = isVictory ? "승리!" : "게임 오버";
                }
                titleText.color = isVictory ? new Color(0.2f, 1f, 0.3f) : new Color(1f, 0.2f, 0.2f);
            }

            if (roundText != null)
            {
                roundText.text = $"도달 라운드: {roundReached}";
            }

            if (contributionText != null)
            {
                // Show multiplayer comparison if available
                if (multiplayerResult != null)
                {
                    contributionText.text = $"나: R{multiplayerResult.myRound} ({multiplayerResult.myContribution}점)\n" +
                                            $"상대: R{multiplayerResult.opponentRound} ({multiplayerResult.opponentContribution}점)";
                }
                else
                {
                    contributionText.text = $"기여도: {contribution}점";
                }
            }

            // Update statistics
            UpdateStatistics(survivalTime);

            if (confirmButtonText != null)
            {
                confirmButtonText.text = "확인";
            }

            // Grant lobby rewards (before cleanup destroys everything)
            int synthesisCount = SynthesisManager.Instance != null ? SynthesisManager.Instance.SessionSynthesisCount : 0;
            int upgradeCount = UnitUpgradeManager.Instance != null ? UnitUpgradeManager.Instance.SessionUpgradeCount : 0;
            GameDifficulty diff = GameplayManager.Instance != null
                ? GameplayManager.Instance.CurrentDifficulty
                : GameDifficulty.Normal;
            int goldReward = LobbyDataManager.GetGameResultGold(roundReached, diff);
            LobbyDataManager.GrantGameRewards(roundReached, synthesisCount, upgradeCount, diff);

            if (isVictory)
                LobbyDataManager.RecordDifficultyCleared(diff);

            if (rewardText != null)
            {
                rewardText.text = $"보상: +{goldReward} 골드";
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
                elapsed += Time.unscaledDeltaTime;
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

            // Load main menu scene (SceneNavigator.LoadMainGame will handle cleanup)
            SceneNavigator navigator = FindFirstObjectByType<SceneNavigator>();
            if (navigator != null)
            {
                navigator.LoadMainGame();
            }
            else
            {
                HideAllCanvases();
                GameplayManager.CleanupAllGameplaySingletons();
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainGame");
            }
        }
        #endregion

        #region Profile Display
        /// <summary>
        /// Setup player profiles display
        /// </summary>
        private void SetupPlayerProfiles()
        {
            UserProfileManager profileManager = UserProfileManager.Instance;
            if (profileManager == null) return;

            if (isCoopMode && coopProfileContainer != null)
            {
                // Show coop profiles
                if (profileContainer != null)
                    profileContainer.SetActive(false);
                coopProfileContainer.SetActive(true);

                // Player 1 profile - use current user's profile
                Sprite avatarSprite = profileManager.GetCurrentAvatarSprite();
                if (player1Avatar != null && avatarSprite != null)
                    player1Avatar.sprite = avatarSprite;
                if (player1Nickname != null)
                    player1Nickname.text = profileManager.Nickname;

                // Player 2 profile (guest or second player)
                if (player2Avatar != null)
                {
                    // Use default avatar for player 2
                    var defaultAvatar = profileManager.GetAvatarData("avatar_default");
                    if (defaultAvatar != null && defaultAvatar.avatarSprite != null)
                        player2Avatar.sprite = defaultAvatar.avatarSprite;
                }
                if (player2Nickname != null)
                    player2Nickname.text = "Player 2";
            }
            else if (profileContainer != null)
            {
                // Show single player profile
                profileContainer.SetActive(true);
                if (coopProfileContainer != null)
                    coopProfileContainer.SetActive(false);

                // Set current user's profile
                Sprite avatarSprite = profileManager.GetCurrentAvatarSprite();
                if (playerAvatar != null && avatarSprite != null)
                    playerAvatar.sprite = avatarSprite;
                if (playerNickname != null)
                    playerNickname.text = profileManager.Nickname;
                if (playerLevel != null)
                {
                    // Level is not stored in UserProfile, so we'll use rounds reached as level
                    int level = GameplayManager.Instance != null ? GameplayManager.Instance.CurrentRound : 1;
                    playerLevel.text = $"Lv. {level}";
                }
            }
        }

        /// <summary>
        /// Update statistics display
        /// </summary>
        private void UpdateStatistics(float survivalTime)
        {
            // Enemies defeated
            if (enemiesDefeatedText != null)
            {
                enemiesDefeatedText.text = $"적 처치: {enemiesDefeated}";
            }

            // Units placed
            if (unitsPlacedText != null)
            {
                unitsPlacedText.text = $"유닛 배치: {unitsPlaced}";
            }

            // Survival time
            if (survivalTimeText != null)
            {
                int minutes = Mathf.FloorToInt(survivalTime / 60);
                int seconds = Mathf.FloorToInt(survivalTime % 60);
                survivalTimeText.text = $"생존 시간: {minutes:00}:{seconds:00}";
            }

            // Accuracy
            if (accuracyText != null)
            {
                float accuracy = totalShots > 0 ? (shotsHit * 100f / totalShots) : 0f;
                accuracyText.text = $"명중률: {accuracy:F1}%";
            }
        }

        /// <summary>
        /// Track enemy defeated (call from external systems)
        /// </summary>
        public void TrackEnemyDefeated()
        {
            enemiesDefeated++;
        }

        /// <summary>
        /// Track unit placed (call from external systems)
        /// </summary>
        public void TrackUnitPlaced()
        {
            unitsPlaced++;
        }

        /// <summary>
        /// Track shot statistics (call from external systems)
        /// </summary>
        public void TrackShot(bool hit)
        {
            totalShots++;
            if (hit) shotsHit++;
        }

        private static void HideAllCanvases()
        {
            Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas c in canvases)
                c.enabled = false;
        }
        #endregion
    }
}
