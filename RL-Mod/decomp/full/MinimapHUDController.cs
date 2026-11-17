using System;
using System.Collections;
using SceneManagement_RL;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinimapHUDController : MonoBehaviour
{
	private const float RESOURCE_DISPLAY_DURATION = 3f;

	[SerializeField]
	private TMP_Text m_goldText;

	[SerializeField]
	private RectTransform m_goldTextRectTransform;

	[SerializeField]
	private TMP_Text m_goldModText;

	[SerializeField]
	private RectTransform m_goldModTextRectTransform;

	[SerializeField]
	private TMP_Text m_runeOreText;

	[SerializeField]
	private TMP_Text m_equipmentOreText;

	[SerializeField]
	private TMP_Text m_soulText;

	[SerializeField]
	private CanvasGroup m_runeCanvasGroup;

	[SerializeField]
	private CanvasGroup m_equipmentCanvasGroup;

	[SerializeField]
	private CanvasGroup m_soulCanvasGroup;

	[SerializeField]
	private GameObject m_bankedGoldGO;

	[SerializeField]
	private TMP_Text m_bankedGoldText;

	[SerializeField]
	private TMP_Text m_ngPlusText;

	[SerializeField]
	private CanvasGroup m_minimapCanvasGroup;

	[SerializeField]
	private FadeOutHUDCollider m_fadeOutHUDCollider;

	private bool m_runeDisplayComplete;

	private bool m_equipmentDisplayComplete;

	private bool m_soulDisplayComplete;

	private Tween m_runeTween;

	private Tween m_equipmentTween;

	private Tween m_soulTween;

	private Coroutine m_runeCoroutine;

	private Coroutine m_equipmentCoroutine;

	private Coroutine m_soulCoroutine;

	private WaitRL_Yield m_runeWaitYield;

	private WaitRL_Yield m_equipmentWaitYield;

	private WaitRL_Yield m_soulWaitYield;

	private Action<MonoBehaviour, EventArgs> m_onRuneChanged;

	private Action<MonoBehaviour, EventArgs> m_onEnterWorld;

	private Action<MonoBehaviour, EventArgs> m_onExitChallenge;

	private Action<MonoBehaviour, EventArgs> m_onGoldChanged;

	private Action<MonoBehaviour, EventArgs> m_onEquipmentOreChanged;

	private Action<MonoBehaviour, EventArgs> m_onRuneOreChanged;

	private Action<MonoBehaviour, EventArgs> m_onSoulChanged;

	private Action<MonoBehaviour, EventArgs> m_onNewGamePlusChange;

	private int m_previousSoulAmount;

	public static bool UpdateTextOnly { get; set; }

	private void Awake()
	{
		m_runeWaitYield = new WaitRL_Yield(0f);
		m_equipmentWaitYield = new WaitRL_Yield(0f);
		m_soulWaitYield = new WaitRL_Yield(0f);
		m_fadeOutHUDCollider.SetCanvasGroup(m_minimapCanvasGroup);
		m_onRuneChanged = OnRuneChanged;
		m_onEnterWorld = OnEnterWorld;
		m_onExitChallenge = OnExitChallenge;
		m_onGoldChanged = OnGoldChanged;
		m_onEquipmentOreChanged = OnEquipmentOreChanged;
		m_onRuneOreChanged = OnRuneOreChanged;
		m_onSoulChanged = OnSoulChanged;
		m_onNewGamePlusChange = OnNewGamePlusChange;
	}

	private void OnEnable()
	{
		m_goldText.text = SaveManager.PlayerSaveData.GoldCollected.ToString();
		m_runeOreText.text = SaveManager.PlayerSaveData.GoldCollected.ToString();
		m_equipmentOreText.text = SaveManager.PlayerSaveData.GoldCollected.ToString();
		m_soulText.text = SaveManager.PlayerSaveData.GoldCollected.ToString();
		m_bankedGoldText.text = SaveManager.PlayerSaveData.GoldSaved.ToString();
		OnNewGamePlusChange(null, null);
		m_goldModText.gameObject.SetActive(value: false);
		m_runeCanvasGroup.gameObject.SetActive(value: false);
		m_runeCanvasGroup.alpha = 0f;
		m_runeDisplayComplete = true;
		m_equipmentCanvasGroup.gameObject.SetActive(value: false);
		m_equipmentCanvasGroup.alpha = 0f;
		m_equipmentDisplayComplete = true;
		m_soulCanvasGroup.gameObject.SetActive(value: false);
		m_soulCanvasGroup.alpha = 0f;
		m_soulDisplayComplete = true;
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.RuneEquippedLevelChanged, m_onRuneChanged);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.ChallengeNPC_EnterChallenge, m_onEnterWorld);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.ChallengeNPC_ExitChallenge, m_onExitChallenge);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.GoldChanged, m_onGoldChanged);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.EquipmentOreChanged, m_onEquipmentOreChanged);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.RuneOreChanged, m_onRuneOreChanged);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.SoulChanged, m_onSoulChanged);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.NGPlusChanged, m_onNewGamePlusChange);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.HouseRulesChanged, m_onNewGamePlusChange);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.WorldCreationComplete, m_onEnterWorld);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.LanguageChanged, m_onNewGamePlusChange);
	}

	private void OnDisable()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.RuneEquippedLevelChanged, m_onRuneChanged);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.ChallengeNPC_EnterChallenge, m_onEnterWorld);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.ChallengeNPC_ExitChallenge, m_onExitChallenge);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.GoldChanged, m_onGoldChanged);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.EquipmentOreChanged, m_onEquipmentOreChanged);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.RuneOreChanged, m_onRuneOreChanged);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.SoulChanged, m_onSoulChanged);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.NGPlusChanged, m_onNewGamePlusChange);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.HouseRulesChanged, m_onNewGamePlusChange);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.WorldCreationComplete, m_onEnterWorld);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.LanguageChanged, m_onNewGamePlusChange);
	}

	private void OnExitChallenge(MonoBehaviour sender, EventArgs args)
	{
		m_runeCanvasGroup.gameObject.SetActive(value: true);
		m_equipmentCanvasGroup.gameObject.SetActive(value: true);
		m_soulCanvasGroup.gameObject.SetActive(value: true);
	}

	private void OnEnterWorld(MonoBehaviour sender, EventArgs args)
	{
		m_runeCanvasGroup.gameObject.SetActive(value: false);
		m_equipmentCanvasGroup.gameObject.SetActive(value: false);
		m_soulCanvasGroup.gameObject.SetActive(value: false);
	}

	private void OnRuneChanged(MonoBehaviour sender, EventArgs args)
	{
		if ((args as RuneEquippedLevelChangeEventArgs).RuneType == RuneType.GoldGain)
		{
			OnGoldChanged(null, null);
		}
	}

	private void OnGoldChanged(MonoBehaviour sender, EventArgs args)
	{
		m_goldText.text = SaveManager.PlayerSaveData.GoldCollected.ToString();
		BaseRoom currentPlayerRoom = PlayerManager.GetCurrentPlayerRoom();
		if (!currentPlayerRoom.IsNativeNull() && currentPlayerRoom.BiomeType == BiomeType.HubTown && SkillTreeManager.GetSkillObjLevel(SkillTreeType.Gold_Saved_Unlock) > 0)
		{
			if (!m_bankedGoldGO.gameObject.activeSelf)
			{
				m_bankedGoldGO.gameObject.SetActive(value: true);
			}
			m_bankedGoldText.text = SaveManager.PlayerSaveData.GoldSaved.ToString();
			LayoutRebuilder.ForceRebuildLayoutImmediate(m_bankedGoldGO.GetComponent<RectTransform>());
		}
		else if (m_bankedGoldGO.gameObject.activeSelf)
		{
			m_bankedGoldGO.gameObject.SetActive(value: false);
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_goldTextRectTransform);
		float goldGainMod = Economy_EV.GetGoldGainMod();
		float num = 1f;
		if (SaveManager.PlayerSaveData.CastleLockState != 0)
		{
			num = ((!(SceneLoader_RL.CurrentScene == SceneLoadingUtility.GetSceneName(SceneID.World))) ? NPC_EV.GetArchitectGoldMod(SaveManager.PlayerSaveData.TimesCastleLocked + 1) : NPC_EV.GetArchitectGoldMod());
		}
		int num2 = Mathf.RoundToInt(((1f + goldGainMod) * num - 1f) * 100f);
		if (num2 > 0)
		{
			if (!m_goldModText.gameObject.activeSelf)
			{
				m_goldModText.gameObject.SetActive(value: true);
			}
			m_goldModText.text = "+" + string.Format(LocalizationManager.GetString("LOC_ID_GENERAL_UI_PERCENT_1", isFemale: false), num2);
			Vector2 anchoredPosition = m_goldModTextRectTransform.anchoredPosition;
			anchoredPosition.x = m_goldTextRectTransform.anchoredPosition.x + m_goldTextRectTransform.sizeDelta.x + 20f;
			m_goldModTextRectTransform.anchoredPosition = anchoredPosition;
		}
		else if (num2 < 0)
		{
			if (!m_goldModText.gameObject.activeSelf)
			{
				m_goldModText.gameObject.SetActive(value: true);
			}
			m_goldModText.text = "<color=red>" + string.Format(LocalizationManager.GetString("LOC_ID_GENERAL_UI_PERCENT_1", isFemale: false), num2) + "</color>";
			Vector2 anchoredPosition2 = m_goldModTextRectTransform.anchoredPosition;
			anchoredPosition2.x = m_goldTextRectTransform.anchoredPosition.x + m_goldTextRectTransform.sizeDelta.x + 20f;
			m_goldModTextRectTransform.anchoredPosition = anchoredPosition2;
		}
		else if (m_goldModText.gameObject.activeSelf)
		{
			m_goldModText.gameObject.SetActive(value: false);
		}
	}

	private void OnRuneOreChanged(MonoBehaviour sender, EventArgs args)
	{
		m_runeOreText.text = SaveManager.PlayerSaveData.RuneOreCollected.ToString();
		if (UpdateTextOnly)
		{
			if (m_runeCanvasGroup.isActiveAndEnabled)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(m_runeCanvasGroup.GetComponent<RectTransform>());
			}
			return;
		}
		if (m_runeDisplayComplete)
		{
			if (m_runeCoroutine != null)
			{
				if (m_runeCanvasGroup.gameObject.activeSelf)
				{
					m_runeCanvasGroup.gameObject.SetActive(value: false);
				}
				StopCoroutine(m_runeCoroutine);
			}
			if (m_runeTween != null)
			{
				m_runeTween.StopTweenWithConditionChecks(runEndHandlers: false, m_runeCanvasGroup);
			}
			if (!SaveManager.PlayerSaveData.InHubTown || ChallengeManager.IsInChallenge)
			{
				m_runeCoroutine = StartCoroutine(RuneOreDisplayCoroutine());
			}
			else
			{
				m_runeCanvasGroup.gameObject.SetActive(value: true);
				m_runeCanvasGroup.alpha = 1f;
			}
		}
		else
		{
			m_runeWaitYield.CreateNew(3f);
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_runeCanvasGroup.GetComponent<RectTransform>());
	}

	private IEnumerator RuneOreDisplayCoroutine()
	{
		m_runeDisplayComplete = false;
		m_runeCanvasGroup.gameObject.SetActive(value: true);
		m_runeCanvasGroup.alpha = 0f;
		TweenManager.TweenTo(m_runeCanvasGroup, 0.1f, Ease.None, "alpha", 1);
		m_runeWaitYield.CreateNew(3f);
		yield return m_runeWaitYield;
		m_runeDisplayComplete = true;
		m_runeTween = TweenManager.TweenTo(m_runeCanvasGroup, 0.1f, Ease.None, "alpha", 0);
		yield return m_runeTween.TweenCoroutine;
		m_runeCanvasGroup.gameObject.SetActive(value: false);
		m_runeTween = null;
	}

	private void OnEquipmentOreChanged(MonoBehaviour sender, EventArgs args)
	{
		m_equipmentOreText.text = SaveManager.PlayerSaveData.EquipmentOreCollected.ToString();
		if (UpdateTextOnly)
		{
			if (m_equipmentCanvasGroup.isActiveAndEnabled)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(m_equipmentCanvasGroup.GetComponent<RectTransform>());
			}
			return;
		}
		if (m_equipmentDisplayComplete)
		{
			if (m_equipmentCoroutine != null)
			{
				if (m_equipmentCanvasGroup.gameObject.activeSelf)
				{
					m_equipmentCanvasGroup.gameObject.SetActive(value: false);
				}
				StopCoroutine(m_equipmentCoroutine);
			}
			if (m_equipmentTween != null)
			{
				m_equipmentTween.StopTweenWithConditionChecks(runEndHandlers: false, m_equipmentCanvasGroup);
			}
			if (!SaveManager.PlayerSaveData.InHubTown || ChallengeManager.IsInChallenge)
			{
				m_equipmentCoroutine = StartCoroutine(EquipmentDisplayCoroutine());
			}
			else
			{
				m_equipmentCanvasGroup.gameObject.SetActive(value: true);
				m_equipmentCanvasGroup.alpha = 1f;
			}
		}
		else
		{
			m_equipmentWaitYield.CreateNew(3f);
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_equipmentCanvasGroup.GetComponent<RectTransform>());
	}

	private IEnumerator EquipmentDisplayCoroutine()
	{
		m_equipmentDisplayComplete = false;
		m_equipmentCanvasGroup.gameObject.SetActive(value: true);
		m_equipmentCanvasGroup.alpha = 0f;
		TweenManager.TweenTo(m_equipmentCanvasGroup, 0.1f, Ease.None, "alpha", 1);
		m_equipmentWaitYield.CreateNew(3f);
		yield return m_equipmentWaitYield;
		m_equipmentDisplayComplete = true;
		m_equipmentTween = TweenManager.TweenTo(m_equipmentCanvasGroup, 0.1f, Ease.None, "alpha", 0);
		yield return m_equipmentTween.TweenCoroutine;
		m_equipmentCanvasGroup.gameObject.SetActive(value: false);
		m_equipmentTween = null;
	}

	private void OnSoulChanged(MonoBehaviour sender, EventArgs args)
	{
		int num = Souls_EV.GetTotalSoulsCollected(SaveManager.PlayerSaveData.GameModeType, deductSpentSouls: true) - SoulDrop.FakeSoulCounter_STATIC;
		bool flag = SaveManager.PlayerSaveData.InHubTown && (!m_soulCanvasGroup.gameObject.activeSelf || m_soulCanvasGroup.alpha <= 0f);
		if (m_previousSoulAmount == num && !flag)
		{
			return;
		}
		m_previousSoulAmount = num;
		m_soulText.text = num.ToString();
		if (UpdateTextOnly)
		{
			if (m_soulCanvasGroup.isActiveAndEnabled)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(m_soulCanvasGroup.GetComponent<RectTransform>());
			}
			return;
		}
		if (m_soulDisplayComplete)
		{
			if (m_soulCoroutine != null)
			{
				if (m_soulCanvasGroup.gameObject.activeSelf)
				{
					m_soulCanvasGroup.gameObject.SetActive(value: false);
				}
				StopCoroutine(m_soulCoroutine);
			}
			if (m_soulTween != null)
			{
				m_soulTween.StopTweenWithConditionChecks(runEndHandlers: false, m_soulCanvasGroup);
			}
			if (!SaveManager.PlayerSaveData.InHubTown || ChallengeManager.IsInChallenge)
			{
				m_soulCoroutine = StartCoroutine(SoulDisplayCoroutine());
			}
			else
			{
				m_soulCanvasGroup.gameObject.SetActive(value: true);
				m_soulCanvasGroup.alpha = 1f;
			}
		}
		else
		{
			m_soulWaitYield.CreateNew(3f);
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_soulCanvasGroup.GetComponent<RectTransform>());
	}

	private IEnumerator SoulDisplayCoroutine()
	{
		m_soulDisplayComplete = false;
		m_soulCanvasGroup.gameObject.SetActive(value: true);
		m_soulCanvasGroup.alpha = 0f;
		TweenManager.TweenTo(m_soulCanvasGroup, 0.1f, Ease.None, "alpha", 1);
		m_soulWaitYield.CreateNew(3f);
		yield return m_soulWaitYield;
		m_soulDisplayComplete = true;
		m_soulTween = TweenManager.TweenTo(m_soulCanvasGroup, 0.1f, Ease.None, "alpha", 0);
		yield return m_soulTween.TweenCoroutine;
		m_soulCanvasGroup.gameObject.SetActive(value: false);
		m_soulTween = null;
	}

	private void OnNewGamePlusChange(MonoBehaviour sender, EventArgs args)
	{
		m_ngPlusText.text = "";
		if (SaveManager.PlayerSaveData.NewGamePlusLevel > 0)
		{
			m_ngPlusText.gameObject.SetActive(value: true);
			m_ngPlusText.text += string.Format(LocalizationManager.GetString("LOC_ID_MINIMAP_NG_TITLE_1", isFemale: false), SaveManager.PlayerSaveData.NewGamePlusLevel);
		}
		int totalBurdenWeight = BurdenManager.GetTotalBurdenWeight();
		if (totalBurdenWeight > 0)
		{
			m_ngPlusText.text += string.Format(LocalizationManager.GetString("LOC_ID_MINIMAP_BURDEN_TITLE_1", isFemale: false), totalBurdenWeight);
		}
		if (SaveManager.PlayerSaveData.EnableHouseRules)
		{
			if (m_ngPlusText.text.Length > 0)
			{
				m_ngPlusText.text += "\n";
			}
			if (SaveManager.PlayerSaveData.Assist_EnableDifficultyDisplay)
			{
				string @string = LocalizationManager.GetString("LOC_ID_ASSIST_MODE_DIFFICULTY_MINIMAP_DISPLAY_1", isFemale: false);
				m_ngPlusText.text += string.Format(@string, AssistMode_EV.GetDifficulty());
			}
			else
			{
				m_ngPlusText.text += LocalizationManager.GetString("LOC_ID_MINIMAP_HOUSE_RULES_TITLE_1", isFemale: false);
			}
		}
	}
}
