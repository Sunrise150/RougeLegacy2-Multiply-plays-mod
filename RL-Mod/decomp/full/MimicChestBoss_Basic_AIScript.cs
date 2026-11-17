using System.Collections;
using RLAudio;
using UnityEngine;

public class MimicChestBoss_Basic_AIScript : MimicChest_Basic_AIScript, IAudioEventEmitter
{
	private const string COIN_PROJECTILE_NAME = "MimicCoinProjectile";

	private const string POTION_PROJECTILE_NAME = "MimicBossPotionProjectile";

	private const string DASH_TRAIL_PROJECTILE_NAME = "MimicBossDashTrailProjectile";

	private const string GALLOP_AUDIO_EVENT_PATH = "event:/SFX/Enemies/sfx_mimicBoss_charge_gallop";

	protected const string INTRO_OPEN = "BossIntro_Open";

	protected const string INTRO_CLOSE = "BossIntro_ShakeAndClose";

	protected const string INTRO_GROWL = "BossIntro_GrowlLoop";

	private bool m_shouldPlayGallop = true;

	protected override Vector2 IdleDuration => new Vector2(0.25f, 0.7f);

	protected override Vector2 WalkTowardsDuration => new Vector2(0.65f, 0.9f);

	protected override Vector2 WalkAwayDuration => new Vector2(0.45f, 0.85f);

	protected override string JumpAudioEventPath => "event:/SFX/Enemies/sfx_mimicBoss_jump";

	protected override string LandAudioEventPath => "event:/SFX/Enemies/sfx_mimicBoss_land";

	protected override string DeathHitAudioEventPath => "event:/SFX/Enemies/sfx_mimicBoss_death_hit";

	protected override float m_airborneKO_AnimationDelay => 1.5f;

	protected override float m_dashAttackSpeed => 20f;

	protected override Vector2 m_dashAttackDuration => new Vector2(5f, 5f);

	protected override Vector2 JumpHeight => new Vector2(21f, 29f);

	protected override string m_verticalShotProjectileName => "MimicBossPotionProjectile";

	protected override float m_dashAttack_Exit_AttackCD => 7.5f;

	protected virtual bool m_spawnDashTrailProjectiles => true;

	protected virtual float m_timeBetweenDashTrailProjectiles => 0.1f;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[5] { "MimicCoinProjectile", "MimicBossPotionProjectile", "MimicBossDashTrailProjectile", "MimicBossBounceBoltProjectile", "MimicBossJumpProjectile" };
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		base.EnemyController.LogicController.DisableLogicActivationByDistance = true;
		base.EnemyController.LogicController.OverrideLogicDelay(0f);
		base.EnemyController.LockFlip = true;
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Intro()
	{
		base.EnemyController.AlwaysFacing = false;
		this.ToDo("INTRO");
		this.ChangeAnimationState("Neutral");
		yield return Wait(2f);
		this.ChangeAnimationState("Hint");
		yield return Wait(2f);
		base.EnemyController.AlwaysFacing = true;
		base.EnemyController.HitboxController.SetHitboxActiveState(HitboxType.Body, active: true);
		base.EnemyController.HitboxController.SetHitboxActiveState(HitboxType.Platform, active: true);
		base.EnemyController.HitboxController.SetHitboxActiveState(HitboxType.Terrain, active: true);
		base.EnemyController.HitboxController.SetHitboxActiveState(HitboxType.Weapon, active: true);
		yield return Default_Attack_Cooldown(0.5f, 99999f);
	}

	public override IEnumerator SpawnAnim()
	{
		yield return Default_Animation("BossIntro_Open", 1f, 2f);
		yield return Default_Animation("BossIntro_ShakeAndClose", 1f, 1f);
		yield return Default_Animation("BossIntro_GrowlLoop", 1f, 1f);
		MusicManager.StopMusic();
		MusicManager.PlayMusic(SongID.StudyBossBGM_Boss5_Phase2, isOverride: false);
		yield return Default_Animation(BaseAIScript.NEUTRAL_STATE, 1f, 0f, waitUntilAnimationComplete: false);
	}

	protected override void OnStartDashing()
	{
		if (m_spawnDashTrailProjectiles)
		{
			StartCoroutine(SpawnDashTrailProjectiles());
		}
	}

	private IEnumerator SpawnDashTrailProjectiles()
	{
		float timeStart = Time.time;
		while (m_isDashAttacking)
		{
			if (Time.time - timeStart >= m_timeBetweenDashTrailProjectiles)
			{
				if (!base.IsPaused)
				{
					this.FireProjectile("MimicCoinProjectile", 3, matchFacing: true, 35f, 0.5f);
					if (base.EnemyController.IsGrounded)
					{
						if (m_shouldPlayGallop)
						{
							AudioManager.PlayOneShotAttached(this, "event:/SFX/Enemies/sfx_mimicBoss_charge_gallop", base.gameObject);
						}
						m_shouldPlayGallop = !m_shouldPlayGallop;
					}
					else
					{
						m_shouldPlayGallop = true;
					}
				}
				timeStart = Time.time;
			}
			yield return null;
		}
	}

	protected override void OnStopDashing()
	{
		_ = m_spawnDashTrailProjectiles;
	}

	protected override void OnDisable()
	{
		if (base.IsInitialized)
		{
			base.EnemyController.GetComponent<Interactable>().SetIsInteractableActive(isActive: false);
			base.EnemyController.LogicController.DisableLogicActivationByDistance = true;
		}
	}

	public override void OnEnemyActivated()
	{
		base.OnEnemyActivated();
		base.EnemyController.GetComponent<Interactable>().SetIsInteractableActive(isActive: false);
	}
}
