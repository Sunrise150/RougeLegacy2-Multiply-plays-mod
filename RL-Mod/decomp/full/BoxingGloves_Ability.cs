using System;
using System.Collections;
using MoreMountains.CorgiEngine;
using UnityEngine;

public class BoxingGloves_Ability : BaseAbility_RL, IAttack, IAbility
{
	[SerializeField]
	protected string m_projectileName2;

	[SerializeField]
	protected string m_projectileName3;

	[Header("Downward Attack Values")]
	[SerializeField]
	protected string m_downAttackTellIntroName;

	[SerializeField]
	protected AbilityData m_downAttackAbilityData;

	[SerializeField]
	protected string m_downAttackProjectileName;

	[SerializeField]
	protected Vector2 m_downAttackProjectileOffset;

	[Header("Upward Attack Values")]
	[SerializeField]
	protected string m_upAttackTellIntroName;

	[SerializeField]
	protected AbilityData m_upAttackAbilityData;

	[SerializeField]
	protected string m_upAttackProjectileName;

	[SerializeField]
	protected Vector2 m_upAttackProjectileOffset;

	private bool m_isUpAttacking;

	private bool m_isDownAttacking;

	private int m_comboNumber = 1;

	private float m_canAttackAgainCounter;

	private Coroutine m_pushForwardCoroutine;

	private int m_punchIndex;

	private Coroutine m_reenableGravityCoroutine;

	private bool m_hasAttacked;

	private Action<Projectile_RL, GameObject> m_continueComboEvent;

	private const float CanAttackAgainDelay = 0.085f;

	private const float GRAVITY_DISABLE_DURATION = 0.25f;

	protected virtual int NumComboAttacks => 999999;

	protected float TellIntroAnimSpeedNormalAttack => 2f;

	protected float TellIntroAnimSpeedDownAttack => 0.9f;

	protected float TellIntroAnimSpeedUpAttack => 0.9f;

	protected float TellIntroAnimExitDelayNormalAttack => 0f;

	protected float TellIntroAnimExitDelayDownAttack => 0f;

	protected float TellIntroAnimExitDelayUpAttack => 0f;

	protected float TellAnimSpeedNormalAttack => 2f;

	protected float TellAnimSpeedDownAttack => TellAnimSpeedNormalAttack;

	protected float TellAnimSpeedUpAttack => TellAnimSpeedNormalAttack;

	protected float TellAnimExitDelayNormalAttack => 0f;

	protected float TellAnimExitDelayDownAttack => 0f;

	protected float TellAnimExitDelayUpAttack => 0f;

	protected float AttackIntroAnimSpeedNormalAttack => 2f;

	protected float AttackIntroAnimSpeedDownAttack => AttackIntroAnimSpeedNormalAttack;

	protected float AttackIntroAnimSpeedUpAttack => AttackIntroAnimSpeedNormalAttack;

	protected float AttackIntroAnimExitDelayNormalAttack => 0f;

	protected float AttackIntroAnimExitDelayDownAttack => 0f;

	protected float AttackIntroAnimExitDelayUpAttack => 0f;

	protected float AttackAnimSpeedNormalAttack => 2f;

	protected float AttackAnimSpeedDownAttack => AttackAnimSpeedNormalAttack;

	protected float AttackAnimSpeedUpAttack => 1.1f;

	protected float AttackAnimExitDelayNormalAttack => 0f;

	protected float AttackAnimExitDelayDownAttack => 0f;

	protected float AttackAnimExitDelayUpAttack => 0f;

	protected float ExitAnimSpeedNormalAttack => 1f;

	protected float ExitAnimSpeedDownAttack => ExitAnimSpeedNormalAttack;

	protected float ExitAnimSpeedUpAttack => 1.2f;

	protected float ExitAnimExitDelayNormalAttack => 0.05f;

	protected float ExitAnimExitDelayDownAttack => 0f;

	protected float ExitAnimExitDelayUpAttack => 0f;

	public override Vector2 ProjectileOffset
	{
		get
		{
			if (PerformUpAttack)
			{
				return m_upAttackProjectileOffset;
			}
			if (PerformDownAttack)
			{
				return m_downAttackProjectileOffset;
			}
			return base.ProjectileOffset;
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
			if (PerformDownAttack)
			{
				return TellIntroAnimSpeedDownAttack;
			}
			return TellIntroAnimSpeedNormalAttack;
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
			if (PerformDownAttack)
			{
				return TellIntroAnimExitDelayDownAttack;
			}
			return TellIntroAnimExitDelayNormalAttack;
		}
	}

	protected override float TellAnimSpeed
	{
		get
		{
			if (PerformUpAttack)
			{
				return TellAnimSpeedUpAttack;
			}
			if (PerformDownAttack)
			{
				return TellAnimSpeedDownAttack;
			}
			return TellAnimSpeedNormalAttack;
		}
	}

	protected override float TellAnimExitDelay
	{
		get
		{
			if (PerformUpAttack)
			{
				return TellAnimExitDelayUpAttack;
			}
			if (PerformDownAttack)
			{
				return TellAnimExitDelayDownAttack;
			}
			return TellAnimExitDelayNormalAttack;
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
			if (PerformDownAttack)
			{
				return AttackIntroAnimSpeedDownAttack;
			}
			return AttackIntroAnimSpeedNormalAttack;
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
			if (PerformDownAttack)
			{
				return AttackIntroAnimExitDelayDownAttack;
			}
			return AttackIntroAnimExitDelayNormalAttack;
		}
	}

	protected override float AttackAnimSpeed
	{
		get
		{
			if (PerformUpAttack)
			{
				return AttackAnimSpeedUpAttack;
			}
			if (IsGrounded)
			{
				return AttackAnimSpeedDownAttack;
			}
			return AttackAnimSpeedNormalAttack;
		}
	}

	protected override float AttackAnimExitDelay
	{
		get
		{
			if (PerformUpAttack)
			{
				return AttackAnimExitDelayUpAttack;
			}
			if (PerformDownAttack)
			{
				return AttackAnimExitDelayDownAttack;
			}
			return AttackAnimExitDelayNormalAttack;
		}
	}

	protected override float ExitAnimSpeed
	{
		get
		{
			if (PerformUpAttack)
			{
				return ExitAnimSpeedUpAttack;
			}
			if (PerformDownAttack)
			{
				return ExitAnimSpeedDownAttack;
			}
			return ExitAnimSpeedNormalAttack;
		}
	}

	protected override float ExitAnimExitDelay
	{
		get
		{
			if (PerformUpAttack)
			{
				return ExitAnimExitDelayUpAttack;
			}
			if (PerformDownAttack)
			{
				return ExitAnimExitDelayDownAttack;
			}
			return ExitAnimExitDelayNormalAttack;
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
			if (PerformDownAttack)
			{
				return m_downAttackProjectileName;
			}
			return m_punchIndex switch
			{
				1 => base.ProjectileName, 
				2 => m_projectileName2, 
				3 => m_projectileName3, 
				_ => base.ProjectileName, 
			};
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
			if (PerformDownAttack)
			{
				return m_downAttackAbilityData;
			}
			return base.AbilityData;
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
			if (PerformDownAttack)
			{
				return m_downAttackTellIntroName;
			}
			return base.AbilityTellIntroName.Replace('1', m_punchIndex.ToString()[0]);
		}
	}

	protected bool PerformUpAttack => m_isUpAttacking;

	protected bool PerformDownAttack => m_isDownAttacking;

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

	public override float MovementMod
	{
		get
		{
			if (!m_abilityController.PlayerController.IsGrounded)
			{
				return 1f;
			}
			return base.MovementMod;
		}
	}

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[5] { m_projectileName, m_projectileName2, m_projectileName3, m_downAttackProjectileName, m_upAttackProjectileName };
	}

	protected override void Awake()
	{
		base.Awake();
		m_continueComboEvent = ContinueComboEvent;
	}

	public override void PreCastAbility()
	{
		m_isUpAttacking = false;
		m_isDownAttacking = false;
		if (Rewired_RL.Player.GetButton("MoveVertical"))
		{
			m_isUpAttacking = true;
		}
		m_punchIndex = 1;
		m_canAttackAgainCounter = 0f;
		m_comboNumber = 0;
		base.PreCastAbility();
	}

	protected override void OnEnterTellLogic()
	{
		m_hasAttacked = false;
		m_comboNumber++;
		base.OnEnterTellLogic();
	}

	protected override void OnEnterAttackLogic()
	{
		base.OnEnterAttackLogic();
		m_canAttackAgainCounter = 0f;
	}

	protected override void FireProjectile()
	{
		base.FireProjectile();
		if ((bool)m_firedProjectile)
		{
			m_firedProjectile.OnCollisionRelay.AddListener(m_continueComboEvent);
		}
	}

	private void ContinueComboEvent(Projectile_RL projectile, GameObject colliderObj)
	{
		if ((bool)projectile)
		{
			projectile.OnCollisionRelay.RemoveListener(m_continueComboEvent);
		}
		if ((bool)this && m_abilityController.PlayerController.ConditionState != CharacterStates.CharacterConditions.Stunned && !PerformDownAttack && !PerformUpAttack)
		{
			if (m_abilityController.PlayerController.CharacterDash.IsDashing)
			{
				m_abilityController.PlayerController.CharacterDash.StopDash();
			}
			m_abilityController.PlayerController.CharacterJump.ResetBrakeForce();
			m_abilityController.PlayerController.ConditionState = CharacterStates.CharacterConditions.DisableHorizontalMovement;
			m_abilityController.PlayerController.SetVelocity(0f, 0f, additive: false);
			m_abilityController.PlayerController.ControllerCorgi.GravityActive(state: false);
			if (m_reenableGravityCoroutine != null)
			{
				StopCoroutine(m_reenableGravityCoroutine);
			}
			m_reenableGravityCoroutine = StartCoroutine(ReenableGravityCoroutine());
		}
	}

	private IEnumerator ReenableGravityCoroutine()
	{
		float delay = Time.time + 0.25f;
		while (Time.time < delay)
		{
			yield return null;
		}
		if (m_abilityController.PlayerController.ConditionState != CharacterStates.CharacterConditions.Stunned)
		{
			m_abilityController.PlayerController.ConditionState = CharacterStates.CharacterConditions.Normal;
		}
		m_abilityController.PlayerController.ControllerCorgi.GravityActive(state: true);
	}

	protected void LateUpdate()
	{
		if (!base.AbilityActive || base.CurrentAbilityAnimState < AbilityAnimState.Attack || m_isUpAttacking)
		{
			return;
		}
		if (m_canAttackAgainCounter >= 0.085f)
		{
			if (m_comboNumber >= NumComboAttacks || !Rewired_RL.Player.GetButton(m_abilityController.GetAbilityInputString(base.CastAbilityType)))
			{
				return;
			}
			m_hasAttacked = true;
			CancelChangeAnimCoroutine();
			m_isUpAttacking = false;
			m_isDownAttacking = false;
			base.IsAnimationComplete = false;
			m_punchIndex++;
			if (m_punchIndex > 3)
			{
				m_punchIndex = 1;
			}
			if (Rewired_RL.Player.GetButton("MoveVertical"))
			{
				m_isUpAttacking = true;
				if (base.AbilityActive)
				{
					m_animator.Play(AbilityTellIntroName);
				}
				else
				{
					m_abilityController.StartAbility(base.CastAbilityType);
				}
			}
			else if (base.AbilityActive)
			{
				m_animator.Play(AbilityTellIntroName);
			}
			else
			{
				m_abilityController.StartAbility(base.CastAbilityType);
			}
		}
		else
		{
			m_canAttackAgainCounter += Time.deltaTime;
		}
	}

	protected override void OnExitExitLogic()
	{
		if (!m_hasAttacked)
		{
			base.OnExitExitLogic();
		}
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		if ((bool)m_firedProjectile)
		{
			m_firedProjectile.OnCollisionRelay.RemoveListener(m_continueComboEvent);
		}
		if (m_reenableGravityCoroutine != null)
		{
			StopCoroutine(m_reenableGravityCoroutine);
		}
		m_abilityController.PlayerController.ControllerCorgi.GravityActive(state: true);
		if (m_abilityController.PlayerController.ConditionState != CharacterStates.CharacterConditions.Stunned)
		{
			m_abilityController.PlayerController.ConditionState = CharacterStates.CharacterConditions.Normal;
		}
		base.StopAbility(abilityInterrupted);
	}
}
