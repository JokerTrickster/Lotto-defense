using UnityEngine;

namespace LottoDefense.UI
{
    /// <summary>
    /// Design tokens for the main menu (협동타워디팬스-style).
    /// Use for consistent colors, spacing, and typography.
    /// </summary>
    public static class MainMenuDesignTokens
    {
        #region Colors (협동타워디팬스 inspired: dark base + accent)
        /// <summary>Background tint for full screen (dark).</summary>
        public static readonly Color BackgroundDark = new Color(0.08f, 0.10f, 0.14f, 1f);

        /// <summary>Primary CTA: Solo / Play (green accent).</summary>
        public static readonly Color ButtonPrimary = new Color(0.18f, 0.65f, 0.35f, 1f);

        /// <summary>Secondary: Co-op (blue accent).</summary>
        public static readonly Color ButtonCoop = new Color(0.25f, 0.45f, 0.75f, 1f);

        /// <summary>Tertiary: Boss Rush (orange/red accent).</summary>
        public static readonly Color ButtonBossRush = new Color(0.75f, 0.35f, 0.2f, 1f);

        /// <summary>Logout / secondary action (muted).</summary>
        public static readonly Color ButtonLogout = new Color(0.5f, 0.28f, 0.28f, 1f);

        /// <summary>Disabled / coming soon button.</summary>
        public static readonly Color ButtonDisabled = new Color(0.35f, 0.35f, 0.38f, 1f);

        /// <summary>Title and primary text.</summary>
        public static readonly Color TextPrimary = Color.white;

        /// <summary>Subtitle or hint text.</summary>
        public static readonly Color TextSecondary = new Color(0.85f, 0.85f, 0.88f, 1f);
        #endregion

        #region Layout (reference 1080x1920 portrait)
        /// <summary>Reference resolution width (portrait).</summary>
        public const float RefWidth = 1080f;

        /// <summary>Reference resolution height (portrait).</summary>
        public const float RefHeight = 1920f;

        /// <summary>Spacing between mode buttons (pixels).</summary>
        public const float ButtonSpacing = 32f;

        /// <summary>Main mode button width.</summary>
        public const float ButtonWidth = 520f;

        /// <summary>Main mode button height.</summary>
        public const float ButtonHeight = 128f;

        /// <summary>Title font size.</summary>
        public const int TitleFontSize = 72;

        /// <summary>Button label font size.</summary>
        public const int ButtonFontSize = 44;
        #endregion
    }
}
