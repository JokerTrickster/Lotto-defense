using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using LottoDefense.Gameplay;
using LottoDefense.Units;

namespace LottoDefense.Editor
{
    /// <summary>
    /// Export game data to CSV for Google Sheets integration.
    /// Unity Menu: Tools → Lotto Defense → Export Data to CSV
    /// </summary>
    public class DataExporter : EditorWindow
    {
        private static string exportPath = "Assets/Data/CSV/";

        [MenuItem("Tools/Lotto Defense/Export Data to CSV")]
        public static void ExportAllData()
        {
            // Create CSV directory if not exists
            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath);
                Debug.Log($"[DataExporter] Created directory: {exportPath}");
            }

            // Load GameBalanceConfig
            GameBalanceConfig config = AssetDatabase.LoadAssetAtPath<GameBalanceConfig>("Assets/Data/GameBalanceConfig.asset");
            if (config == null)
            {
                Debug.LogError("[DataExporter] GameBalanceConfig not found!");
                return;
            }

            // Export each data type
            ExportUnits(config);
            ExportSkills(config);
            ExportMonsters(config);
            ExportGameSettings(config);
            ExportRounds();

            AssetDatabase.Refresh();
            Debug.Log($"✅ [DataExporter] All data exported to {exportPath}");
            EditorUtility.DisplayDialog("Export Complete", $"CSV files exported to:\n{exportPath}", "OK");
        }

        private static void ExportUnits(GameBalanceConfig config)
        {
            StringBuilder csv = new StringBuilder();
            
            // Header
            csv.AppendLine("unitName,rarity,attack,attackSpeed,attackRange,attackPattern,splashRadius,maxTargets,upgradeCost,skillIds");

            // Data rows
            foreach (var unit in config.units)
            {
                string skillIds = string.Join(";", unit.skillIds); // Use ; instead of , for CSV
                csv.AppendLine($"{unit.unitName},{unit.rarity},{unit.attack},{unit.attackSpeed},{unit.attackRange}," +
                    $"{unit.attackPattern},{unit.splashRadius},{unit.maxTargets},{unit.upgradeCost},\"{skillIds}\"");
            }

            File.WriteAllText(exportPath + "Units.csv", csv.ToString(), Encoding.UTF8);
            Debug.Log("[DataExporter] ✅ Units.csv exported");
        }

        private static void ExportSkills(GameBalanceConfig config)
        {
            StringBuilder csv = new StringBuilder();
            
            // Header
            csv.AppendLine("skillId,skillName,skillType,cooldownDuration,damageMultiplier,rangeMultiplier,attackSpeedMultiplier," +
                "effectDuration,targetCount,aoeRadius,slowMultiplier,freezeDuration,ccDuration");

            // Data rows
            foreach (var preset in config.skillPresets)
            {
                var skill = preset.skill;
                csv.AppendLine($"{preset.skillId},\"{skill.skillName}\",{skill.skillType},{skill.cooldownDuration}," +
                    $"{skill.damageMultiplier},{skill.rangeMultiplier},{skill.attackSpeedMultiplier}," +
                    $"{skill.effectDuration},{skill.targetCount},{skill.aoeRadius}," +
                    $"{skill.slowMultiplier},{skill.freezeDuration},{skill.ccDuration}");
            }

            File.WriteAllText(exportPath + "Skills.csv", csv.ToString(), Encoding.UTF8);
            Debug.Log("[DataExporter] ✅ Skills.csv exported");
        }

        private static void ExportMonsters(GameBalanceConfig config)
        {
            StringBuilder csv = new StringBuilder();
            
            // Header
            csv.AppendLine("monsterName,type,maxHealth,attack,defense,moveSpeed,goldReward,healthScaling,defenseScaling");

            // Data rows
            foreach (var monster in config.monsters)
            {
                csv.AppendLine($"\"{monster.monsterName}\",{monster.type},{monster.maxHealth},{monster.attack}," +
                    $"{monster.defense},{monster.moveSpeed},{monster.goldReward}," +
                    $"{monster.healthScaling},{monster.defenseScaling}");
            }

            File.WriteAllText(exportPath + "Monsters.csv", csv.ToString(), Encoding.UTF8);
            Debug.Log("[DataExporter] ✅ Monsters.csv exported");
        }

        private static void ExportGameSettings(GameBalanceConfig config)
        {
            StringBuilder csv = new StringBuilder();
            
            // Header
            csv.AppendLine("setting,value");

            // Game Rules
            csv.AppendLine($"preparationTime,{config.gameRules.preparationTime}");
            csv.AppendLine($"combatTime,{config.gameRules.combatTime}");
            csv.AppendLine($"startingGold,{config.gameRules.startingGold}");
            csv.AppendLine($"summonCost,{config.gameRules.summonCost}");
            csv.AppendLine($"maxMonsterCount,{config.gameRules.maxMonsterCount}");
            csv.AppendLine($"spawnRate,{config.gameRules.spawnRate}");

            // Spawn Rates
            csv.AppendLine($"normalRate,{config.spawnRates.normalRate}");
            csv.AppendLine($"rareRate,{config.spawnRates.rareRate}");
            csv.AppendLine($"epicRate,{config.spawnRates.epicRate}");
            csv.AppendLine($"legendaryRate,{config.spawnRates.legendaryRate}");

            // Sell Gold
            csv.AppendLine($"sellGoldNormal,{config.sellGoldNormal}");
            csv.AppendLine($"sellGoldRare,{config.sellGoldRare}");
            csv.AppendLine($"sellGoldEpic,{config.sellGoldEpic}");
            csv.AppendLine($"sellGoldLegendary,{config.sellGoldLegendary}");

            File.WriteAllText(exportPath + "GameSettings.csv", csv.ToString(), Encoding.UTF8);
            Debug.Log("[DataExporter] ✅ GameSettings.csv exported");
        }

        private static void ExportRounds()
        {
            RoundConfig roundConfig = AssetDatabase.LoadAssetAtPath<RoundConfig>("Assets/Resources/RoundConfig.asset");
            if (roundConfig == null)
            {
                Debug.LogWarning("[DataExporter] RoundConfig not found, skipping Rounds.csv");
                return;
            }

            StringBuilder csv = new StringBuilder();
            
            // Header
            csv.AppendLine("roundNumber,monsterName,totalMonsters,spawnInterval,spawnDuration");

            // Data rows - export all rounds
            for (int i = 1; i <= roundConfig.TotalRounds; i++)
            {
                var config = roundConfig.GetRoundConfig(i);
                string monsterName = config.monsterData != null ? config.monsterData.monsterName : "None";
                csv.AppendLine($"{config.roundNumber},\"{monsterName}\",{config.totalMonsters}," +
                    $"{config.spawnInterval},{config.spawnDuration}");
            }

            File.WriteAllText(exportPath + "Rounds.csv", csv.ToString(), Encoding.UTF8);
            Debug.Log("[DataExporter] ✅ Rounds.csv exported");
        }
    }
}
