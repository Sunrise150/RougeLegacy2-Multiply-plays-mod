using FMODUnity;
using UnityEngine;

namespace RLAudio;

public class SkillTreeSlotAudioEventController : MonoBehaviour, IAudioEventEmitter
{
	[SerializeField]
	[EventRef]
	private string m_selectedPath;

	[SerializeField]
	[EventRef]
	private string m_upgradedPath;

	[SerializeField]
	[EventRef]
	private string m_fullyUpgradedPath;

	[SerializeField]
	[EventRef]
	private string m_upgradeFailedPath;

	private string m_description;

	public string Description
	{
		get
		{
			if (string.IsNullOrEmpty(m_description))
			{
				m_description = ToString();
			}
			return m_description;
		}
	}

	private void Awake()
	{
		SkillTreeSlot component = GetComponent<SkillTreeSlot>();
		component.SelectedRelay.AddListener(OnSlotSelected);
		component.FullyUpgradedRelay.AddListener(OnFullyUpgraded);
		component.UpgradedRelay.AddListener(OnUpgraded);
		component.UpgradeFailedRelay.AddListener(OnUpgradeFailed);
	}

	private void OnUpgradeFailed()
	{
		AudioManager.PlayOneShot(this, m_upgradeFailedPath);
	}

	private void OnUpgraded()
	{
		AudioManager.PlayOneShot(this, m_upgradedPath);
	}

	private void OnFullyUpgraded()
	{
		AudioManager.PlayOneShot(this, m_fullyUpgradedPath);
	}

	private void OnSlotSelected()
	{
		AudioManager.PlayOneShot(this, m_selectedPath);
	}
}
