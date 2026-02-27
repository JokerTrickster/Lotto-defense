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
            Image bgImage = gameObject.AddComponent<Image>();
            bgImage.color = GameSceneDesignTokens.SynthFloatBtnBg;
            bgImage.raycastTarget = true;
            Sprite rounded = CuteUIHelper.GetRoundedRectSprite(14);
            if (rounded != null)
            {
                bgImage.sprite = rounded;
                bgImage.type = Image.Type.Sliced;
            }

            Outline outline = gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.85f, 0.65f, 0.1f, 0.7f);
            outline.effectDistance = new Vector2(2, -2);

            Shadow shadow = gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.3f);
            shadow.effectDistance = new Vector2(1, -2);

            Button button = gameObject.AddComponent<Button>();
            button.targetGraphic = bgImage;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.95f, 0.85f, 1f);
            colors.pressedColor = new Color(0.9f, 0.8f, 0.6f, 1f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            button.onClick.AddListener(OnButtonClicked);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text label = textObj.AddComponent<Text>();
            label.text = "\uC870\uD569";
            label.font = GameFont.Get();
            label.fontSize = 20;
            label.fontStyle = FontStyle.Bold;
            label.color = GameSceneDesignTokens.SynthFloatBtnText;
            label.alignment = TextAnchor.MiddleCenter;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
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
