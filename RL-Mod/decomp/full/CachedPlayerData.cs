using System;
using System.Reflection;
using UnityEngine;

[Serializable]
public class CachedPlayerData
{
	private int m_currentExhaust;

	private int m_currentSpellAmmo;

	private int m_currentTalentAmmo;

	private int m_currentWeaponAmmo;

	private float m_actualResolve;

	private int m_currentHealth;

	private int m_currentMana;

	private int m_spellOrbs;

	private int m_shields;

	private int m_level;

	private int m_actualVitality;

	private int m_moddedVitality;

	private float m_actualStrength;

	private float m_moddedActualStrength;

	private float m_actualMagic;

	private float m_moddedActualMagic;

	private float m_magicCritChance;

	private float m_moddedMagicCritChance;

	private float m_magicCritDmg;

	private float m_moddedMagicCritDmg;

	private float m_critChance;

	private float m_moddedCritChance;

	private float m_critDamage;

	private float m_moddedCritDamage;

	private float m_dexterity;

	private float m_moddedDexterity;

	private float m_focus;

	private float m_moddedFocus;

	private int m_weight;

	private int m_runeWeight;

	private float m_cooldown;

	private int m_baseArmor;

	private int m_soulsCollected;

	public int CurrentExhaust => m_currentExhaust;

	public int CurrentSpellAmmo => m_currentSpellAmmo;

	public int CurrentTalentAmmo => m_currentTalentAmmo;

	public int CurrentWeaponAmmo => m_currentWeaponAmmo;

	public float ActualResolve => m_actualResolve;

	public int CurrentHealth => m_currentHealth;

	public int CurrentMana => m_currentMana;

	public int SpellOrbs => m_spellOrbs;

	public int CurrentArmor => m_shields;

	public int Level => m_level;

	public int VitalityStat => m_actualVitality;

	public int ModdedVitalityStat => m_moddedVitality;

	public float StrengthStat => m_actualStrength;

	public float ModdedStrengthStat => m_moddedActualStrength;

	public float MagicStat => m_actualMagic;

	public float ModdedMagicStat => m_moddedActualMagic;

	public float MagicCritChance => m_magicCritChance;

	public float ModdedMagicCritChance => m_moddedMagicCritChance;

	public float MagicCritDmg => m_magicCritDmg;

	public float ModdedMagicCritDmg => m_moddedMagicCritDmg;

	public float CritChance => m_critChance;

	public float ModdedCritChance => m_moddedCritChance;

	public float CritDamage => m_critDamage;

	public float ModdedCritDamage => m_moddedCritDamage;

	public float DexterityStat => m_dexterity;

	public float ModdedDexterityStat => m_moddedDexterity;

	public float FocusStat => m_focus;

	public float ModdedFocusStat => m_moddedFocus;

	public int Weight => m_weight;

	public int RuneWeight => m_runeWeight;

	public float Cooldown => m_cooldown;

	public int BaseArmor => m_baseArmor;

	public int SoulsCollected => m_soulsCollected;

	public int LastCachedUpdate_FrameCount { get; private set; }

	public void ForceValue(string fieldName, object value)
	{
		FieldInfo field = GetType().GetField(fieldName, BindingFlags.NonPublic);
		if (field != null)
		{
			field.SetValue(this, value);
		}
		else
		{
			Debug.Log("Failed to force cached player data value: " + fieldName + ". Field not found.");
		}
	}

	public void UpdateData()
	{
		if (PlayerManager.IsInstantiated)
		{
			PlayerController playerController = PlayerManager.GetPlayerController();
			playerController.InitializeAllMods(resetHP: false, resetMP: false);
			if (playerController.CachedHealthOverride != 0f)
			{
				m_currentHealth = (int)playerController.CachedHealthOverride;
			}
			else
			{
				m_currentHealth = playerController.CurrentHealthAsInt;
			}
			if (playerController.CachedManaOverride != 0f)
			{
				m_currentMana = (int)playerController.CachedManaOverride;
			}
			else
			{
				m_currentMana = playerController.CurrentManaAsInt;
			}
			m_currentMana = playerController.CurrentManaAsInt;
			m_shields = playerController.CurrentArmor;
			m_spellOrbs = playerController.SpellOrbs;
			m_currentExhaust = playerController.CurrentExhaust;
			if (SkillTreeManager.IsInitialized)
			{
				m_level = SkillTreeManager.GetTotalSkillObjLevel();
			}
			else
			{
				Debug.Log("<color=red>Failed to update cached player LEVEL DATA ONLY.  SkillTree is disposed or not instantiated yet.</color>");
			}
			m_cooldown = playerController.AbilityCoolDownMod;
			m_weight = playerController.ActualAllowedEquipmentWeight;
			m_runeWeight = playerController.ActualRuneWeight;
			m_baseArmor = playerController.BaseArmor;
			m_actualResolve = playerController.ActualResolve;
			m_actualMagic = (playerController.BaseMagic + (float)playerController.MagicAdd) * (1f + playerController.MagicMod);
			if (ChallengeManager.IsInChallenge)
			{
				m_actualMagic = ChallengeManager.ApplyStatCap(m_actualMagic);
			}
			m_moddedActualMagic = playerController.ActualMagic;
			m_actualStrength = (playerController.BaseStrength + (float)playerController.StrengthAdd) * (1f + playerController.StrengthMod);
			if (ChallengeManager.IsInChallenge)
			{
				m_actualStrength = ChallengeManager.ApplyStatCap(m_actualStrength);
			}
			m_moddedActualStrength = playerController.ActualStrength;
			m_actualVitality = Mathf.CeilToInt((float)(playerController.BaseVitality + playerController.VitalityAdd) * (1f + playerController.VitalityMod));
			if (ChallengeManager.IsInChallenge)
			{
				m_actualVitality = (int)ChallengeManager.ApplyStatCap(m_actualVitality);
			}
			m_moddedVitality = playerController.ActualVitality;
			m_dexterity = (playerController.BaseDexterity + playerController.DexterityAdd) * (1f + playerController.DexterityMod);
			if (ChallengeManager.IsInChallenge)
			{
				m_dexterity = ChallengeManager.ApplyStatCap(m_dexterity, isDexterityOrFocus: true);
			}
			m_moddedDexterity = playerController.ActualDexterity;
			m_critChance = playerController.CritChanceAdd + 0f;
			m_moddedCritChance = playerController.ActualCritChance;
			m_critDamage = playerController.BaseCritDamage + playerController.CritDamageAdd;
			m_moddedCritDamage = playerController.ActualCritDamage;
			m_focus = (playerController.BaseFocus + playerController.FocusAdd) * (1f + playerController.FocusMod);
			if (ChallengeManager.IsInChallenge)
			{
				m_focus = ChallengeManager.ApplyStatCap(m_focus, isDexterityOrFocus: true);
			}
			m_moddedFocus = playerController.ActualFocus;
			m_magicCritChance = playerController.MagicCritChanceAdd + 0f;
			m_moddedMagicCritChance = playerController.ActualMagicCritChance;
			m_magicCritDmg = playerController.BaseMagicCritDamage + playerController.MagicCritDamageAdd;
			m_moddedMagicCritDmg = playerController.ActualMagicCritDamage;
			BaseAbility_RL ability = playerController.CastAbility.GetAbility(CastAbilityType.Weapon);
			m_currentWeaponAmmo = (ability ? ability.CurrentAmmo : 0);
			BaseAbility_RL ability2 = playerController.CastAbility.GetAbility(CastAbilityType.Spell);
			m_currentSpellAmmo = (ability2 ? ability2.CurrentAmmo : 0);
			BaseAbility_RL ability3 = playerController.CastAbility.GetAbility(CastAbilityType.Talent);
			m_currentTalentAmmo = (ability3 ? ability3.CurrentAmmo : 0);
			m_soulsCollected = Souls_EV.GetTotalSoulsCollected(SaveManager.PlayerSaveData.GameModeType, deductSpentSouls: true);
			LastCachedUpdate_FrameCount = Time.frameCount;
		}
		else
		{
			Debug.Log("<color=red>Failed to update cached player data.  PlayerManager is disposed or not instantiated yet.</color>");
		}
	}

	public CachedPlayerData Clone()
	{
		return MemberwiseClone() as CachedPlayerData;
	}
}
