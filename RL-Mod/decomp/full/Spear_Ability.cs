using System.Collections;
using MoreMountains.CorgiEngine;
using UnityEngine;

public class Spear_Ability : BaseAbility_RL, IAttack, IAbility
{
	protected enum DirectionalAttackAnimLayer
	{
		Up,
		UpRight,
		Right,
		DownRight,
		Down
	}

	[SerializeField]
	protected bool m_allowOmniAttack;

	protected Vector2 m_upAttackOffset = new Vector2(0f, 6f);

	protected Vector2 m_rightAttackOffset = new Vector2(4f, 2.25f);

	protected Vector2 m_downAttackOffset = new Vector2(0.25f, -1.75f);

	protected Vector2 m_upRightAttackOffset = new Vector2(3.7f, 4.55f);

	protected Vector2 m_downRightAttackOffset = new Vector2(3.2f, -2.55f);

	private float m_spearAttackAngle;

	private Vector2 m_spearAttackOffset;

	private WaitForFixedUpdate m_waitFixedUpdateYield;

	public const float SpearDashDistance = 0f;

	public const float SpearDashSpeed = 0f;

	public const bool SpearAirStop = false;

	protected WaitRL_Yield m_gravityYield;

	protected Coroutine m_gravityYieldCoroutine;

	protected override float TellIntroAnimSpeed => 2.5f;

	protected override float TellIntroAnimExitDelay => 0f;

	protected override float TellAnimSpeed => 1.25f;

	protected override float TellAnimExitDelay => 0f;

	protected override float AttackIntroAnimSpeed => 1f;

	protected override float AttackIntroAnimExitDelay => 0f;

	protected override float AttackAnimSpeed => 1f;

	protected override float AttackAnimExitDelay => 0.08f;

	protected override float ExitAnimSpeed => 1.25f;

	protected override float ExitAnimExitDelay => 0f;

	protected override float AttackBounceHeight => 18f;

	protected bool IsGrounded
	{
		get
		{
			if (m_abilityController != null && !m_abilityController.PlayerController.IsGrounded)
			{
				return false;
			}
			return true;
		}
	}

	protected override void Awake()
	{
		m_waitFixedUpdateYield = new WaitForFixedUpdate();
		base.Awake();
	}

	public override void PreCastAbility()
	{
		float num = Rewired_RL.Player.GetAxis("MoveHorizontal");
		float y = Rewired_RL.Player.GetAxis("MoveVertical");
		if (!m_allowOmniAttack)
		{
			y = 0f;
		}
		if (!PlayerManager.GetPlayerController().IsFacingRight && num == 0f)
		{
			num = -1f;
		}
		if (num == 1f)
		{
			num = 0.99f;
		}
		if (num == -1f)
		{
			num = -0.99f;
		}
		m_spearAttackAngle = CDGHelper.GetLockedAngle(CDGHelper.VectorToAngle(new Vector2(num, y)), 4);
		PlayerInputDirection inputDirection = PlayerInputDirection_RL.GetInputDirection(m_spearAttackAngle);
		m_spearAttackOffset = Vector2.zero;
		DirectionalAttackAnimLayer attackLayer = GetAttackLayer(inputDirection, out m_spearAttackOffset);
		m_abilityController.Animator.SetFloat("Attack_Direction", (float)attackLayer);
		base.PreCastAbility();
	}

	protected override IEnumerator ChangeAnim(float duration)
	{
		yield return base.ChangeAnim(duration);
	}

	protected override void FireProjectile()
	{
		Vector2 vector = m_abilityController.PlayerController.Midpoint - m_abilityController.PlayerController.transform.position;
		m_spearAttackOffset -= vector;
		m_projectileOffset = m_spearAttackOffset;
		base.FireProjectile();
		if (m_spearAttackAngle != 180f)
		{
			Vector3 localEulerAngles = m_firedProjectile.Pivot.transform.localEulerAngles;
			localEulerAngles.z = m_spearAttackAngle;
			m_firedProjectile.Pivot.transform.localEulerAngles = localEulerAngles;
		}
		if (m_spearAttackAngle == 270f)
		{
			m_firedProjectile.OnCollisionRelay.AddListener(m_bounce);
		}
	}

	protected virtual IEnumerator GravityYield(float delay)
	{
		if (m_gravityYield == null)
		{
			m_gravityYield = new WaitRL_Yield(delay);
		}
		else
		{
			m_gravityYield.CreateNew(delay);
		}
		yield return m_gravityYield;
		m_abilityController.PlayerController.ControllerCorgi.GravityActive(state: true);
		m_abilityController.PlayerController.ConditionState = CharacterStates.CharacterConditions.Normal;
	}

	protected virtual IEnumerator PushForward()
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

	protected DirectionalAttackAnimLayer GetAttackLayer(PlayerInputDirection direction, out Vector2 attackOffset)
	{
		switch (direction)
		{
		case PlayerInputDirection.Down:
			attackOffset = m_downAttackOffset;
			return DirectionalAttackAnimLayer.Down;
		default:
			attackOffset = m_rightAttackOffset;
			return DirectionalAttackAnimLayer.Right;
		case PlayerInputDirection.UpLeft:
		case PlayerInputDirection.Up:
		case PlayerInputDirection.UpRight:
			attackOffset = m_upAttackOffset;
			return DirectionalAttackAnimLayer.Up;
		}
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		if ((bool)m_firedProjectile)
		{
			m_firedProjectile.OnCollisionRelay.RemoveListener(m_bounce);
			m_firedProjectile.FlagForDestruction();
		}
		StopAllCoroutines();
		m_abilityController.PlayerController.DisableFriction = false;
		base.StopAbility(abilityInterrupted);
	}
}
