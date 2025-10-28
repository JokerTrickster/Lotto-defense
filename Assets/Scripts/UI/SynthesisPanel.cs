using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;
using LottoDefense.Units;

namespace LottoDefense.UI
{
    /// <summary>
    /// Manages the synthesis UI panel for combining units into higher-tier units.
    /// Displays available recipes, allows unit selection, and executes synthesis.
    /// </summary>
    public class SynthesisPanel : MonoBehaviour
    {
        #region Inspector Fields
        [Header("UI References")]
        [Tooltip("Root GameObject of the synthesis panel")]
        [SerializeField] private GameObject panelRoot;

        [Tooltip("Container for recipe list items")]
        [SerializeField] private Transform recipeListContainer;

        [Tooltip("Prefab for recipe list items")]
        [SerializeField] private RecipeSlotUI recipeSlotPrefab;

        [Tooltip("Container for selected unit icons")]
        [SerializeField] private Transform selectedUnitsContainer;

        [Tooltip("Prefab for selected unit slots")]
        [SerializeField] private SelectedUnitSlotUI selectedUnitSlotPrefab;

        [Tooltip("Button to execute synthesis")]
        [SerializeField] private Button synthesizeButton;

        [Tooltip("Status text showing validation messages")]
        [SerializeField] private TextMeshProUGUI statusText;

        [Tooltip("Close button")]
        [SerializeField] private Button closeButton;

        [Header("Visual Feedback")]
        [Tooltip("Color for valid recipe indicators")]
        [SerializeField] private Color validRecipeColor = Color.green;

        [Tooltip("Color for invalid recipe indicators")]
        [SerializeField] private Color invalidRecipeColor = Color.red;

        [Tooltip("Particle effect for successful synthesis")]
        [SerializeField] private ParticleSystem successParticles;
        #endregion

        #region Private Fields
        private List<UnitData> selectedUnits = new List<UnitData>();
        private SynthesisRecipe currentRecipe = null;
        private List<RecipeSlotUI> recipeSlots = new List<RecipeSlotUI>();
        private List<SelectedUnitSlotUI> selectedUnitSlots = new List<SelectedUnitSlotUI>();
        #endregion

        #region Events
        /// <summary>
        /// Fired when units are selected/deselected.
        /// </summary>
        public event Action<List<UnitData>> OnUnitsSelected;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Setup button listeners
            if (synthesizeButton != null)
            {
                synthesizeButton.onClick.AddListener(TrySynthesis);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Hide);
            }
        }

        private void OnEnable()
        {
            // Subscribe to synthesis manager events
            if (SynthesisManager.Instance != null)
            {
                SynthesisManager.Instance.OnSynthesisComplete += HandleSynthesisComplete;
                SynthesisManager.Instance.OnSynthesisFailed += HandleSynthesisFailed;
                SynthesisManager.Instance.OnRecipeDiscovered += HandleRecipeDiscovered;
            }

            // Subscribe to unit manager events
            if (UnitManager.Instance != null)
            {
                UnitManager.Instance.OnInventoryChanged += HandleInventoryChanged;
            }

            // Initial setup
            RefreshPanel();
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            if (SynthesisManager.Instance != null)
            {
                SynthesisManager.Instance.OnSynthesisComplete -= HandleSynthesisComplete;
                SynthesisManager.Instance.OnSynthesisFailed -= HandleSynthesisFailed;
                SynthesisManager.Instance.OnRecipeDiscovered -= HandleRecipeDiscovered;
            }

            if (UnitManager.Instance != null)
            {
                UnitManager.Instance.OnInventoryChanged -= HandleInventoryChanged;
            }
        }
        #endregion

        #region Panel Control
        /// <summary>
        /// Show the synthesis panel.
        /// </summary>
        public void Show()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
                RefreshPanel();
                Debug.Log("[SynthesisPanel] Panel opened");
            }
        }

        /// <summary>
        /// Hide the synthesis panel.
        /// </summary>
        public void Hide()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
                ClearSelection();
                Debug.Log("[SynthesisPanel] Panel closed");
            }
        }

        /// <summary>
        /// Refresh the entire panel display.
        /// </summary>
        private void RefreshPanel()
        {
            UpdateRecipeList();
            UpdateSelectedUnitsDisplay();
            UpdateSynthesizeButton();
        }
        #endregion

        #region Recipe List
        /// <summary>
        /// Update the recipe list display.
        /// </summary>
        private void UpdateRecipeList()
        {
            // Clear existing slots
            foreach (var slot in recipeSlots)
            {
                if (slot != null)
                    Destroy(slot.gameObject);
            }
            recipeSlots.Clear();

            if (SynthesisManager.Instance == null || recipeListContainer == null || recipeSlotPrefab == null)
                return;

            // Get discovered recipes
            var discoveredRecipes = SynthesisManager.Instance.GetDiscoveredRecipes();

            // Create recipe slots
            foreach (var recipe in discoveredRecipes)
            {
                RecipeSlotUI slot = Instantiate(recipeSlotPrefab, recipeListContainer);
                slot.Initialize(recipe, true);
                slot.OnRecipeClicked += HandleRecipeClicked;
                recipeSlots.Add(slot);
            }

            Debug.Log($"[SynthesisPanel] Displayed {recipeSlots.Count} recipes");
        }

        /// <summary>
        /// Handle recipe slot click.
        /// </summary>
        private void HandleRecipeClicked(SynthesisRecipe recipe)
        {
            // Auto-select units for this recipe if available
            AutoSelectUnitsForRecipe(recipe);
        }

        /// <summary>
        /// Attempt to auto-select units that match the recipe requirements.
        /// </summary>
        private void AutoSelectUnitsForRecipe(SynthesisRecipe recipe)
        {
            if (recipe == null || UnitManager.Instance == null)
                return;

            ClearSelection();

            var inventory = UnitManager.Instance.GetInventory();
            var neededUnits = new List<UnitData>();

            // Try to find units for each ingredient
            foreach (var ingredient in recipe.Ingredients)
            {
                int needed = ingredient.quantity;
                int found = 0;

                foreach (var unit in inventory)
                {
                    if (unit == ingredient.unitData && !neededUnits.Contains(unit))
                    {
                        neededUnits.Add(unit);
                        found++;

                        if (found >= needed)
                            break;
                    }
                }
            }

            // Add selected units
            foreach (var unit in neededUnits)
            {
                AddSelectedUnit(unit);
            }

            ValidateCurrentSelection();
        }
        #endregion

        #region Unit Selection
        /// <summary>
        /// Add a unit to the synthesis selection.
        /// </summary>
        public void AddSelectedUnit(UnitData unit)
        {
            if (unit == null)
                return;

            selectedUnits.Add(unit);
            UpdateSelectedUnitsDisplay();
            ValidateCurrentSelection();

            OnUnitsSelected?.Invoke(selectedUnits);
            Debug.Log($"[SynthesisPanel] Added {unit.unitName} to selection ({selectedUnits.Count} total)");
        }

        /// <summary>
        /// Remove a unit from the synthesis selection.
        /// </summary>
        public void RemoveSelectedUnit(UnitData unit)
        {
            if (unit == null)
                return;

            selectedUnits.Remove(unit);
            UpdateSelectedUnitsDisplay();
            ValidateCurrentSelection();

            OnUnitsSelected?.Invoke(selectedUnits);
            Debug.Log($"[SynthesisPanel] Removed {unit.unitName} from selection ({selectedUnits.Count} total)");
        }

        /// <summary>
        /// Clear all selected units.
        /// </summary>
        public void ClearSelection()
        {
            selectedUnits.Clear();
            currentRecipe = null;
            UpdateSelectedUnitsDisplay();
            UpdateSynthesizeButton();

            OnUnitsSelected?.Invoke(selectedUnits);
            Debug.Log("[SynthesisPanel] Selection cleared");
        }

        /// <summary>
        /// Update the selected units display.
        /// </summary>
        private void UpdateSelectedUnitsDisplay()
        {
            // Clear existing slots
            foreach (var slot in selectedUnitSlots)
            {
                if (slot != null)
                    Destroy(slot.gameObject);
            }
            selectedUnitSlots.Clear();

            if (selectedUnitsContainer == null || selectedUnitSlotPrefab == null)
                return;

            // Create slots for selected units
            foreach (var unit in selectedUnits)
            {
                SelectedUnitSlotUI slot = Instantiate(selectedUnitSlotPrefab, selectedUnitsContainer);
                slot.Initialize(unit, RemoveSelectedUnit);
                selectedUnitSlots.Add(slot);
            }
        }
        #endregion

        #region Validation
        /// <summary>
        /// Validate the current unit selection against available recipes.
        /// </summary>
        private void ValidateCurrentSelection()
        {
            if (SynthesisManager.Instance == null)
                return;

            // Find valid recipes for current selection
            var validRecipes = SynthesisManager.Instance.GetValidRecipes(selectedUnits);

            if (validRecipes.Count > 0)
            {
                currentRecipe = validRecipes[0];
                ShowValidFeedback();
            }
            else
            {
                currentRecipe = null;
                ShowInvalidFeedback();
            }

            UpdateSynthesizeButton();
        }

        /// <summary>
        /// Update the synthesize button state based on validation.
        /// </summary>
        private void UpdateSynthesizeButton()
        {
            if (synthesizeButton == null)
                return;

            bool canSynthesize = currentRecipe != null &&
                                SynthesisManager.Instance != null &&
                                SynthesisManager.Instance.CanSynthesize();

            synthesizeButton.interactable = canSynthesize;
        }

        /// <summary>
        /// Show visual feedback for valid recipe.
        /// </summary>
        private void ShowValidFeedback()
        {
            if (statusText != null && currentRecipe != null)
            {
                statusText.text = $"Ready to synthesize: {currentRecipe.GetResultDisplayText()}";
                statusText.color = validRecipeColor;
            }
        }

        /// <summary>
        /// Show visual feedback for invalid recipe.
        /// </summary>
        private void ShowInvalidFeedback()
        {
            if (statusText != null)
            {
                if (selectedUnits.Count == 0)
                {
                    statusText.text = "Select units to synthesize";
                    statusText.color = Color.white;
                }
                else
                {
                    statusText.text = "Invalid combination - no matching recipe";
                    statusText.color = invalidRecipeColor;
                }
            }
        }
        #endregion

        #region Synthesis Execution
        /// <summary>
        /// Attempt to execute synthesis with current selection.
        /// </summary>
        public void TrySynthesis()
        {
            if (currentRecipe == null)
            {
                ShowErrorFeedback("No valid recipe selected");
                return;
            }

            if (SynthesisManager.Instance == null)
            {
                ShowErrorFeedback("Synthesis system not available");
                return;
            }

            // Execute synthesis
            bool success = SynthesisManager.Instance.TrySynthesize(
                selectedUnits,
                currentRecipe,
                transform.position
            );

            if (success)
            {
                // Success handling is done in HandleSynthesisComplete event
                Debug.Log("[SynthesisPanel] Synthesis initiated");
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handle successful synthesis.
        /// </summary>
        private void HandleSynthesisComplete(UnitData resultUnit, Vector3 position)
        {
            ShowSuccessFeedback(resultUnit);
            ClearSelection();
            RefreshPanel();
        }

        /// <summary>
        /// Handle synthesis failure.
        /// </summary>
        private void HandleSynthesisFailed(string reason)
        {
            ShowErrorFeedback(reason);
        }

        /// <summary>
        /// Handle recipe discovery.
        /// </summary>
        private void HandleRecipeDiscovered(SynthesisRecipe recipe)
        {
            UpdateRecipeList();
            Debug.Log($"[SynthesisPanel] New recipe displayed: {recipe.RecipeName}");
        }

        /// <summary>
        /// Handle inventory changes.
        /// </summary>
        private void HandleInventoryChanged(List<UnitData> inventory, string operation, UnitData affectedUnit)
        {
            // Refresh in case selected units are no longer available
            ValidateCurrentSelection();
        }
        #endregion

        #region Visual Feedback
        /// <summary>
        /// Show success feedback with result unit info.
        /// </summary>
        private void ShowSuccessFeedback(UnitData resultUnit)
        {
            if (statusText != null)
            {
                statusText.text = $"Success! Created {resultUnit.GetDisplayName()}";
                statusText.color = validRecipeColor;
            }

            if (successParticles != null)
            {
                successParticles.Play();
            }

            Debug.Log($"[SynthesisPanel] Success feedback: {resultUnit.GetDisplayName()}");
        }

        /// <summary>
        /// Show error feedback with reason.
        /// </summary>
        private void ShowErrorFeedback(string message)
        {
            if (statusText != null)
            {
                statusText.text = $"Error: {message}";
                statusText.color = invalidRecipeColor;
            }

            Debug.LogWarning($"[SynthesisPanel] Error feedback: {message}");
        }
        #endregion
    }

    #region Helper UI Components
    /// <summary>
    /// UI component for individual recipe list items.
    /// </summary>
    public class RecipeSlotUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image recipeIcon;
        [SerializeField] private TextMeshProUGUI recipeNameText;
        [SerializeField] private TextMeshProUGUI ingredientsText;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private GameObject discoveredIndicator;
        [SerializeField] private GameObject newIndicator;
        [SerializeField] private Button selectButton;

        private SynthesisRecipe recipe;
        public event Action<SynthesisRecipe> OnRecipeClicked;

        private void Awake()
        {
            if (selectButton != null)
            {
                selectButton.onClick.AddListener(() => OnRecipeClicked?.Invoke(recipe));
            }
        }

        public void Initialize(SynthesisRecipe recipeData, bool isDiscovered)
        {
            recipe = recipeData;

            if (recipeIcon != null && recipeData.RecipeIcon != null)
            {
                recipeIcon.sprite = recipeData.RecipeIcon;
            }

            if (recipeNameText != null)
            {
                recipeNameText.text = recipeData.RecipeName;
            }

            if (ingredientsText != null)
            {
                ingredientsText.text = recipeData.GetIngredientDisplayText();
            }

            if (resultText != null)
            {
                resultText.text = recipeData.GetResultDisplayText();
            }

            if (discoveredIndicator != null)
            {
                discoveredIndicator.SetActive(isDiscovered);
            }

            if (newIndicator != null)
            {
                newIndicator.SetActive(!isDiscovered);
            }
        }

        public void SetHighlight(bool highlighted)
        {
            // Optional: Add visual highlight effect
        }
    }

    /// <summary>
    /// UI component for selected unit slots.
    /// </summary>
    public class SelectedUnitSlotUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image unitIcon;
        [SerializeField] private TextMeshProUGUI unitNameText;
        [SerializeField] private Button removeButton;

        private UnitData unit;

        public void Initialize(UnitData unitData, System.Action<UnitData> onRemove)
        {
            unit = unitData;

            if (unitIcon != null && unitData.icon != null)
            {
                unitIcon.sprite = unitData.icon;
            }

            if (unitNameText != null)
            {
                unitNameText.text = unitData.unitName;
            }

            if (removeButton != null)
            {
                removeButton.onClick.RemoveAllListeners();
                removeButton.onClick.AddListener(() => onRemove?.Invoke(unit));
            }
        }
    }
    #endregion
}
