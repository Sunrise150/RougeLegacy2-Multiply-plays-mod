using System.Collections;
using UnityEngine;

public static class SummonProjectile_AIExtension
{
	public static void SummonProjectile(this BaseAIScript aiScript, string projectileName, Vector2 offset, bool matchFacing = true, float angle = 0f, float speedMod = 1f, bool playSummonAudio = true, bool playSpawnAudio = true, bool playLifetimeAudio = true, bool playDeathAudio = true)
	{
		aiScript.RunPersistentCoroutine(aiScript.SummonProjectileCoroutine_V2(projectileName, offset, matchFacing, angle, speedMod, -1, playSummonAudio, playSpawnAudio, playLifetimeAudio, playDeathAudio));
	}

	public static void SummonProjectileFromRoomPos(this BaseAIScript aiScript, string projectileName, int spawnPosIndex, bool matchFacing = true, float angle = 0f, float speedMod = 1f, bool playSummonAudio = true, bool playSpawnAudio = true, bool playLifetimeAudio = true, bool playDeathAudio = true)
	{
		aiScript.RunPersistentCoroutine(aiScript.SummonProjectileCoroutine_V2(projectileName, Vector2.zero, matchFacing, angle, speedMod, spawnPosIndex, playSummonAudio, playSpawnAudio, playLifetimeAudio, playDeathAudio));
	}

	private static IEnumerator SummonProjectileCoroutine_V2(this BaseAIScript aiScript, string projectileName, Vector2 offset, bool matchFacing = true, float angle = 0f, float speedMod = 1f, int spawnPosIndex = -1, bool playSummonAudio = true, bool playSpawnAudio = true, bool playLifetimeAudio = true, bool playDeathAudio = true)
	{
		bool isFacingRight = true;
		if (matchFacing)
		{
			isFacingRight = aiScript.EnemyController.IsFacingRight;
		}
		Vector3 spawnPos = ProjectileManager.DetermineSpawnPosition(aiScript.EnemyController.gameObject, offset, isFacingRight);
		if (spawnPosIndex != -1)
		{
			SpawnPositionController componentInChildren = aiScript.EnemyController.Room.gameObject.GetComponentInChildren<SpawnPositionController>();
			if (componentInChildren != null && componentInChildren.HasSpawnPosition(spawnPosIndex))
			{
				spawnPos = componentInChildren.GetSpawnPosition(spawnPosIndex);
			}
		}
		float startTime = Time.time;
		Projectile_RL projectile = ProjectileLibrary.GetProjectile(projectileName);
		Bounds bounds = EnemyUtility.GetBounds(projectile.gameObject);
		bounds.size *= projectile.transform.localScale.x;
		bool num = bounds.size.y > bounds.size.x;
		Vector3 size = new Vector3(4f, 4f, 4f);
		Bounds bounds2 = new Bounds(Vector3.zero, size);
		bounds2.size *= size.x;
		float num2 = (num ? (bounds.size.y / bounds2.size.y) : (bounds.size.x / bounds2.size.x));
		num2 *= 1f;
		num2 = Mathf.Clamp(num2, 1.25f, 999f);
		size = new Vector3(num2, num2, num2);
		float duration = 1.2f;
		aiScript.CreateSummonCircle(spawnPos, size, duration, scaleWithOwner: false, !playSummonAudio);
		while (Time.time < startTime + 0.6f + 0.6f)
		{
			yield return null;
		}
		ProjectileManager.FireProjectile(aiScript.EnemyController.gameObject, projectileName, spawnPos, matchFacing, angle, speedMod, useAbsPos: true, playSpawnAudio, playLifetimeAudio, playDeathAudio);
	}
}
