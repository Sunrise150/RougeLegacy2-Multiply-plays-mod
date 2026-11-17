using System.Collections;
using UnityEngine;

public class Eyeball_Basic_AIScript : BaseAIScript
{
	protected const string EXPLODING_SHOT_TELL_INTRO = "Explosion_Tell_Intro";

	protected const string EXPLODING_SHOT_TELL_HOLD = "Explosion_Tell_Hold";

	protected const string EXPLODING_SHOT_ATTACK_INTRO = "Explosion_Attack_Intro";

	protected const string EXPLODING_SHOT_ATTACK_HOLD = "Explosion_Attack_Hold";

	protected const string EXPLODING_SHOT_EXIT = "Explosion_Exit";

	protected const string EXPLODING_SHOT_PROJECTILE = "EyeballBounceBoltProjectile";

	protected const string EXPLODING_SHOT_EXPERT_PROJECTILE = "EyeballBounceBoltProjectile";

	protected bool m_faceForward;

	protected virtual float SingleShot_TellIntro_AnimSpeed => 1f;

	protected virtual float SingleShot_TellHold_AnimSpeed => 1.75f;

	protected virtual float SingleShot_TellHold_Duration => 0.8f;

	protected virtual float SingleShot_AttackIntro_AnimSpeed => 1f;

	protected virtual float SingleShot_AttackIntro_Delay => 0f;

	protected virtual float SingleShot_AttackHold_AnimSpeed => 1f;

	protected virtual float SingleShot_AttackHold_Delay => 0f;

	protected virtual float SingleShot_Exit_AnimSpeed => 1f;

	protected virtual float SingleShot_Exit_Duration => 0f;

	protected virtual float SingleShot_Exit_ForceIdle => 0.15f;

	protected virtual float SingleShot_Exit_AttackCD => 0.5f;

	protected virtual float MultiShot_TellIntro_AnimSpeed => 1f;

	protected virtual float MultiShot_TellHold_AnimSpeed => 1.75f;

	protected virtual float MultiShot_TellHold_Duration => 1f;

	protected virtual float MultiShot_AttackIntro_AnimSpeed => 1f;

	protected virtual float MultiShot_AttackIntro_Delay => 0f;

	protected virtual float MultiShot_AttackHold_AnimSpeed => 1f;

	protected virtual float MultiShot_AttackHold_Delay => 0f;

	protected virtual float MultiShot_Exit_AnimSpeed => 1f;

	protected virtual float MultiShot_Exit_Duration => 0f;

	protected virtual float MultiShot_Exit_ForceIdle => 0.15f;

	protected virtual float MultiShot_Exit_AttackCD => 0.75f;

	protected virtual int MultiShot_Attack_Amount => 5;

	protected virtual float MultiShot_AttackHold_DelayBetweenShots => 0.15f;

	protected virtual float MultiShot_Attack_AngleSpread => 75f;

	protected virtual float ExplodingShot_TellIntro_AnimSpeed => 1f;

	protected virtual float ExplodingShot_TellHold_AnimSpeed => 1.75f;

	protected virtual float ExplodingShot_TellHold_Duration => 1.35f;

	protected virtual float ExplodingShot_AttackIntro_AnimSpeed => 1f;

	protected virtual float ExplodingShot_AttackIntro_Delay => 0f;

	protected virtual float ExplodingShot_AttackHold_AnimSpeed => 1f;

	protected virtual float ExplodingShot_AttackHold_Delay => 0f;

	protected virtual float ExplodingShot_Exit_AnimSpeed => 1f;

	protected virtual float ExplodingShot_Exit_Duration => 0f;

	protected virtual float ExplodingShot_Exit_ForceIdle => 0.15f;

	protected virtual float ExplodingShot_Exit_AttackCD => 7f;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[4] { "EyeballBounceBoltProjectile", "EyeballBoltProjectile", "EyeballBounceBoltProjectile", "EyeballBounceBoltProjectile" };
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		base.EnemyController.FollowTarget = true;
		base.EnemyController.Heading = Vector2.zero;
		UpdateEyeball();
		if (enemyController.IsBoss)
		{
			base.LogicController.DisableLogicActivationByDistance = true;
			m_faceForward = true;
		}
	}

	public override void OnEnemyActivated()
	{
		base.OnEnemyActivated();
		if ((bool)base.EnemyController.TargetController && !base.EnemyController.IsBoss)
		{
			base.EnemyController.Heading = base.EnemyController.TargetController.Midpoint - base.EnemyController.Midpoint;
		}
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator ShootSingleFireball()
	{
		yield return Default_TellIntroAndLoop("SingleShot_Tell_Intro", SingleShot_TellIntro_AnimSpeed, "SingleShot_Tell_Hold", SingleShot_TellHold_AnimSpeed, SingleShot_TellHold_Duration);
		yield return Default_Animation("SingleShot_Attack_Intro", SingleShot_AttackIntro_AnimSpeed, SingleShot_AttackIntro_Delay);
		base.EnemyController.FollowTarget = false;
		float angle = CDGHelper.VectorToAngle(base.EnemyController.Heading);
		if (base.LogicController.EnemyLogicType == EnemyLogicType.Expert)
		{
			this.FireProjectile("EyeballBounceBoltProjectile", 1, matchFacing: false, angle);
		}
		else
		{
			this.FireProjectile("EyeballBoltProjectile", 1, matchFacing: false, angle);
		}
		yield return Default_Animation("SingleShot_Attack_Hold", SingleShot_AttackHold_AnimSpeed, SingleShot_AttackHold_Delay);
		yield return Default_Animation("SingleShot_Exit", SingleShot_Exit_AnimSpeed, SingleShot_Exit_Duration);
		base.EnemyController.FollowTarget = true;
		yield return Default_Attack_Cooldown(SingleShot_Exit_ForceIdle, SingleShot_Exit_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator ShootMultiFireball()
	{
		yield return Default_TellIntroAndLoop("ContinuousShot_Tell_Intro", MultiShot_TellIntro_AnimSpeed, "ContinuousShot_Tell_Hold", MultiShot_TellHold_AnimSpeed, MultiShot_TellHold_Duration);
		yield return Default_Animation("ContinuousShot_Attack_Intro", MultiShot_AttackIntro_AnimSpeed, MultiShot_AttackIntro_Delay);
		base.EnemyController.FollowTarget = false;
		float fireAngle = CDGHelper.VectorToAngle(base.EnemyController.Heading);
		yield return this.ChangeAnimationState("ContinuousShot_Attack_Hold");
		this.SetAnimationSpeedMultiplier(MultiShot_AttackHold_AnimSpeed);
		for (int i = 0; i < MultiShot_Attack_Amount; i++)
		{
			this.FireProjectile("EyeballBoltProjectile", 0, matchFacing: false, fireAngle);
			if (base.LogicController.EnemyLogicType == EnemyLogicType.Expert)
			{
				this.FireProjectile("EyeballBoltProjectile", 1, matchFacing: false, fireAngle + MultiShot_Attack_AngleSpread);
				this.FireProjectile("EyeballBoltProjectile", 1, matchFacing: false, fireAngle - MultiShot_Attack_AngleSpread);
			}
			if (MultiShot_AttackHold_DelayBetweenShots > 0f)
			{
				yield return Wait(MultiShot_AttackHold_DelayBetweenShots);
			}
		}
		if (MultiShot_AttackHold_Delay > 0f)
		{
			yield return Wait(MultiShot_AttackHold_Delay);
		}
		yield return Default_Animation("ContinuousShot_Exit", MultiShot_Exit_AnimSpeed, MultiShot_Exit_Duration);
		base.EnemyController.FollowTarget = true;
		yield return Default_Attack_Cooldown(MultiShot_Exit_ForceIdle, MultiShot_Exit_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator ExplodingShot()
	{
		yield return Default_TellIntroAndLoop("Explosion_Tell_Intro", ExplodingShot_TellIntro_AnimSpeed, "Explosion_Tell_Hold", ExplodingShot_TellHold_AnimSpeed, ExplodingShot_TellHold_Duration);
		yield return Default_Animation("Explosion_Attack_Intro", ExplodingShot_AttackIntro_AnimSpeed, ExplodingShot_AttackIntro_Delay);
		float angle = CDGHelper.VectorToAngle(base.EnemyController.Heading);
		if (base.LogicController.EnemyLogicType == EnemyLogicType.Expert)
		{
			this.FireProjectile("EyeballBounceBoltProjectile", 1, matchFacing: false, angle);
			yield return Wait(0.45f);
			angle = CDGHelper.VectorToAngle(base.EnemyController.Heading);
			this.FireProjectile("EyeballBounceBoltProjectile", 1, matchFacing: false, angle);
		}
		else
		{
			this.FireProjectile("EyeballBounceBoltProjectile", 1, matchFacing: false, angle);
		}
		yield return Default_Animation("Explosion_Attack_Hold", ExplodingShot_Exit_AnimSpeed, ExplodingShot_AttackHold_Delay);
		yield return Default_Animation("Explosion_Exit", ExplodingShot_AttackHold_AnimSpeed, ExplodingShot_Exit_Duration);
		base.EnemyController.FollowTarget = true;
		yield return Default_Attack_Cooldown(ExplodingShot_Exit_ForceIdle, ExplodingShot_Exit_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[MinibossEnemy]
	public virtual IEnumerator ShootSpinningFireball()
	{
		yield return null;
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[MinibossEnemy]
	public virtual IEnumerator ShootHomingFireball()
	{
		yield return null;
	}

	private void FixedUpdate()
	{
		if (base.IsInitialized && (bool)base.EnemyController.Target)
		{
			UpdateEyeball();
		}
	}

	private void UpdateEyeball()
	{
		if (!m_faceForward)
		{
			base.EnemyController.Animator.SetFloat("LookDirectionX", base.EnemyController.HeadingX);
			base.EnemyController.Animator.SetFloat("LookDirectionY", base.EnemyController.HeadingY);
		}
		else
		{
			base.EnemyController.Animator.SetFloat("LookDirectionX", 0f);
			base.EnemyController.Animator.SetFloat("LookDirectionY", 0f);
		}
	}
}
