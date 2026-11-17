using UnityEngine;

public class EndingSpawnRoomTypeController : MonoBehaviour, IRoomConsumer
{
	[SerializeField]
	private EndingSpawnRoomType m_endingSpawnRoomType;

	public BaseRoom Room { get; private set; }

	public EndingSpawnRoomType EndingSpawnRoomType => m_endingSpawnRoomType;

	public virtual void SetRoom(BaseRoom room)
	{
		Room = room;
		Room.PlayerEnterRelay.AddListener(OnPlayerEnterRoom);
	}

	private void OnDestroy()
	{
		if ((bool)Room)
		{
			Room.PlayerEnterRelay.RemoveListener(OnPlayerEnterRoom);
		}
	}

	private void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		if (m_endingSpawnRoomType == EndingSpawnRoomType.Docks)
		{
			SaveManager.PlayerSaveData.EndingSpawnRoom = EndingSpawnRoomType.None;
		}
		else if (m_endingSpawnRoomType >= EndingSpawnRoomType.Hallway && m_endingSpawnRoomType < EndingSpawnRoomType.AboveGround)
		{
			SaveManager.PlayerSaveData.EndingSpawnRoom = EndingSpawnRoomType.Hallway;
		}
		else
		{
			SaveManager.PlayerSaveData.EndingSpawnRoom = m_endingSpawnRoomType;
		}
	}
}
