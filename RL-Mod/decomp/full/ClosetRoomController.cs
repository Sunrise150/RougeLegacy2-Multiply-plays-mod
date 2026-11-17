using System.Collections;
using UnityEngine;

public class ClosetRoomController : BaseSpecialRoomController
{
	[SerializeField]
	private TunnelSpawnController m_closetTunnelSpawner;

	protected override void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		base.OnPlayerEnterRoom(sender, eventArgs);
		StartCoroutine(FixTunnelLayer());
		if (!GlobalTimerHUDController.IsRunning)
		{
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.ResetGlobalTimer, null, null);
			GlobalTimerHUDController.ReverseTimer = true;
			GlobalTimerHUDController.ReverseStartTime = 600f;
			GlobalTimerHUDController.TrackNegativeTimeAchievement = true;
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.StartGlobalTimer, null, null);
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayGlobalTimer, null, null);
		}
	}

	protected override void OnPlayerExitRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		if (!PlayerManager.GetPlayerController().IsFacingRight)
		{
			PlayerManager.GetPlayerController().CharacterCorgi.Flip();
		}
		base.OnPlayerExitRoom(sender, eventArgs);
	}

	private IEnumerator FixTunnelLayer()
	{
		yield return null;
		SpriteRenderer componentInChildren = m_closetTunnelSpawner.Tunnel.GetComponentInChildren<SpriteRenderer>();
		componentInChildren.gameObject.layer = 24;
		Vector3 localPosition = componentInChildren.transform.localPosition;
		localPosition.z = 5f;
		componentInChildren.transform.localPosition = localPosition;
	}
}
