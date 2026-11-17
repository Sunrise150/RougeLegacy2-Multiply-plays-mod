using System;
using System.Collections;
using MoreMountains.Tools;
using Rewired;
using Sigtrap.Relays;
using UnityEngine;

namespace MoreMountains.CorgiEngine;

[AddComponentMenu("Corgi Engine/Character/Abilities/Character Dash RL")]
public class CharacterDash_RL : CharacterDash, IHasProjectileNameArray
{
	[NonSerialized]
	private string[] m_projectileNameArray;

	protected Vector2 m_computedDashForce;

	public bool EnableOmnidash = true;

	[SerializeField]
	private int m_totalDashesAllowed = 1;

	[SerializeField]
	private int m_numDashesAvailable = 1;

	private bool m_dashInProgress;

	private bool m_dashRightPressed;

	private bool m_isRecoveryDash;

	private Relay m_dashRelay = new Relay();

	private Relay m_dashCompleteRelay = new Relay();

	public string[] ProjectileNameArray
	{
		get
		{
			if (m_projectileNameArray == null)
			{
				m_projectileNameArray = new string[2] { "RelicDashStrikeExplosionProjectile", "RelicProjectileDashStartProjectile" };
			}
			return m_projectileNameArray;
		}
	}

	public IRelayLink DashRelay => m_dashRelay.link;

	public IRelayLink DashCompleteRelay => m_dashCompleteRelay.link;

	public bool IsVoidDashing
	{
		get
		{
			if (m_dashInProgress)
			{
				return _distanceTraveled > DashDistance;
			}
			return false;
		}
	}

	public bool IsDashing => m_dashInProgress;

	public override float DashDistance
	{
		get
		{
			if (EnableOmnidash)
			{
				return m_dashDistance * 1f;
			}
			return m_dashDistance;
		}
		set
		{
			m_dashDistance = value;
		}
	}

	public int TotalDashesAllowed
	{
		get
		{
			return m_totalDashesAllowed;
		}
		set
		{
			m_totalDashesAllowed = value;
		}
	}

	public int NumDashesAvailable => m_numDashesAvailable;

	protected override void HandleInput()
	{
		if (m_playerController.ConditionState != CharacterStates.CharacterConditions.ControlledMovement && !IsDashing && (_character.REPlayer.GetButtonDown("DashLeft") || _character.REPlayer.GetButtonDown("DashRight")))
		{
			m_dashRightPressed = _character.REPlayer.GetButtonDown("DashRight");
			StartDash();
		}
	}

	public override void ProcessAbility()
	{
		if (_controller.Velocity.y <= 0.1f && _controller.State.IsGrounded && _movement.CurrentState != CharacterStates.MovementStates.Dashing)
		{
			ResetNumberOfDashes();
		}
		if (_movement.CurrentState == CharacterStates.MovementStates.Dashing)
		{
			_controller.GravityActive(state: false);
			_controller.SetVerticalForce(m_computedDashForce.y);
			_controller.SetHorizontalForce(m_computedDashForce.x);
		}
		if (!_dashEndedNaturally && _movement.CurrentState != CharacterStates.MovementStates.Dashing)
		{
			StopDash();
		}
	}

	public override void StartDash()
	{
		if (IsAbilityPermitted())
		{
			m_dashRelay.Dispatch();
			InitiateDash();
		}
	}

	public virtual bool IsAbilityPermitted()
	{
		bool flag = m_playerController.MovementState != CharacterStates.MovementStates.DownStriking && false;
		if (!AbilityPermitted || (_condition.CurrentState != 0 && _condition.CurrentState != CharacterStates.CharacterConditions.DisableHorizontalMovement && _condition.CurrentState != CharacterStates.CharacterConditions.Stunned) || flag || m_dashInProgress)
		{
			return false;
		}
		m_isRecoveryDash = _condition.CurrentState == CharacterStates.CharacterConditions.Stunned && (m_playerController.CharacterHitResponse as PlayerHitResponse).CanRecoverFromStun && !m_playerController.IsGrounded && SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.UnlockAirDash) > 0;
		if (_condition.CurrentState == CharacterStates.CharacterConditions.Stunned && !m_isRecoveryDash)
		{
			return false;
		}
		if (m_numDashesAvailable <= 0 && !m_playerController.IsGrounded && !m_isRecoveryDash)
		{
			return false;
		}
		if (_cooldownTimeStamp > Time.time)
		{
			return false;
		}
		if ((bool)m_playerController && m_playerController.IsInitialized)
		{
			if (m_playerController.CastAbility.AbilityInProgress(CastAbilityType.Weapon) && !m_playerController.CastAbility.GetAbility(CastAbilityType.Weapon).DashInterruptable)
			{
				return false;
			}
			if (m_playerController.CastAbility.AbilityInProgress(CastAbilityType.Spell) && !m_playerController.CastAbility.GetAbility(CastAbilityType.Spell).DashInterruptable)
			{
				return false;
			}
			if (m_playerController.CastAbility.AbilityInProgress(CastAbilityType.Talent) && !m_playerController.CastAbility.GetAbility(CastAbilityType.Talent).DashInterruptable)
			{
				return false;
			}
		}
		return true;
	}

	public override void InitiateDash()
	{
		if ((bool)m_playerController && m_playerController.IsInitialized)
		{
			if (m_playerController.CastAbility.AbilityInProgress(CastAbilityType.Weapon) && !m_playerController.CastAbility.GetAbility(CastAbilityType.Weapon).CanCastWhileDashing)
			{
				m_playerController.CastAbility.StopAbility(CastAbilityType.Weapon, abilityInterrrupted: true);
			}
			if (m_playerController.CastAbility.AbilityInProgress(CastAbilityType.Spell) && !m_playerController.CastAbility.GetAbility(CastAbilityType.Spell).CanCastWhileDashing)
			{
				m_playerController.CastAbility.StopAbility(CastAbilityType.Spell, abilityInterrrupted: true);
			}
			if (m_playerController.CastAbility.AbilityInProgress(CastAbilityType.Talent) && !m_playerController.CastAbility.GetAbility(CastAbilityType.Talent).CanCastWhileDashing)
			{
				m_playerController.CastAbility.StopAbility(CastAbilityType.Talent, abilityInterrrupted: true);
			}
			int level = SaveManager.PlayerSaveData.GetRelic(RelicType.DashStrikeDamageUp).Level;
			if (level > 0)
			{
				Vector2 offset = m_playerController.Midpoint - m_playerController.transform.localPosition;
				Projectile_RL projectile_RL = ProjectileManager.FireProjectile(m_playerController.gameObject, "RelicDashStrikeExplosionProjectile", offset);
				projectile_RL.CastAbilityType = CastAbilityType.Talent;
				projectile_RL.LifespanTimer += 0.2f * (float)(level - 1);
			}
			m_playerController.Animator.SetBool("Bounce", value: false);
			m_playerController.Animator.SetBool("DanceBounce", value: false);
			m_playerController.CharacterFlight.StopFlight();
			BaseAbility_RL ability = m_playerController.CastAbility.GetAbility(CastAbilityType.Weapon);
			if ((bool)ability && ability.CanCastWhileDashing && ability.CritsWhenDashing)
			{
				m_playerController.LookController.SetCritBlinkEffectEnabled(enable: true, CritBlinkEffectTriggerType.DashAttack);
			}
		}
		_movement.ChangeState(CharacterStates.MovementStates.Dashing);
		RelicObj relic = SaveManager.PlayerSaveData.GetRelic(RelicType.ProjectileDashStart);
		if (relic.Level > 0)
		{
			if (relic.IntValue >= 0)
			{
				Projectile_RL obj = ProjectileManager.FireProjectile(offset: new Vector2(0f, 1f), source: base.gameObject, projectileName: "RelicProjectileDashStartProjectile");
				obj.CastAbilityType = CastAbilityType.Talent;
				obj.LifespanTimer += 1f * (float)(relic.Level - 1);
				relic.SetIntValue(0, additive: false);
			}
			else
			{
				relic.SetIntValue(1, additive: true);
			}
		}
		if (m_isRecoveryDash)
		{
			m_playerController.CharacterHitResponse.StopCharacterStunned();
			m_playerController.GetComponent<BlinkPulseEffect>().StartSingleBlinkEffect(Color.white);
			EffectManager.SetEffectParams("SlowTime_Effect", "m_timeScaleValue", 0.1f, "m_slowDuration", 0.1f);
			EffectManager.PlayEffect(base.gameObject, _animator, "SlowTime_Effect", Vector3.zero, 0.2f, EffectStopType.Gracefully);
		}
		if (TraitManager.IsTraitActive(TraitType.InvulnDash))
		{
			m_playerController.HitboxController.ChangeCollisionType(HitboxType.Body, CollisionType.None);
		}
		else
		{
			m_playerController.HitboxController.ChangeCollisionType(HitboxType.Body, CollisionType.Player_Dodging);
		}
		_cooldownTimeStamp = Time.time + DashCooldown + DashDistance / DashForce;
		_dashCoroutine = Dash();
		StartCoroutine(_dashCoroutine);
	}

	protected override IEnumerator Dash()
	{
		bool isVoidDash = SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.UnlockVoidDash) > 0;
		if (isVoidDash)
		{
			_animator.SetBool("VoidDash", value: true);
		}
		_animator.SetTrigger("Dashing_Effect");
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerDash, this, null);
		m_numDashesAvailable--;
		m_numDashesAvailable = Mathf.Clamp(m_numDashesAvailable, 0, 9999);
		m_dashInProgress = true;
		_startTime = Time.time;
		_dashEndedNaturally = false;
		_initialPosition = base.transform.position;
		_distanceTraveled = 0f;
		_shouldKeepDashing = true;
		_dashDirection = Mathf.RoundToInt(_horizontalInput);
		if (_dashDirection == 0f)
		{
			_dashDirection = (_character.IsFacingRight ? 1f : (-1f));
		}
		if (!EnableOmnidash && SaveManager.ConfigData.EnableDualButtonDash)
		{
			if (m_dashRightPressed)
			{
				_dashDirection = 1f;
			}
			else
			{
				_dashDirection = -1f;
			}
		}
		_computedDashForce = DashForce * _dashDirection;
		float initialDashDuration = DashDistance / DashForce;
		float dashStartTime = Time.time;
		float totalDashTime = Time.time + initialDashDuration;
		if (EnableOmnidash)
		{
			ControllerType lastActiveControllerType = ReInput.controllers.GetLastActiveControllerType();
			Vector2 vector;
			if (SaveManager.ConfigData.ToggleMouseAiming && (lastActiveControllerType == ControllerType.Mouse || lastActiveControllerType == ControllerType.Keyboard))
			{
				Vector2 pt = m_playerController.Midpoint;
				Vector2 pt2 = CameraController.GameCamera.ScreenToWorldPoint(ReInput.controllers.Mouse.screenPosition);
				if (TraitManager.IsTraitActive(TraitType.UpsideDown))
				{
					float num = CameraController.GameCamera.transform.position.y - pt.y;
					pt.y += num * 2f;
				}
				vector = CDGHelper.VectorBetweenPts(pt, pt2);
			}
			else
			{
				vector = new Vector2(_horizontalInput, _verticalInput);
			}
			vector.Normalize();
			if (vector == Vector2.zero)
			{
				if (_character.IsFacingRight)
				{
					vector.x = 1f;
				}
				else
				{
					vector.x = -1f;
				}
			}
			if ((vector.x < 0f && m_playerController.IsFacingRight) || (vector.x > 0f && !m_playerController.IsFacingRight))
			{
				m_playerController.CharacterCorgi.Flip();
			}
			m_computedDashForce.x = vector.x * DashForce;
			m_computedDashForce.y = vector.y * DashForce;
		}
		else
		{
			m_computedDashForce.x = _computedDashForce;
			m_computedDashForce.y = 0f;
		}
		if (m_isRecoveryDash && ((!_character.IsFacingRight && _dashDirection > 0f) || (_character.IsFacingRight && _dashDirection < 0f)))
		{
			_character.Flip();
		}
		float maxDistance = DashDistance;
		if (isVoidDash)
		{
			maxDistance *= 2f;
		}
		bool dashButtonReleased = false;
		while (_distanceTraveled < maxDistance && _shouldKeepDashing && _movement.CurrentState == CharacterStates.MovementStates.Dashing && totalDashTime > Time.time)
		{
			_distanceTraveled = Vector3.Distance(_initialPosition, base.transform.position);
			_controller.GravityActive(state: false);
			_controller.SetVerticalForce(m_computedDashForce.y);
			_controller.SetHorizontalForce(m_computedDashForce.x);
			if (!dashButtonReleased)
			{
				dashButtonReleased = ((!m_dashRightPressed) ? _character.REPlayer.GetButtonUp("DashLeft") : _character.REPlayer.GetButtonUp("DashRight"));
			}
			if (isVoidDash)
			{
				totalDashTime = ((dashButtonReleased || (!_character.REPlayer.GetButton("DashLeft") && !_character.REPlayer.GetButton("DashRight"))) ? (dashStartTime + initialDashDuration) : (dashStartTime + initialDashDuration * 2f));
			}
			yield return null;
		}
		StopDash();
	}

	public override void StopDash()
	{
		if (_dashCoroutine != null)
		{
			StopCoroutine(_dashCoroutine);
		}
		_controller.GravityActive(state: true);
		_controller.SetForce(Vector2.zero);
		_dashEndedNaturally = true;
		m_dashInProgress = false;
		_cooldownTimeStamp = 0f;
		if (_movement.CurrentState == CharacterStates.MovementStates.Dashing)
		{
			_movement.ChangeState(CharacterStates.MovementStates.Idle);
		}
		_animator.SetBool("VoidDash", value: false);
		_animator.ResetTrigger("Dashing_Effect");
		m_playerController.HitboxController.ChangeCollisionType(HitboxType.Body, CollisionType.Player);
		BaseAbility_RL ability = m_playerController.CastAbility.GetAbility(CastAbilityType.Weapon);
		if ((bool)ability && ability.CritsWhenDashing)
		{
			m_playerController.LookController.SetCritBlinkEffectEnabled(enable: false, CritBlinkEffectTriggerType.DashAttack);
		}
		m_dashCompleteRelay.Dispatch();
	}

	public void ResetNumberOfDashes()
	{
		m_numDashesAvailable = m_totalDashesAllowed;
	}

	public override void UpdateAnimator()
	{
		bool abilityInProgress = m_playerController.CharacterDownStrike.AbilityInProgress;
		MMAnimator.UpdateAnimatorBool(_animator, "Dashing", _movement.CurrentState == CharacterStates.MovementStates.Dashing && !abilityInProgress, _character._animatorParameters);
	}
}
