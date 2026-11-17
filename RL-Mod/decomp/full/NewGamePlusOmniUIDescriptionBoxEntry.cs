using RL_Windows;
using UnityEngine;

public class NewGamePlusOmniUIDescriptionBoxEntry : BaseOmniUIDescriptionBoxEntry<NewGamePlusOmniUIDescriptionEventArgs, NewGamePlusOmniUIDescriptionBoxEntry.NewGamePlusOmniUIDescriptionBoxType>
{
	public enum NewGamePlusOmniUIDescriptionBoxType
	{
		None,
		Title,
		Description,
		Controls
	}

	protected override void DisplayNullDescriptionBox(MonoBehaviour sender)
	{
		base.DisplayNullDescriptionBox(sender);
		switch (m_descriptionType)
		{
		case NewGamePlusOmniUIDescriptionBoxType.Title:
			m_titleText.text = "???";
			m_icon.sprite = IconLibrary.GetDefaultSprite();
			break;
		case NewGamePlusOmniUIDescriptionBoxType.Description:
		{
			NewGamePlusOmniUIEntry currentlySelectedEntry = (WindowManager.GetWindowController(WindowID.NewGamePlusNPC) as NewGamePlusOmniUIWindowController).CurrentlySelectedEntry;
			if ((bool)currentlySelectedEntry)
			{
				BurdenData burdenData = BurdenLibrary.GetBurdenData(currentlySelectedEntry.BurdenType);
				if ((bool)burdenData)
				{
					m_titleText.text = LocalizationManager.GetString(burdenData.Hint, isFemale: false);
				}
			}
			break;
		}
		}
	}

	protected override void DisplayDescriptionBox(NewGamePlusOmniUIDescriptionEventArgs args)
	{
		BurdenType burdenType = args.BurdenType;
		BurdenObj burdenObj = null;
		if (burdenType != 0)
		{
			burdenObj = BurdenManager.GetBurden(burdenType);
		}
		if (burdenObj.IsNativeNull() && burdenType != 0)
		{
			return;
		}
		switch (m_descriptionType)
		{
		case NewGamePlusOmniUIDescriptionBoxType.Title:
		{
			string text5;
			if (burdenType == BurdenType.None)
			{
				text5 = string.Format(LocalizationManager.GetString("LOC_ID_BURDEN_TITLE_ACTIVATE_TIMELINE_1", isFemale: false), NewGamePlusOmniUIWindowController.CurrentNewGamePlusLevel);
				if (m_icon.gameObject.activeSelf)
				{
					m_icon.gameObject.SetActive(value: false);
				}
			}
			else
			{
				text5 = LocalizationManager.GetString(burdenObj.BurdenData.Title, isFemale: false);
				if (!m_icon.gameObject.activeSelf)
				{
					m_icon.gameObject.SetActive(value: true);
				}
				m_icon.sprite = IconLibrary.GetBurdenIcon(burdenType);
			}
			m_titleText.text = text5;
			break;
		}
		case NewGamePlusOmniUIDescriptionBoxType.Description:
		{
			string text4 = ((burdenType != 0) ? LocalizationManager.GetString(burdenObj.BurdenData.Description, isFemale: false) : LocalizationManager.GetString("LOC_ID_BURDEN_DESCRIPTION_ACTIVATE_TIMELINE_1", isFemale: false));
			m_titleText.text = text4;
			break;
		}
		case NewGamePlusOmniUIDescriptionBoxType.Controls:
		{
			string text = "";
			if (burdenType == BurdenType.None)
			{
				text = LocalizationManager.GetString("LOC_ID_BURDEN_CONTROLS_ACTIVATE_TIMELINE_1", isFemale: false);
				text = ((SaveManager.PlayerSaveData.HighestNGPlusBeaten <= -1) ? string.Format(text, SaveManager.PlayerSaveData.NewGamePlusLevel, LocalizationManager.GetString("LOC_ID_GENERAL_UI_NA_1", isFemale: false)) : string.Format(text, SaveManager.PlayerSaveData.NewGamePlusLevel, SaveManager.PlayerSaveData.HighestNGPlusBeaten));
			}
			else if (BurdenManager.IsBurdenUnlocked(burdenType))
			{
				text = LocalizationManager.GetString(burdenObj.BurdenData.Description2, isFemale: false);
				bool num = CDGHelper.IsPercent(burdenObj.BurdenData.StatsGain);
				float f = (num ? (burdenObj.BurdenData.StatsGain * 100f) : burdenObj.BurdenData.StatsGain);
				float f2 = (num ? (burdenObj.CurrentStatGain * 100f) : burdenObj.CurrentStatGain);
				if (burdenType == BurdenType.EnemySpeed)
				{
					f = burdenObj.BurdenData.StatsGain * 10f;
					f2 = burdenObj.CurrentStatGain * 10f;
				}
				string text2 = f.ToString("F1", LocalizationManager.GetCurrentCultureInfo());
				if (text2[text2.Length - 1] == '0')
				{
					text2 = Mathf.RoundToInt(f).ToString();
				}
				string text3 = f2.ToString("F1", LocalizationManager.GetCurrentCultureInfo());
				if (text3[text3.Length - 1] == '0')
				{
					text3 = Mathf.RoundToInt(f2).ToString();
				}
				text = string.Format(text, text3, burdenObj.CurrentBurdenWeight, text2, burdenObj.InitialBurdenWeight);
			}
			m_titleText.text = text;
			break;
		}
		}
	}
}
