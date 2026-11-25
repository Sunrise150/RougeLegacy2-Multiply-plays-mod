using System;
using System.Diagnostics;

namespace RLMod.Core.Sync
{
    /// <summary>Injectable monotonic clock. Tests substitute a fake; production uses a stopwatch.</summary>
    public interface IClock
    {
        double Now { get; }
    }

    /// <summary>Monotonic stopwatch clock — never jumps backwards with wall-clock adjustments.</summary>
    public sealed class StopwatchClock : IClock
    {
        private readonly Stopwatch _sw = Stopwatch.StartNew();
        public double Now { get { return _sw.Elapsed.TotalSeconds; } }
    }

    /// <summary>
    /// NTP-style offset estimation of the host timeline, borrowed from rl2-coop's ClockSync.
    /// hostTime ≈ localTime + Offset. Small errors converge smoothly (slew) so the render
    /// timeline never jumps; large errors (host restart) snap once. Out-of-order/negative
    /// RTT samples are dropped so a fake or stale reply cannot drag the estimate.
    /// </summary>
    public sealed class ClockSync
    {
        private const int Window = 8;
        private const double SnapThreshold = 0.25; // seconds
        private const double SlewRate = 0.1;
        private const double RttAlpha = 0.2;
        private const double MaxRtt = 5.0;

        private readonly double[] _offsets = new double[Window];
        private readonly double[] _rtts = new double[Window];
        private int _samples;
        private int _next;

        /// <summary>hostTime ≈ localTime + Offset.</summary>
        public double Offset { get; private set; }

        /// <summary>Smoothed round-trip time in seconds.</summary>
        public double Rtt { get; private set; }

        public bool HasSample { get { return _samples > 0; } }

        /// <summary>Coarse one-way estimate from the Welcome packet (no RTT known yet).</summary>
        public void Seed(double hostTime, double localReceiveTime, double oneWayGuess)
        {
            if (HasSample) return;
            Offset = hostTime + oneWayGuess - localReceiveTime;
        }

        /// <summary>Feed one ping/pong exchange. Four timestamps, NTP standard form.</summary>
        public void AddSample(double localSendTime, double hostTime, double localReceiveTime)
        {
            double rtt = localReceiveTime - localSendTime;
            if (rtt < 0 || rtt > MaxRtt) return; // clock went backwards or a stale/forged reply
            double offset = hostTime + rtt * 0.5 - localReceiveTime;

            _offsets[_next] = offset;
            _rtts[_next] = rtt;
            _next = (_next + 1) % Window;
            bool first = _samples == 0;
            if (_samples < Window) _samples++;

            Rtt = first ? rtt : Rtt + (rtt - Rtt) * RttAlpha;

            // Take the sample with the lowest RTT — least queued, most trustworthy offset.
            int best = 0;
            for (int i = 1; i < _samples; i++)
                if (_rtts[i] < _rtts[best]) best = i;
            double target = _offsets[best];

            double error = target - Offset;
            if (first || Math.Abs(error) > SnapThreshold) Offset = target;
            else Offset += error * SlewRate;
        }

        public double ToHost(double localTime) { return localTime + Offset; }

        public void Reset()
        {
            _samples = 0;
            _next = 0;
            Offset = 0;
            Rtt = 0;
            Array.Clear(_offsets, 0, _offsets.Length);
            Array.Clear(_rtts, 0, _rtts.Length);
        }
    }
}
