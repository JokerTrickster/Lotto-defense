using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace LottoDefense.VFX
{
    /// <summary>
    /// 화려한 스킬 이펙트 UI - 유닛 이름 + 스킬 이름 표시
    /// </summary>
    public class SkillEffectUI : MonoBehaviour
    {
        public static void Show(Vector3 worldPosition, string unitName, string skillName, Color skillColor)
        {
            
            // Find or create UI Canvas
            Canvas uiCanvas = FindUICanvas();
            if (uiCanvas == null)
            {
                Debug.LogError("[SkillEffectUI] No UI Canvas found!");
                return;
            }
            
            // Create effect container
            GameObject effectObj = new GameObject($"SkillEffect_{unitName}_{skillName}");
            effectObj.transform.SetParent(uiCanvas.transform, false);
            
            RectTransform rect = effectObj.AddComponent<RectTransform>();
            
            // Convert world position to screen position
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
            rect.position = screenPos;
            rect.sizeDelta = new Vector2(300, 120);
            
            // Background panel (glow effect)
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(effectObj.transform, false);
            
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(skillColor.r, skillColor.g, skillColor.b, 0.3f);
            bgImage.raycastTarget = false;
            
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            
            // Unit name (smaller, on top)
            GameObject unitNameObj = new GameObject("UnitName");
            unitNameObj.transform.SetParent(effectObj.transform, false);
            
            Text unitNameText = unitNameObj.AddComponent<Text>();
            unitNameText.text = unitName;
            unitNameText.font = GetFont();
            unitNameText.fontSize = 24;
            unitNameText.fontStyle = FontStyle.Bold;
            unitNameText.color = new Color(1f, 1f, 1f, 0.9f);
            unitNameText.alignment = TextAnchor.MiddleCenter;
            unitNameText.raycastTarget = false;
            
            // Add shadow for depth
            Shadow unitShadow = unitNameObj.AddComponent<Shadow>();
            unitShadow.effectColor = new Color(0, 0, 0, 0.8f);
            unitShadow.effectDistance = new Vector2(2, -2);
            
            RectTransform unitNameRect = unitNameObj.GetComponent<RectTransform>();
            unitNameRect.anchorMin = new Vector2(0, 0.55f);
            unitNameRect.anchorMax = new Vector2(1, 0.95f);
            unitNameRect.sizeDelta = Vector2.zero;
            
            // Skill name (larger, main text)
            GameObject skillNameObj = new GameObject("SkillName");
            skillNameObj.transform.SetParent(effectObj.transform, false);
            
            Text skillNameText = skillNameObj.AddComponent<Text>();
            skillNameText.text = skillName;
            skillNameText.font = GetFont();
            skillNameText.fontSize = 42;
            skillNameText.fontStyle = FontStyle.Bold;
            skillNameText.color = skillColor;
            skillNameText.alignment = TextAnchor.MiddleCenter;
            skillNameText.raycastTarget = false;
            
            // Add outline for better visibility
            Outline skillOutline = skillNameObj.AddComponent<Outline>();
            skillOutline.effectColor = new Color(0, 0, 0, 1f);
            skillOutline.effectDistance = new Vector2(3, -3);
            
            // Add shadow
            Shadow skillShadow = skillNameObj.AddComponent<Shadow>();
            skillShadow.effectColor = new Color(0, 0, 0, 0.5f);
            skillShadow.effectDistance = new Vector2(4, -4);
            
            RectTransform skillNameRect = skillNameObj.GetComponent<RectTransform>();
            skillNameRect.anchorMin = new Vector2(0, 0.05f);
            skillNameRect.anchorMax = new Vector2(1, 0.55f);
            skillNameRect.sizeDelta = Vector2.zero;
            
            // Add CanvasGroup for fading
            CanvasGroup canvasGroup = effectObj.AddComponent<CanvasGroup>();
            
            // Start animation
            SkillEffectAnimator animator = effectObj.AddComponent<SkillEffectAnimator>();
            animator.Initialize(effectObj, rect, canvasGroup, bgImage, skillColor, screenPos);
            
        }
        
        private static Canvas FindUICanvas()
        {
            // Try to find GameCanvas first
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas c in canvases)
            {
                if (c.gameObject.name == "GameCanvas" || c.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    return c;
                }
            }
            
            // Create one if not found
            GameObject canvasObj = new GameObject("SkillEffectCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999; // Very high
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            return canvas;
        }
        
        private static Font GetFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return font;
        }
    }
    
    /// <summary>
    /// 화려한 애니메이션: 확대 → 펄스 → 축소 + 페이드아웃
    /// </summary>
    public class SkillEffectAnimator : MonoBehaviour
    {
        private GameObject effectObject;
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Image backgroundImage;
        private Color skillColor;
        private Vector3 startScreenPos;
        
        private float elapsed = 0f;
        private float lifetime = 2.5f;
        
        // Animation phases
        private float popInDuration = 0.2f;
        private float pulseDuration = 1.0f;
        private float fadeOutDuration = 1.3f;
        
        public void Initialize(GameObject obj, RectTransform rect, CanvasGroup cg, Image bg, Color color, Vector3 screenPos)
        {
            effectObject = obj;
            rectTransform = rect;
            canvasGroup = cg;
            backgroundImage = bg;
            skillColor = color;
            startScreenPos = screenPos;
            
            // Start with scale 0
            rectTransform.localScale = Vector3.zero;
        }
        
        private void Update()
        {
            if (effectObject == null) return;
            
            elapsed += Time.deltaTime;
            
            // Phase 1: Pop in (0.2s)
            if (elapsed < popInDuration)
            {
                float t = elapsed / popInDuration;
                float easeOut = 1f - Mathf.Pow(1f - t, 3); // Ease out cubic
                rectTransform.localScale = Vector3.one * easeOut;
                
                // Slight rotation
                float rotation = Mathf.Lerp(0, 360, t);
                rectTransform.rotation = Quaternion.Euler(0, 0, rotation * 0.2f);
            }
            // Phase 2: Pulse (1.0s)
            else if (elapsed < popInDuration + pulseDuration)
            {
                float t = (elapsed - popInDuration) / pulseDuration;
                
                // Pulsing scale
                float pulse = 1f + Mathf.Sin(t * Mathf.PI * 4) * 0.1f;
                rectTransform.localScale = Vector3.one * pulse;
                
                // Glow effect on background
                float glow = 0.3f + Mathf.Sin(t * Mathf.PI * 6) * 0.15f;
                backgroundImage.color = new Color(skillColor.r, skillColor.g, skillColor.b, glow);
                
                // Float upward
                float moveUp = t * 80f;
                rectTransform.position = startScreenPos + new Vector3(0, moveUp, 0);
            }
            // Phase 3: Fade out (1.3s)
            else
            {
                float t = (elapsed - popInDuration - pulseDuration) / fadeOutDuration;
                
                // Continue moving up
                float moveUp = (pulseDuration / fadeOutDuration) * 80f + t * 100f;
                rectTransform.position = startScreenPos + new Vector3(0, moveUp, 0);
                
                // Fade out
                canvasGroup.alpha = 1f - t;
                
                // Slight scale up
                float scale = 1f + t * 0.5f;
                rectTransform.localScale = Vector3.one * scale;
            }
            
            // Destroy when done
            if (elapsed >= lifetime)
            {
                Destroy(effectObject);
            }
        }
    }
}
