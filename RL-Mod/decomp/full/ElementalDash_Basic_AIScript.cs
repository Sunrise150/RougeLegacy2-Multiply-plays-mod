using System.Collections;
using UnityEngine;

public class ElementalDash_Basic_AIScript : BaseAIScript
{
	[SerializeField]
	private int m_elementalType;

	protected const string VOID_WALL_TELL_PROJECTILE = "ElementalDashVoidZoneWarningProjectile";

	protected Projectile_RL m_voidWallTellProjectile;

	protected Projectile_RL m_voidWallTellProjectile2;

	protected Projectile_RL m_voidWallTellProjectile3;

	protected const string VOIDBALL_TELL_INTRO = "Shoot_Tell_Intro";

	protected const string VOIDBALL_TELL_HOLD = "Shoot_Tell_Hold";

	protected const string VOIDBALL_ATTACK_INTRO = "Shoot_Attack_Intro";

	protected const string VOIDBALL_ATTACK_HOLD = "Shoot_Attack_Hold";

	protected const string VOIDBALL_EXIT = "Shoot_Exit";

	protected const string VOIDBALL_PROJECTILE = "ElementalDashVoidProjectile";

	protected const string VOIDWALL_TELL_INTRO = "Spin_Tell_Intro";

	protected const string VOIDWALL_TELL_HOLD = "Spin_Tell_Hold";

	protected const string VOIDWALL_ATTACK_INTRO = "Spin_Attack_Intro";

	protected const string VOIDWALL_ATTACK_HOLD = "Spin_Attack_Hold";

	protected const string VOIDWALL_EXIT = "Spin_Exit";

	protected const string VOIDWALL_PROJECTILE = "ElementalDashVoidZoneProjectile";

	protected const string VOIDWALL_PROJECTILE_LARGE = "ElementalDashVoidZoneLargeProjectile";

	protected override Vector2 IdleDuration => new Vector2(0.25f, 0.7f);

	protected override Vector2 WalkTowardsDuration => new Vector2(0.45f, 0.85f);

	protected override Vector2 WalkAwayDuration => new Vector2(0.45f, 0.85f);

	protected override Vector2 RandomFollowOffsetX => new Vector2(-5f, 5f);

	protected override Vector2 RandomFollowOffsetY => new Vector2(2f, 5f);

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

	protected virtual float m_shoot_TotalShotDuration => 0.4f;

	protected virtual float m_shoot_RandAngleOffset => 0f;

	protected virtual bool m_shoot_EnableSpreadShot => false;

	protected virtual int m_shoot_SpreadShot_Angle => 10;

	protected virtual float m_voidwall_TellIntro_AnimSpeed => 1f;

	protected virtual float m_voidWall_TellHold_AnimSpeed => 1f;

	protected virtual float m_voidWall_Tell_Delay => 1.1f;

	protected virtual float m_voidWall_AttackIntro_AnimSpeed => 1f;

	protected virtual float m_voidWall_AttackIntro_Delay => 0f;

	protected virtual float m_voidWall_AttackHold_AnimSpeed => 1.25f;

	protected virtual float m_voidWall_AttackHold_Delay => 0.4f;

	protected virtual float m_voidWall_Exit_AnimSpeed => 1f;

	protected virtual float m_voidWall_Exit_Delay => 0f;

	protected virtual float m_voidWall_Exit_ForceIdle => 0.35f;

	protected virtual float m_voidWall_Exit_AttackCD => 0f;

	protected virtual bool m_voidWall_CreateCenterWall => true;

	protected virtual bool m_voidWall_CreateSideWalls => false;

	protected virtual float m_voidWall_SideWallsOffset => 2f;

	protected virtual bool m_voidWall_LargeWalls => false;

	protected virtual float m_voidWall_Appear_Delay => 1.25f;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[4] { "ElementalDashVoidProjectile", "ElementalDashVoidZoneProjectile", "ElementalDashVoidZoneLargeProjectile", "ElementalDashVoidZoneWarningProjectile" };
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
	public IEnumerator ShootVoidball()
	{
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		base.EnemyController.AlwaysFacing = false;
		yield return Default_TellIntroAndLoop("Shoot_Tell_Intro", m_shoot_TellIntro_AnimSpeed, "Shoot_Tell_Hold", m_shoot_TellHold_AnimSpeed, m_shoot_Tell_Delay);
		yield return Default_Animation("Shoot_Attack_Intro", m_shoot_AttackIntro_AnimSpeed, m_shoot_AttackIntro_Delay);
		yield return Default_Animation("Shoot_Attack_Hold", m_shoot_AttackHold_AnimSpeed, 0f, waitUntilAnimationComplete: false);
		float fireAngle = CDGHelper.VectorToAngle(base.EnemyController.TargetController.Midpoint - base.EnemyController.Midpoint);
		float shotInterval = m_shoot_TotalShotDuration / (float)m_shoot_TotalShots;
		for (int i = 0; i < m_shoot_TotalShots; i++)
		{
			float num = Random.Range(0f - m_shoot_RandAngleOffset, m_shoot_RandAngleOffset);
			fireAngle += num;
			if (m_shoot_EnableSpreadShot)
			{
				this.FireProjectile("ElementalDashVoidProjectile", 0, matchFacing: false, fireAngle);
				this.FireProjectile("ElementalDashVoidProjectile", 0, matchFacing: false, fireAngle + (float)m_shoot_SpreadShot_Angle);
				this.FireProjectile("ElementalDashVoidProjectile", 0, matchFacing: false, fireAngle - (float)m_shoot_SpreadShot_Angle);
				continue;
			}
			this.FireProjectile("ElementalDashVoidProjectile", 0, matchFacing: false, fireAngle);
			if (shotInterval > 0f && i < m_shoot_TotalShots - 1)
			{
				yield return Wait(shotInterval);
			}
		}
		yield return Default_Animation("Shoot_Exit", m_shoot_Exit_AnimSpeed, m_shoot_Exit_Delay);
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
	public IEnumerator VoidWall()
	{
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		base.EnemyController.AlwaysFacing = false;
		yield return Default_TellIntroAndLoop("Spin_Tell_Intro", m_voidwall_TellIntro_AnimSpeed, "Spin_Tell_Hold", m_voidWall_TellHold_AnimSpeed, m_voidWall_Tell_Delay);
		yield return Default_Animation("Spin_Attack_Intro", m_voidWall_AttackIntro_AnimSpeed, m_voidWall_AttackIntro_Delay);
		yield return Default_Animation("Spin_Attack_Hold", m_voidWall_AttackHold_AnimSpeed, 0f, waitUntilAnimationComplete: false);
		Vector3 projectilePos = base.EnemyController.TargetController.transform.position;
		m_voidWallTellProjectile = this.FireProjectile("ElementalDashVoidZoneWarningProjectile", 1, matchFacing: true, 90f);
		projectilePos.z = m_voidWallTellProjectile.transform.position.z;
		m_voidWallTellProjectile.transform.position = projectilePos;
		if (m_voidWall_CreateSideWalls)
		{
			Vector3 position = projectilePos + new Vector3(m_voidWall_SideWallsOffset, 0f, 0f);
			m_voidWallTellProjectile2 = this.FireProjectile("ElementalDashVoidZoneWarningProjectile", 1, matchFacing: true, 90f);
			position.z = m_voidWallTellProjectile2.transform.position.z;
			m_voidWallTellProjectile2.transform.position = position;
			position = projectilePos + new Vector3(0f - m_voidWall_SideWallsOffset, 0f, 0f);
			m_voidWallTellProjectile3 = this.FireProjectile("ElementalDashVoidZoneWarningProjectile", 1, matchFacing: true, 90f);
			position.z = m_voidWallTellProjectile3.transform.position.z;
			m_voidWallTellProjectile3.transform.position = position;
		}
		if (m_voidWall_Appear_Delay > 0f)
		{
			yield return Wait(m_voidWall_Appear_Delay);
		}
		StopProjectile(ref m_voidWallTellProjectile);
		if (m_voidWall_CreateSideWalls)
		{
			StopProjectile(ref m_voidWallTellProjectile2);
			StopProjectile(ref m_voidWallTellProjectile3);
		}
		if (m_voidWall_CreateCenterWall)
		{
			if (m_voidWall_LargeWalls)
			{
				Projectile_RL projectile_RL = this.FireProjectile("ElementalDashVoidZoneLargeProjectile", 0, matchFacing: false, 0f, 0f);
				projectilePos.z = projectile_RL.transform.position.z;
				projectile_RL.transform.position = projectilePos;
			}
			else if (!m_voidWall_LargeWalls)
			{
				Projectile_RL projectile_RL2 = this.FireProjectile("ElementalDashVoidZoneProjectile", 0, matchFacing: false, 0f, 0f);
				projectilePos.z = projectile_RL2.transform.position.z;
				projectile_RL2.transform.position = projectilePos;
			}
		}
		if (m_voidWall_CreateSideWalls)
		{
			Vector3 position2 = projectilePos;
			position2.x += m_voidWall_SideWallsOffset;
			Projectile_RL projectile_RL3 = this.FireProjectile("ElementalDashVoidZoneProjectile", 0, matchFacing: false);
			position2.z = projectile_RL3.transform.position.z;
			projectile_RL3.transform.position = position2;
			position2 = projectilePos;
			position2.x -= m_voidWall_SideWallsOffset;
			Projectile_RL projectile_RL4 = this.FireProjectile("ElementalDashVoidZoneProjectile", 0, matchFacing: false);
			position2.z = projectile_RL4.transform.position.z;
			projectile_RL4.transform.position = position2;
		}
		yield return Default_Animation("Spin_Exit", m_voidWall_Exit_AnimSpeed, m_voidWall_Exit_Delay);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		yield return Default_Attack_Cooldown(m_voidWall_Exit_ForceIdle, m_voidWall_Exit_AttackCD);
		base.EnemyController.AlwaysFacing = true;
	}

	public override void OnLBCompleteOrCancelled()
	{
		base.OnLBCompleteOrCancelled();
		StopProjectile(ref m_voidWallTellProjectile);
		StopProjectile(ref m_voidWallTellProjectile2);
		StopProjectile(ref m_voidWallTellProjectile3);
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
