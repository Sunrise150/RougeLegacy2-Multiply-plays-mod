using System;
using RLMod.Core.Protocol;
using RLMod.Core.Sync;

namespace RL2Multiplayer
{
    /// <summary>
    /// v2.5.0: adapter-layer glue around the game-independent RLMod.Core validation.
    ///
    /// Design borrowed from rl2-coop's "host never trusts the client":
    ///  - On the HOST, the sender id in a packet is never trusted. It is forced to the
    ///    id bound to the peer at handshake time (_peerToPlayerId). A guest cannot
    ///    impersonate another player by lying about its id in the packet body.
    ///  - On a GUEST, packets arrive from the host, which already re-stamped the true
    ///    sender id while relaying, so the id is authoritative.
    ///  - Every numeric field is range/finite checked before reaching game logic;
    ///    invalid packets are dropped (never crash) with a throttled log.
    /// </summary>
    internal static class NetValidation
    {
        private static float _lastWarnAt;
        private const float WarnThrottle = 2f;

        /// <summary>
        /// Resolve the authoritative sender id. Host: from peer binding (overrides the
        /// claimed id). Guest: the claimed id (host-stamped) but sanity-checked.
        /// Returns false when the packet must be dropped.
        /// </summary>
        internal static bool ResolveSender(NetworkManager nm, object peer, int claimed, out int trusted)
        {
            trusted = claimed;
            var litePeer = peer as LiteNetLib.NetPeer;
            if (litePeer == null) return false;

            if (nm.IsHost)
            {
                // Only guests (non-zero bound ids) need re-stamping; host's own loopback is ignored upstream.
                int bound;
                if (nm.TryGetBoundPlayerId(litePeer.Id, out bound))
                {
                    if (bound != claimed)
                    {
                        Warn("sender id 伪造？包内=" + claimed + " 连接绑定=" + bound + " -> 强制用绑定值");
                        trusted = bound; // force-refill; do NOT drop, keeps legit reorder harmless
                    }
                    return true;
                }
                // A packet from a peer not yet bound (pre-Welcome) is ignored.
                return false;
            }

            // Guest side: host is authoritative. Only reject structurally impossible ids.
            if (!Validation.ValidPlayerId(claimed)) { Warn("非法 sender id=" + claimed); return false; }
            return true;
        }

        internal static bool Coord(float x, float y, string tag)
        {
            if (!Validation.ValidCoord(x) || !Validation.ValidCoord(y))
            {
                Warn(tag + " 坐标非法 x=" + x + " y=" + y + " -> 丢包");
                return false;
            }
            return true;
        }

        internal static bool Coord3(float x, float y, float z, string tag)
        {
            if (!Validation.ValidCoord(x) || !Validation.ValidCoord(y) || !Validation.ValidCoord(z))
            {
                Warn(tag + " 3D 坐标非法 -> 丢包");
                return false;
            }
            return true;
        }

        internal static bool Velocity(float vx, float vy, string tag)
        {
            if (!Validation.ValidVelocity(vx) || !Validation.ValidVelocity(vy))
            {
                Warn(tag + " 速度非法 -> 归零由调用方处理");
                return false;
            }
            return true;
        }

        internal static float ClampVelocity(float v)
        {
            return Validation.IsFinite(v) ? Interp.Clamp(v, -Validation.MaxVelocity, Validation.MaxVelocity) : 0f;
        }

        internal static bool Damage(float d, string tag)
        {
            if (!Validation.ValidDamage(d))
            {
                Warn(tag + " 伤害非法 dmg=" + d + " -> 丢包");
                return false;
            }
            return true;
        }

        internal static float ClampAmount(float v)
        {
            return Validation.IsFinite(v) ? Interp.Clamp(v, 0f, Validation.MaxDamage) : 0f;
        }

        internal static bool Count(int n, int max, string tag)
        {
            if (!Validation.ValidCount(n, max))
            {
                Warn(tag + " 数量越界 n=" + n + " max=" + max + " -> 丢包");
                return false;
            }
            return true;
        }

        internal static bool IsFiniteD(double d) { return Validation.IsFinite(d); }

        internal static bool PlayerId(int id, string tag = "playerId")
        {
            if (!Validation.ValidPlayerId(id)) { Warn(tag + " 非法=" + id + " -> 丢包"); return false; }
            return true;
        }

        internal static string Text(string s)
        {
            return Validation.Sanitize(s, Validation.MaxStringChars);
        }

        internal static void Warn(string msg)
        {
            float now = UnityEngine.Time.unscaledTime;
            if (now - _lastWarnAt < WarnThrottle) return;
            _lastWarnAt = now;
            Plugin.Log.LogWarning("[RL2MP] [VALID] " + msg);
        }
    }
}
