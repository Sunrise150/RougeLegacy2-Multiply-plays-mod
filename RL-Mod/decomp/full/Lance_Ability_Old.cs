using System.Collections;
using MoreMountains.CorgiEngine;
using Rewired;
using UnityEngine;

public class Lance_Ability_Old : BaseAbility_RL
{
	private enum ChargeState
	{
		Thrust,
		ShortDash,
		LongDash
	}

	private const string THRUST_ATTACK_PROJECTILE_NAME = "LanceWeaponSwingProjectile";

	private const string DASH_ATTACK_PROJECTILE_NAME = "LanceWeaponProjectile";

	[SerializeField]
	private AnimationCurve m_attackCurve;

	private float m_attackStartTime;

	private ChargeState m_currentChargeState;

	private GenericEffect m_thrustChargingUpEffect;

	private GenericEffect m_lanceChargingUpEffect;

	private bool m_isChargeEffectPlaying;

	private float m_storedFallMultiplier;

	public override string ProjectileName
	{
		get
		{
			if (m_currentChargeState == ChargeState.Thrust)
			{
				return "LanceWeaponSwingProjectile";
			}
			return "LanceWeaponProjectile";
		}
	}

	protected override float TellIntroAnimSpeed => 1.25f;

	protected override float TellIntroAnimExitDelay => 0f;

	protected override float TellAnimSpeed => 1.25f;

	protected override float TellAnimExitDelay => 0f;

	protected override float AttackIntroAnimSpeed => 1f;

	protected override float AttackIntroAnimExitDelay => 0f;

	protected override float AttackAnimSpeed => 5f;

	protected override float AttackAnimExitDelay => 0f;

	protected override float ExitAnimSpeed => 1.5f;

	protected override float ExitAnimExitDelay => 0f;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[2] { "LanceWeaponSwingProjectile", "LanceWeaponProjectile" };
	}

	public override IEnumerator CastAbility()
	{
		if (ReInput.isReady)
		{
			m_currentChargeState = ChargeState.Thrust;
			m_attackStartTime = Time.time;
			m_isChargeEffectPlaying = false;
			m_storedFallMultiplier = m_abilityController.PlayerController.FallMultiplierOverride;
			m_abilityController.PlayerController.CharacterJump.ResetBrakeForce();
			m_abilityController.PlayerController.SetVelocity(0f, 0f, additive: false);
			m_abilityController.PlayerController.FallMultiplierOverride = 0.025f;
			yield return base.CastAbility();
		}
	}

	protected override void OnEnterAttackLogic()
	{
		m_abilityController.BroadcastAbilityCastEvents(base.CastAbilityType);
		FireProjectile();
	}

	protected override IEnumerator ChangeAnim(float duration)
	{
		while (base.CurrentAbilityAnimState == AbilityAnimState.Tell && Rewired_RL.Player.GetButton(m_abilityController.GetAbilityInputString(base.CastAbilityType)))
		{
			if (!m_isChargeEffectPlaying)
			{
				m_isChargeEffectPlaying = true;
				StartChargeEffect();
			}
			SetChargeState();
			yield return null;
		}
		if (base.CurrentAbilityAnimState == AbilityAnimState.Attack)
		{
			SetChargeState();
			m_abilityController.PlayerController.CharacterJump.ResetBrakeForce();
			m_abilityController.PlayerController.SetVelocity(0f, 0f, additive: false);
			m_abilityController.PlayerController.FallMultiplierOverride = 0f;
			if (m_currentChargeState != 0)
			{
				yield return PushForward();
			}
			yield return base.ChangeAnim(duration);
		}
		else
		{
			yield return base.ChangeAnim(duration);
		}
	}

	private void StartChargeEffect()
	{
		m_thrustChargingUpEffect = EffectManager.PlayEffect(m_abilityController.PlayerController.gameObject, m_abilityController.PlayerController.Animator, "ThrustChargingUp_Effect", Vector2.zero, -1f, EffectStopType.Gracefully) as GenericEffect;
		m_thrustChargingUpEffect.AnimatorLayer = 1;
		m_thrustChargingUpEffect.transform.SetParent(m_abilityController.PlayerController.Pivot.transform, worldPositionStays: false);
		m_thrustChargingUpEffect.transform.localPosition = new Vector3(-0.25f, 0f, 0f);
		m_thrustChargingUpEffect.Animator.SetInteger("State", 0);
		m_lanceChargingUpEffect = EffectManager.PlayEffect(m_abilityController.PlayerController.gameObject, m_abilityController.PlayerController.Animator, "LanceChargingUp_Effect", Vector2.zero, -1f, EffectStopType.Gracefully) as GenericEffect;
		m_lanceChargingUpEffect.AnimatorLayer = 1;
		m_lanceChargingUpEffect.transform.SetParent(m_abilityController.PlayerController.Pivot.transform, worldPositionStays: false);
		m_lanceChargingUpEffect.transform.localPosition = Vector3.zero;
		m_lanceChargingUpEffect.Animator.SetInteger("State", 0);
	}

	private void SetChargeState()
	{
		ChargeState currentChargeState = m_currentChargeState;
		if (Time.time >= m_attackStartTime + 0.55f)
		{
			m_currentChargeState = ChargeState.LongDash;
		}
		else if (Time.time >= m_attackStartTime + 0f)
		{
			m_currentChargeState = ChargeState.ShortDash;
		}
		else
		{
			m_currentChargeState = ChargeState.Thrust;
		}
		if (m_thrustChargingUpEffect != null && m_lanceChargingUpEffect != null && currentChargeState != m_currentChargeState)
		{
			switch (m_currentChargeState)
			{
			case ChargeState.Thrust:
				m_thrustChargingUpEffect.Animator.SetInteger("State", 0);
				m_lanceChargingUpEffect.Animator.SetInteger("State", 0);
				break;
			case ChargeState.ShortDash:
				m_thrustChargingUpEffect.Animator.SetInteger("State", 1);
				m_lanceChargingUpEffect.Animator.SetInteger("State", 1);
				break;
			case ChargeState.LongDash:
				m_thrustChargingUpEffect.Animator.SetInteger("State", 2);
				m_lanceChargingUpEffect.Animator.SetInteger("State", 2);
				break;
			}
		}
	}

	protected override void FireProjectile()
	{
		base.FireProjectile();
		if (m_firedProjectile != null)
		{
			if (m_currentChargeState == ChargeState.Thrust)
			{
				m_firedProjectile.Animator.SetBool("IsCharged", value: false);
			}
			else
			{
				m_firedProjectile.Animator.SetBool("IsCharged", value: true);
			}
		}
	}

	protected IEnumerator PushForward()
	{
		float speed = 0f;
		if (m_currentChargeState == ChargeState.LongDash)
		{
			speed = 34f;
		}
		float num = 0f;
		if (m_currentChargeState == ChargeState.LongDash)
		{
			num = 30f;
		}
		float duration = num / speed;
		float endingTime = Time.time + duration;
		if (m_abilityController.PlayerController.IsFacingRight)
		{
			m_abilityController.PlayerController.SetVelocityX(speed, additive: false);
		}
		else
		{
			m_abilityController.PlayerController.SetVelocityX(0f - speed, additive: false);
		}
		if (m_abilityController.PlayerController.IsGrounded)
		{
			m_abilityController.PlayerController.MovementState = CharacterStates.MovementStates.Idle;
		}
		while (Time.time < endingTime)
		{
			float time = 1f - (endingTime - Time.time) / duration;
			float num2 = speed * m_attackCurve.Evaluate(time);
			m_abilityController.PlayerController.DisableFriction = true;
			m_abilityController.PlayerController.DisableDoorBlock = true;
			if (m_abilityController.PlayerController.IsFacingRight)
			{
				m_abilityController.PlayerController.SetVelocityX(num2, additive: false);
			}
			else
			{
				m_abilityController.PlayerController.SetVelocityX(0f - num2, additive: false);
			}
			yield return null;
		}
		m_abilityController.PlayerController.DisableDoorBlock = false;
		m_abilityController.PlayerController.DisableFriction = false;
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		m_abilityController.PlayerController.DisableFriction = false;
		m_abilityController.PlayerController.DisableDoorBlock = false;
		if (m_firedProjectile != null)
		{
			m_firedProjectile.FlagForDestruction();
			m_firedProjectile = null;
		}
		m_isChargeEffectPlaying = false;
		m_abilityController.PlayerController.FallMultiplierOverride = m_storedFallMultiplier;
		StartCooldownTimer();
		base.StopAbility(abilityInterrupted);
	}
}
