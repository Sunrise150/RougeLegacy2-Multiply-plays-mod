using System;
using System.Collections.Generic;
using UnityEngine;

public class NPCType_RL
{
	private static NPCType[] m_typeArray;

	public static NPCType[] TypeArray
	{
		get
		{
			if (m_typeArray == null)
			{
				m_typeArray = Enum.GetValues(typeof(NPCType)) as NPCType[];
			}
			return m_typeArray;
		}
	}

	public static bool IsNPCUnlocked(NPCType npcType)
	{
		switch (npcType)
		{
		case NPCType.Blacksmith:
			return SkillTreeLogicHelper.IsSmithyUnlocked();
		case NPCType.Enchantress:
			return SkillTreeLogicHelper.IsEnchantressUnlocked();
		case NPCType.Architect:
			return SkillTreeLogicHelper.IsArchitectUnlocked();
		case NPCType.Charon:
			return true;
		case NPCType.Dummy:
			return SkillTreeLogicHelper.IsDummyUnlocked();
		case NPCType.PizzaGirl:
			return SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.PizzaGirlUnlocked);
		case NPCType.LivingSafe:
			return SkillTreeLogicHelper.IsLivingSafeUnlocked();
		case NPCType.Totem:
			return SkillTreeLogicHelper.IsTotemUnlocked();
		case NPCType.ChallengeHood:
		case NPCType.NewGamePlusHood:
		case NPCType.SoulShopHood:
			return SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.DriftHouseUnlocked);
		default:
			return true;
		}
	}

	public static bool IsNPCDialogueConditionUnlocked(NPCDialogueEntry entry, NPCDialogueCondition condition)
	{
		switch (condition)
		{
		case NPCDialogueCondition.ContentLock:
			return false;
		case NPCDialogueCondition.CastleBossDefeated:
			return SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CastleBoss_Defeated_FirstTime);
		case NPCDialogueCondition.BridgeBossDefeated:
			return SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.BridgeBoss_Defeated_FirstTime);
		case NPCDialogueCondition.ForestBossDefeated:
			return SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.ForestBoss_Defeated_FirstTime);
		case NPCDialogueCondition.StudyBossDefeated:
			return SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.StudyBoss_Defeated_FirstTime);
		case NPCDialogueCondition.TowerBossDefeated:
			return SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.TowerBoss_Defeated_FirstTime);
		case NPCDialogueCondition.CaveBossDefeated:
			return SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveBoss_Defeated_FirstTime);
		case NPCDialogueCondition.FinalBossDefeated:
			return SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.FinalBoss_Defeated_FirstTime);
		case NPCDialogueCondition.BlacksmithUnlocked:
			return IsNPCUnlocked(NPCType.Blacksmith);
		case NPCDialogueCondition.EnchantressUnlocked:
			return IsNPCUnlocked(NPCType.Enchantress);
		case NPCDialogueCondition.TotemUnlocked:
			return IsNPCUnlocked(NPCType.Totem);
		case NPCDialogueCondition.LivingSafeUnlocked:
			return IsNPCUnlocked(NPCType.LivingSafe);
		case NPCDialogueCondition.DummyUnlocked:
			return IsNPCUnlocked(NPCType.Dummy);
		case NPCDialogueCondition.PizzaGirlUnlocked:
			return IsNPCUnlocked(NPCType.PizzaGirl);
		case NPCDialogueCondition.ArchitectUnlocked:
			return IsNPCUnlocked(NPCType.Architect);
		case NPCDialogueCondition.CharonUnlocked:
			return IsNPCUnlocked(NPCType.Charon);
		case NPCDialogueCondition.NewGamePlusHoodUnlocked:
			return IsNPCUnlocked(NPCType.NewGamePlusHood);
		case NPCDialogueCondition.ChallengeHoodUnlocked:
			return IsNPCUnlocked(NPCType.ChallengeHood);
		case NPCDialogueCondition.SoulShopHoodUnlocked:
			return IsNPCUnlocked(NPCType.SoulShopHood);
		case NPCDialogueCondition.LocIDPlayed:
		{
			NPCType nPCType = NPCType.None;
			int num = -1;
			foreach (KeyValuePair<NPCType, NPCDialogueEntry[]> item in NPCDialogue_EV.NPCDialogueTable)
			{
				NPCDialogueEntry[] value = item.Value;
				for (int i = 0; i < value.Length; i++)
				{
					if (value[i].LocID == entry.ConditionValue)
					{
						nPCType = item.Key;
						num = i;
						break;
					}
				}
				if (nPCType != NPCType.None)
				{
					break;
				}
			}
			if (nPCType != NPCType.None)
			{
				return SaveManager.PlayerSaveData.GetNPCDialoguesRead(nPCType) >= num;
			}
			Debug.Log("NPCDialogueCondition.LocIDPlayed - Could not find matching LocID in NPCDialogue_EV.NPCDialogueTable for LocID: " + entry.ConditionValue);
			return false;
		}
		default:
			return true;
		}
	}
}
