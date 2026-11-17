using System;
using System.Collections;
using MoreMountains.Tools;
using RLAudio;
using Sigtrap.Relays;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace MoreMountains.CorgiEngine;

[AddComponentMenu("Corgi Engine/Character/Abilities/Character Downstrike RL")]
public class CharacterDownStrike_RL : CharacterAbility, IHasProjectileNameArray, IAudioEventEmitter
{
	[Space(10f)]
	[SerializeField]
	protected string m_projectileName;

	[SerializeField]
	protected string m_spinKickProjectileName;

	[SerializeField]
	protected string m_resonantSpinKickProjectileName;

	[SerializeField]
	private string m_resonantDownStrikeProjectileName;

	[SerializeField]
	private string m_relicCaltropDropProjectileName;

	[SerializeField]
	private string m_relicCaltropProjectileName;

	[Space(10f)]
	public float AttackDuration = 3f;

	public float AttackSpeed = 40f;

	public float AttackCooldown;

	public float ForwardKickAngle = -70f;

	public Vector2Int ForwardKickMinMaxAngle = new Vector2Int(-120, -70);

	public Vector2Int DownKickMinMaxAngle = new Vector2Int(-100, -90);

	public float AttackBounceHeight = 2f;

	public float BounceInputLockDuration = 1f;

	public bool AttackButtonTriggersSpinKick;

	public bool SpinKickInstead;

	[SerializeField]
	[ConditionalHide("SpinKickInstead", true)]
	private float m_spinKickTellAnimMultiplier = 1f;

	[SerializeField]
	[ConditionalHide("SpinKickInstead", true)]
	private float m_spinKickAttackAnimMultiplier = 1f;

	[SerializeField]
	[ConditionalHide("SpinKickInstead", true)]
	private float m_spinKickExitAnimMultiplier = 1f;

	[Header("Bounce Mods")]
	public bool ResetsDoubleJump;

	public bool ResetsDash;

	[SerializeField]
	[FormerlySerializedAs("m_downStrikeBounceUnityEvent")]
	private UnityEvent m_bounceEffectTriggeredUnityEvent;

	protected Vector2 m_computedDashForce;

	protected float _cooldownTimeStamp;

	protected float _startTime;

	protected Vector3 _initialPosition;

	protected float _dashDirection;

	protected float _distanceTraveled;

	protected float _slopeAngleSave;

	protected bool _downstrikeEndedNaturally = true;

	protected IEnumerator _downstrikeCoroutine;

	protected float m_totalDashTime;

	protected bool m_attackInProgress;

	protected IEnumerator m_attackCoroutine;

	protected DownstrikeProjectile_RL m_firedProjectile;

	protected float m_inputAngle;

	protected Vector2 m_forwardKickRightAngle;

	protected Vector2 m_forwardKickLeftAngle;

	protected Vector3 m_originalRotation;

	protected float m_bounceLockTimer;

	private int m_consecutiveStrikes;

	private WaitUntil m_waitUntilSpinKickAttackYield;

	private Action<CorgiController_RL> m_resetConsecutiveStrikes;

	private Action<Projectile_RL, GameObject> m_bounce;

	[NonSerialized]
	private string[] m_projectileNameArray;

	private Relay<Projectile_RL, GameObject> m_onSuccessfulDownstrikeRelay = new Relay<Projectile_RL, GameObject>();

	private float m_highestBounceAmount = float.MinValue;

	private bool m_triggerBounce;

	private bool m_triggerBounceInvincibility;

	private ForceManaRegenEventArgs m_regenEventArgs = new ForceManaRegenEventArgs(0f, usePlayerRegenMods: false);

	private PlayerDownstrikeEventArgs m_downstrikeEventArgs = new PlayerDownstrikeEventArgs(null, null);

	public bool IsTriggeringBounce => m_triggerBounce;

	public bool AbilityInProgress => m_attackInProgress;

	public string Description => ToString();

	public string[] ProjectileNameArray
	{
		get
		{
			if (m_projectileNameArray == null)
			{
				m_projectileNameArray = new string[6] { m_projectileName, m_spinKickProjectileName, m_resonantSpinKickProjectileName, m_resonantDownStrikeProjectileName, m_relicCaltropDropProjectileName, m_relicCaltropProjectileName };
			}
			return m_projectileNameArray;
		}
	}

	public OnDownStrikeDelegate OnDownStrikeEvent { get; set; }

	public IRelayLink<Projectile_RL, GameObject> OnSuccessfulDownstrikeRelay => m_onSuccessfulDownstrikeRelay;

	public bool IsHoldingDownStrikeAngle
	{
		get
		{
			float num = Mathf.Atan2(_verticalInput, _horizontalInput) * 57.29578f;
			if (num > (float)ForwardKickMinMaxAngle.x)
			{
				return num < (float)ForwardKickMinMaxAngle.y;
			}
			return false;
		}
	}

	public override string HelpBoxText()
	{
		return "This component allows your character to dash. Here you can define the distance the dash should cover, how much force to apply, and the cooldown between the end of a dash and the start of the next one.";
	}

	protected override void Initialization()
	{
		base.Initialization();
		m_forwardKickRightAngle = CDGHelper.AngleToVector(ForwardKickAngle);
		m_forwardKickRightAngle.Normalize();
		m_forwardKickLeftAngle = CDGHelper.AngleToVector(-180f - ForwardKickAngle);
		m_forwardKickLeftAngle.Normalize();
		m_waitUntilSpinKickAttackYield = new WaitUntil(() => EnteredSpinAttackState());
		m_resetConsecutiveStrikes = ResetConsecutiveStrikes;
		m_playerController.ControllerCorgi.OnCorgiLandedEnterRelay.AddListener(m_resetConsecutiveStrikes);
		m_bounce = Bounce;
	}

	protected override void HandleInput()
	{
		if (m_playerController.ConditionState != CharacterStates.CharacterConditions.ControlledMovement)
		{
			if (AttackButtonTriggersSpinKick && _character.REPlayer.GetButtonDown("Attack"))
			{
				StartDownStrike(ignoreDownstrikeAngle: false);
			}
			else if (!SaveManager.ConfigData.DisablePressDownSpinKick && _character.REPlayer.GetButtonDown("Jump"))
			{
				StartDownStrike(ignoreDownstrikeAngle: false);
			}
			else if (_character.REPlayer.GetButtonDown("Downstrike"))
			{
				StartDownStrike(ignoreDownstrikeAngle: true);
			}
		}
	}

	public virtual void StartDownStrike(bool ignoreDownstrikeAngle)
	{
		bool flag = _movement.CurrentState != CharacterStates.MovementStates.Dashing && false;
		if (AbilityPermitted && !_controller.State.IsGrounded && _condition.CurrentState != CharacterStates.CharacterConditions.Stunned && (_condition.CurrentState == CharacterStates.CharacterConditions.Normal || _condition.CurrentState == CharacterStates.CharacterConditions.DisableHorizontalMovement) && !flag && !m_attackInProgress && !(m_bounceLockTimer > 0f) && (IsHoldingDownStrikeAngle || ignoreDownstrikeAngle) && (!IsHoldingDownStrikeAngle || ignoreDownstrikeAngle || !_controller.State.JustStartedJump) && _cooldownTimeStamp <= Time.time)
		{
			if ((_movement.CurrentState != CharacterStates.MovementStates.Flying && _movement.CurrentState != CharacterStates.MovementStates.Dashing) || !SpinKickInstead)
			{
				_movement.ChangeState(CharacterStates.MovementStates.DownStriking);
			}
			_controller.State.IsCollidingBelow = false;
			_controller.State.IsFalling = true;
			_cooldownTimeStamp = Time.time + AttackCooldown + AttackDuration / AttackSpeed;
			float inputAngle = Mathf.Atan2(_verticalInput, _horizontalInput) * 57.29578f;
			m_inputAngle = inputAngle;
			_downstrikeCoroutine = DownStrike();
			StartCoroutine(_downstrikeCoroutine);
		}
	}

	protected virtual IEnumerator DownStrike()
	{
		if (!AbilityPermitted || (_condition.CurrentState != 0 && _condition.CurrentState != CharacterStates.CharacterConditions.DisableHorizontalMovement))
		{
			yield break;
		}
		m_playerController.CastAbility.StopAllAbilities(stopPersistentAbilities: false);
		_startTime = Time.time;
		_downstrikeEndedNaturally = false;
		_initialPosition = base.transform.position;
		_distanceTraveled = 0f;
		_dashDirection = (_character.IsFacingRight ? 1f : (-1f));
		m_totalDashTime = Time.time + AttackDuration / AttackSpeed;
		m_attackInProgress = true;
		if (m_inputAngle > (float)ForwardKickMinMaxAngle.x && m_inputAngle < (float)DownKickMinMaxAngle.x && !SpinKickInstead)
		{
			m_computedDashForce.x = m_forwardKickLeftAngle.x * AttackSpeed;
			m_computedDashForce.y = m_forwardKickLeftAngle.y * AttackSpeed;
		}
		else if (m_inputAngle > (float)DownKickMinMaxAngle.y && m_inputAngle < (float)ForwardKickMinMaxAngle.y && !SpinKickInstead)
		{
			m_computedDashForce.x = m_forwardKickRightAngle.x * AttackSpeed;
			m_computedDashForce.y = m_forwardKickRightAngle.y * AttackSpeed;
		}
		else
		{
			m_computedDashForce.x = 0f;
			m_computedDashForce.y = -1f * AttackSpeed;
		}
		if (!SpinKickInstead)
		{
			if (SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.UnlockBouncableDownstrike) > 0)
			{
				m_firedProjectile = (DownstrikeProjectile_RL)ProjectileManager.FireProjectile(base.gameObject, m_resonantDownStrikeProjectileName, Vector2.zero, matchFacing: false);
			}
			else
			{
				m_firedProjectile = (DownstrikeProjectile_RL)ProjectileManager.FireProjectile(base.gameObject, m_projectileName, Vector2.zero, matchFacing: false);
			}
			if (TraitManager.IsTraitActive(TraitType.BounceTerrain))
			{
				m_firedProjectile.CanHitWall = true;
			}
			else
			{
				m_firedProjectile.CanHitWall = false;
			}
		}
		else
		{
			MMAnimator.UpdateAnimatorTrigger(_animator, "360Kick");
			MMAnimator.UpdateAnimatorFloat(_animator, "Ability_Anim_Speed", m_spinKickTellAnimMultiplier);
			if (SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.UnlockBouncableDownstrike) > 0)
			{
				m_firedProjectile = (DownstrikeProjectile_RL)ProjectileManager.FireProjectile(base.gameObject, m_resonantSpinKickProjectileName, Vector2.zero, matchFacing: false);
			}
			else
			{
				m_firedProjectile = (DownstrikeProjectile_RL)ProjectileManager.FireProjectile(base.gameObject, m_spinKickProjectileName, Vector2.zero, matchFacing: false);
			}
			m_firedProjectile.transform.position = m_playerController.Midpoint;
			if (TraitManager.IsTraitActive(TraitType.BounceTerrain))
			{
				m_firedProjectile.CanHitWall = true;
			}
			else
			{
				m_firedProjectile.CanHitWall = false;
			}
		}
		int level = SaveManager.PlayerSaveData.GetRelic(RelicType.SpinKickArmorBreak).Level;
		if (level > 0)
		{
			m_firedProjectile.AttachStatusEffect(StatusEffectType.Enemy_ArmorBreak, 3.5f * (float)level);
		}
		m_firedProjectile.OnCollisionRelay.AddListener(m_bounce);
		if (OnDownStrikeEvent != null)
		{
			OnDownStrikeEvent();
		}
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerDownstrikeCast, this, null);
		if (SpinKickInstead)
		{
			yield return m_waitUntilSpinKickAttackYield;
			MMAnimator.UpdateAnimatorFloat(_animator, "Ability_Anim_Speed", m_spinKickAttackAnimMultiplier);
		}
		bool isSpinKicking = SpinKickInstead;
		while (_movement.CurrentState == CharacterStates.MovementStates.DownStriking || isSpinKicking)
		{
			_distanceTraveled = Vector3.Distance(_initialPosition, base.transform.position);
			if (_controller.State.IsCollidingBelow)
			{
				_character.MovementState.ChangeState(CharacterStates.MovementStates.Idle);
				StopDownStrike();
				yield break;
			}
			if (!SpinKickInstead)
			{
				_controller.GravityActive(state: false);
				_controller.SetVerticalForce(m_computedDashForce.y);
				_controller.SetHorizontalForce(m_computedDashForce.x);
			}
			else
			{
				AnimatorStateInfo currentAnimatorStateInfo = _animator.GetCurrentAnimatorStateInfo(0);
				if (!_animator.GetBool("360Kick") && !currentAnimatorStateInfo.IsName("SpinKick_Tell") && !currentAnimatorStateInfo.IsName("SpinKick_Attack"))
				{
					StopDownStrike();
					yield break;
				}
			}
			yield return null;
		}
		StopDownStrike();
	}

	private bool EnteredSpinAttackState()
	{
		if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("SpinKick_Attack"))
		{
			return _movement.CurrentState != CharacterStates.MovementStates.DownStriking;
		}
		return true;
	}

	public override void ProcessAbility()
	{
		base.ProcessAbility();
	}

	public virtual void StopDownStrike()
	{
		if (_downstrikeCoroutine != null)
		{
			StopCoroutine(_downstrikeCoroutine);
		}
		if (!SpinKickInstead && !m_playerController.CharacterDash.IsDashing)
		{
			_controller.GravityActive(state: true);
			_controller.SetForce(Vector2.zero);
		}
		_downstrikeEndedNaturally = true;
		m_attackInProgress = false;
		if (_movement.CurrentState == CharacterStates.MovementStates.DownStriking)
		{
			_movement.ChangeState(CharacterStates.MovementStates.Idle);
		}
		if ((bool)m_firedProjectile)
		{
			m_firedProjectile.OnCollisionRelay.RemoveListener(m_bounce);
			m_firedProjectile.FlagForDestruction();
		}
		MMAnimator.UpdateAnimatorFloat(_animator, "Ability_Anim_Speed", 1f);
		m_inputAngle = 0f;
	}

	protected void Update()
	{
		UpdateBounceLockTimer();
	}

	private void LateUpdate()
	{
		if (m_triggerBounce)
		{
			TriggerBounce(isDownstriking: true);
		}
	}

	public void Bounce(Projectile_RL projectile, GameObject colliderObj)
	{
		m_downstrikeEventArgs.Initialise(projectile, colliderObj);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerDownstrikeBounce, this, m_downstrikeEventArgs);
		Collider2D lastCollidedWith = projectile.HitboxController.LastCollidedWith;
		if (lastCollidedWith.CompareTag("Enemy"))
		{
			int manaRegenOnSpinKick = RuneLogicHelper.GetManaRegenOnSpinKick();
			if (manaRegenOnSpinKick > 0)
			{
				m_regenEventArgs.Initialise(manaRegenOnSpinKick, usePlayerRegenMods: false);
				EffectManager.PlayEffect(m_playerController.gameObject, _animator, "ManaRegenBurst_Effect", Vector3.zero, 0f, EffectStopType.Gracefully);
				Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerForceManaRegen, this, m_regenEventArgs);
			}
			if (m_playerController.CharacterClass.ClassType == ClassType.LuteClass)
			{
				m_playerController.StatusEffectController.StartStatusEffect(StatusEffectType.Player_Dance, 0f, m_playerController);
			}
		}
		if (_condition.CurrentState == CharacterStates.CharacterConditions.Stunned || _condition.CurrentState == CharacterStates.CharacterConditions.Dead)
		{
			return;
		}
		if (lastCollidedWith.CompareTag("EnemyProjectile") || lastCollidedWith.CompareTag("Generic_Bounceable"))
		{
			Vector3 effectPosition = projectile.HitboxController.LastCollidedWith.ClosestPoint(base.transform.position);
			EffectManager.PlayEffect(projectile.gameObject, _animator, "BounceProjectileIndicatorBounced_Effect", effectPosition, 0f, EffectStopType.Gracefully);
			if (m_bounceEffectTriggeredUnityEvent != null)
			{
				m_bounceEffectTriggeredUnityEvent.Invoke();
			}
		}
		DownstrikeKnockbackOverride component = colliderObj.GetComponent<DownstrikeKnockbackOverride>();
		float num = AttackBounceHeight;
		if (component != null)
		{
			num = component.Value;
		}
		BoundsObj component2 = colliderObj.GetComponent<BoundsObj>();
		if ((bool)component2)
		{
			float num2 = num * num / (2f * Mathf.Abs(-50f));
			float num3 = component2.Top - projectile.Owner.transform.position.y;
			if (TraitManager.IsTraitActive(TraitType.GainDownStrike))
			{
				num3 = 0f;
			}
			num = Mathf.Sqrt(2f * Mathf.Abs(-50f) * (num2 + num3));
		}
		m_triggerBounceInvincibility = true;
		if (num > m_highestBounceAmount)
		{
			m_highestBounceAmount = num;
			m_triggerBounce = true;
			m_onSuccessfulDownstrikeRelay.Dispatch(projectile, colliderObj);
		}
	}

	public void ForceTriggerBounce(float bounceHeight)
	{
		m_highestBounceAmount = bounceHeight;
		TriggerBounce(isDownstriking: false);
		DownstrikeProjectile_RL.ConsecutiveStrikes++;
	}

	private void TriggerBounce(bool isDownstriking)
	{
		m_triggerBounce = false;
		float highestBounceAmount = m_highestBounceAmount;
		m_highestBounceAmount = float.MinValue;
		if (_condition.CurrentState == CharacterStates.CharacterConditions.Stunned || _condition.CurrentState == CharacterStates.CharacterConditions.Dead)
		{
			return;
		}
		m_playerController.ControllerCorgi.GravityActive(state: true);
		if (m_playerController.CharacterClass.ClassType == ClassType.LuteClass)
		{
			m_playerController.Animator.SetBool("DanceBounce", value: true);
		}
		else
		{
			m_playerController.Animator.SetBool("Bounce", value: true);
		}
		if ((bool)m_firedProjectile)
		{
			m_firedProjectile.OnCollisionRelay.RemoveListener(m_bounce);
		}
		StopDownStrike();
		m_playerController.CharacterDash.StopDash();
		m_playerController.CharacterFlight.StopFlight();
		if (TraitManager.IsTraitActive(TraitType.GainDownStrike) && isDownstriking && (bool)m_firedProjectile && (bool)m_firedProjectile.HitboxController.LastCollidedWith && m_firedProjectile.HitboxController.LastCollidedWith.isActiveAndEnabled)
		{
			Vector3 vector = m_firedProjectile.HitboxController.LastCollidedWith.ClosestPoint(m_firedProjectile.Midpoint);
			float num = 0.5f;
			RaycastHit2D raycastHit2D = Physics2D.BoxCast(new Vector2(m_playerController.CollisionBounds.position.x, vector.y), m_playerController.CollisionBounds.size, 0f, Vector2.up, num, m_playerController.ControllerCorgi.PlatformMask);
			if ((bool)raycastHit2D)
			{
				num = raycastHit2D.distance;
			}
			if (num > 0f)
			{
				Vector3 localPosition = m_playerController.transform.localPosition;
				localPosition.y = vector.y + num;
				m_playerController.transform.localPosition = localPosition;
				m_playerController.ControllerCorgi.SetRaysParameters();
			}
		}
		m_playerController.SetVelocityY(highestBounceAmount, additive: false);
		_movement.ChangeState(CharacterStates.MovementStates.Jumping);
		if ((bool)m_playerController.CharacterJump)
		{
			m_playerController.CharacterJump.ResetBrakeForce();
			m_playerController.CharacterJump.StartJumpTime();
		}
		m_bounceLockTimer = BounceInputLockDuration;
		if ((ResetsDash || SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.UnlockBouncableDownstrike) > 0) && m_playerController.CharacterDash != null)
		{
			m_playerController.CharacterDash.ResetNumberOfDashes();
		}
		if (ResetsDoubleJump && m_playerController.CharacterJump != null)
		{
			m_playerController.CharacterJump.ResetNumberOfJumps();
		}
		m_playerController.CastAbility.GetAbility(CastAbilityType.Weapon).LockoutTimer += 0.175f;
		if (m_triggerBounceInvincibility)
		{
			m_triggerBounceInvincibility = false;
			StartCoroutine(DisableBodyHitboxCoroutine());
		}
		AudioManager.PlayOneShotAttached(this, "event:/SFX/Weapons/sfx_player_spinkick_bounce", base.gameObject);
		DownstrikeProjectile_RL.ConsecutiveStrikes++;
		RelicObj relic = SaveManager.PlayerSaveData.GetRelic(RelicType.SpinKickLeavesCaltrops);
		if (relic.Level > 0)
		{
			float num2 = Relic_EV.GetRelicMaxStack(relic.RelicType, relic.Level);
			if ((float)relic.IntValue >= num2)
			{
				ProjectileManager.FireProjectile(m_playerController.gameObject, m_relicCaltropDropProjectileName, new Vector2(0f, m_playerController.ControllerCorgi.BoundsHeight / 2f), matchFacing: true, 90f);
				relic.SetIntValue(0, additive: false);
				m_playerController.StartSpinKicksDropCaltropsTimer();
			}
		}
	}

	private IEnumerator DisableBodyHitboxCoroutine()
	{
		bool storedBodyHitboxState = m_playerController.HitboxController.GetCollider(HitboxType.Body);
		m_playerController.HitboxController.SetHitboxActiveState(HitboxType.Body, active: false);
		float delay = Time.time + 0.015f;
		while (Time.time < delay)
		{
			yield return null;
		}
		if (!LocalTeleporterController.IsTeleporting)
		{
			m_playerController.HitboxController.SetHitboxActiveState(HitboxType.Body, storedBodyHitboxState);
		}
	}

	protected void UpdateBounceLockTimer()
	{
		if (m_bounceLockTimer > 0f)
		{
			m_bounceLockTimer -= Time.deltaTime;
			if (m_bounceLockTimer <= 0f)
			{
				m_playerController.Animator.SetBool("Bounce", value: false);
				m_playerController.Animator.SetBool("DanceBounce", value: false);
			}
		}
	}

	protected override void InitializeAnimatorParameters()
	{
		RegisterAnimatorParameter("Kick_Down", AnimatorControllerParameterType.Bool);
		RegisterAnimatorParameter("Kick_DownForward", AnimatorControllerParameterType.Bool);
		RegisterAnimatorParameter("360Kick", AnimatorControllerParameterType.Trigger);
		RegisterAnimatorParameter("SpinKickTell_Anim_Speed", AnimatorControllerParameterType.Float);
		RegisterAnimatorParameter("SpinKickAttack_Anim_Speed", AnimatorControllerParameterType.Float);
		RegisterAnimatorParameter("SpinKickExit_Anim_Speed", AnimatorControllerParameterType.Float);
	}

	public override void UpdateAnimator()
	{
		if (!SpinKickInstead)
		{
			bool flag = (m_inputAngle > (float)ForwardKickMinMaxAngle.x && m_inputAngle < (float)DownKickMinMaxAngle.x) || (m_inputAngle > (float)DownKickMinMaxAngle.y && m_inputAngle < (float)ForwardKickMinMaxAngle.y);
			MMAnimator.UpdateAnimatorBool(_animator, "Kick_Down", _movement.CurrentState == CharacterStates.MovementStates.DownStriking && !flag, _character._animatorParameters);
			MMAnimator.UpdateAnimatorBool(_animator, "Kick_DownForward", _movement.CurrentState == CharacterStates.MovementStates.DownStriking && flag, _character._animatorParameters);
		}
	}

	protected void OnDestroy()
	{
		if ((bool)m_playerController && (bool)m_playerController.ControllerCorgi)
		{
			m_playerController.ControllerCorgi.OnCorgiLandedEnterRelay.RemoveListener(m_resetConsecutiveStrikes);
		}
	}

	private void ResetConsecutiveStrikes(CorgiController corgiController)
	{
		if (m_firedProjectile != null)
		{
			DownstrikeProjectile_RL.ConsecutiveStrikes = 0;
		}
	}
}
