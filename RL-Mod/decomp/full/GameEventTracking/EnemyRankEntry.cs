using System;
using UnityEngine;

namespace GameEventTracking;

[Serializable]
public class EnemyRankEntry
{
	public EnemyRank Rank = EnemyRank.None;

	public Sprite PreviewImage;

	public EnemyRankEntry(EnemyRank rank)
	{
		Rank = rank;
	}
}
