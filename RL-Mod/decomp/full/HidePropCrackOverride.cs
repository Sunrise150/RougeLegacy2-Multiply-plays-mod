using System;
using UnityEngine;

public class HidePropCrackOverride : MonoBehaviour
{
	private PropSpawnController m_propSpawner;

	private Action<MonoBehaviour, EventArgs> m_onPlayerEnterRoom;

	private void Awake()
	{
		m_propSpawner = GetComponent<PropSpawnController>();
		m_onPlayerEnterRoom = OnPlayerEnterRoom;
	}

	private void OnEnable()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
	}

	private void OnDisable()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
	}

	private void OnPlayerEnterRoom(object sender, EventArgs args)
	{
		if (m_propSpawner != null && m_propSpawner.PropInstance != null && m_propSpawner.IsBreakable && !m_propSpawner.PropInstance.Breakable.IsBroken)
		{
			m_propSpawner.PropInstance.Breakable.SpriteRenderer.enabled = false;
		}
	}
}
