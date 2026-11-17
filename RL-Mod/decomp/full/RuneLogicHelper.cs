using UnityEngine;

public static class RuneLogicHelper
{
	public static int GetExtraJumps()
	{
		return RuneManager.GetRuneEquippedLevel(RuneType.DoubleJump);
	}

	public static int GetExtraDashes()
	{
		return RuneManager.GetRuneEquippedLevel(RuneType.Dash);
	}

	public static float GetLifeStealPercent()
	{
		RuneData runeData = RuneLibrary.GetRuneData(RuneType.Lifesteal);
		int numLifeStealRunes = GetNumLifeStealRunes();
		if (numLifeStealRunes <= 0)
		{
			return 0f;
		}
		return runeData.StatMod01 + (float)(numLifeStealRunes - 1) * runeData.ScalingStatMod01;
	}

	public static int GetNumLifeStealRunes()
	{
		int runeEquippedLevel = RuneManager.GetRuneEquippedLevel(RuneType.Lifesteal);
		int num = (int)EquipmentManager.Get_EquipmentSet_BonusTypeStatGain(EquipmentSetBonusType.LifeSteal);
		return runeEquippedLevel + num;
	}

	public static float GetSoulStealPercent()
	{
		RuneData runeData = RuneLibrary.GetRuneData(RuneType.SoulSteal);
		int numSoulStealRunes = GetNumSoulStealRunes();
		if (numSoulStealRunes <= 0)
		{
			return 0f;
		}
		return runeData.StatMod01 + (float)(numSoulStealRunes - 1) * runeData.ScalingStatMod01;
	}

	public static int GetNumSoulStealRunes()
	{
		int runeEquippedLevel = RuneManager.GetRuneEquippedLevel(RuneType.SoulSteal);
		int num = (int)EquipmentManager.Get_EquipmentSet_BonusTypeStatGain(EquipmentSetBonusType.SoulSteal);
		return runeEquippedLevel + num;
	}

	public static float GetGoldGainPercent()
	{
		return RuneManager.GetRune(RuneType.GoldGain).CurrentStatModTotal_1;
	}

	public static float GetManaRegenPercent()
	{
		return RuneManager.GetRune(RuneType.ManaRegen).CurrentStatModTotal_1;
	}

	public static float GetDamageReturnPercent(int numRunes)
	{
		if (numRunes <= 0)
		{
			return 0f;
		}
		RuneData runeData = RuneLibrary.GetRuneData(RuneType.ReturnDamage);
		return runeData.StatMod01 + (float)(numRunes - 1) * runeData.ScalingStatMod01;
	}

	public static int GetMaxManaFlat()
	{
		return Mathf.RoundToInt(RuneManager.GetRune(RuneType.MaxMana).CurrentStatModTotal_1);
	}

	public static float GetHastePercent()
	{
		return RuneManager.GetRune(RuneType.Haste).CurrentStatModTotal_1;
	}

	public static float GetOreGainPercent()
	{
		return RuneManager.GetRune(RuneType.OreGain).CurrentStatModTotal_1;
	}

	public static float GetRuneOreGainPercent()
	{
		return RuneManager.GetRune(RuneType.RuneOreGain).CurrentStatModTotal_1;
	}

	public static float GetMagnetDistance()
	{
		return RuneManager.GetRune(RuneType.Magnet).CurrentStatModTotal_1;
	}

	public static int GetManaRegenOnSpinKick()
	{
		return (int)RuneManager.GetRune(RuneType.ManaOnSpinKick).CurrentStatModTotal_1;
	}

	public static float GetStatusEffectDurationMod()
	{
		return RuneManager.GetRune(RuneType.StatusEffectDuration).CurrentStatModTotal_1;
	}

	public static float GetArmorRegenMod()
	{
		return RuneManager.GetRune(RuneType.ArmorRegen).CurrentStatModTotal_1;
	}

	public static float GetArmorMinBlockAdd()
	{
		return RuneManager.GetRune(RuneType.ArmorMinBlock).CurrentStatModTotal_1;
	}

	public static float GetResolveAdd()
	{
		return RuneManager.GetRune(RuneType.ResolveGain).CurrentStatModTotal_1;
	}

	public static float GetWeaponCritChanceAdd()
	{
		return RuneManager.GetRune(RuneType.WeaponCritChanceAdd).CurrentStatModTotal_1;
	}

	public static float GetMagicCritChanceAdd()
	{
		return RuneManager.GetRune(RuneType.MagicCritChanceAdd).CurrentStatModTotal_1;
	}

	public static float GetWeaponCritDamageAdd()
	{
		return RuneManager.GetRune(RuneType.WeaponCritDamageAdd).CurrentStatModTotal_1;
	}

	public static float GetMagicCritDamageAdd()
	{
		return RuneManager.GetRune(RuneType.MagicCritDamageAdd).CurrentStatModTotal_1;
	}

	public static float GetSuperCritChanceAdd()
	{
		return RuneManager.GetRune(RuneType.SuperCritChanceAdd).CurrentStatModTotal_1;
	}

	public static float GetSuperCritDamageAdd()
	{
		return RuneManager.GetRune(RuneType.SuperCritDamageAdd).CurrentStatModTotal_1;
	}

	public static float GetArmorHealthMod()
	{
		return RuneManager.GetRune(RuneType.ArmorHealth).CurrentStatModTotal_1;
	}
}
