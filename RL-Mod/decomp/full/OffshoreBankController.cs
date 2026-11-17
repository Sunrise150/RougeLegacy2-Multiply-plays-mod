using System;
using FMODUnity;
using RLAudio;
using RL_Windows;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class OffshoreBankController : BaseSpecialPropController, IDisplaySpeechBubble, IAudioEventEmitter
{
	[SerializeField]
	private TMP_Text m_goldSavedText;

	[SerializeField]
	private GameObject m_sprite;

	[SerializeField]
	private GameObject m_pizzaSprite;

	[SerializeField]
	private GameObject m_christmasSprite;

	[SerializeField]
	private UnityEvent m_goldSavedUnityEvent;

	[SerializeField]
	[EventRef]
	private string m_greetingAudioEvent;

	[SerializeField]
	[EventRef]
	private string m_farewellAudioEvent;

	private Vector3 m_storedScale;

	private int m_goldSavedDiff;

	private NPCController m_npcController;

	private Action<MonoBehaviour, EventArgs> m_onGoldSavedChanged;

	private Action m_closeBank;

	private bool m_endingSpeechBubblePlayed;

	public SpeechBubbleType BubbleType => SpeechBubbleType.Dialogue;

	public bool ShouldDisplaySpeechBubble => HasEventDialogue();

	public string Description => ToString();

	private bool HasEventDialogue()
	{
		if (SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround && !SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SeenParade))
		{
			return !m_endingSpeechBubblePlayed;
		}
		return !SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.OffshoreBankDialogue_Intro);
	}

	protected override void Awake()
	{
		base.Awake();
		m_storedScale = m_goldSavedText.transform.localScale;
		m_goldSavedText.text = "";
		m_npcController = GetComponent<NPCController>();
		m_onGoldSavedChanged = OnGoldSavedChanged;
		m_closeBank = CloseBank;
	}

	protected override void InitializePooledPropOnEnter()
	{
		m_endingSpeechBubblePlayed = false;
		m_goldSavedText.text = "[Coin_Icon] " + SaveManager.PlayerSaveData.GoldSaved;
		EndingSpawnRoomTypeController component = base.Room.GetComponent<EndingSpawnRoomTypeController>();
		if (!component || component.EndingSpawnRoomType != EndingSpawnRoomType.AboveGround)
		{
			m_goldSavedText.gameObject.SetLayerRecursively(25);
			m_interactable.SpeechBubble.gameObject.SetLayerRecursively(25);
			if (!m_goldSavedText.gameObject.activeSelf)
			{
				m_goldSavedText.gameObject.SetActive(value: true);
			}
		}
		else if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SeenParade))
		{
			if (m_goldSavedText.gameObject.activeSelf)
			{
				m_goldSavedText.gameObject.SetActive(value: false);
			}
		}
		else if (!m_goldSavedText.gameObject.activeSelf)
		{
			m_goldSavedText.gameObject.SetActive(value: true);
		}
		Vector3 localScale = m_goldSavedText.transform.localScale;
		if (m_goldSavedText.transform.lossyScale.x < 0f)
		{
			localScale.x *= -1f;
			m_goldSavedText.transform.localScale = localScale;
		}
	}

	private void OnEnable()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.GoldSavedChanged, m_onGoldSavedChanged);
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.PizzaGirlUnlocked))
		{
			if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
			{
				m_pizzaSprite.SetActive(value: true);
			}
			else
			{
				m_pizzaSprite.SetActive(value: false);
			}
			m_pizzaSprite.layer = m_pizzaSprite.transform.parent.gameObject.layer;
		}
		if (HolidayLookController.IsHoliday(HolidayType.Christmas))
		{
			if (m_sprite.activeSelf)
			{
				m_sprite.SetActive(value: false);
			}
			if (!m_christmasSprite.activeSelf)
			{
				m_christmasSprite.SetActive(value: true);
			}
		}
		else
		{
			if (!m_sprite.activeSelf)
			{
				m_sprite.SetActive(value: true);
			}
			if (m_christmasSprite.activeSelf)
			{
				m_christmasSprite.SetActive(value: false);
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.GoldSavedChanged, m_onGoldSavedChanged);
	}

	private void OnGoldSavedChanged(object sender, EventArgs args)
	{
		CharonShop charonShop = sender as CharonShop;
		if (charonShop != null)
		{
			(EffectManager.PlayEffect(charonShop.gameObject, null, "GoldBurst_Effect", Vector3.zero, 0f, EffectStopType.Gracefully) as BurstEffect).DestinationOverride = base.transform;
			if (args != null)
			{
				GoldChangedEventArgs goldChangedEventArgs = args as GoldChangedEventArgs;
				m_goldSavedDiff = goldChangedEventArgs.NewGoldAmount - goldChangedEventArgs.PreviousGoldAmount;
			}
			else
			{
				m_goldSavedDiff = 0;
			}
			Invoke("UpdateTextAnim", 0.5f);
		}
		else
		{
			m_goldSavedText.text = "[Coin_Icon] " + SaveManager.PlayerSaveData.GoldSaved;
		}
	}

	public void UpdateTextAnim()
	{
		Vector3 localScale = m_storedScale + new Vector3(0.15f, 0.15f, 0.15f);
		m_goldSavedText.transform.localScale = localScale;
		TweenManager.TweenTo(m_goldSavedText.transform, 0.25f, Ease.Back.EaseOut, "localScale.x", m_storedScale.x, "localScale.y", m_storedScale.y, "localScale.z", m_storedScale.z);
		m_goldSavedText.text = "[Coin_Icon] " + SaveManager.PlayerSaveData.GoldSaved;
		TextPopupManager.DisplayTextAtAbsPos(TextPopupType.GoldCollected, m_goldSavedDiff.ToString(), new Vector2(0f, -1f), m_goldSavedText.gameObject);
		if (m_goldSavedUnityEvent != null)
		{
			m_goldSavedUnityEvent.Invoke();
		}
	}

	public void DisplayDialogue()
	{
		AudioManager.PlayOneShotAttached(this, m_greetingAudioEvent, base.gameObject);
		m_interactable.SetIsInteractableActive(isActive: false);
		if (SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround && !SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SeenParade))
		{
			RunEndingDialogue();
		}
		else if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.OffshoreBankDialogue_Intro))
		{
			SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.OffshoreBankDialogue_Intro, value: true);
			DialogueManager.StartNewDialogue(m_npcController);
			DialogueManager.AddDialogue("LOC_ID_SAFE_TITLE_NAME_1", "LOC_ID_SAFE_DIALOGUE_INTRO_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
			DialogueManager.AddDialogueCompleteEndHandler(m_closeBank);
		}
		else if (NPCDialogueManager.CanSpeak(m_npcController.NPCType))
		{
			m_npcController.RunNextNPCDialogue(m_closeBank);
		}
		else
		{
			DisplayGoldStats();
		}
	}

	private void DisplayGoldStats()
	{
		int goldSaved = SaveManager.PlayerSaveData.GoldSaved;
		int num = 1000 + (int)SkillTreeManager.GetSkillTreeObj(SkillTreeType.Gold_Saved_Cap_Up).CurrentStatGain;
		float num2 = 0.1f + SkillTreeManager.GetSkillTreeObj(SkillTreeType.Gold_Saved_Amount_Saved).CurrentStatGain;
		int goldGivenToCharon = SaveManager.PlayerSaveData.GoldGivenToCharon;
		string @string = LocalizationManager.GetString("LOC_ID_SAFE_TITLE_NAME_1", isFemale: false);
		string text = "";
		if (SkillTreeManager.GetSkillObjLevel(SkillTreeType.Charon_Gold_Stat_Bonus) == 0)
		{
			text = LocalizationManager.GetString("LOC_ID_SAFE_DIALOGUE_STATS_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale, disableWordScramble: true);
			text = string.Format(text, goldSaved, num, Mathf.RoundToInt(num2 * 100f), goldGivenToCharon, "");
		}
		else
		{
			int charonGoldStatBonus = SkillTreeLogicHelper.GetCharonGoldStatBonus();
			int charonGoldRuneWeightBonus = SkillTreeLogicHelper.GetCharonGoldRuneWeightBonus();
			int charonGoldMilestoneCount = SkillTreeLogicHelper.GetCharonGoldMilestoneCount();
			int num3 = ((charonGoldMilestoneCount >= SkillTree_EV.GetMaxCharonBonusLevel()) ? int.MaxValue : (SkillTree_EV.CHARON_GOLD_STAT_BONUS_MILESTONES[charonGoldMilestoneCount] - SaveManager.PlayerSaveData.GoldAcceptedByCharon));
			text = ((num3 == int.MaxValue) ? LocalizationManager.GetString("LOC_ID_SAFE_DIALOGUE_CHARON_DONATION_MAXED_1", isFemale: false, disableWordScramble: true) : LocalizationManager.GetString("LOC_ID_SAFE_DIALOGUE_STATS_2", isFemale: false, disableWordScramble: true));
			text = string.Format(text, goldSaved, num, Mathf.RoundToInt(num2 * 100f), goldGivenToCharon, "", charonGoldStatBonus, num3.ToString(), charonGoldMilestoneCount, charonGoldRuneWeightBonus);
		}
		DialogueManager.StartNewDialogue(m_npcController);
		DialogueManager.AddNonLocDialogue(@string, text);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		DialogueManager.AddDialogueCompleteEndHandler(m_closeBank);
	}

	private void RunEndingDialogue()
	{
		string textLocID;
		if (m_endingSpeechBubblePlayed)
		{
			textLocID = (m_npcController.IsBestFriend ? "LOC_ID_LIVING_SAFE_OUTRO_FRIENDS_REPEAT_1" : "LOC_ID_LIVING_SAFE_OUTRO_STRANGER_REPEAT_1");
		}
		else
		{
			m_endingSpeechBubblePlayed = true;
			textLocID = (m_npcController.IsBestFriend ? "LOC_ID_LIVING_SAFE_OUTRO_FRIENDS_INTRO_1" : "LOC_ID_LIVING_SAFE_OUTRO_STRANGER_INTRO_1");
		}
		DialogueManager.StartNewDialogue(m_npcController);
		DialogueManager.AddDialogue(NPCDialogue_EV.GetNPCTitleLocID(m_npcController.NPCType), textLocID, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		DialogueManager.AddDialogueCompleteEndHandler(m_closeBank);
	}

	private void CloseBank()
	{
		AudioManager.PlayOneShotAttached(this, m_farewellAudioEvent, base.gameObject);
		m_interactable.SetIsInteractableActive(isActive: true);
	}
}
