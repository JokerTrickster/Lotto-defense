using UnityEngine;
using UnityEngine.UI;
using LottoDefense.Gameplay;

namespace LottoDefense.UI
{
    /// <summary>
    /// 게임 배속 조절 버튼 UI
    /// 우상단에 배치되어 ×1, ×2 배속 전환
    /// </summary>
    public class GameSpeedButton : MonoBehaviour
    {
        #region Inspector Fields
        [Header("UI References")]
        [SerializeField] private Button speedButton;
        [SerializeField] private Text speedText;
        
        [Header("Visual Settings")]
        [SerializeField] private Color normalSpeedColor = new Color(0.8f, 0.8f, 0.8f);
        [SerializeField] private Color fastSpeedColor = new Color(1f, 0.5f, 0.2f);
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // 버튼이 없으면 자동 생성
            if (speedButton == null)
            {
                CreateSpeedButton();
            }
            
            // 버튼 클릭 이벤트 연결
            if (speedButton != null)
            {
                speedButton.onClick.AddListener(OnSpeedButtonClicked);
            }
        }

        private void Start()
        {
            // GameSpeedController 이벤트 구독
            if (GameSpeedController.Instance != null)
            {
                GameSpeedController.Instance.OnSpeedChanged += OnSpeedChanged;
                
                // 초기 UI 업데이트
                UpdateUI(GameSpeedController.Instance.CurrentSpeed);
            }
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (GameSpeedController.Instance != null)
            {
                GameSpeedController.Instance.OnSpeedChanged -= OnSpeedChanged;
            }
        }
        #endregion

        #region Button Events
        private void OnSpeedButtonClicked()
        {
            if (GameSpeedController.Instance != null)
            {
                GameSpeedController.Instance.ToggleSpeed();
            }
        }

        private void OnSpeedChanged(float newSpeed)
        {
            UpdateUI(newSpeed);
        }
        #endregion

        #region UI Update
        private void UpdateUI(float speed)
        {
            if (speedText != null)
            {
                speedText.text = $"×{speed:F0}";
            }

            // 배속에 따라 색상 변경
            if (speedButton != null)
            {
                Image buttonImage = speedButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = speed > 1.0f ? fastSpeedColor : normalSpeedColor;
                }
            }
        }
        #endregion

        #region Auto-Create UI
        private void CreateSpeedButton()
        {
            Debug.Log("[GameSpeedButton] Auto-creating speed button UI");

            // Canvas 찾기
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[GameSpeedButton] No Canvas found!");
                return;
            }

            // 버튼 GameObject 생성
            GameObject btnObj = new GameObject("SpeedButton");
            btnObj.transform.SetParent(canvas.transform, false);

            // RectTransform 설정 (우상단)
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(1f, 1f); // 우상단
            btnRect.anchorMax = new Vector2(1f, 1f);
            btnRect.pivot = new Vector2(1f, 1f);
            btnRect.anchoredPosition = new Vector2(-20, -20); // 우상단에서 20px 여백
            btnRect.sizeDelta = new Vector2(80, 50);

            // Image 컴포넌트
            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = normalSpeedColor;

            // Button 컴포넌트
            speedButton = btnObj.AddComponent<Button>();

            // 텍스트 생성
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            speedText = textObj.AddComponent<Text>();
            speedText.text = "×1";
            speedText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (speedText.font == null)
                speedText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            speedText.fontSize = 28;
            speedText.color = Color.white;
            speedText.alignment = TextAnchor.MiddleCenter;
            speedText.fontStyle = FontStyle.Bold;

            Debug.Log("[GameSpeedButton] Speed button created at top-right corner");
        }
        #endregion
    }
}
