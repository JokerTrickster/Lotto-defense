using System;
using System.Collections.Generic;

namespace LottoDefense.Profile
{
    /// <summary>
    /// Represents a user's profile data including nickname, selected avatar, and unlocked avatars.
    /// Serializable for saving/loading via PlayerPrefs or backend API.
    /// </summary>
    [Serializable]
    public class UserProfile
    {
        public string nickname = "Player";
        public string selectedAvatarId = "avatar_default";
        public List<string> unlockedAvatarIds = new List<string>();

        public UserProfile()
        {
            // Default constructor with sensible defaults
            nickname = "Player";
            selectedAvatarId = "avatar_default";
            unlockedAvatarIds = new List<string> { "avatar_default" };
        }

        public UserProfile(string nickname, string selectedAvatarId)
        {
            this.nickname = nickname;
            this.selectedAvatarId = selectedAvatarId;
            this.unlockedAvatarIds = new List<string> { "avatar_default", selectedAvatarId };
        }

        /// <summary>
        /// Check if user has unlocked a specific avatar.
        /// </summary>
        public bool HasUnlockedAvatar(string avatarId)
        {
            return unlockedAvatarIds != null && unlockedAvatarIds.Contains(avatarId);
        }

        /// <summary>
        /// Unlock a new avatar for this user.
        /// </summary>
        public void UnlockAvatar(string avatarId)
        {
            if (unlockedAvatarIds == null)
                unlockedAvatarIds = new List<string>();

            if (!unlockedAvatarIds.Contains(avatarId))
            {
                unlockedAvatarIds.Add(avatarId);
            }
        }

        /// <summary>
        /// Validate that the selected avatar is actually unlocked.
        /// If not, revert to default.
        /// </summary>
        public void ValidateSelectedAvatar()
        {
            if (!HasUnlockedAvatar(selectedAvatarId))
            {
                selectedAvatarId = "avatar_default";
            }
        }
    }
}
