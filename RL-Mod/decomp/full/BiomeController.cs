using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using UnityEngine;

public class BiomeController : MonoBehaviour
{
	private static string STANDARD_ROOM_STORAGE_LOCATION = "Rooms";

	private static string ROOMS_CONNECTED_BY_TUNNEL_STORAGE_LOCATION = "Rooms Connected By Tunnel";

	private BiomeType m_biome;

	private int m_chestCount = -1;

	private Transform m_standardRoomStorageLocation;

	private Transform m_roomsConnectedByTunnelStorageLocation;

	private Bounds m_border;

	private BiomeData m_biomeData;

	private BiomeGridPointManager m_gridPointManager;

	private Room m_transitionRoom;

	public int ChestCount
	{
		get
		{
			if (m_chestCount == -1)
			{
				m_chestCount = base.gameObject.GetComponentsInChildren<ChestObj>().Length;
			}
			return m_chestCount;
		}
	}

	public int StandaloneRoomCount
	{
		get
		{
			if (StandaloneRooms != null)
			{
				return StandaloneRooms.Count;
			}
			return 0;
		}
	}

	public int MergeRoomCount
	{
		get
		{
			if (MergeRooms != null)
			{
				return MergeRooms.Count;
			}
			return 0;
		}
	}

	public int CurrentRoomCount => StandaloneRoomCount;

	public int TargetRoomCount
	{
		get
		{
			int num = 0;
			foreach (KeyValuePair<RoomType, int> item in TargetRoomCountsByRoomType)
			{
				num += item.Value;
			}
			return num;
		}
	}

	public Bounds Bounds { get; private set; }

	public Room TransitionRoom
	{
		get
		{
			if (Biome == BiomeType.Garden && SaveManager.PlayerSaveData.EndingSpawnRoom != 0)
			{
				foreach (Room room in Rooms)
				{
					EndingSpawnRoomTypeController component = room.GetComponent<EndingSpawnRoomTypeController>();
					if ((bool)component && component.EndingSpawnRoomType == SaveManager.PlayerSaveData.EndingSpawnRoom)
					{
						return room;
					}
				}
			}
			return m_transitionRoom;
		}
	}

	public List<Room> StandaloneRooms { get; private set; }

	public List<MergeRoom> MergeRooms { get; private set; }

	public List<Room> RoomsConnectedByTunnel { get; private set; }

	public List<BaseRoom> Rooms { get; private set; }

	public int TotalRoomCount
	{
		get
		{
			if (Rooms == null)
			{
				return 0;
			}
			return Rooms.Count;
		}
	}

	public bool IsInitialised { get; private set; }

	public bool IsInstantiated
	{
		get
		{
			if (Biome == BiomeType.Castle)
			{
				if (Rooms != null && Rooms.Count > 1)
				{
					return Rooms[0].RoomType == RoomType.Transition;
				}
				return false;
			}
			if (Rooms != null)
			{
				return Rooms.Count > 0;
			}
			return false;
		}
	}

	public BiomeType Biome
	{
		get
		{
			return m_biome;
		}
		private set
		{
			m_biome = value;
		}
	}

	public Dictionary<RoomSide, Room> ConnectionRoomTable { get; set; }

	public List<ISpawnController> SpawnControllers { get; private set; }

	public Bounds Border => GridPointManager.Border;

	public Dictionary<Vector2Int, int> RoomSizeCount { get; set; }

	public Dictionary<RoomType, int> TargetRoomCountsByRoomType { get; set; }

	public List<Tunnel> Tunnels => base.gameObject.GetComponentsInChildren<Tunnel>().ToList();

	public TunnelSpawnController[] TunnelSpawnControllers => base.gameObject.GetComponentsInChildren<TunnelSpawnController>(includeInactive: true);

	public Transform StandardRoomStorageLocation
	{
		get
		{
			if (m_standardRoomStorageLocation == null)
			{
				GameObject gameObject = new GameObject(STANDARD_ROOM_STORAGE_LOCATION);
				gameObject.transform.SetParent(base.transform);
				gameObject.transform.localPosition = Vector3.zero;
				m_standardRoomStorageLocation = gameObject.transform;
			}
			return m_standardRoomStorageLocation;
		}
	}

	public Transform RoomsConnectedByTunnelStorageLocation
	{
		get
		{
			if (m_roomsConnectedByTunnelStorageLocation == null)
			{
				GameObject gameObject = new GameObject(ROOMS_CONNECTED_BY_TUNNEL_STORAGE_LOCATION);
				gameObject.transform.SetParent(base.transform);
				gameObject.transform.localPosition = Vector3.zero;
				m_roomsConnectedByTunnelStorageLocation = gameObject.transform;
			}
			return m_roomsConnectedByTunnelStorageLocation;
		}
	}

	public List<CinemachineVirtualCamera> CinemachineVirtualCameras { get; private set; }

	public BiomeData BiomeData
	{
		get
		{
			if (m_biomeData == null && Biome != 0)
			{
				m_biomeData = BiomeDataLibrary.GetData(Biome);
			}
			return m_biomeData;
		}
	}

	public BiomeGridPointManager GridPointManager
	{
		get
		{
			if (m_gridPointManager == null)
			{
				m_gridPointManager = new BiomeGridPointManager(Biome);
			}
			return m_gridPointManager;
		}
	}

	public List<Sky> Skies { get; private set; }

	public List<Weather> Weather { get; private set; }

	public void Initialise(BiomeType biome)
	{
		if (IsInitialised)
		{
			throw new Exception("BiomeController has already been initialised. Method should only be called once.");
		}
		if (biome == BiomeType.None || biome == BiomeType.Any)
		{
			throw new ArgumentException("biome");
		}
		Biome = biome;
		IsInitialised = true;
	}

	public void SetTransitionRoom(Room transitionRoom)
	{
		m_transitionRoom = transitionRoom;
	}

	public void StandardRoomCreatedByWorldBuilder(Room room)
	{
		AddRoom(room);
	}

	private void AddRoom(BaseRoom room)
	{
		if (Rooms == null)
		{
			Rooms = new List<BaseRoom>();
		}
		Rooms.Add(room);
		if (room is Room)
		{
			if (StandaloneRooms == null)
			{
				StandaloneRooms = new List<Room>();
			}
			StandaloneRooms.Add(room as Room);
			if (Bounds == default(Bounds))
			{
				Bounds = new Bounds(room.gameObject.transform.position, Vector3.zero);
			}
			GrowBounds(room as Room);
			if (SpawnControllers == null)
			{
				SpawnControllers = new List<ISpawnController>();
			}
			SpawnControllers.AddRange(room.gameObject.GetComponentsInChildren<ISpawnController>());
		}
		else if (room is MergeRoom)
		{
			if (MergeRooms == null)
			{
				MergeRooms = new List<MergeRoom>();
			}
			MergeRooms.Add(room as MergeRoom);
		}
	}

	public BaseRoom GetRoom(int biomeControllerIndex)
	{
		if (Rooms == null)
		{
			return null;
		}
		if (biomeControllerIndex < 0 || biomeControllerIndex > Rooms.Count)
		{
			int currentSeed = RNGSeedManager.GetCurrentSeed(SceneLoadingUtility.ActiveScene.name);
			Debug.Log("roomIndexOutOfRange - BiomeControllerIndex: " + biomeControllerIndex + " biomeType: " + Biome.ToString() + " seed: " + RNGSeedManager.GetSeedAsHex(currentSeed) + "-" + BurdenManager.GetBurdenLevel(BurdenType.RoomCount));
			throw new IndexOutOfRangeException("roomIndex - BiomeControllerIndex: " + biomeControllerIndex + " biomeType: " + Biome.ToString() + " seed: " + RNGSeedManager.GetSeedAsHex(currentSeed) + "-" + BurdenManager.GetBurdenLevel(BurdenType.RoomCount));
		}
		int index = 0;
		for (int i = 0; i < Rooms.Count; i++)
		{
			if (Rooms[i].BiomeControllerIndex == biomeControllerIndex)
			{
				index = i;
				break;
			}
		}
		return Rooms[index];
	}

	public void MergeRoomCreatedByWorldBuilder(MergeRoom mergeRoom)
	{
		AddRoom(mergeRoom);
	}

	public void RoomConnectedByTunnelCreatedByWorldBuilder(Room room)
	{
		if (Rooms == null)
		{
			Rooms = new List<BaseRoom>();
		}
		if (RoomsConnectedByTunnel == null)
		{
			RoomsConnectedByTunnel = new List<Room>();
		}
		RoomsConnectedByTunnel.Add(room);
		Rooms.Add(room);
	}

	private void GrowBounds(Room room)
	{
		Bounds bounds = default(Bounds);
		bounds = Bounds;
		bounds.Encapsulate(room.Bounds);
		Bounds = bounds;
	}

	public void InvalidConnectionRoomReplacedByWorldBuiler(Room replacedRoom, Room replacementRoom)
	{
		if (StandaloneRooms.Contains(replacedRoom))
		{
			StandaloneRooms.Remove(replacedRoom);
		}
		else
		{
			Debug.LogFormat("<color=red>[{0}] StandaloneRooms does not replacedRoom, but should.</color>", this);
		}
		StandaloneRooms.Add(replacementRoom);
	}

	public void Reset()
	{
		bool flag = WorldBuilder.Instance.LevelManager.Mode != LevelManagerMode.Debug;
		if (StandaloneRooms != null)
		{
			if (Biome == BiomeType.Castle && flag)
			{
				for (int num = StandaloneRooms.Count - 1; num >= 0; num--)
				{
					if (StandaloneRooms[num].RoomType != RoomType.Transition)
					{
						StandaloneRooms[num] = null;
						StandaloneRooms.RemoveAt(num);
					}
				}
			}
			else
			{
				StandaloneRooms.Clear();
			}
		}
		if (MergeRooms != null)
		{
			MergeRooms.Clear();
		}
		for (int num2 = Rooms.Count - 1; num2 >= 0; num2--)
		{
			if (!(Biome == BiomeType.Castle && flag) || Rooms[num2].RoomType != RoomType.Transition)
			{
				UnityEngine.Object.Destroy(Rooms[num2].gameObject);
			}
		}
		if (Rooms != null)
		{
			if (Biome == BiomeType.Castle && flag)
			{
				for (int num3 = Rooms.Count - 1; num3 >= 0; num3--)
				{
					if (Rooms[num3].RoomType != RoomType.Transition)
					{
						Rooms[num3] = null;
						Rooms.RemoveAt(num3);
					}
				}
			}
			else
			{
				Rooms.Clear();
			}
		}
		if (RoomsConnectedByTunnel != null)
		{
			RoomsConnectedByTunnel.Clear();
		}
		if (CinemachineVirtualCameras != null)
		{
			CinemachineVirtualCameras.Clear();
		}
		GridPointManager.RemoveTunnelDestinations();
		if (Skies != null)
		{
			for (int num4 = Skies.Count - 1; num4 >= 0; num4--)
			{
				UnityEngine.Object.Destroy(Skies[num4].gameObject);
			}
			Skies.Clear();
		}
		if (Weather != null)
		{
			for (int num5 = Weather.Count - 1; num5 >= 0; num5--)
			{
				UnityEngine.Object.Destroy(Weather[num5].gameObject);
			}
			Weather.Clear();
		}
	}

	public void CinemachineVirtualCameraCreatedByWorldBuilder(CinemachineVirtualCamera cinemachineVirtualCamera)
	{
		if (CinemachineVirtualCameras == null)
		{
			CinemachineVirtualCameras = new List<CinemachineVirtualCamera>();
		}
		CinemachineVirtualCameras.Add(cinemachineVirtualCamera);
	}

	public int GetRemainingRoomCount()
	{
		return TargetRoomCount - CurrentRoomCount;
	}

	public void SetConnectionPoint(RoomSide side, Room connectionPoint)
	{
		if (ConnectionRoomTable == null)
		{
			ConnectionRoomTable = new Dictionary<RoomSide, Room>();
		}
		ConnectionRoomTable.Add(side, connectionPoint);
	}

	public void SkyCreatedByWorldBuilder(Sky sky)
	{
		if (Skies == null)
		{
			Skies = new List<Sky>();
		}
		if (!Skies.Contains(sky))
		{
			Skies.Add(sky);
		}
	}

	public void WeatherCreatedByWorldBuilder(Weather[] weather)
	{
		if (Weather == null)
		{
			Weather = new List<Weather>();
		}
		for (int i = 0; i < weather.Length; i++)
		{
			Weather.Add(weather[i]);
		}
	}
}
