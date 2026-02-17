using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace LottoDefense.VFX
{
    /// <summary>
    /// Simple floating text using 3D TextMesh - guaranteed to be visible!
    /// </summary>
    public class SimpleFloatingText : MonoBehaviour
    {
        public static void Show(Vector3 worldPosition, string message, Color color, float fontSize = 0.1f)
        {
            Debug.Log($"[SimpleFloatingText] 🎯 Creating: '{message}' at {worldPosition}");
            
            // Create GameObject with TextMesh (3D text)
            GameObject textObj = new GameObject("SkillEffect_" + message);
            textObj.transform.position = worldPosition;
            
            // Add TextMesh component (built-in 3D text)
            TextMesh textMesh = textObj.AddComponent<TextMesh>();
            textMesh.text = message;
            textMesh.fontSize = 100; // High resolution
            textMesh.characterSize = fontSize; // World size
            textMesh.color = color;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.fontStyle = FontStyle.Bold;
            
            // Add MeshRenderer (required for TextMesh)
            MeshRenderer renderer = textObj.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = 1000; // Render on top
            }
            
            // Make text face camera
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                textObj.transform.LookAt(mainCam.transform);
                textObj.transform.Rotate(0, 180, 0); // Flip to face correctly
            }
            
            // Add animator
            textObj.AddComponent<SimpleFloatingTextAnimator>().Initialize(textObj, worldPosition, color);
            
            Debug.Log($"[SimpleFloatingText] ✅ Created 3D TextMesh: '{message}' at {worldPosition}");
        }
    }
    
    /// <summary>
    /// Animator component for SimpleFloatingText with TextMesh.
    /// </summary>
    public class SimpleFloatingTextAnimator : MonoBehaviour
    {
        private GameObject textObject;
        private TextMesh textMesh;
        private Vector3 startPosition;
        private Color startColor;
        private float lifetime = 3f; // Longer lifetime
        private float elapsed = 0f;
        
        public void Initialize(GameObject obj, Vector3 startPos, Color color)
        {
            textObject = obj;
            startPosition = startPos;
            startColor = color;
            textMesh = obj.GetComponent<TextMesh>();
        }
        
        private void Update()
        {
            if (textObject == null) return;
            
            elapsed += Time.deltaTime;
            float t = elapsed / lifetime;
            
            // Move up and slightly scale up
            float moveUp = elapsed * 0.8f;
            float scale = 1f + (elapsed * 0.3f); // Grow slightly
            textObject.transform.position = startPosition + Vector3.up * moveUp;
            textObject.transform.localScale = Vector3.one * scale;
            
            // Always face camera
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                textObject.transform.LookAt(mainCam.transform);
                textObject.transform.Rotate(0, 180, 0);
            }
            
            // Fade out color
            if (textMesh != null)
            {
                float alpha = 1f - t;
                textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            }
            
            // Destroy when done
            if (elapsed >= lifetime)
            {
                Destroy(textObject);
            }
        }
    }
}
