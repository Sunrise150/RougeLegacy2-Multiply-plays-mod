using UnityEngine;

public class JukeboxOmniUIDescriptionBoxEntry : BaseOmniUIDescriptionBoxEntry<JukeboxOmniUIDescriptionEventArgs, JukeboxOmniUIDescriptionBoxEntry.JukeboxOmniUIDescriptionBoxType>
{
	public enum JukeboxOmniUIDescriptionBoxType
	{
		None,
		Title,
		Description
	}

	protected override void DisplayNullDescriptionBox(MonoBehaviour sender)
	{
		base.DisplayNullDescriptionBox(sender);
		if (m_descriptionType == JukeboxOmniUIDescriptionBoxType.Title)
		{
			m_titleText.text = "???";
		}
	}

	protected override void DisplayDescriptionBox(JukeboxOmniUIDescriptionEventArgs args)
	{
		SongID songType = args.SongType;
		if (!Jukebox_EV.JukeboxDataDict.TryGetValue(songType, out var value))
		{
			return;
		}
		bool flag = false;
		if (SaveManager.ModeSaveData.GetAchievementUnlocked(value.AchievementUnlockType) || value.AchievementUnlockType == AchievementType.None)
		{
			flag = true;
		}
		switch (m_descriptionType)
		{
		case JukeboxOmniUIDescriptionBoxType.Title:
			if (!flag)
			{
				m_titleText.text = "???";
			}
			else
			{
				m_titleText.text = LocalizationManager.GetString(value.SongTitleLocID, isFemale: false);
			}
			break;
		case JukeboxOmniUIDescriptionBoxType.Description:
		{
			AchievementData achievementData = AchievementLibrary.GetAchievementData(value.AchievementUnlockType);
			if (achievementData != null)
			{
				if (!flag)
				{
					if (!string.IsNullOrWhiteSpace(achievementData.NotAchievedHiddenLocID))
					{
						m_titleText.text = LocalizationManager.GetString(achievementData.NotAchievedHiddenLocID, isFemale: false);
					}
					else
					{
						m_titleText.text = LocalizationManager.GetString(achievementData.NotAchievedLocID, isFemale: false);
					}
				}
				else
				{
					m_titleText.text = LocalizationManager.GetString(achievementData.AchievedLocID, isFemale: false);
				}
			}
			else if (value.AchievementUnlockType == AchievementType.None)
			{
				m_titleText.text = "";
			}
			else
			{
				m_titleText.text = "ACHIEVEMENT DATA: " + value.AchievementUnlockType.ToString() + " NOT FOUND";
			}
			break;
		}
		}
	}
}
