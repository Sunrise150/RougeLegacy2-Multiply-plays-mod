using System.Collections.Generic;
using UnityEngine;

public class ProjectileSizeComparer : IComparer<Projectile_RL>
{
	public int Compare(Projectile_RL a, Projectile_RL b)
	{
		Bounds bounds = EnemyUtility.GetBounds(a.gameObject);
		Bounds bounds2 = EnemyUtility.GetBounds(b.gameObject);
		float num = bounds.size.x * bounds.size.y;
		float value = bounds2.size.x * bounds2.size.y;
		return num.CompareTo(value);
	}
}
