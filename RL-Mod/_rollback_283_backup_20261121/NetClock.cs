using System;
using RLMod.Core.Sync;

namespace RL2Multiplayer
{
    /// <summary>
    /// v2.5.0: adapter around RLMod.Core.ClockSync. Establishes one shared host timeline
    /// across machines so every timestamped packet and interpolation samples the same axis.
    ///
    /// Host: HostNow = monotonic stopwatch (authoritative).
    /// Guest: HostNow = local stopwatch + offset, where offset is a NTP-style estimate
    ///        from TimePing/TimePong exchanges (lowest-RTT sample, slew/snap).
    ///
    /// All render times (EnemyPositionSync etc.) use HostNow, so local frame jitter and
    /// per-machine clock drift no longer turn into visible twitching.
    /// </summary>
    internal static class NetClock
    {
        private static readonly IClock Local = new StopwatchClock();
        private static readonly ClockSync Sync = new ClockSync();

        private const float BurstInterval = 0.2f;   // first 6 pings fast, so timeline is ready early
        private const int BurstCount = 6;
        private const float SteadyInterval = 1.0f;

        private static double _nextPing;
        private static int _pingsSent;
        private static float _lastLog;

        /// <summary>Local monotonic time in seconds (host timeline base component).</summary>
        internal static double LocalNow { get { return Local.Now; } }

        /// <summary>Current time on the shared host timeline, in seconds.</summary>
        internal static double HostNow
        {
            get
            {
                var nm = NetworkManager.Instance;
                if (nm == null || nm.IsHost) return Local.Now;
                return Sync.ToHost(Local.Now);
            }
        }

        internal static bool Ready { get { return Sync.HasSample; } }
        internal static double Offset { get { return Sync.Offset; } }
        internal static double Rtt { get { return Sync.Rtt; } }

        /// <summary>Welcome packet: coarse one-way seed using LiteNetLib's peer ping.</summary>
        internal static void SeedFromWelcome(double hostTime, double peerPingMs)
        {
            Sync.Seed(hostTime, Local.Now, peerPingMs / 1000.0);
        }

        /// <summary>Guest: drive ping cadence from the network pump. Returns a client timestamp to send, or null.</summary>
        internal static bool ShouldPing(out double clientTime)
        {
            clientTime = 0;
            var nm = NetworkManager.Instance;
            if (nm == null || nm.IsHost) return false;
            double now = Local.Now;
            if (now < _nextPing) return false;
            _pingsSent++;
            _nextPing = now + (_pingsSent < BurstCount ? BurstInterval : SteadyInterval);
            clientTime = now;
            return true;
        }

        /// <summary>Guest: feed a TimePong (clientTime echoed by host, hostTime at host).</summary>
        internal static void OnPong(double clientTime, double hostTime)
        {
            Sync.AddSample(clientTime, hostTime, Local.Now);
            if (Local.Now - _lastLog > 10f)
            {
                _lastLog = (float)Local.Now;
                Plugin.Log.LogInfo("[RL2MP] [CLOCK] 主机时钟已同步: offset=" + Sync.Offset.ToString("F3")
                    + "s rtt=" + Sync.Rtt.ToString("F3") + "s（主机 " + HostNow.ToString("F1") + "s）");
            }
        }

        internal static void Reset()
        {
            Sync.Reset();
            ResetPings();
        }

        /// <summary>重启 ping 突发（欢迎包/重连后调用），让时间轴尽快就绪。</summary>
        internal static void ResetPings()
        {
            _pingsSent = 0;
            _nextPing = 0;
        }
    }
}
