using System.Collections.Generic;
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
        static readonly GameType CharacterRefsType = GameType.Find("CharacterRefs");

        static readonly Member<object> DataMember = CharacterType.Member<object>("data");
        static readonly Member<object> RefsMember = CharacterType.Member<object>("refs");
        static readonly Member<Animator> AnimatorMember = CharacterRefsType.Member<Animator>("animator");
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

        /// <summary>Your scout's Animator (<c>Character.refs.animator</c>, on its <c>Scout</c> object), or
        /// null when there is no scout.</summary>
        public static Animator Animator
        {
            get
            {
                var c = LocalCharacter;
                if (c == null) return null;
                var refs = RefsMember.Get(c);
                return refs == null ? null : AnimatorMember.Get(refs);
            }
        }

        /// <summary>
        /// The scout's bones as VRM humanoid bones, by transform name under its Animator: the scouts
        /// are a Generic rig (no Unity Avatar), so the capture is told which bone is which. The
        /// pelvis-side bones (<c>Hip_L</c>/<c>Hip_R</c>) and the hand's <c>Hand_Upper_*</c> sit between
        /// mapped bones and are folded into them; the scouts have no neck and no ring finger. The
        /// game's names are not symmetric for the thumbs (<c>Thumb_1_L</c>, <c>Thumb_R_1</c>).
        /// </summary>
        public static readonly Dictionary<string, string> Bones = new Dictionary<string, string>
        {
            ["hips"] = "Hip", ["spine"] = "Mid", ["chest"] = "AimJoint", ["upperChest"] = "Torso", ["head"] = "Head",
            ["leftUpperLeg"] = "Leg_L", ["leftLowerLeg"] = "Knee_L", ["leftFoot"] = "Foot_L", ["leftToes"] = "S_Toe_1_L",
            ["rightUpperLeg"] = "Leg_R", ["rightLowerLeg"] = "Knee_R", ["rightFoot"] = "Foot_R", ["rightToes"] = "S_Toe_1_R",
            ["leftShoulder"] = "S_Shoulder_L", ["leftUpperArm"] = "Arm_L", ["leftLowerArm"] = "Elbow_L", ["leftHand"] = "Hand_L",
            ["rightShoulder"] = "S_Shoulder_R", ["rightUpperArm"] = "Arm_R", ["rightLowerArm"] = "Elbow_R", ["rightHand"] = "Hand_R",
            ["leftThumbMetacarpal"] = "Thumb_1_L", ["leftThumbProximal"] = "Thumb_2_L", ["leftThumbDistal"] = "Thumb_3_L",
            ["leftIndexProximal"] = "Index_1_L", ["leftIndexIntermediate"] = "Index_2_L", ["leftIndexDistal"] = "Index_3_L",
            ["leftMiddleProximal"] = "Middle_1_L", ["leftMiddleIntermediate"] = "Middle_2_L", ["leftMiddleDistal"] = "Middle_3_L",
            ["leftLittleProximal"] = "Pinky_1_L", ["leftLittleIntermediate"] = "Pinky_2_L", ["leftLittleDistal"] = "Pinky_3_L",
            ["rightThumbMetacarpal"] = "Thumb_R_1", ["rightThumbProximal"] = "Thumb_R_2", ["rightThumbDistal"] = "Thumb_R_3",
            ["rightIndexProximal"] = "Index_1_R", ["rightIndexIntermediate"] = "Index_2_R", ["rightIndexDistal"] = "Index_3_R",
            ["rightMiddleProximal"] = "Middle_1_R", ["rightMiddleIntermediate"] = "Middle_2_R", ["rightMiddleDistal"] = "Middle_3_R",
            ["rightLittleProximal"] = "Pinky_1_R", ["rightLittleIntermediate"] = "Pinky_2_R", ["rightLittleDistal"] = "Pinky_3_R",
        };

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
