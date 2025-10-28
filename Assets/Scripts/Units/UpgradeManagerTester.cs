using UnityEngine;
using LottoDefense.Gameplay;
using LottoDefense.Grid;

namespace LottoDefense.Units
{
    /// <summary>
    /// Comprehensive test suite for the unit upgrade system.
    /// Validates cost formulas, stat calculations, and integration with gold system.
    /// </summary>
    public class UpgradeManagerTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool runTestsOnStart = true;
        [SerializeField] private bool verboseLogging = true;

        [Header("Test Unit Setup")]
        [SerializeField] private UnitData testUnitData;
        [SerializeField] private GameObject unitPrefab;

        private int testsRun = 0;
        private int testsPassed = 0;
        private int testsFailed = 0;

        #region Unity Lifecycle
        private void Start()
        {
            if (runTestsOnStart)
            {
                RunAllTests();
            }
        }
        #endregion

        #region Test Runner
        /// <summary>
        /// Execute all test suites.
        /// </summary>
        [ContextMenu("Run All Tests")]
        public void RunAllTests()
        {
            Debug.Log("=== UpgradeManager Test Suite Starting ===");
            testsRun = 0;
            testsPassed = 0;
            testsFailed = 0;

            // Core formula tests
            TestUpgradeCostFormula();
            TestAttackMultiplierFormula();
            TestMaxLevelCost();

            // Upgrade logic tests
            TestUpgradeSuccess();
            TestInsufficientGold();
            TestMaxLevelUpgrade();
            TestGoldDeduction();

            // Integration tests
            TestMultipleUpgrades();
            TestUpgradeStatsCalculation();

            // Edge case tests
            TestNullUnitHandling();
            TestLevelProgression();

            PrintTestResults();
        }

        private void PrintTestResults()
        {
            Debug.Log($"=== Test Results ===");
            Debug.Log($"Tests Run: {testsRun}");
            Debug.Log($"Passed: {testsPassed} ({(testsRun > 0 ? (testsPassed * 100 / testsRun) : 0)}%)");
            Debug.Log($"Failed: {testsFailed}");

            if (testsFailed == 0)
            {
                Debug.Log("<color=green>ALL TESTS PASSED!</color>");
            }
            else
            {
                Debug.LogError($"<color=red>{testsFailed} TESTS FAILED</color>");
            }
        }
        #endregion

        #region Cost Formula Tests
        [ContextMenu("Test: Upgrade Cost Formula")]
        public void TestUpgradeCostFormula()
        {
            // Test cost progression matches specification
            // Level 1→2: 10 * 1^1.5 = 10 gold
            AssertEqual(UpgradeManager.Instance.GetUpgradeCost(1), 10, "Cost L1→L2");

            // Level 2→3: 10 * 2^1.5 ≈ 28 gold
            AssertEqual(UpgradeManager.Instance.GetUpgradeCost(2), 28, "Cost L2→L3");

            // Level 3→4: 10 * 3^1.5 ≈ 52 gold
            AssertEqual(UpgradeManager.Instance.GetUpgradeCost(3), 52, "Cost L3→L4");

            // Level 5→6: 10 * 5^1.5 ≈ 112 gold
            AssertEqual(UpgradeManager.Instance.GetUpgradeCost(5), 112, "Cost L5→L6");

            // Level 9→10: 10 * 9^1.5 = 270 gold
            AssertEqual(UpgradeManager.Instance.GetUpgradeCost(9), 270, "Cost L9→L10");
        }

        [ContextMenu("Test: Attack Multiplier Formula")]
        public void TestAttackMultiplierFormula()
        {
            // Test attack multiplier progression
            // Level 1: 1.0x (base)
            AssertFloatEqual(UpgradeManager.Instance.CalculateAttackMultiplier(1), 1.0f, "Multiplier L1");

            // Level 2: 1.1x (+10%)
            AssertFloatEqual(UpgradeManager.Instance.CalculateAttackMultiplier(2), 1.1f, "Multiplier L2");

            // Level 5: 1.4x (+40%)
            AssertFloatEqual(UpgradeManager.Instance.CalculateAttackMultiplier(5), 1.4f, "Multiplier L5");

            // Level 10: 1.9x (+90%)
            AssertFloatEqual(UpgradeManager.Instance.CalculateAttackMultiplier(10), 1.9f, "Multiplier L10");
        }

        [ContextMenu("Test: Max Level Cost")]
        public void TestMaxLevelCost()
        {
            // At max level, cost should be int.MaxValue
            int maxLevel = UpgradeManager.Instance.GetMaxLevel();
            int cost = UpgradeManager.Instance.GetUpgradeCost(maxLevel);
            AssertEqual(cost, int.MaxValue, "Max level cost");
        }
        #endregion

        #region Upgrade Logic Tests
        [ContextMenu("Test: Upgrade Success")]
        public void TestUpgradeSuccess()
        {
            // Setup: Create test unit with sufficient gold
            Unit testUnit = CreateTestUnit();
            if (testUnit == null) return;

            // Give player enough gold
            GameplayManager.Instance.SetGold(1000);

            int initialLevel = testUnit.UpgradeLevel;
            int initialAttack = testUnit.CurrentAttack;
            int initialGold = GameplayManager.Instance.CurrentGold;

            // Attempt upgrade
            bool success = UpgradeManager.Instance.TryUpgradeUnit(testUnit);

            // Verify upgrade succeeded
            AssertTrue(success, "Upgrade should succeed with sufficient gold");
            AssertEqual(testUnit.UpgradeLevel, initialLevel + 1, "Level should increase by 1");
            AssertTrue(testUnit.CurrentAttack > initialAttack, "Attack should increase");

            // Verify gold was deducted
            int expectedCost = UpgradeManager.Instance.GetUpgradeCost(initialLevel);
            AssertEqual(GameplayManager.Instance.CurrentGold, initialGold - expectedCost, "Gold should be deducted");

            // Cleanup
            if (testUnit != null)
                Destroy(testUnit.gameObject);
        }

        [ContextMenu("Test: Insufficient Gold")]
        public void TestInsufficientGold()
        {
            // Setup: Create test unit with insufficient gold
            Unit testUnit = CreateTestUnit();
            if (testUnit == null) return;

            // Set gold to 0
            GameplayManager.Instance.SetGold(0);

            int initialLevel = testUnit.UpgradeLevel;

            // Attempt upgrade
            bool success = UpgradeManager.Instance.TryUpgradeUnit(testUnit);

            // Verify upgrade failed
            AssertFalse(success, "Upgrade should fail with insufficient gold");
            AssertEqual(testUnit.UpgradeLevel, initialLevel, "Level should not change");

            // Cleanup
            if (testUnit != null)
                Destroy(testUnit.gameObject);
        }

        [ContextMenu("Test: Max Level Upgrade")]
        public void TestMaxLevelUpgrade()
        {
            // Setup: Create unit at max level
            Unit testUnit = CreateTestUnit();
            if (testUnit == null) return;

            // Upgrade to max level
            int maxLevel = UpgradeManager.Instance.GetMaxLevel();
            float maxMultiplier = UpgradeManager.Instance.CalculateAttackMultiplier(maxLevel);
            testUnit.ApplyUpgrade(maxLevel, maxMultiplier);

            // Give sufficient gold
            GameplayManager.Instance.SetGold(10000);

            // Attempt upgrade
            bool success = UpgradeManager.Instance.TryUpgradeUnit(testUnit);

            // Verify upgrade failed
            AssertFalse(success, "Upgrade should fail at max level");
            AssertEqual(testUnit.UpgradeLevel, maxLevel, "Level should remain at max");

            // Cleanup
            if (testUnit != null)
                Destroy(testUnit.gameObject);
        }

        [ContextMenu("Test: Gold Deduction")]
        public void TestGoldDeduction()
        {
            // Setup
            Unit testUnit = CreateTestUnit();
            if (testUnit == null) return;

            int initialGold = 500;
            GameplayManager.Instance.SetGold(initialGold);

            int upgradeCost = UpgradeManager.Instance.GetUpgradeCost(testUnit.UpgradeLevel);

            // Perform upgrade
            UpgradeManager.Instance.TryUpgradeUnit(testUnit);

            // Verify exact gold deduction
            int expectedGold = initialGold - upgradeCost;
            AssertEqual(GameplayManager.Instance.CurrentGold, expectedGold, "Exact gold deduction");

            // Cleanup
            if (testUnit != null)
                Destroy(testUnit.gameObject);
        }
        #endregion

        #region Integration Tests
        [ContextMenu("Test: Multiple Upgrades")]
        public void TestMultipleUpgrades()
        {
            // Setup
            Unit testUnit = CreateTestUnit();
            if (testUnit == null) return;

            GameplayManager.Instance.SetGold(10000); // Plenty of gold

            int targetLevel = 5;
            int expectedTotalCost = 0;

            // Calculate expected total cost
            for (int level = 1; level < targetLevel; level++)
            {
                expectedTotalCost += UpgradeManager.Instance.GetUpgradeCost(level);
            }

            int initialGold = GameplayManager.Instance.CurrentGold;

            // Perform multiple upgrades
            for (int i = 0; i < (targetLevel - 1); i++)
            {
                bool success = UpgradeManager.Instance.TryUpgradeUnit(testUnit);
                AssertTrue(success, $"Upgrade {i + 1} should succeed");
            }

            // Verify final state
            AssertEqual(testUnit.UpgradeLevel, targetLevel, $"Should reach level {targetLevel}");
            AssertEqual(GameplayManager.Instance.CurrentGold, initialGold - expectedTotalCost, "Total gold cost");

            // Cleanup
            if (testUnit != null)
                Destroy(testUnit.gameObject);
        }

        [ContextMenu("Test: Upgrade Stats Calculation")]
        public void TestUpgradeStatsCalculation()
        {
            // Setup
            Unit testUnit = CreateTestUnit();
            if (testUnit == null) return;

            // Get upgrade stats
            UpgradeStats stats = UpgradeManager.Instance.GetUpgradeStats(testUnit);

            // Verify stats structure
            AssertEqual(stats.currentLevel, testUnit.UpgradeLevel, "Stats current level");
            AssertEqual(stats.currentAttack, testUnit.CurrentAttack, "Stats current attack");
            AssertFalse(stats.isMaxLevel, "Stats should not be max level initially");

            // Verify next level calculations
            int expectedNextAttack = Mathf.RoundToInt(testUnit.Data.attack *
                UpgradeManager.Instance.CalculateAttackMultiplier(testUnit.UpgradeLevel + 1));
            AssertEqual(stats.nextAttack, expectedNextAttack, "Stats next attack calculation");

            // Cleanup
            if (testUnit != null)
                Destroy(testUnit.gameObject);
        }
        #endregion

        #region Edge Case Tests
        [ContextMenu("Test: Null Unit Handling")]
        public void TestNullUnitHandling()
        {
            // Test various operations with null
            bool canUpgrade = UpgradeManager.Instance.CanUpgradeUnit(null);
            AssertFalse(canUpgrade, "CanUpgrade should return false for null");

            bool upgradeSuccess = UpgradeManager.Instance.TryUpgradeUnit(null);
            AssertFalse(upgradeSuccess, "TryUpgrade should return false for null");

            UpgradeStats stats = UpgradeManager.Instance.GetUpgradeStats(null);
            AssertEqual(stats.currentLevel, 0, "GetUpgradeStats should return default for null");
        }

        [ContextMenu("Test: Level Progression")]
        public void TestLevelProgression()
        {
            // Verify costs increase progressively
            int previousCost = 0;
            for (int level = 1; level < UpgradeManager.Instance.GetMaxLevel(); level++)
            {
                int cost = UpgradeManager.Instance.GetUpgradeCost(level);
                AssertTrue(cost > previousCost, $"Cost L{level}→L{level + 1} should be > previous");
                previousCost = cost;
            }
        }
        #endregion

        #region Helper Methods
        private Unit CreateTestUnit()
        {
            if (testUnitData == null)
            {
                Debug.LogError("[UpgradeManagerTester] Test unit data not assigned!");
                testsFailed++;
                return null;
            }

            // Create unit GameObject
            GameObject unitObj = unitPrefab != null ?
                Instantiate(unitPrefab) :
                new GameObject("TestUnit");

            // Add Unit component if needed
            Unit unit = unitObj.GetComponent<Unit>();
            if (unit == null)
            {
                unit = unitObj.AddComponent<Unit>();
            }

            // Initialize unit
            unit.Initialize(testUnitData, new Vector2Int(0, 0));

            return unit;
        }

        private void AssertEqual(int actual, int expected, string testName)
        {
            testsRun++;
            if (actual == expected)
            {
                testsPassed++;
                if (verboseLogging)
                    Debug.Log($"<color=green>PASS</color> {testName}: {actual} == {expected}");
            }
            else
            {
                testsFailed++;
                Debug.LogError($"<color=red>FAIL</color> {testName}: Expected {expected}, got {actual}");
            }
        }

        private void AssertFloatEqual(float actual, float expected, string testName, float tolerance = 0.01f)
        {
            testsRun++;
            if (Mathf.Abs(actual - expected) <= tolerance)
            {
                testsPassed++;
                if (verboseLogging)
                    Debug.Log($"<color=green>PASS</color> {testName}: {actual:F2} ≈ {expected:F2}");
            }
            else
            {
                testsFailed++;
                Debug.LogError($"<color=red>FAIL</color> {testName}: Expected {expected:F2}, got {actual:F2}");
            }
        }

        private void AssertTrue(bool condition, string testName)
        {
            testsRun++;
            if (condition)
            {
                testsPassed++;
                if (verboseLogging)
                    Debug.Log($"<color=green>PASS</color> {testName}");
            }
            else
            {
                testsFailed++;
                Debug.LogError($"<color=red>FAIL</color> {testName}: Expected true, got false");
            }
        }

        private void AssertFalse(bool condition, string testName)
        {
            testsRun++;
            if (!condition)
            {
                testsPassed++;
                if (verboseLogging)
                    Debug.Log($"<color=green>PASS</color> {testName}");
            }
            else
            {
                testsFailed++;
                Debug.LogError($"<color=red>FAIL</color> {testName}: Expected false, got true");
            }
        }
        #endregion

        #region Manual Test Utilities
        [ContextMenu("Print Cost Curve")]
        public void PrintCostCurve()
        {
            Debug.Log(UpgradeManager.Instance.GetUpgradeCostCurve());
        }

        [ContextMenu("Test Upgrade UI Integration")]
        public void TestUpgradeUIIntegration()
        {
            // Create test unit
            Unit testUnit = CreateTestUnit();
            if (testUnit == null) return;

            // Set gold
            GameplayManager.Instance.SetGold(500);

            // Select unit
            UpgradeManager.Instance.SelectUnit(testUnit);

            Debug.Log($"[Test] Unit selected: {testUnit}");
            Debug.Log($"[Test] Can upgrade: {UpgradeManager.Instance.CanUpgradeUnit(testUnit)}");
            Debug.Log($"[Test] Stats: {UpgradeManager.Instance.GetUpgradeStats(testUnit)}");

            // Cleanup after 5 seconds
            Destroy(testUnit.gameObject, 5f);
        }
        #endregion
    }
}
