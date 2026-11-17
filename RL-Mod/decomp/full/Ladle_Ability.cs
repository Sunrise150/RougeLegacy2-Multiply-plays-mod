using System;
using UnityEngine;

public class Ladle_Ability : BaseAbility_RL, IAttack, IAbility
{
	private Action<Projectile_RL, GameObject> m_onProjectileCollision;

	protected override float TellIntroAnimSpeed => 3f;

	protected override float TellIntroAnimExitDelay => 0.01f;

	protected override float TellAnimSpeed => 1.25f;

	protected override float TellAnimExitDelay => 0f;

	protected override float AttackIntroAnimSpeed => 1.25f;

	protected override float AttackIntroAnimExitDelay => 0f;

	protected override float AttackAnimSpeed => 1.25f;

	protected override float AttackAnimExitDelay => 0.075f;

	protected override float ExitAnimSpeed => 1.25f;

	protected override float ExitAnimExitDelay => 0f;

	protected override void Awake()
	{
		base.Awake();
		m_onProjectileCollision = OnProjectileCollision;
	}

	public override void Initialize(CastAbility_RL abilityController, CastAbilityType castAbilityType)
	{
		base.Initialize(abilityController, castAbilityType);
		abilityController.Animator.SetBool("FryingPan_UsePirateAudio", value: false);
	}

	protected override void FireProjectile()
	{
		base.FireProjectile();
		m_firedProjectile.OnCollisionRelay.AddListener(m_onProjectileCollision);
	}

	private void OnProjectileCollision(Projectile_RL proj, GameObject colliderObj)
	{
		if (CollisionType_RL.IsProjectile(colliderObj))
		{
			m_abilityController.PlayerController.StatusEffectController.StartStatusEffect(StatusEffectType.Player_FreeCrit, 3f, null);
			if ((bool)m_firedProjectile)
			{
				m_firedProjectile.OnCollisionRelay.RemoveListener(m_onProjectileCollision);
			}
		}
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		if ((bool)m_firedProjectile)
		{
			m_firedProjectile.OnCollisionRelay.RemoveListener(m_onProjectileCollision);
		}
		base.StopAbility(abilityInterrupted);
	}
}
