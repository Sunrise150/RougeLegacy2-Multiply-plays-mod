using System;
using RL_Windows;
using TMPro;
using UnityEngine;

public class EnchantressOmniUIWindowController : BaseOmniUIWindowController<BaseOmniUICategoryEntry, EnchantressOmniUIEntry>
{
	[SerializeField]
	private TMP_Text m_loadoutText;

	private Action m_confirmUnequipAllRunes;

	private Action m_cancelUnequipAllRunes;

	private Action m_confirmToggleLoadouts;

	private Action m_cancelToggleLoadouts;

	private EnchantressOmniUIDescriptionEventArgs m_descriptionEventArgs;

	public override bool CanReset => true;

	public override bool CanExit
	{
		get
		{
			int num = 999;
			if (PlayerManager.IsInstantiated)
			{
				num = PlayerManager.GetPlayerController().ActualRuneWeight;
			}
			return RuneManager.GetTotalEquippedWeight() <= num;
		}
	}

	public RuneType SelectedRuneType
	{
		get
		{
			if (base.IsInitialized)
			{
				return base.ActiveEntryArray[base.SelectedEntryIndex].RuneType;
			}
			return RuneType.None;
		}
	}

	public override WindowID ID => WindowID.Enchantress;

	protected override void Awake()
	{
		base.Awake();
		m_confirmUnequipAllRunes = ConfirmUnequipAllRunes;
		m_cancelUnequipAllRunes = CancelUnequipAllRunes;
		m_confirmToggleLoadouts = ConfirmToggleLoadouts;
		m_cancelToggleLoadouts = CancelToggleLoadouts;
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
		RuneType[] typeArray = RuneType_RL.TypeArray;
		base.EntryArray = new EnchantressOmniUIEntry[typeArray.Length - 1];
		int num = 0;
		RuneType[] array = typeArray;
		foreach (RuneType runeType in array)
		{
			if (runeType != 0)
			{
				base.EntryArray[num] = UnityEngine.Object.Instantiate(m_entryPrefab);
				base.EntryArray[num].transform.SetParent(base.EntryLayoutGroup.transform);
				base.EntryArray[num].transform.localScale = Vector3.one;
				base.EntryArray[num].Initialize(runeType, this);
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
				EnchantressOmniUIEntry enchantressOmniUIEntry = base.ActiveEntryArray[i];
				if (RuneManager.GetFoundState(enchantressOmniUIEntry.RuneType) == FoundState.FoundButNotViewed)
				{
					flag = true;
					break;
				}
				if (RuneManager.CanPurchaseRune(enchantressOmniUIEntry.RuneType))
				{
					flag2 = true;
				}
			}
			if (flag)
			{
				SetGameObjectActive(base.TopScrollNewSymbol.gameObject, active: true);
				SetGameObjectActive(base.TopScrollUpgradeSymbol.gameObject, active: false);
			}
			else if (flag2)
			{
				SetGameObjectActive(base.TopScrollNewSymbol.gameObject, active: false);
				SetGameObjectActive(base.TopScrollUpgradeSymbol.gameObject, active: true);
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
		bool flag3 = false;
		bool flag4 = false;
		for (int j = num6; j < base.ActiveEntryArray.Length; j++)
		{
			EnchantressOmniUIEntry enchantressOmniUIEntry2 = base.ActiveEntryArray[j];
			if (RuneManager.GetFoundState(enchantressOmniUIEntry2.RuneType) == FoundState.FoundButNotViewed)
			{
				flag3 = true;
				break;
			}
			if (RuneManager.CanPurchaseRune(enchantressOmniUIEntry2.RuneType))
			{
				flag4 = true;
			}
		}
		if (flag3)
		{
			SetGameObjectActive(base.BottomScrollNewSymbol.gameObject, active: true);
			SetGameObjectActive(base.BottomScrollUpgradeSymbol.gameObject, active: false);
		}
		else if (flag4)
		{
			SetGameObjectActive(base.BottomScrollNewSymbol.gameObject, active: false);
			SetGameObjectActive(base.BottomScrollUpgradeSymbol.gameObject, active: true);
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

	protected override void OnOpen()
	{
		base.OnOpen();
		if (SaveManager.EquipmentSaveData.RuneLoadoutEnabled)
		{
			m_loadoutText.text = LocalizationManager.GetString("LOC_ID_SHOP_MENU_CLASS_RUNES_ON_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
		else
		{
			m_loadoutText.text = LocalizationManager.GetString("LOC_ID_SHOP_MENU_CLASS_RUNES_OFF_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
		if (HasAllRunes())
		{
			StoreAPIManager.GiveAchievement(AchievementType.AllRunes, StoreType.All);
		}
	}

	public bool HasAllRunes()
	{
		bool result = true;
		EnchantressOmniUIEntry[] activeEntryArray = base.ActiveEntryArray;
		for (int i = 0; i < activeEntryArray.Length; i++)
		{
			if (RuneManager.GetFoundState(activeEntryArray[i].RuneType) < FoundState.Purchased)
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
			if (SaveManager.EquipmentSaveData.RuneLoadoutEnabled)
			{
				SaveManager.EquipmentSaveData.GetRuneLoadout(playerController.CharacterClass.ClassType)?.SaveLoadout();
			}
		}
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.Enchantress_Shop_Closed, this, null);
		SaveManager.PlayerSaveData.UpdateCachedData();
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
		obj.SetTitleText("LOC_ID_SHOP_MENU_UNEQUIP_RUNES_MENU_TITLE_1", isLocID: true);
		obj.SetDescriptionText("LOC_ID_SHOP_MENU_UNEQUIP_RUNES_MENU_DESCRIPTION_1", isLocID: true);
		obj.SetNumberOfButtons(2);
		obj.SetOnCancelAction(m_cancelUnequipAllRunes);
		ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
		buttonAtIndex.SetButtonText("LOC_ID_GENERAL_UI_OK_1", isLocID: true);
		buttonAtIndex.SetOnClickAction(m_confirmUnequipAllRunes);
		ConfirmMenu_Button buttonAtIndex2 = obj.GetButtonAtIndex(1);
		buttonAtIndex2.SetButtonText("LOC_ID_GENERAL_UI_CANCEL_1", isLocID: true);
		buttonAtIndex2.SetOnClickAction(m_cancelUnequipAllRunes);
	}

	private void ConfirmUnequipAllRunes()
	{
		RuneType[] typeArray = RuneType_RL.TypeArray;
		foreach (RuneType runeType in typeArray)
		{
			if (runeType != 0)
			{
				RuneManager.SetRuneEquippedLevel(runeType, 0, additive: false);
			}
		}
		if (m_descriptionEventArgs == null)
		{
			m_descriptionEventArgs = new EnchantressOmniUIDescriptionEventArgs(SelectedRuneType, OmniUIButtonType.Equipping);
		}
		else
		{
			m_descriptionEventArgs.Initialize(SelectedRuneType, OmniUIButtonType.Equipping);
		}
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateDescription, this, m_descriptionEventArgs);
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateState, this, null);
		int selectedEntryIndex = base.SelectedEntryIndex;
		base.ActiveEntryArray[selectedEntryIndex].OnDeselect(null);
		base.SelectedEntryIndex = -1;
		SetSelectedEntryIndex(selectedEntryIndex, playAudio: false, usingMouse: false);
		CancelUnequipAllRunes();
	}

	private void CancelUnequipAllRunes()
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
		if (!SaveManager.EquipmentSaveData.RuneLoadoutEnabled)
		{
			confirmMenuWindowController.SetTitleText("LOC_ID_SHOP_MENU_CLASS_RUNES_POPUP_ON_TITLE_1", isLocID: true);
			confirmMenuWindowController.SetDescriptionText("LOC_ID_SHOP_MENU_CLASS_RUNES_POPUP_ON_TEXT_1", isLocID: true);
		}
		else
		{
			confirmMenuWindowController.SetTitleText("LOC_ID_SHOP_MENU_CLASS_RUNES_POPUP_OFF_TITLE_1", isLocID: true);
			confirmMenuWindowController.SetDescriptionText("LOC_ID_SHOP_MENU_CLASS_RUNES_POPUP_OFF_TEXT_1", isLocID: true);
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
		if (!SaveManager.EquipmentSaveData.RuneLoadoutEnabled)
		{
			m_loadoutText.text = LocalizationManager.GetString("LOC_ID_SHOP_MENU_CLASS_RUNES_ON_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
		else
		{
			m_loadoutText.text = LocalizationManager.GetString("LOC_ID_SHOP_MENU_CLASS_RUNES_OFF_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
		SaveManager.EquipmentSaveData.RuneLoadoutEnabled = !SaveManager.EquipmentSaveData.RuneLoadoutEnabled;
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
		if (!SaveManager.EquipmentSaveData.RuneLoadoutEnabled)
		{
			m_loadoutText.text = LocalizationManager.GetString("LOC_ID_SHOP_MENU_CLASS_RUNES_ON_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
		else
		{
			m_loadoutText.text = LocalizationManager.GetString("LOC_ID_SHOP_MENU_CLASS_RUNES_OFF_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
	}
}
