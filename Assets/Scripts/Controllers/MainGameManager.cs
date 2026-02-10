using UnityEngine;
using LottoDefense.Authentication;

namespace LottoDefense.Controllers
{
    public class MainGameManager : MonoBehaviour
    {
        [Header("Development Settings")]
        [Tooltip("Skip authentication check for testing (dev mode only)")]
        [SerializeField] private bool skipAuthenticationCheck = false;

        private void Start()
        {
            if (AuthenticationManager.Instance.IsAuthenticated)
            {
                Debug.Log($"[MainGameManager] Welcome! User ID: {AuthenticationManager.Instance.UserId}");
            }
            else
            {
                if (skipAuthenticationCheck)
                {
                    Debug.Log("[MainGameManager] Dev mode: Skipping authentication check");
                    // Create temporary guest session for testing
                    AuthenticationManager.Instance.LoginAsGuest("dev_test_user");
                }
                else
                {
                    Debug.LogWarning("[MainGameManager] User not authenticated. Redirecting to LoginScene...");
                    UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
                }
            }
        }

        public void Logout()
        {
            AuthenticationManager.Instance.Logout();
            UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
        }
    }
}
