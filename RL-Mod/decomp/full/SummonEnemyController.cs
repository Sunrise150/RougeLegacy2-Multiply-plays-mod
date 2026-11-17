using System;
using UnityEngine;

[RequireComponent(typeof(EnemyController))]
public class SummonEnemyController : MonoBehaviour
{
	[Serializable]
	public struct SummonEnemyEntry
	{
		public EnemyType EnemyType;

		public EnemyRank EnemyRank;

		public int NumSummons;
	}

	[SerializeField]
	private SummonEnemyEntry[] m_enemiesToSummon;

	public SummonEnemyEntry[] EnemiesToSummon => m_enemiesToSummon;

	public bool Contains(EnemyType type, EnemyRank rank)
	{
		SummonEnemyEntry[] enemiesToSummon = EnemiesToSummon;
		for (int i = 0; i < enemiesToSummon.Length; i++)
		{
			SummonEnemyEntry summonEnemyEntry = enemiesToSummon[i];
			if (summonEnemyEntry.EnemyType == type && summonEnemyEntry.EnemyRank == rank)
			{
				return true;
			}
		}
		return false;
	}
}
