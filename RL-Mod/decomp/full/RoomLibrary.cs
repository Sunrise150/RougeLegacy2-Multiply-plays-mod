using System;
using System.Collections.Generic;
using Rooms;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomLibrary : MonoBehaviour
{
	[SerializeField]
	private string m_biomeRoomLibraryPath = "Scriptable Objects/RoomPool";

	private RoomPool m_biomeRoomLibrary;

	[SerializeField]
	private bool m_releaseRoomPoolAfterLoad = true;

	public const string COMPILED_ROOMS_SUB_DIRECTORY = "Prefabs/Compiled Rooms";

	public const string STORAGE_PATH = "Assets/Content/Prefabs/Compiled Rooms";

	public const string PREFAB_RESOURCES_PATH = "Prefabs/Managers/RoomLibrary";

	private static Dictionary<BiomeType, RoomSetCollection> m_biomeToSetCollectionTable = null;

	private static Dictionary<string, CompiledScene_ScriptableObject> m_compiledSceneTable = new Dictionary<string, CompiledScene_ScriptableObject>();

	public static Dictionary<BiomeType, RoomSetCollection> BiomeToSetCollectionTable
	{
		get
		{
			return m_biomeToSetCollectionTable;
		}
		private set
		{
			m_biomeToSetCollectionTable = value;
		}
	}

	private static RoomLibrary Instance { get; set; }

	public static bool IsInstantiated => Instance != null;

	public static bool IsLoaded { get; private set; }

	public static Vector2Int MaxRoomSize { get; private set; }

	public static Dictionary<string, CompiledScene_ScriptableObject> CompiledSceneTable
	{
		get
		{
			return m_compiledSceneTable;
		}
		private set
		{
			m_compiledSceneTable = value;
		}
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			if (GameUtility.IsInLevelEditor)
			{
				SceneManager.sceneLoaded += OnSceneLoaded;
			}
			else
			{
				LoadCompiledRooms();
			}
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
	{
		if (!scene.name.StartsWith("Level"))
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
			LoadCompiledRooms();
		}
	}

	private void OnDestroy()
	{
		if (Instance != null)
		{
			MaxRoomSize = Vector2Int.zero;
			IsLoaded = false;
			if (BiomeToSetCollectionTable != null)
			{
				BiomeToSetCollectionTable.Clear();
				BiomeToSetCollectionTable = null;
			}
			Instance = null;
		}
	}

	private void LoadCompiledRooms()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (m_biomeRoomLibrary == null)
		{
			m_biomeRoomLibrary = CDGResources.Load<RoomPool>(m_biomeRoomLibraryPath);
		}
		BiomeType[] typeArray = BiomeType_RL.TypeArray;
		foreach (BiomeType biomeType in typeArray)
		{
			if (BiomeType_RL.IsValidBiome(biomeType))
			{
				LoadCompiledRooms(biomeType, m_biomeRoomLibrary);
			}
		}
		IsLoaded = true;
	}

	public static void LoadCompiledRoomsFromLevelEditor()
	{
		if (GameUtility.IsInLevelEditor && Instance != null)
		{
			Instance.LoadCompiledRooms();
		}
	}

	private void LoadCompiledRooms(BiomeType biome, RoomPool roomPool)
	{
		if (BiomeToSetCollectionTable == null)
		{
			BiomeToSetCollectionTable = new Dictionary<BiomeType, RoomSetCollection>();
		}
		BiomeToSetCollectionTable.Add(biome, new RoomSetCollection(biome, roomPool));
		foreach (RoomType value in Enum.GetValues(typeof(RoomType)))
		{
			CompiledScene_ScriptableObject[] compiledScenes = roomPool.GetCompiledScenes(biome, value);
			foreach (CompiledScene_ScriptableObject compiledScene_ScriptableObject in compiledScenes)
			{
				if (!CompiledSceneTable.ContainsKey(compiledScene_ScriptableObject.SceneID))
				{
					CompiledSceneTable.Add(compiledScene_ScriptableObject.SceneID, compiledScene_ScriptableObject);
				}
				if (compiledScene_ScriptableObject.RoomMetaData != null)
				{
					foreach (RoomMetaData roomMetaDatum in compiledScene_ScriptableObject.RoomMetaData)
					{
						if (!(roomMetaDatum == null))
						{
							if (roomMetaDatum.Size.x > MaxRoomSize.x)
							{
								MaxRoomSize = new Vector2Int(roomMetaDatum.Size.x, MaxRoomSize.y);
							}
							if (roomMetaDatum.Size.y > MaxRoomSize.y)
							{
								MaxRoomSize = new Vector2Int(MaxRoomSize.x, roomMetaDatum.Size.y);
							}
						}
					}
				}
				else
				{
					Debug.LogFormat("<color=red>[{0}] Room GameObject List is null on Compiled Scene SO ({1})</color>", this, compiledScene_ScriptableObject.name);
				}
			}
		}
	}

	public static RoomSetCollection GetSetCollection(BiomeType biome)
	{
		if (!IsLoaded)
		{
			Instance.LoadCompiledRooms();
		}
		return BiomeToSetCollectionTable[biome];
	}

	public static RoomMetaData GetRoomMetaDataFromID(RoomID roomID)
	{
		string sceneName = roomID.SceneName;
		if (!m_compiledSceneTable.ContainsKey(sceneName))
		{
			CompiledScene_ScriptableObject value = CDGResources.Load<CompiledScene_ScriptableObject>("Prefabs/Compiled Rooms/" + sceneName + "_Compiled", "rooms");
			m_compiledSceneTable.Add(sceneName, value);
		}
		List<RoomMetaData> roomMetaData = m_compiledSceneTable[sceneName].RoomMetaData;
		RoomMetaData roomMetaData2 = null;
		for (int i = 0; i < roomMetaData.Count; i++)
		{
			if (roomMetaData[i].ID == roomID)
			{
				roomMetaData2 = roomMetaData[i];
				break;
			}
		}
		if (roomMetaData2 == null)
		{
			throw new InvalidOperationException($"| RoomLibrary | The Compiled Scene ({m_compiledSceneTable[sceneName].name}) does not contain a RoomMetaData object with ID ({roomID})");
		}
		return roomMetaData2;
	}
}
