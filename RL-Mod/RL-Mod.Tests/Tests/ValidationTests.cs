using RLMod.Core.Protocol;

public class ValidationTests
{
    [Fact]
    public void Rejects_non_finite_coords_and_damage()
    {
        Check.False(Validation.ValidCoord(float.NaN));
        Check.False(Validation.ValidCoord(float.PositiveInfinity));
        Check.False(Validation.ValidCoord(1e9f));
        Check.True(Validation.ValidCoord(123.4f));
        Check.False(Validation.ValidDamage(float.NaN));
        Check.False(Validation.ValidDamage(0f));
        Check.False(Validation.ValidDamage(-5f));
        Check.False(Validation.ValidDamage(Validation.MaxDamage + 1f));
        Check.True(Validation.ValidDamage(999f));
    }

    [Fact]
    public void Valid_amount_allows_zero_but_not_negative()
    {
        Check.True(Validation.ValidAmount(0f));
        Check.False(Validation.ValidAmount(-0.1f));
        Check.False(Validation.ValidAmount(float.NaN));
    }

    [Fact]
    public void Host_time_window_tolerates_jitter_but_rejects_absurd()
    {
        double now = 1000.0;
        Check.True(Validation.ValidHostTime(999.5, now));   // 0.5s ago
        Check.True(Validation.ValidHostTime(1000.3, now));  // 0.3s future
        Check.False(Validation.ValidHostTime(1001.0, now)); // 1s future
        Check.False(Validation.ValidHostTime(990.0, now));  // 10s past
        Check.False(Validation.ValidHostTime(double.NaN, now));
    }

    [Fact]
    public void Counts_and_player_ids_bounded()
    {
        Check.True(Validation.ValidCount(0, Validation.MaxEnemiesPerPacket));
        Check.True(Validation.ValidCount(Validation.MaxEnemiesPerPacket, Validation.MaxEnemiesPerPacket));
        Check.False(Validation.ValidCount(Validation.MaxEnemiesPerPacket + 1, Validation.MaxEnemiesPerPacket));
        Check.False(Validation.ValidCount(-1, Validation.MaxEnemiesPerPacket));
        Check.True(Validation.ValidPlayerId(0));
        Check.True(Validation.ValidPlayerId(3));
        Check.False(Validation.ValidPlayerId(4));
        Check.False(Validation.ValidPlayerId(-1));
    }

    [Fact]
    public void Sanitize_strips_control_chars_and_caps_length()
    {
        Check.Eq("Hi", Validation.Sanitize("H\u0001i"));
        Check.Eq("", Validation.Sanitize(null));
        Check.Eq("", Validation.Sanitize("\u0002\u0003"));
        Check.Eq(10, Validation.Sanitize("0123456789ABCDE", 10).Length);
    }
}
