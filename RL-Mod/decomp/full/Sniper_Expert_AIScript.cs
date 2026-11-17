public class Sniper_Expert_AIScript : Sniper_Basic_AIScript
{
	protected override bool AimShot_FireSideShot => true;

	protected override float AimShot_SideShotSpread => 9f;

	protected override int AimShot_TotalShots => 3;
}
