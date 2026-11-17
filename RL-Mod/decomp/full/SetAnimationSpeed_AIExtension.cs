using UnityEngine;

public static class SetAnimationSpeed_AIExtension
{
	private const string ANIM_SPEED_NAME = "Anim_Speed";

	private const string ABILITY_ANIM_SPEED_NAME = "Ability_Anim_Speed";

	public static void SetAnimationSpeedMultiplier(this BaseAIScript aiScript, float multiplier)
	{
		if (multiplier <= 0f)
		{
			Debug.LogFormat("<color=red>{0}: Animation Speed on Animator Controller ({1}) was set to a value less than or equal to zero. Was this intentional?</color>", Time.frameCount, aiScript.Animator.name);
		}
		if (aiScript.EnemyController.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_Speed))
		{
			multiplier *= 1.25f;
		}
		if (AnimatorUtility.HasParameter(aiScript.Animator, "Anim_Speed"))
		{
			aiScript.Animator.SetFloat("Anim_Speed", multiplier);
		}
		else if (AnimatorUtility.HasParameter(aiScript.Animator, "Ability_Anim_Speed"))
		{
			aiScript.Animator.SetFloat("Ability_Anim_Speed", multiplier);
		}
	}
}
