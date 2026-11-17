using Sigtrap.Relays;

public interface ICooldown
{
	IRelayLink<object, CooldownEventArgs> OnBeginCooldownRelay { get; }

	bool IsOnCooldown { get; }

	float BaseCooldownTime { get; }

	float ActualCooldownTime { get; }

	float CooldownTimer { get; }
}
