namespace LottoDefense.Grid
{
    /// <summary>
    /// Defines the visual states a grid cell can be in.
    /// Used to determine the visual appearance and interaction feedback.
    /// </summary>
    public enum CellState
    {
        /// <summary>
        /// Default state with no interaction.
        /// </summary>
        Normal,

        /// <summary>
        /// Mouse is hovering over the cell.
        /// </summary>
        Hover,

        /// <summary>
        /// Cell is currently selected by the player.
        /// </summary>
        Selected,

        /// <summary>
        /// Cell cannot be used (e.g., invalid placement location).
        /// </summary>
        Invalid,

        /// <summary>
        /// Cell is occupied by a unit.
        /// </summary>
        Occupied
    }
}
