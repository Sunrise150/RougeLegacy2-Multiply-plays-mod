using System;
using UnityEngine;
using UnityEngine.UI;

public class SoulShopOmniUIToggleButton : OmniUIButton, ISoulShopOmniUIButton
{
	[SerializeField]
	private Image m_sliderOnSprite;

	[SerializeField]
	private Image m_sliderOffSprite;

	[SerializeField]
	private GameObject m_notchGameObj;

	private SoulShopOmniUIDescriptionEventArgs m_descriptionEventArgs;

	private PurchaseBoxDialogueEventArgs m_purchaseDialogueArgs = new PurchaseBoxDialogueEventArgs(PurchaseBoxDialogueType.Welcome);

	public SoulShopType SoulShopType { get; set; }

	public SoulShopOmniUIEntry ParentEntry { get; set; }

	public override EventArgs ButtonEventArgs => m_descriptionEventArgs;

	protected override void InitializeButtonEventArgs()
	{
		if (m_descriptionEventArgs == null)
		{
			m_descriptionEventArgs = new SoulShopOmniUIDescriptionEventArgs(SoulShopType, OmniUIButtonType.Equipping);
		}
		else
		{
			m_descriptionEventArgs.Initialize(SoulShopType, OmniUIButtonType.Equipping);
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
		if (!soulShopObj.IsNativeNull() && soulShopObj.CurrentOwnedLevel > 0)
		{
			if (soulShopObj.CurrentEquippedLevel > 0)
			{
				soulShopObj.SetEquippedLevel(0, additive: false);
			}
			else
			{
				soulShopObj.SetEquippedLevel(1, additive: false);
			}
			UpdateState();
		}
		else
		{
			StartCoroutine(ShakeAnimCoroutine());
		}
		RunOnConfirmPressedAnimation();
	}

	public override void UpdateState()
	{
		SoulShopObj soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj(SoulShopType);
		if (soulShopObj.IsNativeNull() || soulShopObj.CurrentOwnedLevel <= 0 || !ParentEntry.IsToggle)
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
		if (soulShopObj.CurrentEquippedLevel > 0)
		{
			SetToggleOn(toggleOn: true);
		}
		else
		{
			SetToggleOn(toggleOn: false);
		}
		IsButtonActive = true;
	}

	private void SetToggleOn(bool toggleOn)
	{
		if (toggleOn)
		{
			if (!m_sliderOnSprite.gameObject.activeSelf)
			{
				m_sliderOnSprite.gameObject.SetActive(value: true);
			}
			if (m_sliderOffSprite.gameObject.activeSelf)
			{
				m_sliderOffSprite.gameObject.SetActive(value: false);
			}
			Vector3 localPosition = m_notchGameObj.transform.localPosition;
			localPosition.x = 45f;
			m_notchGameObj.transform.localPosition = localPosition;
		}
		else
		{
			if (m_sliderOnSprite.gameObject.activeSelf)
			{
				m_sliderOnSprite.gameObject.SetActive(value: false);
			}
			if (!m_sliderOffSprite.gameObject.activeInHierarchy)
			{
				m_sliderOffSprite.gameObject.SetActive(value: true);
			}
			Vector3 localPosition2 = m_notchGameObj.transform.localPosition;
			localPosition2.x = 0f;
			m_notchGameObj.transform.localPosition = localPosition2;
		}
	}
}
