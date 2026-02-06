using UnityEngine;
using LottoDefense.Authentication;

namespace LottoDefense.Controllers
{
    public class MainGameManager : MonoBehaviour
    {
        private void Start()
        {
            if (AuthenticationManager.Instance.IsAuthenticated)
            {
                Debug.Log($"[MainGameManager] Welcome! User ID: {AuthenticationManager.Instance.UserId}");
            }
            else
            {
                Debug.LogWarning("[MainGameManager] User not authenticated. Use LoginScene as start scene for auth flow.");
            }
        }

        public void Logout()
        {
            AuthenticationManager.Instance.Logout();
            UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
        }
    }
}
