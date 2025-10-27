using UnityEngine;
using UnityEngine.UI;
using LottoDefense.Authentication;
using LottoDefense.Configuration;

namespace LottoDefense.UI
{
    [RequireComponent(typeof(Button))]
    public class GoogleLoginButton : MonoBehaviour
    {
        [SerializeField] private LoginConfig config;
        [SerializeField] private Image buttonImage;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();

            if (buttonImage == null)
            {
                buttonImage = GetComponent<Image>();
            }
        }

        private void Start()
        {
            _button.onClick.AddListener(OnLoginButtonClicked);
            UpdateButtonState(true);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnLoginButtonClicked);
        }

        private void OnLoginButtonClicked()
        {
            Debug.Log("Google Login button clicked");
            UpdateButtonState(false);
            AuthenticationManager.Instance.AuthenticateWithGoogle();
        }

        private void UpdateButtonState(bool interactable)
        {
            _button.interactable = interactable;

            if (config != null && buttonImage != null)
            {
                if (interactable)
                {
                    buttonImage.sprite = config.googleSignInButtonNormal;
                }
                else
                {
                    buttonImage.sprite = config.googleSignInButtonDisabled;
                }
            }
        }

        public void ResetButton()
        {
            UpdateButtonState(true);
        }
    }
}
