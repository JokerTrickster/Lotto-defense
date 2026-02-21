using UnityEngine;
using UnityEditor;
using LottoDefense.Gameplay;
using LottoDefense.Monsters;
using System.Collections.Generic;

namespace LottoDefense.Editor
{
    /// <summary>
    /// 라운드별로 다른 몬스터를 자동 배정하는 에디터 스크립트.
    /// </summary>
    public class SetupRoundConfig : EditorWindow
    {
        [MenuItem("Lotto Defense/Setup Round Config (Auto-Assign Monsters)")]
        static void AutoSetupRounds()
        {
            // RoundConfig 찾기
            RoundConfig config = AssetDatabase.LoadAssetAtPath<RoundConfig>("Assets/Resources/RoundConfig.asset");
            if (config == null)
            {
                Debug.LogError("[SetupRoundConfig] RoundConfig.asset not found at Assets/Resources/RoundConfig.asset");
                return;
            }

            // 몬스터 데이터 로드
            string[] monsterNames = new string[]
            {
                "Slime", "Goblin", "Wolf", "Bat", "Orc",
                "Skeleton", "Ghost", "Zombie", "Demon", "Troll",
                "Golem", "Dragon", "Lich", "Hydra", "Phoenix"
            };

            List<MonsterData> monsters = new List<MonsterData>();
            foreach (string name in monsterNames)
            {
                MonsterData monster = AssetDatabase.LoadAssetAtPath<MonsterData>($"Assets/Resources/Monsters/{name}.asset");
                if (monster != null)
                {
                    monsters.Add(monster);
                }
                else
                {
                    Debug.LogWarning($"[SetupRoundConfig] Monster not found: {name}");
                }
            }

            if (monsters.Count == 0)
            {
                Debug.LogError("[SetupRoundConfig] No monsters found! Run 'Create Monster Data' first.");
                return;
            }

            // SerializedObject 사용하여 RoundConfig 수정
            SerializedObject so = new SerializedObject(config);
            SerializedProperty roundConfigsProp = so.FindProperty("roundConfigs");
            roundConfigsProp.ClearArray();

            // 30라운드 설정
            for (int round = 1; round <= 30; round++)
            {
                MonsterData monster = GetMonsterForRound(round, monsters);
                
                roundConfigsProp.InsertArrayElementAtIndex(roundConfigsProp.arraySize);
                SerializedProperty element = roundConfigsProp.GetArrayElementAtIndex(roundConfigsProp.arraySize - 1);
                
                element.FindPropertyRelative("roundNumber").intValue = round;
                element.FindPropertyRelative("monsterData").objectReferenceValue = monster;
                element.FindPropertyRelative("totalMonsters").intValue = GetTotalMonstersForRound(round);
                element.FindPropertyRelative("spawnInterval").floatValue = GetSpawnIntervalForRound(round);
                element.FindPropertyRelative("spawnDuration").floatValue = 15f;

                Debug.Log($"[SetupRoundConfig] Round {round}: {monster.monsterName} (x{GetTotalMonstersForRound(round)})");
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();

            Debug.Log("[SetupRoundConfig] ✅ 30라운드 설정 완료!");
        }

        /// <summary>
        /// 라운드별로 적절한 몬스터 선택.
        /// </summary>
        static MonsterData GetMonsterForRound(int round, List<MonsterData> monsters)
        {
            // Round 1-2: Slime
            if (round <= 2) return monsters.Find(m => m.monsterName == "Slime");
            
            // Round 3-4: Goblin
            if (round <= 4) return monsters.Find(m => m.monsterName == "Goblin");
            
            // Round 5-6: Wolf (Fast)
            if (round <= 6) return monsters.Find(m => m.monsterName == "Wolf");
            
            // Round 7-8: Bat (Fast)
            if (round <= 8) return monsters.Find(m => m.monsterName == "Bat");
            
            // Round 9-10: Orc
            if (round <= 10) return monsters.Find(m => m.monsterName == "Orc");
            
            // Round 11-12: Skeleton
            if (round <= 12) return monsters.Find(m => m.monsterName == "Skeleton");
            
            // Round 13-14: Ghost (Fast)
            if (round <= 14) return monsters.Find(m => m.monsterName == "Ghost");
            
            // Round 15: Dragon (Boss)
            if (round == 15) return monsters.Find(m => m.monsterName == "Dragon");
            
            // Round 16-17: Zombie (Tank)
            if (round <= 17) return monsters.Find(m => m.monsterName == "Zombie");
            
            // Round 18-19: Demon
            if (round <= 19) return monsters.Find(m => m.monsterName == "Demon");
            
            // Round 20: Lich (Boss)
            if (round == 20) return monsters.Find(m => m.monsterName == "Lich");
            
            // Round 21-22: Troll (Tank)
            if (round <= 22) return monsters.Find(m => m.monsterName == "Troll");
            
            // Round 23-24: Golem (Tank)
            if (round <= 24) return monsters.Find(m => m.monsterName == "Golem");
            
            // Round 25: Hydra (Boss)
            if (round == 25) return monsters.Find(m => m.monsterName == "Hydra");
            
            // Round 26-29: Mix of hard monsters
            if (round <= 29)
            {
                int index = round % 4;
                string[] hardMonsters = { "Demon", "Troll", "Golem", "Lich" };
                return monsters.Find(m => m.monsterName == hardMonsters[index]);
            }
            
            // Round 30: Phoenix (Final Boss)
            return monsters.Find(m => m.monsterName == "Phoenix");
        }

        /// <summary>
        /// 라운드별 몬스터 수 (점점 증가).
        /// </summary>
        static int GetTotalMonstersForRound(int round)
        {
            // Boss rounds: 적은 수
            if (round == 15 || round == 20 || round == 25 || round == 30)
            {
                return 10 + (round / 5); // 보스: 13~16마리
            }

            // Normal rounds: 많은 수
            return 30 + (round * 2); // 32~90마리 (점진적 증가)
        }

        /// <summary>
        /// 라운드별 스폰 간격 (후반으로 갈수록 빠름).
        /// </summary>
        static float GetSpawnIntervalForRound(int round)
        {
            // Boss rounds: 느린 스폰
            if (round == 15 || round == 20 || round == 25 || round == 30)
            {
                return 1.0f;
            }

            // Early rounds: 0.5초
            if (round <= 10)
            {
                return 0.5f;
            }

            // Mid rounds: 0.4초
            if (round <= 20)
            {
                return 0.4f;
            }

            // Late rounds: 0.3초
            return 0.3f;
        }
    }
}
