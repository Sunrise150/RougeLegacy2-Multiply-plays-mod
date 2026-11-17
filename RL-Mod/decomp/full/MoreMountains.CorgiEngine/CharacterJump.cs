using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.CorgiEngine;

[AddComponentMenu("Corgi Engine/Character/Abilities/Character Jump")]
public class CharacterJump : CharacterAbility
{
	public enum JumpBehavior
	{
		CanJumpOnGround,
		CanJumpAnywhere,
		CantJump,
		CanJumpAnywhereAnyNumberOfTimes
	}

	[Header("Jump Behaviour")]
	public float JumpHeight = 3.025f;

	public int NumberOfJumps = 3;

	public JumpBehavior JumpRestrictions = JumpBehavior.CanJumpAnywhere;

	public float JumpTimeWindow;

	[Header("Proportional jumps")]
	public bool JumpIsProportionalToThePressTime = true;

	public float JumpMinimumAirTime = 0.1f;

	public float JumpReleaseForceFactor = 2f;

	[Header("Collisions")]
	public float OneWayPlatformsJumpCollisionOffDuration = 0.3f;

	public float MovingPlatformsJumpCollisionOffDuration = 0.05f;

	protected float _jumpButtonPressTime;

	protected bool _jumpButtonPressed;

	protected bool _jumpButtonReleased;

	protected bool _doubleJumping;

	protected CharacterHorizontalMovement _characterHorizontalMovement;

	protected float _lastTimeGrounded;

	public int NumberOfJumpsLeft { get; protected set; }

	public bool JumpHappenedThisFrame { get; set; }

	public virtual bool JumpAuthorized
	{
		get
		{
			if (EvaluateJumpTimeWindow())
			{
				return true;
			}
			if (JumpRestrictions == JumpBehavior.CanJumpAnywhere || JumpRestrictions == JumpBehavior.CanJumpAnywhereAnyNumberOfTimes)
			{
				return true;
			}
			if (JumpRestrictions == JumpBehavior.CanJumpOnGround)
			{
				if (_controller.State.IsGrounded)
				{
					return true;
				}
				if (NumberOfJumpsLeft > 0)
				{
					return true;
				}
			}
			return false;
		}
	}

	public override string HelpBoxText()
	{
		return "This component handles jumps. Here you can define the jump height, whether the jump is proportional to the press length or not, the minimum air time (how long a character should stay in the air before being able to go down if the player has released the jump button), a jump window duration (the time during which, after falling off a cliff, a jump is still possible), jump restrictions, how many jumps the character can perform without touching the ground again, and how long collisions should be disabled when exiting 1-way platforms or moving platforms.";
	}

	protected override void Initialization()
	{
		base.Initialization();
		ResetNumberOfJumps();
		_characterHorizontalMovement = GetComponent<CharacterHorizontalMovement>();
	}

	protected override void HandleInput()
	{
	}

	public override void ProcessAbility()
	{
		base.ProcessAbility();
		JumpHappenedThisFrame = false;
		if (AbilityPermitted)
		{
			if (_controller.State.JustGotGrounded)
			{
				NumberOfJumpsLeft = NumberOfJumps;
				_doubleJumping = false;
			}
			if (_controller.State.IsGrounded)
			{
				_lastTimeGrounded = Time.time;
			}
		}
	}

	protected virtual bool EvaluateJumpTimeWindow()
	{
		if (_movement.CurrentState == CharacterStates.MovementStates.Jumping || _movement.CurrentState == CharacterStates.MovementStates.DoubleJumping || _movement.CurrentState == CharacterStates.MovementStates.WallJumping)
		{
			return false;
		}
		float num = JumpTimeWindow;
		if (_movement.CurrentState == CharacterStates.MovementStates.Dashing)
		{
			num *= 0.5f;
		}
		if (Time.time - _lastTimeGrounded <= num)
		{
			return true;
		}
		return false;
	}

	protected virtual bool EvaluateJumpConditions()
	{
		if (!AbilityPermitted || !JumpAuthorized || _condition.CurrentState != 0 || _movement.CurrentState == CharacterStates.MovementStates.Dashing || _controller.State.IsCollidingAbove)
		{
			return false;
		}
		if (!_controller.State.IsGrounded && !EvaluateJumpTimeWindow() && JumpRestrictions != JumpBehavior.CanJumpAnywhereAnyNumberOfTimes && NumberOfJumpsLeft <= 0)
		{
			return false;
		}
		return true;
	}

	public virtual void JumpStart()
	{
		if (EvaluateJumpConditions())
		{
			_controller.ResetColliderSize();
			_movement.ChangeState(CharacterStates.MovementStates.Jumping);
			MMEventManager.TriggerEvent(new MMCharacterEvent(_character, MMCharacterEventTypes.Jump));
			if (NumberOfJumpsLeft != NumberOfJumps)
			{
				_doubleJumping = true;
			}
			NumberOfJumpsLeft--;
			_condition.ChangeState(CharacterStates.CharacterConditions.Normal);
			_controller.GravityActive(state: true);
			_controller.CollisionsOn();
			SetJumpFlags();
			_controller.SetVerticalForce(Mathf.Sqrt(2f * JumpHeight * Mathf.Abs(_controller.Parameters.Gravity)));
			JumpHappenedThisFrame = true;
		}
	}

	protected virtual bool JumpDownFromOneWayPlatform()
	{
		if (_controller.StandingOn == null)
		{
			return false;
		}
		if (_controller.StandingOn.gameObject.CompareTag("MagicPlatform"))
		{
			return false;
		}
		if (_controller.OneWayPlatformMask.Contains(_controller.StandingOn.layer) || _controller.MovingOneWayPlatformMask.Contains(_controller.StandingOn.layer))
		{
			StartCoroutine(_controller.DisableCollisionsWithOneWayPlatforms(OneWayPlatformsJumpCollisionOffDuration));
			_controller.DetachFromMovingPlatform();
			_controller.StandingOn = null;
			_controller.SetVerticalForce(0f);
			return true;
		}
		return false;
	}

	protected virtual void JumpFromMovingPlatform()
	{
		if (_controller.StandingOn != null && (_controller.MovingPlatformMask.Contains(_controller.StandingOn.layer) || _controller.MovingOneWayPlatformMask.Contains(_controller.StandingOn.layer)))
		{
			StartCoroutine(_controller.DisableCollisionsWithMovingPlatforms(MovingPlatformsJumpCollisionOffDuration));
			_controller.DetachFromMovingPlatform();
		}
	}

	public virtual void JumpStop()
	{
		_jumpButtonPressed = false;
		_jumpButtonReleased = true;
	}

	public virtual void ResetNumberOfJumps()
	{
		NumberOfJumpsLeft = NumberOfJumps;
	}

	public virtual void SetJumpFlags()
	{
		_jumpButtonPressTime = Time.time;
		_jumpButtonPressed = true;
		_jumpButtonReleased = false;
	}

	public virtual void SetNumberOfJumpsLeft(int newNumberOfJumps)
	{
		NumberOfJumpsLeft = newNumberOfJumps;
	}

	public virtual void ResetJumpButtonReleased()
	{
		_jumpButtonReleased = false;
	}

	protected override void InitializeAnimatorParameters()
	{
		RegisterAnimatorParameter("Jumping", AnimatorControllerParameterType.Bool);
		RegisterAnimatorParameter("DoubleJumping", AnimatorControllerParameterType.Bool);
		RegisterAnimatorParameter("HitTheGround", AnimatorControllerParameterType.Bool);
	}

	public override void UpdateAnimator()
	{
		MMAnimator.UpdateAnimatorBool(_animator, "Jumping", _movement.CurrentState == CharacterStates.MovementStates.Jumping, _character._animatorParameters);
		MMAnimator.UpdateAnimatorBool(_animator, "DoubleJumping", _doubleJumping, _character._animatorParameters);
		MMAnimator.UpdateAnimatorBool(_animator, "HitTheGround", _controller.State.JustGotGrounded, _character._animatorParameters);
	}

	public override void Reset()
	{
		base.Reset();
		NumberOfJumpsLeft = NumberOfJumps;
	}
}
