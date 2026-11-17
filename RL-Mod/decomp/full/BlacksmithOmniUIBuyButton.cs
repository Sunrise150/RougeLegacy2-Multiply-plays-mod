using System;
using RL_Windows;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class BlacksmithOmniUIBuyButton : OmniUIButton, IBlacksmithOmniUIButton
{
	[SerializeField]
	private TMP_Text m_buyText;

	[SerializeField]
	private UnityEvent m_cantAffordPurchaseUnityEvent;

	[SerializeField]
	private TMP_Text m_maxText;

	private bool m_buttonIsUpgrade;

	private BlacksmithOmniUIDescriptionEventArgs m_descriptionEventArgs;

	private PurchaseBoxDialogueEventArgs m_purchaseDialogueArgs = new PurchaseBoxDialogueEventArgs(PurchaseBoxDialogueType.Welcome);

	private Action m_cancelConfirmMenuSelection;

	private Action m_confirmUpgradePurchase;

	public EquipmentCategoryType CategoryType { get; set; }

	public EquipmentType EquipmentType { get; set; }

	public override EventArgs ButtonEventArgs => m_descriptionEventArgs;

	protected override void Awake()
	{
		base.Awake();
		m_cancelConfirmMenuSelection = CancelConfirmMenuSelection;
		m_confirmUpgradePurchase = ConfirmUpgradePurchase;
	}

	protected override void InitializeButtonEventArgs()
	{
		OmniUIButtonType buttonType = OmniUIButtonType.Purchasing;
		if (m_buttonIsUpgrade)
		{
			buttonType = OmniUIButtonType.Upgrading;
		}
		if (m_descriptionEventArgs == null)
		{
			m_descriptionEventArgs = new BlacksmithOmniUIDescriptionEventArgs(CategoryType, EquipmentType, buttonType);
		}
		else
		{
			m_descriptionEventArgs.Initialize(CategoryType, EquipmentType, buttonType);
		}
	}

	public override void OnConfirmButtonPressed()
	{
		if (IsButtonActive)
		{
			base.OnConfirmButtonPressed();
			if (!m_buttonIsUpgrade)
			{
				PurchaseEquipment();
			}
			else
			{
				UpgradeEquipment();
			}
			RunOnConfirmPressedAnimation();
		}
	}

	private void OnPurchaseSuccessful()
	{
		BaseEffect baseEffect = EffectManager.PlayEffect(base.gameObject, null, "Purchase_Effect", Vector3.zero, 0f, EffectStopType.Gracefully);
		baseEffect.transform.SetParent(base.transform, worldPositionStays: false);
		baseEffect.transform.SetParent(null, worldPositionStays: true);
		InitializeButtonEventArgs();
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateDescription, this, ButtonEventArgs);
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateState, this, null);
		m_purchaseDialogueArgs.Initialize(PurchaseBoxDialogueType.PurchaseSuccessful);
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.UpdatePurchaseBoxDialogue, this, m_purchaseDialogueArgs);
		Debug.Log("Successfully Purchased " + CategoryType.ToString() + " " + EquipmentType);
		if ((WindowManager.GetWindowController(WindowID.Blacksmith) as BlacksmithOmniUIWindowController).HasAllEquipment())
		{
			StoreAPIManager.GiveAchievement(AchievementType.AllEquipment, StoreType.All);
		}
	}

	private void OnPurchaseFailed()
	{
		EquipmentObj equipment = EquipmentManager.GetEquipment(CategoryType, EquipmentType);
		_ = equipment.GoldCostToUpgrade;
		if (equipment.OreCostToUpgrade > SaveManager.PlayerSaveData.EquipmentOreCollected)
		{
			m_purchaseDialogueArgs.Initialize(PurchaseBoxDialogueType.PurchaseFailed_NoOre);
		}
		else
		{
			m_purchaseDialogueArgs.Initialize(PurchaseBoxDialogueType.PurchaseFailed_NoMoney);
		}
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.UpdatePurchaseBoxDialogue, this, m_purchaseDialogueArgs);
		if (m_cantAffordPurchaseUnityEvent != null)
		{
			m_cantAffordPurchaseUnityEvent.Invoke();
		}
		StartCoroutine(ShakeAnimCoroutine());
	}

	public override void UpdateState()
	{
		m_deselectedSprite.SetAlpha(1f);
		if (EquipmentManager.GetFoundState(CategoryType, EquipmentType) != 0)
		{
			m_buyText.text = LocalizationManager.GetString("LOC_ID_BLACKSMITH_BUY_TITLE_1", isFemale: false);
			m_buttonIsUpgrade = false;
		}
		else
		{
			m_buyText.text = LocalizationManager.GetString("LOC_ID_BLACKSMITH_UPGRADE_TITLE_1", isFemale: false);
			m_buttonIsUpgrade = true;
		}
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
		}
		IsButtonActive = true;
		if (m_maxText.gameObject.activeSelf)
		{
			m_maxText.gameObject.SetActive(value: false);
		}
		if (m_buttonIsUpgrade && EquipmentManager.GetUpgradeLevel(CategoryType, EquipmentType) >= EquipmentManager.GetUpgradeBlueprintsFound(CategoryType, EquipmentType))
		{
			if (base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: false);
			}
			IsButtonActive = false;
			if (EquipmentManager.GetEquipment(CategoryType, EquipmentType).IsMaxUpgradeLevel)
			{
				m_maxText.gameObject.SetActive(value: true);
			}
		}
		if (!EquipmentManager.CanPurchaseEquipment(CategoryType, EquipmentType))
		{
			m_deselectedSprite.SetAlpha(0.35f);
		}
	}

	private void PurchaseEquipment()
	{
		if (EquipmentManager.CanPurchaseEquipment(CategoryType, EquipmentType, suppressLogs: false))
		{
			EquipmentObj equipment = EquipmentManager.GetEquipment(CategoryType, EquipmentType);
			int goldCostToUpgrade = equipment.GoldCostToUpgrade;
			int oreCostToUpgrade = equipment.OreCostToUpgrade;
			int goldCollected = SaveManager.PlayerSaveData.GoldCollected;
			int goldSaved = SaveManager.PlayerSaveData.GoldSaved;
			int equipmentOreCollected = SaveManager.PlayerSaveData.EquipmentOreCollected;
			SaveManager.PlayerSaveData.SubtractFromGoldIncludingBank(goldCostToUpgrade);
			SaveManager.PlayerSaveData.EquipmentOreCollected -= oreCostToUpgrade;
			if (!EquipmentManager.SetFoundState(equipment.CategoryType, equipment.EquipmentType, FoundState.Purchased, overrideValues: true))
			{
				SaveManager.PlayerSaveData.GoldCollected = goldCollected;
				SaveManager.PlayerSaveData.GoldSaved = goldSaved;
				SaveManager.PlayerSaveData.EquipmentOreCollected = equipmentOreCollected;
				Debug.Log("Failed to purchase " + CategoryType.ToString() + " " + EquipmentType.ToString() + " for unknown reason.");
				return;
			}
			SaveManager.PlayerSaveData.GoldSpent += goldCostToUpgrade;
			SaveManager.PlayerSaveData.GoldSpentOnEquipment += goldCostToUpgrade;
			SaveManager.PlayerSaveData.EquipmentOreSpent += oreCostToUpgrade;
			if (EquipmentManager.CanEquip(CategoryType, EquipmentType))
			{
				BlacksmithOmniUIEquipButton.SetEquipped(CategoryType, EquipmentType);
			}
			else
			{
				InitializeCantEquipConfirmMenu();
				WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: true);
			}
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.GoldChanged, null, null);
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.EquipmentOreChanged, null, null);
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.RuneOreChanged, null, null);
			m_buttonIsUpgrade = true;
			OnPurchaseSuccessful();
		}
		else
		{
			OnPurchaseFailed();
		}
	}

	private void UpgradeEquipment()
	{
		if (EquipmentManager.CanPurchaseEquipment(CategoryType, EquipmentType, suppressLogs: false))
		{
			EquipmentObj equipment = EquipmentManager.GetEquipment(CategoryType, EquipmentType);
			int num = PlayerManager.GetPlayerController().ActualAllowedEquipmentWeight - (int)EquipmentManager.GetTotalEquippedStatValue(EquipmentStatType.Weight);
			int num2 = (int)equipment.GetStatValueAtLevel(EquipmentStatType.Weight, equipment.ClampedUpgradeLevel + 1) - (int)equipment.GetCurrentStatValue(EquipmentStatType.Weight);
			if (EquipmentManager.IsEquipped(CategoryType, EquipmentType) && num2 > num)
			{
				InitializeUpgradeUnequipConfirmMenu();
				WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: true);
				return;
			}
			ConfirmUpgradePurchase();
			if (!EquipmentManager.CanEquip(CategoryType, EquipmentType))
			{
				InitializeCantEquipConfirmMenu();
				WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: true);
			}
			return;
		}
		EquipmentObj equipment2 = EquipmentManager.GetEquipment(CategoryType, EquipmentType);
		if (equipment2.FoundState != 0)
		{
			Debug.Log("Cannot level up " + CategoryType.ToString() + " " + EquipmentType.ToString() + ".  Reason: Equipment must be purchased first.");
		}
		else if (!EquipmentManager.CanAfford(CategoryType, EquipmentType))
		{
			Debug.Log("Cannot level up " + CategoryType.ToString() + " " + EquipmentType.ToString() + ".  Reason: Cannot afford upgrading.");
		}
		else if (equipment2.UpgradeLevel >= equipment2.MaxLevel)
		{
			Debug.Log("Cannot level up " + CategoryType.ToString() + " " + EquipmentType.ToString() + ".  Reason: Equipment already at max level.");
		}
		OnPurchaseFailed();
	}

	private void ConfirmUpgradePurchase()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
		EquipmentObj equipment = EquipmentManager.GetEquipment(CategoryType, EquipmentType);
		int goldCostToUpgrade = equipment.GoldCostToUpgrade;
		int oreCostToUpgrade = equipment.OreCostToUpgrade;
		int goldCollected = SaveManager.PlayerSaveData.GoldCollected;
		int goldSaved = SaveManager.PlayerSaveData.GoldSaved;
		int equipmentOreCollected = SaveManager.PlayerSaveData.EquipmentOreCollected;
		SaveManager.PlayerSaveData.SubtractFromGoldIncludingBank(goldCostToUpgrade);
		SaveManager.PlayerSaveData.EquipmentOreCollected -= oreCostToUpgrade;
		if (!EquipmentManager.SetEquipmentUpgradeLevel(equipment.CategoryType, equipment.EquipmentType, 1, additive: true, overrideValues: false))
		{
			SaveManager.PlayerSaveData.GoldCollected = goldCollected;
			SaveManager.PlayerSaveData.GoldSaved = goldSaved;
			SaveManager.PlayerSaveData.EquipmentOreCollected = equipmentOreCollected;
			Debug.Log("Failed to level up " + CategoryType.ToString() + " " + EquipmentType.ToString() + " for unknown reason.");
			return;
		}
		SaveManager.PlayerSaveData.GoldSpent += goldCostToUpgrade;
		SaveManager.PlayerSaveData.GoldSpentOnEquipment += goldCostToUpgrade;
		SaveManager.PlayerSaveData.EquipmentOreSpent += oreCostToUpgrade;
		if (EquipmentManager.CanEquip(CategoryType, EquipmentType))
		{
			BlacksmithOmniUIEquipButton.SetEquipped(CategoryType, EquipmentType);
		}
		else if (PlayerManager.GetPlayerController().ActualAllowedEquipmentWeight < (int)EquipmentManager.GetTotalEquippedStatValue(EquipmentStatType.Weight))
		{
			BlacksmithOmniUIEquipButton.SetEquipped(CategoryType, EquipmentType.None);
		}
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.GoldChanged, null, null);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.EquipmentOreChanged, null, null);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.RuneOreChanged, null, null);
		OnPurchaseSuccessful();
	}

	private void InitializeCantEquipConfirmMenu()
	{
		if (!WindowManager.GetIsWindowLoaded(WindowID.ConfirmMenu))
		{
			WindowManager.LoadWindow(WindowID.ConfirmMenu);
		}
		ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenu) as ConfirmMenuWindowController;
		obj.SetTitleText("LOC_ID_BLACKSMITH_PURCHASE_WEIGHT_EXCEEDED_TITLE_1", isLocID: true);
		obj.SetDescriptionText("LOC_ID_BLACKSMITH_PURCHASE_WEIGHT_EXCEEDED_DESCRIPTION_1", isLocID: true);
		obj.SetNumberOfButtons(1);
		obj.SetOnCancelAction(m_cancelConfirmMenuSelection);
		ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
		buttonAtIndex.SetButtonText("LOC_ID_GENERAL_UI_OK_1", isLocID: true);
		buttonAtIndex.SetOnClickAction(m_cancelConfirmMenuSelection);
	}

	private void InitializeUpgradeUnequipConfirmMenu()
	{
		if (!WindowManager.GetIsWindowLoaded(WindowID.ConfirmMenu))
		{
			WindowManager.LoadWindow(WindowID.ConfirmMenu);
		}
		ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenu) as ConfirmMenuWindowController;
		obj.SetTitleText("LOC_ID_BLACKSMITH_UPGRADE_WEIGHT_EXCEEDED_TITLE_1", isLocID: true);
		obj.SetDescriptionText("LOC_ID_BLACKSMITH_UPGRADE_WEIGHT_EXCEEDED_DESCRIPTION_1", isLocID: true);
		obj.SetNumberOfButtons(2);
		obj.SetOnCancelAction(m_cancelConfirmMenuSelection);
		ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
		buttonAtIndex.SetButtonText("LOC_ID_GENERAL_UI_OK_1", isLocID: true);
		buttonAtIndex.SetOnClickAction(m_confirmUpgradePurchase);
		ConfirmMenu_Button buttonAtIndex2 = obj.GetButtonAtIndex(1);
		buttonAtIndex2.SetButtonText("LOC_ID_GENERAL_UI_CANCEL_1", isLocID: true);
		buttonAtIndex2.SetOnClickAction(m_cancelConfirmMenuSelection);
	}

	private void CancelConfirmMenuSelection()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
	}
}
