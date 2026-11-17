using System;
using UnityEngine;

public class PlayerTakeNoDamage_RoomLogic : MonoBehaviour, IRoomConsumer
{
	private bool m_invincibilityAdded;

	public BaseRoom Room { get; private set; }

	public void SetRoom(BaseRoom room)
	{
		Room = room;
		Room.PlayerEnterRelay.AddListener(OnPlayerEnter);
		Room.PlayerExitRelay.AddListener(OnPlayerExit);
	}

	private void OnPlayerEnter(object sender, EventArgs args)
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		if (!playerController.TakesNoDamage)
		{
			playerController.TakesNoDamage = true;
			m_invincibilityAdded = true;
		}
	}

	private void OnPlayerExit(object sender, EventArgs args)
	{
		if (m_invincibilityAdded)
		{
			PlayerManager.GetPlayerController().TakesNoDamage = false;
			m_invincibilityAdded = false;
		}
	}

	private void OnDestroy()
	{
		if ((bool)Room)
		{
			Room.PlayerEnterRelay.RemoveListener(OnPlayerEnter);
			Room.PlayerExitRelay.RemoveListener(OnPlayerExit);
		}
	}
}
