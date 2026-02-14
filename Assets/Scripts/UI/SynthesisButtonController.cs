using System;
using UnityEngine;
using UnityEngine.UI;
using LottoDefense.Units;

namespace LottoDefense.UI
{
    /// <summary>
    /// Floating "조합" button that hovers above a target unit.
    /// Tracks target unit position via WorldToScreenPoint each frame.
    /// Invokes a callback with (source, target) when clicked.
    /// </summary>
    public class SynthesisButtonController : MonoBehaviour
    {
        #region Private Fields
        private Unit sourceUnit;
        private Unit targetUnit;
        private Action<Unit, Unit> onClickCallback;
        private RectTransform rectTransform;
        private Camera cachedCamera;
        private const float HEIGHT_OFFSET = 0.8f;
        #endregion

        #region Initialization
        public void Initialize(Unit source, Unit target, Action<Unit, Unit> callback)
        {
            sourceUnit = source;
            targetUnit = target;
            onClickCallback = callback;

            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }

            // Critical: Set anchors to bottom-left so position works as absolute screen coords
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(
                GameSceneDesignTokens.SynthFloatBtnWidth,
                GameSceneDesignTokens.SynthFloatBtnHeight);

            CreateButtonUI();
            UpdatePosition();
        }
        #endregion

        #region Unity Lifecycle
        private void Update()
        {
            if (sourceUnit == null || targetUnit == null)
            {
                Destroy(gameObject);
                return;
            }

            UpdatePosition();
        }

        private void OnDestroy()
        {
            onClickCallback = null;
        }
        #endregion

        #region UI Setup
        private void CreateButtonUI()
        {
            // Outer border glow
            Image borderImage = gameObject.AddComponent<Image>();
            borderImage.color = GameSceneDesignTokens.SynthFloatBtnBorder;
            borderImage.raycastTarget = true;

            Outline borderOutline = gameObject.AddComponent<Outline>();
            borderOutline.effectColor = new Color(0f, 0f, 0f, 0.6f);
            borderOutline.effectDistance = new Vector2(2, -2);

            // Inner button panel (child for visual depth)
            GameObject innerObj = new GameObject("Inner");
            innerObj.transform.SetParent(transform, false);
            RectTransform innerRect = innerObj.AddComponent<RectTransform>();
            innerRect.anchorMin = Vector2.zero;
            innerRect.anchorMax = Vector2.one;
            innerRect.offsetMin = new Vector2(2, 2);
            innerRect.offsetMax = new Vector2(-2, -2);

            Image innerBg = innerObj.AddComponent<Image>();
            innerBg.color = GameSceneDesignTokens.SynthFloatBtnBg;
            innerBg.raycastTarget = false;

            // Top highlight strip for depth
            GameObject highlightObj = new GameObject("Highlight");
            highlightObj.transform.SetParent(innerObj.transform, false);
            RectTransform highlightRect = highlightObj.AddComponent<RectTransform>();
            highlightRect.anchorMin = new Vector2(0, 0.55f);
            highlightRect.anchorMax = Vector2.one;
            highlightRect.offsetMin = new Vector2(1, 0);
            highlightRect.offsetMax = new Vector2(-1, -1);

            Image highlightImg = highlightObj.AddComponent<Image>();
            highlightImg.color = new Color(1f, 1f, 1f, 0.2f);
            highlightImg.raycastTarget = false;

            // Button component on root
            Button button = gameObject.AddComponent<Button>();
            button.targetGraphic = borderImage;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            button.onClick.AddListener(OnButtonClicked);

            // Shadow for floating effect
            Shadow shadow = gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
            shadow.effectDistance = new Vector2(3, -3);

            // Text label
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(innerObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text label = textObj.AddComponent<Text>();
            label.text = "\u2728 \uC870\uD569"; // ✨ 조합
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 22;
            label.fontStyle = FontStyle.Bold;
            label.color = GameSceneDesignTokens.SynthFloatBtnText;
            label.alignment = TextAnchor.MiddleCenter;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;

            // Text outline for readability
            Outline textOutline = textObj.AddComponent<Outline>();
            textOutline.effectColor = new Color(1f, 1f, 1f, 0.3f);
            textOutline.effectDistance = new Vector2(1, -1);
        }
        #endregion

        #region Position Tracking
        private void UpdatePosition()
        {
            if (targetUnit == null || rectTransform == null) return;

            Vector3 worldPos = targetUnit.transform.position + Vector3.up * HEIGHT_OFFSET;
            if (cachedCamera == null) cachedCamera = Camera.main;
            Vector3 screenPos = cachedCamera.WorldToScreenPoint(worldPos);
            rectTransform.position = screenPos;
        }
        #endregion

        #region Button Handler
        private void OnButtonClicked()
        {
            if (sourceUnit != null && targetUnit != null)
            {
                onClickCallback?.Invoke(sourceUnit, targetUnit);
            }
        }
        #endregion
    }
}
