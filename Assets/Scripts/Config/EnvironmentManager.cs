using UnityEngine;
using System;

namespace LottoDefense.Config
{
    /// <summary>
    /// Singleton manager for environment configuration.
    /// Handles loading and providing access to environment settings.
    /// </summary>
    public class EnvironmentManager : MonoBehaviour
    {
        #region Singleton
        private static EnvironmentManager _instance;
        public static EnvironmentManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("EnvironmentManager");
                    _instance = go.AddComponent<EnvironmentManager>();
                    DontDestroyOnLoad(go);
                    _instance.Initialize();
                }
                return _instance;
            }
        }
        #endregion

        #region Private Fields
        private EnvironmentConfig _config;
        private const string CONFIG_PATH = "Config/EnvironmentConfig";
        #endregion

        #region Public Properties
        public EnvironmentConfig Config => _config;
        public bool IsInitialized => _config != null;
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
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize the environment manager by loading configuration
        /// </summary>
        private void Initialize()
        {
            LoadConfiguration();
            ValidateConfiguration();
            LogEnvironmentInfo();
        }

        /// <summary>
        /// Load environment configuration from Resources
        /// </summary>
        private void LoadConfiguration()
        {
            _config = Resources.Load<EnvironmentConfig>(CONFIG_PATH);

            if (_config == null)
            {
                Debug.LogError($"[EnvironmentManager] Failed to load EnvironmentConfig from {CONFIG_PATH}");
                CreateDefaultConfiguration();
            }
        }

        /// <summary>
        /// Create default configuration if none exists
        /// </summary>
        private void CreateDefaultConfiguration()
        {
            _config = ScriptableObject.CreateInstance<EnvironmentConfig>();
            Debug.LogWarning("[EnvironmentManager] Using default configuration. Please create EnvironmentConfig asset.");
        }

        /// <summary>
        /// Validate loaded configuration
        /// </summary>
        private void ValidateConfiguration()
        {
            if (_config != null && !_config.Validate())
            {
                Debug.LogError("[EnvironmentManager] Configuration validation failed");
            }
        }

        /// <summary>
        /// Log current environment information
        /// </summary>
        private void LogEnvironmentInfo()
        {
            if (_config != null && _config.EnableLogging)
            {
                Debug.Log($"[EnvironmentManager] Environment: {_config.Environment}");
                Debug.Log($"[EnvironmentManager] API URL: {_config.ApiUrl}");
                Debug.Log($"[EnvironmentManager] WebSocket URL: {_config.WebSocketUrl}");
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get API endpoint URL
        /// </summary>
        public string GetApiEndpoint(string endpoint)
        {
            if (_config == null)
            {
                Debug.LogError("[EnvironmentManager] Configuration not loaded");
                return null;
            }
            return _config.GetApiEndpoint(endpoint);
        }

        /// <summary>
        /// Get WebSocket endpoint URL
        /// </summary>
        public string GetWebSocketEndpoint(string endpoint)
        {
            if (_config == null)
            {
                Debug.LogError("[EnvironmentManager] Configuration not loaded");
                return null;
            }
            return _config.GetWebSocketEndpoint(endpoint);
        }

        /// <summary>
        /// Switch environment at runtime
        /// </summary>
        public void SwitchEnvironment(EnvironmentConfig.EnvironmentType newEnvironment)
        {
            if (_config != null)
            {
                _config.SetEnvironment(newEnvironment);
                LogEnvironmentInfo();

                // Notify other systems of environment change
                OnEnvironmentChanged?.Invoke(newEnvironment);
            }
        }

        /// <summary>
        /// Reload configuration from disk
        /// </summary>
        public void ReloadConfiguration()
        {
            LoadConfiguration();
            ValidateConfiguration();
            LogEnvironmentInfo();
        }
        #endregion

        #region Events
        public event Action<EnvironmentConfig.EnvironmentType> OnEnvironmentChanged;
        #endregion
    }
}