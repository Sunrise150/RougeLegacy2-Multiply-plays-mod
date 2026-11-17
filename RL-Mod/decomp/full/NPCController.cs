using System;
using System.Collections;
using FMODUnity;
using RLAudio;
using RL_Windows;
using UnityEngine;

public class NPCController : MonoBehaviour, IDisplaySpeechBubble, IAudioEventEmitter
{
	[SerializeField]
	private NPCType m_npcType;

	[SerializeField]
	[EventRef]
	private string m_eatPizzaAudioEvent;

	[SerializeField]
	private GameObject m_heartGO;

	private static int NPC_ANIMATOR_PARAM = Animator.StringToHash("NPC");

	private static int m_idleHash = Animator.StringToHash("Idle");

	private static int m_atAttentionHash = Animator.StringToHash("StandingAtAttention");

	private static int m_speakingHash = Animator.StringToHash("Speaking");

	private static int m_speakingPauseHash = Animator.StringToHash("SpeakingPause");

	private static int m_emote1Hash = Animator.StringToHash("Emote1");

	private static int m_emote2Hash = Animator.StringToHash("Emote2");

	private static int EAT_PIZZA_ANIMATOR_STATE_HASH = Animator.StringToHash("EatPizza");

	private Coroutine m_eatPizzaCoroutine;

	private bool m_animatorNPCTypeAssigned;

	private Action<MonoBehaviour, EventArgs> m_onSkillTreeOpened;

	private Action<MonoBehaviour, EventArgs> m_onSkillTreeClosed;

	public string Description => ToString();

	public bool ShouldDisplaySpeechBubble => NPCDialogueManager.CanSpeak(m_npcType);

	public SpeechBubbleType BubbleType => SpeechBubbleType.Dialogue;

	public NPCType NPCType => m_npcType;

	public Animator Animator { get; private set; }

	public NPCState CurrentState { get; private set; } = NPCState.Idle;


	public bool IsBestFriend => GetBestFriendState(m_npcType);

	private void Awake()
	{
		Animator = GetComponent<Animator>();
		m_onSkillTreeOpened = OnSkillTreeOpened;
		m_onSkillTreeClosed = OnSkillTreeClosed;
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.SkillTree_Opened, m_onSkillTreeOpened);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.SkillTree_Closed, m_onSkillTreeClosed);
	}

	private void OnEnable()
	{
		if ((bool)Animator)
		{
			AssignAnimatorNPCType();
			EatPizza();
		}
		UpdateHeartState();
	}

	private void AssignAnimatorNPCType()
	{
		if ((bool)Animator && !m_animatorNPCTypeAssigned)
		{
			if (AnimatorUtility.HasParameter(Animator, NPC_ANIMATOR_PARAM))
			{
				Animator.SetInteger(NPC_ANIMATOR_PARAM, (int)m_npcType);
			}
			m_animatorNPCTypeAssigned = true;
		}
	}

	private void OnDisable()
	{
		m_animatorNPCTypeAssigned = false;
	}

	private void OnSkillTreeOpened(object sender, EventArgs args)
	{
		if ((bool)Animator)
		{
			Animator.enabled = false;
		}
		if (m_eatPizzaCoroutine != null)
		{
			StopCoroutine(m_eatPizzaCoroutine);
		}
	}

	private void OnSkillTreeClosed(object sender, EventArgs args)
	{
		if ((bool)Animator)
		{
			Animator.enabled = true;
		}
		if (base.gameObject.activeInHierarchy)
		{
			EatPizza();
		}
	}

	private void OnDestroy()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.SkillTree_Opened, m_onSkillTreeOpened);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.SkillTree_Closed, m_onSkillTreeClosed);
	}

	private int GetStateHash(NPCState state)
	{
		return state switch
		{
			NPCState.Idle => m_idleHash, 
			NPCState.AtAttention => m_atAttentionHash, 
			NPCState.Speaking => m_speakingHash, 
			NPCState.SpeakingPause => m_speakingPauseHash, 
			NPCState.Emote1 => m_emote1Hash, 
			NPCState.Emote2 => m_emote2Hash, 
			_ => 0, 
		};
	}

	public void SetNPCState(NPCState state, bool skipTransition = false)
	{
		if ((bool)Animator)
		{
			Animator.SetBool(GetStateHash(NPCState.Idle), value: false);
			Animator.SetBool(GetStateHash(NPCState.AtAttention), value: false);
			Animator.SetBool(GetStateHash(NPCState.Speaking), value: false);
			Animator.SetBool(GetStateHash(NPCState.SpeakingPause), value: false);
			Animator.SetBool(GetStateHash(NPCState.Emote1), value: false);
			Animator.SetBool(GetStateHash(NPCState.Emote2), value: false);
			Animator.SetBool(GetStateHash(state), value: true);
			AssignAnimatorNPCType();
			if (skipTransition)
			{
				Animator.Update(1f);
				Animator.Update(1f);
			}
			CurrentState = state;
			EatPizza();
		}
	}

	private void EatPizza()
	{
		if (m_eatPizzaCoroutine != null)
		{
			StopCoroutine(m_eatPizzaCoroutine);
		}
		if (CanEatPizza() && SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.PizzaGirlUnlocked) && CurrentState == NPCState.Idle)
		{
			m_eatPizzaCoroutine = StartCoroutine(EatPizzaCoroutine());
		}
	}

	private IEnumerator EatPizzaCoroutine()
	{
		float delay3 = UnityEngine.Random.Range(5f, 45f);
		delay3 += Time.time;
		while (Time.time < delay3)
		{
			yield return null;
		}
		if (string.IsNullOrEmpty(m_eatPizzaAudioEvent))
		{
			Debug.Log("<color=red>NPC (" + m_npcType.ToString() + ") attempting to eat pizza without an assigned audio event!</color>");
		}
		AudioManager.PlayOneShotAttached(this, m_eatPizzaAudioEvent, base.gameObject);
		Animator.Play(EAT_PIZZA_ANIMATOR_STATE_HASH);
		delay3 = Time.time + UnityEngine.Random.Range(20f, 60f);
		while (Time.time < delay3)
		{
			yield return null;
		}
		EatPizza();
	}

	public void RunNextNPCDialogue(Action onDialogueComplete = null)
	{
		int index = SaveManager.PlayerSaveData.GetNPCDialoguesRead(NPCType) + 1;
		RunNPCDialogueAtIndex(index, onDialogueComplete);
		SaveManager.PlayerSaveData.TriggerGlobalNPCDialogueCD = true;
	}

	public void RepeatLastNPCDialogue(Action onDialogueComplete = null)
	{
		int nPCDialoguesRead = SaveManager.PlayerSaveData.GetNPCDialoguesRead(NPCType);
		nPCDialoguesRead = Mathf.Clamp(nPCDialoguesRead, 0, NPCDialogue_EV.NPCDialogueTable[NPCType].Length - 1);
		RunNPCDialogueAtIndex(nPCDialoguesRead, onDialogueComplete);
	}

	private void RunNPCDialogueAtIndex(int index, Action onDialogueComplete = null)
	{
		if (!NPCDialogue_EV.NPCDialogueTable.TryGetValue(NPCType, out var value))
		{
			return;
		}
		if (value != null)
		{
			if (index < value.Length)
			{
				NPCDialogueEntry nPCDialogueEntry = NPCDialogue_EV.NPCDialogueTable[NPCType][index];
				string text = nPCDialogueEntry.TitleLocID;
				if (string.IsNullOrEmpty(text))
				{
					text = NPCDialogue_EV.GetNPCTitleLocID(NPCType);
				}
				DialogueManager.StartNewNPCDialogue(this);
				DialogueManager.AddDialogue(text, nPCDialogueEntry.LocID, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
				WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
				if (onDialogueComplete != null)
				{
					DialogueManager.AddDialogueCompleteEndHandler(onDialogueComplete);
				}
			}
			else
			{
				RepeatLastNPCDialogue(onDialogueComplete);
			}
		}
		else if (onDialogueComplete != null)
		{
			DialogueManager.AddDialogueCompleteEndHandler(onDialogueComplete);
		}
	}

	public void UpdateHeartState()
	{
		if (SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround)
		{
			if ((bool)m_heartGO)
			{
				m_heartGO.SetActive(IsBestFriend);
			}
		}
		else if ((bool)m_heartGO && m_heartGO.activeSelf)
		{
			m_heartGO.SetActive(value: false);
		}
	}

	public static bool GetBestFriendState(NPCType npcType)
	{
		switch (npcType)
		{
		case NPCType.ChallengeHood:
			return ChallengeManager.GetChallengeTrophyRank(ChallengeType.TutorialPurified) >= ChallengeTrophyRank.Bronze;
		case NPCType.NewGamePlusHood:
		case NPCType.SoulShopHood:
			return SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.FinalBoss_Prime_Defeated_FirstTime);
		default:
			return NPCDialogueManager.AllNPCDialoguesRead(npcType);
		}
	}

	public void ShowHeart()
	{
		if ((bool)m_heartGO)
		{
			m_heartGO.SetActive(value: true);
		}
	}

	public void HideHeart()
	{
		if ((bool)m_heartGO)
		{
			m_heartGO.SetActive(value: false);
		}
	}

	private bool CanEatPizza()
	{
		NPCType nPCType = NPCType;
		if ((uint)nPCType <= 6u)
		{
			return true;
		}
		return false;
	}
}
