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

        // HP Bar components
        private GameObject hpBarContainer;
        private SpriteRenderer hpBarBackground;
        private SpriteRenderer hpBarFill;
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
            if (spriteRenderer != null)
            {
                if (data.sprite != null)
                {
                    spriteRenderer.sprite = data.sprite;
                }
                else
                {
                    // Create default colored sprite if none assigned
                    spriteRenderer.sprite = CreateDefaultSprite(data.type);
                }

                // Ensure monster is visible by setting a reasonable size (half of previous 0.5)
                transform.localScale = Vector3.one * 0.25f;
            }

            // Position at first waypoint
            if (waypoints.Count > 0)
            {
                transform.position = waypoints[0];
            }

            IsActive = true;
            gameObject.SetActive(true);

            // Create HP bar
            CreateHPBar();

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

            // Update HP bar
            UpdateHPBar();

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

        #region Visuals
        /// <summary>
        /// Create a default colored sprite for monsters without assigned sprites.
        /// </summary>
        private Sprite CreateDefaultSprite(MonsterType type)
        {
            // Color based on monster type
            Color color = type switch
            {
                MonsterType.Normal => Color.green,
                MonsterType.Fast => Color.yellow,
                MonsterType.Tank => new Color(0.6f, 0.3f, 0.1f), // Brown
                MonsterType.Boss => Color.red,
                _ => Color.white
            };

            // Create a simple 32x32 texture
            Texture2D texture = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];

            // Create a filled circle
            Vector2 center = new Vector2(16, 16);
            float radius = 14f;

            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    if (distance < radius)
                    {
                        // Inner color
                        pixels[y * 32 + x] = color;
                    }
                    else if (distance < radius + 2)
                    {
                        // Border
                        pixels[y * 32 + x] = Color.black;
                    }
                    else
                    {
                        // Transparent
                        pixels[y * 32 + x] = Color.clear;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            texture.filterMode = FilterMode.Point;

            return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
        }
        #endregion

        #region HP Bar
        /// <summary>
        /// Create HP bar visual above monster.
        /// </summary>
        private void CreateHPBar()
        {
            // Container for HP bar (positioned above monster)
            hpBarContainer = new GameObject("HPBar");
            hpBarContainer.transform.SetParent(transform);
            hpBarContainer.transform.localPosition = new Vector3(0f, 0.6f, 0f); // Higher above monster
            hpBarContainer.transform.localRotation = Quaternion.identity;
            hpBarContainer.transform.localScale = Vector3.one;

            // Larger HP bar for better visibility
            float barWidth = 1.2f;
            float barHeight = 0.15f;

            // Background (red/dark)
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(hpBarContainer.transform);
            bgObj.transform.localPosition = Vector3.zero;
            bgObj.transform.localRotation = Quaternion.identity;
            bgObj.transform.localScale = Vector3.one;

            hpBarBackground = bgObj.AddComponent<SpriteRenderer>();
            hpBarBackground.sprite = CreateBarSprite();
            hpBarBackground.color = new Color(0.3f, 0.1f, 0.1f); // Dark red
            hpBarBackground.sortingOrder = 100;

            // Scale to bar size
            bgObj.transform.localScale = new Vector3(barWidth, barHeight, 1f);

            // Fill (green)
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(hpBarContainer.transform);
            fillObj.transform.localPosition = Vector3.zero;
            fillObj.transform.localRotation = Quaternion.identity;
            fillObj.transform.localScale = Vector3.one;

            hpBarFill = fillObj.AddComponent<SpriteRenderer>();
            hpBarFill.sprite = CreateBarSprite();
            hpBarFill.color = new Color(0.2f, 0.8f, 0.2f); // Bright green
            hpBarFill.sortingOrder = 101;

            // Scale to bar size (full width initially)
            fillObj.transform.localScale = new Vector3(barWidth, barHeight, 1f);

            // Position fill to align left
            fillObj.transform.localPosition = Vector3.zero;
        }

        /// <summary>
        /// Update HP bar fill based on current health.
        /// </summary>
        private void UpdateHPBar()
        {
            if (hpBarFill == null || MaxHealth <= 0) return;

            float hpPercent = (float)CurrentHealth / MaxHealth;
            hpPercent = Mathf.Clamp01(hpPercent);

            // Scale fill width
            Vector3 scale = hpBarFill.transform.localScale;
            float baseWidth = 1.2f; // Match CreateHPBar width
            scale.x = baseWidth * hpPercent;
            hpBarFill.transform.localScale = scale;

            // Adjust position to keep left-aligned
            float offset = baseWidth * (1f - hpPercent) * 0.5f;
            hpBarFill.transform.localPosition = new Vector3(-offset, 0f, 0f);

            // Change color based on health percentage
            if (hpPercent > 0.6f)
                hpBarFill.color = new Color(0.2f, 0.8f, 0.2f); // Green
            else if (hpPercent > 0.3f)
                hpBarFill.color = new Color(1f, 0.8f, 0.2f); // Yellow/Orange
            else
                hpBarFill.color = new Color(1f, 0.2f, 0.2f); // Red
        }

        /// <summary>
        /// Create a simple square sprite for HP bar.
        /// </summary>
        private Sprite CreateBarSprite()
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
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
