using System;
using UnityEngine;

public class DisablePlatformOverride : MonoBehaviour
{
	private PropSpawnController m_propSpawnController;

	private Action<object, EventArgs> m_onPlayerEnterRoom;

	private Action<object, EventArgs> m_onPlayerExitRoom;

	private void Awake()
	{
		m_propSpawnController = GetComponent<PropSpawnController>();
		m_onPlayerEnterRoom = OnPlayerEnterRoom;
		m_onPlayerExitRoom = OnPlayerExitRoom;
	}

	private void OnEnable()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerExitRoom, m_onPlayerExitRoom);
	}

	private void OnDisable()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerExitRoom, m_onPlayerExitRoom);
	}

	private void OnPlayerEnterRoom(object sender, EventArgs args)
	{
		Prop propInstance = m_propSpawnController.PropInstance;
		if ((bool)propInstance && propInstance.HitboxController != null)
		{
			propInstance.HitboxController.SetHitboxActiveState(HitboxType.Platform, active: false);
		}
	}

	private void OnPlayerExitRoom(object sender, EventArgs args)
	{
		Prop propInstance = m_propSpawnController.PropInstance;
		if ((bool)propInstance && propInstance.HitboxController != null)
		{
			propInstance.HitboxController.SetHitboxActiveState(HitboxType.Platform, active: true);
		}
	}
}
