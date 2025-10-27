using UnityEngine;

namespace LottoDefense.UI
{
    public class LegalLinksHandler : MonoBehaviour
    {
        [SerializeField] private string privacyPolicyUrl = "https://example.com/privacy";
        [SerializeField] private string termsOfServiceUrl = "https://example.com/terms";

        public void OpenPrivacyPolicy()
        {
            OpenURL(privacyPolicyUrl);
        }

        public void OpenTermsOfService()
        {
            OpenURL(termsOfServiceUrl);
        }

        private void OpenURL(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                Debug.Log($"Opening URL: {url}");
                Application.OpenURL(url);
            }
            else
            {
                Debug.LogWarning("URL is empty or null!");
            }
        }
    }
}
