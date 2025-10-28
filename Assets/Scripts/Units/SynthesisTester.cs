using UnityEngine;
using System.Collections.Generic;
using LottoDefense.Gameplay;

namespace LottoDefense.Units
{
    /// <summary>
    /// Test script for synthesis system functionality.
    /// Provides runtime testing and validation for synthesis recipes and operations.
    /// </summary>
    public class SynthesisTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [Tooltip("Enable automatic testing on start")]
        [SerializeField] private bool runTestsOnStart = false;

        [Tooltip("Test recipes to validate")]
        [SerializeField] private SynthesisRecipe[] testRecipes;

        [Header("Manual Test Controls")]
        [Tooltip("Recipe to test manually")]
        [SerializeField] private SynthesisRecipe manualTestRecipe;

        private void Start()
        {
            if (runTestsOnStart)
            {
                RunAllTests();
            }
        }

        #region Test Execution
        /// <summary>
        /// Run all automated tests.
        /// </summary>
        [ContextMenu("Run All Tests")]
        public void RunAllTests()
        {
            Debug.Log("=== Synthesis System Tests ===");

            TestManagerInitialization();
            TestRecipeValidation();
            TestRecipeDiscovery();
            TestSynthesisExecution();

            Debug.Log("=== Tests Complete ===");
        }

        /// <summary>
        /// Test synthesis manager initialization.
        /// </summary>
        [ContextMenu("Test Manager Initialization")]
        public void TestManagerInitialization()
        {
            Debug.Log("\n--- Test: Manager Initialization ---");

            if (SynthesisManager.Instance == null)
            {
                Debug.LogError("❌ SynthesisManager instance is null!");
                return;
            }

            Debug.Log($"✅ SynthesisManager initialized");
            Debug.Log($"   Recipes loaded: {SynthesisManager.Instance.AllRecipes.Length}");
            Debug.Log($"   {SynthesisManager.Instance.GetSystemSummary()}");
        }

        /// <summary>
        /// Test recipe validation logic.
        /// </summary>
        [ContextMenu("Test Recipe Validation")]
        public void TestRecipeValidation()
        {
            Debug.Log("\n--- Test: Recipe Validation ---");

            if (testRecipes == null || testRecipes.Length == 0)
            {
                Debug.LogWarning("⚠️ No test recipes assigned");
                return;
            }

            foreach (var recipe in testRecipes)
            {
                if (recipe == null) continue;

                Debug.Log($"\nTesting recipe: {recipe.RecipeName}");

                // Test valid combination
                var validUnits = GetUnitsForRecipe(recipe);
                bool validResult = recipe.ValidateIngredients(validUnits);
                Debug.Log($"  Valid combination: {(validResult ? "✅ PASS" : "❌ FAIL")}");

                // Test invalid combination (empty list)
                bool invalidResult = recipe.ValidateIngredients(new List<UnitData>());
                Debug.Log($"  Empty list rejection: {(!invalidResult ? "✅ PASS" : "❌ FAIL")}");

                // Test ingredient count
                int expectedCount = recipe.GetTotalIngredientCount();
                Debug.Log($"  Total ingredients: {expectedCount}");
            }
        }

        /// <summary>
        /// Test recipe discovery system.
        /// </summary>
        [ContextMenu("Test Recipe Discovery")]
        public void TestRecipeDiscovery()
        {
            Debug.Log("\n--- Test: Recipe Discovery ---");

            if (SynthesisManager.Instance == null)
            {
                Debug.LogError("❌ SynthesisManager not available");
                return;
            }

            var allRecipes = SynthesisManager.Instance.AllRecipes;
            var discovered = SynthesisManager.Instance.GetDiscoveredRecipes();
            var undiscovered = SynthesisManager.Instance.GetUndiscoveredRecipes();

            Debug.Log($"Total recipes: {allRecipes.Length}");
            Debug.Log($"Discovered: {discovered.Count}");
            Debug.Log($"Undiscovered: {undiscovered.Count}");

            // Test discovery
            if (undiscovered.Count > 0)
            {
                var testRecipe = undiscovered[0];
                Debug.Log($"\nDiscovering recipe: {testRecipe.RecipeName}");

                bool wasDiscovered = SynthesisManager.Instance.IsRecipeDiscovered(testRecipe);
                SynthesisManager.Instance.DiscoverRecipe(testRecipe);
                bool isDiscovered = SynthesisManager.Instance.IsRecipeDiscovered(testRecipe);

                Debug.Log($"  Before: {(wasDiscovered ? "discovered" : "undiscovered")}");
                Debug.Log($"  After: {(isDiscovered ? "discovered" : "undiscovered")}");
                Debug.Log($"  Discovery test: {(!wasDiscovered && isDiscovered ? "✅ PASS" : "❌ FAIL")}");
            }
        }

        /// <summary>
        /// Test synthesis execution.
        /// </summary>
        [ContextMenu("Test Synthesis Execution")]
        public void TestSynthesisExecution()
        {
            Debug.Log("\n--- Test: Synthesis Execution ---");

            if (SynthesisManager.Instance == null || UnitManager.Instance == null)
            {
                Debug.LogError("❌ Required managers not available");
                return;
            }

            if (testRecipes == null || testRecipes.Length == 0)
            {
                Debug.LogWarning("⚠️ No test recipes assigned");
                return;
            }

            var testRecipe = testRecipes[0];
            if (testRecipe == null)
            {
                Debug.LogWarning("⚠️ First test recipe is null");
                return;
            }

            Debug.Log($"Testing synthesis with recipe: {testRecipe.RecipeName}");

            // Add required units to inventory
            var requiredUnits = GetUnitsForRecipe(testRecipe);
            Debug.Log($"Adding {requiredUnits.Count} units to inventory...");

            foreach (var unit in requiredUnits)
            {
                UnitManager.Instance.AddUnit(unit);
            }

            // Check phase
            if (!SynthesisManager.Instance.CanSynthesize())
            {
                Debug.LogWarning("⚠️ Cannot synthesize - not in Preparation phase");
                Debug.Log("   Setting GameState to Preparation for test...");

                if (GameplayManager.Instance != null)
                {
                    // Force preparation phase for testing
                    GameplayManager.Instance.SetState(GameState.Preparation);
                }
            }

            // Attempt synthesis
            int inventoryBefore = UnitManager.Instance.Inventory.Count;
            bool success = SynthesisManager.Instance.TrySynthesize(requiredUnits, testRecipe);
            int inventoryAfter = UnitManager.Instance.Inventory.Count;

            Debug.Log($"Synthesis result: {(success ? "✅ SUCCESS" : "❌ FAILED")}");
            Debug.Log($"Inventory before: {inventoryBefore}");
            Debug.Log($"Inventory after: {inventoryAfter}");
            Debug.Log($"Expected change: -{requiredUnits.Count} +1 = {1 - requiredUnits.Count}");
            Debug.Log($"Actual change: {inventoryAfter - inventoryBefore}");
        }

        /// <summary>
        /// Test manual recipe execution.
        /// </summary>
        [ContextMenu("Test Manual Recipe")]
        public void TestManualRecipe()
        {
            if (manualTestRecipe == null)
            {
                Debug.LogWarning("[SynthesisTester] No manual test recipe assigned");
                return;
            }

            Debug.Log($"=== Testing Manual Recipe: {manualTestRecipe.RecipeName} ===");
            Debug.Log(manualTestRecipe.ToString());
            Debug.Log($"Ingredients: {manualTestRecipe.GetIngredientDisplayText()}");
            Debug.Log($"Result: {manualTestRecipe.GetResultDisplayText()}");
            Debug.Log($"Total units required: {manualTestRecipe.GetTotalIngredientCount()}");

            // Test validation with proper units
            var units = GetUnitsForRecipe(manualTestRecipe);
            bool isValid = manualTestRecipe.ValidateIngredients(units);
            Debug.Log($"Validation result: {(isValid ? "✅ VALID" : "❌ INVALID")}");
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Get the units required for a recipe (from ingredient definitions).
        /// </summary>
        private List<UnitData> GetUnitsForRecipe(SynthesisRecipe recipe)
        {
            var units = new List<UnitData>();

            foreach (var ingredient in recipe.Ingredients)
            {
                for (int i = 0; i < ingredient.quantity; i++)
                {
                    units.Add(ingredient.unitData);
                }
            }

            return units;
        }

        /// <summary>
        /// Clear test inventory.
        /// </summary>
        [ContextMenu("Clear Test Inventory")]
        public void ClearTestInventory()
        {
            if (UnitManager.Instance != null)
            {
                UnitManager.Instance.ClearInventory();
                Debug.Log("[SynthesisTester] Test inventory cleared");
            }
        }

        /// <summary>
        /// Reset discovery state.
        /// </summary>
        [ContextMenu("Reset Discovery State")]
        public void ResetDiscoveryState()
        {
            if (SynthesisManager.Instance != null)
            {
                SynthesisManager.Instance.ResetDiscoveryState();
                Debug.Log("[SynthesisTester] Discovery state reset");
            }
        }

        /// <summary>
        /// Add test gold for gacha.
        /// </summary>
        [ContextMenu("Add 100 Gold")]
        public void AddTestGold()
        {
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.ModifyGold(100);
                Debug.Log("[SynthesisTester] Added 100 gold");
            }
        }

        /// <summary>
        /// Draw random units for testing.
        /// </summary>
        [ContextMenu("Draw 10 Random Units")]
        public void DrawRandomUnits()
        {
            if (UnitManager.Instance == null || GameplayManager.Instance == null)
            {
                Debug.LogWarning("[SynthesisTester] Managers not available");
                return;
            }

            // Add gold
            GameplayManager.Instance.ModifyGold(100);

            // Draw units
            int drawn = 0;
            for (int i = 0; i < 10; i++)
            {
                var unit = UnitManager.Instance.DrawUnit();
                if (unit != null)
                {
                    drawn++;
                }
            }

            Debug.Log($"[SynthesisTester] Drew {drawn}/10 units");
            Debug.Log(UnitManager.Instance.GetInventorySummary());
        }
        #endregion

        #region Runtime Info Display
        private void OnGUI()
        {
            if (!Application.isPlaying)
                return;

            GUILayout.BeginArea(new Rect(10, 150, 400, 400));
            GUILayout.Label("=== Synthesis System Status ===", new GUIStyle { fontSize = 16, fontStyle = FontStyle.Bold });

            if (SynthesisManager.Instance != null)
            {
                GUILayout.Label(SynthesisManager.Instance.GetSystemSummary());
                GUILayout.Label($"Can Synthesize: {(SynthesisManager.Instance.CanSynthesize() ? "Yes" : "No")}");
            }
            else
            {
                GUILayout.Label("SynthesisManager: Not initialized");
            }

            GUILayout.Space(10);

            if (UnitManager.Instance != null)
            {
                GUILayout.Label($"Inventory: {UnitManager.Instance.Inventory.Count}/{50}");
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Run All Tests"))
            {
                RunAllTests();
            }

            if (GUILayout.Button("Clear Inventory"))
            {
                ClearTestInventory();
            }

            if (GUILayout.Button("Draw 10 Units"))
            {
                DrawRandomUnits();
            }

            GUILayout.EndArea();
        }
        #endregion
    }
}
