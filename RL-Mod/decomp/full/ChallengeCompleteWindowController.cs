using System;
using System.Collections;
using RL_Windows;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChallengeCompleteWindowController : WindowController, ILocalizable
{
	[SerializeField]
	private Image m_ray;

	[SerializeField]
	private GameObject m_biomeLightController;

	[SerializeField]
	private TMP_Text m_bannerName;

	[SerializeField]
	private TMP_Text m_challengeTitle;

	[SerializeField]
	private TMP_Text m_classTitle;

	[SerializeField]
	private PlayerLookController m_playerModel;

	[SerializeField]
	private ChallengeCompleteStatsEntry[] m_statsEntries;

	[SerializeField]
	private ChallengeCompleteStatsEntry m_ratingEntry;

	[SerializeField]
	private TMP_Text m_trophyAward;

	[SerializeField]
	private Image m_trophyImage;

	[SerializeField]
	private GameObject m_empathiesUnlockedGO;

	private CanvasGroup m_canvasGroup;

	private ChallengeTrophyRank m_refreshTextNewRank;

	private ChallengeTrophyRank m_refreshTextGoalRank;

	private Action<MonoBehaviour, EventArgs> m_refreshText;

	private Action<InputActionEventData> m_onCancelButtonDown;

	public override WindowID ID => WindowID.ChallengeComplete;

	private void Awake()
	{
		m_refreshText = RefreshText;
		m_onCancelButtonDown = OnCancelButtonDown;
	}

	public override void Initialize()
	{
		m_canvasGroup = m_windowCanvas.GetComponent<CanvasGroup>();
		base.Initialize();
		if (m_playerModel.VisualsGameObject != null)
		{
			m_playerModel.VisualsGameObject.SetLayerRecursively(5, includeInactive: true);
		}
	}

	protected override void OnOpen()
	{
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.LanguageChanged, m_refreshText);
		UpdateChallengeScoreAndRank();
		m_windowCanvas.gameObject.SetActive(value: true);
		m_playerModel.InitializeLook(SaveManager.PlayerSaveData.CurrentCharacter);
		m_playerModel.Animator.SetBool("Victory", value: true);
		m_playerModel.Animator.Play("Victory", 0, 1f);
		m_biomeLightController.SetActive(value: true);
		StartCoroutine(OnOpenCoroutine());
	}

	private void UpdateChallengeScoreAndRank()
	{
		ChallengeType challengeType = ChallengeManager.ActiveChallenge.ChallengeType;
		ChallengeTrophyRank challengeTrophyRank = ChallengeManager.GetChallengeTrophyRank(challengeType);
		ChallengeManager.UpdateChallengeScore(challengeType, SaveManager.PlayerSaveData.CurrentCharacter.ClassType, Mathf.RoundToInt(ChallengeManager.CalculateActiveChallengeScore(ChallengeCompleteStatsEntryType.FinalScore)), GlobalTimerHUDController.ElapsedTime, ChallengeManager.GetChallengeEquippedLevel(challengeType) > 0);
		m_empathiesUnlockedGO.SetActive(value: false);
		m_trophyAward.gameObject.SetActive(value: false);
		ChallengeTrophyRank challengeTrophyRank2 = ChallengeManager.GetChallengeTrophyRank(challengeType, checkForSisyphusTrophy: false);
		if (challengeTrophyRank2 != challengeTrophyRank)
		{
			m_trophyAward.gameObject.SetActive(value: true);
			switch (challengeTrophyRank2)
			{
			case ChallengeTrophyRank.Bronze:
				m_trophyAward.text = LocalizationManager.GetString("LOC_ID_CHALLENGE_UI_END_BRONZE_REWARD_1", isFemale: false);
				m_trophyImage.sprite = IconLibrary.GetChallengeIcon(challengeType, ChallengeLibrary.ChallengeIconEntryType.Bronze);
				break;
			case ChallengeTrophyRank.Silver:
				m_trophyAward.text = LocalizationManager.GetString("LOC_ID_CHALLENGE_UI_END_SILVER_REWARD_1", isFemale: false);
				m_trophyImage.sprite = IconLibrary.GetChallengeIcon(challengeType, ChallengeLibrary.ChallengeIconEntryType.Silver);
				break;
			case ChallengeTrophyRank.Gold:
				m_trophyAward.text = LocalizationManager.GetString("LOC_ID_CHALLENGE_UI_END_GOLD_REWARD_1", isFemale: false);
				m_trophyImage.sprite = IconLibrary.GetChallengeIcon(challengeType, ChallengeLibrary.ChallengeIconEntryType.Gold);
				if (challengeType != ChallengeType.Tutorial && challengeType != ChallengeType.TutorialPurified)
				{
					m_empathiesUnlockedGO.SetActive(value: true);
				}
				break;
			}
		}
		ChallengeTrophyRank challengeTrophyRank3 = challengeTrophyRank2;
		if (challengeTrophyRank2 == challengeTrophyRank && challengeTrophyRank2 < ChallengeTrophyRank.Gold)
		{
			challengeTrophyRank3 = challengeTrophyRank2 + 1;
		}
		if (challengeTrophyRank2 == ChallengeTrophyRank.Gold || challengeTrophyRank == ChallengeTrophyRank.Gold)
		{
			ChallengeObj challenge = ChallengeManager.GetChallenge(challengeType);
			ChallengeManager.SetUpgradeBlueprintsFound(challengeType, challenge.MaxLevel, additive: false);
		}
		UpdateAllStats(challengeTrophyRank3);
		m_refreshTextNewRank = challengeTrophyRank2;
		m_refreshTextGoalRank = challengeTrophyRank3;
		if (challengeType == ChallengeType.NightmareKhidr)
		{
			StoreAPIManager.GiveAchievement(AchievementType.NightmareKhidr, StoreType.All);
		}
		if (ChallengeManager.GetTotalTrophiesEarnedOfRankOrHigher(ChallengeTrophyRank.Bronze, includeTutorialPurified: true) >= ChallengeManager.GetTotalTrophyCount())
		{
			StoreAPIManager.GiveAchievement(AchievementType.AllScarsBronze, StoreType.All);
		}
	}

	private IEnumerator OnOpenCoroutine()
	{
		m_canvasGroup.alpha = 0f;
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		yield return TweenManager.TweenTo_UnscaledTime(m_canvasGroup, 0.25f, Ease.None, "alpha", 1).TweenCoroutine;
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
	}

	protected override void OnClose()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.LanguageChanged, m_refreshText);
		m_windowCanvas.gameObject.SetActive(value: false);
		m_biomeLightController.SetActive(value: false);
		m_trophyAward.gameObject.SetActive(value: false);
	}

	protected override void OnFocus()
	{
		base.RewiredPlayer.AddInputEventDelegate(m_onCancelButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
		base.RewiredPlayer.AddInputEventDelegate(m_onCancelButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
	}

	protected override void OnLostFocus()
	{
		base.RewiredPlayer.RemoveInputEventDelegate(m_onCancelButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
		base.RewiredPlayer.RemoveInputEventDelegate(m_onCancelButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
	}

	private void OnCancelButtonDown(InputActionEventData data)
	{
		WindowManager.SetWindowIsOpen(WindowID.ChallengeComplete, isOpen: false);
	}

	private void UpdateAllStats(ChallengeTrophyRank goalRank)
	{
		CharacterData currentCharacter = SaveManager.PlayerSaveData.CurrentCharacter;
		m_bannerName.text = LocalizationManager.GetLocalizedPlayerName(currentCharacter);
		if (ChallengeManager.ActiveChallenge.ChallengeType == ChallengeType.TutorialPurified)
		{
			m_challengeTitle.text = LocalizationManager.GetString(ChallengeManager.GetChallenge(ChallengeType.Tutorial).ChallengeData.Title, isFemale: false);
		}
		else
		{
			m_challengeTitle.text = LocalizationManager.GetString(ChallengeManager.ActiveChallenge.ChallengeData.Title, isFemale: false);
		}
		m_classTitle.text = string.Format(LocalizationManager.GetString("LOC_ID_CHALLENGE_UI_END_CLASS_TITLE_1", currentCharacter.IsFemale), LocalizationManager.GetString(ClassLibrary.GetClassData(currentCharacter.ClassType).PassiveData.Title, currentCharacter.IsFemale));
		ChallengeCompleteStatsEntry[] statsEntries = m_statsEntries;
		for (int i = 0; i < statsEntries.Length; i++)
		{
			statsEntries[i].UpdateStat(goalRank);
		}
		m_ratingEntry.UpdateStat(goalRank);
	}

	private void Update()
	{
		if (m_windowCanvas.gameObject.activeSelf)
		{
			float num = -5f * Time.unscaledDeltaTime;
			m_ray.transform.SetLocalEulerZ(m_ray.transform.localEulerAngles.z + num);
		}
	}

	public void RefreshText(object sender, EventArgs args)
	{
		if (m_refreshTextNewRank != 0)
		{
			switch (m_refreshTextNewRank)
			{
			case ChallengeTrophyRank.Bronze:
				m_trophyAward.text = LocalizationManager.GetString("LOC_ID_CHALLENGE_UI_END_BRONZE_REWARD_1", isFemale: false);
				break;
			case ChallengeTrophyRank.Silver:
				m_trophyAward.text = LocalizationManager.GetString("LOC_ID_CHALLENGE_UI_END_SILVER_REWARD_1", isFemale: false);
				break;
			case ChallengeTrophyRank.Gold:
				m_trophyAward.text = LocalizationManager.GetString("LOC_ID_CHALLENGE_UI_END_GOLD_REWARD_1", isFemale: false);
				break;
			}
		}
		if (m_refreshTextGoalRank != 0)
		{
			UpdateAllStats(m_refreshTextGoalRank);
		}
	}
}
