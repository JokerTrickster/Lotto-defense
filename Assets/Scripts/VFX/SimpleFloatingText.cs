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
        public static void Show(Vector3 worldPosition, string message, Color color, float fontSize = 0.025f)
        {
            // Create GameObject with TextMesh (3D text)
            GameObject textObj = new GameObject("SkillText");
            textObj.transform.position = worldPosition;
            
            // Add TextMesh component (built-in 3D text)
            TextMesh textMesh = textObj.AddComponent<TextMesh>();
            textMesh.text = message;
            textMesh.fontSize = 80; // High resolution
            textMesh.characterSize = fontSize; // World size (작게 조정: 0.12 → 0.05)
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
            
            // Add animator (심플하게)
            textObj.AddComponent<SimpleFloatingTextAnimator>().Initialize(textObj, worldPosition, color);
        }
    }
    
    /// <summary>
    /// 심플한 애니메이터: 위로 떠오르며 페이드아웃
    /// </summary>
    public class SimpleFloatingTextAnimator : MonoBehaviour
    {
        private GameObject textObject;
        private TextMesh textMesh;
        private Vector3 startPosition;
        private Color startColor;
        private float lifetime = 2f; // 2초
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
            
            // 위로 천천히 이동
            float moveUp = elapsed * 0.5f;
            textObject.transform.position = startPosition + Vector3.up * moveUp;
            
            // 카메라 방향으로 회전
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                textObject.transform.LookAt(mainCam.transform);
                textObject.transform.Rotate(0, 180, 0);
            }
            
            // 페이드아웃
            if (textMesh != null)
            {
                float alpha = 1f - t;
                textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            }
            
            // 2초 후 삭제
            if (elapsed >= lifetime)
            {
                Destroy(textObject);
            }
        }
    }
}
