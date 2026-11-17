using System.Collections;
using RLAudio;
using UnityEngine;

public class Wisp_Basic_AIScript : BaseAIScript
{
	private WispAudioEventEmitterController m_audioEmitterController;

	protected const string FIREBALL_PROJECTILE = "FireballEnemyProjectile";

	protected const string DASH_TELL_INTRO = "Dash_Tell_Intro";

	protected const string DASH_TELL_HOLD = "Dash_Tell_Hold";

	protected const string DASH_ATTACK_INTRO = "Dash_Attack_Intro";

	protected const string DASH_ATTACK_HOLD = "Dash_Attack_Hold";

	protected const string DASH_EXIT = "Dash_Exit";

	public override bool ForceDeathAnimation => true;

	protected override Vector2 IdleDuration => new Vector2(0.35f, 0.75f);

	protected override Vector2 WalkTowardsDuration => new Vector2(0.2f, 0.4f);

	protected override Vector2 WalkAwayDuration => new Vector2(0.2f, 0.4f);

	protected override Vector2 RandomFollowOffsetX => new Vector2(-4f, 4f);

	protected override Vector2 RandomFollowOffsetY => new Vector2(-4f, 4f);

	protected override float IdleAnimSpeedMod => 1f;

	protected override float WalkAnimSpeedMod => 1f;

	protected override float ForcedIdleDuration_IfReversingDirection => 0f;

	protected virtual float m_fireballSpeedMultiplier => 1f;

	protected virtual bool m_dropsFireballsWhileWalking => false;

	protected virtual float m_timeBetweenWalkTowardFireballDrops => 2f;

	protected virtual float m_dash_TellIntro_AnimSpeed => 1f;

	protected virtual float m_dash_TellIntro_Delay => 0f;

	protected virtual float m_dash_TellHold_AnimSpeed => 1f;

	protected virtual float m_dash_TellHold_Delay => 1f;

	protected virtual float m_dash_AttackIntro_AnimSpeed => 1f;

	protected virtual float m_dash_AttackIntro_Delay => 0f;

	protected virtual float m_dash_AttackHold_AnimSpeed => 1f;

	protected virtual float m_dash_AttackHold_Delay => 0f;

	protected virtual float m_dash_Exit_AnimSpeed => 1f;

	protected virtual float m_dash_Exit_Delay => 0f;

	protected virtual float m_dash_Exit_ForceIdle => 0f;

	protected virtual float m_dash_Exit_AttackCD => 8f;

	protected virtual float m_dash_Attack_ForwardSpeedMod => 3.25f;

	protected virtual float m_dash_Attack_Duration => 0.275f;

	protected virtual bool m_dropsFireballsDuringDashAttack => false;

	protected virtual float m_fireballDropDuringDashInterval => 0.05f;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[1] { "FireballEnemyProjectile" };
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		base.EnemyController.LogicController.OverrideLogicDelay(0.7f);
		m_audioEmitterController = base.EnemyController.GetComponent<WispAudioEventEmitterController>();
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
		StopAndFaceTarget();
		yield return Default_Animation("Dash_Tell_Intro", m_dash_TellIntro_AnimSpeed, m_dash_TellIntro_Delay, waitUntilAnimationComplete: false);
		yield return Default_Animation("Dash_Tell_Hold", m_dash_TellHold_AnimSpeed, m_dash_TellHold_Delay, waitUntilAnimationComplete: false);
		base.EnemyController.Heading = base.EnemyController.TargetController.Midpoint - base.EnemyController.Midpoint;
		base.EnemyController.LockFlip = true;
		base.EnemyController.FollowTarget = false;
		m_audioEmitterController.PlayDash();
		yield return Default_Animation("Dash_Attack_Intro", m_dash_AttackIntro_AnimSpeed, m_dash_AttackIntro_Delay);
		yield return Default_Animation("Dash_Attack_Hold", m_dash_AttackHold_AnimSpeed, m_dash_AttackHold_Delay, waitUntilAnimationComplete: false);
		base.EnemyController.FlyingMovementType = FlyingMovementType.Towards;
		base.EnemyController.BaseSpeed = base.EnemyController.BaseSpeed * m_dash_Attack_ForwardSpeedMod;
		if (m_dropsFireballsDuringDashAttack)
		{
			yield return DropFireballDuringDash();
		}
		else if (m_dash_Attack_Duration > 0f)
		{
			yield return Wait(m_dash_Attack_Duration);
		}
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		yield return Default_Animation("Dash_Exit", m_dash_Exit_AnimSpeed, m_dash_Exit_Delay);
		yield return Default_Attack_Cooldown(m_dash_Exit_ForceIdle, m_dash_Exit_AttackCD);
		m_audioEmitterController.StopDash();
		base.EnemyController.ResetTurnTrigger();
		base.EnemyController.ResetBaseValues();
		base.EnemyController.LockFlip = false;
	}

	private IEnumerator DropFireballDuringDash()
	{
		int numFireballs = (int)(m_dash_Attack_Duration / m_fireballDropDuringDashInterval);
		float remainingTime = m_dash_Attack_Duration - m_fireballDropDuringDashInterval * (float)numFireballs;
		for (int i = 0; i < numFireballs; i++)
		{
			DropFireball();
			if (m_fireballDropDuringDashInterval > 0f)
			{
				yield return Wait(m_fireballDropDuringDashInterval);
			}
		}
		if (remainingTime > 0f)
		{
			yield return Wait(remainingTime);
		}
	}

	private void DropFireball()
	{
		float angle = 270f;
		if (base.EnemyController.EnemyRank == EnemyRank.Expert || base.EnemyController.EnemyRank == EnemyRank.Miniboss)
		{
			angle = CDGHelper.VectorToAngle(PlayerManager.GetPlayer().transform.position - base.EnemyController.transform.position);
		}
		this.FireProjectile("FireballEnemyProjectile", 0, matchFacing: false, angle, m_fireballSpeedMultiplier);
	}

	public override IEnumerator DeathAnim()
	{
		yield return base.DeathAnim();
		if (base.EnemyController.EnemyRank == EnemyRank.Miniboss)
		{
			Vector2 spawnOffset = new Vector2(Random.Range(-1, 1), Random.Range(-1, 1));
			this.SummonEnemy_NoYield(EnemyType.Wisp, EnemyRank.Expert, spawnOffset, usesAbsPos: false, runSummonAnim: false);
			spawnOffset = new Vector2(Random.Range(-1, 1), Random.Range(-1, 1));
			this.SummonEnemy_NoYield(EnemyType.Wisp, EnemyRank.Expert, spawnOffset, usesAbsPos: false, runSummonAnim: false);
		}
	}
}
