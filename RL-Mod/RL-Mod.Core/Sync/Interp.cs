using System;

namespace RLMod.Core.Sync
{
    /// <summary>Math helpers with no Unity dependency, borrowed from rl2-coop's Interp.</summary>
    public static class Interp
    {
        public static float Lerp(float a, float b, float t) { return a + (b - a) * t; }
        public static float Clamp01(float v) { return v < 0f ? 0f : (v > 1f ? 1f : v); }
        public static float Clamp(float v, float lo, float hi) { return v < lo ? lo : (v > hi ? hi : v); }

        public static Vec3 Lerp(Vec3 a, Vec3 b, float t)
        {
            return new Vec3(Lerp(a.X, b.X, t), Lerp(a.Y, b.Y, t), Lerp(a.Z, b.Z, t));
        }

        /// <summary>
        /// Sample an enemy snapshot buffer at a render time. Enemies freeze (no
        /// extrapolation): past the newest snapshot from=to=last and t=1.
        /// </summary>
        public static bool SampleEnemy(SnapshotBuffer<Vec3> buf, double renderTime, out Vec3 result, out float t)
        {
            Vec3 a, b;
            double over;
            if (!buf.TrySample(renderTime, out a, out b, out t, out over)) { result = default(Vec3); return false; }
            result = Lerp(a, b, t);
            return true;
        }

        /// <summary>Dead-reckon a position a short time past the last snapshot, bounded.</summary>
        public static Vec3 Extrapolate(Vec3 pos, Vec3 vel, double seconds)
        {
            float s = (float)Math.Min(seconds, 0.2);
            return new Vec3(pos.X + vel.X * s, pos.Y + vel.Y * s, pos.Z);
        }
    }

    /// <summary>Plain 3D position value type — bridge to UnityEngine.Vector3 in the adapter layer.</summary>
    public struct Vec3
    {
        public float X, Y, Z;
        public Vec3(float x, float y, float z) { X = x; Y = y; Z = z; }

        public static Vec3 Zero { get { return new Vec3(0f, 0f, 0f); } }

        public float SqrDistance(Vec3 o)
        {
            float dx = X - o.X, dy = Y - o.Y, dz = Z - o.Z;
            return dx * dx + dy * dy + dz * dz;
        }
    }
}
