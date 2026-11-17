using System.Collections;

public class Ninja_Basic_AIScript : BaseAIScript
{
	protected float m_shurikenSpreadAngle = 60f;

	protected float m_shoot_TellIntro_AnimationSpeed = 1f;

	protected float m_shoot_TellHold_AnimationSpeed = 1f;

	protected float m_shoot_Tell_Delay = 1f;

	protected float m_shoot_AttackIntro_AnimationSpeed = 1f;

	protected float m_shoot_AttackIntro_Delay;

	protected float m_shoot_AttackHold_AnimationSpeed = 1f;

	protected float m_shoot_AttackHold_Delay;

	protected float m_shoot_Exit_AnimationSpeed = 1f;

	protected float m_shoot_Exit_Delay;

	protected virtual int NumberOfShurikens => 3;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[1] { "EyeballBounceBoltProjectile" };
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator ThrowShuriken()
	{
		StopAndFaceTarget();
		base.EnemyController.AlwaysFacing = false;
		base.EnemyController.OrientToTarget();
		float startingAngle2 = CDGHelper.WrapAngleDegrees(CDGHelper.ToDegrees(base.EnemyController.Orientation), convertToHalfRadius: false);
		float spreadAngle = m_shurikenSpreadAngle / (float)NumberOfShurikens;
		startingAngle2 -= (m_shurikenSpreadAngle - spreadAngle) / 2f;
		yield return Default_TellIntroAndLoop("ThrowRock_Tell_Intro", m_shoot_TellIntro_AnimationSpeed, "ThrowRock_Tell_Hold", m_shoot_TellHold_AnimationSpeed, m_shoot_Tell_Delay);
		yield return Default_Animation("ThrowRock_Attack_Intro", m_shoot_AttackIntro_AnimationSpeed, m_shoot_AttackIntro_Delay);
		yield return Default_Animation("ThrowRock_Attack_Hold", m_shoot_AttackHold_AnimationSpeed, 0f, waitUntilAnimationComplete: false);
		for (int i = 0; i < NumberOfShurikens; i++)
		{
			float angle = startingAngle2 + spreadAngle * (float)i;
			this.FireProjectile("EyeballBounceBoltProjectile", 0, matchFacing: false, angle);
		}
		if (m_shoot_AttackHold_Delay > 0f)
		{
			yield return Wait(m_shoot_AttackHold_Delay);
		}
		yield return Default_Animation("ThrowRock_Exit", m_shoot_Exit_AnimationSpeed, m_shoot_Exit_Delay);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		base.EnemyController.AlwaysFacing = true;
		yield return Default_Attack_Cooldown(1f, 1f);
	}
}
