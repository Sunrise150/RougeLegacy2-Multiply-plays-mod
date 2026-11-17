using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/Icon Library")]
public class IconLibrary : ScriptableObject
{
	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/IconLibrary";

	private const string ASSET_PATH = "Assets/Content/Scriptable Objects/Libraries/IconLibrary.asset";

	[Header("Default Sprites")]
	[SerializeField]
	private Sprite m_defaultSprite;

	[SerializeField]
	private Sprite m_squareIconFrameSprite;

	[SerializeField]
	private Sprite m_squareNotFoundIconFrameSprite;

	[SerializeField]
	private Sprite m_hexagonIconFrameSprite;

	[SerializeField]
	private Sprite m_hexagonNotFoundIconFrameSprite;

	[Header("Skill Tree Icons")]
	[SerializeField]
	private SkillTreeTypeSpriteDictionary m_skillTreeIconLibrary;

	[SerializeField]
	private Sprite m_skillTreeLockedIcon;

	[SerializeField]
	private Sprite m_skillTreeSoulLockedIcon;

	[Header("Class Icons")]
	[SerializeField]
	private ClassTypeSpriteDictionary m_classIconLibrary;

	[Header("Ability Icons")]
	[SerializeField]
	private AbilityTypeSpriteDictionary m_weaponAbilityIconLibrary;

	[SerializeField]
	private AbilityTypeSpriteDictionary m_weaponAbilityIconLargeLibrary;

	[Space(5f)]
	[SerializeField]
	private AbilityTypeSpriteDictionary m_spellAbilityIconLibrary;

	[SerializeField]
	private AbilityTypeSpriteDictionary m_spellAbilityIconLargeLibrary;

	[Space(5f)]
	[SerializeField]
	private AbilityTypeSpriteDictionary m_talentAbilityIconLibrary;

	[SerializeField]
	private AbilityTypeSpriteDictionary m_talentAbilityIconLargeLibrary;

	[Space(5f)]
	[SerializeField]
	private CooldownRegenTypeSpriteDictionary m_abilityCooldownIconLibrary;

	[Header("Equipment Icons")]
	[SerializeField]
	private EquipmentCategoryTypeSpriteDictionary m_equipmentCategoryIconLibrary;

	[Space(5f)]
	[SerializeField]
	private EquipmentTypeSpriteDictionary m_weaponIconLibrary;

	[Space(5f)]
	[SerializeField]
	private EquipmentTypeSpriteDictionary m_headIconLibrary;

	[Space(5f)]
	[SerializeField]
	private EquipmentTypeSpriteDictionary m_chestIconLibrary;

	[Space(5f)]
	[SerializeField]
	private EquipmentTypeSpriteDictionary m_capeIconLibrary;

	[Space(5f)]
	[SerializeField]
	private EquipmentTypeSpriteDictionary m_trinketIconLibrary;

	[Header("Rune Icons")]
	[SerializeField]
	private RuneTypeSpriteDictionary m_runeIconLibrary;

	[Header("Trait Icons")]
	[SerializeField]
	private TraitTypeSpriteDictionary m_traitIconLibrary;

	[Header("Status Effect Icons")]
	[SerializeField]
	private StatusEffectUITypeSpriteDictionary m_statusEffectIconLibrary;

	[Header("Relic Icons")]
	[SerializeField]
	private RelicTypeSpriteDictionary m_relicIconLibrary;

	[SerializeField]
	private RelicTypeSpriteDictionary m_relicLargeIconLibrary;

	[Header("Heirloom Icons")]
	[SerializeField]
	private HeirloomTypeSpriteDictionary m_heirloomIconLibrary;

	[Header("Dialogue Portrait Images")]
	[SerializeField]
	private DialoguePortraitTypeSpriteDictionary m_dialoguePortraitLibrary;

	[Header("Misc Icons")]
	[SerializeField]
	private MiscIconTypeSpriteDictionary m_miscIconLibrary;

	[Header("Challenge Icons")]
	[SerializeField]
	private ChallengeTypeSpriteDictionary m_challengeIconLibrary;

	[Header("Burden Icons")]
	[SerializeField]
	private BurdenTypeSpriteDictionary m_burdenIconLibrary;

	[Header("Journal Category Icons")]
	[SerializeField]
	private JournalCategoryTypeSpriteDictionary m_journalCategoryIconLibrary;

	[Header("SoulShop Icons")]
	[SerializeField]
	private SoulShopTypeSpriteDictionary m_soulShopIconLibrary;

	[Header("Mouse Icons")]
	[SerializeField]
	private CursorIconTypeTexture2DDictionary m_cursorIconLibrary;

	private static IconLibrary m_instance;

	private static IconLibrary Instance
	{
		get
		{
			if (m_instance == null && Application.isPlaying)
			{
				m_instance = CDGResources.Load<IconLibrary>("Scriptable Objects/Libraries/IconLibrary");
			}
			return m_instance;
		}
	}

	public static Sprite GetDefaultSprite()
	{
		return Instance.m_defaultSprite;
	}

	public static Sprite GetSquareIconFrameSprite(bool getNotFound)
	{
		if (getNotFound)
		{
			return Instance.m_squareNotFoundIconFrameSprite;
		}
		return Instance.m_squareIconFrameSprite;
	}

	public static Sprite GetHexagonIconFrameSprite(bool getNotFound)
	{
		if (getNotFound)
		{
			return Instance.m_hexagonNotFoundIconFrameSprite;
		}
		return Instance.m_hexagonIconFrameSprite;
	}

	public static Sprite GetSkillTreeIcon(SkillTreeType skillTreeType)
	{
		Sprite value = null;
		Instance.m_skillTreeIconLibrary.TryGetValue(skillTreeType, out value);
		if (value == null)
		{
			Debug.Log("<color=red>Sprite: " + skillTreeType.ToString() + " not found in Sprite Library. Please ensure the entry exists in the Icon Library scriptable object.</color>");
			return Instance.m_defaultSprite;
		}
		return value;
	}

	public static Sprite GetSkillTreeLockedIcon()
	{
		return Instance.m_skillTreeLockedIcon;
	}

	public static Sprite GetSkillTreeSoulLockedIcon()
	{
		return Instance.m_skillTreeSoulLockedIcon;
	}

	public static Sprite GetAbilityIcon(AbilityType abilityType, bool returnNullIfNotFound)
	{
		Sprite value = null;
		Dictionary<AbilityType, Sprite> dictionary = Instance.m_weaponAbilityIconLibrary;
		if (!dictionary.TryGetValue(abilityType, out value))
		{
			dictionary = Instance.m_spellAbilityIconLibrary;
		}
		if (!dictionary.TryGetValue(abilityType, out value))
		{
			dictionary = Instance.m_talentAbilityIconLibrary;
		}
		dictionary.TryGetValue(abilityType, out value);
		if (!value)
		{
			Debug.Log("<color=red>Sprite: " + abilityType.ToString() + " not found in Sprite Library. Please ensure the entry exists in the Icon Library scriptable object.</color>");
			if (returnNullIfNotFound)
			{
				return null;
			}
			return Instance.m_defaultSprite;
		}
		return value;
	}

	public static Sprite GetLargeAbilityIcon(AbilityType abilityType, bool returnNullIfNotFound)
	{
		Sprite value = null;
		Dictionary<AbilityType, Sprite> dictionary = Instance.m_weaponAbilityIconLargeLibrary;
		if (!dictionary.TryGetValue(abilityType, out value))
		{
			dictionary = Instance.m_spellAbilityIconLargeLibrary;
		}
		if (!dictionary.TryGetValue(abilityType, out value))
		{
			dictionary = Instance.m_talentAbilityIconLargeLibrary;
		}
		dictionary.TryGetValue(abilityType, out value);
		if (!value)
		{
			Debug.Log("<color=red>Sprite: " + abilityType.ToString() + " not found in Sprite Library. Please ensure the entry exists in the Icon Library scriptable object.</color>");
			if (returnNullIfNotFound)
			{
				return null;
			}
			return Instance.m_defaultSprite;
		}
		return value;
	}

	public static Sprite GetAbilityCooldownIcon(CooldownRegenType cooldownType, bool returnNullIfNotFound)
	{
		Sprite value = null;
		if (Instance.m_abilityCooldownIconLibrary.TryGetValue(cooldownType, out value) && value == null)
		{
			Debug.Log("<color=red>Sprite: " + cooldownType.ToString() + " not found in Sprite Library. Please ensure the entry exists in the Icon Library scriptable object.</color>");
			if (returnNullIfNotFound)
			{
				return null;
			}
			return Instance.m_defaultSprite;
		}
		return value;
	}

	public static Sprite GetEquipmentCategoryIcon(EquipmentCategoryType categoryType)
	{
		Sprite value = null;
		Instance.m_equipmentCategoryIconLibrary.TryGetValue(categoryType, out value);
		if (value == null)
		{
			Debug.Log("<color=red>Sprite: " + categoryType.ToString() + " not found in Sprite Library. Please ensure the entry exists in the Icon Library scriptable object.</color>");
			return Instance.m_defaultSprite;
		}
		return value;
	}

	public static Sprite GetEquipmentIcon(EquipmentCategoryType categoryType, EquipmentType equipmentType)
	{
		EquipmentTypeSpriteDictionary equipmentTypeSpriteDictionary = null;
		switch (categoryType)
		{
		case EquipmentCategoryType.Weapon:
			equipmentTypeSpriteDictionary = Instance.m_weaponIconLibrary;
			break;
		case EquipmentCategoryType.Head:
			equipmentTypeSpriteDictionary = Instance.m_headIconLibrary;
			break;
		case EquipmentCategoryType.Chest:
			equipmentTypeSpriteDictionary = Instance.m_chestIconLibrary;
			break;
		case EquipmentCategoryType.Cape:
			equipmentTypeSpriteDictionary = Instance.m_capeIconLibrary;
			break;
		case EquipmentCategoryType.Trinket:
			equipmentTypeSpriteDictionary = Instance.m_trinketIconLibrary;
			break;
		}
		Sprite value = null;
		equipmentTypeSpriteDictionary.TryGetValue(equipmentType, out value);
		if (value == null)
		{
			Debug.Log("<color=red>Sprite: " + equipmentType.ToString() + " - " + categoryType.ToString() + " not found in Sprite Library. Please ensure the entry exists in the Icon Library scriptable object.</color>");
			return Instance.m_defaultSprite;
		}
		return value;
	}

	public static Sprite GetRuneIcon(RuneType runeType)
	{
		Sprite value = null;
		Instance.m_runeIconLibrary.TryGetValue(runeType, out value);
		if (value == null)
		{
			Debug.Log("<color=red>Sprite: " + runeType.ToString() + " not found in Sprite Library. Please ensure the entry exists in the Icon Library scriptable object.</color>");
			return Instance.m_defaultSprite;
		}
		return value;
	}

	public static Sprite GetStatusEffectSprite(StatusBarEntryType statusEffectUIType)
	{
		Sprite value = null;
		Instance.m_statusEffectIconLibrary.TryGetValue(statusEffectUIType, out value);
		if (value == null)
		{
			Debug.Log("<color=red>Sprite: " + statusEffectUIType.ToString() + " not found in Sprite Library. Please ensure the entry exists in the Icon Library scriptable object.</color>");
			return Instance.m_defaultSprite;
		}
		return value;
	}

	public static Sprite GetRelicSprite(RelicType relicType, bool getLargeIcon)
	{
		if (!(getLargeIcon ? Instance.m_relicLargeIconLibrary : Instance.m_relicIconLibrary).TryGetValue(relicType, out var value))
		{
			Debug.Log("<color=red>Sprite: " + relicType.ToString() + " not found in Sprite Library. Please ensure the entry exists in the Icon Library scriptable object. IsLargeIcon: " + getLargeIcon + "</color>");
			return Instance.m_defaultSprite;
		}
		return value;
	}

	public static Sprite GetHeirloomSprite(HeirloomType heirloomType)
	{
		if (!Instance.m_heirloomIconLibrary.TryGetValue(heirloomType, out var value))
		{
			Debug.Log("<color=red>Sprite: " + heirloomType.ToString() + " not found in Sprite Library. Please ensure the entry exists in the Icon Library scriptable object.</color>");
			return Instance.m_defaultSprite;
		}
		return value;
	}

	public static Sprite GetTraitIcon(TraitType traitType)
	{
		if (!Instance.m_traitIconLibrary.TryGetValue(traitType, out var value))
		{
			Debug.Log("<color=red>Sprite: " + traitType.ToString() + " not found in Sprite Library. Please ensure the entry exists in the Icon Library scriptable object.</color>");
			return Instance.m_defaultSprite;
		}
		return value;
	}

	public static Sprite GetClassIcon(ClassType classType)
	{
		if (!Instance.m_classIconLibrary.TryGetValue(classType, out var value))
		{
			Debug.Log("<color=red>Sprite: " + classType.ToString() + " not found in Sprite Library. Please ensure the entry exists in the Icon Library scriptable object.</color>");
			return Instance.m_defaultSprite;
		}
		return value;
	}

	public static Sprite GetDialoguePortrait(DialoguePortraitType portraitType)
	{
		if (!Instance.m_dialoguePortraitLibrary.TryGetValue(portraitType, out var value))
		{
			Debug.Log("<color=red>Sprite: " + portraitType.ToString() + " not found in Sprite Library. Please ensure the entry exists in the Icon Library scriptable object.</color>");
			return Instance.m_defaultSprite;
		}
		return value;
	}

	public static Sprite GetMiscIcon(MiscIconType iconType)
	{
		if (!Instance.m_miscIconLibrary.TryGetValue(iconType, out var value))
		{
			Debug.Log("<color=red>Sprite: " + iconType.ToString() + " not found in Sprite Library. Please ensure the entry exists in the Icon Library scriptable object.</color>");
			return Instance.m_defaultSprite;
		}
		return value;
	}

	public static Sprite GetChallengeIcon(ChallengeType challengeType, ChallengeLibrary.ChallengeIconEntryType entryType)
	{
		if (!Instance.m_challengeIconLibrary.TryGetValue(challengeType, out var value))
		{
			Debug.Log("<color=red>Icons for " + challengeType.ToString() + " not found in Sprite Library. Please ensure the entry exists in the Icon Library scriptable object.</color>");
			return Instance.m_defaultSprite;
		}
		return entryType switch
		{
			ChallengeLibrary.ChallengeIconEntryType.Challenge => value.ChallengeIcon, 
			ChallengeLibrary.ChallengeIconEntryType.Bronze => value.BronzeTrophyIcon, 
			ChallengeLibrary.ChallengeIconEntryType.Silver => value.SilverTrophyIcon, 
			ChallengeLibrary.ChallengeIconEntryType.Gold => value.GoldTrophyIcon, 
			_ => Instance.m_defaultSprite, 
		};
	}

	public static Sprite GetBurdenIcon(BurdenType burdenType)
	{
		if (!Instance.m_burdenIconLibrary.TryGetValue(burdenType, out var value))
		{
			Debug.Log("<color=red>Sprite: " + burdenType.ToString() + " not found in Sprite Library. Please ensure the entry exists in the Icon Library scriptable object.</color>");
			return Instance.m_defaultSprite;
		}
		return value;
	}

	public static Sprite GetJournalCategoryIcon(JournalCategoryType categoryType)
	{
		if (!Instance.m_journalCategoryIconLibrary.TryGetValue(categoryType, out var value))
		{
			Debug.Log("<color=red>Sprite: " + categoryType.ToString() + " not found in Sprite Library. Please ensure the entry exists in the Icon Library scriptable object.</color>");
			return Instance.m_defaultSprite;
		}
		return value;
	}

	public static Sprite GetSoulShopIcon(SoulShopType soulShopType)
	{
		if (!Instance.m_soulShopIconLibrary.TryGetValue(soulShopType, out var value))
		{
			Debug.Log("<color=red>Sprite: " + soulShopType.ToString() + " not found in Sprite Library. Please ensure the entry exists in the Icon Library scriptable object.</color>");
			return Instance.m_defaultSprite;
		}
		return value;
	}

	public static Texture2D GetCursorIconTexture(CursorIconType cursorType)
	{
		if (!Instance.m_cursorIconLibrary.TryGetValue(cursorType, out var value))
		{
			Debug.Log("<color=red>Texture2D: " + cursorType.ToString() + " not found in Sprite Library. Please ensure the entry exists in the Icon Library scriptable object.</color>");
			return null;
		}
		return value;
	}
}
