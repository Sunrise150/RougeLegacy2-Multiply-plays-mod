using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using RLAudio;
using RL_Windows;
using Rewired;
using SceneManagement_RL;
using Sigtrap.Relays;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.UI;

public class MainMenuWindowController : WindowController
{
	[SerializeField]
	private MainMenuButton m_startingButton;

	[SerializeField]
	private CanvasGroup m_startingTextCanvasGroup;

	[SerializeField]
	private TMP_Text m_boilerText;

	[SerializeField]
	private TMP_Text m_versionNumberText;

	[SerializeField]
	private TMP_Text m_updateNumberText;

	[SerializeField]
	private TMP_Text m_profileSlotText;

	[SerializeField]
	private TMP_Text m_platformUsernameText;

	[SerializeField]
	private Image m_logoImage;

	[SerializeField]
	private Animator m_logoAnimator;

	[SerializeField]
	private PlayableDirector m_playableDirector;

	[SerializeField]
	private CanvasGroup m_part1Canvas;

	[SerializeField]
	private CanvasGroup m_part2Canvas;

	[SerializeField]
	private PatchNotesController m_patchNotesController;

	[SerializeField]
	private GameObject m_backupInstructionText;

	[SerializeField]
	private IntroNewGamePlusChanger m_newGamePlusChanger;

	[SerializeField]
	private GameObject m_discordLink;

	[SerializeField]
	private GameObject m_discordText;

	[SerializeField]
	private UnityEvent m_selectionChangeEvent;

	[SerializeField]
	private UnityEvent m_selectEvent;

	[Header("Intro Cutscene")]
	[SerializeField]
	private CanvasGroup[] m_introTextCanvasGroups;

	[SerializeField]
	private GameObject m_fastForwardObj;

	[SerializeField]
	private MobilePostProcessing m_postProcessing;

	[SerializeField]
	private MobilePostProcessingProfile m_undergroundPPProfile;

	private bool m_allowFastForward;

	private bool m_lockInput;

	private bool m_isLoading;

	private bool m_menuDisplayed;

	private List<MainMenuButton> m_menuButtonList;

	private int m_currentSelectedIndex;

	private WaitRL_Yield m_waitYield;

	public Relay<bool> LoadGameRelay = new Relay<bool>();

	public Relay CutsceneStartRelay = new Relay();

	public Relay CutsceneCompleteRelay = new Relay();

	public Relay<bool> CutsceneFastForwardRelay = new Relay<bool>();

	public Relay CutsceneBlackRelay = new Relay();

	public Relay<CanvasGroup> CutsceneDisplayTextRelay = new Relay<CanvasGroup>();

	public Relay CutsceneCastleAppearsRelay = new Relay();

	public Relay CloudsAppearRelay = new Relay();

	public Relay CutsceneStarsAppearRelay = new Relay();

	private Action m_failedLoadQuitGame;

	private Action m_confirmMenu_Cancel;

	private Action m_confirmWorldReset_Confirm;

	private Action m_switchProfile;

	private Action m_loadBackupWindow;

	private Action<InputActionEventData> m_onConfirmInputHandler;

	private Action<InputActionEventData> m_onVerticalInputHandler;

	private Action<InputActionEventData> m_toggleSpeedUp;

	private static bool m_hasEverHadFocus;

	public float TextAlphaMod { get; set; }

	public override WindowID ID => WindowID.MainMenu;

	private void Awake()
	{
		m_failedLoadQuitGame = FailedLoadQuitGame;
		m_confirmMenu_Cancel = ConfirmMenu_Cancel;
		m_switchProfile = SwitchProfile;
		m_loadBackupWindow = LoadBackupWindow;
		m_confirmWorldReset_Confirm = ConfirmWorldReset_Confirm;
		m_onConfirmInputHandler = OnConfirmInputHandler;
		m_onVerticalInputHandler = OnVerticalInputHandler;
		m_toggleSpeedUp = ToggleSpeedUp;
	}

	public override void Initialize()
	{
		base.Initialize();
		m_waitYield = new WaitRL_Yield(0f);
		m_menuDisplayed = false;
		m_versionNumberText.text = System_EV.GetVersionString();
		m_updateNumberText.text = 32.ToString();
		m_menuButtonList = new List<MainMenuButton>();
		GetComponentsInChildren(includeInactive: true, m_menuButtonList);
		for (int i = 0; i < m_menuButtonList.Count; i++)
		{
			MainMenuButton mainMenuButton = m_menuButtonList[i];
			mainMenuButton.Initialize(this, i);
			mainMenuButton.MenuButtonSelected = (MainMenuButtonSelectedHandler)Delegate.Combine(mainMenuButton.MenuButtonSelected, new MainMenuButtonSelectedHandler(UpdateSelectedOptionItem));
			mainMenuButton.MenuButtonActivated = (MainMenuButtonSelectedHandler)Delegate.Combine(mainMenuButton.MenuButtonActivated, new MainMenuButtonSelectedHandler(PlaySelectedSFX));
		}
	}

	protected override void OnOpen()
	{
		CameraController.GameCamera.transform.position = new Vector3(-60.07f, 13.03f, -25f);
		StartCoroutine(OnOpenCoroutine());
	}

	private IEnumerator OnOpenCoroutine()
	{
		Messenger<SceneMessenger, SceneEvent>.Broadcast(SceneEvent.EnterMainMenu, this, EventArgs.Empty);
		m_lockInput = false;
		InitializeIntroCutscene();
		m_startingButton.OnSelect(null);
		m_windowCanvas.gameObject.SetActive(value: true);
		float saveTimer = Time.unscaledTime + 30f;
		while (SaveFileSystem.IsSaving && Time.unscaledTime < saveTimer)
		{
			yield return null;
		}
		SaveManager.LoadCurrentProfileData();
		if (SaveManager.LoadingFailed)
		{
			Invoke("DisplayLoadingFailed", 0f);
		}
		if ((bool)m_newGamePlusChanger)
		{
			m_newGamePlusChanger.UpdateMainMenu();
		}
		if (!m_menuDisplayed)
		{
			m_isLoading = false;
			TextAlphaMod = 1f;
			m_menuButtonList[0].OnSelect(null);
			m_part1Canvas.gameObject.SetActive(value: true);
			m_part2Canvas.gameObject.SetActive(value: false);
			m_part1Canvas.alpha = 1f;
			m_part2Canvas.alpha = 0f;
			m_logoAnimator.SetTrigger("Appear");
			m_part2Canvas.interactable = false;
		}
		else
		{
			m_logoAnimator.SetTrigger("InstantDisappear");
			m_part1Canvas.gameObject.SetActive(value: false);
			m_part2Canvas.gameObject.SetActive(value: true);
			m_part1Canvas.alpha = 0f;
			m_part2Canvas.alpha = 1f;
			m_part2Canvas.interactable = true;
		}
		UpdateSelectedProfileText();
	}

	private void UpdateSelectedProfileText()
	{
		m_profileSlotText.text = string.Format(LocalizationManager.GetString("LOC_ID_MAIN_MENU_PROFILE_SLOT_1", isFemale: false), SaveManager.CurrentProfile + 1);
	}

	private void InitializeIntroCutscene()
	{
		CanvasGroup[] introTextCanvasGroups = m_introTextCanvasGroups;
		for (int i = 0; i < introTextCanvasGroups.Length; i++)
		{
			introTextCanvasGroups[i].alpha = 0f;
		}
		m_playableDirector.time = 0.0;
		m_allowFastForward = false;
		m_fastForwardObj.SetActive(value: false);
	}

	private void DisplayLoadingFailed()
	{
		InitializeLoadingFailedConfirmMenu();
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: true);
	}

	protected override void OnClose()
	{
		m_windowCanvas.gameObject.SetActive(value: false);
	}

	private void InitializeLoadingFailedConfirmMenu()
	{
		if (SaveManager.GetBackupPathes(SaveManager.CurrentProfile, SaveDataType.Player).Count <= 0)
		{
			ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenu) as ConfirmMenuWindowController;
			obj.SetTitleText("LOC_ID_MAIN_MENU_SAVE_FILE_ERROR_TITLE_1", isLocID: true);
			obj.SetDescriptionText("LOC_ID_MAIN_MENU_SAVE_FILE_ERROR_DESCRIPTION_1", isLocID: true);
			obj.SetNumberOfButtons(2);
			obj.SetOnCancelAction(m_failedLoadQuitGame);
			ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
			buttonAtIndex.SetButtonText("LOC_ID_MAIN_MENU_SWITCH_PROFILE_1", isLocID: true);
			buttonAtIndex.SetOnClickAction(m_switchProfile);
			ConfirmMenu_Button buttonAtIndex2 = obj.GetButtonAtIndex(1);
			buttonAtIndex2.SetButtonText("LOC_ID_MAIN_MENU_QUIT GAME_1", isLocID: true);
			buttonAtIndex2.SetOnClickAction(m_failedLoadQuitGame);
		}
		else
		{
			ConfirmMenuWindowController obj2 = WindowManager.GetWindowController(WindowID.ConfirmMenu) as ConfirmMenuWindowController;
			obj2.SetTitleText("LOC_ID_MAIN_MENU_SAVE_FILE_ERROR_TITLE_1", isLocID: true);
			obj2.SetDescriptionText("LOC_ID_MAIN_MENU_SAVE_FILE_ERROR_DESCRIPTION_2", isLocID: true);
			obj2.SetNumberOfButtons(2);
			obj2.SetOnCancelAction(m_failedLoadQuitGame);
			ConfirmMenu_Button buttonAtIndex3 = obj2.GetButtonAtIndex(0);
			buttonAtIndex3.SetButtonText("LOC_ID_MAIN_MENU_LOAD_BACKUP_1", isLocID: true);
			buttonAtIndex3.SetOnClickAction(m_loadBackupWindow);
			ConfirmMenu_Button buttonAtIndex4 = obj2.GetButtonAtIndex(1);
			buttonAtIndex4.SetButtonText("LOC_ID_MAIN_MENU_QUIT GAME_1", isLocID: true);
			buttonAtIndex4.SetOnClickAction(m_failedLoadQuitGame);
		}
	}

	private void SwitchProfile()
	{
		SaveManager.LoadingFailed = false;
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
		WindowManager.SetWindowIsOpen(WindowID.MainMenu, isOpen: false);
		WindowManager.SetWindowIsOpen(WindowID.Backup, isOpen: false);
		WindowManager.SetWindowIsOpen(WindowID.ProfileSelect, isOpen: true);
	}

	private void FailedLoadQuitGame()
	{
		Application.Quit(10);
		Application.Quit(30);
	}

	private void LoadBackupWindow()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
		WindowManager.SetWindowIsOpen(WindowID.Backup, isOpen: true);
	}

	protected override void OnFocus()
	{
		AddInputListeners();
		if (SaveManager.PlayerSaveData.HasStartedGame)
		{
			int newGamePlusLevel = SaveManager.PlayerSaveData.NewGamePlusLevel;
			if (newGamePlusLevel <= 0)
			{
				m_startingButton.Text.text = LocalizationManager.GetString("LOC_ID_MAIN_MENU_CONTINUE_LEGACY_1", isFemale: false);
			}
			else
			{
				m_startingButton.Text.text = LocalizationManager.GetString("LOC_ID_MAIN_MENU_CONTINUE_LEGACY_1", isFemale: false) + " +" + newGamePlusLevel;
			}
		}
		else
		{
			m_startingButton.Text.text = LocalizationManager.GetString("LOC_ID_MAIN_MENU_START_NEW_LEGACY_1", isFemale: false);
		}
	}

	protected override void OnLostFocus()
	{
		RemoveInputListeners();
	}

	private void DisplayMenu(bool animate)
	{
		if (!animate)
		{
			foreach (MainMenuButton menuButton in m_menuButtonList)
			{
				menuButton.Interactable = true;
			}
			m_menuDisplayed = true;
			m_part1Canvas.alpha = 0f;
			m_part2Canvas.alpha = 1f;
			m_part1Canvas.gameObject.SetActive(value: false);
			m_part2Canvas.gameObject.SetActive(value: true);
			m_logoAnimator.SetTrigger("InstantDisappear");
			m_part2Canvas.interactable = true;
		}
		else
		{
			m_part1Canvas.alpha = 1f;
			m_part2Canvas.alpha = 0f;
			m_part1Canvas.gameObject.SetActive(value: true);
			m_part2Canvas.gameObject.SetActive(value: true);
			m_part2Canvas.interactable = false;
			StartCoroutine(DisplayMenuAnimCoroutine());
		}
	}

	private IEnumerator DisplayMenuAnimCoroutine()
	{
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		m_logoAnimator.SetTrigger("Disappear");
		AudioManager.PlayOneShot(null, "event:/UI/FrontEnd/ui_fe_titleScreen_start");
		TweenManager.TweenTo_UnscaledTime(m_part1Canvas, 0.25f, Ease.None, "alpha", 0);
		yield return TweenManager.TweenTo_UnscaledTime(m_part2Canvas, 1f, Ease.Quad.EaseInOut, "alpha", 1).TweenCoroutine;
		m_part1Canvas.gameObject.SetActive(value: false);
		foreach (MainMenuButton menuButton in m_menuButtonList)
		{
			menuButton.Interactable = true;
		}
		m_menuDisplayed = true;
		m_part2Canvas.interactable = true;
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
	}

	public void LoadGame()
	{
		bool hasStartedGame = SaveManager.PlayerSaveData.HasStartedGame;
		LoadGameRelay.Dispatch(hasStartedGame);
		if (SaveManager.PlayerSaveData.HasStartedGame)
		{
			if (SaveManager.PlayerSaveData.IsDead)
			{
				Messenger<SceneMessenger, SceneEvent>.Broadcast(SceneEvent.ExitMainMenu, this, EventArgs.Empty);
				SceneLoader_RL.LoadScene(SceneID.Lineage, TransitionID.FadeToBlackWithLoading);
				return;
			}
			if (SaveManager.PlayerSaveData.EndingSpawnRoom >= EndingSpawnRoomType.Hallway)
			{
				WorldBuilder.FirstBiomeOverride = BiomeType.Garden;
				SceneLoader_RL.LoadScene(SceneID.World, TransitionID.FadeToBlackWithLoading);
				return;
			}
			WorldBuilder.FirstBiomeOverride = BiomeType.None;
			if (!SaveManager.PlayerSaveData.InCastle || !SaveManager.DoesSaveExist(SaveManager.CurrentProfile, SaveDataType.Stage, checkForBackup: false))
			{
				SceneLoader_RL.LoadScene(SceneID.Town, TransitionID.FadeToBlackWithLoading);
			}
			else if (SaveManager.StageSaveData.ForceResetWorld)
			{
				InitializeDeleteWorldConfirmMenu();
				WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: true);
			}
			else
			{
				SceneLoader_RL.LoadScene(SceneID.World, TransitionID.FadeToBlackWithLoading);
			}
			return;
		}
		m_part2Canvas.interactable = false;
		foreach (MainMenuButton menuButton in m_menuButtonList)
		{
			menuButton.Interactable = false;
		}
		StopAllCoroutines();
		m_isLoading = true;
		StartCoroutine(TutorialSceneCoroutine());
	}

	private void InitializeDeleteWorldConfirmMenu()
	{
		if (!WindowManager.GetIsWindowLoaded(WindowID.ConfirmMenu))
		{
			WindowManager.LoadWindow(WindowID.ConfirmMenu);
		}
		ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenu) as ConfirmMenuWindowController;
		obj.SetTitleText("LOC_ID_MAIN_MENU_WORLD_CHANGED_TITLE_1", isLocID: true);
		obj.SetDescriptionText("LOC_ID_MAIN_MENU_WORLD_CHANGED_DESCRIPTION_1", isLocID: true);
		obj.SetNumberOfButtons(2);
		obj.SetOnCancelAction(m_confirmMenu_Cancel);
		ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
		buttonAtIndex.SetButtonText("LOC_ID_MAIN_MENU_WORLD_CHANGED_BUTTON_OK_1", isLocID: true);
		buttonAtIndex.SetOnClickAction(m_confirmWorldReset_Confirm);
		ConfirmMenu_Button buttonAtIndex2 = obj.GetButtonAtIndex(1);
		buttonAtIndex2.SetButtonText("LOC_ID_MAIN_MENU_WORLD_CHANGED_BUTTON_CANCEL_1", isLocID: true);
		buttonAtIndex2.SetOnClickAction(m_confirmMenu_Cancel);
	}

	private void ConfirmWorldReset_Confirm()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
		PlayerDeathWindowController.GlobalCharacterDeathResetLogic();
		SaveManager.PlayerSaveData.IsDead = true;
		SceneLoader_RL.LoadScene(SceneID.Lineage, TransitionID.FadeToBlackWithLoading);
	}

	private void ConfirmMenu_Cancel()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
	}

	private IEnumerator TutorialSceneCoroutine()
	{
		m_lockInput = true;
		TweenManager.TweenTo(m_part2Canvas, 0.25f, Ease.None, "alpha", 0);
		TweenManager.TweenTo(m_logoImage, 1f, Ease.None, "color.a", 0);
		TweenManager.TweenBy(m_logoImage.rectTransform, 1f, Ease.Quad.EaseIn, "anchoredPosition.y", 100);
		TweenManager.TweenTo(this, 1f, Ease.None, "TextAlphaMod", 0);
		TweenManager.TweenTo(m_versionNumberText, 1f, Ease.None, "alpha", 0);
		TweenManager.TweenTo(m_boilerText, 1f, Ease.None, "alpha", 0);
		TweenManager.TweenTo(m_platformUsernameText, 1f, Ease.None, "alpha", 0);
		yield return IntroCutsceneCoroutine();
		SaveManager.PlayerSaveData.HasStartedGame = true;
		TutorialRoomController.InitializeTutorialPlayer = true;
		SceneLoader_RL.LoadScene(SceneID.Tutorial, TransitionID.FadeToBlackWithLoading);
	}

	private IEnumerator IntroCutsceneCoroutine()
	{
		m_waitYield.CreateNew(0.5f);
		yield return m_waitYield;
		CutsceneStartRelay.Dispatch();
		m_playableDirector.Play();
		m_waitYield.CreateNew(1f);
		yield return m_waitYield;
		m_allowFastForward = true;
		while (m_playableDirector.time < 4.0)
		{
			yield return null;
		}
		CutsceneBlackRelay.Dispatch();
		m_postProcessing.SetBaseProfile(m_undergroundPPProfile);
		m_playableDirector.Pause();
		m_waitYield.CreateNew(0.5f);
		yield return m_waitYield;
		int num = 5;
		for (int i = 0; i < 5; i++)
		{
			DisplayIntroText(m_introTextCanvasGroups[i], 3.5f, i * 3);
		}
		m_waitYield.CreateNew(num * 3 + 3);
		yield return m_waitYield;
		CutsceneStarsAppearRelay.Dispatch();
		m_playableDirector.Resume();
		while (m_playableDirector.time < 10.0)
		{
			yield return null;
		}
		CutsceneCastleAppearsRelay.Dispatch();
		while (m_playableDirector.time < 10.5)
		{
			yield return null;
		}
		CloudsAppearRelay.Dispatch();
		while (m_playableDirector.time < 17.0)
		{
			yield return null;
		}
		yield return DisplayIntroTextCoroutine(6, 3f);
		m_waitYield.CreateNew(3f);
		yield return m_waitYield;
		yield return DisplayIntroTextCoroutine(7, 3f);
		m_waitYield.CreateNew(3f);
		yield return m_waitYield;
		CutsceneCompleteRelay.Dispatch();
	}

	private void DisplayIntroText(int index, float duration)
	{
		CanvasGroup canvasGroup = m_introTextCanvasGroups[index];
		CutsceneDisplayTextRelay.Dispatch(canvasGroup);
		TweenManager.TweenTo(canvasGroup, 0.5f, Ease.Quad.EaseOut, "alpha", 1);
		TweenManager.TweenTo(canvasGroup, 0.5f, Ease.Quad.EaseOut, "delay", duration, "alpha", 0);
	}

	private void DisplayIntroText(CanvasGroup textCanvasGroup, float duration, float delay)
	{
		CutsceneDisplayTextRelay.Dispatch(textCanvasGroup);
		TweenManager.TweenTo(textCanvasGroup, 1f, Ease.Quad.EaseOut, "delay", delay, "alpha", 1);
		TweenManager.TweenTo(textCanvasGroup, 1f, Ease.Quad.EaseOut, "delay", duration + delay, "alpha", 0);
	}

	private IEnumerator DisplayIntroTextCoroutine(int index, float duration)
	{
		CanvasGroup textCanvasGroup = m_introTextCanvasGroups[index];
		TweenManager.TweenTo(textCanvasGroup, 0.5f, Ease.Quad.EaseOut, "alpha", 1);
		CutsceneDisplayTextRelay.Dispatch(textCanvasGroup);
		m_waitYield.CreateNew(duration);
		yield return m_waitYield;
		TweenManager.TweenTo(textCanvasGroup, 0.5f, Ease.Quad.EaseOut, "alpha", 0);
	}

	private void ToggleSpeedUp(InputActionEventData eventData)
	{
		if (m_allowFastForward)
		{
			if (!eventData.GetButtonUp())
			{
				CutsceneFastForwardRelay.Dispatch(t: true);
				RLTimeScale.SetTimeScale(TimeScaleType.Cutscene, 10f);
				m_fastForwardObj.SetActive(value: true);
			}
			else
			{
				CutsceneFastForwardRelay.Dispatch(t: false);
				RLTimeScale.SetTimeScale(TimeScaleType.Cutscene, 1f);
				m_fastForwardObj.SetActive(value: false);
			}
		}
	}

	public void SetEnablePatchNotes(bool enable)
	{
		m_patchNotesController.SetEnablePatchNotes(enable);
		m_lockInput = enable;
		if (!enable)
		{
			PlaySelectedSFX(null);
		}
	}

	private void OnVerticalInputHandler(InputActionEventData eventData)
	{
		if (!m_lockInput)
		{
			int currentSelectedIndex = m_currentSelectedIndex;
			int num = -1;
			float num2 = eventData.GetAxis();
			if (num2 == 0f)
			{
				num2 = 0f - eventData.GetAxisPrev();
			}
			num = ((!(num2 > 0f)) ? ((m_currentSelectedIndex + 1 < m_menuButtonList.Count) ? (m_currentSelectedIndex + 1) : 0) : ((m_currentSelectedIndex - 1 < 0) ? (m_menuButtonList.Count - 1) : (m_currentSelectedIndex - 1)));
			if (currentSelectedIndex != num)
			{
				m_menuButtonList[num].OnSelect(null);
			}
		}
	}

	private void OnConfirmInputHandler(InputActionEventData eventData)
	{
		if (!m_lockInput && !eventData.IsCurrentInputSource(ControllerType.Mouse) && m_menuDisplayed && m_menuButtonList.Count > 0)
		{
			m_menuButtonList[m_currentSelectedIndex].ExecuteButton();
		}
	}

	protected void UpdateSelectedOptionItem(MainMenuButton menuItem)
	{
		if (menuItem.Index != m_currentSelectedIndex)
		{
			if (m_menuButtonList[m_currentSelectedIndex] != null)
			{
				m_menuButtonList[m_currentSelectedIndex].OnDeselect(null);
			}
			m_currentSelectedIndex = menuItem.Index;
			if (m_selectionChangeEvent != null && m_menuDisplayed && menuItem.Interactable)
			{
				m_selectionChangeEvent.Invoke();
			}
		}
	}

	protected void PlaySelectedSFX(MainMenuButton menuItem)
	{
		if (m_selectEvent != null && m_menuDisplayed)
		{
			m_selectEvent.Invoke();
		}
	}

	[DllImport("user32.dll")]
	private static extern IntPtr GetForegroundWindow();

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

	private void Update()
	{
		if (!m_hasEverHadFocus)
		{
			GetWindowThreadProcessId(GetForegroundWindow(), out var processId);
			int id = Process.GetCurrentProcess().Id;
			if (processId == id)
			{
				m_hasEverHadFocus = true;
			}
		}
		else
		{
			if (!ReInput.isReady || WindowManager.ActiveWindow != this)
			{
				return;
			}
			m_startingTextCanvasGroup.alpha = Mathf.Sin(Time.timeSinceLevelLoad * 2f) * TextAlphaMod;
			if (!RewiredMapController.IsCurrentMapEnabled || m_isLoading)
			{
				return;
			}
			Keyboard keyboard = ReInput.controllers.Keyboard;
			if (keyboard.GetModifierKey(ModifierKey.Shift) && keyboard.GetKeyDown(KeyCode.B))
			{
				LoadBackupWindow();
			}
			else
			{
				if (m_menuDisplayed)
				{
					return;
				}
				IList<Joystick> joysticks = ReInput.controllers.Joysticks;
				for (int i = 0; i < joysticks.Count; i++)
				{
					Joystick joystick = joysticks[i];
					if (Rewired_RL.IsStandardJoystick(joystick) && joystick.GetAnyButtonUp())
					{
						DisplayMenu(animate: true);
						return;
					}
				}
				if (ReInput.controllers.Keyboard.GetAnyButtonUp())
				{
					DisplayMenu(animate: true);
				}
				else if (ReInput.controllers.Mouse.GetAnyButtonUp())
				{
					DisplayMenu(animate: true);
				}
			}
		}
	}

	private void AddInputListeners()
	{
		base.RewiredPlayer.AddInputEventDelegate(m_onConfirmInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
		base.RewiredPlayer.AddInputEventDelegate(m_onVerticalInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonRepeating, "Window_Vertical");
		base.RewiredPlayer.AddInputEventDelegate(m_onVerticalInputHandler, UpdateLoopType.Update, InputActionEventType.NegativeButtonRepeating, "Window_Vertical");
		base.RewiredPlayer.AddInputEventDelegate(m_toggleSpeedUp, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
		base.RewiredPlayer.AddInputEventDelegate(m_toggleSpeedUp, UpdateLoopType.Update, InputActionEventType.ButtonJustReleased, "Window_Confirm");
	}

	private void RemoveInputListeners()
	{
		base.RewiredPlayer.RemoveInputEventDelegate(m_onConfirmInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
		base.RewiredPlayer.RemoveInputEventDelegate(m_onVerticalInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonRepeating, "Window_Vertical");
		base.RewiredPlayer.RemoveInputEventDelegate(m_onVerticalInputHandler, UpdateLoopType.Update, InputActionEventType.NegativeButtonRepeating, "Window_Vertical");
		base.RewiredPlayer.RemoveInputEventDelegate(m_toggleSpeedUp, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
		base.RewiredPlayer.RemoveInputEventDelegate(m_toggleSpeedUp, UpdateLoopType.Update, InputActionEventType.ButtonJustReleased, "Window_Confirm");
	}
}
