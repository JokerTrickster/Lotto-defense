using System;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace LottoDefense.Backend
{
    /// <summary>
    /// HTTP client for Tower Defense backend API.
    /// Handles all REST API communication with JWT authentication.
    /// </summary>
    public class APIClient
    {
        private const string BASE_URL = "http://localhost:18082/api/v1/td";
        private string _jwtToken;

        public bool IsAuthenticated => !string.IsNullOrEmpty(_jwtToken);

        public void SetToken(string token)
        {
            _jwtToken = token;
        }

        public void ClearToken()
        {
            _jwtToken = null;
        }

        #region POST Requests
        public IEnumerator Post<TRequest, TResponse>(string endpoint, TRequest body, Action<TResponse> onSuccess, Action<string> onError)
        {
            string url = BASE_URL + endpoint;
            string jsonBody = JsonUtility.ToJson(body);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                if (!string.IsNullOrEmpty(_jwtToken))
                {
                    request.SetRequestHeader("Authorization", "Bearer " + _jwtToken);
                }

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        string responseText = request.downloadHandler.text;
                        APIResponse<TResponse> apiResponse = JsonUtility.FromJson<APIResponse<TResponse>>(responseText);

                        if (apiResponse.success)
                        {
                            onSuccess?.Invoke(apiResponse.data);
                        }
                        else
                        {
                            onError?.Invoke("Server returned success=false");
                        }
                    }
                    catch (Exception e)
                    {
                        onError?.Invoke("JSON parse error: " + e.Message);
                    }
                }
                else
                {
                    string error = request.error;
                    if (request.downloadHandler != null)
                    {
                        error += " - " + request.downloadHandler.text;
                    }
                    onError?.Invoke(error);
                }
            }
        }
        #endregion

        #region GET Requests
        public IEnumerator Get<TResponse>(string endpoint, Action<TResponse> onSuccess, Action<string> onError)
        {
            string url = BASE_URL + endpoint;

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                if (!string.IsNullOrEmpty(_jwtToken))
                {
                    request.SetRequestHeader("Authorization", "Bearer " + _jwtToken);
                }

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        string responseText = request.downloadHandler.text;
                        APIResponse<TResponse> apiResponse = JsonUtility.FromJson<APIResponse<TResponse>>(responseText);

                        if (apiResponse.success)
                        {
                            onSuccess?.Invoke(apiResponse.data);
                        }
                        else
                        {
                            onError?.Invoke("Server returned success=false");
                        }
                    }
                    catch (Exception e)
                    {
                        onError?.Invoke("JSON parse error: " + e.Message);
                    }
                }
                else
                {
                    string error = request.error;
                    if (request.downloadHandler != null)
                    {
                        error += " - " + request.downloadHandler.text;
                    }
                    onError?.Invoke(error);
                }
            }
        }
        #endregion

        #region Response Wrapper
        [Serializable]
        private class APIResponse<T>
        {
            public bool success;
            public T data;
        }
        #endregion
    }
}
