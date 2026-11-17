public static class FaceAwayFromPlayer_AIExtension
{
	public static void FaceAwayFromTarget(this BaseAIScript aiScript)
	{
		if ((bool)aiScript.Target && ((!aiScript.EnemyController.CharacterCorgi.IsFacingRight && aiScript.Target.transform.position.x < aiScript.gameObject.transform.parent.position.x) || (aiScript.EnemyController.CharacterCorgi.IsFacingRight && aiScript.Target.transform.position.x > aiScript.gameObject.transform.parent.position.x)))
		{
			aiScript.EnemyController.CharacterCorgi.Flip();
		}
	}
}
