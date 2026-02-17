using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using LottoDefense.Gameplay;
using LottoDefense.Units;
using LottoDefense.Monsters;

namespace LottoDefense.Editor
{
    /// <summary>
    /// Import CSV data from Google Sheets back into Unity ScriptableObjects.
    /// Unity Menu: Tools → Lotto Defense → Import Data from CSV
    /// </summary>
    public class DataImporter : EditorWindow
    {
        private static string importPath = "Assets/Data/CSV/";

        [MenuItem("Tools/Lotto Defense/Import Data from CSV")]
        public static void ImportAllData()
        {
            if (!Directory.Exists(importPath))
            {
                EditorUtility.DisplayDialog("Error", $"CSV directory not found:\n{importPath}\n\nPlease export data first!", "OK");
                return;
            }

            bool confirmed = EditorUtility.DisplayDialog(
                "Import CSV Data",
                "This will overwrite current game balance data with CSV values.\n\nContinue?",
                "Yes, Import",
                "Cancel"
            );

            if (!confirmed) return;

            // Load configs
            GameBalanceConfig config = AssetDatabase.LoadAssetAtPath<GameBalanceConfig>("Assets/Data/GameBalanceConfig.asset");
            RoundConfig roundConfig = AssetDatabase.LoadAssetAtPath<RoundConfig>("Assets/Resources/RoundConfig.asset");

            if (config == null)
            {
                Debug.LogError("[DataImporter] GameBalanceConfig not found!");
                return;
            }

            // Import each data type
            ImportUnits(config);
            ImportSkills(config);
            ImportMonsters(config);
            ImportGameSettings(config);
            
            if (roundConfig != null)
            {
                ImportRounds(roundConfig);
            }

            // Save changes
            EditorUtility.SetDirty(config);
            if (roundConfig != null) EditorUtility.SetDirty(roundConfig);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("✅ [DataImporter] All CSV data imported successfully!");
            EditorUtility.DisplayDialog("Import Complete", "CSV data has been imported into Unity!\n\nCheck Console for details.", "OK");
        }

        private static void ImportUnits(GameBalanceConfig config)
        {
            string filePath = importPath + "Units.csv";
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"[DataImporter] Units.csv not found");
                return;
            }

            string[] lines = File.ReadAllLines(filePath);
            if (lines.Length < 2) return; // Header + at least 1 data row

            config.units.Clear();

            for (int i = 1; i < lines.Length; i++) // Skip header
            {
                string[] values = ParseCSVLine(lines[i]);
                if (values.Length < 10) continue;

                var unit = new GameBalanceConfig.UnitBalance
                {
                    unitName = values[0],
                    rarity = ParseEnum<Rarity>(values[1]),
                    attack = ParseInt(values[2]),
                    attackSpeed = ParseFloat(values[3]),
                    attackRange = ParseFloat(values[4]),
                    attackPattern = ParseEnum<AttackPattern>(values[5]),
                    splashRadius = ParseFloat(values[6]),
                    maxTargets = ParseInt(values[7]),
                    upgradeCost = ParseInt(values[8]),
                    skillIds = values[9].Split(';').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList()
                };

                config.units.Add(unit);
            }

            Debug.Log($"[DataImporter] ✅ Imported {config.units.Count} units");
        }

        private static void ImportSkills(GameBalanceConfig config)
        {
            string filePath = importPath + "Skills.csv";
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"[DataImporter] Skills.csv not found");
                return;
            }

            string[] lines = File.ReadAllLines(filePath);
            if (lines.Length < 2) return;

            config.skillPresets.Clear();

            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = ParseCSVLine(lines[i]);
                if (values.Length < 13) continue;

                var preset = new GameBalanceConfig.SkillPreset
                {
                    skillId = values[0],
                    skill = new GameBalanceConfig.SkillBalance
                    {
                        skillName = values[1],
                        skillType = ParseEnum<SkillType>(values[2]),
                        cooldownDuration = ParseFloat(values[3]),
                        damageMultiplier = ParseFloat(values[4]),
                        rangeMultiplier = ParseFloat(values[5]),
                        attackSpeedMultiplier = ParseFloat(values[6]),
                        effectDuration = ParseFloat(values[7]),
                        targetCount = ParseInt(values[8]),
                        aoeRadius = ParseFloat(values[9]),
                        slowMultiplier = ParseFloat(values[10]),
                        freezeDuration = ParseFloat(values[11]),
                        ccDuration = ParseFloat(values[12])
                    }
                };

                config.skillPresets.Add(preset);
            }

            Debug.Log($"[DataImporter] ✅ Imported {config.skillPresets.Count} skills");
        }

        private static void ImportMonsters(GameBalanceConfig config)
        {
            string filePath = importPath + "Monsters.csv";
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"[DataImporter] Monsters.csv not found");
                return;
            }

            string[] lines = File.ReadAllLines(filePath);
            if (lines.Length < 2) return;

            config.monsters.Clear();

            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = ParseCSVLine(lines[i]);
                if (values.Length < 9) continue;

                var monster = new GameBalanceConfig.MonsterBalance
                {
                    monsterName = values[0],
                    type = ParseEnum<MonsterType>(values[1]),
                    maxHealth = ParseInt(values[2]),
                    attack = ParseInt(values[3]),
                    defense = ParseInt(values[4]),
                    moveSpeed = ParseFloat(values[5]),
                    goldReward = ParseInt(values[6]),
                    healthScaling = ParseFloat(values[7]),
                    defenseScaling = ParseFloat(values[8])
                };

                config.monsters.Add(monster);
            }

            Debug.Log($"[DataImporter] ✅ Imported {config.monsters.Count} monsters");
        }

        private static void ImportGameSettings(GameBalanceConfig config)
        {
            string filePath = importPath + "GameSettings.csv";
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"[DataImporter] GameSettings.csv not found");
                return;
            }

            string[] lines = File.ReadAllLines(filePath);
            if (lines.Length < 2) return;

            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = ParseCSVLine(lines[i]);
                if (values.Length < 2) continue;

                string setting = values[0];
                string value = values[1];

                switch (setting)
                {
                    case "preparationTime":
                        config.gameRules.preparationTime = ParseInt(value);
                        break;
                    case "combatTime":
                        config.gameRules.combatTime = ParseInt(value);
                        break;
                    case "startingGold":
                        config.gameRules.startingGold = ParseInt(value);
                        break;
                    case "summonCost":
                        config.gameRules.summonCost = ParseInt(value);
                        break;
                    case "maxMonsterCount":
                        config.gameRules.maxMonsterCount = ParseInt(value);
                        break;
                    case "spawnRate":
                        config.gameRules.spawnRate = ParseFloat(value);
                        break;
                    case "normalRate":
                        config.spawnRates.normalRate = ParseFloat(value);
                        break;
                    case "rareRate":
                        config.spawnRates.rareRate = ParseFloat(value);
                        break;
                    case "epicRate":
                        config.spawnRates.epicRate = ParseFloat(value);
                        break;
                    case "legendaryRate":
                        config.spawnRates.legendaryRate = ParseFloat(value);
                        break;
                    case "sellGoldNormal":
                        config.sellGoldNormal = ParseInt(value);
                        break;
                    case "sellGoldRare":
                        config.sellGoldRare = ParseInt(value);
                        break;
                    case "sellGoldEpic":
                        config.sellGoldEpic = ParseInt(value);
                        break;
                    case "sellGoldLegendary":
                        config.sellGoldLegendary = ParseInt(value);
                        break;
                }
            }

            Debug.Log($"[DataImporter] ✅ Imported game settings");
        }

        private static void ImportRounds(RoundConfig roundConfig)
        {
            string filePath = importPath + "Rounds.csv";
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"[DataImporter] Rounds.csv not found");
                return;
            }

            string[] lines = File.ReadAllLines(filePath);
            if (lines.Length < 2) return;

            // Note: RoundConfig uses a list, we need to use reflection or create new configs
            // For now, log a warning that rounds need manual update
            Debug.LogWarning("[DataImporter] ⚠️ Rounds.csv found, but automatic round import not yet implemented. Please update RoundConfig manually.");
        }

        // Helper methods
        private static string[] ParseCSVLine(string line)
        {
            // Simple CSV parser (handles quoted strings)
            List<string> values = new List<string>();
            bool inQuotes = false;
            string current = "";

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    values.Add(current.Trim());
                    current = "";
                }
                else
                {
                    current += c;
                }
            }

            values.Add(current.Trim());
            return values.ToArray();
        }

        private static int ParseInt(string value)
        {
            int result;
            int.TryParse(value, out result);
            return result;
        }

        private static float ParseFloat(string value)
        {
            float result;
            float.TryParse(value, out result);
            return result;
        }

        private static T ParseEnum<T>(string value) where T : struct
        {
            T result;
            System.Enum.TryParse(value, out result);
            return result;
        }
    }
}
