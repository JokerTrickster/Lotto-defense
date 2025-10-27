using UnityEngine;

namespace LottoDefense.Configuration
{
    [CreateAssetMenu(fileName = "LoginConfig", menuName = "Lotto Defense/Login Configuration")]
    public class LoginConfig : ScriptableObject
    {
        [Header("Google Sign-In Settings")]
        public string clientId = "";
        public string clientSecret = "";

        [Header("UI Settings")]
        public Sprite googleSignInButtonNormal;
        public Sprite googleSignInButtonPressed;
        public Sprite googleSignInButtonDisabled;

        [Header("Scene Settings")]
        public string mainGameSceneName = "MainGame";
        public float minLoadingTime = 1.5f;

        [Header("Debug Settings")]
        public bool useSimulatedAuth = true;
        public bool logAuthenticationSteps = true;
    }
}
