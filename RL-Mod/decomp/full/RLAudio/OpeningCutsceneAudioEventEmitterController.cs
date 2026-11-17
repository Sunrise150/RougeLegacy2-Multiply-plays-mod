using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using TMPro;
using UnityEngine;

namespace RLAudio;

public class OpeningCutsceneAudioEventEmitterController : MonoBehaviour, IAudioEventEmitter
{
	[SerializeField]
	[EventRef]
	private string m_startAudioEventPath;

	[SerializeField]
	[EventRef]
	private string m_blackScreenAudioEventPath;

	[SerializeField]
	[EventRef]
	private string m_starsAppearAudioEventPath;

	[SerializeField]
	[EventRef]
	private string m_castleAppearsAudioEventPath;

	[SerializeField]
	[EventRef]
	private string m_textAppearsAudioEventPath;

	[SerializeField]
	[EventRef]
	private string m_continueLegacy;

	[SerializeField]
	[EventRef]
	private string m_newLegacy;

	[SerializeField]
	[EventRef]
	private string m_storyPoint00;

	[SerializeField]
	[EventRef]
	private string m_storyPoint01;

	[SerializeField]
	[EventRef]
	private string m_storyPoint02;

	[SerializeField]
	[EventRef]
	private string m_storyPoint03;

	[SerializeField]
	[EventRef]
	private string m_storyPoint04;

	[SerializeField]
	[EventRef]
	private string m_storyPointBoatInWater;

	[SerializeField]
	[EventRef]
	private string m_storyPointboatWavesAndSqueaks;

	[SerializeField]
	[EventRef]
	private string m_storyPoint05;

	[SerializeField]
	[EventRef]
	private string m_storyPoint06;

	[SerializeField]
	[EventRef]
	private string m_storyPoint07;

	[SerializeField]
	private MusicPlayer m_mainMenuMusicPlayer;

	[SerializeField]
	private StudioEventEmitter m_mainMenuAmbientSoundEventEmitter;

	private string m_description = string.Empty;

	private MainMenuWindowController m_mainMenuWindowController;

	private EventInstance m_startEventIntance;

	private EventInstance m_blackScreenEventInstance;

	private EventInstance m_starsAppearEventInstance;

	private EventInstance m_castleAppearsEventInstance;

	private bool m_isFastForwarding;

	private List<EventInstance> m_currentEvents = new List<EventInstance>();

	private float m_currentFastForwardValue;

	private Coroutine m_fastForwardCoroutine;

	private int m_textEntryNumber;

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
		m_startEventIntance = AudioUtility.GetEventInstance(m_startAudioEventPath, base.transform);
		m_blackScreenEventInstance = AudioUtility.GetEventInstance(m_blackScreenAudioEventPath, base.transform);
		m_starsAppearEventInstance = AudioUtility.GetEventInstance(m_starsAppearAudioEventPath, base.transform);
		m_castleAppearsEventInstance = AudioUtility.GetEventInstance(m_castleAppearsAudioEventPath, base.transform);
		m_mainMenuWindowController = GetComponent<MainMenuWindowController>();
		m_mainMenuWindowController.LoadGameRelay.AddListener(OnLoadGame);
		m_mainMenuWindowController.CutsceneStartRelay.AddListener(OnCutsceneStart);
		m_mainMenuWindowController.CutsceneCompleteRelay.AddListener(OnCutsceneComplete);
		m_mainMenuWindowController.CutsceneFastForwardRelay.AddListener(OnCutsceneFastForward);
		m_mainMenuWindowController.CutsceneBlackRelay.AddListener(OnCutsceneBlack);
		m_mainMenuWindowController.CloudsAppearRelay.AddListener(OnCloudsAppear);
		m_mainMenuWindowController.CutsceneCastleAppearsRelay.AddListener(OnCutsceneCastleAppears);
		m_mainMenuWindowController.CutsceneStarsAppearRelay.AddListener(OnCutsceneStarsAppear);
		m_mainMenuWindowController.CutsceneDisplayTextRelay.AddListener(OnCutsceneDisplayTextCall);
	}

	private void OnDestroy()
	{
		if (m_startEventIntance.isValid())
		{
			m_startEventIntance.release();
		}
		if (m_blackScreenEventInstance.isValid())
		{
			m_blackScreenEventInstance.release();
		}
		if (m_starsAppearEventInstance.isValid())
		{
			m_starsAppearEventInstance.release();
		}
		if (m_castleAppearsEventInstance.isValid())
		{
			m_castleAppearsEventInstance.release();
		}
	}

	private void OnLoadGame(bool isContinueLegacy)
	{
		if (isContinueLegacy)
		{
			AudioManager.PlayOneShot(this, m_continueLegacy);
			return;
		}
		EventInstance eventInstance = AudioUtility.GetEventInstance(m_newLegacy, base.transform);
		PlayEvent(eventInstance);
	}

	private void OnCloudsAppear()
	{
		EventInstance eventInstance = AudioUtility.GetEventInstance(m_storyPointboatWavesAndSqueaks, base.transform);
		PlayEvent(eventInstance);
	}

	private void OnCutsceneBlack()
	{
		PlayEvent(m_blackScreenEventInstance);
	}

	private void OnCutsceneStarsAppear()
	{
		StopEventInstance(m_blackScreenEventInstance);
		PlayEvent(m_starsAppearEventInstance);
		EventInstance eventInstance = AudioUtility.GetEventInstance(m_storyPointBoatInWater, base.transform);
		PlayEvent(eventInstance);
	}

	private void StopEventInstance(EventInstance eventInstance)
	{
		m_currentEvents.Remove(eventInstance);
		AudioManager.Stop(eventInstance);
	}

	private void OnCutsceneDisplayTextCall(CanvasGroup canvasGroup)
	{
		StartCoroutine(PlayTextAudio(canvasGroup));
	}

	private IEnumerator PlayTextAudio(CanvasGroup canvasGroup)
	{
		while (canvasGroup.alpha == 0f)
		{
			yield return null;
		}
		TextMeshProUGUI componentInChildren = canvasGroup.GetComponentInChildren<TextMeshProUGUI>();
		RectTransformUtility.ScreenPointToWorldPointInRectangle(componentInChildren.rectTransform, componentInChildren.rectTransform.position, CameraController.GameCamera, out var worldPoint);
		if (!m_isFastForwarding)
		{
			AudioManager.PlayOneShot(this, m_textAppearsAudioEventPath, worldPoint);
		}
		string text = string.Empty;
		switch (m_textEntryNumber)
		{
		case 0:
			text = m_storyPoint00;
			break;
		case 1:
			text = m_storyPoint01;
			break;
		case 2:
			text = m_storyPoint02;
			break;
		case 3:
			text = m_storyPoint03;
			break;
		case 4:
			text = m_storyPoint04;
			break;
		case 5:
			text = m_storyPoint05;
			break;
		case 6:
			text = m_storyPoint06;
			break;
		}
		if (text != string.Empty)
		{
			EventInstance eventInstance = AudioUtility.GetEventInstance(text, base.transform);
			PlayEvent(eventInstance, worldPoint);
		}
		m_textEntryNumber++;
	}

	private void OnCutsceneCastleAppears()
	{
		PlayEvent(m_castleAppearsEventInstance);
	}

	private void OnCutsceneFastForward(bool isFastForwarding)
	{
		m_isFastForwarding = isFastForwarding;
		for (int i = 0; i < m_currentEvents.Count; i++)
		{
			if (m_currentEvents[i].isValid())
			{
				if (m_fastForwardCoroutine != null)
				{
					StopCoroutine(m_fastForwardCoroutine);
				}
				m_fastForwardCoroutine = StartCoroutine(SetFastForwardParameter(isFastForwarding));
			}
		}
	}

	private IEnumerator SetFastForwardParameter(bool isFastForwarding)
	{
		float timeStart = Time.unscaledTime;
		float initialValue = m_currentFastForwardValue;
		float targetValue = 0f;
		if (isFastForwarding)
		{
			targetValue = 1f;
		}
		while (Time.unscaledTime - timeStart < 0.25f)
		{
			float t = (Time.unscaledTime - timeStart) / 0.25f;
			m_currentFastForwardValue = Mathf.Lerp(initialValue, targetValue, t);
			for (int i = 0; i < m_currentEvents.Count; i++)
			{
				if (m_currentEvents[i].isValid())
				{
					m_currentEvents[i].setParameterByName("FastForward", m_currentFastForwardValue);
				}
			}
			yield return null;
		}
		m_currentFastForwardValue = targetValue;
		m_fastForwardCoroutine = null;
	}

	private void OnCutsceneComplete()
	{
		for (int i = 0; i < m_currentEvents.Count; i++)
		{
			if (m_currentEvents[i].isValid())
			{
				AudioManager.Stop(m_currentEvents[i]);
			}
		}
		AudioManager.PlayOneShot(this, m_storyPoint07);
	}

	private void OnCutsceneStart()
	{
		if (m_mainMenuMusicPlayer != null)
		{
			m_mainMenuMusicPlayer.Stop();
		}
		if (m_mainMenuAmbientSoundEventEmitter != null)
		{
			m_mainMenuAmbientSoundEventEmitter.Stop();
		}
		m_textEntryNumber = 0;
		PlayEvent(m_startEventIntance);
	}

	private void PlayEvent(EventInstance eventInstance)
	{
		PlayEvent(eventInstance, Vector3.zero);
	}

	private void PlayEvent(EventInstance eventInstance, Vector3 position)
	{
		m_currentEvents.Add(eventInstance);
		if (position == Vector3.zero)
		{
			AudioManager.Play(this, eventInstance);
		}
		else
		{
			AudioManager.Play(this, eventInstance, position);
		}
		eventInstance.setParameterByName("FastForward", m_currentFastForwardValue);
		eventInstance.release();
	}
}
