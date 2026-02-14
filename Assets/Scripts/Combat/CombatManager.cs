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
                if (GameplayManager.IsCleaningUp) return null;

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
        private Coroutine subscribeCoroutine;
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
        // public event Action<Unit, Monster, int> OnUnitAttack; // Unused - commented out to avoid CS0067 warning

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
            subscribeCoroutine = StartCoroutine(SubscribeWhenReady());
        }

        private IEnumerator SubscribeWhenReady()
        {
            int maxRetries = 300;
            int retries = 0;
            while (GameplayManager.Instance == null && retries < maxRetries)
            {
                retries++;
                yield return null;
            }
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnStateChanged -= HandleStateChanged;
                GameplayManager.Instance.OnStateChanged += HandleStateChanged;
                Debug.Log("[CombatManager] Subscribed to GameplayManager state changes");
            }
        }

        private void OnDisable()
        {
            if (subscribeCoroutine != null)
            {
                StopCoroutine(subscribeCoroutine);
                subscribeCoroutine = null;
            }
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

            // Subscribe to unit attack events
            SubscribeToUnitEvents();

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

            // Unsubscribe from unit events
            UnsubscribeFromUnitEvents();

            // Reset unit combat states
            ResetAllUnitCombatStates();

            isCombatActive = false;

            OnCombatStopped?.Invoke();

            Debug.Log($"[CombatManager] Combat stopped (total ticks: {combatTickCount})");
        }

        /// <summary>
        /// Reset combat state for all placed units.
        /// </summary>
        private void ResetAllUnitCombatStates()
        {
            List<Unit> units = GetActiveUnits();
            foreach (Unit unit in units)
            {
                if (unit != null)
                {
                    unit.ResetCombat();
                }
            }
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

            // Debug: Log tick info every 10 ticks
            if (combatTickCount % 10 == 1)
            {
                Debug.Log($"[CombatManager] Tick #{combatTickCount}: Units={activeUnits.Count}, Monsters={activeMonsters.Count}");
            }

            // Early exit if no units or monsters
            if (activeUnits.Count == 0 || activeMonsters.Count == 0)
            {
                if (combatTickCount == 1)
                {
                    Debug.LogWarning($"[CombatManager] No combat participants! Units={activeUnits.Count}, Monsters={activeMonsters.Count}");
                }
                return;
            }

            // Process each unit's combat behavior
            foreach (Unit unit in activeUnits)
            {
                if (unit == null || unit.Data == null)
                    continue;

                // Execute unit combat tick
                unit.CombatTick();
            }

            // Fire tick event
            OnCombatTick?.Invoke();
        }
        #endregion

        #region Event Subscription
        /// <summary>
        /// Subscribe to all placed units' attack events when combat starts.
        /// </summary>
        private void SubscribeToUnitEvents()
        {
            List<Unit> units = GetActiveUnits();
            foreach (Unit unit in units)
            {
                if (unit != null)
                {
                    unit.OnAttack -= OnUnitAttacked;
                    unit.OnAttack += OnUnitAttacked;
                }
            }
        }

        /// <summary>
        /// Unsubscribe from all unit events when combat stops.
        /// </summary>
        private void UnsubscribeFromUnitEvents()
        {
            List<Unit> units = GetActiveUnits();
            foreach (Unit unit in units)
            {
                if (unit != null)
                {
                    unit.OnAttack -= OnUnitAttacked;
                }
            }
        }

        /// <summary>
        /// Handle unit attack event callback.
        /// </summary>
        private void OnUnitAttacked(Monster target, int damage)
        {
            if (target == null) return;

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
        /// Get all active monsters from MonsterManager.
        /// Uses MonsterManager's pool tracking instead of expensive scene search.
        /// </summary>
        private List<Monster> GetActiveMonsters()
        {
            if (MonsterManager.Instance == null)
                return new List<Monster>();

            // Use MonsterManager's internal tracking for efficiency
            return MonsterManager.Instance.GetActiveMonstersList();
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
