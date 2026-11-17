public interface ITurretSettingsConsumer
{
	float InitialFireDelay { get; }

	TurretLogicType LogicType { get; }

	float LoopFireDelay { get; }

	float ProjectileSpeedMod { get; }

	bool UseHalfLoopDelay { get; }

	void SetInitialFireDelay(float value);

	void SetLogic(TurretLogicType logicType);

	void SetLoopFireDelay(float value);

	void SetProjectileSpeedMod(float value);

	void SetUseHalfLoopDelay(bool value);
}
