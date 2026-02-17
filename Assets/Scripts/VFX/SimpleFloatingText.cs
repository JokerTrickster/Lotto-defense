using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace LottoDefense.VFX
{
    /// <summary>
    /// Simple floating text that always works - WorldSpace Canvas version.
    /// </summary>
    public class SimpleFloatingText : MonoBehaviour
    {
        public static void Show(Vector3 worldPosition, string message, Color color, float fontSize = 48f)
        {
            GameObject textObj = new GameObject("SkillEffect_" + message);
            
            // Add Canvas (WorldSpace)
            Canvas canvas = textObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            
            // Position in world
            textObj.transform.position = worldPosition;
            textObj.transform.localScale = Vector3.one * 0.01f; // Scale down for WorldSpace
            
            // Add CanvasScaler
            CanvasScaler scaler = textObj.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10;
            
            // Setup RectTransform
            RectTransform rectTransform = textObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(400, 100);
            
            // Add Text object
            GameObject textChild = new GameObject("Text");
            textChild.transform.SetParent(textObj.transform, false);
            
            Text text = textChild.AddComponent<Text>();
            text.text = message;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (text.font == null)
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            
            text.fontSize = (int)fontSize;
            text.fontStyle = FontStyle.Bold;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            
            // Add outline for visibility
            Outline outline = textChild.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(3, -3);
            
            RectTransform textRect = textChild.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            // Add CanvasGroup for fading
            CanvasGroup canvasGroup = textObj.AddComponent<CanvasGroup>();
            
            // Start animation
            textObj.AddComponent<SimpleFloatingTextAnimator>().Initialize(canvasGroup, textObj, worldPosition);
            
            Debug.Log($"[SimpleFloatingText] ✅ Created: '{message}' at {worldPosition}");
        }
    }
    
    /// <summary>
    /// Animator component for SimpleFloatingText.
    /// </summary>
    public class SimpleFloatingTextAnimator : MonoBehaviour
    {
        private CanvasGroup canvasGroup;
        private GameObject textObject;
        private Vector3 startPosition;
        private float lifetime = 2f;
        private float elapsed = 0f;
        
        public void Initialize(CanvasGroup cg, GameObject obj, Vector3 startPos)
        {
            canvasGroup = cg;
            textObject = obj;
            startPosition = startPos;
        }
        
        private void Update()
        {
            if (textObject == null) return;
            
            elapsed += Time.deltaTime;
            float t = elapsed / lifetime;
            
            // Move up
            textObject.transform.position = startPosition + Vector3.up * (elapsed * 0.5f);
            
            // Fade out
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f - t;
            }
            
            // Destroy when done
            if (elapsed >= lifetime)
            {
                Destroy(textObject);
            }
        }
    }
}
