using System;
using System.Collections;
using RLAudio;
using RL_Windows;
using UnityEngine;

public class CainPropController : BaseSpecialPropController, IDisplaySpeechBubble, IAudioEventEmitter
{
	[SerializeField]
	private GameObject m_playerPositionObj;

	[SerializeField]
	private GameObject m_cainCape;

	private Prop m_prop;

	private Action m_startRoomIntro;

	public GameObject Cape => m_cainCape;

	public bool ShouldDisplaySpeechBubble => true;

	public SpeechBubbleType BubbleType => SpeechBubbleType.Dialogue;

	public string Description => ToString();

	protected override void Awake()
	{
		base.Awake();
		m_prop = GetComponent<Prop>();
		m_startRoomIntro = StartRoomIntro;
	}

	protected override void InitializePooledPropOnEnter()
	{
		m_interactable.SetIsInteractableActive(isActive: true);
		if (SaveManager.PlayerSaveData.SpokenToFinalBoss)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public void TalkToCain()
	{
		m_interactable.SetIsInteractableActive(isActive: false);
		StartCoroutine(TalkToCainCoroutine());
	}

	private IEnumerator TalkToCainCoroutine()
	{
		AudioManager.PlayOneShot(this, "event:/SFX/Enemies/Cain/vo_cain_greeting", base.transform.position);
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		yield return MovePlayerToCain();
		float delay = Time.time + 0.25f;
		while (Time.time < delay)
		{
			yield return null;
		}
		RunDialogue();
	}

	private void RunDialogue()
	{
		DialogueManager.StartNewDialogue();
		DialogueManager.AddDialogue("LOC_ID_NAME_CAIN_NAME_1", "LOC_ID_CAIN_ENDING_FIRST_MEETING_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		DialogueManager.AddDialogueCompleteEndHandler(m_startRoomIntro);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
	}

	private void StartRoomIntro()
	{
		FinalBossRoomController component = base.Room.GetComponent<FinalBossRoomController>();
		if ((bool)component)
		{
			RewiredMapController.SetCurrentMapEnabled(enabled: false);
			component.StartIntroManually();
		}
		else
		{
			Debug.Log("Could not trigger boss intro.  FinalBossRoomController not found in room.");
		}
	}

	private IEnumerator MovePlayerToCain()
	{
		PlayerManager.GetPlayerController().SetVelocity(0f, 0f, additive: false);
		Vector3 position = m_playerPositionObj.transform.position;
		PlayerMovementHelper.StopAllMovementInput();
		yield return PlayerMovementHelper.MoveTo(position, checkXAxisOnly: true);
		if (m_prop.transform.localScale.x > 0f)
		{
			PlayerManager.GetPlayerController().SetFacing(faceRight: false);
		}
		else
		{
			PlayerManager.GetPlayerController().SetFacing(faceRight: true);
		}
		PlayerMovementHelper.ResumeAllMovementInput();
	}

	protected override void DisableProp(bool firstTimeDisabled)
	{
	}
}
