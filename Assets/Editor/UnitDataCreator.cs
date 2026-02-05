using UnityEngine;
using UnityEditor;
using LottoDefense.Units;

namespace LottoDefense.Editor
{
    /// <summary>
    /// Editor utility to create default UnitData assets.
    /// Run via menu: Tools > Lotto Defense > Create Default Units
    /// </summary>
    public static class UnitDataCreator
    {
        private const string UNIT_PATH = "Assets/Resources/Units/";

        [MenuItem("Tools/Lotto Defense/Create Default Units")]
        public static void CreateDefaultUnits()
        {
            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Units"))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Units");
            }

            int created = 0;

            // Normal tier units (50% drop rate)
            created += CreateUnit("Soldier", UnitType.Melee, Rarity.Normal, 10, 2, 1.5f, 1.0f,
                "Basic frontline soldier. Reliable damage dealer.");
            created += CreateUnit("Archer", UnitType.Ranged, Rarity.Normal, 8, 1, 3.0f, 1.2f,
                "Basic ranged attacker. Good for backline support.");
            created += CreateUnit("Scout", UnitType.Melee, Rarity.Normal, 6, 1, 1.8f, 1.5f,
                "Fast attacker with low damage but quick attacks.");

            // Rare tier units (30% drop rate)
            created += CreateUnit("Knight", UnitType.Melee, Rarity.Rare, 18, 5, 1.5f, 0.8f,
                "Armored warrior with high defense.");
            created += CreateUnit("Crossbowman", UnitType.Ranged, Rarity.Rare, 15, 2, 4.0f, 0.9f,
                "Powerful ranged attacker with extended range.");
            created += CreateUnit("Assassin", UnitType.Melee, Rarity.Rare, 22, 1, 2.0f, 1.8f,
                "High damage dealer with fast attack speed.");

            // Epic tier units (15% drop rate)
            created += CreateUnit("Paladin", UnitType.Melee, Rarity.Epic, 35, 10, 2.0f, 0.7f,
                "Holy warrior with exceptional defense and damage.");
            created += CreateUnit("Sniper", UnitType.Ranged, Rarity.Epic, 45, 2, 6.0f, 0.5f,
                "Elite marksman with extreme range and power.");
            created += CreateUnit("Berserker", UnitType.Melee, Rarity.Epic, 50, 3, 1.8f, 2.0f,
                "Frenzied warrior with devastating attack speed.");

            // Legendary tier units (5% drop rate)
            created += CreateUnit("Dragon Knight", UnitType.Melee, Rarity.Legendary, 80, 15, 2.5f, 1.2f,
                "Legendary warrior bonded with a dragon.");
            created += CreateUnit("Arcane Archer", UnitType.Ranged, Rarity.Legendary, 70, 5, 5.0f, 1.5f,
                "Master archer wielding magical arrows.");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[UnitDataCreator] Created {created} unit assets in {UNIT_PATH}");
            EditorUtility.DisplayDialog("Unit Creation Complete",
                $"Created {created} unit assets in Resources/Units/", "OK");
        }

        private static int CreateUnit(string name, UnitType type, Rarity rarity,
            int attack, int defense, float range, float attackSpeed, string description)
        {
            string assetPath = UNIT_PATH + name + ".asset";

            // Skip if already exists
            if (AssetDatabase.LoadAssetAtPath<UnitData>(assetPath) != null)
            {
                Debug.Log($"[UnitDataCreator] Skipping existing asset: {name}");
                return 0;
            }

            UnitData unit = ScriptableObject.CreateInstance<UnitData>();
            unit.unitName = name;
            unit.type = type;
            unit.rarity = rarity;
            unit.attack = attack;
            unit.defense = defense;
            unit.attackRange = range;
            unit.attackSpeed = attackSpeed;
            unit.description = description;

            AssetDatabase.CreateAsset(unit, assetPath);
            Debug.Log($"[UnitDataCreator] Created: {name} ({rarity})");
            return 1;
        }

        [MenuItem("Tools/Lotto Defense/Validate Unit Pools")]
        public static void ValidateUnitPools()
        {
            UnitData[] allUnits = Resources.LoadAll<UnitData>("Units");

            int normal = 0, rare = 0, epic = 0, legendary = 0;

            foreach (var unit in allUnits)
            {
                switch (unit.rarity)
                {
                    case Rarity.Normal: normal++; break;
                    case Rarity.Rare: rare++; break;
                    case Rarity.Epic: epic++; break;
                    case Rarity.Legendary: legendary++; break;
                }
            }

            string message = $"Unit Pool Summary:\n" +
                $"Normal: {normal}\n" +
                $"Rare: {rare}\n" +
                $"Epic: {epic}\n" +
                $"Legendary: {legendary}\n" +
                $"Total: {allUnits.Length}";

            Debug.Log(message);
            EditorUtility.DisplayDialog("Unit Pool Validation", message, "OK");
        }
    }
}
