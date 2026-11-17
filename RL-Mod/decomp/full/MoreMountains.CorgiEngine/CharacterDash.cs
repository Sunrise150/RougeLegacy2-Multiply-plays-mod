using System.Collections;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.CorgiEngine;

[AddComponentMenu("Corgi Engine/Character/Abilities/Character Dash")]
public class CharacterDash : CharacterAbility
{
	[Header("Dash")]
	[SerializeField]
	protected float m_dashDistance = 3f;

	public float DashForce = 40f;

	public float DashCooldown = 1f;

	protected float _cooldownTimeStamp;

	protected CharacterHorizontalMovement _characterHorizontalMovement;

	protected float _startTime;

	protected Vector3 _initialPosition;

	protected float _dashDirection;

	protected float _distanceTraveled;

	protected bool _shouldKeepDashing = true;

	protected float _computedDashForce;

	protected float _slopeAngleSave;

	protected bool _dashEndedNaturally = true;

	protected IEnumerator _dashCoroutine;

	public virtual float DashDistance
	{
		get
		{
			return m_dashDistance;
		}
		set
		{
			m_dashDistance = value;
		}
	}

	public override string HelpBoxText()
	{
		return "This component allows your character to dash. Here you can define the distance the dash should cover, how much force to apply, and the cooldown between the end of a dash and the start of the next one.";
	}

	protected override void Initialization()
	{
		base.Initialization();
		_characterHorizontalMovement = GetComponent<CharacterHorizontalMovement>();
	}

	protected override void HandleInput()
	{
	}

	public override void ProcessAbility()
	{
		base.ProcessAbility();
		if (_movement.CurrentState == CharacterStates.MovementStates.Dashing)
		{
			_controller.GravityActive(state: false);
			_controller.SetVerticalForce(0f);
		}
		if (!_dashEndedNaturally && _movement.CurrentState != CharacterStates.MovementStates.Dashing)
		{
			_dashEndedNaturally = true;
			_controller.Parameters.MaximumSlopeAngle = _slopeAngleSave;
		}
	}

	public virtual void StartDash()
	{
	}

	public virtual void InitiateDash()
	{
		_movement.ChangeState(CharacterStates.MovementStates.Dashing);
		_cooldownTimeStamp = Time.time + DashCooldown;
		_dashCoroutine = Dash();
		StartCoroutine(_dashCoroutine);
	}

	protected virtual IEnumerator Dash()
	{
		if (!AbilityPermitted || _condition.CurrentState != 0)
		{
			yield break;
		}
		_startTime = Time.time;
		_dashEndedNaturally = false;
		_initialPosition = base.transform.position;
		_distanceTraveled = 0f;
		_shouldKeepDashing = true;
		_dashDirection = (_character.IsFacingRight ? 1f : (-1f));
		_computedDashForce = DashForce * _dashDirection;
		_slopeAngleSave = _controller.Parameters.MaximumSlopeAngle;
		_controller.Parameters.MaximumSlopeAngle = 0f;
		while (_distanceTraveled < DashDistance && _shouldKeepDashing && _movement.CurrentState == CharacterStates.MovementStates.Dashing)
		{
			_distanceTraveled = Vector3.Distance(_initialPosition, base.transform.position);
			if (_controller.State.IsCollidingLeft || _controller.State.IsCollidingRight)
			{
				_shouldKeepDashing = false;
				_controller.SetForce(Vector2.zero);
			}
			else
			{
				_controller.GravityActive(state: false);
				_controller.SetVerticalForce(0f);
				_controller.SetHorizontalForce(_computedDashForce);
			}
			yield return null;
		}
		StopDash();
	}

	public virtual void StopDash()
	{
		StopCoroutine(_dashCoroutine);
		_controller.DefaultParameters.MaximumSlopeAngle = _slopeAngleSave;
		_controller.Parameters.MaximumSlopeAngle = _slopeAngleSave;
		_controller.GravityActive(state: true);
		_dashEndedNaturally = true;
		if (_movement.CurrentState == CharacterStates.MovementStates.Dashing)
		{
			_movement.RestorePreviousState();
		}
	}

	protected override void InitializeAnimatorParameters()
	{
		RegisterAnimatorParameter("Dashing", AnimatorControllerParameterType.Bool);
	}

	public override void UpdateAnimator()
	{
		MMAnimator.UpdateAnimatorBool(_animator, "Dashing", _movement.CurrentState == CharacterStates.MovementStates.Dashing, _character._animatorParameters);
	}
}
