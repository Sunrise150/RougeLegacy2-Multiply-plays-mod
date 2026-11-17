using System;
using System.Collections;
using Rewired;
using UnityEngine;

public class RLInputRemapper : MonoBehaviour
{
	private static RLInputRemapper Instance;

	private Action<bool> m_onCompleteAction;

	private InputMapper m_inputMapper = new InputMapper();

	private InputMapper m_mouseInputMapper = new InputMapper();

	private InputMapper.Context m_context = new InputMapper.Context();

	private InputMapper.Context m_mouseContext = new InputMapper.Context();

	private ControllerMap m_mouseControllerMap;

	private ControllerMap m_keyboardControllerMap;

	private ControllerMap m_gamepadControllerMap;

	private bool m_isInitialized;

	private ControllerMap m_windowKBControllerMap;

	private ControllerMap m_windowMouseControllerMap;

	private ControllerMap m_illegalMouseControllerMap;

	private ControllerMap m_illegalKBControllerMap;

	private ControllerMap m_illegalGamepadControllerMap;

	private Rewired_RL.InputActionType m_inputActionTypeToRemap;

	private ControllerType m_controllerTypeToRemap;

	private ActionElementMap m_aemToRemap;

	private InputAction m_inputActionToRemap;

	private ControllerMap m_controllerMapToRemap;

	private Pole m_axisContributionToRemap;

	private WaitForSecondsRealtime m_waitYield;

	private GamepadType m_currentlyUsedGamepadType;

	private InputMapper m_inputMapperUsed;

	private bool m_onCompletePlayed;

	private bool m_gamepadWasRemapped;

	private bool m_keyboardWasRemapped;

	public static bool IsInitialized => Instance.m_isInitialized;

	public static Action<bool> OnRemapComplete
	{
		get
		{
			return Instance.m_onCompleteAction;
		}
		set
		{
			Instance.m_onCompleteAction = value;
		}
	}

	private bool RemappingSucceeded => m_inputMapperUsed != null;

	private string GetWindowActionName(string actionName)
	{
		return actionName switch
		{
			"MoveHorizontal" => "Window_Horizontal", 
			"MoveVertical" => "Window_Vertical", 
			_ => null, 
		};
	}

	public static void SetRemappingFlagDirty(bool flagGamepad)
	{
		if (!flagGamepad)
		{
			Instance.m_keyboardWasRemapped = true;
		}
		else
		{
			Instance.m_gamepadWasRemapped = true;
		}
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		if (!IsInitialized)
		{
			Initialize();
		}
	}

	private void OnEnable()
	{
		m_gamepadWasRemapped = false;
		m_keyboardWasRemapped = false;
		Controller controller = ReInput.controllers.GetLastActiveController();
		int firstAvailableJoystickControllerID = RewiredOnStartupController.GetFirstAvailableJoystickControllerID();
		if (controller.identifier.controllerType != ControllerType.Joystick && firstAvailableJoystickControllerID != -1)
		{
			controller = Rewired_RL.Player.controllers.GetController(ControllerType.Joystick, firstAvailableJoystickControllerID);
		}
		if (controller.type == ControllerType.Joystick)
		{
			m_currentlyUsedGamepadType = ControllerGlyphLibrary.GetGlyphData(controller.hardwareTypeGuid).GamepadType;
		}
	}

	private void OnDisable()
	{
		if (m_gamepadWasRemapped)
		{
			SaveManager.SaveControllerMap(saveGamepad: true);
			SaveManager.LoadAllControllerMaps();
			m_gamepadWasRemapped = false;
		}
		if (m_keyboardWasRemapped)
		{
			SaveManager.SaveControllerMap(saveGamepad: false);
			m_keyboardWasRemapped = false;
		}
	}

	private void OnDestroy()
	{
		m_inputMapper.Stop();
		m_mouseInputMapper.Stop();
		m_inputMapper.RemoveAllEventListeners();
		m_mouseInputMapper.RemoveAllEventListeners();
	}

	private void Initialize()
	{
		InitializeMapperOptions(m_inputMapper);
		InitializeMapperOptions(m_mouseInputMapper);
		m_waitYield = new WaitForSecondsRealtime(0.1f);
		m_isInitialized = true;
	}

	private void InitializeControllerMap(ControllerType controllerType)
	{
		Player player = ReInput.players.GetPlayer(0);
		int mapCategoryID = Rewired_RL.GetMapCategoryID(Rewired_RL.MapCategoryType.ActionRemappable);
		int mapCategoryID2 = Rewired_RL.GetMapCategoryID(Rewired_RL.MapCategoryType.WindowRemappable);
		int mapCategoryID3 = Rewired_RL.GetMapCategoryID(Rewired_RL.MapCategoryType.IllegalAssignments);
		int controllerId = ReInput.controllers.GetLastActiveController().identifier.controllerId;
		switch (controllerType)
		{
		case ControllerType.Keyboard:
			m_keyboardControllerMap = player.controllers.maps.GetMap(ControllerType.Keyboard, 0, mapCategoryID, 0);
			m_windowKBControllerMap = player.controllers.maps.GetMap(ControllerType.Keyboard, 0, mapCategoryID2, 0);
			m_illegalKBControllerMap = player.controllers.maps.GetMap(ControllerType.Keyboard, 0, mapCategoryID3, 0);
			break;
		case ControllerType.Mouse:
			m_mouseControllerMap = player.controllers.maps.GetMap(ControllerType.Mouse, 0, mapCategoryID, 0);
			m_windowMouseControllerMap = player.controllers.maps.GetMap(ControllerType.Mouse, 0, mapCategoryID2, 0);
			m_illegalMouseControllerMap = player.controllers.maps.GetMap(ControllerType.Mouse, 0, mapCategoryID3, 0);
			break;
		case ControllerType.Joystick:
			m_gamepadControllerMap = player.controllers.maps.GetMap(ControllerType.Joystick, controllerId, mapCategoryID, 0);
			m_illegalGamepadControllerMap = player.controllers.maps.GetMap(ControllerType.Joystick, controllerId, mapCategoryID3, 0);
			break;
		}
	}

	private void InitializeMapperOptions(InputMapper inputMapper)
	{
		inputMapper.options.allowButtonsOnFullAxisAssignment = true;
		inputMapper.options.allowKeyboardKeysWithModifiers = false;
		inputMapper.options.allowKeyboardModifierKeyAsPrimary = true;
		inputMapper.options.ignoreMouseXAxis = true;
		inputMapper.options.ignoreMouseYAxis = true;
		inputMapper.options.timeout = 3f;
		InputMapper.Options options = inputMapper.options;
		options.isElementAllowedCallback = (Predicate<ControllerPollingInfo>)Delegate.Combine(options.isElementAllowedCallback, new Predicate<ControllerPollingInfo>(OnIsElementAllowed));
		inputMapper.InputMappedEvent += OnInputDetected;
		inputMapper.StoppedEvent += OnMappingComplete;
		inputMapper.ConflictFoundEvent += OnConflictFound;
	}

	public static void ChangeInputRequested(Rewired_RL.InputActionType actionInputType, bool useGamepad, Pole axis = Pole.Positive)
	{
		if (!IsInitialized)
		{
			Instance.Initialize();
		}
		if (!useGamepad)
		{
			Instance.Internal_ChangeInputRequested(actionInputType, ControllerType.Keyboard, axis);
		}
		else
		{
			Instance.Internal_ChangeInputRequested(actionInputType, ControllerType.Joystick, axis);
		}
	}

	private void Internal_ChangeInputRequested(Rewired_RL.InputActionType inputActionType, ControllerType controllerType, Pole axis)
	{
		m_inputActionToRemap = ReInput.mapping.GetAction(Rewired_RL.GetString(inputActionType));
		m_inputActionTypeToRemap = inputActionType;
		m_controllerTypeToRemap = controllerType;
		m_axisContributionToRemap = axis;
		if (controllerType == ControllerType.Keyboard)
		{
			InitializeControllerMap(ControllerType.Keyboard);
			InitializeControllerMap(ControllerType.Mouse);
			if (Rewired_RL.DoesActionExistInMapCategory(m_inputActionToRemap.name, Rewired_RL.MapCategoryType.ActionRemappable, ControllerType.Mouse, axis))
			{
				m_controllerMapToRemap = m_mouseControllerMap;
			}
			else
			{
				if (!Rewired_RL.DoesActionExistInMapCategory(m_inputActionToRemap.name, Rewired_RL.MapCategoryType.ActionRemappable, ControllerType.Keyboard, axis))
				{
					throw new Exception("RLInputRemapper.Internal_ChangeInputRequested() - Could not find ActionElementMap for Action: " + inputActionType.ToString() + " assigned on either the Keyboard map or Mouse map.  Double-check RewiredInputManager.");
				}
				m_controllerMapToRemap = m_keyboardControllerMap;
			}
		}
		else
		{
			InitializeControllerMap(ControllerType.Joystick);
			m_controllerMapToRemap = m_gamepadControllerMap;
		}
		if (m_controllerMapToRemap == null)
		{
			Debug.Log("Failed to find controller to remap. Chances are the player changed controllers in mid-remap.");
			m_onCompletePlayed = true;
			OnFailedMapping();
			return;
		}
		int controllerID = 0;
		if (controllerType == ControllerType.Joystick)
		{
			controllerID = m_controllerMapToRemap.controllerId;
		}
		m_aemToRemap = Rewired_RL.GetActionElementMap(controllerType == ControllerType.Joystick, m_inputActionToRemap, m_axisContributionToRemap, prioritizeNonRemappable: false, controllerID);
		ControllerMap controllerMap = ((controllerType == ControllerType.Keyboard) ? m_keyboardControllerMap : m_gamepadControllerMap);
		AxisRange actionRange = AxisRange.Positive;
		if (axis == Pole.Negative)
		{
			actionRange = AxisRange.Negative;
		}
		m_context.actionId = m_inputActionToRemap.id;
		m_context.controllerMap = controllerMap;
		m_context.actionRange = actionRange;
		m_context.actionElementMapToReplace = controllerMap.GetElementMap(m_aemToRemap.id);
		if (controllerType == ControllerType.Keyboard)
		{
			m_mouseContext.actionId = m_inputActionToRemap.id;
			m_mouseContext.controllerMap = m_mouseControllerMap;
			m_mouseContext.actionRange = actionRange;
			m_mouseContext.actionElementMapToReplace = m_mouseControllerMap.GetElementMap(m_aemToRemap.id);
		}
		m_inputMapperUsed = null;
		StartCoroutine(StartListeningCoroutine(controllerType == ControllerType.Keyboard));
	}

	private IEnumerator StartListeningCoroutine(bool startMouseInputMapper)
	{
		m_onCompletePlayed = false;
		yield return m_waitYield;
		m_inputMapper.Start(m_context);
		if (startMouseInputMapper)
		{
			m_mouseInputMapper.Start(m_mouseContext);
		}
		Debug.Log("Listening for Remap input...");
	}

	private void OnInputDetected(InputMapper.InputMappedEventData data)
	{
		m_inputMapperUsed = data.inputMapper;
		Debug.Log("Remap input detected...");
		m_inputMapper.Stop();
		m_mouseInputMapper.Stop();
		if (data.actionElementMap.controllerMap != m_controllerMapToRemap)
		{
			m_controllerMapToRemap.DeleteElementMap(m_aemToRemap.id);
		}
	}

	private void OnMappingComplete(InputMapper.StoppedEventData data)
	{
		if (!RemappingSucceeded || m_inputMapperUsed == data.inputMapper)
		{
			if (RemappingSucceeded && data.inputMapper == m_inputMapperUsed && !m_onCompletePlayed)
			{
				m_onCompletePlayed = true;
				StartCoroutine(OnSuccessfulMappingCoroutine());
			}
			else if (!RemappingSucceeded && !m_onCompletePlayed)
			{
				m_onCompletePlayed = true;
				OnFailedMapping();
			}
		}
	}

	private void OnFailedMapping()
	{
		Debug.Log("Remapping failed or cancelled by user.");
		if (m_onCompleteAction != null)
		{
			m_onCompleteAction(obj: false);
		}
	}

	private IEnumerator OnSuccessfulMappingCoroutine()
	{
		yield return m_waitYield;
		Debug.Log(string.Concat(str3: ((m_controllerTypeToRemap != ControllerType.Joystick) ? Rewired_RL.GetActionElementMap(useGamepad: false, m_inputActionToRemap, m_axisContributionToRemap, prioritizeNonRemappable: false) : Rewired_RL.GetActionElementMap(useGamepad: true, m_inputActionToRemap, m_axisContributionToRemap, prioritizeNonRemappable: false, GamepadImageSelectorController.ControllerID)).elementIdentifierName, str0: "Remapping successful.  Action: ", str1: m_inputActionToRemap.name, str2: " remapped to element: "));
		string windowActionName = GetWindowActionName(m_inputActionToRemap.name);
		if (m_controllerTypeToRemap == ControllerType.Keyboard && windowActionName != null)
		{
			ReassignAllWindowMappableActions();
		}
		if (m_controllerTypeToRemap == ControllerType.Keyboard)
		{
			m_keyboardWasRemapped = true;
		}
		else if (m_controllerTypeToRemap == ControllerType.Joystick)
		{
			foreach (Joystick joystick in ReInput.controllers.Joysticks)
			{
				if (Rewired_RL.IsStandardJoystick(joystick))
				{
					ControllerMap map = m_gamepadControllerMap.ToControllerTemplateMap<IGamepadTemplate>().ToControllerMap(joystick);
					Rewired_RL.Player.controllers.maps.AddMap(ControllerType.Joystick, joystick.identifier.controllerId, map);
				}
			}
			m_gamepadWasRemapped = true;
		}
		if (m_controllerTypeToRemap == ControllerType.Joystick)
		{
			TextGlyphManager.CreateAllGlyphTables(ControllerType.Joystick);
		}
		else
		{
			TextGlyphManager.CreateAllGlyphTables(ControllerType.Keyboard);
		}
		if (m_onCompleteAction != null)
		{
			m_onCompleteAction(obj: true);
		}
	}

	private void ReassignAllWindowMappableActions()
	{
		Debug.Log("Reassigning all WindowRemappable entries...");
		ReassignWindowMappableAction("MoveHorizontal", Pole.Positive);
		ReassignWindowMappableAction("MoveHorizontal", Pole.Negative);
		ReassignWindowMappableAction("MoveVertical", Pole.Positive);
		ReassignWindowMappableAction("MoveVertical", Pole.Negative);
	}

	private void ReassignWindowMappableAction(string movementActionName, Pole axis)
	{
		string windowActionName = GetWindowActionName(movementActionName);
		InputAction action = ReInput.mapping.GetAction(windowActionName);
		ControllerMap controllerMap = (Rewired_RL.DoesActionExistInMapCategory(windowActionName, Rewired_RL.MapCategoryType.WindowRemappable, ControllerType.Mouse, axis) ? m_windowMouseControllerMap : m_windowKBControllerMap);
		ActionElementMap actionElementMap = Rewired_RL.GetActionElementMap(useGamepad: false, windowActionName, axis, prioritizeNonRemappable: false, 0);
		controllerMap.DeleteElementMap(actionElementMap.id);
		bool num = Rewired_RL.DoesActionExistInMapCategory(movementActionName, Rewired_RL.MapCategoryType.ActionRemappable, ControllerType.Mouse, axis);
		if (!num)
		{
			_ = m_keyboardControllerMap;
		}
		else
		{
			_ = m_mouseControllerMap;
		}
		ActionElementMap actionElementMap2 = Rewired_RL.GetActionElementMap(useGamepad: false, movementActionName, axis, prioritizeNonRemappable: false, 0);
		controllerMap = (num ? m_windowMouseControllerMap : m_windowKBControllerMap);
		ElementAssignment elementAssignment = new ElementAssignment(controllerMap.controllerType, actionElementMap2.elementType, actionElementMap2.elementIdentifierId, actionElementMap2.axisRange, actionElementMap2.keyCode, actionElementMap2.modifierKeyFlags, action.id, axis, actionElementMap2.invert);
		controllerMap.CreateElementMap(elementAssignment);
	}

	private bool OnIsElementAllowed(ControllerPollingInfo info)
	{
		if (info.keyboardKey == KeyCode.Escape)
		{
			m_inputMapper.Stop();
			m_mouseInputMapper.Stop();
			return false;
		}
		if (info.controllerType == ControllerType.Joystick)
		{
			if (ControllerGlyphLibrary.GetGlyphData(info.controller.hardwareTypeGuid).GamepadType != m_currentlyUsedGamepadType)
			{
				return false;
			}
			if (m_illegalGamepadControllerMap.ContainsElementIdentifier(info.elementIdentifierId))
			{
				return false;
			}
			if (info.axisPole == Pole.Negative)
			{
				return false;
			}
		}
		if (info.controllerType == ControllerType.Keyboard && m_illegalKBControllerMap.ContainsElementIdentifier(info.elementIdentifierId))
		{
			return false;
		}
		if (info.controllerType == ControllerType.Mouse && m_illegalMouseControllerMap.ContainsElementIdentifier(info.elementIdentifierId))
		{
			return false;
		}
		return true;
	}

	private void OnConflictFound(InputMapper.ConflictFoundEventData data)
	{
		bool flag = false;
		foreach (ElementAssignmentConflictInfo conflict in data.conflicts)
		{
			ActionElementMap elementMap = conflict.controllerMap.GetElementMap(conflict.elementMapId);
			if (elementMap.elementIdentifierId != m_aemToRemap.elementIdentifierId || elementMap.controllerMap.controllerType != m_aemToRemap.controllerMap.controllerType)
			{
				ElementAssignment elementAssignment = new ElementAssignment(m_aemToRemap.controllerMap.controllerType, m_aemToRemap.elementType, m_aemToRemap.elementIdentifierId, elementMap.axisRange, m_aemToRemap.keyCode, m_aemToRemap.modifierKeyFlags, elementMap.actionId, elementMap.axisContribution, m_aemToRemap.invert);
				m_controllerMapToRemap.ReplaceOrCreateElementMap(elementAssignment);
				flag = true;
			}
		}
		if (flag)
		{
			data.responseCallback(InputMapper.ConflictResponse.Replace);
		}
		else
		{
			data.responseCallback(InputMapper.ConflictResponse.Ignore);
		}
	}

	private void CreateDefaultElementMap(Rewired_RL.InputActionType actionInputType, ControllerType controllerType)
	{
		Controller controller = ReInput.controllers.GetController(controllerType, 0);
		ControllerMap controllerMap = ((controllerType == ControllerType.Keyboard) ? m_keyboardControllerMap : m_gamepadControllerMap);
		int layoutId = ReInput.mapping.GetLayoutId(ControllerType.Keyboard, "Default");
		int mapCategoryId = ReInput.mapping.GetMapCategoryId("Player");
		ActionElementMap firstElementMapWithAction = ReInput.mapping.GetControllerMapInstance(controller, mapCategoryId, layoutId).GetFirstElementMapWithAction(Rewired_RL.GetString(actionInputType));
		ElementAssignment elementAssignment = new ElementAssignment(controllerMap.controllerType, firstElementMapWithAction.elementType, firstElementMapWithAction.elementIdentifierId, firstElementMapWithAction.axisRange, firstElementMapWithAction.keyCode, firstElementMapWithAction.modifierKeyFlags, firstElementMapWithAction.actionId, firstElementMapWithAction.axisContribution, firstElementMapWithAction.invert);
		controllerMap.CreateElementMap(elementAssignment);
	}
}
