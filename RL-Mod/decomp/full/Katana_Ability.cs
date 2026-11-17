using FMODUnity;
using MoreMountains.CorgiEngine;
using UnityEngine;

public class Katana_Ability : Sword_Ability
{
	private enum KatanaAttackDirection
	{
		Up = 1,
		None,
		Down
	}

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

	[Header("Audio Event Emitters")]
	[SerializeField]
	private StudioEventEmitter m_prepEventEmitter;

	[SerializeField]
	private StudioEventEmitter m_retractBladeEventEmitter;

	private bool m_isAirAttacking;

	private bool m_isGroundAttacking;

	private float TellIntroAnim = 1f;

	private float TellHoldAnim = 2f;

	private float AttackIntroAnim = 1f;

	private float AttackHoldAnim = 1f;

	private float ExitIntroAnim = 1f;

	private float Attack_Ground_HoldDelay = 0.1f;

	private float Attack_Air_HoldDelay = 0.1f;

	private KatanaAttackDirection m_attackDirection;

	protected float TellIntroAnimSpeedGround => 1.4f;

	protected float TellIntroAnimSpeedAir => 1.4f;

	protected float TellIntroAnimExitDelayGround => 0f;

	protected float TellIntroAnimExitDelayAir => 0f;

	protected float TellHoldAnimSpeedGround => 2.35f;

	protected float TellHoldAnimSpeedAir => 2.35f;

	protected float TellHoldAnimDelayGround => 0f;

	protected float TellHoldAnimDelayAir => 0f;

	protected float AttackIntroAnimSpeedGround => 1.25f;

	protected float AttackIntroAnimSpeedAir => 1.25f;

	protected float AttackIntroAnimExitDelayGround => 0f;

	protected float AttackIntroAnimExitDelayAir => 0f;

	protected float AttackHoldAnimSpeedGround => 1f;

	protected float AttackHoldAnimSpeedAir => 1f;

	protected float AttackHoldAnimDelayGround => 0.15f;

	protected float AttackHoldAnimDelayAir => 0.15f;

	protected float ExitIntroAnimSpeedGround => 2.2f;

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
			return m_airborneProjectileName;
		}
	}

	public override AbilityData AbilityData
	{
		get
		{
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
			if (PerformGroundAttack)
			{
				return base.AbilityTellIntroName;
			}
			return m_abilityAirTellIntroName;
		}
	}

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
		m_projectileNameArray = new string[2] { m_projectileName, m_airborneProjectileName };
	}

	public override void PreCastAbility()
	{
		m_isGroundAttacking = false;
		m_isAirAttacking = false;
		if (IsGrounded)
		{
			m_isGroundAttacking = true;
		}
		else
		{
			m_isAirAttacking = true;
		}
		if (Rewired_RL.Player.GetButton("MoveVertical"))
		{
			m_attackDirection = KatanaAttackDirection.Up;
		}
		else if (Rewired_RL.Player.GetNegativeButton("MoveVertical"))
		{
			m_attackDirection = KatanaAttackDirection.Down;
		}
		else
		{
			m_attackDirection = KatanaAttackDirection.None;
		}
		m_animator.SetFloat("Attack_Direction", (float)m_attackDirection);
		m_prepEventEmitter.Play();
		base.PreCastAbility();
	}

	protected override void FireProjectile()
	{
		base.FireProjectile();
		if (m_isGroundAttacking || !m_isGroundAttacking)
		{
			if (m_attackDirection == KatanaAttackDirection.Up)
			{
				Vector3 localEulerAngles = m_firedProjectile.transform.localEulerAngles;
				localEulerAngles.z = 35f;
				m_firedProjectile.transform.localEulerAngles = localEulerAngles;
			}
			else if (m_attackDirection == KatanaAttackDirection.Down)
			{
				Vector3 localEulerAngles2 = m_firedProjectile.transform.localEulerAngles;
				localEulerAngles2.z = -35f;
				m_firedProjectile.transform.localEulerAngles = localEulerAngles2;
			}
		}
	}

	protected override void OnEnterExitLogic()
	{
		m_retractBladeEventEmitter.Play();
		base.OnEnterExitLogic();
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		m_isAirAttacking = false;
		m_isGroundAttacking = false;
		m_abilityController.PlayerController.DisableFriction = false;
		if ((bool)m_firedProjectile)
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
}
