using UnityEngine;

public static class FireProjectile_AIExtension
{
	public static Projectile_RL FireProjectile(this BaseAIScript aiScript, string projectileName, Vector2 offset, bool matchFacing = true, float angle = 0f, float speedMod = 1f, bool playSpawnAudio = true, bool playLifetimeAudio = true, bool playDeathAudio = true)
	{
		return ProjectileManager.FireProjectile(aiScript.EnemyController.gameObject, projectileName, offset, matchFacing, angle, speedMod, useAbsPos: false, playSpawnAudio, playLifetimeAudio, playDeathAudio);
	}

	public static Projectile_RL FireProjectileAbsPos(this BaseAIScript aiScript, string projectileName, Vector2 pos, bool matchFacing = true, float angle = 0f, float speedMod = 1f, bool playSpawnAudio = true, bool playLifetimeAudio = true, bool playDeathAudio = true)
	{
		return ProjectileManager.FireProjectile(aiScript.EnemyController.gameObject, projectileName, pos, matchFacing, angle, speedMod, useAbsPos: true, playSpawnAudio, playLifetimeAudio, playDeathAudio);
	}

	public static Projectile_RL FireProjectile(this BaseAIScript aiScript, string projectileName, int spawnPosIndex, bool matchFacing = true, float angle = 0f, float speedMod = 1f, bool playSpawnAudio = true, bool playLifetimeAudio = true, bool playDeathAudio = true)
	{
		Vector2 offset = aiScript.GetRelativeSpawnPositionAtIndex(spawnPosIndex, matchFacing);
		return ProjectileManager.FireProjectile(aiScript.EnemyController.gameObject, projectileName, offset, matchFacing, angle, speedMod, useAbsPos: false, playSpawnAudio, playLifetimeAudio, playDeathAudio);
	}
}
