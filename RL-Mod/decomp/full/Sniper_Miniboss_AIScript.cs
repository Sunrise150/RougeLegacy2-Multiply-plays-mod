public class Sniper_Miniboss_AIScript : Sniper_Basic_AIScript
{
	protected override bool AimShot_FireSideShot => true;

	protected override float AimShot_SideShotSpread => 3f;

	protected override int AimShot_TotalShots => 20;
}
