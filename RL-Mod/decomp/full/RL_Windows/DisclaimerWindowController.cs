using System.Collections;
using System.Collections.Generic;
using Rewired;
using SceneManagement_RL;
using UnityEngine;

namespace RL_Windows;

public class DisclaimerWindowController : WindowController
{
	private const float AUTO_LOAD_DELAY = 10f;

	private const float INPUT_LOCKOUT_DURATION = 1f;

	[SerializeField]
	private RectTransform m_controllerRectTransform;

	private bool m_isLoading;

	private float m_entryTime;

	public override WindowID ID => WindowID.Disclaimer;

	protected override void OnOpen()
	{
		m_windowCanvas.gameObject.SetActive(value: true);
		m_entryTime = Time.unscaledTime;
		m_isLoading = false;
		StartCoroutine(AnimateControllerCoroutine());
	}

	private IEnumerator AnimateControllerCoroutine()
	{
		float delay2 = Time.time + 1f;
		while (Time.time < delay2)
		{
			yield return null;
		}
		float tweenAmount = 10f;
		while (true)
		{
			yield return TweenManager.TweenBy(m_controllerRectTransform, 0.5f, Ease.Quad.EaseOut, "anchoredPosition.y", tweenAmount).TweenCoroutine;
			yield return TweenManager.TweenBy(m_controllerRectTransform, 0.5f, Ease.Bounce.EaseOut, "anchoredPosition.y", 0f - tweenAmount).TweenCoroutine;
			delay2 = Time.time + 2f;
			while (Time.time < delay2)
			{
				yield return null;
			}
		}
	}

	protected override void OnClose()
	{
	}

	protected override void OnFocus()
	{
	}

	protected override void OnLostFocus()
	{
	}

	private void Update()
	{
		if (!ReInput.isReady || m_isLoading || !RewiredMapController.IsCurrentMapEnabled || Time.unscaledTime < m_entryTime + 1f)
		{
			return;
		}
		IList<Joystick> joysticks = ReInput.controllers.Joysticks;
		for (int i = 0; i < joysticks.Count; i++)
		{
			Joystick joystick = joysticks[i];
			if (Rewired_RL.IsStandardJoystick(joystick) && joystick.GetAnyButtonDown())
			{
				LoadMainMenu();
				return;
			}
		}
		if (ReInput.controllers.Keyboard.GetAnyButton())
		{
			LoadMainMenu();
		}
		else if (ReInput.controllers.Mouse.GetAnyButton())
		{
			LoadMainMenu();
		}
		else if (Time.unscaledTime > m_entryTime + 10f)
		{
			LoadMainMenu();
		}
	}

	private void LoadMainMenu()
	{
		m_isLoading = true;
		SceneLoader_RL.LoadScene(SceneID.MainMenu, TransitionID.FadeToBlackNoLoading);
	}
}
