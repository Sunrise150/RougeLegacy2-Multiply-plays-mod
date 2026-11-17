using System;
using System.Collections;
using RLAudio;
using RL_Windows;
using Rewired;
using Sigtrap.Relays;
using TMPro;
using UnityEngine;

public class BossIntroWindowController : WindowController, IAudioEventEmitter, ILocalizable
{
	private const string ON_ENTER_AUDIO = "event:/SFX/Enemies/sfx_dancingBoss_intro_enter";

	private const string DISPLAY_NAME_AUDIO = "event:/SFX/Enemies/sfx_dancingBoss_intro_title";

	[SerializeField]
	private CanvasGroup m_topCanvasGroup;

	[SerializeField]
	private CanvasGroup m_bottomCanvasGroup;

	[SerializeField]
	private CanvasGroup m_topContentCanvasGroup;

	[SerializeField]
	private CanvasGroup m_bottomContentCanvasGroup;

	[SerializeField]
	private TMP_Text m_subTitleText;

	[SerializeField]
	private TMP_Text m_titleText;

	[SerializeField]
	private RectTransform m_bottomFrillMask;

	[SerializeField]
	private CanvasGroup m_bottomFrillCanvasGroup;

	[SerializeField]
	private GameObject m_fastForwardObj;

	private WaitRL_Yield m_waitYield;

	private bool m_disableInput;

	private string m_description;

	private EnemyTypeAndRank m_typeAndRank;

	private Action<MonoBehaviour, EventArgs> m_refreshText;

	private Action<InputActionEventData> m_toggleSpeedUp;

	private Relay m_speedUpRelay = new Relay();

	private Relay m_stopSpeedUpRelay = new Relay();

	public IRelayLink StartSpeedUpRelay => m_speedUpRelay.link;

	public IRelayLink StopSpeedUpRelay => m_stopSpeedUpRelay.link;

	public bool WindowFadingIn { get; private set; }

	public bool FadeInBarsAtStart { get; set; }

	public bool DisplayBossName { get; set; }

	public string Description
	{
		get
		{
			if (string.IsNullOrEmpty(m_description))
			{
				m_description = ToString();
			}
			return m_description;
		}
	}

	public override WindowID ID => WindowID.BossIntro;

	private void Awake()
	{
		m_refreshText = RefreshText;
		m_toggleSpeedUp = ToggleSpeedUp;
	}

	public override void Initialize()
	{
		base.Initialize();
		m_waitYield = new WaitRL_Yield(0f);
	}

	protected override void OnOpen()
	{
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.LanguageChanged, m_refreshText);
		InitializeOnOpen();
		DisplayBossName = false;
		m_disableInput = false;
		WindowFadingIn = true;
		m_windowCanvas.gameObject.SetActive(value: true);
		StartCoroutine(OnOpenCoroutine());
	}

	private void InitializeOnOpen()
	{
		m_fastForwardObj.SetActive(value: false);
		if (!FadeInBarsAtStart)
		{
			m_topCanvasGroup.alpha = 1f;
			m_bottomCanvasGroup.alpha = 1f;
		}
		else
		{
			m_topCanvasGroup.alpha = 0f;
			m_bottomCanvasGroup.alpha = 0f;
		}
		m_topContentCanvasGroup.alpha = 0f;
		m_bottomContentCanvasGroup.alpha = 0f;
		m_bottomFrillCanvasGroup.alpha = 0f;
		Vector2 sizeDelta = m_bottomFrillMask.sizeDelta;
		sizeDelta.x = 0f;
		m_bottomFrillMask.sizeDelta = sizeDelta;
	}

	private IEnumerator OnOpenCoroutine()
	{
		PlayAudio("event:/SFX/Enemies/sfx_dancingBoss_intro_enter");
		if (FadeInBarsAtStart)
		{
			TweenManager.TweenTo(m_topCanvasGroup, 0.25f, Ease.None, "alpha", 1);
			TweenManager.TweenTo(m_bottomCanvasGroup, 0.25f, Ease.None, "alpha", 1);
		}
		while (!DisplayBossName)
		{
			yield return null;
		}
		PlayAudio("event:/SFX/Enemies/sfx_dancingBoss_intro_title");
		TweenManager.TweenTo(m_topContentCanvasGroup, 1.5f, Ease.None, "alpha", 1);
		TweenManager.TweenTo(m_bottomContentCanvasGroup, 1.5f, Ease.None, "alpha", 1);
		TweenManager.TweenTo(m_bottomFrillCanvasGroup, 0.5f, Ease.None, "delay", 0.75f, "alpha", 1);
		yield return TweenManager.TweenTo(m_bottomFrillMask, 2f, Ease.Quad.EaseInOut, "delay", 0.5f, "sizeDelta.x", 200).TweenCoroutine;
		m_waitYield.CreateNew(2.5f);
		yield return m_waitYield;
		RLTimeScale.SetTimeScale(TimeScaleType.Cutscene, 1f);
		m_disableInput = true;
		WindowFadingIn = false;
		TweenManager.TweenTo(m_topCanvasGroup, 0.5f, Ease.None, "alpha", 0);
		yield return TweenManager.TweenTo(m_bottomCanvasGroup, 0.5f, Ease.None, "alpha", 0).TweenCoroutine;
		WindowManager.SetWindowIsOpen(WindowID.BossIntro, isOpen: false);
	}

	protected override void OnClose()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.LanguageChanged, m_refreshText);
		m_windowCanvas.gameObject.SetActive(value: false);
		FadeInBarsAtStart = false;
	}

	protected override void OnFocus()
	{
		base.RewiredPlayer.AddInputEventDelegate(m_toggleSpeedUp, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
		base.RewiredPlayer.AddInputEventDelegate(m_toggleSpeedUp, UpdateLoopType.Update, InputActionEventType.ButtonJustReleased, "Window_Confirm");
	}

	protected override void OnLostFocus()
	{
		base.RewiredPlayer.RemoveInputEventDelegate(m_toggleSpeedUp, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
		base.RewiredPlayer.RemoveInputEventDelegate(m_toggleSpeedUp, UpdateLoopType.Update, InputActionEventType.ButtonJustReleased, "Window_Confirm");
	}

	private void ToggleSpeedUp(InputActionEventData eventData)
	{
		if (!m_disableInput)
		{
			if (!eventData.GetButtonUp())
			{
				RLTimeScale.SetTimeScale(TimeScaleType.Cutscene, 10f);
				m_fastForwardObj.SetActive(value: true);
				m_speedUpRelay.Dispatch();
			}
			else
			{
				RLTimeScale.SetTimeScale(TimeScaleType.Cutscene, 1f);
				m_fastForwardObj.SetActive(value: false);
				m_stopSpeedUpRelay.Dispatch();
			}
		}
	}

	public void StopSpeedUp()
	{
		RLTimeScale.SetTimeScale(TimeScaleType.Cutscene, 1f);
		m_fastForwardObj.SetActive(value: false);
		m_stopSpeedUpRelay.Dispatch();
	}

	public void SetEnemyType(EnemyType type, EnemyRank rank)
	{
		m_typeAndRank = new EnemyTypeAndRank(type, rank);
		EnemyData enemyData = EnemyClassLibrary.GetEnemyClassData(type).GetEnemyData(rank);
		m_subTitleText.text = LocalizationManager.GetString(enemyData.Description02, isFemale: false);
		m_titleText.text = LocalizationManager.GetString(enemyData.Title, isFemale: false);
	}

	private void PlayAudio(string audioPath)
	{
		if (!string.IsNullOrEmpty(audioPath))
		{
			AudioManager.Play(this, audioPath);
		}
	}

	public void RefreshText(object sender, EventArgs args)
	{
		SetEnemyType(m_typeAndRank.Type, m_typeAndRank.Rank);
	}
}
