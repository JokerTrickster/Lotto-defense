using UnityEngine;
using UnityEditor;
using LottoDefense.Monsters;

namespace LottoDefense.Editor
{
    /// <summary>
    /// Editor script to quickly create multiple monster data assets.
    /// </summary>
    public class CreateMonsters : EditorWindow
    {
        [MenuItem("Lotto Defense/Create Monster Data")]
        static void CreateMonsterData()
        {
            CreateMonster("Slime", MonsterType.Normal, 50, 5, 1.5f, 5, 1.08f, 1.03f);
            CreateMonster("Goblin", MonsterType.Normal, 80, 8, 2.0f, 8, 1.10f, 1.04f);
            CreateMonster("Orc", MonsterType.Normal, 120, 12, 1.8f, 12, 1.12f, 1.05f);
            CreateMonster("Wolf", MonsterType.Fast, 60, 6, 3.0f, 10, 1.09f, 1.03f);
            CreateMonster("Bat", MonsterType.Fast, 40, 4, 3.5f, 8, 1.08f, 1.02f);
            CreateMonster("Skeleton", MonsterType.Normal, 100, 10, 2.2f, 15, 1.11f, 1.05f);
            CreateMonster("Zombie", MonsterType.Tank, 200, 15, 1.2f, 18, 1.15f, 1.08f);
            CreateMonster("Troll", MonsterType.Tank, 300, 20, 1.0f, 25, 1.18f, 1.10f);
            CreateMonster("Ghost", MonsterType.Fast, 70, 7, 2.8f, 12, 1.10f, 1.04f);
            CreateMonster("Demon", MonsterType.Normal, 150, 15, 2.5f, 20, 1.14f, 1.07f);
            CreateMonster("Golem", MonsterType.Tank, 400, 25, 0.8f, 30, 1.20f, 1.12f);
            CreateMonster("Dragon", MonsterType.Boss, 1000, 50, 1.5f, 100, 1.25f, 1.15f);
            CreateMonster("Lich", MonsterType.Boss, 800, 40, 1.8f, 80, 1.22f, 1.12f);
            CreateMonster("Hydra", MonsterType.Boss, 1200, 60, 1.3f, 120, 1.28f, 1.18f);
            CreateMonster("Phoenix", MonsterType.Boss, 900, 45, 2.0f, 90, 1.24f, 1.14f);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CreateMonsters] 15개 몬스터 생성 완료!");
        }

        static void CreateMonster(string name, MonsterType type, int hp, int def, float speed, int gold, float hpScale, float defScale)
        {
            MonsterData monster = ScriptableObject.CreateInstance<MonsterData>();
            monster.monsterName = name;
            monster.type = type;
            monster.maxHealth = hp;
            monster.defense = def;
            monster.moveSpeed = speed;
            monster.goldReward = gold;
            monster.healthScaling = hpScale;
            monster.defenseScaling = defScale;
            monster.attack = def; // attack = defense for simplicity

            string path = $"Assets/Resources/Monsters/{name}.asset";
            AssetDatabase.CreateAsset(monster, path);
            Debug.Log($"[CreateMonsters] {name} 생성: HP={hp}, Speed={speed}, Gold={gold}");
        }
    }
}
