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

            currentUnit = unit;

            if (statsContainer != null) statsContainer.SetActive(true);
            if (emptyStateText != null) emptyStateText.gameObject.SetActive(false);

            UpdateStats();
        }

        public void Hide()
        {
            currentUnit = null;

            if (statsContainer != null) statsContainer.SetActive(false);
            if (emptyStateText != null) emptyStateText.gameObject.SetActive(true);
        }
        #endregion

        #region Unity Lifecycle
        private void Update()
        {
            if (currentUnit != null)
            {
                UpdateStats();
            }
        }
        #endregion

        #region Stat Display
        private void UpdateStats()
        {
            if (currentUnit == null || currentUnit.Data == null) return;

            // Portrait color
            if (portraitImage != null)
            {
                portraitImage.color = UnitData.GetRarityColor(currentUnit.Data.rarity);
            }

            // Unit name
            if (unitNameText != null)
            {
                unitNameText.text = currentUnit.Data.GetDisplayName();
                unitNameText.color = UnitData.GetRarityColor(currentUnit.Data.rarity);
            }

            // Upgrade levels
            int atkLevel = 0;
            int spdLevel = 0;
            if (UnitUpgradeManager.Instance != null)
            {
                atkLevel = UnitUpgradeManager.Instance.GetRarityAttackLevel(currentUnit.Data.rarity);
                spdLevel = UnitUpgradeManager.Instance.GetRaritySpeedLevel(currentUnit.Data.rarity);
            }

            // Attack
            if (attackText != null)
            {
                string atkBonus = atkLevel > 0 ? $"<color=#FFD700>(+{atkLevel})</color>" : "";
                attackText.text = $"ATK: {currentUnit.CurrentAttack}{atkBonus}";
            }

            // Attack Speed
            if (speedText != null)
            {
                string spdBonus = spdLevel > 0 ? $"<color=#FFD700>(+{spdLevel})</color>" : "";
                speedText.text = $"SPD: {currentUnit.CurrentAttackSpeed:F1}{spdBonus}";
            }

            // Range
            if (rangeText != null)
            {
                rangeText.text = $"RNG: {currentUnit.Data.attackRange:F1}";
            }

            // Attack Pattern
            if (patternText != null)
            {
                patternText.text = $"TYPE: {GetPatternDisplayName(currentUnit.Data.attackPattern)}";
            }

            // Defense
            if (defenseText != null)
            {
                defenseText.text = $"DEF: {currentUnit.Data.defense}";
            }

            // Skill + Mana bar
            if (currentUnit.HasSkill)
            {
                if (manaBarContainer != null) manaBarContainer.SetActive(true);

                if (skillText != null && currentUnit.Data.skills.Length > 0)
                {
                    skillText.text = $"SKILL: {currentUnit.Data.skills[0].skillName}";
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
                if (skillText != null) skillText.text = "";
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
