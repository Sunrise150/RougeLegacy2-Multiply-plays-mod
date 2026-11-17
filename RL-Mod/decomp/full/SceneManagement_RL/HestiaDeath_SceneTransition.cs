using System;
using System.Collections;
using FMOD.Studio;
using RLAudio;
using RL_Windows;
using Rewired;
using TMPro;
using UnityEngine;

namespace SceneManagement_RL;

public class HestiaDeath_SceneTransition : Transition_V2, ISceneLoadingTransition, ITransition
{
	[SerializeField]
	private CanvasGroup m_bgCanvasGroup;

	[SerializeField]
	private CanvasGroup m_imageCanvasGroup;

	[SerializeField]
	protected TMP_Text m_text1;

	[SerializeField]
	protected TMP_Text m_text2;

	[SerializeField]
	private GameObject m_fastForwardObj;

	private WaitRL_Yield m_waitYield;

	private bool m_allowSpeedUp;

	private Action<InputActionEventData> m_toggleSpeedUp;

	private EventInstance m_ambienceSFXEventInstance;

	private float m_timeScale = 1f;

	protected virtual string AmbienceSFXName => "event:/Environment/amb_heirloomSpace_withPipes";

	public override TransitionID ID => TransitionID.HestiaDeath;

	protected override void Awake()
	{
		base.Awake();
		m_imageCanvasGroup.alpha = 0f;
		m_bgCanvasGroup.alpha = 0f;
		m_text1.alpha = 0f;
		m_text2.alpha = 0f;
		m_waitYield = new WaitRL_Yield(0f);
		m_fastForwardObj.SetActive(value: false);
		m_toggleSpeedUp = ToggleSpeedUp;
	}

	private void OnEnable()
	{
		Rewired_RL.Player.AddInputEventDelegate(m_toggleSpeedUp, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
		Rewired_RL.Player.AddInputEventDelegate(m_toggleSpeedUp, UpdateLoopType.Update, InputActionEventType.ButtonJustReleased, "Window_Confirm");
	}

	private void OnDisable()
	{
		if (!GameManager.IsApplicationClosing)
		{
			Rewired_RL.Player.RemoveInputEventDelegate(m_toggleSpeedUp, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
			Rewired_RL.Player.RemoveInputEventDelegate(m_toggleSpeedUp, UpdateLoopType.Update, InputActionEventType.ButtonJustReleased, "Window_Confirm");
		}
	}

	private void OnDestroy()
	{
		if (m_ambienceSFXEventInstance.isValid())
		{
			m_ambienceSFXEventInstance.release();
		}
	}

	protected virtual void InitializeText()
	{
		int num = Mathf.Min(SaveManager.PlayerSaveData.HestiaCutsceneDisplayCount, Ending_EV.HESTIA_CUTSCENE_DIALOGUE_1_LOCID.Length - 1);
		m_text1.text = LocalizationManager.GetString(Ending_EV.HESTIA_CUTSCENE_DIALOGUE_1_LOCID[num], SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		m_text2.text = LocalizationManager.GetString(Ending_EV.HESTIA_CUTSCENE_DIALOGUE_2_LOCID[num], SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.Play_Hestia_DeathCutscene, value: false);
		SaveManager.PlayerSaveData.HestiaCutsceneDisplayCount++;
	}

	public IEnumerator TransitionIn()
	{
		if (!m_ambienceSFXEventInstance.isValid())
		{
			m_ambienceSFXEventInstance = AudioUtility.GetEventInstance(AmbienceSFXName, CameraController.UICamera.transform);
		}
		InitializeText();
		m_imageCanvasGroup.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
		m_imageCanvasGroup.alpha = 0f;
		m_bgCanvasGroup.alpha = 0f;
		m_text1.alpha = 0f;
		m_text2.alpha = 0f;
		m_allowSpeedUp = false;
		m_fastForwardObj.SetActive(value: false);
		m_timeScale = 1f;
		yield return TweenManager.TweenTo_UnscaledTime(m_bgCanvasGroup, 1f, Ease.None, "alpha", 1).TweenCoroutine;
		MapController.DeathMapCamera.gameObject.SetActive(value: false);
		yield return Wait(0.5f);
		(WindowManager.GetWindowController(WindowID.PlayerDeath) as PlayerDeathWindowController).StopDeathSnapshot();
		AudioManager.StopAllAudio(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		if (m_ambienceSFXEventInstance.isValid())
		{
			AudioManager.Play(null, m_ambienceSFXEventInstance);
		}
		m_timeScale = 1f;
		m_allowSpeedUp = true;
		RewiredMapController.SetMapEnabled(GameInputMode.Window, enabled: true);
		StartCoroutine(LocalScale(m_imageCanvasGroup.transform, 12f, 1f));
		StartCoroutine(Alpha(m_imageCanvasGroup, 1f, 1f));
		yield return Wait(2f);
		AudioManager.PlayOneShot(null, "event:/Cut_Scenes/sfx_openingCutscene_textIn", CameraController.UICamera.transform.position);
		StartCoroutine(Alpha(m_text1, 1f, 1f));
		yield return Wait(4f);
		AudioManager.PlayOneShot(null, "event:/Cut_Scenes/sfx_openingCutscene_textIn", CameraController.UICamera.transform.position);
		StartCoroutine(Alpha(m_text2, 1f, 1f));
		yield return Wait(6f);
		if (m_ambienceSFXEventInstance.isValid())
		{
			AudioManager.Stop(m_ambienceSFXEventInstance);
		}
		StartCoroutine(Alpha(m_text1, 1f, 0f));
		StartCoroutine(Alpha(m_text2, 1f, 0f));
		yield return Alpha(m_imageCanvasGroup, 1f, 0f);
		m_allowSpeedUp = false;
		m_timeScale = 1f;
		RewiredMapController.SetMapEnabled(GameInputMode.Window, enabled: false);
		m_fastForwardObj.SetActive(value: false);
	}

	private IEnumerator LocalScale(Transform transform, float duration, float amount)
	{
		float elapsedTime = 0f;
		Vector3 localScale = transform.localScale;
		float startingValue = localScale.x;
		float endValue = amount - startingValue;
		while (elapsedTime < duration)
		{
			yield return null;
			elapsedTime += Time.unscaledDeltaTime * m_timeScale;
			float value = GetValue(Ease.None, elapsedTime, startingValue, endValue, duration);
			localScale.x = (localScale.y = (localScale.z = value));
			transform.localScale = localScale;
		}
		localScale.x = (localScale.y = (localScale.z = amount));
		transform.localScale = localScale;
	}

	private IEnumerator Alpha(CanvasGroup canvasGroup, float duration, float amount)
	{
		float elapsedTime = 0f;
		float startingAlpha = canvasGroup.alpha;
		float endAlpha = amount - startingAlpha;
		while (elapsedTime < duration)
		{
			yield return null;
			elapsedTime += Time.unscaledDeltaTime * m_timeScale;
			canvasGroup.alpha = GetValue(Ease.None, elapsedTime, startingAlpha, endAlpha, duration);
		}
		canvasGroup.alpha = amount;
	}

	private IEnumerator Alpha(TMP_Text text, float duration, float amount)
	{
		float elapsedTime = 0f;
		float startingAlpha = text.alpha;
		float endAlpha = amount - startingAlpha;
		while (elapsedTime < duration)
		{
			yield return null;
			elapsedTime += Time.unscaledDeltaTime * m_timeScale;
			text.alpha = GetValue(Ease.None, elapsedTime, startingAlpha, endAlpha, duration);
		}
		text.alpha = amount;
	}

	private float GetValue(EaseDelegate ease, float elapsedTime, float startingValue, float endValue, float totalDuration)
	{
		return ease(elapsedTime, startingValue, endValue, totalDuration);
	}

	private IEnumerator Wait(float duration)
	{
		for (float elapsedTime = 0f; elapsedTime < duration; elapsedTime += Time.unscaledDeltaTime * m_timeScale)
		{
			yield return null;
		}
	}

	public IEnumerator TransitionOut()
	{
		yield return null;
		m_waitYield.CreateNew(0.5f, useUnscaledTime: true);
		yield return m_waitYield;
		yield return TweenManager.TweenTo_UnscaledTime(m_bgCanvasGroup, 0.5f, Ease.None, "alpha", 0).TweenCoroutine;
	}

	public override IEnumerator Run()
	{
		yield break;
	}

	private void ToggleSpeedUp(InputActionEventData eventData)
	{
		if (m_allowSpeedUp)
		{
			if (eventData.GetButtonUp())
			{
				m_timeScale = 1f;
				m_fastForwardObj.SetActive(value: false);
			}
			else
			{
				m_timeScale = 10f;
				m_fastForwardObj.SetActive(value: true);
			}
		}
	}
}
