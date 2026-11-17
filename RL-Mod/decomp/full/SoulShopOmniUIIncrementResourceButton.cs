using System;
using TMPro;
using UnityEngine;

public class SoulShopOmniUIIncrementResourceButton : OmniUIButton, ISoulShopOmniUIButton
{
	public static int OreTransferAmount;

	public static int AetherTransferAmount;

	public static int SoulTransferLevel;

	[SerializeField]
	private TMP_Text m_resourceText;

	[SerializeField]
	private bool m_soulSwap;

	[SerializeField]
	[ConditionalHide("m_soulSwap", true, Inverse = true)]
	private bool m_aetherToOre;

	[SerializeField]
	private bool m_isDecrement;

	protected SoulShopOmniUIDescriptionEventArgs m_descriptionEventArgs;

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
			m_descriptionEventArgs.Initialize(SoulShopType, OmniUIButtonType.Equipping);
		}
	}

	public override void UpdateState()
	{
		if (m_soulSwap)
		{
			int soulSwapCost = Souls_EV.GetSoulSwapCost(SoulTransferLevel);
			int num = SoulTransferLevel * 150;
			if ((m_isDecrement && soulSwapCost <= 0) || (!m_isDecrement && SoulTransferLevel >= Souls_EV.ORE_AETHER_TO_SOUL_COST_LEVELS.Length - 1))
			{
				m_deselectedSprite.SetAlpha(0.25f);
				IsButtonActive = false;
			}
			else
			{
				m_deselectedSprite.SetAlpha(1f);
				IsButtonActive = true;
			}
			string text = soulSwapCost.ToString();
			if (text.Length > 6)
			{
				text = Mathf.RoundToInt((float)soulSwapCost / 1000f) + "K";
			}
			string text2 = num.ToString();
			if (text2.Length > 6)
			{
				text2 = Mathf.RoundToInt((float)num / 1000f) + "K";
			}
			m_resourceText.text = $"{text} : {text2}";
			return;
		}
		int num2 = SaveManager.PlayerSaveData.EquipmentOreCollected;
		int num3 = OreTransferAmount;
		if (m_aetherToOre)
		{
			num2 = SaveManager.PlayerSaveData.RuneOreCollected;
			num3 = AetherTransferAmount;
		}
		int num4 = Mathf.RoundToInt(num3 / 2);
		if ((m_isDecrement && num3 <= 0) || (!m_isDecrement && num3 >= num2) || (!m_isDecrement && num2 - num3 < 500))
		{
			m_deselectedSprite.SetAlpha(0.25f);
			IsButtonActive = false;
		}
		else
		{
			m_deselectedSprite.SetAlpha(1f);
			IsButtonActive = true;
		}
		string text3 = num3.ToString();
		if (text3.Length > 6)
		{
			text3 = Mathf.RoundToInt((float)num3 / 1000f) + "K";
		}
		string text4 = num4.ToString();
		if (text4.Length > 6)
		{
			text4 = Mathf.RoundToInt((float)num4 / 1000f) + "K";
		}
		m_resourceText.text = $"{text3} : {text4}";
	}

	public override void OnConfirmButtonPressed()
	{
		if (!IsButtonActive)
		{
			return;
		}
		base.OnConfirmButtonPressed();
		if (!m_isDecrement)
		{
			if (m_soulSwap)
			{
				SoulTransferLevel++;
				if (SoulTransferLevel > Souls_EV.ORE_AETHER_TO_SOUL_COST_LEVELS.Length - 1)
				{
					SoulTransferLevel = Souls_EV.ORE_AETHER_TO_SOUL_COST_LEVELS.Length - 1;
				}
			}
			else if (!m_aetherToOre)
			{
				OreTransferAmount += 500;
			}
			else
			{
				AetherTransferAmount += 500;
			}
		}
		else if (m_soulSwap)
		{
			SoulTransferLevel--;
			if (SoulTransferLevel < 0)
			{
				SoulTransferLevel = 0;
			}
		}
		else if (!m_aetherToOre)
		{
			OreTransferAmount -= 500;
		}
		else
		{
			AetherTransferAmount -= 500;
		}
		RunOnConfirmPressedAnimation();
		ParentEntry.UpdateState();
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateDescription, this, ButtonEventArgs);
	}
}
