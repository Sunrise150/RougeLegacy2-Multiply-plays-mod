using System;
using System.Collections;
using FMOD.Studio;
using RLAudio;
using UnityEngine;

public class MimicChest_Basic_AIScript : BaseAIScript
{
	protected float m_death_AnimationSpeed = 1f;

	protected float m_death_AnimationDelay = 0.5f;

	protected float m_airborneKO_AnimationSpeed = 1f;

	protected float m_groundedKOAnimationSpeed = 1f;

	protected float m_groundedKOAnimationDelay = 0.5f;

	protected bool m_playDeathParticlesOnZeroHP = true;

	protected float m_deathParticleDuration = 0.5f;

	private const string DEATH_POOF_EFFECT = "EnemyGenericDeathPuff_Small_Effect_MediumVariant";

	private Color m_deathTintColor = new Color(0f, 0f, 0f, 0.5f);

	protected bool m_isDashAttacking;

	private MaterialPropertyBlock m_matPropertyBlock;

	private bool m_playerWasInDashWakeRange;

	private bool m_wasGrounded = true;

	private EventInstance m_dashEventInstance;

	private Action<object, CharacterHitEventArgs> m_onPlayerHit;

	protected float m_verticalShot_TellHold_AnimationSpeed = 1.25f;

	protected float m_verticalShot_TellHold_Delay = 1.25f;

	protected float m_verticalShot_AttackIntro_AnimationSpeed = 1.25f;

	protected float m_verticalShot_AttackIntro_Delay;

	protected float m_verticalShot_AttackHold_AnimationSpeed = 1.25f;

	protected float m_verticalShot_AttackHold_Delay = 0.25f;

	protected float m_verticalShot_Exit_AnimationSpeed = 0.65f;

	protected float m_verticalShot_Exit_Delay;

	protected float m_verticalShot_Exit_ForceIdle = 0.15f;

	protected float m_verticalShot_Exit_AttackCD = 10f;

	protected float m_dashAttack_TellIntro_AnimationSpeed = 1.25f;

	protected float m_dashAttack_TellHold_AnimationSpeed = 1.25f;

	protected float m_dashAttack_TellIntroAndHold_Delay = 0.75f;

	protected float m_dashAttack_AttackIntro_AnimationSpeed = 1.25f;

	protected float m_dashAttack_AttackIntro_Delay;

	protected float m_dashAttack_AttackHold_AnimationSpeed = 1.25f;

	protected float m_dashAttack_AttackHold_Delay = 0.15f;

	protected float m_dashAttack_Exit_AnimationSpeed = 1.25f;

	protected float m_dashAttack_Exit_Delay;

	protected float m_dashAttack_Exit_ForceIdle = 0.25f;

	public override bool ForceDeathAnimation => true;

	protected virtual string JumpAudioEventPath => "event:/SFX/Enemies/sfx_enemy_mimicChest_jump";

	protected virtual string LandAudioEventPath => "event:/SFX/Enemies/sfx_enemy_mimicChest_land";

	protected virtual string DeathHitAudioEventPath => "event:/SFX/Enemies/sfx_enemy_mimicChest_death_killingBlow";

	private string DashLoopAudioEventPath => "event:/SFX/Enemies/sfx_enemy_mimicChest_dash_start_loop";

	private string ChangeDirectionAudioEventPath => "event:/SFX/Enemies/sfx_enemy_mimicChest_dash_changeDirection";

	protected virtual Vector2 JumpHeight => new Vector2(17f, 17f);

	protected virtual int NumCoinsFiredOnHit => 0;

	protected virtual Vector2 CoinFireAngle_OnHit => new Vector2(80f, 100f);

	protected virtual Vector2 CoinFireSpeed_OnHit => new Vector2(1.25f, 1.65f);

	protected virtual int NumCoinsFiredOnLanding => 0;

	protected virtual Vector2 CoinFireAngle_OnLand => new Vector2(80f, 100f);

	protected virtual Vector2 CoinFireSpeed_OnLand => new Vector2(1.25f, 1.65f);

	protected virtual int NumCoinsFiredOnDeath => 5;

	protected virtual Vector2 CoinFireAngle_OnDeath => new Vector2(68f, 112f);

	protected virtual Vector2 CoinFireSpeed_OnDeath => new Vector2(0.75f, 1f);

	protected virtual int DeathCoinAttack_CoinSpreadAmount => 1;

	protected virtual float DeathCoinAttack_AngleSpread => 18f;

	protected virtual bool m_advancedBoss => false;

	protected virtual float m_airborneKO_AnimationDelay => 0.65f;

	protected virtual string m_verticalShotProjectileName => "MimicCoinProjectile";

	protected virtual int m_verticalShot_TotalShotSpread => 2;

	protected virtual int m_verticalShot_TotalLoops => 5;

	protected virtual int m_verticalShot_InitialAngle => 90;

	protected virtual Vector2 m_verticalShot_RandomAngleAngleOffset => new Vector2(-21f, 3f);

	protected virtual float m_verticalShot_LoopDelay => 0.75f;

	protected virtual float m_verticalShot_SpeedMod => 1f;

	protected virtual float m_dashAttack_Exit_AttackCD => 6f;

	protected virtual float m_dashAttackSpeed => 22f;

	protected virtual Vector2 m_dashAttackDuration => new Vector2(2f, 2f);

	protected virtual bool m_canDashWake => false;

	protected virtual float m_dashWakeActivationRange => 5.5f;

	protected virtual float m_dashWakeAttackOdds => 0.5f;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[4] { "MimicCoinProjectile", "MimicCoinMinibossProjectile", "MimicBossBounceBoltProjectile", "MimicBossJumpProjectile" };
	}

	private void Awake()
	{
		m_onPlayerHit = OnPlayerHit;
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		base.EnemyController.HealthChangeRelay.AddListener(SpewCoins);
		base.EnemyController.LogicController.DisableLogicActivationByDistance = true;
		base.EnemyController.LogicController.OverrideLogicDelay(0f);
		base.EnemyController.LockFlip = true;
		base.EnemyController.CharacterHitResponse.OnCharacterHitRelay.AddListener(OnEnemyHit);
		base.EnemyController.LogicController.DisableRestLogicInterrupt = true;
		base.EnemyController.StatusEffectController.SetAllStatusEffectsHidden(hide: true);
		m_matPropertyBlock = new MaterialPropertyBlock();
		m_playerWasInDashWakeRange = false;
		m_dashEventInstance = AudioUtility.GetEventInstance(DashLoopAudioEventPath, base.transform);
	}

	private void OnEnable()
	{
		if (PlayerManager.IsInstantiated && !PlayerManager.IsDisposed)
		{
			PlayerManager.GetPlayerController().CharacterHitResponse.OnCharacterHitRelay.AddListener(m_onPlayerHit);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (PlayerManager.IsInstantiated && !PlayerManager.IsDisposed)
		{
			PlayerManager.GetPlayerController().CharacterHitResponse.OnCharacterHitRelay.RemoveListener(m_onPlayerHit);
		}
		if (base.IsInitialized)
		{
			base.EnemyController.StatusEffectController.SetAllStatusEffectsHidden(hide: true);
			base.EnemyController.LogicController.DisableLogicActivationByDistance = true;
			for (int i = 0; i < base.EnemyController.RendererArray.Count; i++)
			{
				Renderer renderer = base.EnemyController.RendererArray[i];
				renderer.GetPropertyBlock(m_matPropertyBlock);
				m_matPropertyBlock.SetColor(ShaderID_RL._MultiplyColor, base.EnemyController.RendererArrayDefaultTint[i].DefaultMultiplyColor);
				renderer.SetPropertyBlock(m_matPropertyBlock);
			}
			if (m_dashEventInstance.isValid())
			{
				AudioManager.Stop(m_dashEventInstance);
			}
		}
	}

	public override void OnEnemyActivated()
	{
		base.OnEnemyActivated();
		if (!base.EnemyController.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_Invuln))
		{
			return;
		}
		foreach (Renderer item in base.EnemyController.RendererArray)
		{
			if (item.sharedMaterial.HasProperty(ShaderID_RL._ShieldToggle))
			{
				item.GetPropertyBlock(BaseStatusEffect.m_matBlockHelper_STATIC);
				BaseStatusEffect.m_matBlockHelper_STATIC.SetInt(ShaderID_RL._ShieldToggle, 1);
				item.SetPropertyBlock(BaseStatusEffect.m_matBlockHelper_STATIC);
			}
		}
	}

	private void OnEnemyHit(object sender, CharacterHitEventArgs args)
	{
		base.EnemyController.StatusEffectController.SetAllStatusEffectsHidden(hide: false);
	}

	private void OnPlayerHit(object sender, CharacterHitEventArgs args)
	{
		if (args.Attacker == base.EnemyController)
		{
			base.EnemyController.LogicController.TriggerAggro(null, null);
			base.EnemyController.StatusEffectController.SetAllStatusEffectsHidden(hide: false);
			if (PlayerManager.IsInstantiated && !PlayerManager.IsDisposed)
			{
				PlayerManager.GetPlayerController().CharacterHitResponse.OnCharacterHitRelay.RemoveListener(OnPlayerHit);
			}
		}
	}

	private void OnDestroy()
	{
		if (m_dashEventInstance.isValid())
		{
			m_dashEventInstance.release();
		}
		if ((bool)base.EnemyController)
		{
			base.EnemyController.HealthChangeRelay.RemoveListener(SpewCoins);
			base.EnemyController.CharacterHitResponse.OnCharacterHitRelay.RemoveListener(OnEnemyHit);
		}
	}

	protected void SpewCoins(object sender, HealthChangeEventArgs args)
	{
		if (!(args.PrevHealthValue <= args.NewHealthValue) && !(args.NewHealthValue <= 0f))
		{
			for (int i = 0; i < NumCoinsFiredOnHit; i++)
			{
				float angle = UnityEngine.Random.Range(CoinFireAngle_OnHit.x, CoinFireAngle_OnHit.y);
				float speedMod = UnityEngine.Random.Range(CoinFireSpeed_OnHit.x, CoinFireSpeed_OnHit.y);
				this.FireProjectile(m_verticalShotProjectileName, 0, matchFacing: false, angle, speedMod);
			}
		}
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator VerticalShot_Attack()
	{
		StopAndFaceTarget();
		yield return Default_Animation("VerticalShot_Tell_Hold", m_verticalShot_TellHold_AnimationSpeed, m_verticalShot_TellHold_Delay);
		yield return Default_Animation("HopIntro", m_verticalShot_AttackIntro_AnimationSpeed, m_verticalShot_AttackIntro_Delay);
		for (int i = 0; i < m_verticalShot_TotalLoops; i++)
		{
			for (int j = 0; j < m_verticalShot_TotalShotSpread; j++)
			{
				float num = UnityEngine.Random.Range((int)m_verticalShot_RandomAngleAngleOffset.x, (int)m_verticalShot_RandomAngleAngleOffset.y);
				this.FireProjectile(m_verticalShotProjectileName, 1, matchFacing: true, (float)m_verticalShot_InitialAngle + num * (float)(j + 1), m_verticalShot_SpeedMod);
			}
			yield return Wait(m_verticalShot_LoopDelay);
		}
		yield return Default_Animation("Hop", m_verticalShot_AttackHold_AnimationSpeed, m_verticalShot_AttackHold_Delay, waitUntilAnimationComplete: false);
		yield return Default_Animation("HopLand", m_verticalShot_Exit_AnimationSpeed, m_verticalShot_Exit_Delay);
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_verticalShot_Exit_ForceIdle, m_verticalShot_Exit_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Dash_Attack()
	{
		StopAndFaceTarget();
		yield return Default_TellIntroAndLoop("Dash_Tell_Intro", m_dashAttack_TellIntro_AnimationSpeed, "Dash_Tell_Hold", m_dashAttack_TellHold_AnimationSpeed, m_dashAttack_TellIntroAndHold_Delay);
		yield return Default_Animation("Dash_Attack_Intro", m_dashAttack_AttackIntro_AnimationSpeed, m_dashAttack_AttackIntro_Delay);
		yield return Default_Animation("Dash_Attack_Hold", m_dashAttack_AttackHold_AnimationSpeed, m_dashAttack_AttackHold_Delay, waitUntilAnimationComplete: false);
		base.EnemyController.DisableFriction = true;
		if (base.EnemyController.IsFacingRight)
		{
			SetVelocityX(m_dashAttackSpeed, additive: false);
		}
		else
		{
			SetVelocityX(0f - m_dashAttackSpeed, additive: false);
		}
		base.EnemyController.LockFlip = true;
		m_isDashAttacking = true;
		OnStartDashing();
		yield return Wait(m_dashAttackDuration.x, m_dashAttackDuration.y);
		OnStopDashing();
		m_isDashAttacking = false;
		if (base.EnemyController.IsFacingRight)
		{
			SetVelocityX(m_dashAttackSpeed * 0.5f, additive: false);
		}
		else
		{
			SetVelocityX(m_dashAttackSpeed * -0.5f, additive: false);
		}
		base.EnemyController.DisableFriction = true;
		yield return Default_Animation("Dash_Exit", m_dashAttack_Exit_AnimationSpeed, m_dashAttack_Exit_Delay);
		SetVelocityX(0f, additive: false);
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_dashAttack_Exit_ForceIdle, m_dashAttack_Exit_AttackCD);
		base.EnemyController.LockFlip = false;
	}

	protected virtual void OnStartDashing()
	{
		AudioManager.PlayAttached(this, m_dashEventInstance, base.gameObject);
	}

	protected virtual void OnStopDashing()
	{
		AudioManager.Stop(m_dashEventInstance);
	}

	private void OnJump()
	{
		AudioManager.PlayOneShotAttached(this, JumpAudioEventPath, base.gameObject);
	}

	private void OnGrounded()
	{
		AudioManager.PlayOneShotAttached(this, LandAudioEventPath, base.gameObject);
	}

	private IEnumerator OnBeginDeathAnim()
	{
		AudioManager.PlayOneShotAttached(this, DeathHitAudioEventPath, base.gameObject);
		if (base.EnemyController.IsBoss)
		{
			yield return base.DeathAnim();
		}
	}

	public override void OnLBCompleteOrCancelled()
	{
		base.EnemyController.DisableFriction = false;
		base.EnemyController.LockFlip = false;
		m_isDashAttacking = false;
		base.OnLBCompleteOrCancelled();
	}

	public override IEnumerator WalkTowards()
	{
		while (base.EnemyController.KnockedIntoAir)
		{
			yield return null;
		}
		base.EnemyController.LockFlip = false;
		StopAndFaceTarget();
		base.EnemyController.LockFlip = true;
		this.SetAnimationSpeedMultiplier(WalkAnimSpeedMod);
		float y = UnityEngine.Random.Range(JumpHeight.x, JumpHeight.y);
		if (base.EnemyController.IsTargetToMyRight)
		{
			SetVelocity(base.EnemyController.ActualSpeed, y, additive: false);
		}
		else
		{
			SetVelocity(0f - base.EnemyController.ActualSpeed, y, additive: false);
		}
		OnJump();
		yield return Wait(0.25f);
		yield return WaitUntilIsGrounded();
		for (int i = 0; i < NumCoinsFiredOnLanding; i++)
		{
			float angle = UnityEngine.Random.Range(CoinFireAngle_OnLand.x, CoinFireAngle_OnLand.y);
			float speedMod = UnityEngine.Random.Range(CoinFireSpeed_OnLand.x, CoinFireSpeed_OnLand.y);
			if (m_advancedBoss)
			{
				this.FireProjectile("MimicBossBounceBoltProjectile", 2, matchFacing: false);
				this.FireProjectile("MimicBossBounceBoltProjectile", 2, matchFacing: false, 180f);
			}
			else
			{
				this.FireProjectile(m_verticalShotProjectileName, 0, matchFacing: false, angle, speedMod);
			}
		}
		this.SetAnimationSpeedMultiplier(1f);
		base.EnemyController.LockFlip = false;
	}

	public override IEnumerator DeathAnim()
	{
		if ((bool)base.EnemyController && (bool)base.EnemyController.EnemySpawnController)
		{
			base.EnemyController.EnemySpawnController.ForceEnemyDead(disableEnemy: false);
		}
		if (m_playDeathParticlesOnZeroHP)
		{
			EffectManager.PlayEffect(base.EnemyController.gameObject, base.EnemyController.Animator, "EnemyGenericDeathPuff_Small_Effect_MediumVariant", base.EnemyController.transform.position, m_deathParticleDuration, EffectStopType.Gracefully);
		}
		yield return OnBeginDeathAnim();
		foreach (Renderer item in base.EnemyController.RendererArray)
		{
			item.GetPropertyBlock(m_matPropertyBlock);
			m_matPropertyBlock.SetColor(ShaderID_RL._MultiplyColor, m_deathTintColor);
			item.SetPropertyBlock(m_matPropertyBlock);
		}
		if (!base.EnemyController.ControllerCorgi.State.IsGrounded)
		{
			yield return Default_Animation("KOAirborne", m_airborneKO_AnimationSpeed, m_airborneKO_AnimationDelay, waitUntilAnimationComplete: false);
			while (!base.EnemyController.ControllerCorgi.State.IsGrounded)
			{
				yield return null;
			}
		}
		SetVelocity(0f, 0f, additive: false);
		yield return Default_Animation("MimicChest_KOGrounded", m_groundedKOAnimationSpeed, m_groundedKOAnimationDelay);
		yield return Default_Animation("MimicChest_KOShake", m_death_AnimationSpeed, m_death_AnimationDelay);
		this.FireProjectile(m_verticalShotProjectileName, 0, matchFacing: false, 90f);
		for (int i = 0; i < DeathCoinAttack_CoinSpreadAmount; i++)
		{
			float angle = 90f + (float)(i + 1) * DeathCoinAttack_AngleSpread;
			this.FireProjectile(m_verticalShotProjectileName, 0, matchFacing: false, angle);
			angle = 90f - (float)(i + 1) * DeathCoinAttack_AngleSpread;
			this.FireProjectile(m_verticalShotProjectileName, 0, matchFacing: false, angle);
		}
	}

	protected void LateUpdate()
	{
		if (!base.IsInitialized)
		{
			return;
		}
		if (m_canDashWake && !base.LogicController.LogicIsActivated)
		{
			bool flag = CDGHelper.DistanceBetweenPts(base.LogicController.PlayerController.Midpoint, base.EnemyController.Midpoint) < m_dashWakeActivationRange;
			if (!LocalTeleporterController.IsTeleporting && flag && !m_playerWasInDashWakeRange && UnityEngine.Random.Range(0f, 1f) < m_dashWakeAttackOdds)
			{
				base.LogicController.TriggerAggro(null, null);
				base.EnemyController.StatusEffectController.SetAllStatusEffectsHidden(hide: false);
				if (PlayerManager.IsInstantiated && !PlayerManager.IsDisposed)
				{
					PlayerManager.GetPlayerController().CharacterHitResponse.OnCharacterHitRelay.RemoveListener(OnPlayerHit);
				}
				base.LogicController.ForceExecuteLogicBlockName_OnceOnly = "Dash_Attack";
			}
			m_playerWasInDashWakeRange = flag;
		}
		if (m_isDashAttacking)
		{
			bool flag2 = base.EnemyController.ControllerCorgi.State.IsCollidingLeft;
			if (base.EnemyController.IsFacingRight)
			{
				flag2 = base.EnemyController.ControllerCorgi.State.IsCollidingRight;
			}
			if (flag2)
			{
				base.EnemyController.LockFlip = false;
				base.EnemyController.CharacterCorgi.Flip();
				base.EnemyController.LockFlip = true;
				if (base.EnemyController.IsFacingRight)
				{
					SetVelocityX(m_dashAttackSpeed, additive: false);
				}
				else
				{
					SetVelocityX(0f - m_dashAttackSpeed, additive: false);
				}
				AudioManager.PlayOneShotAttached(this, ChangeDirectionAudioEventPath, base.gameObject);
			}
		}
		if (base.EnemyController.IsGrounded && !m_wasGrounded)
		{
			OnGrounded();
		}
		m_wasGrounded = base.EnemyController.IsGrounded;
	}

	public override void ResetScript()
	{
		string forceExecuteLogicBlockName_OnceOnly = base.LogicController.ForceExecuteLogicBlockName_OnceOnly;
		base.ResetScript();
		base.LogicController.ForceExecuteLogicBlockName_OnceOnly = forceExecuteLogicBlockName_OnceOnly;
		if (!base.EnemyController.IsFacingRight)
		{
			base.EnemyController.LockFlip = false;
			base.EnemyController.CharacterCorgi.Flip();
		}
		base.EnemyController.LockFlip = true;
	}
}
