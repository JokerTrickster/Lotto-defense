using UnityEngine;
using System;

namespace LottoDefense.Authentication
{
    public class AuthenticationManager : MonoBehaviour
    {
        private static AuthenticationManager _instance;
        public static AuthenticationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("AuthenticationManager");
                    _instance = go.AddComponent<AuthenticationManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        public event Action<bool> OnAuthenticationComplete;
        public event Action<string> OnAuthenticationError;

        private bool _isAuthenticated = false;
        public bool IsAuthenticated => _isAuthenticated;

        private string _userId;
        public string UserId => _userId;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.Log("AuthenticationManager already exists, destroying duplicate");
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("AuthenticationManager created and set to DontDestroyOnLoad");
        }

        public void AuthenticateWithGoogle()
        {
            Debug.Log($"[AuthManager] Starting Google authentication... (Instance: {GetInstanceID()})");

            // TODO: Implement actual Google Sign-In integration
            // For now, simulate authentication
            SimulateAuthentication();
        }

        private void SimulateAuthentication()
        {
            // Simulate network delay
            StartCoroutine(SimulateAuthenticationCoroutine());
        }

        private System.Collections.IEnumerator SimulateAuthenticationCoroutine()
        {
            yield return new UnityEngine.WaitForSeconds(1.5f);

            // Simulate successful authentication
            _isAuthenticated = true;
            _userId = "user_" + UnityEngine.Random.Range(1000, 9999);

            Debug.Log($"[AuthManager] Authentication successful. User ID: {_userId}, IsAuthenticated: {_isAuthenticated} (Instance: {GetInstanceID()})");
            OnAuthenticationComplete?.Invoke(true);
        }

        public void Logout()
        {
            _isAuthenticated = false;
            _userId = null;
            Debug.Log("User logged out");
        }

        /// <summary>
        /// Login as guest (for development/testing purposes).
        /// </summary>
        public void LoginAsGuest(string guestId)
        {
            _isAuthenticated = true;
            _userId = guestId;
            Debug.Log($"[AuthManager] Guest login successful. User ID: {_userId}");
            OnAuthenticationComplete?.Invoke(true);
        }

        public void CancelAuthentication()
        {
            StopAllCoroutines();
            OnAuthenticationError?.Invoke("Authentication cancelled by user");
        }
    }
}
