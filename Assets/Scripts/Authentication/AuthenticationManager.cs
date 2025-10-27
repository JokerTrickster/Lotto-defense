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
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void AuthenticateWithGoogle()
        {
            Debug.Log("Starting Google authentication...");

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

            Debug.Log($"Authentication successful. User ID: {_userId}");
            OnAuthenticationComplete?.Invoke(true);
        }

        public void Logout()
        {
            _isAuthenticated = false;
            _userId = null;
            Debug.Log("User logged out");
        }

        public void CancelAuthentication()
        {
            StopAllCoroutines();
            OnAuthenticationError?.Invoke("Authentication cancelled by user");
        }
    }
}
