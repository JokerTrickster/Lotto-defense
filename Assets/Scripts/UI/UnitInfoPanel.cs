using UnityEngine;
using UnityEngine.UI;
using LottoDefense.Units;

namespace LottoDefense.UI
{
    /// <summary>
    /// StarCraft-style unit info panel shown in the bottom UI when a unit is selected.
    /// Displays portrait, name, stats, attack pattern, skill, and mana bar.
    /// When no unit is selected, shows an empty state message.
    /// </summary>
    public class UnitInfoPanel : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private Image portraitImage;
        [SerializeField] private Text unitNameText;
        [SerializeField] private Text upgradeLevelText;
        [SerializeField] private Text attackText;
        [SerializeField] private Text speedText;
        [SerializeField] private Text rangeText;
        [SerializeField] private Text patternText;
        [SerializeField] private Text defenseText;
        [SerializeField] private Text skillText;
        [SerializeField] private Image manaBarFill;
        [SerializeField] private GameObject manaBarContainer;
        [SerializeField] private GameObject statsContainer;
        [SerializeField] private Text emptyStateText;
        #endregion

        #region Private Fields
        private Unit currentUnit;
        #endregion

        #region Public Methods
        public void Show(Unit unit)
        {
            if (unit == null) return;

            // Unsubscribe from previous unit
            if (currentUnit != null)
            {
                currentUnit.OnManaChanged -= HandleManaChanged;
            }

            currentUnit = unit;
            currentUnit.OnManaChanged += HandleManaChanged;

            if (statsContainer != null) statsContainer.SetActive(true);
            if (emptyStateText != null) emptyStateText.gameObject.SetActive(false);

            UpdateStats();
        }

        public void Hide()
        {
            if (currentUnit != null)
            {
                currentUnit.OnManaChanged -= HandleManaChanged;
            }
            currentUnit = null;

            if (statsContainer != null) statsContainer.SetActive(false);
            if (emptyStateText != null) emptyStateText.gameObject.SetActive(true);
        }

        private void OnDestroy()
        {
            if (currentUnit != null)
            {
                currentUnit.OnManaChanged -= HandleManaChanged;
                currentUnit = null;
            }
        }

        /// <summary>
        /// Call externally after upgrades to refresh displayed stats.
        /// </summary>
        public void RefreshStats()
        {
            if (currentUnit != null)
            {
                UpdateStats();
            }
        }
        #endregion

        #region Event Handlers
        private void HandleManaChanged(float current, float max)
        {
            if (manaBarFill != null)
            {
                manaBarFill.fillAmount = max > 0f ? current / max : 0f;
            }
        }
        #endregion

        #region Stat Display
        private void UpdateStats()
        {
            if (currentUnit == null || currentUnit.Data == null) return;

            Color rarityColor = UnitData.GetRarityColor(currentUnit.Data.rarity);

            if (portraitImage != null)
            {
                portraitImage.color = rarityColor;
            }

            if (unitNameText != null)
            {
                unitNameText.text = currentUnit.Data.unitName;
                unitNameText.color = rarityColor;
            }

            int atkLevel = 0;
            int spdLevel = 0;
            if (UnitUpgradeManager.Instance != null)
            {
                atkLevel = UnitUpgradeManager.Instance.GetRarityAttackLevel(currentUnit.Data.rarity);
                spdLevel = UnitUpgradeManager.Instance.GetRaritySpeedLevel(currentUnit.Data.rarity);
            }

            if (upgradeLevelText != null)
            {
                string atkStr = atkLevel > 0
                    ? $"<color=#FF6B50>\uACF5\uACA9Lv.{atkLevel}</color>"
                    : "<color=#999999>\uACF5\uACA9Lv.0</color>";
                string spdStr = spdLevel > 0
                    ? $"<color=#4DC080>\uACF5\uC18DLv.{spdLevel}</color>"
                    : "<color=#999999>\uACF5\uC18DLv.0</color>";
                upgradeLevelText.text = $"{atkStr} {spdStr}";
            }

            if (attackText != null)
            {
                attackText.text = $"\uACF5\uACA9\uB825 {currentUnit.CurrentAttack}";
            }

            if (speedText != null)
            {
                speedText.text = $"\uACF5\uACA9\uC18D\uB3C4 {currentUnit.CurrentAttackSpeed:F1}";
            }

            if (rangeText != null)
            {
                rangeText.text = $"\uC0AC\uAC70\uB9AC {currentUnit.Data.attackRange:F1}";
            }

            if (patternText != null)
            {
                patternText.text = $"\uD0C0\uC785 {GetPatternDisplayName(currentUnit.Data.attackPattern)}";
            }

            if (defenseText != null)
            {
                defenseText.text = $"\uBC29\uC5B4 {currentUnit.Data.defense}";
            }

            if (currentUnit.HasSkill)
            {
                if (manaBarContainer != null) manaBarContainer.SetActive(true);

                if (skillText != null && currentUnit.Data.skills.Length > 0)
                {
                    UnitSkill activeSkill = null;
                    foreach (var s in currentUnit.Data.skills)
                    {
                        if (s.skillType == SkillType.Active)
                        {
                            activeSkill = s;
                            break;
                        }
                    }
                    UnitSkill displaySkill = activeSkill ?? currentUnit.Data.skills[0];
                    string desc = !string.IsNullOrEmpty(displaySkill.description)
                        ? $" - {displaySkill.description}"
                        : "";
                    skillText.text = $"{displaySkill.skillName}{desc}";
                }

                if (manaBarFill != null)
                {
                    float ratio = currentUnit.MaxMana > 0f ? currentUnit.CurrentMana / currentUnit.MaxMana : 0f;
                    manaBarFill.fillAmount = ratio;
                }
            }
            else
            {
                if (manaBarContainer != null) manaBarContainer.SetActive(false);
                if (skillText != null) skillText.text = "\uC5C6\uC74C";
            }
        }
        #endregion

        #region Helpers
        private static string GetPatternDisplayName(AttackPattern pattern)
        {
            switch (pattern)
            {
                case AttackPattern.SingleTarget: return "\uB2E8\uC77C";
                case AttackPattern.Splash:       return "\uC2A4\uD50C\uB798\uC2DC";
                case AttackPattern.AOE:          return "\uAD11\uC5ED";
                case AttackPattern.Pierce:       return "\uAD00\uD1B5";
                case AttackPattern.Chain:        return "\uC5F0\uC1C4";
                default:                         return pattern.ToString();
            }
        }
        #endregion
    }
}
