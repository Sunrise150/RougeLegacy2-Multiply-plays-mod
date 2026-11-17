using UnityEngine;

public class Skeleton_Miniboss_AIScript : Skeleton_Basic_AIScript
{
	protected override bool m_jump_tweak_X => true;

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		m_jump_Power = new Vector2(-8.5f, 40.5f);
		m_jump_Tell_Delay = 0.55f;
		m_jump_Exit_AttackCD = 0f;
		m_throwBone_AttackCD = 0f;
		m_throwRib_Exit_AttackCD = 4.5f;
		base.LogicController.DisableLogicActivationByDistance = true;
	}
}
