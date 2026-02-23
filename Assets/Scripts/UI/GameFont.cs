using UnityEngine;

namespace LottoDefense.UI
{
    /// <summary>
    /// Central provider for the game font. Loads a custom font from Resources/Fonts/GameFont first
    /// (friendly, rounded fonts suit the cute/casual style), then falls back to Unity built-in.
    /// </summary>
    public static class GameFont
    {
        private static Font _cached;

        /// <summary>
        /// Returns the game font. Load order: Resources/Fonts/GameFont → LegacyRuntime.ttf → Arial.ttf.
        /// </summary>
        public static Font Get()
        {
            if (_cached != null) return _cached;

            _cached = Resources.Load<Font>("Fonts/GameFont");
            if (_cached != null) return _cached;

            _cached = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_cached != null) return _cached;

            _cached = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return _cached;
        }
    }
}
