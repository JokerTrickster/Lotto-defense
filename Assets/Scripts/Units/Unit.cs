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
        private int outOfRangeTicks; // Track how long target has been out of range
        private const int MAX_OUT_OF_RANGE_TICKS = 20; // 2 seconds before target switch
        private float activeDamageBuff = 1f;
        private float activeSpeedBuff = 1f;

        /// <summary>
        /// Attack range converted from grid units to world-space units.
        /// attackRange is defined in grid cells, but distance checks use world coordinates.
        /// </summary>
        private float WorldAttackRange
        {
            get
            {
                float cellSize = GridManager.Instance != null ? GridManager.Instance.CellSize : 1f;
                return Data.attackRange * cellSize;
            }
        }
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
        /// Per-instance cloned skills (prevents shared state between same-type units).
        /// </summary>
        private UnitSkill[] instanceSkills;

        /// <summary>
        /// Whether this unit has at least one skill.
        /// </summary>
        public bool HasSkill => instanceSkills != null && instanceSkills.Length > 0;

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
            // Block selection when a UI panel (quest/synthesis guide) covers the screen
            if (IsPointerOverUI()) return;

            // Notify placement manager when this placed unit is clicked
            if (UnitPlacementManager.Instance != null)
            {
                UnitPlacementManager.Instance.OnPlacedUnitClicked(this);
            }
        }

        private bool IsPointerOverUI()
        {
            if (UnityEngine.EventSystems.EventSystem.current == null) return false;
            if (Input.touchCount > 0)
                return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
            return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
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
                    originalColor = UnitData.GetRarityColor(unitData.rarity);
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
            ManaRegenPerSecond = 10f; // 기본값 (스킬 없을 때 fallback)

            // Clone skills per-unit to prevent shared cooldown state between same-type units
            if (unitData.skills != null && unitData.skills.Length > 0)
            {
                instanceSkills = new UnitSkill[unitData.skills.Length];
                for (int i = 0; i < unitData.skills.Length; i++)
                {
                    instanceSkills[i] = UnitSkill.FromBalance(new Gameplay.GameBalanceConfig.SkillBalance
                    {
                        skillName = unitData.skills[i].skillName,
                        description = unitData.skills[i].description,
                        skillType = unitData.skills[i].skillType,
                        cooldownDuration = unitData.skills[i].cooldownDuration,
                        initialCooldown = unitData.skills[i].initialCooldown,
                        damageMultiplier = unitData.skills[i].damageMultiplier,
                        rangeMultiplier = unitData.skills[i].rangeMultiplier,
                        attackSpeedMultiplier = unitData.skills[i].attackSpeedMultiplier,
                        effectDuration = unitData.skills[i].effectDuration,
                        targetCount = unitData.skills[i].targetCount,
                        aoeRadius = unitData.skills[i].aoeRadius,
                        slowMultiplier = unitData.skills[i].slowMultiplier,
                        freezeDuration = unitData.skills[i].freezeDuration,
                        ccDuration = unitData.skills[i].ccDuration
                    });
                    instanceSkills[i].Initialize();
                }

                // Active 스킬의 cooldownDuration에 맞춰 마나 재생 속도 설정
                // 마나가 정확히 cooldownDuration 초 후에 가득 차서 스킬이 자동 발동됨
                float skillCooldown = 0f;
                foreach (var skill in instanceSkills)
                {
                    if (skill.skillType == SkillType.Active && skill.cooldownDuration > 0f)
                    {
                        skillCooldown = skill.cooldownDuration;
                        break;
                    }
                }
                if (skillCooldown > 0f)
                {
                    ManaRegenPerSecond = MaxMana / skillCooldown;
                }
                Debug.Log($"[Unit] Initialized {instanceSkills.Length} skills for {Data.GetDisplayName()} (activeSkillCooldown={skillCooldown}s, manaRegen={ManaRegenPerSecond:F1}/s)");

                // Create mana bar for units with skills
                CreateManaBar();
            }
            else
            {
                instanceSkills = null;
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

            // Update skill cooldowns BEFORE mana regen so cooldown reaches 0
            // on the same tick that mana reaches max (preventing TryActivate failure)
            UpdateSkillCooldowns(tickInterval);

            // Regenerate mana over time (only if unit has skills)
            if (HasSkill)
            {
                RegenerateMana(tickInterval);

                // Diagnostic: log mana progress every 50 ticks (5 seconds)
                if (combatTickCount % 50 == 1)
                {
                    Debug.Log($"[Unit] {Data.GetDisplayName()} mana={CurrentMana:F1}/{MaxMana} ({GetManaPercentage()*100:F0}%) regenRate={ManaRegenPerSecond:F1}/s skills={instanceSkills?.Length ?? 0}");
                }
            }
            else if (combatTickCount == 1)
            {
                Debug.LogWarning($"[Unit] {Data.GetDisplayName()} HasSkill=false (instanceSkills={instanceSkills?.Length ?? -1}, Data.skills={Data.skills?.Length ?? -1})");
            }

            // Find or validate target
            if (CurrentTarget == null || !CurrentTarget.IsActive)
            {
                CurrentTarget = FindNearestMonster();
                outOfRangeTicks = 0;
                if (CurrentTarget != null)
                {
                    Debug.Log($"[Unit] {Data.GetDisplayName()} acquired target: {CurrentTarget.Data.monsterName} at distance {Vector3.Distance(transform.position, CurrentTarget.transform.position):F2} (worldRange={WorldAttackRange:F2})");
                }
                else if (combatTickCount % 30 == 1)
                {
                    // Periodic diagnostic: log when no target found
                    int monsterCount = LottoDefense.Monsters.MonsterManager.Instance != null
                        ? LottoDefense.Monsters.MonsterManager.Instance.ActiveMonsterCount : -1;
                    Debug.Log($"[Unit] {Data.GetDisplayName()} no target found (pos={transform.position}, worldRange={WorldAttackRange:F2}, monsters={monsterCount})");
                }
            }

            // Attack if ready and has target
            if (CurrentTarget != null && currentCooldown <= 0f)
            {
                float distance = Vector3.Distance(transform.position, CurrentTarget.transform.position);

                if (distance <= WorldAttackRange)
                {
                    outOfRangeTicks = 0;
                    ExecuteAttack();
                    currentCooldown = attackCooldown;
                }
                else
                {
                    outOfRangeTicks++;
                    // If target has been out of range too long, find a new one
                    if (outOfRangeTicks >= MAX_OUT_OF_RANGE_TICKS)
                    {
                        Monster newTarget = FindNearestMonster();
                        if (newTarget != null)
                        {
                            CurrentTarget = newTarget;
                            outOfRangeTicks = 0;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Find the nearest active monster within attack range.
        /// </summary>
        private Monster FindNearestMonster()
        {
            if (LottoDefense.Monsters.MonsterManager.Instance == null) return null;

            var activeMonsters = LottoDefense.Monsters.MonsterManager.Instance.GetActiveMonstersList();
            Monster nearest = null;
            float nearestDistance = float.MaxValue;
            float closestOutOfRange = float.MaxValue; // Track closest monster even if out of range

            foreach (Monster monster in activeMonsters)
            {
                if (!monster.IsActive) continue;

                float distance = Vector3.Distance(transform.position, monster.transform.position);
                if (distance <= WorldAttackRange && distance < nearestDistance)
                {
                    nearest = monster;
                    nearestDistance = distance;
                }
                else if (distance < closestOutOfRange)
                {
                    closestOutOfRange = distance;
                }
            }

            // Diagnostic: log when monsters exist but none in range (once per 50 ticks)
            if (nearest == null && activeMonsters.Count > 0 && combatTickCount % 50 == 1)
            {
                Debug.Log($"[Unit] {Data.GetDisplayName()} has {activeMonsters.Count} monsters but nearest is {closestOutOfRange:F2} away (worldRange={WorldAttackRange:F2})");
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
            LottoDefense.VFX.VFXManager.Instance?.PlayMissileEffect(transform.position, targetPos, Data.rarity);
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
                        LottoDefense.VFX.VFXManager.Instance?.PlaySplashEffect(targetPos, monster.transform.position);
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

                if (distance <= WorldAttackRange)
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
                LottoDefense.VFX.VFXManager.Instance?.PlayChainEffect(currentTarget.transform.position, nextTarget.transform.position);

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
        /// Reset combat state (called when combat ends).
        /// </summary>
        public void ResetCombat()
        {
            CurrentTarget = null;
            currentCooldown = 0f;
            combatTickCount = 0;
            outOfRangeTicks = 0;

            // Reset mana and update UI
            CurrentMana = 0f;
            OnManaChanged?.Invoke(CurrentMana, MaxMana);

            // Reset skill cooldowns
            if (instanceSkills != null)
            {
                foreach (var skill in instanceSkills)
                {
                    skill.ResetCooldown();
                }
            }
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
            if (CurrentMana < MaxMana)
            {
                float manaGain = ManaRegenPerSecond * deltaTime;
                CurrentMana = Mathf.Min(CurrentMana + manaGain, MaxMana);

                // Fire mana changed event
                OnManaChanged?.Invoke(CurrentMana, MaxMana);
            }

            // Auto-trigger skill when mana is full
            // Retries each tick in case skill cooldown wasn't ready on the previous tick
            if (CurrentMana >= MaxMana)
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
            if (instanceSkills == null || instanceSkills.Length == 0)
            {
                Debug.LogWarning($"[Unit] {Data?.GetDisplayName()} has no skills to trigger!");
                return;
            }

            // Find first active skill that can be triggered
            UnitSkill skillToActivate = null;
            foreach (var skill in instanceSkills)
            {
                if (skill.skillType == SkillType.Active)
                {
                    Debug.Log($"[Unit] {Data.GetDisplayName()} trying Active skill '{skill.skillName}' (onCooldown={skill.IsOnCooldown}, cd={skill.currentCooldown:F1})");
                    if (skill.TryActivate())
                    {
                        skillToActivate = skill;
                        break;
                    }
                }
            }

            if (skillToActivate != null)
            {
                Debug.Log($"[Unit] {Data.GetDisplayName()} auto-triggered skill: {skillToActivate.skillName}");

                // Apply skill effect
                ApplySkillEffect(skillToActivate);

                // Visual feedback for skill activation (single unit only)
                LottoDefense.VFX.VFXManager.Instance?.ShowFloatingText(
                    transform.position + Vector3.up * 0.5f, skillToActivate.skillName,
                    UnitData.GetRarityColor(Data.rarity));

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

            // Apply crowd control to all active monsters in range
            if (skill.slowMultiplier > 0f || skill.freezeDuration > 0f)
            {
                ApplyCrowdControl(skill);
            }

            // Visual effect
            if (skill.vfxPrefab != null)
            {
                GameObject vfx = Instantiate(skill.vfxPrefab, transform.position, Quaternion.identity);
                Destroy(vfx, 2f);
            }

            Debug.Log($"[Unit] Applied skill effect: {skill.skillName} (DMG×{skill.damageMultiplier}, AS×{skill.attackSpeedMultiplier}, Slow={skill.slowMultiplier}, Freeze={skill.freezeDuration}s, Duration: {skill.effectDuration}s)");
        }

        /// <summary>
        /// Apply crowd control (slow/freeze) to all active monsters within attack range.
        /// </summary>
        private void ApplyCrowdControl(UnitSkill skill)
        {
            if (MonsterManager.Instance == null) return;

            var monsters = MonsterManager.Instance.GetActiveMonstersList();
            foreach (var monster in monsters)
            {
                if (monster == null || !monster.IsActive) continue;

                float distance = Vector3.Distance(transform.position, monster.transform.position);
                if (distance > WorldAttackRange) continue;

                if (skill.freezeDuration > 0f)
                {
                    monster.ApplyFreeze(skill.freezeDuration);
                }
                else if (skill.slowMultiplier > 0f)
                {
                    monster.ApplySlow(skill.slowMultiplier, skill.ccDuration);
                }
            }
        }

        /// <summary>
        /// Apply temporary damage buff using multiplier tracking (safe with upgrades).
        /// </summary>
        private System.Collections.IEnumerator ApplyTemporaryDamageBuff(float multiplier, float duration)
        {
            activeDamageBuff = multiplier;
            RecalculateStats();
            Debug.Log($"[Unit] Damage buff applied: x{multiplier} for {duration}s (Attack: {CurrentAttack})");

            yield return new WaitForSeconds(duration);

            activeDamageBuff = 1f;
            RecalculateStats();
            Debug.Log($"[Unit] Damage buff expired (Attack: {CurrentAttack})");
        }

        /// <summary>
        /// Apply temporary attack speed buff using multiplier tracking (safe with upgrades).
        /// </summary>
        private System.Collections.IEnumerator ApplyTemporaryAttackSpeedBuff(float multiplier, float duration)
        {
            activeSpeedBuff = multiplier;
            RecalculateStats();
            Debug.Log($"[Unit] Attack speed buff applied: x{multiplier} for {duration}s (Cooldown: {attackCooldown:F2}s)");

            yield return new WaitForSeconds(duration);

            activeSpeedBuff = 1f;
            RecalculateStats();
            Debug.Log($"[Unit] Attack speed buff expired (Cooldown: {attackCooldown:F2}s)");
        }

        /// <summary>
        /// Recalculate attack and speed from base stats + upgrades + active buffs.
        /// </summary>
        private void RecalculateStats()
        {
            if (Data == null) return;

            float attackMultiplier = 1f;
            float speedMultiplier = 1f;

            if (UnitUpgradeManager.Instance != null)
            {
                attackMultiplier = UnitUpgradeManager.Instance.GetAttackMultiplier(AttackUpgradeLevel, Data);
                speedMultiplier = UnitUpgradeManager.Instance.GetAttackSpeedMultiplier(AttackSpeedUpgradeLevel, Data);
            }

            CurrentAttack = Mathf.RoundToInt(Data.attack * attackMultiplier * activeDamageBuff);
            CurrentAttackSpeed = Data.attackSpeed * speedMultiplier * activeSpeedBuff;
            attackCooldown = 1f / CurrentAttackSpeed;
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
            if (instanceSkills == null) return;

            foreach (var skill in instanceSkills)
            {
                skill.UpdateCooldown(deltaTime);
            }
        }
        #endregion

        #region Shared Materials
        private static Material s_defaultSpriteMaterial;
        private static Material DefaultSpriteMaterial
        {
            get
            {
                if (s_defaultSpriteMaterial == null)
                    s_defaultSpriteMaterial = new Material(Shader.Find("Sprites/Default"));
                return s_defaultSpriteMaterial;
            }
        }
        #endregion

        #region Attack Range Indicator
        private static Texture2D s_circleTexture;
        private static Sprite s_circleSprite;

        private static Sprite GetOrCreateCircleSprite()
        {
            if (s_circleSprite != null) return s_circleSprite;

            const int size = 256;
            s_circleTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            s_circleTexture.filterMode = FilterMode.Bilinear;
            s_circleTexture.wrapMode = TextureWrapMode.Clamp;

            float center = size * 0.5f;
            Color[] pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center + 0.5f;
                    float dy = y - center + 0.5f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy) / center;

                    float alpha = 0f;
                    if (dist <= 1f)
                    {
                        // Soft radial gradient: subtle fill from center to edge
                        alpha = Mathf.Lerp(0.03f, 0.12f, dist * dist);

                        // Brighter ring near edge (82%-94%)
                        if (dist > 0.82f && dist <= 0.94f)
                        {
                            float edge = (dist - 0.82f) / 0.12f;
                            alpha = Mathf.Lerp(alpha, 0.4f, Mathf.Sqrt(edge));
                        }

                        // Soft outer falloff (94%-100%)
                        if (dist > 0.94f)
                        {
                            float falloff = (dist - 0.94f) / 0.06f;
                            alpha = 0.4f * (1f - falloff * falloff);
                        }
                    }

                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            s_circleTexture.SetPixels(pixels);
            s_circleTexture.Apply();

            s_circleSprite = Sprite.Create(
                s_circleTexture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                size / 2f
            );

            return s_circleSprite;
        }

        private void ShowAttackRange()
        {
            if (Data == null) return;

            if (rangeIndicator == null)
            {
                rangeIndicator = new GameObject("RangeIndicator");
                rangeIndicator.transform.SetParent(transform);
                rangeIndicator.transform.localPosition = Vector3.zero;
                rangeIndicator.transform.localRotation = Quaternion.identity;

                // Compensate for parent scale so range matches world-space attack range
                float parentScale = transform.localScale.x;
                float compensate = parentScale > 0.001f ? 1f / parentScale : 1f;
                rangeIndicator.transform.localScale = Vector3.one * compensate;

                Color rangeColor = UnitData.GetRarityColor(Data.rarity);
                float radius = WorldAttackRange;

                // Filled gradient circle
                GameObject fillObj = new GameObject("Fill");
                fillObj.transform.SetParent(rangeIndicator.transform, false);
                fillObj.transform.localPosition = Vector3.zero;
                fillObj.transform.localScale = Vector3.one * radius;

                SpriteRenderer fillSR = fillObj.AddComponent<SpriteRenderer>();
                fillSR.sprite = GetOrCreateCircleSprite();
                fillSR.color = rangeColor;
                fillSR.sortingOrder = -1;

                // Border ring
                LineRenderer border = rangeIndicator.AddComponent<LineRenderer>();
                border.useWorldSpace = false;
                border.loop = true;
                border.startWidth = 0.04f;
                border.endWidth = 0.04f;
                border.positionCount = 64;
                border.material = DefaultSpriteMaterial;
                border.sortingOrder = -1;

                Color borderColor = rangeColor;
                borderColor.a = 0.6f;
                border.startColor = borderColor;
                border.endColor = borderColor;

                float step = 360f / 64f;
                for (int i = 0; i < 64; i++)
                {
                    float angle = i * step * Mathf.Deg2Rad;
                    border.SetPosition(i, new Vector3(
                        Mathf.Cos(angle) * radius,
                        Mathf.Sin(angle) * radius,
                        0f
                    ));
                }
            }

            rangeIndicator.SetActive(true);
        }

        private void HideAttackRange()
        {
            if (rangeIndicator != null)
            {
                rangeIndicator.SetActive(false);
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
        /// Set attack upgrade level directly (for rarity-wide upgrades).
        /// </summary>
        public void SetAttackUpgradeLevel(int level)
        {
            AttackUpgradeLevel = level;
            RecalculateStats();
        }

        /// <summary>
        /// Set attack speed upgrade level directly (for rarity-wide upgrades).
        /// </summary>
        public void SetAttackSpeedUpgradeLevel(int level)
        {
            AttackSpeedUpgradeLevel = level;
            RecalculateStats();
        }

        #endregion
    }
}
