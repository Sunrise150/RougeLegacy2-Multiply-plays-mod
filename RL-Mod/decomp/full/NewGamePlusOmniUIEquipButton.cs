using System;
using RL_Windows;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class NewGamePlusOmniUIEquipButton : OmniUIButton, INewGamePlusOmniUIButton
{
	[SerializeField]
	private bool m_isRightArrow;

	[SerializeField]
	private GameObject m_buttonGO;

	[SerializeField]
	private TMP_Text m_levelText;

	private NewGamePlusOmniUIDescriptionEventArgs m_descriptionEventArgs;

	public int CurrentNewGamePlusLevel
	{
		get
		{
			return NewGamePlusOmniUIWindowController.CurrentNewGamePlusLevel;
		}
		set
		{
			NewGamePlusOmniUIWindowController.CurrentNewGamePlusLevel = value;
		}
	}

	public override EventArgs ButtonEventArgs => m_descriptionEventArgs;

	public BurdenType BurdenType { get; set; }

	public override void OnSelect(BaseEventData eventData)
	{
		if (BurdenType != 0 || (WindowManager.GetWindowController(WindowID.NewGamePlusNPC) as NewGamePlusOmniUIWindowController).InTimelineEntry)
		{
			base.OnSelect(eventData);
		}
	}

	protected override void InitializeButtonEventArgs()
	{
		OmniUIButtonType buttonType = (m_isRightArrow ? OmniUIButtonType.Equipping : OmniUIButtonType.Unequipping);
		if (m_descriptionEventArgs == null)
		{
			m_descriptionEventArgs = new NewGamePlusOmniUIDescriptionEventArgs(BurdenType, buttonType);
		}
		else
		{
			m_descriptionEventArgs.Initialize(BurdenType, buttonType);
		}
	}

	public override void UpdateState()
	{
		if (BurdenType == BurdenType.None)
		{
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: true);
			}
			IsButtonActive = true;
			m_levelText.text = CurrentNewGamePlusLevel.ToString();
			return;
		}
		BurdenObj burden = BurdenManager.GetBurden(BurdenType);
		if (!burden.IsNativeNull())
		{
			int currentLevel = burden.CurrentLevel;
			int maxLevel = burden.MaxLevel;
			if (maxLevel > 0)
			{
				m_deselectedSprite.SetAlpha(1f);
				IsButtonActive = true;
			}
			else
			{
				m_deselectedSprite.SetAlpha(0.25f);
				IsButtonActive = false;
			}
			m_levelText.text = currentLevel + "/" + maxLevel;
		}
	}

	public override void OnConfirmButtonPressed()
	{
		if (BurdenType == BurdenType.None)
		{
			ChangeNGPlusLevel();
		}
		else
		{
			ChangeBurdenLevel();
		}
		RunOnConfirmPressedAnimation();
		base.OnConfirmButtonPressed();
	}

	private void ChangeBurdenLevel()
	{
		BurdenObj burden = BurdenManager.GetBurden(BurdenType);
		if (burden.IsNativeNull())
		{
			return;
		}
		if (m_isRightArrow)
		{
			if (burden.CurrentLevel < burden.MaxLevel)
			{
				BurdenManager.SetBurdenLevel(BurdenType, 1, additive: true);
				InitializeButtonEventArgs();
				Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateDescription, this, ButtonEventArgs);
				Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateState, this, null);
			}
			else
			{
				StartCoroutine(ShakeAnimCoroutine());
			}
		}
		else if (burden.CurrentLevel > 0)
		{
			if (burden.CurrentLevel >= burden.MaxLevel)
			{
				BurdenManager.SetBurdenLevel(BurdenType, burden.CurrentLevel - 1, additive: false);
			}
			else
			{
				BurdenManager.SetBurdenLevel(BurdenType, -1, additive: true);
			}
			InitializeButtonEventArgs();
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateDescription, this, ButtonEventArgs);
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateState, this, null);
		}
		else
		{
			StartCoroutine(ShakeAnimCoroutine());
		}
	}

	private void ChangeNGPlusLevel()
	{
		int currentNewGamePlusLevel = CurrentNewGamePlusLevel;
		if (m_isRightArrow && CurrentNewGamePlusLevel < 100 && CurrentNewGamePlusLevel < SaveManager.PlayerSaveData.HighestNGPlusBeaten + 1)
		{
			CurrentNewGamePlusLevel++;
		}
		else if (!m_isRightArrow && CurrentNewGamePlusLevel > 0)
		{
			CurrentNewGamePlusLevel--;
		}
		CurrentNewGamePlusLevel = Mathf.Clamp(CurrentNewGamePlusLevel, 0, 100);
		if (currentNewGamePlusLevel != CurrentNewGamePlusLevel)
		{
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateDescription, this, ButtonEventArgs);
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateState, this, null);
		}
		else
		{
			StartCoroutine(ShakeAnimCoroutine());
		}
	}
}
