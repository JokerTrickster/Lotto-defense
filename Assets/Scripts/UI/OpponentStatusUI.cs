using UnityEngine;
using UnityEngine.UI;
using LottoDefense.Networking;

namespace LottoDefense.UI
{
    /// <summary>
    /// Displays opponent's status during multiplayer gameplay.
    /// Subscribes to MultiplayerManager events to show opponent life, round, and alive status.
    /// </summary>
    public class OpponentStatusUI : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private GameObject statusPanel;
        [SerializeField] private Text opponentNameText;
        [SerializeField] private Text opponentLifeText;
        [SerializeField] private Text opponentRoundText;
        [SerializeField] private Text opponentStatusText;
        [SerializeField] private CanvasGroup canvasGroup;
        #endregion

        #region Private Fields
        private bool opponentIsDead;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            if (statusPanel != null)
                statusPanel.SetActive(true);

            UpdateDisplay(null);
        }

        private void OnEnable()
        {
            if (MultiplayerManager.Instance != null)
            {
                MultiplayerManager.Instance.OnOpponentStateUpdated += HandleOpponentState;
                MultiplayerManager.Instance.OnOpponentDead += HandleOpponentDead;
                MultiplayerManager.Instance.OnMatchResult += HandleMatchResult;
            }
        }

        private void OnDisable()
        {
            if (MultiplayerManager.Instance != null)
            {
                MultiplayerManager.Instance.OnOpponentStateUpdated -= HandleOpponentState;
                MultiplayerManager.Instance.OnOpponentDead -= HandleOpponentDead;
                MultiplayerManager.Instance.OnMatchResult -= HandleMatchResult;
            }
        }
        #endregion

        #region Event Handlers
        private void HandleOpponentState(OpponentStatePayload state)
        {
            UpdateDisplay(state);
        }

        private void HandleOpponentDead(string playerName, int finalRound)
        {
            opponentIsDead = true;

            if (opponentNameText != null)
                opponentNameText.text = playerName;

            if (opponentStatusText != null)
            {
                opponentStatusText.text = $"탈락! (R{finalRound})";
                opponentStatusText.color = new Color(1f, 0.3f, 0.3f);
            }

            if (opponentLifeText != null)
            {
                opponentLifeText.text = "0";
                opponentLifeText.color = new Color(0.5f, 0.5f, 0.5f);
            }
        }

        private void HandleMatchResult(MatchResultPayload result)
        {
            if (opponentStatusText != null)
            {
                opponentStatusText.text = result.isWinner ? "패배" : "승리";
                opponentStatusText.color = result.isWinner
                    ? new Color(0.3f, 1f, 0.3f)
                    : new Color(1f, 0.3f, 0.3f);
            }
        }
        #endregion

        #region Display
        private void UpdateDisplay(OpponentStatePayload state)
        {
            if (state == null)
            {
                if (opponentNameText != null) opponentNameText.text = "상대";
                if (opponentLifeText != null) opponentLifeText.text = "--";
                if (opponentRoundText != null) opponentRoundText.text = "R--";
                if (opponentStatusText != null)
                {
                    opponentStatusText.text = "대기중";
                    opponentStatusText.color = new Color(0.7f, 0.7f, 0.7f);
                }
                return;
            }

            if (opponentNameText != null)
                opponentNameText.text = state.playerName;

            if (opponentLifeText != null)
            {
                opponentLifeText.text = state.life.ToString();
                opponentLifeText.color = state.life > 5
                    ? new Color(0.3f, 1f, 0.3f)
                    : new Color(1f, 0.4f, 0.3f);
            }

            if (opponentRoundText != null)
                opponentRoundText.text = $"R{state.round}";

            if (!opponentIsDead && opponentStatusText != null)
            {
                opponentStatusText.text = state.isAlive ? "생존" : "탈락";
                opponentStatusText.color = state.isAlive
                    ? new Color(0.3f, 1f, 0.3f)
                    : new Color(1f, 0.3f, 0.3f);
            }
        }
        #endregion

        #region Static Factory
        /// <summary>
        /// Create the opponent status UI dynamically on the game canvas.
        /// </summary>
        public static OpponentStatusUI CreateOnCanvas(Canvas canvas, Font font)
        {
            if (canvas == null) return null;

            // Small status bar at top-right below HUD
            GameObject root = new GameObject("OpponentStatusUI");
            root.transform.SetParent(canvas.transform, false);

            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0f, 1f);
            rootRect.anchorMax = new Vector2(0.55f, 1f);
            rootRect.pivot = new Vector2(0f, 1f);
            rootRect.anchoredPosition = new Vector2(8, -140);
            rootRect.sizeDelta = new Vector2(0, 40);

            Image bg = root.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.15f, 0.85f);
            bg.raycastTarget = false;

            Outline outline = root.AddComponent<Outline>();
            outline.effectColor = new Color(0.8f, 0.3f, 0.3f, 0.5f);
            outline.effectDistance = new Vector2(1, -1);

            HorizontalLayoutGroup hlg = root.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(8, 8, 4, 4);
            hlg.spacing = 10;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.childAlignment = TextAnchor.MiddleLeft;

            // Label
            Text labelText = CreateText(root.transform, "Label", "VS", 16,
                new Color(1f, 0.5f, 0.5f), font, 30);
            labelText.fontStyle = FontStyle.Bold;

            // Opponent name
            Text nameText = CreateText(root.transform, "OpponentName", "상대", 16,
                Color.white, font, 60);

            // Opponent life
            Text lifeText = CreateText(root.transform, "OpponentLife", "--", 16,
                new Color(1f, 0.4f, 0.4f), font, 30);
            lifeText.fontStyle = FontStyle.Bold;

            // Opponent round
            Text roundText = CreateText(root.transform, "OpponentRound", "R--", 16,
                new Color(1f, 0.9f, 0.4f), font, 35);

            // Status
            Text statusText = CreateText(root.transform, "OpponentStatus", "대기중", 14,
                new Color(0.7f, 0.7f, 0.7f), font, 50);

            CanvasGroup cg = root.AddComponent<CanvasGroup>();

            OpponentStatusUI component = root.AddComponent<OpponentStatusUI>();
            SetField(component, "statusPanel", root);
            SetField(component, "opponentNameText", nameText);
            SetField(component, "opponentLifeText", lifeText);
            SetField(component, "opponentRoundText", roundText);
            SetField(component, "opponentStatusText", statusText);
            SetField(component, "canvasGroup", cg);

            return component;
        }

        private static Text CreateText(Transform parent, string name, string text, int fontSize,
            Color color, Font font, float width)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();

            LayoutElement le = obj.AddComponent<LayoutElement>();
            le.preferredWidth = width;

            Text t = obj.AddComponent<Text>();
            t.font = font;
            t.text = text;
            t.fontSize = fontSize;
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.raycastTarget = false;
            return t;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(target, value);
        }
        #endregion
    }
}
