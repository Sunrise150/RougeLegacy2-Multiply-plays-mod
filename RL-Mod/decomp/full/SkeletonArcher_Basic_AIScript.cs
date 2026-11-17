using System.Collections;
using UnityEngine;

public class SkeletonArcher_Basic_AIScript : BaseAIScript
{
	private static int m_attackDirectionStringHash = Animator.StringToHash("Attack_Direction");

	protected float m_shootArrow_TellIntro_AnimationSpeed = 0.75f;

	protected float m_shootArrow_TellHold_AnimationSpeed = 0.75f;

	protected const float m_shootArrow_TellIntroAndHold_Delay = 0.5f;

	protected float m_shootArrow_AttackIntro_AnimationSpeed = 1f;

	protected float m_shootArrow_AttackIntro_Delay;

	protected float m_shootArrow_AttackHold_AnimationSpeed = 1f;

	protected const float m_shootArrow_AttackHold_Delay = 0f;

	protected float m_shootArrow_Exit_AnimationSpeed = 1f;

	protected float m_shootArrow_Exit_Delay = 0.15f;

	protected int m_arrowHighAngle = 82;

	protected int m_arrowMediumAngle = 35;

	protected int m_arrowLowAngle = 12;

	protected float m_shootArrow_Exit_ForceIdle = 0.15f;

	protected float m_shootArrow_AttackCD;

	protected const string SHOOT_TELL_INTRO = "Shoot_Tell_Intro";

	protected const string SHOOT_TELL_HOLD = "Shoot_Tell_Hold";

	protected const string SHOOT_ATTACK_INTRO = "Shoot_Attack_Intro";

	protected const string SHOOT_ATTACK_HOLD = "Shoot_Attack_Hold";

	protected const string SHOOT_EXIT = "Shoot_Attack_Exit";

	protected const string ARROW_PROJECTILE = "SkeletonArcherBoltProjectile";

	protected Vector2 ARROW_POS_OFFSET = new Vector2(1f, 0.25f);

	protected float m_jump_Tell_Delay = 0.4f;

	protected float m_jump_Tell_AnimationSpeed = 0.75f;

	protected float m_jump_Exit_Delay;

	protected float m_jump_Exit_ForceIdle = 0.15f;

	protected float m_jump_Exit_AttackCD = 3f;

	protected override Vector2 IdleDuration => new Vector2(0.25f, 0.7f);

	protected override Vector2 WalkTowardsDuration => new Vector2(0.45f, 0.85f);

	protected override Vector2 WalkAwayDuration => new Vector2(0.45f, 0.85f);

	protected virtual bool m_shoot_ExtraArrowCount => false;

	protected virtual bool m_shootArrow_AimAtPlayer => true;

	protected virtual Vector2 m_jump_Power => new Vector2(3f, 32f);

	protected virtual int m_jump_Amount => 1;

	protected virtual float m_jump_AimDelay => 0.15f;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[1] { "SkeletonArcherBoltProjectile" };
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator ShootArrowLow()
	{
		StopAndFaceTarget();
		base.EnemyController.AlwaysFacing = false;
		yield return ShootArrowCoroutine(m_arrowLowAngle);
		base.EnemyController.AlwaysFacing = true;
		yield return Default_Attack_Cooldown(m_shootArrow_Exit_ForceIdle, m_shootArrow_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator ShootArrowMedium()
	{
		StopAndFaceTarget();
		base.EnemyController.AlwaysFacing = false;
		yield return ShootArrowCoroutine(m_arrowMediumAngle);
		base.EnemyController.AlwaysFacing = true;
		yield return Default_Attack_Cooldown(m_shootArrow_Exit_ForceIdle, m_shootArrow_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator ShootArrowHigh()
	{
		StopAndFaceTarget();
		base.EnemyController.AlwaysFacing = false;
		yield return ShootArrowCoroutine(m_arrowHighAngle);
		base.EnemyController.AlwaysFacing = true;
		yield return Default_Attack_Cooldown(m_shootArrow_Exit_ForceIdle, m_shootArrow_AttackCD);
	}

	public IEnumerator ShootArrowCoroutine(float angle)
	{
		Vector2 posOffset = ARROW_POS_OFFSET;
		if (m_shootArrow_AimAtPlayer)
		{
			Vector3 vector = new Vector3(base.EnemyController.Midpoint.x, base.EnemyController.Midpoint.y + posOffset.y, base.EnemyController.Midpoint.z);
			Vector3 midpoint = base.EnemyController.TargetController.Midpoint;
			angle = Mathf.Atan2(midpoint.y - vector.y, midpoint.x - vector.x);
			angle = CDGHelper.ToDegrees(angle);
			angle = CDGHelper.WrapAngleDegrees(angle, convertToHalfRadius: true);
			if (!base.EnemyController.IsFacingRight)
			{
				angle = CDGHelper.WrapAngleDegrees(-180f - angle, convertToHalfRadius: true);
			}
		}
		posOffset = CDGHelper.RotatedPoint(posOffset, Vector2.zero, angle);
		posOffset += (Vector2)(base.EnemyController.Midpoint - base.EnemyController.transform.localPosition);
		float t = (angle - -90f) / 180f;
		float value = Mathf.Lerp(4f, 0f, t);
		base.EnemyController.Animator.SetFloat(m_attackDirectionStringHash, value);
		yield return Default_TellIntroAndLoop("Shoot_Tell_Intro", m_shootArrow_TellIntro_AnimationSpeed, "Shoot_Tell_Hold", m_shootArrow_TellHold_AnimationSpeed, 0.5f);
		yield return Default_Animation("Shoot_Attack_Intro", m_shootArrow_AttackIntro_AnimationSpeed, m_shootArrow_AttackIntro_Delay);
		yield return Default_Animation("Shoot_Attack_Hold", m_shootArrow_AttackHold_AnimationSpeed, 0f, waitUntilAnimationComplete: false);
		this.FireProjectile("SkeletonArcherBoltProjectile", posOffset, matchFacing: true, angle);
		if (m_shoot_ExtraArrowCount)
		{
			this.FireProjectile("SkeletonArcherBoltProjectile", posOffset, matchFacing: true, angle + 15f);
			this.FireProjectile("SkeletonArcherBoltProjectile", posOffset, matchFacing: true, angle - 15f);
		}
		yield return Default_Animation("Shoot_Attack_Exit", m_shootArrow_Exit_AnimationSpeed, m_shootArrow_Exit_Delay);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Jump()
	{
		for (int i = 0; i < m_jump_Amount; i++)
		{
			StopAndFaceTarget();
			base.EnemyController.AlwaysFacing = false;
			yield return Default_Animation("Jump_Tell", m_jump_Tell_AnimationSpeed, m_jump_Tell_Delay);
			if (Random.Range(0, 2) == 0)
			{
				SetVelocityX(m_jump_Power.x, additive: true);
			}
			else
			{
				SetVelocityX(0f - m_jump_Power.x, additive: true);
			}
			SetVelocityY(m_jump_Power.y, additive: false);
			yield return this.ChangeAnimationState("JumpUp");
			yield return Wait(0.05f);
			yield return Wait(m_jump_AimDelay);
			this.FaceTarget();
			yield return ShootArrowCoroutine(m_arrowHighAngle);
			yield return WaitUntilIsGrounded();
		}
		if (m_jump_Exit_Delay > 0f)
		{
			yield return Wait(m_jump_Exit_Delay);
		}
		base.EnemyController.AlwaysFacing = true;
		yield return Default_Attack_Cooldown(m_jump_Exit_ForceIdle, m_jump_Exit_AttackCD);
	}

	private float GetAimAngle(float angle)
	{
		return Mathf.Lerp(2f, 0f, angle / 90f);
	}
}
