public interface IPersistentAbility
{
	bool IsPersistentActive { get; }

	void StopPersistentAbility();
}
