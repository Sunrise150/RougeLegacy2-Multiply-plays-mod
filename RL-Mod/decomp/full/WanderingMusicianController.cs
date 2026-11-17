using System.Collections;
using RL_Windows;
using UnityEngine;

public class WanderingMusicianController : BaseSpecialPropController
{
	private bool m_specialMusicIsPlaying;

	private WaitUntil m_waitUntilSpawnControllerReady;

	protected override void Awake()
	{
		m_waitUntilSpawnControllerReady = new WaitUntil(() => WanderingMusicianSpawnController.IsInitialized);
		base.Awake();
	}

	public void RunWanderer()
	{
		if (!base.IsPropComplete)
		{
			PropComplete();
			DialogueManager.StartNewDialogue();
			DialogueManager.AddNonLocDialogue("Wanderer", "Hi, I'm the Wanderer!");
			WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		}
		else
		{
			DialogueManager.StartNewDialogue();
			DialogueManager.AddNonLocDialogue("Wanderer", "I already told you all I know...");
			WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		}
	}

	protected override void DisableProp(bool firstTimeDisabled)
	{
	}

	private void OnEnable()
	{
		StartCoroutine(CheckForEnable());
	}

	private IEnumerator CheckForEnable()
	{
		yield return m_waitUntilSpawnControllerReady;
		if (base.Room != null)
		{
			if (WanderingMusicianSpawnController.BiomeControllerIndex == base.Room.BiomeControllerIndex && WanderingMusicianSpawnController.BiomeControllerType == base.Room.AppearanceBiomeType)
			{
				m_specialMusicIsPlaying = true;
			}
			else
			{
				base.gameObject.SetActive(value: false);
			}
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_ = m_specialMusicIsPlaying;
	}

	protected override void InitializePooledPropOnEnter()
	{
	}
}
