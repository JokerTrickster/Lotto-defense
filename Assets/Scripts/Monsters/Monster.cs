using UnityEngine;
using System.Collections.Generic;
using System;
using LottoDefense.VFX;
using LottoDefense.Gameplay;

namespace LottoDefense.Monsters
{
    /// <summary>
    /// Individual monster behavior handling movement along waypoint paths and health management.
    /// Poolable component that can be reused for efficient spawning.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class Monster : MonoBehaviour
    {
        #region Properties
        /// <summary>
        /// Monster data template used for this instance.
        /// </summary>
        public MonsterData Data { get; private set; }

        /// <summary>
        /// Current health points.
        /// </summary>
        public int CurrentHealth { get; private set; }

        /// <summary>
        /// Maximum health for this instance (scaled by round).
        /// </summary>
        public int MaxHealth { get; private set; }

        /// <summary>
        /// Defense value for this instance (scaled by round).
        /// </summary>
        public int Defense { get; private set; }

        /// <summary>
        /// Whether this monster is currently active.
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Current waypoint index in the path.
        /// </summary>
        public int CurrentWaypointIndex { get; private set; }
        #endregion

        #region Events
        /// <summary>
        /// Fired when monster dies.
        /// </summary>
        public event Action<Monster> OnDeath;

        /// <summary>
        /// Fired when monster reaches the end of its path.
        /// </summary>
        public event Action<Monster> OnReachEnd;
        #endregion

        #region Private Fields
        private List<Vector3> waypoints;
        private SpriteRenderer spriteRenderer;
        private float moveSpeed;
        private int goldReward;
        private bool loopPath;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (IsActive && waypoints != null && waypoints.Count > 0)
            {
                Move();
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize monster with data and waypoint path.
        /// </summary>
        /// <param name="data">Monster data template</param>
        /// <param name="pathWaypoints">List of waypoint positions to follow</param>
        /// <param name="round">Current round number for scaling</param>
        /// <param name="loopPath">If true, monster loops path (e.g. square loop); if false, ReachEnd at path end</param>
        public void Initialize(MonsterData data, List<Vector3> pathWaypoints, int round, bool loopPath = false)
        {
            if (data == null)
            {
                Debug.LogError("[Monster] Cannot initialize with null MonsterData!");
                return;
            }

            if (pathWaypoints == null || pathWaypoints.Count == 0)
            {
                Debug.LogError("[Monster] Cannot initialize with empty waypoint list!");
                return;
            }

            Data = data;
            waypoints = new List<Vector3>(pathWaypoints);
            CurrentWaypointIndex = 0;
            this.loopPath = loopPath;

            // Apply round scaling
            MaxHealth = data.GetScaledHealth(round);
            CurrentHealth = MaxHealth;
            Defense = data.GetScaledDefense(round);
            moveSpeed = data.moveSpeed;
            goldReward = data.goldReward;

            // Setup visuals
            if (spriteRenderer != null && data.sprite != null)
            {
                spriteRenderer.sprite = data.sprite;
            }

            // Position at first waypoint
            if (waypoints.Count > 0)
            {
                transform.position = waypoints[0];
            }

            IsActive = true;
            gameObject.SetActive(true);

            Debug.Log($"[Monster] {data.monsterName} initialized - HP: {CurrentHealth}/{MaxHealth}, Def: {Defense}, Speed: {moveSpeed}");
        }
        #endregion

        #region Movement
        /// <summary>
        /// Move towards current waypoint target.
        /// </summary>
        private void Move()
        {
            if (CurrentWaypointIndex >= waypoints.Count)
            {
                ReachEnd();
                return;
            }

            Vector3 targetPosition = waypoints[CurrentWaypointIndex];
            float step = moveSpeed * Time.deltaTime;

            // Move towards target waypoint
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

            // Check if waypoint reached
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                OnWaypointReached();
            }
        }

        /// <summary>
        /// Called when monster reaches a waypoint.
        /// </summary>
        private void OnWaypointReached()
        {
            CurrentWaypointIndex++;

            if (CurrentWaypointIndex >= waypoints.Count)
            {
                if (loopPath)
                {
                    CurrentWaypointIndex = 0;
                }
                else
                {
                    ReachEnd();
                }
            }
        }

        /// <summary>
        /// Called when monster reaches the end of its path.
        /// </summary>
        private void ReachEnd()
        {
            if (!IsActive)
                return;

            Debug.Log($"[Monster] {Data.monsterName} reached end of path");

            IsActive = false;
            OnReachEnd?.Invoke(this);
        }
        #endregion

        #region Combat
        /// <summary>
        /// Apply damage to this monster.
        /// </summary>
        /// <param name="rawDamage">Raw attack damage from attacker</param>
        public void TakeDamage(int rawDamage)
        {
            if (!IsActive || CurrentHealth <= 0)
                return;

            // Apply defense reduction: damage = attack - defense (minimum 1)
            int actualDamage = Mathf.Max(1, rawDamage - Defense);
            CurrentHealth -= actualDamage;

            // Show damage number VFX
            if (VFXManager.Instance != null)
            {
                VFXManager.Instance.ShowDamageNumber(transform.position, actualDamage, false);
            }

            Debug.Log($"[Monster] {Data.monsterName} took {actualDamage} damage (raw: {rawDamage}, def: {Defense}, HP: {CurrentHealth}/{MaxHealth})");

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Handle monster death.
        /// Note: Gold is awarded by MonsterManager.HandleMonsterDeath to avoid duplication.
        /// </summary>
        private void Die()
        {
            if (!IsActive)
                return;

            Debug.Log($"[Monster] {Data.monsterName} died");

            // Show gold popup VFX
            if (VFXManager.Instance != null)
            {
                VFXManager.Instance.ShowGoldPopup(transform.position, goldReward);
            }

            // Play death animation
            if (VFXManager.Instance != null)
            {
                VFXManager.Instance.PlayMonsterDeathEffect(this);
            }

            IsActive = false;
            OnDeath?.Invoke(this);
        }
        #endregion

        #region Pooling Support
        /// <summary>
        /// Reset monster state for pooling reuse.
        /// </summary>
        public void ResetForPool()
        {
            IsActive = false;
            CurrentHealth = 0;
            MaxHealth = 0;
            Defense = 0;
            CurrentWaypointIndex = 0;
            waypoints = null;
            loopPath = false;
            Data = null;
            OnDeath = null;
            OnReachEnd = null;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Get gold reward for killing this monster.
        /// </summary>
        public int GetGoldReward()
        {
            return goldReward;
        }
        #endregion
    }
}
