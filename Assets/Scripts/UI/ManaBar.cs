using UnityEngine;
using UnityEngine.UI;
using LottoDefense.Units;
using LottoDefense.Gameplay;

namespace LottoDefense.UI
{
    /// <summary>
    /// Displays a mana bar above a unit that has skills.
    /// Automatically updates when mana changes.
    /// </summary>
    public class ManaBar : MonoBehaviour
    {
        #region Serialized Fields
        [Header("UI References")]
        [SerializeField] private Image fillImage;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Visual Settings")]
        [SerializeField] private Color manaColor = new Color(0.2f, 0.5f, 1f, 1f); // Blue
        [SerializeField] private Color fullManaColor = new Color(0.4f, 0.7f, 1f, 1f); // Bright blue when full
        [SerializeField] private float heightPadding = 0.05f; // Extra padding above unit sprite
        [SerializeField] private Vector2 barSize = new Vector2(0.8f, 0.1f); // Bar dimensions
        #endregion

        #region Private Fields
        private Unit ownerUnit;
        private RectTransform rectTransform;
        private Camera cachedCamera;
        private bool isInitialized = false;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            cachedCamera = Camera.main;
        }

        private void Update()
        {
            if (!isInitialized || ownerUnit == null) return;

            // Update position to follow unit
            UpdatePosition();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize the mana bar for a specific unit.
        /// </summary>
        /// <param name="unit">Unit to track</param>
        public void Initialize(Unit unit)
        {
            if (unit == null)
            {
                Debug.LogError("[ManaBar] Cannot initialize with null unit!");
                return;
            }

            ownerUnit = unit;

            // Subscribe to mana change events
            ownerUnit.OnManaChanged += HandleManaChanged;

            // Setup UI
            SetupUI();

            // Initial update
            if (ownerUnit.HasSkill)
            {
                UpdateBar(ownerUnit.CurrentMana, ownerUnit.MaxMana);
                Show();
            }
            else
            {
                Hide();
            }

            // Subscribe to game state changes to hide on game end
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnStateChanged += HandleGameStateChanged;
            }

            isInitialized = true;
            
            Debug.Log($"[ManaBar] ✅ Initialized for {ownerUnit.Data.GetDisplayName()} (size: {rectTransform?.sizeDelta}, sprite: {fillImage?.sprite != null})");
        }

        /// <summary>
        /// Setup UI components.
        /// </summary>
        private void SetupUI()
        {
            // Setup canvas group if not assigned
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            // Setup fill image if not assigned
            if (fillImage == null)
            {
                // Try to find Fill object specifically (not Background)
                Transform fillTransform = transform.Find("Fill");
                if (fillTransform != null)
                {
                    fillImage = fillTransform.GetComponent<Image>();
                }

                // Create if not found
                if (fillImage == null)
                {
                    GameObject fillObj = new GameObject("Fill");
                    fillObj.transform.SetParent(transform, false);
                    fillImage = fillObj.AddComponent<Image>();
                    fillImage.color = manaColor;
                    fillImage.type = Image.Type.Filled;
                    fillImage.fillMethod = Image.FillMethod.Horizontal;
                    fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;

                    // Setup rect transform for fill
                    RectTransform fillRect = fillObj.GetComponent<RectTransform>();
                    fillRect.anchorMin = Vector2.zero;
                    fillRect.anchorMax = Vector2.one;
                    fillRect.sizeDelta = Vector2.zero;
                    fillRect.anchoredPosition = Vector2.zero;
                }
            }

            // Configure fill image
            fillImage.color = manaColor;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImage.fillAmount = 0f;
            fillImage.enabled = true;
            
            // Ensure sprite is set (required for Filled type)
            if (fillImage.sprite == null)
            {
                // Create a white sprite as default
                Texture2D whiteTex = new Texture2D(1, 1);
                whiteTex.SetPixel(0, 0, Color.white);
                whiteTex.Apply();
                fillImage.sprite = Sprite.Create(whiteTex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
                Debug.LogWarning("[ManaBar] Created default white sprite for fillImage");
            }
            
            // Force UI update
            fillImage.SetAllDirty();

            // Setup rect transform (only if not already set by Unit.cs)
            if (rectTransform != null)
            {
                // Keep existing size if already set, otherwise use default
                if (rectTransform.sizeDelta == Vector2.zero)
                {
                    rectTransform.sizeDelta = new Vector2(barSize.x * 100f, barSize.y * 100f);
                }
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handle mana change events from the unit.
        /// </summary>
        private void HandleManaChanged(float currentMana, float maxMana)
        {
            if (fillImage == null)
            {
                Debug.LogError($"[ManaBar] fillImage is NULL for {ownerUnit?.Data?.GetDisplayName() ?? "unknown"}!");
                return;
            }
            
            UpdateBar(currentMana, maxMana);
        }

        /// <summary>
        /// Hide mana bar when game ends (Victory/Defeat).
        /// </summary>
        private void HandleGameStateChanged(GameState oldState, GameState newState)
        {
            if (newState == GameState.Victory || newState == GameState.Defeat)
            {
                Hide();
            }
        }
        #endregion

        #region Bar Update
        /// <summary>
        /// Update the fill amount and color of the mana bar.
        /// </summary>
        private void UpdateBar(float currentMana, float maxMana)
        {
            if (fillImage == null)
            {
                Debug.LogError($"[ManaBar] UpdateBar: fillImage is NULL!");
                return;
            }

            float fillAmount = maxMana > 0f ? currentMana / maxMana : 0f;
            
            // BEFORE update
            float oldFillAmount = fillImage.fillAmount;
            
            fillImage.fillAmount = fillAmount;

            // Change color when full
            if (fillAmount >= 1f)
            {
                fillImage.color = fullManaColor;
            }
            else
            {
                fillImage.color = manaColor;
            }
            
            // CRITICAL: Force UI update to ensure visual changes are reflected immediately
            fillImage.SetAllDirty();
            
            // Force canvas update (static method, no instance needed)
            Canvas.ForceUpdateCanvases();
            
            // Debug log only at important milestones (0%, 50%, 100%)
            int percentNow = Mathf.FloorToInt(fillAmount * 100f);
            int percentOld = Mathf.FloorToInt(oldFillAmount * 100f);
            
            if (percentNow == 0 && percentOld > 0) // Reset to 0 (skill used)
            {
                Debug.Log($"[ManaBar] 💙 {ownerUnit?.Data?.GetDisplayName()}: RESET {percentOld}%→0% (skill used!)");
            }
            else if (percentNow >= 50 && percentOld < 50) // 50% milestone
            {
                Debug.Log($"[ManaBar] 💙 {ownerUnit?.Data?.GetDisplayName()}: 50% reached ({currentMana:F1}/{maxMana:F0})");
            }
            else if (percentNow >= 100 && percentOld < 100) // 100% milestone
            {
                Debug.Log($"[ManaBar] 💙 {ownerUnit?.Data?.GetDisplayName()}: 100% FULL! ({currentMana:F1}/{maxMana:F0})");
            }
        }

        /// <summary>
        /// Update position to stay above the unit.
        /// </summary>
        private void UpdatePosition()
        {
            if (ownerUnit == null || rectTransform == null) return;

            // Position mana bar just above the unit sprite
            float unitHalfHeight = ownerUnit.transform.localScale.y * 0.5f;
            Vector3 worldPos = ownerUnit.transform.position + Vector3.up * (unitHalfHeight + heightPadding);
            if (cachedCamera == null) cachedCamera = Camera.main;
            if (cachedCamera == null)
            {
                Debug.LogError("[ManaBar] Camera.main is NULL!");
                return;
            }
            
            Vector3 screenPos = cachedCamera.WorldToScreenPoint(worldPos);

            // Update rect transform position
            rectTransform.position = screenPos;
        }
        #endregion

        #region Visibility
        /// <summary>
        /// Show the mana bar.
        /// </summary>
        public void Show()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Hide the mana bar.
        /// </summary>
        public void Hide()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
            gameObject.SetActive(false);
        }
        #endregion

        #region Cleanup
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (ownerUnit != null)
            {
                ownerUnit.OnManaChanged -= HandleManaChanged;
            }
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnStateChanged -= HandleGameStateChanged;
            }
        }
        #endregion
    }
}
