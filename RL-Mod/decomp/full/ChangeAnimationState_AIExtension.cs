using System.Collections;

public static class ChangeAnimationState_AIExtension
{
	public static IEnumerator ChangeAnimationState(this BaseAIScript aiScript, string animParamName)
	{
		aiScript.Animator.Play(animParamName, aiScript.DefaultAnimationLayer);
		yield break;
	}

	public static IEnumerator ChangeAnimationState(this BaseAIScript aiScript, int animParamID)
	{
		aiScript.Animator.Play(animParamID, aiScript.DefaultAnimationLayer);
		yield break;
	}
}
