using UnityEngine;

public class GroundBowProjectile_RL : Projectile_RL
{
	public override void WeaponOnEnterHitResponse(IHitboxController otherHBController)
	{
		bool flag = false;
		if (CollisionType_RL.IsProjectile(otherHBController.RootGameObject))
		{
			Projectile_RL component = otherHBController.RootGameObject.GetComponent<Projectile_RL>();
			if (!component || !Projectile_RL.CollisionFlagAllowed(this, component))
			{
				return;
			}
			if ((bool)component && CompareTag("PlayerProjectile") && component.CompareTag("EnemyProjectile") && component.DieOnCharacterCollision)
			{
				flag = true;
			}
		}
		if (base.Owner == otherHBController.RootGameObject && !CanHitOwner)
		{
			return;
		}
		m_onCollisionRelay.Dispatch(this, otherHBController.RootGameObject);
		if (DieOnCharacterCollision)
		{
			bool flag2 = true;
			if (otherHBController.CollisionType == CollisionType.FlimsyBreakable)
			{
				flag2 = false;
			}
			if (flag)
			{
				flag2 = false;
			}
			if (flag2)
			{
				m_collidedObj = otherHBController.RootGameObject;
				FlagForDestruction();
				m_destructionType = ProjectileDestructionType.CharacterCollision;
				if (otherHBController.RootGameObject.CompareTag("Player") && PlayerManager.GetPlayerController().CharacterDash.IsDashing && SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.UnlockVoidDash) > 0)
				{
					m_destructionType = ProjectileDestructionType.VoidDashCollision;
				}
			}
		}
		if ((bool)base.Owner && (bool)PlayerManager.GetPlayerController())
		{
			GameObject rootGameObject = otherHBController.RootGameObject;
			EnemyController component2 = rootGameObject.GetComponent<EnemyController>();
			if ((bool)component2 && (component2.RicochetsAttackerOnHit || base.RicochetsOwnerWhenHits) && base.CanBeRicocheted)
			{
				RicochetPlayer(null, rootGameObject);
			}
		}
		PerformHitEffectCheck(otherHBController);
	}
}
