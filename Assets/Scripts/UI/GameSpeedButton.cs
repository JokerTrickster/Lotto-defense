using UnityEngine;
using UnityEngine.UI;
using LottoDefense.Gameplay;

namespace LottoDefense.UI
{
    public class GameSpeedButton : MonoBehaviour
    {
        [SerializeField] private Button speedButton;
        [SerializeField] private Text speedText;
        [SerializeField] private Image buttonImage;

        [SerializeField] private Color normalSpeedColor = new Color(0.55f, 0.78f, 0.95f);
        [SerializeField] private Color fastSpeedColor = new Color(0.95f, 0.65f, 0.4f);
        [SerializeField] private Color maxSpeedColor = new Color(0.95f, 0.5f, 0.5f);

        private void Awake()
        {
            if (speedButton == null)
                CreateSpeedButton();

            if (speedButton != null)
                speedButton.onClick.AddListener(OnSpeedButtonClicked);
        }

        private void Start()
        {
            if (GameSpeedController.Instance != null)
            {
                GameSpeedController.Instance.OnSpeedChanged += OnSpeedChanged;
                UpdateUI(GameSpeedController.Instance.CurrentSpeed);
            }
        }

        private void OnDestroy()
        {
            if (GameSpeedController.Instance != null)
                GameSpeedController.Instance.OnSpeedChanged -= OnSpeedChanged;
        }

        private void OnSpeedButtonClicked()
        {
            if (GameSpeedController.Instance != null)
                GameSpeedController.Instance.ToggleSpeed();
        }

        private void OnSpeedChanged(float newSpeed)
        {
            UpdateUI(newSpeed);
        }

        private void UpdateUI(float speed)
        {
            if (speedText != null)
                speedText.text = $"x{speed:F0}";

            if (buttonImage != null)
            {
                if (speed >= 3f)
                    buttonImage.color = maxSpeedColor;
                else if (speed >= 2f)
                    buttonImage.color = fastSpeedColor;
                else
                    buttonImage.color = normalSpeedColor;
            }
        }

        private void CreateSpeedButton()
        {
            RectTransform myRect = GetComponent<RectTransform>();
            if (myRect == null)
                myRect = gameObject.AddComponent<RectTransform>();

            float btnSize = GameSceneDesignTokens.UtilityButtonSize;
            myRect.anchorMin = new Vector2(1f, 1f);
            myRect.anchorMax = new Vector2(1f, 1f);
            myRect.pivot = new Vector2(1f, 1f);
            float yOffset = GameSceneDesignTokens.HudHeight + 8 + (btnSize + 8) * 2;
            myRect.anchoredPosition = new Vector2(-12, -yOffset);
            myRect.sizeDelta = new Vector2(btnSize, btnSize);

            buttonImage = gameObject.AddComponent<Image>();
            buttonImage.color = normalSpeedColor;
            Sprite rounded = CuteUIHelper.GetRoundedRectSprite(14);
            if (rounded != null)
            {
                buttonImage.sprite = rounded;
                buttonImage.type = Image.Type.Sliced;
            }

            Shadow shadow = gameObject.AddComponent<Shadow>();
            shadow.effectColor = CuteUIHelper.SoftShadow;
            shadow.effectDistance = new Vector2(2, -2);

            speedButton = gameObject.AddComponent<Button>();
            ColorBlock colors = speedButton.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.98f, 0.95f, 1f);
            colors.pressedColor = new Color(0.85f, 0.82f, 0.78f, 1f);
            colors.fadeDuration = 0.08f;
            speedButton.colors = colors;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(2, 2);
            textRect.offsetMax = new Vector2(-2, -2);

            speedText = textObj.AddComponent<Text>();
            speedText.text = "x1";
            speedText.font = GameFont.Get();
            speedText.fontSize = 22;
            speedText.color = CuteUIHelper.DarkText;
            speedText.alignment = TextAnchor.MiddleCenter;
            speedText.fontStyle = FontStyle.Bold;
            speedText.resizeTextForBestFit = true;
            speedText.resizeTextMinSize = 14;
            speedText.resizeTextMaxSize = 22;
        }
    }
}
