using UnityEngine;

public class SkillTreeLogicHelper
{
	public static int GetCharonGoldMilestoneCount()
	{
		int num = 0;
		if (SkillTreeManager.GetSkillObjLevel(SkillTreeType.Charon_Gold_Stat_Bonus) > 0)
		{
			int maxCharonBonusLevel = SkillTree_EV.GetMaxCharonBonusLevel();
			for (int i = 0; i < maxCharonBonusLevel && SaveManager.PlayerSaveData.GoldAcceptedByCharon >= SkillTree_EV.CHARON_GOLD_STAT_BONUS_MILESTONES[i]; i++)
			{
				num++;
			}
		}
		return num;
	}

	public static int GetCharonGoldStatBonus()
	{
		return GetCharonGoldMilestoneCount();
	}

	public static int GetCharonGoldRuneWeightBonus()
	{
		return GetCharonGoldMilestoneCount() * 2;
	}

	public static int GetCharonGoldArmorBonus()
	{
		return GetCharonGoldMilestoneCount() * 2;
	}

	public static int GetVitalityAdds()
	{
		return 0 + (int)SkillTreeManager.GetSkillTreeObj(SkillTreeType.Health_Up).CurrentStatGain + (int)SkillTreeManager.GetSkillTreeObj(SkillTreeType.Health_Up2).CurrentStatGain + (int)SkillTreeManager.GetSkillTreeObj(SkillTreeType.Health_Up3).CurrentStatGain + GetCharonGoldStatBonus();
	}

	public static int GetStrengthAdds()
	{
		return 0 + (int)SkillTreeManager.GetSkillTreeObj(SkillTreeType.Attack_Up).CurrentStatGain + (int)SkillTreeManager.GetSkillTreeObj(SkillTreeType.Attack_Up2).CurrentStatGain + (int)SkillTreeManager.GetSkillTreeObj(SkillTreeType.Attack_Up3).CurrentStatGain + GetCharonGoldStatBonus();
	}

	public static int GetMagicAdds()
	{
		return 0 + (int)SkillTreeManager.GetSkillTreeObj(SkillTreeType.Magic_Attack_Up).CurrentStatGain + (int)SkillTreeManager.GetSkillTreeObj(SkillTreeType.Magic_Attack_Up2).CurrentStatGain + (int)SkillTreeManager.GetSkillTreeObj(SkillTreeType.Magic_Attack_Up3).CurrentStatGain + GetCharonGoldStatBonus();
	}

	public static float GetInvulnTimeExtension()
	{
		return SkillTreeManager.GetSkillTreeObj(SkillTreeType.Invuln_Time_Up).CurrentStatGain;
	}

	public static int GetEquipWeightAdds()
	{
		return 0 + (int)SkillTreeManager.GetSkillTreeObj(SkillTreeType.Equip_Up).CurrentStatGain + (int)SkillTreeManager.GetSkillTreeObj(SkillTreeType.Equip_Up2).CurrentStatGain + (int)SkillTreeManager.GetSkillTreeObj(SkillTreeType.Equip_Up3).CurrentStatGain;
	}

	public static int GetRuneWeightAdds()
	{
		return 0 + (int)SkillTreeManager.GetSkillTreeObj(SkillTreeType.Rune_Equip_Up).CurrentStatGain + (int)SkillTreeManager.GetSkillTreeObj(SkillTreeType.Rune_Equip_Up2).CurrentStatGain + (int)SkillTreeManager.GetSkillTreeObj(SkillTreeType.Rune_Equip_Up3).CurrentStatGain + GetCharonGoldRuneWeightBonus();
	}

	public static int GetArmorAdds()
	{
		return 0 + (int)SkillTreeManager.GetSkillTreeObj(SkillTreeType.Armor_Up).CurrentStatGain + (int)SkillTreeManager.GetSkillTreeObj(SkillTreeType.Armor_Up2).CurrentStatGain + (int)SkillTreeManager.GetSkillTreeObj(SkillTreeType.Armor_Up3).CurrentStatGain + GetCharonGoldArmorBonus();
	}

	public static float GetDexterityAdds()
	{
		return 0f + SkillTreeManager.GetSkillTreeObj(SkillTreeType.Dexterity_Add1).CurrentStatGain + SkillTreeManager.GetSkillTreeObj(SkillTreeType.Dexterity_Add2).CurrentStatGain + SkillTreeManager.GetSkillTreeObj(SkillTreeType.Dexterity_Add3).CurrentStatGain + (float)GetCharonGoldStatBonus();
	}

	public static float GetCritDamageAdds()
	{
		return 0f + SkillTreeManager.GetSkillTreeObj(SkillTreeType.Crit_Damage_Up).CurrentStatGain;
	}

	public static float GetFocusAdds()
	{
		return 0f + SkillTreeManager.GetSkillTreeObj(SkillTreeType.Focus_Up1).CurrentStatGain + SkillTreeManager.GetSkillTreeObj(SkillTreeType.Focus_Up2).CurrentStatGain + SkillTreeManager.GetSkillTreeObj(SkillTreeType.Focus_Up3).CurrentStatGain + (float)GetCharonGoldStatBonus();
	}

	public static float GetMagicCritDamageAdds()
	{
		return 0f + SkillTreeManager.GetSkillTreeObj(SkillTreeType.Magic_Crit_Damage_Up).CurrentStatGain;
	}

	public static float GetAbilityCooldownMods()
	{
		return SkillTreeManager.GetSkillTreeObj(SkillTreeType.Cooldown_Reduction_Up).CurrentStatGain;
	}

	public static float GetPotionMods()
	{
		return SkillTreeManager.GetSkillTreeObj(SkillTreeType.Potion_Up).CurrentStatGain;
	}

	public static float GetArchitectCostMods()
	{
		return SkillTreeManager.GetSkillTreeObj(SkillTreeType.Architect_Cost_Down).CurrentStatGain;
	}

	public static bool IsClassUnlocked(ClassType classType)
	{
		return GetClassLevel(classType) > 0;
	}

	public static int GetClassLevel(ClassType classType)
	{
		SkillTreeType skillTypeFromClass = GetSkillTypeFromClass(classType);
		if (skillTypeFromClass == SkillTreeType.None)
		{
			return 0;
		}
		return SkillTreeManager.GetSkillTreeObj(skillTypeFromClass).Level;
	}

	public static SkillTreeType GetSkillTypeFromClass(ClassType classType)
	{
		return classType switch
		{
			ClassType.SwordClass => SkillTreeType.Sword_Class_Unlock, 
			ClassType.AxeClass => SkillTreeType.Axe_Class_Unlock, 
			ClassType.MagicWandClass => SkillTreeType.Wand_Class_Unlock, 
			ClassType.BowClass => SkillTreeType.Bow_Class_Unlock, 
			ClassType.SpearClass => SkillTreeType.Spear_Class_Unlock, 
			ClassType.SaberClass => SkillTreeType.Saber_Class_Unlock, 
			ClassType.LadleClass => SkillTreeType.Ladle_Class_Unlock, 
			ClassType.DualBladesClass => SkillTreeType.DualBlades_Class_Unlock, 
			ClassType.GunClass => SkillTreeType.Gun_Class_Unlock, 
			ClassType.BoxingGloveClass => SkillTreeType.BoxingGlove_Class_Unlock, 
			ClassType.LanceClass => SkillTreeType.Lancer_Class_Unlock, 
			ClassType.KatanaClass => SkillTreeType.Samurai_Class_Unlock, 
			ClassType.CannonClass => SkillTreeType.Pirate_Class_Unlock, 
			ClassType.LuteClass => SkillTreeType.Music_Class_Unlock, 
			ClassType.AstroClass => SkillTreeType.Astro_Class_Unlock, 
			_ => SkillTreeType.None, 
		};
	}

	public static ClassType GetClassTypeFromSkill(SkillTreeType skillType)
	{
		return skillType switch
		{
			SkillTreeType.Knight_Upgrade => ClassType.SwordClass, 
			SkillTreeType.Axe_Class_Unlock => ClassType.AxeClass, 
			SkillTreeType.Wand_Class_Unlock => ClassType.MagicWandClass, 
			SkillTreeType.Bow_Class_Unlock => ClassType.BowClass, 
			SkillTreeType.Spear_Class_Unlock => ClassType.SpearClass, 
			SkillTreeType.Saber_Class_Unlock => ClassType.SaberClass, 
			SkillTreeType.Ladle_Class_Unlock => ClassType.LadleClass, 
			SkillTreeType.DualBlades_Class_Unlock => ClassType.DualBladesClass, 
			SkillTreeType.Gun_Class_Unlock => ClassType.GunClass, 
			SkillTreeType.BoxingGlove_Class_Unlock => ClassType.BoxingGloveClass, 
			SkillTreeType.Lancer_Class_Unlock => ClassType.LanceClass, 
			SkillTreeType.Samurai_Class_Unlock => ClassType.KatanaClass, 
			SkillTreeType.Pirate_Class_Unlock => ClassType.CannonClass, 
			SkillTreeType.Music_Class_Unlock => ClassType.LuteClass, 
			SkillTreeType.Astro_Class_Unlock => ClassType.AstroClass, 
			_ => ClassType.None, 
		};
	}

	public static DeathDefiedType IsDeathDefied()
	{
		if (SaveManager.PlayerSaveData.GetRelic(RelicType.ExtraLife_Unity).Level > 0)
		{
			SaveManager.PlayerSaveData.GetRelic(RelicType.ExtraLife_UnityUsed).SetLevel(1, additive: true);
			SaveManager.PlayerSaveData.GetRelic(RelicType.ExtraLife_Unity).SetLevel(-1, additive: true);
			return DeathDefiedType.ExtraLife_Unity;
		}
		if (SaveManager.PlayerSaveData.GetRelic(RelicType.ExtraLife).Level > 0)
		{
			SaveManager.PlayerSaveData.GetRelic(RelicType.ExtraLife).SetLevel(-1, additive: true);
			SaveManager.PlayerSaveData.GetRelic(RelicType.ExtraLifeUsed).SetLevel(1, additive: true);
			return DeathDefiedType.ExtraLife;
		}
		float currentStatGain = SkillTreeManager.GetSkillTreeObj(SkillTreeType.Death_Dodge).CurrentStatGain;
		if (Random.Range(0f, 1f) <= currentStatGain)
		{
			return DeathDefiedType.Death_Dodge;
		}
		if (ChallengeManager.IsInChallenge)
		{
			return DeathDefiedType.FailedChallenge;
		}
		return DeathDefiedType.None;
	}

	public static float GetConsecutiveDownstrikeDamageMod()
	{
		return SkillTreeManager.GetSkillTreeObj(SkillTreeType.Down_Strike_Up).CurrentStatGain;
	}

	public static bool IsSmithyUnlocked()
	{
		return SkillTreeManager.GetSkillObjLevel(SkillTreeType.Smithy) > 0;
	}

	public static bool IsEnchantressUnlocked()
	{
		return SkillTreeManager.GetSkillObjLevel(SkillTreeType.Enchantress) > 0;
	}

	public static bool IsArchitectUnlocked()
	{
		return SkillTreeManager.GetSkillObjLevel(SkillTreeType.Architect) > 0;
	}

	public static bool IsLivingSafeUnlocked()
	{
		return SkillTreeManager.GetSkillObjLevel(SkillTreeType.Gold_Saved_Unlock) > 0;
	}

	public static bool IsTotemUnlocked()
	{
		return SkillTreeManager.GetSkillObjLevel(SkillTreeType.Unlock_Totem) > 0;
	}

	public static bool IsDummyUnlocked()
	{
		return SkillTreeManager.GetSkillObjLevel(SkillTreeType.Unlock_Dummy) > 0;
	}

	public static bool IsRandomizeChildrenUnlocked()
	{
		return SkillTreeManager.GetSkillObjLevel(SkillTreeType.Randomize_Children) > 0;
	}

	public static bool IsWeightCDReductionUnlocked()
	{
		return SkillTreeManager.GetSkillObjLevel(SkillTreeType.Weight_CD_Reduce) > 0;
	}

	public static float GetGoldGainMod()
	{
		return SkillTreeManager.GetSkillTreeObj(SkillTreeType.Gold_Gain_Up).CurrentStatGain + SkillTreeManager.GetSkillTreeObj(SkillTreeType.Gold_Gain_Up_2).CurrentStatGain + SkillTreeManager.GetSkillTreeObj(SkillTreeType.Gold_Gain_Up_3).CurrentStatGain + SkillTreeManager.GetSkillTreeObj(SkillTreeType.Gold_Gain_Up_4).CurrentStatGain + SkillTreeManager.GetSkillTreeObj(SkillTreeType.Gold_Gain_Up_5).CurrentStatGain + TraitManager.GetActualTraitGoldGain(SaveManager.PlayerSaveData.CurrentCharacter.TraitOne) + TraitManager.GetActualTraitGoldGain(SaveManager.PlayerSaveData.CurrentCharacter.TraitTwo);
	}

	public static float GetClassXPMod()
	{
		return SkillTreeManager.GetSkillTreeObj(SkillTreeType.XP_Up).CurrentStatGain;
	}

	public static float GetEquipmentOreMod()
	{
		return SkillTreeManager.GetSkillTreeObj(SkillTreeType.Equipment_Ore_Gain_Up).CurrentStatGain;
	}

	public static float GetRuneOreMod()
	{
		return SkillTreeManager.GetSkillTreeObj(SkillTreeType.Rune_Ore_Gain_Up).CurrentStatGain;
	}

	public static float GetBossHPMPRegenMod()
	{
		return SkillTreeManager.GetSkillTreeObj(SkillTreeType.Boss_Health_Restore).CurrentStatGain;
	}

	public static float GetRelicCostMod()
	{
		return SkillTreeManager.GetSkillTreeObj(SkillTreeType.Relic_Cost_Down).CurrentStatGain;
	}

	public static float GetResolveAdds()
	{
		return 0f + SkillTreeManager.GetSkillTreeObj(SkillTreeType.Resolve_Up).CurrentStatGain;
	}

	public static float GetDashDamageMod()
	{
		return SkillTreeManager.GetSkillTreeObj(SkillTreeType.Dash_Strike_Up).CurrentStatGain;
	}

	public static float GetCritChanceAdds()
	{
		return SkillTreeManager.GetSkillTreeObj(SkillTreeType.Crit_Chance_Flat_Up).CurrentStatGain;
	}

	public static float GetMagicCritChanceAdds()
	{
		return SkillTreeManager.GetSkillTreeObj(SkillTreeType.Magic_Crit_Chance_Flat_Up).CurrentStatGain;
	}
}
