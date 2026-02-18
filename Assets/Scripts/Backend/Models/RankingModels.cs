using System;
using System.Collections.Generic;

namespace LottoDefense.Backend.Models
{
    #region Ranking Responses
    [Serializable]
    public class RankingResponse
    {
        public string game_mode;
        public List<RankingItem> rankings;
    }

    [Serializable]
    public class RankingItem
    {
        public int rank;
        public int user_id;
        public string username;
        public int rounds_reached;
        public int? survival_time_seconds;
        public float survival_minutes;
        public string played_at;

        // Co-op only
        public int? player2_id;
        public string player2_username;

        /// <summary>
        /// Get formatted survival time (e.g., "3:45" for 3 minutes 45 seconds)
        /// </summary>
        public string GetFormattedTime()
        {
            if (survival_time_seconds == null || survival_time_seconds == 0)
                return "--:--";

            int totalSeconds = survival_time_seconds.Value;
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            return string.Format("{0}:{1:00}", minutes, seconds);
        }

        /// <summary>
        /// Get formatted minutes (e.g., "3.8분")
        /// </summary>
        public string GetFormattedMinutes()
        {
            if (survival_minutes <= 0)
                return "--";

            return string.Format("{0:F1}분", survival_minutes);
        }
    }
    #endregion
}
