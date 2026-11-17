using System;
using System.Collections;
using RL_Windows;
using UnityEngine;

public class DialogueDisplayController : MonoBehaviour, IDisplaySpeechBubble
{
	[SerializeField]
	protected bool m_useLocID = true;

	[SerializeField]
	protected string m_dialogue;

	[SerializeField]
	protected string m_repeatedDialogue;

	[SerializeField]
	protected string m_speaker;

	[SerializeField]
	protected PlayerSaveFlag m_spokenFlag;

	[SerializeField]
	protected DialogueWindowStyle m_dialogueWindowStyle;

	[SerializeField]
	protected GameObject m_playerPositionObj;

	protected NPCController m_npcController;

	private Action m_stopDialogue;

	public bool ShouldDisplaySpeechBubble => true;

	public SpeechBubbleType BubbleType => SpeechBubbleType.Dialogue;

	protected virtual void Awake()
	{
		m_npcController = GetComponent<NPCController>();
		m_stopDialogue = StopDialogue;
	}

	public void DisplayDialogue()
	{
		StartCoroutine(DisplayDialogueCoroutine());
	}

	protected virtual IEnumerator DisplayDialogueCoroutine()
	{
		if ((bool)m_npcController)
		{
			m_npcController.SetNPCState(NPCState.AtAttention);
		}
		if ((bool)m_playerPositionObj)
		{
			yield return MovePlayerToPos();
		}
		StartDialogue();
	}

	protected virtual void StartDialogue()
	{
		bool flag = false;
		PlayerSaveFlag playerSaveFlag = m_spokenFlag;
		if (playerSaveFlag != 0 && SaveManager.PlayerSaveData.GetFlag(playerSaveFlag))
		{
			flag = true;
		}
		bool flag2 = m_useLocID;
		string text = ((!flag) ? m_dialogue : m_repeatedDialogue);
		string text2 = m_speaker;
		if ((bool)m_npcController)
		{
			text2 = NPCDialogue_EV.GetNPCTitleLocID(m_npcController.NPCType);
		}
		Prop component = GetComponent<Prop>();
		if ((bool)component && (bool)component.PropSpawnController)
		{
			DialogueDisplayOverride component2 = component.PropSpawnController.gameObject.GetComponent<DialogueDisplayOverride>();
			if ((bool)component2)
			{
				playerSaveFlag = component2.SpokenFlagOverride;
				flag = false;
				if (playerSaveFlag != 0 && SaveManager.PlayerSaveData.GetFlag(playerSaveFlag))
				{
					flag = true;
				}
				flag2 = component2.UseLocIDOverride;
				text = ((!flag) ? component2.DialogueOverride : component2.RepeatedDialogueOverride);
				if (!string.IsNullOrEmpty(component2.SpeakerOverride))
				{
					text2 = component2.SpeakerOverride;
				}
			}
		}
		DialogueManager.StartNewNPCDialogue(m_npcController);
		if (flag2)
		{
			DialogueManager.AddDialogue(text2, text, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale, m_dialogueWindowStyle);
		}
		else
		{
			DialogueManager.AddNonLocDialogue(text2, text, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale, m_dialogueWindowStyle);
		}
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		DialogueManager.AddDialogueCompleteEndHandler(m_stopDialogue);
		if (!flag && playerSaveFlag != 0)
		{
			SaveManager.PlayerSaveData.SetFlag(playerSaveFlag, value: true);
		}
	}

	protected virtual void StopDialogue()
	{
		if ((bool)m_npcController)
		{
			m_npcController.SetNPCState(NPCState.Idle);
		}
	}

	protected virtual IEnumerator MovePlayerToPos()
	{
		PlayerManager.GetPlayerController().SetVelocity(0f, 0f, additive: false);
		Vector3 position = m_playerPositionObj.transform.position;
		PlayerMovementHelper.StopAllMovementInput();
		yield return PlayerMovementHelper.MoveTo(position, checkXAxisOnly: true);
		if (m_playerPositionObj.transform.lossyScale.x > 0f)
		{
			PlayerManager.GetPlayerController().SetFacing(faceRight: false);
		}
		else
		{
			PlayerManager.GetPlayerController().SetFacing(faceRight: true);
		}
		float startTime = Time.time;
		while (Time.time < startTime + 0.25f)
		{
			yield return null;
		}
		PlayerMovementHelper.ResumeAllMovementInput();
	}
}
