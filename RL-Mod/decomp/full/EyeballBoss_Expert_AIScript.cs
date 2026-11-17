public class EyeballBoss_Expert_AIScript : EyeballBoss_Basic_AIScript
{
	protected override bool ZoneBlast_Variant => true;

	protected override bool SpinningFireball_Variant => true;

	protected override bool HomingFireball_Variant => true;

	protected override bool HomingFireball_Variant_Blue => true;

	protected override float MultiShot_Exit_AttackCD => 2.5f;
}
