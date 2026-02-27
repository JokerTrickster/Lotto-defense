using UnityEngine;

namespace LottoDefense.Config
{
    /// <summary>
    /// Environment configuration ScriptableObject for managing different server environments.
    /// Supports development, staging, and production environments with their respective endpoints.
    /// </summary>
    [CreateAssetMenu(fileName = "EnvironmentConfig", menuName = "LottoDefense/Config/Environment")]
    public class EnvironmentConfig : ScriptableObject
    {
        [Header("Environment Settings")]
        [SerializeField] private EnvironmentType environmentType = EnvironmentType.Development;

        [Header("API Endpoints")]
        [SerializeField] private string developmentApiUrl = "https://dev-api.lotto-defense.com";
        [SerializeField] private string stagingApiUrl = "https://staging-api.lotto-defense.com";
        [SerializeField] private string productionApiUrl = "https://api.lotto-defense.com";

        [Header("WebSocket Endpoints")]
        [SerializeField] private string developmentWsUrl = "wss://dev-ws.lotto-defense.com";
        [SerializeField] private string stagingWsUrl = "wss://staging-ws.lotto-defense.com";
        [SerializeField] private string productionWsUrl = "wss://ws.lotto-defense.com";

        [Header("Security Settings")]
        [SerializeField] private bool useSecureStorage = true;
        [SerializeField] private bool enableSslPinning = false;
        [SerializeField] private int tokenRefreshThreshold = 300; // seconds before expiry

        [Header("Network Settings")]
        [SerializeField] private int connectionTimeout = 30; // seconds
        [SerializeField] private int maxRetryAttempts = 3;
        [SerializeField] private float retryDelayBase = 1f; // seconds for exponential backoff
        [SerializeField] private int maxConcurrentRequests = 5;

        [Header("Debug Settings")]
        [SerializeField] private bool enableLogging = true;
        [SerializeField] private bool verboseLogging = false;
        [SerializeField] private bool logNetworkCalls = true;

        public enum EnvironmentType
        {
            Development,
            Staging,
            Production
        }

        #region Properties
        public EnvironmentType Environment => environmentType;

        public string ApiUrl
        {
            get
            {
                switch (environmentType)
                {
                    case EnvironmentType.Development:
                        return developmentApiUrl;
                    case EnvironmentType.Staging:
                        return stagingApiUrl;
                    case EnvironmentType.Production:
                        return productionApiUrl;
                    default:
                        return developmentApiUrl;
                }
            }
        }

        public string WebSocketUrl
        {
            get
            {
                switch (environmentType)
                {
                    case EnvironmentType.Development:
                        return developmentWsUrl;
                    case EnvironmentType.Staging:
                        return stagingWsUrl;
                    case EnvironmentType.Production:
                        return productionWsUrl;
                    default:
                        return developmentWsUrl;
                }
            }
        }

        public bool UseSecureStorage => useSecureStorage;
        public bool EnableSslPinning => enableSslPinning;
        public int TokenRefreshThreshold => tokenRefreshThreshold;
        public int ConnectionTimeout => connectionTimeout;
        public int MaxRetryAttempts => maxRetryAttempts;
        public float RetryDelayBase => retryDelayBase;
        public int MaxConcurrentRequests => maxConcurrentRequests;
        public bool EnableLogging => enableLogging;
        public bool VerboseLogging => verboseLogging;
        public bool LogNetworkCalls => logNetworkCalls;
        #endregion

        #region Public Methods
        /// <summary>
        /// Switch to a different environment
        /// </summary>
        public void SetEnvironment(EnvironmentType newEnvironment)
        {
            environmentType = newEnvironment;
            Debug.Log($"[EnvironmentConfig] Switched to {newEnvironment} environment");
        }

        /// <summary>
        /// Get full API endpoint URL
        /// </summary>
        public string GetApiEndpoint(string endpoint)
        {
            if (!endpoint.StartsWith("/"))
                endpoint = "/" + endpoint;
            return ApiUrl + endpoint;
        }

        /// <summary>
        /// Get full WebSocket endpoint URL
        /// </summary>
        public string GetWebSocketEndpoint(string endpoint)
        {
            if (!endpoint.StartsWith("/"))
                endpoint = "/" + endpoint;
            return WebSocketUrl + endpoint;
        }

        /// <summary>
        /// Validate configuration
        /// </summary>
        public bool Validate()
        {
            bool isValid = true;

            if (string.IsNullOrEmpty(ApiUrl))
            {
                Debug.LogError("[EnvironmentConfig] API URL is not configured");
                isValid = false;
            }

            if (string.IsNullOrEmpty(WebSocketUrl))
            {
                Debug.LogError("[EnvironmentConfig] WebSocket URL is not configured");
                isValid = false;
            }

            if (connectionTimeout <= 0)
            {
                Debug.LogError("[EnvironmentConfig] Invalid connection timeout");
                isValid = false;
            }

            if (maxRetryAttempts < 0)
            {
                Debug.LogError("[EnvironmentConfig] Invalid max retry attempts");
                isValid = false;
            }

            return isValid;
        }
        #endregion
    }
}