public class FlyingBurst_Advanced_AIScript : FlyingBurst_Basic_AIScript
{
	protected override float MeleeFireSpread => 180f;

	protected override int NumFireballsMelee => 5;

	protected override int NumFireballsBasic => 0;
}
