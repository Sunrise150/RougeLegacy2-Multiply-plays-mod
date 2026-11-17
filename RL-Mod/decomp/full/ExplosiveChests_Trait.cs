public class ExplosiveChests_Trait : BaseTrait
{
	public override TraitType TraitType => TraitType.ExplosiveChests;

	private void Start()
	{
		ProjectileManager.Instance.AddProjectileToPool("ExplosiveChestsPotionProjectile");
		ProjectileManager.Instance.AddProjectileToPool("ExplosiveChestsPotionExplosionProjectile");
	}
}
