using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class ComplexSpawnController : MonoBehaviour, IComplexSpawnController, ISpawnController, IRoomConsumer, IIDConsumer
{
	[SerializeField]
	[ReadOnly]
	private int m_id;

	private SpawnLogicController m_spawnLogicController;

	private BaseRoom m_room;

	private bool m_hasCheckedForSpawnController;

	public GameObject GameObject => base.gameObject;

	public int ID
	{
		get
		{
			return m_id;
		}
		private set
		{
			m_id = value;
		}
	}

	public bool ShouldSpawn
	{
		get
		{
			if (SpawnLogicController != null)
			{
				return SpawnLogicController.ShouldSpawn;
			}
			return true;
		}
	}

	public BaseRoom Room
	{
		get
		{
			return m_room;
		}
		private set
		{
			m_room = value;
		}
	}

	public SpawnLogicController SpawnLogicController
	{
		get
		{
			if (!m_hasCheckedForSpawnController)
			{
				m_hasCheckedForSpawnController = true;
				m_spawnLogicController = GetComponent<SpawnLogicController>();
			}
			return m_spawnLogicController;
		}
	}

	protected virtual void Awake()
	{
		SpriteRenderer component = GetComponent<SpriteRenderer>();
		if ((bool)component)
		{
			component.enabled = false;
		}
	}

	private void OnDisable()
	{
		if (base.gameObject.activeInHierarchy)
		{
			Reset();
		}
	}

	protected abstract void Reset();

	public void SetID(int id)
	{
	}

	public virtual void SetRoom(BaseRoom room)
	{
		if ((bool)Room)
		{
			Room.PlayerEnterRelay.RemoveListener(OnPlayerEnterRoom);
			Room.PlayerExitRelay.RemoveListener(OnPlayerExitRoom);
			Room.RoomDestroyedRelay.RemoveListener(OnRoomDestroyed);
		}
		Room = room;
		Room.PlayerEnterRelay.AddListener(OnPlayerEnterRoom);
		Room.PlayerExitRelay.AddListener(OnPlayerExitRoom);
		Room.RoomDestroyedRelay.AddListener(OnRoomDestroyed);
	}

	protected abstract void Spawn();

	private void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		if (ShouldSpawn)
		{
			Spawn();
		}
	}

	private void OnPlayerExitRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		if (ShouldSpawn)
		{
			Reset();
		}
	}

	private void OnRoomDestroyed(object sender, EventArgs e)
	{
		Room.PlayerEnterRelay.RemoveListener(OnPlayerEnterRoom);
		Room.PlayerExitRelay.RemoveListener(OnPlayerExitRoom);
		Room.RoomDestroyedRelay.RemoveListener(OnRoomDestroyed);
	}

	[SpecialName]
	GameObject ISpawnController.get_gameObject()
	{
		return base.gameObject;
	}
}
