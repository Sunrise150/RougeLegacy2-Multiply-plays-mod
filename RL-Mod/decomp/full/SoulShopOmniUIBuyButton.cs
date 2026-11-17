using System;
using TMPro;
using UnityEngine;

public class SoulShopOmniUIBuyButton : OmniUIButton, ISoulShopOmniUIButton
{
	[SerializeField]
	protected TMP_Text m_titleText;

	private SoulShopOmniUIDescriptionEventArgs m_descriptionEventArgs;

	protected PurchaseBoxDialogueEventArgs m_purchaseDialogueArgs = new PurchaseBoxDialogueEventArgs(PurchaseBoxDialogueType.Welcome);

	public override EventArgs ButtonEventArgs => m_descriptionEventArgs;

	public SoulShopType SoulShopType { get; set; }

	public SoulShopOmniUIEntry ParentEntry { get; set; }

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
		if (!IsButtonActive)
		{
			return;
		}
		base.OnConfirmButtonPressed();
		SoulShopObj soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj(SoulShopType);
		if (soulShopObj.IsNativeNull() || soulShopObj.CurrentOwnedLevel >= soulShopObj.MaxLevel)
		{
			StartCoroutine(ShakeAnimCoroutine());
		}
		else
		{
			int costAtLevel = soulShopObj.GetCostAtLevel(soulShopObj.CurrentOwnedLevel + 1, applyMaxLevelScalingCap: true);
			if (Souls_EV.GetTotalSoulsCollected(SaveManager.PlayerSaveData.GameModeType, deductSpentSouls: true) >= costAtLevel)
			{
				SaveManager.ModeSaveData.SoulSpent += costAtLevel;
				soulShopObj.SetOwnedLevel(1, additive: true, clampToMaxLevel: true);
				soulShopObj.SetEquippedLevel(1, additive: true);
				Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.SoulChanged, null, null);
				BaseEffect baseEffect = EffectManager.PlayEffect(base.gameObject, null, "SoulPurchase_Effect", Vector3.zero, 0f, EffectStopType.Gracefully);
				baseEffect.transform.SetParent(base.transform, worldPositionStays: false);
				baseEffect.transform.SetParent(null, worldPositionStays: true);
				InitializeButtonEventArgs();
				Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateDescription, this, ButtonEventArgs);
				Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateState, this, null);
				m_purchaseDialogueArgs.Initialize(PurchaseBoxDialogueType.PurchaseSuccessful);
				Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.UpdatePurchaseBoxDialogue, this, m_purchaseDialogueArgs);
				PlayerManager.GetPlayerController().ResetHealth();
				Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerResolveChanged, this, null);
				if (SoulShopType == SoulShopType.UnlockJukebox)
				{
					BaseRoom currentPlayerRoom = PlayerManager.GetCurrentPlayerRoom();
					PropSpawnController propSpawnController = currentPlayerRoom.gameObject.FindObjectReference<PropSpawnController>("Jukebox");
					if ((bool)propSpawnController && (bool)propSpawnController.PropInstance && !propSpawnController.PropInstance.gameObject.activeSelf)
					{
						propSpawnController.PropInstance.gameObject.SetActive(value: true);
					}
					PropSpawnController propSpawnController2 = currentPlayerRoom.gameObject.FindObjectReference<PropSpawnController>("PropBehindJukebox");
					if ((bool)propSpawnController2 && (bool)propSpawnController2.PropInstance)
					{
						propSpawnController2.PropInstance.gameObject.SetActive(value: false);
					}
				}
				if (SoulShopType == SoulShopType.UnlockOverload)
				{
					StoreAPIManager.GiveAchievement(AchievementType.SoulShopOverload, StoreType.All);
				}
			}
			else
			{
				m_purchaseDialogueArgs.Initialize(PurchaseBoxDialogueType.PurchaseFailed_NoMoney);
				Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.UpdatePurchaseBoxDialogue, this, m_purchaseDialogueArgs);
				StartCoroutine(ShakeAnimCoroutine());
			}
		}
		RunOnConfirmPressedAnimation();
	}

	public override void UpdateState()
	{
		SoulShopObj soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj(SoulShopType);
		if (!soulShopObj.IsNativeNull())
		{
			if (soulShopObj.CurrentOwnedLevel > 0)
			{
				m_titleText.text = LocalizationManager.GetString("LOC_ID_BLACKSMITH_UPGRADE_TITLE_1", isFemale: false);
			}
			else
			{
				m_titleText.text = LocalizationManager.GetString("LOC_ID_BLACKSMITH_BUY_TITLE_1", isFemale: false);
			}
		}
		if (soulShopObj.IsNativeNull() || soulShopObj.CurrentOwnedLevel >= soulShopObj.MaxLevel || !ParentEntry.IsUnlocked)
		{
			if (base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: false);
			}
			IsButtonActive = false;
			return;
		}
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
		}
		if (Souls_EV.GetTotalSoulsCollected(SaveManager.PlayerSaveData.GameModeType, deductSpentSouls: true) < soulShopObj.GetCostAtLevel(soulShopObj.CurrentOwnedLevel + 1, applyMaxLevelScalingCap: true))
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
