using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace Rooms;

public class RoomMetaData : ScriptableObject
{
	[SerializeField]
	[ReadOnly]
	private bool m_canFlip;

	[SerializeField]
	[ReadOnly]
	private bool m_canMerge;

	[SerializeField]
	[ReadOnly]
	private DoorLocation[] m_doorLocations;

	[SerializeField]
	[ReadOnly]
	private RoomID m_id;

	[SerializeField]
	private string m_roomPath;

	[SerializeField]
	[ReadOnly]
	private string m_contentPath;

	[SerializeField]
	[ReadOnly]
	private Vector2Int m_size;

	[SerializeField]
	[ReadOnly]
	private int[] m_tunnelExits;

	[SerializeField]
	[ReadOnly]
	private SpecialRoomType m_specialRoomType;

	[SerializeField]
	[ReadOnly]
	private bool m_showIconOnMap = true;

	[SerializeField]
	[ReadOnly]
	private bool m_isEasy;

	[SerializeField]
	private BiomeType m_biomeOverride;

	private Room m_cachedPrefab;

	private RoomContentMetaData m_cachedContent;

	public bool CanFlip
	{
		get
		{
			return m_canFlip;
		}
		private set
		{
			m_canFlip = value;
		}
	}

	public bool CanMerge
	{
		get
		{
			return m_canMerge;
		}
		private set
		{
			m_canMerge = value;
		}
	}

	public DoorLocation[] DoorLocations
	{
		get
		{
			return m_doorLocations;
		}
		private set
		{
			m_doorLocations = value;
		}
	}

	public string RoomPath
	{
		get
		{
			return m_roomPath;
		}
		private set
		{
			m_roomPath = value;
		}
	}

	public bool IsEasy
	{
		get
		{
			return m_isEasy;
		}
		private set
		{
			m_isEasy = value;
		}
	}

	public RoomID ID
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

	public Vector2Int Size
	{
		get
		{
			return m_size;
		}
		private set
		{
			m_size = value;
		}
	}

	public int[] ExitTunnels
	{
		get
		{
			return m_tunnelExits;
		}
		private set
		{
			m_tunnelExits = value;
		}
	}

	public bool IsSpecialRoom => SpecialRoomType != SpecialRoomType.None;

	public SpecialRoomType SpecialRoomType
	{
		get
		{
			return m_specialRoomType;
		}
		private set
		{
			m_specialRoomType = value;
		}
	}

	public string ContentPath => m_contentPath;

	public bool ShowIconOnMap
	{
		get
		{
			return m_showIconOnMap;
		}
		private set
		{
			m_showIconOnMap = value;
		}
	}

	public BiomeType BiomeOverride
	{
		get
		{
			return m_biomeOverride;
		}
		private set
		{
			m_biomeOverride = value;
		}
	}

	public IEnumerator LoadPrefabAsync()
	{
		if (!m_cachedPrefab && !string.IsNullOrEmpty(RoomPath))
		{
			CDGAsyncLoadRequest<GameObject> req = CDGResources.LoadAsync<GameObject>(RoomPath, "rooms");
			while (!req.IsDone)
			{
				yield return null;
			}
			m_cachedPrefab = req.Asset.GetComponent<Room>();
		}
	}

	public Room GetPrefab(bool throwOnNull = true)
	{
		if (!m_cachedPrefab && !string.IsNullOrEmpty(RoomPath))
		{
			m_cachedPrefab = CDGResources.Load<GameObject>(RoomPath, "rooms", throwOnNull).GetComponent<Room>();
		}
		return m_cachedPrefab;
	}

	public RoomContentMetaData GetContent()
	{
		if (m_cachedContent == null && !string.IsNullOrEmpty(ContentPath))
		{
			m_cachedContent = CDGResources.Load<RoomContentMetaData>(ContentPath);
		}
		return m_cachedContent;
	}

	public void ClearCachedPrefab()
	{
		m_cachedPrefab = null;
	}

	public void ClearCachedContentData()
	{
		m_cachedContent = null;
	}

	public void UpdateRoomPath(string path)
	{
		RoomPath = path;
	}

	public RoomMetaData(Room room)
	{
		UpdateData(room);
	}

	public void UpdateData(Room room)
	{
		CanFlip = room.CanFlip;
		CanMerge = room.CanMerge;
		Door[] array = (from door in room.GetComponentsInChildren<Door>(includeInactive: true)
			where !door.DisabledFromLevelEditor
			select door).ToArray();
		DoorLocations = new DoorLocation[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			DoorLocations[i] = new DoorLocation(array[i].Side, array[i].Number);
		}
		ID = room.RoomID;
		Size = room.Size;
		BaseSpecialRoomController component = room.GetComponent<BaseSpecialRoomController>();
		if (component != null)
		{
			SpecialRoomType = component.SpecialRoomType;
		}
		else
		{
			SpecialRoomType = SpecialRoomType.None;
		}
		if ((bool)room.GetComponent<HeirloomBiomeOverrideController>())
		{
			SpecialRoomType = SpecialRoomType.Heirloom;
		}
		TunnelSpawnController componentInChildren = room.GetComponentInChildren<TunnelSpawnController>();
		if (componentInChildren != null && componentInChildren.Category == TunnelCategory.Boss)
		{
			SpecialRoomType = SpecialRoomType.BossEntrance;
		}
		bool showIconOnMap = true;
		MapRoomEntryIconOverrideController component2 = room.GetComponent<MapRoomEntryIconOverrideController>();
		if (component2 != null)
		{
			showIconOnMap = component2.ShowRoomIconOnMap;
		}
		ShowIconOnMap = showIconOnMap;
		TunnelSpawnController[] array2 = (from spawnController in room.GetComponentsInChildren<TunnelSpawnController>(includeInactive: true)
			where spawnController.Direction == TunnelDirection.Exit
			select spawnController).ToArray();
		ExitTunnels = new int[array2.Length];
		for (int j = 0; j < array2.Length; j++)
		{
			ExitTunnels[j] = array2[j].Index;
		}
		BiomeOverride = room.AppearanceOverride;
		IsEasy = room.IsEasy;
	}

	public bool GetHasExitTunnel(int index)
	{
		for (int i = 0; i < ExitTunnels.Length; i++)
		{
			if (ExitTunnels[i] == index)
			{
				return true;
			}
		}
		return false;
	}

	public bool GetHasDoor(DoorLocation doorLocation, bool isMirrored = false)
	{
		if (isMirrored)
		{
			DoorLocation mirrorDoorLocation = RoomUtility.GetMirrorDoorLocation(Size, doorLocation);
			for (int i = 0; i < DoorLocations.Length; i++)
			{
				if (DoorLocations[i] == mirrorDoorLocation)
				{
					return true;
				}
			}
			return false;
		}
		for (int j = 0; j < DoorLocations.Length; j++)
		{
			if (DoorLocations[j] == doorLocation)
			{
				return true;
			}
		}
		return false;
	}

	public IEnumerable<DoorLocation> GetDoorsOnSide(RoomSide side, bool isMirrored = false)
	{
		if (DoorLocations == null)
		{
			Debugger.Break();
		}
		if (isMirrored)
		{
			side = RoomUtility.GetMirrorSide(side);
		}
		return DoorLocations.Where((DoorLocation location) => location.RoomSide == side);
	}

	public bool GetHasDoorsOnSide(RoomSide side, bool isMirrored)
	{
		return GetDoorsOnSide(side, isMirrored).Count() > 0;
	}
}
