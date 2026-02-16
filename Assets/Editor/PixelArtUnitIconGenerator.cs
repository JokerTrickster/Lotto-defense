using UnityEngine;
using UnityEditor;
using System.IO;

namespace LottoDefense.Editor
{
    /// <summary>
    /// Unity 에디터 도구: 픽셀 아트 스타일 유닛 아이콘 생성
    /// 메뉴: Tools > Generate Pixel Art Unit Icons
    /// </summary>
    public class PixelArtUnitIconGenerator : EditorWindow
    {
        private int iconSize = 64; // 픽셀 아트는 작은 사이즈가 적합
        private int pixelSize = 4; // 각 도트의 크기 (확대 배율)

        [MenuItem("Tools/Generate Pixel Art Unit Icons")]
        public static void ShowWindow()
        {
            GetWindow<PixelArtUnitIconGenerator>("Pixel Art Icon Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Pixel Art Unit Icon Generator", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            iconSize = EditorGUILayout.IntSlider("Base Size (pixels)", iconSize, 16, 128);
            pixelSize = EditorGUILayout.IntSlider("Pixel Scale", pixelSize, 1, 8);

            int finalSize = iconSize * pixelSize;
            EditorGUILayout.HelpBox($"최종 이미지 크기: {finalSize}x{finalSize}", MessageType.Info);

            EditorGUILayout.Space();

            if (GUILayout.Button("Generate Pixel Art Icons", GUILayout.Height(40)))
            {
                GenerateIcons();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "🎨 픽셀 아트 스타일 유닛 아이콘\n\n" +
                "생성될 유닛:\n" +
                "⚔️ Warrior (전사) - 빨간 갑옷 + 검\n" +
                "🏹 Archer (궁수) - 초록 망토 + 활\n" +
                "✨ Mage (마법사) - 파란 로브 + 모자\n" +
                "🔥 Phoenix (불사조) - 주황 불새\n" +
                "🐉 DragonKnight (용 기사) - 보라 용\n\n" +
                "저장: Assets/Resources/Sprites/Units/",
                MessageType.Info);
        }

        private void GenerateIcons()
        {
            string outputPath = "Assets/Resources/Sprites/Units";
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            GenerateWarriorPixelArt(outputPath);
            GenerateArcherPixelArt(outputPath);
            GenerateMagePixelArt(outputPath);
            GeneratePhoenixPixelArt(outputPath);
            GenerateDragonKnightPixelArt(outputPath);

            AssetDatabase.Refresh();
            Debug.Log("[PixelArt] ✅ All pixel art icons generated!");
            EditorUtility.DisplayDialog("Complete", $"픽셀 아트 아이콘 생성 완료!\n크기: {iconSize * pixelSize}x{iconSize * pixelSize}", "OK");
        }

        #region Pixel Art Generators

        private void GenerateWarriorPixelArt(string path)
        {
            // 16x16 픽셀 패턴 (전사: 빨간 갑옷 + 검)
            string[] pattern = new string[]
            {
                "                ",
                "      ****      ",
                "     **CC**     ",
                "    **CCCC**    ",
                "    *RRRRRR*    ",
                "   **RRRRRR**   ",
                "   **RRRRRR**   ",
                "    *RRRRRR*    ",
                "     *RRRR*     ",
                "    **RRRR**    ",
                "   ***RRRR***   ",
                "   **RRRRRRR**  ",
                "    **RRRRR**   ",
                "     **RRR**    ",
                "      *****     ",
                "                "
            };

            var colors = new System.Collections.Generic.Dictionary<char, Color>
            {
                { ' ', Color.clear },
                { '*', new Color(0.2f, 0.2f, 0.2f, 1f) }, // 검은색 외곽선
                { 'R', new Color(0.8f, 0.2f, 0.2f, 1f) }, // 빨간 갑옷
                { 'C', new Color(0.9f, 0.8f, 0.7f, 1f) }  // 살색 얼굴
            };

            CreatePixelArtIcon(pattern, colors, path, "Warrior.png");
        }

        private void GenerateArcherPixelArt(string path)
        {
            // 16x16 픽셀 패턴 (궁수: 초록 망토 + 활)
            string[] pattern = new string[]
            {
                "                ",
                "      ****      ",
                "     **CC**     ",
                "    **CCCC**    ",
                "    *GGGGGG*    ",
                "   **GGGGGG**   ",
                "  ***GGGGGG***  ",
                "  **GGGGGGGGG** ",
                "   **GGGGGGG**  ",
                "    **GGGGG**   ",
                "     **GGG**    ",
                "    ***GGG***   ",
                "    **GGGGG**   ",
                "     **GGG**    ",
                "      *****     ",
                "                "
            };

            var colors = new System.Collections.Generic.Dictionary<char, Color>
            {
                { ' ', Color.clear },
                { '*', new Color(0.2f, 0.2f, 0.2f, 1f) }, // 검은색 외곽선
                { 'G', new Color(0.3f, 0.7f, 0.3f, 1f) }, // 초록 망토
                { 'C', new Color(0.9f, 0.8f, 0.7f, 1f) }  // 살색 얼굴
            };

            CreatePixelArtIcon(pattern, colors, path, "Archer.png");
        }

        private void GenerateMagePixelArt(string path)
        {
            // 16x16 픽셀 패턴 (마법사: 파란 로브 + 모자)
            string[] pattern = new string[]
            {
                "                ",
                "     ******     ",
                "    **PPPP**    ",
                "   **PPPPPP**   ",
                "   ***CCCC***   ",
                "    **CCCC**    ",
                "    **BBBB**    ",
                "   **BBBBBB**   ",
                "   **BBBBBB**   ",
                "    *BBBBBB*    ",
                "    **BBBB**    ",
                "   ***BBBB***   ",
                "   **BBBBBB**   ",
                "    **BBBB**    ",
                "     ******     ",
                "                "
            };

            var colors = new System.Collections.Generic.Dictionary<char, Color>
            {
                { ' ', Color.clear },
                { '*', new Color(0.2f, 0.2f, 0.2f, 1f) }, // 검은색 외곽선
                { 'B', new Color(0.2f, 0.4f, 0.9f, 1f) }, // 파란 로브
                { 'P', new Color(0.4f, 0.2f, 0.7f, 1f) }, // 보라 모자
                { 'C', new Color(0.9f, 0.8f, 0.7f, 1f) }  // 살색 얼굴
            };

            CreatePixelArtIcon(pattern, colors, path, "Mage.png");
        }

        private void GeneratePhoenixPixelArt(string path)
        {
            // 16x16 픽셀 패턴 (불사조: 주황 불새)
            string[] pattern = new string[]
            {
                "                ",
                "    **    **    ",
                "   **Y*  *Y**   ",
                "   *YYY**YYY*   ",
                "   **YYYYYY**   ",
                "    **OOOO**    ",
                "   ***OOOO***   ",
                "   **OOOOOO**   ",
                "  **OOOOOOOO**  ",
                "  *OOOOOOOOOO*  ",
                " **OOOOOOOOOO** ",
                "  **YYYYYYYY**  ",
                "   ***YYYY***   ",
                "    ***RR***    ",
                "     ******     ",
                "                "
            };

            var colors = new System.Collections.Generic.Dictionary<char, Color>
            {
                { ' ', Color.clear },
                { '*', new Color(0.2f, 0.2f, 0.2f, 1f) }, // 검은색 외곽선
                { 'Y', new Color(1f, 0.9f, 0.2f, 1f) },   // 노란 불꽃
                { 'O', new Color(1f, 0.5f, 0.1f, 1f) },   // 주황 몸체
                { 'R', new Color(0.9f, 0.2f, 0.1f, 1f) }  // 빨간 불꽃
            };

            CreatePixelArtIcon(pattern, colors, path, "Phoenix.png");
        }

        private void GenerateDragonKnightPixelArt(string path)
        {
            // 16x16 픽셀 패턴 (용 기사: 보라 용)
            string[] pattern = new string[]
            {
                "                ",
                "    **          ",
                "   *PP*   **    ",
                "   *PPP***DD*   ",
                "  **PPPPPPDDD*  ",
                "  *PPPPPPDDDDD* ",
                "  *PPPPPPPDDD** ",
                "  **PPPPPPDD**  ",
                "   **PPPPPPD*   ",
                "    **PPPPP**   ",
                "   ***PPPPP**   ",
                "   **PPPPPPP**  ",
                "    **PPPPP**   ",
                "     **PPP**    ",
                "      *****     ",
                "                "
            };

            var colors = new System.Collections.Generic.Dictionary<char, Color>
            {
                { ' ', Color.clear },
                { '*', new Color(0.2f, 0.2f, 0.2f, 1f) }, // 검은색 외곽선
                { 'P', new Color(0.5f, 0.2f, 0.7f, 1f) }, // 보라 용 몸체
                { 'D', new Color(0.7f, 0.5f, 0.9f, 1f) }  // 연보라 용 날개
            };

            CreatePixelArtIcon(pattern, colors, path, "DragonKnight.png");
        }

        #endregion

        #region Pixel Art Utilities

        private void CreatePixelArtIcon(string[] pattern, System.Collections.Generic.Dictionary<char, Color> colorMap, string path, string filename)
        {
            int baseSize = pattern.Length;
            int finalSize = baseSize * pixelSize;
            Texture2D icon = new Texture2D(finalSize, finalSize, TextureFormat.RGBA32, false);

            // 투명 배경으로 초기화
            for (int y = 0; y < finalSize; y++)
            {
                for (int x = 0; x < finalSize; x++)
                {
                    icon.SetPixel(x, y, Color.clear);
                }
            }

            // 패턴을 픽셀로 변환 (상하 반전 - Unity 좌표계)
            for (int y = 0; y < baseSize; y++)
            {
                for (int x = 0; x < baseSize; x++)
                {
                    if (x < pattern[y].Length)
                    {
                        char pixel = pattern[y][x];
                        if (colorMap.ContainsKey(pixel))
                        {
                            Color color = colorMap[pixel];
                            
                            // pixelSize만큼 확대하여 그리기
                            for (int py = 0; py < pixelSize; py++)
                            {
                                for (int px = 0; px < pixelSize; px++)
                                {
                                    int finalX = x * pixelSize + px;
                                    int finalY = (baseSize - 1 - y) * pixelSize + py; // 상하 반전
                                    icon.SetPixel(finalX, finalY, color);
                                }
                            }
                        }
                    }
                }
            }

            icon.filterMode = FilterMode.Point; // 픽셀 아트는 Point 필터
            icon.Apply();

            byte[] bytes = icon.EncodeToPNG();
            string fullPath = Path.Combine(path, filename);
            File.WriteAllBytes(fullPath, bytes);
            
            Debug.Log($"[PixelArt] 💾 Saved: {fullPath} ({finalSize}x{finalSize})");
        }

        #endregion
    }
}
