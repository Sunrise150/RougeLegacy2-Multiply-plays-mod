using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using Rewired;
using UnityEngine;

namespace MoreMountains.CorgiEngine;

[SelectionBase]
[AddComponentMenu("Corgi Engine/Character/Core/Character")]
public class Character : MonoBehaviour
{
	public enum CharacterTypes
	{
		Player,
		AI
	}

	public enum FacingDirections
	{
		Left,
		Right
	}

	public enum SpawnFacingDirections
	{
		Default,
		Left,
		Right
	}

	[Information("The Character script is the mandatory basis for all Character abilities. Your character can either be a Non Player Character, controlled by an AI, or a Player character, controlled by the player. In this case, you'll need to specify a PlayerID, which must match the one specified in your InputManager. Usually 'Player1', 'Player2', etc.", InformationAttribute.InformationType.Info, false)]
	public CharacterTypes CharacterType = CharacterTypes.AI;

	public string PlayerID = "";

	[Header("Direction")]
	[Information("It's usually good practice to build all your characters facing right. If that's not the case of this character, select Left instead.", InformationAttribute.InformationType.Info, false)]
	public FacingDirections InitialFacingDirection = FacingDirections.Right;

	[Information("Here you can force a direction the character should face when spawning. If set to default, it'll match your model's initial facing direction.", InformationAttribute.InformationType.Info, false)]
	public SpawnFacingDirections DirectionOnSpawn;

	[Header("Animator")]
	[Information("The engine will try and find an animator for this character. If it's on the same gameobject it should have found it. If it's nested somewhere, you'll need to bind it below. You can also decide to get rid of it altogether, in that case, just uncheck 'use mecanim'.", InformationAttribute.InformationType.Info, false)]
	public Animator CharacterAnimator;

	public bool UseDefaultMecanim = true;

	[Header("Model")]
	[Information("Leave this unbound if this is a regular, sprite-based character, and if the SpriteRenderer and the Character are on the same GameObject. If not, you'll want to parent the actual model to the Character object, and bind it below. See the 3D demo characters for an example of that. The idea behind that is that the model may move, flip, but the collider will remain unchanged.", InformationAttribute.InformationType.Info, false)]
	public GameObject CharacterModel;

	[Information("You can also decide if the character must automatically flip when going backwards or not. Additionnally, if you're not using sprites, you can define here how the character's model's localscale will be affected by flipping. By default it flips on the x axis, but you can change that to fit your model.", InformationAttribute.InformationType.Info, false)]
	public bool FlipOnDirectionChange = true;

	public Vector3 FlipValue = new Vector3(-1f, 1f, 1f);

	public bool UseRegularUpdate;

	public MMStateMachine<CharacterStates.MovementStates> MovementState;

	public MMStateMachine<CharacterStates.CharacterConditions> ConditionState;

	protected Player m_rePlayer;

	protected CorgiController _controller;

	protected SpriteRenderer _spriteRenderer;

	protected Color _initialColor;

	protected CharacterAbility[] _characterAbilities;

	protected float _originalGravity;

	protected bool _spawnDirectionForced;

	private bool m_lockflip;

	private Vector2 m_frozenVelocity;

	public CharacterStates CharacterState { get; protected set; }

	public bool IsFacingRight { get; set; }

	public Animator _animator { get; protected set; }

	public List<string> _animatorParameters { get; set; }

	public Player REPlayer => m_rePlayer;

	public bool HasTurnAnimParam { get; protected set; }

	public bool IsInitialized { get; protected set; }

	public bool LockFlip
	{
		get
		{
			return m_lockflip;
		}
		set
		{
			m_lockflip = value;
		}
	}

	public bool IsFrozen => ConditionState.CurrentState == CharacterStates.CharacterConditions.Frozen;

	protected virtual void Awake()
	{
		Initialization();
	}

	protected virtual void Start()
	{
		StartCoroutine(InitializeInput());
	}

	protected virtual IEnumerator InitializeInput()
	{
		while (!ReInput.isReady)
		{
			yield return null;
		}
		m_rePlayer = Rewired_RL.Player;
	}

	protected virtual void Initialization()
	{
		MovementState = new MMStateMachine<CharacterStates.MovementStates>();
		ConditionState = new MMStateMachine<CharacterStates.CharacterConditions>();
		if (InitialFacingDirection == FacingDirections.Left)
		{
			IsFacingRight = false;
		}
		else
		{
			IsFacingRight = true;
		}
		CharacterState = new CharacterStates();
		_spriteRenderer = GetComponent<SpriteRenderer>();
		_controller = GetComponent<CorgiController>();
		_characterAbilities = GetComponents<CharacterAbility>();
		if ((bool)CharacterAnimator)
		{
			_animator = CharacterAnimator;
		}
		else
		{
			_animator = GetComponent<Animator>();
		}
		if ((bool)_animator)
		{
			InitializeAnimatorParameters();
		}
		_originalGravity = _controller.Parameters.Gravity;
		ForceSpawnDirection();
		IsInitialized = true;
	}

	public virtual void SetPlayerID(string newPlayerID)
	{
		PlayerID = newPlayerID;
	}

	protected virtual void FixedUpdate()
	{
		if (!UseRegularUpdate)
		{
			EveryFrame();
		}
	}

	protected virtual void Update()
	{
		if (UseRegularUpdate)
		{
			EveryFrame();
		}
		CharacterAbility[] characterAbilities = _characterAbilities;
		foreach (CharacterAbility characterAbility in characterAbilities)
		{
			if (characterAbility.enabled && characterAbility.AbilityInitialized)
			{
				characterAbility.InternalHandleInput();
			}
		}
	}

	protected virtual void EveryFrame()
	{
		HandleCharacterStatus();
		EarlyProcessAbilities();
		ProcessAbilities();
		LateProcessAbilities();
		UpdateAnimators();
	}

	protected virtual void EarlyProcessAbilities()
	{
		CharacterAbility[] characterAbilities = _characterAbilities;
		foreach (CharacterAbility characterAbility in characterAbilities)
		{
			if (characterAbility.enabled && characterAbility.AbilityInitialized)
			{
				characterAbility.EarlyProcessAbility();
			}
		}
	}

	protected virtual void ProcessAbilities()
	{
		CharacterAbility[] characterAbilities = _characterAbilities;
		foreach (CharacterAbility characterAbility in characterAbilities)
		{
			if (characterAbility.enabled && characterAbility.AbilityInitialized)
			{
				characterAbility.ProcessAbility();
			}
		}
	}

	protected virtual void LateProcessAbilities()
	{
		CharacterAbility[] characterAbilities = _characterAbilities;
		foreach (CharacterAbility characterAbility in characterAbilities)
		{
			if (characterAbility.enabled && characterAbility.AbilityInitialized)
			{
				characterAbility.LateProcessAbility();
			}
		}
	}

	protected virtual void InitializeAnimatorParameters()
	{
		if (!(_animator == null))
		{
			_animatorParameters = new List<string>();
			MMAnimator.AddAnimatorParamaterIfExists(_animator, "Grounded", AnimatorControllerParameterType.Bool, _animatorParameters);
			MMAnimator.AddAnimatorParamaterIfExists(_animator, "xSpeed", AnimatorControllerParameterType.Float, _animatorParameters);
			MMAnimator.AddAnimatorParamaterIfExists(_animator, "ySpeed", AnimatorControllerParameterType.Float, _animatorParameters);
			MMAnimator.AddAnimatorParamaterIfExists(_animator, "CollidingLeft", AnimatorControllerParameterType.Bool, _animatorParameters);
			MMAnimator.AddAnimatorParamaterIfExists(_animator, "CollidingRight", AnimatorControllerParameterType.Bool, _animatorParameters);
			MMAnimator.AddAnimatorParamaterIfExists(_animator, "CollidingBelow", AnimatorControllerParameterType.Bool, _animatorParameters);
			MMAnimator.AddAnimatorParamaterIfExists(_animator, "CollidingAbove", AnimatorControllerParameterType.Bool, _animatorParameters);
			MMAnimator.AddAnimatorParamaterIfExists(_animator, "FacingRight", AnimatorControllerParameterType.Bool, _animatorParameters);
			MMAnimator.AddAnimatorParamaterIfExists(_animator, "Turn", AnimatorControllerParameterType.Trigger, _animatorParameters);
			MMAnimator.AddAnimatorParamaterIfExists(_animator, "FacingSpeed", AnimatorControllerParameterType.Float, _animatorParameters);
			HasTurnAnimParam = AnimatorUtility.HasParameter(_animator, "Turn");
		}
	}

	protected virtual void UpdateAnimators()
	{
		if (!UseDefaultMecanim || !_animator || !_controller.IsInitialized)
		{
			return;
		}
		MMAnimator.UpdateAnimatorBool(_animator, "Grounded", _controller.State.IsGrounded, _animatorParameters);
		MMAnimator.UpdateAnimatorFloat(_animator, "xSpeed", _controller.Velocity.x, _animatorParameters);
		MMAnimator.UpdateAnimatorFloat(_animator, "ySpeed", _controller.Velocity.y, _animatorParameters);
		MMAnimator.UpdateAnimatorBool(_animator, "CollidingLeft", _controller.State.IsCollidingLeft, _animatorParameters);
		MMAnimator.UpdateAnimatorBool(_animator, "CollidingRight", _controller.State.IsCollidingRight, _animatorParameters);
		MMAnimator.UpdateAnimatorBool(_animator, "CollidingBelow", _controller.State.IsCollidingBelow, _animatorParameters);
		MMAnimator.UpdateAnimatorBool(_animator, "CollidingAbove", _controller.State.IsCollidingAbove, _animatorParameters);
		MMAnimator.UpdateAnimatorBool(_animator, "FacingRight", IsFacingRight, _animatorParameters);
		float num = _controller.Velocity.x;
		if (!IsFacingRight)
		{
			num *= -1f;
		}
		MMAnimator.UpdateAnimatorFloat(_animator, "FacingSpeed", num, _animatorParameters);
		CharacterAbility[] characterAbilities = _characterAbilities;
		foreach (CharacterAbility characterAbility in characterAbilities)
		{
			if (characterAbility.enabled && characterAbility.AbilityInitialized)
			{
				characterAbility.UpdateAnimator();
			}
		}
	}

	protected virtual void HandleCharacterStatus()
	{
		if (ConditionState.CurrentState == CharacterStates.CharacterConditions.Frozen)
		{
			_controller.GravityActive(state: false);
			_controller.SetForce(Vector2.zero);
		}
	}

	public virtual void Freeze()
	{
		_controller.GravityActive(state: false);
		m_frozenVelocity = _controller.Velocity;
		_controller.SetForce(Vector2.zero);
		ConditionState.ChangeState(CharacterStates.CharacterConditions.Frozen);
	}

	public virtual void UnFreeze()
	{
		_controller.SetForce(m_frozenVelocity);
		_controller.GravityActive(state: true);
		ConditionState.ChangeState(CharacterStates.CharacterConditions.Normal);
	}

	public virtual void RecalculateRays()
	{
		_controller.SetRaysParameters();
	}

	public virtual void Disable()
	{
		base.enabled = false;
		_controller.enabled = false;
		GetComponent<Collider2D>().enabled = false;
	}

	public virtual void RespawnAt(Transform spawnPoint, FacingDirections facingDirection)
	{
		if (base.gameObject.activeInHierarchy)
		{
			Face(facingDirection);
			ConditionState.ChangeState(CharacterStates.CharacterConditions.Normal);
			GetComponent<Collider2D>().enabled = true;
			_controller.CollisionsOn();
			base.transform.position = spawnPoint.position;
		}
	}

	public virtual void Flip(bool IgnoreFlipOnDirectionChange = false, bool ignoreTurnParam = false)
	{
		if (LockFlip || IsFrozen || (!FlipOnDirectionChange && !IgnoreFlipOnDirectionChange))
		{
			return;
		}
		if (CharacterModel == null && _spriteRenderer == null)
		{
			Debug.LogWarning("Attempted to flip " + base.name + " sprite/model but failed because none could be found. Please ensure the CharacterModel on the Character component is set correctly.");
		}
		if ((bool)CharacterModel)
		{
			CharacterModel.transform.localScale = Vector3.Scale(CharacterModel.transform.localScale, FlipValue);
			if (CharacterModel.transform.localScale.x > 0f)
			{
				IsFacingRight = true;
			}
			else if (CharacterModel.transform.localScale.x < 0f)
			{
				IsFacingRight = false;
			}
		}
		else if ((bool)_spriteRenderer)
		{
			_spriteRenderer.flipX = !_spriteRenderer.flipX;
			if (!_spriteRenderer.flipX)
			{
				IsFacingRight = true;
			}
			else
			{
				IsFacingRight = false;
			}
		}
		CharacterAbility[] characterAbilities = _characterAbilities;
		foreach (CharacterAbility characterAbility in characterAbilities)
		{
			if (characterAbility.enabled)
			{
				characterAbility.Flip();
			}
		}
		if (!ignoreTurnParam)
		{
			if (HasTurnAnimParam)
			{
				_animator.SetTrigger("Turn");
				_animator.Update(0f);
			}
		}
		else if (HasTurnAnimParam)
		{
			_animator.ResetTrigger("Turn");
			_animator.Update(0f);
		}
	}

	protected virtual void ForceSpawnDirection()
	{
		if (DirectionOnSpawn != 0 && !_spawnDirectionForced)
		{
			_spawnDirectionForced = true;
			if (DirectionOnSpawn == SpawnFacingDirections.Left)
			{
				Face(FacingDirections.Left);
			}
			if (DirectionOnSpawn == SpawnFacingDirections.Right)
			{
				Face(FacingDirections.Right);
			}
		}
	}

	public virtual void Face(FacingDirections facingDirection)
	{
		if (facingDirection == FacingDirections.Right)
		{
			if (!IsFacingRight)
			{
				Flip(IgnoreFlipOnDirectionChange: true);
			}
		}
		else if (IsFacingRight)
		{
			Flip(IgnoreFlipOnDirectionChange: true);
		}
	}

	public virtual void Reset()
	{
		_spawnDirectionForced = false;
		if (_characterAbilities == null || _characterAbilities.Length == 0)
		{
			return;
		}
		CharacterAbility[] characterAbilities = _characterAbilities;
		foreach (CharacterAbility characterAbility in characterAbilities)
		{
			if (characterAbility.enabled)
			{
				characterAbility.Reset();
			}
		}
	}

	protected virtual void OnRevive()
	{
		ForceSpawnDirection();
	}

	protected virtual void OnDeath()
	{
	}

	protected virtual void OnEnable()
	{
	}

	protected virtual void OnDisable()
	{
	}
}
