using System.Collections;
using MoreMountains.CorgiEngine;
using UnityEngine;

public class Tonfa_Ability : BaseAbility_RL, IAttack, IAbility
{
	private int m_comboNumber = 1;

	private int m_animComboNumber = 1;

	private bool m_attackStartLeft;

	private string m_abilityTellIntroRight;

	private float m_canAttackAgainCounter;

	private Coroutine m_pushForwardCoroutine;

	private bool m_hasAttacked;

	private const float CanAttackAgainDelay = 0.035f;

	private const float TonfaDashDistance = 1.5f;

	private const float TonfaDashDuration = 0.15f;

	public bool InCombo { get; set; }

	protected virtual int NumComboAttacks => 99;

	protected override float TellIntroAnimSpeed => 1.5f;

	protected override float TellIntroAnimExitDelay => 0f;

	protected override float TellAnimSpeed => 1f;

	protected override float TellAnimExitDelay => 0f;

	protected override float AttackIntroAnimSpeed => 1f;

	protected override float AttackIntroAnimExitDelay => 0f;

	protected override float AttackAnimSpeed => 1.25f;

	protected override float AttackAnimExitDelay => 0.145f;

	protected override float ExitAnimSpeed => 1f;

	protected override float ExitAnimExitDelay => 0f;

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

	public string AbilityComboName => m_abilityTellIntroName.Replace("1Left", m_animComboNumber.ToString());

	public override string AbilityTellIntroName
	{
		get
		{
			if (m_attackStartLeft)
			{
				return base.AbilityTellIntroName;
			}
			return m_abilityTellIntroRight;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (m_abilityTellIntroName != null)
		{
			m_abilityTellIntroRight = m_abilityTellIntroName.Replace("Left", "Right");
		}
	}

	public override void PreCastAbility()
	{
		m_attackStartLeft = !m_attackStartLeft;
		m_comboNumber = 0;
		m_animComboNumber = 1;
		InCombo = false;
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
		if (m_firedProjectile != null)
		{
			m_firedProjectile.OnCollisionRelay.AddListener(ContinueComboEvent);
		}
		if (m_comboNumber <= 1 && m_abilityController.PlayerController.IsGrounded)
		{
			StopPushForwardCoroutine();
			m_pushForwardCoroutine = StartCoroutine(PushForwardCoroutine());
		}
	}

	private IEnumerator PushForwardCoroutine()
	{
		float num = 0.15f;
		float num2 = 1.5f / num;
		float endingTime = Time.time + num;
		if (m_abilityController.PlayerController.CharacterCorgi.REPlayer.GetAxis("MoveHorizontal") > 0f && !m_abilityController.PlayerController.IsFacingRight)
		{
			m_abilityController.PlayerController.CharacterCorgi.Flip();
		}
		else if (m_abilityController.PlayerController.CharacterCorgi.REPlayer.GetAxis("MoveHorizontal") < 0f && m_abilityController.PlayerController.IsFacingRight)
		{
			m_abilityController.PlayerController.CharacterCorgi.Flip();
		}
		m_abilityController.PlayerController.ConditionState = CharacterStates.CharacterConditions.DisableHorizontalMovement;
		if (m_abilityController.PlayerController.IsFacingRight)
		{
			m_abilityController.PlayerController.SetVelocityX(num2, additive: false);
		}
		else
		{
			m_abilityController.PlayerController.SetVelocityX(0f - num2, additive: false);
		}
		if (m_abilityController.PlayerController.IsGrounded)
		{
			m_abilityController.PlayerController.MovementState = CharacterStates.MovementStates.Idle;
		}
		while (Time.time < endingTime)
		{
			m_abilityController.PlayerController.DisableFriction = true;
			PreventPlatformDrop();
			yield return null;
		}
		m_abilityController.PlayerController.DisableFriction = false;
	}

	private void StopPushForwardCoroutine()
	{
		if (m_pushForwardCoroutine != null)
		{
			StopCoroutine(m_pushForwardCoroutine);
		}
		m_abilityController.PlayerController.DisableFriction = false;
	}

	private void ContinueComboEvent(Projectile_RL projectile, GameObject colliderObj)
	{
		if (projectile != null)
		{
			projectile.OnCollisionRelay.RemoveListener(ContinueComboEvent);
		}
		if (m_abilityController.PlayerController.ConditionState != CharacterStates.CharacterConditions.Stunned)
		{
			InCombo = true;
			StopPushForwardCoroutine();
			if (m_abilityController.PlayerController.ConditionState != CharacterStates.CharacterConditions.DisableHorizontalMovement)
			{
				m_abilityController.PlayerController.CharacterJump.ResetBrakeForce();
				m_abilityController.PlayerController.ConditionState = CharacterStates.CharacterConditions.DisableHorizontalMovement;
				m_abilityController.PlayerController.SetVelocity(0f, 0f, additive: false);
				m_abilityController.PlayerController.ControllerCorgi.GravityActive(state: false);
			}
		}
	}

	protected void LateUpdate()
	{
		if (!base.AbilityActive)
		{
			return;
		}
		if (m_canAttackAgainCounter >= 0.035f)
		{
			if (m_comboNumber >= NumComboAttacks || base.CurrentAbilityAnimState < AbilityAnimState.Attack || !Rewired_RL.Player.GetButtonDown(m_abilityController.GetAbilityInputString(base.CastAbilityType)))
			{
				return;
			}
			if (InCombo)
			{
				m_animComboNumber++;
				if (m_animComboNumber > 5)
				{
					m_animComboNumber = 2;
				}
				m_hasAttacked = true;
				InCombo = false;
				CancelChangeAnimCoroutine();
				m_animator.Play(AbilityComboName);
			}
			else
			{
				m_abilityController.StopAbility(base.CastAbilityType, abilityInterrrupted: true);
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

	private void RecastAbility()
	{
		m_abilityController.StartAbility(base.CastAbilityType);
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		if (m_firedProjectile != null)
		{
			m_firedProjectile.OnCollisionRelay.RemoveListener(ContinueComboEvent);
		}
		m_animator.ResetTrigger("ContinueCombo");
		StopPushForwardCoroutine();
		m_abilityController.PlayerController.ControllerCorgi.GravityActive(state: true);
		if (m_abilityController.PlayerController.ConditionState != CharacterStates.CharacterConditions.Stunned)
		{
			m_abilityController.PlayerController.ConditionState = CharacterStates.CharacterConditions.Normal;
		}
		base.StopAbility(abilityInterrupted);
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
}
