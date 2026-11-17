using System.Collections;
using FMOD.Studio;
using FMODUnity;
using MoreMountains.CorgiEngine;
using RLAudio;
using Rewired;
using UnityEngine;

public class Lance_Ability : BaseAbility_RL, IAudioEventEmitter
{
	private enum ChargeState
	{
		Thrust,
		LongDash
	}

	[SerializeField]
	private AnimationCurve m_attackCurve;

	[SerializeField]
	private string m_chargeAttackProjectileName;

	[SerializeField]
	private Vector2 m_chargeAttackOffset;

	private float m_attackStartTime;

	private ChargeState m_currentChargeState;

	private GenericEffect m_thrustChargingUpEffect;

	private GenericEffect m_lanceChargingUpEffect;

	private bool m_isChargeEffectPlaying;

	private float m_storedFallMultiplier;

	private bool m_isAirThrustAttacking;

	private EventInstance m_chargeLoopEventInstance;

	private const string POLEARM_CHARGE_PARAM = "polearm_charge";

	private const string CHARGE_LOOP_EVENT_PATH = "event:/SFX/Weapons/sfx_weapon_polearm_charge_loop";

	private const string DASH_EVENT_PATH = "event:/SFX/Weapons/sfx_weapon_polearm_dash";

	private bool m_oneWayCollisionDisabled;

	public override bool HasAttackFlipCheck
	{
		get
		{
			if (m_currentChargeState == ChargeState.Thrust)
			{
				return base.HasAttackFlipCheck;
			}
			return false;
		}
	}

	public override string ProjectileName
	{
		get
		{
			if (m_currentChargeState == ChargeState.Thrust)
			{
				return base.ProjectileName;
			}
			return m_chargeAttackProjectileName;
		}
	}

	public override Vector2 ProjectileOffset
	{
		get
		{
			if (m_currentChargeState == ChargeState.Thrust)
			{
				return base.ProjectileOffset;
			}
			return m_chargeAttackOffset;
		}
	}

	public bool DashAttacking { get; private set; }

	protected override float TellIntroAnimSpeed => 1.5f;

	protected override float TellIntroAnimExitDelay => 0f;

	protected override float TellAnimSpeed => 5f;

	protected override float TellAnimExitDelay => 0f;

	protected override float AttackIntroAnimSpeed => 5f;

	protected override float AttackIntroAnimExitDelay => 0f;

	protected override float AttackAnimSpeed => 5f;

	protected override float AttackAnimExitDelay => 0.22f;

	protected override float ExitAnimSpeed => 1f;

	protected override float ExitAnimExitDelay
	{
		get
		{
			if (m_isAirThrustAttacking)
			{
				return 0.22f;
			}
			return 0f;
		}
	}

	public override bool LockDirectionWhenCasting
	{
		get
		{
			if (m_isChargeEffectPlaying)
			{
				return false;
			}
			return base.LockDirectionWhenCasting;
		}
	}

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[2] { base.ProjectileName, m_chargeAttackProjectileName };
	}

	public override void Initialize(CastAbility_RL abilityController, CastAbilityType castAbilityType)
	{
		base.Initialize(abilityController, castAbilityType);
		m_chargeLoopEventInstance = AudioUtility.GetEventInstance("event:/SFX/Weapons/sfx_weapon_polearm_charge_loop", base.transform);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (m_chargeLoopEventInstance.isValid())
		{
			m_chargeLoopEventInstance.release();
		}
	}

	public override IEnumerator CastAbility()
	{
		if (ReInput.isReady)
		{
			m_isAirThrustAttacking = false;
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
		if (base.CurrentAbilityAnimState == AbilityAnimState.Attack_Intro)
		{
			PlayDashAudio(m_currentChargeState == ChargeState.LongDash);
		}
		if (base.CurrentAbilityAnimState == AbilityAnimState.Attack)
		{
			if (m_currentChargeState == ChargeState.LongDash)
			{
				m_abilityController.PlayerController.CharacterJump.ResetBrakeForce();
				m_abilityController.PlayerController.SetVelocity(0f, 0f, additive: false);
				m_abilityController.PlayerController.FallMultiplierOverride = 0f;
				m_abilityController.PlayerController.ControllerCorgi.StickWhenWalkingDownSlopes = false;
				yield return PushForward();
				yield return base.ChangeAnim(0f);
			}
			else
			{
				yield return base.ChangeAnim(duration);
			}
		}
		else
		{
			yield return base.ChangeAnim(duration);
		}
	}

	private void StartChargeEffect()
	{
		m_thrustChargingUpEffect = EffectManager.PlayEffect(m_abilityController.PlayerController.gameObject, m_abilityController.PlayerController.Animator, "ThrustChargingUp_Effect", Vector2.zero, -2f, EffectStopType.Gracefully) as GenericEffect;
		m_thrustChargingUpEffect.AnimatorLayer = 1;
		m_thrustChargingUpEffect.transform.SetParent(m_abilityController.PlayerController.Pivot.transform, worldPositionStays: false);
		m_thrustChargingUpEffect.transform.localPosition = new Vector3(-0.25f, 0f, 0f);
		m_thrustChargingUpEffect.Animator.SetInteger("State", 0);
		m_lanceChargingUpEffect = EffectManager.PlayEffect(m_abilityController.PlayerController.gameObject, m_abilityController.PlayerController.Animator, "LanceChargingUp_Effect", Vector2.zero, -2f, EffectStopType.Gracefully) as GenericEffect;
		m_lanceChargingUpEffect.AnimatorLayer = 1;
		m_lanceChargingUpEffect.transform.SetParent(m_abilityController.PlayerController.Pivot.transform, worldPositionStays: false);
		m_lanceChargingUpEffect.transform.localPosition = Vector3.zero;
		m_lanceChargingUpEffect.Animator.SetInteger("State", 0);
		m_chargeLoopEventInstance.setParameterByName("polearm_charge", 0.5f, ignoreseekspeed: true);
		AudioManager.PlayAttached(this, m_chargeLoopEventInstance, base.gameObject);
	}

	private void StopChargeEffect()
	{
		if (m_isChargeEffectPlaying)
		{
			if ((bool)m_lanceChargingUpEffect)
			{
				m_lanceChargingUpEffect.Stop(EffectStopType.Gracefully);
				m_lanceChargingUpEffect = null;
			}
			if ((bool)m_thrustChargingUpEffect)
			{
				m_thrustChargingUpEffect.Stop(EffectStopType.Gracefully);
				m_thrustChargingUpEffect = null;
			}
			m_isChargeEffectPlaying = false;
			AudioManager.Stop(m_chargeLoopEventInstance);
		}
	}

	private void SetChargeState()
	{
		if (Time.time >= m_attackStartTime + 0.55f)
		{
			if (m_currentChargeState == ChargeState.Thrust)
			{
				CancelChangeAnimCoroutine();
				m_animator.Play("LanceDash_Tell_Hold");
				m_chargeLoopEventInstance.setParameterByName("polearm_charge", 1f);
			}
			m_currentChargeState = ChargeState.LongDash;
		}
		else
		{
			m_currentChargeState = ChargeState.Thrust;
		}
		ChargeState currentChargeState = m_currentChargeState;
		ChargeState chargeState = ChargeState.Thrust;
		if (Time.time >= m_attackStartTime + 0.55f - 0.02f)
		{
			chargeState = ChargeState.LongDash;
		}
		if (m_thrustChargingUpEffect != null && m_lanceChargingUpEffect != null && currentChargeState != chargeState)
		{
			switch (chargeState)
			{
			case ChargeState.Thrust:
				m_thrustChargingUpEffect.Animator.SetInteger("State", 0);
				m_lanceChargingUpEffect.Animator.SetInteger("State", 0);
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
		StopChargeEffect();
		if (m_currentChargeState == ChargeState.LongDash)
		{
			DashAttacking = true;
		}
		base.FireProjectile();
		if (!m_firedProjectile)
		{
			return;
		}
		if (m_currentChargeState == ChargeState.Thrust)
		{
			m_firedProjectile.Animator.SetBool("IsCharged", value: false);
			if (!m_abilityController.PlayerController.IsGrounded)
			{
				m_isAirThrustAttacking = true;
				m_abilityController.PlayerController.FallMultiplierOverride = m_storedFallMultiplier;
				m_abilityController.PlayerController.ConditionState = CharacterStates.CharacterConditions.DisableHorizontalMovement;
				if (!m_abilityController.PlayerController.IsFacingRight)
				{
					m_abilityController.PlayerController.SetVelocity(Ability_EV.LANCE_SWING_ATTACK_KNOCKBACK.x, Ability_EV.LANCE_SWING_ATTACK_KNOCKBACK.y, additive: false);
				}
				else
				{
					m_abilityController.PlayerController.SetVelocity(0f - Ability_EV.LANCE_SWING_ATTACK_KNOCKBACK.x, Ability_EV.LANCE_SWING_ATTACK_KNOCKBACK.y, additive: false);
				}
			}
		}
		else
		{
			m_firedProjectile.Animator.SetBool("IsCharged", value: true);
			float num = m_abilityController.PlayerController.transform.localScale.x / m_abilityController.PlayerController.BaseScaleToOffsetWith;
			Vector2 collisionPointOffset = new Vector2(2f * num, 0f);
			if (!m_abilityController.PlayerController.IsFacingRight)
			{
				collisionPointOffset.x *= -1f;
			}
			m_firedProjectile.CollisionPointOffset = collisionPointOffset;
			m_firedProjectile.ActualCritChance += 100f;
		}
	}

	protected IEnumerator PushForward()
	{
		float speed = 34f;
		float num = 30f;
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
		if (!m_abilityController.PlayerController.ControllerCorgi.DisableOneWayCollision)
		{
			m_abilityController.PlayerController.ControllerCorgi.DisableOneWayCollision = true;
			m_oneWayCollisionDisabled = true;
		}
		while (Time.time < endingTime)
		{
			float time = 1f - (endingTime - Time.time) / duration;
			float num2 = speed * m_attackCurve.Evaluate(time);
			float velocityY = (Rewired_RL.Player.GetButton("MoveVertical") ? 18f : ((!Rewired_RL.Player.GetNegativeButton("MoveVertical") || m_abilityController.PlayerController.IsGrounded) ? 0f : (-18f)));
			m_abilityController.PlayerController.DisableFriction = true;
			m_abilityController.PlayerController.DisableDoorBlock = true;
			if (m_abilityController.PlayerController.IsFacingRight)
			{
				m_abilityController.PlayerController.SetVelocity(num2, velocityY, additive: false);
			}
			else
			{
				m_abilityController.PlayerController.SetVelocity(0f - num2, velocityY, additive: false);
			}
			yield return null;
		}
		m_abilityController.PlayerController.DisableDoorBlock = false;
		m_abilityController.PlayerController.DisableFriction = false;
		if (m_oneWayCollisionDisabled)
		{
			m_abilityController.PlayerController.ControllerCorgi.DisableOneWayCollision = false;
			m_oneWayCollisionDisabled = false;
		}
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		m_abilityController.PlayerController.DisableFriction = false;
		m_abilityController.PlayerController.DisableDoorBlock = false;
		if (m_oneWayCollisionDisabled)
		{
			m_abilityController.PlayerController.ControllerCorgi.DisableOneWayCollision = false;
			m_oneWayCollisionDisabled = false;
		}
		m_abilityController.PlayerController.ControllerCorgi.StickWhenWalkingDownSlopes = true;
		if (m_abilityController.PlayerController.ConditionState != CharacterStates.CharacterConditions.Stunned)
		{
			m_abilityController.PlayerController.ConditionState = CharacterStates.CharacterConditions.Normal;
		}
		if ((bool)m_firedProjectile)
		{
			m_firedProjectile.FlagForDestruction();
			m_firedProjectile = null;
		}
		StopChargeEffect();
		m_abilityController.PlayerController.FallMultiplierOverride = m_storedFallMultiplier;
		StartCooldownTimer();
		DashAttacking = false;
		base.StopAbility(abilityInterrupted);
	}

	private void PlayDashAudio(bool fullyCharged)
	{
		EventInstance instance = RuntimeManager.CreateInstance("event:/SFX/Weapons/sfx_weapon_polearm_dash");
		RuntimeManager.AttachInstanceToGameObject(instance, base.transform, (Rigidbody2D)null);
		instance.setParameterByName("polearm_charge", fullyCharged ? 1f : 0f, ignoreseekspeed: true);
		instance.start();
		instance.release();
	}
}
