using UnityEngine;

namespace LottoDefense.UI
{
    /// <summary>
    /// Adjusts a RectTransform to fit within the device's safe area.
    /// Handles notches, Dynamic Island, home indicators, and rounded corners.
    /// Attach to a full-screen RectTransform that parents all UI content.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaAdapter : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Rect lastSafeArea;
        private Vector2Int lastScreenSize;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        private void Update()
        {
            // Re-apply if screen size or safe area changed (rotation, etc.)
            if (Screen.safeArea != lastSafeArea ||
                Screen.width != lastScreenSize.x ||
                Screen.height != lastScreenSize.y)
            {
                ApplySafeArea();
            }
        }

        private void ApplySafeArea()
        {
            Rect safeArea = Screen.safeArea;
            lastSafeArea = safeArea;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);

            if (Screen.width <= 0 || Screen.height <= 0) return;

            // Convert safe area from pixel coords to anchor coords (0..1)
            Vector2 anchorMin = new Vector2(
                safeArea.x / Screen.width,
                safeArea.y / Screen.height);
            Vector2 anchorMax = new Vector2(
                (safeArea.x + safeArea.width) / Screen.width,
                (safeArea.y + safeArea.height) / Screen.height);

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}
