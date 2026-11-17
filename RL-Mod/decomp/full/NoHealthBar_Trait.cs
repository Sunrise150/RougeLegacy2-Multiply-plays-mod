using System.Collections;

public class NoHealthBar_Trait : BaseTrait
{
	public override TraitType TraitType => TraitType.NoHealthBar;

	private IEnumerator Start()
	{
		while (!PlayerManager.IsInstantiated)
		{
			yield return null;
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerHealthChange, this, new HealthChangeEventArgs(playerController, playerController.CurrentHealth, playerController.ActualMaxHealth));
	}

	private void OnDestroy()
	{
		if (!PlayerManager.IsDisposed)
		{
			PlayerController playerController = PlayerManager.GetPlayerController();
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerHealthChange, this, new HealthChangeEventArgs(playerController, playerController.CurrentHealth, playerController.ActualMaxHealth));
		}
	}
}
