using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using LottoDefense.Grid;
using LottoDefense.Gameplay;
using LottoDefense.UI;
using LottoDefense.VFX;

namespace LottoDefense.Monsters
{
    /// <summary>
    /// Path type for monster spawning.
    /// </summary>
    public enum PathType
    {
        /// <summary>Monsters run in a loop around the grid boundary (only path type used).</summary>
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
                if (GameplayManager.IsCleaningUp) return null;

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
        private const int BOSS_ROUND = 5; // Boss appears on round 5
        private const float BOSS_SPAWN_DELAY = 2f; // Delay before boss spawn for dramatic effect
        #endregion

        #region Inspector Fields
        [Header("Round Configuration")]
        [Tooltip("라운드별 몬스터 설정 (RoundConfig 사용 권장)")]
        [SerializeField] private RoundConfig roundConfig;

        [Header("Fallback Monster Data (RoundConfig 없을 때)")]
        [Tooltip("Pool of monster data to randomly spawn from")]
        [SerializeField] private MonsterData[] monsterDataPool;

        [Header("Fallback Spawn Settings (RoundConfig 없을 때)")]
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
        private Coroutine subscribeCoroutine;
        private GameHUD cachedGameHUD;
        private MonsterData currentRoundMonsterType; // 현재 라운드에서 스폰할 몬스터 타입 (1종류만)
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
        /// Initialize monster manager and pool.
        /// </summary>
        private void Initialize()
        {
            // Create monster pool
            GameObject poolObj = new GameObject("MonsterPool");
            poolObj.transform.SetParent(transform);
            monsterPool = poolObj.AddComponent<MonsterPool>();
            monsterPool.Initialize();

        }
        #endregion

        #region State Management
        /// <summary>
        /// Handle game state changes.
        /// </summary>
        private void HandleStateChanged(GameState oldState, GameState newState)
        {

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
        /// Uses RoundConfig if available, otherwise selects ONE random monster type.
        /// </summary>
        public void StartSpawning()
        {
            if (isSpawning)
            {
                Debug.LogWarning("[MonsterManager] Already spawning!");
                return;
            }

            monstersSpawnedThisRound = 0;
            isSpawning = true;

            int currentRound = GameplayManager.Instance != null ? GameplayManager.Instance.CurrentRound : 1;

            // RoundConfig가 있으면 사용
            if (roundConfig != null)
            {
                RoundMonsterConfig config = roundConfig.GetRoundConfig(currentRound);
                currentRoundMonsterType = config.monsterData;
                maxSpawnsPerRound = config.totalMonsters;
                spawnInterval = config.spawnInterval;
                spawnDuration = config.spawnDuration;

            }
            // RoundConfig가 없으면 기존 방식 (랜덤)
            else
            {
                if (monsterDataPool == null || monsterDataPool.Length == 0)
                {
                    Debug.LogError("[MonsterManager] No monster data configured!");
                    return;
                }

                // 라운드 시작 시 1개 몬스터 타입 선택 (이 라운드 동안 이 타입만 스폰)
                currentRoundMonsterType = SelectRandomMonster();
            }

            spawnCoroutine = StartCoroutine(SpawnRoutine());
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

        }

        /// <summary>
        /// Coroutine that handles timed monster spawning.
        /// Spawns at most maxSpawnsPerRound and only for spawnDuration seconds (e.g. 15s = 30 at 2/sec).
        /// All monsters in a round are the same type (selected at round start).
        /// Boss round (round 5): Shows boss entrance effect and spawns 1 boss monster.
        /// </summary>
        private IEnumerator SpawnRoutine()
        {
            int currentRound = GameplayManager.Instance != null ? GameplayManager.Instance.CurrentRound : 1;

            // Check if this is boss round
            if (currentRound == BOSS_ROUND)
            {
                yield return StartCoroutine(SpawnBossRoutine());
            }
            else
            {
                // Normal spawning routine
                float elapsed = 0f;

                while (monstersSpawnedThisRound < maxSpawnsPerRound && elapsed < spawnDuration)
                {
                    yield return new WaitForSeconds(spawnInterval);
                    elapsed += spawnInterval;

                    // 라운드 시작 시 선택된 1개 타입만 스폰
                    SpawnMonster(currentRoundMonsterType, PathType.SquareLoop);

                    monstersSpawnedThisRound++;
                }

                isSpawning = false;
            }

            CheckRoundComplete();
        }

        /// <summary>
        /// Boss spawn routine with dramatic entrance effect.
        /// </summary>
        private IEnumerator SpawnBossRoutine()
        {

            // Show boss warning effect
            if (VFXManager.Instance != null)
            {
                VFXManager.Instance.ShowBossWarning();
            }

            // Wait for dramatic pause
            yield return new WaitForSeconds(BOSS_SPAWN_DELAY);

            // Create boss with increased stats
            MonsterData bossData = CreateBossData(currentRoundMonsterType);

            // Spawn boss at center of top edge
            SpawnMonster(bossData, PathType.SquareLoop);

            // Boss spawn effect
            if (VFXManager.Instance != null)
            {
                Monster boss = GetLastSpawnedMonster();
                if (boss != null)
                {
                    VFXManager.Instance.ShowBossSpawnEffect(boss.transform.position);
                }
            }

            monstersSpawnedThisRound = 1; // Only 1 boss
            isSpawning = false;

        }

        /// <summary>
        /// Create enhanced boss version of a monster.
        /// </summary>
        private MonsterData CreateBossData(MonsterData baseData)
        {
            // Create a copy with boosted stats
            MonsterData bossData = ScriptableObject.CreateInstance<MonsterData>();
            bossData.monsterName = $"BOSS {baseData.monsterName}";
            bossData.maxHealth = baseData.maxHealth * 10; // 10x HP
            bossData.moveSpeed = baseData.moveSpeed * 0.7f; // 30% slower (more imposing)
            bossData.attack = baseData.attack * 5; // 5x attack
            bossData.defense = baseData.defense * 3; // 3x defense
            bossData.goldReward = baseData.goldReward * 20; // 20x gold reward
            bossData.type = MonsterType.Boss;

            return bossData;
        }

        /// <summary>
        /// Get the most recently spawned monster.
        /// </summary>
        private Monster GetLastSpawnedMonster()
        {
            List<Monster> monsters = monsterPool.GetActiveMonsters();
            if (monsters != null && monsters.Count > 0)
            {
                return monsters[monsters.Count - 1];
            }
            return null;
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


            // Award gold
            if (GameplayManager.Instance != null)
            {
                int goldReward = monster.GetGoldReward();
                GameplayManager.Instance.ModifyGold(goldReward);
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


            // Decrease player life
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.ModifyLife(-1);
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

            // All monsters use square loop path around grid boundary
            return GridManager.Instance.GetSquareLoopWaypoints();
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
