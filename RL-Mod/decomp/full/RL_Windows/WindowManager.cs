using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rewired;
using SceneManagement_RL;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace RL_Windows;

public class WindowManager : MonoBehaviour
{
	[SerializeField]
	private WindowController[] m_windowControllerPrefabs;

	private static List<WindowController> m_openWindows = new List<WindowController>();

	private static List<WindowController> m_loadedWindows = new List<WindowController>();

	private static Coroutine m_loadWindowCoroutine = null;

	private static EventSystem m_eventSystem = null;

	private static bool m_pauseIfPossible = false;

	private static bool m_pauseForGamepadDisconnected = false;

	private static string PREFAB_PATH = "Prefabs/Managers/WindowManager";

	private static Dictionary<SceneID, List<WindowID>> SCENE_ID_TO_LOADED_WINDOW_IDS_TABLE = new Dictionary<SceneID, List<WindowID>>
	{
		{
			SceneID.MainMenu,
			new List<WindowID>
			{
				WindowID.MainMenu,
				WindowID.Options,
				WindowID.Suboptions,
				WindowID.ProfileSelect,
				WindowID.ConfirmMenu,
				WindowID.ConfirmMenuBig,
				WindowID.Backup
			}
		},
		{
			SceneID.Tutorial,
			new List<WindowID>
			{
				WindowID.Pause,
				WindowID.Options,
				WindowID.Suboptions,
				WindowID.Map,
				WindowID.PlayerCard,
				WindowID.GearCard,
				WindowID.Glossary,
				WindowID.Quest,
				WindowID.ConfirmMenu,
				WindowID.ConfirmMenuBig
			}
		},
		{
			SceneID.Town,
			new List<WindowID>
			{
				WindowID.Pause,
				WindowID.Options,
				WindowID.Suboptions,
				WindowID.Map,
				WindowID.PlayerCard,
				WindowID.GearCard,
				WindowID.Glossary,
				WindowID.Quest,
				WindowID.Blacksmith,
				WindowID.Enchantress,
				WindowID.SkillTree,
				WindowID.SkillTreePopUp,
				WindowID.Dialogue,
				WindowID.PlayerDeath,
				WindowID.ConfirmMenu,
				WindowID.ConfirmMenuBig,
				WindowID.Totem,
				WindowID.NewGamePlusNPC,
				WindowID.ChallengeNPC,
				WindowID.SoulShop
			}
		},
		{
			SceneID.World,
			new List<WindowID>
			{
				WindowID.Pause,
				WindowID.Options,
				WindowID.Suboptions,
				WindowID.Map,
				WindowID.PlayerCard,
				WindowID.GearCard,
				WindowID.Glossary,
				WindowID.Quest,
				WindowID.SpecialItemDrop,
				WindowID.PlayerDeath,
				WindowID.DeathDefy,
				WindowID.Dialogue,
				WindowID.Teleporter,
				WindowID.ConfirmMenu,
				WindowID.ConfirmMenuBig,
				WindowID.BossIntro,
				WindowID.SkillTreePopUp
			}
		},
		{
			SceneID.SkillTree,
			new List<WindowID>
			{
				WindowID.SkillTree,
				WindowID.SkillTreePopUp,
				WindowID.ConfirmMenu,
				WindowID.ConfirmMenuBig
			}
		},
		{
			SceneID.Lineage,
			new List<WindowID>
			{
				WindowID.Lineage,
				WindowID.ConfirmMenu,
				WindowID.ConfirmMenuBig
			}
		},
		{
			SceneID.Disclaimer,
			new List<WindowID> { WindowID.Disclaimer }
		},
		{
			SceneID.Pause,
			new List<WindowID>
			{
				WindowID.Pause,
				WindowID.ConfirmMenu,
				WindowID.ConfirmMenuBig
			}
		},
		{
			SceneID.Parade,
			new List<WindowID>
			{
				WindowID.Pause,
				WindowID.Options,
				WindowID.Suboptions,
				WindowID.Map,
				WindowID.PlayerCard,
				WindowID.GearCard,
				WindowID.Glossary,
				WindowID.Quest,
				WindowID.ConfirmMenu,
				WindowID.ConfirmMenuBig
			}
		},
		{
			SceneID.Credits,
			new List<WindowID>()
		},
		{
			SceneID.Testing,
			new List<WindowID>()
		}
	};

	private static string[] DISABLED_PAUSE_SCENES = new string[6] { "MainMenu", "Lineage", "Credits", "Parade", "Splash", "Disclaimer" };

	private static List<WindowController> m_openWindowHelper = new List<WindowController>();

	private static WindowManager Instance { get; set; }

	public static EventSystem EventSystem
	{
		get
		{
			if (m_eventSystem == null)
			{
				m_eventSystem = UnityEngine.Object.FindObjectOfType<EventSystem>();
				if (m_eventSystem == null)
				{
					Debug.LogFormat("<color=red>[WindowManager] Failed to find EventSystem</color>", PREFAB_PATH);
				}
			}
			return m_eventSystem;
		}
	}

	public static Dictionary<SceneID, List<WindowID>> SceneIDToLoadedWindowIDsTable => SCENE_ID_TO_LOADED_WINDOW_IDS_TABLE;

	public static WindowController ActiveWindow
	{
		get
		{
			if (OpenWindows != null && OpenWindows.Count > 0)
			{
				return OpenWindows.Last();
			}
			return null;
		}
	}

	public static List<WindowController> OpenWindows
	{
		get
		{
			return m_openWindows;
		}
		private set
		{
			m_openWindows = value;
		}
	}

	public static List<WindowController> LoadedWindows
	{
		get
		{
			return m_loadedWindows;
		}
		private set
		{
			m_loadedWindows = value;
		}
	}

	public static bool IsInstantiated => Instance != null;

	public static bool IsAnyWindowOpen => OpenWindows.Count > 0;

	private static bool CanPauseNow
	{
		get
		{
			if (RewiredMapController.IsMapEnabled(GameInputMode.Game) && WorldBuilder.Instance != null)
			{
				return WorldBuilder.State == BiomeBuildStateID.Complete;
			}
			return false;
		}
	}

	private static bool IsAlreadyPaused
	{
		get
		{
			if (RewiredMapController.IsMapEnabled(GameInputMode.Window))
			{
				return !GetIsWindowOpen(WindowID.BossIntro);
			}
			return false;
		}
	}

	public static event EventHandler<WindowStateChangeEventArgs> WindowStateChangeEvent;

	private void Awake()
	{
		if ((bool)Instance && Instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		Instance = this;
		if (!base.transform.parent)
		{
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
	}

	private void Start()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
		SceneManager.sceneLoaded += OnSceneLoaded;
		if (ReInput.isReady)
		{
			ReInput.players.GetPlayer(0).AddInputEventDelegate(OnStartButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Pause");
			ReInput.players.GetPlayer(0).AddInputEventDelegate(OnSelectButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Select");
		}
	}

	private void OnDestroy()
	{
		if (Instance != null && Instance == this)
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
			if (ReInput.isReady)
			{
				ReInput.players.GetPlayer(0).RemoveInputEventDelegate(OnStartButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Pause");
				ReInput.players.GetPlayer(0).RemoveInputEventDelegate(OnSelectButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Select");
			}
		}
	}

	private static void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
	{
		LoadSceneWindows(scene);
	}

	private void OnSelectButtonDown(InputActionEventData obj)
	{
		if (DISABLED_PAUSE_SCENES.Contains(SceneManager.GetActiveScene().name))
		{
			return;
		}
		if (GetIsWindowLoaded(WindowID.Map))
		{
			(LoadedWindows.Single((WindowController x) => x.ID == WindowID.Pause) as PauseWindowController).SetStartingSubWindow(WindowID.Map);
			SetWindowIsOpen(WindowID.Pause, isOpen: true);
		}
		else
		{
			SetWindowIsOpen(WindowID.Pause, isOpen: true);
			(LoadedWindows.Single((WindowController x) => x.ID == WindowID.Pause) as PauseWindowController).SetSelectedTab(WindowID.Map);
		}
	}

	private void OnStartButtonDown(InputActionEventData obj)
	{
		if (!DISABLED_PAUSE_SCENES.Contains(SceneManager.GetActiveScene().name))
		{
			SetWindowIsOpen(WindowID.Pause, isOpen: true);
		}
	}

	public static void CloseAllOpenWindows()
	{
		m_openWindowHelper.Clear();
		m_openWindowHelper.AddRange(OpenWindows);
		foreach (WindowController item in m_openWindowHelper)
		{
			SetWindowIsOpen(item.ID, isOpen: false);
		}
	}

	private static void CloseWindow(WindowID windowID)
	{
		WindowController windowController = OpenWindows.Single((WindowController x) => x.ID == windowID);
		if (OpenWindows.Count <= 1)
		{
			RewiredMapController.SetMap(GameInputMode.Game);
		}
		if (SceneLoader_RL.IsLoading)
		{
			RewiredMapController.SetCurrentMapEnabled(enabled: false);
		}
		windowController.SetIsOpen(isOpen: false);
		windowController.SetHasFocus(hasFocus: false);
		OpenWindows.Remove(windowController);
		UpdateOpenWindowsSortOrder();
		bool flag = false;
		if (OpenWindows.Count > 0)
		{
			EventSystem.SetSelectedGameObject(null);
			flag = OpenWindows.Any((WindowController openWindow) => openWindow.PauseGameWhenOpen);
			OpenWindows.Last().SetHasFocus(hasFocus: true);
		}
		if (GameManager.IsGamePaused && !flag)
		{
			GameManager.SetIsPaused(isPaused: false);
		}
		if (WindowManager.WindowStateChangeEvent != null)
		{
			WindowManager.WindowStateChangeEvent(Instance, new WindowStateChangeEventArgs(windowID, isOpen: true));
		}
	}

	private static void CreateWindowInstanceAndAddToLoadedWindows(WindowID windowID)
	{
		WindowController windowController = Instance.m_windowControllerPrefabs.SingleOrDefault((WindowController prefab) => prefab.ID == windowID);
		if (windowController != null)
		{
			WindowController windowController2 = UnityEngine.Object.Instantiate(windowController, Instance.transform);
			windowController2.Initialize();
			LoadedWindows.Add(windowController2);
			return;
		}
		Debug.LogFormat("<color=red>[{0}] Couldn't find WindowController Prefab with ID ({1}) in m_windowControllerPrefabs. Did you add the Window's Prefab to the WindowManager Prefab's m_windowControllerPrefabs list?</color>", Instance, windowID);
		WindowController[] windowControllerPrefabs = Instance.m_windowControllerPrefabs;
		foreach (WindowController windowController3 in windowControllerPrefabs)
		{
			Debug.LogFormat("<color=red>[{0}] ID = {1}</color>", Instance, windowController3.ID);
		}
	}

	private static void UpdateOpenWindowsSortOrder()
	{
		for (int i = 0; i < OpenWindows.Count; i++)
		{
			WindowController windowController = OpenWindows[i];
			if (windowController.SortOrderOverride == -1)
			{
				windowController.WindowCanvas.sortingOrder = (i + 1) * 10;
			}
			else
			{
				windowController.WindowCanvas.sortingOrder = windowController.SortOrderOverride;
			}
		}
	}

	public static bool GetIsWindowLoaded(WindowID windowID)
	{
		foreach (WindowController loadedWindow in LoadedWindows)
		{
			if (loadedWindow.ID == windowID)
			{
				return true;
			}
		}
		return false;
	}

	public static bool GetIsWindowOpen(WindowID windowID)
	{
		foreach (WindowController openWindow in OpenWindows)
		{
			if (openWindow.ID == windowID)
			{
				return true;
			}
		}
		return false;
	}

	public static WindowController GetWindowController(WindowID windowID)
	{
		if (GetIsWindowLoaded(windowID))
		{
			return LoadedWindows.Single((WindowController x) => x.ID == windowID);
		}
		return null;
	}

	private static List<WindowID> GetWindowsToLoad(SceneID sceneID)
	{
		List<WindowID> value = null;
		if (SceneIDToLoadedWindowIDsTable.TryGetValue(sceneID, out value))
		{
			return value;
		}
		return new List<WindowID>();
	}

	private static List<WindowID> GetWindowsToUnload(List<WindowID> windowsToLoad)
	{
		List<WindowID> list = new List<WindowID>();
		foreach (WindowController loadedWindow in LoadedWindows)
		{
			if (!windowsToLoad.Contains(loadedWindow.ID))
			{
				list.Add(loadedWindow.ID);
			}
		}
		return list;
	}

	private static void InitialiseWindowsInActiveScene()
	{
		WindowController[] array = (from windowController in UnityEngine.Object.FindObjectsOfType<WindowController>()
			where windowController.gameObject.scene.buildIndex != -1
			select windowController).ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (!array[i].transform.IsChildOf(Instance.transform))
			{
				array[i].transform.SetParent(Instance.transform);
			}
			if (!LoadedWindows.Contains(array[i]))
			{
				LoadedWindows.Add(array[i]);
			}
			array[i].Initialize();
		}
	}

	public static void LoadSceneWindows(Scene scene)
	{
		InitialiseWindowsInActiveScene();
		SceneID sceneID = SceneLoadingUtility.GetSceneID(scene.name);
		if (sceneID == SceneID.None || !SceneIDToLoadedWindowIDsTable.ContainsKey(sceneID))
		{
			return;
		}
		RewiredMapController.SetMap(GameInputMode.Game);
		if (SceneLoader_RL.IsLoading)
		{
			RewiredMapController.SetCurrentMapEnabled(enabled: false);
		}
		List<WindowID> windowsToLoad = GetWindowsToLoad(sceneID);
		UnloadWindows(GetWindowsToUnload(windowsToLoad));
		foreach (WindowController loadedWindow in LoadedWindows)
		{
			SetWindowIsOpen(loadedWindow.ID, isOpen: false);
		}
		foreach (WindowID windowID in windowsToLoad)
		{
			if (!GetIsWindowLoaded(windowID))
			{
				CreateWindowInstanceAndAddToLoadedWindows(windowID);
			}
			SetWindowIsOpen(windowID, LoadedWindows.Single((WindowController x) => x.ID == windowID).OpenOnSceneLoad);
		}
	}

	public static void LoadWindow(WindowID windowID)
	{
		CreateWindowInstanceAndAddToLoadedWindows(windowID);
	}

	private static void OpenWindow(WindowID windowID)
	{
		if (LoadedWindows.Exists((WindowController x) => x.ID == windowID))
		{
			EventSystem.SetSelectedGameObject(null);
			if (ActiveWindow != null)
			{
				ActiveWindow.SetHasFocus(hasFocus: false);
			}
			WindowController windowController = LoadedWindows.Single((WindowController x) => x.ID == windowID);
			OpenWindows.Add(windowController);
			UpdateOpenWindowsSortOrder();
			if (!GameManager.IsGamePaused && windowController.PauseGameWhenOpen)
			{
				GameManager.SetIsPaused(isPaused: true);
			}
			RewiredMapController.SetMap(GameInputMode.Window);
			if (SceneLoader_RL.IsLoading)
			{
				RewiredMapController.SetCurrentMapEnabled(enabled: false);
			}
			windowController.SetIsOpen(isOpen: true);
			windowController.SetHasFocus(hasFocus: true);
			if (WindowManager.WindowStateChangeEvent != null)
			{
				WindowManager.WindowStateChangeEvent(Instance, new WindowStateChangeEventArgs(windowID, isOpen: true));
			}
		}
		else
		{
			Debug.LogFormat("<color=red>[{0}] LoadedWindows does not contain an entry with Key ({1})</color>", Instance, windowID);
		}
	}

	public static void SetWindowIsOpen(WindowID windowID, bool isOpen)
	{
		if (isOpen)
		{
			if (!GetIsWindowLoaded(windowID))
			{
				CreateWindowInstanceAndAddToLoadedWindows(windowID);
			}
			if (GetIsWindowLoaded(windowID) && !GetIsWindowOpen(windowID))
			{
				OpenWindow(windowID);
			}
		}
		else if (GetIsWindowLoaded(windowID) && GetIsWindowOpen(windowID))
		{
			CloseWindow(windowID);
		}
	}

	private static void UnloadWindows(List<WindowID> windowsToUnload)
	{
		foreach (WindowID item in windowsToUnload)
		{
			WindowController windowController = GetWindowController(item);
			if (windowController != null && windowController.gameObject != null)
			{
				if (GetIsWindowOpen(item))
				{
					CloseWindow(item);
				}
				if (GetIsWindowLoaded(item))
				{
					LoadedWindows.Remove(windowController);
				}
				UnityEngine.Object.Destroy(windowController.gameObject);
			}
			else
			{
				Debug.LogFormat("<color=red>| {0} | Entry with ID ({1}) in Loaded Windows Table is null. If Window is Scene specific, its Destroy on Load Field should be set to True.</color>", Instance, item);
			}
		}
	}

	public static void PauseWhenPossible(bool shouldPause, bool gamepadDisconnected = false)
	{
		if (shouldPause)
		{
			if (CanPauseNow)
			{
				ForcePause();
				return;
			}
			if (IsAlreadyPaused)
			{
				ResetPauseRequestState();
				return;
			}
			m_pauseIfPossible = true;
			if (gamepadDisconnected)
			{
				m_pauseForGamepadDisconnected = true;
			}
		}
		else if (!m_pauseForGamepadDisconnected)
		{
			m_pauseIfPossible = false;
		}
	}

	private void Update()
	{
		if (m_pauseIfPossible)
		{
			if (CanPauseNow)
			{
				ForcePause();
			}
			else if (IsAlreadyPaused)
			{
				ResetPauseRequestState();
			}
		}
	}

	private static void ForcePause()
	{
		SetWindowIsOpen(WindowID.Pause, isOpen: true);
		ResetPauseRequestState();
	}

	private static void ResetPauseRequestState()
	{
		m_pauseIfPossible = false;
		m_pauseForGamepadDisconnected = false;
	}

	public static void SetWindowIsOpen(WindowID windowID, bool isOpen, TransitionID transitionID)
	{
		if (m_loadWindowCoroutine == null)
		{
			m_loadWindowCoroutine = Instance.StartCoroutine(LoadWindowWithTransitionCoroutine(windowID, isOpen, transitionID));
			return;
		}
		Debug.LogFormat("<color=red>|{0}| Load Scene Coroutine is already running, but SHOULD NOT be</color>", Instance);
	}

	private static IEnumerator LoadWindowWithTransitionCoroutine(WindowID windowID, bool isOpen, TransitionID transitionID)
	{
		yield return null;
		SetInputIsEnabled(isEnabled: false);
		ISceneLoadingTransition transition = TransitionLibrary.GetTransitionInstance(transitionID) as ISceneLoadingTransition;
		transition.GameObject.SetActive(value: true);
		yield return transition.TransitionIn();
		SetWindowIsOpen(windowID, isOpen);
		SetInputIsEnabled(isEnabled: false);
		yield return transition.TransitionOut();
		SetInputIsEnabled(isEnabled: true);
		transition.GameObject.SetActive(value: false);
		m_loadWindowCoroutine = null;
	}

	private static void SetInputIsEnabled(bool isEnabled)
	{
		if (ReInput.isReady && PlayerManager.IsInstantiated)
		{
			RewiredMapController.SetCurrentMapEnabled(isEnabled);
		}
	}
}
