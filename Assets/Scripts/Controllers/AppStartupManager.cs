using UnityEngine;
using UnityEngine.SceneManagement;
using LottoDefense.Authentication;

namespace LottoDefense.Controllers
{
    public class AppStartupManager : MonoBehaviour
    {
        [SerializeField] private string loginSceneName = "LoginScene";
        [SerializeField] private string mainGameSceneName = "MainGame";

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            CheckAuthenticationStatus();
        }

        private void CheckAuthenticationStatus()
        {
            if (AuthenticationManager.Instance.IsAuthenticated)
            {
                Debug.Log("User already authenticated, loading main game...");
                LoadScene(mainGameSceneName);
            }
            else
            {
                Debug.Log("User not authenticated, loading login scene...");
                LoadScene(loginSceneName);
            }
        }

        private void LoadScene(string sceneName)
        {
            if (!string.IsNullOrEmpty(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogError($"Scene name is empty or null!");
            }
        }
    }
}
