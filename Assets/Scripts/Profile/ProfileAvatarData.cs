using UnityEngine;

namespace LottoDefense.Profile
{
    /// <summary>
    /// ScriptableObject defining a single profile avatar option.
    /// Contains sprite, metadata, and unlock conditions.
    /// </summary>
    [CreateAssetMenu(fileName = "ProfileAvatar", menuName = "LottoDefense/Profile/Avatar Data")]
    public class ProfileAvatarData : ScriptableObject
    {
        [Header("Avatar Identity")]
        [Tooltip("Unique identifier for this avatar")]
        public string avatarId;

        [Tooltip("Display name shown in selection UI")]
        public string avatarName;

        [Header("Visual")]
        [Tooltip("Avatar sprite (recommended 128x128 or 256x256)")]
        public Sprite avatarSprite;

        [Tooltip("Border color for avatar frame")]
        public Color borderColor = Color.white;

        [Header("Unlock Conditions")]
        [Tooltip("Is this avatar available by default?")]
        public bool isDefaultUnlocked = true;

        [Tooltip("Required quest ID to unlock (empty if default)")]
        public string requiredQuestId = "";

        [Tooltip("Required achievement to unlock (optional)")]
        public string requiredAchievement = "";

        [Tooltip("Flavor text describing how to unlock")]
        public string unlockHint = "";

        [Header("Rarity")]
        [Tooltip("Avatar rarity for visual effects")]
        public AvatarRarity rarity = AvatarRarity.Common;
    }

    public enum AvatarRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Hidden
    }
}
