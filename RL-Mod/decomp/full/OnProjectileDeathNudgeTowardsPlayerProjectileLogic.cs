using UnityEngine;

public class OnProjectileDeathNudgeTowardsPlayerProjectileLogic : OnProjectileDeathProjectileLogic
{
	protected override void SpawnProjectile(Projectile_RL projectile, GameObject colliderObj)
	{
		Vector2 offset = base.Offset;
		if (PlayerManager.IsInstantiated)
		{
			GameObject player = PlayerManager.GetPlayer();
			if ((bool)player)
			{
				base.Offset += 0.5f * CDGHelper.AngleToVector(CDGHelper.AngleBetweenPts(base.SourceProjectile.Midpoint, player.transform.position));
			}
		}
		base.SpawnProjectile(projectile, colliderObj);
		base.Offset = offset;
	}
}
