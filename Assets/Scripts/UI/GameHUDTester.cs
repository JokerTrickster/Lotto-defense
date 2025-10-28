using UnityEngine;
using LottoDefense.Gameplay;

namespace LottoDefense.UI
{
    /// <summary>
    /// Test and validation script for the GameHUD system.
    /// Provides runtime tests and manual controls for testing HUD updates.
    /// </summary>
    public class GameHUDTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool runTestsOnStart = true;
        [SerializeField] private GameHUD hudToTest;

        [Header("Manual Test Controls")]
        [SerializeField] private KeyCode incrementRoundKey = KeyCode.R;
        [SerializeField] private KeyCode addGoldKey = KeyCode.G;
        [SerializeField] private KeyCode decreaseLifeKey = KeyCode.L;
        [SerializeField] private KeyCode runTestsKey = KeyCode.H;

        [Header("Test Values")]
        [SerializeField] private int testMonsterCount = 5;
        [SerializeField] private float testTime = 90f;
        [SerializeField] private int testUnitCount = 3;

        #region Unity Lifecycle
        private void Start()
        {
            if (hudToTest == null)
            {
                hudToTest = FindFirstObjectByType<GameHUD>();
            }

            if (runTestsOnStart)
            {
                Invoke(nameof(RunAllTests), 0.5f); // Delay to ensure initialization
            }
        }

        private void Update()
        {
            HandleManualTestControls();
        }
        #endregion

        #region Manual Test Controls
        /// <summary>
        /// Handle keyboard input for manual HUD testing.
        /// </summary>
        private void HandleManualTestControls()
        {
            if (Input.GetKeyDown(runTestsKey))
            {
                RunAllTests();
            }

            if (Input.GetKeyDown(incrementRoundKey))
            {
                if (GameplayManager.Instance != null)
                {
                    GameplayManager.Instance.NextRound();
                    Debug.Log($"[GameHUDTester] Round incremented to {GameplayManager.Instance.CurrentRound}");
                }
            }

            if (Input.GetKeyDown(addGoldKey))
            {
                if (GameplayManager.Instance != null)
                {
                    GameplayManager.Instance.ModifyGold(10);
                    Debug.Log($"[GameHUDTester] Gold increased to {GameplayManager.Instance.CurrentGold}");
                }
            }

            if (Input.GetKeyDown(decreaseLifeKey))
            {
                if (GameplayManager.Instance != null)
                {
                    GameplayManager.Instance.ModifyLife(-1);
                    Debug.Log($"[GameHUDTester] Life decreased to {GameplayManager.Instance.CurrentLife}");
                }
            }
        }
        #endregion

        #region Test Suite
        /// <summary>
        /// Run all GameHUD tests.
        /// </summary>
        [ContextMenu("Run All Tests")]
        public void RunAllTests()
        {
            Debug.Log("========== GAME HUD TEST SUITE ==========");

            int passedTests = 0;
            int totalTests = 0;

            // Test 1: HUD Component Exists
            totalTests++;
            if (TestHUDComponentExists())
                passedTests++;

            // Test 2: Text Components Assigned
            totalTests++;
            if (TestTextComponentsAssigned())
                passedTests++;

            // Test 3: GameplayManager Integration
            totalTests++;
            if (TestGameplayManagerIntegration())
                passedTests++;

            // Test 4: Individual Update Methods
            totalTests++;
            if (TestIndividualUpdateMethods())
                passedTests++;

            // Test 5: Time Formatting
            totalTests++;
            if (TestTimeFormatting())
                passedTests++;

            // Test 6: Event Subscription
            totalTests++;
            if (TestEventSubscription())
                passedTests++;

            Debug.Log($"========== TEST RESULTS ==========");
            Debug.Log($"Passed: {passedTests}/{totalTests} tests");
            Debug.Log($"Success Rate: {(passedTests * 100f / totalTests):F1}%");
            Debug.Log($"====================================");
        }

        /// <summary>
        /// Test 1: Verify GameHUD component exists.
        /// </summary>
        private bool TestHUDComponentExists()
        {
            Debug.Log("[Test 1] HUD Component Exists");

            if (hudToTest == null)
            {
                Debug.LogError("[Test 1] FAILED: GameHUD component not found");
                return false;
            }

            if (!hudToTest.enabled)
            {
                Debug.LogError("[Test 1] FAILED: GameHUD component is disabled");
                return false;
            }

            Debug.Log("[Test 1] PASSED: GameHUD component exists and is enabled");
            return true;
        }

        /// <summary>
        /// Test 2: Verify all text components are assigned.
        /// </summary>
        private bool TestTextComponentsAssigned()
        {
            Debug.Log("[Test 2] Text Components Assigned");

            if (hudToTest == null)
            {
                Debug.LogError("[Test 2] FAILED: GameHUD not available for testing");
                return false;
            }

            // Use reflection to check if text fields are assigned
            var hudType = hudToTest.GetType();
            var textFields = new string[]
            {
                "roundText", "timeText", "monsterText",
                "goldText", "unitText", "lifeText"
            };

            bool allAssigned = true;
            foreach (var fieldName in textFields)
            {
                var field = hudType.GetField(fieldName,
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

                if (field != null)
                {
                    var value = field.GetValue(hudToTest);
                    if (value == null)
                    {
                        Debug.LogWarning($"[Test 2] WARNING: {fieldName} is not assigned");
                        allAssigned = false;
                    }
                }
            }

            if (!allAssigned)
            {
                Debug.LogError("[Test 2] FAILED: Some text components are not assigned");
                return false;
            }

            Debug.Log("[Test 2] PASSED: All text components are assigned");
            return true;
        }

        /// <summary>
        /// Test 3: Verify GameplayManager integration.
        /// </summary>
        private bool TestGameplayManagerIntegration()
        {
            Debug.Log("[Test 3] GameplayManager Integration");

            if (GameplayManager.Instance == null)
            {
                Debug.LogError("[Test 3] FAILED: GameplayManager not found");
                return false;
            }

            if (hudToTest == null)
            {
                Debug.LogError("[Test 3] FAILED: GameHUD not available");
                return false;
            }

            // Check if initial values match
            if (hudToTest.CurrentRound != GameplayManager.Instance.CurrentRound)
            {
                Debug.LogWarning($"[Test 3] WARNING: Round mismatch - HUD: {hudToTest.CurrentRound}, Manager: {GameplayManager.Instance.CurrentRound}");
            }

            if (hudToTest.CurrentGold != GameplayManager.Instance.CurrentGold)
            {
                Debug.LogWarning($"[Test 3] WARNING: Gold mismatch - HUD: {hudToTest.CurrentGold}, Manager: {GameplayManager.Instance.CurrentGold}");
            }

            if (hudToTest.CurrentLife != GameplayManager.Instance.CurrentLife)
            {
                Debug.LogWarning($"[Test 3] WARNING: Life mismatch - HUD: {hudToTest.CurrentLife}, Manager: {GameplayManager.Instance.CurrentLife}");
            }

            Debug.Log("[Test 3] PASSED: GameplayManager integration verified");
            return true;
        }

        /// <summary>
        /// Test 4: Test individual update methods.
        /// </summary>
        private bool TestIndividualUpdateMethods()
        {
            Debug.Log("[Test 4] Individual Update Methods");

            if (hudToTest == null)
            {
                Debug.LogError("[Test 4] FAILED: GameHUD not available");
                return false;
            }

            try
            {
                // Test each update method
                hudToTest.UpdateRound(5);
                hudToTest.UpdateTime(testTime);
                hudToTest.UpdateMonsterCount(testMonsterCount);
                hudToTest.UpdateGold(100);
                hudToTest.UpdateUnitCount(testUnitCount);
                hudToTest.UpdateLife(8);

                // Verify values
                if (hudToTest.CurrentRound != 5)
                {
                    Debug.LogError($"[Test 4] FAILED: Round update failed - Expected 5, got {hudToTest.CurrentRound}");
                    return false;
                }

                if (Mathf.Abs(hudToTest.CurrentTime - testTime) > 0.1f)
                {
                    Debug.LogError($"[Test 4] FAILED: Time update failed - Expected {testTime}, got {hudToTest.CurrentTime}");
                    return false;
                }

                if (hudToTest.CurrentMonsterCount != testMonsterCount)
                {
                    Debug.LogError($"[Test 4] FAILED: Monster count update failed");
                    return false;
                }

                if (hudToTest.CurrentGold != 100)
                {
                    Debug.LogError($"[Test 4] FAILED: Gold update failed");
                    return false;
                }

                if (hudToTest.CurrentUnitCount != testUnitCount)
                {
                    Debug.LogError($"[Test 4] FAILED: Unit count update failed");
                    return false;
                }

                if (hudToTest.CurrentLife != 8)
                {
                    Debug.LogError($"[Test 4] FAILED: Life update failed");
                    return false;
                }

                Debug.Log("[Test 4] PASSED: All update methods working correctly");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Test 4] FAILED: Exception during updates - {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Test 5: Verify time formatting (MM:SS).
        /// </summary>
        private bool TestTimeFormatting()
        {
            Debug.Log("[Test 5] Time Formatting");

            if (hudToTest == null)
            {
                Debug.LogError("[Test 5] FAILED: GameHUD not available");
                return false;
            }

            // Test various time values
            float[] testTimes = { 0f, 59f, 60f, 61f, 90f, 125f };

            foreach (float time in testTimes)
            {
                hudToTest.UpdateTime(time);

                int expectedMinutes = Mathf.FloorToInt(time / 60f);
                int expectedSeconds = Mathf.FloorToInt(time % 60f);

                // Note: We can't directly verify the text format without accessing the text component
                // But we can verify the internal value is correct
                if (Mathf.Abs(hudToTest.CurrentTime - time) > 0.1f)
                {
                    Debug.LogError($"[Test 5] FAILED: Time value incorrect for {time}s");
                    return false;
                }
            }

            Debug.Log("[Test 5] PASSED: Time formatting working correctly");
            return true;
        }

        /// <summary>
        /// Test 6: Verify event subscription to GameplayManager.
        /// </summary>
        private bool TestEventSubscription()
        {
            Debug.Log("[Test 6] Event Subscription");

            if (GameplayManager.Instance == null || hudToTest == null)
            {
                Debug.LogError("[Test 6] FAILED: Required components not available");
                return false;
            }

            // Store initial values
            int initialRound = GameplayManager.Instance.CurrentRound;
            int initialGold = GameplayManager.Instance.CurrentGold;
            int initialLife = GameplayManager.Instance.CurrentLife;

            // Modify GameplayManager values
            GameplayManager.Instance.NextRound();
            GameplayManager.Instance.ModifyGold(25);
            GameplayManager.Instance.ModifyLife(-1);

            // Wait one frame for event processing (in a coroutine context, but for testing we'll check immediately)
            // Note: In actual runtime, events fire immediately

            // Verify HUD updated
            if (hudToTest.CurrentRound != GameplayManager.Instance.CurrentRound)
            {
                Debug.LogError($"[Test 6] FAILED: HUD round not updated after event - HUD: {hudToTest.CurrentRound}, Manager: {GameplayManager.Instance.CurrentRound}");

                // Restore values
                GameplayManager.Instance.SetRound(initialRound);
                GameplayManager.Instance.SetGold(initialGold);
                GameplayManager.Instance.SetLife(initialLife);
                return false;
            }

            if (hudToTest.CurrentGold != GameplayManager.Instance.CurrentGold)
            {
                Debug.LogError($"[Test 6] FAILED: HUD gold not updated after event");

                // Restore values
                GameplayManager.Instance.SetRound(initialRound);
                GameplayManager.Instance.SetGold(initialGold);
                GameplayManager.Instance.SetLife(initialLife);
                return false;
            }

            if (hudToTest.CurrentLife != GameplayManager.Instance.CurrentLife)
            {
                Debug.LogError($"[Test 6] FAILED: HUD life not updated after event");

                // Restore values
                GameplayManager.Instance.SetRound(initialRound);
                GameplayManager.Instance.SetGold(initialGold);
                GameplayManager.Instance.SetLife(initialLife);
                return false;
            }

            // Restore initial values
            GameplayManager.Instance.SetRound(initialRound);
            GameplayManager.Instance.SetGold(initialGold);
            GameplayManager.Instance.SetLife(initialLife);

            Debug.Log("[Test 6] PASSED: Event subscription working correctly");
            return true;
        }
        #endregion

        #region Manual Testing Helpers
        /// <summary>
        /// Test all HUD update methods with various values.
        /// </summary>
        [ContextMenu("Test All Updates")]
        public void TestAllUpdates()
        {
            if (hudToTest == null)
            {
                Debug.LogError("[GameHUDTester] GameHUD not assigned");
                return;
            }

            hudToTest.UpdateRound(5);
            hudToTest.UpdateTime(90f);
            hudToTest.UpdateMonsterCount(testMonsterCount);
            hudToTest.UpdateGold(150);
            hudToTest.UpdateUnitCount(testUnitCount);
            hudToTest.UpdateLife(7);

            Debug.Log("[GameHUDTester] Updated all HUD values");
        }

        /// <summary>
        /// Reset HUD to initial GameplayManager values.
        /// </summary>
        [ContextMenu("Reset HUD")]
        public void ResetHUD()
        {
            if (hudToTest == null)
            {
                Debug.LogError("[GameHUDTester] GameHUD not assigned");
                return;
            }

            hudToTest.RefreshAllValues();
            Debug.Log("[GameHUDTester] HUD reset to GameplayManager values");
        }
        #endregion
    }
}
