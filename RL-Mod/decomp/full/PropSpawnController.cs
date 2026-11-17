using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Sigtrap.Relays;
using Spawn;
using UnityEngine;

public class PropSpawnController : MonoBehaviour, ISimpleSpawnController, ISpawnController, ICameraLayerController, ISetSpawnType, IMirror
{
	[SerializeField]
	private bool m_override;

	[SerializeField]
	private Prop m_overrideProp;

	[SerializeField]
	private PropSpawnControllerData m_spawnControllerData;

	[SerializeField]
	private bool m_isOverrideSubLayer;

	[SerializeField]
	private int m_subLayerOverride;

	[SerializeField]
	private bool m_isOverrideSubLayerMod;

	[SerializeField]
	private int m_subLayerModOverride;

	[SerializeField]
	private bool m_isOverrideCameraLayer;

	[SerializeField]
	private bool m_isOverrideZPosition;

	[SerializeField]
	private float m_zPositionOverride;

	[SerializeField]
	private CameraLayer m_cameraLayerOverride;

	[SerializeField]
	private bool m_disableCulling;

	[SerializeField]
	private SpriteDrawMode m_spriteRendererDrawMode;

	[SerializeField]
	private Vector2 m_spriteRendererSize;

	private static List<Prop> m_breakableDecoPropsHelper_STATIC = new List<Prop>();

	private Prop m_propInstance;

	private Prop m_propPrefab;

	private int m_cachedPropPrefabNameHash;

	private Prop[] m_breakableDecoPropInstances;

	private bool m_breakableDecoPropsArrayInitialized;

	private BaseRoom m_room;

	private SpawnLogicController m_spawnLogicController;

	private SpriteRenderer m_spriteRenderer;

	private bool m_isMirrored;

	private Relay m_onPropInstanceInitializedRelay = new Relay();

	private Relay m_beforePropInstanceInitializedRelay = new Relay();

	private bool m_hasCheckedForSpawnLogicController;

	private DecoSpawnData[][] m_decoSpawnDataArray;

	private static Dictionary<Prop, DecoController[]> m_propPrefabDecoControllers_STATIC = new Dictionary<Prop, DecoController[]>();

	public SpriteDrawMode SpriteRendererDrawMode => m_spriteRendererDrawMode;

	public Vector2 SpriteRendererSize => m_spriteRendererSize;

	public DecoSpawnData[][] DecoSpawnData => m_decoSpawnDataArray;

	public IRelayLink BeforePropInstanceInitializedRelay => m_beforePropInstanceInitializedRelay.link;

	public IRelayLink OnPropInstanceInitializedRelay => m_onPropInstanceInitializedRelay.link;

	public bool DisableCulling
	{
		get
		{
			return m_disableCulling;
		}
		set
		{
			m_disableCulling = value;
		}
	}

	public bool IsBreakable
	{
		get
		{
			if ((bool)PropInstance)
			{
				return PropInstance.Breakable;
			}
			return false;
		}
	}

	public int ActualSubLayer => SubLayer + SubLayerMod;

	public CameraLayer CameraLayer
	{
		get
		{
			if (IsOverrideCameraLayer)
			{
				return CameraLayerOverride;
			}
			return Data.CameraLayer;
		}
	}

	public PropSpawnControllerData Data
	{
		get
		{
			return m_spawnControllerData;
		}
		set
		{
			m_spawnControllerData = value;
		}
	}

	public bool IsMirrored
	{
		get
		{
			return m_isMirrored;
		}
		private set
		{
			m_isMirrored = value;
		}
	}

	public bool ShouldSpawn
	{
		get
		{
			if ((bool)SpawnLogicController)
			{
				if (SpawnLogicController.ShouldSpawn)
				{
					return base.gameObject.activeSelf;
				}
				return false;
			}
			return true;
		}
	}

	public bool Override
	{
		get
		{
			return m_override;
		}
		set
		{
			if (!Application.isPlaying)
			{
				m_override = value;
			}
		}
	}

	public Prop OverrideProp
	{
		get
		{
			return m_overrideProp;
		}
		set
		{
			if (!Application.isPlaying)
			{
				m_overrideProp = value;
			}
		}
	}

	public Prop PropInstance
	{
		get
		{
			return m_propInstance;
		}
		private set
		{
			m_propInstance = value;
		}
	}

	public Prop PropPrefab
	{
		get
		{
			return m_propPrefab;
		}
		private set
		{
			if (m_propPrefab != value)
			{
				m_propPrefab = value;
				if ((bool)m_propPrefab)
				{
					m_cachedPropPrefabNameHash = Animator.StringToHash(m_propPrefab.name);
				}
			}
		}
	}

	public Prop[] BreakableDecoInstances => m_breakableDecoPropInstances;

	public BiomePropsEntry[] PropTable => Data.PropsPerBiome;

	public Prop[] DefaultProps => Data.DefaultProps;

	public BaseRoom Room => m_room;

	public SpawnLogicController SpawnLogicController
	{
		get
		{
			if (!m_hasCheckedForSpawnLogicController)
			{
				m_spawnLogicController = GetComponent<SpawnLogicController>();
				m_hasCheckedForSpawnLogicController = true;
			}
			return m_spawnLogicController;
		}
		set
		{
			m_spawnLogicController = value;
		}
	}

	public int SubLayer
	{
		get
		{
			if (IsOverrideSubLayer)
			{
				return SubLayerOverride;
			}
			return Data.SubLayer;
		}
	}

	public int SubLayerMod
	{
		get
		{
			if (IsOverrideSubLayerMod)
			{
				return SubLayerModOverride;
			}
			return Data.SubLayerMod;
		}
	}

	public bool IsOverrideSubLayer
	{
		get
		{
			return m_isOverrideSubLayer;
		}
		set
		{
			m_isOverrideSubLayer = value;
		}
	}

	public int SubLayerOverride
	{
		get
		{
			return m_subLayerOverride;
		}
		set
		{
			m_subLayerOverride = value;
		}
	}

	public bool IsOverrideSubLayerMod
	{
		get
		{
			return m_isOverrideSubLayerMod;
		}
		set
		{
			m_isOverrideSubLayerMod = value;
		}
	}

	public int SubLayerModOverride
	{
		get
		{
			return m_subLayerModOverride;
		}
		set
		{
			m_subLayerModOverride = value;
		}
	}

	public bool IsOverrideCameraLayer
	{
		get
		{
			return m_isOverrideCameraLayer;
		}
		set
		{
			m_isOverrideCameraLayer = value;
		}
	}

	public CameraLayer CameraLayerOverride
	{
		get
		{
			return m_cameraLayerOverride;
		}
		set
		{
			m_cameraLayerOverride = value;
		}
	}

	public bool IsOverrideZPosition
	{
		get
		{
			return m_isOverrideZPosition;
		}
		set
		{
			m_isOverrideZPosition = value;
		}
	}

	public float ZPositionOverride
	{
		get
		{
			return m_zPositionOverride;
		}
		set
		{
			m_zPositionOverride = value;
		}
	}

	public static DecoController[] GetPrefabDecoControllers(Prop propPrefab)
	{
		if (m_propPrefabDecoControllers_STATIC.TryGetValue(propPrefab, out var value))
		{
			return value;
		}
		value = propPrefab.GetComponents<DecoController>();
		m_propPrefabDecoControllers_STATIC.Add(propPrefab, value);
		return value;
	}

	public void ForcePropPrefab(Prop propPrefab)
	{
		PropPrefab = propPrefab;
	}

	private void Start()
	{
		if (!Override)
		{
			return;
		}
		bool flag = false;
		BiomePropsEntry[] propTable = PropTable;
		for (int i = 0; i < propTable.Length; i++)
		{
			if (propTable[i].Props.Contains(OverrideProp))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			OverrideProp = null;
			Override = false;
		}
	}

	private void GetPropInstance()
	{
		try
		{
			PropInstance = PropManager.GetProp(PropPrefab, m_cachedPropPrefabNameHash);
		}
		catch (Exception)
		{
			throw new Exception("<color=red>| " + this?.ToString() + " | An exception occurred while attempting to get a Prop instance.</color>");
		}
		if ((bool)PropInstance)
		{
			PropInstance.ResetInitialization();
			PropInstance.gameObject.SetActive(value: true);
			PropInstance.Initialize(Room, this);
			for (int i = 0; i < PropInstance.DecoControllers.Length; i++)
			{
				PropInstance.DecoControllers[i].Initialize(m_decoSpawnDataArray[i]);
			}
			IRoomConsumer[] roomConsumers = PropInstance.RoomConsumers;
			for (int j = 0; j < roomConsumers.Length; j++)
			{
				roomConsumers[j].SetRoom(Room);
			}
		}
		else
		{
			Debug.LogFormat("<color=red>| {0} | Failed to get instance of Prop ({1})</color>", this, base.name);
		}
	}

	public void Mirror()
	{
		IsMirrored = true;
	}

	public void InitializePropInstance()
	{
		if (!ShouldSpawn || !PropPrefab || !PropPrefab.gameObject)
		{
			return;
		}
		m_beforePropInstanceInitializedRelay.Dispatch();
		GetPropInstance();
		if ((bool)PropInstance)
		{
			m_breakableDecoPropsHelper_STATIC.Clear();
			DecoController[] decoControllers = PropInstance.DecoControllers;
			for (int i = 0; i < decoControllers.Length; i++)
			{
				DecoLocation[] decoLocations = decoControllers[i].DecoLocations;
				foreach (DecoLocation decoLocation in decoLocations)
				{
					if (!decoLocation.DecoInstance)
					{
						continue;
					}
					Prop[] props = decoLocation.DecoInstance.Props;
					foreach (Prop prop in props)
					{
						if ((bool)prop.Breakable)
						{
							m_breakableDecoPropsHelper_STATIC.Add(prop);
						}
					}
				}
			}
			if (!m_breakableDecoPropsArrayInitialized || ChallengeManager.IsInChallenge)
			{
				m_breakableDecoPropInstances = m_breakableDecoPropsHelper_STATIC.ToArray();
				m_breakableDecoPropsArrayInitialized = true;
			}
			else
			{
				if (m_breakableDecoPropInstances.Length != m_breakableDecoPropsHelper_STATIC.Count)
				{
					Debug.Log("Mismatch on : " + base.name + ". DecoPropHelperSize: " + m_breakableDecoPropsHelper_STATIC.Count + " propArraySize: " + m_breakableDecoPropInstances.Length);
				}
				for (int l = 0; l < m_breakableDecoPropInstances.Length; l++)
				{
					m_breakableDecoPropInstances[l] = m_breakableDecoPropsHelper_STATIC[l];
				}
			}
		}
		m_onPropInstanceInitializedRelay.Dispatch();
	}

	private void OnDisable()
	{
		if (!PropInstance)
		{
			return;
		}
		DecoController[] decoControllers = PropInstance.DecoControllers;
		for (int i = 0; i < decoControllers.Length; i++)
		{
			DecoLocation[] decoLocations = decoControllers[i].DecoLocations;
			for (int j = 0; j < decoLocations.Length; j++)
			{
				decoLocations[j].ReturnDecoToPool();
			}
		}
		PropInstance.gameObject.SetActive(value: false);
		PropInstance = null;
	}

	public bool Spawn()
	{
		if (Room == null)
		{
			throw new ArgumentNullException("Room");
		}
		if (ShouldSpawn && GameUtility.IsInLevelEditor)
		{
			SetSpawnType();
		}
		return PropPrefab;
	}

	public void SetSpawnType()
	{
		if (Override)
		{
			PropPrefab = OverrideProp;
		}
		else
		{
			Prop[] array = DefaultProps;
			BiomePropsEntry biomePropsEntry = null;
			BiomePropsEntry[] propTable = PropTable;
			foreach (BiomePropsEntry biomePropsEntry2 in propTable)
			{
				if (biomePropsEntry2.Biome == Room.AppearanceBiomeType)
				{
					biomePropsEntry = biomePropsEntry2;
					break;
				}
			}
			if (biomePropsEntry != null && biomePropsEntry.Props.Length != 0)
			{
				array = biomePropsEntry.Props;
			}
			if (array != null && array.Length != 0)
			{
				int num = 0;
				if (array.Length > 1)
				{
					num = RNGManager.GetRandomNumber(RngID.Prop_RoomSeed, $"PropSpawnController.SetSpawnType()", 0, array.Length);
				}
				PropPrefab = array[num];
			}
		}
		SetDecoTypes(PropPrefab);
	}

	private void SetDecoTypes(Prop prop)
	{
		if (!prop || !PropPrefab)
		{
			return;
		}
		DecoController[] prefabDecoControllers = GetPrefabDecoControllers(PropPrefab);
		int num = prefabDecoControllers.Length;
		m_decoSpawnDataArray = new DecoSpawnData[num][];
		for (int i = 0; i < num; i++)
		{
			DecoController decoController = prefabDecoControllers[i];
			int num2 = decoController.DecoLocations.Length;
			DecoSpawnData[] array = new DecoSpawnData[num2];
			for (int j = 0; j < num2; j++)
			{
				DecoLocation decoLocation = decoController.DecoLocations[j];
				DecoSpawnData decoSpawnData = default(DecoSpawnData);
				bool num3 = RNGManager.GetRandomNumber(RngID.Deco_RoomSeed, "Get is Deco empty", 0, 100) > Mathf.RoundToInt(100f * decoLocation.ChanceEmpty);
				int num4 = decoLocation.PotentialDecos.Length;
				if (num3 && num4 > 0)
				{
					int randomNumber = RNGManager.GetRandomNumber(RngID.Deco_RoomSeed, "Get Random Deco Index", 0, num4);
					bool isFlipped = RNGManager.GetRandomNumber(RngID.Deco_RoomSeed, "Get Flip Deco", 0, 2) == 1;
					decoSpawnData.ShouldSpawn = true;
					decoSpawnData.DecoPropIndex = (byte)randomNumber;
					decoSpawnData.IsFlipped = isFlipped;
				}
				array[j] = decoSpawnData;
			}
			m_decoSpawnDataArray[i] = array;
		}
	}

	public void SetCameraLayer(CameraLayer value)
	{
	}

	public void SetRoom(BaseRoom value)
	{
		m_room = value;
	}

	public void SetSubLayer(int subLayer, bool isDeco = false)
	{
	}

	[SpecialName]
	GameObject ISpawnController.get_gameObject()
	{
		return base.gameObject;
	}
}
