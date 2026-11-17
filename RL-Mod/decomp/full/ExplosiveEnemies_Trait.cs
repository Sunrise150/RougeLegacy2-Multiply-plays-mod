public class ExplosiveEnemies_Trait : BaseTrait
{
	public override TraitType TraitType => TraitType.ExplosiveEnemies;

	private void Start()
	{
		ProjectileManager.Instance.AddProjectileToPool("ExplosiveEnemiesPotionProjectile");
		ProjectileManager.Instance.AddProjectileToPool("ExplosiveEnemiesPotionExplosionProjectile");
	}
}
