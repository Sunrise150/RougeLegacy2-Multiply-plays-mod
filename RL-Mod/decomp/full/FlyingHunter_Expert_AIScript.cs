public class FlyingHunter_Expert_AIScript : FlyingHunter_Basic_AIScript
{
	protected override int TeleportOddsToFlipSideOnHit => 50;

	protected override bool TeleportSpawnProjectilesOnHit => true;

	protected override float TeleportHealthLossTrigger => 0.15f;
}
