using UnityEngine;
using UnityEngine.UI;
using LottoDefense.Backend.Models;
using System.Text;

namespace LottoDefense.Backend.UI
{
    /// <summary>
    /// Displays weekly rankings (top 10, last 7 days).
    /// Shows single-player and co-op rankings with rounds reached and clear time.
    /// </summary>
    public class RankingUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject rankingPanel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text rankingListText;
        [SerializeField] private Text statusText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button singleButton;
        [SerializeField] private Button coopButton;
        [SerializeField] private Button refreshButton;

        [Header("Settings")]
        [SerializeField] private bool showOnStart = false;

        private string currentMode = "single";

        private void Start()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (singleButton != null)
                singleButton.onClick.AddListener(() => LoadRankings("single"));

            if (coopButton != null)
                coopButton.onClick.AddListener(() => LoadRankings("coop"));

            if (refreshButton != null)
                refreshButton.onClick.AddListener(() => LoadRankings(currentMode));

            if (rankingPanel != null)
                rankingPanel.SetActive(showOnStart);

            if (showOnStart)
                LoadRankings("single");
        }

        public void Show()
        {
            if (rankingPanel != null)
                rankingPanel.SetActive(true);

            LoadRankings(currentMode);
        }

        public void Hide()
        {
            if (rankingPanel != null)
                rankingPanel.SetActive(false);
        }

        private void LoadRankings(string gameMode)
        {
            currentMode = gameMode;

            ShowStatus("랭킹 불러오는 중...", false);

            if (titleText != null)
            {
                titleText.text = gameMode == "single" ? "싱글 플레이 랭킹 (주간)" : "협동 플레이 랭킹 (주간)";
            }

            APIManager.Instance.GetWeeklyRankings(gameMode,
                (RankingResponse response) =>
                {
                    ShowStatus("", false);
                    DisplayRankings(response);
                },
                (string error) =>
                {
                    ShowStatus("랭킹 로드 실패: " + error, true);
                    ClearRankingList();
                });
        }

        private void DisplayRankings(RankingResponse response)
        {
            if (response.rankings == null || response.rankings.Count == 0)
            {
                if (rankingListText != null)
                    rankingListText.text = "아직 랭킹 기록이 없습니다.";
                return;
            }

            StringBuilder sb = new StringBuilder();

            if (response.game_mode == "single")
            {
                // Single-player: Rank | Username | Rounds | Time
                sb.AppendLine("순위   유저명             층수   시간");
                sb.AppendLine("─────────────────────────────────");

                foreach (var item in response.rankings)
                {
                    string rankStr = item.rank.ToString().PadRight(6);
                    string usernameStr = TruncateString(item.username, 15).PadRight(15);
                    string roundsStr = item.rounds_reached.ToString().PadLeft(4);
                    string timeStr = item.GetFormattedMinutes().PadLeft(8);

                    sb.AppendLine($"{rankStr} {usernameStr} {roundsStr}   {timeStr}");
                }
            }
            else
            {
                // Co-op: Rank | Player1 + Player2 | Rounds | Time
                sb.AppendLine("순위   플레이어              층수   시간");
                sb.AppendLine("─────────────────────────────────");

                foreach (var item in response.rankings)
                {
                    string rankStr = item.rank.ToString().PadRight(6);
                    
                    string players = item.username;
                    if (!string.IsNullOrEmpty(item.player2_username))
                    {
                        players += " + " + item.player2_username;
                    }
                    string playersStr = TruncateString(players, 18).PadRight(18);

                    string roundsStr = item.rounds_reached.ToString().PadLeft(4);
                    string timeStr = item.GetFormattedMinutes().PadLeft(8);

                    sb.AppendLine($"{rankStr} {playersStr} {roundsStr}   {timeStr}");
                }
            }

            if (rankingListText != null)
                rankingListText.text = sb.ToString();
        }

        private void ClearRankingList()
        {
            if (rankingListText != null)
                rankingListText.text = "";
        }

        private void ShowStatus(string message, bool isError)
        {
            if (statusText != null)
            {
                statusText.text = message;
                statusText.color = isError ? Color.red : Color.white;
            }
        }

        private string TruncateString(string str, int maxLength)
        {
            if (string.IsNullOrEmpty(str))
                return "";

            if (str.Length <= maxLength)
                return str;

            return str.Substring(0, maxLength - 2) + "..";
        }

        /// <summary>
        /// Create RankingUI dynamically in the given canvas.
        /// </summary>
        public static RankingUI CreateInCanvas(Canvas canvas)
        {
            GameObject rankingUIObj = new GameObject("RankingUI");
            rankingUIObj.transform.SetParent(canvas.transform, false);

            RankingUI rankingUI = rankingUIObj.AddComponent<RankingUI>();

            // TODO: Build UI programmatically or load from prefab
            Debug.LogWarning("[RankingUI] UI elements should be assigned via Inspector or created programmatically");

            return rankingUI;
        }
    }
}
