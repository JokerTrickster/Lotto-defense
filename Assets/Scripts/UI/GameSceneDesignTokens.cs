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
        /// <summary>Summon button gradient main (brighter vibrant green).</summary>
        public static readonly Color SummonButtonBg = new Color(0.15f, 0.8f, 0.35f, 1f);

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

        /// <summary>Summon button border glow (bright green outline).</summary>
        public static readonly Color SummonButtonBorder = new Color(0.3f, 1f, 0.55f, 0.8f);

        /// <summary>Summon button inner highlight (lighter green for depth).</summary>
        public static readonly Color SummonButtonInner = new Color(0.2f, 0.9f, 0.45f, 1f);
        #endregion

        #region Action Button Colors (Bottom Panel)
        /// <summary>Auto synthesis button (cyan-blue).</summary>
        public static readonly Color AutoSynthBtnBg = new Color(0.15f, 0.5f, 0.9f, 1f);
        public static readonly Color AutoSynthBtnBorder = new Color(0.3f, 0.65f, 1f, 0.8f);

        /// <summary>Attack upgrade button (fiery red-orange).</summary>
        public static readonly Color AttackUpBtnBg = new Color(0.85f, 0.25f, 0.15f, 1f);
        public static readonly Color AttackUpBtnBorder = new Color(1f, 0.45f, 0.3f, 0.8f);

        /// <summary>Attack speed upgrade button (emerald green).</summary>
        public static readonly Color SpeedUpBtnBg = new Color(0.1f, 0.7f, 0.35f, 1f);
        public static readonly Color SpeedUpBtnBorder = new Color(0.25f, 0.9f, 0.5f, 0.8f);

        /// <summary>Disabled action button.</summary>
        public static readonly Color ActionBtnDisabled = new Color(0.25f, 0.25f, 0.3f, 0.85f);
        public static readonly Color ActionBtnDisabledBorder = new Color(0.35f, 0.35f, 0.4f, 0.5f);
        #endregion

        #region Selection Panel Colors
        /// <summary>Panel background (dark navy, high opacity).</summary>
        public static readonly Color SelectionPanelBg = new Color(0.08f, 0.1f, 0.18f, 0.96f);

        /// <summary>Panel border (gold accent).</summary>
        public static readonly Color SelectionPanelBorder = new Color(1f, 0.78f, 0.2f, 0.9f);

        /// <summary>Sell button (red).</summary>
        public static readonly Color SellBtnBg = new Color(0.8f, 0.2f, 0.15f, 1f);
        public static readonly Color SellBtnBorder = new Color(1f, 0.4f, 0.35f, 0.7f);

        #endregion

        #region Synthesis Floating Button Colors
        /// <summary>Floating synthesis button background (bright amber-gold).</summary>
        public static readonly Color SynthFloatBtnBg = new Color(1f, 0.78f, 0.1f, 1f);

        /// <summary>Floating synthesis button border.</summary>
        public static readonly Color SynthFloatBtnBorder = new Color(1f, 0.92f, 0.5f, 0.9f);

        /// <summary>Floating synthesis button text (dark brown).</summary>
        public static readonly Color SynthFloatBtnText = new Color(0.2f, 0.12f, 0f, 1f);
        #endregion

        #region Countdown Colors
        /// <summary>Countdown number color (bright white with glow feel).</summary>
        public static readonly Color CountdownText = Color.white;

        /// <summary>Countdown overlay background (dark vignette).</summary>
        public static readonly Color CountdownOverlay = new Color(0f, 0f, 0f, 0.75f);

        /// <summary>Countdown "3" color (red).</summary>
        public static readonly Color Countdown3Color = new Color(1f, 0.3f, 0.3f, 1f);

        /// <summary>Countdown "2" color (yellow).</summary>
        public static readonly Color Countdown2Color = new Color(1f, 0.9f, 0.3f, 1f);

        /// <summary>Countdown "1" color (green).</summary>
        public static readonly Color Countdown1Color = new Color(0.3f, 1f, 0.5f, 1f);

        /// <summary>Countdown "START!" color (bright green).</summary>
        public static readonly Color CountdownStartColor = new Color(0.3f, 1f, 0.5f, 1f);
        #endregion

        #region Typography (font sizes - Mobile optimized 1.5x)
        /// <summary>HUD stat value (large, prominent number).</summary>
        public const int StatValueSize = 38; // 50 → 38 (compact HUD)

        /// <summary>HUD stat label ("ROUND", "GOLD" etc).</summary>
        public const int StatLabelSize = 18; // 24 → 18 (compact HUD)

        /// <summary>HUD phase/timer text.</summary>
        public const int PhaseTextSize = 38; // 26 → 38

        /// <summary>Summon button main text.</summary>
        public const int SummonTextSize = 60; // 40 → 60

        /// <summary>Summon button cost subtext.</summary>
        public const int SummonCostSize = 42; // 28 → 42

        /// <summary>Menu button text.</summary>
        public const int MenuTextSize = 38; // 26 → 38

        /// <summary>Countdown number.</summary>
        public const int CountdownSize = 240; // 160 → 240
        #endregion

        #region Layout (reference 1080x1920 - Mobile optimized 1.5x)
        /// <summary>HUD panel total height.</summary>
        public const float HudHeight = 160f; // 200 → 160 (compact HUD)

        /// <summary>HUD horizontal padding.</summary>
        public const float HudPaddingH = 32f; // 24 → 32

        /// <summary>HUD vertical padding.</summary>
        public const float HudPaddingV = 8f; // 16 → 8 (compact HUD)

        /// <summary>Stat card corner radius approximation (visual padding).</summary>
        public const float StatCardPadding = 12f; // 8 → 12

        /// <summary>Summon button height (large touch target for mobile).</summary>
        public const float SummonButtonHeight = 140f; // 110 → 140

        /// <summary>Menu button height (comfortable touch target).</summary>
        public const float MenuButtonHeight = 52f; // 80 → 52 (compact utility icon)

        /// <summary>Bottom area total reserve (summon + spacing).</summary>
        public const float BottomReserve = 180f; // 280 → 180 (less bottom reserve)

        /// <summary>Utility button size (back, guide icons).</summary>
        public const float UtilityButtonSize = 52f;

        /// <summary>Gap between bottom buttons.</summary>
        public const float ButtonGap = 18f; // 12 → 18

        /// <summary>Button horizontal margin from screen edge.</summary>
        public const float ButtonMarginH = 0.03f; // 0.04 → 0.03 (less margin for more button space)

        /// <summary>Bottom action panel height.</summary>
        public const float BottomPanelHeight = 130f;

        /// <summary>Floating synthesis button size.</summary>
        public const float SynthFloatBtnWidth = 100f;
        public const float SynthFloatBtnHeight = 44f;
        #endregion
    }
}
