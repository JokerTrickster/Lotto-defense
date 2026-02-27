using System;
using System.Text;
using UnityEngine;
using System.Security.Cryptography;
using LottoDefense.Config;

namespace LottoDefense.Security
{
    /// <summary>
    /// Secure storage for authentication tokens with encryption.
    /// Uses device-specific keys and Unity PlayerPrefs with AES encryption.
    /// </summary>
    public class SecureTokenStorage : MonoBehaviour
    {
        #region Singleton
        private static SecureTokenStorage _instance;
        public static SecureTokenStorage Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("SecureTokenStorage");
                    _instance = go.AddComponent<SecureTokenStorage>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        #endregion

        #region Constants
        private const string ACCESS_TOKEN_KEY = "ltd_at";
        private const string REFRESH_TOKEN_KEY = "ltd_rt";
        private const string TOKEN_EXPIRY_KEY = "ltd_exp";
        private const string DEVICE_ID_KEY = "ltd_did";
        private const string SALT_KEY = "ltd_salt";
        #endregion

        #region Private Fields
        private string deviceKey;
        private bool useEncryption = true;
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
        private void Initialize()
        {
            // Check if encryption should be used from config
            if (EnvironmentManager.Instance?.Config != null)
            {
                useEncryption = EnvironmentManager.Instance.Config.UseSecureStorage;
            }

            // Generate or retrieve device-specific key
            deviceKey = GetOrCreateDeviceKey();
        }

        /// <summary>
        /// Get or create a device-specific encryption key
        /// </summary>
        private string GetOrCreateDeviceKey()
        {
            string deviceId = PlayerPrefs.GetString(DEVICE_ID_KEY, "");

            if (string.IsNullOrEmpty(deviceId))
            {
                // Generate new device ID
                deviceId = SystemInfo.deviceUniqueIdentifier;

                // If device ID is not available, generate a random one
                if (string.IsNullOrEmpty(deviceId) || deviceId == "n/a")
                {
                    deviceId = Guid.NewGuid().ToString();
                }

                PlayerPrefs.SetString(DEVICE_ID_KEY, deviceId);
                PlayerPrefs.Save();
            }

            // Combine with application identifier for extra security
            return $"{Application.identifier}_{deviceId}";
        }
        #endregion

        #region Token Management
        /// <summary>
        /// Store access token securely
        /// </summary>
        public void StoreAccessToken(string token, long expiryTime)
        {
            if (string.IsNullOrEmpty(token))
            {
                Debug.LogError("[SecureTokenStorage] Cannot store empty access token");
                return;
            }

            string encryptedToken = useEncryption ? Encrypt(token) : token;
            PlayerPrefs.SetString(ACCESS_TOKEN_KEY, encryptedToken);
            PlayerPrefs.SetString(TOKEN_EXPIRY_KEY, expiryTime.ToString());
            PlayerPrefs.Save();

            if (EnvironmentManager.Instance?.Config?.VerboseLogging ?? false)
            {
                Debug.Log("[SecureTokenStorage] Access token stored securely");
            }
        }

        /// <summary>
        /// Store refresh token securely
        /// </summary>
        public void StoreRefreshToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                Debug.LogError("[SecureTokenStorage] Cannot store empty refresh token");
                return;
            }

            string encryptedToken = useEncryption ? Encrypt(token) : token;
            PlayerPrefs.SetString(REFRESH_TOKEN_KEY, encryptedToken);
            PlayerPrefs.Save();

            if (EnvironmentManager.Instance?.Config?.VerboseLogging ?? false)
            {
                Debug.Log("[SecureTokenStorage] Refresh token stored securely");
            }
        }

        /// <summary>
        /// Retrieve access token
        /// </summary>
        public string GetAccessToken()
        {
            string encryptedToken = PlayerPrefs.GetString(ACCESS_TOKEN_KEY, "");

            if (string.IsNullOrEmpty(encryptedToken))
                return null;

            try
            {
                return useEncryption ? Decrypt(encryptedToken) : encryptedToken;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SecureTokenStorage] Failed to decrypt access token: {e.Message}");
                ClearTokens(); // Clear corrupted tokens
                return null;
            }
        }

        /// <summary>
        /// Retrieve refresh token
        /// </summary>
        public string GetRefreshToken()
        {
            string encryptedToken = PlayerPrefs.GetString(REFRESH_TOKEN_KEY, "");

            if (string.IsNullOrEmpty(encryptedToken))
                return null;

            try
            {
                return useEncryption ? Decrypt(encryptedToken) : encryptedToken;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SecureTokenStorage] Failed to decrypt refresh token: {e.Message}");
                ClearTokens(); // Clear corrupted tokens
                return null;
            }
        }

        /// <summary>
        /// Get token expiry time
        /// </summary>
        public long GetTokenExpiry()
        {
            string expiryStr = PlayerPrefs.GetString(TOKEN_EXPIRY_KEY, "0");

            if (long.TryParse(expiryStr, out long expiry))
                return expiry;

            return 0;
        }

        /// <summary>
        /// Check if access token needs refresh
        /// </summary>
        public bool NeedsTokenRefresh()
        {
            long expiryTime = GetTokenExpiry();

            if (expiryTime == 0)
                return true;

            // Get threshold from config or use default
            int threshold = EnvironmentManager.Instance?.Config?.TokenRefreshThreshold ?? 300;

            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return (expiryTime - currentTime) <= threshold;
        }

        /// <summary>
        /// Check if access token is valid
        /// </summary>
        public bool IsTokenValid()
        {
            string token = GetAccessToken();

            if (string.IsNullOrEmpty(token))
                return false;

            long expiryTime = GetTokenExpiry();

            if (expiryTime == 0)
                return false;

            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return currentTime < expiryTime;
        }

        /// <summary>
        /// Clear all stored tokens
        /// </summary>
        public void ClearTokens()
        {
            PlayerPrefs.DeleteKey(ACCESS_TOKEN_KEY);
            PlayerPrefs.DeleteKey(REFRESH_TOKEN_KEY);
            PlayerPrefs.DeleteKey(TOKEN_EXPIRY_KEY);
            PlayerPrefs.Save();

            Debug.Log("[SecureTokenStorage] All tokens cleared");
        }
        #endregion

        #region Encryption
        /// <summary>
        /// Encrypt string using AES
        /// </summary>
        private string Encrypt(string plainText)
        {
            if (!useEncryption || string.IsNullOrEmpty(plainText))
                return plainText;

            try
            {
                byte[] keyBytes = GenerateKey();
                byte[] ivBytes = GenerateIV();
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = keyBytes;
                    aes.IV = ivBytes;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var encryptor = aes.CreateEncryptor())
                    {
                        byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                        // Combine IV and encrypted data
                        byte[] result = new byte[ivBytes.Length + encryptedBytes.Length];
                        Array.Copy(ivBytes, 0, result, 0, ivBytes.Length);
                        Array.Copy(encryptedBytes, 0, result, ivBytes.Length, encryptedBytes.Length);

                        return Convert.ToBase64String(result);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SecureTokenStorage] Encryption failed: {e.Message}");
                return plainText; // Fallback to unencrypted
            }
        }

        /// <summary>
        /// Decrypt string using AES
        /// </summary>
        private string Decrypt(string encryptedText)
        {
            if (!useEncryption || string.IsNullOrEmpty(encryptedText))
                return encryptedText;

            try
            {
                byte[] keyBytes = GenerateKey();
                byte[] combinedBytes = Convert.FromBase64String(encryptedText);

                // Extract IV and encrypted data
                byte[] ivBytes = new byte[16];
                byte[] encryptedBytes = new byte[combinedBytes.Length - 16];
                Array.Copy(combinedBytes, 0, ivBytes, 0, 16);
                Array.Copy(combinedBytes, 16, encryptedBytes, 0, encryptedBytes.Length);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = keyBytes;
                    aes.IV = ivBytes;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var decryptor = aes.CreateDecryptor())
                    {
                        byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                        return Encoding.UTF8.GetString(decryptedBytes);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SecureTokenStorage] Decryption failed: {e.Message}");
                throw; // Re-throw to handle at caller level
            }
        }

        /// <summary>
        /// Generate encryption key from device key
        /// </summary>
        private byte[] GenerateKey()
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(deviceKey));
            }
        }

        /// <summary>
        /// Generate initialization vector
        /// </summary>
        private byte[] GenerateIV()
        {
            // Use a stored salt or generate new one
            string salt = PlayerPrefs.GetString(SALT_KEY, "");

            if (string.IsNullOrEmpty(salt))
            {
                salt = Guid.NewGuid().ToString();
                PlayerPrefs.SetString(SALT_KEY, salt);
                PlayerPrefs.Save();
            }

            using (MD5 md5 = MD5.Create())
            {
                return md5.ComputeHash(Encoding.UTF8.GetBytes(salt));
            }
        }
        #endregion

        #region Cleanup
        private void OnApplicationPause(bool pauseStatus)
        {
            // Save PlayerPrefs when app is paused
            if (pauseStatus)
            {
                PlayerPrefs.Save();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            // Save PlayerPrefs when app loses focus
            if (!hasFocus)
            {
                PlayerPrefs.Save();
            }
        }

        private void OnDestroy()
        {
            // Ensure all data is saved
            PlayerPrefs.Save();
        }
        #endregion
    }
}