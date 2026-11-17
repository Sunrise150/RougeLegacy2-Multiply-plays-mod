using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using RLAudio;
using RL_Windows;
using Rewired;
using SceneManagement_RL;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LineageWindowController : WindowController, IAudioEventEmitter, ILocalizable
{
	private const int STARTING_NUMBER_OF_SUCCESSORS = 3;

	private const float PLAYER_PLATFORM_WIDTH = 7.5f;

	[SerializeField]
	private CanvasGroup m_windowCanvasGroup;

	[SerializeField]
	private CanvasGroup m_bannerAndDescripCanvasGroup;

	[SerializeField]
	private PlayerLookController m_playerModelPrefab;

	[SerializeField]
	private LineageGoldFlagController m_goldFlagPrefab;

	[SerializeField]
	private GameObject m_goldFlagsGO;

	[SerializeField]
	private LineageRoomController m_lineageRoomController;

	[SerializeField]
	private GameObject m_triggerObject;

	[SerializeField]
	private float m_portraitXOffset;

	[SerializeField]
	private GameObject m_rerollHeirsNavObj;

	[SerializeField]
	private TMP_Text m_rerollHeirsText;

	[SerializeField]
	private GameObject m_lockClassGO;

	[SerializeField]
	private GameObject m_pressUpGO;

	[SerializeField]
	[EventRef]
	private string m_enterSceneTransitionAudioEvent;

	[SerializeField]
	[EventRef]
	private string m_heirSelectionChangeAudioEvent;

	[SerializeField]
	[EventRef]
	private string m_heirChosenAudioEvent;

	[SerializeField]
	[EventRef]
	private string m_transitionToTownAudioEvent;

	[SerializeField]
	[EventRef]
	private string m_viewPortraitsAudioEvent;

	[SerializeField]
	[EventRef]
	private string m_scrollPortraitAudioEvent;

	[SerializeField]
	[EventRef]
	private string m_portraitEndAudioEvent;

	[SerializeField]
	[EventRef]
	private string m_rerollAudioEvent;

	[SerializeField]
	[EventRef]
	private string m_weaponChangeAudioEvent;

	public static bool CharacterLoadedFromLineage;

	private CharacterData[] m_characterDataArray;

	private PlayerLookController[] m_playerModels;

	private LineageGoldFlagController[] m_goldFlags;

	private LineageHeirChangedEventArgs m_lineageEventArgs;

	private bool m_isViewingPortraits;

	private int m_selectedCharacterIndex;

	private int m_currentPortraitRoomIndex;

	private bool m_openTransitionRunning;

	private bool m_openTransitionComplete;

	private bool m_isShiftingPortraits;

	private bool m_inputListenersAdded;

	private int m_numRerolls;

	private int m_storedSeed;

	private int m_portraitsViewed;

	private bool m_upArrowAnimating;

	private bool m_switchingWeapons;

	private Action<object, EventArgs> m_refreshText;

	private Action m_cancelConfirmMenuSelection;

	private Action m_confirmHeirSelection;

	private Action m_confirmQuitSelection;

	private Action<InputActionEventData> m_onConfirmHeirButtonPressed;

	private Action<InputActionEventData> m_onViewLineageButtonPressed;

	private Action<InputActionEventData> m_onRandomizeChildrenPressed;

	private Action<InputActionEventData> m_onCancelButtonDown;

	private Action<InputActionEventData> m_onHorizontalDirectionPressed;

	private Action<InputActionEventData> m_onHorizontalButtonReleased;

	private Action<InputActionEventData> m_onVerticalButtonPressed;

	private static MaterialPropertyBlock m_matBlock;

	public string Description => ToString();

	public int CurrentSelectedCharacterIndex => m_selectedCharacterIndex;

	public int NumberOfSuccessors
	{
		get
		{
			int num = (int)SkillTreeManager.GetSkillTreeObj(SkillTreeType.More_Children).CurrentStatGain;
			return 3 + num;
		}
	}

	public override WindowID ID => WindowID.Lineage;

	public Scene Scene => base.gameObject.scene;

	private void Awake()
	{
		m_refreshText = RefreshText;
		m_cancelConfirmMenuSelection = CancelConfirmMenuSelection;
		m_confirmHeirSelection = ConfirmHeirSelection;
		m_confirmQuitSelection = ConfirmQuitSelection;
		m_onConfirmHeirButtonPressed = OnConfirmHeirButtonPressed;
		m_onViewLineageButtonPressed = OnViewLineageButtonPressed;
		m_onRandomizeChildrenPressed = OnRandomizeChildrenPressed;
		m_onCancelButtonDown = OnCancelButtonDown;
		m_onHorizontalDirectionPressed = OnHorizontalDirectionPressed;
		m_onHorizontalButtonReleased = OnHorizontalButtonReleased;
		m_onVerticalButtonPressed = OnVerticalButtonPressed;
	}

	private void Start()
	{
		Camera camera = Camera.main;
		if (CameraController.IsInstantiated)
		{
			camera = CameraController.GameCamera;
		}
		Vector3 position = camera.transform.position;
		position.x = m_lineageRoomController.EndingRoom.transform.position.x;
		position.y = m_lineageRoomController.EndingRoom.transform.position.y;
		position.x -= 3.7f;
		camera.transform.position = position;
	}

	public override void Initialize()
	{
		m_characterDataArray = new CharacterData[NumberOfSuccessors];
		m_playerModels = new PlayerLookController[NumberOfSuccessors];
		m_goldFlags = new LineageGoldFlagController[NumberOfSuccessors];
		m_lineageEventArgs = new LineageHeirChangedEventArgs(null, classLocked: false, spellLocked: false);
		base.Initialize();
	}

	private void CreateRandomCharacters()
	{
		LineageRoomController component = m_lineageRoomController.EndingRoom.GetComponent<LineageRoomController>();
		Vector3 position = component.LineageModelPositionObject.transform.position;
		float num = 7.5f / (float)Mathf.Clamp(NumberOfSuccessors - 1, 1, 999);
		float num2 = 7.5f / 2f;
		List<CharacterData> charsAlreadyChosen = new List<CharacterData>();
		for (int i = 0; i < m_characterDataArray.Length; i++)
		{
			if (!m_playerModels[i])
			{
				m_playerModels[i] = UnityEngine.Object.Instantiate(m_playerModelPrefab, component.transform);
			}
			float x = position.x - num2 + num * (float)i;
			float y = position.y;
			float z = position.z - (float)i * 0.3f;
			Vector3 position2 = new Vector3(x, y, z);
			m_playerModels[i].transform.position = position2;
			bool forceRandomizeKit = i == 0 && SaveManager.ModeSaveData.GetSoulShopObj(SoulShopType.ForceRandomizeKit).CurrentEquippedLevel > 0;
			m_characterDataArray[i] = CharacterCreator.GenerateRandomCharacter(charsAlreadyChosen, forceRandomizeKit);
			if (i == m_characterDataArray.Length - 1)
			{
				bool flag = false;
				SoulShopObj soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj(SoulShopType.ChooseYourClass);
				if (!soulShopObj.IsNativeNull() && soulShopObj.CurrentEquippedLevel > 0)
				{
					ClassType soulShopClassChosen = SaveManager.ModeSaveData.SoulShopClassChosen;
					if (soulShopClassChosen != 0)
					{
						CharacterCreator.GenerateClass(soulShopClassChosen, m_characterDataArray[i]);
						flag = true;
					}
				}
				if (flag)
				{
					if (m_characterDataArray[i].TraitOne == TraitType.RandomizeKit)
					{
						m_characterDataArray[i].TraitOne = m_characterDataArray[i].TraitTwo;
						m_characterDataArray[i].AntiqueOneOwned = m_characterDataArray[i].AntiqueTwoOwned;
						m_characterDataArray[i].TraitTwo = TraitType.None;
						m_characterDataArray[i].AntiqueTwoOwned = RelicType.None;
					}
					if (m_characterDataArray[i].TraitTwo == TraitType.RandomizeKit)
					{
						m_characterDataArray[i].TraitTwo = TraitType.None;
						m_characterDataArray[i].AntiqueTwoOwned = RelicType.None;
					}
				}
				SoulShopObj soulShopObj2 = SaveManager.ModeSaveData.GetSoulShopObj(SoulShopType.ChooseYourSpell);
				if (!soulShopObj2.IsNativeNull() && soulShopObj2.CurrentEquippedLevel > 0)
				{
					AbilityType soulShopSpellChosen = SaveManager.ModeSaveData.SoulShopSpellChosen;
					if (soulShopSpellChosen != 0)
					{
						m_characterDataArray[i].Spell = soulShopSpellChosen;
						if (m_characterDataArray[i].ClassType == ClassType.MagicWandClass && CharacterCreator.GetAvailableTalents(ClassType.MagicWandClass).IndexOf(soulShopSpellChosen) != -1)
						{
							AbilityType[] availableSpells = CharacterCreator.GetAvailableSpells(ClassType.MagicWandClass);
							AbilityType abilityType = ((availableSpells.Length != 0) ? availableSpells[RNGManager.GetRandomNumber(RngID.Lineage, "GetRandomTalent", 0, availableSpells.Length)] : AbilityType.None);
							m_characterDataArray[i].Talent = abilityType;
							if (abilityType == AbilityType.None)
							{
								Debug.Log("<color=red>ERROR: Talent list was empty when attempting to change talent to something else due to locked spell.");
							}
						}
					}
				}
				if (flag)
				{
					if (!m_lockClassGO.activeSelf)
					{
						m_lockClassGO.SetActive(value: true);
					}
					m_lockClassGO.transform.SetParent(m_playerModels[i].transform, worldPositionStays: false);
					float num3 = 1.5f / m_playerModels[i].transform.lossyScale.x;
					m_lockClassGO.transform.localScale = new Vector3(num3, num3, num3);
					m_lockClassGO.transform.localPosition = new Vector3(0f, 2.25f, 1f);
				}
				else if (m_lockClassGO.activeSelf)
				{
					m_lockClassGO.SetActive(value: false);
				}
			}
			if (SaveManager.EquipmentSaveData.EquipmentLoadoutEnabled)
			{
				SaveManager.EquipmentSaveData.GetEquipmentLoadout(m_characterDataArray[i].ClassType)?.LoadLoadout(m_characterDataArray[i]);
			}
			else
			{
				CharacterData currentCharacter = SaveManager.PlayerSaveData.CurrentCharacter;
				m_characterDataArray[i].EdgeEquipmentType = currentCharacter.EdgeEquipmentType;
				m_characterDataArray[i].CapeEquipmentType = currentCharacter.CapeEquipmentType;
				m_characterDataArray[i].HeadEquipmentType = currentCharacter.HeadEquipmentType;
				m_characterDataArray[i].ChestEquipmentType = currentCharacter.ChestEquipmentType;
				m_characterDataArray[i].TrinketEquipmentType = currentCharacter.TrinketEquipmentType;
			}
			m_playerModels[i].InitializeLook(m_characterDataArray[i]);
			Vector3 localScale = m_playerModels[i].transform.localScale;
			localScale.z = 0.5f;
			m_playerModels[i].transform.localScale = localScale;
			if (!m_goldFlags[i])
			{
				m_goldFlags[i] = UnityEngine.Object.Instantiate(m_goldFlagPrefab, component.transform);
				position2.y -= 1f;
				position2.x -= num2;
				m_goldFlags[i].transform.position = position2;
				StartCoroutine(RepositionGoldFlag(m_goldFlags[i].gameObject));
			}
			float goldGain = TraitManager.GetActualTraitGoldGain(m_characterDataArray[i].TraitOne) + TraitManager.GetActualTraitGoldGain(m_characterDataArray[i].TraitTwo);
			m_goldFlags[i].SetGoldGain(goldGain);
		}
		UpdateSelectedCharacter();
	}

	private IEnumerator RepositionGoldFlag(GameObject goldFlag)
	{
		yield return null;
		goldFlag.transform.SetParent(m_goldFlagsGO.transform, worldPositionStays: true);
		Vector3 localPosition = goldFlag.transform.localPosition;
		localPosition.z = 0f;
		goldFlag.transform.localPosition = localPosition;
		goldFlag.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
	}

	public void RunOpenTransition(float duration)
	{
		StartCoroutine(OnEnterTransitionCoroutine(duration));
	}

	private IEnumerator OnEnterTransitionCoroutine(float duration)
	{
		if (!m_openTransitionRunning)
		{
			m_openTransitionRunning = true;
			AudioManager.PlayOneShot(this, m_enterSceneTransitionAudioEvent);
			yield return TweenManager.TweenTo_UnscaledTime(Camera.main.transform, duration, Ease.Quad.EaseInOut, "position.x", m_lineageRoomController.EndingRoom.transform.position.x).TweenCoroutine;
			m_lineageRoomController.EnableSpotlight(turnOn: true);
			m_playerModels[CurrentSelectedCharacterIndex].Animator.SetBool("Victory", value: true);
			yield return TweenManager.TweenTo_UnscaledTime(m_windowCanvasGroup, 0.25f, Ease.None, "alpha", 1).TweenCoroutine;
			AddInputListeners();
			m_openTransitionRunning = false;
			m_openTransitionComplete = true;
		}
	}

	protected override void OnOpen()
	{
		m_switchingWeapons = false;
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.LanguageChanged, m_refreshText);
		int previousMasterSeed = SaveManager.PlayerSaveData.PreviousMasterSeed;
		int masterSeed = SaveManager.PlayerSaveData.MasterSeed;
		RNGSeedManager.SetMasterSeedOverride(Environment.TickCount);
		if (!AspectRatioManager.ForceEnable_16_9 && !AspectRatioManager.IsScreen_16_9_AspectRatio && AspectRatioManager.CurrentScreenAspectRatio < 1.7777778f)
		{
			AspectRatioManager.ForceEnable_16_9 = true;
			Messenger<SceneMessenger, SceneEvent>.Broadcast(SceneEvent.ResolutionChanged, this, null);
		}
		RNGManager.Reset();
		m_storedSeed = RNGManager.GetSeed(RngID.Lineage);
		if (SaveManager.PlayerSaveData.LineageSeed == -1)
		{
			SaveManager.PlayerSaveData.LineageSeed = RNGManager.GetSeed(RngID.Lineage);
			SaveManager.SaveCurrentProfileGameData(SaveDataType.Player, SavingType.FileOnly, ignoreTimeRestriction: true);
		}
		else
		{
			SaveManager.PlayerSaveData.PreviousMasterSeed = previousMasterSeed;
			SaveManager.PlayerSaveData.MasterSeed = masterSeed;
			RNGManager.SetSeed(RngID.Lineage, SaveManager.PlayerSaveData.LineageSeed);
			for (int i = 0; i < SaveManager.PlayerSaveData.TimesRolledLineage; i++)
			{
				CreateRandomCharacters();
			}
		}
		CreateRandomCharacters();
		m_windowCanvas.gameObject.SetActive(value: true);
		m_selectedCharacterIndex = 0;
		m_currentPortraitRoomIndex = 0;
		m_isViewingPortraits = false;
		m_numRerolls = SkillTreeManager.GetSkillObjLevel(SkillTreeType.Randomize_Children);
		m_numRerolls -= SaveManager.PlayerSaveData.TimesRolledLineage;
		UpdateRerollHeirsNav();
		UpdateSelectedCharacter();
		m_windowCanvasGroup.alpha = 0f;
		m_lineageRoomController.EnableSpotlight(turnOn: false);
		m_playerModels[CurrentSelectedCharacterIndex].Animator.SetBool("Victory", value: false);
		m_playerModels[CurrentSelectedCharacterIndex].Animator.Play("Idle", 0, 0f);
		TweenManager.RunFunction(0.5f, this, "RunOpenTransition", 1);
		Messenger<SceneMessenger, SceneEvent>.Broadcast(SceneEvent.EnterLineageScreen, this, EventArgs.Empty);
	}

	protected override void OnClose()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.LanguageChanged, m_refreshText);
		m_openTransitionRunning = false;
		m_openTransitionComplete = false;
		if (AspectRatioManager.ForceEnable_16_9)
		{
			AspectRatioManager.ForceEnable_16_9 = false;
			Messenger<SceneMessenger, SceneEvent>.Broadcast(SceneEvent.ResolutionChanged, this, null);
		}
		RNGManager.SetSeed(RngID.Lineage, m_storedSeed);
		RemoveInputListeners();
		m_windowCanvas.gameObject.SetActive(value: false);
	}

	private void AddInputListeners()
	{
		if (ReInput.isReady)
		{
			base.RewiredPlayer.AddInputEventDelegate(m_onConfirmHeirButtonPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
			base.RewiredPlayer.AddInputEventDelegate(m_onViewLineageButtonPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_X");
			base.RewiredPlayer.AddInputEventDelegate(m_onRandomizeChildrenPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Y");
			base.RewiredPlayer.AddInputEventDelegate(m_onCancelButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
			base.RewiredPlayer.AddInputEventDelegate(m_onHorizontalDirectionPressed, UpdateLoopType.Update, InputActionEventType.ButtonRepeating, "Window_Horizontal");
			base.RewiredPlayer.AddInputEventDelegate(m_onHorizontalDirectionPressed, UpdateLoopType.Update, InputActionEventType.NegativeButtonRepeating, "Window_Horizontal");
			base.RewiredPlayer.AddInputEventDelegate(m_onHorizontalButtonReleased, UpdateLoopType.Update, InputActionEventType.ButtonJustReleased, "Window_Horizontal");
			base.RewiredPlayer.AddInputEventDelegate(m_onHorizontalButtonReleased, UpdateLoopType.Update, InputActionEventType.NegativeButtonJustReleased, "Window_Horizontal");
			base.RewiredPlayer.AddInputEventDelegate(m_onVerticalButtonPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Vertical");
			base.RewiredPlayer.AddInputEventDelegate(m_onVerticalButtonPressed, UpdateLoopType.Update, InputActionEventType.NegativeButtonJustPressed, "Window_Vertical");
			m_inputListenersAdded = true;
		}
	}

	private void RemoveInputListeners()
	{
		if (ReInput.isReady)
		{
			base.RewiredPlayer.RemoveInputEventDelegate(m_onConfirmHeirButtonPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
			base.RewiredPlayer.RemoveInputEventDelegate(m_onViewLineageButtonPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_X");
			base.RewiredPlayer.RemoveInputEventDelegate(m_onRandomizeChildrenPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Y");
			base.RewiredPlayer.RemoveInputEventDelegate(m_onCancelButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
			base.RewiredPlayer.RemoveInputEventDelegate(m_onHorizontalDirectionPressed, UpdateLoopType.Update, InputActionEventType.ButtonRepeating, "Window_Horizontal");
			base.RewiredPlayer.RemoveInputEventDelegate(m_onHorizontalDirectionPressed, UpdateLoopType.Update, InputActionEventType.NegativeButtonRepeating, "Window_Horizontal");
			base.RewiredPlayer.RemoveInputEventDelegate(m_onHorizontalButtonReleased, UpdateLoopType.Update, InputActionEventType.ButtonJustReleased, "Window_Horizontal");
			base.RewiredPlayer.RemoveInputEventDelegate(m_onHorizontalButtonReleased, UpdateLoopType.Update, InputActionEventType.NegativeButtonJustReleased, "Window_Horizontal");
			base.RewiredPlayer.RemoveInputEventDelegate(m_onVerticalButtonPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Vertical");
			base.RewiredPlayer.RemoveInputEventDelegate(m_onVerticalButtonPressed, UpdateLoopType.Update, InputActionEventType.NegativeButtonJustPressed, "Window_Vertical");
			m_inputListenersAdded = false;
		}
	}

	protected override void OnFocus()
	{
		if (m_openTransitionComplete && !m_inputListenersAdded)
		{
			AddInputListeners();
		}
	}

	protected override void OnLostFocus()
	{
		if (m_openTransitionComplete && m_inputListenersAdded)
		{
			RemoveInputListeners();
		}
	}

	private void UpdateRerollHeirsNav()
	{
		if (m_numRerolls > 0)
		{
			if (!m_rerollHeirsNavObj.gameObject.activeSelf)
			{
				m_rerollHeirsNavObj.gameObject.SetActive(value: true);
			}
			m_rerollHeirsText.text = string.Format(LocalizationManager.GetString("LOC_ID_LINEAGE_REROLL_HEIRS_1", isFemale: false), m_numRerolls);
		}
		else if (m_rerollHeirsNavObj.gameObject.activeSelf)
		{
			m_rerollHeirsNavObj.gameObject.SetActive(value: false);
		}
	}

	private void OnRandomizeChildrenPressed(InputActionEventData data)
	{
		if (!m_isViewingPortraits && m_numRerolls > 0)
		{
			m_numRerolls--;
			AudioManager.PlayOneShot(this, m_rerollAudioEvent);
			AmbientSoundController.Instance.StopOnTransition = false;
			SceneLoader_RL.RunTransitionWithLogic(delegate
			{
				CreateRandomCharacters();
			}, TransitionID.QuickSwipe, cleanup: false);
			AmbientSoundController.Instance.StopOnTransition = true;
			SaveManager.PlayerSaveData.TimesRolledLineage++;
			SaveManager.SaveCurrentProfileGameData(SaveDataType.Player, SavingType.FileOnly, ignoreTimeRestriction: true);
			UpdateRerollHeirsNav();
		}
	}

	private void OnCancelButtonDown(InputActionEventData data)
	{
		if (m_isViewingPortraits)
		{
			if (!m_isShiftingPortraits)
			{
				TogglePortraitViewing();
			}
		}
		else
		{
			InitializeExitConfirmMenu();
			WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: true);
		}
	}

	private void OnHorizontalDirectionPressed(InputActionEventData data)
	{
		bool flag = data.GetAxis() > 0f;
		if (!m_isViewingPortraits)
		{
			if (data.GetButtonDown() || data.GetNegativeButtonDown())
			{
				int selectedCharacterIndex = m_selectedCharacterIndex;
				if (flag)
				{
					m_selectedCharacterIndex++;
				}
				else
				{
					m_selectedCharacterIndex--;
				}
				if (m_selectedCharacterIndex > NumberOfSuccessors - 1)
				{
					m_selectedCharacterIndex = 0;
				}
				else if (m_selectedCharacterIndex < 0)
				{
					m_selectedCharacterIndex = NumberOfSuccessors - 1;
				}
				if (selectedCharacterIndex != m_selectedCharacterIndex)
				{
					UpdateSelectedCharacter();
					AudioManager.PlayOneShot(this, m_heirSelectionChangeAudioEvent);
				}
			}
		}
		else if (!m_isShiftingPortraits)
		{
			ShiftPortraits(flag);
		}
	}

	private void OnHorizontalButtonReleased(InputActionEventData data)
	{
		m_portraitsViewed = 0;
	}

	private void OnVerticalButtonPressed(InputActionEventData data)
	{
		if (m_isViewingPortraits || m_switchingWeapons)
		{
			return;
		}
		CharacterData characterData = m_characterDataArray[m_selectedCharacterIndex];
		if (!IsVariantAbilityUnlocked(characterData))
		{
			return;
		}
		switch (characterData.ClassType)
		{
		case ClassType.SwordClass:
			if (characterData.Weapon != AbilityType.ChakramWeapon)
			{
				characterData.Weapon = AbilityType.ChakramWeapon;
			}
			else
			{
				characterData.Weapon = AbilityType.SwordWeapon;
			}
			break;
		case ClassType.BowClass:
			if (characterData.Weapon != AbilityType.GroundBowWeapon)
			{
				characterData.Weapon = AbilityType.GroundBowWeapon;
			}
			else
			{
				characterData.Weapon = AbilityType.BowWeapon;
			}
			break;
		case ClassType.AxeClass:
			if (characterData.Weapon != AbilityType.AxeSpinnerWeapon)
			{
				characterData.Weapon = AbilityType.AxeSpinnerWeapon;
			}
			else
			{
				characterData.Weapon = AbilityType.AxeWeapon;
			}
			break;
		case ClassType.LadleClass:
			if (characterData.Weapon != AbilityType.SpoonsWeapon)
			{
				characterData.Weapon = AbilityType.SpoonsWeapon;
			}
			else
			{
				characterData.Weapon = AbilityType.FryingPanWeapon;
			}
			break;
		case ClassType.BoxingGloveClass:
			if (characterData.Weapon != AbilityType.ExplosiveHandsWeapon)
			{
				characterData.Weapon = AbilityType.ExplosiveHandsWeapon;
			}
			else
			{
				characterData.Weapon = AbilityType.BoxingGloveWeapon;
			}
			break;
		case ClassType.MagicWandClass:
			if (characterData.Weapon != AbilityType.ScytheWeapon)
			{
				characterData.Weapon = AbilityType.ScytheWeapon;
			}
			else
			{
				characterData.Weapon = AbilityType.MagicWandWeapon;
			}
			break;
		case ClassType.LuteClass:
			if (characterData.Weapon != AbilityType.KineticBowWeapon)
			{
				characterData.Weapon = AbilityType.KineticBowWeapon;
			}
			else
			{
				characterData.Weapon = AbilityType.LuteWeapon;
			}
			break;
		}
		m_playerModels[m_selectedCharacterIndex].InitializeLook(m_characterDataArray[m_selectedCharacterIndex]);
		UpdateSelectedCharacter();
		StartCoroutine(SwitchWeaponAnimCoroutine(m_playerModels[m_selectedCharacterIndex]));
	}

	private IEnumerator SwitchWeaponAnimCoroutine(PlayerLookController lookController)
	{
		m_switchingWeapons = true;
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		if (m_matBlock == null)
		{
			m_matBlock = new MaterialPropertyBlock();
		}
		BaseEffect baseEffect = EffectManager.PlayEffect(lookController.gameObject, lookController.Animator, "LineageScreenWeaponSwap_Effect", lookController.transform.position, 0f, EffectStopType.Gracefully);
		float num = lookController.transform.localScale.x / 1.4f;
		baseEffect.transform.localScale = new Vector3(num, num, num);
		AudioManager.PlayOneShot(this, m_weaponChangeAudioEvent);
		if ((bool)lookController.CurrentWeaponGeo)
		{
			lookController.CurrentWeaponGeo.GetPropertyBlock(m_matBlock);
			Color storedColor = m_matBlock.GetColor(ShaderID_RL._AddColor);
			m_matBlock.SetColor(ShaderID_RL._AddColor, new Color(1f, 1f, 1f, 1f));
			lookController.CurrentWeaponGeo.SetPropertyBlock(m_matBlock);
			float delay = Time.unscaledTime + 0.05f;
			while (Time.unscaledTime < delay)
			{
				yield return null;
			}
			lookController.CurrentWeaponGeo.GetPropertyBlock(m_matBlock);
			m_matBlock.SetColor(ShaderID_RL._AddColor, storedColor);
			lookController.CurrentWeaponGeo.SetPropertyBlock(m_matBlock);
		}
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
		m_switchingWeapons = false;
	}

	private bool IsVariantAbilityUnlocked(CharacterData charData)
	{
		if (charData.TraitOne == TraitType.RandomizeKit || charData.TraitTwo == TraitType.RandomizeKit)
		{
			return false;
		}
		ClassType classType = charData.ClassType;
		SoulShopObj soulShopObj = null;
		bool flag = false;
		switch (classType)
		{
		case ClassType.SwordClass:
			soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj(SoulShopType.SwordVariant);
			flag = true;
			break;
		case ClassType.BowClass:
			soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj(SoulShopType.ArcherVariant);
			flag = true;
			break;
		case ClassType.AxeClass:
			soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj(SoulShopType.AxeVariant);
			flag = true;
			break;
		case ClassType.LadleClass:
			soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj(SoulShopType.LadleVariant);
			flag = true;
			break;
		case ClassType.BoxingGloveClass:
			soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj(SoulShopType.BoxerVariant);
			flag = true;
			break;
		case ClassType.MagicWandClass:
			soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj(SoulShopType.WandVariant);
			flag = true;
			break;
		case ClassType.LuteClass:
			soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj(SoulShopType.LuteVariant);
			flag = true;
			break;
		}
		if (flag)
		{
			if (!soulShopObj.IsNativeNull())
			{
				return soulShopObj.CurrentEquippedLevel > 0;
			}
			return false;
		}
		return false;
	}

	private void OnConfirmHeirButtonPressed(InputActionEventData data)
	{
		if (m_isViewingPortraits)
		{
			TogglePortraitViewing();
			return;
		}
		InitializeSelectHeirConfirmMenu();
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: true);
		AudioManager.PlayOneShot(this, m_heirChosenAudioEvent);
	}

	private void InitializeSelectHeirConfirmMenu()
	{
		ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenu) as ConfirmMenuWindowController;
		obj.SetTitleText("LOC_ID_LINEAGE_CHOOSE_HEIR_MENU_TITLE_1", isLocID: true);
		obj.SetDescriptionText("LOC_ID_LINEAGE_CHOOSE_HEIR_MENU_DESCRIPTION_1", isLocID: true);
		obj.SetNumberOfButtons(2);
		obj.SetOnCancelAction(m_cancelConfirmMenuSelection);
		ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
		buttonAtIndex.SetButtonText("LOC_ID_GENERAL_UI_YES_1", isLocID: true);
		buttonAtIndex.SetOnClickAction(m_confirmHeirSelection);
		ConfirmMenu_Button buttonAtIndex2 = obj.GetButtonAtIndex(1);
		buttonAtIndex2.SetButtonText("LOC_ID_GENERAL_UI_NO_1", isLocID: true);
		buttonAtIndex2.SetOnClickAction(m_cancelConfirmMenuSelection);
	}

	private void InitializeExitConfirmMenu()
	{
		ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenu) as ConfirmMenuWindowController;
		obj.SetTitleText("LOC_ID_LINEAGE_QUIT_MENU_TITLE_1", isLocID: true);
		obj.SetDescriptionText("LOC_ID_LINEAGE_QUIT_MENU_DESCRIPTION_1", isLocID: true);
		obj.SetNumberOfButtons(2);
		obj.SetOnCancelAction(m_cancelConfirmMenuSelection);
		ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
		buttonAtIndex.SetButtonText("LOC_ID_GENERAL_UI_YES_1", isLocID: true);
		buttonAtIndex.SetOnClickAction(m_confirmQuitSelection);
		ConfirmMenu_Button buttonAtIndex2 = obj.GetButtonAtIndex(1);
		buttonAtIndex2.SetButtonText("LOC_ID_GENERAL_UI_NO_1", isLocID: true);
		buttonAtIndex2.SetOnClickAction(m_cancelConfirmMenuSelection);
	}

	private void ConfirmHeirSelection()
	{
		AudioManager.PlayOneShot(this, m_transitionToTownAudioEvent);
		CharacterLoadedFromLineage = true;
		CharacterData characterData = m_characterDataArray[m_selectedCharacterIndex];
		characterData.CopyEquipment(SaveManager.PlayerSaveData.CurrentCharacter);
		SaveManager.PlayerSaveData.CurrentCharacter = characterData;
		SaveManager.PlayerSaveData.IsDead = false;
		SaveManager.PlayerSaveData.LineageSeed = -1;
		SaveManager.PlayerSaveData.TimesRolledLineage = 0;
		NPCDialogueManager.PopulateNPCDialogues(inHubtown: true);
		TraitManager.SetTraitSeenState(characterData.TraitOne, TraitSeenState.SeenTwice, forceState: false);
		TraitManager.SetTraitSeenState(characterData.TraitTwo, TraitSeenState.SeenTwice, forceState: false);
		SaveManager.PlayerSaveData.SetSpellSeenState(characterData.Spell, seenState: true);
		if (characterData.ClassType == ClassType.MagicWandClass && AbilityType_RL.IsMageSpellOrTalent(characterData.Talent))
		{
			SaveManager.PlayerSaveData.SetSpellSeenState(characterData.Talent, seenState: true);
		}
		if (SaveManager.EquipmentSaveData.EquipmentLoadoutEnabled)
		{
			SaveManager.EquipmentSaveData.GetEquipmentLoadout(SaveManager.PlayerSaveData.CurrentCharacter.ClassType)?.LoadLoadout(SaveManager.PlayerSaveData.CurrentCharacter);
			EquipmentLoadout.VerifyLoadoutWeight();
		}
		if (SaveManager.EquipmentSaveData.RuneLoadoutEnabled)
		{
			SaveManager.EquipmentSaveData.GetRuneLoadout(SaveManager.PlayerSaveData.CurrentCharacter.ClassType)?.LoadLoadout();
		}
		SaveManager.SaveCurrentProfileGameData(SaveDataType.Player, SavingType.FileOnly, ignoreTimeRestriction: true);
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.Lineage_ConfirmHeir, this, EventArgs.Empty);
		if (SaveManager.PlayerSaveData.EndingSpawnRoom >= EndingSpawnRoomType.Hallway)
		{
			WorldBuilder.FirstBiomeOverride = BiomeType.Garden;
			SceneLoader_RL.LoadScene(SceneID.World, TransitionID.FadeToBlackWithLoading);
		}
		else
		{
			WorldBuilder.FirstBiomeOverride = BiomeType.None;
			SceneLoader_RL.LoadScene(SceneID.Town, TransitionID.FadeToBlackWithLoading);
		}
	}

	private void CancelConfirmMenuSelection()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
	}

	private void ConfirmQuitSelection()
	{
		SceneLoader_RL.LoadScene(SceneID.MainMenu, TransitionID.FadeToBlackWithLoading);
	}

	private void OnViewLineageButtonPressed(InputActionEventData data)
	{
		TogglePortraitViewing();
	}

	public void TogglePortraitViewing()
	{
		StartCoroutine(TogglePortraitAnimCoroutine(!m_isViewingPortraits));
	}

	private IEnumerator TogglePortraitAnimCoroutine(bool toggleIn)
	{
		m_isViewingPortraits = toggleIn;
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		if (toggleIn)
		{
			TweenManager.TweenTo(m_bannerAndDescripCanvasGroup, 0.25f, Ease.None, "alpha", 0);
			m_lineageRoomController.EnableSpotlight(turnOn: false);
			m_playerModels[CurrentSelectedCharacterIndex].Animator.SetBool("Victory", value: false);
			m_currentPortraitRoomIndex = LineageRoomController.NumPortraits - 1;
			LineagePortrait portraitAtIndex = m_lineageRoomController.GetPortraitAtIndex(m_currentPortraitRoomIndex);
			LineageRoomController.ResetRoomIndex(m_lineageRoomController, instantiatingFromLineageWindow: true);
			AudioManager.PlayOneShot(this, m_viewPortraitsAudioEvent);
			yield return TweenManager.TweenTo(Camera.main.transform, 0.5f, Ease.Quad.EaseInOut, "position.x", portraitAtIndex.transform.position.x + m_portraitXOffset).TweenCoroutine;
		}
		else
		{
			AudioManager.PlayOneShot(this, m_portraitEndAudioEvent);
			float duration = Mathf.Clamp((float)(LineageRoomController.NumPortraits - m_currentPortraitRoomIndex) * 0.05f, 0.5f, float.MaxValue);
			yield return TweenManager.TweenTo(Camera.main.transform, duration, Ease.Quad.EaseInOut, "position.x", m_lineageRoomController.EndingRoom.transform.position.x).TweenCoroutine;
			TweenManager.TweenTo(m_bannerAndDescripCanvasGroup, 0.25f, Ease.None, "alpha", 1);
			m_lineageRoomController.EnableSpotlight(turnOn: true);
			m_playerModels[CurrentSelectedCharacterIndex].Animator.SetBool("Victory", value: true);
		}
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
	}

	private void ShiftPortraits(bool moveRight)
	{
		int currentPortraitRoomIndex = m_currentPortraitRoomIndex;
		if (moveRight)
		{
			m_currentPortraitRoomIndex++;
		}
		else
		{
			m_currentPortraitRoomIndex--;
		}
		if (m_currentPortraitRoomIndex > LineageRoomController.NumPortraits - 1)
		{
			m_currentPortraitRoomIndex = LineageRoomController.NumPortraits - 1;
		}
		if (m_currentPortraitRoomIndex < 0)
		{
			m_currentPortraitRoomIndex = 0;
		}
		if (currentPortraitRoomIndex != m_currentPortraitRoomIndex)
		{
			LineagePortrait portraitAtIndex = m_lineageRoomController.GetPortraitAtIndex(m_currentPortraitRoomIndex);
			if ((bool)portraitAtIndex)
			{
				StartCoroutine(ShiftToPortraitCoroutine(portraitAtIndex));
			}
			else
			{
				Debug.LogWarning("<color=yellow>Could not navigate to portrait at index: " + m_currentPortraitRoomIndex + ". Portrait could not be found.</color>");
			}
		}
	}

	private IEnumerator ShiftToPortraitCoroutine(LineagePortrait portrait)
	{
		m_isShiftingPortraits = true;
		int num = 5;
		float num2 = (float)m_portraitsViewed / (float)num;
		float duration = Mathf.Clamp(0.5f - 0.4f * num2, 0.1f, 0.5f);
		m_portraitsViewed = Mathf.Clamp(m_portraitsViewed + 1, 0, num);
		AudioManager.PlayOneShot(this, m_scrollPortraitAudioEvent);
		yield return TweenManager.TweenTo(Camera.main.transform, duration, Ease.Quad.EaseInOut, "position.x", portrait.transform.position.x + m_portraitXOffset).TweenCoroutine;
		m_isShiftingPortraits = false;
	}

	private void Update()
	{
		if ((bool)m_triggerObject.gameObject)
		{
			Camera camera = ((!CameraController.IsInstantiated) ? Camera.main : CameraController.GameCamera);
			m_triggerObject.gameObject.transform.position = camera.transform.position;
		}
	}

	private Vector2Int GetRoomIndexByCamPosition()
	{
		float num = Mathf.Abs(Camera.main.gameObject.transform.position.x);
		float num2 = 32f;
		num -= num2;
		if (num < 0f)
		{
			num = 0f;
		}
		int x = (int)(num / num2);
		int y = Mathf.CeilToInt(num / num2);
		return new Vector2Int(x, y);
	}

	private void UpdateSelectedCharacter()
	{
		bool classLocked = m_selectedCharacterIndex == m_characterDataArray.Length - 1 && SaveManager.ModeSaveData.GetSoulShopObj(SoulShopType.ChooseYourClass).CurrentEquippedLevel > 0;
		bool spellLocked = m_selectedCharacterIndex == m_characterDataArray.Length - 1 && SaveManager.ModeSaveData.GetSoulShopObj(SoulShopType.ChooseYourSpell).CurrentEquippedLevel > 0;
		m_lineageEventArgs.Initialize(m_characterDataArray[m_selectedCharacterIndex], classLocked, spellLocked);
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.Lineage_SelectedNewHeir, this, m_lineageEventArgs);
		for (int i = 0; i < NumberOfSuccessors; i++)
		{
			if (i == m_selectedCharacterIndex)
			{
				m_playerModels[i].Animator.SetBool("Victory", value: true);
			}
			else
			{
				m_playerModels[i].Animator.SetBool("Victory", value: false);
			}
		}
		if (IsVariantAbilityUnlocked(m_characterDataArray[m_selectedCharacterIndex]))
		{
			if (!m_pressUpGO.activeSelf)
			{
				m_pressUpGO.SetActive(value: true);
			}
			m_pressUpGO.transform.SetParent(m_playerModels[m_selectedCharacterIndex].transform, worldPositionStays: false);
			float num = 1f / m_playerModels[m_selectedCharacterIndex].transform.lossyScale.x;
			m_pressUpGO.transform.localScale = new Vector3(num, num, num);
			m_pressUpGO.transform.localPosition = new Vector3(0f, 2.75f, 1f);
			if (!m_upArrowAnimating)
			{
				m_upArrowAnimating = true;
				StartCoroutine(AnimateUpArrowCoroutine());
			}
		}
		else if (m_pressUpGO.activeSelf)
		{
			m_pressUpGO.SetActive(value: false);
		}
	}

	private IEnumerator AnimateUpArrowCoroutine()
	{
		float arrowYPos = m_pressUpGO.transform.localPosition.y;
		while (true)
		{
			m_pressUpGO.transform.SetLocalPositionY(arrowYPos + 0.05f);
			yield return TweenManager.TweenTo_UnscaledTime(m_pressUpGO.transform, 0.5f, Ease.Back.EaseOut, "localPosition.y", arrowYPos).TweenCoroutine;
		}
	}

	public void RefreshText(object sender, EventArgs args)
	{
		m_rerollHeirsText.text = string.Format(LocalizationManager.GetString("LOC_ID_LINEAGE_REROLL_HEIRS_1", isFemale: false), m_numRerolls);
	}
}
