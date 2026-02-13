using UnityEngine;
using UnityEngine.UI;

namespace LottoDefense.UI
{
    public class NotificationBadge : MonoBehaviour
    {
        private Text countText;
        private Image bgImage;

        public static NotificationBadge Create(Transform parent, Font font)
        {
            GameObject badgeObj = new GameObject("Badge");
            badgeObj.transform.SetParent(parent, false);

            RectTransform rect = badgeObj.GetComponent<RectTransform>();
            if (rect == null) rect = badgeObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(-4f, -4f);
            rect.sizeDelta = new Vector2(LobbyDesignTokens.BadgeSize, LobbyDesignTokens.BadgeSize);

            Image bg = badgeObj.AddComponent<Image>();
            bg.color = LobbyDesignTokens.BadgeBg;

            GameObject textObj = new GameObject("Count");
            textObj.transform.SetParent(badgeObj.transform, false);
            Text text = textObj.AddComponent<Text>();
            text.font = font;
            text.fontSize = LobbyDesignTokens.BadgeFontSize;
            text.color = LobbyDesignTokens.BadgeText;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            NotificationBadge badge = badgeObj.AddComponent<NotificationBadge>();
            badge.countText = text;
            badge.bgImage = bg;

            badgeObj.SetActive(false);
            return badge;
        }

        public void Show(int count)
        {
            if (count <= 0)
            {
                Hide();
                return;
            }
            gameObject.SetActive(true);
            if (countText != null)
                countText.text = count > 99 ? "99+" : count.ToString();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
