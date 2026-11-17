using System;
using System.Collections.Generic;
using RL_Windows;
using TMPro;
using UnityEngine;

public class BlacksmithOmniUIWindowController : BaseOmniUIWindowController<BlacksmithOmniUICategoryEntry, BlacksmithOmniUIEntry>
{
	[SerializeField]
	private GameObject m_unityDownTextPosition;

	[SerializeField]
	private TMP_Text m_loadoutText;

	private Action m_cancelUnequipAllGear;

	private Action m_confirmUnequipAllGear;

	private Action m_confirmToggleLoadouts;

	private Action m_cancelToggleLoadouts;

	private static List<EquipmentObj> m_achievementHelper_STATIC = new List<EquipmentObj>();

	private BlacksmithOmniUIDescriptionEventArgs m_descriptionEventArgs;

	public override bool CanReset => true;

	public Vector3 UnityDownTextPosition
	{
		get
		{
			if ((bool)m_unityDownTextPosition)
			{
				return m_unityDownTextPosition.transform.position;
			}
			return Vector3.zero;
		}
	}

	public override bool CanExit
	{
		get
		{
			int num = 999;
			if (PlayerManager.IsInstantiated)
			{
				num = PlayerManager.GetPlayerController().ActualAllowedEquipmentWeight;
			}
			return (int)EquipmentManager.GetTotalEquippedStatValue(EquipmentStatType.Weight) <= num;
		}
	}

	public EquipmentCategoryType HighlightedCategory
	{
		get
		{
			if (base.IsInitialized && base.HighlightedCategoryIndex != -1)
			{
				return base.ActiveCategoryEntryArray[base.HighlightedCategoryIndex].CategoryType;
			}
			return EquipmentCategoryType.None;
		}
	}

	public EquipmentType SelectedEquipmentType
	{
		get
		{
			if (base.IsInitialized && base.SelectedEntryIndex != -1)
			{
				return base.ActiveEntryArray[base.SelectedEntryIndex].EquipmentType;
			}
			return EquipmentType.None;
		}
	}

	public override WindowID ID => WindowID.Blacksmith;

	protected override void Awake()
	{
		base.Awake();
		m_confirmUnequipAllGear = ConfirmUnequipAllGear;
		m_cancelUnequipAllGear = CancelUnequipAllGear;
		m_confirmToggleLoadouts = ConfirmToggleLoadouts;
		m_cancelToggleLoadouts = CancelToggleLoadouts;
	}

	protected override void CreateCategoryEntries()
	{
		if (base.CategoryEntryArray != null)
		{
			Array.Clear(base.CategoryEntryArray, 0, base.CategoryEntryArray.Length);
			base.CategoryEntryArray = null;
		}
		Array categoryTypeArray = EquipmentType_RL.CategoryTypeArray;
		base.CategoryEntryArray = new BlacksmithOmniUICategoryEntry[categoryTypeArray.Length - 1];
		int num = 0;
		foreach (EquipmentCategoryType item in categoryTypeArray)
		{
			if (item != 0)
			{
				base.CategoryEntryArray[num] = UnityEngine.Object.Instantiate(m_categoryEntryPrefab);
				base.CategoryEntryArray[num].transform.SetParent(base.CategoryEntryLayoutGroup.transform);
				base.CategoryEntryArray[num].transform.localScale = Vector3.one;
				base.CategoryEntryArray[num].Initialize(item, num, this);
				num++;
			}
		}
	}

	protected override void CreateEntries()
	{
		if (base.EntryArray != null)
		{
			Array.Clear(base.EntryArray, 0, base.EntryArray.Length);
			base.EntryArray = null;
		}
		Array typeArray = EquipmentType_RL.TypeArray;
		base.EntryArray = new BlacksmithOmniUIEntry[typeArray.Length - 1];
		int num = 0;
		foreach (EquipmentType item in typeArray)
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
			bool flag2 = false;
			for (int i = 0; i < num5; i++)
			{
				BlacksmithOmniUIEntry blacksmithOmniUIEntry = base.ActiveEntryArray[i];
				if (EquipmentManager.GetFoundState(HighlightedCategory, blacksmithOmniUIEntry.EquipmentType) == FoundState.FoundButNotViewed)
				{
					flag = true;
					break;
				}
				if (EquipmentManager.CanPurchaseEquipment(HighlightedCategory, blacksmithOmniUIEntry.EquipmentType))
				{
					flag2 = true;
				}
			}
			if (flag)
			{
				if (!base.TopScrollNewSymbol.gameObject.activeSelf)
				{
					base.TopScrollNewSymbol.gameObject.SetActive(value: true);
				}
				if (base.TopScrollUpgradeSymbol.gameObject.activeSelf)
				{
					base.TopScrollUpgradeSymbol.gameObject.SetActive(value: false);
				}
			}
			else if (flag2)
			{
				if (base.TopScrollNewSymbol.gameObject.activeSelf)
				{
					base.TopScrollNewSymbol.gameObject.SetActive(value: false);
				}
				if (!base.TopScrollUpgradeSymbol.gameObject.activeSelf)
				{
					base.TopScrollUpgradeSymbol.gameObject.SetActive(value: true);
				}
			}
			else
			{
				if (base.TopScrollNewSymbol.gameObject.activeSelf)
				{
					base.TopScrollNewSymbol.gameObject.SetActive(value: false);
				}
				if (base.TopScrollUpgradeSymbol.gameObject.activeSelf)
				{
					base.TopScrollUpgradeSymbol.gameObject.SetActive(value: false);
				}
			}
		}
		if (!base.BottomScrollArrow.activeSelf)
		{
			return;
		}
		bool flag3 = false;
		bool flag4 = false;
		for (int j = num6; j < base.ActiveEntryArray.Length; j++)
		{
			BlacksmithOmniUIEntry blacksmithOmniUIEntry2 = base.ActiveEntryArray[j];
			if (EquipmentManager.GetFoundState(HighlightedCategory, blacksmithOmniUIEntry2.EquipmentType) == FoundState.FoundButNotViewed)
			{
				flag3 = true;
				break;
			}
			if (EquipmentManager.CanPurchaseEquipment(HighlightedCategory, blacksmithOmniUIEntry2.EquipmentType))
			{
				flag4 = true;
			}
		}
		if (flag3)
		{
			if (!base.BottomScrollNewSymbol.gameObject.activeSelf)
			{
				base.BottomScrollNewSymbol.gameObject.SetActive(value: true);
			}
			if (base.BottomScrollUpgradeSymbol.gameObject.activeSelf)
			{
				base.BottomScrollUpgradeSymbol.gameObject.SetActive(value: false);
			}
		}
		else if (flag4)
		{
			if (base.BottomScrollNewSymbol.gameObject.activeSelf)
			{
				base.BottomScrollNewSymbol.gameObject.SetActive(value: false);
			}
			if (!base.BottomScrollUpgradeSymbol.gameObject.activeSelf)
			{
				base.BottomScrollUpgradeSymbol.gameObject.SetActive(value: true);
			}
		}
		else
		{
			if (base.BottomScrollNewSymbol.gameObject.activeSelf)
			{
				base.BottomScrollNewSymbol.gameObject.SetActive(value: false);
			}
			if (base.BottomScrollUpgradeSymbol.gameObject.activeSelf)
			{
				base.BottomScrollUpgradeSymbol.gameObject.SetActive(value: false);
			}
		}
	}

	protected override void OnConfirmButtonJustPressed()
	{
		base.OnConfirmButtonJustPressed();
		if (!m_warningMessageVisible && !base.IsInCategories && base.ActiveEntryArray.Length == 0)
		{
			base.ChooseCategoryText.text = LocalizationManager.GetString("LOC_ID_BLACKSMITH_NO_BLUEPRINTS_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
	}

	protected override void OnCancelButtonJustPressed()
	{
		base.OnCancelButtonJustPressed();
		if (!m_warningMessageVisible && base.IsInCategories)
		{
			base.ChooseCategoryText.text = LocalizationManager.GetString("LOC_ID_BLACKSMITH_SELECT_CATEGORY_1", isFemale: false);
		}
	}

	protected override void OnOpen()
	{
		base.ChooseCategoryText.text = LocalizationManager.GetString("LOC_ID_BLACKSMITH_SELECT_CATEGORY_1", isFemale: false);
		if (SaveManager.EquipmentSaveData.EquipmentLoadoutEnabled)
		{
			m_loadoutText.text = LocalizationManager.GetString("LOC_ID_SHOP_MENU_CLASS_GEAR_ON_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
		else
		{
			m_loadoutText.text = LocalizationManager.GetString("LOC_ID_SHOP_MENU_CLASS_GEAR_OFF_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
		if (HasAllEquipment())
		{
			StoreAPIManager.GiveAchievement(AchievementType.AllEquipment, StoreType.All);
		}
		EquipmentType[] typeArray = EquipmentType_RL.TypeArray;
		foreach (EquipmentType equipmentType in typeArray)
		{
			if (equipmentType != 0)
			{
				EquipmentSetData equipmentSetData = EquipmentSetLibrary.GetEquipmentSetData(equipmentType);
				if (equipmentSetData != null && EquipmentManager.Get_EquipmentSet_CurrentLevel(equipmentType) >= equipmentSetData.SetRequirement03)
				{
					StoreAPIManager.GiveAchievement(AchievementType.UnlockHighestUnity, StoreType.All);
					break;
				}
			}
		}
		base.OnOpen();
	}

	public bool HasAllEquipment()
	{
		EquipmentManager.GetTotalAvailableBlueprints(m_achievementHelper_STATIC);
		bool result = true;
		foreach (EquipmentObj item in m_achievementHelper_STATIC)
		{
			if (EquipmentManager.GetFoundState(item.CategoryType, item.EquipmentType) < FoundState.Purchased)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	protected override void OnClose()
	{
		base.OnClose();
		if (PlayerManager.IsInstantiated)
		{
			PlayerController playerController = PlayerManager.GetPlayerController();
			playerController.ResetBaseValues();
			playerController.RecreateRendererArray();
			playerController.ResetRendererArrayColor();
			playerController.BlinkPulseEffect.ResetAllBlackFills();
			if (SaveManager.EquipmentSaveData.EquipmentLoadoutEnabled)
			{
				SaveManager.EquipmentSaveData.GetEquipmentLoadout(playerController.CharacterClass.ClassType)?.SaveLoadout(SaveManager.PlayerSaveData.CurrentCharacter);
			}
		}
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.Blacksmith_Shop_Closed, this, null);
		SaveManager.PlayerSaveData.UpdateCachedData();
	}

	protected override int GetEquippedIndex()
	{
		EquipmentObj equipped = EquipmentManager.GetEquipped(HighlightedCategory);
		if (equipped != null)
		{
			for (int i = 0; i < base.ActiveEntryArray.Length; i++)
			{
				if (base.ActiveEntryArray[i].EquipmentType == equipped.EquipmentType)
				{
					return i;
				}
			}
		}
		return base.GetEquippedIndex();
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
		obj.SetTitleText("LOC_ID_SHOP_MENU_UNEQUIP_GEAR_MENU_TITLE_1", isLocID: true);
		obj.SetDescriptionText("LOC_ID_SHOP_MENU_UNEQUIP_GEAR_MENU_DESCRIPTION_1", isLocID: true);
		obj.SetNumberOfButtons(2);
		obj.SetOnCancelAction(m_cancelUnequipAllGear);
		ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
		buttonAtIndex.SetButtonText("LOC_ID_GENERAL_UI_OK_1", isLocID: true);
		buttonAtIndex.SetOnClickAction(m_confirmUnequipAllGear);
		ConfirmMenu_Button buttonAtIndex2 = obj.GetButtonAtIndex(1);
		buttonAtIndex2.SetButtonText("LOC_ID_GENERAL_UI_CANCEL_1", isLocID: true);
		buttonAtIndex2.SetOnClickAction(m_cancelUnequipAllGear);
	}

	private void ConfirmUnequipAllGear()
	{
		BlacksmithOmniUIEquipButton.SetEquipped(EquipmentCategoryType.Head, EquipmentType.None);
		BlacksmithOmniUIEquipButton.SetEquipped(EquipmentCategoryType.Chest, EquipmentType.None);
		BlacksmithOmniUIEquipButton.SetEquipped(EquipmentCategoryType.Weapon, EquipmentType.None);
		BlacksmithOmniUIEquipButton.SetEquipped(EquipmentCategoryType.Cape, EquipmentType.None);
		BlacksmithOmniUIEquipButton.SetEquipped(EquipmentCategoryType.Trinket, EquipmentType.None);
		if (m_descriptionEventArgs == null)
		{
			m_descriptionEventArgs = new BlacksmithOmniUIDescriptionEventArgs(HighlightedCategory, SelectedEquipmentType, OmniUIButtonType.Equipping);
		}
		else
		{
			m_descriptionEventArgs.Initialize(HighlightedCategory, SelectedEquipmentType, OmniUIButtonType.Equipping);
		}
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateState, this, null);
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateDescription, this, m_descriptionEventArgs);
		int selectedEntryIndex = base.SelectedEntryIndex;
		base.ActiveEntryArray[selectedEntryIndex].OnDeselect(null);
		base.SelectedEntryIndex = -1;
		SetSelectedEntryIndex(selectedEntryIndex, playAudio: false, usingMouse: false);
		CancelUnequipAllGear();
	}

	private void CancelUnequipAllGear()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
		SetKeyboardEnabled(enable: true);
	}

	protected override void OnXButtonJustPressed()
	{
		InitializeLoadoutMenu();
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenuBig, isOpen: true);
		SetKeyboardEnabled(enable: false);
		base.OnYButtonJustPressed();
	}

	private void InitializeLoadoutMenu()
	{
		if (!WindowManager.GetIsWindowLoaded(WindowID.ConfirmMenuBig))
		{
			WindowManager.LoadWindow(WindowID.ConfirmMenuBig);
		}
		ConfirmMenuWindowController confirmMenuWindowController = WindowManager.GetWindowController(WindowID.ConfirmMenuBig) as ConfirmMenuWindowController;
		if (!SaveManager.EquipmentSaveData.EquipmentLoadoutEnabled)
		{
			confirmMenuWindowController.SetTitleText("LOC_ID_SHOP_MENU_CLASS_GEAR_POPUP_ON_TITLE_1", isLocID: true);
			confirmMenuWindowController.SetDescriptionText("LOC_ID_SHOP_MENU_CLASS_GEAR_POPUP_ON_TEXT_1", isLocID: true);
		}
		else
		{
			confirmMenuWindowController.SetTitleText("LOC_ID_SHOP_MENU_CLASS_GEAR_POPUP_OFF_TITLE_1", isLocID: true);
			confirmMenuWindowController.SetDescriptionText("LOC_ID_SHOP_MENU_CLASS_GEAR_POPUP_OFF_TEXT_1", isLocID: true);
		}
		confirmMenuWindowController.SetNumberOfButtons(2);
		confirmMenuWindowController.SetOnCancelAction(m_cancelToggleLoadouts);
		ConfirmMenu_Button buttonAtIndex = confirmMenuWindowController.GetButtonAtIndex(0);
		buttonAtIndex.SetButtonText("LOC_ID_GENERAL_UI_OK_1", isLocID: true);
		buttonAtIndex.SetOnClickAction(m_confirmToggleLoadouts);
		ConfirmMenu_Button buttonAtIndex2 = confirmMenuWindowController.GetButtonAtIndex(1);
		buttonAtIndex2.SetButtonText("LOC_ID_GENERAL_UI_CANCEL_1", isLocID: true);
		buttonAtIndex2.SetOnClickAction(m_cancelToggleLoadouts);
	}

	private void ConfirmToggleLoadouts()
	{
		if (!SaveManager.EquipmentSaveData.EquipmentLoadoutEnabled)
		{
			m_loadoutText.text = LocalizationManager.GetString("LOC_ID_SHOP_MENU_CLASS_GEAR_ON_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
		else
		{
			m_loadoutText.text = LocalizationManager.GetString("LOC_ID_SHOP_MENU_CLASS_GEAR_OFF_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
		SaveManager.EquipmentSaveData.EquipmentLoadoutEnabled = !SaveManager.EquipmentSaveData.EquipmentLoadoutEnabled;
		CancelToggleLoadouts();
	}

	private void CancelToggleLoadouts()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenuBig, isOpen: false);
		SetKeyboardEnabled(enable: true);
	}

	protected override void RefreshText(object sender, EventArgs args)
	{
		base.RefreshText(sender, args);
		if (!SaveManager.EquipmentSaveData.EquipmentLoadoutEnabled)
		{
			m_loadoutText.text = LocalizationManager.GetString("LOC_ID_SHOP_MENU_CLASS_GEAR_ON_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
		else
		{
			m_loadoutText.text = LocalizationManager.GetString("LOC_ID_SHOP_MENU_CLASS_GEAR_OFF_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
	}
}
