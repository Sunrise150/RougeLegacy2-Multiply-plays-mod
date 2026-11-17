using System;
using UnityEngine;

public class Ferr2DBuildController : MonoBehaviour, IRoomConsumer
{
	[SerializeField]
	private bool m_buildMeshOnly = true;

	[SerializeField]
	private Ferr2DBuildStage m_buildStage;

	[SerializeField]
	private Ferr2DT_PathTerrain m_terrain;

	private bool m_isBuilt;

	private Action<object, RoomViaDoorEventArgs> m_onPlayerEnterRoom;

	private Action<object, EventArgs> m_onRoomDestroyed;

	public BaseRoom Room { get; private set; }

	private void Awake()
	{
		m_onPlayerEnterRoom = OnPlayerEnterRoom;
		m_onRoomDestroyed = OnRoomDestroyed;
	}

	private void Start()
	{
		if (m_buildStage == Ferr2DBuildStage.Start)
		{
			Build();
		}
	}

	private void Build()
	{
		m_isBuilt = true;
		if (m_buildMeshOnly)
		{
			m_terrain.BuildMeshOnly();
		}
		else
		{
			m_terrain.Build();
		}
	}

	private void OnRoomDestroyed(object sender, EventArgs eventArgs)
	{
		if ((bool)Room)
		{
			Room.PlayerEnterRelay.RemoveListener(m_onPlayerEnterRoom);
			Room.RoomDestroyedRelay.RemoveListener(m_onRoomDestroyed);
		}
	}

	private void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		if (!m_isBuilt)
		{
			Build();
			Room.PlayerEnterRelay.RemoveListener(m_onPlayerEnterRoom);
			Room.RoomDestroyedRelay.RemoveListener(m_onRoomDestroyed);
		}
	}

	public void SetRoom(BaseRoom room)
	{
		if (m_buildStage == Ferr2DBuildStage.PlayerEnterRoom)
		{
			Room = room;
			Room.PlayerEnterRelay.AddListener(m_onPlayerEnterRoom);
			Room.RoomDestroyedRelay.AddListener(m_onRoomDestroyed);
		}
	}
}
