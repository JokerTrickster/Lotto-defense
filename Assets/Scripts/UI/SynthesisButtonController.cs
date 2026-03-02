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
        private const float HEIGHT_PADDING = 0.05f;
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
            // Drop shadow (behind button)
            GameObject shadowObj = new GameObject("Shadow");
            shadowObj.transform.SetParent(transform, false);
            RectTransform shadowRect = shadowObj.AddComponent<RectTransform>();
            shadowRect.anchorMin = Vector2.zero;
            shadowRect.anchorMax = Vector2.one;
            shadowRect.offsetMin = new Vector2(-1f, -3f);
            shadowRect.offsetMax = new Vector2(3f, -1f);
            Image shadowImage = shadowObj.AddComponent<Image>();
            shadowImage.color = GameSceneDesignTokens.SynthFloatBtnShadow;
            Sprite shadowRounded = CuteUIHelper.GetRoundedRectSprite(18);
            if (shadowRounded != null)
            {
                shadowImage.sprite = shadowRounded;
                shadowImage.type = Image.Type.Sliced;
            }
            shadowImage.raycastTarget = false;

            // Border layer
            GameObject borderObj = new GameObject("Border");
            borderObj.transform.SetParent(transform, false);
            RectTransform borderRect = borderObj.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = new Vector2(-1.5f, -1.5f);
            borderRect.offsetMax = new Vector2(1.5f, 1.5f);
            Image borderImage = borderObj.AddComponent<Image>();
            borderImage.color = GameSceneDesignTokens.SynthFloatBtnBorder;
            Sprite borderRounded = CuteUIHelper.GetRoundedRectSprite(18);
            if (borderRounded != null)
            {
                borderImage.sprite = borderRounded;
                borderImage.type = Image.Type.Sliced;
            }
            borderImage.raycastTarget = false;

            // Main background
            Image bgImage = gameObject.AddComponent<Image>();
            bgImage.color = GameSceneDesignTokens.SynthFloatBtnBg;
            bgImage.raycastTarget = true;
            Sprite rounded = CuteUIHelper.GetRoundedRectSprite(16);
            if (rounded != null)
            {
                bgImage.sprite = rounded;
                bgImage.type = Image.Type.Sliced;
            }

            // Top highlight (gives 3D/glossy feel)
            GameObject highlightObj = new GameObject("Highlight");
            highlightObj.transform.SetParent(transform, false);
            RectTransform hlRect = highlightObj.AddComponent<RectTransform>();
            hlRect.anchorMin = new Vector2(0f, 0.45f);
            hlRect.anchorMax = Vector2.one;
            hlRect.offsetMin = new Vector2(2f, 0f);
            hlRect.offsetMax = new Vector2(-2f, -2f);
            Image hlImage = highlightObj.AddComponent<Image>();
            hlImage.color = GameSceneDesignTokens.SynthFloatBtnTopHighlight;
            Sprite hlRounded = CuteUIHelper.GetRoundedRectSprite(14);
            if (hlRounded != null)
            {
                hlImage.sprite = hlRounded;
                hlImage.type = Image.Type.Sliced;
            }
            hlImage.raycastTarget = false;

            // Button component
            Button button = gameObject.AddComponent<Button>();
            button.targetGraphic = bgImage;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.95f, 0.85f, 1f);
            colors.pressedColor = new Color(0.85f, 0.72f, 0.5f, 1f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            button.onClick.AddListener(OnButtonClicked);

            // Text (centered)
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            textRect.pivot = new Vector2(0.5f, 0.5f);

            Text label = textObj.AddComponent<Text>();
            label.text = "\u2728 \uC870\uD569";
            label.font = GameFont.Get();
            label.fontSize = 22;
            label.fontStyle = FontStyle.Bold;
            label.color = GameSceneDesignTokens.SynthFloatBtnText;
            label.alignment = TextAnchor.MiddleCenter;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;

            // Text shadow for readability
            Shadow textShadow = textObj.AddComponent<Shadow>();
            textShadow.effectColor = new Color(1f, 1f, 1f, 0.4f);
            textShadow.effectDistance = new Vector2(0f, 1f);
        }
        #endregion

        #region Position Tracking
        private void UpdatePosition()
        {
            if (targetUnit == null || rectTransform == null) return;

            float unitHalfHeight = targetUnit.transform.localScale.y * 0.5f;
            Vector3 worldPos = targetUnit.transform.position + Vector3.up * (unitHalfHeight + HEIGHT_PADDING);
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
