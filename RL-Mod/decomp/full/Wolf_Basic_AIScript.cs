using System.Collections;
using UnityEngine;

public class Wolf_Basic_AIScript : BaseAIScript
{
	protected Projectile_RL m_warningProjectile;

	protected Projectile_RL m_howlProjectile;

	protected float m_jump_TellIntro_AnimationSpeed = 1f;

	protected float m_jump_TellHold_AnimationSpeed = 1f;

	protected float m_jump_TellIntroAndHold_Delay = 0.55f;

	protected float m_jump_AttackIntro_AnimationSpeed = 1.25f;

	protected float m_jump_AttackIntro_Delay;

	protected float m_jump_AttackHold_AnimationSpeed = 1.25f;

	protected float m_jump_AttackHold_Delay;

	protected float m_jump_Exit_AnimationSpeed = 1.25f;

	protected float m_jump_Exit_Delay;

	protected float m_jump_Exit_ForceIdle;

	protected float m_jump_Exit_AttackCD = 10f;

	protected float m_dash_TellIntro_AnimationSpeed = 1.25f;

	protected float m_dash_TellHold_AnimationSpeed = 1.25f;

	protected float m_dash_TellIntroAndHold_Delay;

	protected float m_dash_TellHold_BackwardsDelay = 0.425f;

	protected float m_dash_AttackIntro_AnimationSpeed = 1.25f;

	protected float m_dash_AttackIntro_Delay;

	protected float m_dash_AttackHold_AnimationSpeed = 1.25f;

	protected float m_dash_AttackHold_Delay;

	protected float m_dash_Exit_AnimationSpeed = 0.65f;

	protected float m_dash_Exit_Delay;

	protected float m_dash_Exit_ForceIdle;

	protected float m_dash_Exit_AttackCD = 10f;

	protected override Vector2 IdleDuration => new Vector2(0.25f, 0.45f);

	protected override Vector2 WalkTowardsDuration => new Vector2(0.5f, 1f);

	protected override Vector2 WalkAwayDuration => new Vector2(0.5f, 1f);

	protected virtual float m_howl_TellIntro_AnimationSpeed => 1f;

	protected virtual float m_howl_TellHold_AnimationSpeed => 1f;

	protected virtual float m_howl_TellIntroAndHold_Delay => 1.25f;

	protected virtual float m_howl_AttackIntro_AnimationSpeed => 1f;

	protected virtual float m_howl_AttackIntro_Delay => 0f;

	protected virtual float m_howl_AttackHold_AnimationSpeed => 1f;

	protected virtual float m_howl_AttackHold_Delay => 1f;

	protected virtual float m_howl_Exit_ForceIdle => 0f;

	protected virtual float m_howl_Exit_AttackCD => 16f;

	protected virtual bool m_howl_Randomize_Howl => true;

	protected virtual Vector2 m_howl_Randomize_Howl_Timer => new Vector2(0.25f, 2.75f);

	protected virtual bool m_howl_Spawn_Projectile => false;

	protected virtual bool m_howl_At_Start => true;

	protected virtual int m_howlMaxSummons => 3;

	protected virtual Vector2 m_jumpPower => new Vector2(15f, 26f);

	protected virtual Vector2 m_dash_TellHold_BackPower => new Vector2(0f, 0f);

	protected virtual Vector2 m_dash_AttackHold_ForwardPower => new Vector2(25f, 12f);

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[8] { "WolfShoutProjectile", "WolfShoutAlternateProjectile", "WolfShoutWarningProjectile", "WolfForwardBeamProjectile", "WolfWarningForwardBeamProjectile", "WolfIceBoltProjectile", "WolfIceGravityBoltProjectile", "WolfIceBoltExplosionProjectile" };
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		base.EnemyController.LogicController.OverrideLogicDelay(0.7f);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Howl_Attack()
	{
		StopAndFaceTarget();
		yield return Wait(0.1f);
		if (m_howl_Randomize_Howl)
		{
			float seconds = Random.Range(m_howl_Randomize_Howl_Timer.x, m_howl_Randomize_Howl_Timer.y);
			Wait(seconds);
		}
		if (m_howl_Spawn_Projectile)
		{
			m_warningProjectile = this.FireProjectile("WolfShoutWarningProjectile", 1);
		}
		if (m_howl_At_Start)
		{
			yield return Default_TellIntroAndLoop("Howl_Tell_Intro", m_howl_TellIntro_AnimationSpeed, "Howl_Tell_Hold", m_howl_TellHold_AnimationSpeed, 0f);
		}
		else
		{
			yield return Default_TellIntroAndLoop("Howl_Tell_Intro", m_howl_TellIntro_AnimationSpeed, "Howl_Tell_Hold", m_howl_TellHold_AnimationSpeed, m_howl_TellIntroAndHold_Delay);
		}
		SetAttackingWithContactDamage(enable: true);
		if (m_howl_Spawn_Projectile)
		{
			StopProjectile(ref m_warningProjectile);
			m_howlProjectile = this.FireProjectile("WolfShoutAlternateProjectile", 1);
		}
		if (base.EnemyController.EnemyRank == EnemyRank.Miniboss)
		{
			this.FireProjectile("WolfIceBoltProjectile", 1, matchFacing: false, 30f);
			this.FireProjectile("WolfIceBoltProjectile", 1, matchFacing: false, 60f);
			this.FireProjectile("WolfIceBoltProjectile", 1, matchFacing: false, 90f);
			this.FireProjectile("WolfIceBoltProjectile", 1, matchFacing: false, 120f);
			this.FireProjectile("WolfIceBoltProjectile", 1, matchFacing: false, 150f);
			this.FireProjectile("WolfIceBoltProjectile", 1, matchFacing: false, 180f);
			this.FireProjectile("WolfIceBoltProjectile", 1, matchFacing: false, 210f);
			this.FireProjectile("WolfIceBoltProjectile", 1, matchFacing: false, 240f);
			this.FireProjectile("WolfIceBoltProjectile", 1, matchFacing: false, 270f);
			this.FireProjectile("WolfIceBoltProjectile", 1, matchFacing: false, 300f);
			this.FireProjectile("WolfIceBoltProjectile", 1, matchFacing: false, 330f);
			this.FireProjectile("WolfIceBoltProjectile", 1, matchFacing: false);
		}
		yield return Default_Animation("Howl_Attack_Intro", m_howl_AttackIntro_AnimationSpeed, m_howl_AttackIntro_Delay);
		yield return Default_Animation("Howl_Attack_Hold", m_howl_AttackHold_AnimationSpeed, m_howl_AttackHold_Delay);
		SetAttackingWithContactDamage(enable: false);
		if (m_howl_Spawn_Projectile)
		{
			StopProjectile(ref m_howlProjectile);
		}
		yield return Default_Animation("Howl_Exit", m_jump_Exit_AnimationSpeed, m_jump_Exit_Delay);
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_howl_Exit_ForceIdle, m_howl_Exit_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Jump_Attack()
	{
		yield return Jump();
		if (base.LogicController.EnemyLogicType == EnemyLogicType.Expert)
		{
			yield return Dash();
		}
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_jump_Exit_ForceIdle, m_jump_Exit_AttackCD);
	}

	private IEnumerator Jump()
	{
		StopAndFaceTarget();
		yield return Wait(0.1f);
		yield return Default_TellIntroAndLoop("Pounce_Tell_Intro", m_jump_TellIntro_AnimationSpeed, "Pounce_Tell_Hold", m_jump_TellHold_AnimationSpeed, m_jump_TellIntroAndHold_Delay);
		SetAttackingWithContactDamage(enable: true);
		yield return Default_Animation("Pounce_Attack_Intro", m_jump_AttackIntro_AnimationSpeed, m_jump_AttackIntro_Delay);
		yield return Single_Action_Jump(m_jumpPower.x, m_jumpPower.y);
		yield return Default_Animation("Pounce_Attack_Hold", m_jump_AttackHold_AnimationSpeed, m_jump_AttackHold_Delay, waitUntilAnimationComplete: false);
		yield return WaitUntilIsGrounded();
		if (base.LogicController.EnemyLogicType == EnemyLogicType.Miniboss)
		{
			this.FireProjectile("WolfIceGravityBoltProjectile", 3, matchFacing: true, 115f);
			this.FireProjectile("WolfIceGravityBoltProjectile", 5, matchFacing: true, 90f);
			this.FireProjectile("WolfIceGravityBoltProjectile", 4, matchFacing: true, 65f);
		}
		SetVelocityX(0f, additive: false);
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Animation("Pounce_Exit", m_jump_Exit_AnimationSpeed, m_jump_Exit_Delay);
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
		yield return Dash();
		if (base.LogicController.EnemyLogicType == EnemyLogicType.Expert)
		{
			yield return Jump();
		}
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_dash_Exit_ForceIdle, m_dash_Exit_AttackCD);
	}

	private IEnumerator Dash()
	{
		StopAndFaceTarget();
		yield return Wait(0.1f);
		yield return Single_Action_Jump(m_dash_TellHold_BackPower.x, m_dash_TellHold_BackPower.y);
		yield return Default_TellIntroAndLoop("Dash_Tell_Intro", m_dash_TellIntro_AnimationSpeed, "Dash_Tell_Hold", m_dash_TellHold_AnimationSpeed, m_dash_TellIntroAndHold_Delay);
		SetAttackingWithContactDamage(enable: true);
		if (m_dash_TellHold_BackwardsDelay > 0f)
		{
			yield return Wait(m_dash_TellHold_BackwardsDelay);
		}
		yield return Default_Animation("Dash_Attack_Intro", m_dash_AttackIntro_AnimationSpeed, m_dash_AttackIntro_Delay);
		yield return Single_Action_Jump(m_dash_AttackHold_ForwardPower.x, m_dash_AttackHold_ForwardPower.y);
		yield return Default_Animation("Dash_Attack_Hold", m_dash_AttackHold_AnimationSpeed, m_dash_AttackHold_Delay, waitUntilAnimationComplete: false);
		yield return WaitUntilIsGrounded();
		if (base.LogicController.EnemyLogicType == EnemyLogicType.Miniboss)
		{
			this.FireProjectile("WolfIceGravityBoltProjectile", 3, matchFacing: true, 115f);
			this.FireProjectile("WolfIceGravityBoltProjectile", 5, matchFacing: true, 90f);
			this.FireProjectile("WolfIceGravityBoltProjectile", 4, matchFacing: true, 65f);
		}
		SetVelocityX(0f, additive: false);
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Animation("Dash_Exit", m_dash_Exit_AnimationSpeed, m_dash_Exit_Delay);
	}

	public IEnumerator Single_Action_Jump(float jumpX, float jumpY)
	{
		if (base.EnemyController.IsFacingRight)
		{
			SetVelocityX(jumpX, additive: false);
		}
		else
		{
			SetVelocityX(0f - jumpX, additive: false);
		}
		SetVelocityY(jumpY, additive: false);
		yield return Wait(0.05f);
		if (base.EnemyController.IsFacingRight)
		{
			base.EnemyController.JumpHorizontalVelocity = jumpX;
		}
		else
		{
			base.EnemyController.JumpHorizontalVelocity = 0f - jumpX;
		}
	}

	public override void OnLBCompleteOrCancelled()
	{
		base.OnLBCompleteOrCancelled();
		StopProjectile(ref m_warningProjectile);
		StopProjectile(ref m_howlProjectile);
	}
}
