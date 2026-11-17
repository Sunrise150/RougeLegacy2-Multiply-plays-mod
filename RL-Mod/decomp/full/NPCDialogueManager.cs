using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NPCDialogueManager : MonoBehaviour
{
	private static NPCDialogueManager m_instance;

	private static NPCDialogueManager Instance
	{
		get
		{
			return m_instance;
		}
		set
		{
			m_instance = value;
		}
	}

	public static bool IsInitialized { get; private set; }

	private void Awake()
	{
		Instance = this;
		IsInitialized = true;
	}

	private void OnDestroy()
	{
		IsInitialized = false;
	}

	private void OnLevelEditorWorldCreationComplete(object sender, EventArgs args)
	{
		PopulateNPCDialogues_Internal(OnPlayManager.CurrentBiome == BiomeType.HubTown);
	}

	private void GetAvailableNPCDialogues(List<NPCType> availableDialogues, List<NPCType> priorityDialogues)
	{
		availableDialogues.Clear();
		priorityDialogues.Clear();
		NPCType[] typeArray = NPCType_RL.TypeArray;
		foreach (NPCType nPCType in typeArray)
		{
			if (nPCType == NPCType.None || !NPCType_RL.IsNPCUnlocked(nPCType) || !NPCDialogue_EV.NPCDialogueTable.TryGetValue(nPCType, out var value))
			{
				continue;
			}
			int highestUnlockedNPCDialogue = GetHighestUnlockedNPCDialogue(nPCType);
			if (highestUnlockedNPCDialogue == -1)
			{
				continue;
			}
			int nPCDialoguesRead = SaveManager.PlayerSaveData.GetNPCDialoguesRead(nPCType);
			if (nPCDialoguesRead >= highestUnlockedNPCDialogue)
			{
				continue;
			}
			bool flag = false;
			if (nPCDialoguesRead + 1 < value.Length)
			{
				value[nPCDialoguesRead + 1].UnlockConditions.Contains(NPCDialogueCondition.LocIDPlayed);
			}
			if (SaveManager.PlayerSaveData.GetNPCDialogueCooldown(nPCType) <= 0)
			{
				if (flag)
				{
					priorityDialogues.Add(nPCType);
				}
				availableDialogues.Add(nPCType);
			}
		}
	}

	private void PopulateNPCDialogues_Internal(bool inHubtown)
	{
		SaveManager.PlayerSaveData.PopulatedNPCDialoguesList.Clear();
		if (!inHubtown)
		{
			if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.DragonDialogue_AfterDefeatingTubal))
			{
				int highestUnlockedNPCDialogue = GetHighestUnlockedNPCDialogue(NPCType.Dragon);
				if (SaveManager.PlayerSaveData.GetNPCDialoguesRead(NPCType.Dragon) < highestUnlockedNPCDialogue)
				{
					SaveManager.PlayerSaveData.PopulatedNPCDialoguesList.Add(NPCType.Dragon);
				}
			}
			return;
		}
		List<NPCType> list = new List<NPCType>();
		List<NPCType> list2 = new List<NPCType>();
		GetAvailableNPCDialogues(list2, list);
		int num = 99;
		if (inHubtown)
		{
			num = UnityEngine.Random.Range(NPCDialogue_EV.MAX_DIALOGUE_HUB_TOWN.x, NPCDialogue_EV.MAX_DIALOGUE_HUB_TOWN.y + 1);
		}
		if (list2.Count <= num)
		{
			SaveManager.PlayerSaveData.PopulatedNPCDialoguesList.AddRange(list2);
			num = 0;
			return;
		}
		list2.Sort(DialogueSort);
		list.Sort(DialogueSort);
		int num2 = 0;
		int num3 = 2;
		while (num > 0)
		{
			bool num4 = list.Count > 0;
			List<NPCType> list3 = list2;
			if (num4)
			{
				list3 = list;
			}
			int index = 0;
			if (num2 >= num3)
			{
				index = UnityEngine.Random.Range(0, list3.Count);
			}
			num2++;
			NPCType item = list3[index];
			SaveManager.PlayerSaveData.PopulatedNPCDialoguesList.Add(item);
			list3.RemoveAt(index);
			if (num4)
			{
				list2.Remove(item);
			}
			num--;
		}
	}

	private int DialogueSort(NPCType a, NPCType b)
	{
		int num = NPCDialogue_EV.NPCDialogueOrderList.IndexOf(a);
		int num2 = NPCDialogue_EV.NPCDialogueOrderList.IndexOf(b);
		if (num > num2)
		{
			return 1;
		}
		if (num < num2)
		{
			return -1;
		}
		return 0;
	}

	private bool IsPriorityDialogue_Internal(NPCType npcType)
	{
		if (NPCDialogue_EV.NPCDialogueTable.TryGetValue(npcType, out var value))
		{
			int highestUnlockedNPCDialogue = GetHighestUnlockedNPCDialogue(npcType);
			int nPCDialoguesRead = SaveManager.PlayerSaveData.GetNPCDialoguesRead(npcType);
			if (nPCDialoguesRead < highestUnlockedNPCDialogue && value[nPCDialoguesRead + 1].UnlockConditions.Contains(NPCDialogueCondition.LocIDPlayed))
			{
				return true;
			}
		}
		return false;
	}

	public static void PopulateNPCDialogues(bool inHubtown)
	{
		Instance.PopulateNPCDialogues_Internal(inHubtown);
	}

	public static void InitializeGlobalDialogueCD()
	{
		if (SaveManager.PlayerSaveData.TriggerGlobalNPCDialogueCD)
		{
			SaveManager.PlayerSaveData.TriggerGlobalNPCDialogueCD = false;
			SaveManager.PlayerSaveData.GlobalNPCDialogueCD = 0;
		}
	}

	public static int GetHighestUnlockedNPCDialogue(NPCType npcType)
	{
		int result = -1;
		if (NPCDialogue_EV.NPCDialogueTable.TryGetValue(npcType, out var value))
		{
			for (int i = 0; i < value.Length && value[i].IsUnlocked(); i++)
			{
				result = i;
			}
		}
		return result;
	}

	public static bool AllNPCDialoguesRead(NPCType npcType)
	{
		int num = 0;
		if (NPCDialogue_EV.NPCDialogueTable.ContainsKey(npcType))
		{
			num = NPCDialogue_EV.NPCDialogueTable[npcType].Length;
		}
		int nPCDialoguesRead = SaveManager.PlayerSaveData.GetNPCDialoguesRead(npcType);
		if (num > 0 && nPCDialoguesRead >= num - 1)
		{
			return true;
		}
		return false;
	}

	public static void ReduceAllNPCDialogueCooldowns(int reductionAmount)
	{
		NPCType[] typeArray = NPCType_RL.TypeArray;
		foreach (NPCType nPCType in typeArray)
		{
			if (nPCType != NPCType.None)
			{
				SaveManager.PlayerSaveData.SetNPCDialogueCooldown(nPCType, reductionAmount, additive: true);
			}
		}
	}

	public static bool CanSpeak(NPCType npcType)
	{
		if (SaveManager.PlayerSaveData.EndingSpawnRoom >= EndingSpawnRoomType.Manor)
		{
			return false;
		}
		bool flag = false;
		if (SaveManager.PlayerSaveData.GlobalNPCDialogueCD > 0 && !flag)
		{
			return false;
		}
		return SaveManager.PlayerSaveData.PopulatedNPCDialoguesList.Contains(npcType);
	}

	public static void MarkNPCAsSpoken(NPCType npcType, bool isNPCDialogue, NPCController npcController)
	{
		if (!CanSpeak(npcType))
		{
			return;
		}
		int nPCDialogueCooldown = NPCDialogue_EV.GetNPCDialogueCooldown(npcType);
		SaveManager.PlayerSaveData.SetNPCDialogueCooldown(npcType, nPCDialogueCooldown, additive: false);
		if (isNPCDialogue)
		{
			SaveManager.PlayerSaveData.SetNPCDialoguesRead(npcType, 1, additive: true);
			int nPCDialoguesRead = SaveManager.PlayerSaveData.GetNPCDialoguesRead(npcType);
			int num = 0;
			if (NPCDialogue_EV.NPCDialogueTable.ContainsKey(npcType))
			{
				num = NPCDialogue_EV.NPCDialogueTable[npcType].Length;
			}
			if (num > 0 && nPCDialoguesRead >= num - 1)
			{
				SoulDrop.FakeSoulCounter_STATIC = 500;
				Vector3 position = npcController.transform.position;
				position.y += 1f;
				ItemDropManager.DropItem(ItemDropType.Soul, 500, position, largeSpurt: true, forceMagnetize: true);
				switch (npcType)
				{
				case NPCType.Blacksmith:
					StoreAPIManager.GiveAchievement(AchievementType.StoryNPCBlacksmith, StoreType.All);
					break;
				case NPCType.Enchantress:
					StoreAPIManager.GiveAchievement(AchievementType.StoryNPCEnchantress, StoreType.All);
					break;
				case NPCType.Architect:
					StoreAPIManager.GiveAchievement(AchievementType.StoryNPCArchitect, StoreType.All);
					break;
				case NPCType.PizzaGirl:
					StoreAPIManager.GiveAchievement(AchievementType.StoryNPCPizzaGirl, StoreType.All);
					break;
				case NPCType.Dummy:
					StoreAPIManager.GiveAchievement(AchievementType.StoryNPCQuinn, StoreType.All);
					break;
				case NPCType.Totem:
					StoreAPIManager.GiveAchievement(AchievementType.StoryNPCMasteryTotem, StoreType.All);
					break;
				case NPCType.LivingSafe:
					StoreAPIManager.GiveAchievement(AchievementType.StoryNPCLivingSafe, StoreType.All);
					break;
				case NPCType.Dragon:
					StoreAPIManager.GiveAchievement(AchievementType.StoryNPCDragon, StoreType.All);
					break;
				}
			}
		}
		SaveManager.PlayerSaveData.PopulatedNPCDialoguesList.Remove(npcType);
	}

	public static int GetTotalCompleteDialogues()
	{
		int num = 0;
		NPCType[] typeArray = NPCType_RL.TypeArray;
		foreach (NPCType nPCType in typeArray)
		{
			if (nPCType != NPCType.None && AllNPCDialoguesRead(nPCType))
			{
				num++;
			}
		}
		return num;
	}

	public static bool IsPriorityDialogue(NPCType npcType)
	{
		return Instance.IsPriorityDialogue_Internal(npcType);
	}

	public static bool SageTotemNewNameRevealed()
	{
		int num = -1;
		int num2 = 0;
		NPCDialogueEntry[] array = NPCDialogue_EV.NPCDialogueTable[NPCType.Totem];
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].LocID == "LOC_ID_HUB_TOWN_DIALOGUE_TOTEM_MY_NEW_NAME_1 ")
			{
				num = num2;
				break;
			}
			num2++;
		}
		if (num > -1)
		{
			return SaveManager.PlayerSaveData.GetNPCDialoguesRead(NPCType.Totem) >= num;
		}
		return false;
	}
}
