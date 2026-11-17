using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ChestSpawnController : MonoBehaviour, ISimpleSpawnController, ISpawnController, ILevelConsumer, ISetSpawnType
{
	[SerializeField]
	private SpecialItemType m_specialItemOverride;

	[SerializeField]
	private bool m_overrideGold;

	[SerializeField]
	private bool m_overrideLevel;

	[SerializeField]
	private bool m_overrideType;

	[SerializeField]
	private BossID m_bossID;

	[SerializeField]
	private ChestType m_chestType;

	[SerializeField]
	private int m_gold;

	[SerializeField]
	private int m_level = -1;

	private SpawnLogicController m_spawnLogic;

	private BaseRoom m_room;

	private GridPointManager m_gridPointManager;

	private bool m_hasCheckedForSpawnLogicController;

	public SpecialItemType SpecialItemOverride
	{
		get
		{
			return m_specialItemOverride;
		}
		set
		{
			m_specialItemOverride = value;
		}
	}

	public BossID BossID
	{
		get
		{
			return m_bossID;
		}
		set
		{
			if (ChestType == ChestType.Boss)
			{
				m_bossID = value;
			}
		}
	}

	public GameObject GameObject => base.gameObject;

	public int Gold
	{
		get
		{
			return m_gold;
		}
		set
		{
			if ((Application.isPlaying && !OverrideGold) || !Application.isPlaying)
			{
				m_gold = value;
			}
		}
	}

	public bool IsInitialised { get; private set; }

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

	public int Level => m_level;

	public bool OverrideGold
	{
		get
		{
			return m_overrideGold;
		}
		set
		{
			m_overrideGold = value;
		}
	}

	public bool OverrideLevel
	{
		get
		{
			return m_overrideLevel;
		}
		set
		{
			m_overrideLevel = value;
		}
	}

	public bool OverrideType
	{
		get
		{
			return m_overrideType;
		}
		set
		{
			m_overrideType = value;
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
			if (!m_hasCheckedForSpawnLogicController)
			{
				m_hasCheckedForSpawnLogicController = true;
				m_spawnLogic = GetComponent<SpawnLogicController>();
			}
			return m_spawnLogic;
		}
	}

	public ChestType ChestType
	{
		get
		{
			return m_chestType;
		}
		set
		{
			if ((Application.isPlaying && !OverrideType) || !Application.isPlaying)
			{
				m_chestType = value;
			}
		}
	}

	public int ChestIndex { get; private set; }

	public ChestObj ChestInstance { get; private set; }

	public void SetLevel(int value)
	{
		if ((Application.isPlaying && !OverrideLevel) || !Application.isPlaying)
		{
			m_level = value;
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

	private void Initialise()
	{
		if (GameUtility.IsInLevelEditor)
		{
			SetSpawnType();
		}
		IsInitialised = true;
	}

	public void SetSpawnType()
	{
		InitializeChestType();
		InitializeGoldAmount();
	}

	public void SetChestIndex(int index)
	{
		ChestIndex = index;
	}

	private void InitializeChestType()
	{
		int roll = RNGManager.GetRandomNumber(RngID.Chest_RoomSeed, $"(ChestSpawnController) InitializeChestType", 0, 100);
		List<KeyValuePair<ChestType, Vector2Int>> list = (from keyValuePair in Economy_EV.GetChestTypeRollRanges()
			where roll >= keyValuePair.Value.x && roll <= keyValuePair.Value.y
			select keyValuePair).ToList();
		if (list.Count == 1)
		{
			ChestType = list.First().Key;
			return;
		}
		Debug.LogFormat("{0}: Failed to find a matching entry in Chest Roll Ranges.", Time.frameCount);
	}

	private void InitializeGoldAmount()
	{
		if (ChestType != ChestType.Boss && ChestType != ChestType.Black)
		{
			try
			{
				Vector2Int vector2Int = Economy_EV.BASE_GOLD_DROP_AMOUNT[m_chestType];
				int randomNumber = RNGManager.GetRandomNumber(RngID.Chest_RoomSeed, $"(ChestSpawnController) InitializeGoldAmount - baseGoldAmount", vector2Int.x, vector2Int.y + 1);
				float randomNumber2 = RNGManager.GetRandomNumber(RngID.Chest_RoomSeed, $"(ChestSpawnController) InitializeGoldAmount - levelAdd", Economy_EV.CHEST_GOLD_DROP_PER_LEVEL_ADD.x, Economy_EV.CHEST_GOLD_DROP_PER_LEVEL_ADD.y);
				randomNumber2 *= (float)Level;
				float num = Economy_EV.CHEST_TYPE_GOLD_MOD[m_chestType];
				int newGamePlusLevel = SaveManager.PlayerSaveData.NewGamePlusLevel;
				Gold = (int)(((float)randomNumber + randomNumber2 - (float)newGamePlusLevel) * num);
				Gold *= Economy_EV.GetItemDropValue(ItemDropType.Coin);
				return;
			}
			catch (KeyNotFoundException)
			{
				Debug.LogFormat("<color=red>|{0}| Key ({1}) is invalid. Check Chest ({2}) and ensure Override Type field has valid value.</color>", this, ChestType, base.gameObject.FullHierarchyPath());
				throw;
			}
		}
		if (BossID != 0)
		{
			BossDrop bossDrop = Economy_EV.BOSS_DROP_TABLE[BossID];
			Gold = bossDrop.GoldAmount;
		}
		else if (ChestType == ChestType.Boss)
		{
			Debug.LogFormat("<color=red>[{0}] Chest is of Type, Boss, but its BossID is set to None. You must specify a Boss ID</color>", this);
		}
	}

	public void SetRoom(BaseRoom room)
	{
		Room = room;
		Room room2 = room as Room;
		if ((bool)room2)
		{
			m_gridPointManager = room2.GridPointManager;
		}
	}

	public bool Spawn()
	{
		bool result = false;
		if (ShouldSpawn && Level != -1)
		{
			Initialise();
			result = true;
		}
		if (base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: false);
		}
		return result;
	}

	public void InitializeChestInstance()
	{
		if (ShouldSpawn)
		{
			ChestInstance = ChestManager.GetChest(ChestType);
			if ((bool)ChestInstance)
			{
				ChestInstance.gameObject.SetActive(value: true);
				ChestInstance.SetChestIndex(ChestIndex);
				ChestInstance.gameObject.transform.SetParent(base.transform.parent);
				ChestInstance.Initialise(ChestType, Level, Gold, BossID);
				ChestInstance.SpecialItemOverride = SpecialItemOverride;
				List<IRoomConsumer> roomConsumerListHelper_STATIC = SimpleSpawnController.m_roomConsumerListHelper_STATIC;
				roomConsumerListHelper_STATIC.Clear();
				ChestInstance.gameObject.GetComponentsInChildren(roomConsumerListHelper_STATIC);
				foreach (IRoomConsumer item in roomConsumerListHelper_STATIC)
				{
					item.SetRoom(Room);
				}
				ChestInstance.gameObject.transform.position = base.transform.position;
			}
			else
			{
				Debug.LogFormat("<color=red>m_prefab Field is null on ({0})</color>", this);
			}
		}
		else
		{
			ChestInstance = null;
		}
	}

	[SpecialName]
	GameObject ISpawnController.get_gameObject()
	{
		return base.gameObject;
	}
}
