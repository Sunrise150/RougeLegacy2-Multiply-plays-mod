using System.Collections;
using RLAudio;
using UnityEngine;

public class FlyingHunter_Basic_AIScript : BaseAIScript
{
	private bool m_isSleeping = true;

	private TrailRenderer m_trailRenderer;

	private const string TELEPORT_BEGIN_AUDIO_PATH = "event:/SFX/Enemies/sfx_enemy_flyingHunter_teleport_vanish";

	private const string TELEPORT_END_AUDIO_PATH = "event:/SFX/Enemies/sfx_enemy_flyingHunter_teleport_appear";

	private const string SHOOT_AUDIO_PATH = "event:/SFX/Enemies/sfx_enemy_flyingHunter_shoot";

	private bool m_chaseTarget;

	private WaitRL_Yield m_teleportWaitYield;

	private float m_requiredHPDropToTeleport;

	private const string TELEPORT_STUNNED_ANIM = "Teleporting";

	protected float m_teleportStunned_AnimSpeed = 1f;

	protected float m_teleportStunned_Delay;

	private const string BURST_TELL_INTRO = "BurstAttack_Tell_Intro";

	private const string BURST_TELL_HOLD = "BurstAttack_Tell_Hold";

	private const string BURST_ATTACK_INTRO = "BurstAttack_Attack_Intro";

	private const string BURST_ATTACK_HOLD = "BurstAttack_Attack_Hold";

	private const string BURST_EXIT = "BurstAttack_Exit";

	private const string BURST_PROJECTILE = "FlyingHunterBoltProjectile";

	protected float m_burst_Tell_AnimationSpeed = 1f;

	protected float m_burst_TellHold_AnimationSpeed = 1f;

	protected float m_burst_TellAndHold_Delay = 0.2f;

	protected float m_burst_AttackIntro_Delay;

	protected float m_burst_AttackIntro_AnimationSpeed = 2f;

	protected float m_burst_AttackHold_Delay;

	protected float m_burst_AttackHold_AnimationSpeed = 2f;

	protected float m_burst_Exit_AnimationSpeed = 1f;

	protected float m_burst_Exit_Delay;

	protected virtual float TeleportHealthLossTrigger => 0.25f;

	protected virtual float WakeUpChaseDelay => 0.5f;

	protected virtual bool TeleportOnHit => true;

	protected virtual Vector2 TeleportDistanceFromPlayer => new Vector2(10f, 10f);

	protected virtual int TeleportAngleFromPlayer => 60;

	protected virtual int TeleportOddsToFlipSideOnHit => 100;

	protected virtual bool TeleportSpawnProjectilesOnHit => true;

	protected virtual float OnHitDelayDuration => 0.6f;

	protected override Vector2 RandomFollowOffsetX => Vector2.zero;

	protected override Vector2 RandomFollowOffsetY => Vector2.zero;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[1] { "FlyingHunterBoltProjectile" };
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		if (TeleportOnHit)
		{
			base.EnemyController.CharacterHitResponse.OnCharacterHitRelay.AddListener(OnEnemyHit);
		}
		m_teleportWaitYield = new WaitRL_Yield(0f);
		m_trailRenderer = base.EnemyController.GetComponentInChildren<TrailRenderer>();
		base.EnemyController.ResetToNeutralWhenUnculling = false;
		base.EnemyController.DisableDistanceThresholdCheck = true;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if ((bool)m_trailRenderer)
		{
			m_trailRenderer.Clear();
		}
	}

	private void OnDestroy()
	{
		if ((bool)base.EnemyController && TeleportOnHit)
		{
			base.EnemyController.CharacterHitResponse.OnCharacterHitRelay.RemoveListener(OnEnemyHit);
		}
	}

	protected void OnEnemyHit(object sender, CharacterHitEventArgs args)
	{
		if (args.DamageTaken > 0f && base.EnemyController.CurrentHealth <= m_requiredHPDropToTeleport)
		{
			m_requiredHPDropToTeleport = base.EnemyController.CurrentHealth - (float)base.EnemyController.ActualMaxHealth * TeleportHealthLossTrigger;
			base.LogicController.StopAllLogic();
			base.LogicController.ForceExecuteLogicBlockName_OnceOnly = "Teleport";
		}
	}

	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[WanderLogic]
	[RestLogic]
	public IEnumerator Teleport()
	{
		_ = (float)Random.Range(0, 359);
		float num = Random.Range(0, 100);
		float num2 = Random.Range(-TeleportAngleFromPlayer, TeleportAngleFromPlayer);
		if (base.EnemyController.IsTargetToMyRight)
		{
			num2 += -180f;
		}
		if (num < (float)TeleportOddsToFlipSideOnHit)
		{
			num2 += -180f;
		}
		AudioManager.PlayOneShot(this, "event:/SFX/Enemies/sfx_enemy_flyingHunter_teleport_vanish", base.EnemyController.transform.position);
		float num3 = Random.Range(TeleportDistanceFromPlayer.x, TeleportDistanceFromPlayer.y);
		Vector3 position = CDGHelper.AngleToVector(num2) * num3;
		position += PlayerManager.GetPlayerController().transform.position;
		position.z = base.EnemyController.transform.position.z;
		base.EnemyController.transform.position = position;
		base.EnemyController.UpdateBounds();
		base.EnemyController.ConstrainEnemyMovementToRoom();
		PlayerController playerController = PlayerManager.GetPlayerController();
		if (CDGHelper.DistanceBetweenPts(base.EnemyController.transform.position, playerController.Midpoint) < num3 / 2f)
		{
			position = base.EnemyController.transform.position;
			if (position.y > base.EnemyController.Room.Bounds.center.y)
			{
				position.y -= num3;
			}
			else
			{
				position.y += num3;
			}
			base.EnemyController.transform.position = position;
			base.EnemyController.UpdateBounds();
		}
		if ((bool)m_trailRenderer)
		{
			m_trailRenderer.Clear();
		}
		yield return null;
		SetVelocity(0f, 0f, additive: false);
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		AudioManager.PlayOneShotAttached(this, "event:/SFX/Enemies/sfx_enemy_flyingHunter_teleport_appear", base.gameObject);
		if (TeleportSpawnProjectilesOnHit)
		{
			AudioManager.PlayOneShotAttached(this, "event:/SFX/Enemies/sfx_enemy_flyingHunter_shoot", base.gameObject);
		}
		base.EnemyController.Animator.SetBool("Teleporting", value: true);
		yield return Default_Animation("Teleporting", m_teleportStunned_AnimSpeed, m_teleportStunned_Delay);
		base.EnemyController.Animator.SetBool("Teleporting", value: false);
		if (TeleportSpawnProjectilesOnHit)
		{
			yield return Default_TellIntroAndLoop("BurstAttack_Tell_Intro", m_burst_Tell_AnimationSpeed, "BurstAttack_Tell_Hold", m_burst_TellHold_AnimationSpeed, m_burst_TellAndHold_Delay);
			yield return Default_Animation("BurstAttack_Attack_Intro", m_burst_AttackIntro_AnimationSpeed, m_burst_AttackIntro_Delay);
			int num4 = Random.Range(0, 2);
			if (base.LogicController.EnemyLogicType == EnemyLogicType.Basic)
			{
				if (num4 == 0)
				{
					this.FireProjectile("FlyingHunterBoltProjectile", 0, matchFacing: false);
					this.FireProjectile("FlyingHunterBoltProjectile", 0, matchFacing: false, 180f);
				}
				else
				{
					this.FireProjectile("FlyingHunterBoltProjectile", 0, matchFacing: false, 90f);
					this.FireProjectile("FlyingHunterBoltProjectile", 0, matchFacing: false, 270f);
				}
			}
			else if (base.LogicController.EnemyLogicType == EnemyLogicType.Advanced)
			{
				if (num4 == 0)
				{
					this.FireProjectile("FlyingHunterBoltProjectile", 0, matchFacing: false, 45f);
					this.FireProjectile("FlyingHunterBoltProjectile", 0, matchFacing: false, 135f);
					this.FireProjectile("FlyingHunterBoltProjectile", 0, matchFacing: false, -45f);
					this.FireProjectile("FlyingHunterBoltProjectile", 0, matchFacing: false, -135f);
				}
				else
				{
					this.FireProjectile("FlyingHunterBoltProjectile", 0, matchFacing: false);
					this.FireProjectile("FlyingHunterBoltProjectile", 0, matchFacing: false, 90f);
					this.FireProjectile("FlyingHunterBoltProjectile", 0, matchFacing: false, 180f);
					this.FireProjectile("FlyingHunterBoltProjectile", 0, matchFacing: false, 270f);
				}
			}
			else if (base.LogicController.EnemyLogicType == EnemyLogicType.Expert)
			{
				switch (num4)
				{
				case 0:
					this.FireProjectile("FlyingHunterBoltProjectile", 0, matchFacing: false, 30f);
					this.FireProjectile("FlyingHunterBoltProjectile", 0, matchFacing: false, 60f);
					this.FireProjectile("FlyingHunterBoltProjectile", 0, matchFacing: false, 120f);
					this.FireProjectile("FlyingHunterBoltProjectile", 0, matchFacing: false, 150f);
					this.FireProjectile("FlyingHunterBoltProjectile", 0, matchFacing: false, -30f);
					this.FireProjectile("FlyingHunterBoltProjectile", 0, matchFacing: false, -60f);
					this.FireProjectile("FlyingHunterBoltProjectile", 0, matchFacing: false, -120f);
					this.FireProjectile("FlyingHunterBoltProjectile", 0, matchFacing: false, -150f);
					break;
				case 1:
					this.FireProjectile("FlyingHunterBoltProjectile", 0, matchFacing: false, -15f);
					this.FireProjectile("FlyingHunterBoltProjectile", 0, matchFacing: false, 15f);
					this.FireProjectile("FlyingHunterBoltProjectile", 0, matchFacing: false, 75f);
					this.FireProjectile("FlyingHunterBoltProjectile", 0, matchFacing: false, 105f);
					this.FireProjectile("FlyingHunterBoltProjectile", 0, matchFacing: false, 165f);
					this.FireProjectile("FlyingHunterBoltProjectile", 0, matchFacing: false, 195f);
					this.FireProjectile("FlyingHunterBoltProjectile", 0, matchFacing: false, 255f);
					this.FireProjectile("FlyingHunterBoltProjectile", 0, matchFacing: false, 285f);
					break;
				}
			}
			yield return Default_Animation("BurstAttack_Attack_Hold", m_burst_AttackHold_AnimationSpeed, m_burst_AttackHold_Delay);
			yield return Default_Animation("BurstAttack_Exit", m_burst_Exit_AnimationSpeed, m_burst_Exit_Delay);
		}
		yield return Idle(OnHitDelayDuration);
		yield return this.ChangeAnimationState("Neutral");
	}

	public override void OnLBCompleteOrCancelled()
	{
		base.OnLBCompleteOrCancelled();
		base.EnemyController.Animator.SetBool("Teleporting", value: false);
	}

	public override IEnumerator WalkTowards()
	{
		while (!m_chaseTarget)
		{
			yield return null;
		}
		if (m_isSleeping)
		{
			m_isSleeping = false;
			base.Animator.SetBool("Awake", value: true);
			yield return this.ChangeAnimationState("WakeUp");
			yield return Wait(WakeUpChaseDelay);
		}
		yield return base.WalkTowards();
	}

	public override IEnumerator Idle()
	{
		if (m_chaseTarget)
		{
			yield return WalkTowards();
		}
		else
		{
			yield return base.Idle();
		}
	}

	public override void ResetScript()
	{
		GoToSleep();
		m_chaseTarget = false;
		m_requiredHPDropToTeleport = 0f;
		base.ResetScript();
	}

	private void GoToSleep()
	{
		m_isSleeping = true;
		m_chaseTarget = false;
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		SetVelocity(0f, 0f, additive: false);
		base.Animator.SetBool("Awake", value: false);
		base.Animator.Play("Sleeping");
	}

	public override void OnEnemyActivated()
	{
		m_requiredHPDropToTeleport = base.EnemyController.CurrentHealth - (float)base.EnemyController.ActualMaxHealth * TeleportHealthLossTrigger;
		base.OnEnemyActivated();
	}

	private void FixedUpdate()
	{
		if (base.IsInitialized && (base.LogicController.CurrentRangeState == LogicState.Close || base.LogicController.IsAggroed) && !m_chaseTarget)
		{
			m_chaseTarget = true;
		}
	}
}
