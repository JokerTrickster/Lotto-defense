using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using LottoDefense.Units;
using LottoDefense.Monsters;
using LottoDefense.Gameplay;

namespace LottoDefense.Combat
{
    /// <summary>
    /// Singleton manager orchestrating all combat activity during Combat phase.
    /// Coordinates auto-targeting and attacks between units and monsters with tick-based updates.
    /// </summary>
    public class CombatManager : MonoBehaviour
    {
        #region Singleton
        private static CombatManager _instance;

        /// <summary>
        /// Global access point for the CombatManager singleton.
        /// </summary>
        public static CombatManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<CombatManager>();

                    if (_instance == null)
                    {
                        GameObject go = new GameObject("CombatManager");
                        _instance = go.AddComponent<CombatManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Constants
        private const float COMBAT_TICK_INTERVAL = 0.1f; // 10 updates per second
        #endregion

        #region Inspector Fields
        [Header("Combat Settings")]
        [Tooltip("Combat update interval in seconds (default: 0.1 = 10 ticks/sec)")]
        [SerializeField] private float combatTickInterval = COMBAT_TICK_INTERVAL;
        #endregion

        #region Private Fields
        private Coroutine combatCoroutine;
        private bool isCombatActive = false;
        private int combatTickCount = 0;
        #endregion

        #region Properties
        /// <summary>
        /// Whether combat is currently active.
        /// </summary>
        public bool IsCombatActive => isCombatActive;

        /// <summary>
        /// Total number of combat ticks processed this session.
        /// </summary>
        public int CombatTickCount => combatTickCount;
        #endregion

        #region Events
        /// <summary>
        /// Fired when combat starts.
        /// </summary>
        public event Action OnCombatStarted;

        /// <summary>
        /// Fired when combat stops.
        /// </summary>
        public event Action OnCombatStopped;

        /// <summary>
        /// Fired each combat tick.
        /// </summary>
        public event Action OnCombatTick;

        /// <summary>
        /// Fired when a unit attacks a monster.
        /// Parameters: Unit, Monster, damage dealt
        /// </summary>
        public event Action<Unit, Monster, int> OnUnitAttack;

        /// <summary>
        /// Fired when a monster takes damage.
        /// Parameters: Monster, damage amount
        /// </summary>
        public event Action<Monster, int> OnMonsterDamaged;
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

        private void OnEnable()
        {
            // Subscribe to game state changes
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnStateChanged += HandleStateChanged;
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from game state changes
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnStateChanged -= HandleStateChanged;
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize combat manager.
        /// </summary>
        private void Initialize()
        {
            Debug.Log("[CombatManager] Initialized");
        }
        #endregion

        #region State Management
        /// <summary>
        /// Handle game state changes.
        /// </summary>
        private void HandleStateChanged(GameState oldState, GameState newState)
        {
            Debug.Log($"[CombatManager] State changed: {oldState} -> {newState}");

            switch (newState)
            {
                case GameState.Combat:
                    StartCombat();
                    break;

                case GameState.Preparation:
                case GameState.RoundResult:
                case GameState.Victory:
                case GameState.Defeat:
                    StopCombat();
                    break;
            }
        }
        #endregion

        #region Combat Control
        /// <summary>
        /// Start combat tick system.
        /// </summary>
        public void StartCombat()
        {
            if (isCombatActive)
            {
                Debug.LogWarning("[CombatManager] Combat already active!");
                return;
            }

            isCombatActive = true;
            combatTickCount = 0;

            combatCoroutine = StartCoroutine(CombatUpdateRoutine());

            OnCombatStarted?.Invoke();

            Debug.Log("[CombatManager] Combat started");
        }

        /// <summary>
        /// Stop combat tick system.
        /// </summary>
        public void StopCombat()
        {
            if (!isCombatActive)
            {
                return;
            }

            if (combatCoroutine != null)
            {
                StopCoroutine(combatCoroutine);
                combatCoroutine = null;
            }

            isCombatActive = false;

            OnCombatStopped?.Invoke();

            Debug.Log($"[CombatManager] Combat stopped (total ticks: {combatTickCount})");
        }
        #endregion

        #region Combat Loop
        /// <summary>
        /// Main combat update coroutine running at fixed tick rate.
        /// </summary>
        private IEnumerator CombatUpdateRoutine()
        {
            while (isCombatActive)
            {
                yield return new WaitForSeconds(combatTickInterval);

                // Process combat tick
                ProcessCombatTick();
            }
        }

        /// <summary>
        /// Process a single combat tick.
        /// </summary>
        private void ProcessCombatTick()
        {
            combatTickCount++;

            // Get all active units and monsters
            List<Unit> activeUnits = GetActiveUnits();
            List<Monster> activeMonsters = GetActiveMonsters();

            // Early exit if no units or monsters
            if (activeUnits.Count == 0 || activeMonsters.Count == 0)
            {
                return;
            }

            // Process each unit's combat behavior
            foreach (Unit unit in activeUnits)
            {
                if (unit == null || unit.Data == null)
                    continue;

                // Execute unit combat tick
                unit.CombatTick();

                // Subscribe to attack events if not already subscribed
                unit.OnAttack -= HandleUnitAttack;
                unit.OnAttack += HandleUnitAttack;
            }

            // Fire tick event
            OnCombatTick?.Invoke();
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handle unit attack event.
        /// </summary>
        private void HandleUnitAttack(Unit unit, Monster target, int damage)
        {
            if (unit == null || target == null)
                return;

            // Fire combat manager attack event
            OnUnitAttack?.Invoke(unit, target, damage);

            // Fire monster damaged event
            OnMonsterDamaged?.Invoke(target, damage);
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Get all active units from UnitManager.
        /// </summary>
        private List<Unit> GetActiveUnits()
        {
            if (UnitManager.Instance == null)
                return new List<Unit>();

            return UnitManager.Instance.GetPlacedUnits();
        }

        /// <summary>
        /// Get all active monsters in the scene.
        /// </summary>
        private List<Monster> GetActiveMonsters()
        {
            List<Monster> activeMonsters = new List<Monster>();

            // Find all monsters in scene
            Monster[] allMonsters = FindObjectsByType<Monster>(FindObjectsSortMode.None);

            foreach (Monster monster in allMonsters)
            {
                if (monster.IsActive)
                {
                    activeMonsters.Add(monster);
                }
            }

            return activeMonsters;
        }
        #endregion

        #region Debugging
        /// <summary>
        /// Get combat manager statistics for debugging.
        /// </summary>
        public string GetStats()
        {
            int unitCount = GetActiveUnits().Count;
            int monsterCount = GetActiveMonsters().Count;

            return $"Combat: {(isCombatActive ? "Active" : "Inactive")}, " +
                   $"Ticks: {combatTickCount}, " +
                   $"Units: {unitCount}, " +
                   $"Monsters: {monsterCount}";
        }
        #endregion
    }
}
