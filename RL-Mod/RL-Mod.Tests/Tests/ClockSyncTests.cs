using RLMod.Core.Sync;

public class ClockSyncTests
{
    [Fact]
    public void First_exchange_sets_the_offset()
    {
        var c = new ClockSync();
        // local send 10.00, host 110.02, local recv 10.04 → rtt 0.04, offset ≈ 100.0
        c.AddSample(10.00, 110.02, 10.04);
        Check.True(c.HasSample);
        Check.Eq(100.0, c.Offset, 6);
        Check.Eq(0.04, c.Rtt, 6);
        Check.Eq(110.5, c.ToHost(10.5), 6);
    }

    [Fact]
    public void Nonsense_round_trips_are_ignored()
    {
        var c = new ClockSync();
        c.AddSample(5, 100, 4);   // reply before request (negative rtt)
        c.AddSample(0, 100, 30);  // 30s rtt (stale/forged)
        Check.False(c.HasSample, "invalid samples must not be accepted");
        Check.Eq(0.0, c.Offset, 6);
    }

    [Fact]
    public void Slow_samples_do_not_drag_the_estimate()
    {
        var c = new ClockSync();
        // Two low-RTT samples at offset 100, then one queued (0.6s RTT) at offset 100.2.
        c.AddSample(0.0, 100.01, 0.02);
        c.AddSample(1.0, 101.01, 1.02);
        c.AddSample(2.0, 102.50, 2.60);
        Check.InRange(c.Offset, 99.9, 100.1, "queued reply must not win");
    }

    [Fact]
    public void Big_jumps_snap_small_drift_slews()
    {
        var c = new ClockSync();
        // First sample has higher RTT (0.05); offset ~100.
        c.AddSample(0.0, 100.025, 0.05);
        double before = c.Offset;

        // New sample has the LOWEST RTT so it becomes the best; error ~0.08s
        // is below the snap threshold → only 10% is corrected (slew).
        c.AddSample(1.0, 101.085, 1.01); // offset = 101.085 + 0.005 - 1.01 = 100.08
        Check.InRange(c.Offset - before, 0.001, 0.02, "small drift must slew");

        // Big jump: host timeline moves +100s → snap in one step.
        c.AddSample(2.0, 202.005, 2.01);
        Check.InRange(c.Offset, 199.9, 200.2, "big jump must snap");
    }

    [Fact]
    public void Seed_is_coarse_estimate_before_samples()
    {
        var c = new ClockSync();
        c.Seed(100.0, 0.0, 0.05); // offset ≈ 100.05
        Check.Eq(100.05, c.Offset, 6);
        c.AddSample(0.0, 100.0, 0.02);
        Check.True(c.HasSample);
        c.Seed(500.0, 0.0, 0.0); // ignored once real samples exist
        Check.True(c.Offset < 101.0);
    }

    [Fact]
    public void Reset_clears_all_state()
    {
        var c = new ClockSync();
        c.AddSample(0.0, 100.0, 0.02);
        c.Reset();
        Check.False(c.HasSample);
        Check.Eq(0.0, c.Offset, 6);
    }
}
