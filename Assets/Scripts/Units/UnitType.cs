namespace LottoDefense.Units
{
    /// <summary>
    /// Defines the three types of units available in the game.
    /// Each type has different combat behaviors and specializations.
    /// </summary>
    public enum UnitType
    {
        /// <summary>
        /// Close-range combat units with high defense and short attack range.
        /// Specializes in blocking enemies and dealing damage at close quarters.
        /// </summary>
        Melee,

        /// <summary>
        /// Long-range attack units that can hit enemies from a distance.
        /// Typically has higher attack but lower defense than Melee units.
        /// </summary>
        Ranged,

        /// <summary>
        /// Support units that apply debuffs to enemies (slow, weaken, etc).
        /// Lower direct damage but provides strategic advantages through status effects.
        /// </summary>
        Debuffer
    }
}
