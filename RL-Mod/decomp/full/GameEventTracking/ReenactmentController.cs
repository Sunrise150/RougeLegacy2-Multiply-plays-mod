using System.Collections;
using System.Collections.Generic;
using Sigtrap.Relays;
using UnityEngine;

namespace GameEventTracking;

public class ReenactmentController : MonoBehaviour
{
	private const float TIME_BETWEEN_PREVIEWS = 0.15f;

	private const float FASTFORWARD_TIME_BETWEEN_PREVIEWS = 0.01f;

	private const int NUM_ROWS_DISPLAYED = 5;

	[SerializeField]
	private RectTransform m_mapArea;

	[SerializeField]
	private Transform m_previewLocation;

	[SerializeField]
	private ChestPreviewController m_chestPreviewPrefab;

	[SerializeField]
	private EnemyPreviewController m_enemyPreviewPrefab;

	private Coroutine m_coroutine;

	private float m_intervalTime;

	private WaitRL_Yield m_waitYield;

	private Vector3[] m_worldCorners = new Vector3[4];

	private bool m_skipWaiting;

	private int m_previewCount;

	private int m_rowConstraintCount;

	private int m_previewIndex;

	private Relay<RoomTrackerData> m_roomTrackerTriggeredRelay = new Relay<RoomTrackerData>();

	private Relay<EnemyTrackerData> m_enemyTrackerTriggeredRelay = new Relay<EnemyTrackerData>();

	private Relay<ChestTrackerData> m_chestTrackerTriggeredRelay = new Relay<ChestTrackerData>();

	private Relay<ItemTrackerData> m_itemTrackerTriggeredRelay = new Relay<ItemTrackerData>();

	public IRelayLink<RoomTrackerData> OnRoomTrackerTriggeredRelay => m_roomTrackerTriggeredRelay.link;

	public IRelayLink<EnemyTrackerData> OnEnemyTrackerTriggeredRelay => m_enemyTrackerTriggeredRelay.link;

	public IRelayLink<ChestTrackerData> OnChestTrackerTriggeredRelay => m_chestTrackerTriggeredRelay.link;

	public IRelayLink<ItemTrackerData> OnItemTrackerTriggeredRelay => m_itemTrackerTriggeredRelay.link;

	public bool IsComplete { get; private set; }

	public bool IsFastforwarding { get; private set; }

	private void Awake()
	{
		m_waitYield = new WaitRL_Yield(0f);
	}

	private void CreateChestPreview(IGameEventData gameEvent)
	{
		ChestType chestType = ((ChestTrackerData)(object)gameEvent).ChestType;
		if ((bool)AssetPreviewManager_V2.GetChestPreviewEntry(chestType))
		{
			Object.Instantiate(AssetPreviewManager_V2.GetChestPreviewEntry(chestType), m_previewLocation);
			m_previewCount++;
			TrimPreviewCount();
		}
		else
		{
			Debug.LogFormat("Failed to find chest preview for {0}", chestType);
		}
	}

	private void CreateEnemyPreview(IGameEventData gameEvent)
	{
		EnemyTrackerData enemyTrackerData = (EnemyTrackerData)(object)gameEvent;
		GameObject enemyPreviewEntry = AssetPreviewManager_V2.GetEnemyPreviewEntry(enemyTrackerData.EnemyType, enemyTrackerData.EnemyRank);
		if ((bool)enemyPreviewEntry)
		{
			Object.Instantiate(enemyPreviewEntry, m_previewLocation);
			m_previewCount++;
			TrimPreviewCount();
		}
		else
		{
			Debug.LogFormat("Failed to find enemy preview for {0}-{1}", enemyTrackerData.EnemyType, enemyTrackerData.EnemyRank);
		}
	}

	private void TrimPreviewCount()
	{
		int num = m_rowConstraintCount * 5;
		if (m_previewCount > num)
		{
			for (int i = m_previewIndex; i < m_previewIndex + m_rowConstraintCount; i++)
			{
				StartCoroutine(TrimPreviewCoroutine(m_previewLocation.GetChild(i).gameObject));
			}
			m_previewIndex += m_rowConstraintCount;
			m_previewCount -= m_rowConstraintCount;
		}
	}

	private IEnumerator TrimPreviewCoroutine(GameObject previewObj)
	{
		if (IsFastforwarding)
		{
			yield return TweenManager.TweenTo_UnscaledTime(previewObj.GetComponent<CanvasGroup>(), 0.1f, Ease.None, "alpha", 0).TweenCoroutine;
		}
		else
		{
			yield return TweenManager.TweenTo_UnscaledTime(previewObj.GetComponent<CanvasGroup>(), 0.5f, Ease.None, "alpha", 0).TweenCoroutine;
		}
		previewObj.SetActive(value: false);
	}

	public void FastForward()
	{
		if (m_coroutine != null)
		{
			if (IsFastforwarding)
			{
				m_skipWaiting = true;
			}
			m_intervalTime = 0.01f;
			IsFastforwarding = true;
		}
	}

	public void Initialise(int constraintCount)
	{
		IsFastforwarding = false;
		m_intervalTime = 0.15f;
		m_skipWaiting = false;
		m_rowConstraintCount = constraintCount;
		if (MapController.IsInitialized)
		{
			MapController.SetAllBiomeVisibility(isAllVisible: false, updateWasVisitedState: false, retainVisitedRoomData: false);
			MapController.DeathMapCamera.gameObject.SetActive(value: true);
			MapController.SetCameraFollowIsOn(isOn: false);
			MapController.DeathMapCamera.orthographicSize = 3f;
			m_mapArea.GetWorldCorners(m_worldCorners);
			Vector3 vector = CameraController.UICamera.WorldToScreenPoint(m_worldCorners[0]);
			Vector3 vector2 = CameraController.UICamera.WorldToScreenPoint(m_worldCorners[2]);
			MapController.DeathMapCamera.pixelRect = new Rect(vector.x, vector.y, vector2.x - vector.x, vector2.y - vector.y);
			if (GameUtility.IsInGame && GameEventTrackerManager.RoomEventTracker.RoomsEntered.Count == 0)
			{
				GameEventTrackerManager.RoomEventTracker.RoomsEntered.Add(new RoomTrackerData(BiomeType.Castle, 0));
			}
			if (GameEventTrackerManager.RoomEventTracker.RoomsEntered.Count > 0)
			{
				RoomTrackerData roomTrackerData = GameEventTrackerManager.RoomEventTracker.RoomsEntered[0];
				MapController.SetPlayerIconInRoom(roomTrackerData.Biome, roomTrackerData.BiomeControllerIndex);
				MapController.SetRoomVisible(roomTrackerData.Biome, roomTrackerData.BiomeControllerIndex, isVisible: true, showReenactment: true);
			}
			else
			{
				MapController.SetPlayerIconInRoom(PlayerManager.GetCurrentPlayerRoom().BiomeType, 0);
			}
			MapController.CentreCameraAroundPlayerIcon();
		}
	}

	public void Reset()
	{
		StopAllCoroutines();
		m_coroutine = null;
		IsComplete = false;
		for (int i = 0; i < m_previewLocation.childCount; i++)
		{
			Object.Destroy(m_previewLocation.GetChild(i).gameObject);
		}
	}

	public void Run()
	{
		if (m_coroutine == null)
		{
			m_coroutine = StartCoroutine(RunCoroutine());
		}
	}

	private IEnumerator RunCoroutine()
	{
		m_previewIndex = 0;
		m_previewCount = 0;
		List<IGameEventData> gameEvents = GameEventTrackerManager.GetGameEvents();
		foreach (IGameEventData item in gameEvents)
		{
			if (item is EnemyTrackerData t)
			{
				MapController.SetIconVisibility(MapIconType.EnemyKilled, t.Biome, t.BiomeControllerIndex, visible: true, t.EnemyIndex);
				m_enemyTrackerTriggeredRelay.Dispatch(t);
				CreateEnemyPreview(item);
				if (!m_skipWaiting)
				{
					m_waitYield.CreateNew(m_intervalTime / 2f, useUnscaledTime: true);
					yield return m_waitYield;
				}
			}
			else if (item is ChestTrackerData t2)
			{
				m_chestTrackerTriggeredRelay.Dispatch(t2);
				MapController.SetIconVisibility(MapIconType.ChestOpened, t2.Biome, t2.BiomeControllerIndex, visible: true, t2.ChestIndex);
				CreateChestPreview(item);
				if (!m_skipWaiting)
				{
					m_waitYield.CreateNew(m_intervalTime, useUnscaledTime: true);
					yield return m_waitYield;
				}
			}
			else if (item is ItemTrackerData)
			{
				m_itemTrackerTriggeredRelay.Dispatch((ItemTrackerData)(object)item);
			}
			else
			{
				if (!(item is RoomTrackerData t3))
				{
					continue;
				}
				MapController.SetRoomVisible(t3.Biome, t3.BiomeControllerIndex, isVisible: true, showReenactment: true);
				MapController.SetPlayerIconInRoom(t3.Biome, t3.BiomeControllerIndex);
				m_roomTrackerTriggeredRelay.Dispatch(t3);
				if (!m_skipWaiting)
				{
					m_waitYield.CreateNew(m_intervalTime, useUnscaledTime: true);
					yield return m_waitYield;
				}
				if (MapController.FollowMode == CameraFollowMode.Constant)
				{
					while (MapController.IsCameraMoving)
					{
						yield return null;
					}
				}
			}
		}
		MapController.SetCameraFollowIsOn(isOn: false);
		m_coroutine = null;
		IsComplete = true;
	}

	public static bool IsTunnelRoom(BaseRoom room)
	{
		Room room2 = room as Room;
		MergeRoom mergeRoom = room as MergeRoom;
		bool result = false;
		if (room2 != null && (bool)room2)
		{
			if (room2.GridPointManager.IsTunnelDestination)
			{
				result = true;
			}
		}
		else if (mergeRoom != null && (bool)mergeRoom)
		{
			GridPointManager[] standaloneGridPointManagers = mergeRoom.StandaloneGridPointManagers;
			for (int i = 0; i < standaloneGridPointManagers.Length; i++)
			{
				if (standaloneGridPointManagers[i].IsTunnelDestination)
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}
}
