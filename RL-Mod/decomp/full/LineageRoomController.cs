using System;
using System.Collections;
using RLAudio;
using SceneManagement_RL;
using UnityEngine;

public class LineageRoomController : MonoBehaviour
{
	private enum LineageRoomType
	{
		None = 0,
		Starting = 10,
		Connecting = 20,
		Ending = 30
	}

	private static Room[] m_connectingRoomArray;

	private static LineageRoomController[] m_connectingRoomControllerArray;

	private static Room m_currentRoom;

	private static int m_roomIndex;

	private static Vector3 m_startingRoomPosition;

	public static int NumConnectorRooms;

	public static int NumPortraits;

	[SerializeField]
	private LineageRoomType m_roomType;

	[Header("Starting Room Fields")]
	[SerializeField]
	private Room m_connectorRoomPrefab;

	[SerializeField]
	private Room m_endingRoomPrefab;

	[Header("Connecting Room Fields")]
	[SerializeField]
	private LineagePortrait m_leftPortrait;

	[SerializeField]
	private LineagePortrait m_centrePortrait;

	[SerializeField]
	private LineagePortrait m_rightPortrait;

	[Header("End Room Fields")]
	[SerializeField]
	private GameObject m_lineageModelPositionObject;

	[SerializeField]
	private GameObject m_finalBossTunnelObject;

	[Header("Fields For All Rooms")]
	[SerializeField]
	private PlayerTrigger m_roomTrigger;

	private Room m_room;

	private Room m_endingRoom;

	private static bool m_hasBiomeEnterEventBroadcast;

	private static GameObject m_sky;

	private Animator m_animator;

	private Action<MonoBehaviour, EventArgs> m_onHeirSelectionChanged;

	private Action<MonoBehaviour, EventArgs> m_onBiomeCreationComplete;

	private Coroutine m_spotlightTweenCoroutine;

	private float m_currentSpotlightPos;

	public static int NumRooms => NumConnectorRooms + 2;

	public bool InstantiatingFromLineageWindow => SceneLoadingUtility.ActiveScene.name == SceneLoadingUtility.GetSceneName(SceneID.Lineage);

	public Room EndingRoom => m_endingRoom;

	public GameObject LineageModelPositionObject => m_lineageModelPositionObject;

	public LineagePortrait LeftPortrait => m_leftPortrait;

	public LineagePortrait RightPortrait => m_rightPortrait;

	public LineagePortrait CentrePortrait => m_centrePortrait;

	public bool IsInitialized { get; private set; }

	public void EnableSpotlight(bool turnOn)
	{
		if (m_roomType == LineageRoomType.Ending)
		{
			m_animator.SetBool("SpotlightOn", turnOn);
		}
		else if ((bool)m_endingRoom)
		{
			m_endingRoom.GetComponent<LineageRoomController>().EnableSpotlight(turnOn);
		}
	}

	private void Awake()
	{
		m_room = GetComponent<Room>();
		m_animator = GetComponent<Animator>();
		if ((bool)m_roomTrigger && InstantiatingFromLineageWindow)
		{
			m_roomTrigger.PlayerEnter += OnEnterRoom;
		}
		if (!m_sky && InstantiatingFromLineageWindow)
		{
			Sky sky = BiomeCreatorTools.CreateSkyInstance(BiomeArtDataLibrary.GetArtData(BiomeType.Lineage));
			sky.RoomList.Add(m_room);
			m_sky = sky.gameObject;
		}
		if (m_roomType == LineageRoomType.Starting)
		{
			m_onBiomeCreationComplete = OnBiomeCreationComplete;
			Initialize();
		}
		if (m_roomType == LineageRoomType.Ending)
		{
			if (InstantiatingFromLineageWindow)
			{
				m_finalBossTunnelObject.SetActive(value: false);
			}
			m_onHeirSelectionChanged = OnHeirSelectionChanged;
			Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.Lineage_SelectedNewHeir, m_onHeirSelectionChanged);
		}
	}

	private void OnEnable()
	{
		if ((m_roomType == LineageRoomType.Starting || m_roomType == LineageRoomType.Ending) && !InstantiatingFromLineageWindow && WorldBuilder.State == BiomeBuildStateID.Complete)
		{
			StartCoroutine(OnEnterManorCoroutine());
		}
	}

	private IEnumerator OnEnterManorCoroutine()
	{
		if (m_roomType == LineageRoomType.Starting)
		{
			SetupRoom(0);
			int max = Mathf.Max(1, NumConnectorRooms);
			int num = Mathf.Clamp(NumConnectorRooms, 0, max);
			for (int i = 0; i < num; i++)
			{
				m_connectingRoomArray[i].GetComponent<LineageRoomController>().SetupRoom(i + 1);
			}
			m_endingRoom.GetComponent<LineageRoomController>().SetupRoom(NumConnectorRooms - 1);
		}
		while (SceneLoader_RL.IsRunningTransitionWithLogic)
		{
			yield return null;
		}
		if (m_roomType == LineageRoomType.Ending)
		{
			AmbientSoundController.Instance.PlayManorAmbientAudio();
			ForceNowEnteringEventArgs eventArgs = new ForceNowEnteringEventArgs("LOC_ID_LOCATION_TITLE_HOME_1", 7);
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.ForceNowEntering, this, eventArgs);
		}
	}

	private void Start()
	{
		if (!m_hasBiomeEnterEventBroadcast)
		{
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.BiomeEnter, this, new BiomeEventArgs(BiomeType.Lineage));
			m_hasBiomeEnterEventBroadcast = true;
		}
	}

	private void OnDestroy()
	{
		if ((bool)m_roomTrigger && InstantiatingFromLineageWindow)
		{
			m_roomTrigger.PlayerEnter -= OnEnterRoom;
		}
		if (m_roomType == LineageRoomType.Starting)
		{
			if (m_connectingRoomArray != null)
			{
				Array.Clear(m_connectingRoomArray, 0, m_connectingRoomArray.Length);
			}
			m_connectingRoomArray = null;
			if (m_connectingRoomControllerArray != null)
			{
				Array.Clear(m_connectingRoomControllerArray, 0, m_connectingRoomControllerArray.Length);
			}
			m_connectingRoomControllerArray = null;
			m_currentRoom = null;
			m_roomIndex = 0;
			m_startingRoomPosition = Vector2.zero;
			NumConnectorRooms = 0;
			NumPortraits = 0;
		}
		if (m_roomType == LineageRoomType.Ending)
		{
			Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.Lineage_SelectedNewHeir, m_onHeirSelectionChanged);
		}
		m_hasBiomeEnterEventBroadcast = false;
		if ((bool)m_sky)
		{
			UnityEngine.Object.Destroy(m_sky);
			m_sky = null;
		}
	}

	private void Initialize()
	{
		if (SaveManager.LineageSaveData.LineageHeirList.Count <= 0)
		{
			throw new Exception("No portraits found in save data. Lineage Room Controller currently cannot handle this scenario.");
		}
		NumPortraits = SaveManager.LineageSaveData.LineageHeirList.Count;
		if (!InstantiatingFromLineageWindow)
		{
			NumPortraits = Mathf.Min(NumPortraits, 20);
		}
		int num = NumPortraits - 4;
		if (num > 0)
		{
			num = (NumConnectorRooms = Mathf.CeilToInt((float)num / 3f));
		}
		m_leftPortrait.gameObject.SetActive(value: false);
		m_centrePortrait.gameObject.SetActive(value: false);
		m_rightPortrait.gameObject.SetActive(value: false);
		SetupRoom(0);
		int num2 = (InstantiatingFromLineageWindow ? 3 : Mathf.Max(1, num));
		m_connectingRoomArray = new Room[num2];
		m_connectingRoomControllerArray = new LineageRoomController[num2];
		m_startingRoomPosition = m_room.transform.position;
		int num3 = Mathf.Clamp(NumConnectorRooms, 0, num2);
		for (int i = 0; i < num3; i++)
		{
			Room room = UnityEngine.Object.Instantiate(m_connectorRoomPrefab, InstantiatingFromLineageWindow ? m_room.transform.parent : m_room.transform);
			if (InstantiatingFromLineageWindow)
			{
				PositionRoomAtIndex(room.gameObject, NumConnectorRooms - i);
			}
			else
			{
				PositionRoomAtIndex(room.gameObject, i + 1);
			}
			room.Initialise(BiomeType.Lineage, RoomType.Standard, 0);
			RoomUtility.BuildAllFerr2DTerrains(room);
			m_connectingRoomArray[i] = room;
			LineageRoomController component = room.GetComponent<LineageRoomController>();
			if (InstantiatingFromLineageWindow)
			{
				component.SetupRoom(NumConnectorRooms - i);
			}
			m_connectingRoomControllerArray[i] = component;
		}
		m_endingRoom = UnityEngine.Object.Instantiate(m_endingRoomPrefab, InstantiatingFromLineageWindow ? m_room.transform.parent : m_room.transform);
		m_endingRoom.Initialise(BiomeType.Lineage, RoomType.Standard, 0);
		RoomUtility.BuildAllFerr2DTerrains(m_endingRoom);
		PositionRoomAtIndex(m_endingRoom.gameObject, NumConnectorRooms + 1);
		if (InstantiatingFromLineageWindow)
		{
			m_endingRoom.GetComponent<LineageRoomController>().SetupRoom(NumConnectorRooms - 1);
		}
		if (InstantiatingFromLineageWindow)
		{
			m_sky.transform.SetPositionX(m_endingRoom.gameObject.transform.position.x);
		}
		if (!m_room.IsInitialised)
		{
			m_room.Initialise(BiomeType.Lineage, RoomType.Standard);
			RoomUtility.BuildAllFerr2DTerrains(m_room);
		}
		Vector2[] array = new Vector2[4]
		{
			new Vector2(m_room.Bounds.min.x, m_room.Bounds.max.y),
			new Vector2(m_room.Bounds.min.x, m_room.Bounds.min.y),
			new Vector2(m_room.Bounds.max.x + (float)(m_room.UnitDimensions.x * (NumConnectorRooms + 1)), m_room.Bounds.min.y),
			new Vector2(m_room.Bounds.max.x + (float)(m_room.UnitDimensions.x * (NumConnectorRooms + 1)), m_room.Bounds.max.y)
		};
		for (int j = 0; j < array.Length; j++)
		{
			array[j].x -= m_room.transform.position.x;
			array[j].y -= m_room.transform.position.y;
		}
		m_room.GetComponent<PolygonCollider2D>().SetPath(0, array);
		if (!InstantiatingFromLineageWindow)
		{
			m_room.ResetBounds();
			if (GameUtility.IsInLevelEditor)
			{
				Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.LevelEditorWorldCreationComplete, m_onBiomeCreationComplete);
			}
			else
			{
				Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.BiomeCreationComplete, m_onBiomeCreationComplete);
			}
		}
		ResetRoomIndex(this, InstantiatingFromLineageWindow);
		IsInitialized = true;
	}

	public static void ResetRoomIndex(LineageRoomController startingRoom, bool instantiatingFromLineageWindow)
	{
		if (instantiatingFromLineageWindow)
		{
			m_currentRoom = startingRoom.EndingRoom;
			m_roomIndex = NumRooms - 1;
		}
		else
		{
			m_currentRoom = startingRoom.m_room;
			m_roomIndex = 0;
		}
	}

	private void OnHeirSelectionChanged(MonoBehaviour sender, EventArgs args)
	{
		if (m_roomType != LineageRoomType.Ending)
		{
			return;
		}
		LineageWindowController lineageWindowController = sender as LineageWindowController;
		if (lineageWindowController.NumberOfSuccessors <= 1)
		{
			m_animator.SetFloat("CharacterSelection", 0.5f);
			return;
		}
		float desiredSpotlightPos = (float)lineageWindowController.CurrentSelectedCharacterIndex / (float)(lineageWindowController.NumberOfSuccessors - 1);
		if (m_spotlightTweenCoroutine != null)
		{
			StopCoroutine(m_spotlightTweenCoroutine);
		}
		m_spotlightTweenCoroutine = StartCoroutine(SpotlightTweenCoroutine(desiredSpotlightPos));
	}

	private IEnumerator SpotlightTweenCoroutine(float desiredSpotlightPos)
	{
		float tweenSpeed = 10f * Time.unscaledDeltaTime;
		bool moveLeft = desiredSpotlightPos < m_currentSpotlightPos;
		while ((m_currentSpotlightPos < desiredSpotlightPos && !moveLeft) || (m_currentSpotlightPos > desiredSpotlightPos && moveLeft))
		{
			if (!moveLeft)
			{
				m_currentSpotlightPos += tweenSpeed;
			}
			else
			{
				m_currentSpotlightPos -= tweenSpeed;
			}
			m_animator.SetFloat("CharacterSelection", m_currentSpotlightPos);
			if ((m_currentSpotlightPos < desiredSpotlightPos && !moveLeft) || (m_currentSpotlightPos > desiredSpotlightPos && moveLeft))
			{
				yield return null;
			}
		}
		m_animator.SetFloat("CharacterSelection", desiredSpotlightPos);
	}

	private void PositionRoomAtIndex(GameObject room, int index)
	{
		Vector3 startingRoomPosition = m_startingRoomPosition;
		startingRoomPosition.x = m_startingRoomPosition.x + (float)(index * m_room.UnitDimensions.x);
		room.transform.position = startingRoomPosition;
	}

	private void OnEnterRoom(object sender, EventArgs args)
	{
		if (m_currentRoom != m_room)
		{
			bool incremented = false;
			if (m_room.transform.position.x > m_currentRoom.transform.position.x)
			{
				m_roomIndex++;
				incremented = true;
			}
			else
			{
				m_roomIndex--;
			}
			m_currentRoom = m_room;
			if (m_roomIndex > 1 && m_roomIndex < NumConnectorRooms)
			{
				ShiftRooms(incremented);
			}
		}
	}

	private void ShiftRooms(bool incremented)
	{
		for (int i = 0; i < m_connectingRoomArray.Length; i++)
		{
			Room room = m_connectingRoomArray[i];
			if (room != m_currentRoom && Mathf.Abs(m_currentRoom.transform.position.x - room.transform.position.x) > m_room.Bounds.size.x)
			{
				LineageRoomController lineageRoomController = m_connectingRoomControllerArray[i];
				if (incremented)
				{
					PositionRoomAtIndex(room.gameObject, m_roomIndex + 1);
					lineageRoomController.SetupRoom(m_roomIndex + 1);
				}
				else
				{
					PositionRoomAtIndex(room.gameObject, m_roomIndex - 1);
					lineageRoomController.SetupRoom(m_roomIndex - 1);
				}
				break;
			}
		}
	}

	public void SetupRoom(int roomIndex)
	{
		if (m_roomType == LineageRoomType.Starting)
		{
			if (NumPortraits > 1)
			{
				int num = NumPortraits - 1;
				int num2 = (int)((float)num / 3f) * 3;
				int num3 = num - num2;
				if (num3 == 0)
				{
					num3 = 3;
				}
				switch (num3)
				{
				case 1:
					m_rightPortrait.gameObject.SetActive(value: true);
					SetPortrait(m_rightPortrait, 0);
					break;
				case 2:
					m_rightPortrait.gameObject.SetActive(value: true);
					m_centrePortrait.gameObject.SetActive(value: true);
					SetPortrait(m_rightPortrait, 1);
					SetPortrait(m_centrePortrait, 0);
					break;
				case 3:
					m_rightPortrait.gameObject.SetActive(value: true);
					m_centrePortrait.gameObject.SetActive(value: true);
					m_leftPortrait.gameObject.SetActive(value: true);
					SetPortrait(m_rightPortrait, 2);
					SetPortrait(m_centrePortrait, 1);
					SetPortrait(m_leftPortrait, 0);
					break;
				}
			}
		}
		else if (m_roomType == LineageRoomType.Connecting)
		{
			int num4 = NumPortraits - 1;
			int num5 = (int)((float)num4 / 3f) * 3;
			int num6 = num4 - num5;
			if (num6 == 0)
			{
				num6 = 3;
			}
			int num7 = roomIndex * 3 - (3 - num6);
			SetPortrait(m_leftPortrait, num7);
			SetPortrait(m_centrePortrait, num7 + 1);
			SetPortrait(m_rightPortrait, num7 + 2);
		}
		else
		{
			SetPortrait(m_leftPortrait, NumPortraits - 1);
		}
	}

	private void SetPortrait(LineagePortrait portrait, int portraitIndex)
	{
		CharacterData portraitLook;
		if (InstantiatingFromLineageWindow)
		{
			portraitLook = SaveManager.LineageSaveData.LineageHeirList[portraitIndex];
		}
		else
		{
			int count = SaveManager.LineageSaveData.LineageHeirList.Count;
			int value = count - NumPortraits + portraitIndex;
			value = Mathf.Clamp(value, 0, count - 1);
			portraitLook = SaveManager.LineageSaveData.LineageHeirList[value];
		}
		portrait.Index = portraitIndex;
		portrait.SetPortraitLook(portraitLook);
	}

	public LineagePortrait GetPortraitAtIndex(int portraitIndex)
	{
		if (RightPortrait.Index == portraitIndex)
		{
			return RightPortrait;
		}
		if (CentrePortrait.Index == portraitIndex)
		{
			return CentrePortrait;
		}
		if (LeftPortrait.Index == portraitIndex)
		{
			return LeftPortrait;
		}
		LineageRoomController component = m_endingRoom.GetComponent<LineageRoomController>();
		if (component.LeftPortrait.Index == portraitIndex)
		{
			return component.LeftPortrait;
		}
		LineageRoomController[] connectingRoomControllerArray = m_connectingRoomControllerArray;
		foreach (LineageRoomController lineageRoomController in connectingRoomControllerArray)
		{
			if (lineageRoomController.LeftPortrait.Index == portraitIndex)
			{
				return lineageRoomController.LeftPortrait;
			}
			if (lineageRoomController.RightPortrait.Index == portraitIndex)
			{
				return lineageRoomController.RightPortrait;
			}
			if (lineageRoomController.CentrePortrait.Index == portraitIndex)
			{
				return lineageRoomController.CentrePortrait;
			}
		}
		return null;
	}

	private void OnBiomeCreationComplete(MonoBehaviour sender, EventArgs args)
	{
		if (GameUtility.IsInLevelEditor)
		{
			Sky[] array = UnityEngine.Object.FindObjectsOfType<Sky>(includeInactive: true);
			foreach (Sky sky in array)
			{
				if (sky.name.StartsWith("Lineage"))
				{
					sky.transform.position = new Vector3(m_endingRoom.gameObject.transform.position.x, m_endingRoom.gameObject.transform.position.y, sky.transform.position.z);
					break;
				}
			}
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.LevelEditorWorldCreationComplete, m_onBiomeCreationComplete);
			return;
		}
		foreach (Sky sky2 in WorldBuilder.GetBiomeController(BiomeType.Garden).Skies)
		{
			if (sky2.name.StartsWith("Lineage"))
			{
				sky2.transform.position = new Vector3(m_endingRoom.gameObject.transform.position.x, m_endingRoom.gameObject.transform.position.y, sky2.transform.position.z);
				break;
			}
		}
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.BiomeCreationComplete, m_onBiomeCreationComplete);
	}
}
