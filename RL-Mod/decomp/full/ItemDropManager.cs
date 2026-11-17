using System;
using System.Collections;
using System.Collections.Generic;
using RL_Windows;
using Sigtrap.Relays;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ItemDropManager : MonoBehaviour
{
	private struct GoldDropData
	{
		public ItemDropType Type;

		public int Amount;

		public GoldDropData(ItemDropType type, int amount)
		{
			Type = type;
			Amount = amount;
		}
	}

	private const string ITEMDROPMANAGER_NAME = "ItemDropManager";

	private const string RESOURCE_PATH = "Prefabs/Managers/ItemDropManager";

	private const float DELAY_BETWEEN_GOLD_DROPS = 0.02f;

	private const float MAX_GOLD_DROP_ANIM_DURATION = 1.5f;

	public Relay<ItemDropType, Vector2, int> DropGoldRelay = new Relay<ItemDropType, Vector2, int>();

	public Relay<ItemDropType, Vector2> DropItemRelay = new Relay<ItemDropType, Vector2>();

	private Dictionary<ItemDropType, GenericPool_RL<BaseItemDrop>> m_itemDropDict;

	private List<ItemDropType> m_goldDropTypes;

	private WaitForSeconds m_coinDropWaitYield = new WaitForSeconds(0.02f);

	private Action<MonoBehaviour, EventArgs> m_onPlayerEnterRoom;

	private static ItemDropManager m_itemDropManager;

	private static ItemDropManager Instance
	{
		get
		{
			if (!m_itemDropManager)
			{
				m_itemDropManager = CDGHelper.FindStaticInstance<ItemDropManager>(createNewIfNull: false);
			}
			return m_itemDropManager;
		}
	}

	public static bool HasActiveItemDrops
	{
		get
		{
			bool flag = TraitManager.IsTraitActive(TraitType.NoMeat);
			foreach (KeyValuePair<ItemDropType, GenericPool_RL<BaseItemDrop>> item in Instance.m_itemDropDict)
			{
				if (((item.Key != ItemDropType.HealthDrop && item.Key != ItemDropType.PizzaDrop) || !flag) && item.Value.HasActiveObjects)
				{
					return true;
				}
			}
			return false;
		}
	}

	public static bool IsInitialized { get; private set; }

	private void Awake()
	{
		m_onPlayerEnterRoom = OnPlayerEnterRoom;
		Initialize();
		SceneManager.sceneLoaded += OnSceneLoaded;
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
	}

	private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
	{
		if (IsInitialized && (bool)m_itemDropManager)
		{
			DisableAllItemDrops();
		}
	}

	private void OnPlayerEnterRoom(object sender, EventArgs args)
	{
		SoulDrop.FakeSoulCounter_STATIC = 0;
		MinimapHUDController.UpdateTextOnly = true;
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.SoulChanged, null, null);
		MinimapHUDController.UpdateTextOnly = false;
		StopAllCoroutines();
	}

	private void OnDestroy()
	{
		IsInitialized = false;
		m_itemDropManager = null;
		SceneManager.sceneLoaded -= OnSceneLoaded;
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
	}

	private void Initialize()
	{
		m_itemDropDict = new Dictionary<ItemDropType, GenericPool_RL<BaseItemDrop>>();
		foreach (ItemDropEntry itemDropEntry in ItemDropLibrary.ItemDropEntryList)
		{
			BaseItemDrop itemDropPrefab = itemDropEntry.ItemDropPrefab;
			if (m_itemDropDict.ContainsKey(itemDropPrefab.ItemDropType))
			{
				throw new Exception("Item Drop Type: " + itemDropPrefab.ItemDropType.ToString() + " already found in Item Drop Library.  Duplicates not allowed.");
			}
			GenericPool_RL<BaseItemDrop> genericPool_RL = new GenericPool_RL<BaseItemDrop>();
			genericPool_RL.Initialize(itemDropPrefab, itemDropEntry.ItemDropPoolSize, disablePooling: false);
			m_itemDropDict.Add(itemDropPrefab.ItemDropType, genericPool_RL);
		}
		m_goldDropTypes = new List<ItemDropType>();
		foreach (ItemDropType value in Enum.GetValues(typeof(ItemDropType)))
		{
			if (Economy_EV.GetItemDropValue(value, getGoldValueOnly: true) > 0)
			{
				m_goldDropTypes.Add(value);
			}
		}
		m_goldDropTypes.Sort(GoldValueSort);
		IsInitialized = true;
	}

	private int GoldValueSort(ItemDropType obj1, ItemDropType obj2)
	{
		if (obj1 == obj2)
		{
			return 0;
		}
		if (Economy_EV.GetItemDropValue(obj1) < Economy_EV.GetItemDropValue(obj2))
		{
			return -1;
		}
		return 1;
	}

	private bool CanDropGold(bool fromChest)
	{
		float architectGoldMod = NPC_EV.GetArchitectGoldMod();
		int level = SaveManager.PlayerSaveData.GetRelic(RelicType.NoGoldXPBonus).Level;
		bool result = !TraitManager.IsTraitActive(TraitType.BonusChestGold) || fromChest;
		if (architectGoldMod > 0f && level <= 0)
		{
			return result;
		}
		return false;
	}

	private void Internal_DropItem(ItemDropType itemDropType, int amount, Vector3 position, bool useLargeSpurt, bool forceMagnetize, bool fromChest, bool dropOneWithFullAmount)
	{
		if (Economy_EV.GetItemDropValue(itemDropType, getGoldValueOnly: true) > 0 && !CanDropGold(fromChest))
		{
			return;
		}
		switch (itemDropType)
		{
		case ItemDropType.HealthDrop:
			if (TraitManager.IsTraitActive(TraitType.MushroomGrow))
			{
				itemDropType = ItemDropType.MushroomDrop;
			}
			else if (Economy_EV.CanDropPizza() && UnityEngine.Random.Range(0f, 1f) < 0.1f)
			{
				itemDropType = ItemDropType.PizzaDrop;
			}
			if (HolidayLookController.IsHoliday(HolidayType.Halloween))
			{
				itemDropType = ItemDropType.CandyDrop;
			}
			else if (HolidayLookController.IsHoliday(HolidayType.Christmas))
			{
				itemDropType = ItemDropType.CookieDrop;
			}
			break;
		case ItemDropType.ManaDrop:
			if (HolidayLookController.IsHoliday(HolidayType.Christmas))
			{
				itemDropType = ItemDropType.MilkManaDrop;
			}
			break;
		}
		DropItemRelay.Dispatch(itemDropType, position);
		GenericPool_RL<BaseItemDrop> value = null;
		if (!m_itemDropDict.TryGetValue(itemDropType, out value))
		{
			throw new Exception("Item Drop: " + itemDropType.ToString() + " cannot be found in ItemDropManager.  Please make sure the prefab is added to the Item Drop Library prefab.");
		}
		int num = -1;
		int num2 = 0;
		int num3;
		if (dropOneWithFullAmount)
		{
			num3 = 1;
			num = amount;
		}
		else
		{
			int itemDropValue = Economy_EV.GetItemDropValue(itemDropType);
			num3 = ((itemDropValue <= 0) ? 1 : (amount / itemDropValue));
			if (num3 > 10)
			{
				num3 = 10;
				num = (int)((float)amount / (float)num3);
			}
			int num4 = ((num != -1) ? num : itemDropValue);
			num2 = amount - num3 * num4;
		}
		for (int i = 0; i < num3; i++)
		{
			BaseItemDrop freeObj = value.GetFreeObj();
			EffectTriggerAnimBehaviour.DISABLE_GLOBALLY = true;
			freeObj.gameObject.SetActive(value: true);
			freeObj.ValueOverride = num;
			if (num2 > 0 && i == num3 - 1)
			{
				freeObj.ValueOverride += num2;
			}
			freeObj.transform.position = new Vector3(position.x, position.y, freeObj.transform.position.z);
			if (freeObj.CorgiController.IsInitialized)
			{
				freeObj.CorgiController.SetRaysParameters();
				freeObj.CorgiController.ResetState();
				freeObj.CorgiController.State.IsCollidingBelow = false;
			}
			float num5 = UnityEngine.Random.Range(Economy_EV.ITEM_DROP_REGULAR_SPURT_MINMAX_X.x, Economy_EV.ITEM_DROP_REGULAR_SPURT_MINMAX_X.y);
			float y = UnityEngine.Random.Range(Economy_EV.ITEM_DROP_REGULAR_SPURT_MINMAX_Y.x, Economy_EV.ITEM_DROP_REGULAR_SPURT_MINMAX_Y.y);
			if (useLargeSpurt)
			{
				num5 = UnityEngine.Random.Range(Economy_EV.ITEM_DROP_LARGE_SPURT_MINMAX_X.x, Economy_EV.ITEM_DROP_LARGE_SPURT_MINMAX_X.y);
				y = UnityEngine.Random.Range(Economy_EV.ITEM_DROP_LARGE_SPURT_MINMAX_Y.x, Economy_EV.ITEM_DROP_LARGE_SPURT_MINMAX_Y.y);
			}
			num5 *= (float)CDGHelper.RandomPlusMinus();
			Vector2 force = new Vector2(num5, y);
			freeObj.CorgiController.SetForce(force);
			if (forceMagnetize)
			{
				freeObj.ForceMagnetizeOnGrounded();
			}
			freeObj.OnSpawnCollectCollisionCheck();
			EffectTriggerAnimBehaviour.DISABLE_GLOBALLY = false;
			_ = freeObj.gameObject.activeSelf;
		}
	}

	private void Internal_DropSpecialItem(ISpecialItemDrop specialItemDrop, bool displayItemDropWindow)
	{
		if (specialItemDrop is IBlueprintDrop blueprintDrop)
		{
			if (EquipmentManager.GetFoundState(blueprintDrop.CategoryType, blueprintDrop.EquipmentType) == FoundState.NotFound)
			{
				EquipmentManager.SetFoundState(blueprintDrop.CategoryType, blueprintDrop.EquipmentType, FoundState.FoundButNotViewed, overrideValues: false);
			}
			else
			{
				EquipmentManager.SetUpgradeBlueprintsFound(blueprintDrop.CategoryType, blueprintDrop.EquipmentType, 1, additive: true);
			}
		}
		if (specialItemDrop is IRuneDrop runeDrop)
		{
			RuneManager.SetUpgradeBlueprintsFound(runeDrop.RuneType, 1, additive: true);
		}
		if (specialItemDrop is IRelicDrop { RelicType: var relicType } relicDrop)
		{
			RelicObj relic = SaveManager.PlayerSaveData.GetRelic(relicType);
			if (relicDrop.RelicModType == RelicModType.DoubleRelic)
			{
				relic.SetLevel(2, additive: true);
			}
			else
			{
				relic.SetLevel(1, additive: true);
			}
		}
		if (specialItemDrop is IAbilityDrop { AbilityType: var abilityType } abilityDrop)
		{
			CharacterData currentCharacter = SaveManager.PlayerSaveData.CurrentCharacter;
			switch (abilityDrop.CastAbilityType)
			{
			case CastAbilityType.Weapon:
				currentCharacter.Weapon = abilityType;
				break;
			case CastAbilityType.Spell:
				currentCharacter.Spell = abilityType;
				SaveManager.PlayerSaveData.SetSpellSeenState(currentCharacter.Spell, seenState: true);
				break;
			case CastAbilityType.Talent:
				currentCharacter.Talent = abilityType;
				if (currentCharacter.ClassType == ClassType.MagicWandClass)
				{
					SaveManager.PlayerSaveData.SetSpellSeenState(currentCharacter.Talent, seenState: true);
				}
				break;
			}
			SaveManager.PlayerSaveData.CurrentCharacter = currentCharacter;
			PlayerController playerController = PlayerManager.GetPlayerController();
			playerController.CharacterClass.SetAbility(abilityDrop.CastAbilityType, abilityType);
			if (playerController.LookController.IsClassLookInitialized)
			{
				playerController.LookController.InitializeEquipmentLook(SaveManager.PlayerSaveData.CurrentCharacter);
			}
		}
		if (specialItemDrop is IChallengeDrop challengeDrop)
		{
			ChallengeManager.SetUpgradeBlueprintsFound(challengeDrop.ChallengeType, 1, additive: true);
		}
		if (displayItemDropWindow)
		{
			if (!WindowManager.GetIsWindowLoaded(WindowID.SpecialItemDrop))
			{
				WindowManager.LoadWindow(WindowID.SpecialItemDrop);
			}
			(WindowManager.GetWindowController(WindowID.SpecialItemDrop) as SpecialItemDropWindowController).AddSpecialItemDrop(specialItemDrop);
			if (!WindowManager.GetIsWindowOpen(WindowID.SpecialItemDrop))
			{
				WindowManager.SetWindowIsOpen(WindowID.SpecialItemDrop, isOpen: true);
			}
		}
	}

	private void Internal_DropGold(int amount, Vector3 position, bool largeSpurt, bool forceMagnetize, bool fromChest)
	{
		if (!CanDropGold(fromChest))
		{
			return;
		}
		int num = amount;
		if (TraitManager.IsTraitActive(TraitType.BonusChestGold))
		{
			float num2 = UnityEngine.Random.Range(Trait_EV.BONUS_CHEST_GOLD_DIE_ROLL.x, Trait_EV.BONUS_CHEST_GOLD_DIE_ROLL.y);
			float num3 = Trait_EV.BONUS_CHEST_GOLD_DIE_MOD * num2;
			num = Mathf.RoundToInt((float)num * num3);
			string text = string.Format(LocalizationManager.GetString("LOC_ID_GOLD_UI_GOLD_PERCENT_POPUP_1", isFemale: false), (int)(num3 * 100f));
			TextPopupManager.DisplayTextDefaultPos(TextPopupType.GoldCollected, text, PlayerManager.GetPlayerController(), attachToSource: true);
		}
		int num4 = num;
		int num5 = 0;
		int num6 = 1;
		List<GoldDropData> list = new List<GoldDropData>();
		if (num <= Economy_EV.GetItemDropValue(ItemDropType.Coin))
		{
			Internal_DropItem(ItemDropType.Coin, Economy_EV.GetItemDropValue(ItemDropType.Coin), position, largeSpurt, forceMagnetize, fromChest, dropOneWithFullAmount: true);
			return;
		}
		for (int num7 = m_goldDropTypes.Count - 1; num7 >= 0; num7--)
		{
			ItemDropType itemDropType = m_goldDropTypes[num7];
			int itemDropValue = Economy_EV.GetItemDropValue(itemDropType);
			int num8 = (int)((float)num4 / (float)itemDropValue);
			int num9 = num8 * itemDropValue;
			num4 -= num9;
			int num10 = -1;
			if (num8 > 10)
			{
				num8 = 10;
				num10 = num9 / num8;
			}
			for (int i = 0; i < num8; i++)
			{
				int amount2 = ((num10 > 0) ? num10 : itemDropValue);
				list.Add(new GoldDropData(itemDropType, amount2));
			}
			num5 += num8;
		}
		float num11 = (float)num5 * 0.02f;
		if (num11 > 1.5f)
		{
			num6 = Mathf.CeilToInt(num11 / 1.5f);
			num11 = 1.5f;
		}
		if (num6 <= 0)
		{
			throw new Exception("ERROR: numDropsPerDelay must be greater than 0. Double check code.");
		}
		StartCoroutine(DropGoldCoroutine(num5, num6, position, list, largeSpurt, forceMagnetize, fromChest));
	}

	private IEnumerator DropGoldCoroutine(int totalItemDrops, int numDropsPerDelay, Vector2 position, List<GoldDropData> allItemsDroppedList, bool largeSpurt, bool forceMagnetize, bool fromChest)
	{
		int dropCounter = 0;
		DispatchDropGoldRelay(allItemsDroppedList, position);
		while (totalItemDrops > 0)
		{
			for (int i = 0; i < numDropsPerDelay; i++)
			{
				ItemDropType itemDropType;
				int amount;
				if (dropCounter < allItemsDroppedList.Count)
				{
					itemDropType = allItemsDroppedList[dropCounter].Type;
					amount = allItemsDroppedList[dropCounter].Amount;
				}
				else
				{
					itemDropType = ItemDropType.Coin;
					amount = Economy_EV.GetItemDropValue(itemDropType);
				}
				Internal_DropItem(itemDropType, amount, position, largeSpurt, forceMagnetize, fromChest, dropOneWithFullAmount: true);
				dropCounter++;
				totalItemDrops--;
				if (totalItemDrops <= 0)
				{
					break;
				}
			}
			yield return m_coinDropWaitYield;
		}
		yield return null;
	}

	private void DispatchDropGoldRelay(List<GoldDropData> allItemsDroppedList, Vector2 position)
	{
		ItemDropType[] types = ItemDropType_RL.Types;
		foreach (ItemDropType itemDropType in types)
		{
			int num = 0;
			for (int j = 0; j < allItemsDroppedList.Count; j++)
			{
				if (allItemsDroppedList[j].Type == itemDropType)
				{
					num++;
				}
			}
			if (num > 0)
			{
				DropGoldRelay.Dispatch(itemDropType, position, num);
			}
		}
	}

	public static bool HasActiveItemDropsOfType(ItemDropType itemDropType)
	{
		foreach (KeyValuePair<ItemDropType, GenericPool_RL<BaseItemDrop>> item in Instance.m_itemDropDict)
		{
			if (item.Key == itemDropType && item.Value.HasActiveObjects)
			{
				return true;
			}
		}
		return false;
	}

	public static void DropGold(int amount, Vector3 position, bool largeSpurt, bool forceMagnetize = false, bool fromChest = false)
	{
		Instance.Internal_DropGold(amount, position, largeSpurt, forceMagnetize, fromChest);
	}

	public static void DropItem(ItemDropType itemDropType, int amount, Vector3 position, bool largeSpurt, bool forceMagnetize = false, bool fromChest = false)
	{
		Instance.Internal_DropItem(itemDropType, amount, position, largeSpurt, forceMagnetize, fromChest, dropOneWithFullAmount: false);
	}

	public static void DropSpecialItem(ISpecialItemDrop specialItemDrop, bool displayItemDropWindow = true)
	{
		Instance.Internal_DropSpecialItem(specialItemDrop, displayItemDropWindow);
	}

	public static void DisableAllItemDrops()
	{
		foreach (KeyValuePair<ItemDropType, GenericPool_RL<BaseItemDrop>> item in Instance.m_itemDropDict)
		{
			item.Value.DisableAll();
		}
	}

	public static void Reset()
	{
		DisableAllItemDrops();
	}

	public static void DestroyPools()
	{
		foreach (KeyValuePair<ItemDropType, GenericPool_RL<BaseItemDrop>> item in Instance.m_itemDropDict)
		{
			item.Value.DestroyPool();
		}
		Instance.m_itemDropDict.Clear();
	}
}
