using FMODUnity;
using GameEventTracking;
using UnityEngine;

namespace RLAudio;

public class ReenactmentControllerAudioEventController : MonoBehaviour, IAudioEventEmitter
{
	[SerializeField]
	[EventRef]
	private string m_smallRoomEnteredEventPath;

	[SerializeField]
	[EventRef]
	private string m_largeRoomEnteredEventPath;

	[SerializeField]
	[EventRef]
	private string m_enemyKilledEventPath;

	[SerializeField]
	[EventRef]
	private string m_chestOpenedEventPath;

	[SerializeField]
	[EventRef]
	private string m_goldGainEventPath;

	[SerializeField]
	[EventRef]
	private string m_discoveryEventPath;

	private string m_description = string.Empty;

	public string Description
	{
		get
		{
			if (m_description == string.Empty)
			{
				m_description = ToString();
			}
			return m_description;
		}
	}

	private void Awake()
	{
		ReenactmentController component = GetComponent<ReenactmentController>();
		component.OnRoomTrackerTriggeredRelay.AddListener(OnRoomEntered);
		component.OnEnemyTrackerTriggeredRelay.AddListener(OnEnemyKilled);
		component.OnChestTrackerTriggeredRelay.AddListener(OnChestOpened);
		component.OnItemTrackerTriggeredRelay.AddListener(OnGoldGain);
	}

	private void OnDestroy()
	{
		ReenactmentController component = GetComponent<ReenactmentController>();
		component.OnRoomTrackerTriggeredRelay.RemoveListener(OnRoomEntered);
		component.OnEnemyTrackerTriggeredRelay.RemoveListener(OnEnemyKilled);
		component.OnChestTrackerTriggeredRelay.RemoveListener(OnChestOpened);
		component.OnItemTrackerTriggeredRelay.RemoveListener(OnGoldGain);
	}

	private void OnGoldGain(ItemTrackerData goldData)
	{
		AudioManager.Play(this, m_goldGainEventPath);
	}

	private void OnChestOpened(ChestTrackerData chestData)
	{
		AudioManager.Play(this, m_chestOpenedEventPath);
		if (!chestData.ContainsGold)
		{
			AudioManager.Play(this, m_discoveryEventPath);
		}
	}

	private void OnEnemyKilled(EnemyTrackerData enemyData)
	{
		AudioManager.Play(this, m_enemyKilledEventPath);
	}

	private void OnRoomEntered(RoomTrackerData roomData)
	{
		foreach (GridPointManager gridPointManager in WorldBuilder.GetBiomeController(roomData.Biome).GridPointManager.GridPointManagers)
		{
			if (gridPointManager.BiomeControllerIndex == roomData.BiomeControllerIndex)
			{
				if (RoomUtility.GetIsRoomLarge(gridPointManager.Size))
				{
					AudioManager.Play(this, m_largeRoomEnteredEventPath);
				}
				else
				{
					AudioManager.Play(this, m_smallRoomEnteredEventPath);
				}
				break;
			}
		}
	}
}
