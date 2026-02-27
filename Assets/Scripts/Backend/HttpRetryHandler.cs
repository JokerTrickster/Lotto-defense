using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using LottoDefense.Config;

namespace LottoDefense.Backend
{
    /// <summary>
    /// HTTP retry handler with exponential backoff for network resilience.
    /// Handles transient network failures with configurable retry strategies.
    /// </summary>
    public class HttpRetryHandler : MonoBehaviour
    {
        #region Configuration
        private int maxRetryAttempts = 3;
        private float baseDelay = 1f;
        private float maxDelay = 32f;
        private float jitterRange = 0.3f;
        #endregion

        #region Initialization
        private void Start()
        {
            // Load configuration from EnvironmentManager
            if (EnvironmentManager.Instance != null && EnvironmentManager.Instance.Config != null)
            {
                var config = EnvironmentManager.Instance.Config;
                maxRetryAttempts = config.MaxRetryAttempts;
                baseDelay = config.RetryDelayBase;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Execute HTTP request with retry logic
        /// </summary>
        public IEnumerator ExecuteWithRetry(
            UnityWebRequest request,
            Action<string> onSuccess,
            Action<string> onError,
            int? customMaxRetries = null)
        {
            int attempts = 0;
            int maxAttempts = customMaxRetries ?? maxRetryAttempts;
            string lastError = null;

            while (attempts < maxAttempts)
            {
                attempts++;

                if (EnvironmentManager.Instance.Config?.LogNetworkCalls ?? false)
                {
                    Debug.Log($"[HttpRetryHandler] Attempt {attempts}/{maxAttempts} for {request.method} {request.url}");
                }

                // Clone the request for retry (UnityWebRequest can't be reused)
                using (UnityWebRequest attemptRequest = CloneRequest(request))
                {
                    yield return attemptRequest.SendWebRequest();

                    if (attemptRequest.result == UnityWebRequest.Result.Success)
                    {
                        onSuccess?.Invoke(attemptRequest.downloadHandler.text);
                        yield break; // Success, exit retry loop
                    }

                    // Check if error is retryable
                    if (!IsRetryableError(attemptRequest))
                    {
                        lastError = GetErrorMessage(attemptRequest);
                        Debug.LogError($"[HttpRetryHandler] Non-retryable error: {lastError}");
                        break; // Non-retryable error, exit retry loop
                    }

                    lastError = GetErrorMessage(attemptRequest);

                    if (attempts < maxAttempts)
                    {
                        float delay = CalculateDelay(attempts);
                        Debug.LogWarning($"[HttpRetryHandler] Request failed: {lastError}. Retrying in {delay:F2} seconds...");
                        yield return new WaitForSeconds(delay);
                    }
                }
            }

            // All attempts failed
            string finalError = $"Request failed after {attempts} attempts. Last error: {lastError}";
            Debug.LogError($"[HttpRetryHandler] {finalError}");
            onError?.Invoke(finalError);
        }

        /// <summary>
        /// Execute HTTP request with typed response and retry logic
        /// </summary>
        public IEnumerator ExecuteWithRetry<T>(
            UnityWebRequest request,
            Action<T> onSuccess,
            Action<string> onError,
            int? customMaxRetries = null) where T : class
        {
            yield return ExecuteWithRetry(
                request,
                (response) =>
                {
                    try
                    {
                        T data = JsonUtility.FromJson<T>(response);
                        onSuccess?.Invoke(data);
                    }
                    catch (Exception e)
                    {
                        onError?.Invoke($"Failed to parse response: {e.Message}");
                    }
                },
                onError,
                customMaxRetries
            );
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Calculate delay for next retry attempt with exponential backoff and jitter
        /// </summary>
        private float CalculateDelay(int attemptNumber)
        {
            // Exponential backoff: delay = base * 2^(attempt-1)
            float exponentialDelay = baseDelay * Mathf.Pow(2, attemptNumber - 1);

            // Cap at maximum delay
            exponentialDelay = Mathf.Min(exponentialDelay, maxDelay);

            // Add jitter to prevent thundering herd
            float jitter = UnityEngine.Random.Range(-jitterRange, jitterRange) * exponentialDelay;

            return Mathf.Max(0.1f, exponentialDelay + jitter);
        }

        /// <summary>
        /// Check if error is retryable based on HTTP status code
        /// </summary>
        private bool IsRetryableError(UnityWebRequest request)
        {
            // Network errors are typically retryable
            if (request.result == UnityWebRequest.Result.ConnectionError)
                return true;

            // Check HTTP status codes
            long statusCode = request.responseCode;

            // Retryable status codes
            // 408 Request Timeout
            // 429 Too Many Requests
            // 500 Internal Server Error
            // 502 Bad Gateway
            // 503 Service Unavailable
            // 504 Gateway Timeout
            if (statusCode == 408 || statusCode == 429 ||
                statusCode == 500 || statusCode == 502 ||
                statusCode == 503 || statusCode == 504)
            {
                return true;
            }

            // Client errors (4xx except timeout/rate limit) are not retryable
            if (statusCode >= 400 && statusCode < 500)
                return false;

            return false;
        }

        /// <summary>
        /// Get error message from request
        /// </summary>
        private string GetErrorMessage(UnityWebRequest request)
        {
            if (request.result == UnityWebRequest.Result.ConnectionError)
                return $"Network error: {request.error}";

            if (request.result == UnityWebRequest.Result.ProtocolError)
                return $"HTTP {request.responseCode}: {request.error}";

            return request.error ?? "Unknown error";
        }

        /// <summary>
        /// Clone UnityWebRequest for retry
        /// </summary>
        private UnityWebRequest CloneRequest(UnityWebRequest original)
        {
            UnityWebRequest cloned = null;

            switch (original.method)
            {
                case "GET":
                    cloned = UnityWebRequest.Get(original.url);
                    break;

                case "POST":
                    cloned = new UnityWebRequest(original.url, "POST");
                    if (original.uploadHandler != null && original.uploadHandler.data != null)
                    {
                        cloned.uploadHandler = new UploadHandlerRaw(original.uploadHandler.data);
                        cloned.uploadHandler.contentType = original.uploadHandler.contentType;
                    }
                    cloned.downloadHandler = new DownloadHandlerBuffer();
                    break;

                case "PUT":
                    cloned = new UnityWebRequest(original.url, "PUT");
                    if (original.uploadHandler != null && original.uploadHandler.data != null)
                    {
                        cloned.uploadHandler = new UploadHandlerRaw(original.uploadHandler.data);
                        cloned.uploadHandler.contentType = original.uploadHandler.contentType;
                    }
                    cloned.downloadHandler = new DownloadHandlerBuffer();
                    break;

                case "DELETE":
                    cloned = UnityWebRequest.Delete(original.url);
                    cloned.downloadHandler = new DownloadHandlerBuffer();
                    break;

                default:
                    cloned = new UnityWebRequest(original.url, original.method);
                    if (original.uploadHandler != null && original.uploadHandler.data != null)
                    {
                        cloned.uploadHandler = new UploadHandlerRaw(original.uploadHandler.data);
                        cloned.uploadHandler.contentType = original.uploadHandler.contentType;
                    }
                    cloned.downloadHandler = new DownloadHandlerBuffer();
                    break;
            }

            // Copy headers
            if (original.GetRequestHeaders() != null)
            {
                foreach (var header in original.GetRequestHeaders())
                {
                    cloned.SetRequestHeader(header.Key, header.Value);
                }
            }

            // Copy timeout
            cloned.timeout = original.timeout;

            return cloned;
        }
        #endregion
    }
}