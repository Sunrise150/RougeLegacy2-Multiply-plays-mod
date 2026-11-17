using System.Collections.Generic;
using UnityEngine;

public class EnemySizeComparer : IComparer<EnemyController>
{
	public int Compare(EnemyController a, EnemyController b)
	{
		Bounds bounds = EnemyUtility.GetBounds(a.gameObject);
		Bounds bounds2 = EnemyUtility.GetBounds(b.gameObject);
		float num = bounds.size.x * bounds.size.y;
		float value = bounds2.size.x * bounds2.size.y;
		return num.CompareTo(value);
	}
}
