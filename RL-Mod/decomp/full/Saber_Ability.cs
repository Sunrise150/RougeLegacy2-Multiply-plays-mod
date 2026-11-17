using System.Collections;
using MoreMountains.CorgiEngine;
using RLAudio;
using UnityEngine;

public class Saber_Ability : Sword_Ability
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

	[Header("Upward Attack Values")]
	[SerializeField]
	protected string m_upAttackTellIntroName;

	[SerializeField]
	protected AbilityData m_upAttackAbilityData;

	[SerializeField]
	protected string m_upAttackProjectileName;

	[SerializeField]
	protected Vector2 m_upAttackProjectileOffset;

	[SerializeField]
	protected bool m_hasUpAttackFlipCheck;

	private bool m_isAirAttacking;

	private bool m_isGroundAttacking;

	private bool m_isUpAttacking;

	private bool m_performedAttackCancel;

	private bool m_lastAttackWasAirAttack;

	private WaitForFixedUpdate m_waitFixedUpdateYield;

	private bool m_ignoreAnimationComplete;

	private float TellIntroAnim = 1.6f;

	private float TellHoldAnim = 2f;

	private float AttackIntroAnim = 1f;

	private float AttackHoldAnim = 1.5f;

	private float ExitIntroAnim = 1f;

	private float ExitHoldAnim = 1f;

	private float AttackHoldDelay = 0.1f;

	protected float TellIntroAnimSpeedGround => TellIntroAnim;

	protected float TellIntroAnimSpeedAir => TellIntroAnim;

	protected float TellIntroAnimSpeedUpAttack => TellIntroAnim;

	protected float TellIntroAnimExitDelayGround => 0f;

	protected float TellIntroAnimExitDelayAir => 0f;

	protected float TellIntroAnimExitDelayUpAttack => 0f;

	protected float TellHoldAnimSpeedGround => TellHoldAnim;

	protected float TellHoldAnimSpeedAir => TellHoldAnim;

	protected float TellHoldAnimSpeedUpAttack => TellHoldAnim;

	protected float TellHoldAnimDelayGround => 0f;

	protected float TellHoldAnimDelayAir => 0f;

	protected float TellHoldAnimDelayUpAttack => 0f;

	protected float AttackIntroAnimSpeedGround => AttackIntroAnim;

	protected float AttackIntroAnimSpeedAir => AttackIntroAnim;

	protected float AttackIntroAnimSpeedUpAttack => AttackIntroAnim;

	protected float AttackIntroAnimExitDelayGround => 0f;

	protected float AttackIntroAnimExitDelayAir => 0f;

	protected float AttackIntroAnimExitDelayUpAttack => 0f;

	protected float AttackHoldAnimSpeedGround => AttackHoldAnim;

	protected float AttackHoldAnimSpeedAir => AttackHoldAnim;

	protected float AttackHoldAnimSpeedUpAttack => AttackHoldAnim;

	protected float AttackHoldAnimDelayGround => AttackHoldDelay;

	protected float AttackHoldAnimDelayAir => 0f;

	protected float AttackHoldAnimDelayUpAttack => 0f;

	protected float ExitIntroAnimSpeedGround => ExitIntroAnim;

	protected float ExitIntroAnimSpeedAir => ExitIntroAnim;

	protected float ExitIntroAnimSpeedUpAttack => ExitIntroAnim;

	protected float ExitHoldAnimDelayGround => 0f;

	protected float ExitHoldAnimDelayAir => 0f;

	protected float ExitHoldAnimDelayUpAttack => 0f;

	public override Vector2 ProjectileOffset
	{
		get
		{
			if (PerformUpAttack)
			{
				return m_upAttackProjectileOffset;
			}
			if (PerformGroundAttack)
			{
				return base.ProjectileOffset;
			}
			return m_airBorneProjectileOffset;
		}
	}

	protected override float TellIntroAnimSpeed
	{
		get
		{
			if (PerformUpAttack)
			{
				return TellIntroAnimSpeedUpAttack;
			}
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
			if (PerformUpAttack)
			{
				return TellIntroAnimExitDelayUpAttack;
			}
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
			if (PerformUpAttack)
			{
				return TellHoldAnimSpeedUpAttack;
			}
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
			if (PerformUpAttack)
			{
				return TellHoldAnimDelayUpAttack;
			}
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
			if (PerformUpAttack)
			{
				return AttackIntroAnimSpeedUpAttack;
			}
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
			if (PerformUpAttack)
			{
				return AttackIntroAnimExitDelayUpAttack;
			}
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
			if (PerformUpAttack)
			{
				return AttackHoldAnimSpeedUpAttack;
			}
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
			if (PerformUpAttack)
			{
				return AttackHoldAnimDelayUpAttack;
			}
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
			if (PerformUpAttack)
			{
				return ExitIntroAnimSpeedUpAttack;
			}
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
			if (PerformUpAttack)
			{
				return ExitHoldAnimDelayUpAttack;
			}
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
			if (PerformUpAttack)
			{
				return m_hasUpAttackFlipCheck;
			}
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
			if (PerformUpAttack)
			{
				return m_upAttackProjectileName;
			}
			if (PerformGroundAttack)
			{
				return base.ProjectileName;
			}
			return m_airborneProjectileName;
		}
	}

	public override AbilityData AbilityData
	{
		get
		{
			if (PerformUpAttack)
			{
				return m_upAttackAbilityData;
			}
			if (PerformGroundAttack)
			{
				return base.AbilityData;
			}
			return m_airborneAbilityData;
		}
	}

	public override string AbilityTellIntroName
	{
		get
		{
			if (PerformUpAttack)
			{
				return m_upAttackTellIntroName;
			}
			if (PerformGroundAttack)
			{
				return base.AbilityTellIntroName;
			}
			return m_abilityAirTellIntroName;
		}
	}

	protected bool PerformUpAttack => m_isUpAttacking;

	protected bool PerformGroundAttack
	{
		get
		{
			if (!IsGrounded || m_isAirAttacking)
			{
				return m_isGroundAttacking;
			}
			return true;
		}
	}

	protected bool IsGrounded
	{
		get
		{
			if ((bool)m_abilityController && !m_abilityController.PlayerController.IsGrounded)
			{
				return false;
			}
			return true;
		}
	}

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[3] { m_projectileName, m_airborneProjectileName, m_upAttackProjectileName };
	}

	protected override void Awake()
	{
		m_waitFixedUpdateYield = new WaitForFixedUpdate();
		base.Awake();
	}

	public override void PreCastAbility()
	{
		m_isGroundAttacking = false;
		m_isAirAttacking = false;
		m_isUpAttacking = false;
		m_ignoreAnimationComplete = false;
		if (IsGrounded)
		{
			m_isGroundAttacking = true;
			m_abilityController.PlayerController.SetVelocity(0f, 0f, additive: false);
			m_abilityController.PlayerController.ConditionState = CharacterStates.CharacterConditions.DisableHorizontalMovement;
		}
		else
		{
			m_isAirAttacking = true;
		}
		m_lastAttackWasAirAttack = m_isAirAttacking;
		m_performedAttackCancel = false;
		base.PreCastAbility();
	}

	protected override IEnumerator ChangeAnim(float duration)
	{
		if (base.CurrentAbilityAnimState == AbilityAnimState.TellIntro && IsGrounded)
		{
			yield return base.ChangeAnim(0f);
		}
		else
		{
			yield return base.ChangeAnim(duration);
		}
	}

	protected IEnumerator PushForward()
	{
		float speed = 0f;
		float num = 0f / speed;
		float endingTime = Time.fixedTime + num;
		if (m_abilityController.PlayerController.IsFacingRight)
		{
			m_abilityController.PlayerController.SetVelocityX(speed, additive: false);
		}
		else
		{
			m_abilityController.PlayerController.SetVelocityX(0f - speed, additive: false);
		}
		if (IsGrounded)
		{
			m_abilityController.PlayerController.MovementState = CharacterStates.MovementStates.Idle;
		}
		while (Time.fixedTime < endingTime)
		{
			float num2 = speed * 1f;
			m_abilityController.PlayerController.DisableFriction = true;
			if (m_abilityController.PlayerController.IsFacingRight)
			{
				m_abilityController.PlayerController.SetVelocityX(num2, additive: false);
			}
			else
			{
				m_abilityController.PlayerController.SetVelocityX(0f - num2, additive: false);
			}
			PreventPlatformDrop();
			yield return m_waitFixedUpdateYield;
		}
		m_abilityController.PlayerController.SetVelocityX(0f, additive: false);
		m_abilityController.PlayerController.DisableFriction = false;
	}

	private void PreventPlatformDrop()
	{
		CorgiController_RL controllerCorgi = m_abilityController.PlayerController.ControllerCorgi;
		Vector2 origin = controllerCorgi.BoundsBottomLeftCorner;
		Vector2 origin2 = controllerCorgi.BoundsBottomRightCorner;
		float distance = Mathf.Clamp(controllerCorgi.BoundsHeight / 2f + controllerCorgi.BoundsWidth / 2f, controllerCorgi.StickyRaycastLength + 0.5f, float.MaxValue);
		if (m_abilityController.PlayerController.Velocity.x > 0f)
		{
			if (!Physics2D.Raycast(origin2, Vector2.down, distance, m_abilityController.PlayerController.ControllerCorgi.SavedPlatformMask))
			{
				m_abilityController.PlayerController.SetVelocityX(0f, additive: false);
			}
		}
		else if (m_abilityController.PlayerController.Velocity.x < 0f && !Physics2D.Raycast(origin, Vector2.down, distance, m_abilityController.PlayerController.ControllerCorgi.SavedPlatformMask))
		{
			m_abilityController.PlayerController.SetVelocityX(0f, additive: false);
		}
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		m_isAirAttacking = false;
		m_isGroundAttacking = false;
		m_isUpAttacking = false;
		m_ignoreAnimationComplete = false;
		m_abilityController.PlayerController.DisableFriction = false;
		if (m_firedProjectile != null)
		{
			m_firedProjectile.gameObject.SetActive(value: false);
			m_firedProjectile = null;
		}
		m_abilityController.PlayerController.ControllerCorgi.GravityActive(state: true);
		if (m_abilityController.PlayerController.ConditionState != CharacterStates.CharacterConditions.Stunned)
		{
			m_abilityController.PlayerController.ConditionState = CharacterStates.CharacterConditions.Normal;
		}
		base.StopAbility(abilityInterrupted);
	}

	protected override void Update()
	{
		base.Update();
		if (Rewired_RL.Player.GetButton(m_abilityController.GetAbilityInputString(base.CastAbilityType)) && m_lastAttackWasAirAttack && IsGrounded)
		{
			float axis = m_abilityController.PlayerController.CharacterCorgi.REPlayer.GetAxis("MoveHorizontal");
			if ((axis > 0f && !m_abilityController.PlayerController.IsFacingRight) || (axis < 0f && m_abilityController.PlayerController.IsFacingRight))
			{
				m_abilityController.PlayerController.CharacterCorgi.Flip();
			}
			m_abilityController.StopAbility(base.CastAbilityType, abilityInterrrupted: true);
			if (base.CurrentAbilityAnimState == AbilityAnimState.Exit)
			{
				m_ignoreAnimationComplete = true;
			}
			m_performedAttackCancel = true;
			m_abilityController.StartAbility(base.CastAbilityType);
		}
	}

	protected override void OnExitExitLogic()
	{
		if (!m_ignoreAnimationComplete)
		{
			base.OnExitExitLogic();
		}
		m_ignoreAnimationComplete = false;
	}

	protected override void PlayDashCritAudio()
	{
		AudioManager.PlayOneShotAttached(this, "event:/SFX/Weapons/sfx_weapon_crit_sabre_swing", base.gameObject);
	}
}
