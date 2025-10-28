using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace LottoDefense.Units
{
    /// <summary>
    /// Defines a single ingredient requirement for a synthesis recipe.
    /// </summary>
    [System.Serializable]
    public struct RecipeIngredient
    {
        [Tooltip("Required unit data")]
        public UnitData unitData;

        [Tooltip("Number of this unit required")]
        [Min(1)]
        public int quantity;

        public RecipeIngredient(UnitData data, int qty)
        {
            unitData = data;
            quantity = Mathf.Max(1, qty);
        }
    }

    /// <summary>
    /// ScriptableObject defining a synthesis recipe.
    /// Specifies ingredients required and the resulting unit when combined.
    /// </summary>
    [CreateAssetMenu(fileName = "NewRecipe", menuName = "Lotto Defense/Synthesis Recipe", order = 2)]
    public class SynthesisRecipe : ScriptableObject
    {
        #region Inspector Fields
        [Header("Recipe Definition")]
        [Tooltip("Display name of this recipe")]
        [SerializeField] private string recipeName = "New Recipe";

        [Tooltip("Units required for this synthesis")]
        [SerializeField] private RecipeIngredient[] ingredients = new RecipeIngredient[0];

        [Tooltip("Unit produced by this recipe")]
        [SerializeField] private UnitData resultUnit;

        [Header("Discovery System")]
        [Tooltip("Whether this recipe is available from game start")]
        [SerializeField] private bool startsDiscovered = true;

        [Tooltip("Icon displayed in recipe list")]
        [SerializeField] private Sprite recipeIcon;

        [Tooltip("Description text shown in UI")]
        [TextArea(2, 4)]
        [SerializeField] private string recipeDescription = "";
        #endregion

        #region Properties
        /// <summary>
        /// Display name of this recipe.
        /// </summary>
        public string RecipeName => recipeName;

        /// <summary>
        /// Array of ingredient requirements.
        /// </summary>
        public RecipeIngredient[] Ingredients => ingredients;

        /// <summary>
        /// Result unit produced by synthesis.
        /// </summary>
        public UnitData ResultUnit => resultUnit;

        /// <summary>
        /// Whether this recipe is discovered at game start.
        /// </summary>
        public bool StartsDiscovered => startsDiscovered;

        /// <summary>
        /// Icon for UI display.
        /// </summary>
        public Sprite RecipeIcon => recipeIcon;

        /// <summary>
        /// Description text for UI.
        /// </summary>
        public string Description => recipeDescription;
        #endregion

        #region Validation
        /// <summary>
        /// Validates that the provided units satisfy this recipe's ingredient requirements.
        /// </summary>
        /// <param name="selectedUnits">Units selected for synthesis</param>
        /// <returns>True if requirements are met</returns>
        public bool ValidateIngredients(List<UnitData> selectedUnits)
        {
            if (selectedUnits == null || selectedUnits.Count == 0)
                return false;

            // Check each ingredient requirement
            foreach (var ingredient in ingredients)
            {
                if (ingredient.unitData == null)
                    continue;

                // Count how many of this unit type are in selection
                int count = selectedUnits.Count(u => u == ingredient.unitData);

                // Must have at least the required quantity
                if (count < ingredient.quantity)
                    return false;
            }

            // Verify no extra units (exact match required)
            int totalRequired = GetTotalIngredientCount();
            if (selectedUnits.Count != totalRequired)
                return false;

            return true;
        }

        /// <summary>
        /// Calculate total number of units required by this recipe.
        /// </summary>
        /// <returns>Sum of all ingredient quantities</returns>
        public int GetTotalIngredientCount()
        {
            return ingredients.Sum(i => i.quantity);
        }

        /// <summary>
        /// Check if a specific ingredient is required by this recipe.
        /// </summary>
        /// <param name="unitData">Unit to check</param>
        /// <returns>True if this unit is an ingredient</returns>
        public bool RequiresIngredient(UnitData unitData)
        {
            return ingredients.Any(i => i.unitData == unitData);
        }

        /// <summary>
        /// Get the required quantity for a specific ingredient.
        /// </summary>
        /// <param name="unitData">Unit to check</param>
        /// <returns>Required quantity, or 0 if not an ingredient</returns>
        public int GetIngredientQuantity(UnitData unitData)
        {
            var ingredient = ingredients.FirstOrDefault(i => i.unitData == unitData);
            return ingredient.quantity;
        }
        #endregion

        #region Unity Editor Validation
        private void OnValidate()
        {
            // Ensure name is not empty
            if (string.IsNullOrWhiteSpace(recipeName))
            {
                recipeName = "Unnamed Recipe";
            }

            // Validate ingredient quantities
            for (int i = 0; i < ingredients.Length; i++)
            {
                if (ingredients[i].quantity < 1)
                {
                    var ingredient = ingredients[i];
                    ingredient.quantity = 1;
                    ingredients[i] = ingredient;
                }
            }

            // Warn about missing data
            if (resultUnit == null)
            {
                Debug.LogWarning($"[SynthesisRecipe] '{recipeName}' has no result unit assigned!");
            }

            if (ingredients.Length == 0)
            {
                Debug.LogWarning($"[SynthesisRecipe] '{recipeName}' has no ingredients!");
            }
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get formatted string for debugging.
        /// </summary>
        public override string ToString()
        {
            string ingredientList = string.Join(", ", ingredients.Select(i =>
                $"{i.quantity}x {i.unitData?.unitName ?? "null"}"));

            return $"[Recipe] {recipeName}: {ingredientList} → {resultUnit?.unitName ?? "null"}";
        }

        /// <summary>
        /// Get user-friendly display text for recipe requirements.
        /// </summary>
        public string GetIngredientDisplayText()
        {
            if (ingredients.Length == 0)
                return "No ingredients";

            var parts = ingredients.Select(i =>
                $"{i.quantity}x {i.unitData?.unitName ?? "Unknown"}");

            return string.Join(" + ", parts);
        }

        /// <summary>
        /// Get formatted result text.
        /// </summary>
        public string GetResultDisplayText()
        {
            return resultUnit != null ? resultUnit.GetDisplayName() : "Unknown Result";
        }
        #endregion
    }
}
