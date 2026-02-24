namespace LottoDefense.Gameplay
{
    /// <summary>
    /// 게임 난이도
    /// </summary>
    public enum GameDifficulty
    {
        /// <summary>보통 - 기본 스탯</summary>
        Normal = 0,
        
        /// <summary>어려움 - 체력 +50%, 방어력 +30%</summary>
        Hard = 1,
        
        /// <summary>매우 어려움 - 체력 +100%, 방어력 +50%</summary>
        VeryHard = 2
    }

    /// <summary>
    /// 난이도별 스탯 배율
    /// </summary>
    public static class DifficultyMultipliers
    {
        public static float GetHealthMultiplier(GameDifficulty difficulty)
        {
            switch (difficulty)
            {
                case GameDifficulty.Normal: return 1.0f;
                case GameDifficulty.Hard: return 1.5f;
                case GameDifficulty.VeryHard: return 2.0f;
                default: return 1.0f;
            }
        }

        public static float GetDefenseMultiplier(GameDifficulty difficulty)
        {
            switch (difficulty)
            {
                case GameDifficulty.Normal: return 1.0f;
                case GameDifficulty.Hard: return 1.3f;
                case GameDifficulty.VeryHard: return 1.5f;
                default: return 1.0f;
            }
        }

        public static float GetGoldMultiplier(GameDifficulty difficulty)
        {
            switch (difficulty)
            {
                case GameDifficulty.Normal: return 1.0f;
                case GameDifficulty.Hard: return 1.5f;
                case GameDifficulty.VeryHard: return 2.0f;
                default: return 1.0f;
            }
        }

        public static string GetDisplayName(GameDifficulty difficulty)
        {
            switch (difficulty)
            {
                case GameDifficulty.Normal: return "보통";
                case GameDifficulty.Hard: return "어려움";
                case GameDifficulty.VeryHard: return "매우 어려움";
                default: return "보통";
            }
        }
    }
}
