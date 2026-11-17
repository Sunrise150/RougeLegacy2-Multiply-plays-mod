using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FairyRuleHUDController : MonoBehaviour, ILocalizable
{
	[SerializeField]
	private CanvasGroup m_panel;

	[SerializeField]
	private GameObject m_entryGroup;

	[SerializeField]
	private CanvasGroup m_bgCanvasGroup;

	[SerializeField]
	private FairyRuleHUDEntry m_fairyRuleHUDEntryPrefab;

	[SerializeField]
	private FadeOutHUDCollider m_fairyHUDCollider;

	[SerializeField]
	private ParticleSystem m_particleEffect;

	[SerializeField]
	private Image m_orbImage;

	private FairyRoomController m_fairyRoomController;

	private Dictionary<FairyRule, FairyRuleHUDEntry> m_hudEntryTable;

	private static FairyRuleHUDEntry[] m_hudEntryArray_STATIC;

	private Tween m_fadeInTween;

	private Action<MonoBehaviour, EventArgs> m_onPlayerEnterFairyRoom;

	private Action<MonoBehaviour, EventArgs> m_onFairyRoomStateChange;

	private Action<MonoBehaviour, EventArgs> m_onPlayerExitFairyRoom;

	private Action<MonoBehaviour, EventArgs> m_onPlayerDeath;

	private Action<MonoBehaviour, EventArgs> m_onFairyRuleStateChange;

	private Action<MonoBehaviour, EventArgs> m_refreshText;

	private float m_timerValue;

	public CanvasGroup Panel => m_panel;

	public CanvasGroup BGCanvasGroup => m_bgCanvasGroup;

	private void Awake()
	{
		if (m_hudEntryArray_STATIC == null)
		{
			m_hudEntryArray_STATIC = new FairyRuleHUDEntry[5];
			for (int i = 0; i < 5; i++)
			{
				FairyRuleHUDEntry fairyRuleHUDEntry = UnityEngine.Object.Instantiate(m_fairyRuleHUDEntryPrefab, m_entryGroup.transform);
				fairyRuleHUDEntry.gameObject.SetActive(value: false);
				m_hudEntryArray_STATIC[i] = fairyRuleHUDEntry;
			}
		}
		m_hudEntryTable = new Dictionary<FairyRule, FairyRuleHUDEntry>();
		m_onPlayerEnterFairyRoom = OnPlayerEnterFairyRoom;
		m_onFairyRoomStateChange = OnFairyRoomStateChange;
		m_onPlayerExitFairyRoom = OnPlayerExitFairyRoom;
		m_onPlayerDeath = OnPlayerDeath;
		m_onFairyRuleStateChange = OnFairyRuleStateChange;
		m_refreshText = RefreshText;
	}

	private void Start()
	{
		SetHUDIsVisible(isVisible: false);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterFairyRoom, m_onPlayerEnterFairyRoom);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.FairyRoomStateChange, m_onFairyRoomStateChange);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerExitFairyRoom, m_onPlayerExitFairyRoom);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerDeath, m_onPlayerDeath);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.FairyRoomRuleStateChange, m_onFairyRuleStateChange);
		m_fairyHUDCollider.SetCanvasGroup(BGCanvasGroup);
		BGCanvasGroup.alpha = 0.7f;
	}

	private void OnDestroy()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterFairyRoom, m_onPlayerEnterFairyRoom);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.FairyRoomStateChange, m_onFairyRoomStateChange);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerExitFairyRoom, m_onPlayerExitFairyRoom);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerDeath, m_onPlayerDeath);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.FairyRoomRuleStateChange, m_onFairyRuleStateChange);
		UnityEngine.Object.Destroy(m_fairyHUDCollider);
		if (m_hudEntryArray_STATIC != null)
		{
			Array.Clear(m_hudEntryArray_STATIC, 0, m_hudEntryArray_STATIC.Length);
			m_hudEntryArray_STATIC = null;
		}
	}

	private void OnPlayerDeath(MonoBehaviour sender, EventArgs eventArgs)
	{
		SetHUDIsVisible(isVisible: false);
	}

	private void OnPlayerEnterFairyRoom(MonoBehaviour sender, EventArgs eventArgs)
	{
		StopAllCoroutines();
		if (eventArgs is FairyRoomEnteredEventArgs fairyRoomEnteredEventArgs && fairyRoomEnteredEventArgs.FairyRoomController != null)
		{
			m_fairyRoomController = fairyRoomEnteredEventArgs.FairyRoomController;
			if (m_fairyRoomController.FairyRoomRuleEntries.Any((FairyRoomRuleEntry entry) => entry.FairyRuleID == FairyRuleID.HiddenChest))
			{
				return;
			}
			FairyRuleHUDEntry[] hudEntryArray_STATIC = m_hudEntryArray_STATIC;
			foreach (FairyRuleHUDEntry fairyRuleHUDEntry in hudEntryArray_STATIC)
			{
				if (fairyRuleHUDEntry.gameObject.activeSelf)
				{
					fairyRuleHUDEntry.gameObject.SetActive(value: false);
				}
			}
			m_hudEntryTable.Clear();
			if (fairyRoomEnteredEventArgs.FairyRoomController.FairyRoomRuleEntries.Count > m_hudEntryArray_STATIC.Length)
			{
				throw new Exception("Fairy Rule HUD Controller currently only supports " + m_hudEntryArray_STATIC.Length + " entries.");
			}
			for (int j = 0; j < fairyRoomEnteredEventArgs.FairyRoomController.FairyRoomRuleEntries.Count; j++)
			{
				FairyRoomRuleEntry fairyRoomRuleEntry = fairyRoomEnteredEventArgs.FairyRoomController.FairyRoomRuleEntries[j];
				FairyRuleHUDEntry fairyRuleHUDEntry2 = m_hudEntryArray_STATIC[j];
				fairyRuleHUDEntry2.gameObject.SetActive(value: true);
				fairyRuleHUDEntry2.Initialise("• " + LocalizationManager.GetString(fairyRoomRuleEntry.FairyRule.Description, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale));
				if (m_fairyRoomController.State == FairyRoomState.Passed)
				{
					fairyRuleHUDEntry2.SetState(FairyRoomState.Passed);
				}
				else if (m_fairyRoomController.State == FairyRoomState.Failed)
				{
					fairyRuleHUDEntry2.SetState(FairyRoomState.Failed);
				}
				else
				{
					fairyRuleHUDEntry2.SetState(fairyRoomRuleEntry.FairyRule.State);
					if (fairyRoomRuleEntry.FairyRuleID == FairyRuleID.TimeLimit)
					{
						TimeLimit_FairyRule timeLimit_FairyRule = fairyRoomRuleEntry.FairyRule as TimeLimit_FairyRule;
						if ((bool)timeLimit_FairyRule)
						{
							StartCoroutine(FairyTimerCoroutine(timeLimit_FairyRule, fairyRuleHUDEntry2));
						}
					}
				}
				m_hudEntryTable.Add(fairyRoomRuleEntry.FairyRule, fairyRuleHUDEntry2);
			}
			Panel.alpha = 0f;
			if (m_fadeInTween != null)
			{
				m_fadeInTween.StopTweenWithConditionChecks(runEndHandlers: false, Panel, "FairyRuleFade");
			}
			m_fadeInTween = TweenManager.TweenTo(Panel, 0.5f, Ease.None, "alpha", 1);
			m_fadeInTween.ID = "FairyRuleFade";
			SetHUDIsVisible(isVisible: true);
		}
		else
		{
			Debug.LogFormat("<color=red>[{0}] Failed to cast eventArgs as given Type", this);
		}
	}

	private void OnFairyRoomStateChange(object sender, EventArgs args)
	{
		Color color;
		switch ((args as FairyRoomEnteredEventArgs).FairyRoomController.State)
		{
		case FairyRoomState.Failed:
			ColorUtility.TryParseHtmlString("#CA000B", out color);
			break;
		case FairyRoomState.Passed:
			ColorUtility.TryParseHtmlString("#3ACF00", out color);
			break;
		case FairyRoomState.Running:
			ColorUtility.TryParseHtmlString("#E7BF51", out color);
			break;
		default:
			ColorUtility.TryParseHtmlString("#000000", out color);
			break;
		}
		m_orbImage.color = color;
	}

	private IEnumerator FairyTimerCoroutine(TimeLimit_FairyRule fairyRule, FairyRuleHUDEntry hudEntry)
	{
		m_timerValue = 0f;
		while ((bool)fairyRule && fairyRule.State != FairyRoomState.Passed && fairyRule.State != FairyRoomState.Failed)
		{
			int num = (int)fairyRule.TimeRemaining;
			if (m_timerValue != (float)num)
			{
				m_timerValue = num;
				hudEntry.SetText("• " + fairyRule.TimeRemainingDescription);
			}
			yield return null;
		}
		hudEntry.SetText("• " + fairyRule.TimeRemainingDescription);
	}

	private void OnPlayerExitFairyRoom(MonoBehaviour sender, EventArgs eventArgs)
	{
		if (eventArgs is FairyRoomEnteredEventArgs fairyRoomEnteredEventArgs && fairyRoomEnteredEventArgs.FairyRoomController == m_fairyRoomController)
		{
			SetHUDIsVisible(isVisible: false);
		}
	}

	private void OnFairyRuleStateChange(MonoBehaviour sender, EventArgs eventArgs)
	{
		FairyRoomRuleStateChangeEventArgs fairyRoomRuleStateChangeEventArgs = eventArgs as FairyRoomRuleStateChangeEventArgs;
		if (fairyRoomRuleStateChangeEventArgs != null && fairyRoomRuleStateChangeEventArgs.Rule != null && fairyRoomRuleStateChangeEventArgs.Rule.ID != FairyRuleID.HiddenChest)
		{
			if (fairyRoomRuleStateChangeEventArgs.Rule.State == FairyRoomState.Failed)
			{
				foreach (KeyValuePair<FairyRule, FairyRuleHUDEntry> item in m_hudEntryTable)
				{
					item.Value.SetState(FairyRoomState.Failed);
				}
				return;
			}
			if (m_hudEntryTable != null)
			{
				if (fairyRoomRuleStateChangeEventArgs.Rule != null)
				{
					if (m_hudEntryTable.ContainsKey(fairyRoomRuleStateChangeEventArgs.Rule))
					{
						m_hudEntryTable[fairyRoomRuleStateChangeEventArgs.Rule].SetState(fairyRoomRuleStateChangeEventArgs.Rule.State);
						return;
					}
					Debug.LogFormat("<color=red>[{0}] HUD Entry Table doesn't contain an entry with the given Key</color>", this);
				}
				else
				{
					Debug.LogFormat("<color=red>[{0}] args.Rule is null</color>", this);
				}
			}
			else
			{
				Debug.LogFormat("<color=red>[{0}] HUD Entry Table has not been initialized</color>", this);
			}
		}
		else if (fairyRoomRuleStateChangeEventArgs == null || fairyRoomRuleStateChangeEventArgs.Rule == null)
		{
			Debug.LogFormat("<color=red>[{0}] Failed to cast eventArgs as given Type</color>", this);
		}
	}

	private void SetHUDIsVisible(bool isVisible)
	{
		Panel.gameObject.SetActive(isVisible);
		m_particleEffect.gameObject.SetActive(isVisible);
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
		if (!m_fairyRoomController)
		{
			return;
		}
		for (int i = 0; i < m_fairyRoomController.FairyRoomRuleEntries.Count; i++)
		{
			FairyRuleHUDEntry fairyRuleHUDEntry = m_hudEntryArray_STATIC[i];
			FairyRoomRuleEntry fairyRoomRuleEntry = m_fairyRoomController.FairyRoomRuleEntries[i];
			TimeLimit_FairyRule timeLimit_FairyRule = fairyRoomRuleEntry.FairyRule as TimeLimit_FairyRule;
			if ((bool)timeLimit_FairyRule)
			{
				fairyRuleHUDEntry.SetText("• " + timeLimit_FairyRule.TimeRemainingDescription);
			}
			else
			{
				fairyRuleHUDEntry.Initialise("• " + LocalizationManager.GetString(fairyRoomRuleEntry.FairyRule.Description, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale));
			}
			fairyRuleHUDEntry.SetState(m_fairyRoomController.State);
		}
	}
}
