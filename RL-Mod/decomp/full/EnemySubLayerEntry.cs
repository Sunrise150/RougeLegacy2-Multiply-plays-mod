using System;

[Serializable]
public class EnemySubLayerEntry
{
	public EnemyTypeAndRank Enemy;

	public int SubLayer;

	public EnemySubLayerEntry(EnemyTypeAndRank enemyTypeAndRank, int subLayer)
	{
		Enemy = enemyTypeAndRank;
		SubLayer = subLayer;
	}
}
