using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Serialization;

namespace RLAudio;

public class DialogWindowAudioEventController : MonoBehaviour, IAudioEventEmitter
{
	[SerializeField]
	[EventRef]
	private string m_windowOpenedEventPath;

	[SerializeField]
	[EventRef]
	private string m_windowClosedEventPath;

	[SerializeField]
	[EventRef]
	[FormerlySerializedAs("m_writeLineEventPath")]
	private string m_writeTextEventPath;

	[SerializeField]
	[EventRef]
	[FormerlySerializedAs("m_writeCharacterEventPath")]
	private string m_writePunctuationEventPath;

	[SerializeField]
	[EventRef]
	[FormerlySerializedAs("m_writeCharacterEventPath")]
	private string m_playerInputToNextPageEventPath;

	[SerializeField]
	[EventRef]
	private string m_completeEventPath;

	private EventInstance m_writeLineEventInstance;

	private string m_description = string.Empty;

	private bool m_isPlayingWriteLineAudio;

	private bool m_isFirstPage = true;

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
		DialogueWindowController component = GetComponent<DialogueWindowController>();
		component.WriteLineOfDialogStartRelay.AddListener(OnWriteCharacter);
		component.WriteLineOfDialogEndRelay.AddListener(OnWritePageComplete);
		component.WriteCharacterStartRelay.AddListener(OnWritePunctuation);
		component.NextLineOfDialogRelay.AddListener(OnNextPage);
		component.WindowOpenedEvent.AddListener(OnWindowOpened);
		component.DialogWindowBeginClosingRelay.AddListener(OnWindowClosed);
		m_writeLineEventInstance = AudioUtility.GetEventInstance(m_writeTextEventPath, base.transform);
	}

	private void OnDestroy()
	{
		if (m_writeLineEventInstance.isValid())
		{
			m_writeLineEventInstance.release();
		}
	}

	private void OnWindowOpened()
	{
		AudioManager.PlayOneShot(this, m_windowOpenedEventPath);
		m_isFirstPage = true;
	}

	private void OnWindowClosed()
	{
		AudioManager.PlayOneShot(this, m_windowClosedEventPath);
		StopWriteTextAudio();
	}

	private void OnWriteCharacter()
	{
		PlayWriteTextAudio();
	}

	private void PlayWriteTextAudio()
	{
		if (!m_isPlayingWriteLineAudio)
		{
			AudioManager.Play(this, m_writeLineEventInstance);
			m_isPlayingWriteLineAudio = true;
		}
	}

	private void OnWritePunctuation()
	{
		StopWriteTextAudio();
		AudioManager.PlayOneShot(this, m_writePunctuationEventPath);
	}

	private void StopWriteTextAudio()
	{
		if (m_isPlayingWriteLineAudio)
		{
			AudioManager.Stop(m_writeLineEventInstance);
			m_isPlayingWriteLineAudio = false;
		}
	}

	private void OnNextPage()
	{
		if (m_isFirstPage)
		{
			m_isFirstPage = false;
		}
		else
		{
			AudioManager.PlayOneShot(this, m_playerInputToNextPageEventPath);
		}
		StopWriteTextAudio();
		PlayWriteTextAudio();
	}

	private void OnWritePageComplete()
	{
		AudioManager.PlayOneShot(this, m_completeEventPath);
		StopWriteTextAudio();
	}
}
