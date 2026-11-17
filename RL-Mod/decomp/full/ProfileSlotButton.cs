using System;
using RL_Windows;
using SceneManagement_RL;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ProfileSlotButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, ISelectHandler, IDeselectHandler, IPointerClickHandler, ILocalizable
{
	[SerializeField]
	private TMP_Text m_slotTitleText;

	[SerializeField]
	private TMP_Text m_slotInfoText;

	[SerializeField]
	private TMP_Text m_slotNameText;

	[SerializeField]
	private GameObject m_deleteProfileGO;

	[SerializeField]
	protected GameObject m_selectedIndicator;

	private PlayerSaveData m_loadedPlayerData;

	private ProfileSelectWindowController m_profileSelectWindowController;

	private PlayerCardOpenedEventArgs m_playerCardEventArgs;

	private Action<MonoBehaviour, EventArgs> m_refreshText;

	public ProfileSlotSelectedHandler ProfileSlotSelected { get; set; }

	public bool Interactable { get; set; }

	public byte SlotNumber { get; private set; }

	private void Awake()
	{
		m_refreshText = RefreshText;
	}

	public void Initialize(byte slotNumber, ProfileSelectWindowController windowController)
	{
		m_selectedIndicator.SetActive(value: false);
		m_deleteProfileGO.SetActive(value: false);
		SlotNumber = slotNumber;
		m_slotTitleText.text = string.Format(LocalizationManager.GetString("LOC_ID_PROFILE_SELECT_SLOT_X_1", isFemale: false, disableWordScramble: true), SlotNumber + 1) + " - ";
		m_profileSelectWindowController = windowController;
		m_playerCardEventArgs = new PlayerCardOpenedEventArgs(null);
		UpdateProfileSlotSaveData();
	}

	public void UpdateProfileSlotSaveData()
	{
		PlayerSaveData playerSaveData = null;
		m_slotInfoText.gameObject.SetActive(value: true);
		if (SaveManager.DoesSaveExist(SlotNumber, SaveDataType.Player, checkForBackup: false))
		{
			object loadDataOutput = null;
			SaveManager.LoadGameData(SlotNumber, SaveDataType.Player, out loadDataOutput);
			playerSaveData = loadDataOutput as PlayerSaveData;
		}
		else
		{
			playerSaveData = new PlayerSaveData();
		}
		if (playerSaveData == null)
		{
			playerSaveData = new PlayerSaveData();
		}
		uint secondsPlayed = playerSaveData.SecondsPlayed;
		int num = (int)(secondsPlayed / 3600);
		int num2 = (int)(secondsPlayed % 3600 / 60);
		secondsPlayed %= 60;
		m_slotInfoText.text = $"{num}:{num2:D2}:{secondsPlayed:D2}";
		m_loadedPlayerData = playerSaveData;
		RefreshText(null, null);
	}

	public virtual void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			ExecuteButton();
		}
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		OnSelect(eventData);
	}

	public virtual void OnSelect(BaseEventData eventData)
	{
		m_selectedIndicator.SetActive(value: true);
		if (m_loadedPlayerData != null && m_loadedPlayerData.HasStartedGame)
		{
			m_deleteProfileGO.SetActive(value: true);
			m_profileSelectWindowController.SetPlayerCardActive(active: true);
			m_profileSelectWindowController.SetPlayerCardCharData(m_loadedPlayerData.CurrentCharacter);
			m_playerCardEventArgs.Initialize(m_loadedPlayerData);
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.PlayerCardWindow_Opened, this, m_playerCardEventArgs);
		}
		else
		{
			m_deleteProfileGO.SetActive(value: false);
			m_profileSelectWindowController.SetPlayerCardActive(active: false);
		}
		if (ProfileSlotSelected != null)
		{
			ProfileSlotSelected(this);
		}
	}

	public virtual void OnDeselect(BaseEventData eventData)
	{
		m_selectedIndicator.SetActive(value: false);
		m_deleteProfileGO.SetActive(value: false);
	}

	public void ExecuteButton()
	{
		if (!Interactable)
		{
			return;
		}
		SaveManager.ConfigData.CurrentProfile = SlotNumber;
		SaveManager.SaveConfigFile();
		if (SaveManager.DoesSaveExist(SlotNumber, SaveDataType.Player, checkForBackup: false))
		{
			switch (SaveManager.LoadAllGameData(SlotNumber))
			{
			case LOAD_RESULT.FAILED_STAGE:
				if (SaveManager.PlayerSaveData.InCastle)
				{
					SaveManager.StageSaveData = new StageSaveData();
					SaveManager.StageSaveData.ForceResetWorld = true;
				}
				break;
			default:
				SaveManager.CreateNewSaveData();
				SaveManager.LoadingFailed = true;
				break;
			case LOAD_RESULT.OK:
				break;
			}
			SaveManager.LoadProfileConfigFile();
			if (SaveManager.PlayerSaveData.HighestNGPlusBeaten != m_profileSelectWindowController.OnEnter_PreviousHighestNGPlusBeaten)
			{
				m_profileSelectWindowController.SetAllSlotsInteractable(interactable: false);
				SceneLoader_RL.RunTransitionWithLogic(CloseProfileSelectWindow, TransitionID.QuickSwipe, cleanup: false);
			}
			else
			{
				CloseProfileSelectWindow();
			}
		}
		else
		{
			CreateNewProfile();
		}
	}

	private void CloseProfileSelectWindow()
	{
		WindowManager.SetWindowIsOpen(WindowID.ProfileSelect, isOpen: false);
		WindowManager.SetWindowIsOpen(WindowID.MainMenu, isOpen: true);
		m_profileSelectWindowController.SetAllSlotsInteractable(interactable: true);
	}

	private void CreateNewProfile()
	{
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		SaveManager.CreateNewSaveData();
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
		if (SaveManager.PlayerSaveData.HighestNGPlusBeaten != m_profileSelectWindowController.OnEnter_PreviousHighestNGPlusBeaten)
		{
			m_profileSelectWindowController.SetAllSlotsInteractable(interactable: false);
			SceneLoader_RL.RunTransitionWithLogic(CloseProfileSelectWindow, TransitionID.QuickSwipe, cleanup: false);
		}
		else
		{
			CloseProfileSelectWindow();
		}
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
		m_slotTitleText.text = string.Format(LocalizationManager.GetString("LOC_ID_PROFILE_SELECT_SLOT_X_1", isFemale: false, disableWordScramble: true), SlotNumber + 1) + " - ";
		if (!m_loadedPlayerData.HasStartedGame)
		{
			m_slotInfoText.gameObject.SetActive(value: false);
			m_slotNameText.text = LocalizationManager.GetString("LOC_ID_PROFILE_SELECT_NEW_LEGACY_1", isFemale: false, disableWordScramble: true);
		}
		else
		{
			m_slotNameText.text = LocalizationManager.GetLocalizedPlayerName(m_loadedPlayerData.CurrentCharacter);
		}
	}
}
