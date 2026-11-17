public static class FacePlayer_AIExtension
{
	public static void FaceTarget(this BaseAIScript aiScript)
	{
		if ((bool)aiScript.Target && ((!aiScript.EnemyController.CharacterCorgi.IsFacingRight && aiScript.Target.transform.position.x > aiScript.gameObject.transform.parent.position.x) || (aiScript.EnemyController.CharacterCorgi.IsFacingRight && aiScript.Target.transform.position.x < aiScript.gameObject.transform.parent.position.x)))
		{
			aiScript.EnemyController.CharacterCorgi.Flip();
		}
	}
}
