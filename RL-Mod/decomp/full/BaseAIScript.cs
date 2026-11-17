using System;
using System.Collections;
using System.Linq;
using RLAudio;
using UnityEngine;

public abstract class BaseAIScript : MonoBehaviour, IHasProjectileNameArray, IAudioEventEmitter
{
	private const string ANIM_SPEED_STRING = "Anim_Speed";

	private const string ABILITY_ANIM_SPEED_STRING = "Ability_Anim_Speed";

	private const string DEATH_SCREEN_FLASH_AUDIO_PATH = "event:/SFX/Enemies/sfx_spellswordBoss_death_flash";

	protected static readonly int NEUTRAL_STATE = Animator.StringToHash("Neutral");

	private Animator m_animator;

	private LogicController m_logicController;

	private EnemyController m_enemyController;

	private Coroutine m_setContactDamageCoroutine;

	private bool m_isInitialized;

	private string m_description = string.Empty;

	[NonSerialized]
	protected string[] m_projectileNameArray;

	[NonSerialized]
	public int DefaultAnimationLayer;

	private WaitUntilAnimChanged_Yield m_waitAnimChangedYield;

	private WaitUntilAnimComplete_Yield m_waitAnimCompleteYield;

	private WaitRL_Yield m_waitYield;

	protected virtual Vector2 IdleDuration => new Vector2(0.35f, 0.75f);

	protected virtual Vector2 WalkTowardsDuration => new Vector2(0.4f, 0.9f);

	protected virtual Vector2 WalkAwayDuration => new Vector2(0.4f, 0.9f);

	protected virtual Vector2 RandomFollowOffsetX => new Vector2(3f, 8.5f);

	protected virtual Vector2 RandomFollowOffsetY => new Vector2(1.5f, 6f);

	protected virtual float IdleAnimSpeedMod => 1f;

	protected virtual float WalkAnimSpeedMod => 1f;

	protected virtual float ForcedIdleDuration_IfReversingDirection => 0.175f;

	public string[] ProjectileNameArray
	{
		get
		{
			if (m_projectileNameArray == null)
			{
				InitializeProjectileNameArray();
				if (m_projectileNameArray == null)
				{
					m_projectileNameArray = EnemySpawnController.EmptyProjectileNameArray_STATIC;
				}
			}
			return m_projectileNameArray;
		}
	}

	public string Description
	{
		get
		{
			if (m_description == string.Empty)
			{
				m_description = ToString();
			}
			return m_description;
		}
	}

	public bool IsPaused { get; private set; }

	public Animator Animator => m_animator;

	public LogicController LogicController => m_logicController;

	public EnemyController EnemyController => m_enemyController;

	public GameObject Target => m_enemyController.Target;

	public BaseCharacterController TargetController => m_enemyController.TargetController;

	public bool IsInitialized => m_isInitialized;

	public virtual bool ForceDeathAnimation => false;

	protected abstract void InitializeProjectileNameArray();

	public virtual void Initialize(EnemyController enemyController)
	{
		m_enemyController = enemyController;
		m_animator = m_enemyController.Animator;
		m_logicController = m_enemyController.LogicController;
		base.transform.localPosition = Vector3.zero;
		m_isInitialized = true;
	}

	protected virtual void OnDisable()
	{
	}

	public virtual void OnEnemyActivated()
	{
	}

	public virtual void Pause()
	{
		IsPaused = true;
		if (m_waitYield != null)
		{
			m_waitYield.Pause();
		}
	}

	public virtual void Unpause()
	{
		IsPaused = false;
		if (m_waitYield != null)
		{
			m_waitYield.Unpause();
		}
	}

	public virtual void OnLBCompleteOrCancelled()
	{
		EnemyController.ResetBaseValues();
		SetAttackingWithContactDamage(enable: false, 0f);
		if (AnimatorUtility.HasParameter(m_animator, "Anim_Speed"))
		{
			m_animator.SetFloat("Anim_Speed", 1f);
		}
		else if (AnimatorUtility.HasParameter(m_animator, "Ability_Anim_Speed"))
		{
			m_animator.SetFloat("Ability_Anim_Speed", 1f);
		}
		EnemyController.ResetTurnTrigger();
		Unpause();
	}

	public virtual void ResetScript()
	{
		LogicController.ForceExecuteLogicBlockName_OnceOnly = null;
		OnLBCompleteOrCancelled();
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[RestLogic]
	[WanderLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public virtual IEnumerator Idle()
	{
		yield return Idle(UnityEngine.Random.Range(IdleDuration.x, IdleDuration.y));
	}

	public virtual IEnumerator Idle(float idleDuration)
	{
		if (EnemyController.IsFlying || (!EnemyController.IsFlying && EnemyController.IsGrounded))
		{
			EnemyController.FlyingMovementType = FlyingMovementType.Stop;
			EnemyController.FollowTarget = true;
			if (!EnemyController.KnockedIntoAir)
			{
				SetVelocity(Vector2.zero, additive: false);
			}
		}
		this.SetAnimationSpeedMultiplier(IdleAnimSpeedMod);
		yield return Wait(idleDuration);
		this.SetAnimationSpeedMultiplier(1f);
		yield return null;
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[RestLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public virtual IEnumerator WalkTowards()
	{
		if (LogicController.PreviousLogicBlockName == "WalkAway")
		{
			yield return Idle(ForcedIdleDuration_IfReversingDirection);
		}
		this.SetAnimationSpeedMultiplier(WalkAnimSpeedMod);
		if (EnemyController.IsFlying || (!EnemyController.IsFlying && EnemyController.IsGrounded))
		{
			if (EnemyController.IsFlying)
			{
				EnemyController.FollowTarget = true;
				EnemyController.FlyingMovementType = FlyingMovementType.Towards;
				EnemyController.GenerateRandomFollowOffset(RandomFollowOffsetX, RandomFollowOffsetY);
			}
			else
			{
				bool flag = true;
				if ((EnemyController.IsTargetToMyRight && EnemyController.RightSidePlatformDropPrevented) || (!EnemyController.IsTargetToMyRight && EnemyController.LeftSidePlatformDropPrevented))
				{
					flag = false;
				}
				if (flag)
				{
					if (EnemyController.IsTargetToMyRight)
					{
						SetVelocity(EnemyController.ActualSpeed, 0f, additive: false);
					}
					else
					{
						SetVelocity(0f - EnemyController.ActualSpeed, 0f, additive: false);
					}
				}
				if (!EnemyController.AlwaysFacing)
				{
					this.FaceTarget();
				}
				if ((EnemyController.IsFacingRight && EnemyController.ControllerCorgi.State.IsCollidingRight && EnemyController.Velocity.x > 0f) || (!EnemyController.IsFacingRight && EnemyController.ControllerCorgi.State.IsCollidingLeft && EnemyController.Velocity.x < 0f))
				{
					EnemyController.SetVelocityX(0f, additive: false);
				}
			}
		}
		yield return Wait(UnityEngine.Random.Range(WalkTowardsDuration.x, WalkTowardsDuration.y));
		this.SetAnimationSpeedMultiplier(1f);
		yield return null;
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[RestLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public virtual IEnumerator WalkAway()
	{
		if (LogicController.PreviousLogicBlockName == "WalkTowards")
		{
			yield return Idle(ForcedIdleDuration_IfReversingDirection);
		}
		this.SetAnimationSpeedMultiplier(WalkAnimSpeedMod);
		if (EnemyController.IsFlying || (!EnemyController.IsFlying && EnemyController.IsGrounded))
		{
			if (EnemyController.IsFlying)
			{
				EnemyController.FollowTarget = true;
				EnemyController.FlyingMovementType = FlyingMovementType.Away;
			}
			else
			{
				bool flag = true;
				if ((!EnemyController.IsTargetToMyRight && EnemyController.RightSidePlatformDropPrevented) || (EnemyController.IsTargetToMyRight && EnemyController.LeftSidePlatformDropPrevented))
				{
					flag = false;
				}
				if (flag)
				{
					if (EnemyController.IsTargetToMyRight)
					{
						SetVelocity(0f - EnemyController.ActualSpeed, 0f, additive: false);
					}
					else
					{
						SetVelocity(EnemyController.ActualSpeed, 0f, additive: false);
					}
				}
				if (!EnemyController.AlwaysFacing)
				{
					this.FaceAwayFromTarget();
				}
			}
		}
		yield return Wait(UnityEngine.Random.Range(WalkAwayDuration.x, WalkAwayDuration.y));
		this.SetAnimationSpeedMultiplier(1f);
		yield return null;
	}

	public virtual IEnumerator DeathAnim()
	{
		if (EnemyController.IsBoss)
		{
			RLTimeScale.SetTimeScale(TimeScaleType.Cutscene, 0f);
			yield return Wait(0.25f, unscaledTime: true);
			PlayDeathAnimAudio();
			BossRoomController component = EnemyController.Room.gameObject.GetComponent<BossRoomController>();
			bool flag = ((!component) ? (EnemyManager.NumActiveEnemies == 0 && EnemyManager.NumActiveSummonedEnemies == 0) : component.AllBossesDeadOrDying);
			if ((bool)component && EnemyController.KilledByKnockout && EnemyController.IsDead && flag)
			{
				StoreAPIManager.GiveAchievement(AchievementType.BoxerBoss, StoreType.All);
				CameraController.PlayKOEffect(7f);
				yield return Wait(0.5f, unscaledTime: true);
			}
			else
			{
				EffectManager.PlayEffect(base.gameObject, m_animator, "ScreenFlashEffect", Vector3.zero, 0f, EffectStopType.Gracefully);
				yield return Wait(0.2f, unscaledTime: true);
				EffectManager.PlayEffect(base.gameObject, m_animator, "ScreenFlashEffect", Vector3.zero, 0f, EffectStopType.Gracefully);
				yield return Wait(0.2f, unscaledTime: true);
			}
			yield return Wait(0.5f, unscaledTime: true);
			RLTimeScale.SetTimeScale(TimeScaleType.Cutscene, 1f);
		}
	}

	protected virtual void PlayDeathAnimAudio()
	{
		AudioManager.PlayOneShot(this, "event:/SFX/Enemies/sfx_spellswordBoss_death_flash", base.gameObject.transform.position);
	}

	public virtual IEnumerator SpawnAnim()
	{
		yield break;
	}

	public virtual IEnumerator Default_TellIntroAndLoop(string tellIntroName, float tellIntroAnimSpeed, string tellHoldName, float tellHoldAnimSpeed, float totalDuration)
	{
		if (tellHoldAnimSpeed == 0f)
		{
			tellHoldAnimSpeed = 1f;
		}
		float startTime = Time.time;
		this.SetAnimationSpeedMultiplier(tellIntroAnimSpeed);
		yield return this.ChangeAnimationState(tellIntroName);
		yield return WaitUntilAnimComplete(DefaultAnimationLayer);
		float midElapsedTime = Time.time - startTime;
		float duration = totalDuration - midElapsedTime;
		this.SetAnimationSpeedMultiplier(tellHoldAnimSpeed);
		yield return this.ChangeAnimationState(tellHoldName);
		if (midElapsedTime < totalDuration && duration > 0f)
		{
			yield return Wait(duration);
		}
	}

	public virtual IEnumerator Default_Animation(string animName, float animSpeed, float delay, bool waitUntilAnimationComplete = true)
	{
		this.SetAnimationSpeedMultiplier(animSpeed);
		yield return this.ChangeAnimationState(animName);
		if (waitUntilAnimationComplete)
		{
			yield return WaitUntilAnimComplete(DefaultAnimationLayer);
		}
		if (delay > 0f)
		{
			yield return Wait(delay);
		}
	}

	public virtual IEnumerator Default_Animation(int animID, float animSpeed, float delay, bool waitUntilAnimationComplete = true)
	{
		this.SetAnimationSpeedMultiplier(animSpeed);
		yield return this.ChangeAnimationState(animID);
		if (waitUntilAnimationComplete)
		{
			yield return WaitUntilAnimComplete(DefaultAnimationLayer);
		}
		if (delay > 0f)
		{
			yield return Wait(delay);
		}
	}

	public virtual IEnumerator Default_Attack_Cooldown(float idleDuration, float attackCD)
	{
		yield return this.ChangeAnimationState(NEUTRAL_STATE);
		float num = BurdenManager.GetBurdenStatGain(BurdenType.EnemyAggression);
		if (EnemyController.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_Speed) || EnemyController.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_ArmorShred))
		{
			num += 0.25f;
		}
		num = Mathf.Max(1f - num, 0.25f);
		idleDuration *= num;
		if (idleDuration > 0f)
		{
			yield return Idle(idleDuration);
		}
		if (attackCD > 0f)
		{
			this.TriggerAttackCooldown(attackCD, ignoreMods: false);
		}
		this.TriggerRestState(ignoreMods: false);
	}

	public virtual void StopAndFaceTarget()
	{
		this.FaceTarget();
		if (EnemyController.IsFlying)
		{
			EnemyController.FlyingMovementType = FlyingMovementType.Stop;
			SetVelocity(Vector2.zero, additive: false);
		}
		else if (EnemyController.IsGrounded)
		{
			SetVelocityX(0f, additive: false);
		}
	}

	public void SetVelocity(Vector2 velocity, bool additive)
	{
		m_enemyController.SetVelocity(velocity.x, velocity.y, additive);
	}

	public void SetVelocity(float x, float y, bool additive)
	{
		m_enemyController.SetVelocity(x, y, additive);
	}

	public void SetVelocityX(float velocity, bool additive)
	{
		m_enemyController.SetVelocityX(velocity, additive);
	}

	public void SetVelocityY(float velocity, bool additive)
	{
		m_enemyController.SetVelocityY(velocity, additive);
	}

	public void Flip()
	{
		m_enemyController.CharacterCorgi.Flip();
	}

	public Vector3 GetRelativeSpawnPositionAtIndex(int spawnPosIndex, bool matchFacing)
	{
		Vector3 spawnPosition = EnemyController.SpawnPositionController.GetSpawnPosition(spawnPosIndex);
		spawnPosition = EnemyController.transform.InverseTransformPoint(spawnPosition);
		spawnPosition *= EnemyController.BaseScaleToOffsetWith;
		if (!EnemyController.IsFacingRight && matchFacing)
		{
			spawnPosition.x = 0f - spawnPosition.x;
		}
		return spawnPosition;
	}

	public Vector3 GetAbsoluteSpawnPositionAtIndex(int spawnPosIndex, bool matchFacing)
	{
		Vector3 spawnPosition = EnemyController.SpawnPositionController.GetSpawnPosition(spawnPosIndex);
		if (!EnemyController.IsFacingRight && matchFacing)
		{
			spawnPosition.x = 0f - spawnPosition.x;
		}
		return spawnPosition;
	}

	public void RemoveStatusEffects(bool includeCommanderBuffs)
	{
		if (includeCommanderBuffs)
		{
			EnemyController.StatusEffectController.StopAllStatusEffects(interrupted: false);
			return;
		}
		StatusEffectType[] typeArray = StatusEffectType_RL.TypeArray;
		foreach (StatusEffectType statusEffectType in typeArray)
		{
			if (!StatusEffect_EV.COMMANDER_STATUS_EFFECT_ARRAY.Contains(statusEffectType))
			{
				EnemyController.StatusEffectController.StopStatusEffect(statusEffectType, interrupted: false);
			}
		}
	}

	protected void SetAttackingWithContactDamage(bool enable, float delay = 0.1f)
	{
		if (m_setContactDamageCoroutine != null)
		{
			StopCoroutine(m_setContactDamageCoroutine);
			m_setContactDamageCoroutine = null;
		}
		if (!enable)
		{
			EnemyController.AttackingWithContactDamage = enable;
		}
		else
		{
			m_setContactDamageCoroutine = StartCoroutine(AttackingWithContactDamageCoroutine(delay));
		}
	}

	protected IEnumerator AttackingWithContactDamageCoroutine(float delay)
	{
		delay += Time.time;
		while (Time.time < delay)
		{
			yield return null;
		}
		EnemyController.AttackingWithContactDamage = true;
	}

	protected void StopProjectile(ref Projectile_RL projectile)
	{
		if ((bool)projectile && projectile.isActiveAndEnabled && projectile.OwnerController == m_enemyController)
		{
			projectile.FlagForDestruction();
		}
		projectile = null;
	}

	public IEnumerator WaitUntilAnimChanged(string animatorBoolName, int animLayer)
	{
		if (m_waitAnimChangedYield == null)
		{
			m_waitAnimChangedYield = new WaitUntilAnimChanged_Yield(Animator, animatorBoolName, (AnimationLayer)animLayer);
		}
		else
		{
			m_waitAnimChangedYield.CreateNew(Animator, animatorBoolName, (AnimationLayer)animLayer);
		}
		yield return m_waitAnimChangedYield;
	}

	public IEnumerator WaitUntilAnimComplete(int animLayer)
	{
		if (m_waitAnimCompleteYield == null)
		{
			m_waitAnimCompleteYield = new WaitUntilAnimComplete_Yield(Animator, animLayer);
		}
		else
		{
			m_waitAnimCompleteYield.CreateNew(Animator, animLayer);
		}
		yield return m_waitAnimCompleteYield;
	}

	public IEnumerator WaitUntilIsGrounded()
	{
		while (!EnemyController.IsGrounded)
		{
			yield return null;
		}
		EnemyController.ResetTurnTrigger();
	}

	protected IEnumerator Wait(float seconds, bool unscaledTime = false)
	{
		if (EnemyController.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_Speed))
		{
			seconds /= 1.25f;
		}
		if (m_waitYield == null)
		{
			m_waitYield = new WaitRL_Yield(seconds, unscaledTime);
		}
		else
		{
			m_waitYield.CreateNew(seconds, unscaledTime);
		}
		if (IsPaused)
		{
			m_waitYield.Pause();
		}
		yield return m_waitYield;
	}

	protected IEnumerator Wait(float minSeconds, float maxSeconds, bool unscaledTime = false)
	{
		float waitTime = UnityEngine.Random.Range(minSeconds, maxSeconds);
		if (m_waitYield == null)
		{
			m_waitYield = new WaitRL_Yield(waitTime, unscaledTime);
		}
		else
		{
			m_waitYield.CreateNew(waitTime, unscaledTime);
		}
		yield return m_waitYield;
	}
}
