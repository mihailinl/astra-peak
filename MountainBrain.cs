using Astra.Sdk;
using UnityEngine;

namespace AstraPeak
{
    /// <summary>
    /// How she climbs PEAK with you. Every ordinary frame is the foundation's follower (walking the
    /// terrain, sliding along rocks, stepping up and down). This brain adds the mountain:
    /// <list type="bullet">
    /// <item>while you are on a wall, a rope or a vine (or in the air above where she can walk), she
    /// waits where she stands, under gravity, looking at you;</item>
    /// <item>when you stand again higher (or lower) than she can walk, she LEAPS to a spot beside you:
    /// a real arc, airborne the whole way, so her animation set plays a jump, not a teleport. With no
    /// ground beside you, she waits instead of landing in you.</item>
    /// </list>
    /// Only raw facts reach her animation set (her motion, <c>airborne</c>, <c>climbing</c>); what they
    /// look like is the set's business.
    /// </summary>
    sealed class MountainBrain : IBrain
    {
        /// <summary>A height gap she does not try to walk: she leaps it.</summary>
        const float LeapHeight = 2.5f;
        /// <summary>A leap is never longer than this (beyond, the follower's teleport is honest).</summary>
        const float LeapReach = 30f;

        Vector3 from, to;
        float leapTime, leapDuration, apex;
        bool leaping;

        public void Step(FrameContext f)
        {
            var her = f.Her;
            if (leaping)
            {
                Fly(f);
                return;
            }
            if (f.Player == null || !her.Placed)
            {
                f.Default.Step(f);
                return;
            }
            var player = f.Player.Value;
            var gap = player.Feet - her.Position;
            float flat = new Vector3(gap.x, 0, gap.z).magnitude;

            // On a wall, a rope or a vine — or in the air above/below where she can walk (a jump off
            // the top, the moment between letting go of a wall and touching ground): she WAITS where
            // she stands, under gravity, looking at you. The follower would otherwise teleport her.
            bool beyondWalking = Mathf.Abs(gap.y) > LeapHeight;
            if (PeakPlayer.Climbing || (!player.Grounded && beyondWalking))
            {
                f.Hold(player.Feet);
                return;
            }
            if (player.Grounded && beyondWalking && flat < LeapReach)
            {
                // Leap to a spot beside you; with no ground beside you (a pinnacle, a narrow ledge)
                // she waits rather than land in you.
                if (f.TrySpotNear(player.Feet, player.Forward, f.Follow.KeepDistance, out var spot)) Leap(her, spot);
                else f.Hold(player.Feet);
                return;
            }
            f.Default.Step(f);
        }

        void Leap(Her her, Vector3 target)
        {
            from = her.Position;
            to = target;
            float distance = Vector3.Distance(from, to);
            leapDuration = Mathf.Clamp(0.45f + distance * 0.06f, 0.6f, 1.6f);
            // The arc clears the higher end by a metre: up a cliff it rises above the top, down it hops.
            apex = Mathf.Max(from.y, to.y) + 1f;
            leapTime = 0;
            leaping = true;
            her.Airborne = true;
        }

        void Fly(FrameContext f)
        {
            var her = f.Her;
            leapTime += f.DeltaTime;
            float t = Mathf.Clamp01(leapTime / leapDuration);
            var flat = Vector3.Lerp(from, to, t);
            // A parabola through from.y (t=0), apex, to.y (t=1): two halves, each a quadratic ease.
            float y = t < 0.5f
                ? Mathf.Lerp(from.y, apex, 1 - (1 - 2 * t) * (1 - 2 * t))
                : Mathf.Lerp(apex, to.y, (2 * t - 1) * (2 * t - 1));
            her.Position = new Vector3(flat.x, y, flat.z);
            her.Airborne = t < 1;
            Face(her, to - from, f.DeltaTime * 3);
            if (t >= 1) leaping = false;
        }

        static void Face(Her her, Vector3 toward, float dt)
        {
            toward.y = 0;
            if (toward.sqrMagnitude < 1e-4f) return;
            her.Facing = Vector3.RotateTowards(her.Facing, toward.normalized, 4.7f * dt, 0f);
        }
    }
}
