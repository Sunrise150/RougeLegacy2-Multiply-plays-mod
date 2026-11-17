using System.Collections;
using FMODUnity;
using UnityEngine;

namespace RLAudio;

public class PlayerManaAudioEventController : MonoBehaviour, IAudioEventEmitter
{
	[SerializeField]
	[EventRef]
	private string m_maxManaAudioEventPath;

	private string m_description = string.Empty;

	private bool m_hasMaxManaAudioPlayed = true;

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

	private IEnumerator Start()
	{
		while (!PlayerManager.IsInstantiated)
		{
			yield return null;
		}
		PlayerManager.GetPlayerController().ManaChangeRelay.AddListener(OnManaChange);
	}

	private void OnManaChange(ManaChangeEventArgs manaChangeEventArgs)
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		if (playerController.CurrentManaAsInt < playerController.ActualMaxMana && m_hasMaxManaAudioPlayed)
		{
			m_hasMaxManaAudioPlayed = false;
		}
		else if (playerController.CurrentManaAsInt >= playerController.ActualMaxMana && !m_hasMaxManaAudioPlayed)
		{
			AudioManager.PlayOneShot(this, m_maxManaAudioEventPath);
			m_hasMaxManaAudioPlayed = true;
		}
	}

	private void OnDestroy()
	{
		if (!GameManager.IsApplicationClosing && PlayerManager.IsInstantiated && PlayerManager.GetPlayerController().ManaChangeRelay != null)
		{
			PlayerManager.GetPlayerController().ManaChangeRelay.RemoveListener(OnManaChange);
		}
	}
}
