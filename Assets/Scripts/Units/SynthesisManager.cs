using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using LottoDefense.Grid;
using LottoDefense.Gameplay;

namespace LottoDefense.Units
{
    /// <summary>
    /// Singleton manager responsible for unit synthesis system.
    /// Handles recipe validation, unit transformation, and discovery state management.
    /// </summary>
    public class SynthesisManager : MonoBehaviour
    {
        #region Singleton
        private static SynthesisManager _instance;

        /// <summary>
        /// Global access point for the SynthesisManager singleton.
        /// </summary>
        public static SynthesisManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<SynthesisManager>();

                    if (_instance == null)
                    {
                        GameObject go = new GameObject("SynthesisManager");
                        _instance = go.AddComponent<SynthesisManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Inspector Fields
        [Header("Recipe Configuration")]
        [Tooltip("All available synthesis recipes")]
        [SerializeField] private SynthesisRecipe[] allRecipes = new SynthesisRecipe[0];

        [Header("Visual Effects")]
        [Tooltip("Particle effect spawned at synthesis location")]
        [SerializeField] private GameObject synthesisEffectPrefab;

        [Tooltip("Duration of synthesis effect in seconds")]
        [SerializeField] private float effectDuration = 1.5f;
        #endregion

        #region Properties
        /// <summary>
        /// Set of discovered recipe names.
        /// </summary>
        public HashSet<string> DiscoveredRecipes { get; private set; } = new HashSet<string>();

        /// <summary>
        /// All configured recipes.
        /// </summary>
        public SynthesisRecipe[] AllRecipes => allRecipes;
        #endregion

        #region Events
        /// <summary>
        /// Fired when a new recipe is discovered.
        /// Parameters: recipe
        /// </summary>
        public event Action<SynthesisRecipe> OnRecipeDiscovered;

        /// <summary>
        /// Fired when synthesis completes successfully.
        /// Parameters: resultUnit, synthesisPosition
        /// </summary>
        public event Action<UnitData, Vector3> OnSynthesisComplete;

        /// <summary>
        /// Fired when synthesis fails.
        /// Parameters: failureReason
        /// </summary>
        public event Action<string> OnSynthesisFailed;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize synthesis manager and load discovery state.
        /// </summary>
        private void Initialize()
        {
            LoadRecipes();
            InitializeDiscoveredRecipes();

            Debug.Log($"[SynthesisManager] Initialized with {allRecipes.Length} recipes, {DiscoveredRecipes.Count} discovered");
        }

        /// <summary>
        /// Load recipes from Resources if not assigned in inspector.
        /// </summary>
        private void LoadRecipes()
        {
            if (allRecipes.Length == 0)
            {
                allRecipes = Resources.LoadAll<SynthesisRecipe>("Recipes");
                Debug.Log($"[SynthesisManager] Loaded {allRecipes.Length} recipes from Resources/Recipes");
            }
        }

        /// <summary>
        /// Initialize discovered recipes based on StartsDiscovered flag.
        /// </summary>
        private void InitializeDiscoveredRecipes()
        {
            DiscoveredRecipes.Clear();

            foreach (var recipe in allRecipes)
            {
                if (recipe != null && recipe.StartsDiscovered)
                {
                    DiscoveredRecipes.Add(recipe.RecipeName);
                }
            }
        }
        #endregion

        #region Synthesis Operations
        /// <summary>
        /// Attempt to synthesize units using the specified recipe.
        /// </summary>
        /// <param name="selectedUnits">Units to consume (from inventory)</param>
        /// <param name="recipe">Recipe to execute</param>
        /// <param name="synthesisPosition">World position for synthesis effect (optional)</param>
        /// <returns>True if synthesis succeeded</returns>
        public bool TrySynthesize(List<UnitData> selectedUnits, SynthesisRecipe recipe, Vector3? synthesisPosition = null)
        {
            // Validate phase
            if (!CanSynthesize())
            {
                string reason = "Can only synthesize during Preparation phase";
                Debug.LogWarning($"[SynthesisManager] {reason}");
                OnSynthesisFailed?.Invoke(reason);
                return false;
            }

            // Validate recipe
            if (recipe == null)
            {
                string reason = "Recipe is null";
                Debug.LogWarning($"[SynthesisManager] {reason}");
                OnSynthesisFailed?.Invoke(reason);
                return false;
            }

            // Validate ingredients
            if (!recipe.ValidateIngredients(selectedUnits))
            {
                string reason = "Ingredient requirements not met";
                Debug.LogWarning($"[SynthesisManager] {reason} for recipe '{recipe.RecipeName}'");
                OnSynthesisFailed?.Invoke(reason);
                return false;
            }

            // Perform synthesis
            Vector3 effectPos = synthesisPosition ?? Vector3.zero;
            PerformSynthesis(selectedUnits, recipe, effectPos);

            return true;
        }

        /// <summary>
        /// Execute the synthesis transformation.
        /// </summary>
        private void PerformSynthesis(List<UnitData> ingredients, SynthesisRecipe recipe, Vector3 synthesisPosition)
        {
            long startTime = System.Diagnostics.Stopwatch.GetTimestamp();

            // Remove ingredient units from inventory
            RemoveIngredientUnits(ingredients);

            // Add result unit to inventory
            if (UnitManager.Instance.AddUnit(recipe.ResultUnit))
            {
                // Spawn visual effect
                SpawnSynthesisEffect(synthesisPosition);

                // Discover recipe if not already discovered
                if (!DiscoveredRecipes.Contains(recipe.RecipeName))
                {
                    DiscoverRecipe(recipe);
                }

                // Notify listeners
                OnSynthesisComplete?.Invoke(recipe.ResultUnit, synthesisPosition);

                long endTime = System.Diagnostics.Stopwatch.GetTimestamp();
                float elapsedMs = (endTime - startTime) * 1000f / System.Diagnostics.Stopwatch.Frequency;

                Debug.Log($"[SynthesisManager] Synthesis complete: {recipe.RecipeName} → {recipe.ResultUnit.GetDisplayName()} ({elapsedMs:F2}ms)");
            }
            else
            {
                Debug.LogError("[SynthesisManager] Failed to add result unit to inventory!");
                OnSynthesisFailed?.Invoke("Failed to add result to inventory");
            }
        }

        /// <summary>
        /// Remove ingredient units from inventory.
        /// </summary>
        private void RemoveIngredientUnits(List<UnitData> units)
        {
            foreach (var unit in units)
            {
                UnitManager.Instance.RemoveUnit(unit);
            }

            Debug.Log($"[SynthesisManager] Removed {units.Count} ingredient units from inventory");
        }

        /// <summary>
        /// Spawn visual effect at synthesis location.
        /// </summary>
        private void SpawnSynthesisEffect(Vector3 position)
        {
            if (synthesisEffectPrefab != null)
            {
                GameObject effect = Instantiate(synthesisEffectPrefab, position, Quaternion.identity);
                Destroy(effect, effectDuration);
            }
        }
        #endregion

        #region Recipe Discovery
        /// <summary>
        /// Mark a recipe as discovered.
        /// </summary>
        /// <param name="recipe">Recipe to discover</param>
        public void DiscoverRecipe(SynthesisRecipe recipe)
        {
            if (recipe == null)
                return;

            if (DiscoveredRecipes.Add(recipe.RecipeName))
            {
                OnRecipeDiscovered?.Invoke(recipe);
                Debug.Log($"[SynthesisManager] New recipe discovered: {recipe.RecipeName}");

                SaveDiscoveryState();
            }
        }

        /// <summary>
        /// Check if a recipe has been discovered.
        /// </summary>
        public bool IsRecipeDiscovered(SynthesisRecipe recipe)
        {
            return recipe != null && DiscoveredRecipes.Contains(recipe.RecipeName);
        }

        /// <summary>
        /// Get all discovered recipes.
        /// </summary>
        public List<SynthesisRecipe> GetDiscoveredRecipes()
        {
            return allRecipes.Where(r => r != null && DiscoveredRecipes.Contains(r.RecipeName)).ToList();
        }

        /// <summary>
        /// Get all undiscovered recipes.
        /// </summary>
        public List<SynthesisRecipe> GetUndiscoveredRecipes()
        {
            return allRecipes.Where(r => r != null && !DiscoveredRecipes.Contains(r.RecipeName)).ToList();
        }
        #endregion

        #region Recipe Queries
        /// <summary>
        /// Get all valid recipes for the provided unit selection.
        /// </summary>
        /// <param name="selectedUnits">Units to check</param>
        /// <returns>List of recipes that can be executed with these units</returns>
        public List<SynthesisRecipe> GetValidRecipes(List<UnitData> selectedUnits)
        {
            if (selectedUnits == null || selectedUnits.Count == 0)
                return new List<SynthesisRecipe>();

            var validRecipes = new List<SynthesisRecipe>();

            foreach (var recipe in allRecipes)
            {
                if (recipe != null && recipe.ValidateIngredients(selectedUnits))
                {
                    validRecipes.Add(recipe);
                }
            }

            return validRecipes;
        }

        /// <summary>
        /// Find recipes that use the specified unit as an ingredient.
        /// </summary>
        public List<SynthesisRecipe> GetRecipesUsingIngredient(UnitData unit)
        {
            if (unit == null)
                return new List<SynthesisRecipe>();

            return allRecipes.Where(r => r != null && r.RequiresIngredient(unit)).ToList();
        }

        /// <summary>
        /// Check if synthesis is currently allowed (must be in Preparation phase).
        /// </summary>
        public bool CanSynthesize()
        {
            return GameplayManager.Instance != null &&
                   GameplayManager.Instance.CurrentState == GameState.Preparation;
        }
        #endregion

        #region Save/Load System
        private const string DISCOVERY_KEY = "SynthesisDiscoveryState";

        /// <summary>
        /// Save discovered recipes to PlayerPrefs.
        /// </summary>
        public void SaveDiscoveryState()
        {
            string discoveryData = string.Join(",", DiscoveredRecipes);
            PlayerPrefs.SetString(DISCOVERY_KEY, discoveryData);
            PlayerPrefs.Save();

            Debug.Log($"[SynthesisManager] Saved discovery state: {DiscoveredRecipes.Count} recipes");
        }

        /// <summary>
        /// Load discovered recipes from PlayerPrefs.
        /// </summary>
        public void LoadDiscoveryState()
        {
            if (PlayerPrefs.HasKey(DISCOVERY_KEY))
            {
                string discoveryData = PlayerPrefs.GetString(DISCOVERY_KEY);

                if (!string.IsNullOrEmpty(discoveryData))
                {
                    var recipeNames = discoveryData.Split(',');
                    DiscoveredRecipes = new HashSet<string>(recipeNames);

                    Debug.Log($"[SynthesisManager] Loaded discovery state: {DiscoveredRecipes.Count} recipes");
                }
            }
        }

        /// <summary>
        /// Reset all discovery progress.
        /// </summary>
        public void ResetDiscoveryState()
        {
            DiscoveredRecipes.Clear();
            InitializeDiscoveredRecipes();
            SaveDiscoveryState();

            Debug.Log("[SynthesisManager] Discovery state reset");
        }
        #endregion

        #region Debug Utilities
        /// <summary>
        /// Get summary of synthesis system state.
        /// </summary>
        public string GetSystemSummary()
        {
            int discovered = DiscoveredRecipes.Count;
            int total = allRecipes.Length;
            int undiscovered = total - discovered;

            return $"Synthesis Recipes: {discovered}/{total} discovered, {undiscovered} hidden";
        }

        /// <summary>
        /// Log all recipes and their discovery status.
        /// </summary>
        [ContextMenu("Log All Recipes")]
        public void LogAllRecipes()
        {
            Debug.Log("=== Synthesis Recipes ===");
            foreach (var recipe in allRecipes)
            {
                if (recipe != null)
                {
                    bool discovered = IsRecipeDiscovered(recipe);
                    Debug.Log($"  {(discovered ? "[✓]" : "[ ]")} {recipe.ToString()}");
                }
            }
        }
        #endregion
    }
}
