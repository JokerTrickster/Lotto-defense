using System;
using System.Collections;
using UnityEngine;
using LottoDefense.Backend.Models;

namespace LottoDefense.Backend
{
    /// <summary>
    /// Singleton manager for backend API communication.
    /// Handles authentication, token persistence, and API calls.
    /// </summary>
    public class APIManager : MonoBehaviour
    {
        #region Singleton
        private static APIManager _instance;
        public static APIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("APIManager");
                    _instance = go.AddComponent<APIManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Load saved token
            LoadToken();
        }
        #endregion

        #region Fields
        private APIClient _client = new APIClient();
        private const string TOKEN_KEY = "td_jwt_token";
        private const string USERNAME_KEY = "td_username";

        public bool IsLoggedIn => _client.IsAuthenticated;
        public string Username { get; private set; }
        #endregion

        #region Token Management
        private void LoadToken()
        {
            if (PlayerPrefs.HasKey(TOKEN_KEY))
            {
                string token = PlayerPrefs.GetString(TOKEN_KEY);
                _client.SetToken(token);

                if (PlayerPrefs.HasKey(USERNAME_KEY))
                {
                    Username = PlayerPrefs.GetString(USERNAME_KEY);
                }

                Debug.Log("[APIManager] Token loaded, user: " + Username);
            }
        }

        private void SaveToken(string token, string username)
        {
            PlayerPrefs.SetString(TOKEN_KEY, token);
            PlayerPrefs.SetString(USERNAME_KEY, username);
            PlayerPrefs.Save();

            _client.SetToken(token);
            Username = username;

            Debug.Log("[APIManager] Token saved, user: " + username);
        }

        public void Logout()
        {
            PlayerPrefs.DeleteKey(TOKEN_KEY);
            PlayerPrefs.DeleteKey(USERNAME_KEY);
            PlayerPrefs.Save();

            _client.ClearToken();
            Username = null;

            Debug.Log("[APIManager] Logged out");
        }
        #endregion

        #region Auth API
        /// <summary>
        /// Register a new user account.
        /// </summary>
        public void Register(string username, string email, string password, Action<AuthResponse> onSuccess, Action<string> onError)
        {
            RegisterRequest request = new RegisterRequest
            {
                username = username,
                email = email,
                password = password
            };

            StartCoroutine(_client.Post("/auth/register", request,
                (AuthResponse response) =>
                {
                    SaveToken(response.token, response.user.username);
                    onSuccess?.Invoke(response);
                },
                onError));
        }

        /// <summary>
        /// Login with email and password.
        /// </summary>
        public void Login(string email, string password, Action<AuthResponse> onSuccess, Action<string> onError)
        {
            LoginRequest request = new LoginRequest
            {
                email = email,
                password = password
            };

            StartCoroutine(_client.Post("/auth/login", request,
                (AuthResponse response) =>
                {
                    SaveToken(response.token, response.user.username);
                    onSuccess?.Invoke(response);
                },
                onError));
        }

        /// <summary>
        /// Get current user info and stats.
        /// </summary>
        public void GetUserInfo(Action<UserInfoResponse> onSuccess, Action<string> onError)
        {
            if (!IsLoggedIn)
            {
                onError?.Invoke("Not logged in");
                return;
            }

            StartCoroutine(_client.Get("/users/me", onSuccess, onError));
        }
        #endregion

        #region Game API
        /// <summary>
        /// Save single-player game result.
        /// </summary>
        public void SaveGameResult(int roundsReached, int monstersKilled, int goldEarned, string result, Action<GameResultResponse> onSuccess, Action<string> onError)
        {
            if (!IsLoggedIn)
            {
                onError?.Invoke("Not logged in");
                return;
            }

            SaveGameResultRequest request = new SaveGameResultRequest
            {
                game_mode = "single",
                rounds_reached = roundsReached,
                monsters_killed = monstersKilled,
                gold_earned = goldEarned,
                result = result
            };

            StartCoroutine(_client.Post("/game/single/result", request, onSuccess, onError));
        }

        /// <summary>
        /// Get user's game statistics.
        /// </summary>
        public void GetStats(Action<UserStatsResponse> onSuccess, Action<string> onError)
        {
            if (!IsLoggedIn)
            {
                onError?.Invoke("Not logged in");
                return;
            }

            StartCoroutine(_client.Get("/users/me/stats", onSuccess, onError));
        }

        /// <summary>
        /// Get game history with pagination.
        /// </summary>
        public void GetGameHistory(int limit, int offset, Action<GameHistoryResponse> onSuccess, Action<string> onError)
        {
            if (!IsLoggedIn)
            {
                onError?.Invoke("Not logged in");
                return;
            }

            string endpoint = $"/game/history?mode=single&limit={limit}&offset={offset}";
            StartCoroutine(_client.Get(endpoint, onSuccess, onError));
        }
        #endregion
    }
}
