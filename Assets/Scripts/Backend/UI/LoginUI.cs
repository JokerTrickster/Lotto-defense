using UnityEngine;
using UnityEngine.UI;
using LottoDefense.Backend.Models;

namespace LottoDefense.Backend.UI
{
    /// <summary>
    /// Login and registration UI.
    /// Shows at game start if user is not logged in.
    /// </summary>
    public class LoginUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject loginPanel;
        [SerializeField] private GameObject registerPanel;

        [Header("Login")]
        [SerializeField] private InputField loginEmail;
        [SerializeField] private InputField loginPassword;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button showRegisterButton;

        [Header("Register")]
        [SerializeField] private InputField registerUsername;
        [SerializeField] private InputField registerEmail;
        [SerializeField] private InputField registerPassword;
        [SerializeField] private InputField registerPasswordConfirm;
        [SerializeField] private Button registerButton;
        [SerializeField] private Button showLoginButton;

        [Header("Status")]
        [SerializeField] private Text statusText;
        [SerializeField] private Button guestButton;

        private void Start()
        {
            // Check if already logged in
            if (APIManager.Instance.IsLoggedIn)
            {
                Debug.Log("[LoginUI] Already logged in as: " + APIManager.Instance.Username);
                gameObject.SetActive(false);
                return;
            }

            SetupButtons();
            ShowLoginPanel();
        }

        private void SetupButtons()
        {
            if (loginButton != null)
                loginButton.onClick.AddListener(OnLoginClicked);

            if (showRegisterButton != null)
                showRegisterButton.onClick.AddListener(ShowRegisterPanel);

            if (registerButton != null)
                registerButton.onClick.AddListener(OnRegisterClicked);

            if (showLoginButton != null)
                showLoginButton.onClick.AddListener(ShowLoginPanel);

            if (guestButton != null)
                guestButton.onClick.AddListener(OnGuestClicked);
        }

        private void ShowLoginPanel()
        {
            if (loginPanel != null) loginPanel.SetActive(true);
            if (registerPanel != null) registerPanel.SetActive(false);
            ClearStatus();
        }

        private void ShowRegisterPanel()
        {
            if (loginPanel != null) loginPanel.SetActive(false);
            if (registerPanel != null) registerPanel.SetActive(true);
            ClearStatus();
        }

        private void OnLoginClicked()
        {
            string email = loginEmail?.text?.Trim();
            string password = loginPassword?.text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowStatus("이메일과 비밀번호를 입력하세요", true);
                return;
            }

            ShowStatus("로그인 중...", false);
            SetButtonsEnabled(false);

            APIManager.Instance.Login(email, password,
                (AuthResponse response) =>
                {
                    ShowStatus($"환영합니다, {response.user.username}님!", false);
                    Invoke(nameof(CloseUI), 1f);
                },
                (string error) =>
                {
                    ShowStatus("로그인 실패: " + error, true);
                    SetButtonsEnabled(true);
                });
        }

        private void OnRegisterClicked()
        {
            string username = registerUsername?.text?.Trim();
            string email = registerEmail?.text?.Trim();
            string password = registerPassword?.text;
            string passwordConfirm = registerPasswordConfirm?.text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowStatus("모든 항목을 입력하세요", true);
                return;
            }

            if (username.Length < 3)
            {
                ShowStatus("닉네임은 3자 이상이어야 합니다", true);
                return;
            }

            if (password.Length < 6)
            {
                ShowStatus("비밀번호는 6자 이상이어야 합니다", true);
                return;
            }

            if (password != passwordConfirm)
            {
                ShowStatus("비밀번호가 일치하지 않습니다", true);
                return;
            }

            ShowStatus("가입 중...", false);
            SetButtonsEnabled(false);

            APIManager.Instance.Register(username, email, password,
                (AuthResponse response) =>
                {
                    ShowStatus($"가입 완료! 환영합니다, {response.user.username}님!", false);
                    Invoke(nameof(CloseUI), 1f);
                },
                (string error) =>
                {
                    ShowStatus("가입 실패: " + error, true);
                    SetButtonsEnabled(true);
                });
        }

        private void OnGuestClicked()
        {
            ShowStatus("게스트 모드로 시작합니다 (결과 저장 안 됨)", false);
            Invoke(nameof(CloseUI), 1f);
        }

        private void ShowStatus(string message, bool isError)
        {
            if (statusText != null)
            {
                statusText.text = message;
                statusText.color = isError ? Color.red : Color.white;
            }
            Debug.Log("[LoginUI] " + message);
        }

        private void ClearStatus()
        {
            if (statusText != null)
                statusText.text = "";
        }

        private void SetButtonsEnabled(bool enabled)
        {
            if (loginButton != null) loginButton.interactable = enabled;
            if (registerButton != null) registerButton.interactable = enabled;
            if (showRegisterButton != null) showRegisterButton.interactable = enabled;
            if (showLoginButton != null) showLoginButton.interactable = enabled;
        }

        private void CloseUI()
        {
            gameObject.SetActive(false);
        }
    }
}
