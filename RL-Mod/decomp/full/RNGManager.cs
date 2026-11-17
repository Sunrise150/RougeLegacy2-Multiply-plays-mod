using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class RNGManager : MonoBehaviour
{
	[SerializeField]
	[FormerlySerializedAs("m_maxRandomNumber")]
	private int m_maxRngControllerSeed = 1000;

	private static List<RNGController> m_rngControllers = null;

	private static Dictionary<RngID, RNGController> m_rngControllerTable = new Dictionary<RngID, RNGController>
	{
		{
			RngID.BiomeCreation,
			new RNGController(RngID.BiomeCreation)
		},
		{
			RngID.MergeRooms,
			new RNGController(RngID.MergeRooms)
		},
		{
			RngID.Chest,
			new RNGController(RngID.Chest)
		},
		{
			RngID.Enemy,
			new RNGController(RngID.Enemy)
		},
		{
			RngID.Lineage,
			new RNGController(RngID.Lineage)
		},
		{
			RngID.Prop,
			new RNGController(RngID.Prop)
		},
		{
			RngID.SpecialRoomProps,
			new RNGController(RngID.SpecialRoomProps)
		},
		{
			RngID.Room_RNGSeed,
			new RNGController(RngID.Room_RNGSeed)
		},
		{
			RngID.Room_RNGSeed_Generator,
			new RNGController(RngID.Room_RNGSeed_Generator)
		},
		{
			RngID.Prop_RoomSeed,
			new RNGController(RngID.Prop_RoomSeed)
		},
		{
			RngID.Deco_RoomSeed,
			new RNGController(RngID.Deco_RoomSeed)
		},
		{
			RngID.Enemy_RoomSeed,
			new RNGController(RngID.Enemy_RoomSeed)
		},
		{
			RngID.Chest_RoomSeed,
			new RNGController(RngID.Chest_RoomSeed)
		},
		{
			RngID.SpecialProps_RoomSeed,
			new RNGController(RngID.SpecialProps_RoomSeed)
		},
		{
			RngID.Hazards_RoomSeed,
			new RNGController(RngID.Hazards_RoomSeed)
		},
		{
			RngID.Tunnel_RoomSeed,
			new RNGController(RngID.Tunnel_RoomSeed)
		}
	};

	private static RNGManager Instance { get; set; }

	public static bool IsInstantiated => Instance != null;

	public static List<RNGController> RNGControllers
	{
		get
		{
			if (Application.isPlaying && m_rngControllers == null)
			{
				m_rngControllers = m_rngControllerTable.Values.Select((RNGController entry) => entry).ToList();
			}
			return m_rngControllers;
		}
	}

	public static event EventHandler<EventArgs> InitialisedEvent;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			if (base.transform.parent == null)
			{
				UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			}
			if (RNGManager.InitialisedEvent != null)
			{
				RNGManager.InitialisedEvent(Instance, EventArgs.Empty);
			}
		}
		else
		{
			Debug.LogFormat("<color=red>|{0}| Multiple instances present in Scene. Destroying duplicate</color>", this);
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public static int GetRandomNumber(RngID randomNumberGenerator, string callerDescription, int minInclusive, int maxExclusive)
	{
		if (!m_rngControllerTable[randomNumberGenerator].IsInitialized)
		{
			InitializeRandomNumberGenerators();
		}
		return m_rngControllerTable[randomNumberGenerator].GetRandomNumber(callerDescription, minInclusive, maxExclusive);
	}

	public static float GetRandomNumber(RngID randomNumberGenerator, string callerDescription, float min, float max)
	{
		if (!m_rngControllerTable[randomNumberGenerator].IsInitialized)
		{
			InitializeRandomNumberGenerators();
		}
		return m_rngControllerTable[randomNumberGenerator].GetRandomNumber(callerDescription, min, max);
	}

	public static int GetSeed(RngID id)
	{
		if (m_rngControllerTable.ContainsKey(id))
		{
			return m_rngControllerTable[id].Seed;
		}
		return -1;
	}

	public static void InitializeRandomNumberGenerators()
	{
		int currentSeed = RNGSeedManager.GetCurrentSeed(SceneLoadingUtility.ActiveScene.name);
		if (currentSeed == -1)
		{
			RNGSeedManager.GenerateNewSeed(SceneLoadingUtility.ActiveScene.name);
			currentSeed = RNGSeedManager.GetCurrentSeed(SceneLoadingUtility.ActiveScene.name);
		}
		InitializeRandomNumberGenerators(currentSeed);
	}

	public static void InitializeRandomNumberGenerators(int masterSeed)
	{
		System.Random random = new System.Random(masterSeed);
		foreach (KeyValuePair<RngID, RNGController> item in m_rngControllerTable)
		{
			item.Value.SetSeed(random.Next(0, Instance.m_maxRngControllerSeed));
		}
	}

	public static void SetSeed(RngID id, int seed)
	{
		if (m_rngControllerTable.TryGetValue(id, out var value))
		{
			value.SetSeed(seed);
		}
		else
		{
			_ = $"<color=red>| RNGManager | The RNGController table does not contain an entry for RngID (<b>{id}</b>). If you see this message, please add a bug report to Pivotal.</color>";
		}
	}

	private void OnDestroy()
	{
		m_rngControllerTable = null;
	}

	public static void PrintSeedsToConsole()
	{
		string text = string.Format("<color=blue>| {0} |: Master Seed = (HEX = {1}, INT = {2}) ", Instance, RNGSeedManager.GetCurrentSeed(SceneLoadingUtility.ActiveScene.name).ToString("X"), RNGSeedManager.GetCurrentSeed(SceneLoadingUtility.ActiveScene.name));
		foreach (KeyValuePair<RngID, RNGController> item in m_rngControllerTable)
		{
			RNGController value = item.Value;
			text += $"<color=blue>({value.ID}) RNG Seed = ({value.Seed}) </color>";
		}
		text += "</color>";
		Debug.LogFormat("{0}", text);
	}

	public static void Reset()
	{
		RNGSeedManager.GenerateNewSeed(SceneLoadingUtility.ActiveScene.name);
		InitializeRandomNumberGenerators();
	}

	public static void Reset(int seedOverride)
	{
		RNGSeedManager.SetTempSeed(seedOverride.ToString("X"));
		Reset();
	}
}
