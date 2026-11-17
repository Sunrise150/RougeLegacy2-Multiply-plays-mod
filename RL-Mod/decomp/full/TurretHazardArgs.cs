public class TurretHazardArgs : HazardArgs
{
	public TurretLogicType LogicType { get; private set; }

	public float InitialFireDelay { get; private set; }

	public float LoopFireDelay { get; private set; }

	public float ProjectileSpeedMod { get; private set; }

	public bool UseHalfLoopDelay { get; private set; }

	public TurretHazardArgs(StateID initialState, TurretLogicType logicType, bool useHalfLoopDelay, float initialFireDelay, float loopFireDelay, float projectileSpeedMod)
		: base(initialState)
	{
		UseHalfLoopDelay = useHalfLoopDelay;
		LogicType = logicType;
		InitialFireDelay = initialFireDelay;
		LoopFireDelay = loopFireDelay;
		ProjectileSpeedMod = projectileSpeedMod;
	}
}
