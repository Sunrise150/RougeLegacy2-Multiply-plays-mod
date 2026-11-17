using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class CharacterClass : MonoBehaviour
{
	[SerializeField]
	[ReadOnlyOnPlay]
	protected ClassType m_classType = ClassType.SwordClass;

	[Space(10f)]
	[SerializeField]
	[ReadOnly]
	protected AbilityType m_weaponAbilityType;

	[SerializeField]
	[ReadOnly]
	protected AbilityType m_spellAbilityType;

	[SerializeField]
	[ReadOnly]
	protected AbilityType m_talentAbilityType;

	[Space(10f)]
	[SerializeField]
	[ReadOnly]
	protected ClassData m_classData;

	private PlayerController m_playerController;

	public bool IsInitialized { get; private set; }

	public bool OverrideSaveFileValues
	{
		get
		{
			if (SaveManager.IsRunning)
			{
				return false;
			}
			if (LineageWindowController.CharacterLoadedFromLineage)
			{
				return false;
			}
			return true;
		}
	}

	public ClassData ClassData => m_classData;

	public ClassType ClassType
	{
		get
		{
			return m_classType;
		}
		set
		{
			m_classType = value;
			if (Application.isPlaying)
			{
				m_classData = ClassLibrary.GetClassData(m_classType);
				LookCreator.InitializeClassLook(m_classType, m_playerController.LookController);
				m_playerController.InitializeAllMods(resetHP: true, resetMP: true);
			}
		}
	}

	public AbilityType WeaponAbilityType
	{
		get
		{
			return m_weaponAbilityType;
		}
		set
		{
			m_weaponAbilityType = value;
			if (Application.isPlaying)
			{
				SetAbility(CastAbilityType.Weapon, value);
			}
		}
	}

	public AbilityType SpellAbilityType
	{
		get
		{
			return m_spellAbilityType;
		}
		set
		{
			m_spellAbilityType = value;
			if (Application.isPlaying)
			{
				SetAbility(CastAbilityType.Spell, value);
			}
		}
	}

	public AbilityType TalentAbilityType
	{
		get
		{
			return m_talentAbilityType;
		}
		set
		{
			m_talentAbilityType = value;
			if (Application.isPlaying)
			{
				SetAbility(CastAbilityType.Talent, value);
			}
		}
	}

	protected virtual void Awake()
	{
		if (Application.isPlaying)
		{
			m_playerController = GetComponent<PlayerController>();
			if (!OverrideSaveFileValues)
			{
				LoadPlayerSaveData();
			}
			m_classData = ClassLibrary.GetClassData(m_classType);
			IsInitialized = true;
		}
	}

	private void LoadPlayerSaveData()
	{
		CharacterData currentCharacter = SaveManager.PlayerSaveData.CurrentCharacter;
		m_classType = currentCharacter.ClassType;
		m_weaponAbilityType = currentCharacter.Weapon;
		m_spellAbilityType = currentCharacter.Spell;
		m_talentAbilityType = currentCharacter.Talent;
	}

	private IEnumerator Start()
	{
		while (!m_playerController.IsInitialized)
		{
			yield return null;
		}
		if (m_classData != null)
		{
			InitializeAbilities();
		}
	}

	private void InitializeAbilities()
	{
		SetAbility(CastAbilityType.Weapon, WeaponAbilityType);
		SetAbility(CastAbilityType.Spell, SpellAbilityType);
		SetAbility(CastAbilityType.Talent, TalentAbilityType);
	}

	private AbilityType ChooseRandomAbility(CastAbilityType castAbilityType)
	{
		AbilityType[] array = null;
		switch (castAbilityType)
		{
		case CastAbilityType.Weapon:
			array = m_classData.WeaponData.WeaponAbilityArray;
			break;
		case CastAbilityType.Spell:
			array = m_classData.SpellData.SpellAbilityArray;
			break;
		case CastAbilityType.Talent:
			array = m_classData.TalentData.TalentAbilityArray;
			break;
		}
		int randomNumber = RNGManager.GetRandomNumber(RngID.Lineage, "CharacterClass.ChooseRandomAbility()", 0, array.Length);
		return array[randomNumber];
	}

	public void SetAbility(CastAbilityType castAbilityType, AbilityType abilityType, bool destroyOldAbility = true)
	{
		if (abilityType == AbilityType.Random)
		{
			abilityType = ChooseRandomAbility(castAbilityType);
		}
		switch (castAbilityType)
		{
		case CastAbilityType.Weapon:
			m_weaponAbilityType = abilityType;
			break;
		case CastAbilityType.Spell:
			m_spellAbilityType = abilityType;
			break;
		case CastAbilityType.Talent:
			m_talentAbilityType = abilityType;
			break;
		}
		m_playerController.CastAbility.SetAbility(castAbilityType, abilityType, destroyOldAbility);
		if (m_playerController.LookController.IsClassLookInitialized)
		{
			m_playerController.LookController.InitializeEquipmentLook(SaveManager.PlayerSaveData.CurrentCharacter);
		}
	}

	public void SetAbility(CastAbilityType castAbilityType, BaseAbility_RL ability, bool destroyOldAbility = false)
	{
		AbilityType abilityType = AbilityType.None;
		if (ability != null)
		{
			abilityType = ability.AbilityType;
		}
		switch (castAbilityType)
		{
		case CastAbilityType.Weapon:
			m_weaponAbilityType = abilityType;
			break;
		case CastAbilityType.Spell:
			m_spellAbilityType = abilityType;
			break;
		case CastAbilityType.Talent:
			m_talentAbilityType = abilityType;
			break;
		}
		m_playerController.CastAbility.SetAbility(castAbilityType, ability, destroyOldAbility);
		if (m_playerController.LookController.IsClassLookInitialized)
		{
			m_playerController.LookController.InitializeEquipmentLook(SaveManager.PlayerSaveData.CurrentCharacter);
		}
	}
}
