using UnityEngine;
using UnityEditor;
using LottoDefense.Units;

/// <summary>
/// Editor utility to generate simple colored circle icons for units.
/// </summary>
public class UnitIconGenerator : EditorWindow
{
    [MenuItem("Lotto Defense/Generate Unit Icons")]
    public static void GenerateUnitIcons()
    {
        // Load all UnitData assets
        string[] guids = AssetDatabase.FindAssets("t:UnitData", new[] { "Assets/Resources/Units" });

        int updated = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            UnitData unitData = AssetDatabase.LoadAssetAtPath<UnitData>(path);

            if (unitData != null && unitData.icon == null)
            {
                // Generate icon sprite
                Sprite icon = CreateCircleSprite(unitData);
                unitData.icon = icon;

                EditorUtility.SetDirty(unitData);
                updated++;

                Debug.Log($"[UnitIconGenerator] Generated icon for {unitData.unitName}");
            }
        }

        AssetDatabase.SaveAssets();

        if (updated > 0)
        {
            EditorUtility.DisplayDialog("Success",
                $"Generated icons for {updated} unit(s)!\n\nIcons are colored circles based on rarity.",
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Info",
                "All units already have icons assigned.",
                "OK");
        }
    }

    /// <summary>
    /// Create a simple colored circle sprite for a unit.
    /// </summary>
    private static Sprite CreateCircleSprite(UnitData unitData)
    {
        int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);

        // Get color based on rarity
        Color color = GetRarityColor(unitData.rarity);

        // Draw circle
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 2f; // Leave 2px border

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);

                if (distance <= radius)
                {
                    // Inside circle - use rarity color
                    float alpha = 1f - (distance / radius) * 0.3f; // Slight gradient
                    texture.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha));
                }
                else
                {
                    // Outside circle - transparent
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }

        texture.Apply();

        // Convert to sprite
        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            100f
        );
        sprite.name = $"{unitData.unitName}_Icon";

        return sprite;
    }

    /// <summary>
    /// Get color based on rarity.
    /// </summary>
    private static Color GetRarityColor(UnitRarity rarity)
    {
        switch (rarity)
        {
            case UnitRarity.Common:
                return new Color(0.7f, 0.7f, 0.7f); // Gray
            case UnitRarity.Rare:
                return new Color(0.3f, 0.6f, 1f); // Blue
            case UnitRarity.Epic:
                return new Color(0.7f, 0.3f, 1f); // Purple
            case UnitRarity.Legendary:
                return new Color(1f, 0.8f, 0.2f); // Gold
            default:
                return Color.white;
        }
    }
}
