using System.Collections.Generic;
using UnityEngine;

namespace LottoDefense.Utils
{
    /// <summary>
    /// Loads sprite PNGs from Resources/Sprites/ and converts them to Sprite objects at runtime.
    /// Caches loaded sprites to avoid redundant texture loading.
    /// </summary>
    public static class GameSpriteLoader
    {
        private static readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

        /// <summary>
        /// Load a monster sprite by monster name (e.g. "Goblin", "Dragon Boss").
        /// Strips spaces from name to match filename (e.g. "Dragon Boss" → "DragonBoss.png").
        /// Returns null if not found (caller should fall back to procedural sprite).
        /// </summary>
        public static Sprite LoadMonsterSprite(string monsterName)
        {
            return LoadSprite("Sprites/Monsters", monsterName);
        }

        /// <summary>
        /// Load a unit sprite by unit name (e.g. "Warrior", "Dragon Knight").
        /// Strips spaces from name to match filename.
        /// Returns null if not found.
        /// </summary>
        public static Sprite LoadUnitSprite(string unitName)
        {
            return LoadSprite("Sprites/Units", unitName);
        }

        private static Sprite LoadSprite(string folder, string name)
        {
            string sanitized = name.Replace(" ", "");
            string key = $"{folder}/{sanitized}";

            if (spriteCache.TryGetValue(key, out Sprite cached))
            {
                return cached;
            }

            Texture2D tex = Resources.Load<Texture2D>(key);
            if (tex == null)
            {
                return null;
            }

            tex.filterMode = FilterMode.Point;
            // pixelsPerUnit = tex.width so the sprite is 1 world unit wide
            Sprite sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                tex.width
            );
            spriteCache[key] = sprite;
            return sprite;
        }
    }
}
