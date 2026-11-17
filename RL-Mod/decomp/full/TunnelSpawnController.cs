using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rooms;
using SceneManagement_RL;
using UnityEngine;

public class TunnelSpawnController : MonoBehaviour, ISimpleSpawnController, ISpawnController
{
	[SerializeField]
	private TunnelDirection m_tunnelType = TunnelDirection.Entrance;

	[SerializeField]
	private TransitionID m_transitionType = TransitionID.QuickSwipe;

	[SerializeField]
	private SpawnLogicController m_spawnLogicController;

	[SerializeField]
	private GameObject m_prefab;

	[SerializeField]
	private RoomMetaData[] m_destinationRoomPrefabsOverride;

	[SerializeField]
	[ReadOnly]
	private RoomID[] m_destinationRoomIds;

	[SerializeField]
	[Range(0f, 10f)]
	private int m_index;

	[SerializeField]
	private TunnelCategory m_tunnelCategory;

	[SerializeField]
	public TunnelSpawnControllerVisuals Visuals;

	[SerializeField]
	private bool m_isLocked;

	[SerializeField]
	private bool m_leadsToRoot;

	[SerializeField]
	private bool m_showTunnel = true;

	[SerializeField]
	private bool m_disableInteractIcon;

	private BaseRoom m_room;

	private RoomMetaData m_roomMetaData;

	public TransitionID TransitionType => m_transitionType;

	public GameObject TunnelPrefabOverride => m_prefab;

	public Room DestinationRoomPrefab { get; private set; }

	public GameObject GameObject => base.gameObject;

	public bool HasBeenSpawned { get; private set; }

	public bool ShouldSpawn
	{
		get
		{
			if ((bool)SpawnLogicController)
			{
				return SpawnLogicController.ShouldSpawn;
			}
			return true;
		}
	}

	public int Index
	{
		get
		{
			return m_index;
		}
		set
		{
			if (!Application.isPlaying)
			{
				m_index = value;
			}
		}
	}

	public RoomType LeadsToRoomType => Category switch
	{
		TunnelCategory.Boss => RoomType.Boss, 
		TunnelCategory.Bonus => RoomType.Bonus, 
		TunnelCategory.Final => RoomType.Boss, 
		TunnelCategory.Fairy => RoomType.Fairy, 
		_ => RoomType.Standard, 
	};

	public bool IsLocked => m_isLocked;

	public BaseRoom Room => m_room;

	public SpawnLogicController SpawnLogicController => m_spawnLogicController;

	public Tunnel Tunnel { get; private set; }

	public TunnelCategory Category
	{
		get
		{
			return m_tunnelCategory;
		}
		set
		{
			if (!Application.isPlaying)
			{
				m_tunnelCategory = value;
			}
		}
	}

	public TunnelDirection Direction
	{
		get
		{
			return m_tunnelType;
		}
		set
		{
			m_tunnelType = value;
		}
	}

	public RoomMetaData[] DestinationRoomPrefabsOverride
	{
		get
		{
			return m_destinationRoomPrefabsOverride;
		}
		set
		{
			if (!Application.isPlaying)
			{
				m_destinationRoomPrefabsOverride = value;
			}
		}
	}

	public bool IsDestinationRoot => m_leadsToRoot;

	public bool ShowTunnel => m_showTunnel;

	public RoomMetaData RoomMetaData
	{
		get
		{
			return m_roomMetaData;
		}
		private set
		{
			m_roomMetaData = value;
		}
	}

	private RoomMetaData GetRandomRoomMetaData(RoomMetaData[] rooms)
	{
		if (rooms != null)
		{
			int num = rooms.Length;
			if (num > 0)
			{
				int num2 = 0;
				if (num > 1)
				{
					num2 = RNGManager.GetRandomNumber(RngID.Tunnel_RoomSeed, "Tunnel Spawn Controller: Get Random Tunnel Destination", 0, num);
				}
				return rooms[num2];
			}
			Debug.LogFormat("<color=red>| {0} | Destination Room Prefabs is empty, but shouldn't be.</color>", this);
			return null;
		}
		Debug.LogFormat("<color=red>| {0} | Destination Room Prefabs is NULL, but shouldn't be.</color>", this);
		return null;
	}

	public void SetRoom(BaseRoom room)
	{
		m_room = room;
	}

	public void SetTunnelInstance(Tunnel instance)
	{
		Tunnel = instance;
		if ((bool)Tunnel && (bool)Tunnel.Interactable)
		{
			Tunnel.Interactable.ForceDisableInteractPrompt(m_disableInteractIcon);
		}
		if (!Room || !Tunnel)
		{
			return;
		}
		List<IRoomConsumer> roomConsumerListHelper_STATIC = SimpleSpawnController.m_roomConsumerListHelper_STATIC;
		roomConsumerListHelper_STATIC.Clear();
		Tunnel.GameObject.GetComponentsInChildren(roomConsumerListHelper_STATIC);
		foreach (IRoomConsumer item in roomConsumerListHelper_STATIC)
		{
			item.SetRoom(Room);
		}
	}

	public bool Spawn()
	{
		bool result = false;
		if (ShouldSpawn && !HasBeenSpawned && Direction != 0)
		{
			HasBeenSpawned = true;
			if (Direction == TunnelDirection.Entrance)
			{
				RoomMetaData = null;
				if (!IsDestinationRoot && DestinationRoomPrefabsOverride.Length != 0)
				{
					RoomMetaData = GetRandomRoomMetaData(DestinationRoomPrefabsOverride);
					if (!RoomMetaData)
					{
						Debug.LogFormat("<color=red>| {0} | A destination override entry is null on Tunnel Spawn Controller (<b>{1}</b>) in Room (<b>{2}</b>)</color>", this, $"{base.transform.parent.name}/{base.name}", Room.gameObject.name);
					}
				}
				SetDestinationRoomPrefab(RoomMetaData);
			}
			result = true;
		}
		base.gameObject.SetActive(value: false);
		return result;
	}

	public void SetDestinationRoomPrefab(RoomMetaData roomMetaData)
	{
		if ((bool)roomMetaData)
		{
			RoomMetaData = roomMetaData;
			DestinationRoomPrefab = roomMetaData.GetPrefab();
		}
	}

	[SpecialName]
	GameObject ISpawnController.get_gameObject()
	{
		return base.gameObject;
	}
}
