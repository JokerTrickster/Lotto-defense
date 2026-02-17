using System;
using System.Collections.Generic;

namespace LottoDefense.Backend.Models
{
    #region Game Requests
    [Serializable]
    public class SaveGameResultRequest
    {
        public string game_mode;            // "single" or "coop"
        public int rounds_reached;          // 도달한 라운드
        public int monsters_killed;         // 처치한 몬스터 수
        public int gold_earned;             // 획득한 골드
        public int? survival_time_seconds;  // 생존 시간 (초)
        public int? final_army_value;       // 최종 군대 가치
        public string result;               // "victory", "defeat", "disconnect"
    }
    #endregion

    #region Game Responses
    [Serializable]
    public class GameResultResponse
    {
        public int game_id;
        public int new_highest_round;
        public List<RewardData> rewards;
    }

    [Serializable]
    public class RewardData
    {
        public string type;    // "gold", "item"
        public int? amount;
        public string item_id;
    }

    [Serializable]
    public class GameHistoryResponse
    {
        public long total;
        public List<GameHistoryItem> games;
    }

    [Serializable]
    public class GameHistoryItem
    {
        public int game_id;
        public string game_mode;
        public int rounds_reached;
        public int monsters_killed;
        public int gold_earned;
        public int? survival_time_seconds;
        public string result;
        public string played_at;
    }

    [Serializable]
    public class UserStatsResponse
    {
        public SingleStats single;
        public CoopStats coop;
        public GoldStats gold;
    }

    [Serializable]
    public class SingleStats
    {
        public int highest_round;
        public int total_games;
        public int total_kills;
        public float average_round;
    }

    [Serializable]
    public class CoopStats
    {
        public int highest_round;
        public int total_games;
        public int total_kills;
        public int wins;
        public float win_rate;
    }

    [Serializable]
    public class GoldStats
    {
        public int total_earned;
        public int current;
    }
    #endregion
}
