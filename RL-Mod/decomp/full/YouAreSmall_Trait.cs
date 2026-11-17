using System.Collections;
using UnityEngine;

public class YouAreSmall_Trait : BaseTrait
{
	public override TraitType TraitType => TraitType.YouAreSmall;

	private IEnumerator Start()
	{
		if (!PlayerManager.IsInstantiated)
		{
			yield return new WaitUntil(() => PlayerManager.IsInstantiated);
		}
		GameObject player = PlayerManager.GetPlayer();
		Vector3 localScale = player.transform.localScale;
		localScale.x = Mathf.Clamp(localScale.x * 0.55f, 0.7f, 2.4f);
		localScale.y = Mathf.Clamp(localScale.y * 0.55f, 0.7f, 2.4f);
		player.transform.localScale = localScale;
		PlayerManager.GetPlayerController().ControllerCorgi.InitializeRays();
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerScaleChanged, null, null);
	}

	private void OnDestroy()
	{
		if (!PlayerManager.IsDisposed)
		{
			GameObject player = PlayerManager.GetPlayer();
			Vector3 localScale = player.transform.localScale;
			localScale.x = Mathf.Clamp(localScale.x * 1.4f, 0.7f, 2.4f);
			localScale.y = Mathf.Clamp(localScale.y * 1.4f, 0.7f, 2.4f);
			player.transform.localScale = localScale;
			PlayerManager.GetPlayerController().ControllerCorgi.InitializeRays();
			if (!GameUtility.IsApplicationQuitting)
			{
				Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerScaleChanged, null, null);
			}
		}
	}
}
