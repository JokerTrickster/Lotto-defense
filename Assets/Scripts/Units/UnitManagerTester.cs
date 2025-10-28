using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using LottoDefense.Gameplay;

namespace LottoDefense.Units
{
    /// <summary>
    /// Comprehensive test suite for UnitManager functionality.
    /// Tests gacha probability distribution, gold integration, and inventory management.
    /// </summary>
    public class UnitManagerTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [Tooltip("Number of gacha draws to perform for probability testing")]
        [SerializeField] private int probabilityTestCount = 10000;

        [Tooltip("Enable automatic testing on Start")]
        [SerializeField] private bool runTestsOnStart = false;

        [Header("Test Unit Data")]
        [Tooltip("Test units to populate the manager (assign 4 per rarity)")]
        [SerializeField] private List<UnitData> testNormalUnits = new List<UnitData>();
        [SerializeField] private List<UnitData> testRareUnits = new List<UnitData>();
        [SerializeField] private List<UnitData> testEpicUnits = new List<UnitData>();
        [SerializeField] private List<UnitData> testLegendaryUnits = new List<UnitData>();

        private Dictionary<Rarity, int> gachaResults = new Dictionary<Rarity, int>();

        #region Unity Lifecycle
        private void Start()
        {
            if (runTestsOnStart)
            {
                RunAllTests();
            }
        }
        #endregion

        #region Public Test Methods
        /// <summary>
        /// Execute all test suites.
        /// </summary>
        [ContextMenu("Run All Tests")]
        public void RunAllTests()
        {
            Debug.Log("========== UNIT MANAGER TEST SUITE ==========");

            TestUnitPoolValidation();
            TestGoldIntegration();
            TestInventoryOperations();
            TestGachaProbabilityDistribution();
            TestInventoryCapacity();
            TestEventNotifications();

            Debug.Log("========== ALL TESTS COMPLETE ==========");
        }

        /// <summary>
        /// Test 1: Verify unit pools are properly configured.
        /// </summary>
        [ContextMenu("Test 1: Unit Pool Validation")]
        public void TestUnitPoolValidation()
        {
            Debug.Log("\n[TEST 1] Unit Pool Validation");

            var normalPool = UnitManager.Instance.GetUnitPool(Rarity.Normal);
            var rarePool = UnitManager.Instance.GetUnitPool(Rarity.Rare);
            var epicPool = UnitManager.Instance.GetUnitPool(Rarity.Epic);
            var legendaryPool = UnitManager.Instance.GetUnitPool(Rarity.Legendary);

            Debug.Log($"Normal pool: {normalPool.Count} units");
            Debug.Log($"Rare pool: {rarePool.Count} units");
            Debug.Log($"Epic pool: {epicPool.Count} units");
            Debug.Log($"Legendary pool: {legendaryPool.Count} units");

            bool passed = normalPool.Count > 0 && rarePool.Count > 0 &&
                         epicPool.Count > 0 && legendaryPool.Count > 0;

            Debug.Log(passed ? "✅ PASSED: All rarity pools populated" : "❌ FAILED: Some pools are empty");
        }

        /// <summary>
        /// Test 2: Verify gold cost integration with GameplayManager.
        /// </summary>
        [ContextMenu("Test 2: Gold Integration")]
        public void TestGoldIntegration()
        {
            Debug.Log("\n[TEST 2] Gold Cost Integration");

            // Reset gold to known state
            GameplayManager.Instance.SetGold(100);
            int initialGold = GameplayManager.Instance.CurrentGold;

            Debug.Log($"Initial gold: {initialGold}");

            // Perform single draw
            UnitData drawn = UnitManager.Instance.DrawUnit();
            int goldAfterDraw = GameplayManager.Instance.CurrentGold;

            Debug.Log($"Gold after draw: {goldAfterDraw}");
            Debug.Log($"Gold deducted: {initialGold - goldAfterDraw}");

            bool passed = (initialGold - goldAfterDraw) == UnitManager.Instance.GetGachaCost();
            Debug.Log(passed ? "✅ PASSED: Correct gold deduction" : "❌ FAILED: Incorrect gold amount");

            // Test insufficient gold
            GameplayManager.Instance.SetGold(3);
            Debug.Log("\nTesting insufficient gold (3 gold)...");
            UnitData shouldBeNull = UnitManager.Instance.DrawUnit();

            bool insufficientGoldPassed = shouldBeNull == null;
            Debug.Log(insufficientGoldPassed ? "✅ PASSED: Draw blocked with insufficient gold" : "❌ FAILED: Draw allowed with insufficient gold");
        }

        /// <summary>
        /// Test 3: Verify inventory add/remove operations.
        /// </summary>
        [ContextMenu("Test 3: Inventory Operations")]
        public void TestInventoryOperations()
        {
            Debug.Log("\n[TEST 3] Inventory Operations");

            // Clear inventory
            UnitManager.Instance.ClearInventory();
            Debug.Log("Inventory cleared");

            // Test adding units
            var normalPool = UnitManager.Instance.GetUnitPool(Rarity.Normal);
            if (normalPool.Count > 0)
            {
                UnitData testUnit = normalPool[0];
                bool added = UnitManager.Instance.AddUnit(testUnit);
                Debug.Log($"Add unit result: {added}");
                Debug.Log($"Inventory count: {UnitManager.Instance.Inventory.Count}");

                // Test removing unit
                bool removed = UnitManager.Instance.RemoveUnit(testUnit);
                Debug.Log($"Remove unit result: {removed}");
                Debug.Log($"Inventory count after remove: {UnitManager.Instance.Inventory.Count}");

                bool passed = added && removed && UnitManager.Instance.Inventory.Count == 0;
                Debug.Log(passed ? "✅ PASSED: Add/Remove operations work correctly" : "❌ FAILED: Inventory operations failed");
            }
            else
            {
                Debug.LogError("❌ FAILED: No units available for testing");
            }
        }

        /// <summary>
        /// Test 4: Run large-scale probability distribution test.
        /// Verifies gacha drop rates match expected percentages (50/30/15/5).
        /// </summary>
        [ContextMenu("Test 4: Gacha Probability Distribution")]
        public void TestGachaProbabilityDistribution()
        {
            Debug.Log($"\n[TEST 4] Gacha Probability Distribution ({probabilityTestCount} draws)");

            // Clear previous results
            gachaResults.Clear();
            gachaResults[Rarity.Normal] = 0;
            gachaResults[Rarity.Rare] = 0;
            gachaResults[Rarity.Epic] = 0;
            gachaResults[Rarity.Legendary] = 0;

            // Clear inventory and set high gold
            UnitManager.Instance.ClearInventory();
            GameplayManager.Instance.SetGold(probabilityTestCount * 10);

            // Perform many draws
            for (int i = 0; i < probabilityTestCount; i++)
            {
                UnitData drawn = UnitManager.Instance.DrawUnit();
                if (drawn != null)
                {
                    gachaResults[drawn.rarity]++;
                }
            }

            // Calculate percentages
            float normalPercent = (gachaResults[Rarity.Normal] / (float)probabilityTestCount) * 100f;
            float rarePercent = (gachaResults[Rarity.Rare] / (float)probabilityTestCount) * 100f;
            float epicPercent = (gachaResults[Rarity.Epic] / (float)probabilityTestCount) * 100f;
            float legendaryPercent = (gachaResults[Rarity.Legendary] / (float)probabilityTestCount) * 100f;

            Debug.Log("=== GACHA RESULTS ===");
            Debug.Log($"Normal: {gachaResults[Rarity.Normal]} ({normalPercent:F2}% | Expected: 50%)");
            Debug.Log($"Rare: {gachaResults[Rarity.Rare]} ({rarePercent:F2}% | Expected: 30%)");
            Debug.Log($"Epic: {gachaResults[Rarity.Epic]} ({epicPercent:F2}% | Expected: 15%)");
            Debug.Log($"Legendary: {gachaResults[Rarity.Legendary]} ({legendaryPercent:F2}% | Expected: 5%)");

            // Validate results (allow 2% margin of error)
            bool normalOk = Mathf.Abs(normalPercent - 50f) < 2f;
            bool rareOk = Mathf.Abs(rarePercent - 30f) < 2f;
            bool epicOk = Mathf.Abs(epicPercent - 15f) < 2f;
            bool legendaryOk = Mathf.Abs(legendaryPercent - 5f) < 2f;

            bool passed = normalOk && rareOk && epicOk && legendaryOk;

            if (passed)
            {
                Debug.Log("✅ PASSED: All probabilities within 2% margin of expected values");
            }
            else
            {
                Debug.LogWarning("⚠️ WARNING: Some probabilities deviate from expected values");
                if (!normalOk) Debug.LogWarning($"Normal rate deviation: {Mathf.Abs(normalPercent - 50f):F2}%");
                if (!rareOk) Debug.LogWarning($"Rare rate deviation: {Mathf.Abs(rarePercent - 30f):F2}%");
                if (!epicOk) Debug.LogWarning($"Epic rate deviation: {Mathf.Abs(epicPercent - 15f):F2}%");
                if (!legendaryOk) Debug.LogWarning($"Legendary rate deviation: {Mathf.Abs(legendaryPercent - 5f):F2}%");
            }
        }

        /// <summary>
        /// Test 5: Verify inventory capacity limits.
        /// </summary>
        [ContextMenu("Test 5: Inventory Capacity")]
        public void TestInventoryCapacity()
        {
            Debug.Log("\n[TEST 5] Inventory Capacity Limits");

            // Clear and fill inventory to max
            UnitManager.Instance.ClearInventory();
            GameplayManager.Instance.SetGold(10000);

            int maxCapacity = 50; // Default from UnitManager
            Debug.Log($"Attempting to fill inventory to capacity ({maxCapacity})...");

            int drawCount = 0;
            while (UnitManager.Instance.CanDraw() && drawCount < maxCapacity + 10)
            {
                UnitManager.Instance.DrawUnit();
                drawCount++;
            }

            Debug.Log($"Total draws attempted: {drawCount}");
            Debug.Log($"Final inventory count: {UnitManager.Instance.Inventory.Count}");
            Debug.Log($"Inventory full: {UnitManager.Instance.IsInventoryFull}");
            Debug.Log($"Available slots: {UnitManager.Instance.AvailableSlots}");

            bool passed = UnitManager.Instance.IsInventoryFull &&
                         UnitManager.Instance.Inventory.Count == maxCapacity;

            Debug.Log(passed ? "✅ PASSED: Inventory capacity enforced correctly" : "❌ FAILED: Inventory capacity issue");
        }

        /// <summary>
        /// Test 6: Verify event notifications are fired correctly.
        /// </summary>
        [ContextMenu("Test 6: Event Notifications")]
        public void TestEventNotifications()
        {
            Debug.Log("\n[TEST 6] Event Notifications");

            bool onUnitDrawnFired = false;
            bool onInventoryChangedFired = false;
            bool onDrawFailedFired = false;

            // Subscribe to events
            UnitManager.Instance.OnUnitDrawn += (unit, gold) => {
                onUnitDrawnFired = true;
                Debug.Log($"Event: OnUnitDrawn - {unit.GetDisplayName()}, Gold: {gold}");
            };

            UnitManager.Instance.OnInventoryChanged += (inventory, operation, unit) => {
                onInventoryChangedFired = true;
                Debug.Log($"Event: OnInventoryChanged - {operation}, Unit: {unit?.unitName}");
            };

            UnitManager.Instance.OnDrawFailed += (reason) => {
                onDrawFailedFired = true;
                Debug.Log($"Event: OnDrawFailed - {reason}");
            };

            // Test successful draw
            UnitManager.Instance.ClearInventory();
            GameplayManager.Instance.SetGold(100);
            UnitManager.Instance.DrawUnit();

            Debug.Log($"OnUnitDrawn fired: {onUnitDrawnFired}");
            Debug.Log($"OnInventoryChanged fired: {onInventoryChangedFired}");

            // Reset flags
            onDrawFailedFired = false;

            // Test failed draw (no gold)
            GameplayManager.Instance.SetGold(0);
            UnitManager.Instance.DrawUnit();

            Debug.Log($"OnDrawFailed fired: {onDrawFailedFired}");

            bool passed = onUnitDrawnFired && onInventoryChangedFired && onDrawFailedFired;
            Debug.Log(passed ? "✅ PASSED: All events fire correctly" : "❌ FAILED: Some events did not fire");
        }
        #endregion

        #region Debug Utilities
        /// <summary>
        /// Display current UnitManager state in console.
        /// </summary>
        [ContextMenu("Display Manager Status")]
        public void DisplayManagerStatus()
        {
            Debug.Log("\n========== UNIT MANAGER STATUS ==========");
            Debug.Log(UnitManager.Instance.GetDropRateInfo());
            Debug.Log(UnitManager.Instance.GetInventorySummary());
            Debug.Log($"Can Draw: {UnitManager.Instance.CanDraw()}");
            Debug.Log($"Gacha Cost: {UnitManager.Instance.GetGachaCost()} gold");
            Debug.Log($"Current Gold: {GameplayManager.Instance.CurrentGold}");
            Debug.Log("========================================");
        }

        /// <summary>
        /// Test single draw and log result.
        /// </summary>
        [ContextMenu("Test Single Draw")]
        public void TestSingleDraw()
        {
            GameplayManager.Instance.SetGold(100);
            UnitData drawn = UnitManager.Instance.DrawUnit();

            if (drawn != null)
            {
                Debug.Log($"Drew: {drawn}");
            }
            else
            {
                Debug.LogWarning("Draw failed - check gold and inventory capacity");
            }
        }
        #endregion
    }
}
