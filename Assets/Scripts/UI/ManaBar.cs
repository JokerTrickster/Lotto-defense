using UnityEngine;
using UnityEngine.UI;
using LottoDefense.Units;

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
        [SerializeField] private Color fullManaColor = new Color(1f, 0.8f, 0.2f, 1f); // Gold when full
        [SerializeField] private float heightOffset = 0.5f; // Distance above unit
        [SerializeField] private Vector2 barSize = new Vector2(0.8f, 0.1f); // Bar dimensions
        #endregion

        #region Private Fields
        private Unit ownerUnit;
        private RectTransform rectTransform;
        private bool isInitialized = false;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
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

            isInitialized = true;
            Debug.Log($"[ManaBar] Initialized for {ownerUnit.Data.GetDisplayName()}");
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
                // Try to find in children
                fillImage = GetComponentInChildren<Image>();

                // Create if not found
                if (fillImage == null)
                {
                    GameObject fillObj = new GameObject("Fill");
                    fillObj.transform.SetParent(transform);
                    fillImage = fillObj.AddComponent<Image>();
                    fillImage.color = manaColor;
                    fillImage.type = Image.Type.Filled;
                    fillImage.fillMethod = Image.FillMethod.Horizontal;
                    fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
                }
            }

            // Configure fill image
            fillImage.color = manaColor;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImage.fillAmount = 0f;

            // Setup rect transform
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(barSize.x * 100f, barSize.y * 100f);
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handle mana change events from the unit.
        /// </summary>
        private void HandleManaChanged(float currentMana, float maxMana)
        {
            UpdateBar(currentMana, maxMana);
        }
        #endregion

        #region Bar Update
        /// <summary>
        /// Update the fill amount and color of the mana bar.
        /// </summary>
        private void UpdateBar(float currentMana, float maxMana)
        {
            if (fillImage == null) return;

            float fillAmount = maxMana > 0f ? currentMana / maxMana : 0f;
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
        }

        /// <summary>
        /// Update position to stay above the unit.
        /// </summary>
        private void UpdatePosition()
        {
            if (ownerUnit == null || rectTransform == null) return;

            // Convert unit world position to screen position
            Vector3 worldPos = ownerUnit.transform.position + Vector3.up * heightOffset;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

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
        }
        #endregion
    }
}
