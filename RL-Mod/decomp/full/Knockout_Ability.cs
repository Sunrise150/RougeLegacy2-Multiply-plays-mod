using System;
using UnityEngine;

public class Knockout_Ability : AimedAbility_RL, ITalent, IAbility
{
	protected float gravityMod;

	private Action<Projectile_RL, GameObject> m_onCollision;

	protected override AbilityAnimState StateToHoldAttackOn => AbilityAnimState.Attack_Intro;

	protected override float TellAnimSpeed => 2f;

	protected override float TellIntroAnimSpeed => 2f;

	protected override void Awake()
	{
		base.Awake();
		m_onCollision = OnCollision;
	}

	protected override void FireProjectile()
	{
		base.FireProjectile();
		if ((bool)m_firedProjectile)
		{
			m_firedProjectile.OnCollisionRelay.AddListener(m_onCollision);
			m_firedProjectile.ExternalKnockbackMod = CDGHelper.AngleToVector(m_unmoddedAngle);
		}
	}

	protected override void UpdateArrowAim(bool doNotUpdatePlayerAnims = false)
	{
		base.UpdateArrowAim(doNotUpdatePlayerAnims);
		if (!m_abilityController || !m_abilityController.PlayerController.IsGrounded || !m_abilityController.PlayerController.ControllerCorgi.StandingOnCollider || !m_abilityController.PlayerController.ControllerCorgi.StandingOnCollider.CompareTag("Platform"))
		{
			return;
		}
		Vector2 kNOCKOUT_MIN_MAX_ANGLE = Ability_EV.KNOCKOUT_MIN_MAX_ANGLE;
		m_aimAngle = Mathf.Clamp(m_aimAngle, kNOCKOUT_MIN_MAX_ANGLE.x, kNOCKOUT_MIN_MAX_ANGLE.y);
		m_unmoddedAngle = Mathf.Clamp(m_unmoddedAngle, kNOCKOUT_MIN_MAX_ANGLE.x, kNOCKOUT_MIN_MAX_ANGLE.y);
		if (m_unmoddedAngle == kNOCKOUT_MIN_MAX_ANGLE.x || m_unmoddedAngle == kNOCKOUT_MIN_MAX_ANGLE.y)
		{
			if (m_abilityController.PlayerController.IsFacingRight)
			{
				m_unmoddedAngle = kNOCKOUT_MIN_MAX_ANGLE.x;
			}
			else
			{
				m_unmoddedAngle = kNOCKOUT_MIN_MAX_ANGLE.y;
			}
		}
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		if ((bool)m_firedProjectile)
		{
			m_firedProjectile.OnCollisionRelay.RemoveListener(m_onCollision);
		}
		base.StopAbility(abilityInterrupted);
	}

	protected void OnCollision(Projectile_RL projectile, GameObject colliderObj)
	{
		if (colliderObj.CompareTag("Enemy"))
		{
			EffectManager.SetEffectParams("SlowTime_Effect", "m_timeScaleValue", 0.05f);
			EffectManager.PlayEffect(m_abilityController.PlayerController.gameObject, m_abilityController.PlayerController.Animator, "SlowTime_Effect", Vector3.zero, 0.175f, EffectStopType.Gracefully);
		}
	}
}
