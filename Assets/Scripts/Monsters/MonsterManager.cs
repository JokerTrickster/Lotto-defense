using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using LottoDefense.Grid;
using LottoDefense.Gameplay;
using LottoDefense.UI;

namespace LottoDefense.Monsters
{
    /// <summary>
    /// Path type for monster spawning.
    /// </summary>
    public enum PathType
    {
        Top,
        Bottom,
        /// <summary>Monsters run in a loop around the square (grid boundary).</summary>
        SquareLoop
    }

    /// <summary>
    /// Singleton manager controlling monster spawning, lifecycle, and integration with game systems.
    /// Manages object pooling and coordinates with GameplayManager and GridManager.
    /// </summary>
    public class MonsterManager : MonoBehaviour
    {
        #region Singleton
        private static MonsterManager _instance;

        /// <summary>
        /// Global access point for the MonsterManager singleton.
        /// </summary>
        public static MonsterManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<MonsterManager>();

                    if (_instance == null)
                    {
                        GameObject go = new GameObject("MonsterManager");
                        _instance = go.AddComponent<MonsterManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Constants
        private const float SPAWN_INTERVAL = 0.5f; // 2 per second
        private const int MAX_SPAWNS_PER_ROUND = 30; // 30 monsters per round
        private const float SPAWN_DURATION = 15f; // Spawn for 15 seconds only
        private const int MAX_ACTIVE_MONSTERS = 100; // Game over when active monsters exceed this
        #endregion

        #region Inspector Fields
        [Header("Monster Data")]
        [Tooltip("Pool of monster data to randomly spawn from")]
        [SerializeField] private MonsterData[] monsterDataPool;

        [Header("Spawn Settings")]
        [Tooltip("Spawn rate in seconds (0.5 = 2 per second)")]
        [SerializeField] private float spawnInterval = SPAWN_INTERVAL;

        [Tooltip("Maximum monsters to spawn per round")]
        [SerializeField] private int maxSpawnsPerRound = MAX_SPAWNS_PER_ROUND;

        [Tooltip("Only spawn for this many seconds per round (e.g. 15s = 30 monsters at 2/sec)")]
        [SerializeField] private float spawnDuration = SPAWN_DURATION;

        [Header("Game Over Settings")]
        [Tooltip("Game over when active monsters exceed this count")]
        [SerializeField] private int maxActiveMonsters = MAX_ACTIVE_MONSTERS;
        #endregion

        #region Private Fields
        private MonsterPool monsterPool;
        private bool isSpawning = false;
        private int monstersSpawnedThisRound = 0;
        private Coroutine spawnCoroutine;
        private GameHUD cachedGameHUD;
        #endregion

        #region Properties
        /// <summary>
        /// Number of monsters currently alive.
        /// </summary>
        public int ActiveMonsterCount => monsterPool != null ? monsterPool.ActiveCount : 0;

        /// <summary>
        /// Whether spawning is currently active.
        /// </summary>
        public bool IsSpawning => isSpawning;
        #endregion

        #region Events
        /// <summary>
        /// Fired when a monster is spawned.
        /// </summary>
        public event Action<Monster, PathType> OnMonsterSpawned;

        /// <summary>
        /// Fired when a monster dies.
        /// </summary>
        public event Action<Monster> OnMonsterDied;

        /// <summary>
        /// Fired when a monster reaches the end of its path.
        /// </summary>
        public event Action<Monster> OnMonsterReachedEnd;

        /// <summary>
        /// Fired when all monsters for a round have been spawned and defeated/escaped.
        /// </summary>
        public event Action OnRoundComplete;

        /// <summary>
        /// Fired when monster count exceeds limit and triggers game over.
        /// </summary>
        public event Action OnMonsterOverflow;
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
        /// Initialize monster manager and pool.
        /// </summary>
        private void Initialize()
        {
            // Create monster pool
            GameObject poolObj = new GameObject("MonsterPool");
            poolObj.transform.SetParent(transform);
            monsterPool = poolObj.AddComponent<MonsterPool>();
            monsterPool.Initialize();

            Debug.Log("[MonsterManager] Initialized");
        }
        #endregion

        #region State Management
        /// <summary>
        /// Handle game state changes.
        /// </summary>
        private void HandleStateChanged(GameState oldState, GameState newState)
        {
            Debug.Log($"[MonsterManager] State changed: {oldState} -> {newState}");

            switch (newState)
            {
                case GameState.Combat:
                    StartSpawning();
                    break;

                case GameState.Preparation:
                case GameState.RoundResult:
                case GameState.Victory:
                case GameState.Defeat:
                    StopSpawning();
                    break;
            }
        }
        #endregion

        #region Spawning Control
        /// <summary>
        /// Start monster spawning for the current round.
        /// </summary>
        public void StartSpawning()
        {
            if (isSpawning)
            {
                Debug.LogWarning("[MonsterManager] Already spawning!");
                return;
            }

            if (monsterDataPool == null || monsterDataPool.Length == 0)
            {
                Debug.LogError("[MonsterManager] No monster data configured!");
                return;
            }

            monstersSpawnedThisRound = 0;
            isSpawning = true;

            spawnCoroutine = StartCoroutine(SpawnRoutine());

            Debug.Log($"[MonsterManager] Started spawning for round {GameplayManager.Instance.CurrentRound}");
        }

        /// <summary>
        /// Stop monster spawning.
        /// </summary>
        public void StopSpawning()
        {
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }

            isSpawning = false;

            Debug.Log("[MonsterManager] Stopped spawning");
        }

        /// <summary>
        /// Coroutine that handles timed monster spawning.
        /// Spawns at most maxSpawnsPerRound and only for spawnDuration seconds (e.g. 15s = 30 at 2/sec).
        /// </summary>
        private IEnumerator SpawnRoutine()
        {
            float elapsed = 0f;

            while (monstersSpawnedThisRound < maxSpawnsPerRound && elapsed < spawnDuration)
            {
                yield return new WaitForSeconds(spawnInterval);
                elapsed += spawnInterval;

                MonsterData data = SelectRandomMonster();
                SpawnMonster(data, PathType.SquareLoop);

                monstersSpawnedThisRound++;
            }

            isSpawning = false;
            Debug.Log($"[MonsterManager] Finished spawning {monstersSpawnedThisRound} monsters (elapsed {elapsed:F1}s)");

            CheckRoundComplete();
        }
        #endregion

        #region Monster Lifecycle
        /// <summary>
        /// Spawn a monster on a specific path.
        /// </summary>
        /// <param name="data">Monster data template</param>
        /// <param name="pathType">Path to spawn on</param>
        public void SpawnMonster(MonsterData data, PathType pathType)
        {
            if (data == null)
            {
                Debug.LogError("[MonsterManager] Cannot spawn monster with null data!");
                return;
            }

            // Get monster from pool
            Monster monster = monsterPool.GetMonster();
            if (monster == null)
            {
                Debug.LogError("[MonsterManager] Failed to get monster from pool!");
                return;
            }

            List<Vector3> waypoints = GetPathWaypoints(pathType);
            if (waypoints == null || waypoints.Count == 0)
            {
                Debug.LogError($"[MonsterManager] No waypoints for path: {pathType}");
                monsterPool.ReturnMonster(monster);
                return;
            }

            int currentRound = GameplayManager.Instance != null ? GameplayManager.Instance.CurrentRound : 1;
            bool isLoopingPath = (pathType == PathType.SquareLoop);
            monster.Initialize(data, waypoints, currentRound, isLoopingPath);

            // Subscribe to monster events
            monster.OnDeath += HandleMonsterDeath;
            monster.OnReachEnd += HandleMonsterReachEnd;

            OnMonsterSpawned?.Invoke(monster, pathType);

            // Update HUD
            UpdateMonsterCountHUD();

            // Check for monster overflow (game over condition)
            CheckMonsterOverflow();

            Debug.Log($"[MonsterManager] Spawned {data.monsterName} on {pathType} path (HP: {monster.CurrentHealth}, Active: {ActiveMonsterCount}/{maxActiveMonsters})");
        }

        /// <summary>
        /// Check if monster count exceeds limit and trigger game over.
        /// </summary>
        private void CheckMonsterOverflow()
        {
            if (ActiveMonsterCount > maxActiveMonsters)
            {
                Debug.LogWarning($"[MonsterManager] Monster overflow! {ActiveMonsterCount} > {maxActiveMonsters}");

                StopSpawning();
                OnMonsterOverflow?.Invoke();

                // Trigger defeat
                if (GameplayManager.Instance != null)
                {
                    GameplayManager.Instance.ChangeState(GameState.Defeat);
                }
            }
        }

        /// <summary>
        /// Handle monster death event.
        /// </summary>
        private void HandleMonsterDeath(Monster monster)
        {
            if (monster == null)
                return;

            Debug.Log($"[MonsterManager] Monster died: {monster.Data.monsterName}");

            // Award gold
            if (GameplayManager.Instance != null)
            {
                int goldReward = monster.GetGoldReward();
                GameplayManager.Instance.ModifyGold(goldReward);
                Debug.Log($"[MonsterManager] Awarded {goldReward} gold");
            }

            // Unsubscribe from events
            monster.OnDeath -= HandleMonsterDeath;
            monster.OnReachEnd -= HandleMonsterReachEnd;

            // Fire event
            OnMonsterDied?.Invoke(monster);

            // Return to pool
            monsterPool.ReturnMonster(monster);

            // Update HUD
            UpdateMonsterCountHUD();

            // Check if round complete
            CheckRoundComplete();
        }

        /// <summary>
        /// Handle monster reaching end of path.
        /// </summary>
        private void HandleMonsterReachEnd(Monster monster)
        {
            if (monster == null)
                return;

            Debug.Log($"[MonsterManager] Monster reached end: {monster.Data.monsterName}");

            // Decrease player life
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.ModifyLife(-1);
                Debug.Log("[MonsterManager] Player life decreased");
            }

            // Unsubscribe from events
            monster.OnDeath -= HandleMonsterDeath;
            monster.OnReachEnd -= HandleMonsterReachEnd;

            // Fire event
            OnMonsterReachedEnd?.Invoke(monster);

            // Return to pool
            monsterPool.ReturnMonster(monster);

            // Update HUD
            UpdateMonsterCountHUD();

            // Check if round complete
            CheckRoundComplete();
        }

        /// <summary>
        /// Destroy/remove a monster from the game.
        /// </summary>
        /// <param name="monster">Monster to destroy</param>
        public void DestroyMonster(Monster monster)
        {
            if (monster == null)
                return;

            // Unsubscribe from events
            monster.OnDeath -= HandleMonsterDeath;
            monster.OnReachEnd -= HandleMonsterReachEnd;

            // Return to pool
            monsterPool.ReturnMonster(monster);
        }
        #endregion

        #region Round Management
        /// <summary>
        /// Check if the round is complete (all monsters spawned and cleared).
        /// </summary>
        private void CheckRoundComplete()
        {
            // Round is complete when spawning finished and no active monsters
            if (!isSpawning && ActiveMonsterCount == 0 && monstersSpawnedThisRound > 0)
            {
                Debug.Log("[MonsterManager] Round complete - all monsters cleared");
                OnRoundComplete?.Invoke();
            }
        }

        /// <summary>
        /// Clear all active monsters (for round cleanup).
        /// </summary>
        public void ClearAllMonsters()
        {
            if (monsterPool != null)
            {
                monsterPool.ClearAll();
            }

            Debug.Log("[MonsterManager] All monsters cleared");
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Select a random monster from the pool.
        /// </summary>
        private MonsterData SelectRandomMonster()
        {
            if (monsterDataPool == null || monsterDataPool.Length == 0)
                return null;

            int randomIndex = UnityEngine.Random.Range(0, monsterDataPool.Length);
            return monsterDataPool[randomIndex];
        }

        /// <summary>
        /// Get waypoints for a specific path type.
        /// </summary>
        private List<Vector3> GetPathWaypoints(PathType pathType)
        {
            if (GridManager.Instance == null)
            {
                Debug.LogError("[MonsterManager] GridManager not found!");
                return null;
            }

            switch (pathType)
            {
                case PathType.Top:
                    return GridManager.Instance.GetTopPathWaypoints();
                case PathType.Bottom:
                    return GridManager.Instance.GetBottomPathWaypoints();
                case PathType.SquareLoop:
                    return GridManager.Instance.GetSquareLoopWaypoints();
                default:
                    return GridManager.Instance.GetSquareLoopWaypoints();
            }
        }
        #endregion

        #region UI Integration
        /// <summary>
        /// Get cached GameHUD reference.
        /// </summary>
        private GameHUD GetGameHUD()
        {
            if (cachedGameHUD == null)
            {
                cachedGameHUD = FindFirstObjectByType<GameHUD>();
            }
            return cachedGameHUD;
        }

        /// <summary>
        /// Update GameHUD with current monster count.
        /// </summary>
        private void UpdateMonsterCountHUD()
        {
            GameHUD hud = GetGameHUD();
            if (hud != null)
            {
                hud.UpdateMonsterCount(ActiveMonsterCount);
            }
        }
        #endregion

        #region Active Monsters Query
        /// <summary>
        /// Get list of all currently active monsters.
        /// Used by CombatManager for efficient combat processing.
        /// </summary>
        /// <returns>List of active Monster components</returns>
        public List<Monster> GetActiveMonstersList()
        {
            if (monsterPool == null)
                return new List<Monster>();

            return monsterPool.GetActiveMonsters();
        }
        #endregion

        #region Debugging
        /// <summary>
        /// Get current monster manager statistics.
        /// </summary>
        public string GetStats()
        {
            if (monsterPool == null)
                return "Pool not initialized";

            return $"Spawning: {isSpawning}, Spawned this round: {monstersSpawnedThisRound}/{maxSpawnsPerRound}, {monsterPool.GetPoolStats()}";
        }
        #endregion
    }
}
