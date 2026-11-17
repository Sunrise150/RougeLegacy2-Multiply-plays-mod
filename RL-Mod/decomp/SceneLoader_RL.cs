using System;
using System.Collections;
using Rewired;
using Sigtrap.Relays;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneManagement_RL;

public class SceneLoader_RL : MonoBehaviour
{
	private static Coroutine m_loadSceneCoroutine = null;

	private static Coroutine m_runTransitionWithLogicCoroutine = null;

	private static WaitForSeconds m_runTransitionOutYield;

	private static string m_previousScene;

	private static string m_currentScene;

	public static Relay<string> SceneLoadingStartRelay = new Relay<string>();

	public static Relay<string> SceneLoadingEndRelay = new Relay<string>();

	public static Relay TransitionStartRelay = new Relay();

	public static Relay TransitionCompleteRelay = new Relay();

	public static TransitionID CurrentTransitionID { get; private set; } = TransitionID.None;


	private static SceneLoader_RL Instance { get; set; }

	public static bool IsLoading => m_loadSceneCoroutine != null;

	public static bool IsRunningTransitionWithLogic => m_runTransitionWithLogicCoroutine != null;

	public static string PreviousScene
	{
		get
		{
			return m_previousScene;
		}
		private set
		{
			m_previousScene = value;
		}
	}

	public static string CurrentScene
	{
		get
		{
			return m_currentScene;
		}
		private set
		{
			m_currentScene = value;
		}
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			TransitionLibrary.InitializeTransitionInstances();
			CurrentScene = SceneManager.GetActiveScene().name;
			if (base.transform.parent == null)
			{
				UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			}
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private static IEnumerator TransitionIn(ISceneLoadingTransition transition)
	{
		CurrentTransitionID = transition.ID;
		TransitionStartRelay.Dispatch();
		if (transition != null)
		{
			transition.GameObject.SetActive(value: true);
			yield return null;
			yield return transition.TransitionIn();
		}
	}

	private static IEnumerator TransitionOut(ISceneLoadingTransition transition)
	{
		if (transition != null)
		{
			LevelManager_RL levelManager = UnityEngine.Object.FindObjectOfType<LevelManager_RL>();
			if ((bool)levelManager)
			{
				while (!levelManager.IsComplete)
				{
					yield return null;
				}
			}
			yield return null;
			yield return transition.TransitionOut();
			transition.GameObject.SetActive(value: false);
		}
		CurrentTransitionID = TransitionID.None;
		TransitionCompleteRelay.Dispatch();
	}

	public static void LoadScene(SceneID sceneID, TransitionID transitionID)
	{
		PreviousScene = SceneLoadingUtility.ActiveScene.name;
		CurrentScene = SceneLoadingUtility.GetSceneName(sceneID);
		if (m_loadSceneCoroutine == null)
		{
			m_loadSceneCoroutine = Instance.StartCoroutine(LoadSceneCoroutine(CurrentScene, transitionID));
			return;
		}
		Debug.LogFormat("<color=red>|{0}| Load Scene Coroutine is already running, but SHOULD NOT be</color>", Instance);
	}

	private static IEnumerator LoadSceneAsynchronously(string sceneName)
	{
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
		asyncOperation.allowSceneActivation = true;
		yield return asyncOperation;
	}

	private static IEnumerator LoadSceneCoroutine(string sceneName, TransitionID transitionID)
	{
		SceneLoadingStartRelay.Dispatch(sceneName);
		SetInputIsEnabled(isEnabled: false);
		if (PlayerManager.IsInstantiated && !PlayerManager.IsDisposed)
		{
			PlayerManager.GetPlayerController().CharacterHitResponse.SetInvincibleTime(9999f, additive: false, playInvincibility: false);
		}
		ISceneLoadingTransition transition = TransitionLibrary.GetTransitionInstance(transitionID) as ISceneLoadingTransition;
		yield return TransitionIn(transition);
		TweenManager.StopAllTweens(runEndHandlers: false);
		EffectManager.DisableAllEffects();
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.UpdatePools, null, null);
		yield return LoadSceneAsynchronously(sceneName);
		AnimatorUtility.ClearAnimatorTables();
		float storedScale = Time.timeScale;
		Time.timeScale = 1f;
		if (m_runTransitionOutYield == null)
		{
			m_runTransitionOutYield = new WaitForSeconds(0.1f);
		}
		yield return m_runTransitionOutYield;
		Time.timeScale = storedScale;
		RLTimeScale.Reset();
		yield return TransitionOut(transition);
		if (PlayerManager.IsInstantiated && !PlayerManager.IsDisposed)
		{
			PlayerManager.GetPlayerController().CharacterHitResponse.SetInvincibleTime(1f, additive: false, playInvincibility: false);
		}
		SetInputIsEnabled(isEnabled: true);
		SceneLoadingEndRelay.Dispatch(sceneName);
		m_loadSceneCoroutine = null;
	}

	public static void RunTransitionWithLogic(IEnumerator logicCoroutine, TransitionID transitionID, bool cleanup)
	{
		if (m_runTransitionWithLogicCoroutine != null)
		{
			Instance.StopCoroutine(m_runTransitionWithLogicCoroutine);
		}
		m_runTransitionWithLogicCoroutine = Instance.StartCoroutine(RunTransitionWithLogicCoroutine(logicCoroutine, transitionID, cleanup));
	}

	private static IEnumerator RunTransitionWithLogicCoroutine(IEnumerator logicCoroutine, TransitionID transitionID, bool cleanup)
	{
		SetInputIsEnabled(isEnabled: false);
		if (PlayerManager.IsInstantiated && !PlayerManager.IsDisposed)
		{
			PlayerManager.GetPlayerController().CharacterHitResponse.SetInvincibleTime(9999f, additive: false, playInvincibility: false);
		}
		ISceneLoadingTransition transition = TransitionLibrary.GetTransitionInstance(transitionID) as ISceneLoadingTransition;
		yield return TransitionIn(transition);
		if (cleanup)
		{
			TweenManager.StopAllTweens(runEndHandlers: false);
			yield return logicCoroutine;
			AnimatorUtility.ClearAnimatorTables();
		}
		else
		{
			yield return logicCoroutine;
		}
		yield return TransitionOut(transition);
		if (PlayerManager.IsInstantiated && !PlayerManager.IsDisposed)
		{
			PlayerManager.GetPlayerController().CharacterHitResponse.SetInvincibleTime(1f, additive: false, playInvincibility: false);
		}
		SetInputIsEnabled(isEnabled: true);
		m_runTransitionWithLogicCoroutine = null;
	}

	public static void RunTransitionWithLogic(Action action, TransitionID transitionID, bool cleanup)
	{
		if (m_runTransitionWithLogicCoroutine != null)
		{
			Instance.StopCoroutine(m_runTransitionWithLogicCoroutine);
		}
		m_runTransitionWithLogicCoroutine = Instance.StartCoroutine(RunTransitionWithLogicCoroutine(action, transitionID, cleanup));
	}

	private static IEnumerator RunTransitionWithLogicCoroutine(Action action, TransitionID transitionID, bool cleanup)
	{
		SetInputIsEnabled(isEnabled: false);
		if (PlayerManager.IsInstantiated && !PlayerManager.IsDisposed)
		{
			PlayerManager.GetPlayerController().CharacterHitResponse.SetInvincibleTime(9999f, additive: false, playInvincibility: false);
		}
		ISceneLoadingTransition transition = TransitionLibrary.GetTransitionInstance(transitionID) as ISceneLoadingTransition;
		yield return TransitionIn(transition);
		if (cleanup)
		{
			TweenManager.StopAllTweens(runEndHandlers: false);
			action();
			AnimatorUtility.ClearAnimatorTables();
		}
		else
		{
			action();
		}
		yield return TransitionOut(transition);
		if (PlayerManager.IsInstantiated && !PlayerManager.IsDisposed)
		{
			PlayerManager.GetPlayerController().CharacterHitResponse.SetInvincibleTime(1f, additive: false, playInvincibility: false);
		}
		SetInputIsEnabled(isEnabled: true);
		m_runTransitionWithLogicCoroutine = null;
	}

	private static void SetInputIsEnabled(bool isEnabled)
	{
		if (ReInput.isReady)
		{
			RewiredMapController.SetCurrentMapEnabled(isEnabled);
		}
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.2.0.10050' (yours is '8.2.0.7535-95108c96')
