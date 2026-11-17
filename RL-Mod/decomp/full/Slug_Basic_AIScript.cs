using System.Collections;
using RLAudio;
using UnityEngine;

public class Slug_Basic_AIScript : BaseAIScript
{
	private const float RAYCAST_LENGTH = 0.25f;

	protected const string WALK_PROJECTILE = "SlugBoltProjectile";

	private const string TRAIL_DESPAWN_AUDIO_PATH = "event:/SFX/Enemies/sfx_enemy_slug_trail_despawn";

	private bool m_moveRight;

	private Projectile_RL m_lastProjectileTrailSpawned;

	private ParticleSystem m_slimeTrail;

	protected const string VERTICAL_SHOT_TELL_INTRO = "Shoot_Tell_Intro";

	protected const string VERTICAL_SHOT_TELL_HOLD = "Shoot_Tell_Hold";

	protected const string VERTICAL_SHOT_ATTACK_INTRO = "Shoot_Attack_Intro";

	protected const string VERTICAL_SHOT_ATTACK_HOLD = "Shoot_Attack_Hold";

	protected const string VERTICAL_SHOT_EXIT = "Shoot_Exit";

	protected const string VERTICAL_SHOT_PROJECTILE = "SlugGravityBoltProjectile";

	private bool m_slimeTrailDisplacedRight;

	private bool m_slimeTrailDisplacedLeft;

	protected override Vector2 IdleDuration => new Vector2(0.5f, 0.5f);

	protected override Vector2 WalkTowardsDuration => new Vector2(0.45f, 0.45f);

	protected virtual float m_WalkTowards_ForceIdle => 0f;

	protected virtual int m_trail_ProjectileAmount => 3;

	protected virtual float m_verticalShot_TellIntro_AnimationSpeed => 1.25f;

	protected virtual float m_verticalShot_TellHold_AnimationSpeed => 1.25f;

	protected virtual float m_verticalShot_TellIntroAndHold_Delay => 1f;

	protected virtual float m_verticalShot_AttackIntro_AnimationSpeed => 1.25f;

	protected virtual float m_verticalShot_AttackIntro_Delay => 0f;

	protected virtual float m_verticalShot_AttackHold_AnimationSpeed => 1.25f;

	protected virtual float m_verticalShot_AttackHold_Delay => 0f;

	protected virtual float m_verticalShot_Exit_AnimationSpeed => 0.65f;

	protected virtual float m_verticalShot_Exit_Delay => 0f;

	protected virtual float m_verticalShot_Exit_ForceIdle => 0.35f;

	protected virtual float m_verticalShot_Exit_AttackCD => 2f;

	protected virtual int m_verticalShot_TotalShotSpread => 0;

	protected virtual int m_verticalShot_SideBulletAngleOffset => 14;

	protected virtual float m_verticalShot_SpeedMod => 1f;

	protected virtual int m_verticalShot_RepeatAttackPattern => 0;

	protected virtual float m_verticalShot_RepeatAttackPatternDelay => 0f;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[2] { "SlugBoltProjectile", "SlugGravityBoltProjectile" };
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		m_moveRight = CDGHelper.RandomPlusMinus() > 0;
		base.EnemyController.OnEnemyDeathRelay.AddListener(OnSlugDeath);
		m_slimeTrail = base.EnemyController.gameObject.FindObjectReference<ParticleSystem>("SlimeTrail");
	}

	public override void ResetScript()
	{
		m_moveRight = CDGHelper.RandomPlusMinus() > 0;
		base.ResetScript();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		m_lastProjectileTrailSpawned = null;
		if ((bool)m_slimeTrail)
		{
			m_slimeTrail.transform.SetLocalPositionX(0f);
		}
		m_slimeTrailDisplacedLeft = false;
		m_slimeTrailDisplacedRight = false;
	}

	private void OnDestroy()
	{
		if ((bool)base.EnemyController)
		{
			base.EnemyController.OnEnemyDeathRelay.RemoveListener(OnSlugDeath);
		}
	}

	private void OnSlugDeath(object sender, EnemyDeathEventArgs args)
	{
		ProjectileManager.DisableAllProjectiles(flagForDestruction: true, base.EnemyController.gameObject, "SlugBoltProjectile");
		if ((bool)m_lastProjectileTrailSpawned)
		{
			StopProjectile(ref m_lastProjectileTrailSpawned);
			AudioManager.PlayDelayedOneShot(this, "event:/SFX/Enemies/sfx_enemy_slug_trail_despawn", base.EnemyController.Pivot.transform.position, 0.8f);
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
		SetVelocity(0f, 0f, additive: false);
		yield return Default_TellIntroAndLoop("Shoot_Tell_Intro", m_verticalShot_TellIntro_AnimationSpeed, "Shoot_Tell_Hold", m_verticalShot_TellHold_AnimationSpeed, m_verticalShot_TellIntroAndHold_Delay);
		yield return Default_Animation("Shoot_Attack_Intro", m_verticalShot_AttackIntro_AnimationSpeed, m_verticalShot_AttackIntro_Delay);
		this.SetAnimationSpeedMultiplier(m_verticalShot_AttackHold_AnimationSpeed);
		yield return this.ChangeAnimationState("Shoot_Attack_Hold");
		for (int i = 0; i < m_verticalShot_RepeatAttackPattern; i++)
		{
			this.FireProjectile("SlugGravityBoltProjectile", 1, matchFacing: true, 90f, m_verticalShot_SpeedMod);
			for (int j = 1; j < m_verticalShot_TotalShotSpread + 1; j++)
			{
				this.FireProjectile("SlugGravityBoltProjectile", 1, matchFacing: true, 90 + m_verticalShot_SideBulletAngleOffset * j, m_verticalShot_SpeedMod);
				this.FireProjectile("SlugGravityBoltProjectile", 1, matchFacing: true, 90 - m_verticalShot_SideBulletAngleOffset * j, m_verticalShot_SpeedMod);
			}
			if (m_verticalShot_RepeatAttackPatternDelay > 0f)
			{
				yield return Wait(m_verticalShot_RepeatAttackPatternDelay);
			}
		}
		float num = m_verticalShot_RepeatAttackPattern * (m_verticalShot_RepeatAttackPattern - 1);
		num = Mathf.Clamp(m_verticalShot_AttackHold_Delay - num, 0f, 2.1474836E+09f);
		if (num > 0f)
		{
			yield return Wait(num);
		}
		yield return Default_Animation("Shoot_Exit", m_verticalShot_Exit_AnimationSpeed, m_verticalShot_Exit_Delay);
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_verticalShot_Exit_ForceIdle, m_verticalShot_Exit_AttackCD);
	}

	public override IEnumerator WalkTowards()
	{
		if (m_moveRight)
		{
			SetVelocityX(base.EnemyController.ActualSpeed, additive: false);
		}
		else
		{
			SetVelocityX(0f - base.EnemyController.ActualSpeed, additive: false);
		}
		if (base.EnemyController.IsFacingRight != m_moveRight)
		{
			Flip();
		}
		if (m_trail_ProjectileAmount > 0)
		{
			float getWalkDuration = Random.Range(WalkTowardsDuration.x, WalkTowardsDuration.y);
			float timeBetweenProjectiles = getWalkDuration / (float)m_trail_ProjectileAmount;
			for (int i = 0; i < m_trail_ProjectileAmount; i++)
			{
				if (base.EnemyController.IsGrounded)
				{
					m_lastProjectileTrailSpawned = this.FireProjectile("SlugBoltProjectile", 0, matchFacing: false, 0f, 0f);
				}
				if (getWalkDuration > 0f)
				{
					yield return Wait(timeBetweenProjectiles);
				}
			}
		}
		else
		{
			yield return Wait(Random.Range(WalkTowardsDuration.x, WalkTowardsDuration.y));
		}
		this.SetAnimationSpeedMultiplier(1f);
		if (m_WalkTowards_ForceIdle > 0f)
		{
			yield return Idle(m_WalkTowards_ForceIdle);
		}
	}

	private void Turn()
	{
		SetVelocityX(0f - base.EnemyController.Velocity.x, additive: false);
		m_moveRight = !m_moveRight;
		if (base.EnemyController.IsFacingRight != m_moveRight)
		{
			Flip();
		}
	}

	private void LateUpdate()
	{
		if (!base.IsInitialized || base.EnemyController.IsDead)
		{
			return;
		}
		if ((bool)m_slimeTrail)
		{
			if (!base.EnemyController.IsGrounded && m_slimeTrail.isPlaying)
			{
				m_slimeTrail.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
			}
			else if (base.EnemyController.IsGrounded && !m_slimeTrail.isPlaying)
			{
				m_slimeTrail.Play(withChildren: true);
			}
		}
		if ((base.EnemyController.ControllerCorgi.State.IsCollidingLeft && !base.EnemyController.IsFacingRight) || (base.EnemyController.ControllerCorgi.State.IsCollidingRight && base.EnemyController.IsFacingRight) || (base.EnemyController.LeftSidePlatformDropPrevented && !base.EnemyController.IsFacingRight) || (base.EnemyController.RightSidePlatformDropPrevented && base.EnemyController.IsFacingRight) || (base.EnemyController.TouchingLeftRoomEdge && !base.EnemyController.IsFacingRight) || (base.EnemyController.TouchingRightRoomEdge && base.EnemyController.IsFacingRight))
		{
			Turn();
		}
		float belowSlopeAngle = base.EnemyController.ControllerCorgi.State.BelowSlopeAngle;
		if ((belowSlopeAngle > 0f && !base.EnemyController.IsFacingRight) || (belowSlopeAngle < 0f && base.EnemyController.IsFacingRight))
		{
			if (!m_slimeTrailDisplacedRight)
			{
				if ((bool)m_slimeTrail)
				{
					m_slimeTrail.gameObject.transform.position = GetAbsoluteSpawnPositionAtIndex(3, matchFacing: false);
				}
				m_slimeTrailDisplacedRight = true;
			}
		}
		else if ((belowSlopeAngle > 0f && base.EnemyController.IsFacingRight) || (belowSlopeAngle < 0f && !base.EnemyController.IsFacingRight))
		{
			if (!m_slimeTrailDisplacedLeft)
			{
				if ((bool)m_slimeTrail)
				{
					m_slimeTrail.gameObject.transform.position = GetAbsoluteSpawnPositionAtIndex(2, matchFacing: false);
				}
				m_slimeTrailDisplacedLeft = true;
			}
		}
		else if (m_slimeTrailDisplacedLeft || m_slimeTrailDisplacedRight)
		{
			if ((bool)m_slimeTrail)
			{
				m_slimeTrail.gameObject.transform.position = base.EnemyController.transform.position;
			}
			m_slimeTrailDisplacedLeft = false;
			m_slimeTrailDisplacedRight = false;
		}
	}
}
