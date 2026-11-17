using System;
using UnityEngine;

public class DisableCaveLanternInRoom : MonoBehaviour
{
	private Action<MonoBehaviour, EventArgs> m_onPlayerEnterRoom;

	private Action<MonoBehaviour, EventArgs> m_onPlayerExitRoom;

	private Action m_onAwake;

	private void Awake()
	{
		m_onPlayerEnterRoom = OnPlayerEnterRoom;
		m_onPlayerExitRoom = OnPlayerExitRoom;
		m_onAwake = OnAwake;
	}

	private void OnEnable()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerExitRoom, m_onPlayerExitRoom);
	}

	private void OnPlayerEnterRoom(MonoBehaviour sender, EventArgs args)
	{
		if ((bool)CaveLanternPostProcessingController.Instance)
		{
			CaveLanternPostProcessingController.Instance.enabled = false;
		}
		else
		{
			CaveLanternPostProcessingController.OnAwakeRelay.AddListener(m_onAwake);
		}
	}

	private void OnAwake()
	{
		CaveLanternPostProcessingController.Instance.enabled = false;
	}

	private void OnPlayerExitRoom(MonoBehaviour sender, EventArgs args)
	{
		if ((bool)CaveLanternPostProcessingController.Instance)
		{
			CaveLanternPostProcessingController.Instance.enabled = true;
		}
		CaveLanternPostProcessingController.OnAwakeRelay.RemoveListener(m_onAwake);
	}

	private void OnDisable()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerExitRoom, m_onPlayerExitRoom);
	}
}
