using UnityEngine;
using System.Collections.Generic;

namespace LottoDefense.Monsters
{
    /// <summary>
    /// Object pool system for efficient monster management.
    /// Pre-warms pool with monster instances to avoid runtime instantiation overhead.
    /// </summary>
    public class MonsterPool : MonoBehaviour
    {
        #region Constants
        private const int INITIAL_POOL_SIZE = 20;
        private const int EXPAND_SIZE = 5;
        #endregion

        #region Inspector Fields
        [Header("Pool Settings")]
        [Tooltip("Initial pool capacity")]
        [SerializeField] private int poolSize = INITIAL_POOL_SIZE;

        [Tooltip("Default monster prefab (can be overridden by MonsterData.prefab)")]
        [SerializeField] private GameObject defaultMonsterPrefab;
        #endregion

        #region Private Fields
        private Queue<Monster> availableMonsters;
        private List<Monster> activeMonsters;
        private Transform poolContainer;
        #endregion

        #region Properties
        /// <summary>
        /// Number of available monsters in pool.
        /// </summary>
        public int AvailableCount => availableMonsters.Count;

        /// <summary>
        /// Number of currently active monsters.
        /// </summary>
        public int ActiveCount => activeMonsters.Count;

        /// <summary>
        /// Total pool capacity.
        /// </summary>
        public int TotalCapacity => availableMonsters.Count + activeMonsters.Count;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize and prewarm the pool.
        /// </summary>
        public void Initialize()
        {
            availableMonsters = new Queue<Monster>();
            activeMonsters = new List<Monster>();

            // Create pool container
            poolContainer = new GameObject("MonsterPool_Container").transform;
            poolContainer.SetParent(transform);
            poolContainer.localPosition = Vector3.zero;

            PrewarmPool();

        }

        /// <summary>
        /// Prewarm pool with initial monster instances.
        /// </summary>
        private void PrewarmPool()
        {
            for (int i = 0; i < poolSize; i++)
            {
                Monster monster = CreateMonsterInstance();
                if (monster != null)
                {
                    monster.ResetForPool();
                    availableMonsters.Enqueue(monster);
                }
            }

        }
        #endregion

        #region Pool Operations
        /// <summary>
        /// Get a monster from the pool.
        /// </summary>
        /// <returns>Available monster instance, or null if pool exhausted and cannot expand</returns>
        public Monster GetMonster()
        {
            // Expand pool if needed
            if (availableMonsters.Count == 0)
            {
                Debug.LogWarning($"[MonsterPool] Pool exhausted, expanding by {EXPAND_SIZE}");
                ExpandPool(EXPAND_SIZE);
            }

            if (availableMonsters.Count == 0)
            {
                Debug.LogError("[MonsterPool] Failed to get monster - pool exhausted!");
                return null;
            }

            Monster monster = availableMonsters.Dequeue();
            activeMonsters.Add(monster);

            return monster;
        }

        /// <summary>
        /// Return a monster to the pool.
        /// </summary>
        /// <param name="monster">Monster to return</param>
        public void ReturnMonster(Monster monster)
        {
            if (monster == null)
            {
                Debug.LogError("[MonsterPool] Cannot return null monster!");
                return;
            }

            if (!activeMonsters.Contains(monster))
            {
                Debug.LogWarning("[MonsterPool] Attempting to return monster not tracked as active");
                return;
            }

            activeMonsters.Remove(monster);
            monster.ResetForPool();
            availableMonsters.Enqueue(monster);
        }

        /// <summary>
        /// Expand pool capacity at runtime.
        /// </summary>
        /// <param name="count">Number of monsters to add</param>
        private void ExpandPool(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Monster monster = CreateMonsterInstance();
                if (monster != null)
                {
                    monster.ResetForPool();
                    availableMonsters.Enqueue(monster);
                }
            }

        }
        #endregion

        #region Monster Creation
        /// <summary>
        /// Create a new monster instance.
        /// </summary>
        /// <returns>New monster component</returns>
        private Monster CreateMonsterInstance()
        {
            GameObject monsterObj;

            if (defaultMonsterPrefab != null)
            {
                monsterObj = Instantiate(defaultMonsterPrefab, poolContainer);
            }
            else
            {
                // Create default monster GameObject
                monsterObj = CreateDefaultMonster();
            }

            monsterObj.name = $"Monster_{TotalCapacity}";

            Monster monster = monsterObj.GetComponent<Monster>();
            if (monster == null)
            {
                monster = monsterObj.AddComponent<Monster>();
            }

            return monster;
        }

        /// <summary>
        /// Create default monster GameObject when no prefab provided.
        /// </summary>
        /// <returns>Monster GameObject with basic components</returns>
        private GameObject CreateDefaultMonster()
        {
            GameObject obj = new GameObject("Monster");
            obj.transform.SetParent(poolContainer);

            // Add SpriteRenderer
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;

            // Create simple circle sprite
            Texture2D texture = new Texture2D(32, 32);
            Color monsterColor = new Color(1f, 0.3f, 0.3f, 1f);

            for (int x = 0; x < 32; x++)
            {
                for (int y = 0; y < 32; y++)
                {
                    float dx = x - 16f;
                    float dy = y - 16f;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);

                    if (distance < 14f)
                    {
                        texture.SetPixel(x, y, monsterColor);
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
            }

            texture.Apply();
            sr.sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);

            // Add collider for future combat
            CircleCollider2D collider = obj.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;

            return obj;
        }
        #endregion

        #region Active Monsters Query
        /// <summary>
        /// Get a copy of the active monsters list.
        /// </summary>
        /// <returns>List of currently active monsters</returns>
        public List<Monster> GetActiveMonsters()
        {
            return new List<Monster>(activeMonsters);
        }
        #endregion

        #region Utility
        /// <summary>
        /// Clear all active monsters and return to pool.
        /// </summary>
        public void ClearAll()
        {
            // Return all active monsters to pool
            while (activeMonsters.Count > 0)
            {
                Monster monster = activeMonsters[0];
                ReturnMonster(monster);
            }

        }

        /// <summary>
        /// Get pool statistics for debugging.
        /// </summary>
        public string GetPoolStats()
        {
            return $"Pool Stats - Available: {AvailableCount}, Active: {ActiveCount}, Total: {TotalCapacity}";
        }
        #endregion
    }
}
