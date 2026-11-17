using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChallengeOmniUIEntry : BaseOmniUIEntry
{
	[SerializeField]
	private Image m_frame;

	private bool m_isEntryActive = true;

	private ChallengeOmniUIDescriptionEventArgs m_eventArgs;

	public ChallengeType ChallengeType { get; protected set; }

	public override EventArgs EntryEventArgs
	{
		get
		{
			if (m_eventArgs == null)
			{
				m_eventArgs = new ChallengeOmniUIDescriptionEventArgs(ChallengeType, OmniUIButtonType.Purchasing);
			}
			else
			{
				m_eventArgs.Initialize(ChallengeType, OmniUIButtonType.Purchasing);
			}
			return m_eventArgs;
		}
	}

	public override bool IsEntryActive => m_isEntryActive;

	public void Initialize(ChallengeType challengeType, ChallengeOmniUIWindowController windowController)
	{
		ChallengeType = challengeType;
		Initialize(windowController);
		OmniUIButton[] buttonArray = m_buttonArray;
		for (int i = 0; i < buttonArray.Length; i++)
		{
			((IChallengeOmniUIButton)buttonArray[i]).ChallengeType = ChallengeType;
		}
	}

	public override void UpdateActive()
	{
		ChallengeData challengeData = ChallengeLibrary.GetChallengeData(ChallengeType);
		if (challengeData == null || challengeData.Disabled)
		{
			if (base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: false);
			}
		}
		else if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
		}
	}

	public override void UpdateState()
	{
		ChallengeObj challenge = ChallengeManager.GetChallenge(ChallengeType);
		FoundState foundState = challenge.FoundState;
		if (foundState == FoundState.FoundButNotViewed)
		{
			if (!m_newSymbol.gameObject.activeSelf)
			{
				m_newSymbol.gameObject.SetActive(value: true);
			}
		}
		else if (m_newSymbol.gameObject.activeSelf)
		{
			m_newSymbol.gameObject.SetActive(value: false);
		}
		if (foundState > FoundState.NotFound)
		{
			m_icon.sprite = IconLibrary.GetChallengeIcon(ChallengeType, ChallengeLibrary.ChallengeIconEntryType.Challenge);
		}
		else
		{
			m_icon.sprite = IconLibrary.GetDefaultSprite();
		}
		if (foundState == FoundState.NotFound)
		{
			m_isEntryActive = false;
		}
		base.UpdateState();
		if (foundState == FoundState.NotFound)
		{
			m_isEntryActive = true;
		}
		if (foundState == FoundState.NotFound)
		{
			if (Challenge_EV.ScarBossRequirementTable.ContainsKey(ChallengeType) && SaveManager.PlayerSaveData.GetFlag(Challenge_EV.ScarBossRequirementTable[ChallengeType]))
			{
				m_titleText.text = LocalizationManager.GetString("LOC_ID_CHALLENGE_BOSS_REQUIREMENT_HINT_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			}
			else
			{
				m_titleText.text = LocalizationManager.GetString("LOC_ID_CHALLENGE_BOSS_REQUIREMENT_LOCKED_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			}
			return;
		}
		m_titleText.text = LocalizationManager.GetString(challenge.ChallengeData.Title, isFemale: false);
		ChallengeType challengeType = ChallengeType;
		if (challengeType == ChallengeType.Tutorial)
		{
			challengeType = ChallengeType.TutorialPurified;
		}
		switch (ChallengeManager.GetChallengeTrophyRank(challengeType))
		{
		case ChallengeTrophyRank.Bronze:
			m_frame.sprite = IconLibrary.GetMiscIcon(MiscIconType.OmniUIEntry_Frame_Bronze);
			break;
		case ChallengeTrophyRank.Silver:
			m_frame.sprite = IconLibrary.GetMiscIcon(MiscIconType.OmniUIEntry_Frame_Silver);
			break;
		case ChallengeTrophyRank.Gold:
			m_frame.sprite = IconLibrary.GetMiscIcon(MiscIconType.OmniUIEntry_Frame_Gold);
			break;
		default:
			m_frame.sprite = IconLibrary.GetMiscIcon(MiscIconType.OmniUIEntry_Frame_Default);
			break;
		}
	}

	public override void OnSelect(BaseEventData eventData)
	{
		if (base.Interactable)
		{
			if (ChallengeManager.GetFoundState(ChallengeType) == FoundState.FoundButNotViewed)
			{
				ChallengeManager.SetFoundState(ChallengeType, FoundState.FoundAndViewed, overrideValues: false);
				Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateState, this, null);
			}
			base.OnSelect(eventData);
		}
	}
}
