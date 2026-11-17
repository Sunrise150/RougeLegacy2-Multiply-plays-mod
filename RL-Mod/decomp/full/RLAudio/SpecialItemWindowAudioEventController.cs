using FMODUnity;
using RL_Windows;
using UnityEngine;

namespace RLAudio;

public class SpecialItemWindowAudioEventController : MonoBehaviour, IAudioEventEmitter
{
	[SerializeField]
	[EventRef]
	private string m_runeWindowOpenedPath;

	[SerializeField]
	[EventRef]
	private string m_runeWindowClosedPath;

	[SerializeField]
	[EventRef]
	private string m_runeRevealedPath;

	[SerializeField]
	[EventRef]
	private string m_blueprintWindowOpenedPath;

	[SerializeField]
	[EventRef]
	private string m_blueprintWindowClosedPath;

	[SerializeField]
	[EventRef]
	private string m_blueprintRevealedPath;

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
		SpecialItemDropWindowController component = base.gameObject.GetComponent<SpecialItemDropWindowController>();
		component.WindowOpenedRelay.AddListener(OnWindowOpened);
		component.WindowClosedRelay.AddListener(OnWindowClosed);
		component.ItemRevealedRelay.AddListener(OnItemRevealed);
	}

	private void OnWindowOpened(SpecialItemType itemID)
	{
		string path = string.Empty;
		switch (itemID)
		{
		case SpecialItemType.Blueprint:
			path = m_blueprintWindowOpenedPath;
			break;
		case SpecialItemType.Rune:
			path = m_runeWindowOpenedPath;
			break;
		}
		AudioManager.PlayOneShot(this, path);
	}

	private void OnItemRevealed(SpecialItemType itemID)
	{
		string path = string.Empty;
		switch (itemID)
		{
		case SpecialItemType.Blueprint:
			path = m_blueprintRevealedPath;
			break;
		case SpecialItemType.Rune:
			path = m_runeRevealedPath;
			break;
		}
		AudioManager.PlayOneShot(this, path);
	}

	private void OnWindowClosed(SpecialItemType itemID)
	{
		string path = string.Empty;
		switch (itemID)
		{
		case SpecialItemType.Blueprint:
			path = m_blueprintWindowClosedPath;
			break;
		case SpecialItemType.Rune:
			path = m_runeWindowClosedPath;
			break;
		}
		AudioManager.PlayOneShot(this, path);
	}
}
