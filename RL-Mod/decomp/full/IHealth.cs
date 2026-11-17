using Sigtrap.Relays;

public interface IHealth
{
	int CurrentHealthAsInt { get; }

	float CurrentHealth { get; }

	int BaseMaxHealth { get; }

	int ActualMaxHealth { get; }

	float MaxHealthMod { get; }

	int MaxHealthAdd { get; }

	IRelayLink<object, HealthChangeEventArgs> HealthChangeRelay { get; }

	IRelayLink<object, HealthChangeEventArgs> MaxHealthChangeRelay { get; }

	void SetHealth(float amount, bool additive, bool runEvents);
}
