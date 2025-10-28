using UnityEngine;

namespace LottoDefense.Gameplay
{
    /// <summary>
    /// Developer tool for testing GameplayManager state transitions and events.
    /// Attach to any GameObject in the scene to enable keyboard testing.
    /// </summary>
    public class GameplayManagerTester : MonoBehaviour
    {
        [Header("Test Controls Info")]
        [Tooltip("Press keys during Play Mode to test state transitions")]
        [SerializeField] private bool showInstructions = true;

        private void Start()
        {
            // Subscribe to GameplayManager events
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnStateChanged += HandleStateChanged;
                GameplayManager.Instance.OnGameValueChanged += HandleGameValueChanged;

                Debug.Log("=== GameplayManager Tester Active ===");
                Debug.Log("Test Controls:");
                Debug.Log("  [1] Start Countdown");
                Debug.Log("  [2] Change to Preparation");
                Debug.Log("  [3] Change to Combat");
                Debug.Log("  [4] Change to RoundResult");
                Debug.Log("  [5] Change to Victory");
                Debug.Log("  [6] Change to Defeat");
                Debug.Log("  [+] Add 10 Gold");
                Debug.Log("  [-] Remove 1 Life");
                Debug.Log("  [N] Next Round");
                Debug.Log("====================================");
            }
            else
            {
                Debug.LogError("[GameplayManagerTester] GameplayManager not found!");
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnStateChanged -= HandleStateChanged;
                GameplayManager.Instance.OnGameValueChanged -= HandleGameValueChanged;
            }
        }

        private void Update()
        {
            if (GameplayManager.Instance == null) return;

            // State transition tests
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Debug.Log("[Tester] Starting Countdown...");
                GameplayManager.Instance.StartCountdown();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Debug.Log("[Tester] Changing to Preparation...");
                GameplayManager.Instance.ChangeState(GameState.Preparation);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Debug.Log("[Tester] Changing to Combat...");
                GameplayManager.Instance.ChangeState(GameState.Combat);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                Debug.Log("[Tester] Changing to RoundResult...");
                GameplayManager.Instance.ChangeState(GameState.RoundResult);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                Debug.Log("[Tester] Changing to Victory...");
                GameplayManager.Instance.ChangeState(GameState.Victory);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                Debug.Log("[Tester] Changing to Defeat...");
                GameplayManager.Instance.ChangeState(GameState.Defeat);
            }

            // Game value tests
            else if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                Debug.Log("[Tester] Adding 10 Gold...");
                GameplayManager.Instance.ModifyGold(10);
            }
            else if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                Debug.Log("[Tester] Removing 1 Life...");
                GameplayManager.Instance.ModifyLife(-1);
            }
            else if (Input.GetKeyDown(KeyCode.N))
            {
                Debug.Log("[Tester] Advancing to Next Round...");
                GameplayManager.Instance.NextRound();
            }

            // Status display
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                DisplayCurrentStatus();
            }
        }

        /// <summary>
        /// Event handler for state changes.
        /// </summary>
        private void HandleStateChanged(GameState oldState, GameState newState)
        {
            Debug.Log($"<color=green>[Event] State Changed: {oldState} → {newState}</color>");
        }

        /// <summary>
        /// Event handler for game value changes.
        /// </summary>
        private void HandleGameValueChanged(string valueType, int newValue)
        {
            Debug.Log($"<color=cyan>[Event] {valueType} Changed: {newValue}</color>");
        }

        /// <summary>
        /// Display current game status.
        /// </summary>
        private void DisplayCurrentStatus()
        {
            Debug.Log("=== Current Game Status ===");
            Debug.Log($"State: {GameplayManager.Instance.CurrentState}");
            Debug.Log($"Round: {GameplayManager.Instance.CurrentRound}");
            Debug.Log($"Life: {GameplayManager.Instance.CurrentLife}");
            Debug.Log($"Gold: {GameplayManager.Instance.CurrentGold}");
            Debug.Log("==========================");
        }

        // Display instructions in Inspector
        private void OnValidate()
        {
            if (showInstructions)
            {
                Debug.Log("GameplayManagerTester: Check the Start() method for keyboard controls.");
            }
        }
    }
}
