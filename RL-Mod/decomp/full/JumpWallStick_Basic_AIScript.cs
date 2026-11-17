using System.Collections;
using UnityEngine;

public class JumpWallStick_Basic_AIScript : BaseAIScript
{
	private WallStickCollision m_wallStickCollision;

	protected float m_jump_Tell_Delay = 0.4f;

	protected float m_jump_Tell_AnimationSpeed = 0.75f;

	protected float m_jump_Exit_Delay;

	protected float m_jump_Exit_ForceIdle = 0.15f;

	protected float m_jump_Exit_AttackCD;

	protected override Vector2 IdleDuration => new Vector2(0.25f, 0.7f);

	protected override Vector2 WalkTowardsDuration => new Vector2(0.45f, 0.85f);

	protected override Vector2 WalkAwayDuration => new Vector2(0.45f, 0.85f);

	protected virtual float Jump_Power => 20f;

	protected virtual bool Jump_SpawnProjectiles => false;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[0];
	}

	private void OnEnable()
	{
		if (base.IsInitialized)
		{
			Vector3 localEulerAngles = base.EnemyController.Pivot.transform.localEulerAngles;
			localEulerAngles.z = 0f;
			base.EnemyController.Pivot.transform.localEulerAngles = localEulerAngles;
		}
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		m_wallStickCollision = base.EnemyController.GetComponent<WallStickCollision>();
		m_wallStickCollision.enabled = false;
		base.EnemyController.ControllerCorgi.DefaultParameters.Gravity = 0f;
		base.LogicController.ExecuteLogicInAir = true;
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
		Vector2 jumpAngleVector = CDGHelper.VectorBetweenPts(base.EnemyController.Midpoint, base.EnemyController.TargetController.Midpoint);
		jumpAngleVector.Normalize();
		if (!AllowJumpAttack(jumpAngleVector))
		{
			yield return Idle();
			yield break;
		}
		StopAndFaceTarget();
		base.EnemyController.AlwaysFacing = false;
		yield return Default_Animation("Jump_Tell", m_jump_Tell_AnimationSpeed, m_jump_Tell_Delay);
		base.EnemyController.FlyingMovementType = FlyingMovementType.Override;
		base.EnemyController.SetVelocity(jumpAngleVector.x * Jump_Power, jumpAngleVector.y * Jump_Power, additive: false);
		yield return this.ChangeAnimationState("JumpUp");
		yield return Wait(0.05f);
		m_wallStickCollision.enabled = true;
		while (!m_wallStickCollision.StickingToWall)
		{
			yield return null;
		}
		base.Animator.SetBool("Grounded", value: true);
		m_wallStickCollision.enabled = false;
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		if (m_jump_Exit_Delay > 0f)
		{
			Wait(m_jump_Exit_Delay);
		}
		base.EnemyController.AlwaysFacing = true;
		yield return Default_Attack_Cooldown(m_jump_Exit_ForceIdle, m_jump_Exit_AttackCD);
	}

	private bool AllowJumpAttack(Vector2 jumpVector)
	{
		RaycastHit2D raycastHit2D = Physics2D.Raycast(base.EnemyController.Midpoint, jumpVector, 20f, base.EnemyController.ControllerCorgi.PlatformMask);
		if (!raycastHit2D || !m_wallStickCollision.LastHitRaycast || ((bool)raycastHit2D && (bool)m_wallStickCollision.LastHitRaycast && raycastHit2D.collider != m_wallStickCollision.LastHitRaycast.collider))
		{
			return true;
		}
		return false;
	}

	public override void OnLBCompleteOrCancelled()
	{
		m_wallStickCollision.enabled = false;
		base.OnLBCompleteOrCancelled();
	}
}
