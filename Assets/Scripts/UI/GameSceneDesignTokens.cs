using UnityEngine;

namespace LottoDefense.UI
{
    /// <summary>
    /// Design tokens for the in-game UI (GameScene).
    /// Mobile tower defense style: dark translucent panels, vivid accent colors, large touch targets.
    /// </summary>
    public static class GameSceneDesignTokens
    {
        #region HUD Colors
        /// <summary>HUD panel background (dark, high opacity for readability).</summary>
        public static readonly Color HudBackground = new Color(0.06f, 0.08f, 0.12f, 0.92f);

        /// <summary>HUD stat card background (slightly lighter than panel).</summary>
        public static readonly Color HudStatCardBg = new Color(0.12f, 0.14f, 0.20f, 0.85f);

        /// <summary>HUD divider / border line color.</summary>
        public static readonly Color HudBorder = new Color(0.25f, 0.28f, 0.35f, 0.6f);
        #endregion

        #region Stat Colors (vivid, high contrast on dark background)
        /// <summary>Round number accent (cyan/teal).</summary>
        public static readonly Color RoundColor = new Color(0.2f, 0.85f, 0.9f, 1f);

        /// <summary>Phase label accent (white with slight warmth).</summary>
        public static readonly Color PhaseColor = new Color(0.95f, 0.95f, 0.98f, 1f);

        /// <summary>Timer accent (soft yellow).</summary>
        public static readonly Color TimeColor = new Color(1f, 0.92f, 0.5f, 1f);

        /// <summary>Life/HP (vivid red-pink).</summary>
        public static readonly Color LifeColor = new Color(1f, 0.35f, 0.4f, 1f);

        /// <summary>Gold (rich gold).</summary>
        public static readonly Color GoldColor = new Color(1f, 0.82f, 0.15f, 1f);

        /// <summary>Monster count (orange-red).</summary>
        public static readonly Color MonsterColor = new Color(1f, 0.55f, 0.25f, 1f);

        /// <summary>Unit count (sky blue).</summary>
        public static readonly Color UnitColor = new Color(0.4f, 0.75f, 1f, 1f);

        /// <summary>Stat label (dimmed white for "ROUND", "GOLD" etc).</summary>
        public static readonly Color StatLabel = new Color(0.6f, 0.62f, 0.68f, 1f);
        #endregion

        #region Button Colors
        /// <summary>Summon button gradient main (vibrant green).</summary>
        public static readonly Color SummonButtonBg = new Color(0.1f, 0.7f, 0.3f, 1f);

        /// <summary>Summon button pressed state.</summary>
        public static readonly Color SummonButtonPressed = new Color(0.08f, 0.5f, 0.2f, 1f);

        /// <summary>Summon button highlighted state.</summary>
        public static readonly Color SummonButtonHighlight = new Color(0.15f, 0.8f, 0.35f, 1f);

        /// <summary>Summon button disabled state.</summary>
        public static readonly Color SummonButtonDisabled = new Color(0.25f, 0.35f, 0.28f, 0.7f);

        /// <summary>Back to menu button (muted blue-gray).</summary>
        public static readonly Color MenuButtonBg = new Color(0.22f, 0.24f, 0.35f, 0.9f);

        /// <summary>Menu button pressed.</summary>
        public static readonly Color MenuButtonPressed = new Color(0.15f, 0.16f, 0.25f, 0.95f);

        /// <summary>Button text color (bright white).</summary>
        public static readonly Color ButtonText = Color.white;

        /// <summary>Button text shadow (dark for contrast).</summary>
        public static readonly Color ButtonTextShadow = new Color(0f, 0f, 0f, 0.5f);

        /// <summary>Gold cost text on summon button (gold accent).</summary>
        public static readonly Color ButtonCostText = new Color(1f, 0.88f, 0.3f, 1f);
        #endregion

        #region Countdown Colors
        /// <summary>Countdown number color (bright white with glow feel).</summary>
        public static readonly Color CountdownText = Color.white;

        /// <summary>Countdown overlay background (dark vignette).</summary>
        public static readonly Color CountdownOverlay = new Color(0f, 0f, 0f, 0.5f);
        #endregion

        #region Typography (font sizes for 1080x1920 reference)
        /// <summary>HUD stat value (large, prominent number).</summary>
        public const int StatValueSize = 34;

        /// <summary>HUD stat label ("ROUND", "GOLD" etc).</summary>
        public const int StatLabelSize = 16;

        /// <summary>HUD phase/timer text.</summary>
        public const int PhaseTextSize = 26;

        /// <summary>Summon button main text.</summary>
        public const int SummonTextSize = 40;

        /// <summary>Summon button cost subtext.</summary>
        public const int SummonCostSize = 28;

        /// <summary>Menu button text.</summary>
        public const int MenuTextSize = 26;

        /// <summary>Countdown number.</summary>
        public const int CountdownSize = 160;
        #endregion

        #region Layout (reference 1080x1920)
        /// <summary>HUD panel total height.</summary>
        public const float HudHeight = 160f;

        /// <summary>HUD horizontal padding.</summary>
        public const float HudPaddingH = 24f;

        /// <summary>HUD vertical padding.</summary>
        public const float HudPaddingV = 12f;

        /// <summary>Stat card corner radius approximation (visual padding).</summary>
        public const float StatCardPadding = 8f;

        /// <summary>Summon button height (large touch target).</summary>
        public const float SummonButtonHeight = 110f;

        /// <summary>Menu button height.</summary>
        public const float MenuButtonHeight = 64f;

        /// <summary>Bottom area total reserve (summon + menu + spacing).</summary>
        public const float BottomReserve = 230f;

        /// <summary>Gap between bottom buttons.</summary>
        public const float ButtonGap = 12f;

        /// <summary>Button horizontal margin from screen edge.</summary>
        public const float ButtonMarginH = 0.04f;
        #endregion
    }
}
