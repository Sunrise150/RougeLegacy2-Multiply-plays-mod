using RLAudio;
using UnityEngine;

public static class PlayProjectileSFX_AIExtension
{
	public static void PlayProjectileSFX(this BaseAIScript aiScript, string projectileName, bool useSingleShotAudio = true)
	{
		Vector2 position = aiScript.transform.parent.transform.position;
		string text = ProjectileAudioLibrary.GetMultiShotAudioPath(projectileName);
		if (useSingleShotAudio)
		{
			text = ProjectileAudioLibrary.GetSingleShotAudioPath(projectileName);
		}
		if (text != string.Empty)
		{
			AudioManager.PlayDelayedOneShot(null, text, position);
		}
	}
}
