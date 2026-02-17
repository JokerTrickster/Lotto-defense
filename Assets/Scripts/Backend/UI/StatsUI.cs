using UnityEngine;
using UnityEngine.UI;
using LottoDefense.Backend.Models;

namespace LottoDefense.Backend.UI
{
    /// <summary>
    /// Displays user statistics (highest round, total games, kills, etc.)
    /// </summary>
    public class StatsUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject statsPanel;
        [SerializeField] private Text usernameText;
        [SerializeField] private Text singleStatsText;
        [SerializeField] private Text coopStatsText;
        [SerializeField] private Text goldText;
        [SerializeField] private Text statusText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button refreshButton;

        private void Start()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (refreshButton != null)
                refreshButton.onClick.AddListener(LoadStats);

            // Hide by default
            if (statsPanel != null)
                statsPanel.SetActive(false);
        }

        public void Show()
        {
            if (statsPanel != null)
                statsPanel.SetActive(true);

            LoadStats();
        }

        public void Hide()
        {
            if (statsPanel != null)
                statsPanel.SetActive(false);
        }

        private void LoadStats()
        {
            if (!APIManager.Instance.IsLoggedIn)
            {
                ShowStatus("로그인이 필요합니다", true);
                return;
            }

            ShowStatus("통계 불러오는 중...", false);

            if (usernameText != null)
                usernameText.text = APIManager.Instance.Username;

            APIManager.Instance.GetStats(
                (UserStatsResponse stats) =>
                {
                    ShowStatus("", false);
                    DisplayStats(stats);
                },
                (string error) =>
                {
                    ShowStatus("통계 로드 실패: " + error, true);
                });
        }

        private void DisplayStats(UserStatsResponse stats)
        {
            if (singleStatsText != null)
            {
                singleStatsText.text = string.Format(
                    "<b>싱글 플레이</b>\n" +
                    "최고 라운드: {0}\n" +
                    "총 게임 수: {1}\n" +
                    "총 처치 수: {2}\n" +
                    "평균 라운드: {3:F1}",
                    stats.single.highest_round,
                    stats.single.total_games,
                    stats.single.total_kills,
                    stats.single.average_round
                );
            }

            if (coopStatsText != null)
            {
                singleStatsText.text = string.Format(
                    "<b>협동 플레이</b>\n" +
                    "최고 라운드: {0}\n" +
                    "총 게임 수: {1}\n" +
                    "총 처치 수: {2}\n" +
                    "승리 수: {3}\n" +
                    "승률: {4:P1}",
                    stats.coop.highest_round,
                    stats.coop.total_games,
                    stats.coop.total_kills,
                    stats.coop.wins,
                    stats.coop.win_rate
                );
            }

            if (goldText != null)
            {
                goldText.text = string.Format(
                    "<b>골드</b>\n" +
                    "총 획득: {0}\n" +
                    "보유: {1}",
                    stats.gold.total_earned,
                    stats.gold.current
                );
            }
        }

        private void ShowStatus(string message, bool isError)
        {
            if (statusText != null)
            {
                statusText.text = message;
                statusText.color = isError ? Color.red : Color.white;
            }
        }

        /// <summary>
        /// Create StatsUI dynamically in the given canvas.
        /// </summary>
        public static StatsUI CreateInCanvas(Canvas canvas)
        {
            GameObject statsUIObj = new GameObject("StatsUI");
            statsUIObj.transform.SetParent(canvas.transform, false);

            StatsUI statsUI = statsUIObj.AddComponent<StatsUI>();

            // TODO: Build UI programmatically or load from prefab
            Debug.LogWarning("[StatsUI] UI elements should be assigned via Inspector or created programmatically");

            return statsUI;
        }
    }
}
