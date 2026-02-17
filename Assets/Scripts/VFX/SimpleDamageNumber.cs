using UnityEngine;

namespace LottoDefense.VFX
{
    /// <summary>
    /// 간단한 3D TextMesh 데미지 숫자 - VFXManager 없이 독립적으로 동작
    /// </summary>
    public class SimpleDamageNumber : MonoBehaviour
    {
        /// <summary>
        /// 데미지 숫자 표시 (몬스터 위에)
        /// </summary>
        public static void Show(Vector3 worldPosition, int damage, bool isCritical = false)
        {
            // 데미지 텍스트 생성
            GameObject damageObj = new GameObject("DamageNumber");
            damageObj.transform.position = worldPosition + Vector3.up * 0.3f; // 몬스터 위
            
            // TextMesh 추가
            TextMesh textMesh = damageObj.AddComponent<TextMesh>();
            textMesh.text = damage.ToString();
            textMesh.fontSize = 80;
            textMesh.characterSize = 0.03f; // 작은 크기
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.fontStyle = FontStyle.Bold;
            
            // 색상: 일반(노란색), 크리티컬(빨간색)
            if (isCritical)
            {
                textMesh.color = new Color(1f, 0.2f, 0.2f); // 빨간색
                textMesh.characterSize = 0.04f; // 크리티컬은 조금 더 크게
            }
            else
            {
                textMesh.color = new Color(1f, 1f, 0.3f); // 노란색
            }
            
            // MeshRenderer 설정
            MeshRenderer renderer = damageObj.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = 1000;
            }
            
            // 카메라 방향으로 회전
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                damageObj.transform.LookAt(mainCam.transform);
                damageObj.transform.Rotate(0, 180, 0);
            }
            
            // 애니메이터 추가
            damageObj.AddComponent<SimpleDamageNumberAnimator>().Initialize(damageObj, worldPosition, isCritical);
        }
    }
    
    /// <summary>
    /// 데미지 숫자 애니메이션 (위로 떠오르며 페이드아웃)
    /// </summary>
    public class SimpleDamageNumberAnimator : MonoBehaviour
    {
        private GameObject damageObject;
        private TextMesh textMesh;
        private Vector3 startPosition;
        private bool isCritical;
        private float lifetime = 1.0f;
        private float elapsed = 0f;
        
        public void Initialize(GameObject obj, Vector3 startPos, bool crit)
        {
            damageObject = obj;
            startPosition = startPos + Vector3.up * 0.3f;
            isCritical = crit;
            textMesh = obj.GetComponent<TextMesh>();
        }
        
        private void Update()
        {
            if (damageObject == null) return;
            
            elapsed += Time.deltaTime;
            float t = elapsed / lifetime;
            
            // 위로 빠르게 이동 (크리티컬은 더 빠르게)
            float moveSpeed = isCritical ? 0.8f : 0.5f;
            float moveUp = elapsed * moveSpeed;
            damageObject.transform.position = startPosition + Vector3.up * moveUp;
            
            // 카메라 방향 유지
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                damageObject.transform.LookAt(mainCam.transform);
                damageObject.transform.Rotate(0, 180, 0);
            }
            
            // 페이드아웃
            if (textMesh != null)
            {
                Color color = textMesh.color;
                color.a = 1f - t;
                textMesh.color = color;
            }
            
            // 1초 후 삭제
            if (elapsed >= lifetime)
            {
                Destroy(damageObject);
            }
        }
    }
}
