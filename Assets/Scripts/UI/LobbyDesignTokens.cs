using UnityEngine;

namespace LottoDefense.UI
{
    public static class LobbyDesignTokens
    {
        #region Background Colors
        public static readonly Color Background = new Color(0.08f, 0.08f, 0.16f, 1f);
        public static readonly Color TopBarBg = new Color(0.06f, 0.06f, 0.12f, 0.95f);
        public static readonly Color ModalOverlay = new Color(0f, 0f, 0f, 0.7f);
        public static readonly Color ModalPanelBg = new Color(0.12f, 0.12f, 0.22f, 1f);
        public static readonly Color CardBg = new Color(0.15f, 0.15f, 0.28f, 1f);
        public static readonly Color CardBgLocked = new Color(0.1f, 0.1f, 0.15f, 1f);
        public static readonly Color CardBgHighlight = new Color(0.2f, 0.2f, 0.35f, 1f);
        #endregion

        #region Button Colors
        public static readonly Color ButtonPrimary = new Color(0.26f, 0.52f, 0.96f, 1f);
        public static readonly Color ButtonSuccess = new Color(0.2f, 0.75f, 0.3f, 1f);
        public static readonly Color ButtonDanger = new Color(0.85f, 0.2f, 0.2f, 1f);
        public static readonly Color ButtonDisabled = new Color(0.3f, 0.3f, 0.35f, 1f);
        public static readonly Color ButtonSecondary = new Color(0.35f, 0.35f, 0.45f, 1f);
        public static readonly Color ButtonText = Color.white;
        public static readonly Color ButtonClose = new Color(0.7f, 0.2f, 0.2f, 1f);
        #endregion

        #region Currency Colors
        public static readonly Color GoldColor = new Color(1f, 0.84f, 0f, 1f);
        public static readonly Color TicketColor = new Color(0.4f, 0.8f, 1f, 1f);
        #endregion

        #region Badge Colors
        public static readonly Color BadgeBg = new Color(0.9f, 0.15f, 0.15f, 1f);
        public static readonly Color BadgeText = Color.white;
        #endregion

        #region Text Colors
        public static readonly Color TextPrimary = Color.white;
        public static readonly Color TextSecondary = new Color(0.7f, 0.7f, 0.8f, 1f);
        public static readonly Color TextMuted = new Color(0.5f, 0.5f, 0.6f, 1f);
        public static readonly Color TextSuccess = new Color(0.3f, 1f, 0.4f, 1f);
        #endregion

        #region Rarity Colors
        public static readonly Color RarityNormal = new Color(0.7f, 0.7f, 0.7f, 1f);
        public static readonly Color RarityRare = new Color(0.3f, 0.5f, 1f, 1f);
        public static readonly Color RarityEpic = new Color(0.7f, 0.3f, 1f, 1f);
        public static readonly Color RarityLegendary = new Color(1f, 0.6f, 0.1f, 1f);
        #endregion

        #region Progress Bar Colors
        public static readonly Color ProgressBg = new Color(0.15f, 0.15f, 0.2f, 1f);
        public static readonly Color ProgressFill = new Color(0.3f, 0.7f, 1f, 1f);
        public static readonly Color ProgressComplete = new Color(0.3f, 1f, 0.4f, 1f);
        #endregion

        #region Tab Colors
        public static readonly Color TabActive = new Color(0.26f, 0.52f, 0.96f, 1f);
        public static readonly Color TabInactive = new Color(0.2f, 0.2f, 0.3f, 1f);
        #endregion

        #region Typography
        public const int TitleSize = 72;
        public const int HeaderSize = 42;
        public const int SubHeaderSize = 32;
        public const int BodySize = 26;
        public const int SmallSize = 22;
        public const int BadgeFontSize = 18;
        public const int CurrencySize = 30;
        public const int ButtonFontSize = 32;
        public const int IconButtonFontSize = 28;
        #endregion

        #region Layout
        public const float TopBarHeight = 120f;
        public const float IconButtonSize = 88f;
        public const float IconSpacing = 16f;
        public const float ModalPadding = 40f;
        public const float CardPadding = 16f;
        public const float BadgeSize = 36f;
        public const float ProgressBarHeight = 20f;
        public const float GameStartButtonWidth = 500f;
        public const float GameStartButtonHeight = 120f;
        #endregion
    }
}
