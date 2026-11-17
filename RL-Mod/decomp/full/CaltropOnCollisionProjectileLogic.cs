using UnityEngine;

public class CaltropOnCollisionProjectileLogic : OnCollisionSpawnProjectileLogic
{
	protected override void Awake()
	{
		base.Awake();
		base.SourceProjectile.CastAbilityType = CastAbilityType.Talent;
	}

	protected override void SpawnProjectile(Projectile_RL projectile, GameObject colliderObj)
	{
		Vector3 vector = base.SourceProjectile.Midpoint;
		if (base.SourceProjectile.HitboxController.LastCollidedWith != null)
		{
			vector = base.SourceProjectile.HitboxController.LastCollidedWith.ClosestPoint(base.SourceProjectile.Midpoint);
		}
		Vector2 point = vector;
		point.y += base.SourceProjectile.transform.localScale.y * 0.5f;
		if (!(vector.y > base.SourceProjectile.Midpoint.y) || !base.SourceProjectile.HitboxController.LastCollidedWith.OverlapPoint(point))
		{
			base.SpawnProjectile(projectile, colliderObj);
			base.SourceProjectile.FlagForDestruction();
		}
	}
}
