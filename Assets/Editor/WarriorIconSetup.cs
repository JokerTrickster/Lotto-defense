using UnityEngine;
using UnityEditor;
using LottoDefense.Units;

/// <summary>
/// Quick setup for Warrior unit icon.
/// </summary>
public class WarriorIconSetup
{
    [MenuItem("Lotto Defense/Quick Setup/Warrior Icon Only")]
    public static void SetupWarriorIcon()
    {
        // Load Warrior asset
        UnitData warrior = AssetDatabase.LoadAssetAtPath<UnitData>("Assets/Resources/Units/Warrior.asset");

        if (warrior == null)
        {
            Debug.LogError("Warrior.asset not found!");
            return;
        }

        // Generate icon
        int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);

        // Warrior = Common = Gray circle
        Color color = new Color(0.7f, 0.7f, 0.7f);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);

                if (distance <= radius)
                {
                    float alpha = 1f - (distance / radius) * 0.3f;
                    texture.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha));
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }

        texture.Apply();

        // Create sprite
        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            100f
        );
        sprite.name = "Warrior_Icon";

        // Assign to warrior
        warrior.icon = sprite;

        EditorUtility.SetDirty(warrior);
        AssetDatabase.SaveAssets();

        Debug.Log("[WarriorIconSetup] Warrior icon created successfully!");
        EditorUtility.DisplayDialog("Success",
            "Warrior icon created!\n\nGray circle icon has been assigned to Warrior.asset",
            "OK");
    }
}
