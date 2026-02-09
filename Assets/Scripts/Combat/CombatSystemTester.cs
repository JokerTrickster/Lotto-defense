using UnityEngine;
using LottoDefense.Combat;
using LottoDefense.Units;
using LottoDefense.Monsters;
using LottoDefense.Gameplay;

namespace LottoDefense.Combat
{
    /// <summary>
    /// Test script for combat system functionality.
    /// Allows manual testing and debugging of combat mechanics in Unity Editor.
    /// </summary>
    public class CombatSystemTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [Tooltip("Unit data to spawn for testing")]
        [SerializeField] private UnitData testUnitData;

        [Tooltip("Test unit placement positions")]
        [SerializeField] private Vector2Int[] testUnitPositions = new Vector2Int[]
        {
            new Vector2Int(1, 5),
            new Vector2Int(4, 5),
            new Vector2Int(2, 3),
            new Vector2Int(3, 7)
        };

        [Header("Combat Statistics")]
        [SerializeField] private bool showStats = true;
        [SerializeField] private float statsUpdateInterval = 1f;

        private float lastStatsUpdate = 0f;

        #region Unity Lifecycle
        private void Start()
        {
            // Subscribe to combat events for debugging
            if (CombatManager.Instance != null)
            {
                CombatManager.Instance.OnCombatStarted += HandleCombatStarted;
                CombatManager.Instance.OnCombatStopped += HandleCombatStopped;
                CombatManager.Instance.OnCombatTick += HandleCombatTick;
                // CombatManager.Instance.OnUnitAttack += HandleUnitAttack; // Commented out - event removed from CombatManager
                CombatManager.Instance.OnMonsterDamaged += HandleMonsterDamaged;
            }
        }

        private void Update()
        {
            // Show stats periodically
            if (showStats && Time.time - lastStatsUpdate > statsUpdateInterval)
            {
                PrintStats();
                lastStatsUpdate = Time.time;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from combat events
            if (CombatManager.Instance != null)
            {
                CombatManager.Instance.OnCombatStarted -= HandleCombatStarted;
                CombatManager.Instance.OnCombatStopped -= HandleCombatStopped;
                CombatManager.Instance.OnCombatTick -= HandleCombatTick;
                // CombatManager.Instance.OnUnitAttack -= HandleUnitAttack; // Commented out - event removed from CombatManager
                CombatManager.Instance.OnMonsterDamaged -= HandleMonsterDamaged;
            }
        }
        #endregion

        #region Test Controls
        /// <summary>
        /// Spawn test units at predefined positions.
        /// </summary>
        [ContextMenu("Spawn Test Units")]
        public void SpawnTestUnits()
        {
            if (testUnitData == null)
            {
                Debug.LogError("[CombatSystemTester] No test unit data assigned!");
                return;
            }

            if (UnitManager.Instance == null)
            {
                Debug.LogError("[CombatSystemTester] UnitManager not found!");
                return;
            }

            foreach (Vector2Int pos in testUnitPositions)
            {
                Unit unit = UnitManager.Instance.PlaceUnit(testUnitData, pos);
                if (unit != null)
                {
                    Debug.Log($"[CombatSystemTester] Spawned test unit at {pos}");
                }
            }
        }

        /// <summary>
        /// Clear all test units from the grid.
        /// </summary>
        [ContextMenu("Clear Test Units")]
        public void ClearTestUnits()
        {
            if (UnitManager.Instance != null)
            {
                UnitManager.Instance.ClearAllUnits();
                Debug.Log("[CombatSystemTester] Cleared all test units");
            }
        }

        /// <summary>
        /// Start combat manually.
        /// </summary>
        [ContextMenu("Start Combat")]
        public void StartCombat()
        {
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.ChangeState(GameState.Combat);
                Debug.Log("[CombatSystemTester] Started combat");
            }
        }

        /// <summary>
        /// Stop combat manually.
        /// </summary>
        [ContextMenu("Stop Combat")]
        public void StopCombat()
        {
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.ChangeState(GameState.Preparation);
                Debug.Log("[CombatSystemTester] Stopped combat");
            }
        }

        /// <summary>
        /// Print current combat statistics.
        /// </summary>
        [ContextMenu("Print Stats")]
        public void PrintStats()
        {
            Debug.Log("=== COMBAT SYSTEM STATS ===");

            if (CombatManager.Instance != null)
            {
                Debug.Log($"Combat: {CombatManager.Instance.GetStats()}");
            }

            if (UnitManager.Instance != null)
            {
                Debug.Log($"Units: {UnitManager.Instance.GetStats()}");
            }

            if (MonsterManager.Instance != null)
            {
                Debug.Log($"Monsters: {MonsterManager.Instance.GetStats()}");
            }

            if (GameplayManager.Instance != null)
            {
                Debug.Log($"Game: Round {GameplayManager.Instance.CurrentRound}, " +
                         $"Gold: {GameplayManager.Instance.CurrentGold}, " +
                         $"Life: {GameplayManager.Instance.CurrentLife}");
            }
        }
        #endregion

        #region Event Handlers
        private void HandleCombatStarted()
        {
            Debug.Log("[CombatSystemTester] *** COMBAT STARTED ***");
        }

        private void HandleCombatStopped()
        {
            Debug.Log("[CombatSystemTester] *** COMBAT STOPPED ***");
        }

        private void HandleCombatTick()
        {
            // Optionally log each tick (can be very verbose)
            // Debug.Log($"[CombatSystemTester] Combat tick: {CombatManager.Instance.CombatTickCount}");
        }

        private void HandleUnitAttack(Unit unit, Monster target, int damage)
        {
            Debug.Log($"[CombatSystemTester] ⚔️ {unit.Data.unitName} attacked {target.Data.monsterName} for {damage} damage");
        }

        private void HandleMonsterDamaged(Monster monster, int damage)
        {
            Debug.Log($"[CombatSystemTester] 💥 {monster.Data.monsterName} took {damage} damage (HP: {monster.CurrentHealth}/{monster.MaxHealth})");
        }
        #endregion

        #region GUI
        private void OnGUI()
        {
            if (!showStats)
                return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Box("Combat System Tester");

            if (GUILayout.Button("Spawn Test Units"))
            {
                SpawnTestUnits();
            }

            if (GUILayout.Button("Clear Test Units"))
            {
                ClearTestUnits();
            }

            GUILayout.Space(10);

            if (CombatManager.Instance != null && !CombatManager.Instance.IsCombatActive)
            {
                if (GUILayout.Button("Start Combat"))
                {
                    StartCombat();
                }
            }
            else if (CombatManager.Instance != null && CombatManager.Instance.IsCombatActive)
            {
                if (GUILayout.Button("Stop Combat"))
                {
                    StopCombat();
                }
            }

            GUILayout.Space(10);

            if (CombatManager.Instance != null)
            {
                GUILayout.Label($"Combat Ticks: {CombatManager.Instance.CombatTickCount}");
            }

            if (UnitManager.Instance != null)
            {
                GUILayout.Label($"Placed Units: {UnitManager.Instance.PlacedUnitCount}");
            }

            if (MonsterManager.Instance != null)
            {
                GUILayout.Label($"Active Monsters: {MonsterManager.Instance.ActiveMonsterCount}");
            }

            GUILayout.EndArea();
        }
        #endregion
    }
}
