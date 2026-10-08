using Astra.Sdk;
using UnityEngine;

namespace AstraPeak
{
    /// <summary>
    /// PEAK's local scout, as the Astra SDK's <see cref="PlayerInfo"/>. Reached by NAME
    /// (<see cref="Astra.Sdk.GameType"/>): this plugin never compiles against the game's own
    /// assemblies (verified against the installed game with <c>tools/check-game-members</c> in
    /// astra-bepinex).
    /// </summary>
    static class PeakPlayer
    {
        /// <summary>The top of a scout's head above its head bone, as a fraction of the head height.</summary>
        const float HeadTop = 0.1f;

        static readonly GameType CharacterType = GameType.Find("Character");
        static readonly GameType CharacterDataType = GameType.Find("CharacterData");

        static readonly Member<object> DataMember = CharacterType.Member<object>("data");
        static readonly Member<Vector3> Head = CharacterType.Member<Vector3>("Head");
        static readonly Member<bool> Dead = CharacterDataType.Member<bool>("dead");
        static readonly Member<float> TargetHeadHeight = CharacterDataType.Member<float>("targetHeadHeight");
        static readonly Member<bool> IsGrounded = CharacterDataType.Member<bool>("isGrounded");
        static readonly Member<Vector3> GroundPos = CharacterDataType.Member<Vector3>("groundPos");
        static readonly Member<Vector3> LookDirectionFlat = CharacterDataType.Member<Vector3>("lookDirection_Flat");
        static readonly Member<bool> IsCrouching = CharacterDataType.Member<bool>("isCrouching");
        static readonly Member<bool> IsClimbing = CharacterDataType.Member<bool>("isClimbing");
        static readonly Member<bool> IsRopeClimbing = CharacterDataType.Member<bool>("isRopeClimbing");
        static readonly Member<bool> IsVineClimbing = CharacterDataType.Member<bool>("isVineClimbing");

        static object LocalCharacter => CharacterType.Static<object>("localCharacter");

        /// <summary>Your scout, or null in the airport menu, while loading, or when you are dead (she
        /// then stays where she is).</summary>
        public static PlayerInfo? Locate()
        {
            var c = LocalCharacter;
            var root = c as Component;
            if (root == null) return null;
            var d = DataMember.Get(c);
            if (d == null || Dead.Get(d)) return null;
            // On the ground, PEAK knows the ground point; in the air or on a wall, the feet are a
            // head-height (of the pose of the moment) below the head.
            float thh = TargetHeadHeight.Get(d);
            float headHeight = thh > 0.1f ? thh : 1.6f;
            var head = Head.Get(c);
            bool grounded = IsGrounded.Get(d);
            var groundPos = GroundPos.Get(d);
            var feet = grounded ? groundPos : head + Vector3.down * headHeight;
            // The scout's height is its head above the GROUND it stands on (targetHeadHeight is above
            // the rig's root, which is not the feet).
            float standing = head.y - groundPos.y;
            bool climbing = Climbing;
            return new PlayerInfo
            {
                Feet = feet,
                Forward = LookDirectionFlat.Get(d),
                Root = root.gameObject,
                Grounded = grounded && !climbing,
                // PEAK's head height is the head of the pose of the moment (lower in a crouch,
                // different in a jump): it is the scout's height only while it STANDS.
                Height = grounded && !IsCrouching.Get(d) && !climbing && standing > 0.2f ? standing * (1 + HeadTop) : 0,
            };
        }

        /// <summary>On a wall, a rope or a vine.</summary>
        public static bool Climbing
        {
            get
            {
                var c = LocalCharacter;
                if (c == null) return false;
                var d = DataMember.Get(c);
                if (d == null) return false;
                return IsClimbing.Get(d) || IsRopeClimbing.Get(d) || IsVineClimbing.Get(d);
            }
        }
    }
}
