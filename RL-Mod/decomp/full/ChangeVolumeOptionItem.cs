using RLAudio;
using UnityEngine;

public class ChangeVolumeOptionItem : IncrementDecrementOptionItem
{
	private enum AudioType
	{
		SFX,
		Music,
		Master
	}

	[SerializeField]
	private AudioType m_audioType;

	protected override void OnEnable()
	{
		base.OnEnable();
		switch (m_audioType)
		{
		case AudioType.Master:
			m_currentIncrementValue = Mathf.RoundToInt(AudioManager.GetMasterVolume() * 100f);
			break;
		case AudioType.SFX:
			m_currentIncrementValue = Mathf.RoundToInt(AudioManager.GetSFXVolume() * 100f);
			break;
		case AudioType.Music:
			m_currentIncrementValue = Mathf.RoundToInt(AudioManager.GetMusicVolume() * 100f);
			break;
		}
		UpdateIncrementBar();
	}

	public override void Initialize()
	{
		m_minValue = 0f;
		m_maxValue = 100f;
		m_snapToMultiplesOf = 5;
		m_numberOfIncrements = 100 / m_snapToMultiplesOf;
		base.Initialize();
	}

	public override void ActivateOption()
	{
		base.ActivateOption();
		if (AudioManager.IsInitialized && m_audioType != AudioType.Music)
		{
			_ = m_audioType;
			_ = 2;
		}
	}

	public override void DeactivateOption(bool confirmOptionChange)
	{
		base.DeactivateOption(confirmOptionChange);
		if (AudioManager.IsInitialized && m_audioType != AudioType.Music)
		{
			_ = m_audioType;
			_ = 2;
		}
	}

	protected override void Increment()
	{
		UpdateVolume();
	}

	protected override void Decrement()
	{
		UpdateVolume();
	}

	private void UpdateVolume()
	{
		if (AudioManager.IsInitialized)
		{
			switch (m_audioType)
			{
			case AudioType.Master:
				AudioManager.SetMasterVolume(m_currentIncrementValue / m_maxValue);
				break;
			case AudioType.SFX:
				AudioManager.SetSFXVolume(m_currentIncrementValue / m_maxValue);
				break;
			case AudioType.Music:
				AudioManager.SetMusicVolume(m_currentIncrementValue / m_maxValue);
				break;
			}
		}
	}

	public override void CancelOptionChange()
	{
		base.CancelOptionChange();
		UpdateVolume();
	}

	public override void ConfirmOptionChange()
	{
		SaveManager.ConfigData.MasterVolume = AudioManager.GetMasterVolume();
		SaveManager.ConfigData.MusicVolume = AudioManager.GetMusicVolume();
		SaveManager.ConfigData.SFXVolume = AudioManager.GetSFXVolume();
	}

	protected override void UpdateIncrementBar()
	{
		m_currentIncrementValue = Mathf.RoundToInt(m_currentIncrementValue);
		base.UpdateIncrementBar();
	}

	public override void SetCurrentIncrementValue(float value, bool useNormalizedValue)
	{
		base.SetCurrentIncrementValue(value, useNormalizedValue);
		UpdateVolume();
	}
}
