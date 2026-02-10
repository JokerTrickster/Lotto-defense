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
                Debug.Log("[MainGameManager] Auto guest login for development");
                AuthenticationManager.Instance.LoginAsGuest("dev_user_" + Random.Range(1000, 9999));
            }
        }

        public void Logout()
        {
            AuthenticationManager.Instance.Logout();
            UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
        }
    }
}
