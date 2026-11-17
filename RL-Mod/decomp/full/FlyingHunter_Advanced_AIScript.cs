public class FlyingHunter_Advanced_AIScript : FlyingHunter_Basic_AIScript
{
	protected override int TeleportOddsToFlipSideOnHit => 100;

	protected override bool TeleportSpawnProjectilesOnHit => true;

	protected override float TeleportHealthLossTrigger => 0.2f;
}
