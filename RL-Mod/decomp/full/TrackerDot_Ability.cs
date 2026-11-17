using UnityEngine;

public class TrackerDot_Ability : FollowTargetAbility_RL
{
	protected override AbilityAnimState StateToHoldAttackOn => AbilityAnimState.Tell;

	protected override void FireProjectile()
	{
		if (!string.IsNullOrEmpty(ProjectileName))
		{
			Vector2 offset = m_targetGO.transform.position - m_abilityController.PlayerController.transform.position;
			m_firedProjectile = ProjectileManager.FireProjectile(m_abilityController.gameObject, ProjectileName, offset, matchFacing: false);
			m_abilityController.InitializeProjectile(m_firedProjectile);
			ApplyAbilityCosts();
		}
	}
}
