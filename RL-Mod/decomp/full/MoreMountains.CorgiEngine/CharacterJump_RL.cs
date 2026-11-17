using System;
using System.Collections;
using Rewired;
using UnityEngine;

namespace MoreMountains.CorgiEngine;

[AddComponentMenu("Corgi Engine/Character/Abilities/Character Jump RL")]
public class CharacterJump_RL : CharacterJump
{
	[Tooltip("Deceleration Force applied if Jump Button is released when Character's Jump Height is above the Minimum Jump Height")]
	public float JumpReleaseForce = 2f;

	[Tooltip("Deceleration Force applied if Jump Button is released when Character's Jump Height is below the Minimum Jump Height")]
	public float MinimumJumpReleaseForce = 2f;

	public float FallGravityMultiplier = -1f;

	public float MinimumJumpHeight = 1f;

	public float DoubleJumpHeight = 3f;

	public float JumpHeightMultiplier = 1f;

	public bool DisableBrakeForce;

	[Space(10f)]
	public bool CanJumpWhileDashing = true;

	private bool _prevJumpButtonReleased;

	private bool _prevIsFalling;

	private bool m_isJumpDash;

	private bool m_useBrakeForce;

	private float m_jumpStartTime;

	private Action<MonoBehaviour, EventArgs> m_onPlayerEnterRoom;

	private Coroutine m_preventBrakingCoroutine;

	private WaitRL_Yield m_disableBrakeWaitYield;

	private bool m_queueJump;

	private Coroutine m_queueJumpCoroutine;

	private Coroutine _debugJumpCoroutine;

	private float _playerYPositionAtStart_DEBUG;

	private float _heightAtPeak_DEBUG;

	private float _peakYPosition_DEBUG;

	private float _brakeForce;

	public bool IsJumpDash => m_isJumpDash;

	public override bool JumpAuthorized
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
				if (IsJumpWithinLeeway())
				{
					return true;
				}
				if (base.NumberOfJumpsLeft > 0)
				{
					return true;
				}
			}
			return false;
		}
	}

	public void StartJumpTime()
	{
		m_jumpStartTime = Time.time;
	}

	public void ResetBrakeForce()
	{
		m_useBrakeForce = false;
	}

	private void Awake()
	{
		m_onPlayerEnterRoom = OnPlayerEnterRoom;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
	}

	private void OnPlayerEnterRoom(MonoBehaviour sender, EventArgs eventArgs)
	{
		RoomViaDoorEventArgs roomViaDoorEventArgs = eventArgs as RoomViaDoorEventArgs;
		if (!(roomViaDoorEventArgs.ViaDoor != null) || roomViaDoorEventArgs.ViaDoor.Side == RoomSide.Bottom)
		{
			if (m_preventBrakingCoroutine != null)
			{
				StopCoroutine(m_preventBrakingCoroutine);
			}
			m_preventBrakingCoroutine = StartCoroutine(PreventBraking());
			PlayerController playerController = PlayerManager.GetPlayerController();
			float num = 4.375f;
			playerController.SetVelocityY(Mathf.Min(b: Mathf.Sqrt(2f * Mathf.Abs(_controller.Parameters.Gravity) * num), a: playerController.Velocity.y), additive: false);
		}
	}

	private IEnumerator PreventBraking()
	{
		if (m_disableBrakeWaitYield == null)
		{
			m_disableBrakeWaitYield = new WaitRL_Yield(Room_EV.DISABLE_BRAKING_FORCE_TIME);
		}
		bool disableBrakeForceValueOnStart = DisableBrakeForce;
		DisableBrakeForce = true;
		yield return m_disableBrakeWaitYield;
		DisableBrakeForce = disableBrakeForceValueOnStart;
	}

	protected override void HandleInput()
	{
		if (!ReInput.isReady || m_playerController.ConditionState == CharacterStates.CharacterConditions.ControlledMovement)
		{
			return;
		}
		if (SaveManager.ConfigData.EnableQuickDrop && _movement.CurrentState != CharacterStates.MovementStates.Jumping && !Rewired_RL.Player.GetButton("FreeLook") && _character.REPlayer.GetNegativeButton("MoveVertical") && _controller.State.IsGrounded && !m_playerController.CastAbility.IsAiming)
		{
			float axis = Rewired_RL.Player.GetAxis("MoveVertical");
			float axis2 = Rewired_RL.Player.GetAxis("MoveHorizontal");
			bool flag = RewiredOnStartupController.CurrentActiveControllerType == ControllerType.Keyboard && axis == -1f;
			Vector2 pt = new Vector2(axis2, axis);
			pt.Normalize();
			float num = CDGHelper.VectorToAngle(pt);
			if (((num > 255f && num < 285f) || flag) && JumpDownFromOneWayPlatform())
			{
				return;
			}
		}
		if (_character.REPlayer.GetButtonDown("Jump"))
		{
			JumpStart();
		}
		if (_jumpButtonPressed && !_character.REPlayer.GetButton("Jump"))
		{
			JumpStop();
		}
	}

	public bool IsAbilityPermitted()
	{
		return true;
	}

	public override void ProcessAbility()
	{
		base.ProcessAbility();
		base.JumpHappenedThisFrame = false;
		if (!AbilityPermitted || _condition.CurrentState == CharacterStates.CharacterConditions.Stunned)
		{
			return;
		}
		if (_controller.Velocity.y <= 0.1f && _controller.State.IsGrounded)
		{
			ResetDoubleJump();
		}
		if (!_controller.State.IsGrounded && !_prevIsFalling && !EvaluateJumpTimeWindow())
		{
			_prevIsFalling = true;
			if (base.NumberOfJumpsLeft >= NumberOfJumps)
			{
				base.NumberOfJumpsLeft--;
			}
		}
		if (_controller.State.IsGrounded)
		{
			_lastTimeGrounded = Time.time;
		}
		else
		{
			if (DisableBrakeForce)
			{
				return;
			}
			if (_controller.Velocity.y <= 0f)
			{
				m_useBrakeForce = false;
			}
			if (m_useBrakeForce && _jumpButtonReleased && _prevJumpButtonReleased != _jumpButtonReleased && _controller.Velocity.y > 0f)
			{
				_brakeForce = 0f - JumpReleaseForce;
			}
			else
			{
				_brakeForce = 0f;
			}
			if (_controller.Velocity.y > 0f)
			{
				if (_brakeForce != 0f)
				{
					_controller.AddVerticalForce(_brakeForce * 40f * Time.deltaTime);
				}
			}
			else if (_brakeForce != 0f)
			{
				_brakeForce = 0f;
			}
		}
	}

	private bool OtherAbilitiesCanBeInterruptedByJump()
	{
		if ((bool)m_playerController && m_playerController.IsInitialized)
		{
			if (m_playerController.CastAbility.AbilityInProgress(CastAbilityType.Weapon) && !m_playerController.CastAbility.GetAbility(CastAbilityType.Weapon).JumpInterruptable)
			{
				return false;
			}
			if (m_playerController.CastAbility.AbilityInProgress(CastAbilityType.Spell) && !m_playerController.CastAbility.GetAbility(CastAbilityType.Spell).JumpInterruptable)
			{
				return false;
			}
			if (m_playerController.CastAbility.AbilityInProgress(CastAbilityType.Talent) && !m_playerController.CastAbility.GetAbility(CastAbilityType.Talent).JumpInterruptable)
			{
				return false;
			}
		}
		return true;
	}

	protected override bool EvaluateJumpConditions()
	{
		if (m_playerController.IsGrounded && _condition.CurrentState == CharacterStates.CharacterConditions.Normal && !m_playerController.CharacterDownStrike.IsHoldingDownStrikeAngle)
		{
			m_queueJump = true;
		}
		if (!m_playerController.IsGrounded && m_jumpStartTime + 0.1f > Time.time)
		{
			return false;
		}
		if (_condition.CurrentState == CharacterStates.CharacterConditions.Stunned && (m_playerController.CharacterHitResponse as PlayerHitResponse).CanRecoverFromStun && !m_playerController.IsGrounded)
		{
			return true;
		}
		if (!_controller.State.IsGrounded && m_playerController.CharacterDownStrike.IsHoldingDownStrikeAngle && !SaveManager.ConfigData.DisablePressDownSpinKick)
		{
			return false;
		}
		if (SaveManager.PlayerSaveData.GetRelic(RelicType.FlightBonusCurse).Level > 0 && !_controller.State.IsGrounded)
		{
			m_queueJump = false;
			if (!OtherAbilitiesCanBeInterruptedByJump())
			{
				return false;
			}
			if (!m_playerController.CharacterFlight.IsFlying && !GlobalTeleporterController.IsTeleporting)
			{
				m_playerController.CharacterFlight.StartFlight(2.1474836E+09f, 0.1f);
			}
			else
			{
				m_playerController.CharacterFlight.StopFlight();
			}
			return false;
		}
		if (m_playerController.CastAbility.TalentAbilityType == AbilityType.CometTalent && m_playerController.CastAbility.GetAbility(CastAbilityType.Talent).AbilityActive)
		{
			return true;
		}
		CharacterDash_RL characterDash = m_playerController.CharacterDash;
		if (!AbilityPermitted || !JumpAuthorized || (_condition.CurrentState != 0 && _condition.CurrentState != CharacterStates.CharacterConditions.DisableHorizontalMovement) || (_movement.CurrentState == CharacterStates.MovementStates.Dashing && !CanJumpWhileDashing && !characterDash.IsVoidDashing) || _movement.CurrentState == CharacterStates.MovementStates.DownStriking || (_controller.State.IsCollidingAbove && (!_character.REPlayer.GetNegativeButton("MoveVertical") || !_controller.State.IsGrounded)))
		{
			return false;
		}
		if (!_controller.State.IsGrounded && !IsJumpWithinLeeway() && !EvaluateJumpTimeWindow() && JumpRestrictions != JumpBehavior.CanJumpAnywhereAnyNumberOfTimes && base.NumberOfJumpsLeft <= 0)
		{
			return false;
		}
		if (ReInput.isReady && _character.REPlayer.GetNegativeButton("MoveVertical") && _controller.State.IsGrounded)
		{
			bool flag = m_playerController.CastAbility.IsAiming;
			if (flag && m_playerController.CastAbility.WeaponAbilityType == AbilityType.CannonWeapon && m_playerController.CastAbility.GetAbility(CastAbilityType.Weapon).IsAiming)
			{
				flag = false;
			}
			if (!flag && JumpDownFromOneWayPlatform())
			{
				return false;
			}
		}
		if (!OtherAbilitiesCanBeInterruptedByJump())
		{
			return false;
		}
		return true;
	}

	public bool IsJumpWithinLeeway()
	{
		if (_controller.State.IsGrounded)
		{
			return true;
		}
		if (_controller.Velocity.y > 0f)
		{
			return false;
		}
		if (!_controller.IsWithinJumpLeeway)
		{
			return false;
		}
		return true;
	}

	private IEnumerator QueueJumpCoroutine()
	{
		float startTime = Time.time;
		while (Time.time < startTime + 0.2f)
		{
			if (EvaluateJumpConditions())
			{
				JumpStart();
				if (!_character.REPlayer.GetButton("Jump"))
				{
					JumpStop();
				}
				m_queueJumpCoroutine = null;
				yield break;
			}
			yield return null;
		}
		m_queueJumpCoroutine = null;
	}

	public override void JumpStart()
	{
		m_queueJump = false;
		if (!EvaluateJumpConditions())
		{
			if (AbilityPermitted && m_queueJump && m_queueJumpCoroutine == null)
			{
				m_queueJumpCoroutine = StartCoroutine(QueueJumpCoroutine());
			}
			return;
		}
		if (m_queueJumpCoroutine != null)
		{
			StopCoroutine(m_queueJumpCoroutine);
			m_queueJumpCoroutine = null;
		}
		bool flag = _condition.CurrentState == CharacterStates.CharacterConditions.Stunned && (m_playerController.CharacterHitResponse as PlayerHitResponse).CanRecoverFromStun && !m_playerController.IsGrounded;
		_controller.ResetColliderSize();
		m_isJumpDash = false;
		if ((CanJumpWhileDashing || m_playerController.CharacterDash.IsVoidDashing) && _movement.CurrentState == CharacterStates.MovementStates.Dashing)
		{
			m_playerController.CharacterDash.StopDash();
		}
		m_playerController.CharacterFlight.StopFlight();
		_controller.State.JustStartedJump = true;
		m_jumpStartTime = Time.time;
		_movement.ChangeState(CharacterStates.MovementStates.Jumping);
		if (IsJumpWithinLeeway() && !flag)
		{
			ResetDoubleJump();
			if (TraitManager.IsTraitActive(TraitType.HighJump))
			{
				StartCoroutine(HighJumpEffectCoroutine());
			}
		}
		bool flag2 = m_playerController.CastAbility.TalentAbilityType == AbilityType.CometTalent && m_playerController.CastAbility.GetAbility(CastAbilityType.Talent).AbilityActive;
		if (base.NumberOfJumpsLeft != NumberOfJumps || flag || flag2)
		{
			_doubleJumping = true;
		}
		if (!_doubleJumping && !flag)
		{
			m_playerController.CharacterDash.ResetNumberOfDashes();
		}
		if (!flag)
		{
			base.NumberOfJumpsLeft--;
			if (base.NumberOfJumpsLeft < 0)
			{
				base.NumberOfJumpsLeft = 0;
			}
		}
		_condition.ChangeState(CharacterStates.CharacterConditions.Normal);
		_controller.GravityActive(state: true);
		_controller.CollisionsOn();
		SetJumpFlags();
		m_useBrakeForce = true;
		float num = JumpHeight * JumpHeightMultiplier;
		if (base.NumberOfJumpsLeft < NumberOfJumps - 1)
		{
			num = DoubleJumpHeight * JumpHeightMultiplier;
		}
		if (flag)
		{
			num /= 2f;
			_controller.SetHorizontalForce(0f);
			m_playerController.CharacterHitResponse.StopCharacterStunned();
			m_playerController.GetComponent<BlinkPulseEffect>().StartSingleBlinkEffect(Color.white);
			EffectManager.SetEffectParams("SlowTime_Effect", "m_timeScaleValue", 0.1f, "m_slowDuration", 0.1f);
			EffectManager.PlayEffect(base.gameObject, _animator, "SlowTime_Effect", Vector3.zero, 0.2f, EffectStopType.Gracefully);
			if (SaveManager.PlayerSaveData.GetRelic(RelicType.PlatformOnAerial).Level > 0)
			{
				ProjectileManager.FireProjectile(base.gameObject, "CreatePlatformTalentProjectile", new Vector2(0f, -0.25f));
			}
		}
		float verticalForce = Mathf.Sqrt(2f * Mathf.Abs(_controller.Parameters.Gravity) * num);
		_controller.SetVerticalForce(verticalForce);
		base.JumpHappenedThisFrame = true;
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerJump, this, null);
		if (_doubleJumping)
		{
			_animator.SetTrigger("AirJump");
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerDoubleJump, this, null);
		}
	}

	private IEnumerator HighJumpEffectCoroutine()
	{
		m_playerController.Animator.SetBool("Trait_HighJump", value: true);
		yield return null;
		yield return null;
		m_playerController.Animator.SetBool("Trait_HighJump", value: false);
	}

	private IEnumerator DebugJump(float jumpHeight)
	{
		_playerYPositionAtStart_DEBUG = base.transform.position.y;
		_heightAtPeak_DEBUG = 0f;
		_peakYPosition_DEBUG = _playerYPositionAtStart_DEBUG + jumpHeight;
		while (Mathf.Abs(_controller.Velocity.y) > 0.01f)
		{
			if (_controller.Velocity.y <= 0f && _heightAtPeak_DEBUG == 0f)
			{
				_heightAtPeak_DEBUG = base.transform.position.y - _playerYPositionAtStart_DEBUG;
			}
			yield return null;
		}
		_debugJumpCoroutine = null;
	}

	private void ResetDoubleJump()
	{
		base.NumberOfJumpsLeft = NumberOfJumps;
		_prevIsFalling = false;
		_doubleJumping = false;
		_animator.ResetTrigger("AirJump");
	}

	public override void UpdateAnimator()
	{
		_animator.SetBool("IsDoubleJumping", _doubleJumping);
		base.UpdateAnimator();
	}

	private void OnDrawGizmos()
	{
	}

	public override void SetJumpFlags()
	{
		_jumpButtonPressTime = Time.time;
		_jumpButtonPressed = true;
		_jumpButtonReleased = false;
		_prevJumpButtonReleased = _jumpButtonReleased;
		_brakeForce = 0f;
		DisableBrakeForce = false;
		m_useBrakeForce = false;
	}

	public override void ResetJumpButtonReleased()
	{
		_jumpButtonReleased = false;
		_prevJumpButtonReleased = _jumpButtonReleased;
	}
}
