using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace RLAudio;

public class DeathScreenXPAudioEventController : MonoBehaviour, IAudioEventEmitter
{
	[SerializeField]
	[EventRef]
	private string m_xpGainAudioPath;

	[SerializeField]
	[EventRef]
	private string m_xpGainEndAudioPath;

	[SerializeField]
	[EventRef]
	private string m_levelUpAudioPath;

	private string m_description = string.Empty;

	private EventInstance m_xpGainEventInstance;

	private EventInstance m_xpGainEndEventInstance;

	private float m_percentageComplete;

	private const string XP_PARAMETER = "xp_gain";

	public string Description
	{
		get
		{
			if (m_description == string.Empty)
			{
				m_description = ToString();
			}
			return m_description;
		}
	}

	private void Awake()
	{
		PlayerDeathRankTextController component = GetComponent<PlayerDeathRankTextController>();
		component.XPGainedRelay.AddListener(OnXPGained);
		component.XPGainStartRelay.AddListener(OnXPGainStart);
		component.XPGainCompleteRelay.AddListener(OnXPGainComplete);
		component.LevelGainedRelay.AddListener(OnLevelGained);
	}

	private void Start()
	{
		m_xpGainEventInstance = AudioUtility.GetEventInstance(m_xpGainAudioPath, base.transform);
		m_xpGainEndEventInstance = AudioUtility.GetEventInstance(m_xpGainEndAudioPath, base.transform);
	}

	private void OnDestroy()
	{
		if (m_xpGainEventInstance.isValid())
		{
			m_xpGainEventInstance.release();
		}
		if (m_xpGainEndEventInstance.isValid())
		{
			m_xpGainEndEventInstance.release();
		}
	}

	private void OnXPGainStart()
	{
		AudioManager.Play(this, m_xpGainEventInstance);
	}

	private void OnXPGained(int currentXP, int nextLevelXP)
	{
		m_percentageComplete = (float)currentXP / (float)nextLevelXP;
		m_xpGainEventInstance.setParameterByName("xp_gain", m_percentageComplete);
	}

	private void OnXPGainComplete()
	{
		AudioManager.Stop(m_xpGainEventInstance);
		m_xpGainEndEventInstance.setParameterByName("xp_gain", m_percentageComplete);
		AudioManager.Play(this, m_xpGainEndEventInstance);
	}

	private void OnLevelGained(int level)
	{
		AudioManager.PlayOneShot(this, m_levelUpAudioPath);
	}
}
