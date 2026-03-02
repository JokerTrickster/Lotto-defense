using UnityEngine;

namespace LottoDefense.UI
{
    /// <summary>
    /// Design tokens for the in-game UI (GameScene).
    /// Cute/casual style: bright pastel panels, rounded shapes, warm accents, friendly colors.
    /// </summary>
    public static class GameSceneDesignTokens
    {
        #region HUD Colors
        public static readonly Color HudBackground = new Color(0.95f, 0.91f, 0.96f, 0.94f);
        public static readonly Color HudStatCardBg = new Color(1f, 0.97f, 0.93f, 0.9f);
        public static readonly Color HudBorder = new Color(0.82f, 0.75f, 0.85f, 0.5f);
        #endregion

        #region Stat Colors (warm, saturated accents on light background)
        public static readonly Color RoundColor = new Color(0.35f, 0.65f, 0.9f, 1f);
        public static readonly Color PhaseColor = new Color(0.3f, 0.25f, 0.2f, 1f);
        public static readonly Color TimeColor = new Color(0.85f, 0.6f, 0.2f, 1f);
        public static readonly Color LifeColor = new Color(0.95f, 0.35f, 0.45f, 1f);
        public static readonly Color GoldColor = new Color(0.9f, 0.7f, 0.1f, 1f);
        public static readonly Color MonsterColor = new Color(0.9f, 0.5f, 0.3f, 1f);
        public static readonly Color UnitColor = new Color(0.4f, 0.7f, 0.9f, 1f);
        public static readonly Color StatLabel = new Color(0.5f, 0.45f, 0.4f, 1f);
        #endregion

        #region Button Colors
        public static readonly Color SummonButtonBg = new Color(0.45f, 0.82f, 0.55f, 1f);
        public static readonly Color SummonButtonPressed = new Color(0.35f, 0.65f, 0.42f, 1f);
        public static readonly Color SummonButtonHighlight = new Color(0.5f, 0.88f, 0.6f, 1f);
        public static readonly Color SummonButtonDisabled = new Color(0.7f, 0.75f, 0.72f, 0.7f);
        public static readonly Color MenuButtonBg = new Color(0.75f, 0.7f, 0.82f, 0.9f);
        public static readonly Color MenuButtonPressed = new Color(0.6f, 0.55f, 0.68f, 0.95f);
        public static readonly Color ButtonText = new Color(0.25f, 0.2f, 0.15f, 1f);
        public static readonly Color ButtonTextShadow = new Color(1f, 1f, 1f, 0.3f);
        public static readonly Color ButtonCostText = new Color(0.7f, 0.5f, 0.1f, 1f);
        public static readonly Color SummonButtonBorder = new Color(0.55f, 0.9f, 0.65f, 0.8f);
        public static readonly Color SummonButtonInner = new Color(0.6f, 0.92f, 0.68f, 1f);
        #endregion

        #region Action Button Colors (Bottom Panel)
        public static readonly Color AutoSynthBtnBg = new Color(0.5f, 0.72f, 0.95f, 1f);
        public static readonly Color AutoSynthBtnBorder = new Color(0.6f, 0.8f, 1f, 0.8f);
        public static readonly Color AttackUpBtnBg = new Color(0.95f, 0.6f, 0.42f, 1f);
        public static readonly Color AttackUpBtnBorder = new Color(1f, 0.72f, 0.55f, 0.8f);
        public static readonly Color SpeedUpBtnBg = new Color(0.42f, 0.82f, 0.72f, 1f);
        public static readonly Color SpeedUpBtnBorder = new Color(0.55f, 0.9f, 0.8f, 0.8f);
        public static readonly Color ActionBtnDisabled = new Color(0.78f, 0.76f, 0.74f, 0.85f);
        public static readonly Color ActionBtnDisabledBorder = new Color(0.7f, 0.68f, 0.66f, 0.5f);
        #endregion

        #region Selection Panel Colors
        public static readonly Color SelectionPanelBg = new Color(1f, 0.97f, 0.93f, 0.96f);
        public static readonly Color SelectionPanelBorder = new Color(0.9f, 0.75f, 0.4f, 0.8f);
        public static readonly Color SellBtnBg = new Color(0.95f, 0.5f, 0.5f, 1f);
        public static readonly Color SellBtnBorder = new Color(1f, 0.65f, 0.6f, 0.7f);
        #endregion

        #region Unit Info Panel Colors
        public static readonly Color UnitInfoAttack = new Color(0.9f, 0.4f, 0.3f, 1f);
        public static readonly Color UnitInfoSpeed = new Color(0.3f, 0.75f, 0.5f, 1f);
        public static readonly Color UnitInfoRange = new Color(0.4f, 0.65f, 0.9f, 1f);
        public static readonly Color UnitInfoPatternColor = new Color(0.85f, 0.65f, 0.3f, 1f);
        public static readonly Color UnitInfoSkillColor = new Color(0.45f, 0.7f, 0.85f, 1f);
        public static readonly Color ManaBarBg = new Color(0.85f, 0.82f, 0.78f, 0.9f);
        public static readonly Color ManaBarFill = new Color(0.5f, 0.7f, 0.95f, 1f);
        public static readonly Color UnitInfoDefense = new Color(0.65f, 0.6f, 0.5f, 1f);
        public const float UnitInfoPanelHeight = 220f;
        public const int UnitInfoNameSize = 32;
        public const int UnitInfoDetailSize = 28;
        public const float UnitInfoStatRowHeight = 34f;
        public const int UnitInfoSkillTextSize = 28;
        #endregion

        #region Synthesis Floating Button Colors
        public static readonly Color SynthFloatBtnBg = new Color(1f, 0.78f, 0.28f, 1f);
        public static readonly Color SynthFloatBtnTopHighlight = new Color(1f, 0.92f, 0.6f, 1f);
        public static readonly Color SynthFloatBtnBorder = new Color(0.82f, 0.58f, 0.08f, 1f);
        public static readonly Color SynthFloatBtnText = new Color(0.35f, 0.2f, 0.02f, 1f);
        public static readonly Color SynthFloatBtnShadow = new Color(0.6f, 0.4f, 0.05f, 0.5f);
        #endregion

        #region Countdown Colors
        public static readonly Color CountdownText = Color.white;
        public static readonly Color CountdownOverlay = new Color(0.25f, 0.18f, 0.12f, 0.65f);
        public static readonly Color Countdown3Color = new Color(0.95f, 0.4f, 0.45f, 1f);
        public static readonly Color Countdown2Color = new Color(1f, 0.8f, 0.35f, 1f);
        public static readonly Color Countdown1Color = new Color(0.45f, 0.85f, 0.55f, 1f);
        public static readonly Color CountdownStartColor = new Color(0.45f, 0.85f, 0.55f, 1f);
        #endregion

        #region Typography (font sizes - Mobile optimized 1.5x)
        public const int StatValueSize = 38;
        public const int StatLabelSize = 18;
        public const int PhaseTextSize = 38;
        public const int SummonTextSize = 42;
        public const int SummonCostSize = 30;
        public const int MenuTextSize = 38;
        public const int CountdownSize = 240;
        #endregion

        #region Layout (reference 1080x1920 - Mobile optimized 1.5x)
        public const float HudHeight = 160f;
        public const float HudPaddingH = 32f;
        public const float HudPaddingV = 8f;
        public const float StatCardPadding = 12f;
        public const float SummonButtonHeight = 90f;
        public const float MenuButtonHeight = 52f;
        public const float BottomReserve = 180f;
        public const float UtilityButtonSize = 78f;
        public const float ButtonGap = 18f;
        public const float ButtonMarginH = 0.03f;
        public const float BottomPanelHeight = 130f;
        public const float CommandPanelHeight = 440f;
        public const float CommandButtonHeight = 70f;
        public const int CommandButtonTextSize = 30;
        public const float SynthFloatBtnWidth = 130f;
        public const float SynthFloatBtnHeight = 50f;
        #endregion

        #region Quest UI Colors
        public static readonly Color QuestPanelBg = new Color(0.98f, 0.95f, 0.9f, 0.97f);
        public static readonly Color QuestHiddenBg = new Color(0.92f, 0.9f, 0.87f, 0.9f);
        public static readonly Color QuestCompletedBg = new Color(0.88f, 0.96f, 0.88f, 0.9f);
        public static readonly Color QuestRewardedBg = new Color(0.9f, 0.88f, 0.85f, 0.7f);
        public static readonly Color QuestHiddenText = new Color(0.55f, 0.5f, 0.45f, 1f);
        public static readonly Color QuestCompletedText = new Color(0.3f, 0.7f, 0.4f, 1f);
        public static readonly Color QuestRewardedText = new Color(0.55f, 0.5f, 0.45f, 1f);
        public static readonly Color QuestRewardBtnBg = new Color(1f, 0.78f, 0.3f, 1f);
        public static readonly Color QuestBtnBg = new Color(0.7f, 0.55f, 0.9f, 1f);
        #endregion
    }
}
