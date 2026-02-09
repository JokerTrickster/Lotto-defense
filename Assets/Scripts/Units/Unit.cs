using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using LottoDefense.Grid;
using LottoDefense.Monsters;

namespace LottoDefense.Units
{
    /// <summary>
    /// MonoBehaviour component attached to placed unit instances on the grid.
    /// Handles visual representation, grid position tracking, and selection state.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class Unit : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Visual Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = Color.yellow;
        [SerializeField] private float selectionGlowIntensity = 1.5f;
        #endregion

        #region Components
        private SpriteRenderer spriteRenderer;
        private GameObject rangeIndicator; // Attack range circle indicator
        #endregion

        #region Properties
        /// <summary>
        /// Reference to the UnitData defining this unit's stats and behavior.
        /// </summary>
        public UnitData Data { get; private set; }

        /// <summary>
        /// Current grid position of this unit.
        /// </summary>
        public Vector2Int GridPosition { get; set; }

        /// <summary>
        /// Whether this unit is currently selected for placement/swapping.
        /// </summary>
        public bool IsSelected { get; private set; }

        /// <summary>
        /// Current upgrade level of this unit (1-10).
        /// Level 1 is base stats, each level increases effectiveness.
        /// </summary>
        public int UpgradeLevel { get; private set; } = 1;

        /// <summary>
        /// Current attack value including upgrade multiplier.
        /// </summary>
        public int CurrentAttack { get; private set; }

        /// <summary>
        /// Current target monster for combat.
        /// </summary>
        public Monster CurrentTarget { get; private set; }
        #endregion

        #region Events
        /// <summary>
        /// Fired when this unit attacks a monster.
        /// Parameters: Monster target, int damage
        /// </summary>
        public event Action<Monster, int> OnAttack;
        #endregion

        #region Private Combat Fields
        private float attackCooldown;
        private float currentCooldown;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnMouseDown()
        {
            // Notify placement manager when this placed unit is clicked
            if (UnitPlacementManager.Instance != null)
            {
                UnitPlacementManager.Instance.OnPlacedUnitClicked(this);
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize this unit instance with its data and grid position.
        /// </summary>
        /// <param name="unitData">Unit data template</param>
        /// <param name="gridPos">Grid coordinates</param>
        public void Initialize(UnitData unitData, Vector2Int gridPos)
        {
            Data = unitData;
            GridPosition = gridPos;
            UpgradeLevel = 1;
            CurrentAttack = unitData.attack; // Start with base attack
            attackCooldown = 1f / unitData.attackSpeed;
            currentCooldown = 0f;

            // Setup visual representation
            if (spriteRenderer != null)
            {
                if (unitData.icon != null)
                {
                    spriteRenderer.sprite = unitData.icon;
                    spriteRenderer.color = normalColor;
                }
                // When icon is null, keep existing sprite/color from prefab (e.g. generated circle)
                spriteRenderer.sortingOrder = 10; // Above grid cells
            }

            // Position at grid cell center and scale to fit cell
            if (GridManager.Instance != null)
            {
                transform.position = GridManager.Instance.GridToWorld(gridPos);
                float cellSize = GridManager.Instance.CellSize;
                transform.localScale = Vector3.one * cellSize * 0.8f;
            }

            gameObject.name = $"Unit_{unitData.unitName}_{gridPos.x}_{gridPos.y}";
            Debug.Log($"[Unit] Initialized {Data.GetDisplayName()} at {GridPosition}");
        }
        #endregion

        #region Selection Management
        /// <summary>
        /// Mark this unit as selected with visual feedback and show attack range.
        /// </summary>
        public void Select()
        {
            IsSelected = true;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = selectedColor * selectionGlowIntensity;
            }

            // Show attack range indicator
            ShowAttackRange();

            Debug.Log($"[Unit] Selected {Data.GetDisplayName()} at {GridPosition}");
        }

        /// <summary>
        /// Deselect this unit and restore normal visuals.
        /// </summary>
        public void Deselect()
        {
            IsSelected = false;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = normalColor;
            }

            // Hide attack range indicator
            HideAttackRange();

            Debug.Log($"[Unit] Deselected {Data.GetDisplayName()} at {GridPosition}");
        }
        #endregion

        #region Position Management
        /// <summary>
        /// Move this unit to a new grid position with visual update.
        /// </summary>
        /// <param name="newGridPos">Target grid coordinates</param>
        public void MoveTo(Vector2Int newGridPos)
        {
            Vector2Int oldPos = GridPosition;
            GridPosition = newGridPos;

            // Update world position
            if (GridManager.Instance != null)
            {
                transform.position = GridManager.Instance.GridToWorld(newGridPos);
            }

            // Update name for debugging
            gameObject.name = $"Unit_{Data.unitName}_{newGridPos.x}_{newGridPos.y}";

            Debug.Log($"[Unit] Moved {Data.GetDisplayName()} from {oldPos} to {newGridPos}");
        }
        #endregion

        #region Upgrade Management
        /// <summary>
        /// Apply an upgrade to this unit, increasing its level and stats.
        /// </summary>
        /// <param name="newLevel">New upgrade level (must be greater than current)</param>
        /// <param name="attackMultiplier">Multiplier to apply to base attack</param>
        public void ApplyUpgrade(int newLevel, float attackMultiplier)
        {
            if (newLevel <= UpgradeLevel)
            {
                Debug.LogWarning($"[Unit] Cannot apply upgrade - new level {newLevel} <= current level {UpgradeLevel}");
                return;
            }

            if (newLevel > 10)
            {
                Debug.LogWarning($"[Unit] Cannot apply upgrade - level {newLevel} exceeds maximum (10)");
                return;
            }

            int oldLevel = UpgradeLevel;
            int oldAttack = CurrentAttack;

            UpgradeLevel = newLevel;
            CurrentAttack = Mathf.RoundToInt(Data.attack * attackMultiplier);

            Debug.Log($"[Unit] {Data.GetDisplayName()} upgraded: L{oldLevel}→L{newLevel}, ATK {oldAttack}→{CurrentAttack}");
        }

        /// <summary>
        /// Get the current attack multiplier based on upgrade level.
        /// </summary>
        public float GetAttackMultiplier()
        {
            return 1.0f + (0.1f * (UpgradeLevel - 1));
        }
        #endregion

        #region Combat System
        /// <summary>
        /// Called each combat tick by CombatManager.
        /// Handles target acquisition and attack execution.
        /// </summary>
        public void CombatTick()
        {
            if (Data == null) return;

            // Update cooldown
            currentCooldown -= 0.1f; // Combat tick interval

            // Find or validate target
            if (CurrentTarget == null || !CurrentTarget.IsActive)
            {
                CurrentTarget = FindNearestMonster();
            }

            // Attack if ready and has target
            if (CurrentTarget != null && currentCooldown <= 0f)
            {
                ExecuteAttack();
                currentCooldown = attackCooldown;
            }
        }

        /// <summary>
        /// Find the nearest active monster within attack range.
        /// </summary>
        private Monster FindNearestMonster()
        {
            Monster[] allMonsters = UnityEngine.Object.FindObjectsByType<Monster>(FindObjectsSortMode.None);
            Monster nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (Monster monster in allMonsters)
            {
                if (!monster.IsActive) continue;

                float distance = Vector3.Distance(transform.position, monster.transform.position);
                if (distance <= Data.attackRange && distance < nearestDistance)
                {
                    nearest = monster;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Execute an attack on the current target.
        /// </summary>
        private void ExecuteAttack()
        {
            if (CurrentTarget == null || !CurrentTarget.IsActive) return;

            int damage = CurrentAttack;
            CurrentTarget.TakeDamage(damage);

            // Visual effect: missile/laser from unit to target
            DrawMissileEffect(transform.position, CurrentTarget.transform.position);

            OnAttack?.Invoke(CurrentTarget, damage);
        }

        /// <summary>
        /// Draw a missile/laser effect from start to end position.
        /// </summary>
        private void DrawMissileEffect(Vector3 start, Vector3 end)
        {
            StartCoroutine(MissileEffectCoroutine(start, end));
        }

        /// <summary>
        /// Coroutine to animate a missile/laser line effect.
        /// </summary>
        private System.Collections.IEnumerator MissileEffectCoroutine(Vector3 start, Vector3 end)
        {
            // Create a temporary GameObject for the line
            GameObject lineObj = new GameObject("MissileEffect");
            LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();

            // Configure line renderer
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            lineRenderer.positionCount = 2;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

            // Set color based on unit rarity
            Color missileColor = GetMissileColor();
            lineRenderer.startColor = missileColor;
            lineRenderer.endColor = missileColor;

            // Animate the missile
            float duration = 0.15f; // Fast missile
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Interpolate position
                Vector3 currentPos = Vector3.Lerp(start, end, t);
                lineRenderer.SetPosition(0, start);
                lineRenderer.SetPosition(1, currentPos);

                yield return null;
            }

            // Fade out
            float fadeDuration = 0.1f;
            elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = 1f - (elapsed / fadeDuration);

                Color fadeColor = missileColor;
                fadeColor.a = alpha;
                lineRenderer.startColor = fadeColor;
                lineRenderer.endColor = fadeColor;

                yield return null;
            }

            // Clean up
            Destroy(lineObj);
        }

        /// <summary>
        /// Get missile color based on unit rarity.
        /// </summary>
        private Color GetMissileColor()
        {
            if (Data == null) return Color.white;

            switch (Data.rarity)
            {
                case Rarity.Normal: return new Color(0.6f, 0.8f, 1f); // Light blue
                case Rarity.Rare: return new Color(0.4f, 0.6f, 1f); // Blue
                case Rarity.Epic: return new Color(0.7f, 0.3f, 1f); // Purple
                case Rarity.Legendary: return new Color(1f, 0.8f, 0.2f); // Gold
                default: return Color.white;
            }
        }

        /// <summary>
        /// Reset combat state (called when combat ends).
        /// </summary>
        public void ResetCombat()
        {
            CurrentTarget = null;
            currentCooldown = 0f;
        }
        #endregion

        #region Attack Range Indicator
        /// <summary>
        /// Show attack range indicator when unit is selected.
        /// </summary>
        private void ShowAttackRange()
        {
            if (Data == null) return;

            // Create range indicator if it doesn't exist
            if (rangeIndicator == null)
            {
                rangeIndicator = new GameObject("RangeIndicator");
                rangeIndicator.transform.SetParent(transform);
                rangeIndicator.transform.localPosition = Vector3.zero;
                rangeIndicator.transform.localRotation = Quaternion.identity;

                // Create circle using LineRenderer
                LineRenderer lineRenderer = rangeIndicator.AddComponent<LineRenderer>();
                lineRenderer.useWorldSpace = false;
                lineRenderer.startWidth = 0.05f;
                lineRenderer.endWidth = 0.05f;
                lineRenderer.positionCount = 64; // Smooth circle
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

                // Get color based on rarity
                Color rangeColor = GetRarityColor(Data.rarity);
                rangeColor.a = 0.5f; // Semi-transparent
                lineRenderer.startColor = rangeColor;
                lineRenderer.endColor = rangeColor;

                // Create circle points
                float angleStep = 360f / 64f;
                float radius = Data.attackRange;
                for (int i = 0; i < 64; i++)
                {
                    float angle = i * angleStep * Mathf.Deg2Rad;
                    Vector3 pos = new Vector3(
                        Mathf.Cos(angle) * radius,
                        Mathf.Sin(angle) * radius,
                        0f
                    );
                    lineRenderer.SetPosition(i, pos);
                }
            }

            rangeIndicator.SetActive(true);
        }

        /// <summary>
        /// Hide attack range indicator when unit is deselected.
        /// </summary>
        private void HideAttackRange()
        {
            if (rangeIndicator != null)
            {
                rangeIndicator.SetActive(false);
            }
        }

        /// <summary>
        /// Get color based on unit rarity.
        /// </summary>
        private Color GetRarityColor(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Normal: return new Color(0.7f, 0.7f, 0.7f); // Gray
                case Rarity.Rare: return new Color(0.3f, 0.6f, 1f); // Blue
                case Rarity.Epic: return new Color(0.7f, 0.3f, 1f); // Purple
                case Rarity.Legendary: return new Color(1f, 0.8f, 0.2f); // Gold
                default: return Color.white;
            }
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get a summary string for debugging.
        /// </summary>
        public override string ToString()
        {
            return $"{Data?.GetDisplayName() ?? "Unknown"} at {GridPosition} (L{UpgradeLevel}, ATK:{CurrentAttack})";
        }
        #endregion
    }
}
