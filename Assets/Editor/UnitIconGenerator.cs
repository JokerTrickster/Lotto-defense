using UnityEngine;
using UnityEditor;
using System.IO;

namespace LottoDefense.Editor
{
    /// <summary>
    /// Unity 에디터 도구: 프로시저럴 유닛 아이콘 생성
    /// 메뉴: Tools > Generate Unit Icons
    /// </summary>
    public class UnitIconGenerator : EditorWindow
    {
        private int iconSize = 256;
        private bool generateAll = true;

        [MenuItem("Tools/Generate Unit Icons")]
        public static void ShowWindow()
        {
            GetWindow<UnitIconGenerator>("Unit Icon Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Unit Icon Generator", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            iconSize = EditorGUILayout.IntSlider("Icon Size", iconSize, 64, 512);
            generateAll = EditorGUILayout.Toggle("Generate All Units", generateAll);

            EditorGUILayout.Space();

            if (GUILayout.Button("Generate Icons", GUILayout.Height(40)))
            {
                GenerateIcons();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "생성될 유닛:\n" +
                "• Warrior (전사) - 빨간색 검\n" +
                "• Archer (궁수) - 초록색 활\n" +
                "• Mage (마법사) - 파란색 지팡이\n" +
                "• Phoenix (불사조) - 주황색 불꽃\n" +
                "• DragonKnight (용 기사) - 보라색 용\n\n" +
                "저장 위치: Assets/Resources/Sprites/Units/",
                MessageType.Info);
        }

        private void GenerateIcons()
        {
            string outputPath = "Assets/Resources/Sprites/Units";
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            // 각 유닛별 아이콘 생성
            GenerateWarriorIcon(outputPath);
            GenerateArcherIcon(outputPath);
            GenerateMageIcon(outputPath);
            GeneratePhoenixIcon(outputPath);
            GenerateDragonKnightIcon(outputPath);

            AssetDatabase.Refresh();
            Debug.Log("[UnitIconGenerator] ✅ All unit icons generated!");
            EditorUtility.DisplayDialog("Complete", "유닛 아이콘 생성 완료!", "OK");
        }

        #region Icon Generators

        private void GenerateWarriorIcon(string path)
        {
            Texture2D icon = new Texture2D(iconSize, iconSize, TextureFormat.RGBA32, false);
            Color bgColor = new Color(0.8f, 0.2f, 0.2f, 1f); // 빨간색 배경
            Color swordColor = new Color(0.9f, 0.9f, 0.9f, 1f); // 은색 검
            Color handleColor = new Color(0.6f, 0.4f, 0.2f, 1f); // 갈색 손잡이

            // 배경 원
            DrawCircle(icon, iconSize / 2, iconSize / 2, iconSize / 2 - 10, bgColor);

            // 검 (중앙에 세로로)
            int swordWidth = iconSize / 12;
            int swordHeight = (int)(iconSize * 0.6f);
            int swordX = iconSize / 2 - swordWidth / 2;
            int swordY = iconSize / 2 - swordHeight / 2;

            // 검날
            DrawRect(icon, swordX, swordY, swordWidth, swordHeight - iconSize / 8, swordColor);

            // 검 손잡이
            DrawRect(icon, swordX, swordY + swordHeight - iconSize / 8, swordWidth, iconSize / 8, handleColor);

            // 십자 가드
            DrawRect(icon, swordX - swordWidth, swordY + swordHeight - iconSize / 6, swordWidth * 3, swordWidth / 2, swordColor);

            SaveIcon(icon, path, "Warrior.png");
        }

        private void GenerateArcherIcon(string path)
        {
            Texture2D icon = new Texture2D(iconSize, iconSize, TextureFormat.RGBA32, false);
            Color bgColor = new Color(0.2f, 0.7f, 0.3f, 1f); // 초록색 배경
            Color bowColor = new Color(0.6f, 0.4f, 0.2f, 1f); // 갈색 활
            Color stringColor = new Color(0.9f, 0.9f, 0.9f, 1f); // 흰색 현

            // 배경 원
            DrawCircle(icon, iconSize / 2, iconSize / 2, iconSize / 2 - 10, bgColor);

            // 활 (곡선)
            int centerX = iconSize / 2;
            int centerY = iconSize / 2;
            int bowHeight = (int)(iconSize * 0.6f);

            // 활 상단
            DrawArc(icon, centerX - iconSize / 12, centerY - bowHeight / 2, iconSize / 12, bowHeight / 2, bowColor, 8);
            // 활 하단
            DrawArc(icon, centerX - iconSize / 12, centerY, iconSize / 12, bowHeight / 2, bowColor, 8);

            // 활시위
            DrawLine(icon, centerX - iconSize / 12, centerY - bowHeight / 2 + 10, centerX - iconSize / 12, centerY + bowHeight / 2 - 10, stringColor, 3);

            // 화살
            DrawLine(icon, centerX - iconSize / 4, centerY, centerX + iconSize / 8, centerY, new Color(0.8f, 0.6f, 0.4f, 1f), 4);

            SaveIcon(icon, path, "Archer.png");
        }

        private void GenerateMageIcon(string path)
        {
            Texture2D icon = new Texture2D(iconSize, iconSize, TextureFormat.RGBA32, false);
            Color bgColor = new Color(0.3f, 0.3f, 0.8f, 1f); // 파란색 배경
            Color staffColor = new Color(0.6f, 0.4f, 0.2f, 1f); // 갈색 지팡이
            Color orbColor = new Color(0.4f, 0.7f, 1f, 1f); // 하늘색 구슬

            // 배경 원
            DrawCircle(icon, iconSize / 2, iconSize / 2, iconSize / 2 - 10, bgColor);

            // 지팡이
            int staffWidth = iconSize / 16;
            int staffHeight = (int)(iconSize * 0.7f);
            DrawRect(icon, iconSize / 2 - staffWidth / 2, iconSize / 2 - staffHeight / 2, staffWidth, staffHeight, staffColor);

            // 마법 구슬 (상단)
            DrawCircle(icon, iconSize / 2, iconSize / 2 - staffHeight / 2 + iconSize / 12, iconSize / 8, orbColor);

            // 마법 이펙트 (작은 별들)
            Color starColor = new Color(1f, 1f, 0.4f, 1f);
            DrawCircle(icon, iconSize / 2 - iconSize / 6, iconSize / 2 - iconSize / 8, iconSize / 20, starColor);
            DrawCircle(icon, iconSize / 2 + iconSize / 6, iconSize / 2 + iconSize / 12, iconSize / 20, starColor);
            DrawCircle(icon, iconSize / 2 - iconSize / 10, iconSize / 2 + iconSize / 6, iconSize / 20, starColor);

            SaveIcon(icon, path, "Mage.png");
        }

        private void GeneratePhoenixIcon(string path)
        {
            Texture2D icon = new Texture2D(iconSize, iconSize, TextureFormat.RGBA32, false);
            Color bgColor = new Color(0.9f, 0.5f, 0.2f, 1f); // 주황색 배경
            Color fireColor1 = new Color(1f, 0.8f, 0.2f, 1f); // 노란 불꽃
            Color fireColor2 = new Color(1f, 0.4f, 0.1f, 1f); // 주황 불꽃
            Color fireColor3 = new Color(1f, 0.2f, 0.1f, 1f); // 빨간 불꽃

            // 배경 원
            DrawCircle(icon, iconSize / 2, iconSize / 2, iconSize / 2 - 10, bgColor);

            // 불사조 몸체 (원)
            DrawCircle(icon, iconSize / 2, iconSize / 2, iconSize / 6, fireColor2);

            // 불꽃 날개 (좌측)
            DrawCircle(icon, iconSize / 2 - iconSize / 5, iconSize / 2 - iconSize / 10, iconSize / 8, fireColor1);
            DrawCircle(icon, iconSize / 2 - iconSize / 4, iconSize / 2 - iconSize / 6, iconSize / 10, fireColor2);
            DrawCircle(icon, iconSize / 2 - iconSize / 3, iconSize / 2 - iconSize / 8, iconSize / 12, fireColor3);

            // 불꽃 날개 (우측)
            DrawCircle(icon, iconSize / 2 + iconSize / 5, iconSize / 2 - iconSize / 10, iconSize / 8, fireColor1);
            DrawCircle(icon, iconSize / 2 + iconSize / 4, iconSize / 2 - iconSize / 6, iconSize / 10, fireColor2);
            DrawCircle(icon, iconSize / 2 + iconSize / 3, iconSize / 2 - iconSize / 8, iconSize / 12, fireColor3);

            // 꼬리 불꽃
            DrawCircle(icon, iconSize / 2, iconSize / 2 + iconSize / 5, iconSize / 10, fireColor1);
            DrawCircle(icon, iconSize / 2, iconSize / 2 + iconSize / 4, iconSize / 12, fireColor2);

            SaveIcon(icon, path, "Phoenix.png");
        }

        private void GenerateDragonKnightIcon(string path)
        {
            Texture2D icon = new Texture2D(iconSize, iconSize, TextureFormat.RGBA32, false);
            Color bgColor = new Color(0.5f, 0.2f, 0.7f, 1f); // 보라색 배경
            Color dragonColor = new Color(0.3f, 0.1f, 0.5f, 1f); // 진한 보라색
            Color scaleColor = new Color(0.7f, 0.5f, 0.9f, 1f); // 연한 보라색

            // 배경 원
            DrawCircle(icon, iconSize / 2, iconSize / 2, iconSize / 2 - 10, bgColor);

            // 용 머리 (큰 원)
            DrawCircle(icon, iconSize / 2, iconSize / 2, iconSize / 5, dragonColor);

            // 용 뿔 (좌우)
            DrawCircle(icon, iconSize / 2 - iconSize / 8, iconSize / 2 - iconSize / 6, iconSize / 12, scaleColor);
            DrawCircle(icon, iconSize / 2 + iconSize / 8, iconSize / 2 - iconSize / 6, iconSize / 12, scaleColor);

            // 용 날개 (좌측)
            DrawCircle(icon, iconSize / 2 - iconSize / 4, iconSize / 2 + iconSize / 12, iconSize / 8, scaleColor);
            DrawCircle(icon, iconSize / 2 - iconSize / 3, iconSize / 2 + iconSize / 10, iconSize / 10, dragonColor);

            // 용 날개 (우측)
            DrawCircle(icon, iconSize / 2 + iconSize / 4, iconSize / 2 + iconSize / 12, iconSize / 8, scaleColor);
            DrawCircle(icon, iconSize / 2 + iconSize / 3, iconSize / 2 + iconSize / 10, iconSize / 10, dragonColor);

            // 눈 (흰색)
            Color eyeColor = new Color(1f, 1f, 1f, 1f);
            DrawCircle(icon, iconSize / 2 - iconSize / 12, iconSize / 2 - iconSize / 20, iconSize / 24, eyeColor);
            DrawCircle(icon, iconSize / 2 + iconSize / 12, iconSize / 2 - iconSize / 20, iconSize / 24, eyeColor);

            SaveIcon(icon, path, "DragonKnight.png");
        }

        #endregion

        #region Drawing Utilities

        private void DrawCircle(Texture2D tex, int centerX, int centerY, int radius, Color color)
        {
            for (int y = 0; y < tex.height; y++)
            {
                for (int x = 0; x < tex.width; x++)
                {
                    int dx = x - centerX;
                    int dy = y - centerY;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist <= radius)
                    {
                        // 안티앨리어싱
                        float alpha = 1f - Mathf.Clamp01(dist - radius + 1);
                        Color existingColor = tex.GetPixel(x, y);
                        Color blended = Color.Lerp(existingColor, color, color.a * alpha);
                        tex.SetPixel(x, y, blended);
                    }
                }
            }
        }

        private void DrawRect(Texture2D tex, int x, int y, int width, int height, Color color)
        {
            for (int py = y; py < y + height && py < tex.height; py++)
            {
                for (int px = x; px < x + width && px < tex.width; px++)
                {
                    if (px >= 0 && py >= 0)
                    {
                        tex.SetPixel(px, py, color);
                    }
                }
            }
        }

        private void DrawLine(Texture2D tex, int x1, int y1, int x2, int y2, Color color, int thickness)
        {
            int dx = Mathf.Abs(x2 - x1);
            int dy = Mathf.Abs(y2 - y1);
            int sx = x1 < x2 ? 1 : -1;
            int sy = y1 < y2 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                // 두께를 위해 주변 픽셀도 칠함
                for (int ty = -thickness / 2; ty <= thickness / 2; ty++)
                {
                    for (int tx = -thickness / 2; tx <= thickness / 2; tx++)
                    {
                        int px = x1 + tx;
                        int py = y1 + ty;
                        if (px >= 0 && px < tex.width && py >= 0 && py < tex.height)
                        {
                            tex.SetPixel(px, py, color);
                        }
                    }
                }

                if (x1 == x2 && y1 == y2) break;

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x1 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y1 += sy;
                }
            }
        }

        private void DrawArc(Texture2D tex, int centerX, int centerY, int radiusX, int radiusY, Color color, int thickness)
        {
            // 간단한 곡선 (타원 일부)
            for (int angle = 0; angle < 180; angle += 2)
            {
                float rad = angle * Mathf.Deg2Rad;
                int x = centerX + (int)(radiusX * Mathf.Cos(rad));
                int y = centerY + (int)(radiusY * Mathf.Sin(rad));

                for (int ty = -thickness / 2; ty <= thickness / 2; ty++)
                {
                    for (int tx = -thickness / 2; tx <= thickness / 2; tx++)
                    {
                        int px = x + tx;
                        int py = y + ty;
                        if (px >= 0 && px < tex.width && py >= 0 && py < tex.height)
                        {
                            tex.SetPixel(px, py, color);
                        }
                    }
                }
            }
        }

        private void SaveIcon(Texture2D tex, string path, string filename)
        {
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            string fullPath = Path.Combine(path, filename);
            File.WriteAllBytes(fullPath, bytes);
            Debug.Log($"[UnitIconGenerator] 💾 Saved: {fullPath}");
        }

        #endregion
    }
}
