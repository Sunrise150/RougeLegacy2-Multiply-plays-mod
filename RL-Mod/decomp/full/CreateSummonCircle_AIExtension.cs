using UnityEngine;

public static class CreateSummonCircle_AIExtension
{
	public static void CreateSummonCircle(this BaseAIScript aiScript, Vector2 position, Vector2 scale, float duration, bool scaleWithOwner = true, bool muteAudio = false)
	{
		if (scaleWithOwner)
		{
			scale *= aiScript.EnemyController.ActualScale;
		}
		BaseEffect baseEffect = EffectManager.PlayEffect(null, null, "SpellCircle_Effect", position, duration, EffectStopType.Gracefully);
		if (baseEffect is GenericEffect)
		{
			GenericEffect genericEffect = baseEffect as GenericEffect;
			if (genericEffect.Animator != null)
			{
				genericEffect.Animator.SetBool("MuteAudio", muteAudio);
			}
		}
		baseEffect.transform.localScale = new Vector3(scale.x, scale.y, 1f);
	}

	public static void CreateSummonCircle(this BaseAIScript aiScript, int spawnPosIndex, Vector2 scale, float duration, bool scaleWithOwner = true, bool matchFacing = true)
	{
		if (scaleWithOwner)
		{
			scale *= aiScript.EnemyController.ActualScale;
		}
		Vector3 localSpawnPosition = aiScript.EnemyController.SpawnPositionController.GetLocalSpawnPosition(spawnPosIndex);
		Vector3 spawnPosition = aiScript.EnemyController.SpawnPositionController.GetSpawnPosition(spawnPosIndex);
		spawnPosition = aiScript.EnemyController.transform.InverseTransformPoint(spawnPosition);
		localSpawnPosition = spawnPosition;
		localSpawnPosition *= aiScript.EnemyController.BaseScaleToOffsetWith;
		if (!aiScript.EnemyController.IsFacingRight && matchFacing)
		{
			localSpawnPosition.x = 0f - localSpawnPosition.x;
		}
		EffectManager.PlayEffect(null, null, "SpellCircle_Effect", aiScript.EnemyController.Midpoint + localSpawnPosition, duration, EffectStopType.Gracefully).transform.localScale = new Vector3(scale.x, scale.y, 1f);
	}
}
