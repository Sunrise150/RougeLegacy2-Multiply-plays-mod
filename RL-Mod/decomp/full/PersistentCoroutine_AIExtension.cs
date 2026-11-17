using System.Collections;
using UnityEngine;

public static class PersistentCoroutine_AIExtension
{
	public static Coroutine RunPersistentCoroutine(this BaseAIScript aiScript, IEnumerator coroutine)
	{
		return aiScript.EnemyController.StartCoroutine(coroutine);
	}

	public static void StopPersistentCoroutine(this BaseAIScript aiScript, IEnumerator coroutine)
	{
		if (coroutine != null)
		{
			aiScript.EnemyController.StopCoroutine(coroutine);
		}
	}

	public static void StopPersistentCoroutine(this BaseAIScript aiScript, Coroutine coroutine)
	{
		if (coroutine != null)
		{
			aiScript.EnemyController.StopCoroutine(coroutine);
		}
	}
}
