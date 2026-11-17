using System;
using UnityEngine.EventSystems;

public class JukeboxOmniUIEntry : BaseOmniUIEntry
{
	private JukeboxOmniUIDescriptionEventArgs m_eventArgs;

	public SongID SongType { get; protected set; }

	private JukeboxData Data => Jukebox_EV.JukeboxDataDict[SongType];

	public override EventArgs EntryEventArgs
	{
		get
		{
			if (m_eventArgs == null)
			{
				m_eventArgs = new JukeboxOmniUIDescriptionEventArgs(SongType, isPlaying: true);
			}
			else
			{
				m_eventArgs.Initialize(SongType, isPlaying: true);
			}
			return m_eventArgs;
		}
	}

	public override bool IsEntryActive => true;

	public void Initialize(SongID songType, JukeboxOmniUIWindowController windowController)
	{
		SongType = songType;
		Initialize(windowController);
		OmniUIButton[] buttonArray = m_buttonArray;
		for (int i = 0; i < buttonArray.Length; i++)
		{
			IJukeboxOmniUIButton obj = (IJukeboxOmniUIButton)buttonArray[i];
			obj.SongType = SongType;
			obj.JukeboxWindowController = windowController;
			obj.JukeboxEntry = this;
		}
		m_titleText.text = LocalizationManager.GetString(Data.SongTitleLocID, isFemale: false);
	}

	public override void UpdateActive()
	{
	}

	public override void UpdateState()
	{
		bool flag = false;
		if (SaveManager.ModeSaveData.GetAchievementUnlocked(Data.AchievementUnlockType) || Data.AchievementUnlockType == AchievementType.None)
		{
			flag = true;
		}
		if (!flag)
		{
			m_titleText.text = "???";
			if (m_newSymbol.gameObject.activeSelf)
			{
				m_newSymbol.gameObject.SetActive(value: false);
			}
		}
		else
		{
			if (SaveManager.PlayerSaveData.GetSongFoundState(SongType) == FoundState.FoundButNotViewed)
			{
				if (!m_newSymbol.gameObject.activeSelf)
				{
					m_newSymbol.gameObject.SetActive(value: true);
				}
			}
			else if (m_newSymbol.gameObject.activeSelf)
			{
				m_newSymbol.gameObject.SetActive(value: false);
			}
			m_titleText.text = LocalizationManager.GetString(Data.SongTitleLocID, isFemale: false);
		}
		base.UpdateState();
	}

	public override void OnSelect(BaseEventData eventData)
	{
		if (base.Interactable)
		{
			bool flag = false;
			if (SaveManager.ModeSaveData.GetAchievementUnlocked(Data.AchievementUnlockType) || Data.AchievementUnlockType == AchievementType.None)
			{
				flag = true;
			}
			base.OnSelect(eventData);
		}
	}
}
