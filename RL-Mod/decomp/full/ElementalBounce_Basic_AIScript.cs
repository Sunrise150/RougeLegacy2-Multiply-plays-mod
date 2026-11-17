using System.Collections;
using UnityEngine;

public class ElementalBounce_Basic_AIScript : BaseAIScript
{
	[SerializeField]
	private int m_elementalType;

	protected Projectile_RL m_bounceBallTellProjectile;

	protected Projectile_RL m_bounceBallTellProjectile2;

	protected Projectile_RL m_bounceBallTellProjectile3;

	protected Projectile_RL m_bounceBallTellProjectile4;

	protected Projectile_RL m_bounceBallTellProjectile5;

	protected const string FIRESPIRE_TELL_INTRO = "Spin_Tell_Intro";

	protected const string FIRESPIRE_TELL_HOLD = "Spin_Tell_Hold";

	protected const string FIRESPIRE_ATTACK_INTRO = "Spin_Attack_Intro";

	protected const string FIRESPIRE_ATTACK_HOLD = "Spin_Attack_Hold";

	protected const string FIRESPIRE_EXIT = "Spin_Exit";

	protected const string FIRESPIRE_PROJECTILE = "ElementalBounceBounceFragmentProjectile";

	protected const string FIRESPIRE_HARD_PROJECTILE = "ElementalBounceBounceFragmentHomingProjectile";

	protected const string FIRESPIRE_TELL_PROJECTILE = "ElementalBounceZoneWarningProjectile";

	protected const string BOUNCEBALL_TELL_INTRO = "Shoot_Tell_Intro";

	protected const string BOUNCEBALL_TELL_HOLD = "Shoot_Tell_Hold";

	protected const string BOUNCEBALL_ATTACK_INTRO = "Shoot_Attack_Intro";

	protected const string BOUNCEBALL_ATTACK_HOLD = "Shoot_Attack_Hold";

	protected const string BOUNCEBALL_EXIT = "Shoot_Exit";

	protected const string BOUNCEBALL_PROJECTILE = "ElementalBounceBounceProjectile";

	protected const string BOUNCEBALL_HARD_PROJECTILE = "ElementalBounceBounceHomingProjectile";

	protected const string BOUNCEBALL_FRAGMENT_PROJECTILE = "ElementalBounceBounceFragmentProjectile";

	protected const string BOUNCEBALL_FRAGMENT_HOMING_PROJECTILE = "ElementalBounceBounceFragmentHomingProjectile";

	protected override Vector2 IdleDuration => new Vector2(0.25f, 0.7f);

	protected override Vector2 WalkTowardsDuration => new Vector2(1.25f, 2f);

	protected override Vector2 WalkAwayDuration => new Vector2(1.25f, 2f);

	protected override Vector2 RandomFollowOffsetX => new Vector2(3.5f, 10f);

	protected override Vector2 RandomFollowOffsetY => new Vector2(1f, 6f);

	protected virtual float m_fireSpire_TellIntro_AnimSpeed => 1f;

	protected virtual float m_fireSpire_TellHold_AnimSpeed => 1f;

	protected virtual float m_fireSpire_Tell_Delay => 1.15f;

	protected virtual float m_fireSpire_AttackIntro_AnimSpeed => 1f;

	protected virtual float m_fireSpire_AttackIntro_Delay => 0f;

	protected virtual float m_fireSpire_AttackHold_AnimSpeed => 0.4f;

	protected virtual float m_fireSpire_AttackHold_Delay => 0.4f;

	protected virtual float m_fireSpire_Exit_AnimSpeed => 1f;

	protected virtual float m_fireSpire_Exit_Delay => 0f;

	protected virtual float m_fireSpire_Exit_ForceIdle => 0.75f;

	protected virtual float m_fireSpire_Exit_AttackCD => 5f;

	protected virtual int m_fireSpire_TotalShots => 1;

	protected virtual float m_fireSpire_Appear_Delay => 0f;

	protected virtual bool m_fireSpire_Hard_Version => false;

	protected virtual float m_shoot_TellIntro_AnimSpeed => 1f;

	protected virtual float m_shoot_TellHold_AnimSpeed => 1f;

	protected virtual float m_shoot_Tell_Delay => 1.1f;

	protected virtual float m_shoot_AttackIntro_AnimSpeed => 1f;

	protected virtual float m_shoot_AttackIntro_Delay => 0f;

	protected virtual float m_shoot_AttackHold_AnimSpeed => 1.25f;

	protected virtual float m_shoot_AttackHold_Delay => 0.4f;

	protected virtual float m_shoot_Exit_AnimSpeed => 1f;

	protected virtual float m_shoot_Exit_Delay => 0f;

	protected virtual float m_shoot_Exit_ForceIdle => 0.75f;

	protected virtual float m_shoot_Exit_AttackCD => 0f;

	protected virtual int m_shoot_TotalShots => 1;

	protected virtual float m_shoot_Appear_Delay => 0f;

	protected virtual bool m_shoot_Hard_Version => false;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[7] { "ElementalBounceBounceProjectile", "ElementalBounceBounceHomingProjectile", "ElementalBounceBounceFragmentProjectile", "ElementalBounceBounceFragmentHomingProjectile", "ElementalBounceBounceFragmentProjectile", "ElementalBounceBounceFragmentHomingProjectile", "ElementalBounceZoneWarningProjectile" };
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		enemyController.Animator.SetInteger("ElementalType", m_elementalType);
	}

	private void OnEnable()
	{
		if (base.IsInitialized)
		{
			base.EnemyController.Animator.SetInteger("ElementalType", m_elementalType);
		}
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator SpinAttack()
	{
		string projectileName = "ElementalBounceBounceFragmentProjectile";
		if (m_fireSpire_Hard_Version)
		{
			projectileName = "ElementalBounceBounceFragmentHomingProjectile";
		}
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		base.EnemyController.AlwaysFacing = false;
		Vector3 projectilePos = base.EnemyController.TargetController.transform.position;
		Vector3 projectilePos2 = base.EnemyController.TargetController.transform.position;
		Vector3 projectilePos3 = base.EnemyController.TargetController.transform.position;
		Vector3 projectilePos4 = base.EnemyController.TargetController.transform.position;
		Vector3 projectilePos5 = base.EnemyController.TargetController.transform.position;
		float groundYPos = GetTargetGroundYPos();
		projectilePos.y = groundYPos + 2f;
		projectilePos2.y = groundYPos + 2f;
		projectilePos3.y = groundYPos + 2f;
		projectilePos4.y = groundYPos + 2f;
		projectilePos5.y = groundYPos + 2f;
		projectilePos2.x -= 1.5f;
		projectilePos3.x += 1.5f;
		projectilePos4.x -= 3f;
		projectilePos5.x += 3f;
		m_bounceBallTellProjectile = this.FireProjectile("ElementalBounceZoneWarningProjectile", 1, matchFacing: true, 90f);
		projectilePos.z = m_bounceBallTellProjectile.transform.position.z;
		m_bounceBallTellProjectile.transform.position = projectilePos;
		m_bounceBallTellProjectile2 = this.FireProjectile("ElementalBounceZoneWarningProjectile", 1, matchFacing: true, 90f);
		projectilePos2.z = m_bounceBallTellProjectile2.transform.position.z;
		m_bounceBallTellProjectile2.transform.position = projectilePos2;
		m_bounceBallTellProjectile3 = this.FireProjectile("ElementalBounceZoneWarningProjectile", 1, matchFacing: true, 90f);
		projectilePos3.z = m_bounceBallTellProjectile3.transform.position.z;
		m_bounceBallTellProjectile3.transform.position = projectilePos3;
		m_bounceBallTellProjectile4 = this.FireProjectile("ElementalBounceZoneWarningProjectile", 1, matchFacing: true, 90f);
		projectilePos3.z = m_bounceBallTellProjectile3.transform.position.z;
		m_bounceBallTellProjectile4.transform.position = projectilePos4;
		m_bounceBallTellProjectile5 = this.FireProjectile("ElementalBounceZoneWarningProjectile", 1, matchFacing: true, 90f);
		projectilePos3.z = m_bounceBallTellProjectile3.transform.position.z;
		m_bounceBallTellProjectile5.transform.position = projectilePos5;
		yield return Default_TellIntroAndLoop("Spin_Tell_Intro", m_fireSpire_TellIntro_AnimSpeed, "Spin_Tell_Hold", m_fireSpire_TellHold_AnimSpeed, m_fireSpire_Tell_Delay);
		yield return Default_Animation("Spin_Attack_Intro", m_fireSpire_AttackIntro_AnimSpeed, m_fireSpire_AttackIntro_Delay);
		yield return Default_Animation("Spin_Attack_Hold", m_fireSpire_AttackHold_AnimSpeed, 0f, waitUntilAnimationComplete: false);
		for (int i = 0; i < m_fireSpire_TotalShots; i++)
		{
			projectilePos.y = groundYPos + 0.35f;
			projectilePos2.y = groundYPos + 0.35f;
			projectilePos3.y = groundYPos + 0.35f;
			projectilePos4.y = groundYPos + 0.35f;
			projectilePos5.y = groundYPos + 0.35f;
			if (m_fireSpire_Appear_Delay > 0f)
			{
				yield return Wait(m_fireSpire_Appear_Delay);
			}
			StopProjectile(ref m_bounceBallTellProjectile);
			StopProjectile(ref m_bounceBallTellProjectile2);
			StopProjectile(ref m_bounceBallTellProjectile3);
			StopProjectile(ref m_bounceBallTellProjectile4);
			StopProjectile(ref m_bounceBallTellProjectile5);
			Projectile_RL projectile_RL = this.FireProjectile(projectileName, 0, matchFacing: false, 90f);
			projectilePos.z = projectile_RL.transform.position.z;
			projectile_RL.transform.position = projectilePos;
			projectile_RL = this.FireProjectile(projectileName, 0, matchFacing: false, 90f);
			projectilePos2.z = projectile_RL.transform.position.z;
			projectile_RL.transform.position = projectilePos2;
			projectile_RL = this.FireProjectile(projectileName, 0, matchFacing: false, 90f);
			projectilePos3.z = projectile_RL.transform.position.z;
			projectile_RL.transform.position = projectilePos3;
			projectile_RL = this.FireProjectile(projectileName, 0, matchFacing: false, 90f);
			projectilePos5.z = projectile_RL.transform.position.z;
			projectile_RL.transform.position = projectilePos4;
			projectile_RL = this.FireProjectile(projectileName, 0, matchFacing: false, 90f);
			projectilePos5.z = projectile_RL.transform.position.z;
			projectile_RL.transform.position = projectilePos5;
		}
		yield return Default_Animation("Spin_Exit", m_fireSpire_Exit_AnimSpeed, m_fireSpire_Exit_Delay);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		yield return Default_Attack_Cooldown(m_fireSpire_Exit_ForceIdle, m_fireSpire_Exit_AttackCD);
		base.EnemyController.AlwaysFacing = true;
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator ShootBounceball()
	{
		string projectileName = "ElementalBounceBounceProjectile";
		if (m_shoot_Hard_Version)
		{
			projectileName = "ElementalBounceBounceHomingProjectile";
		}
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		base.EnemyController.AlwaysFacing = false;
		yield return Default_TellIntroAndLoop("Shoot_Tell_Intro", m_shoot_TellIntro_AnimSpeed, "Shoot_Tell_Hold", m_shoot_TellHold_AnimSpeed, m_shoot_Tell_Delay);
		yield return Default_Animation("Shoot_Attack_Intro", m_shoot_AttackIntro_AnimSpeed, m_shoot_AttackIntro_Delay);
		yield return Default_Animation("Shoot_Attack_Hold", m_shoot_AttackHold_AnimSpeed, 0f, waitUntilAnimationComplete: false);
		for (int i = 0; i < m_shoot_TotalShots; i++)
		{
			if (m_shoot_Appear_Delay > 0f)
			{
				yield return Wait(m_shoot_Appear_Delay);
			}
			float angle = CDGHelper.VectorToAngle(base.EnemyController.TargetController.Midpoint - base.EnemyController.Midpoint);
			this.FireProjectile(projectileName, 0, matchFacing: false, angle);
		}
		yield return Default_Animation("Shoot_Exit", m_shoot_Exit_AnimSpeed, m_shoot_Exit_Delay);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		yield return Default_Attack_Cooldown(m_shoot_Exit_ForceIdle, m_shoot_Exit_AttackCD);
		base.EnemyController.AlwaysFacing = true;
	}

	public override void OnLBCompleteOrCancelled()
	{
		base.OnLBCompleteOrCancelled();
		StopProjectile(ref m_bounceBallTellProjectile);
		StopProjectile(ref m_bounceBallTellProjectile2);
		StopProjectile(ref m_bounceBallTellProjectile3);
		StopProjectile(ref m_bounceBallTellProjectile4);
		StopProjectile(ref m_bounceBallTellProjectile5);
	}

	private float GetTargetGroundYPos()
	{
		LayerMask layerMask = 256;
		layerMask = (int)layerMask | 0x800;
		RaycastHit2D raycastHit2D = Physics2D.Raycast(base.EnemyController.TargetController.Midpoint, Vector2.down, 100f, layerMask);
		if ((bool)raycastHit2D)
		{
			return raycastHit2D.point.y;
		}
		return base.EnemyController.TargetController.transform.position.y;
	}
}
