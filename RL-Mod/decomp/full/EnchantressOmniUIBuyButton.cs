using System;
using RL_Windows;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class EnchantressOmniUIBuyButton : OmniUIButton, IEnchantressOmniUIButton
{
	[SerializeField]
	private TMP_Text m_buyText;

	[SerializeField]
	private UnityEvent m_cantAffordPurchaseUnityEvent;

	[SerializeField]
	private TMP_Text m_maxText;

	private EnchantressOmniUIDescriptionEventArgs m_descriptionEventArgs;

	private PurchaseBoxDialogueEventArgs m_purchaseDialogueArgs = new PurchaseBoxDialogueEventArgs(PurchaseBoxDialogueType.Welcome);

	private Action m_cancelConfirmMenuSelection;

	public override EventArgs ButtonEventArgs => m_descriptionEventArgs;

	public RuneType RuneType { get; set; }

	protected override void Awake()
	{
		base.Awake();
		m_cancelConfirmMenuSelection = CancelConfirmMenuSelection;
	}

	protected override void InitializeButtonEventArgs()
	{
		if (m_descriptionEventArgs == null)
		{
			m_descriptionEventArgs = new EnchantressOmniUIDescriptionEventArgs(RuneType, OmniUIButtonType.Purchasing);
		}
		else
		{
			m_descriptionEventArgs.Initialize(RuneType, OmniUIButtonType.Purchasing);
		}
	}

	public override void OnConfirmButtonPressed()
	{
		if (!IsButtonActive)
		{
			return;
		}
		base.OnConfirmButtonPressed();
		if (RuneManager.GetFoundState(RuneType) != FoundState.NotFound)
		{
			if (PurchaseRune())
			{
				InitializeButtonEventArgs();
				Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateDescription, this, ButtonEventArgs);
				Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateState, this, null);
				m_purchaseDialogueArgs.Initialize(PurchaseBoxDialogueType.PurchaseSuccessful);
				Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.UpdatePurchaseBoxDialogue, this, m_purchaseDialogueArgs);
			}
			else
			{
				Debug.Log("Could not afford rune: " + RuneType);
				RuneObj rune = RuneManager.GetRune(RuneType);
				_ = rune.GoldCostToUpgrade;
				if (rune.OreCostToUpgrade > SaveManager.PlayerSaveData.RuneOreCollected)
				{
					m_purchaseDialogueArgs.Initialize(PurchaseBoxDialogueType.PurchaseFailed_NoOre);
				}
				else
				{
					m_purchaseDialogueArgs.Initialize(PurchaseBoxDialogueType.PurchaseFailed_NoMoney);
				}
				Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.UpdatePurchaseBoxDialogue, this, m_purchaseDialogueArgs);
				StartCoroutine(ShakeAnimCoroutine());
			}
		}
		RunOnConfirmPressedAnimation();
	}

	public override void UpdateState()
	{
		m_deselectedSprite.SetAlpha(1f);
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
		}
		IsButtonActive = true;
		if (m_maxText.gameObject.activeSelf)
		{
			m_maxText.gameObject.SetActive(value: false);
		}
		int upgradeLevel = RuneManager.GetUpgradeLevel(RuneType);
		if (upgradeLevel > 0 && upgradeLevel >= RuneManager.GetUpgradeBlueprintsFound(RuneType))
		{
			if (base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: false);
			}
			IsButtonActive = false;
			if (RuneManager.GetRune(RuneType).IsMaxUpgradeLevel)
			{
				m_maxText.gameObject.SetActive(value: true);
			}
		}
		if (!RuneManager.CanPurchaseRune(RuneType))
		{
			m_deselectedSprite.SetAlpha(0.35f);
		}
	}

	private bool PurchaseRune()
	{
		if (RuneManager.CanPurchaseRune(RuneType))
		{
			RuneObj rune = RuneManager.GetRune(RuneType);
			int goldCostToUpgrade = rune.GoldCostToUpgrade;
			int oreCostToUpgrade = rune.OreCostToUpgrade;
			int goldCollected = SaveManager.PlayerSaveData.GoldCollected;
			int goldSaved = SaveManager.PlayerSaveData.GoldSaved;
			int runeOreCollected = SaveManager.PlayerSaveData.RuneOreCollected;
			SaveManager.PlayerSaveData.SubtractFromGoldIncludingBank(goldCostToUpgrade);
			SaveManager.PlayerSaveData.RuneOreCollected -= oreCostToUpgrade;
			if (RuneManager.GetFoundState(RuneType) < FoundState.Purchased)
			{
				RuneManager.SetFoundState(RuneType, FoundState.Purchased, overrideValues: false);
			}
			if (!RuneManager.SetRuneUpgradeLevel(RuneType, 1, additive: true, overrideValues: false))
			{
				SaveManager.PlayerSaveData.GoldCollected = goldCollected;
				SaveManager.PlayerSaveData.GoldSaved = goldSaved;
				SaveManager.PlayerSaveData.RuneOreCollected = runeOreCollected;
				return false;
			}
			SaveManager.PlayerSaveData.GoldSpent += goldCostToUpgrade;
			SaveManager.PlayerSaveData.GoldSpentOnRunes += goldCostToUpgrade;
			SaveManager.PlayerSaveData.RuneOreSpent += oreCostToUpgrade;
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.GoldChanged, null, null);
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.EquipmentOreChanged, null, null);
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.RuneOreChanged, null, null);
			if (RuneManager.CanEquip(RuneType))
			{
				RuneManager.SetRuneEquippedLevel(RuneType, 1, additive: true);
			}
			else
			{
				InitializeCantEquipConfirmMenu();
				WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: true);
			}
			BaseEffect baseEffect = EffectManager.PlayEffect(base.gameObject, null, "Purchase_Effect", Vector3.zero, 0f, EffectStopType.Gracefully);
			baseEffect.transform.SetParent(base.transform, worldPositionStays: false);
			baseEffect.transform.SetParent(null, worldPositionStays: true);
			Debug.Log("Successfully Purchased " + rune.RuneType);
			if ((WindowManager.GetWindowController(WindowID.Enchantress) as EnchantressOmniUIWindowController).HasAllRunes())
			{
				StoreAPIManager.GiveAchievement(AchievementType.AllRunes, StoreType.All);
			}
			return true;
		}
		if (m_cantAffordPurchaseUnityEvent != null)
		{
			m_cantAffordPurchaseUnityEvent.Invoke();
		}
		return false;
	}

	private void InitializeCantEquipConfirmMenu()
	{
		if (!WindowManager.GetIsWindowLoaded(WindowID.ConfirmMenu))
		{
			WindowManager.LoadWindow(WindowID.ConfirmMenu);
		}
		ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenu) as ConfirmMenuWindowController;
		obj.SetTitleText("LOC_ID_ENCHANTRESS_PURCHASE_WEIGHT_EXCEEDED_TITLE_1", isLocID: true);
		obj.SetDescriptionText("LOC_ID_ENCHANTRESS_PURCHASE_WEIGHT_EXCEEDED_DESCRIPTION_1", isLocID: true);
		obj.SetNumberOfButtons(1);
		obj.SetOnCancelAction(m_cancelConfirmMenuSelection);
		ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
		buttonAtIndex.SetButtonText("LOC_ID_GENERAL_UI_OK_1", isLocID: true);
		buttonAtIndex.SetOnClickAction(m_cancelConfirmMenuSelection);
	}

	private void CancelConfirmMenuSelection()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
	}
}
