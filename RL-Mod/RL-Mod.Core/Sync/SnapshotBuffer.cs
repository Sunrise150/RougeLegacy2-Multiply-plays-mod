using System;

namespace RLMod.Core.Sync
{
    /// <summary>
    /// Ring buffer of timestamped snapshots, borrowed from rl2-coop's SnapshotBuffer.
    /// Timestamps live on a single (host) timeline. Out-of-order arrivals are slotted
    /// in; duplicates, NaN/Inf and too-old packets are dropped. TrySample interpolates
    /// between the two snapshots bracketing the render time.
    /// </summary>
    public sealed class SnapshotBuffer<T> where T : struct
    {
        private readonly double[] _times;
        private readonly T[] _values;
        private int _start;
        private int _count;

        public SnapshotBuffer(int capacity = 32)
        {
            if (capacity < 2) throw new ArgumentOutOfRangeException(nameof(capacity));
            _times = new double[capacity];
            _values = new T[capacity];
        }

        public int Count { get { return _count; } }
        public double NewestTime { get { return _count == 0 ? double.NaN : _times[Index(_count - 1)]; } }
        public T Newest { get { return _count == 0 ? default(T) : _values[Index(_count - 1)]; } }

        public void Clear() { _start = 0; _count = 0; }

        public bool Add(double time, T value)
        {
            if (double.IsNaN(time) || double.IsInfinity(time)) return false;

            if (_count > 0)
            {
                double newest = _times[Index(_count - 1)];
                if (time < newest)
                {
                    if (_count == _times.Length && time <= _times[Index(0)]) return false;
                    int pos = _count;
                    while (pos > 0 && _times[Index(pos - 1)] > time) pos--;
                    if (pos > 0 && _times[Index(pos - 1)] == time) return false;
                    InsertAt(pos, time, value);
                    return true;
                }
                if (time == newest) return false;
            }

            if (_count == _times.Length) { _start = (_start + 1) % _times.Length; _count--; }
            int slot = Index(_count);
            _times[slot] = time;
            _values[slot] = value;
            _count++;
            return true;
        }

        /// <summary>
        /// Interpolate at renderTime. from/to bracket it; t in [0,1]. overshoot > 0 means
        /// renderTime is past the newest snapshot (caller may extrapolate, bounded).
        /// </summary>
        public bool TrySample(double renderTime, out T from, out T to, out float t, out double overshoot)
        {
            overshoot = 0;
            t = 0;
            if (_count == 0) { from = to = default(T); return false; }

            int first = Index(0);
            if (_count == 1 || renderTime <= _times[first])
            {
                from = to = _values[first];
                if (_count == 1) overshoot = Math.Max(0, renderTime - _times[first]);
                return true;
            }

            int last = Index(_count - 1);
            if (renderTime >= _times[last])
            {
                from = to = _values[last];
                t = 1;
                overshoot = renderTime - _times[last];
                return true;
            }

            for (int i = _count - 2; i >= 0; i--)
            {
                int a = Index(i);
                if (_times[a] <= renderTime)
                {
                    int b = Index(i + 1);
                    from = _values[a];
                    to = _values[b];
                    double span = _times[b] - _times[a];
                    t = span <= 0 ? 1f : (float)((renderTime - _times[a]) / span);
                    return true;
                }
            }

            from = to = _values[first];
            return true;
        }

        private void InsertAt(int pos, double time, T value)
        {
            if (_count == _times.Length)
            {
                _start = (_start + 1) % _times.Length;
                _count--;
                pos--;
            }
            for (int i = _count; i > pos; i--)
            {
                _times[Index(i)] = _times[Index(i - 1)];
                _values[Index(i)] = _values[Index(i - 1)];
            }
            _times[Index(pos)] = time;
            _values[Index(pos)] = value;
            _count++;
        }

        private int Index(int logical) { return (_start + logical) % _times.Length; }
    }

    public sealed class InterpolationSettings
    {
        /// <summary>Render delay in seconds. ~3 snapshots at 30Hz survives one lost packet + jitter.</summary>
        public double Delay = 0.1;
        /// <summary>Longest dead-reckoning past the last snapshot before freezing.</summary>
        public double MaxExtrapolation = 0.2;
    }
}
