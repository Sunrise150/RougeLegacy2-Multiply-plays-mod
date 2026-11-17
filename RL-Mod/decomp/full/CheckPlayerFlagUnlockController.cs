using System;
using UnityEngine;

public class CheckPlayerFlagUnlockController : MonoBehaviour, IRoomConsumer
{
	[SerializeField]
	private PlayerSaveFlag m_playerFlagToCheck;

	[SerializeField]
	private bool m_disableWhenUnlocked;

	[SerializeField]
	[Tooltip("If enabled, will be considered unlocked if Global_EV.UNLOCK_ALL_SHOPS == true.")]
	private bool m_isShop;

	private ISpawnController m_spawner;

	private Action<MonoBehaviour, EventArgs> m_onPlayerEnterRoom;

	public BaseRoom Room { get; private set; }

	private void Awake()
	{
		m_spawner = GetComponent<ISpawnController>();
		m_onPlayerEnterRoom = OnPlayerEnterRoom;
	}

	public void SetRoom(BaseRoom room)
	{
		Room = room;
		Room.PlayerEnterRelay.AddListener(OnPlayerEnterRoom);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterHubTown, m_onPlayerEnterRoom);
	}

	private void OnDestroy()
	{
		if ((bool)Room)
		{
			Room.PlayerEnterRelay.RemoveListener(OnPlayerEnterRoom);
		}
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterHubTown, m_onPlayerEnterRoom);
	}

	private void OnPlayerEnterRoom(object sender, EventArgs args)
	{
		bool flag = SaveManager.PlayerSaveData.GetFlag(m_playerFlagToCheck);
		GameObject gameObject = base.gameObject;
		if (!m_spawner.IsNativeNull())
		{
			bool flag2 = false;
			PropSpawnController propSpawnController = m_spawner as PropSpawnController;
			if ((bool)propSpawnController && (bool)propSpawnController.PropInstance)
			{
				gameObject = propSpawnController.PropInstance.gameObject;
				flag2 = true;
			}
			EnemySpawnController enemySpawnController = m_spawner as EnemySpawnController;
			if (!flag2 && (bool)enemySpawnController && (bool)enemySpawnController.EnemyInstance)
			{
				gameObject = enemySpawnController.EnemyInstance.gameObject;
				flag2 = true;
			}
			IHazardSpawnController hazardSpawnController = m_spawner as IHazardSpawnController;
			if (!flag2 && hazardSpawnController != null && !hazardSpawnController.Hazard.IsNativeNull())
			{
				gameObject = hazardSpawnController.Hazard.gameObject;
				flag2 = true;
			}
			SpecialPlatformSpawnController specialPlatformSpawnController = m_spawner as SpecialPlatformSpawnController;
			if (!flag2 && (bool)specialPlatformSpawnController && (bool)specialPlatformSpawnController.SpecialPlatformInstance)
			{
				gameObject = specialPlatformSpawnController.SpecialPlatformInstance.gameObject;
			}
		}
		if (flag)
		{
			if (!m_disableWhenUnlocked)
			{
				gameObject.SetActive(value: true);
			}
			else
			{
				gameObject.SetActive(value: false);
			}
		}
		else if (!m_disableWhenUnlocked)
		{
			gameObject.SetActive(value: false);
		}
		else
		{
			gameObject.SetActive(value: true);
		}
	}
}
