using System;
using RLAudio;
using TMPro;
using UnityEngine;

public class JukeboxOmniUIBuyButton : OmniUIButton, IJukeboxOmniUIButton
{
	[SerializeField]
	private TMP_Text m_buyText;

	[SerializeField]
	private GameObject m_playSymbol;

	[SerializeField]
	private GameObject m_stopSymbol;

	private SongID m_songType;

	private JukeboxOmniUIDescriptionEventArgs m_descriptionEventArgs;

	public JukeboxOmniUIWindowController JukeboxWindowController { get; set; }

	public JukeboxOmniUIEntry JukeboxEntry { get; set; }

	public bool IsPlayingSong
	{
		get
		{
			if (MusicManager.CurrentSong == SongType)
			{
				return MusicManager.IsPlayingOverride;
			}
			return false;
		}
	}

	public override EventArgs ButtonEventArgs => m_descriptionEventArgs;

	public SongID SongType
	{
		get
		{
			return m_songType;
		}
		set
		{
			m_songType = value;
		}
	}

	protected override void InitializeButtonEventArgs()
	{
		if (m_descriptionEventArgs == null)
		{
			m_descriptionEventArgs = new JukeboxOmniUIDescriptionEventArgs(SongType, IsPlayingSong);
		}
		else
		{
			m_descriptionEventArgs.Initialize(SongType, IsPlayingSong);
		}
	}

	public override void OnConfirmButtonPressed()
	{
		base.OnConfirmButtonPressed();
		if (!IsPlayingSong)
		{
			StopSong();
			PlaySong();
		}
		else
		{
			StopSong();
		}
		InitializeButtonEventArgs();
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateDescription, this, ButtonEventArgs);
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateState, this, null);
		RunOnConfirmPressedAnimation();
	}

	public void PlaySong()
	{
		MusicManager.StopMusicOverride();
		MusicManager.PlayMusic(SongType, isOverride: true);
		JukeboxWindowController.JukeboxSpectrum.StartSpectrum();
		PropSpawnController propSpawnController = PlayerManager.GetCurrentPlayerRoom().gameObject.FindObjectReference<PropSpawnController>("Jukebox");
		if ((bool)propSpawnController && (bool)propSpawnController.PropInstance)
		{
			propSpawnController.PropInstance.Animators[0].SetBool("Playing", value: true);
		}
		if (SaveManager.PlayerSaveData.GetSongFoundState(SongType) == FoundState.FoundButNotViewed)
		{
			SaveManager.PlayerSaveData.SetSongFoundState(SongType, FoundState.FoundAndViewed);
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateState, this, null);
		}
	}

	public void StopSong()
	{
		MusicManager.StopMusicOverride();
		PropSpawnController propSpawnController = PlayerManager.GetCurrentPlayerRoom().gameObject.FindObjectReference<PropSpawnController>("Jukebox");
		if ((bool)propSpawnController && (bool)propSpawnController.PropInstance)
		{
			propSpawnController.PropInstance.Animators[0].SetBool("Playing", value: false);
		}
		JukeboxWindowController.JukeboxSpectrum.StopSpectrum();
	}

	public override void UpdateState()
	{
		Jukebox_EV.JukeboxDataDict.TryGetValue(SongType, out var value);
		SongID songID = MusicLibrary.GetBiomeMusic(PlayerManager.GetCurrentPlayerRoom().BiomeType)[0];
		if (value == null || (!SaveManager.ModeSaveData.GetAchievementUnlocked(value.AchievementUnlockType) && value.AchievementUnlockType != 0) || SongType == songID)
		{
			if (SaveManager.PlayerSaveData.GetSongFoundState(SongType) == FoundState.FoundButNotViewed)
			{
				SaveManager.PlayerSaveData.SetSongFoundState(SongType, FoundState.FoundAndViewed);
				Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateState, this, null);
			}
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
		IsButtonActive = true;
		if (IsPlayingSong)
		{
			if (m_playSymbol.activeSelf)
			{
				m_playSymbol.SetActive(value: false);
			}
			if (!m_stopSymbol.activeSelf)
			{
				m_stopSymbol.SetActive(value: true);
			}
		}
		else
		{
			if (!m_playSymbol.activeSelf)
			{
				m_playSymbol.SetActive(value: true);
			}
			if (m_stopSymbol.activeSelf)
			{
				m_stopSymbol.SetActive(value: false);
			}
		}
		if (m_buyText.gameObject.activeSelf)
		{
			m_buyText.gameObject.SetActive(value: false);
		}
	}
}
