using System;
using RL_Windows;
using UnityEngine;

public class SoulShopOmniUISoulSwapBuyButton : SoulShopOmniUIBuyButton
{
	private Action m_cancelConfirmMenu;

	private Action m_confirmTransfer;

	protected override void Awake()
	{
		base.Awake();
		m_cancelConfirmMenu = CancelConfirmMenu;
		m_confirmTransfer = ConfirmTransfer;
	}

	public override void OnConfirmButtonPressed()
	{
		if (IsButtonActive)
		{
			if (base.gameObject.activeSelf)
			{
				BaseOmniUIEntry.StaticSelectedButtonIndex = base.ButtonIndex;
			}
			m_onConfirmPressedRelay.Dispatch(this);
			int soulSwapCost = Souls_EV.GetSoulSwapCost(SaveManager.ModeSaveData.GetSoulShopObj(base.SoulShopType).CurrentOwnedLevel);
			if (SaveManager.PlayerSaveData.EquipmentOreCollected >= soulSwapCost && SaveManager.PlayerSaveData.RuneOreCollected >= soulSwapCost)
			{
				InitializeConfirmMenu();
				WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: true);
			}
			else
			{
				m_purchaseDialogueArgs.Initialize(PurchaseBoxDialogueType.PurchaseFailed_NoMoney);
				Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.UpdatePurchaseBoxDialogue, this, m_purchaseDialogueArgs);
				StartCoroutine(ShakeAnimCoroutine());
			}
			RunOnConfirmPressedAnimation();
		}
	}

	private void InitializeConfirmMenu()
	{
		int soulSwapCost = Souls_EV.GetSoulSwapCost(SaveManager.ModeSaveData.GetSoulShopObj(base.SoulShopType).CurrentOwnedLevel);
		int num = 150;
		if (!WindowManager.GetIsWindowLoaded(WindowID.ConfirmMenu))
		{
			WindowManager.LoadWindow(WindowID.ConfirmMenu);
		}
		ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenu) as ConfirmMenuWindowController;
		obj.SetTitleText("LOC_ID_SOULSHOP_BUY_SOUL_STONES_TITLE_1", isLocID: true);
		string text = string.Format(LocalizationManager.GetString("LOC_ID_SOULSHOP_BUY_SOUL_STONES_DESCRIPTION_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), soulSwapCost, num);
		obj.SetDescriptionText(text, isLocID: false);
		obj.SetNumberOfButtons(2);
		obj.SetOnCancelAction(m_cancelConfirmMenu);
		ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
		buttonAtIndex.SetButtonText("LOC_ID_GENERAL_UI_YES_1", isLocID: true);
		buttonAtIndex.SetOnClickAction(m_confirmTransfer);
		ConfirmMenu_Button buttonAtIndex2 = obj.GetButtonAtIndex(1);
		buttonAtIndex2.SetButtonText("LOC_ID_GENERAL_UI_NO_1", isLocID: true);
		buttonAtIndex2.SetOnClickAction(m_cancelConfirmMenu);
	}

	private void ConfirmTransfer()
	{
		Vector3 vector = Vector3.up * 6f;
		SoulShopObj soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj(base.SoulShopType);
		int soulSwapCost = Souls_EV.GetSoulSwapCost(soulShopObj.CurrentOwnedLevel);
		int num = 150;
		SaveManager.PlayerSaveData.RuneOreCollected -= soulSwapCost;
		SaveManager.PlayerSaveData.EquipmentOreCollected -= soulSwapCost;
		SaveManager.ModeSaveData.SoulSwapResourcesSpent += soulSwapCost;
		SaveManager.ModeSaveData.MiscSoulCollected += num;
		soulShopObj.SetOwnedLevel(1, additive: true, clampToMaxLevel: false);
		string text = $"<color=red>-{soulSwapCost} [RuneOre_Icon] [EquipmentOre_Icon]</color>\n<color=green>+{num} [Soul_Icon]</color>";
		TextPopupManager.DisplayTextAtAbsPos(TextPopupType.PlayerRankUp, text, base.ParentEntry.WindowController.CommonFields.PlayerModel.transform.position + vector);
		InitializeButtonEventArgs();
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateDescription, this, ButtonEventArgs);
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateState, this, ButtonEventArgs);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.EquipmentOreChanged, this, null);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.RuneOreChanged, this, null);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.SoulChanged, this, null);
		m_purchaseDialogueArgs.Initialize(PurchaseBoxDialogueType.PurchaseSuccessful);
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.UpdatePurchaseBoxDialogue, this, m_purchaseDialogueArgs);
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
	}

	private void CancelConfirmMenu()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
	}

	public override void UpdateState()
	{
		m_titleText.text = LocalizationManager.GetString("LOC_ID_BLACKSMITH_BUY_TITLE_1", isFemale: false);
		SoulShopObj soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj(base.SoulShopType);
		if (!base.ParentEntry.IsUnlocked || soulShopObj.CurrentOwnedLevel >= soulShopObj.MaxLevel)
		{
			if (base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: false);
			}
			IsButtonActive = false;
			return;
		}
		int soulSwapCost = Souls_EV.GetSoulSwapCost(soulShopObj.CurrentOwnedLevel);
		if (soulSwapCost > SaveManager.PlayerSaveData.EquipmentOreCollected || soulSwapCost > SaveManager.PlayerSaveData.RuneOreCollected)
		{
			m_deselectedSprite.SetAlpha(0.25f);
			IsButtonActive = false;
		}
		else
		{
			m_deselectedSprite.SetAlpha(1f);
			IsButtonActive = true;
		}
	}
}
