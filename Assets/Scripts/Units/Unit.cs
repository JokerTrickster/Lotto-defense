using UnityEngine;
using UnityEngine.UI;
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
        [SerializeField] private Color selectedColor = Color.yellow;
        [SerializeField] private float selectionGlowIntensity = 1.5f;
        #endregion

        #region Components
        private SpriteRenderer spriteRenderer;
        private GameObject rangeIndicator; // Attack range circle indicator
        private LottoDefense.UI.ManaBar manaBar; // Mana bar UI (for units with skills)
        #endregion

        #region Visual State
        /// <summary>
        /// Original color of this unit based on rarity.
        /// Set during initialization and restored on deselect.
        /// </summary>
        private Color originalColor;
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
        /// Attack upgrade level (0-10).
        /// Each level increases attack power by 10%.
        /// </summary>
        public int AttackUpgradeLevel { get; private set; } = 0;

        /// <summary>
        /// Attack speed upgrade level (0-10).
        /// Each level increases attack speed by 8%.
        /// </summary>
        public int AttackSpeedUpgradeLevel { get; private set; } = 0;

        /// <summary>
        /// Current attack value including upgrade multiplier.
        /// </summary>
        public int CurrentAttack { get; private set; }

        /// <summary>
        /// Current attack speed including upgrade multiplier.
        /// </summary>
        public float CurrentAttackSpeed { get; private set; }

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
        private int combatTickCount; // Track combat ticks for logging throttle
        #endregion

        #region Mana & Skill System
        /// <summary>
        /// Current mana amount (0 to maxMana).
        /// </summary>
        public float CurrentMana { get; private set; }

        /// <summary>
        /// Maximum mana capacity.
        /// </summary>
        public float MaxMana { get; private set; } = 100f;

        /// <summary>
        /// Mana regeneration per second.
        /// </summary>
        public float ManaRegenPerSecond { get; private set; } = 10f;

        /// <summary>
        /// Whether this unit has at least one skill.
        /// </summary>
        public bool HasSkill => Data != null && Data.skills != null && Data.skills.Length > 0;

        /// <summary>
        /// Fired when mana changes.
        /// Parameters: currentMana, maxMana
        /// </summary>
        public event Action<float, float> OnManaChanged;

        /// <summary>
        /// Fired when skill is auto-triggered.
        /// Parameter: UnitSkill
        /// </summary>
        public event Action<UnitSkill> OnSkillTriggered;
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

        private void OnDestroy()
        {
            // Clean up mana bar
            if (manaBar != null)
            {
                Destroy(manaBar.gameObject);
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
            AttackUpgradeLevel = 0;
            AttackSpeedUpgradeLevel = 0;
            CurrentAttack = unitData.attack; // Start with base attack
            CurrentAttackSpeed = unitData.attackSpeed; // Start with base attack speed
            attackCooldown = 1f / unitData.attackSpeed;
            currentCooldown = 0f;

            // Setup visual representation
            if (spriteRenderer != null)
            {
                if (unitData.icon != null)
                {
                    spriteRenderer.sprite = unitData.icon;
                    // Use rarity color for unit
                    originalColor = GetRarityColorForUnit(unitData.rarity);
                    spriteRenderer.color = originalColor;
                }
                else
                {
                    // When icon is null, sprite/color from prefab (generated circle with rarity color)
                    // Store the existing color as original
                    originalColor = spriteRenderer.color;
                }
                spriteRenderer.sortingOrder = 10; // Above grid cells
            }

            // Position at grid cell center and scale to fit cell
            if (GridManager.Instance != null)
            {
                transform.position = GridManager.Instance.GridToWorld(gridPos);
                float cellSize = GridManager.Instance.CellSize;
                transform.localScale = Vector3.one * cellSize * 0.8f;
            }

            // Initialize mana system (시간 기반 마나 재생)
            CurrentMana = 0f;
            MaxMana = 100f;
            ManaRegenPerSecond = 10f; // 초당 10 마나 재생 (10초에 100 마나 = 스킬 발동)

            // Initialize skills
            if (unitData.skills != null && unitData.skills.Length > 0)
            {
                foreach (var skill in unitData.skills)
                {
                    skill.Initialize();
                }
                Debug.Log($"[Unit] Initialized {unitData.skills.Length} skills for {Data.GetDisplayName()}");

                // Create mana bar for units with skills
                CreateManaBar();
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
        /// Deselect this unit and restore original rarity-based color.
        /// </summary>
        public void Deselect()
        {
            IsSelected = false;

            if (spriteRenderer != null)
            {
                // Restore original rarity color instead of white
                spriteRenderer.color = originalColor;
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
        /// Once locked onto a target, pursues it until death (not just until out of range).
        /// </summary>
        public void CombatTick()
        {
            if (Data == null) return;

            combatTickCount++;

            float tickInterval = 0.1f; // Combat tick interval

            // Update cooldown
            currentCooldown -= tickInterval;

            // Regenerate mana over time (only if unit has skills)
            if (HasSkill)
            {
                RegenerateMana(tickInterval);
            }

            // Update skill cooldowns
            UpdateSkillCooldowns(tickInterval);

            // Find or validate target - ONLY if target is dead/inactive
            // Do NOT switch targets just because out of range - pursue until death!
            if (CurrentTarget == null || !CurrentTarget.IsActive)
            {
                CurrentTarget = FindNearestMonster();
                if (CurrentTarget != null)
                {
                    Debug.Log($"[Unit] {Data.GetDisplayName()} found target: {CurrentTarget.Data.monsterName} at distance {Vector3.Distance(transform.position, CurrentTarget.transform.position):F2}");
                }
            }

            // Attack if ready and has target
            // NOTE: Attack even if out of range - unit will chase target until it dies
            if (CurrentTarget != null && currentCooldown <= 0f)
            {
                float distance = Vector3.Distance(transform.position, CurrentTarget.transform.position);

                // Only attack if within range, but keep target locked
                if (distance <= Data.attackRange)
                {
                    ExecuteAttack();
                    currentCooldown = attackCooldown;

                    // Only log if target still exists after attack (it might have died)
                    if (CurrentTarget != null && CurrentTarget.Data != null)
                    {
                        Debug.Log($"[Unit] {Data.GetDisplayName()} attacked {CurrentTarget.Data.monsterName} for {CurrentAttack} damage. Next attack in {attackCooldown:F2}s");
                    }
                }
                else
                {
                    // Target out of range but still locked - log occasionally
                    if (combatTickCount % 20 == 0) // Log every 2 seconds
                    {
                        Debug.Log($"[Unit] {Data.GetDisplayName()} target out of range ({distance:F2} > {Data.attackRange}), waiting for target to return or die");
                    }
                }
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
        /// Execute an attack on the current target based on attack pattern.
        /// </summary>
        private void ExecuteAttack()
        {
            if (CurrentTarget == null || !CurrentTarget.IsActive) return;

            int damage = CurrentAttack;
            Vector3 targetPos = CurrentTarget.transform.position; // Save position before damage

            // Execute attack based on pattern
            switch (Data.attackPattern)
            {
                case AttackPattern.SingleTarget:
                    ExecuteSingleTargetAttack(CurrentTarget, damage);
                    break;

                case AttackPattern.Splash:
                    ExecuteSplashAttack(CurrentTarget, targetPos, damage);
                    break;

                case AttackPattern.AOE:
                    ExecuteAOEAttack(targetPos, damage);
                    break;

                case AttackPattern.Pierce:
                    ExecutePierceAttack(CurrentTarget, damage);
                    break;

                case AttackPattern.Chain:
                    ExecuteChainAttack(CurrentTarget, damage);
                    break;

                default:
                    ExecuteSingleTargetAttack(CurrentTarget, damage);
                    break;
            }

            // Visual effect
            DrawMissileEffect(transform.position, targetPos);
        }

        /// <summary>
        /// Execute single target attack (default).
        /// </summary>
        private void ExecuteSingleTargetAttack(Monster target, int damage)
        {
            if (target == null || !target.IsActive) return;

            target.TakeDamage(damage);
            OnAttack?.Invoke(target, damage);
        }

        /// <summary>
        /// Execute splash attack (primary target + nearby enemies).
        /// </summary>
        private void ExecuteSplashAttack(Monster primaryTarget, Vector3 targetPos, int primaryDamage)
        {
            if (primaryTarget == null || !primaryTarget.IsActive) return;

            // Damage primary target
            primaryTarget.TakeDamage(primaryDamage);
            OnAttack?.Invoke(primaryTarget, primaryDamage);

            // Find nearby enemies within splash radius
            if (MonsterManager.Instance != null && Data.splashRadius > 0f)
            {
                var allMonsters = MonsterManager.Instance.GetActiveMonstersList();
                float falloffPercent = Data.splashDamageFalloff / 100f;

                foreach (var monster in allMonsters)
                {
                    if (monster == null || !monster.IsActive || monster == primaryTarget) continue;

                    float distance = Vector3.Distance(targetPos, monster.transform.position);
                    if (distance <= Data.splashRadius)
                    {
                        // Calculate damage with falloff
                        float damageMultiplier = Mathf.Lerp(1f, falloffPercent, distance / Data.splashRadius);
                        int splashDamage = Mathf.RoundToInt(primaryDamage * damageMultiplier);

                        monster.TakeDamage(splashDamage);

                        // Visual splash effect
                        DrawSplashEffect(targetPos, monster.transform.position);
                    }
                }
            }
        }

        /// <summary>
        /// Execute AOE attack (all enemies in radius).
        /// </summary>
        private void ExecuteAOEAttack(Vector3 center, int damage)
        {
            if (MonsterManager.Instance == null || Data.splashRadius <= 0f) return;

            var allMonsters = MonsterManager.Instance.GetActiveMonstersList();
            float falloffPercent = Data.splashDamageFalloff / 100f;

            foreach (var monster in allMonsters)
            {
                if (monster == null || !monster.IsActive) continue;

                float distance = Vector3.Distance(center, monster.transform.position);
                if (distance <= Data.splashRadius)
                {
                    // Calculate damage with falloff
                    float damageMultiplier = Mathf.Lerp(1f, falloffPercent, distance / Data.splashRadius);
                    int aoeDamage = Mathf.RoundToInt(damage * damageMultiplier);

                    monster.TakeDamage(aoeDamage);
                    OnAttack?.Invoke(monster, aoeDamage);
                }
            }
        }

        /// <summary>
        /// Execute pierce attack (hits multiple enemies in a line).
        /// </summary>
        private void ExecutePierceAttack(Monster firstTarget, int damage)
        {
            if (firstTarget == null || !firstTarget.IsActive || MonsterManager.Instance == null) return;

            Vector3 direction = (firstTarget.transform.position - transform.position).normalized;
            var allMonsters = MonsterManager.Instance.GetActiveMonstersList();

            int hitCount = 0;
            int maxHits = Data.maxTargets > 0 ? Data.maxTargets : 999;

            foreach (var monster in allMonsters)
            {
                if (monster == null || !monster.IsActive) continue;
                if (hitCount >= maxHits) break;

                // Check if monster is in line of fire
                Vector3 toMonster = monster.transform.position - transform.position;
                float distance = toMonster.magnitude;

                if (distance <= Data.attackRange)
                {
                    float angle = Vector3.Angle(direction, toMonster.normalized);
                    if (angle < 15f) // Within 15 degree cone
                    {
                        monster.TakeDamage(damage);
                        OnAttack?.Invoke(monster, damage);
                        hitCount++;
                    }
                }
            }
        }

        /// <summary>
        /// Execute chain attack (bounces between nearby enemies).
        /// </summary>
        private void ExecuteChainAttack(Monster firstTarget, int damage)
        {
            if (firstTarget == null || !firstTarget.IsActive || MonsterManager.Instance == null) return;

            int maxChains = Data.maxTargets > 0 ? Data.maxTargets : 3;
            float chainRange = Data.splashRadius > 0f ? Data.splashRadius : 2f;

            Monster currentTarget = firstTarget;
            HashSet<Monster> hitMonsters = new HashSet<Monster>();

            for (int i = 0; i < maxChains; i++)
            {
                if (currentTarget == null || !currentTarget.IsActive) break;

                // Damage current target
                int chainDamage = Mathf.RoundToInt(damage * Mathf.Pow(0.8f, i)); // 20% reduction per chain
                currentTarget.TakeDamage(chainDamage);
                OnAttack?.Invoke(currentTarget, chainDamage);
                hitMonsters.Add(currentTarget);

                // Find next target
                Monster nextTarget = FindNearestMonsterExcluding(currentTarget.transform.position, chainRange, hitMonsters);
                if (nextTarget == null) break;

                // Visual chain effect
                DrawChainEffect(currentTarget.transform.position, nextTarget.transform.position);

                currentTarget = nextTarget;
            }
        }

        /// <summary>
        /// Find nearest monster within range, excluding already hit monsters.
        /// </summary>
        private Monster FindNearestMonsterExcluding(Vector3 position, float range, HashSet<Monster> exclude)
        {
            if (MonsterManager.Instance == null) return null;

            var allMonsters = MonsterManager.Instance.GetActiveMonstersList();
            Monster nearest = null;
            float nearestDist = float.MaxValue;

            foreach (var monster in allMonsters)
            {
                if (monster == null || !monster.IsActive || exclude.Contains(monster)) continue;

                float dist = Vector3.Distance(position, monster.transform.position);
                if (dist <= range && dist < nearestDist)
                {
                    nearest = monster;
                    nearestDist = dist;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Draw splash effect line from center to hit position.
        /// </summary>
        private void DrawSplashEffect(Vector3 center, Vector3 hitPos)
        {
            StartCoroutine(SplashEffectCoroutine(center, hitPos));
        }

        private System.Collections.IEnumerator SplashEffectCoroutine(Vector3 start, Vector3 end)
        {
            GameObject lineObj = new GameObject("SplashEffect");
            LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();

            lineRenderer.startWidth = 0.03f;
            lineRenderer.endWidth = 0.03f;
            lineRenderer.positionCount = 2;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = new Color(1f, 0.5f, 0f, 0.6f); // Orange
            lineRenderer.endColor = new Color(1f, 0.5f, 0f, 0f);

            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);

            yield return new WaitForSeconds(0.1f);
            Destroy(lineObj);
        }

        /// <summary>
        /// Draw chain effect line between targets.
        /// </summary>
        private void DrawChainEffect(Vector3 from, Vector3 to)
        {
            StartCoroutine(ChainEffectCoroutine(from, to));
        }

        private System.Collections.IEnumerator ChainEffectCoroutine(Vector3 start, Vector3 end)
        {
            GameObject lineObj = new GameObject("ChainEffect");
            LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();

            lineRenderer.startWidth = 0.04f;
            lineRenderer.endWidth = 0.04f;
            lineRenderer.positionCount = 2;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = new Color(0.5f, 0.5f, 1f, 0.8f); // Electric blue
            lineRenderer.endColor = new Color(0.5f, 0.5f, 1f, 0.8f);

            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);

            yield return new WaitForSeconds(0.15f);
            Destroy(lineObj);
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
            combatTickCount = 0;
        }
        #endregion

        #region Mana & Skill Management
        /// <summary>
        /// Regenerate mana over time.
        /// Called every combat tick to gradually increase mana.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since last tick</param>
        private void RegenerateMana(float deltaTime)
        {
            if (CurrentMana >= MaxMana) return; // Already full

            float previousMana = CurrentMana;
            float manaGain = ManaRegenPerSecond * deltaTime;
            CurrentMana = Mathf.Min(CurrentMana + manaGain, MaxMana);

            // Fire mana changed event
            OnManaChanged?.Invoke(CurrentMana, MaxMana);

            // Auto-trigger skill when mana reaches max
            if (previousMana < MaxMana && CurrentMana >= MaxMana)
            {
                TriggerSkill();
            }
        }

        /// <summary>
        /// Auto-trigger the first active skill when mana is full.
        /// Resets mana to 0 after activation.
        /// </summary>
        private void TriggerSkill()
        {
            if (Data == null || Data.skills == null || Data.skills.Length == 0)
            {
                Debug.LogWarning($"[Unit] {Data.GetDisplayName()} has no skills to trigger!");
                return;
            }

            // Find first active skill that can be triggered
            UnitSkill skillToActivate = null;
            foreach (var skill in Data.skills)
            {
                if (skill.skillType == SkillType.Active && skill.TryActivate())
                {
                    skillToActivate = skill;
                    break;
                }
            }

            if (skillToActivate != null)
            {
                Debug.Log($"[Unit] {Data.GetDisplayName()} auto-triggered skill: {skillToActivate.skillName}");

                // Apply skill effect
                ApplySkillEffect(skillToActivate);

                // Reset mana to 0 after using skill
                CurrentMana = 0f;
                OnManaChanged?.Invoke(CurrentMana, MaxMana);

                // Fire skill triggered event
                OnSkillTriggered?.Invoke(skillToActivate);
            }
            else
            {
                Debug.LogWarning($"[Unit] {Data.GetDisplayName()} mana full but no skill could be activated!");
            }
        }

        /// <summary>
        /// Apply skill effect based on skill type and parameters.
        /// </summary>
        /// <param name="skill">Skill to apply</param>
        private void ApplySkillEffect(UnitSkill skill)
        {
            if (skill == null) return;

            // Apply damage multiplier temporarily
            if (skill.damageMultiplier > 1f)
            {
                StartCoroutine(ApplyTemporaryDamageBuff(skill.damageMultiplier, skill.effectDuration));
            }

            // Apply attack speed buff temporarily
            if (skill.attackSpeedMultiplier > 1f)
            {
                StartCoroutine(ApplyTemporaryAttackSpeedBuff(skill.attackSpeedMultiplier, skill.effectDuration));
            }

            // Visual effect
            if (skill.vfxPrefab != null)
            {
                GameObject vfx = Instantiate(skill.vfxPrefab, transform.position, Quaternion.identity);
                Destroy(vfx, 2f); // Clean up after 2 seconds
            }

            Debug.Log($"[Unit] Applied skill effect: {skill.skillName} (DMG×{skill.damageMultiplier}, AS×{skill.attackSpeedMultiplier}, Duration: {skill.effectDuration}s)");
        }

        /// <summary>
        /// Apply temporary damage buff.
        /// </summary>
        private System.Collections.IEnumerator ApplyTemporaryDamageBuff(float multiplier, float duration)
        {
            int originalAttack = CurrentAttack;
            CurrentAttack = Mathf.RoundToInt(CurrentAttack * multiplier);

            Debug.Log($"[Unit] Damage buff applied: {originalAttack} → {CurrentAttack} for {duration}s");

            yield return new WaitForSeconds(duration);

            CurrentAttack = originalAttack;
            Debug.Log($"[Unit] Damage buff expired: {CurrentAttack}");
        }

        /// <summary>
        /// Apply temporary attack speed buff.
        /// </summary>
        private System.Collections.IEnumerator ApplyTemporaryAttackSpeedBuff(float multiplier, float duration)
        {
            float originalCooldown = attackCooldown;
            attackCooldown = originalCooldown / multiplier;

            Debug.Log($"[Unit] Attack speed buff applied: {originalCooldown:F2}s → {attackCooldown:F2}s for {duration}s");

            yield return new WaitForSeconds(duration);

            attackCooldown = originalCooldown;
            Debug.Log($"[Unit] Attack speed buff expired: {attackCooldown:F2}s");
        }

        /// <summary>
        /// Get mana percentage (0 to 1).
        /// </summary>
        public float GetManaPercentage()
        {
            return MaxMana > 0f ? CurrentMana / MaxMana : 0f;
        }

        /// <summary>
        /// Update skill cooldowns (called from Update or CombatTick).
        /// </summary>
        private void UpdateSkillCooldowns(float deltaTime)
        {
            if (Data == null || Data.skills == null) return;

            foreach (var skill in Data.skills)
            {
                skill.UpdateCooldown(deltaTime);
            }
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
        /// Get color based on unit rarity for attack range indicator.
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

        /// <summary>
        /// Get color for unit sprite based on rarity (slightly brighter for visibility).
        /// </summary>
        private Color GetRarityColorForUnit(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Normal: return new Color(0.6f, 0.8f, 1f); // Light blue
                case Rarity.Rare: return new Color(0.4f, 0.6f, 1f); // Blue
                case Rarity.Epic: return new Color(0.7f, 0.3f, 1f); // Purple
                case Rarity.Legendary: return new Color(1f, 0.84f, 0f); // Gold
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

        #region Mana Bar Management
        /// <summary>
        /// Create and initialize mana bar UI for this unit.
        /// Only called for units with skills.
        /// </summary>
        private void CreateManaBar()
        {
            // Find or create canvas for mana bars
            Canvas manaBarCanvas = FindManaBarCanvas();
            if (manaBarCanvas == null)
            {
                Debug.LogError("[Unit] Failed to find/create mana bar canvas!");
                return;
            }

            // Create mana bar GameObject
            GameObject manaBarObj = new GameObject($"ManaBar_{Data.unitName}");
            manaBarObj.transform.SetParent(manaBarCanvas.transform);

            // Setup mana bar rect transform FIRST (before adding component)
            RectTransform manaBarRect = manaBarObj.GetComponent<RectTransform>();
            if (manaBarRect == null)
            {
                manaBarRect = manaBarObj.AddComponent<RectTransform>();
            }
            manaBarRect.sizeDelta = new Vector2(80f, 8f); // 80 pixels wide, 8 pixels tall

            // Create background image
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(manaBarObj.transform, false);
            UnityEngine.UI.Image bgImage = bgObj.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); // Dark background
            bgImage.raycastTarget = false; // Don't block clicks on units

            // Setup rect transform for background
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            bgRect.anchoredPosition = Vector2.zero;

            // Create fill image
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(manaBarObj.transform, false);
            UnityEngine.UI.Image fillImage = fillObj.AddComponent<UnityEngine.UI.Image>();
            fillImage.color = new Color(0.2f, 0.5f, 1f, 1f); // Blue mana color
            fillImage.raycastTarget = false; // Don't block clicks on units
            fillImage.type = UnityEngine.UI.Image.Type.Filled;
            fillImage.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)UnityEngine.UI.Image.OriginHorizontal.Left;

            // Setup rect transform for fill
            RectTransform fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            fillRect.anchoredPosition = Vector2.zero;

            // Add ManaBar component AFTER creating children
            manaBar = manaBarObj.AddComponent<LottoDefense.UI.ManaBar>();

            // Initialize the mana bar (this will find the Fill image via GetComponentInChildren)
            manaBar.Initialize(this);

            Debug.Log($"[Unit] Created mana bar for {Data.GetDisplayName()}");
        }

        /// <summary>
        /// Find or create the canvas for mana bars.
        /// </summary>
        private Canvas FindManaBarCanvas()
        {
            // Try to find existing GameCanvas
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas canvas in canvases)
            {
                if (canvas.gameObject.name == "GameCanvas")
                {
                    return canvas;
                }
            }

            // If not found, create a new canvas
            Debug.LogWarning("[Unit] GameCanvas not found, creating new canvas for mana bars");
            GameObject canvasObj = new GameObject("ManaBarCanvas");
            Canvas newCanvas = canvasObj.AddComponent<Canvas>();
            newCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            newCanvas.sortingOrder = 100; // Above other UI

            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            return newCanvas;
        }
        #endregion

        #region Upgrade System
        /// <summary>
        /// Upgrade attack level (increases attack damage by percentage from UnitData).
        /// </summary>
        public void UpgradeAttack()
        {
            AttackUpgradeLevel++;

            // Recalculate attack with multiplier from UnitData settings
            if (UnitUpgradeManager.Instance != null)
            {
                float multiplier = UnitUpgradeManager.Instance.GetAttackMultiplier(AttackUpgradeLevel, Data);
                CurrentAttack = Mathf.RoundToInt(Data.attack * multiplier);
            }

            Debug.Log($"[Unit] {Data.GetDisplayName()} attack upgraded to level {AttackUpgradeLevel} (Attack: {CurrentAttack})");
        }

        /// <summary>
        /// Upgrade attack speed level (increases attack speed by percentage from UnitData).
        /// </summary>
        public void UpgradeAttackSpeed()
        {
            AttackSpeedUpgradeLevel++;

            // Recalculate attack speed and cooldown with multiplier from UnitData settings
            if (UnitUpgradeManager.Instance != null)
            {
                float multiplier = UnitUpgradeManager.Instance.GetAttackSpeedMultiplier(AttackSpeedUpgradeLevel, Data);
                CurrentAttackSpeed = Data.attackSpeed * multiplier;
                attackCooldown = 1f / CurrentAttackSpeed;
            }

            Debug.Log($"[Unit] {Data.GetDisplayName()} attack speed upgraded to level {AttackSpeedUpgradeLevel} (Speed: {CurrentAttackSpeed:F2})");
        }
        #endregion
    }
}
