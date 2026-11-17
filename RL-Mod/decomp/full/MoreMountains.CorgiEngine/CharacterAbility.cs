using MoreMountains.Tools;
using Rewired;
using UnityEngine;

namespace MoreMountains.CorgiEngine;

[RequireComponent(typeof(Character))]
public class CharacterAbility : MonoBehaviour
{
	public bool AbilityPermitted = true;

	protected Character _character;

	protected CharacterHorizontalMovement _characterBasicMovement;

	protected CorgiController _controller;

	protected Animator _animator;

	protected CharacterStates _state;

	protected SpriteRenderer _spriteRenderer;

	protected MMStateMachine<CharacterStates.MovementStates> _movement;

	protected MMStateMachine<CharacterStates.CharacterConditions> _condition;

	protected bool _abilityInitialized;

	protected float _verticalInput;

	protected float _horizontalInput;

	protected PlayerController m_playerController;

	public bool AbilityInitialized => _abilityInitialized;

	public virtual string HelpBoxText()
	{
		return "";
	}

	protected virtual void Start()
	{
		Initialization();
	}

	protected virtual void Initialization()
	{
		_character = GetComponent<Character>();
		_controller = GetComponent<CorgiController>();
		_characterBasicMovement = GetComponent<CharacterHorizontalMovement>();
		_spriteRenderer = GetComponent<SpriteRenderer>();
		_animator = _character._animator;
		_state = _character.CharacterState;
		_movement = _character.MovementState;
		_condition = _character.ConditionState;
		m_playerController = GetComponent<PlayerController>();
		_abilityInitialized = true;
		if (_animator != null)
		{
			InitializeAnimatorParameters();
		}
	}

	protected virtual void InitializeAnimatorParameters()
	{
	}

	public virtual void InternalHandleInput()
	{
		if (ReInput.isReady && _character.REPlayer != null)
		{
			_verticalInput = _character.REPlayer.GetAxis("MoveVertical");
			_horizontalInput = _character.REPlayer.GetAxis("MoveHorizontal");
			HandleInput();
		}
	}

	protected virtual void HandleInput()
	{
	}

	public virtual void EarlyProcessAbility()
	{
	}

	public virtual void ProcessAbility()
	{
	}

	public virtual void LateProcessAbility()
	{
	}

	public virtual void UpdateAnimator()
	{
	}

	public virtual void PermitAbility(bool abilityPermitted)
	{
		AbilityPermitted = abilityPermitted;
	}

	public virtual void Flip()
	{
	}

	public virtual void Reset()
	{
	}

	protected virtual void RegisterAnimatorParameter(string parameterName, AnimatorControllerParameterType parameterType)
	{
		if (!(_animator == null) && _animator.HasParameterOfType(parameterName, parameterType))
		{
			_character._animatorParameters.Add(parameterName);
		}
	}

	protected virtual void OnRespawn()
	{
	}

	protected virtual void OnDeath()
	{
	}

	protected virtual void OnHit()
	{
	}

	protected virtual void OnEnable()
	{
	}

	protected virtual void OnDisable()
	{
	}
}
