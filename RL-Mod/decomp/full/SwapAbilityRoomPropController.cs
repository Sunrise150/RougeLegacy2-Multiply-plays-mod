using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using RL_Windows;
using TMPro;
using UnityEngine;

public class SwapAbilityRoomPropController : DualChoicePropController, ILocalizable
{
	[SerializeField]
	private CastAbilityType m_castAbilityTypeToSwap;

	[SerializeField]
	private GameObject m_closedSign;

	private AbilityType m_abilityType;

	private WaitRL_Yield m_waitYield;

	private int m_totalRoomRolls;

	private Action<MonoBehaviour, EventArgs> m_refreshText;

	private static List<AbilityType> m_potentialAbilityList = new List<AbilityType>();

	public AbilityType AbilityType => m_abilityType;

	protected override void Awake()
	{
		m_waitYield = new WaitRL_Yield(0f);
		m_refreshText = RefreshText;
		base.Awake();
	}

	protected override void InitializePooledPropOnEnter()
	{
		base.InitializePooledPropOnEnter();
		base.RightInfoTextBox.gameObject.SetActive(value: false);
		base.RightIcon.gameObject.SetActive(value: false);
		m_closedSign.gameObject.SetActive(value: false);
		m_totalRoomRolls = 1;
		if (int.TryParse(GetRoomMiscData("TotalAbilityRolls"), NumberStyles.Any, SaveManager.CultureInfo, out var result))
		{
			m_totalRoomRolls = result;
		}
		m_potentialAbilityList.Clear();
		AbilityType[] abilityArray = GetAbilityArray(m_castAbilityTypeToSwap);
		for (int i = 0; i < abilityArray.Length; i++)
		{
			if (abilityArray[i] != 0 && (bool)AbilityLibrary.GetAbility(abilityArray[i]))
			{
				m_potentialAbilityList.Add(abilityArray[i]);
			}
		}
		RollAbilities(m_totalRoomRolls, addToTotalRoomRolls: false);
	}

	public void RollAbilities(int numRolls, bool addToTotalRoomRolls)
	{
		if (addToTotalRoomRolls)
		{
			m_totalRoomRolls += numRolls;
			SetRoomMiscData("TotalAbilityRolls", m_totalRoomRolls.ToString());
		}
		for (int i = 0; i < numRolls; i++)
		{
			AbilityType currentPlayerAbility = GetCurrentPlayerAbility(CastAbilityType.Weapon);
			AbilityType currentPlayerAbility2 = GetCurrentPlayerAbility(CastAbilityType.Spell);
			AbilityType currentPlayerAbility3 = GetCurrentPlayerAbility(CastAbilityType.Talent);
			m_abilityType = AbilityType.None;
			while (m_abilityType == AbilityType.None)
			{
				if (m_potentialAbilityList.Count == 0)
				{
					m_abilityType = AbilityType.SwordWeapon;
					break;
				}
				int randomNumber = RNGManager.GetRandomNumber(RngID.SpecialProps_RoomSeed, "AbilitySwapRoomPropController.RollAbilities()", 0, m_potentialAbilityList.Count);
				AbilityType abilityType = m_potentialAbilityList[randomNumber];
				for (int j = 0; j < m_potentialAbilityList.Count; j++)
				{
					if (m_potentialAbilityList[j] == abilityType)
					{
						m_potentialAbilityList.RemoveAt(j);
						j--;
					}
				}
				if (abilityType != currentPlayerAbility && abilityType != currentPlayerAbility2 && abilityType != currentPlayerAbility3)
				{
					m_abilityType = abilityType;
				}
			}
		}
		InitializeTextBox();
	}

	public void InitializeTextBox()
	{
		PlayerManager.GetPlayerController();
		AbilityType abilityType = AbilityType.None;
		GenericInfoTextBox genericInfoTextBox = null;
		SpriteRenderer spriteRenderer = null;
		abilityType = AbilityType;
		genericInfoTextBox = base.LeftInfoTextBox;
		spriteRenderer = base.LeftIcon;
		BaseAbility_RL ability = AbilityLibrary.GetAbility(abilityType);
		if (!ability)
		{
			throw new Exception("Invalid ability selected. AbilityType: " + abilityType);
		}
		AbilityData abilityData = ability.AbilityData;
		if ((bool)abilityData)
		{
			genericInfoTextBox.HeaderText.text = LocalizationManager.GetString(abilityData.Title, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			string costString = GetCostString(m_castAbilityTypeToSwap);
			genericInfoTextBox.SubHeaderText.text = costString;
			string @string = LocalizationManager.GetString(abilityData.Description, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			if (m_castAbilityTypeToSwap == CastAbilityType.Spell && !SaveManager.PlayerSaveData.GetSpellSeenState(abilityType))
			{
				@string = LocalizationManager.GetString("LOC_ID_LINEAGE_HIDDEN_ABILITY_1", isFemale: false);
			}
			TMP_Text subHeaderText = genericInfoTextBox.SubHeaderText;
			subHeaderText.text = subHeaderText.text + "\n" + @string;
			genericInfoTextBox.DescriptionText.text = "";
			genericInfoTextBox.SubHeaderText.GetComponent<TextGlyphConverter>().UpdateText(forceUpdate: true);
			genericInfoTextBox.DescriptionText.GetComponent<TextGlyphConverter>().UpdateText(forceUpdate: true);
			spriteRenderer.sprite = IconLibrary.GetAbilityIcon(abilityType, returnNullIfNotFound: false);
		}
	}

	private string GetCostString(CastAbilityType castAbilityType)
	{
		string locID = null;
		switch (castAbilityType)
		{
		case CastAbilityType.Weapon:
			locID = "LOC_ID_RELIC_COST_WEAPON_SWAP_1";
			break;
		case CastAbilityType.Spell:
			locID = "LOC_ID_RELIC_COST_SPELL_SWAP_1";
			break;
		case CastAbilityType.Talent:
			locID = "LOC_ID_RELIC_COST_TALENT_SWAP_1";
			break;
		}
		float costAmount = RelicLibrary.GetRelicData(GetSwapRelic(castAbilityType)).CostAmount;
		float relicCostMod = SkillTreeLogicHelper.GetRelicCostMod();
		int num = Mathf.RoundToInt(Mathf.Clamp(costAmount - relicCostMod, 0f, float.MaxValue) * 100f);
		return string.Format(LocalizationManager.GetString(locID, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), num);
	}

	public static int CalculateHealthCost(float cost, PlayerController playerController, bool applySkillReduc)
	{
		if (applySkillReduc)
		{
			cost = Mathf.Clamp(cost - SkillTreeLogicHelper.GetRelicCostMod(), 0f, 1f);
		}
		return Mathf.RoundToInt((float)playerController.ClassModdedMaxHealth * (1f + playerController.TraitMaxHealthMod) * cost);
	}

	public void ChooseLeftAbility()
	{
		StartCoroutine(ChooseAbility(AbilityType));
	}

	public void ChooseRightAbility()
	{
	}

	private IEnumerator ChooseAbility(AbilityType abilityType)
	{
		base.LeftIcon.gameObject.SetActive(value: false);
		PlayerController playerController = PlayerManager.GetPlayerController();
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		playerController.Animator.SetBool("Victory", value: true);
		m_waitYield.CreateNew(0.5f);
		yield return m_waitYield;
		if (m_castAbilityTypeToSwap == CastAbilityType.Weapon && TraitManager.IsTraitActive(TraitType.CantAttack))
		{
			if (SaveManager.PlayerSaveData.CurrentCharacter.TraitOne == TraitType.CantAttack)
			{
				SaveManager.PlayerSaveData.CurrentCharacter.TraitOne = TraitType.CanNowAttack;
				TraitManager.SetTraitSeenState(TraitType.CanNowAttack, TraitSeenState.SeenTwice, forceState: true);
			}
			else if (SaveManager.PlayerSaveData.CurrentCharacter.TraitTwo == TraitType.CantAttack)
			{
				SaveManager.PlayerSaveData.CurrentCharacter.TraitTwo = TraitType.CanNowAttack;
				TraitManager.SetTraitSeenState(TraitType.CanNowAttack, TraitSeenState.SeenTwice, forceState: true);
			}
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.TraitsChanged, this, new TraitChangedEventArgs(SaveManager.PlayerSaveData.CurrentCharacter.TraitOne, SaveManager.PlayerSaveData.CurrentCharacter.TraitTwo));
		}
		AbilityDrop abilityDrop = new AbilityDrop(abilityType, m_castAbilityTypeToSwap);
		if (!WindowManager.GetIsWindowLoaded(WindowID.SpecialItemDrop))
		{
			WindowManager.LoadWindow(WindowID.SpecialItemDrop);
		}
		(WindowManager.GetWindowController(WindowID.SpecialItemDrop) as SpecialItemDropWindowController).AddSpecialItemDrop(abilityDrop);
		WindowManager.SetWindowIsOpen(WindowID.SpecialItemDrop, isOpen: true);
		while (WindowManager.GetIsWindowOpen(WindowID.SpecialItemDrop))
		{
			yield return null;
		}
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		playerController.Animator.SetBool("Victory", value: false);
		m_waitYield.CreateNew(0.25f);
		yield return m_waitYield;
		ItemDropManager.DropSpecialItem(abilityDrop, displayItemDropWindow: false);
		SaveManager.PlayerSaveData.GetRelic(GetSwapRelic(m_castAbilityTypeToSwap)).SetLevel(1, additive: true);
		if (SaveManager.PlayerSaveData.GetRelic(RelicType.WeaponSwap).Level > 0 && SaveManager.PlayerSaveData.GetRelic(RelicType.TalentSwap).Level > 0 && SaveManager.PlayerSaveData.GetRelic(RelicType.SpellSwap).Level > 0)
		{
			StoreAPIManager.GiveAchievement(AchievementType.AllBlessings, StoreType.All);
		}
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
		DisableProp(firstTimeDisabled: true);
		PropComplete();
	}

	private AbilityType[] GetAbilityArray(CastAbilityType castAbilityType)
	{
		return castAbilityType switch
		{
			CastAbilityType.Spell => AbilityType_RL.ValidSpellTypeArray, 
			CastAbilityType.Talent => AbilityType_RL.ValidTalentTypeArray, 
			_ => AbilityType_RL.ValidWeaponTypeArray, 
		};
	}

	private AbilityType GetCurrentPlayerAbility(CastAbilityType castAbilityType)
	{
		BaseAbility_RL ability = PlayerManager.GetPlayerController().CastAbility.GetAbility(castAbilityType);
		if ((bool)ability)
		{
			return ability.AbilityType;
		}
		return AbilityType.None;
	}

	private RelicType GetSwapRelic(CastAbilityType castAbilityType)
	{
		return castAbilityType switch
		{
			CastAbilityType.Spell => RelicType.SpellSwap, 
			CastAbilityType.Talent => RelicType.TalentSwap, 
			_ => RelicType.WeaponSwap, 
		};
	}

	protected override void DisableProp(bool firstTimeDisabled)
	{
		base.DisableProp(firstTimeDisabled);
		m_closedSign.gameObject.SetActive(value: true);
		if (firstTimeDisabled)
		{
			StartCoroutine(ClosedSignPopupCoroutine());
		}
	}

	private IEnumerator ClosedSignPopupCoroutine()
	{
		Vector3 localPosition = m_closedSign.transform.localPosition;
		localPosition.y -= 2f;
		m_closedSign.transform.localPosition = localPosition;
		TweenManager.TweenBy(m_closedSign.transform, 0.25f, Ease.Back.EaseOut, "localPosition.y", 2);
		float delayTime = Time.time + 0.2f;
		while (Time.time < delayTime)
		{
			yield return null;
		}
		EffectManager.PlayEffect(m_closedSign.gameObject, null, "LandedDust_Effect", m_closedSign.transform.position, 0f, EffectStopType.Gracefully);
	}

	private void OnEnable()
	{
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.LanguageChanged, m_refreshText);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.LanguageChanged, m_refreshText);
	}

	public void RefreshText(object sender, EventArgs args)
	{
		InitializeTextBox();
	}
}
