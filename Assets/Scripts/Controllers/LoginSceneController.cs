using UnityEngine;
using UnityEngine.SceneManagement;
using LottoDefense.Authentication;
using LottoDefense.Configuration;
using LottoDefense.UI;

namespace LottoDefense.Controllers
{
    public class LoginSceneController : MonoBehaviour
    {
        [SerializeField] private LoginConfig config;
        [SerializeField] private GoogleLoginButton loginButton;
        [SerializeField] private LoadingOverlay loadingOverlay;

        [Header("Settings (if no LoginConfig)")]
        [SerializeField] private string mainGameSceneName = "MainGame";
        [SerializeField] private float minLoadingTime = 1.5f;

        private void Start()
        {
            SubscribeToAuthEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromAuthEvents();
        }

        private void SubscribeToAuthEvents()
        {
            AuthenticationManager.Instance.OnAuthenticationComplete += HandleAuthenticationComplete;
            AuthenticationManager.Instance.OnAuthenticationError += HandleAuthenticationError;
        }

        private void UnsubscribeFromAuthEvents()
        {
            if (AuthenticationManager.Instance != null)
            {
                AuthenticationManager.Instance.OnAuthenticationComplete -= HandleAuthenticationComplete;
                AuthenticationManager.Instance.OnAuthenticationError -= HandleAuthenticationError;
            }
        }

        private void HandleAuthenticationComplete(bool success)
        {
            if (success)
            {
                Debug.Log("Authentication successful, loading main game...");
                loadingOverlay.Show("Loading game...");
                LoadMainGame();
            }
            else
            {
                Debug.LogError("Authentication failed");
                loadingOverlay.Hide();
                if (loginButton != null)
                {
                    loginButton.ResetButton();
                }
            }
        }

        private void HandleAuthenticationError(string error)
        {
            Debug.LogError($"Authentication error: {error}");
            loadingOverlay.Hide();
            if (loginButton != null)
            {
                loginButton.ResetButton();
            }
        }

        private void LoadMainGame()
        {
            string sceneName = config != null ? config.mainGameSceneName : mainGameSceneName;

            if (!string.IsNullOrEmpty(sceneName))
            {
                StartCoroutine(LoadMainGameCoroutine());
            }
            else
            {
                Debug.LogWarning("Main game scene name not configured!");
            }
        }

        private System.Collections.IEnumerator LoadMainGameCoroutine()
        {
            float loadTime = config != null ? config.minLoadingTime : minLoadingTime;
            string sceneName = config != null ? config.mainGameSceneName : mainGameSceneName;

            yield return new WaitForSeconds(loadTime);

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }
    }
}
