using System.Collections;
using RLAudio;
using UnityEngine;

public class FlyingSkull_Basic_AIScript : BaseAIScript, IAudioEventEmitter
{
	private const string SHOOT_PREP_AUDIO_EVENT_PATH = "event:/SFX/Enemies/sfx_enemy_flyingSkull_shoot_prep";

	private const string SHOOT_AUDIO_EVENT_PATH = "event:/SFX/Enemies/sfx_enemy_flyingSkull_shoot";

	private const string DASH_PREP_AUDIO_EVENT_PATH = "event:/SFX/Enemies/sfx_enemy_flyingSkull_dash_prep";

	private const string DASH_AUDIO_EVENT_PATH = "event:/SFX/Enemies/sfx_enemy_flyingSkull_dash";

	protected int m_NearBone_Angle = 7;

	protected int m_FarBoneAngle = 20;

	private const string BONE_PROJECTILE = "FlyingSkullBoneProjectile";

	protected const string SINGLE_TELL_INTRO = "SingleShot_Tell_Intro";

	protected const string SINGLE_TELL_HOLD = "SingleShot_Tell_Hold";

	protected const string SINGLE_ATTACK_INTRO = "SingleShot_Attack_Intro";

	protected const string SINGLE_ATTACK_HOLD = "SingleShot_Attack_Hold";

	protected const string SINGLE_ATTACK_EXIT = "SingleShot_Exit";

	protected const string MULTI_TELL_INTRO = "MultiShot_Tell_Intro";

	protected const string MULTI_TELL_HOLD = "MultiShot_Tell_Hold";

	protected const string MULTI_ATTACK_INTRO = "MultiShot_Attack_Intro";

	protected const string MULTI_ATTACK_HOLD = "MultiShot_Attack_Hold";

	protected const string MULTI_ATTACK_EXIT = "MultiShot_Exit";

	protected override Vector2 IdleDuration => new Vector2(0.25f, 0.7f);

	protected override Vector2 WalkTowardsDuration => new Vector2(0.45f, 0.85f);

	protected override Vector2 WalkAwayDuration => new Vector2(0.45f, 0.85f);

	protected override Vector2 RandomFollowOffsetX => new Vector2(-5f, 5f);

	protected override Vector2 RandomFollowOffsetY => new Vector2(2f, 5f);

	protected virtual float m_shoot_TellIntro_AnimationSpeed => 1f;

	protected virtual float m_shoot_TellHold_AnimationSpeed => 2f;

	protected virtual float m_shoot_Tell_Delay => 0.75f;

	protected virtual float m_shoot_AttackIntro_AnimationSpeed => 1f;

	protected virtual float m_shoot_AttackIntro_Delay => 0f;

	protected virtual float m_shoot_AttackHold_AnimationSpeed => 1.25f;

	protected virtual float m_shoot_AttackHold_Delay => 0.4f;

	protected virtual float m_shoot_Exit_AnimationSpeed => 1f;

	protected virtual float m_shoot_Exit_Delay => 0f;

	protected virtual float m_shoot_Exit_ForceIdle => 0.35f;

	protected virtual float m_shoot_Exit_AttackCD => 0f;

	protected virtual float m_shoot_BeforeTellDelay => 0f;

	protected virtual float m_shoot_AfterAttackDelay => 0f;

	protected virtual int m_shoot_TotalShots => 1;

	protected virtual float m_shoot_MultiShotDelay => 0.15f;

	protected virtual bool m_shoot_ShootNear => true;

	protected virtual bool m_shoot_ShootFar => true;

	protected virtual bool m_shoot_ShootMirror => false;

	protected virtual float m_dash_TellIntro_AnimSpeed => 1f;

	protected virtual float m_dash_TellIntro_Delay => 0f;

	protected virtual float m_dash_TellHold_AnimSpeed => 1.5f;

	protected virtual float m_dash_TellHold_Delay => 0.65f;

	protected virtual float m_dash_AttackIntro_AnimSpeed => 2.75f;

	protected virtual float m_dash_AttackIntro_Delay => 0.5f;

	protected virtual float m_dash_AttackHold_AnimSpeed => 2.75f;

	protected virtual float m_dash_AttackHold_Delay => 0f;

	protected virtual float m_dash_Exit_AnimSpeed => 1f;

	protected virtual float m_dash_Exit_Delay => 0f;

	protected virtual float m_dash_Exit_ForceIdle => 0.25f;

	protected virtual float m_dash_Exit_AttackCD => 1f;

	protected virtual float m_dash_Attack_Speed => 15f;

	protected virtual float m_dash_Tell_Duration => 1f;

	protected virtual float m_dash_Attack_Duration => 1f;

	protected virtual bool m_dropsBonesDuringDashAttack => true;

	protected virtual int m_bonesDroppedDuringDashCount => 4;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[1] { "FlyingSkullBoneProjectile" };
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator ShootFireball()
	{
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		base.EnemyController.AlwaysFacing = false;
		if (m_shoot_BeforeTellDelay > 0f)
		{
			yield return Wait(m_shoot_BeforeTellDelay);
		}
		AudioManager.PlayOneShotAttached(this, "event:/SFX/Enemies/sfx_enemy_flyingSkull_shoot_prep", base.gameObject);
		yield return Default_TellIntroAndLoop("MultiShot_Tell_Intro", m_shoot_TellIntro_AnimationSpeed, "MultiShot_Tell_Hold", m_shoot_TellHold_AnimationSpeed, m_shoot_Tell_Delay);
		AudioManager.PlayOneShotAttached(this, "event:/SFX/Enemies/sfx_enemy_flyingSkull_shoot", base.gameObject);
		yield return Default_Animation("MultiShot_Attack_Intro", m_shoot_AttackIntro_AnimationSpeed, m_shoot_AttackIntro_Delay);
		yield return Default_Animation("MultiShot_Attack_Hold", m_shoot_AttackHold_AnimationSpeed, 0f, waitUntilAnimationComplete: false);
		for (int i = 0; i < m_shoot_TotalShots; i++)
		{
			if (m_shoot_ShootNear)
			{
				this.FireProjectile("FlyingSkullBoneProjectile", 0, matchFacing: true, 90 - m_NearBone_Angle);
				if (m_shoot_ShootMirror)
				{
					this.FireProjectile("FlyingSkullBoneProjectile", 0, matchFacing: true, 90 + m_NearBone_Angle);
				}
			}
			if (m_shoot_ShootFar)
			{
				this.FireProjectile("FlyingSkullBoneProjectile", 0, matchFacing: true, 90 - m_FarBoneAngle);
				if (m_shoot_ShootMirror)
				{
					this.FireProjectile("FlyingSkullBoneProjectile", 0, matchFacing: true, 90 + m_FarBoneAngle);
				}
			}
			if (m_shoot_MultiShotDelay > 0f)
			{
				yield return Wait(m_shoot_MultiShotDelay);
			}
		}
		if (m_shoot_AfterAttackDelay > 0f)
		{
			yield return Wait(m_shoot_AfterAttackDelay);
		}
		yield return Default_Animation("MultiShot_Exit", m_shoot_Exit_AnimationSpeed, m_shoot_Exit_Delay);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		yield return Default_Attack_Cooldown(m_shoot_Exit_ForceIdle, m_shoot_Exit_AttackCD);
		base.EnemyController.AlwaysFacing = true;
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator DashAttack()
	{
		this.ToDo("DASH!");
		StopAndFaceTarget();
		base.EnemyController.BaseTurnSpeed = 0f;
		base.EnemyController.FollowTarget = false;
		base.EnemyController.ForceFaceTarget();
		base.EnemyController.LockFlip = true;
		AudioManager.PlayOneShotAttached(this, "event:/SFX/Enemies/sfx_enemy_flyingSkull_dash_prep", base.gameObject);
		yield return Default_TellIntroAndLoop("SingleShot_Tell_Intro", m_dash_TellIntro_AnimSpeed, "SingleShot_Tell_Hold", m_dash_TellHold_AnimSpeed, m_dash_Tell_Duration);
		LeanIntoDash();
		SetDashVelocity();
		AudioManager.PlayOneShotAttached(this, "event:/SFX/Enemies/sfx_enemy_flyingSkull_dash", base.gameObject);
		yield return Default_Animation(BaseAIScript.NEUTRAL_STATE, 1f, 0f);
		if (m_dropsBonesDuringDashAttack)
		{
			yield return DropBonesDuringDash();
		}
		else if (m_dash_Attack_Duration > 0f)
		{
			yield return Wait(m_dash_Attack_Duration);
		}
		base.EnemyController.Pivot.transform.localRotation = Quaternion.identity;
		base.EnemyController.BaseTurnSpeed = 0f;
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		if (m_dash_AttackHold_Delay > 0f)
		{
			yield return Wait(m_dash_AttackHold_Delay);
		}
		base.EnemyController.BaseSpeed = base.EnemyController.EnemyData.Speed;
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		yield return Default_Attack_Cooldown(m_dash_Exit_ForceIdle, m_dash_Exit_AttackCD);
		base.EnemyController.FollowTarget = true;
		base.EnemyController.LockFlip = false;
		base.EnemyController.ResetTurnTrigger();
		base.EnemyController.ResetBaseValues();
	}

	private void SetDashVelocity()
	{
		Vector3 vector = Vector2.left;
		if (base.EnemyController.IsFacingRight)
		{
			vector = Vector2.right;
		}
		base.EnemyController.Heading = vector;
		base.EnemyController.BaseSpeed = m_dash_Attack_Speed;
		base.EnemyController.FlyingMovementType = FlyingMovementType.Towards;
	}

	private void LeanIntoDash()
	{
		float z = 20f;
		if (base.EnemyController.IsFacingRight)
		{
			z = -20f;
		}
		base.EnemyController.Pivot.transform.localRotation = Quaternion.Euler(0f, 0f, z);
	}

	private IEnumerator DropBonesDuringDash()
	{
		float timeBetweenDrops = m_dash_Attack_Duration / (float)m_bonesDroppedDuringDashCount;
		float angle = 80f;
		if (base.EnemyController.IsFacingRight)
		{
			angle = 110f;
		}
		float elapsedDashTime = 0f;
		for (int i = 0; i < m_bonesDroppedDuringDashCount; i++)
		{
			this.FireProjectile("FlyingSkullBoneProjectile", 0, matchFacing: false, angle);
			yield return Wait(timeBetweenDrops);
			elapsedDashTime += timeBetweenDrops;
		}
		float num = m_dash_Attack_Duration - elapsedDashTime;
		if (num > 0f)
		{
			yield return Wait(num);
		}
	}
}
