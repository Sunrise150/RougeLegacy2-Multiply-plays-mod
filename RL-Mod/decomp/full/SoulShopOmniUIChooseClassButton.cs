using System;
using System.Collections.Generic;

public class SoulShopOmniUIChooseClassButton : OmniUIIncrementButton<ClassType>, ISoulShopOmniUIButton
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
			m_incrementList = new List<ClassType>();
		}
		m_incrementList.AddRange(CharacterCreator.GetAvailableClasses());
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
			ClassType soulShopClassChosen = SaveManager.ModeSaveData.SoulShopClassChosen;
			int num = m_incrementList.IndexOf(soulShopClassChosen);
			if (num != -1)
			{
				m_selectedIndex = num;
			}
			else
			{
				m_selectedIndex = 0;
				soulShopClassChosen = m_incrementList[m_selectedIndex];
				SaveManager.ModeSaveData.SoulShopClassChosen = soulShopClassChosen;
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
			ClassType soulShopClassChosen = m_incrementList[m_selectedIndex];
			SaveManager.ModeSaveData.SoulShopClassChosen = soulShopClassChosen;
			RunOnConfirmPressedAnimation();
			ParentEntry.UpdateState();
		}
	}

	protected override void UpdateIncrementText()
	{
		string @string = LocalizationManager.GetString(ClassLibrary.GetClassData(m_incrementList[m_selectedIndex]).PassiveData.Title, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		m_levelText.text = @string;
	}
}
