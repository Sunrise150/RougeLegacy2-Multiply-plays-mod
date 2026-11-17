using UnityEngine;

public static class FireProjectileFromRoomPos_AIExtension
{
	public static Projectile_RL FireProjectileFromRoomPos(this BaseAIScript aiScript, string projectileName, int spawnPosIndex, bool matchFacing = true, float angle = 0f, float speedMod = 1f)
	{
		SpawnPositionController componentInChildren = aiScript.EnemyController.Room.gameObject.GetComponentInChildren<SpawnPositionController>();
		if ((bool)componentInChildren && componentInChildren.HasSpawnPosition(spawnPosIndex))
		{
			Vector2 offset = componentInChildren.GetSpawnPosition(spawnPosIndex);
			return ProjectileManager.FireProjectile(aiScript.EnemyController.gameObject, projectileName, offset, matchFacing, angle, speedMod, useAbsPos: true);
		}
		return ProjectileManager.FireProjectile(aiScript.EnemyController.gameObject, projectileName, Vector2.zero, matchFacing, angle, speedMod);
	}
}
