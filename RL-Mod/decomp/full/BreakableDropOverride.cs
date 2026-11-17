using System;
using UnityEngine;

public class BreakableDropOverride : MonoBehaviour
{
	[SerializeField]
	private ItemDropType m_itemDropOverride;

	private PropSpawnController m_propSpawnController;

	private Action<MonoBehaviour, EventArgs> m_onPlayerEnterRoom;

	private void Awake()
	{
		m_propSpawnController = GetComponent<PropSpawnController>();
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
		if ((bool)m_propSpawnController && m_propSpawnController.ShouldSpawn && m_propSpawnController.IsBreakable)
		{
			m_propSpawnController.PropInstance.Breakable.ItemDropTypeOverride = m_itemDropOverride;
		}
	}
}
