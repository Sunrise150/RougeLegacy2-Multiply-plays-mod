using System;
using TMPro;
using UnityEngine;

public class PlayerCardDescriptionUpdater : MonoBehaviour, ILocalizable
{
	private enum PlayerCardDescriptionType
	{
		None,
		Class,
		ProficiencyLvl,
		Health,
		Mana,
		Strength,
		Magic,
		MagicCritChance,
		MagicCritDamage,
		WeaponCritChance,
		WeaponCritDamage,
		Weight,
		Cooldown,
		RuneWeight,
		Armor,
		PlayerName,
		Gold,
		EquipmentOre,
		Level,
		XP,
		Vitality,
		Dexterity,
		Focus,
		RuneOre,
		Soul,
		Resolve
	}

	[SerializeField]
	private PlayerCardDescriptionType m_descriptionType;

	[SerializeField]
	private TMP_Text m_text;

	private PlayerCardOpenedEventArgs m_assignedArgs;

	private Action<MonoBehaviour, EventArgs> m_updateStat;

	private Action<MonoBehaviour, EventArgs> m_refreshText;

	private void Awake()
	{
		m_updateStat = UpdateStat;
		m_refreshText = RefreshText;
	}

	private void OnEnable()
	{
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.PlayerCardWindow_Opened, m_updateStat);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.LanguageChanged, m_refreshText);
	}

	private void OnDisable()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.PlayerCardWindow_Opened, m_updateStat);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.LanguageChanged, m_refreshText);
	}

	private void UpdateStat(MonoBehaviour sender, EventArgs args)
	{
		string text = "";
		PlayerSaveData playerData = (m_assignedArgs = args as PlayerCardOpenedEventArgs).PlayerData;
		CharacterData currentCharacter = playerData.CurrentCharacter;
		switch (m_descriptionType)
		{
		case PlayerCardDescriptionType.Class:
			text = ClassLibrary.GetClassData(currentCharacter.ClassType).PassiveData.Title;
			text = LocalizationManager.GetString(text, currentCharacter.IsFemale);
			break;
		case PlayerCardDescriptionType.ProficiencyLvl:
			text = "0";
			break;
		case PlayerCardDescriptionType.Health:
			text = playerData.CachedData.CurrentHealth.ToString();
			break;
		case PlayerCardDescriptionType.Mana:
			text = playerData.CachedData.CurrentMana.ToString();
			break;
		case PlayerCardDescriptionType.Strength:
		{
			float value9 = ((playerData.CachedData.StrengthStat > 0f) ? ((playerData.CachedData.ModdedStrengthStat - playerData.CachedData.StrengthStat) / playerData.CachedData.StrengthStat) : 0f);
			if (playerData.CachedData.ModdedStrengthStat == 0f)
			{
				value9 = 0f;
			}
			text = GetValueString(playerData.CachedData.StrengthStat, isPercent: false) + " " + GetModString(value9);
			break;
		}
		case PlayerCardDescriptionType.Magic:
		{
			float value5 = ((playerData.CachedData.MagicStat > 0f) ? ((playerData.CachedData.ModdedMagicStat - playerData.CachedData.MagicStat) / playerData.CachedData.MagicStat) : 0f);
			if (playerData.CachedData.ModdedMagicStat == 0f)
			{
				value5 = 0f;
			}
			text = GetValueString(playerData.CachedData.MagicStat, isPercent: false) + " " + GetModString(value5);
			break;
		}
		case PlayerCardDescriptionType.MagicCritChance:
		{
			float magicCritChance = playerData.CachedData.MagicCritChance;
			float moddedMagicCritChance = playerData.CachedData.ModdedMagicCritChance;
			float value8 = moddedMagicCritChance - magicCritChance;
			if (moddedMagicCritChance == 0f)
			{
				value8 = 0f;
			}
			text = GetValueString(magicCritChance, isPercent: true) + " " + GetModString(value8);
			break;
		}
		case PlayerCardDescriptionType.MagicCritDamage:
		{
			float magicCritDmg = playerData.CachedData.MagicCritDmg;
			float moddedMagicCritDmg = playerData.CachedData.ModdedMagicCritDmg;
			float value6 = moddedMagicCritDmg - magicCritDmg;
			if (moddedMagicCritDmg == 0f)
			{
				value6 = 0f;
			}
			text = GetValueString(magicCritDmg, isPercent: true) + " " + GetModString(value6);
			break;
		}
		case PlayerCardDescriptionType.WeaponCritChance:
		{
			float critChance = playerData.CachedData.CritChance;
			float moddedCritChance = playerData.CachedData.ModdedCritChance;
			float value4 = moddedCritChance - critChance;
			if (moddedCritChance == 0f)
			{
				value4 = 0f;
			}
			text = GetValueString(critChance, isPercent: true) + " " + GetModString(value4);
			break;
		}
		case PlayerCardDescriptionType.WeaponCritDamage:
		{
			float critDamage = playerData.CachedData.CritDamage;
			float moddedCritDamage = playerData.CachedData.ModdedCritDamage;
			float value = moddedCritDamage - critDamage;
			if (moddedCritDamage == 0f)
			{
				value = 0f;
			}
			text = GetValueString(critDamage, isPercent: true) + " " + GetModString(value);
			break;
		}
		case PlayerCardDescriptionType.Vitality:
		{
			float value2 = ((playerData.CachedData.VitalityStat > 0) ? (((float)playerData.CachedData.ModdedVitalityStat - (float)playerData.CachedData.VitalityStat) / (float)playerData.CachedData.VitalityStat) : 0f);
			if (playerData.CachedData.ModdedVitalityStat == 0)
			{
				value2 = 0f;
			}
			if (ChallengeManager.IsInitialized && ChallengeManager.IsInChallenge)
			{
				value2 = ChallengeManager.GetActiveHandicapMod();
			}
			text = GetValueString(playerData.CachedData.VitalityStat, isPercent: false) + " " + GetModString(value2);
			break;
		}
		case PlayerCardDescriptionType.Dexterity:
		{
			float value7 = ((playerData.CachedData.DexterityStat > 0f) ? ((playerData.CachedData.ModdedDexterityStat - playerData.CachedData.DexterityStat) / playerData.CachedData.DexterityStat) : 0f);
			if (playerData.CachedData.ModdedDexterityStat == 0f)
			{
				value7 = 0f;
			}
			text = GetValueString(playerData.CachedData.DexterityStat, isPercent: false) + " " + GetModString(value7);
			break;
		}
		case PlayerCardDescriptionType.Focus:
		{
			float value3 = ((playerData.CachedData.FocusStat > 0f) ? ((playerData.CachedData.ModdedFocusStat - playerData.CachedData.FocusStat) / playerData.CachedData.FocusStat) : 0f);
			if (playerData.CachedData.ModdedFocusStat == 0f)
			{
				value3 = 0f;
			}
			text = GetValueString(playerData.CachedData.FocusStat, isPercent: false) + " " + GetModString(value3);
			break;
		}
		case PlayerCardDescriptionType.Weight:
			text = playerData.CachedData.Weight.ToString();
			break;
		case PlayerCardDescriptionType.Cooldown:
			text = GetValueString(playerData.CachedData.Cooldown, isPercent: true);
			break;
		case PlayerCardDescriptionType.RuneWeight:
			text = playerData.CachedData.RuneWeight.ToString();
			break;
		case PlayerCardDescriptionType.Armor:
			text = playerData.CachedData.CurrentArmor.ToString();
			break;
		case PlayerCardDescriptionType.PlayerName:
			text = LocalizationManager.GetLocalizedPlayerName(currentCharacter);
			break;
		case PlayerCardDescriptionType.Gold:
			text = playerData.GetActualAvailableGoldString();
			if (text.Length > 7)
			{
				text = string.Format(LocalizationManager.GetString("LOC_ID_HERO_CARD_THOUSANDS_TITLE_1", isFemale: false), text.Substring(0, 5));
			}
			break;
		case PlayerCardDescriptionType.EquipmentOre:
			text = playerData.EquipmentOreCollected.ToString();
			if (text.Length > 7)
			{
				text = string.Format(LocalizationManager.GetString("LOC_ID_HERO_CARD_THOUSANDS_TITLE_1", isFemale: false), text.Substring(0, 5));
			}
			break;
		case PlayerCardDescriptionType.RuneOre:
			text = playerData.RuneOreCollected.ToString();
			if (text.Length > 7)
			{
				text = string.Format(LocalizationManager.GetString("LOC_ID_HERO_CARD_THOUSANDS_TITLE_1", isFemale: false), text.Substring(0, 5));
			}
			break;
		case PlayerCardDescriptionType.Soul:
			text = playerData.CachedData.SoulsCollected.ToString();
			if (text.Length > 7)
			{
				text = string.Format(LocalizationManager.GetString("LOC_ID_HERO_CARD_THOUSANDS_TITLE_1", isFemale: false), text.Substring(0, 5));
			}
			break;
		case PlayerCardDescriptionType.Level:
			text = string.Format(LocalizationManager.GetString("LOC_ID_HERO_CARD_LEVEL_SHORT_1", isFemale: false), playerData.CachedData.Level);
			break;
		case PlayerCardDescriptionType.XP:
			text = "EXP. 100/100 - What goes here?";
			break;
		case PlayerCardDescriptionType.Resolve:
			text = string.Format(LocalizationManager.GetString("LOC_ID_GENERAL_UI_PERCENT_1", isFemale: false), Mathf.RoundToInt(playerData.CachedData.ActualResolve * 100f));
			break;
		}
		m_text.text = text;
	}

	private string GetValueString(float value, bool isPercent)
	{
		if (isPercent)
		{
			value *= 100f;
		}
		string text = value.ToString("F1", LocalizationManager.GetCurrentCultureInfo());
		if (text[text.Length - 1] == '0')
		{
			text = Mathf.RoundToInt(value).ToString();
		}
		if (isPercent)
		{
			return string.Format(LocalizationManager.GetString("LOC_ID_GENERAL_UI_PERCENT_1", isFemale: false), text);
		}
		return text;
	}

	private string GetModString(float value)
	{
		value *= 100f;
		string text = value.ToString("F1", LocalizationManager.GetCurrentCultureInfo());
		if (text[text.Length - 1] == '0')
		{
			text = Mathf.RoundToInt(value).ToString();
		}
		if (value > 0f)
		{
			return "<color=#00a01e>(+" + string.Format(LocalizationManager.GetString("LOC_ID_GENERAL_UI_PERCENT_1", isFemale: false), text) + ")</color>";
		}
		if (value < 0f)
		{
			return "<color=red>(" + string.Format(LocalizationManager.GetString("LOC_ID_GENERAL_UI_PERCENT_1", isFemale: false), text) + ")</color>";
		}
		return "";
	}

	public void RefreshText(object sender, EventArgs args)
	{
		if (m_assignedArgs != null)
		{
			UpdateStat(null, m_assignedArgs);
		}
	}
}
