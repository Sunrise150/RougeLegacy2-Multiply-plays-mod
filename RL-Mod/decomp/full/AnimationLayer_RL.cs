using System.Collections.Generic;

public static class AnimationLayer_RL
{
	private static int DEFAULT_SQUASH_AND_STRETCH_LAYER_INDEX = 1;

	private static Dictionary<EnemyType, int> ENEMY_SQUASH_AND_STRETCH_LAYER_INDEX_TABLE = new Dictionary<EnemyType, int> { 
	{
		EnemyType.Skeleton,
		2
	} };

	public static int GetSquashAndStretchLayerIndex(EnemyType enemyType)
	{
		if (ENEMY_SQUASH_AND_STRETCH_LAYER_INDEX_TABLE.ContainsKey(enemyType))
		{
			return ENEMY_SQUASH_AND_STRETCH_LAYER_INDEX_TABLE[enemyType];
		}
		return DEFAULT_SQUASH_AND_STRETCH_LAYER_INDEX;
	}
}
