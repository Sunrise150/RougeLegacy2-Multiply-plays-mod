using RLMod.Core.Sync;

public class SnapshotBufferTests
{
    [Fact]
    public void Samples_between_two_snapshots()
    {
        var buf = new SnapshotBuffer<Vec3>(8);
        buf.Add(1.0, new Vec3(0, 0, 0));
        buf.Add(2.0, new Vec3(10, 0, 0));
        Vec3 result;
        float t;
        Check.True(Interp.SampleEnemy(buf, 1.5, out result, out t));
        Check.Eq(5.0, result.X, 5, "halfway should be x=5");
        Check.InRange(t, 0.49, 0.51);
    }

    [Fact]
    public void Out_of_order_arrivals_slotted_duplicates_dropped()
    {
        var buf = new SnapshotBuffer<Vec3>(8);
        Check.True(buf.Add(1.0, new Vec3(1, 0, 0)));
        Check.True(buf.Add(3.0, new Vec3(3, 0, 0)));
        Check.True(buf.Add(2.0, new Vec3(2, 0, 0))); // late arrival, should slot in
        Check.False(buf.Add(2.0, new Vec3(99, 0, 0))); // duplicate time, dropped
        Check.False(buf.Add(double.NaN, new Vec3(0, 0, 0)));
        Check.False(buf.Add(double.PositiveInfinity, new Vec3(0, 0, 0)));
        Check.Eq(3, buf.Count);

        Vec3 r;
        float t;
        Interp.SampleEnemy(buf, 2.0, out r, out t);
        Check.Eq(2.0, r.X, 5, "sample at slot-in time must read the slotted value");
    }

    [Fact]
    public void Ring_keeps_only_newest_window_and_rejects_old()
    {
        var buf = new SnapshotBuffer<Vec3>(4);
        for (int i = 0; i < 10; i++) buf.Add(i, new Vec3(i, 0, 0));
        Check.Eq(4, buf.Count);
        Check.Eq(9.0, buf.NewestTime);
        Check.False(buf.Add(0.0, new Vec3(0, 0, 0)), "too-old packet must be rejected");
    }

    [Fact]
    public void Enemies_freeze_past_newest_no_extrapolation()
    {
        var buf = new SnapshotBuffer<Vec3>(8);
        buf.Add(1.0, new Vec3(0, 0, 0));
        buf.Add(2.0, new Vec3(10, 0, 0));
        Vec3 r;
        float t;
        Check.True(Interp.SampleEnemy(buf, 5.0, out r, out t));
        Check.Eq(10.0, r.X, 5, "enemy must freeze at last snapshot, not extrapolate");
    }

    [Fact]
    public void Extrapolation_is_bounded()
    {
        Vec3 pos = new Vec3(1, 0, 0);
        Vec3 vel = new Vec3(10, 0, 0);
        Vec3 r = Interp.Extrapolate(pos, vel, 3.9);
        Check.Eq(3.0, r.X, 5, "extrapolation capped at 0.2s → 1 + 10*0.2 = 3");
    }

    [Fact]
    public void Empty_buffer_samples_fail()
    {
        var buf = new SnapshotBuffer<Vec3>(8);
        Vec3 r;
        float t;
        Check.False(Interp.SampleEnemy(buf, 1.0, out r, out t));
    }
}
