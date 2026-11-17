using System;
using System.Collections;
using MoreMountains.CorgiEngine;
using Sigtrap.Relays;
using UnityEngine;

public class Door : MonoBehaviour, IRoomConsumer
{
	private const int LEDGE_LENGTH = 4;

	private const int LEDGE_THICKNESS = 1;

	private Ferr2DT_PathTerrain m_terrain;

	private BaseRoom m_room;

	private Door m_connectedDoor;

	private PolygonCollider2D m_collider;

	private Vector2 m_centerPoint;

	private PlayerTrigger m_exitTrigger;

	private Coroutine m_exitTriggerCoroutine;

	private Vector2 m_roomCoordinates = new Vector2(-10000f, -10000f);

	private bool m_hasExitTriggerBeenCreated;

	private BiomeType m_transitionsToBiome;

	private Relay<object, DoorEventArgs> m_playerEnterRelay = new Relay<object, DoorEventArgs>();

	private Relay<object, DoorEventArgs> m_closeRelay = new Relay<object, DoorEventArgs>();

	private Relay<object, DoorConnectEventArgs> m_doorConnectRelay = new Relay<object, DoorConnectEventArgs>();

	private Relay<object, DoorConnectEventArgs> m_doorDisconnectRelay = new Relay<object, DoorConnectEventArgs>();

	private Relay<object, EventArgs> m_doorDestroyedRelay = new Relay<object, EventArgs>();

	private DoorConnectEventArgs m_doorConnectEventArgs;

	private DoorEventArgs m_doorArgs;

	private Action<object, EventArgs> m_onConnectedDoorDestroyed;

	private Action<object, RoomViaDoorEventArgs> m_onPlayerEnterRoom;

	private Action<object, RoomViaDoorEventArgs> m_onPlayerExitRoom;

	private Action<object, EventArgs> m_onRoomDestroyed;

	[SerializeField]
	private RoomSide m_side = RoomSide.None;

	[SerializeField]
	private int m_index = -1;

	[SerializeField]
	private bool m_disableDoor;

	[SerializeField]
	private bool m_disableLedge;

	public IRelayLink<object, DoorEventArgs> PlayerEnterRelay => m_playerEnterRelay.link;

	public IRelayLink<object, DoorEventArgs> CloseRelay => m_closeRelay.link;

	public IRelayLink<object, DoorConnectEventArgs> DoorConnectRelay => m_doorConnectRelay.link;

	public IRelayLink<object, DoorConnectEventArgs> DoorDisconnectRelay => m_doorDisconnectRelay.link;

	public IRelayLink<object, EventArgs> DoorDestroyedRelay => m_doorDestroyedRelay.link;

	public BaseRoom Room => m_room;

	public Ferr2DT_PathTerrain Ferr2D
	{
		get
		{
			if (m_terrain == null)
			{
				m_terrain = GetComponent<Ferr2DT_PathTerrain>();
				if (m_terrain == null)
				{
					Debug.LogFormat("Unable to find Ferr2DT_PathTerrain Component on ({0})", base.name);
				}
			}
			return m_terrain;
		}
	}

	public Door ConnectedDoor
	{
		get
		{
			return m_connectedDoor;
		}
		private set
		{
			m_connectedDoor = value;
		}
	}

	public BaseRoom ConnectedRoom
	{
		get
		{
			if (ConnectedDoor != null)
			{
				return ConnectedDoor.Room;
			}
			return null;
		}
	}

	public GridPointManager GridPointManager { get; private set; }

	public Vector2Int GridPointCoordinates { get; private set; }

	public Vector2 Coordinates { get; private set; }

	public bool DisabledFromLevelEditor => m_disableDoor;

	public Vector2 CenterPoint
	{
		get
		{
			return m_centerPoint;
		}
		private set
		{
			m_centerPoint = value;
		}
	}

	public int Number
	{
		get
		{
			return m_index;
		}
		set
		{
			m_index = value;
		}
	}

	public RoomSide Side
	{
		get
		{
			return m_side;
		}
		set
		{
			m_side = value;
		}
	}

	public GameObject OneWay { get; private set; }

	public GameObject GameObject
	{
		get
		{
			if (this != null)
			{
				return base.gameObject;
			}
			return null;
		}
	}

	public bool IsBiomeTransitionPoint => TransitionsToBiome != BiomeType.None;

	public BiomeType TransitionsToBiome
	{
		get
		{
			return m_transitionsToBiome;
		}
		private set
		{
			m_transitionsToBiome = value;
		}
	}

	public void SetRoom(BaseRoom value)
	{
		if ((bool)m_room)
		{
			RemoveRoomEventHandlers();
		}
		m_room = value;
		if ((bool)m_room)
		{
			AddRoomEventHandlers();
		}
		if (!(value is Room))
		{
			return;
		}
		Room room = value as Room;
		CenterPoint = RoomUtility.GetDoorCenterPoint(room.Coordinates, room.Size, new DoorLocation(Side, Number));
		Coordinates = RoomUtility.GetDoorCoordinates(value as Room, this);
		GridPointManager = (value as Room).GridPointManager;
		if (GridPointManager != null)
		{
			GridPoint gridPoint = GridPointManager.GetGridPoint(new DoorLocation(Side, Number));
			if (gridPoint != null)
			{
				GridPointCoordinates = gridPoint.GridCoordinates;
			}
		}
	}

	public void SetOneWay(GameObject oneWay)
	{
		OneWay = oneWay;
	}

	public void SetIsBiomeTransitionPoint(BiomeType biome)
	{
		TransitionsToBiome = biome;
	}

	private void Awake()
	{
		m_onPlayerEnterRoom = OnPlayerEnterRoom;
		m_onPlayerExitRoom = OnPlayerExitRoom;
		m_onRoomDestroyed = OnRoomDestroyed;
		m_onConnectedDoorDestroyed = OnConnectedDoorDestroyed;
	}

	private void Start()
	{
		MeshRenderer component = GetComponent<MeshRenderer>();
		if ((bool)component)
		{
			component.enabled = false;
		}
		m_collider = GetComponent<PolygonCollider2D>();
		if ((bool)m_collider)
		{
			if (!m_collider.isTrigger)
			{
				m_collider.isTrigger = true;
			}
			return;
		}
		throw new MissingComponentException("PolygonCollider2D");
	}

	private void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		if (!m_hasExitTriggerBeenCreated)
		{
			m_hasExitTriggerBeenCreated = true;
			CreateExitTrigger();
			CreateLedge();
		}
		if (m_exitTrigger != null)
		{
			m_exitTrigger.SetTriggerActive(isActive: true);
			return;
		}
		Debug.LogFormat("<color=red>{0}: Door ({1}, {2}) in Room ({3}) m_exitTrigger is null.</color>", Time.frameCount, Side, Number, Room.gameObject.name);
	}

	private void OnPlayerExitRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		if (m_exitTrigger != null && m_exitTrigger.IsTriggerActive)
		{
			m_exitTrigger.SetTriggerActive(isActive: false);
		}
		else if (m_exitTrigger == null)
		{
			Debug.LogFormat("<color=red>{0}: Door ({1}, {2}) in Room ({3}) m_exitTrigger is null.</color>", Time.frameCount, Side, Number, Room.gameObject.name);
		}
	}

	private void OnRoomDestroyed(object sender, EventArgs eventArgs)
	{
		SetRoom(null);
	}

	private void CreateExitTrigger()
	{
		Vector2 exitColliderSize = GetExitColliderSize();
		Vector2 exitColliderOffset = GetExitColliderOffset(exitColliderSize);
		if (Side == RoomSide.Left || Side == RoomSide.Right)
		{
			int num = 0;
			foreach (Door door in Room.Doors)
			{
				if (door.Side == Side)
				{
					num++;
				}
			}
			if (num == 1)
			{
				exitColliderSize.y *= 2f;
			}
		}
		m_exitTrigger = PlayerTrigger.CreateInstance(exitColliderSize, CenterPoint + exitColliderOffset, base.transform);
		m_exitTrigger.PlayerEnter += OnPlayerEnterExitTrigger;
		m_exitTrigger.SetTriggerActive(isActive: false);
	}

	private Vector2 GetExitColliderSize()
	{
		return new Vector2(32f, 18f);
	}

	private Vector2 GetExitColliderOffset(Vector2 size)
	{
		Vector2 result = Vector2.zero;
		switch (Side)
		{
		case RoomSide.Top:
			result = new Vector2(0f, 0.5f * size.y);
			break;
		case RoomSide.Bottom:
			result = new Vector2(0f, -0.5f * size.y);
			break;
		case RoomSide.Left:
			result = new Vector2(-0.5f * size.x, 0f);
			break;
		case RoomSide.Right:
			result = new Vector2(0.5f * size.x, 0f);
			break;
		}
		switch (Side)
		{
		case RoomSide.Top:
			if (Room_EV.TRANSITION_TOP_DISTANCE < 0f)
			{
				result += new Vector2(0f, Room_EV.TRANSITION_TOP_DISTANCE);
			}
			break;
		case RoomSide.Bottom:
			if (Room_EV.TRANSITION_BOTTOM_DISTANCE > 0f)
			{
				result += new Vector2(0f, Room_EV.TRANSITION_BOTTOM_DISTANCE);
			}
			break;
		case RoomSide.Left:
			if (Room_EV.TRANSITION_LEFT_RIGHT_DISTANCE < 0f)
			{
				result += new Vector2(0f - Room_EV.TRANSITION_LEFT_RIGHT_DISTANCE, 0f);
			}
			break;
		case RoomSide.Right:
			if (Room_EV.TRANSITION_LEFT_RIGHT_DISTANCE < 0f)
			{
				result += new Vector2(Room_EV.TRANSITION_LEFT_RIGHT_DISTANCE, 0f);
			}
			break;
		}
		return result;
	}

	private void CreateLedge()
	{
		if ((GameUtility.IsInGame && !WorldBuilder.DeactivateRoomGameObjects) || m_disableLedge)
		{
			return;
		}
		if (Side == RoomSide.Left || Side == RoomSide.Right)
		{
			int num = 1;
			if (Side == RoomSide.Right)
			{
				num = -1;
			}
			Vector2 position = CenterPoint - new Vector2((float)num * 0.5f * 4f, 2.5f);
			CreateLedge(new Vector2(4f, 1f), position);
			bool flag = false;
			if (Room.AppearanceBiomeType == BiomeType.Stone)
			{
				if (Room.RoomType == RoomType.Transition && Side == RoomSide.Left)
				{
					flag = true;
				}
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				Vector2 position2 = CenterPoint - new Vector2((float)num * 0.5f * 4f, -3.5f);
				CreateLedge(new Vector2(4f, 1f), position2);
			}
		}
		else if (Room.BiomeType != BiomeType.Tower || Room.AppearanceBiomeType != BiomeType.TowerExterior)
		{
			int num2 = 1;
			if (Side == RoomSide.Bottom)
			{
				num2 = -1;
			}
			Vector2 position3 = CenterPoint + new Vector2(-3.5f, (float)num2 * 0.5f * 4f);
			CreateLedge(new Vector2(1f, 4f), position3);
			Vector2 position4 = CenterPoint + new Vector2(3.5f, (float)num2 * 0.5f * 4f);
			CreateLedge(new Vector2(1f, 4f), position4);
		}
	}

	private void CreateLedge(Vector2 size, Vector2 position)
	{
		GameObject obj = new GameObject("Ledge", typeof(BoxCollider2D));
		obj.layer = LayerMask.NameToLayer("Platform_CollidesWithAll");
		obj.transform.SetParent(base.transform);
		obj.transform.position = position;
		obj.GetComponent<BoxCollider2D>().size = size;
	}

	private void OnPlayerEnterExitTrigger(object sender, EventArgs eventArgs)
	{
		if (m_exitTriggerCoroutine != null)
		{
			Debug.LogFormat("<color=red>{0}: Coroutine was still running, but should not have been.</color>", Time.frameCount);
			StopCoroutine(m_exitTriggerCoroutine);
			m_exitTriggerCoroutine = null;
		}
		m_exitTriggerCoroutine = StartCoroutine(OnPlayerStayExitTrigger());
	}

	private IEnumerator OnPlayerStayExitTrigger()
	{
		while (m_exitTrigger.IsPlayerInTrigger)
		{
			if (GetOnTriggerStayExitRoom())
			{
				RoomSide side = Side;
				_ = PlayerManager.GetPlayer().transform.position;
				_ = CenterPoint;
				if (side == RoomSide.Left || side == RoomSide.Right)
				{
					_ = CenterPoint;
				}
				if (m_doorArgs == null)
				{
					m_doorArgs = new DoorEventArgs(this);
				}
				m_playerEnterRelay.Dispatch(this, m_doorArgs);
				break;
			}
			yield return null;
		}
		m_exitTriggerCoroutine = null;
	}

	private bool GetOnTriggerStayExitRoom()
	{
		GameObject player = PlayerManager.GetPlayer();
		PlayerController playerController = PlayerManager.GetPlayerController();
		bool result = false;
		if (Side != RoomSide.Bottom && (playerController.ConditionState == CharacterStates.CharacterConditions.Stunned || ((playerController.ConditionState == CharacterStates.CharacterConditions.DisableHorizontalMovement || playerController.CharacterMove.MovementDisabled) && !playerController.DisableDoorBlock)))
		{
			Vector3 localPosition = player.transform.localPosition;
			switch (Side)
			{
			case RoomSide.Top:
				if (player.transform.localPosition.y > CenterPoint.y && playerController.Velocity.y > 0f)
				{
					playerController.SetVelocityY(0f, additive: false);
					localPosition.y = CenterPoint.y;
				}
				break;
			case RoomSide.Bottom:
				if (player.transform.localPosition.y < CenterPoint.y && playerController.Velocity.y < 0f)
				{
					playerController.SetVelocityY(0f, additive: false);
					localPosition.y = CenterPoint.y;
				}
				break;
			case RoomSide.Left:
				if (player.transform.localPosition.x < CenterPoint.x && playerController.Velocity.x < 0f)
				{
					playerController.SetVelocityX(0f, additive: false);
					localPosition.x = CenterPoint.x;
				}
				break;
			case RoomSide.Right:
				if (player.transform.localPosition.x > CenterPoint.x && playerController.Velocity.x > 0f)
				{
					playerController.SetVelocityX(0f, additive: false);
					localPosition.x = CenterPoint.x;
				}
				break;
			}
			player.transform.localPosition = localPosition;
		}
		else
		{
			switch (Side)
			{
			case RoomSide.Top:
				if (player.transform.localPosition.y > CenterPoint.y + Room_EV.TRANSITION_TOP_DISTANCE && playerController.Velocity.y > 0f)
				{
					result = true;
				}
				break;
			case RoomSide.Bottom:
				if (player.transform.localPosition.y < CenterPoint.y + Room_EV.TRANSITION_BOTTOM_DISTANCE && playerController.Velocity.y < 0f)
				{
					result = true;
				}
				break;
			case RoomSide.Left:
				if (player.transform.localPosition.x < CenterPoint.x - Room_EV.TRANSITION_LEFT_RIGHT_DISTANCE && playerController.Velocity.x < 0f)
				{
					result = true;
				}
				break;
			case RoomSide.Right:
				if (player.transform.localPosition.x > CenterPoint.x + Room_EV.TRANSITION_LEFT_RIGHT_DISTANCE && playerController.Velocity.x > 0f)
				{
					result = true;
				}
				break;
			}
		}
		return result;
	}

	public void InitialiseInEditor(RoomSide side, int number)
	{
		Side = side;
		Number = number;
	}

	private void OnDestroy()
	{
		m_doorDestroyedRelay.Dispatch(this, EventArgs.Empty);
	}

	public void Close(bool replaceWithWall = true)
	{
		if (replaceWithWall)
		{
			Ferr2DT_PathTerrain ferr2DT_PathTerrain = UnityEngine.Object.Instantiate(RoomPrefabLibrary.DoorSealPrefab);
			Room.TerrainManager.AddPlatform(ferr2DT_PathTerrain);
			ferr2DT_PathTerrain.transform.position = base.transform.position;
			ferr2DT_PathTerrain.transform.SetParent(base.transform.parent);
			ferr2DT_PathTerrain.name = "***Door Seal***";
			ferr2DT_PathTerrain.ClearPoints();
			foreach (Vector2 point in Ferr2D.PathData.GetPoints(0))
			{
				ferr2DT_PathTerrain.AddPoint(point);
			}
			ferr2DT_PathTerrain.Build();
		}
		RemoveRoomEventHandlers();
		if (OneWay != null)
		{
			Room.TerrainManager.RemoveOneWay(OneWay);
			UnityEngine.Object.Destroy(OneWay.gameObject);
		}
		m_closeRelay.Dispatch(this, new DoorEventArgs(this));
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void AddRoomEventHandlers()
	{
		m_room.PlayerEnterRelay.AddListener(m_onPlayerEnterRoom);
		m_room.PlayerExitRelay.AddListener(m_onPlayerExitRoom);
		m_room.RoomDestroyedRelay.AddListener(m_onRoomDestroyed);
	}

	private void RemoveRoomEventHandlers()
	{
		m_room.PlayerEnterRelay.RemoveListener(m_onPlayerEnterRoom);
		m_room.PlayerExitRelay.RemoveListener(m_onPlayerExitRoom);
		m_room.RoomDestroyedRelay.RemoveListener(m_onRoomDestroyed);
	}

	private void OnDrawGizmosSelected()
	{
		if (Application.isPlaying)
		{
			Gizmos.DrawSphere(CenterPoint, 0.5f);
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!collision.CompareTag("Door"))
		{
			return;
		}
		Door component = collision.GetComponent<Door>();
		if (component != null && component.Room != Room && component.Side == RoomUtility.GetOppositeSide(Side) && !component.DisabledFromLevelEditor)
		{
			ConnectedDoor = component;
			if (m_doorConnectEventArgs == null)
			{
				m_doorConnectEventArgs = new DoorConnectEventArgs(this, component);
			}
			else
			{
				m_doorConnectEventArgs.Initialize(this, component);
			}
			m_doorConnectRelay.Dispatch(this, m_doorConnectEventArgs);
			ConnectedDoor.DoorDestroyedRelay.AddListener(m_onConnectedDoorDestroyed);
		}
		else if (component == null)
		{
			Debug.LogFormat("{0}: Unable to find Door Component on ({1})", this, collision.name);
		}
	}

	private void OnConnectedDoorDestroyed(object sender, EventArgs eventArgs)
	{
		if ((bool)ConnectedDoor)
		{
			if (m_doorConnectEventArgs == null)
			{
				m_doorConnectEventArgs = new DoorConnectEventArgs(this, ConnectedDoor);
			}
			else
			{
				m_doorConnectEventArgs.Initialize(this, ConnectedDoor);
			}
			m_doorDisconnectRelay.Dispatch(this, m_doorConnectEventArgs);
			ConnectedDoor.DoorDestroyedRelay.RemoveListener(m_onConnectedDoorDestroyed);
			ConnectedDoor = null;
		}
	}
}
