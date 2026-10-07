using Astra.Sdk;
using UnityEngine;

namespace AstraPeak
{
    /// <summary>PEAK's local scout, as the Astra SDK's <see cref="PlayerInfo"/>.</summary>
    static class PeakPlayer
    {
        /// <summary>The top of a scout's head above its head bone, as a fraction of the head height.</summary>
        const float HeadTop = 0.1f;

        /// <summary>Your scout, or null in the airport menu, while loading, or when you are dead (she
        /// then stays where she is).</summary>
        public static PlayerInfo? Locate()
        {
            var c = Character.localCharacter;
            if (c == null || c.data == null || c.data.dead) return null;
            var d = c.data;
            // On the ground, PEAK knows the ground point; in the air or on a wall, the feet are a
            // head-height (of the pose of the moment) below the head.
            float headHeight = d.targetHeadHeight > 0.1f ? d.targetHeadHeight : 1.6f;
            var feet = d.isGrounded ? d.groundPos : c.Head + Vector3.down * headHeight;
            // The scout's height is its head above the GROUND it stands on (targetHeadHeight is above
            // the rig's root, which is not the feet).
            float standing = c.Head.y - d.groundPos.y;
            return new PlayerInfo
            {
                Feet = feet,
                Forward = d.lookDirection_Flat,
                Root = c.gameObject,
                Grounded = d.isGrounded && !Climbing,
                // PEAK's head height is the head of the pose of the moment (lower in a crouch,
                // different in a jump): it is the scout's height only while it STANDS.
                Height = d.isGrounded && !d.isCrouching && !Climbing && standing > 0.2f ? standing * (1 + HeadTop) : 0,
            };
        }

        /// <summary>On a wall, a rope or a vine.</summary>
        public static bool Climbing
        {
            get
            {
                var c = Character.localCharacter;
                if (c == null || c.data == null) return false;
                var d = c.data;
                return d.isClimbing || d.isRopeClimbing || d.isVineClimbing;
            }
        }
    }
}
