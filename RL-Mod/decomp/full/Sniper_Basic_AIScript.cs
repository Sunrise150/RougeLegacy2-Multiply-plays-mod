using System.Collections;
using UnityEngine;

public class Sniper_Basic_AIScript : BaseAIScript
{
	protected float m_shoot_TellIntro_AnimationSpeed = 1f;

	protected float m_shoot_TellHold_AnimationSpeed = 2f;

	protected float m_shoot_TellHold_Delay = 0.5f;

	protected float m_shoot_AttackIntro_AnimationSpeed = 1f;

	protected float m_shoot_AttackIntro_Delay;

	protected float m_shoot_AttackHold_AnimationSpeed = 1f;

	protected float m_shoot_AttackHold_Delay;

	protected float m_shoot_Exit_AnimationSpeed = 1f;

	protected float m_shoot_Exit_Delay;

	protected float m_shoot_Exit_ForceIdle = 1.25f;

	protected float m_shoot_Exit_AttackCD;

	private SpriteRenderer m_aimIndicator;

	private bool m_aimFollowTarget = true;

	private float m_reaimStartTime;

	protected const string SINGLE_SHOT_TELL_INTRO = "SingleShot_Tell_Intro";

	protected const string SINGLE_SHOT_TELL_HOLD = "SingleShot_Tell_Hold";

	protected const string SINGLE_SHOT_ATTACK_INTRO = "SingleShot_Attack_Intro";

	protected const string SINGLE_SHOT_ATTACK_HOLD = "SingleShot_Attack_Hold";

	protected const string SINGLE_SHOT_EXIT = "SingleShot_Exit";

	protected const string SINGLE_SHOT_PROJECTILE = "SniperBoltProjectile";

	protected override Vector2 IdleDuration => new Vector2(0.25f, 0.7f);

	protected override Vector2 WalkTowardsDuration => new Vector2(0.45f, 0.85f);

	protected override Vector2 WalkAwayDuration => new Vector2(0.45f, 0.85f);

	protected virtual float m_shoot_AimDelayDuration => 0.75f;

	protected virtual float m_shoot_FireDelayDuration => 0f;

	protected virtual int AimShot_TotalShots => 1;

	protected virtual float AimShot_MultiShotDelay => 0.075f;

	protected virtual bool AimShot_FireSideShot => false;

	protected virtual bool AimShot_FireSlashBolts => true;

	protected virtual float AimShot_SideShotSpread => 4f;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[2] { "SniperBoltProjectile", "SniperSlashBoltProjectile" };
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		GameObject @object = enemyController.GetComponent<ObjectReferenceFinder>().GetObject("AimIndicator");
		m_aimIndicator = @object.GetComponent<SpriteRenderer>();
		m_aimIndicator.transform.SetParent(base.EnemyController.Pivot.transform, worldPositionStays: true);
		m_aimIndicator.gameObject.SetActive(value: false);
		m_aimFollowTarget = true;
		FacePlayerImmediately();
	}

	private void OnEnable()
	{
		if ((bool)m_aimIndicator)
		{
			m_aimIndicator.gameObject.SetActive(value: false);
		}
		if ((bool)base.EnemyController && base.EnemyController.IsInitialized)
		{
			FacePlayerImmediately();
		}
	}

	private void FacePlayerImmediately()
	{
		Vector3 localEulerAngles = base.EnemyController.Pivot.transform.localEulerAngles;
		localEulerAngles.z = CDGHelper.AngleBetweenPts(base.EnemyController.Midpoint, base.EnemyController.TargetController.Midpoint);
		base.EnemyController.Pivot.transform.localEulerAngles = localEulerAngles;
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator AimAndFire()
	{
		if (m_shoot_AimDelayDuration > 0f)
		{
			yield return Wait(m_shoot_AimDelayDuration);
		}
		m_aimFollowTarget = false;
		m_aimIndicator.gameObject.SetActive(value: true);
		base.Animator.SetBool("Armed", value: true);
		yield return Default_TellIntroAndLoop("SingleShot_Tell_Intro", m_shoot_TellIntro_AnimationSpeed, "SingleShot_Tell_Hold", m_shoot_TellHold_AnimationSpeed, m_shoot_TellHold_Delay);
		yield return Default_Animation("SingleShot_Attack_Intro", m_shoot_AttackIntro_AnimationSpeed, m_shoot_AttackIntro_Delay);
		if (m_shoot_FireDelayDuration > 0f)
		{
			yield return Wait(m_shoot_FireDelayDuration);
		}
		float fireAngle = base.EnemyController.Pivot.transform.localEulerAngles.z;
		for (int i = 0; i < AimShot_TotalShots; i++)
		{
			if (AimShot_FireSlashBolts)
			{
				this.FireProjectile("SniperSlashBoltProjectile", 0, matchFacing: false, fireAngle);
				if (AimShot_FireSideShot)
				{
					this.FireProjectile("SniperSlashBoltProjectile", 0, matchFacing: false, fireAngle + AimShot_SideShotSpread);
					this.FireProjectile("SniperSlashBoltProjectile", 0, matchFacing: false, fireAngle - AimShot_SideShotSpread);
				}
			}
			else
			{
				this.FireProjectile("SniperBoltProjectile", 0, matchFacing: false, fireAngle);
				if (AimShot_FireSideShot)
				{
					this.FireProjectile("SniperBoltProjectile", 0, matchFacing: false, fireAngle + AimShot_SideShotSpread);
					this.FireProjectile("SniperBoltProjectile", 0, matchFacing: false, fireAngle - AimShot_SideShotSpread);
				}
			}
			if (AimShot_MultiShotDelay > 0f)
			{
				yield return Wait(AimShot_MultiShotDelay);
			}
		}
		m_aimIndicator.gameObject.SetActive(value: false);
		base.Animator.SetBool("Armed", value: false);
		yield return Default_Animation("SingleShot_Attack_Hold", m_shoot_AttackHold_AnimationSpeed, m_shoot_AttackHold_Delay, waitUntilAnimationComplete: false);
		yield return Default_Animation("SingleShot_Exit", m_shoot_Exit_AnimationSpeed, m_shoot_Exit_Delay);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		m_aimFollowTarget = true;
		m_reaimStartTime = Time.time;
		yield return Default_Attack_Cooldown(m_shoot_Exit_ForceIdle, m_shoot_Exit_AttackCD);
	}

	public override void OnLBCompleteOrCancelled()
	{
		base.Animator.SetBool("Armed", value: false);
		m_aimFollowTarget = true;
		base.OnLBCompleteOrCancelled();
		m_aimIndicator.gameObject.SetActive(value: false);
	}

	public override void Unpause()
	{
		base.Unpause();
		m_reaimStartTime = Time.time;
	}

	private void Update()
	{
		if (base.IsInitialized && !base.IsPaused && m_aimFollowTarget)
		{
			float z = CDGHelper.AngleBetweenPts(base.EnemyController.Midpoint, base.EnemyController.TargetController.Midpoint);
			Vector3 localEulerAngles = new Vector3(0f, 0f, z);
			float num = 1f;
			float num2 = m_reaimStartTime + num;
			if (Time.time < num2)
			{
				Vector3 localEulerAngles2 = base.EnemyController.Pivot.transform.localEulerAngles;
				localEulerAngles2.z = Mathf.LerpAngle(localEulerAngles2.z, localEulerAngles.z, 1f - (num2 - Time.time / num));
				base.EnemyController.Pivot.transform.localEulerAngles = localEulerAngles2;
			}
			else
			{
				base.EnemyController.Pivot.transform.localEulerAngles = localEulerAngles;
			}
			float num3 = CDGHelper.DistanceBetweenPts(base.EnemyController.Midpoint, base.EnemyController.TargetController.Midpoint);
			Vector2 direction = (base.EnemyController.TargetController.Midpoint - base.EnemyController.Midpoint).normalized;
			RaycastHit2D raycastHit2D = Physics2D.Raycast(base.EnemyController.Midpoint, direction, num3, base.EnemyController.ControllerCorgi.PlatformMask);
			if ((bool)raycastHit2D)
			{
				num3 = raycastHit2D.distance;
			}
			Vector2 size = m_aimIndicator.size;
			size.x = num3 * (1f / m_aimIndicator.transform.lossyScale.x);
			m_aimIndicator.size = size;
		}
	}
}
