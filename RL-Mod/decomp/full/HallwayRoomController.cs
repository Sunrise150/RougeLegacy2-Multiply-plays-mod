using System;
using System.Collections;
using FMODUnity;
using RLAudio;
using RL_Windows;
using UnityEngine;

public class HallwayRoomController : BaseSpecialRoomController
{
	[SerializeField]
	private TunnelSpawnController m_closetTunnelSpawner;

	[SerializeField]
	private PropSpawnController[] m_memoryProps;

	private Action m_displayNextMemory;

	private int m_memoryIndex;

	protected override void Awake()
	{
		base.Awake();
		m_displayNextMemory = DisplayNextMemory;
	}

	protected override void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		base.OnPlayerEnterRoom(sender, eventArgs);
		StartCoroutine(FlipPlayer());
		StartCoroutine(FixTunnelLayer());
		PropSpawnController[] memoryProps = m_memoryProps;
		foreach (PropSpawnController obj in memoryProps)
		{
			obj.PropInstance.Pivot.gameObject.SetActive(value: false);
			obj.PropInstance.GetComponent<StudioEventEmitter>().Stop();
		}
		m_memoryIndex = 0;
		SetMemoriesEnabled();
		StartCoroutine(StartEarthShiftChallengeCoroutine());
	}

	private IEnumerator FixTunnelLayer()
	{
		yield return null;
		SpriteRenderer componentInChildren = m_closetTunnelSpawner.Tunnel.GetComponentInChildren<SpriteRenderer>();
		componentInChildren.gameObject.layer = 24;
		Vector3 localPosition = componentInChildren.transform.localPosition;
		localPosition.z = 5f;
		componentInChildren.transform.localPosition = localPosition;
	}

	private IEnumerator StartEarthShiftChallengeCoroutine()
	{
		float delay2 = Time.time + 2f;
		while (Time.time < delay2)
		{
			yield return null;
		}
		bool readingMemory = WindowManager.GetIsWindowOpen(WindowID.Dialogue);
		while (WindowManager.GetIsWindowOpen(WindowID.Dialogue))
		{
			yield return null;
		}
		if (readingMemory)
		{
			delay2 = Time.time + 0.5f;
			while (Time.time < delay2)
			{
				yield return null;
			}
		}
		AudioManager.PlayOneShot(null, "event:/SFX/Interactables/sfx_pizzaGirl_greeting", CameraController.GameCamera.transform.position);
		DialogueManager.StartNewDialogue();
		DialogueManager.AddDialogue(NPCDialogue_EV.GetNPCTitleLocID(NPCType.PizzaGirl), "LOC_ID_PIZZA_GIRL_ENDING_TELLING_PLAYER_TO_FOLLOW_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		yield return null;
		while (WindowManager.GetIsWindowOpen(WindowID.Dialogue))
		{
			yield return null;
		}
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.ResetGlobalTimer, null, null);
		GlobalTimerHUDController.ReverseTimer = true;
		GlobalTimerHUDController.ReverseStartTime = 600f;
		GlobalTimerHUDController.TrackNegativeTimeAchievement = true;
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.StartGlobalTimer, null, null);
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayGlobalTimer, null, null);
	}

	private void SetMemoriesEnabled()
	{
		if (BurdenManager.IsBurdenActive(BurdenType.FinalBossUp) && !m_memoryProps.IsNativeNull() && m_memoryProps.Length != 0)
		{
			for (int i = 0; i <= m_memoryIndex; i++)
			{
				SetMemoryActive(i);
			}
		}
	}

	private void SetMemoryActive(int index)
	{
		Prop propInstance = m_memoryProps[index].PropInstance;
		propInstance.Pivot.gameObject.SetActive(value: true);
		propInstance.GetComponent<StudioEventEmitter>().Play();
		if (index == m_memoryIndex)
		{
			propInstance.GetComponent<JournalSpecialPropController>().FinishedReadingRelay.AddListener(m_displayNextMemory);
		}
	}

	private void DisplayNextMemory()
	{
		m_memoryProps[m_memoryIndex].PropInstance.GetComponent<JournalSpecialPropController>().FinishedReadingRelay.RemoveListener(m_displayNextMemory);
		if (m_memoryIndex < m_memoryProps.Length - 1)
		{
			m_memoryIndex++;
			SetMemoryActive(m_memoryIndex);
		}
	}

	private IEnumerator FlipPlayer()
	{
		yield return null;
		PlayerController playerController = PlayerManager.GetPlayerController();
		if (!playerController.IsFacingRight)
		{
			playerController.CharacterCorgi.Flip(IgnoreFlipOnDirectionChange: true, ignoreTurnParam: true);
		}
	}
}
