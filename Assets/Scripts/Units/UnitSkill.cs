using UnityEngine;
using System;

namespace LottoDefense.Units
{
    /// <summary>
    /// Type of unit skill.
    /// </summary>
    public enum SkillType
    {
        Active,     // Manually triggered skill
        Passive,    // Always active effect
        OnHit,      // Triggered when unit attacks
        OnKill      // Triggered when unit kills a monster
    }

    /// <summary>
    /// Defines a unit's special skill/ability.
    /// Each unit can have one or more skills with cooldowns and effects.
    /// </summary>
    [System.Serializable]
    public class UnitSkill
    {
        #region Serialized Fields
        [Header("Skill Identity")]
        [Tooltip("Skill display name")]
        public string skillName = "기본 스킬";

        [Tooltip("Skill description shown in UI")]
        [TextArea(2, 4)]
        public string description = "스킬 설명";

        [Tooltip("Type of skill activation")]
        public SkillType skillType = SkillType.Active;

        [Header("Cooldown Settings")]
        [Tooltip("Cooldown duration in seconds (0 = no cooldown)")]
        public float cooldownDuration = 10f;

        [Tooltip("Initial cooldown on spawn (seconds)")]
        public float initialCooldown = 0f;

        [Header("Effect Settings")]
        [Tooltip("Damage multiplier for attack skills (1.5 = 150% damage)")]
        public float damageMultiplier = 1.0f;

        [Tooltip("Range multiplier for range skills (2.0 = 200% range)")]
        public float rangeMultiplier = 1.0f;

        [Tooltip("Attack speed multiplier (1.5 = 150% faster)")]
        public float attackSpeedMultiplier = 1.0f;

        [Tooltip("Duration of skill effect in seconds (for buffs)")]
        public float effectDuration = 0f;

        [Tooltip("Number of targets affected (0 = single target)")]
        public int targetCount = 0;

        [Tooltip("Area of effect radius (0 = no AOE)")]
        public float aoeRadius = 0f;

        [Header("Visual Settings")]
        [Tooltip("Skill icon (optional)")]
        public Sprite skillIcon;

        [Tooltip("VFX prefab to spawn when skill activates")]
        public GameObject vfxPrefab;

        [Tooltip("VFX color tint")]
        public Color vfxColor = Color.white;
        #endregion

        #region Runtime State
        /// <summary>
        /// Current cooldown remaining (seconds).
        /// </summary>
        [NonSerialized]
        public float currentCooldown;

        /// <summary>
        /// Whether skill is currently on cooldown.
        /// </summary>
        public bool IsOnCooldown => currentCooldown > 0f;

        /// <summary>
        /// Cooldown progress (0 = ready, 1 = just used).
        /// </summary>
        public float CooldownProgress => cooldownDuration > 0f ? currentCooldown / cooldownDuration : 0f;
        #endregion

        #region Events
        /// <summary>
        /// Fired when skill is activated.
        /// </summary>
        [NonSerialized]
        public Action<UnitSkill> OnSkillActivated;

        /// <summary>
        /// Fired when skill cooldown completes.
        /// </summary>
        [NonSerialized]
        public Action<UnitSkill> OnCooldownComplete;
        #endregion

        #region Public Methods
        /// <summary>
        /// Initialize skill runtime state.
        /// </summary>
        public void Initialize()
        {
            currentCooldown = initialCooldown;
        }

        /// <summary>
        /// Try to activate this skill.
        /// </summary>
        /// <returns>True if skill was activated, false if on cooldown</returns>
        public bool TryActivate()
        {
            if (IsOnCooldown)
            {
                Debug.Log($"[UnitSkill] {skillName} is on cooldown ({currentCooldown:F1}s remaining)");
                return false;
            }

            // Start cooldown
            currentCooldown = cooldownDuration;

            // Fire event
            OnSkillActivated?.Invoke(this);

            Debug.Log($"[UnitSkill] {skillName} activated! Cooldown: {cooldownDuration}s");
            return true;
        }

        /// <summary>
        /// Update skill cooldown (call from Update loop).
        /// </summary>
        /// <param name="deltaTime">Time since last frame</param>
        public void UpdateCooldown(float deltaTime)
        {
            if (currentCooldown > 0f)
            {
                float previousCooldown = currentCooldown;
                currentCooldown -= deltaTime;

                // Clamp to 0
                if (currentCooldown <= 0f)
                {
                    currentCooldown = 0f;

                    // Fire cooldown complete event
                    OnCooldownComplete?.Invoke(this);

                    Debug.Log($"[UnitSkill] {skillName} cooldown complete!");
                }
            }
        }

        /// <summary>
        /// Reset cooldown to 0 (skill ready immediately).
        /// </summary>
        public void ResetCooldown()
        {
            currentCooldown = 0f;
        }

        /// <summary>
        /// Check if this skill should trigger on hit.
        /// </summary>
        public bool ShouldTriggerOnHit()
        {
            return skillType == SkillType.OnHit && !IsOnCooldown;
        }

        /// <summary>
        /// Check if this skill should trigger on kill.
        /// </summary>
        public bool ShouldTriggerOnKill()
        {
            return skillType == SkillType.OnKill && !IsOnCooldown;
        }

        /// <summary>
        /// Check if this is a passive skill (always active).
        /// </summary>
        public bool IsPassive()
        {
            return skillType == SkillType.Passive;
        }
        #endregion
    }
}
