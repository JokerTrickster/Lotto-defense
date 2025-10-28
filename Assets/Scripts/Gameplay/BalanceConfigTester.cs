using UnityEngine;

namespace LottoDefense.Gameplay
{
    /// <summary>
    /// Test harness for BalanceConfig functionality.
    /// Validates balance formulas and provides runtime testing.
    /// </summary>
    public class BalanceConfigTester : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Configuration")]
        [Tooltip("Balance configuration to test")]
        [SerializeField] private BalanceConfig balanceConfig;

        [Header("Test Parameters")]
        [Tooltip("Test round number")]
        [SerializeField] private int testRound = 1;

        [Tooltip("Test upgrade level")]
        [SerializeField] private int testUpgradeLevel = 1;

        [Header("Display Options")]
        [Tooltip("Show test UI")]
        [SerializeField] private bool showTestUI = true;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            if (balanceConfig == null)
            {
                Debug.LogError("[BalanceConfigTester] No BalanceConfig assigned!");
                return;
            }

            Debug.Log("[BalanceConfigTester] Initialized - Press B for balance report");
        }

        private void Update()
        {
            // Keyboard shortcuts
            if (Input.GetKeyDown(KeyCode.B))
            {
                PrintBalanceReport();
            }

            if (Input.GetKeyDown(KeyCode.V))
            {
                ValidateBalance();
            }
        }

        private void OnGUI()
        {
            if (!showTestUI)
                return;

            GUILayout.BeginArea(new Rect(Screen.width - 310, 10, 300, 500));
            GUILayout.Label("BALANCE CONFIG TESTER", GUI.skin.box);

            if (balanceConfig == null)
            {
                GUILayout.Label("No BalanceConfig assigned!", GUI.skin.box);
                GUILayout.EndArea();
                return;
            }

            // Test round selector
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Test Round: {testRound}");
            if (GUILayout.Button("-") && testRound > 1)
                testRound--;
            if (GUILayout.Button("+") && testRound < balanceConfig.MaxRounds)
                testRound++;
            GUILayout.EndHorizontal();

            // Test upgrade level selector
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Upgrade Level: {testUpgradeLevel}");
            if (GUILayout.Button("-") && testUpgradeLevel > 1)
                testUpgradeLevel--;
            if (GUILayout.Button("+") && testUpgradeLevel < balanceConfig.MaxUpgradeLevel)
                testUpgradeLevel++;
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Display current values
            int goldReward = balanceConfig.GetGoldReward(testRound);
            int upgradeCost = balanceConfig.GetUpgradeCost(testUpgradeLevel);
            float attackMult = balanceConfig.GetAttackMultiplier(testUpgradeLevel + 1);
            float hpMult = balanceConfig.GetHPMultiplier(testRound);
            float defMult = balanceConfig.GetDefenseMultiplier(testRound);

            GUILayout.Label($"Gold/Monster: {goldReward}", GUI.skin.box);
            GUILayout.Label($"Upgrade Cost: {upgradeCost}", GUI.skin.box);
            GUILayout.Label($"Next ATK Mult: x{attackMult:F2}", GUI.skin.box);
            GUILayout.Label($"HP Multiplier: x{hpMult:F2}", GUI.skin.box);
            GUILayout.Label($"DEF Multiplier: x{defMult:F2}", GUI.skin.box);

            GUILayout.Space(10);

            if (GUILayout.Button("Print Balance Report"))
            {
                PrintBalanceReport();
            }

            if (GUILayout.Button("Validate Balance"))
            {
                ValidateBalance();
            }

            if (GUILayout.Button("Test Gold Economy"))
            {
                TestGoldEconomy();
            }

            if (GUILayout.Button("Test Upgrade Curve"))
            {
                TestUpgradeCurve();
            }

            if (GUILayout.Button("Test Difficulty Scaling"))
            {
                TestDifficultyScaling();
            }

            GUILayout.EndArea();
        }
        #endregion

        #region Test Methods
        /// <summary>
        /// Print full balance report to console.
        /// </summary>
        [ContextMenu("Print Balance Report")]
        public void PrintBalanceReport()
        {
            if (balanceConfig == null)
            {
                Debug.LogError("[BalanceConfigTester] No BalanceConfig assigned!");
                return;
            }

            string report = balanceConfig.GenerateBalanceReport();
            Debug.Log(report);
        }

        /// <summary>
        /// Validate balance configuration values.
        /// </summary>
        [ContextMenu("Validate Balance")]
        public void ValidateBalance()
        {
            if (balanceConfig == null)
            {
                Debug.LogError("[BalanceConfigTester] No BalanceConfig assigned!");
                return;
            }

            Debug.Log("=== BALANCE VALIDATION ===");

            // Validate gacha probabilities
            bool gachaValid = balanceConfig.ValidateProbabilities();
            Debug.Log($"Gacha Probabilities: {(gachaValid ? "PASS" : "FAIL")}");

            // Validate gold economy
            int totalGoldFromMonsters = 0;
            for (int round = 1; round <= balanceConfig.MaxRounds; round++)
            {
                // Assume ~10 monsters per round
                int monstersPerRound = 10;
                int goldThisRound = balanceConfig.GetGoldReward(round) * monstersPerRound;
                totalGoldFromMonsters += goldThisRound;
            }

            int totalUpgradeCost = balanceConfig.GetTotalUpgradeCost();

            Debug.Log($"Total Gold Earned: ~{totalGoldFromMonsters}");
            Debug.Log($"Total Upgrade Cost: {totalUpgradeCost}");

            bool economyBalanced = totalGoldFromMonsters > totalUpgradeCost * 2;
            Debug.Log($"Economy Balance: {(economyBalanced ? "PASS" : "WARNING - might be tight")}");

            // Validate difficulty scaling
            float minHP = balanceConfig.GetHPMultiplier(1);
            float maxHP = balanceConfig.GetHPMultiplier(balanceConfig.MaxRounds);
            bool hpScalingValid = maxHP > minHP && maxHP <= 10f; // Reasonable upper limit

            Debug.Log($"HP Scaling: {minHP:F2}x → {maxHP:F2}x {(hpScalingValid ? "PASS" : "FAIL")}");

            Debug.Log("=== VALIDATION COMPLETE ===");
        }

        /// <summary>
        /// Test gold economy across all rounds.
        /// </summary>
        [ContextMenu("Test Gold Economy")]
        public void TestGoldEconomy()
        {
            if (balanceConfig == null)
            {
                Debug.LogError("[BalanceConfigTester] No BalanceConfig assigned!");
                return;
            }

            Debug.Log("=== GOLD ECONOMY TEST ===");

            int cumulativeGold = balanceConfig.StartingGold;
            const int monstersPerRound = 10;

            for (int round = 1; round <= balanceConfig.MaxRounds; round += 5)
            {
                int goldPerMonster = balanceConfig.GetGoldReward(round);
                int roundGold = goldPerMonster * monstersPerRound;
                cumulativeGold += roundGold;

                Debug.Log($"Round {round}: {goldPerMonster}g/monster × {monstersPerRound} = {roundGold}g | Total: {cumulativeGold}g");
            }

            Debug.Log($"Total Gold by Round {balanceConfig.MaxRounds}: {cumulativeGold}");
            Debug.Log($"Total Upgrade Cost (L1→L{balanceConfig.MaxUpgradeLevel}): {balanceConfig.GetTotalUpgradeCost()}");
            Debug.Log("=== TEST COMPLETE ===");
        }

        /// <summary>
        /// Test upgrade cost curve.
        /// </summary>
        [ContextMenu("Test Upgrade Curve")]
        public void TestUpgradeCurve()
        {
            if (balanceConfig == null)
            {
                Debug.LogError("[BalanceConfigTester] No BalanceConfig assigned!");
                return;
            }

            Debug.Log("=== UPGRADE CURVE TEST ===");

            int totalCost = 0;

            for (int level = 1; level < balanceConfig.MaxUpgradeLevel; level++)
            {
                int cost = balanceConfig.GetUpgradeCost(level);
                float multiplier = balanceConfig.GetAttackMultiplier(level + 1);
                totalCost += cost;

                Debug.Log($"L{level}→L{level + 1}: {cost} gold (ATK x{multiplier:F2}) | Total: {totalCost}g");
            }

            Debug.Log($"Total Cost L1→L{balanceConfig.MaxUpgradeLevel}: {totalCost} gold");
            Debug.Log("=== TEST COMPLETE ===");
        }

        /// <summary>
        /// Test difficulty scaling across rounds.
        /// </summary>
        [ContextMenu("Test Difficulty Scaling")]
        public void TestDifficultyScaling()
        {
            if (balanceConfig == null)
            {
                Debug.LogError("[BalanceConfigTester] No BalanceConfig assigned!");
                return;
            }

            Debug.Log("=== DIFFICULTY SCALING TEST ===");

            for (int round = 1; round <= balanceConfig.MaxRounds; round += 5)
            {
                float hpMult = balanceConfig.GetHPMultiplier(round);
                float defMult = balanceConfig.GetDefenseMultiplier(round);

                Debug.Log($"Round {round:D2}: HP x{hpMult:F2}, DEF x{defMult:F2}");
            }

            Debug.Log("=== TEST COMPLETE ===");
        }
        #endregion

        #region Simulation
        /// <summary>
        /// Simulate a full 30-round playthrough economy.
        /// </summary>
        [ContextMenu("Simulate Full Playthrough")]
        public void SimulateFullPlaythrough()
        {
            if (balanceConfig == null)
            {
                Debug.LogError("[BalanceConfigTester] No BalanceConfig assigned!");
                return;
            }

            Debug.Log("=== PLAYTHROUGH SIMULATION ===");

            int currentGold = balanceConfig.StartingGold;
            int totalGoldEarned = 0;
            int totalGoldSpent = 0;
            const int monstersPerRound = 10;

            for (int round = 1; round <= balanceConfig.MaxRounds; round++)
            {
                // Earn gold from monsters
                int goldPerMonster = balanceConfig.GetGoldReward(round);
                int roundGold = goldPerMonster * monstersPerRound;
                currentGold += roundGold;
                totalGoldEarned += roundGold;

                // Simulate spending (upgrade every 5 rounds)
                if (round % 5 == 0 && round / 5 < balanceConfig.MaxUpgradeLevel)
                {
                    int upgradeLevel = round / 5;
                    int upgradeCost = balanceConfig.GetUpgradeCost(upgradeLevel);

                    if (currentGold >= upgradeCost)
                    {
                        currentGold -= upgradeCost;
                        totalGoldSpent += upgradeCost;
                        Debug.Log($"Round {round}: Upgraded to L{upgradeLevel + 1} for {upgradeCost}g");
                    }
                }

                if (round % 10 == 0)
                {
                    Debug.Log($"Round {round}: Gold: {currentGold} | Earned: {totalGoldEarned} | Spent: {totalGoldSpent}");
                }
            }

            Debug.Log($"\nFinal Stats:");
            Debug.Log($"  Gold Remaining: {currentGold}");
            Debug.Log($"  Total Earned: {totalGoldEarned}");
            Debug.Log($"  Total Spent: {totalGoldSpent}");
            Debug.Log($"  Economy Health: {(currentGold > 0 ? "HEALTHY" : "TIGHT")}");

            Debug.Log("=== SIMULATION COMPLETE ===");
        }
        #endregion
    }
}
