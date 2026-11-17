using System;
using RLAudio;
using RL_Windows;
using UnityEngine;

public class RebelTunnel : Tunnel
{
	private Action m_enterTunnelAfterDialogue;

	protected override void Awake()
	{
		m_enterTunnelAfterDialogue = EnterTunnelAfterDialogue;
		base.Awake();
	}

	protected override void OnEnable()
	{
		bool value = SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.RebelKey) > 0 && SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.Rebel_Door_Opened);
		base.Animator.SetBool("Open", value);
		base.OnEnable();
	}

	protected override void OnPlayerInteractedWithTunnel(GameObject otherObj)
	{
		if (SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.RebelKey) > 0)
		{
			if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.Rebel_Door_Opened))
			{
				SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.Rebel_Door_Opened, value: true);
				base.Animator.SetBool("Open", value: true);
				AudioManager.PlayOneShotAttached(null, "event:/SFX/Interactables/sfx_env_prop_trapDoor_open", base.gameObject);
				DialogueManager.StartNewDialogue();
				DialogueManager.AddDialogue("LOC_ID_REBEL_HIDEOUT_DOOR_LOCKED_TITLE_1", "LOC_ID_REBEL_HIDEOUT_DOOR_UNLOCKED_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
				DialogueManager.AddDialogueCompleteEndHandler(m_enterTunnelAfterDialogue);
				WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
			}
			else
			{
				base.OnPlayerInteractedWithTunnel(null);
			}
		}
		else
		{
			DialogueManager.StartNewDialogue();
			DialogueManager.AddDialogue("LOC_ID_REBEL_HIDEOUT_DOOR_LOCKED_TITLE_1", "LOC_ID_REBEL_HIDEOUT_DOOR_LOCKED_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		}
	}

	private void EnterTunnelAfterDialogue()
	{
		base.OnPlayerInteractedWithTunnel(null);
	}
}
