using System;
using RL_Windows;
using UnityEngine;

public class SoulShopOmniUITransferResourceButton : OmniUIButton, ISoulShopOmniUIButton
{
	[SerializeField]
	private bool m_soulSwap;

	[SerializeField]
	[ConditionalHide("m_soulSwap", true, Inverse = true)]
	private bool m_aetherToOre;

	private SoulShopOmniUIDescriptionEventArgs m_descriptionEventArgs;

	private PurchaseBoxDialogueEventArgs m_purchaseDialogueArgs = new PurchaseBoxDialogueEventArgs(PurchaseBoxDialogueType.Welcome);

	private Action m_cancelConfirmMenu;

	private Action m_confirmTransfer;

	public override EventArgs ButtonEventArgs => m_descriptionEventArgs;

	public SoulShopType SoulShopType { get; set; }

	public SoulShopOmniUIEntry ParentEntry { get; set; }

	protected override void Awake()
	{
		base.Awake();
		m_cancelConfirmMenu = CancelConfirmMenu;
		m_confirmTransfer = ConfirmTransfer;
	}

	protected override void InitializeButtonEventArgs()
	{
		if (m_descriptionEventArgs == null)
		{
			m_descriptionEventArgs = new SoulShopOmniUIDescriptionEventArgs(SoulShopType, OmniUIButtonType.Purchasing);
		}
		else
		{
			m_descriptionEventArgs.Initialize(SoulShopType, OmniUIButtonType.Purchasing);
		}
	}

	public override void OnConfirmButtonPressed()
	{
		if (IsButtonActive)
		{
			base.OnConfirmButtonPressed();
			InitializeConfirmMenu();
			WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: true);
			RunOnConfirmPressedAnimation();
		}
	}

	public override void UpdateState()
	{
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
		}
		if (m_soulSwap)
		{
			if (SoulShopOmniUIIncrementResourceButton.SoulTransferLevel <= 0 || Souls_EV.GetSoulSwapCost(SoulShopOmniUIIncrementResourceButton.SoulTransferLevel) > SaveManager.PlayerSaveData.EquipmentOreCollected || Souls_EV.GetSoulSwapCost(SoulShopOmniUIIncrementResourceButton.SoulTransferLevel) > SaveManager.PlayerSaveData.RuneOreCollected)
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
		else if ((!m_aetherToOre && (SoulShopOmniUIIncrementResourceButton.OreTransferAmount <= 0 || SoulShopOmniUIIncrementResourceButton.OreTransferAmount > SaveManager.PlayerSaveData.EquipmentOreCollected)) || (m_aetherToOre && (SoulShopOmniUIIncrementResourceButton.AetherTransferAmount <= 0 || SoulShopOmniUIIncrementResourceButton.AetherTransferAmount > SaveManager.PlayerSaveData.RuneOreCollected)))
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

	private void InitializeConfirmMenu()
	{
		if (!WindowManager.GetIsWindowLoaded(WindowID.ConfirmMenu))
		{
			WindowManager.LoadWindow(WindowID.ConfirmMenu);
		}
		ConfirmMenuWindowController confirmMenuWindowController = WindowManager.GetWindowController(WindowID.ConfirmMenu) as ConfirmMenuWindowController;
		if (m_soulSwap)
		{
			confirmMenuWindowController.SetTitleText("LOC_ID_SOULSHOP_BUY_SOUL_STONES_TITLE_1", isLocID: true);
			string text = string.Format(LocalizationManager.GetString("LOC_ID_SOULSHOP_BUY_SOUL_STONES_DESCRIPTION_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), Souls_EV.GetSoulSwapCost(SoulShopOmniUIIncrementResourceButton.SoulTransferLevel), SoulShopOmniUIIncrementResourceButton.SoulTransferLevel * 150);
			confirmMenuWindowController.SetDescriptionText(text, isLocID: false);
		}
		else if (!m_aetherToOre)
		{
			confirmMenuWindowController.SetTitleText("LOC_ID_SOULSHOP_CONVERT_ORE_TO_AETHER_TITLE_1", isLocID: true);
			string text2 = string.Format(LocalizationManager.GetString("LOC_ID_SOULSHOP_CONVERT_ORE_TO_AETHER_DESCRIPTION_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), SoulShopOmniUIIncrementResourceButton.OreTransferAmount, Mathf.RoundToInt(SoulShopOmniUIIncrementResourceButton.OreTransferAmount / 2));
			confirmMenuWindowController.SetDescriptionText(text2, isLocID: false);
		}
		else
		{
			confirmMenuWindowController.SetTitleText("LOC_ID_SOULSHOP_CONVERT_AETHER_TO_ORE_TITLE_1", isLocID: true);
			string text3 = string.Format(LocalizationManager.GetString("LOC_ID_SOULSHOP_CONVERT_AETHER_TO_ORE_DESCRIPTION_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), SoulShopOmniUIIncrementResourceButton.AetherTransferAmount, Mathf.RoundToInt(SoulShopOmniUIIncrementResourceButton.AetherTransferAmount / 2));
			confirmMenuWindowController.SetDescriptionText(text3, isLocID: false);
		}
		confirmMenuWindowController.SetNumberOfButtons(2);
		confirmMenuWindowController.SetOnCancelAction(m_cancelConfirmMenu);
		ConfirmMenu_Button buttonAtIndex = confirmMenuWindowController.GetButtonAtIndex(0);
		buttonAtIndex.SetButtonText("LOC_ID_GENERAL_UI_YES_1", isLocID: true);
		buttonAtIndex.SetOnClickAction(m_confirmTransfer);
		ConfirmMenu_Button buttonAtIndex2 = confirmMenuWindowController.GetButtonAtIndex(1);
		buttonAtIndex2.SetButtonText("LOC_ID_GENERAL_UI_NO_1", isLocID: true);
		buttonAtIndex2.SetOnClickAction(m_cancelConfirmMenu);
	}

	private void ConfirmTransfer()
	{
		Vector3 vector = Vector3.up * 6f;
		if (m_soulSwap)
		{
			int soulSwapCost = Souls_EV.GetSoulSwapCost(SoulShopOmniUIIncrementResourceButton.SoulTransferLevel);
			int num = SoulShopOmniUIIncrementResourceButton.SoulTransferLevel * 150;
			SaveManager.PlayerSaveData.RuneOreCollected -= soulSwapCost;
			SaveManager.PlayerSaveData.EquipmentOreCollected -= soulSwapCost;
			SaveManager.ModeSaveData.SoulSwapResourcesSpent += soulSwapCost;
			SaveManager.ModeSaveData.MiscSoulCollected += num;
			string text = $"<color=red>-{soulSwapCost} [RuneOre_Icon] [EquipmentOre_Icon]</color>\n<color=green>+{num} [Soul_Icon]</color>";
			TextPopupManager.DisplayTextAtAbsPos(TextPopupType.PlayerRankUp, text, ParentEntry.WindowController.CommonFields.PlayerModel.transform.position + vector);
		}
		else if (!m_aetherToOre)
		{
			int oreTransferAmount = SoulShopOmniUIIncrementResourceButton.OreTransferAmount;
			int num2 = Mathf.RoundToInt(SoulShopOmniUIIncrementResourceButton.OreTransferAmount / 2);
			SaveManager.PlayerSaveData.EquipmentOreCollected -= oreTransferAmount;
			SaveManager.PlayerSaveData.RuneOreCollected += num2;
			while (SoulShopOmniUIIncrementResourceButton.OreTransferAmount > SaveManager.PlayerSaveData.EquipmentOreCollected)
			{
				SoulShopOmniUIIncrementResourceButton.OreTransferAmount -= 500;
			}
			string text2 = $"<color=red>-{oreTransferAmount} [EquipmentOre_Icon]</color>\n<color=green>+{num2} [RuneOre_Icon]</color>";
			TextPopupManager.DisplayTextAtAbsPos(TextPopupType.PlayerRankUp, text2, ParentEntry.WindowController.CommonFields.PlayerModel.transform.position + vector);
		}
		else
		{
			int aetherTransferAmount = SoulShopOmniUIIncrementResourceButton.AetherTransferAmount;
			int num3 = Mathf.RoundToInt(SoulShopOmniUIIncrementResourceButton.AetherTransferAmount / 2);
			SaveManager.PlayerSaveData.RuneOreCollected -= aetherTransferAmount;
			SaveManager.PlayerSaveData.EquipmentOreCollected += num3;
			while (SoulShopOmniUIIncrementResourceButton.AetherTransferAmount > SaveManager.PlayerSaveData.RuneOreCollected)
			{
				SoulShopOmniUIIncrementResourceButton.AetherTransferAmount -= 500;
			}
			string text3 = $"<color=red>-{aetherTransferAmount} [RuneOre_Icon]</color>\n<color=green>+{num3} [EquipmentOre_Icon]</color>";
			TextPopupManager.DisplayTextAtAbsPos(TextPopupType.PlayerRankUp, text3, ParentEntry.WindowController.CommonFields.PlayerModel.transform.position + vector);
		}
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateDescription, this, ButtonEventArgs);
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateState, this, ButtonEventArgs);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.EquipmentOreChanged, this, null);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.RuneOreChanged, this, null);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.SoulChanged, this, null);
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
	}

	private void CancelConfirmMenu()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
	}
}
