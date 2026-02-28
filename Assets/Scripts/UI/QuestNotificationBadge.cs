using UnityEngine;
using UnityEngine.UI;
using LottoDefense.Quests;

namespace LottoDefense.UI
{
    /// <summary>
    /// 퀘스트 버튼에 표시되는 알림 배지 (완료된 퀘스트 개수)
    /// </summary>
    public class QuestNotificationBadge : MonoBehaviour
    {
        #region Singleton
        private static QuestNotificationBadge _instance;
        
        public static QuestNotificationBadge Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<QuestNotificationBadge>();
                }
                return _instance;
            }
        }
        #endregion

        #region UI Elements
        private GameObject badgeObj;
        private Text badgeText;
        private int notificationCount = 0;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _instance = this;
        }

        private void Start()
        {
            // QuestManager 이벤트 구독
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestCompleted += OnQuestCompleted;
            }

            // 초기 상태: 숨김
            UpdateBadge();
        }

        private void OnDestroy()
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestCompleted -= OnQuestCompleted;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 배지 UI 생성 (퀘스트 버튼의 우상단에 배치)
        /// </summary>
        public void CreateBadge(Transform parentButton)
        {
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null)
                defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // 배지 컨테이너
            badgeObj = new GameObject("QuestBadge");
            badgeObj.transform.SetParent(parentButton, false);

            RectTransform badgeRect = badgeObj.AddComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(1f, 1f); // 우상단
            badgeRect.anchorMax = new Vector2(1f, 1f);
            badgeRect.pivot = new Vector2(0.5f, 0.5f);
            badgeRect.anchoredPosition = new Vector2(-8, -8); // 버튼 우상단에서 약간 안쪽
            badgeRect.sizeDelta = new Vector2(28, 28); // 배지 크기

            // 배지 배경 (빨간 원)
            Image badgeImage = badgeObj.AddComponent<Image>();
            badgeImage.color = new Color(0.95f, 0.25f, 0.25f, 1f); // 빨간색
            badgeImage.type = Image.Type.Sliced;
            
            // 동그란 모양
            Sprite circleSprite = CreateCircleSprite();
            if (circleSprite != null)
            {
                badgeImage.sprite = circleSprite;
            }

            // 배지 외곽선
            Outline badgeOutline = badgeObj.AddComponent<Outline>();
            badgeOutline.effectColor = new Color(0.8f, 0.15f, 0.15f, 1f);
            badgeOutline.effectDistance = new Vector2(1, -1);

            // 숫자 텍스트
            GameObject textObj = new GameObject("BadgeText");
            textObj.transform.SetParent(badgeObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            badgeText = textObj.AddComponent<Text>();
            badgeText.text = "0";
            badgeText.font = defaultFont;
            badgeText.fontSize = 18;
            badgeText.fontStyle = FontStyle.Bold;
            badgeText.color = Color.white;
            badgeText.alignment = TextAnchor.MiddleCenter;
            badgeText.horizontalOverflow = HorizontalWrapMode.Overflow;
            badgeText.verticalOverflow = VerticalWrapMode.Overflow;
            badgeText.raycastTarget = false;

            // 텍스트 외곽선
            Outline textOutline = textObj.AddComponent<Outline>();
            textOutline.effectColor = new Color(0.3f, 0.1f, 0.1f, 0.8f);
            textOutline.effectDistance = new Vector2(1, -1);

            // 초기 상태: 숨김
            badgeObj.SetActive(false);

            Debug.Log("[QuestNotificationBadge] Badge UI created");
        }

        /// <summary>
        /// 알림 카운트 증가
        /// </summary>
        public void IncrementCount()
        {
            notificationCount++;
            UpdateBadge();
            Debug.Log($"[QuestNotificationBadge] Count increased to {notificationCount}");
        }

        /// <summary>
        /// 알림 카운트 초기화 (퀘스트 UI를 열었을 때)
        /// </summary>
        public void ResetCount()
        {
            notificationCount = 0;
            UpdateBadge();
            Debug.Log("[QuestNotificationBadge] Count reset");
        }

        /// <summary>
        /// 수동으로 카운트 설정
        /// </summary>
        public void SetCount(int count)
        {
            notificationCount = Mathf.Max(0, count);
            UpdateBadge();
        }
        #endregion

        #region Private Methods
        private void OnQuestCompleted(QuestInstance quest)
        {
            // 히든 퀘스트 달성 시 알림 증가
            IncrementCount();
        }

        private void UpdateBadge()
        {
            if (badgeObj == null || badgeText == null)
                return;

            if (notificationCount > 0)
            {
                badgeObj.SetActive(true);
                badgeText.text = notificationCount.ToString();

                // 99+ 표시
                if (notificationCount > 99)
                {
                    badgeText.text = "99+";
                    badgeText.fontSize = 14; // 작게
                }
                else
                {
                    badgeText.fontSize = 18; // 기본 크기
                }
            }
            else
            {
                badgeObj.SetActive(false);
            }
        }

        private Sprite CreateCircleSprite()
        {
            // 32x32 원형 텍스처 생성
            int size = 32;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;

            Color[] pixels = new Color[size * size];
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f - 1f; // 외곽 1픽셀 여유

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float distance = Vector2.Distance(pos, center);

                    // 원형 안티앨리어싱
                    if (distance <= radius - 1f)
                    {
                        pixels[y * size + x] = Color.white;
                    }
                    else if (distance <= radius)
                    {
                        float alpha = 1f - (distance - (radius - 1f));
                        pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect
            );
        }
        #endregion
    }
}
