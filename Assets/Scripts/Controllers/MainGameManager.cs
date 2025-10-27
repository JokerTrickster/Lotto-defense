using UnityEngine;
using LottoDefense.Authentication;

namespace LottoDefense.Controllers
{
    public class MainGameManager : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log($"[MainGameManager] Checking authentication... (AuthManager Instance: {AuthenticationManager.Instance.GetInstanceID()})");

            // Check if user is authenticated
            if (AuthenticationManager.Instance.IsAuthenticated)
            {
                Debug.Log($"[MainGameManager] Welcome to Main Game! User ID: {AuthenticationManager.Instance.UserId}");
            }
            else
            {
                Debug.LogWarning($"[MainGameManager] User not authenticated! IsAuthenticated: {AuthenticationManager.Instance.IsAuthenticated}");
                Debug.LogWarning("Redirecting to login...");
                UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
            }
        }

        public void Logout()
        {
            AuthenticationManager.Instance.Logout();
            UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
        }
    }
}
