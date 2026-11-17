using System;
using System.Collections.Generic;

public class SoulShopOmniUIChooseSpellButton : OmniUIIncrementButton<AbilityType>, ISoulShopOmniUIButton
{
	protected SoulShopOmniUIDescriptionEventArgs m_descriptionEventArgs;

	public override EventArgs ButtonEventArgs => m_descriptionEventArgs;

	public SoulShopType SoulShopType { get; set; }

	public SoulShopOmniUIEntry ParentEntry { get; set; }

	private void OnEnable()
	{
		InitializeIncrementList();
	}

	public override void InitializeIncrementList()
	{
		if (m_incrementList != null)
		{
			m_incrementList.Clear();
		}
		else
		{
			m_incrementList = new List<AbilityType>();
		}
		m_incrementList.AddRange(AbilityType_RL.ValidSpellTypeArray);
	}

	protected override void InitializeButtonEventArgs()
	{
		if (m_descriptionEventArgs == null)
		{
			m_descriptionEventArgs = new SoulShopOmniUIDescriptionEventArgs(SoulShopType, OmniUIButtonType.Purchasing);
		}
		else if (m_isDecrementButton)
		{
			m_descriptionEventArgs.Initialize(SoulShopType, OmniUIButtonType.Unequipping);
		}
		else
		{
			m_descriptionEventArgs.Initialize(SoulShopType, OmniUIButtonType.Equipping);
		}
	}

	public override void UpdateState()
	{
		SoulShopObj soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj(SoulShopType);
		if (!soulShopObj.IsNativeNull() && soulShopObj.CurrentOwnedLevel > 0)
		{
			if (!base.transform.parent.gameObject.activeSelf)
			{
				base.transform.parent.gameObject.SetActive(value: true);
			}
			AbilityType soulShopSpellChosen = SaveManager.ModeSaveData.SoulShopSpellChosen;
			int num = m_incrementList.IndexOf(soulShopSpellChosen);
			if (num != -1)
			{
				m_selectedIndex = num;
			}
			else
			{
				m_selectedIndex = 0;
				soulShopSpellChosen = m_incrementList[m_selectedIndex];
				SaveManager.ModeSaveData.SoulShopSpellChosen = soulShopSpellChosen;
			}
			UpdateIncrementText();
		}
		else if (base.transform.parent.gameObject.activeSelf)
		{
			base.transform.parent.gameObject.SetActive(value: false);
		}
	}

	public override void OnConfirmButtonPressed()
	{
		if (IsButtonActive)
		{
			base.OnConfirmButtonPressed();
			AbilityType soulShopSpellChosen = m_incrementList[m_selectedIndex];
			SaveManager.ModeSaveData.SoulShopSpellChosen = soulShopSpellChosen;
			RunOnConfirmPressedAnimation();
			ParentEntry.UpdateState();
		}
	}

	protected override void UpdateIncrementText()
	{
		string @string = LocalizationManager.GetString(AbilityLibrary.GetAbility(m_incrementList[m_selectedIndex]).AbilityData.Title, isFemale: false);
		m_levelText.text = @string;
	}
}
