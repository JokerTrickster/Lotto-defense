using UnityEngine;
using LottoDefense.Units;
using LottoDefense.Monsters;

namespace LottoDefense.VFX
{
    /// <summary>
    /// Test harness for VFXManager functionality.
    /// Provides runtime testing and validation of visual effects system.
    /// </summary>
    public class VFXManagerTester : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Test Settings")]
        [Tooltip("Enable automatic testing on start")]
        [SerializeField] private bool autoTest = false;

        [Tooltip("Test interval in seconds")]
        [SerializeField] private float testInterval = 2f;

        [Header("Test References")]
        [Tooltip("Sample unit for attack animation testing")]
        [SerializeField] private Unit testUnit;

        [Tooltip("Sample monster for damage testing")]
        [SerializeField] private Monster testMonster;

        [Header("Test Controls")]
        [Tooltip("Show test buttons in game")]
        [SerializeField] private bool showTestUI = true;
        #endregion

        #region Private Fields
        private float nextTestTime = 0f;
        private int testCycleCount = 0;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            Debug.Log("[VFXManagerTester] Initialized - Press T for manual tests");

            if (autoTest)
            {
                Debug.Log($"[VFXManagerTester] Auto-testing enabled (interval: {testInterval}s)");
            }
        }

        private void Update()
        {
            // Manual test trigger
            if (Input.GetKeyDown(KeyCode.T))
            {
                RunAllTests();
            }

            // Auto test
            if (autoTest && Time.time >= nextTestTime)
            {
                nextTestTime = Time.time + testInterval;
                testCycleCount++;
                Debug.Log($"[VFXManagerTester] Auto-test cycle {testCycleCount}");
                RunAllTests();
            }
        }

        private void OnGUI()
        {
            if (!showTestUI)
                return;

            GUILayout.BeginArea(new Rect(10, 80, 300, 400));
            GUILayout.Label("VFX MANAGER TESTER", GUI.skin.box);

            if (GUILayout.Button("Test All VFX"))
            {
                RunAllTests();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Test Damage Numbers"))
            {
                TestDamageNumbers();
            }

            if (GUILayout.Button("Test Gold Popups"))
            {
                TestGoldPopups();
            }

            if (GUILayout.Button("Test Floating Text"))
            {
                TestFloatingText();
            }

            if (GUILayout.Button("Test Attack Animation"))
            {
                TestAttackAnimation();
            }

            if (GUILayout.Button("Test Death Effect"))
            {
                TestDeathEffect();
            }

            if (GUILayout.Button("Test Unit Placement"))
            {
                TestUnitPlacement();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Stress Test (100 damage numbers)"))
            {
                StressTestDamageNumbers();
            }

            GUILayout.EndArea();
        }
        #endregion

        #region Test Methods
        /// <summary>
        /// Run all VFX tests sequentially.
        /// </summary>
        [ContextMenu("Run All Tests")]
        public void RunAllTests()
        {
            Debug.Log("=== VFX MANAGER TEST SUITE ===");

            TestDamageNumbers();
            TestGoldPopups();
            TestFloatingText();

            if (testUnit != null && testMonster != null)
            {
                TestAttackAnimation();
            }

            Debug.Log("=== VFX TESTS COMPLETE ===");
        }

        /// <summary>
        /// Test damage number display at random positions.
        /// </summary>
        [ContextMenu("Test Damage Numbers")]
        public void TestDamageNumbers()
        {
            if (VFXManager.Instance == null)
            {
                Debug.LogError("[VFXManagerTester] VFXManager not found!");
                return;
            }

            // Spawn 5 damage numbers at random positions
            for (int i = 0; i < 5; i++)
            {
                Vector3 randomPos = new Vector3(
                    Random.Range(-5f, 5f),
                    Random.Range(-3f, 3f),
                    0f
                );

                int randomDamage = Random.Range(10, 100);
                bool isCrit = Random.value > 0.8f; // 20% crit chance

                VFXManager.Instance.ShowDamageNumber(randomPos, randomDamage, isCrit);
            }

            Debug.Log("[VFXManagerTester] Spawned 5 damage numbers");
        }

        /// <summary>
        /// Test gold popup display.
        /// </summary>
        [ContextMenu("Test Gold Popups")]
        public void TestGoldPopups()
        {
            if (VFXManager.Instance == null)
            {
                Debug.LogError("[VFXManagerTester] VFXManager not found!");
                return;
            }

            // Spawn 3 gold popups
            for (int i = 0; i < 3; i++)
            {
                Vector3 randomPos = new Vector3(
                    Random.Range(-5f, 5f),
                    Random.Range(-3f, 3f),
                    0f
                );

                int randomGold = Random.Range(2, 6);
                VFXManager.Instance.ShowGoldPopup(randomPos, randomGold);
            }

            Debug.Log("[VFXManagerTester] Spawned 3 gold popups");
        }

        /// <summary>
        /// Test floating text with custom messages.
        /// </summary>
        [ContextMenu("Test Floating Text")]
        public void TestFloatingText()
        {
            if (VFXManager.Instance == null)
            {
                Debug.LogError("[VFXManagerTester] VFXManager not found!");
                return;
            }

            string[] messages = new string[]
            {
                "CRITICAL!",
                "Level Up!",
                "Victory!",
                "+10 Bonus",
                "Combo x3"
            };

            Color[] colors = new Color[]
            {
                Color.red,
                Color.cyan,
                Color.green,
                Color.yellow,
                Color.magenta
            };

            for (int i = 0; i < messages.Length; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-5f, 5f),
                    Random.Range(-3f, 3f),
                    0f
                );

                VFXManager.Instance.ShowFloatingText(pos, messages[i], colors[i]);
            }

            Debug.Log($"[VFXManagerTester] Spawned {messages.Length} floating text messages");
        }

        /// <summary>
        /// Test attack animation on test unit/monster.
        /// </summary>
        [ContextMenu("Test Attack Animation")]
        public void TestAttackAnimation()
        {
            if (VFXManager.Instance == null)
            {
                Debug.LogError("[VFXManagerTester] VFXManager not found!");
                return;
            }

            if (testUnit == null)
            {
                Debug.LogWarning("[VFXManagerTester] No test unit assigned!");
                return;
            }

            if (testMonster == null)
            {
                Debug.LogWarning("[VFXManagerTester] No test monster assigned!");
                return;
            }

            VFXManager.Instance.PlayAttackAnimation(testUnit, testMonster);
            Debug.Log("[VFXManagerTester] Played attack animation");
        }

        /// <summary>
        /// Test monster death effect.
        /// </summary>
        [ContextMenu("Test Death Effect")]
        public void TestDeathEffect()
        {
            if (VFXManager.Instance == null)
            {
                Debug.LogError("[VFXManagerTester] VFXManager not found!");
                return;
            }

            if (testMonster == null)
            {
                Debug.LogWarning("[VFXManagerTester] No test monster assigned!");
                return;
            }

            VFXManager.Instance.PlayMonsterDeathEffect(testMonster);
            Debug.Log("[VFXManagerTester] Played death effect");
        }

        /// <summary>
        /// Test unit placement effect.
        /// </summary>
        [ContextMenu("Test Unit Placement")]
        public void TestUnitPlacement()
        {
            if (VFXManager.Instance == null)
            {
                Debug.LogError("[VFXManagerTester] VFXManager not found!");
                return;
            }

            if (testUnit == null)
            {
                Debug.LogWarning("[VFXManagerTester] No test unit assigned!");
                return;
            }

            VFXManager.Instance.PlayUnitPlacementEffect(testUnit);
            Debug.Log("[VFXManagerTester] Played unit placement effect");
        }

        /// <summary>
        /// Stress test: spawn many damage numbers simultaneously.
        /// </summary>
        [ContextMenu("Stress Test")]
        public void StressTestDamageNumbers()
        {
            if (VFXManager.Instance == null)
            {
                Debug.LogError("[VFXManagerTester] VFXManager not found!");
                return;
            }

            int count = 100;

            for (int i = 0; i < count; i++)
            {
                Vector3 randomPos = new Vector3(
                    Random.Range(-8f, 8f),
                    Random.Range(-4f, 4f),
                    0f
                );

                int randomDamage = Random.Range(1, 999);
                bool isCrit = Random.value > 0.7f;

                VFXManager.Instance.ShowDamageNumber(randomPos, randomDamage, isCrit);
            }

            Debug.Log($"[VFXManagerTester] Stress test: spawned {count} damage numbers");
        }
        #endregion

        #region Validation
        /// <summary>
        /// Validate VFXManager configuration and state.
        /// </summary>
        [ContextMenu("Validate VFXManager")]
        public void ValidateVFXManager()
        {
            Debug.Log("=== VFX MANAGER VALIDATION ===");

            if (VFXManager.Instance == null)
            {
                Debug.LogError("FAIL: VFXManager instance not found!");
                return;
            }

            Debug.Log("PASS: VFXManager instance found");

            // Additional validation can be added here

            Debug.Log("=== VALIDATION COMPLETE ===");
        }
        #endregion
    }
}
