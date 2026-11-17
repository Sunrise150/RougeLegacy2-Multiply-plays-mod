using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class SoulShopOmniUIEntry : BaseOmniUIEntry
{
	[SerializeField]
	private bool m_isToggle;

	[SerializeField]
	private SoulShopOmniUIToggleButton m_toggleButton;

	[SerializeField]
	private SoulShopType m_soulShopType;

	[SerializeField]
	[Tooltip("Used for Soul Shop Entries with that titles that don't use the full width.  Tells the entry how much to move the TMP right margin if you want to use the full width of the entry.")]
	private int m_rightMarginOffset;

	private SoulShopOmniUIDescriptionEventArgs m_eventArgs;

	public SoulShopType SoulShopType => m_soulShopType;

	public bool IsToggle => m_isToggle;

	public bool IsUnlocked
	{
		get
		{
			SoulShopData soulShopData = SoulShopLibrary.GetSoulShopData(SoulShopType);
			if ((bool)soulShopData)
			{
				SoulShopObj soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj(SoulShopType);
				if (soulShopObj != null && soulShopObj.CurrentOwnedLevel > 0)
				{
					return true;
				}
				int totalSoulShopObjOwnedLevel = SaveManager.ModeSaveData.GetTotalSoulShopObjOwnedLevel();
				int unlockLevel = soulShopData.UnlockLevel;
				if (totalSoulShopObjOwnedLevel >= unlockLevel)
				{
					return true;
				}
				return false;
			}
			return false;
		}
	}

	public override EventArgs EntryEventArgs
	{
		get
		{
			if (m_eventArgs == null)
			{
				m_eventArgs = new SoulShopOmniUIDescriptionEventArgs(SoulShopType, OmniUIButtonType.Purchasing);
			}
			else
			{
				m_eventArgs.Initialize(SoulShopType, OmniUIButtonType.Purchasing);
			}
			return m_eventArgs;
		}
	}

	public override bool IsEntryActive
	{
		get
		{
			if (SoulShopType == SoulShopType.None)
			{
				return false;
			}
			SoulShopData soulShopData = SoulShopLibrary.GetSoulShopData(SoulShopType);
			if ((bool)soulShopData)
			{
				if (!soulShopData.Disabled)
				{
					return IsUnlocked;
				}
				return false;
			}
			return false;
		}
	}

	public override void Initialize(IOmniUIWindowController windowController)
	{
		base.Initialize(windowController);
		OmniUIButton[] buttonArray = m_buttonArray;
		for (int i = 0; i < buttonArray.Length; i++)
		{
			ISoulShopOmniUIButton obj = (ISoulShopOmniUIButton)buttonArray[i];
			obj.SoulShopType = SoulShopType;
			obj.ParentEntry = this;
			if (obj is IOmniUIIncrementButton omniUIIncrementButton)
			{
				omniUIIncrementButton.InitializeIncrementList();
			}
		}
		m_toggleButton.gameObject.SetActive(value: false);
	}

	public override void OnSelect(BaseEventData eventData)
	{
		if (base.Interactable)
		{
			SoulShopObj soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj(SoulShopType);
			if (!soulShopObj.IsNativeNull() && !soulShopObj.WasViewed)
			{
				soulShopObj.WasViewed = true;
				UpdateState();
			}
			base.OnSelect(eventData);
		}
	}

	public override void UpdateActive()
	{
		SoulShopData soulShopData = SoulShopLibrary.GetSoulShopData(SoulShopType);
		if (!soulShopData || soulShopData.Disabled)
		{
			if (base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: false);
			}
		}
		else if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
		}
	}

	public override void UpdateState()
	{
		base.UpdateState();
		SoulShopData soulShopData = SoulShopLibrary.GetSoulShopData(SoulShopType);
		SoulShopObj soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj(SoulShopType);
		bool isUnlocked = IsUnlocked;
		if (!soulShopData || soulShopObj.IsNativeNull())
		{
			return;
		}
		if (!isUnlocked)
		{
			string @string = LocalizationManager.GetString("LOC_ID_SOULSHOP_MISC_SKILL_LOCKED_1", isFemale: false);
			@string = string.Format(@string, soulShopData.UnlockLevel - SaveManager.ModeSaveData.GetTotalSoulShopObjOwnedLevel());
			m_titleText.text = @string;
			if (m_newSymbol.gameObject.activeSelf)
			{
				m_newSymbol.gameObject.SetActive(value: false);
			}
		}
		else
		{
			string text = LocalizationManager.GetString(soulShopData.Title, isFemale: false);
			if (soulShopObj.CurrentOwnedLevel > 0 && soulShopObj.MaxLevel > 1)
			{
				text += $" +{soulShopObj.CurrentOwnedLevel}";
			}
			m_titleText.text = text;
			if (!soulShopObj.WasViewed)
			{
				if (!m_newSymbol.gameObject.activeSelf)
				{
					m_newSymbol.gameObject.SetActive(value: true);
				}
			}
			else if (m_newSymbol.gameObject.activeSelf)
			{
				m_newSymbol.gameObject.SetActive(value: false);
			}
		}
		if (m_rightMarginOffset != 0)
		{
			if (!isUnlocked || soulShopObj.CurrentOwnedLevel <= 0)
			{
				m_titleText.margin = new Vector4(0f, 0f, m_rightMarginOffset, 0f);
			}
			else
			{
				m_titleText.margin = Vector4.zero;
			}
		}
		if (IsEntryActive)
		{
			m_icon.sprite = IconLibrary.GetSoulShopIcon(SoulShopType);
		}
		else
		{
			m_icon.sprite = IconLibrary.GetDefaultSprite();
		}
	}
}
