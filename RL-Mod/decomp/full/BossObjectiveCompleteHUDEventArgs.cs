public class BossObjectiveCompleteHUDEventArgs : ObjectiveCompleteHUDEventArgs
{
	public EnemyType EnemyType { get; private set; }

	public EnemyRank EnemyRank { get; private set; }

	public BossObjectiveCompleteHUDEventArgs(EnemyType enemyType, EnemyRank enemyRank, float displayDuration = 5f, string titleTextOverride = null, string subtitleTextOverride = null, string descriptionTextOverride = null)
		: base(ObjectiveCompleteHUDType.Boss, displayDuration)
	{
		Initialize(enemyType, enemyRank, displayDuration, titleTextOverride, subtitleTextOverride, descriptionTextOverride);
	}

	public void Initialize(EnemyType enemyType, EnemyRank enemyRank, float displayDuration = 5f, string titleTextOverride = null, string subtitleTextOverride = null, string descriptionTextOverride = null)
	{
		Initialize(ObjectiveCompleteHUDType.Boss, displayDuration, titleTextOverride, subtitleTextOverride, descriptionTextOverride);
		EnemyType = enemyType;
		EnemyRank = enemyRank;
	}
}
