using System;
using UnityEngine;

[Serializable]
public struct EnemyTypeAndRank
{
	[SerializeField]
	private EnemyType m_type;

	[SerializeField]
	private EnemyRank m_rank;

	public EnemyType Type => m_type;

	public EnemyRank Rank => m_rank;

	public EnemyTypeAndRank(EnemyType enemyType, EnemyRank enemyLevel)
	{
		m_type = enemyType;
		m_rank = enemyLevel;
	}

	public override bool Equals(object obj)
	{
		if (obj is EnemyTypeAndRank)
		{
			return this == (EnemyTypeAndRank)obj;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override string ToString()
	{
		return $"{m_rank} {m_type}";
	}

	public static bool operator ==(EnemyTypeAndRank x, EnemyTypeAndRank y)
	{
		if (x.Rank == y.Rank)
		{
			return x.Type == y.Type;
		}
		return false;
	}

	public static bool operator !=(EnemyTypeAndRank x, EnemyTypeAndRank y)
	{
		return !(x == y);
	}
}
