using System;
using System.Collections;
using FMOD.Studio;
using RLAudio;
using UnityEngine;

public class Zombie_Basic_AIScript : BaseAIScript
{
	private const float HIDE_DELAY = 3f;

	private const float RISE_DELAY = 0.5f;

	private float m_digDownDelay;

	private float m_digUpDelay;

	private Coroutine m_digMoveEffectCoroutine;

	private EventInstance m_tunnelAudioEventInstance;

	private BaseEffect m_colorTrailEffect;

	private BaseEffect m_enemiesCensoredEffect;

	private Action<object, EventArgs> m_onZombieHit;

	protected float m_swing_TellIntro_AnimationSpeed = 1f;

	protected float m_swing_TellHold_AnimationSpeed = 1f;

	protected float m_swing_TellIntroAndHold_Delay;

	protected float m_swing_AttackIntro_AnimationSpeed = 0.5f;

	protected float m_swing_AttackIntro_Delay = 0.35f;

	protected float m_swing_AttackHold_AnimationSpeed = 1.25f;

	protected float m_swing_AttackHold_Delay;

	protected float m_swing_Exit_AnimationSpeed = 0.65f;

	protected float m_swing_Exit_Delay;

	protected float m_swing_Exit_ForceIdle = 0.15f;

	private bool m_isHidden;

	protected float m_digDash_TellIntro_AnimationSpeed = 1f;

	protected float m_digDash_TellHold_AnimationSpeed = 1f;

	protected float m_digDash_TellIntroAndHold_Delay;

	protected float m_digDash_AttackIntro_AnimationSpeed = 0.5f;

	protected float m_digDash_AttackIntro_Delay = 0.35f;

	protected float m_digDash_AttackHold_AnimationSpeed = 1.25f;

	protected float m_digDash_AttackHold_Delay;

	protected float m_digDash_Attack_RevealDelay = 0.65f;

	protected float m_digDash_Exit_AnimationSpeed = 0.65f;

	protected float m_digDash_Exit_Delay = 0.65f;

	protected float m_digDash_Attack_Duration = 1.25f;

	protected float m_digDash_Attack_MoveSpeedOverride = 12f;

	protected float m_digDash_Exit_ForceIdle = 0.15f;

	protected float m_digDash_AttackCD = 1f;

	protected const string LUNGE_TELL_INTRO = "Lunge_Tell_Intro";

	protected const string LUNGE_TELL_HOLD = "Lunge_Tell_Hold";

	protected const string LUNGE_ATTACK_INTRO = "Lunge_Attack_Intro";

	protected const string LUNGE_ATTACK_HOLD = "Lunge_Attack_Hold";

	protected const string LUNGE_EXIT = "Lunge_Exit";

	protected const string LUNGE_PROJECTILE = "ZombieLungeProjectile";

	protected override Vector2 IdleDuration => new Vector2(0.35f, 0.55f);

	protected override Vector2 WalkTowardsDuration => new Vector2(0.5f, 1.05f);

	protected override Vector2 WalkAwayDuration => new Vector2(0.5f, 0.5f);

	protected virtual float DigDownAnimSpeed => 1.7f;

	protected virtual float DigUpAnimSpeed => 1.7f;

	protected virtual float DigDown_IdleDelay => 0.2f;

	protected virtual float DigUp_IdleDelay => 0.2f;

	protected virtual float m_tunnel_moveSpeed => 0f;

	protected virtual Vector2 swing_Dash_AttackSpeed => new Vector2(14f, 0f);

	protected virtual float m_swing_Dash_AttackTime => 0.5f;

	protected virtual float m_swing_AttackCD => 2.5f;

	protected bool IsHidden
	{
		get
		{
			return m_isHidden;
		}
		set
		{
			m_isHidden = value;
		}
	}

	protected virtual float Lunge_TellIntro_AnimSpeed => 1f;

	protected virtual float Lunge_TellHold_AnimSpeed => 1.25f;

	protected virtual float Lunge_TellHold_Duration => 0.7f;

	protected virtual float Lunge_TellHold_DurationShort => 0.35f;

	protected virtual float Lunge_AttackIntro_AnimSpeed => 2f;

	protected virtual float Lunge_AttackIntro_Delay => 0f;

	protected virtual float Lunge_AttackHold_AnimSpeed => 1f;

	protected virtual float Lunge_AttackHold_Delay => 0f;

	protected virtual float Lunge_Exit_AnimSpeed => 1.25f;

	protected virtual float Lunge_Exit_Duration => 0f;

	protected virtual float Lunge_Exit_ForceIdle => 0.15f;

	protected virtual float Lunge_Exit_AttackCD => 0.75f;

	protected virtual float Lunge_Count => 1f;

	protected virtual float Delay_Between_Lunges => 0f;

	protected virtual Vector2 Lunge_Power => new Vector2(20f, 9f);

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[2] { "ZombiePassBoltMinibossProjectile", "ZombieLungeProjectile" };
	}

	private void Awake()
	{
		m_onZombieHit = OnZombieHit;
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		base.LogicController.DisableDamageDuringInitialDelay = false;
		ResetZombie();
		base.EnemyController.CharacterHitResponse.OnCharacterHitRelay.AddListener(m_onZombieHit);
		base.EnemyController.StatusEffectController.SetAllStatusEffectsHidden(hide: true);
		base.EnemyController.InvisibleDuringSummonAnim = true;
		if (!m_tunnelAudioEventInstance.isValid())
		{
			m_tunnelAudioEventInstance = AudioUtility.GetEventInstance("event:/SFX/Enemies/sfx_enemy_zombie_burrow_move_loop", base.transform);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		ResetZombie();
		if ((bool)m_colorTrailEffect && m_colorTrailEffect.isActiveAndEnabled && m_colorTrailEffect.Source == base.EnemyController.gameObject)
		{
			m_colorTrailEffect.Stop(EffectStopType.Immediate);
		}
		DisableEnemiesCensoredEffect();
	}

	private void OnEnable()
	{
		if (base.IsInitialized)
		{
			ResetZombie();
		}
	}

	private void OnDestroy()
	{
		if (!GameManager.IsApplicationClosing && (bool)base.EnemyController)
		{
			base.EnemyController.CharacterHitResponse.OnCharacterHitRelay.RemoveListener(m_onZombieHit);
		}
		if (m_tunnelAudioEventInstance.isValid())
		{
			m_tunnelAudioEventInstance.release();
		}
	}

	private void OnZombieHit(object sender, EventArgs args)
	{
		m_digDownDelay = Time.time + 3f;
	}

	private void ResetZombie()
	{
		if (!GameManager.IsApplicationClosing && base.IsInitialized)
		{
			bool num = EffectManager.AnimatorEffectsDisabled(base.Animator);
			EffectManager.RemoveAnimatorFromDisableList(base.Animator);
			base.EnemyController.Animator.Play("Hidden", 0, 1f);
			base.EnemyController.Animator.Update(0f);
			base.EnemyController.HitboxController.SetHitboxActiveState(HitboxType.Weapon, active: false);
			base.EnemyController.HitboxController.SetHitboxActiveState(HitboxType.Body, active: false);
			base.EnemyController.StatusEffectController.SetAllStatusEffectsHidden(hide: true);
			base.EnemyController.Visuals.SetActive(value: false);
			IsHidden = true;
			if (num)
			{
				EffectManager.AddAnimatorToDisableList(base.Animator);
			}
			if (m_tunnelAudioEventInstance.isValid())
			{
				m_tunnelAudioEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			}
		}
	}

	public override IEnumerator Idle()
	{
		if (!IsHidden && Time.time >= m_digDownDelay)
		{
			yield return DigDown();
		}
		yield return base.Idle();
	}

	public override IEnumerator WalkTowards()
	{
		if (IsHidden)
		{
			if (Time.time >= m_digUpDelay && base.LogicController.CurrentRangeState == LogicState.Close)
			{
				yield return DigUp();
			}
			else if (m_tunnel_moveSpeed > 0f)
			{
				AudioManager.PlayAttached(this, m_tunnelAudioEventInstance, base.gameObject);
				m_digMoveEffectCoroutine = this.RunPersistentCoroutine(DigMoveEffectCoroutine());
				yield return base.WalkTowards();
				this.StopPersistentCoroutine(m_digMoveEffectCoroutine);
				AudioManager.Stop(m_tunnelAudioEventInstance);
			}
			else
			{
				yield return base.Idle();
			}
		}
		else
		{
			yield return base.WalkTowards();
		}
	}

	public override IEnumerator WalkAway()
	{
		if (IsHidden && m_tunnel_moveSpeed > 0f)
		{
			AudioManager.PlayAttached(this, m_tunnelAudioEventInstance, base.gameObject);
			m_digMoveEffectCoroutine = this.RunPersistentCoroutine(DigMoveEffectCoroutine());
			yield return base.WalkTowards();
			this.StopPersistentCoroutine(m_digMoveEffectCoroutine);
			AudioManager.Stop(m_tunnelAudioEventInstance);
		}
		else
		{
			yield return base.WalkAway();
		}
	}

	protected virtual IEnumerator DigDown()
	{
		SetVelocity(Vector2.zero, additive: false);
		if (!IsHidden)
		{
			base.EnemyController.BaseKnockbackDefense = 99999f;
			base.EnemyController.HitboxController.SetHitboxActiveState(HitboxType.Weapon, active: false);
			IsHidden = true;
			SetVelocity(Vector2.zero, additive: false);
			this.SetAnimationSpeedMultiplier(DigDownAnimSpeed);
			yield return this.ChangeAnimationState("DigDown");
			yield return WaitUntilAnimComplete(DefaultAnimationLayer);
			base.EnemyController.StatusEffectController.SetAllStatusEffectsHidden(hide: true);
			base.EnemyController.BaseKnockbackDefense = base.EnemyController.EnemyData.KnockbackDefence;
			base.EnemyController.HitboxController.SetHitboxActiveState(HitboxType.Body, active: false);
			base.EnemyController.Visuals.SetActive(value: false);
			if (DigDown_IdleDelay > 0f)
			{
				yield return Wait(DigDown_IdleDelay);
			}
			m_digUpDelay = Time.time + 0.5f;
			FadeOutColorTrail();
			DisableEnemiesCensoredEffect();
		}
		yield return Default_Attack_Cooldown(m_digDash_Exit_ForceIdle, m_digDash_AttackCD);
	}

	protected virtual IEnumerator DigUp()
	{
		if (IsHidden)
		{
			if (TraitManager.IsTraitActive(TraitType.EnemiesCensored))
			{
				DisableEnemiesCensoredEffect();
				m_enemiesCensoredEffect = EnemiesCensored_Trait.ApplyCensoredEffect(base.EnemyController);
			}
			base.EnemyController.HitboxController.SetHitboxActiveState(HitboxType.Body, active: true);
			IsHidden = false;
			base.EnemyController.Visuals.SetActive(value: true);
			base.EnemyController.BaseKnockbackDefense = 99999f;
			SetVelocity(Vector2.zero, additive: false);
			float multiplier = DigUpAnimSpeed;
			if (!base.EnemyController.IsGrounded)
			{
				multiplier = 5f;
			}
			this.SetAnimationSpeedMultiplier(multiplier);
			yield return this.ChangeAnimationState("DigUp");
			yield return WaitUntilAnimComplete(DefaultAnimationLayer);
			base.EnemyController.StatusBarController.ResetPositionAndScale(base.EnemyController);
			base.EnemyController.StatusEffectController.SetAllStatusEffectsHidden(hide: false);
			base.EnemyController.BaseKnockbackDefense = base.EnemyController.EnemyData.KnockbackDefence;
			base.EnemyController.HitboxController.SetHitboxActiveState(HitboxType.Weapon, active: true);
			if (DigUp_IdleDelay > 0f)
			{
				yield return Wait(DigUp_IdleDelay);
			}
			m_digDownDelay = Time.time + 3f;
			if (TraitManager.IsTraitActive(TraitType.ColorTrails))
			{
				FadeOutColorTrail();
				m_colorTrailEffect = EffectManager.PlayEffect(base.EnemyController.gameObject, base.EnemyController.Animator, "ColorTrails_Trait_Effect", base.EnemyController.Midpoint, 0f, EffectStopType.Gracefully);
				TrailRenderer componentInChildren = m_colorTrailEffect.GetComponentInChildren<TrailRenderer>();
				componentInChildren.startColor = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
				componentInChildren.endColor = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
				componentInChildren.widthMultiplier = 1.5f;
				componentInChildren.time = 18f;
				m_colorTrailEffect.transform.SetParent(base.EnemyController.gameObject.transform, worldPositionStays: true);
			}
		}
	}

	public override void OnLBCompleteOrCancelled()
	{
		base.EnemyController.BaseKnockbackDefense = base.EnemyController.EnemyData.KnockbackDefence;
		base.EnemyController.LockFlip = false;
		base.EnemyController.DisableFriction = false;
		this.StopPersistentCoroutine(m_digMoveEffectCoroutine);
		base.OnLBCompleteOrCancelled();
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[RestLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Swing_Attack()
	{
		if (IsHidden)
		{
			yield return DigUp();
		}
		StopAndFaceTarget();
		yield return Default_TellIntroAndLoop("Swing_Tell_Intro", m_swing_TellIntro_AnimationSpeed, "Swing_Tell_Hold", m_swing_TellHold_AnimationSpeed, m_swing_TellIntroAndHold_Delay);
		yield return Default_Animation("Swing_Attack_Intro", m_swing_AttackIntro_AnimationSpeed, m_swing_AttackIntro_Delay);
		yield return Default_Animation("Swing_Attack_Hold", m_swing_AttackHold_AnimationSpeed, m_swing_AttackHold_Delay, waitUntilAnimationComplete: false);
		Single_Action_Dash(swing_Dash_AttackSpeed.x, swing_Dash_AttackSpeed.y);
		if (m_swing_Dash_AttackTime > 0f)
		{
			yield return Wait(m_swing_Dash_AttackTime);
		}
		SetVelocityX(0f, additive: false);
		yield return Default_Animation("Swing_Exit", m_swing_Exit_AnimationSpeed, m_swing_Exit_Delay);
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_swing_Exit_ForceIdle, m_swing_AttackCD);
	}

	public override IEnumerator Default_Attack_Cooldown(float idleDuration, float attackCD)
	{
		float num = BurdenManager.GetBurdenStatGain(BurdenType.EnemyAggression);
		if (base.EnemyController.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_Speed) || base.EnemyController.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_ArmorShred))
		{
			num += 0.25f;
		}
		num = Mathf.Max(1f - num, 0f);
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

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[RestLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Tunnel_WalkTowards()
	{
		if (!IsHidden)
		{
			yield return DigDown();
		}
		base.EnemyController.BaseSpeed = m_tunnel_moveSpeed;
		AudioManager.PlayAttached(this, m_tunnelAudioEventInstance, base.gameObject);
		m_digMoveEffectCoroutine = this.RunPersistentCoroutine(DigMoveEffectCoroutine());
		yield return base.WalkTowards();
		this.StopPersistentCoroutine(m_digMoveEffectCoroutine);
		AudioManager.Stop(m_tunnelAudioEventInstance);
		base.EnemyController.ResetBaseValues();
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[RestLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Tunnel_WalkAway()
	{
		if (!IsHidden)
		{
			yield return DigDown();
		}
		base.EnemyController.BaseSpeed = m_tunnel_moveSpeed;
		AudioManager.PlayAttached(this, m_tunnelAudioEventInstance, base.gameObject);
		m_digMoveEffectCoroutine = this.RunPersistentCoroutine(DigMoveEffectCoroutine());
		yield return base.WalkAway();
		this.StopPersistentCoroutine(m_digMoveEffectCoroutine);
		AudioManager.Stop(m_tunnelAudioEventInstance);
		base.EnemyController.ResetBaseValues();
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[RestLogic]
	[MinibossEnemy]
	public IEnumerator Dig_Dash()
	{
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		base.EnemyController.FollowOffset = Vector3.zero;
		base.EnemyController.BaseTurnSpeed = 900f;
		base.EnemyController.FollowTarget = true;
		base.EnemyController.LockFlip = false;
		if (IsHidden)
		{
			yield return DigUp();
		}
		if (m_digDash_Attack_RevealDelay > 0f)
		{
			yield return Wait(m_digDash_Attack_RevealDelay);
		}
		yield return Default_TellIntroAndLoop("Swing_Tell_Intro", m_digDash_TellIntro_AnimationSpeed, "Swing_Tell_Hold", m_digDash_TellHold_AnimationSpeed, m_digDash_TellIntroAndHold_Delay);
		yield return Default_Animation("Swing_Attack_Intro", m_digDash_AttackIntro_AnimationSpeed, m_digDash_AttackIntro_Delay);
		base.EnemyController.FlyingMovementType = FlyingMovementType.Towards;
		base.EnemyController.BaseSpeed = m_digDash_Attack_MoveSpeedOverride;
		if (base.LogicController.EnemyLogicType == EnemyLogicType.Miniboss)
		{
			this.FireProjectile("ZombiePassBoltMinibossProjectile", 0, matchFacing: true, 0f, 0f);
		}
		base.EnemyController.BaseTurnSpeed = 0f;
		base.EnemyController.FollowTarget = false;
		base.EnemyController.LockFlip = true;
		yield return Default_Animation("Swing_Attack_Hold", m_digDash_AttackHold_AnimationSpeed, m_digDash_AttackHold_Delay, waitUntilAnimationComplete: false);
		if (m_digDash_Attack_Duration > 0f)
		{
			yield return Wait(m_digDash_Attack_Duration);
		}
		if (base.LogicController.EnemyLogicType == EnemyLogicType.Miniboss)
		{
			this.FireProjectile("ZombiePassBoltMinibossProjectile", 0, matchFacing: true, 0f, 0f);
		}
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		base.EnemyController.BaseSpeed = base.EnemyController.EnemyData.Speed;
		yield return Default_Animation("Swing_Exit", m_digDash_Exit_AnimationSpeed, m_digDash_Exit_Delay);
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_digDash_Exit_ForceIdle, m_digDash_AttackCD);
	}

	private IEnumerator DigMoveEffectCoroutine()
	{
		float smokeDelayInterval = Time.time + 0.15f;
		while (true)
		{
			if (Time.time >= smokeDelayInterval)
			{
				smokeDelayInterval = Time.time + 0.15f;
				EffectManager.PlayEffect(base.EnemyController.gameObject, null, "DustCloudTinyParticles_Effect", base.EnemyController.transform.localPosition, 0f, EffectStopType.Gracefully);
			}
			yield return null;
		}
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Lunge_Attack()
	{
		if (IsHidden)
		{
			yield return Idle();
			yield break;
		}
		for (int i = 0; (float)i < Lunge_Count; i++)
		{
			base.EnemyController.LockFlip = false;
			StopAndFaceTarget();
			base.EnemyController.LockFlip = true;
			if (i == 0)
			{
				yield return Default_TellIntroAndLoop("Lunge_Tell_Intro", Lunge_TellIntro_AnimSpeed, "Lunge_Tell_Hold", Lunge_TellHold_AnimSpeed, Lunge_TellHold_Duration);
			}
			else
			{
				yield return Default_TellIntroAndLoop("Lunge_Tell_Intro", Lunge_TellIntro_AnimSpeed, "Lunge_Tell_Hold", Lunge_TellHold_AnimSpeed, Lunge_TellHold_DurationShort);
			}
			yield return Default_Animation("Lunge_Attack_Intro", Lunge_AttackIntro_AnimSpeed, Lunge_AttackIntro_Delay);
			base.EnemyController.DisableFriction = true;
			if (base.EnemyController.IsFacingRight)
			{
				SetVelocityX(Lunge_Power.x, additive: false);
			}
			else
			{
				SetVelocityX(0f - Lunge_Power.x, additive: false);
			}
			SetVelocityY(Lunge_Power.y, additive: false);
			this.FireProjectile("ZombieLungeProjectile", 1);
			yield return Default_Animation("Lunge_Attack_Hold", Lunge_AttackHold_AnimSpeed, Lunge_AttackHold_Delay);
			base.EnemyController.DisableFriction = false;
			yield return WaitUntilIsGrounded();
			base.EnemyController.SetVelocityX(0f, additive: false);
			if (Delay_Between_Lunges > 0f)
			{
				yield return Wait(Delay_Between_Lunges);
			}
		}
		yield return Default_Animation("Lunge_Exit", Lunge_Exit_AnimSpeed, Lunge_Exit_Duration);
		base.EnemyController.LockFlip = false;
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(Lunge_Exit_ForceIdle, Lunge_Exit_AttackCD);
	}

	public void Single_Action_Dash(float dashXVelocity, float dashYVelocity)
	{
		if (base.EnemyController.IsFacingRight)
		{
			SetVelocityX(dashXVelocity, additive: false);
		}
		else
		{
			SetVelocityX(0f - dashXVelocity, additive: false);
		}
		SetVelocityY(dashYVelocity, additive: false);
	}

	private void FadeOutColorTrail()
	{
		if ((bool)m_colorTrailEffect && m_colorTrailEffect.isActiveAndEnabled && m_colorTrailEffect.Source == base.EnemyController.gameObject)
		{
			m_colorTrailEffect.transform.SetParent(null, worldPositionStays: true);
			StartCoroutine(FadeOutColorTrailCoroutine(0.1f));
		}
	}

	private void DisableEnemiesCensoredEffect()
	{
		if ((bool)m_enemiesCensoredEffect && m_enemiesCensoredEffect.isActiveAndEnabled && m_enemiesCensoredEffect.Source == base.EnemyController.gameObject)
		{
			m_enemiesCensoredEffect.Stop(EffectStopType.Immediate);
		}
		m_enemiesCensoredEffect = null;
	}

	private IEnumerator FadeOutColorTrailCoroutine(float duration)
	{
		FadeOutTrailRenderer fadeOut = m_colorTrailEffect.GetComponent<FadeOutTrailRenderer>();
		float delay = Time.time + duration;
		while (Time.time < delay)
		{
			float opacity = (delay - Time.time) / duration;
			fadeOut.SetOpacity(opacity);
			yield return null;
		}
		fadeOut.SetOpacity(0f);
		m_colorTrailEffect.Stop(EffectStopType.Immediate);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator ForceDigUpFast()
	{
		yield return DigUp();
	}

	private void LateUpdate()
	{
		if (base.IsInitialized && (bool)base.EnemyController && IsHidden && !base.EnemyController.IsGrounded)
		{
			base.LogicController.StopAllLogic();
			base.LogicController.ForceExecuteLogicBlockName_OnceOnly = "ForceDigUpFast";
		}
	}
}
