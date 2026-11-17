using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.CorgiEngine;

[AddComponentMenu("Corgi Engine/Character/Abilities/Character Horizontal Movement")]
public class CharacterHorizontalMovement : CharacterAbility
{
	[Header("Speed")]
	public float WalkSpeed = 6f;

	protected float _horizontalMovement;

	protected float _horizontalMovementForce;

	protected float _normalizedHorizontalSpeed;

	public float MovementSpeed { get; set; }

	public float MovementSpeedMultiplier { get; set; }

	public float HorizontalMovementForce => _horizontalMovementForce;

	public bool MovementForbidden { get; set; }

	public override string HelpBoxText()
	{
		return "This component handles basic left/right movement, friction, and ground hit detection. Here you can define standard movement speed, walk speed, and what effects to use when the character hits the ground after a jump/fall.";
	}

	protected override void Initialization()
	{
		base.Initialization();
		MovementSpeed = WalkSpeed;
		MovementSpeedMultiplier = 1f;
		MovementForbidden = false;
	}

	public override void ProcessAbility()
	{
		base.ProcessAbility();
		HandleHorizontalMovement();
	}

	protected override void HandleInput()
	{
		_horizontalMovement = _horizontalInput;
	}

	public virtual void SetHorizontalMove(float value)
	{
		_horizontalMovement = value;
	}

	protected virtual void HandleHorizontalMovement()
	{
		CheckJustGotGrounded();
		if (MovementForbidden)
		{
			_horizontalMovement = 0f;
		}
		if (_horizontalMovement > 0.1f)
		{
			_normalizedHorizontalSpeed = _horizontalMovement;
			if (!_character.IsFacingRight)
			{
				_character.Flip();
			}
		}
		else if (_horizontalMovement < -0.1f)
		{
			_normalizedHorizontalSpeed = _horizontalMovement;
			if (_character.IsFacingRight)
			{
				_character.Flip();
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
		float num = (_controller.State.IsGrounded ? _controller.Parameters.SpeedAccelerationOnGround : _controller.Parameters.SpeedAccelerationInAir);
		float b = _normalizedHorizontalSpeed * MovementSpeed * _controller.Parameters.SpeedFactor * MovementSpeedMultiplier;
		_horizontalMovementForce = Mathf.Lerp(_controller.Velocity.x, b, Time.deltaTime * num);
		_horizontalMovementForce = HandleFriction(_horizontalMovementForce);
		_controller.SetHorizontalForce(_horizontalMovementForce);
	}

	protected virtual void CheckJustGotGrounded()
	{
		if (_controller.State.JustGotGrounded)
		{
			if (!_controller.State.ColliderResized)
			{
				_movement.ChangeState(CharacterStates.MovementStates.Idle);
			}
			_controller.SlowFall(0f);
		}
	}

	protected virtual float HandleFriction(float force)
	{
		if (_controller.Friction > 1f)
		{
			force /= _controller.Friction;
		}
		if (_controller.Friction < 1f && _controller.Friction > 0f)
		{
			force = Mathf.Lerp(_controller.Velocity.x, force, Time.deltaTime * _controller.Friction * 10f);
		}
		return force;
	}

	public virtual void ResetHorizontalSpeed()
	{
		MovementSpeed = WalkSpeed;
	}

	protected override void InitializeAnimatorParameters()
	{
		RegisterAnimatorParameter("Speed", AnimatorControllerParameterType.Float);
		RegisterAnimatorParameter("Walking", AnimatorControllerParameterType.Bool);
	}

	public override void UpdateAnimator()
	{
		MMAnimator.UpdateAnimatorFloat(_animator, "Speed", Mathf.Abs(_normalizedHorizontalSpeed), _character._animatorParameters);
		MMAnimator.UpdateAnimatorBool(_animator, "Walking", _movement.CurrentState == CharacterStates.MovementStates.Walking, _character._animatorParameters);
	}

	protected virtual void OnRevive()
	{
		Initialization();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
	}
}
