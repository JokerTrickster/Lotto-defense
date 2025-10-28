namespace LottoDefense.Gameplay
{
    /// <summary>
    /// Defines all possible game phases in the Lotto Defense gameplay flow.
    /// </summary>
    public enum GameState
    {
        /// <summary>
        /// Initial countdown phase (3-2-1) before gameplay begins.
        /// </summary>
        Countdown,

        /// <summary>
        /// Player preparation phase for placing/upgrading towers.
        /// </summary>
        Preparation,

        /// <summary>
        /// Active combat phase with monsters spawning and moving.
        /// </summary>
        Combat,

        /// <summary>
        /// Round result display phase showing performance and rewards.
        /// </summary>
        RoundResult,

        /// <summary>
        /// Victory state when player completes all target rounds.
        /// </summary>
        Victory,

        /// <summary>
        /// Defeat state when player's life reaches zero.
        /// </summary>
        Defeat
    }
}
