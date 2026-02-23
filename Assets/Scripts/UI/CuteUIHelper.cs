using UnityEngine;
using System.Collections.Generic;

namespace LottoDefense.UI
{
    public static class CuteUIHelper
    {
        private static readonly Dictionary<int, Sprite> s_roundedRectCache = new Dictionary<int, Sprite>();

        /// <summary>
        /// Creates or retrieves a cached white rounded rectangle sprite suitable for 9-slice scaling.
        /// The sprite uses Image.Type.Sliced so it can stretch to any size while keeping corners round.
        /// </summary>
        public static Sprite GetRoundedRectSprite(int radius = 16)
        {
            if (s_roundedRectCache.TryGetValue(radius, out Sprite cached) && cached != null)
                return cached;

            int size = radius * 2 + 2;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float alpha = GetRoundedRectAlpha(x, y, size, size, radius);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();

            Vector4 border = new Vector4(radius, radius, radius, radius);
            Sprite sprite = Sprite.Create(
                tex,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                border
            );

            s_roundedRectCache[radius] = sprite;
            return sprite;
        }

        private static float GetRoundedRectAlpha(int px, int py, int w, int h, int r)
        {
            float x = px;
            float y = py;

            float cornerX = -1f;
            float cornerY = -1f;

            if (x < r) cornerX = r;
            else if (x >= w - r) cornerX = w - r - 1;

            if (y < r) cornerY = r;
            else if (y >= h - r) cornerY = h - r - 1;

            if (cornerX >= 0 && cornerY >= 0)
            {
                float dx = x - cornerX;
                float dy = y - cornerY;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist > r) return 0f;
                if (dist > r - 1f) return r - dist;
                return 1f;
            }

            return 1f;
        }

        /// <summary>
        /// Soft pastel shadow color for cute UI elements.
        /// </summary>
        public static readonly Color SoftShadow = new Color(0.4f, 0.3f, 0.25f, 0.25f);

        /// <summary>
        /// Warm brown border color for cute UI elements.
        /// </summary>
        public static readonly Color WarmBorder = new Color(0.75f, 0.65f, 0.55f, 0.6f);

        /// <summary>
        /// Dark brown text color for use on light backgrounds.
        /// </summary>
        public static readonly Color DarkText = new Color(0.25f, 0.2f, 0.15f, 1f);

        /// <summary>
        /// Warm cream background color.
        /// </summary>
        public static readonly Color CreamBg = new Color(0.98f, 0.95f, 0.90f, 0.96f);

        /// <summary>
        /// Soft peach background for panels.
        /// </summary>
        public static readonly Color PeachBg = new Color(1f, 0.96f, 0.92f, 0.96f);

        /// <summary>
        /// Warm dark overlay for popups.
        /// </summary>
        public static readonly Color WarmOverlay = new Color(0.3f, 0.2f, 0.15f, 0.6f);
    }
}
