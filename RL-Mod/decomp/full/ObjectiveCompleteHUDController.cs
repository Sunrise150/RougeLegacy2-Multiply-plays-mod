using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ObjectiveCompleteHUDController : MonoBehaviour, ILocalizable
{
	[SerializeField]
	private TMP_Text m_playerNameText;

	[SerializeField]
	private TMP_Text m_titleText;

	[SerializeField]
	private TMP_Text m_subTitleText;

	[SerializeField]
	private TMP_Text m_descriptionText;

	[SerializeField]
	[Tooltip("Spacer used purely for keeping the spacing correct when disabling the description text.")]
	private GameObject m_descriptionSpacer;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private GameObject m_canvasGO;

	[SerializeField]
	private UnityEvent m_bossCompleteUnityEvent;

	[SerializeField]
	private UnityEvent m_insightCompleteUnityEvent;

	[SerializeField]
	private UnityEvent m_heirloomCompleteUnityEvent;

	private WaitRL_Yield m_waitYield;

	private Tween m_tweenIn;

	private Tween m_tweenOut;

	private float m_storedAlpha;

	private ObjectiveCompleteHUDEventArgs m_refreshTextEventArgs;

	private Action<MonoBehaviour, EventArgs> m_displayHUD;

	private Action<MonoBehaviour, EventArgs> m_hideHUD;

	private Action<MonoBehaviour, EventArgs> m_onPause;

	private Action<MonoBehaviour, EventArgs> m_refreshText;

	private void Awake()
	{
		m_displayHUD = DisplayHUD;
		m_hideHUD = HideHUD;
		m_onPause = OnPause;
		m_refreshText = RefreshText;
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.DisplayObjectiveCompleteHUD, m_displayHUD);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.SkillTree_Opened, m_hideHUD);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerExitRoom, m_hideHUD);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.GamePauseStateChange, m_onPause);
		m_waitYield = new WaitRL_Yield(0f);
		m_canvasGroup.alpha = 0f;
		m_canvasGO.SetActive(value: false);
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDestroy()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.DisplayObjectiveCompleteHUD, m_displayHUD);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.SkillTree_Opened, m_hideHUD);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerExitRoom, m_hideHUD);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.GamePauseStateChange, m_onPause);
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
	{
		HideHUD(null, null);
	}

	private void OnPause(object sender, EventArgs args)
	{
		if (!(args is GamePauseStateChangeEventArgs gamePauseStateChangeEventArgs))
		{
			return;
		}
		if (gamePauseStateChangeEventArgs.IsPaused)
		{
			m_storedAlpha = m_canvasGroup.alpha;
			m_canvasGroup.alpha = 0f;
			if (!m_tweenIn.IsNativeNull())
			{
				m_tweenIn.SetPaused(pause: true);
			}
			if (!m_tweenOut.IsNativeNull())
			{
				m_tweenOut.SetPaused(pause: true);
			}
		}
		else
		{
			m_canvasGroup.alpha = m_storedAlpha;
			if (!m_tweenIn.IsNativeNull())
			{
				m_tweenIn.SetPaused(pause: false);
			}
			if (!m_tweenOut.IsNativeNull())
			{
				m_tweenOut.SetPaused(pause: false);
			}
		}
	}

	private void HideHUD(object sender, EventArgs args)
	{
		StopAllCoroutines();
		m_canvasGroup.alpha = 0f;
		if (!m_canvasGO.activeSelf)
		{
			m_canvasGO.SetActive(value: false);
		}
		m_storedAlpha = 0f;
		if (m_tweenIn != null)
		{
			m_tweenIn.StopTweenWithConditionChecks(runEndHandlers: false, m_canvasGroup);
		}
		if (m_tweenOut != null)
		{
			m_tweenOut.StopTweenWithConditionChecks(runEndHandlers: false, m_canvasGroup);
		}
	}

	private void DisplayHUD(MonoBehaviour sender, EventArgs args)
	{
		HideHUD(null, null);
		ObjectiveCompleteHUDEventArgs objectiveCompleteHUDEventArgs = (m_refreshTextEventArgs = args as ObjectiveCompleteHUDEventArgs);
		UpdateObjectiveCompleteText(objectiveCompleteHUDEventArgs, runEvent: true);
		m_canvasGO.SetActive(value: true);
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(DisplayHUDCoroutine(objectiveCompleteHUDEventArgs.DisplayDuration));
		}
	}

	private void UpdateObjectiveCompleteText(ObjectiveCompleteHUDEventArgs objectiveArgs, bool runEvent)
	{
		m_playerNameText.gameObject.SetActive(value: false);
		m_titleText.gameObject.SetActive(value: true);
		m_subTitleText.gameObject.SetActive(value: true);
		m_descriptionText.gameObject.SetActive(value: true);
		m_descriptionSpacer.SetActive(value: false);
		switch (objectiveArgs.HUDType)
		{
		case ObjectiveCompleteHUDType.Boss:
			m_subTitleText.gameObject.SetActive(value: false);
			m_descriptionText.gameObject.SetActive(value: false);
			m_descriptionSpacer.SetActive(value: true);
			m_titleText.text = LocalizationManager.GetString("LOC_ID_BIG_TEXT_UI_BOSS_DEFEATED_TITLE_1", isFemale: false);
			if (m_bossCompleteUnityEvent != null && runEvent)
			{
				m_bossCompleteUnityEvent.Invoke();
			}
			break;
		case ObjectiveCompleteHUDType.Insight:
		{
			InsightObjectiveCompleteHUDEventArgs insightObjectiveCompleteHUDEventArgs = objectiveArgs as InsightObjectiveCompleteHUDEventArgs;
			if (Insight_EV.LocIDTable.ContainsKey(insightObjectiveCompleteHUDEventArgs.InsightType))
			{
				InsightLocIDEntry insightLocIDEntry = Insight_EV.LocIDTable[insightObjectiveCompleteHUDEventArgs.InsightType];
				string locID;
				string locID2;
				if (insightObjectiveCompleteHUDEventArgs.Discovered)
				{
					locID = "LOC_ID_BIG_TEXT_UI_INSIGHT_DISCOVERY_TITLE_1";
					locID2 = "LOC_ID_BIG_TEXT_UI_INSIGHT_DISCLAIMER_1";
				}
				else
				{
					locID = "LOC_ID_BIG_TEXT_UI_INSIGHT_RESOLVED_TITLE_1";
					locID2 = insightLocIDEntry.ResolvedTextLocID;
				}
				m_titleText.text = LocalizationManager.GetString(locID, isFemale: false);
				m_subTitleText.text = LocalizationManager.GetString(insightLocIDEntry.TitleLocID, isFemale: false);
				m_descriptionText.text = LocalizationManager.GetString(locID2, isFemale: false);
			}
			else
			{
				Debug.Log("<color=red>Insight: " + insightObjectiveCompleteHUDEventArgs.InsightType.ToString() + " could not be found in Insight_EV.LocIDTable.</color>");
			}
			if (m_insightCompleteUnityEvent != null && runEvent)
			{
				m_insightCompleteUnityEvent.Invoke();
			}
			break;
		}
		case ObjectiveCompleteHUDType.Heirloom:
			m_titleText.text = LocalizationManager.GetString("LOC_ID_BIG_TEXT_UI_HEIRLOOM_GAINED_TITLE_1", isFemale: false);
			m_subTitleText.gameObject.SetActive(value: false);
			m_descriptionText.gameObject.SetActive(value: false);
			m_descriptionSpacer.SetActive(value: true);
			if (m_heirloomCompleteUnityEvent != null && runEvent)
			{
				m_heirloomCompleteUnityEvent.Invoke();
			}
			break;
		case ObjectiveCompleteHUDType.Traits:
		{
			m_playerNameText.gameObject.SetActive(value: true);
			CharacterData currentCharacter = SaveManager.PlayerSaveData.CurrentCharacter;
			bool isFemale = SaveManager.PlayerSaveData.CurrentCharacter.IsFemale;
			string localizedPlayerName = LocalizationManager.GetLocalizedPlayerName(currentCharacter);
			string @string = LocalizationManager.GetString(ClassLibrary.GetClassData(currentCharacter.ClassType).PassiveData.Title, isFemale);
			TraitData traitData = TraitLibrary.GetTraitData(currentCharacter.TraitOne);
			string text = "";
			string text2 = "";
			if (traitData != null)
			{
				if (currentCharacter.TraitOne == TraitType.Disposition)
				{
					text = LocalizationManager.GetString(traitData.GetTraitTitleLocID(), isFemale);
					text2 = LocalizationManager.GetString(Gay_Trait.GetDispositionLocID(currentCharacter, getDescription2: true), isFemale);
				}
				else if (currentCharacter.TraitOne == TraitType.Antique)
				{
					RelicData relicData = ((currentCharacter.AntiqueOneOwned != 0) ? RelicLibrary.GetRelicData(currentCharacter.AntiqueOneOwned) : null);
					text = ((relicData != null) ? LocalizationManager.GetString(relicData.Title, isFemale: false) : "NULL RELIC");
					text2 = LocalizationManager.GetString((relicData != null) ? relicData.Description : ("NULL ANTIQUE DESCRIPTION(" + currentCharacter.AntiqueOneOwned.ToString() + ")"), isFemale, out var isFormatString);
					if (isFormatString)
					{
						float altValue;
						float relicFormatString = Relic_EV.GetRelicFormatString(currentCharacter.AntiqueOneOwned, 1, out altValue);
						text2 = string.Format(text2, relicFormatString.ToCIString(), altValue.ToCIString());
					}
				}
				else
				{
					text = LocalizationManager.GetString(traitData.GetTraitTitleLocID(), isFemale);
					text2 = LocalizationManager.GetString(traitData.Description_2, isFemale);
				}
			}
			TraitData traitData2 = TraitLibrary.GetTraitData(currentCharacter.TraitTwo);
			string text3 = "";
			string text4 = "";
			if (traitData2 != null)
			{
				if (currentCharacter.TraitTwo == TraitType.Disposition)
				{
					text3 = LocalizationManager.GetString(traitData2.GetTraitTitleLocID(), isFemale);
					text4 = LocalizationManager.GetString(Gay_Trait.GetDispositionLocID(currentCharacter, getDescription2: true), isFemale);
				}
				else if (currentCharacter.TraitTwo == TraitType.Antique)
				{
					RelicData relicData2 = ((currentCharacter.AntiqueTwoOwned != 0) ? RelicLibrary.GetRelicData(currentCharacter.AntiqueTwoOwned) : null);
					text3 = ((relicData2 != null) ? LocalizationManager.GetString(relicData2.Title, isFemale: false) : "NULL RELIC");
					text4 = LocalizationManager.GetString((relicData2 != null) ? relicData2.Description : ("NULL ANTIQUE DESCRIPTION(" + currentCharacter.AntiqueTwoOwned.ToString() + ")"), isFemale, out var isFormatString2);
					if (isFormatString2)
					{
						float altValue2;
						float relicFormatString2 = Relic_EV.GetRelicFormatString(currentCharacter.AntiqueTwoOwned, 1, out altValue2);
						text4 = string.Format(text4, relicFormatString2.ToCIString(), altValue2.ToCIString());
					}
				}
				else
				{
					text3 = LocalizationManager.GetString(traitData2.GetTraitTitleLocID(), isFemale);
					text4 = LocalizationManager.GetString(traitData2.Description_2, isFemale);
				}
			}
			m_playerNameText.text = string.Format(LocalizationManager.GetString("LOC_ID_HERO_CARD_FULL_PLAYER_CLASS_FORMATTER_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), localizedPlayerName, @string);
			if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(text3))
			{
				m_titleText.text = text + " + " + text3;
			}
			else
			{
				m_titleText.text = text + text3;
			}
			if (!string.IsNullOrEmpty(text2) && !string.IsNullOrEmpty(text4))
			{
				m_subTitleText.text = text2 + " + " + text4;
			}
			else
			{
				m_subTitleText.text = text2 + text4;
			}
			m_descriptionText.text = LocalizationManager.GetString("LOC_ID_BIG_TEXT_UI_VISIT_PAUSE_TEXT_1", isFemale: false);
			break;
		}
		case ObjectiveCompleteHUDType.PizzaGirl:
			m_subTitleText.gameObject.SetActive(value: false);
			m_titleText.text = LocalizationManager.GetString("LOC_ID_BIG_TEXT_UI_PORTAL_UNLOCKED_TITLE_1", isFemale: false);
			m_descriptionText.text = LocalizationManager.GetString("LOC_ID_BIG_TEXT_UI_PORTAL_UNLOCKED_TEXT_1", isFemale: false);
			if (m_bossCompleteUnityEvent != null && runEvent)
			{
				m_bossCompleteUnityEvent.Invoke();
			}
			break;
		case ObjectiveCompleteHUDType.Scar:
		{
			ScarObjectiveCompleteHUDEventArgs scarObjectiveCompleteHUDEventArgs = objectiveArgs as ScarObjectiveCompleteHUDEventArgs;
			m_descriptionText.gameObject.SetActive(value: false);
			m_descriptionSpacer.SetActive(value: true);
			m_titleText.text = LocalizationManager.GetString("LOC_ID_BIG_TEXT_UI_SCAR_DISCOVERED_TITLE_1", isFemale: false);
			m_subTitleText.text = LocalizationManager.GetString(ChallengeLibrary.GetChallengeData(scarObjectiveCompleteHUDEventArgs.ChallengeType).Title, isFemale: false);
			if (m_insightCompleteUnityEvent != null && runEvent)
			{
				m_insightCompleteUnityEvent.Invoke();
			}
			break;
		}
		case ObjectiveCompleteHUDType.WorldComplete:
			m_titleText.text = LocalizationManager.GetString("LOC_ID_NG_UI_WORLD_COMPLETE_1", isFemale: false);
			m_subTitleText.gameObject.SetActive(value: false);
			if (SaveManager.PlayerSaveData.HighestNGPlusBeaten == -1)
			{
				m_descriptionText.text = LocalizationManager.GetString("LOC_ID_NG_UI_NGPLUS_UNLOCKED_1", isFemale: false);
				break;
			}
			if (SaveManager.PlayerSaveData.NewGamePlusLevel > SaveManager.PlayerSaveData.HighestNGPlusBeaten)
			{
				m_descriptionText.text = LocalizationManager.GetString("LOC_ID_NG_UI_NEW_THREAD_1", isFemale: false);
				break;
			}
			m_descriptionText.gameObject.SetActive(value: false);
			m_descriptionSpacer.SetActive(value: true);
			break;
		case ObjectiveCompleteHUDType.LQA:
		{
			LQAObjectiveCompleteHUDEventArgs lQAObjectiveCompleteHUDEventArgs = objectiveArgs as LQAObjectiveCompleteHUDEventArgs;
			if (lQAObjectiveCompleteHUDEventArgs.DisplayPlayer)
			{
				m_playerNameText.gameObject.SetActive(value: true);
			}
			if (string.IsNullOrEmpty(lQAObjectiveCompleteHUDEventArgs.TitleTextOverride))
			{
				m_titleText.gameObject.SetActive(value: false);
			}
			else
			{
				m_titleText.text = LocalizationManager.GetString(lQAObjectiveCompleteHUDEventArgs.TitleTextOverride, isFemale: false);
			}
			if (string.IsNullOrEmpty(lQAObjectiveCompleteHUDEventArgs.SubTitleTextOverride))
			{
				m_subTitleText.gameObject.SetActive(value: false);
			}
			else
			{
				m_subTitleText.text = LocalizationManager.GetString(lQAObjectiveCompleteHUDEventArgs.SubTitleTextOverride, isFemale: false);
			}
			if (string.IsNullOrEmpty(lQAObjectiveCompleteHUDEventArgs.DescriptionTextOverride))
			{
				m_descriptionText.gameObject.SetActive(value: false);
			}
			else
			{
				m_descriptionText.text = LocalizationManager.GetString(lQAObjectiveCompleteHUDEventArgs.DescriptionTextOverride, isFemale: false);
			}
			break;
		}
		}
		if (objectiveArgs.HUDType != ObjectiveCompleteHUDType.LQA)
		{
			if (!string.IsNullOrEmpty(objectiveArgs.TitleTextOverride))
			{
				m_titleText.text = objectiveArgs.TitleTextOverride;
			}
			if (!string.IsNullOrEmpty(objectiveArgs.SubTitleTextOverride))
			{
				m_subTitleText.text = objectiveArgs.SubTitleTextOverride;
			}
			if (!string.IsNullOrEmpty(objectiveArgs.DescriptionTextOverride))
			{
				m_descriptionText.text = objectiveArgs.DescriptionTextOverride;
			}
		}
	}

	private IEnumerator DisplayHUDCoroutine(float duration)
	{
		float num = 1f;
		float fadeOutTime = 1f;
		duration -= num + fadeOutTime;
		m_tweenIn = TweenManager.TweenTo(m_canvasGroup, num, Ease.None, "alpha", 1);
		yield return m_tweenIn.TweenCoroutine;
		if (duration > 0f)
		{
			m_waitYield.CreateNew(duration);
			yield return m_waitYield;
		}
		m_tweenOut = TweenManager.TweenTo(m_canvasGroup, fadeOutTime, Ease.None, "alpha", 0);
		yield return m_tweenOut.TweenCoroutine;
		m_canvasGO.SetActive(value: false);
	}

	private void OnEnable()
	{
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.LanguageChanged, m_refreshText);
	}

	private void OnDisable()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.LanguageChanged, m_refreshText);
	}

	public void RefreshText(object sender, EventArgs args)
	{
		if (m_refreshTextEventArgs != null)
		{
			UpdateObjectiveCompleteText(m_refreshTextEventArgs, runEvent: false);
		}
	}
}
