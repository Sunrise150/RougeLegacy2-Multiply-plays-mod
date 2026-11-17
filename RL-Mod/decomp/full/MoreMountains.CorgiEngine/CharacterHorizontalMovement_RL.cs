using MoreMountains.Tools;
using Rewired;
using UnityEngine;

namespace MoreMountains.CorgiEngine;

[AddComponentMenu("Corgi Engine/Character/Abilities/Character Horizontal Movement RL")]
public class CharacterHorizontalMovement_RL : CharacterHorizontalMovement
{
	[SerializeField]
	private bool m_analogMovement = true;

	[SerializeField]
	private float m_baseAnimWalkSpeed = 10f;

	[SerializeField]
	private float m_baseScaleAnimWalkSpeed = 1f;

	private float m_disableMovementTimer;

	public bool MovementDisabled => Time.time < m_disableMovementTimer;

	public float TopSpeed => base.MovementSpeed * _controller.Parameters.SpeedFactor * base.MovementSpeedMultiplier;

	public void DisableMovement(float duration)
	{
		m_disableMovementTimer = Time.time + duration;
	}

	public void ResetHorizontalMovement()
	{
		_horizontalInput = 0f;
		_horizontalMovement = 0f;
		_normalizedHorizontalSpeed = 0f;
	}

	protected override void HandleInput()
	{
		if (ReInput.isReady && m_playerController.ConditionState != CharacterStates.CharacterConditions.DisableHorizontalMovement && m_playerController.ConditionState != CharacterStates.CharacterConditions.Dead && m_playerController.ConditionState != CharacterStates.CharacterConditions.ControlledMovement)
		{
			base.HandleInput();
		}
	}

	protected override void HandleHorizontalMovement()
	{
		CheckJustGotGrounded();
		if (m_playerController.ConditionState == CharacterStates.CharacterConditions.DisableHorizontalMovement && m_playerController.IsGrounded)
		{
			if (!m_playerController.DisableFriction)
			{
				m_playerController.SetVelocityX(0f, additive: false);
			}
			_horizontalMovement = 0f;
		}
		if (!AbilityPermitted || (_condition.CurrentState != 0 && _condition.CurrentState != CharacterStates.CharacterConditions.ControlledMovement) || _condition.CurrentState == CharacterStates.CharacterConditions.Stunned || m_playerController.ConditionState == CharacterStates.CharacterConditions.DisableHorizontalMovement || m_playerController.MovementState == CharacterStates.MovementStates.Dashing || (m_playerController.MovementState == CharacterStates.MovementStates.DownStriking && !m_playerController.CharacterDownStrike.SpinKickInstead) || m_playerController.ConditionState == CharacterStates.CharacterConditions.Dead || Time.time < m_disableMovementTimer)
		{
			return;
		}
		if (ReInput.isReady && Rewired_RL.Player.GetButton("FreeLook"))
		{
			base.MovementForbidden = true;
		}
		else
		{
			base.MovementForbidden = false;
		}
		if (base.MovementForbidden)
		{
			_horizontalMovement = 0f;
		}
		if (_horizontalMovement > 0.1f)
		{
			_normalizedHorizontalSpeed = _horizontalMovement;
			if (!_character.IsFacingRight && !LockFlip())
			{
				_character.Flip();
				if (_controller.State.IsCollidingBelow && _controller.Velocity.y > 0f && !_controller.State.JustStartedJump)
				{
					_controller.SetVerticalForce(0f);
				}
			}
		}
		else if (_horizontalMovement < -0.1f)
		{
			_normalizedHorizontalSpeed = _horizontalMovement;
			if (_character.IsFacingRight && !LockFlip())
			{
				_character.Flip();
				if (_controller.State.IsCollidingBelow && _controller.Velocity.y > 0f && !_controller.State.JustStartedJump)
				{
					_controller.SetVerticalForce(0f);
				}
			}
		}
		else
		{
			_normalizedHorizontalSpeed = 0f;
		}
		if (_controller.State.IsGrounded && _normalizedHorizontalSpeed != 0f && _movement.CurrentState == CharacterStates.MovementStates.Idle)
		{
			_movement.ChangeState(CharacterStates.MovementStates.Walking);
		}
		if (_movement.CurrentState == CharacterStates.MovementStates.Walking && _normalizedHorizontalSpeed == 0f)
		{
			_movement.ChangeState(CharacterStates.MovementStates.Idle);
		}
		if (!_controller.State.IsGrounded && (_movement.CurrentState == CharacterStates.MovementStates.Walking || _movement.CurrentState == CharacterStates.MovementStates.Idle))
		{
			_movement.ChangeState(CharacterStates.MovementStates.Falling);
		}
		if (!m_analogMovement)
		{
			if (_normalizedHorizontalSpeed > 0f)
			{
				_normalizedHorizontalSpeed = 1f;
			}
			else if (_normalizedHorizontalSpeed < 0f)
			{
				_normalizedHorizontalSpeed = -1f;
			}
		}
		float num = (_controller.State.IsGrounded ? _controller.Parameters.SpeedAccelerationOnGround : _controller.Parameters.SpeedAccelerationInAir);
		float num2 = _normalizedHorizontalSpeed * base.MovementSpeed * _controller.Parameters.SpeedFactor * base.MovementSpeedMultiplier;
		float num3 = 1f;
		float num4 = 1f;
		float num5 = 1f;
		bool flag = false;
		if ((bool)m_playerController && m_playerController.IsInitialized)
		{
			flag = m_playerController.DisableFriction;
			if (m_playerController.IsGrounded)
			{
				if (m_playerController.CastAbility.AbilityInProgress(CastAbilityType.Weapon))
				{
					num5 = m_playerController.CastAbility.GetAbility(CastAbilityType.Weapon).MovementMod;
				}
				if (m_playerController.CastAbility.AbilityInProgress(CastAbilityType.Spell))
				{
					num4 = m_playerController.CastAbility.GetAbility(CastAbilityType.Spell).MovementMod;
				}
				if (m_playerController.CastAbility.AbilityInProgress(CastAbilityType.Talent))
				{
					num3 = m_playerController.CastAbility.GetAbility(CastAbilityType.Talent).MovementMod;
				}
				num2 *= num3 * num4 * num5;
			}
			else
			{
				if (m_playerController.CastAbility.AbilityInProgress(CastAbilityType.Weapon))
				{
					num5 = m_playerController.CastAbility.GetAbility(CastAbilityType.Weapon).AirMovementMod;
				}
				if (m_playerController.CastAbility.AbilityInProgress(CastAbilityType.Spell))
				{
					num4 = m_playerController.CastAbility.GetAbility(CastAbilityType.Spell).AirMovementMod;
				}
				if (m_playerController.CastAbility.AbilityInProgress(CastAbilityType.Talent))
				{
					num3 = m_playerController.CastAbility.GetAbility(CastAbilityType.Talent).AirMovementMod;
				}
				num2 *= num3 * num4 * num5;
			}
		}
		if (flag || (!_controller.State.IsGrounded && m_playerController.CharacterJump != null && m_playerController.CharacterJump.IsJumpDash))
		{
			_horizontalMovementForce = Mathf.Lerp(_controller.Velocity.x, num2 + _controller.Velocity.x, Time.deltaTime);
		}
		else
		{
			_horizontalMovementForce = Mathf.Lerp(_controller.Velocity.x, num2, Time.deltaTime * num);
		}
		_horizontalMovementForce = HandleFriction(_horizontalMovementForce);
		_controller.SetHorizontalForce(_horizontalMovementForce);
	}

	protected override void CheckJustGotGrounded()
	{
		if (_controller.State.JustGotGrounded && !_controller.State.JustStartedJump)
		{
			if (!_controller.State.ColliderResized && _movement.CurrentState != CharacterStates.MovementStates.Dashing)
			{
				_movement.ChangeState(CharacterStates.MovementStates.Idle);
			}
			_controller.SlowFall(0f);
		}
	}

	protected virtual bool LockFlip()
	{
		if ((bool)m_playerController && m_playerController.IsInitialized)
		{
			if (m_playerController.CastAbility.AbilityInProgress(CastAbilityType.Weapon) && m_playerController.CastAbility.GetAbility(CastAbilityType.Weapon).LockDirectionWhenCasting)
			{
				return true;
			}
			if (m_playerController.CastAbility.AbilityInProgress(CastAbilityType.Spell) && m_playerController.CastAbility.GetAbility(CastAbilityType.Spell).LockDirectionWhenCasting)
			{
				return true;
			}
			if (m_playerController.CastAbility.AbilityInProgress(CastAbilityType.Talent) && m_playerController.CastAbility.GetAbility(CastAbilityType.Talent).LockDirectionWhenCasting)
			{
				return true;
			}
		}
		return false;
	}

	public override void SetHorizontalMove(float value)
	{
		base.SetHorizontalMove(value);
		_normalizedHorizontalSpeed = value;
	}

	protected override void InitializeAnimatorParameters()
	{
		RegisterAnimatorParameter("Speed", AnimatorControllerParameterType.Float);
		RegisterAnimatorParameter("Walking", AnimatorControllerParameterType.Bool);
		RegisterAnimatorParameter("Walk_Anim_Speed", AnimatorControllerParameterType.Float);
	}

	public override void UpdateAnimator()
	{
		float num = 1f;
		float num2 = 1f;
		float num3 = 1f;
		float num4 = _normalizedHorizontalSpeed;
		float num5 = 1f;
		if ((bool)m_playerController && m_playerController.IsInitialized)
		{
			if (m_playerController.CastAbility.AbilityInProgress(CastAbilityType.Weapon))
			{
				num3 = m_playerController.CastAbility.GetAbility(CastAbilityType.Weapon).MovementMod;
			}
			if (m_playerController.CastAbility.AbilityInProgress(CastAbilityType.Spell))
			{
				num2 = m_playerController.CastAbility.GetAbility(CastAbilityType.Spell).MovementMod;
			}
			if (m_playerController.CastAbility.AbilityInProgress(CastAbilityType.Talent))
			{
				num = m_playerController.CastAbility.GetAbility(CastAbilityType.Talent).MovementMod;
			}
			num4 *= num * num2 * num3;
			if (!m_playerController.IsFacingRight)
			{
				num5 = -1f;
			}
		}
		if (!m_playerController.CharacterFlight.IsFlying)
		{
			MMAnimator.UpdateAnimatorFloat(_animator, "Speed", num4 * num5, _character._animatorParameters);
			MMAnimator.UpdateAnimatorBool(_animator, "Walking", _movement.CurrentState == CharacterStates.MovementStates.Walking && m_playerController.ConditionState != CharacterStates.CharacterConditions.DisableHorizontalMovement && num4 != 0f, _character._animatorParameters);
			float num6 = base.MovementSpeed / m_baseAnimWalkSpeed;
			num6 *= m_baseScaleAnimWalkSpeed / m_playerController.transform.localScale.x;
			MMAnimator.UpdateAnimatorFloat(_animator, "Walk_Anim_Speed", num6, _character._animatorParameters);
		}
	}
}
