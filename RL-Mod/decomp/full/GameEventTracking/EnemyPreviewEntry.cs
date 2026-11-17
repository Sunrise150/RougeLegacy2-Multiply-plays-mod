using System;
using System.Collections.Generic;

namespace GameEventTracking;

[Serializable]
public class EnemyPreviewEntry
{
	public EnemyType Type;

	public List<EnemyRankEntry> RankEntries;

	public EnemyPreviewEntry(EnemyType enemyType)
	{
		Type = enemyType;
		RankEntries = new List<EnemyRankEntry>();
		foreach (EnemyRank value in Enum.GetValues(typeof(EnemyRank)))
		{
			if (value != EnemyRank.Any && value != EnemyRank.None)
			{
				RankEntries.Add(new EnemyRankEntry(value));
			}
		}
	}
}
