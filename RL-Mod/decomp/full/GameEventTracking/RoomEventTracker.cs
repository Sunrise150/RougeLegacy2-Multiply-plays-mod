using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameEventTracking;

public class RoomEventTracker : MonoBehaviour, IGameEventTracker<IRoomEventTrackerState>
{
	private IRoomEventTrackerState m_state;

	private List<RoomTrackerData> m_roomsEntered = new List<RoomTrackerData>();

	private MergeRoomChunkManager m_mergeRoomChunkManager;

	private Action<MonoBehaviour, EventArgs> m_onPlayerEnterRoom;

	private Action<MonoBehaviour, EventArgs> m_onPlayerExitRoom;

	private Action<Vector2> m_onPlayerEnteredMergeRoomChunk;

	public List<RoomTrackerData> RoomsEntered
	{
		get
		{
			return m_roomsEntered;
		}
		private set
		{
			m_roomsEntered = value;
		}
	}

	private void Awake()
	{
		m_onPlayerEnterRoom = OnPlayerEnterRoom;
		m_onPlayerExitRoom = OnPlayerExitRoom;
		m_onPlayerEnteredMergeRoomChunk = OnPlayerEnteredMergeRoomChunk;
	}

	private void Start()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerExitRoom, m_onPlayerExitRoom);
	}

	private void OnDestroy()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerExitRoom, m_onPlayerExitRoom);
	}

	public IEnumerable<IGameEventData> GetGameEvents()
	{
		foreach (RoomTrackerData item in RoomsEntered)
		{
			yield return item;
		}
	}

	private void OnPlayerEnterRoom(MonoBehaviour sender, EventArgs eventArgs)
	{
		if (SceneManager.GetActiveScene().name != "World")
		{
			return;
		}
		if (eventArgs is RoomViaDoorEventArgs { Room: var room } roomViaDoorEventArgs)
		{
			if (!ReenactmentController.IsTunnelRoom(room) && room.BiomeType != BiomeType.Garden)
			{
				if (room is MergeRoom && room.BiomeType == BiomeType.Stone)
				{
					m_mergeRoomChunkManager = room.gameObject.GetComponentInChildren<MergeRoomChunkManager>();
					m_mergeRoomChunkManager.PlayerEnteredChunkRelay.AddListener(m_onPlayerEnteredMergeRoomChunk);
				}
				else
				{
					bool viaBiomeTransitionDoor = roomViaDoorEventArgs.ViaDoor != null && roomViaDoorEventArgs.ViaDoor.IsBiomeTransitionPoint;
					RoomsEntered.Add(new RoomTrackerData(room.BiomeType, room.BiomeControllerIndex, default(Vector2), viaBiomeTransitionDoor));
				}
			}
		}
		else
		{
			Debug.LogFormat("<color=red>| {0} | Failed to cast event args as RoomViaDoorEventArgs</color>", this);
		}
	}

	private void OnPlayerEnteredMergeRoomChunk(Vector2 mergeRoomChunkWorldPosition)
	{
		mergeRoomChunkWorldPosition.x += mergeRoomChunkWorldPosition.y - mergeRoomChunkWorldPosition.x;
		mergeRoomChunkWorldPosition.y = m_mergeRoomChunkManager.MergeRoom.Bounds.center.y;
		RoomsEntered.Add(new RoomTrackerData(m_mergeRoomChunkManager.MergeRoom.BiomeType, m_mergeRoomChunkManager.MergeRoom.BiomeControllerIndex, mergeRoomChunkWorldPosition));
	}

	private void OnPlayerExitRoom(MonoBehaviour sender, EventArgs eventArgs)
	{
		if (m_mergeRoomChunkManager != null)
		{
			m_mergeRoomChunkManager.PlayerEnteredChunkRelay.RemoveListener(m_onPlayerEnteredMergeRoomChunk);
		}
	}

	public void Reset()
	{
		if (RoomsEntered != null)
		{
			RoomsEntered.Clear();
		}
		if (m_mergeRoomChunkManager != null)
		{
			m_mergeRoomChunkManager.PlayerEnteredChunkRelay.RemoveListener(m_onPlayerEnteredMergeRoomChunk);
		}
	}

	public void RestoreState(IRoomEventTrackerState state)
	{
		RoomsEntered = state.RoomsEntered;
	}

	public IRoomEventTrackerState SaveState()
	{
		if (m_state == null)
		{
			m_state = new RoomEventTrackerState(RoomsEntered);
		}
		else
		{
			m_state.Initialise(RoomsEntered);
		}
		return m_state;
	}
}
