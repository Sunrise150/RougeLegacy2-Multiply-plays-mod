using System.Collections;
using UnityEngine;

public class SkeletonBoss_Advanced_AIScript : SkeletonBoss_Basic_AIScript
{
	protected override bool m_jump_Land_Spawn_Curse => true;

	public override IEnumerator DeathAnim()
	{
		if (!m_modeShiftSummons_Appeared)
		{
			SummonModeShiftBosses(forceSummon: true);
		}
		yield return base.DeathAnim();
		for (int i = 0; i < 3; i++)
		{
			Vector3 vector = i switch
			{
				0 => GetAbsoluteSpawnPositionAtIndex(9, matchFacing: false), 
				1 => GetAbsoluteSpawnPositionAtIndex(10, matchFacing: false), 
				_ => GetAbsoluteSpawnPositionAtIndex(11, matchFacing: false), 
			};
			if (IsCollidingAtSpawnPoint(vector))
			{
				vector.x = base.EnemyController.Midpoint.x;
			}
			EnemyManager.SummonEnemy(base.EnemyController, EnemyType.Skeleton, EnemyRank.Miniboss, vector, useAbsPos: true).DisableCulling = true;
		}
	}
}
