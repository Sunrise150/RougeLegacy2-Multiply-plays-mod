public static class SharedWorldObjectPoolManager
{
	public static void DestroyBiomePools()
	{
		ProjectileManager.DestroyBiomePools();
		PropManager.DestroyPools();
		EnemyManager.DestroyPools();
		HazardManager.DestroyPools();
		DecoManager.DestroyPools();
		ChestManager.DestroyPools();
		BackgroundsPoolManager.DestroyPools();
		EffectTriggerAnimBehaviour.ClearNormalizedStartCounters();
	}

	public static void DestroyAllPools()
	{
		DestroyBiomePools();
		TextPopupManager.DestroyPools();
		ItemDropManager.DestroyPools();
		ProjectileManager.DestroyOffscreenIconPool();
	}

	public static void CreateBiomePools(BiomeType newBiome)
	{
		ProjectileManager.ClearProjectileQueue();
		ProjectileManager.CreateBiomePools(newBiome);
		PropManager.CreateBiomePools(newBiome);
		EnemyManager.CreateBiomePools(newBiome);
		HazardManager.CreateBiomePools(newBiome);
		DecoManager.CreateBiomePools(newBiome);
		ChestManager.CreateBiomePools(newBiome);
		BackgroundsPoolManager.CreateBackgroundPools(newBiome);
		ProjectileManager.CreatePoolsFromQueuedProjectiles();
	}
}
