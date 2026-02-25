using UnityEngine;
using UnityEngine.UI;
using LottoDefense.Networking;

namespace LottoDefense.UI
{
    /// <summary>
    /// 협동 플레이 시 상대방 상태를 표시하는 UI
    /// </summary>
    public class OpponentStatusUI : MonoBehaviour
    {
        #region Singleton
        private static OpponentStatusUI _instance;
        
        public static OpponentStatusUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<OpponentStatusUI>();
                }
                return _instance;
            }
        }
        #endregion

        #region UI Elements
        private GameObject opponentPanel;
        private Text opponentNameText;
        private Text opponentHPText;
        private Text opponentRoundText;
        private Text opponentGoldText;
        private Text opponentKillsText;
        private Image opponentHPBar;
        private GameObject defeatOverlay;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _instance = this;
            CreateOpponentUI();
            
            // 이벤트 구독
            CoopStateSync.Instance.OnOpponentStateUpdated += UpdateOpponentState;
            CoopStateSync.Instance.OnOpponentDefeated += ShowDefeat;
        }

        private void OnDestroy()
        {
            if (CoopStateSync.Instance != null)
            {
                CoopStateSync.Instance.OnOpponentStateUpdated -= UpdateOpponentState;
                CoopStateSync.Instance.OnOpponentDefeated -= ShowDefeat;
            }
        }

        private void Update()
        {
            // 협동 플레이 중일 때만 표시
            if (opponentPanel != null)
            {
                opponentPanel.SetActive(CoopStateSync.Instance.IsEnabled);
            }
        }
        #endregion

        #region UI Creation
        private void CreateOpponentUI()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[OpponentStatusUI] No Canvas found!");
                return;
            }

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null)
                defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // 메인 패널 (좌상단)
            opponentPanel = new GameObject("OpponentPanel");
            opponentPanel.transform.SetParent(canvas.transform, false);

            RectTransform panelRect = opponentPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 1f); // 좌상단
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.anchoredPosition = new Vector2(20, -20);
            panelRect.sizeDelta = new Vector2(300, 150);

            Image panelBg = opponentPanel.AddComponent<Image>();
            panelBg.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

            // 타이틀
            GameObject titleObj = CreateText(opponentPanel, "Title", "상대방 상태", 24, defaultFont);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.05f, 0.8f);
            titleRect.anchorMax = new Vector2(0.95f, 0.95f);
            titleRect.sizeDelta = Vector2.zero;

            // 이름
            GameObject nameObj = CreateText(opponentPanel, "Name", "대기 중...", 20, defaultFont);
            opponentNameText = nameObj.GetComponent<Text>();
            opponentNameText.fontStyle = FontStyle.Bold;
            opponentNameText.color = new Color(0.3f, 0.7f, 1f);
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.05f, 0.65f);
            nameRect.anchorMax = new Vector2(0.95f, 0.78f);
            nameRect.sizeDelta = Vector2.zero;

            // HP 바 배경
            GameObject hpBarBg = new GameObject("HPBarBg");
            hpBarBg.transform.SetParent(opponentPanel.transform, false);
            RectTransform hpBarBgRect = hpBarBg.AddComponent<RectTransform>();
            hpBarBgRect.anchorMin = new Vector2(0.05f, 0.48f);
            hpBarBgRect.anchorMax = new Vector2(0.95f, 0.58f);
            hpBarBgRect.sizeDelta = Vector2.zero;
            Image hpBarBgImage = hpBarBg.AddComponent<Image>();
            hpBarBgImage.color = new Color(0.3f, 0.3f, 0.3f);

            // HP 바
            GameObject hpBarObj = new GameObject("HPBar");
            hpBarObj.transform.SetParent(hpBarBg.transform, false);
            RectTransform hpBarRect = hpBarObj.AddComponent<RectTransform>();
            hpBarRect.anchorMin = Vector2.zero;
            hpBarRect.anchorMax = new Vector2(1f, 1f);
            hpBarRect.sizeDelta = Vector2.zero;
            opponentHPBar = hpBarObj.AddComponent<Image>();
            opponentHPBar.color = new Color(0.2f, 0.8f, 0.2f);
            opponentHPBar.type = Image.Type.Filled;
            opponentHPBar.fillMethod = Image.FillMethod.Horizontal;
            opponentHPBar.fillAmount = 1f;

            // HP 텍스트
            GameObject hpTextObj = CreateText(hpBarBg, "HPText", "HP: 10", 16, defaultFont);
            opponentHPText = hpTextObj.GetComponent<Text>();
            opponentHPText.alignment = TextAnchor.MiddleCenter;
            RectTransform hpTextRect = hpTextObj.GetComponent<RectTransform>();
            hpTextRect.anchorMin = Vector2.zero;
            hpTextRect.anchorMax = Vector2.one;
            hpTextRect.sizeDelta = Vector2.zero;

            // 라운드
            GameObject roundObj = CreateText(opponentPanel, "Round", "라운드: 1", 18, defaultFont);
            opponentRoundText = roundObj.GetComponent<Text>();
            RectTransform roundRect = roundObj.GetComponent<RectTransform>();
            roundRect.anchorMin = new Vector2(0.05f, 0.32f);
            roundRect.anchorMax = new Vector2(0.95f, 0.42f);
            roundRect.sizeDelta = Vector2.zero;

            // 골드
            GameObject goldObj = CreateText(opponentPanel, "Gold", "골드: 0", 18, defaultFont);
            opponentGoldText = goldObj.GetComponent<Text>();
            RectTransform goldRect = goldObj.GetComponent<RectTransform>();
            goldRect.anchorMin = new Vector2(0.05f, 0.18f);
            goldRect.anchorMax = new Vector2(0.95f, 0.28f);
            goldRect.sizeDelta = Vector2.zero;

            // 처치
            GameObject killsObj = CreateText(opponentPanel, "Kills", "처치: 0", 18, defaultFont);
            opponentKillsText = killsObj.GetComponent<Text>();
            RectTransform killsRect = killsObj.GetComponent<RectTransform>();
            killsRect.anchorMin = new Vector2(0.05f, 0.05f);
            killsRect.anchorMax = new Vector2(0.95f, 0.15f);
            killsRect.sizeDelta = Vector2.zero;

            // 패배 오버레이 (처음엔 숨김)
            defeatOverlay = new GameObject("DefeatOverlay");
            defeatOverlay.transform.SetParent(opponentPanel.transform, false);
            RectTransform defeatRect = defeatOverlay.AddComponent<RectTransform>();
            defeatRect.anchorMin = Vector2.zero;
            defeatRect.anchorMax = Vector2.one;
            defeatRect.sizeDelta = Vector2.zero;
            Image defeatBg = defeatOverlay.AddComponent<Image>();
            defeatBg.color = new Color(0.8f, 0.2f, 0.2f, 0.8f);

            GameObject defeatTextObj = CreateText(defeatOverlay, "DefeatText", "패배", 28, defaultFont);
            Text defeatText = defeatTextObj.GetComponent<Text>();
            defeatText.alignment = TextAnchor.MiddleCenter;
            defeatText.fontStyle = FontStyle.Bold;
            defeatText.color = Color.white;
            RectTransform defeatTextRect = defeatTextObj.GetComponent<RectTransform>();
            defeatTextRect.anchorMin = Vector2.zero;
            defeatTextRect.anchorMax = Vector2.one;
            defeatTextRect.sizeDelta = Vector2.zero;

            defeatOverlay.SetActive(false);

            // 초기에는 숨김
            opponentPanel.SetActive(false);

            Debug.Log("[OpponentStatusUI] UI created");
        }

        private GameObject CreateText(GameObject parent, string name, string text, int fontSize, Font font)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent.transform, false);
            
            Text textComp = textObj.AddComponent<Text>();
            textComp.text = text;
            textComp.font = font;
            textComp.fontSize = fontSize;
            textComp.color = Color.white;
            textComp.alignment = TextAnchor.MiddleLeft;

            return textObj;
        }
        #endregion

        #region Public Methods
        public void UpdateOpponentState(OpponentStateData state)
        {
            if (state == null) return;

            opponentNameText.text = state.opponent_name;
            opponentHPText.text = $"HP: {state.hp}";
            opponentRoundText.text = $"라운드: {state.round}";
            opponentGoldText.text = $"골드: {state.gold}";
            opponentKillsText.text = $"처치: {state.kills}";

            // HP 바 업데이트 (최대 10으로 가정)
            float hpPercent = Mathf.Clamp01(state.hp / 10f);
            opponentHPBar.fillAmount = hpPercent;

            // HP에 따라 색상 변경
            if (hpPercent > 0.5f)
                opponentHPBar.color = new Color(0.2f, 0.8f, 0.2f); // 초록
            else if (hpPercent > 0.2f)
                opponentHPBar.color = new Color(0.9f, 0.7f, 0.2f); // 노랑
            else
                opponentHPBar.color = new Color(0.9f, 0.2f, 0.2f); // 빨강
        }

        public void ShowDefeat()
        {
            if (defeatOverlay != null)
            {
                defeatOverlay.SetActive(true);
            }
        }

        public void Hide()
        {
            if (opponentPanel != null)
            {
                opponentPanel.SetActive(false);
            }
        }
        #endregion
    }
}
