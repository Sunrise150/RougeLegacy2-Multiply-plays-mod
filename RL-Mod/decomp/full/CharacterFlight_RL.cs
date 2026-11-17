using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using Rewired;
using UnityEngine;

public class CharacterFlight_RL : CharacterAbility
{
	private float _horizontalMovement;

	private float _verticalMovement;

	private float m_flightTime;

	public float MovementSpeedMultiplier { get; set; }

	public bool MovementForbidden { get; set; }

	public bool IsAssistFlying { get; private set; }

	public bool IsFlying
	{
		get
		{
			if (_abilityInitialized)
			{
				return _movement.CurrentState == CharacterStates.MovementStates.Flying;
			}
			return false;
		}
	}

	public bool FlightTimeAuthorized => m_flightTime > Time.time;

	public override string HelpBoxText()
	{
		return "This component handles basic left/right movement, friction, and ground hit detection. Here you can define standard movement speed, walk speed, and what effects to use when the character hits the ground after a jump/fall.";
	}

	protected override void Initialization()
	{
		base.Initialization();
		MovementSpeedMultiplier = 1f;
		MovementForbidden = false;
	}

	protected override void HandleInput()
	{
		if (ReInput.isReady && m_playerController.ConditionState != CharacterStates.CharacterConditions.ControlledMovement && SaveManager.PlayerSaveData.EnableHouseRules && SaveManager.PlayerSaveData.Assist_EnableFlightToggle && _character.REPlayer.GetButtonDown("Flight"))
		{
			if (!IsFlying)
			{
				IsAssistFlying = true;
				StartFlight(2.1474836E+09f);
			}
			else
			{
				IsAssistFlying = false;
				StopFlight();
			}
		}
	}

	public override void ProcessAbility()
	{
		base.ProcessAbility();
		HandleFlightMovement();
	}

	protected bool EvaluateFlightConditions()
	{
		if (!AbilityPermitted || !FlightTimeAuthorized || !IsFlying)
		{
			return false;
		}
		return true;
	}

	public void StartFlight(float duration = 5f, float speedMod = 0f)
	{
		if ((_condition.CurrentState == CharacterStates.CharacterConditions.Normal || _condition.CurrentState == CharacterStates.CharacterConditions.ControlledMovement) && _condition.CurrentState != CharacterStates.CharacterConditions.Stunned && m_playerController.ConditionState != CharacterStates.CharacterConditions.DisableHorizontalMovement && m_playerController.MovementState != CharacterStates.MovementStates.Dashing && (m_playerController.MovementState != CharacterStates.MovementStates.DownStriking || m_playerController.CharacterDownStrike.SpinKickInstead) && m_playerController.ConditionState != CharacterStates.CharacterConditions.Dead && !IsFlying)
		{
			MovementSpeedMultiplier = 1f + speedMod;
			_movement.ChangeState(CharacterStates.MovementStates.Flying);
			m_flightTime = Time.time + duration;
			_controller.GravityActive(state: false);
			_controller.State.IsCollidingBelow = false;
			_animator.SetLayerWeight(10, 1f);
		}
	}

	public void StopFlight()
	{
		if (_abilityInitialized)
		{
			_controller.GravityActive(state: true);
			_animator.SetLayerWeight(10, 0f);
			if (IsFlying)
			{
				_movement.ChangeState(CharacterStates.MovementStates.Idle);
			}
			IsAssistFlying = false;
		}
	}

	protected void HandleFlightMovement()
	{
		if (!EvaluateFlightConditions())
		{
			if (IsFlying)
			{
				StopFlight();
			}
			return;
		}
		if (m_playerController.ControllerCorgi.IsGravityActive)
		{
			m_playerController.ControllerCorgi.GravityActive(state: false);
		}
		_horizontalMovement = _horizontalInput;
		_verticalMovement = _verticalInput;
		if (TraitManager.IsTraitActive(TraitType.UpsideDown))
		{
			_verticalMovement *= -1f;
		}
		if (MovementForbidden || Rewired_RL.Player.GetButton("FreeLook") || m_playerController.ConditionState == CharacterStates.CharacterConditions.DisableHorizontalMovement)
		{
			_horizontalMovement = 0f;
			_verticalMovement = 0f;
		}
		if (_horizontalMovement > 0.1f)
		{
			if (!_character.IsFacingRight && !m_playerController.CastAbility.AnyAbilityInProgress)
			{
				_character.Flip();
			}
		}
		else if (_horizontalMovement < -0.1f && !m_playerController.CastAbility.AnyAbilityInProgress && _character.IsFacingRight)
		{
			_character.Flip();
		}
		_ = _controller.Parameters.SpeedAccelerationInAir;
		float num = m_playerController.CharacterMove.MovementSpeed;
		if ((bool)m_playerController && m_playerController.IsInitialized)
		{
			float num2 = 1f;
			float num3 = 1f;
			float num4 = 1f;
			if (m_playerController.CastAbility.AbilityInProgress(CastAbilityType.Weapon))
			{
				num4 = m_playerController.CastAbility.GetAbility(CastAbilityType.Weapon).AirMovementMod;
			}
			if (m_playerController.CastAbility.AbilityInProgress(CastAbilityType.Spell))
			{
				num3 = m_playerController.CastAbility.GetAbility(CastAbilityType.Spell).AirMovementMod;
			}
			if (m_playerController.CastAbility.AbilityInProgress(CastAbilityType.Talent))
			{
				num2 = m_playerController.CastAbility.GetAbility(CastAbilityType.Talent).AirMovementMod;
			}
			num *= num2 * num3 * num4;
		}
		float num5 = _horizontalMovement * num * _controller.Parameters.SpeedFactor * MovementSpeedMultiplier;
		float num6 = _verticalMovement * num * _controller.Parameters.SpeedFactor * MovementSpeedMultiplier;
		float horizontalForce = num5;
		float verticalForce = num6;
		_controller.SetHorizontalForce(horizontalForce);
		_controller.SetVerticalForce(verticalForce);
	}

	protected override void InitializeAnimatorParameters()
	{
		RegisterAnimatorParameter("Speed", AnimatorControllerParameterType.Float);
		RegisterAnimatorParameter("Jumping", AnimatorControllerParameterType.Bool);
		RegisterAnimatorParameter("IsFlying", AnimatorControllerParameterType.Bool);
	}

	public override void UpdateAnimator()
	{
		float magnitude = new Vector2(_horizontalMovement, _verticalMovement).magnitude;
		if (IsFlying)
		{
			MMAnimator.UpdateAnimatorFloat(_animator, "Speed", Mathf.Abs(magnitude), _character._animatorParameters);
			MMAnimator.UpdateAnimatorBool(_animator, "Jumping", IsFlying, _character._animatorParameters);
		}
		MMAnimator.UpdateAnimatorBool(_animator, "IsFlying", IsFlying, _character._animatorParameters);
	}
}
