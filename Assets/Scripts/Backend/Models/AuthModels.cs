using System;

namespace LottoDefense.Backend.Models
{
    #region Auth Requests
    [Serializable]
    public class RegisterRequest
    {
        public string username;
        public string email;
        public string password;
    }

    [Serializable]
    public class LoginRequest
    {
        public string email;
        public string password;
    }
    #endregion

    #region Auth Responses
    [Serializable]
    public class AuthResponse
    {
        public UserData user;
        public string token;
    }

    [Serializable]
    public class UserData
    {
        public int id;
        public string username;
        public string email;
        public string created_at;
        public string last_login;
    }

    [Serializable]
    public class UserInfoResponse
    {
        public UserData user;
        public UserStatsData stats;
    }

    [Serializable]
    public class UserStatsData
    {
        public int single_highest_round;
        public int single_total_games;
        public int single_total_kills;
        public int coop_highest_round;
        public int coop_total_games;
        public int coop_total_kills;
        public int coop_wins;
        public int total_gold_earned;
        public int current_gold;
        public int quests_completed;
    }
    #endregion
}
