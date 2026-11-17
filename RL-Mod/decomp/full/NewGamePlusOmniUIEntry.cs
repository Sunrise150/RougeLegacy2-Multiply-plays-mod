using System;
using UnityEngine.EventSystems;

public class NewGamePlusOmniUIEntry : BaseOmniUIEntry
{
	private NewGamePlusOmniUIDescriptionEventArgs m_eventArgs;

	public BurdenType BurdenType { get; protected set; }

	public bool IsNGPlusButton => BurdenType == BurdenType.None;

	public override EventArgs EntryEventArgs
	{
		get
		{
			if (m_eventArgs == null)
			{
				m_eventArgs = new NewGamePlusOmniUIDescriptionEventArgs(BurdenType, OmniUIButtonType.Purchasing);
			}
			else
			{
				m_eventArgs.Initialize(BurdenType, OmniUIButtonType.Purchasing);
			}
			return m_eventArgs;
		}
	}

	public override bool IsEntryActive
	{
		get
		{
			if (BurdenType == BurdenType.None)
			{
				return true;
			}
			return BurdenManager.IsBurdenUnlocked(BurdenType);
		}
	}

	public void Initialize(BurdenType burdenType, NewGamePlusOmniUIWindowController windowController)
	{
		BurdenType = burdenType;
		Initialize(windowController);
		OmniUIButton[] buttonArray = m_buttonArray;
		for (int i = 0; i < buttonArray.Length; i++)
		{
			((INewGamePlusOmniUIButton)buttonArray[i]).BurdenType = BurdenType;
		}
		if (IsNGPlusButton)
		{
			m_titleText.text = string.Format(LocalizationManager.GetString("LOC_ID_NEWGAMEPLUS_ENTER_NG_TITLE_1", isFemale: false), NewGamePlusOmniUIWindowController.CurrentNewGamePlusLevel);
			return;
		}
		string @string = LocalizationManager.GetString(BurdenLibrary.GetBurdenData(BurdenType).Title, isFemale: false);
		m_titleText.text = @string;
	}

	public override void UpdateActive()
	{
		BurdenData burdenData = BurdenLibrary.GetBurdenData(BurdenType);
		if (!IsNGPlusButton && (!burdenData || burdenData.Disabled))
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
		bool isNGPlusButton = IsNGPlusButton;
		if (isNGPlusButton)
		{
			m_titleText.text = string.Format(LocalizationManager.GetString("LOC_ID_BURDEN_TITLE_ACTIVATE_TIMELINE_1", isFemale: false), NewGamePlusOmniUIWindowController.CurrentNewGamePlusLevel);
		}
		else
		{
			if (BurdenManager.GetBurden(BurdenType).FoundState == FoundState.FoundButNotViewed && BurdenManager.IsBurdenUnlocked(BurdenType))
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
			if (IsEntryActive)
			{
				m_icon.sprite = IconLibrary.GetBurdenIcon(BurdenType);
			}
			else
			{
				m_icon.sprite = IconLibrary.GetDefaultSprite();
			}
		}
		base.UpdateState();
		if (!isNGPlusButton)
		{
			if (!IsEntryActive)
			{
				m_titleText.text = "???";
				return;
			}
			string @string = LocalizationManager.GetString(BurdenLibrary.GetBurdenData(BurdenType).Title, isFemale: false);
			m_titleText.text = @string;
			return;
		}
		if (m_iconGO.activeSelf)
		{
			m_iconGO.SetActive(value: false);
		}
		if (m_inactiveIconGO.activeSelf)
		{
			m_inactiveIconGO.SetActive(value: false);
		}
	}

	public override void OnSelect(BaseEventData eventData)
	{
		if (!base.Interactable)
		{
			return;
		}
		if (BurdenType != 0)
		{
			FoundState foundState = BurdenManager.GetFoundState(BurdenType);
			bool flag = BurdenManager.IsBurdenUnlocked(BurdenType);
			if (foundState == FoundState.FoundButNotViewed && flag)
			{
				BurdenManager.SetFoundState(BurdenType, FoundState.FoundAndViewed, overrideValues: false);
				Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateState, this, null);
			}
		}
		base.OnSelect(eventData);
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		NewGamePlusOmniUIWindowController newGamePlusOmniUIWindowController = m_windowController as NewGamePlusOmniUIWindowController;
		if (BurdenType == BurdenType.None && !newGamePlusOmniUIWindowController.InTimelineEntry)
		{
			newGamePlusOmniUIWindowController.SetTimelineEntryActive(active: true, playSFX: true, usingMouse: true);
		}
		else if (BurdenType != 0 && newGamePlusOmniUIWindowController.InTimelineEntry)
		{
			newGamePlusOmniUIWindowController.SetTimelineEntryActive(active: false, playSFX: true, usingMouse: true);
		}
		if (!(m_windowController as NewGamePlusOmniUIWindowController).InTimelineEntry)
		{
			base.OnPointerEnter(eventData);
		}
	}
}
