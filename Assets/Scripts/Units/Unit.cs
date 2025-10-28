using UnityEngine;
using System;
using System.Collections.Generic;
using LottoDefense.Monsters;
using LottoDefense.Grid;
using LottoDefense.VFX;

namespace LottoDefense.Units
{
    /// <summary>
    /// Individual unit behavior handling combat, targeting, and grid placement.
    /// Automatically targets and attacks monsters within range during combat phase.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class Unit : MonoBehaviour
    {
        #region Properties
        /// <summary>
        /// Unit data template used for this instance.
        /// </summary>
        public UnitData Data { get; private set; }

        /// <summary>
        /// Grid position where this unit is placed.
        /// </summary>
        public Vector2Int GridPosition { get; set; }

        /// <summary>
        /// Current target monster being attacked.
        /// </summary>
        public Monster CurrentTarget { get; private set; }

        /// <summary>
        /// Whether this unit is currently attacking.
        /// </summary>
        public bool IsAttacking => CurrentTarget != null && CurrentTarget.IsActive;

        /// <summary>
        /// Time when the last attack was executed.
        /// </summary>
        public float LastAttackTime { get; private set; }
        #endregion

        #region Events
        /// <summary>
        /// Fired when unit attacks a monster.
        /// </summary>
        public event Action<Unit, Monster, int> OnAttack;

        /// <summary>
        /// Fired when unit acquires a new target.
        /// </summary>
        public event Action<Unit, Monster> OnTargetAcquired;

        /// <summary>
        /// Fired when unit loses its target.
        /// </summary>
        public event Action<Unit> OnTargetLost;
        #endregion

        #region Private Fields
        private SpriteRenderer spriteRenderer;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize unit with data and grid position.
        /// </summary>
        /// <param name="data">Unit data template</param>
        /// <param name="gridPos">Grid coordinates</param>
        public void Initialize(UnitData data, Vector2Int gridPos)
        {
            if (data == null)
            {
                Debug.LogError("[Unit] Cannot initialize with null UnitData!");
                return;
            }

            Data = data;
            GridPosition = gridPos;
            CurrentTarget = null;
            LastAttackTime = 0f;

            // Setup visuals
            if (spriteRenderer != null && data.icon != null)
            {
                spriteRenderer.sprite = data.icon;
            }

            Debug.Log($"[Unit] {data.unitName} initialized at {gridPos} - Attack: {data.attack}, Range: {data.attackRange}, Speed: {data.attackSpeed}/s");
        }
        #endregion

        #region Target Acquisition
        /// <summary>
        /// Acquire the nearest monster within attack range.
        /// Priority: Monster closest to reaching the end (highest waypoint progress).
        /// </summary>
        /// <returns>True if target acquired</returns>
        public bool AcquireTarget()
        {
            Monster previousTarget = CurrentTarget;

            // Get all monsters in range
            List<Monster> monstersInRange = GetMonstersInRange();

            if (monstersInRange.Count == 0)
            {
                if (CurrentTarget != null)
                {
                    CurrentTarget = null;
                    OnTargetLost?.Invoke(this);
                }
                return false;
            }

            // Select target with highest waypoint progress (closest to end)
            Monster bestTarget = null;
            int highestProgress = -1;

            foreach (Monster monster in monstersInRange)
            {
                if (monster.CurrentWaypointIndex > highestProgress)
                {
                    highestProgress = monster.CurrentWaypointIndex;
                    bestTarget = monster;
                }
            }

            CurrentTarget = bestTarget;

            if (CurrentTarget != previousTarget && CurrentTarget != null)
            {
                OnTargetAcquired?.Invoke(this, CurrentTarget);
                Debug.Log($"[Unit] {Data.unitName} acquired target: {CurrentTarget.Data.monsterName} (waypoint {highestProgress})");
            }

            return CurrentTarget != null;
        }

        /// <summary>
        /// Get all monsters within this unit's attack range.
        /// </summary>
        /// <returns>List of monsters in range</returns>
        public List<Monster> GetMonstersInRange()
        {
            List<Monster> monstersInRange = new List<Monster>();

            if (Data == null || MonsterManager.Instance == null)
                return monstersInRange;

            Vector3 unitWorldPos = transform.position;

            // Get all active monsters from MonsterManager
            // Since MonsterManager doesn't expose GetActiveMonsters, we'll need to find them
            Monster[] allMonsters = FindObjectsByType<Monster>(FindObjectsSortMode.None);

            foreach (Monster monster in allMonsters)
            {
                if (!monster.IsActive)
                    continue;

                // Check if monster is within attack range
                if (IsTargetInRange(monster))
                {
                    monstersInRange.Add(monster);
                }
            }

            return monstersInRange;
        }

        /// <summary>
        /// Check if a specific monster is within attack range.
        /// </summary>
        /// <param name="monster">Monster to check</param>
        /// <returns>True if in range</returns>
        public bool IsTargetInRange(Monster monster)
        {
            if (monster == null || !monster.IsActive || Data == null)
                return false;

            float distance = Vector3.Distance(transform.position, monster.transform.position);

            // Convert attack range from cells to world units
            float rangeInWorldUnits = Data.attackRange * (GridManager.Instance?.CellSize ?? 1f);

            return distance <= rangeInWorldUnits;
        }
        #endregion

        #region Combat
        /// <summary>
        /// Check if unit can attack based on attack speed cooldown.
        /// </summary>
        /// <returns>True if cooldown has elapsed</returns>
        public bool CanAttack()
        {
            if (Data == null)
                return false;

            float timeSinceLastAttack = Time.time - LastAttackTime;
            float attackCooldown = Data.GetAttackCooldown();

            return timeSinceLastAttack >= attackCooldown;
        }

        /// <summary>
        /// Attack the current target if in range and cooldown ready.
        /// </summary>
        /// <returns>True if attack was executed</returns>
        public bool Attack()
        {
            if (CurrentTarget == null || !CurrentTarget.IsActive)
            {
                AcquireTarget();
                return false;
            }

            if (!CanAttack())
                return false;

            if (!IsTargetInRange(CurrentTarget))
            {
                AcquireTarget();
                return false;
            }

            // Calculate damage: unit attack value
            // Monster will apply its own defense in TakeDamage
            int damage = Data.attack;

            // Play attack animation
            if (VFXManager.Instance != null)
            {
                VFXManager.Instance.PlayAttackAnimation(this, CurrentTarget);
            }

            // Apply damage to monster
            CurrentTarget.TakeDamage(damage);

            // Update attack time
            LastAttackTime = Time.time;

            // Fire event
            OnAttack?.Invoke(this, CurrentTarget, damage);

            Debug.Log($"[Unit] {Data.unitName} attacked {CurrentTarget.Data.monsterName} for {damage} damage");

            // Check if target died
            if (!CurrentTarget.IsActive)
            {
                CurrentTarget = null;
                OnTargetLost?.Invoke(this);
            }

            return true;
        }

        /// <summary>
        /// Execute combat tick update.
        /// Called by CombatManager during combat phase.
        /// </summary>
        public void CombatTick()
        {
            // Update target if needed
            if (CurrentTarget == null || !CurrentTarget.IsActive || !IsTargetInRange(CurrentTarget))
            {
                AcquireTarget();
            }

            // Attack if ready
            if (CurrentTarget != null && CanAttack())
            {
                Attack();
            }
        }
        #endregion

        #region Cleanup
        /// <summary>
        /// Reset unit state for reuse.
        /// </summary>
        public void ResetForPool()
        {
            Data = null;
            GridPosition = Vector2Int.zero;
            CurrentTarget = null;
            LastAttackTime = 0f;
            OnAttack = null;
            OnTargetAcquired = null;
            OnTargetLost = null;
            gameObject.SetActive(false);
        }
        #endregion
    }
}
