using System.Collections;
using UnityEngine;

public class AxeHold_Ability : Sword_Ability
{
	[Header("Airborne Values")]
	[SerializeField]
	protected string m_abilityAirTellIntroName;

	[SerializeField]
	protected AbilityData m_airborneAbilityData;

	[SerializeField]
	protected string m_airborneProjectileName;

	[SerializeField]
	protected Vector2 m_airBorneProjectileOffset;

	[SerializeField]
	protected bool m_hasAirborneAttackFlipCheck;

	[Header("Attack Landing Values")]
	[SerializeField]
	protected AbilityData m_landingAbilityData;

	[SerializeField]
	protected string m_landingProjectileName;

	[SerializeField]
	protected Vector2 m_landingProjectileOffset;

	private bool m_isAirAttacking;

	private WaitUntil m_waitUntilLandedYield;

	protected float AttackMovementModAfterSpinLand;

	protected float TellIntroAnimSpeedGround => 10f;

	protected float TellIntroAnimSpeedAir => 10f;

	protected float TellIntroAnimExitDelayGround => 0f;

	protected float TellIntroAnimExitDelayAir => 0f;

	protected float TellHoldAnimSpeedGround => 1f;

	protected float TellHoldAnimSpeedAir => 1.15f;

	protected float TellHoldAnimDelayGround => 0.025f;

	protected float TellHoldAnimDelayAir => 0f;

	protected float AttackIntroAnimSpeedGround => 1f;

	protected float AttackIntroAnimSpeedAir => 1.15f;

	protected float AttackIntroAnimExitDelayGround => 0f;

	protected float AttackIntroAnimExitDelayAir => 0f;

	protected float AttackHoldAnimSpeedGround => 0.85f;

	protected float AttackHoldAnimSpeedAir => 0.85f;

	protected float AttackHoldAnimDelayGround => 0f;

	protected float AttackHoldAnimDelayAir => 0f;

	protected float ExitIntroAnimSpeedGround => 1.6f;

	protected float ExitIntroAnimSpeedAir => 1.4f;

	protected float ExitHoldAnimDelayGround => 0f;

	protected float ExitHoldAnimDelayAir => 0f;

	public override Vector2 ProjectileOffset
	{
		get
		{
			if (PerformGroundAttack)
			{
				return base.ProjectileOffset;
			}
			if (PerformLandingAttack)
			{
				return m_landingProjectileOffset;
			}
			return m_airBorneProjectileOffset;
		}
	}

	protected override float TellIntroAnimSpeed
	{
		get
		{
			if (PerformGroundAttack)
			{
				return TellIntroAnimSpeedGround;
			}
			return TellIntroAnimSpeedAir;
		}
	}

	protected override float TellIntroAnimExitDelay
	{
		get
		{
			if (PerformGroundAttack)
			{
				return TellIntroAnimExitDelayGround;
			}
			return TellIntroAnimExitDelayAir;
		}
	}

	protected override float TellAnimSpeed
	{
		get
		{
			if (PerformGroundAttack)
			{
				return TellHoldAnimSpeedGround;
			}
			return TellHoldAnimSpeedAir;
		}
	}

	protected override float TellAnimExitDelay
	{
		get
		{
			if (PerformGroundAttack)
			{
				return TellHoldAnimDelayGround;
			}
			return TellHoldAnimDelayAir;
		}
	}

	protected override float AttackIntroAnimSpeed
	{
		get
		{
			if (PerformGroundAttack)
			{
				return AttackIntroAnimSpeedGround;
			}
			return AttackIntroAnimSpeedAir;
		}
	}

	protected override float AttackIntroAnimExitDelay
	{
		get
		{
			if (PerformGroundAttack)
			{
				return AttackIntroAnimExitDelayGround;
			}
			return AttackIntroAnimExitDelayAir;
		}
	}

	protected override float AttackAnimSpeed
	{
		get
		{
			if (IsGrounded)
			{
				return AttackHoldAnimSpeedGround;
			}
			return AttackHoldAnimSpeedAir;
		}
	}

	protected override float AttackAnimExitDelay
	{
		get
		{
			if (PerformGroundAttack)
			{
				return AttackHoldAnimDelayGround;
			}
			return AttackHoldAnimDelayAir;
		}
	}

	protected override float ExitAnimSpeed
	{
		get
		{
			if (PerformGroundAttack)
			{
				return ExitIntroAnimSpeedGround;
			}
			return ExitIntroAnimSpeedAir;
		}
	}

	protected override float ExitAnimExitDelay
	{
		get
		{
			if (PerformGroundAttack)
			{
				return ExitHoldAnimDelayGround;
			}
			return ExitHoldAnimDelayAir;
		}
	}

	public override bool HasAttackFlipCheck
	{
		get
		{
			if (PerformGroundAttack)
			{
				return base.HasAttackFlipCheck;
			}
			return m_hasAirborneAttackFlipCheck;
		}
	}

	public override string ProjectileName
	{
		get
		{
			if (PerformGroundAttack)
			{
				return base.ProjectileName;
			}
			if (PerformLandingAttack)
			{
				return m_landingProjectileName;
			}
			return m_airborneProjectileName;
		}
	}

	public override AbilityData AbilityData
	{
		get
		{
			if (!base.AbilityActive)
			{
				if (IsGrounded)
				{
					return m_landingAbilityData;
				}
				return m_airborneAbilityData;
			}
			if (PerformGroundAttack)
			{
				return base.AbilityData;
			}
			if (PerformLandingAttack)
			{
				return m_landingAbilityData;
			}
			return m_airborneAbilityData;
		}
	}

	public override string AbilityTellIntroName
	{
		get
		{
			if (PerformGroundAttack)
			{
				return base.AbilityTellIntroName;
			}
			return m_abilityAirTellIntroName;
		}
	}

	protected bool PerformGroundAttack { get; private set; }

	protected bool PerformLandingAttack
	{
		get
		{
			if (!PerformGroundAttack && IsGrounded && m_isAirAttacking)
			{
				return base.CurrentAbilityAnimState == AbilityAnimState.Exit;
			}
			return false;
		}
	}

	protected bool IsGrounded
	{
		get
		{
			if (m_abilityController != null && (!m_abilityController.PlayerController.IsGrounded || (m_abilityController.PlayerController.IsGrounded && m_abilityController.PlayerController.CharacterJump.JumpHappenedThisFrame)))
			{
				return false;
			}
			return true;
		}
	}

	public override float MovementMod
	{
		get
		{
			if (!PerformGroundAttack && IsGrounded)
			{
				return AttackMovementModAfterSpinLand;
			}
			return base.MovementMod;
		}
	}

	public override bool JumpInterruptable
	{
		get
		{
			if ((!PerformGroundAttack && IsGrounded) || m_abilityController.PlayerController.CharacterJump.IsJumpWithinLeeway())
			{
				return false;
			}
			return base.JumpInterruptable;
		}
	}

	protected override bool IsInCritWindow
	{
		get
		{
			if (!PerformGroundAttack)
			{
				return false;
			}
			return base.IsInCritWindow;
		}
	}

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[3] { m_projectileName, m_airborneProjectileName, m_landingProjectileName };
	}

	protected override void Awake()
	{
		base.Awake();
		m_waitUntilLandedYield = new WaitUntil(() => IsGrounded);
	}

	public override void PreCastAbility()
	{
		if (IsGrounded)
		{
			m_isAirAttacking = false;
		}
		else
		{
			m_isAirAttacking = true;
		}
		PerformGroundAttack = IsGrounded && !m_isAirAttacking;
		base.PreCastAbility();
	}

	protected override void OnEnterExitLogic()
	{
		if (!PerformGroundAttack && m_firedProjectile != null)
		{
			m_firedProjectile.FlagForDestruction();
		}
		if (PerformLandingAttack)
		{
			PerformAttackFlipCheck();
			FireProjectile();
		}
		base.OnEnterExitLogic();
	}

	protected override void FireProjectile()
	{
		if ((!PerformGroundAttack && !IsGrounded) || PerformGroundAttack || PerformLandingAttack)
		{
			base.FireProjectile();
		}
	}

	protected override IEnumerator ChangeAnim(float duration)
	{
		if (base.CurrentAbilityAnimState != AbilityAnimState.Exit && !PerformGroundAttack && IsGrounded)
		{
			if (base.CurrentAbilityAnimState == AbilityAnimState.Attack_Intro)
			{
				PerformGroundAttack = true;
				m_isAirAttacking = false;
				m_animator.Play("AxeGrounded_Attack_Intro");
			}
			yield return base.ChangeAnim(0f);
		}
		else if (base.CurrentAbilityAnimState == AbilityAnimState.Attack && !IsGrounded)
		{
			yield return m_waitUntilLandedYield;
			yield return base.ChangeAnim(0f);
		}
		else if (base.CurrentAbilityAnimState == AbilityAnimState.Tell && IsGrounded)
		{
			if (duration <= 0f)
			{
				yield return null;
			}
			while (duration > 0f)
			{
				duration -= Time.deltaTime;
				yield return null;
			}
			while (Rewired_RL.Player.GetButton(m_abilityController.GetAbilityInputString(base.CastAbilityType)))
			{
				yield return null;
			}
			if (IsInCritWindow)
			{
				if ((bool)m_critEffectGO)
				{
					BaseEffect baseEffect = EffectManager.PlayEffect(m_abilityController.gameObject, null, "CriticalSuccess_Effect", Vector3.zero, 1f, EffectStopType.Gracefully);
					baseEffect.transform.SetParent(m_critEffectGO.transform, worldPositionStays: false);
					baseEffect.transform.localPosition = new Vector3(m_critEffectOffset.x, 0f, 0f);
				}
				base.ForceTriggerCrit = true;
			}
			m_abilityCastStartTime = 0f;
			m_animator.SetTrigger("Change_Ability_Anim");
			PerformTurnAnimCheck();
		}
		else
		{
			yield return base.ChangeAnim(duration);
		}
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		m_isAirAttacking = false;
		base.StopAbility(abilityInterrupted);
	}
}
