using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using LottoDefense.Units;

namespace LottoDefense.UI
{
    /// <summary>
    /// Manages the inventory UI display showing available units for placement.
    /// Provides click-to-select functionality to trigger unit placement mode.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        #region Inspector Fields
        [Header("UI References")]
        [Tooltip("Container where unit card UI elements will be instantiated")]
        [SerializeField] private Transform inventoryContainer;

        [Tooltip("Prefab for individual unit cards")]
        [SerializeField] private GameObject unitCardPrefab;

        [Header("Settings")]
        [Tooltip("Maximum number of unit cards to display")]
        [SerializeField] private int maxDisplayedUnits = 10;
        #endregion

        #region Private Fields
        private List<UnitCardUI> unitCards = new List<UnitCardUI>();
        #endregion

        #region Unity Lifecycle
        private void OnEnable()
        {
            // Subscribe to UnitManager events
            if (UnitManager.Instance != null)
            {
                UnitManager.Instance.OnInventoryChanged += HandleInventoryChanged;
                UnitManager.Instance.OnUnitDrawn += HandleUnitDrawn;
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from UnitManager events
            if (UnitManager.Instance != null)
            {
                UnitManager.Instance.OnInventoryChanged -= HandleInventoryChanged;
                UnitManager.Instance.OnUnitDrawn -= HandleUnitDrawn;
            }
        }

        private void Start()
        {
            // Initial inventory refresh
            RefreshInventoryDisplay();
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handle inventory changes from UnitManager.
        /// </summary>
        private void HandleInventoryChanged(List<UnitData> inventory, string operation, UnitData affectedUnit)
        {
            RefreshInventoryDisplay();
            Debug.Log($"[InventoryUI] Inventory updated: {operation} - {affectedUnit?.unitName ?? "null"}");
        }

        /// <summary>
        /// Handle new unit drawn from gacha.
        /// </summary>
        private void HandleUnitDrawn(UnitData unit, int remainingGold)
        {
            Debug.Log($"[InventoryUI] New unit drawn: {unit.GetDisplayName()}");
            // RefreshInventoryDisplay will be called via OnInventoryChanged
        }
        #endregion

        #region Inventory Display
        /// <summary>
        /// Refresh the entire inventory display by recreating all unit cards.
        /// </summary>
        public void RefreshInventoryDisplay()
        {
            // Clear existing cards
            ClearInventoryDisplay();

            if (UnitManager.Instance == null)
            {
                Debug.LogWarning("[InventoryUI] UnitManager not found");
                return;
            }

            // Get current inventory
            List<UnitData> inventory = UnitManager.Instance.GetInventory();

            // Create unit cards
            int displayCount = Mathf.Min(inventory.Count, maxDisplayedUnits);
            for (int i = 0; i < displayCount; i++)
            {
                CreateUnitCard(inventory[i], i);
            }

            Debug.Log($"[InventoryUI] Displayed {displayCount} units from inventory");
        }

        /// <summary>
        /// Clear all unit cards from the display.
        /// </summary>
        private void ClearInventoryDisplay()
        {
            foreach (var card in unitCards)
            {
                if (card != null && card.gameObject != null)
                {
                    Destroy(card.gameObject);
                }
            }
            unitCards.Clear();
        }

        /// <summary>
        /// Create a unit card UI element for the specified unit data.
        /// </summary>
        private void CreateUnitCard(UnitData unitData, int index)
        {
            GameObject cardObject;

            if (unitCardPrefab != null)
            {
                cardObject = Instantiate(unitCardPrefab, inventoryContainer);
            }
            else
            {
                // Create default card if no prefab
                cardObject = CreateDefaultUnitCard();
                cardObject.transform.SetParent(inventoryContainer, false);
            }

            cardObject.name = $"UnitCard_{unitData.unitName}_{index}";

            // Add UnitCardUI component
            UnitCardUI cardUI = cardObject.GetComponent<UnitCardUI>();
            if (cardUI == null)
            {
                cardUI = cardObject.AddComponent<UnitCardUI>();
            }

            // Initialize card
            cardUI.Initialize(unitData, OnUnitCardClicked);
            unitCards.Add(cardUI);
        }

        /// <summary>
        /// Create a default unit card GameObject when no prefab is provided.
        /// </summary>
        private GameObject CreateDefaultUnitCard()
        {
            GameObject card = new GameObject("UnitCard");

            // Add Image component for visual representation
            Image image = card.AddComponent<Image>();
            image.color = Color.white;

            // Add Button component for click handling
            Button button = card.AddComponent<Button>();

            // Set size
            RectTransform rt = card.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(80f, 80f);

            return card;
        }
        #endregion

        #region Click Handling
        /// <summary>
        /// Handle unit card click events to trigger placement mode.
        /// </summary>
        private void OnUnitCardClicked(UnitData unitData)
        {
            if (unitData == null)
            {
                Debug.LogWarning("[InventoryUI] Clicked unit data is null");
                return;
            }

            // Trigger placement mode via UnitPlacementManager
            if (UnitPlacementManager.Instance != null)
            {
                UnitPlacementManager.Instance.SelectUnitForPlacement(unitData);
                Debug.Log($"[InventoryUI] Selected {unitData.GetDisplayName()} for placement");
            }
            else
            {
                Debug.LogError("[InventoryUI] UnitPlacementManager not found!");
            }
        }
        #endregion

        #region Public API
        /// <summary>
        /// Get the number of unit cards currently displayed.
        /// </summary>
        public int GetDisplayedUnitCount()
        {
            return unitCards.Count;
        }
        #endregion
    }

    /// <summary>
    /// Component attached to individual unit card UI elements.
    /// Handles display and click events for a single unit.
    /// </summary>
    public class UnitCardUI : MonoBehaviour
    {
        #region Properties
        public UnitData UnitData { get; private set; }
        #endregion

        #region Private Fields
        private System.Action<UnitData> onClickCallback;
        private Button button;
        private Image iconImage;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize this unit card with data and click callback.
        /// </summary>
        public void Initialize(UnitData unitData, System.Action<UnitData> clickCallback)
        {
            UnitData = unitData;
            onClickCallback = clickCallback;

            // Setup button
            button = GetComponent<Button>();
            if (button == null)
            {
                button = gameObject.AddComponent<Button>();
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnCardClicked);

            // Setup icon if available
            iconImage = GetComponent<Image>();
            if (iconImage != null && unitData.icon != null)
            {
                iconImage.sprite = unitData.icon;
            }

            // Find child Image for icon if main Image is for background
            Image[] images = GetComponentsInChildren<Image>();
            if (images.Length > 1 && unitData.icon != null)
            {
                images[1].sprite = unitData.icon;
            }
        }
        #endregion

        #region Click Handling
        /// <summary>
        /// Handle card click events.
        /// </summary>
        private void OnCardClicked()
        {
            onClickCallback?.Invoke(UnitData);
        }
        #endregion
    }
}
