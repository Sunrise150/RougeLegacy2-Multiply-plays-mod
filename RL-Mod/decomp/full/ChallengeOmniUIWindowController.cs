using System;
using RLAudio;
using UnityEngine;

public class ChallengeOmniUIWindowController : BaseOmniUIWindowController<BaseOmniUICategoryEntry, ChallengeOmniUIEntry>
{
	[SerializeField]
	private GameObject m_descriptionGO;

	[SerializeField]
	private GameObject m_controlsGO;

	[SerializeField]
	private GameObject m_scoreboardGO;

	private bool m_scoreboardActive;

	private ChallengeOmniUIScoreboardEntry[] m_scoreboardEntries;

	private Action<object, EventArgs> m_onUpdateDescription;

	public override bool CanReset => true;

	public override WindowID ID => WindowID.ChallengeNPC;

	protected override void CreateCategoryEntries()
	{
	}

	protected override void Awake()
	{
		base.Awake();
		m_onUpdateDescription = OnUpdateDescription;
	}

	protected override void CreateEntries()
	{
		if (base.EntryArray != null)
		{
			Array.Clear(base.EntryArray, 0, base.EntryArray.Length);
			base.EntryArray = null;
		}
		Array cHALLENGE_ORDER = Challenge_EV.CHALLENGE_ORDER;
		base.EntryArray = new ChallengeOmniUIEntry[cHALLENGE_ORDER.Length];
		int num = 0;
		foreach (ChallengeType item in cHALLENGE_ORDER)
		{
			if (item != 0 && item != ChallengeType.TutorialPurified)
			{
				base.EntryArray[num] = UnityEngine.Object.Instantiate(m_entryPrefab);
				base.EntryArray[num].transform.SetParent(base.EntryLayoutGroup.transform);
				base.EntryArray[num].transform.localScale = Vector3.one;
				base.EntryArray[num].Initialize(item, this);
				base.EntryArray[num].SetEntryIndex(num);
				num++;
			}
		}
		m_scoreboardEntries = m_scoreboardGO.GetComponentsInChildren<ChallengeOmniUIScoreboardEntry>(includeInactive: true);
		m_descriptionGO.SetActive(value: true);
		m_controlsGO.SetActive(value: true);
		m_scoreboardGO.SetActive(value: false);
		base.ResetTextbox.SetActive(value: false);
	}

	protected override void UpdateScrollArrows(float scrollAmount)
	{
		base.UpdateScrollArrows(scrollAmount);
		if (!base.TopScrollArrow.activeSelf && !base.BottomScrollArrow.activeSelf)
		{
			return;
		}
		float num = (float)base.ActiveEntryArray.Length * m_entryHeight;
		float num2 = Mathf.Clamp(1f - base.ScrollBar.value, 0f, 1f);
		float height = base.ContentViewport.rect.height;
		float num3 = height;
		float num4 = num3 - height;
		if (num2 > 0f)
		{
			num3 = height + (num - height) * num2;
			num4 = num3 - height;
		}
		int num5 = Mathf.CeilToInt(num4 / m_entryHeight);
		int num6 = Mathf.FloorToInt(num3 / m_entryHeight);
		if (base.TopScrollArrow.activeSelf)
		{
			bool flag = false;
			for (int i = 0; i < num5; i++)
			{
				if (ChallengeManager.GetFoundState(base.ActiveEntryArray[i].ChallengeType) == FoundState.FoundButNotViewed)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				SetGameObjectActive(base.TopScrollNewSymbol.gameObject, active: true);
				SetGameObjectActive(base.TopScrollUpgradeSymbol.gameObject, active: false);
			}
			else
			{
				SetGameObjectActive(base.TopScrollNewSymbol.gameObject, active: false);
				SetGameObjectActive(base.TopScrollUpgradeSymbol.gameObject, active: false);
			}
		}
		if (!base.BottomScrollArrow.activeSelf)
		{
			return;
		}
		bool flag2 = false;
		for (int j = num6; j < base.ActiveEntryArray.Length; j++)
		{
			if (ChallengeManager.GetFoundState(base.ActiveEntryArray[j].ChallengeType) == FoundState.FoundButNotViewed)
			{
				flag2 = true;
				break;
			}
		}
		if (flag2)
		{
			SetGameObjectActive(base.BottomScrollNewSymbol.gameObject, active: true);
			SetGameObjectActive(base.BottomScrollUpgradeSymbol.gameObject, active: false);
		}
		else
		{
			SetGameObjectActive(base.BottomScrollNewSymbol.gameObject, active: false);
			SetGameObjectActive(base.BottomScrollUpgradeSymbol.gameObject, active: false);
		}
	}

	private void SetGameObjectActive(GameObject obj, bool active)
	{
		if (obj.activeSelf == !active)
		{
			obj.SetActive(active);
		}
	}

	protected override void OnOpen()
	{
		base.OnOpen();
		SetScoreboardActive(active: false, playAudio: false);
		SetGameObjectActive(base.ResetTextbox, active: false);
		if (ChallengeManager.GetTotalTrophiesEarnedOfRankOrHigher(ChallengeTrophyRank.Bronze, includeTutorialPurified: true) >= ChallengeManager.GetTotalTrophyCount())
		{
			StoreAPIManager.GiveAchievement(AchievementType.AllScarsBronze, StoreType.All);
		}
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.OmniUI_UpdateDescription, m_onUpdateDescription);
	}

	protected override void OnClose()
	{
		base.OnClose();
		if (PlayerManager.IsInstantiated)
		{
			PlayerManager.GetPlayerController().ResetBaseValues();
		}
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.ChallengeNPC_Menu_Closed, this, null);
		SaveManager.PlayerSaveData.UpdateCachedData();
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.OmniUI_UpdateDescription, m_onUpdateDescription);
	}

	private void OnUpdateDescription(object sender, EventArgs args)
	{
		SetScoreboardActive(active: false, playAudio: false);
		if (!(args is ChallengeOmniUIDescriptionEventArgs challengeOmniUIDescriptionEventArgs))
		{
			return;
		}
		ChallengeObj challenge = ChallengeManager.GetChallenge(challengeOmniUIDescriptionEventArgs.ChallengeType);
		if (challenge != null)
		{
			if (challenge.ScoringType == ChallengeScoringType.Battle && challengeOmniUIDescriptionEventArgs.ChallengeType != ChallengeType.Tutorial && challengeOmniUIDescriptionEventArgs.ChallengeType != ChallengeType.TutorialPurified)
			{
				SetGameObjectActive(base.ResetTextbox, active: true);
			}
			else
			{
				SetGameObjectActive(base.ResetTextbox, active: false);
			}
		}
	}

	protected override void OnYButtonJustPressed()
	{
		base.OnYButtonJustPressed();
		ChallengeObj challenge = ChallengeManager.GetChallenge(base.ActiveEntryArray[base.SelectedEntryIndex].ChallengeType);
		if (challenge != null)
		{
			FoundState foundState = challenge.FoundState;
			if (base.ResetTextbox.activeSelf && foundState != FoundState.NotFound)
			{
				SetScoreboardActive(!m_scoreboardActive, playAudio: true);
			}
		}
	}

	private void SetScoreboardActive(bool active, bool playAudio)
	{
		if (m_scoreboardActive == active)
		{
			return;
		}
		if (active)
		{
			ChallengeType challengeType = base.ActiveEntryArray[base.SelectedEntryIndex].ChallengeType;
			ChallengeOmniUIScoreboardEntry[] scoreboardEntries = m_scoreboardEntries;
			for (int i = 0; i < scoreboardEntries.Length; i++)
			{
				scoreboardEntries[i].UpdateEntry(challengeType);
			}
		}
		if (playAudio)
		{
			AudioManager.PlayOneShot(null, "event:/UI/FrontEnd/ui_fe_menu_tab_over");
		}
		SetGameObjectActive(m_descriptionGO, !active);
		SetGameObjectActive(m_controlsGO, !active);
		SetGameObjectActive(m_scoreboardGO, active);
		m_scoreboardActive = active;
	}

	public void ForceRefresh()
	{
		if (base.SelectedEntryIndex < base.ActiveEntryArray.Length)
		{
			base.ActiveEntryArray[base.SelectedEntryIndex].OnSelect(null);
		}
	}

	protected override void OnLBButtonJustPressed()
	{
	}

	protected override void OnRBButtonJustPressed()
	{
	}
}
