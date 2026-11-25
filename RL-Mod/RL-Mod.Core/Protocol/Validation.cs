using System;
using RLMod.Core.Sync;

namespace RLMod.Core.Protocol
{
    /// <summary>
    /// Pure, game-independent input validation for network packets. Borrowed from
    /// rl2-coop's layered "host never trusts the client" design: every numeric field
    /// is checked for finiteness and a sane range before it reaches game logic.
    /// </summary>
    public static class Validation
    {
        public const int MaxPlayers = 4;
        public const int MaxEnemiesPerPacket = 64;
        public const int MaxObstaclesPerPacket = 64;
        public const int MaxStringChars = 256;
        public const float MaxDamage = 1000000f;
        public const float MaxCoord = 100000f;    // rooms are tens of units; 100k is generous
        public const float MaxVelocity = 500f;
        public const float MaxTimeSpan = 5f;      // accepted state timestamp window into the past

        public static bool IsFinite(float f) { return !float.IsNaN(f) && !float.IsInfinity(f); }
        public static bool IsFinite(double f) { return !double.IsNaN(f) && !double.IsInfinity(f); }

        public static bool ValidCoord(float v) { return IsFinite(v) && v >= -MaxCoord && v <= MaxCoord; }
        public static bool ValidCoord(float x, float y) { return ValidCoord(x) && ValidCoord(y); }
        public static bool ValidCoord(float x, float y, float z) { return ValidCoord(x) && ValidCoord(y) && ValidCoord(z); }
        public static bool ValidCoord(Vec3 p) { return ValidCoord(p.X, p.Y, p.Z); }
        public static bool ValidVelocity(float v) { return IsFinite(v) && v >= -MaxVelocity && v <= MaxVelocity; }

        /// <summary>Damage: must be finite, positive and below the cap. Rejects NaN/Inf/negative/absurd.</summary>
        public static bool ValidDamage(float d) { return IsFinite(d) && d > 0f && d <= MaxDamage; }

        /// <summary>Attenuating fields (stun, timers) may be zero but must be finite and non-negative.</summary>
        public static bool ValidAmount(float d) { return IsFinite(d) && d >= 0f && d <= MaxDamage; }

        public static bool ValidCount(int n, int max) { return n >= 0 && n <= max; }

        public static bool ValidPlayerId(int id) { return id >= 0 && id < MaxPlayers; }

        /// <summary>
        /// A state timestamp on the host timeline is sane if finite and within
        /// [now - 5s, now + 0.5s]. Out-of-range is rewritten to `now` by the caller.
        /// </summary>
        public static bool ValidHostTime(double stateTime, double now)
        {
            return IsFinite(stateTime) && stateTime <= now + 0.5 && stateTime >= now - MaxTimeSpan;
        }

        /// <summary>String sanitized for chat/name: strip control chars, cap length, non-null.</summary>
        public static string Sanitize(string s, int maxChars = MaxStringChars)
        {
            if (s == null) return string.Empty;
            if (s.Length > maxChars) s = s.Substring(0, maxChars);
            char[] buf = new char[s.Length];
            int n = 0;
            for (int i = 0; i < s.Length; i++)
                if (!char.IsControl(s[i])) buf[n++] = s[i];
            return new string(buf, 0, n);
        }
    }
}
