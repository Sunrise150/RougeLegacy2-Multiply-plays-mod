using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SpecialPlatformSpawnController : MonoBehaviour, ISimpleSpawnController, ISpawnController, IRoomConsumer
{
	[SerializeField]
	[HideInInspector]
	private SpecialPlatformType m_platformType;

	[SerializeField]
	[HideInInspector]
	private StateID m_initialState;

	[SerializeField]
	[ReadOnly]
	private int m_id;

	[SerializeField]
	[ReadOnly]
	private int m_width = 4;

	private SpawnLogicController m_spawnLogicController;

	private BaseRoom m_room;

	public const int MIN_WIDTH = 1;

	public const int MAX_WIDTH = 32;

	private static StateID[] m_potentialStates_STATIC;

	private bool m_hasCheckedForSpawnController;

	public SpecialPlatform SpecialPlatformInstance { get; private set; }

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

	public StateID InitialState
	{
		get
		{
			return m_initialState;
		}
		set
		{
			m_initialState = value;
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

	public SpecialPlatformType Type
	{
		get
		{
			return m_platformType;
		}
		set
		{
			m_platformType = value;
		}
	}

	public int Width
	{
		get
		{
			return m_width;
		}
		private set
		{
			m_width = value;
		}
	}

	private void Start()
	{
		SpriteRenderer component = base.gameObject.GetComponent<SpriteRenderer>();
		if (component != null)
		{
			component.enabled = false;
			return;
		}
		Debug.LogFormat("<color=red>| {0} | No SpriteRenderer found. If you see this message please add a bug report to Pivotal</color>", this);
	}

	private void OnDrawGizmos()
	{
	}

	public void SetColor(Color color)
	{
		if (!Application.isPlaying)
		{
			GetComponent<SpriteRenderer>().color = color;
			ID = EnemySpawnController.ColorTable[color];
		}
	}

	public void SetRoom(BaseRoom room)
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

	protected virtual void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs roomArgs)
	{
		if ((bool)SpecialPlatformInstance)
		{
			SpecialPlatformInstance.SetState(InitialState);
		}
	}

	public void SetWidth(int width)
	{
		if (!Application.isPlaying)
		{
			Width = Mathf.Clamp(width, 1, 32);
			SpriteRenderer component = GetComponent<SpriteRenderer>();
			component.size = new Vector2(Width, component.size.y);
			BoxCollider2D component2 = GetComponent<BoxCollider2D>();
			component2.size = new Vector2(Width, component2.size.y);
		}
	}

	public bool Spawn()
	{
		if (ShouldSpawn)
		{
			if (Type == SpecialPlatformType.BiomeSpecific)
			{
				SpecialPlatformType[] platformTypesInBiome = SpecialPlatformLibrary.GetPlatformTypesInBiome(Room.AppearanceBiomeType);
				int num = 0;
				int num2 = platformTypesInBiome.Length;
				if (num2 > 1)
				{
					num = RNGManager.GetRandomNumber(RngID.BiomeCreation, "Get Random Platform Index", 0, num2);
				}
				Type = platformTypesInBiome[num];
			}
			SpecialPlatformInstance = SpecialPlatformLibrary.CreatePlatformInstance(Type);
			if ((bool)SpecialPlatformInstance)
			{
				SpecialPlatformInstance.transform.SetParent(Room.gameObject.transform);
				SpecialPlatformInstance.transform.position = base.transform.position;
				SpecialPlatformInstance.Width = Width;
				List<IRoomConsumer> roomConsumerListHelper_STATIC = SimpleSpawnController.m_roomConsumerListHelper_STATIC;
				roomConsumerListHelper_STATIC.Clear();
				SpecialPlatformInstance.gameObject.GetComponentsInChildren(roomConsumerListHelper_STATIC);
				foreach (IRoomConsumer item in roomConsumerListHelper_STATIC)
				{
					item.SetRoom(Room);
				}
				if (InitialState == StateID.Random)
				{
					if (m_potentialStates_STATIC == null)
					{
						m_potentialStates_STATIC = Enum.GetValues(typeof(StateID)) as StateID[];
					}
					int randomNumber = RNGManager.GetRandomNumber(RngID.BiomeCreation, "Get Random Platform State Index", 0, m_potentialStates_STATIC.Length - 1);
					InitialState = (StateID)m_potentialStates_STATIC.GetValue(randomNumber);
				}
				SpecialPlatformInstance.SetState(InitialState);
			}
			base.gameObject.SetActive(value: false);
			return SpecialPlatformInstance;
		}
		return false;
	}

	[SpecialName]
	GameObject ISpawnController.get_gameObject()
	{
		return base.gameObject;
	}
}
