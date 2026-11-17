using System.Collections;
using System.IO;
using FMODUnity;
using RL_Windows;
using SceneManagement_RL;
using UnityEngine;

public class SplashSceneLoader : MonoBehaviour
{
	[SerializeField]
	private TransitionID m_transitionID;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private StudioEventEmitter m_audioEmitter;

	[SerializeField]
	private SharedGameObjects_Loader m_sharedGameObjectsLoader;

	[SerializeField]
	private CanvasGroup m_loadingIndicatorCanvasGroup;

	[SerializeField]
	private RectTransform m_rotatingGear;

	private IEnumerator Start()
	{
		Application.backgroundLoadingPriority = ThreadPriority.High;
		RuntimeManager.LoadBank("Master.strings");
		RuntimeManager.LoadBank("Master", loadSamples: true);
		RuntimeManager.LoadBank("Music");
		float startTime = Time.time;
		float duration = 0.2f;
		while (Time.time < startTime + duration)
		{
			float t = Time.time - startTime;
			m_loadingIndicatorCanvasGroup.alpha = Ease.Quad.EaseOut(t, 0f, 1f, duration);
			yield return null;
		}
		CDGResources.Init(loadSynchronous: false);
		while (CDGResources.IsLoading)
		{
			yield return null;
		}
		while (RuntimeManager.AnyBankLoading())
		{
			yield return null;
		}
		yield return m_sharedGameObjectsLoader.Initialize();
		yield return null;
		Application.backgroundLoadingPriority = ThreadPriority.Low;
		yield return TweenManager.TweenTo_UnscaledTime(m_loadingIndicatorCanvasGroup, 0.2f, Ease.None, "alpha", 0).TweenCoroutine;
		if (File.Exists(SaveManager.GetConfigPath()))
		{
			while (!SaveManager.IsConfigFileLoaded)
			{
				yield return null;
			}
		}
		float volume = SaveManager.ConfigData.MasterVolume * SaveManager.ConfigData.SFXVolume;
		m_audioEmitter.Play();
		m_audioEmitter.EventInstance.setVolume(volume);
		yield return TweenManager.TweenTo_UnscaledTime(m_canvasGroup, 1f, Ease.Quad.EaseOut, "alpha", 1);
		yield return new WaitForSecondsRealtime(3f);
		yield return WaitUntilStoreInitialized();
		if (StoreAPIManager.InitState == StoreAPIManager.StoreInitState.Succeeded)
		{
			LoadScene();
		}
	}

	private IEnumerator WaitUntilStoreInitialized()
	{
		float num = StoreAPIManager.GetInitTimeout();
		if (num <= 0f)
		{
			num = float.PositiveInfinity;
		}
		float timeoutTime = Time.unscaledTime + num;
		while (StoreAPIManager.InitState == StoreAPIManager.StoreInitState.LoggingIn)
		{
			if (Time.unscaledTime > timeoutTime)
			{
				DisplayLoginFailedWindow();
				yield break;
			}
			yield return null;
		}
		if (StoreAPIManager.InitState == StoreAPIManager.StoreInitState.Failed)
		{
			DisplayLoginFailedWindow();
		}
	}

	private void DisplayLoginFailedWindow()
	{
		if (!WindowManager.GetIsWindowLoaded(WindowID.ConfirmMenu))
		{
			WindowManager.LoadWindow(WindowID.ConfirmMenu);
		}
		InitializeAPIFailMenu();
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: true);
	}

	private void InitializeAPIFailMenu()
	{
		StoreAPIManager.GetInitFailureMessage(out var title, out var description);
		ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenu) as ConfirmMenuWindowController;
		obj.SetTitleText(title, isLocID: false);
		obj.SetDescriptionText(description, isLocID: false);
		obj.SetNumberOfButtons(1);
		obj.SetOnCancelAction(QuitGame);
		ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
		buttonAtIndex.SetButtonText("LOC_ID_GENERAL_UI_OK_1", isLocID: true);
		buttonAtIndex.SetOnClickAction(QuitGame);
		Canvas componentInChildren = obj.GetComponentInChildren<Canvas>(includeInactive: true);
		componentInChildren.renderMode = RenderMode.ScreenSpaceOverlay;
		componentInChildren.sortingOrder = 10;
	}

	private void Update()
	{
		Vector3 localEulerAngles = m_rotatingGear.localEulerAngles;
		localEulerAngles.z -= 30f * Time.unscaledDeltaTime;
		m_rotatingGear.localEulerAngles = localEulerAngles;
	}

	private void QuitGame()
	{
		Application.Quit(10);
	}

	private void LoadScene()
	{
		SceneLoader_RL.LoadScene(SceneID.Disclaimer, m_transitionID);
	}
}
