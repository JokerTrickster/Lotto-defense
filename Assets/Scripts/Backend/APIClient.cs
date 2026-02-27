using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using LottoDefense.Config;
using LottoDefense.Security;

namespace LottoDefense.Backend
{
    /// <summary>
    /// HTTP client for Tower Defense backend API.
    /// Handles all REST API communication with JWT authentication, retry logic, and secure storage.
    /// </summary>
    public class APIClient : MonoBehaviour
    {
        #region Singleton
        private static APIClient _instance;
        public static APIClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("APIClient");
                    _instance = go.AddComponent<APIClient>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        #endregion

        #region Private Fields
        private HttpRetryHandler retryHandler;
        private Queue<IEnumerator> requestQueue = new Queue<IEnumerator>();
        private bool isProcessingQueue = false;
        private int maxConcurrentRequests = 5;
        private int currentRequests = 0;
        #endregion

        #region Public Properties
        public bool IsAuthenticated => SecureTokenStorage.Instance != null && SecureTokenStorage.Instance.IsTokenValid();
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }

        private void Initialize()
        {
            // Create retry handler
            GameObject retryGo = new GameObject("HttpRetryHandler");
            retryGo.transform.SetParent(transform);
            retryHandler = retryGo.AddComponent<HttpRetryHandler>();

            // Load max concurrent requests from config
            if (EnvironmentManager.Instance?.Config != null)
            {
                maxConcurrentRequests = EnvironmentManager.Instance.Config.MaxConcurrentRequests;
            }
        }
        #endregion

        #region Token Management
        public void SetToken(string token)
        {
            SetToken(token, long.MaxValue);
        }

        public void SetToken(string token, long expiryTime)
        {
            SecureTokenStorage.Instance.StoreAccessToken(token, expiryTime);
        }

        public void SetRefreshToken(string token)
        {
            SecureTokenStorage.Instance.StoreRefreshToken(token);
        }

        public void ClearToken()
        {
            SecureTokenStorage.Instance.ClearTokens();
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        public IEnumerator RefreshToken(Action<bool> onComplete)
        {
            string refreshToken = SecureTokenStorage.Instance.GetRefreshToken();

            if (string.IsNullOrEmpty(refreshToken))
            {
                Debug.LogError("[APIClient] No refresh token available");
                onComplete?.Invoke(false);
                yield break;
            }

            var request = new RefreshTokenRequest { refreshToken = refreshToken };

            yield return Post("/auth/refresh", request,
                (RefreshTokenResponse response) =>
                {
                    SetToken(response.accessToken, response.expiresIn);
                    Debug.Log("[APIClient] Token refreshed successfully");
                    onComplete?.Invoke(true);
                },
                (error) =>
                {
                    Debug.LogError($"[APIClient] Token refresh failed: {error}");
                    ClearToken();
                    onComplete?.Invoke(false);
                }
            );
        }

        /// <summary>
        /// Ensure token is valid before making request
        /// </summary>
        private IEnumerator EnsureValidToken(Action<bool> onComplete)
        {
            if (SecureTokenStorage.Instance.IsTokenValid())
            {
                onComplete?.Invoke(true);
                yield break;
            }

            if (SecureTokenStorage.Instance.NeedsTokenRefresh())
            {
                yield return RefreshToken(onComplete);
            }
            else
            {
                onComplete?.Invoke(false);
            }
        }
        #endregion

        #region POST Requests
        public IEnumerator Post<TRequest, TResponse>(string endpoint, TRequest body, Action<TResponse> onSuccess, Action<string> onError)
        {
            // Ensure valid token
            bool tokenValid = false;
            yield return EnsureValidToken((valid) => tokenValid = valid);

            if (!tokenValid && !endpoint.Contains("/auth/"))
            {
                onError?.Invoke("Authentication required");
                yield break;
            }

            string url = EnvironmentManager.Instance.GetApiEndpoint(endpoint);
            string jsonBody = JsonUtility.ToJson(body);

            UnityWebRequest request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // Add auth header if available
            string token = SecureTokenStorage.Instance?.GetAccessToken();
            if (!string.IsNullOrEmpty(token))
            {
                request.SetRequestHeader("Authorization", "Bearer " + token);
            }

            // Set timeout from config
            if (EnvironmentManager.Instance?.Config != null)
            {
                request.timeout = EnvironmentManager.Instance.Config.ConnectionTimeout;
            }

            // Execute with retry logic
            yield return retryHandler.ExecuteWithRetry(request,
                (response) =>
                {
                    try
                    {
                        APIResponse<TResponse> apiResponse = JsonUtility.FromJson<APIResponse<TResponse>>(response);
                        if (apiResponse.success)
                        {
                            onSuccess?.Invoke(apiResponse.data);
                        }
                        else
                        {
                            onError?.Invoke(apiResponse.message ?? "Request failed");
                        }
                    }
                    catch (Exception e)
                    {
                        onError?.Invoke($"Parse error: {e.Message}");
                    }
                },
                onError
            );
        }

        /// <summary>
        /// POST request without authentication
        /// </summary>
        public IEnumerator PostNoAuth<TRequest, TResponse>(string endpoint, TRequest body, Action<TResponse> onSuccess, Action<string> onError)
        {
            string url = EnvironmentManager.Instance.GetApiEndpoint(endpoint);
            string jsonBody = JsonUtility.ToJson(body);

            UnityWebRequest request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // Set timeout from config
            if (EnvironmentManager.Instance?.Config != null)
            {
                request.timeout = EnvironmentManager.Instance.Config.ConnectionTimeout;
            }

            // Execute with retry logic
            yield return retryHandler.ExecuteWithRetry(request,
                (response) =>
                {
                    try
                    {
                        APIResponse<TResponse> apiResponse = JsonUtility.FromJson<APIResponse<TResponse>>(response);
                        if (apiResponse.success)
                        {
                            onSuccess?.Invoke(apiResponse.data);
                        }
                        else
                        {
                            onError?.Invoke(apiResponse.message ?? "Request failed");
                        }
                    }
                    catch (Exception e)
                    {
                        onError?.Invoke($"Parse error: {e.Message}");
                    }
                },
                onError
            );
        }
        #endregion

        #region GET Requests
        public IEnumerator Get<TResponse>(string endpoint, Action<TResponse> onSuccess, Action<string> onError)
        {
            // Ensure valid token
            bool tokenValid = false;
            yield return EnsureValidToken((valid) => tokenValid = valid);

            if (!tokenValid && !endpoint.Contains("/auth/"))
            {
                onError?.Invoke("Authentication required");
                yield break;
            }

            string url = EnvironmentManager.Instance.GetApiEndpoint(endpoint);

            UnityWebRequest request = UnityWebRequest.Get(url);

            // Add auth header if available
            string token = SecureTokenStorage.Instance?.GetAccessToken();
            if (!string.IsNullOrEmpty(token))
            {
                request.SetRequestHeader("Authorization", "Bearer " + token);
            }

            // Set timeout from config
            if (EnvironmentManager.Instance?.Config != null)
            {
                request.timeout = EnvironmentManager.Instance.Config.ConnectionTimeout;
            }

            // Execute with retry logic
            yield return retryHandler.ExecuteWithRetry(request,
                (response) =>
                {
                    try
                    {
                        APIResponse<TResponse> apiResponse = JsonUtility.FromJson<APIResponse<TResponse>>(response);
                        if (apiResponse.success)
                        {
                            onSuccess?.Invoke(apiResponse.data);
                        }
                        else
                        {
                            onError?.Invoke(apiResponse.message ?? "Request failed");
                        }
                    }
                    catch (Exception e)
                    {
                        onError?.Invoke($"Parse error: {e.Message}");
                    }
                },
                onError
            );
        }

        /// <summary>
        /// PUT request with authentication
        /// </summary>
        public IEnumerator Put<TRequest, TResponse>(string endpoint, TRequest body, Action<TResponse> onSuccess, Action<string> onError)
        {
            // Ensure valid token
            bool tokenValid = false;
            yield return EnsureValidToken((valid) => tokenValid = valid);

            if (!tokenValid)
            {
                onError?.Invoke("Authentication required");
                yield break;
            }

            string url = EnvironmentManager.Instance.GetApiEndpoint(endpoint);
            string jsonBody = JsonUtility.ToJson(body);

            UnityWebRequest request = new UnityWebRequest(url, "PUT");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // Add auth header
            string token = SecureTokenStorage.Instance.GetAccessToken();
            request.SetRequestHeader("Authorization", "Bearer " + token);

            // Set timeout from config
            if (EnvironmentManager.Instance?.Config != null)
            {
                request.timeout = EnvironmentManager.Instance.Config.ConnectionTimeout;
            }

            // Execute with retry logic
            yield return retryHandler.ExecuteWithRetry(request,
                (response) =>
                {
                    try
                    {
                        APIResponse<TResponse> apiResponse = JsonUtility.FromJson<APIResponse<TResponse>>(response);
                        if (apiResponse.success)
                        {
                            onSuccess?.Invoke(apiResponse.data);
                        }
                        else
                        {
                            onError?.Invoke(apiResponse.message ?? "Request failed");
                        }
                    }
                    catch (Exception e)
                    {
                        onError?.Invoke($"Parse error: {e.Message}");
                    }
                },
                onError
            );
        }

        /// <summary>
        /// DELETE request with authentication
        /// </summary>
        public IEnumerator Delete<TResponse>(string endpoint, Action<TResponse> onSuccess, Action<string> onError)
        {
            // Ensure valid token
            bool tokenValid = false;
            yield return EnsureValidToken((valid) => tokenValid = valid);

            if (!tokenValid)
            {
                onError?.Invoke("Authentication required");
                yield break;
            }

            string url = EnvironmentManager.Instance.GetApiEndpoint(endpoint);

            UnityWebRequest request = UnityWebRequest.Delete(url);
            request.downloadHandler = new DownloadHandlerBuffer();

            // Add auth header
            string token = SecureTokenStorage.Instance.GetAccessToken();
            request.SetRequestHeader("Authorization", "Bearer " + token);

            // Set timeout from config
            if (EnvironmentManager.Instance?.Config != null)
            {
                request.timeout = EnvironmentManager.Instance.Config.ConnectionTimeout;
            }

            // Execute with retry logic
            yield return retryHandler.ExecuteWithRetry(request,
                (response) =>
                {
                    try
                    {
                        APIResponse<TResponse> apiResponse = JsonUtility.FromJson<APIResponse<TResponse>>(response);
                        if (apiResponse.success)
                        {
                            onSuccess?.Invoke(apiResponse.data);
                        }
                        else
                        {
                            onError?.Invoke(apiResponse.message ?? "Request failed");
                        }
                    }
                    catch (Exception e)
                    {
                        onError?.Invoke($"Parse error: {e.Message}");
                    }
                },
                onError
            );
        }
        #endregion

        #region Request Queue Management
        /// <summary>
        /// Add request to queue for rate limiting
        /// </summary>
        public void QueueRequest(IEnumerator request)
        {
            requestQueue.Enqueue(request);
            if (!isProcessingQueue)
            {
                StartCoroutine(ProcessRequestQueue());
            }
        }

        /// <summary>
        /// Process queued requests with rate limiting
        /// </summary>
        private IEnumerator ProcessRequestQueue()
        {
            isProcessingQueue = true;

            while (requestQueue.Count > 0)
            {
                // Wait if too many concurrent requests
                while (currentRequests >= maxConcurrentRequests)
                {
                    yield return new WaitForSeconds(0.1f);
                }

                IEnumerator request = requestQueue.Dequeue();
                currentRequests++;

                // Start request and track completion
                StartCoroutine(ExecuteAndTrack(request));

                // Small delay between starting requests
                yield return new WaitForSeconds(0.05f);
            }

            isProcessingQueue = false;
        }

        /// <summary>
        /// Execute request and track completion
        /// </summary>
        private IEnumerator ExecuteAndTrack(IEnumerator request)
        {
            yield return request;
            currentRequests--;
        }
        #endregion

        #region Response Wrapper
        [Serializable]
        private class APIResponse<T>
        {
            public bool success;
            public T data;
            public string message;
            public int code;
        }
        #endregion

        #region Request/Response Models
        [Serializable]
        public class RefreshTokenRequest
        {
            public string refreshToken;
        }

        [Serializable]
        public class RefreshTokenResponse
        {
            public string accessToken;
            public long expiresIn;
        }

        [Serializable]
        public class LoginRequest
        {
            public string deviceId;
            public string nickname;
            public string platform;
        }

        [Serializable]
        public class LoginResponse
        {
            public uint playerId;
            public string accessToken;
            public string refreshToken;
            public long expiresIn;
        }

        [Serializable]
        public class GameStartRequest
        {
            public string difficulty;
            public string gameMode;
        }

        [Serializable]
        public class GameStartResponse
        {
            public string sessionId;
            public long startTime;
        }

        [Serializable]
        public class GameResultRequest
        {
            public string sessionId;
            public int score;
            public int roundReached;
            public int monstersKilled;
            public int goldEarned;
            public float playTime;
        }

        [Serializable]
        public class GameResultResponse
        {
            public int expGained;
            public int goldReward;
            public bool newLevel;
            public int currentLevel;
        }

        [Serializable]
        public class LeaderboardEntry
        {
            public uint playerId;
            public string nickname;
            public int score;
            public int rank;
            public long timestamp;
        }

        [Serializable]
        public class LeaderboardResponse
        {
            public LeaderboardEntry[] entries;
            public int totalPlayers;
            public LeaderboardEntry myRank;
        }
        #endregion

        #region Authentication API
        /// <summary>
        /// Login with device ID
        /// </summary>
        public IEnumerator Login(string nickname, Action<LoginResponse> onSuccess, Action<string> onError)
        {
            var request = new LoginRequest
            {
                deviceId = SystemInfo.deviceUniqueIdentifier,
                nickname = nickname,
                platform = Application.platform.ToString()
            };

            yield return PostNoAuth("/auth/login", request,
                (LoginResponse response) =>
                {
                    // Store tokens securely
                    SetToken(response.accessToken, response.expiresIn);
                    SetRefreshToken(response.refreshToken);

                    // Store player ID
                    PlayerPrefs.SetInt("PlayerId", (int)response.playerId);
                    PlayerPrefs.Save();

                    onSuccess?.Invoke(response);
                },
                onError
            );
        }

        /// <summary>
        /// Logout and clear tokens
        /// </summary>
        public IEnumerator Logout(Action onSuccess, Action<string> onError)
        {
            yield return Post<object, object>("/auth/logout", null,
                (_) =>
                {
                    ClearToken();
                    onSuccess?.Invoke();
                },
                onError
            );
        }
        #endregion

        #region Game API
        /// <summary>
        /// Start new game session
        /// </summary>
        public IEnumerator StartGame(string difficulty, string gameMode, Action<GameStartResponse> onSuccess, Action<string> onError)
        {
            var request = new GameStartRequest
            {
                difficulty = difficulty,
                gameMode = gameMode
            };

            yield return Post("/game/start", request, onSuccess, onError);
        }

        /// <summary>
        /// Submit game result
        /// </summary>
        public IEnumerator SubmitGameResult(GameResultRequest result, Action<GameResultResponse> onSuccess, Action<string> onError)
        {
            yield return Post("/game/result", result, onSuccess, onError);
        }

        /// <summary>
        /// Get leaderboard
        /// </summary>
        public IEnumerator GetLeaderboard(string timeframe, int limit, Action<LeaderboardResponse> onSuccess, Action<string> onError)
        {
            yield return Get($"/leaderboard?timeframe={timeframe}&limit={limit}", onSuccess, onError);
        }
        #endregion
    }
}
