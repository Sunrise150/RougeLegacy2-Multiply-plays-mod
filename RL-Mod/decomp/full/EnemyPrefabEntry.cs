using System;

[Serializable]
public class EnemyPrefabEntry
{
	public EnemyType EnemyType;

	public string BasicPrefabPath;

	public string AdvancedPrefabPath;

	public string ExpertPrefabPath;

	public string MinibossPrefabPath;

	public string GetPrefabPath(EnemyRank enemyRank)
	{
		return enemyRank switch
		{
			EnemyRank.Basic => BasicPrefabPath, 
			EnemyRank.Advanced => AdvancedPrefabPath, 
			EnemyRank.Expert => ExpertPrefabPath, 
			EnemyRank.Miniboss => MinibossPrefabPath, 
			_ => throw new ArgumentException("Can't Get Prefab path for enemy of rank: " + enemyRank), 
		};
	}
}
