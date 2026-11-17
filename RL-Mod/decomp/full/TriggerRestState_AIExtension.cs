using UnityEngine;

public static class TriggerRestState_AIExtension
{
	public static void TriggerRestState(this BaseAIScript aiScript, Vector2 minMax, bool ignoreMods)
	{
		float duration = Random.Range(aiScript.EnemyController.ActualRestCooldown.x, aiScript.EnemyController.ActualRestCooldown.y);
		aiScript.LogicController.EnterRestState(duration, ignoreMods);
	}

	public static void TriggerRestState(this BaseAIScript aiScript, bool ignoreMods)
	{
		aiScript.TriggerRestState(aiScript.EnemyController.ActualRestCooldown, ignoreMods);
	}
}
