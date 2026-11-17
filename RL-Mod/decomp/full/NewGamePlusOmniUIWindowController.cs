using System;
using System.Collections.Generic;
using RL_Windows;
using UnityEngine;

public class NewGamePlusOmniUIWindowController : BaseOmniUIWindowController<BaseOmniUICategoryEntry, NewGamePlusOmniUIEntry>
{
	[SerializeField]
	private NewGamePlusOmniUIEntry m_timelineEntry;

	private Dictionary<BurdenType, int> m_storedBurdenValues = new Dictionary<BurdenType, int>();

	private Action m_cancelRemoveAllBurdens;

	private Action m_confirmRemoveAllBurdens;

	private Action m_quitMenu;

	private NewGamePlusOmniUIDescriptionEventArgs m_descriptionEventArgs;

	public override bool CanReset => true;

	public static int CurrentNewGamePlusLevel { get; set; }

	public static bool EnteringNGPlus { get; set; }

	public bool InTimelineEntry { get; private set; }

	public NewGamePlusOmniUIEntry CurrentlySelectedEntry
	{
		get
		{
			if (InTimelineEntry)
			{
				return m_timelineEntry;
			}
			if (base.ActiveEntryArray != null && base.SelectedEntryIndex < base.ActiveEntryArray.Length)
			{
				return base.ActiveEntryArray[base.SelectedEntryIndex];
			}
			return null;
		}
	}

	public BurdenType SelectedBurdenType
	{
		get
		{
			if (base.IsInitialized)
			{
				return base.ActiveEntryArray[base.SelectedEntryIndex].BurdenType;
			}
			return BurdenType.None;
		}
	}

	public override WindowID ID => WindowID.NewGamePlusNPC;

	protected override void Awake()
	{
		base.Awake();
		m_cancelRemoveAllBurdens = CancelRemoveAllBurdens;
		m_confirmRemoveAllBurdens = ConfirmRemoveAllBurdens;
		m_quitMenu = QuitMenu;
	}

	protected override void CreateCategoryEntries()
	{
	}

	protected override void CreateEntries()
	{
		if (base.EntryArray != null)
		{
			Array.Clear(base.EntryArray, 0, base.EntryArray.Length);
			base.EntryArray = null;
		}
		Array typeArray = BurdenType_RL.TypeArray;
		base.EntryArray = new NewGamePlusOmniUIEntry[typeArray.Length - 1];
		int num = 0;
		foreach (BurdenType item in typeArray)
		{
			if (item != 0)
			{
				base.EntryArray[num] = UnityEngine.Object.Instantiate(m_entryPrefab);
				base.EntryArray[num].transform.SetParent(base.EntryLayoutGroup.transform);
				base.EntryArray[num].transform.localScale = Vector3.one;
				base.EntryArray[num].Initialize(item, this);
				base.EntryArray[num].SetEntryIndex(num);
				num++;
			}
		}
		m_timelineEntry.Initialize(BurdenType.None, this);
		m_timelineEntry.SetEntryIndex(0);
	}

	protected override void OnOpen()
	{
		m_storedBurdenValues.Clear();
		BurdenType[] typeArray = BurdenType_RL.TypeArray;
		foreach (BurdenType burdenType in typeArray)
		{
			if (burdenType != 0)
			{
				m_storedBurdenValues.Add(burdenType, BurdenManager.GetBurdenLevel(burdenType));
			}
		}
		EnteringNGPlus = false;
		CurrentNewGamePlusLevel = SaveManager.PlayerSaveData.NewGamePlusLevel;
		base.OnOpen();
		SetTimelineEntryActive(active: true, playSFX: false, usingMouse: false);
	}

	protected override void OnClose()
	{
		base.OnClose();
		if (EnteringNGPlus)
		{
			return;
		}
		Debug.Log("<color=yellow>Player did not enter new timeline. Resetting burden values.</color>");
		foreach (KeyValuePair<BurdenType, int> storedBurdenValue in m_storedBurdenValues)
		{
			BurdenManager.SetBurdenLevel(storedBurdenValue.Key, storedBurdenValue.Value, additive: false, broadcast: false);
		}
	}

	public override void UpdateAllEntryStates()
	{
		base.UpdateAllEntryStates();
		m_timelineEntry.UpdateState();
	}

	public void SetTimelineEntryActive(bool active, bool playSFX, bool usingMouse)
	{
		InTimelineEntry = active;
		if (!m_timelineEntry.Interactable)
		{
			m_timelineEntry.Interactable = true;
		}
		if (active)
		{
			base.ActiveEntryArray[base.SelectedEntryIndex].OnDeselect(null);
			base.ActiveEntryArray[base.SelectedEntryIndex].DeselectAllButtons();
			m_timelineEntry.DeselectAllButtons();
			m_timelineEntry.UpdateState();
			m_timelineEntry.OnSelect(null);
			m_timelineEntry.SelectRightMostButton();
			if (playSFX && m_changeSelectedOptionEvent != null)
			{
				m_changeSelectedOptionEvent.Invoke();
			}
		}
		else
		{
			int selectedEntryIndex = base.SelectedEntryIndex;
			base.SelectedEntryIndex = -1;
			SetSelectedEntryIndex(selectedEntryIndex, playAudio: true, usingMouse);
			m_timelineEntry.OnDeselect(null);
			m_timelineEntry.DeselectAllButtons();
		}
	}

	protected override void OnUpButtonJustPressed()
	{
		if (!InTimelineEntry)
		{
			if (base.SelectedEntryIndex == 0)
			{
				SetTimelineEntryActive(active: true, playSFX: true, usingMouse: false);
			}
			else
			{
				base.OnUpButtonJustPressed();
			}
		}
		else
		{
			SetTimelineEntryActive(active: false, playSFX: true, usingMouse: false);
			SetSelectedEntryIndex(base.ActiveEntryArray.Length - 1, playAudio: true, usingMouse: false);
		}
	}

	protected override void OnDownButtonJustPressed()
	{
		if (!InTimelineEntry)
		{
			if (base.SelectedEntryIndex == base.ActiveEntryArray.Length - 1)
			{
				SetTimelineEntryActive(active: true, playSFX: true, usingMouse: false);
			}
			else
			{
				base.OnDownButtonJustPressed();
			}
		}
		else
		{
			SetTimelineEntryActive(active: false, playSFX: true, usingMouse: false);
			SetSelectedEntryIndex(0, playAudio: true, usingMouse: false);
		}
	}

	protected override void OnLeftButtonJustPressed()
	{
		if (InTimelineEntry)
		{
			m_timelineEntry.SelectButton(selectRight: false);
		}
		else
		{
			base.OnLeftButtonJustPressed();
		}
	}

	protected override void OnRightButtonJustPressed()
	{
		if (InTimelineEntry)
		{
			m_timelineEntry.SelectButton(selectRight: true);
		}
		else
		{
			base.OnRightButtonJustPressed();
		}
	}

	protected override void OnLBButtonJustPressed()
	{
	}

	protected override void OnRBButtonJustPressed()
	{
	}

	protected override void OnConfirmButtonJustPressed()
	{
		if (InTimelineEntry)
		{
			m_timelineEntry.OnConfirmButtonPressed();
		}
		else
		{
			base.OnConfirmButtonJustPressed();
		}
	}

	protected override void UpdateScrollArrows(float scrollAmount)
	{
		base.UpdateScrollArrows(scrollAmount);
		if (!base.TopScrollArrow.activeSelf && !base.BottomScrollArrow.activeSelf)
		{
			return;
		}
		float num = (float)base.ActiveEntryArray.Length * m_entryHeight;
		float num2 = Mathf.Clamp(1f - base.ScrollBar.value, 0f, 1f);
		float height = base.ContentViewport.rect.height;
		float num3 = height;
		float num4 = num3 - height;
		if (num2 > 0f)
		{
			num3 = height + (num - height) * num2;
			num4 = num3 - height;
		}
		int num5 = Mathf.CeilToInt(num4 / m_entryHeight);
		int num6 = Mathf.FloorToInt(num3 / m_entryHeight);
		if (base.TopScrollArrow.activeSelf)
		{
			bool flag = false;
			for (int i = 0; i < num5; i++)
			{
				NewGamePlusOmniUIEntry newGamePlusOmniUIEntry = base.ActiveEntryArray[i];
				if (newGamePlusOmniUIEntry.BurdenType != 0 && BurdenManager.GetFoundState(newGamePlusOmniUIEntry.BurdenType) == FoundState.FoundButNotViewed)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				SetGameObjectActive(base.TopScrollNewSymbol.gameObject, active: true);
				SetGameObjectActive(base.TopScrollUpgradeSymbol.gameObject, active: false);
			}
			else
			{
				SetGameObjectActive(base.TopScrollNewSymbol.gameObject, active: false);
				SetGameObjectActive(base.TopScrollUpgradeSymbol.gameObject, active: false);
			}
		}
		if (!base.BottomScrollArrow.activeSelf)
		{
			return;
		}
		bool flag2 = false;
		for (int j = num6; j < base.ActiveEntryArray.Length; j++)
		{
			if (BurdenManager.GetFoundState(base.ActiveEntryArray[j].BurdenType) == FoundState.FoundButNotViewed)
			{
				flag2 = true;
				break;
			}
		}
		if (flag2)
		{
			SetGameObjectActive(base.BottomScrollNewSymbol.gameObject, active: true);
			SetGameObjectActive(base.BottomScrollUpgradeSymbol.gameObject, active: false);
		}
		else
		{
			SetGameObjectActive(base.BottomScrollNewSymbol.gameObject, active: false);
			SetGameObjectActive(base.BottomScrollUpgradeSymbol.gameObject, active: false);
		}
	}

	private void SetGameObjectActive(GameObject obj, bool active)
	{
		if (obj.activeSelf == !active)
		{
			obj.SetActive(active);
		}
	}

	protected override void OnYButtonJustPressed()
	{
		InitializeResetMenu();
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: true);
		SetKeyboardEnabled(enable: false);
		base.OnYButtonJustPressed();
	}

	private void InitializeResetMenu()
	{
		if (!WindowManager.GetIsWindowLoaded(WindowID.ConfirmMenu))
		{
			WindowManager.LoadWindow(WindowID.ConfirmMenu);
		}
		ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenu) as ConfirmMenuWindowController;
		obj.SetTitleText("LOC_ID_SHOP_MENU_UNEQUIP_BURDENS_MENU_TITLE_1", isLocID: true);
		obj.SetDescriptionText("LOC_ID_SHOP_MENU_UNEQUIP_BURDENS_MENU_DESCRIPTION_1", isLocID: true);
		obj.SetNumberOfButtons(2);
		obj.SetOnCancelAction(m_cancelRemoveAllBurdens);
		ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
		buttonAtIndex.SetButtonText("LOC_ID_GENERAL_UI_OK_1", isLocID: true);
		buttonAtIndex.SetOnClickAction(m_confirmRemoveAllBurdens);
		ConfirmMenu_Button buttonAtIndex2 = obj.GetButtonAtIndex(1);
		buttonAtIndex2.SetButtonText("LOC_ID_GENERAL_UI_CANCEL_1", isLocID: true);
		buttonAtIndex2.SetOnClickAction(m_cancelRemoveAllBurdens);
	}

	private void ConfirmRemoveAllBurdens()
	{
		BurdenType[] typeArray = BurdenType_RL.TypeArray;
		foreach (BurdenType burdenType in typeArray)
		{
			if (burdenType != 0)
			{
				BurdenManager.SetBurdenLevel(burdenType, 0, additive: false);
			}
		}
		if (m_descriptionEventArgs == null)
		{
			m_descriptionEventArgs = new NewGamePlusOmniUIDescriptionEventArgs(SelectedBurdenType, OmniUIButtonType.Equipping);
		}
		else
		{
			m_descriptionEventArgs.Initialize(SelectedBurdenType, OmniUIButtonType.Equipping);
		}
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateState, this, null);
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateDescription, this, m_descriptionEventArgs);
		SetTimelineEntryActive(active: true, playSFX: false, usingMouse: false);
		CancelRemoveAllBurdens();
	}

	private void CancelRemoveAllBurdens()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
		SetKeyboardEnabled(enable: true);
	}

	protected override void OnCancelButtonJustPressed()
	{
		bool flag = false;
		foreach (KeyValuePair<BurdenType, int> storedBurdenValue in m_storedBurdenValues)
		{
			if (BurdenManager.GetBurdenLevel(storedBurdenValue.Key) != storedBurdenValue.Value)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			InitializeQuitMenu();
			WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: true);
			SetKeyboardEnabled(enable: false);
		}
		else
		{
			base.OnCancelButtonJustPressed();
		}
	}

	private void InitializeQuitMenu()
	{
		if (!WindowManager.GetIsWindowLoaded(WindowID.ConfirmMenu))
		{
			WindowManager.LoadWindow(WindowID.ConfirmMenu);
		}
		ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenu) as ConfirmMenuWindowController;
		obj.SetTitleText("LOC_ID_NEWGAMEPLUS_EXIT_MENU_CONFIRM_TITLE_1", isLocID: true);
		obj.SetDescriptionText("LOC_ID_NEWGAMEPLUS_EXIT_MENU_CONFIRM_DESCRIPTION_1", isLocID: true);
		obj.SetNumberOfButtons(2);
		obj.SetOnCancelAction(m_cancelRemoveAllBurdens);
		ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
		buttonAtIndex.SetButtonText("LOC_ID_GENERAL_UI_OK_1", isLocID: true);
		buttonAtIndex.SetOnClickAction(m_quitMenu);
		ConfirmMenu_Button buttonAtIndex2 = obj.GetButtonAtIndex(1);
		buttonAtIndex2.SetButtonText("LOC_ID_GENERAL_UI_CANCEL_1", isLocID: true);
		buttonAtIndex2.SetOnClickAction(m_cancelRemoveAllBurdens);
	}

	private void QuitMenu()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
		SetKeyboardEnabled(enable: true);
		base.OnCancelButtonJustPressed();
	}
}
