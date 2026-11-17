using System;
using RLAudio;
using UnityEngine;
using UnityEngine.Events;

public class DialogueDisplayController_GreenHood : DialogueDisplayController, IAudioEventEmitter
{
	[SerializeField]
	private UnityEvent m_onDialogueSpokenEvent;

	private Action m_onDialogueEndHandler;

	public string Description => ToString();

	protected override void Awake()
	{
		base.Awake();
		m_onDialogueEndHandler = OnDialogueEndHandler;
	}

	private void OnEnable()
	{
		GetComponent<NPCController>().SetNPCState(NPCState.AtAttention, skipTransition: true);
	}

	protected override void StartDialogue()
	{
		base.StartDialogue();
		DialogueManager.AddDialogueCompleteEndHandler(m_onDialogueEndHandler);
	}

	private void OnDialogueEndHandler()
	{
		GetComponent<Interactable>().gameObject.SetActive(value: false);
		if (m_onDialogueSpokenEvent != null)
		{
			m_onDialogueSpokenEvent.Invoke();
		}
		AudioManager.PlayOneShotAttached(this, "event:/Cut_Scenes/sfx_cutscene_ngPlusTeleport_playerAppear", base.gameObject);
		base.gameObject.SetActive(value: false);
	}
}
