using System;
using RL_Windows;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class BackupSlotEntry : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, ISelectHandler, IDeselectHandler, IPointerClickHandler
{
	[SerializeField]
	private TMP_Text m_text;

	[SerializeField]
	private GameObject m_selectedBG;

	private string m_playerFilePath;

	private string m_equipmentFilePath;

	private string m_lineageFilePath;

	private string m_stageFilePath;

	private string m_gameModeFilePath;

	private Action m_cancelConfirmMenuSelection;

	private Action m_loadBackup;

	private Action m_returnToMainMenu;

	public bool IsChangeProfileOption { get; private set; }

	public BackupSlotSelectedHandler BackupSlotSelected { get; set; }

	public int Index { get; private set; }

	public void InitializeAsChangeProfileSlotOption(int index)
	{
		Index = index;
		m_text.text = LocalizationManager.GetString("LOC_ID_BACKUP_MENU_CHANGE_SLOT_1", isFemale: false);
		IsChangeProfileOption = true;
		m_selectedBG.SetActive(value: false);
	}

	private void Awake()
	{
		m_cancelConfirmMenuSelection = CancelConfirmMenuSelection;
		m_loadBackup = LoadBackup;
		m_returnToMainMenu = ReturnToMainMenu;
	}

	public void Initialize(int index, string gameModeFilePath, string playerFilePath, string equipmentFilePath, string lineageFilePath, string stageFilePath)
	{
		Index = index;
		m_playerFilePath = playerFilePath;
		m_equipmentFilePath = equipmentFilePath;
		m_lineageFilePath = lineageFilePath;
		m_stageFilePath = stageFilePath;
		m_gameModeFilePath = gameModeFilePath;
		if (!string.IsNullOrEmpty(playerFilePath))
		{
			DateTime dateTime = SaveManager.PathToDateTime(playerFilePath);
			m_text.text = string.Format(LocalizationManager.GetString("LOC_ID_BACKUP_MENU_BACKUP_SAVE_TITLE_1", isFemale: false), Index + 1, dateTime.Date.ToShortDateString(), dateTime.ToLongTimeString());
		}
		else
		{
			m_text.text = LocalizationManager.GetString("LOC_ID_BACKUP_MENU_EMPTY_SLOT_1", isFemale: false);
		}
		IsChangeProfileOption = false;
		m_selectedBG.SetActive(value: false);
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
		if (BackupSlotSelected != null)
		{
			BackupSlotSelected(this);
		}
		m_selectedBG.SetActive(value: true);
	}

	public virtual void OnDeselect(BaseEventData eventData)
	{
		m_selectedBG.SetActive(value: false);
	}

	public void ExecuteButton()
	{
		if (!IsChangeProfileOption)
		{
			InitializeLoadBackupConfirmMenu();
			WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: true);
		}
		else
		{
			ChangeProfileSlot();
		}
	}

	private void InitializeLoadBackupConfirmMenu()
	{
		ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenu) as ConfirmMenuWindowController;
		obj.SetTitleText("LOC_ID_BACKUP_MENU_CONFIRM_LOAD_BACKUP_TITLE_1", isLocID: true);
		obj.SetDescriptionText(string.Format(LocalizationManager.GetString("LOC_ID_BACKUP_MENU_CONFIRM_LOAD_BACKUP_DESCRIPTION_1", isFemale: false), Index + 1), isLocID: true);
		obj.SetNumberOfButtons(2);
		obj.SetOnCancelAction(m_cancelConfirmMenuSelection);
		obj.SetStartingSelectedButton(1);
		ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
		buttonAtIndex.SetButtonText("LOC_ID_GENERAL_UI_YES_1", isLocID: true);
		buttonAtIndex.SetOnClickAction(m_loadBackup);
		ConfirmMenu_Button buttonAtIndex2 = obj.GetButtonAtIndex(1);
		buttonAtIndex2.SetButtonText("LOC_ID_GENERAL_UI_NO_1", isLocID: true);
		buttonAtIndex2.SetOnClickAction(m_cancelConfirmMenuSelection);
	}

	private void ChangeProfileSlot()
	{
		SaveManager.LoadingFailed = false;
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
		WindowManager.SetWindowIsOpen(WindowID.MainMenu, isOpen: false);
		WindowManager.SetWindowIsOpen(WindowID.Backup, isOpen: false);
		WindowManager.SetWindowIsOpen(WindowID.ProfileSelect, isOpen: true);
	}

	private void StartNewGame()
	{
		SaveFileSystem.SaveBatch saveBatch = SaveFileSystem.BeginSaveBatch(SaveManager.CurrentProfile);
		SaveManager.DeleteSaveFile(saveBatch, SaveManager.CurrentProfile, SaveDataType.Player);
		SaveManager.DeleteSaveFile(saveBatch, SaveManager.CurrentProfile, SaveDataType.Equipment);
		SaveManager.DeleteSaveFile(saveBatch, SaveManager.CurrentProfile, SaveDataType.Lineage);
		SaveManager.DeleteSaveFile(saveBatch, SaveManager.CurrentProfile, SaveDataType.Stage);
		SaveManager.DeleteSaveFile(saveBatch, SaveManager.CurrentProfile, SaveDataType.GameMode);
		saveBatch.End();
		SaveManager.CreateNewSaveData();
		(WindowManager.GetWindowController(WindowID.ProfileSelect) as ProfileSelectWindowController).ForceUpdateProfileSlot(SaveManager.CurrentProfile);
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
		WindowManager.SetWindowIsOpen(WindowID.Backup, isOpen: false);
	}

	private void LoadBackup()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
		object loadDataOutput = null;
		LOAD_RESULT lOAD_RESULT = SaveManager.LoadGameDataByFilePath(m_gameModeFilePath, out loadDataOutput);
		if (lOAD_RESULT != 0)
		{
			Debug.Log("Failed to load backup gamemode save file.  Backup has not been loaded. RESULT: " + lOAD_RESULT);
			LoadFailed();
			return;
		}
		object loadDataOutput2 = null;
		lOAD_RESULT = SaveManager.LoadGameDataByFilePath(m_playerFilePath, out loadDataOutput2);
		if (lOAD_RESULT != 0)
		{
			Debug.Log("Failed to load backup player save file.  Backup has not been loaded. RESULT: " + lOAD_RESULT);
			LoadFailed();
			return;
		}
		object loadDataOutput3 = null;
		lOAD_RESULT = SaveManager.LoadGameDataByFilePath(m_equipmentFilePath, out loadDataOutput3);
		if (lOAD_RESULT != 0)
		{
			Debug.Log("Failed to load backup equipment save file.  Backup has not been loaded. RESULT: " + lOAD_RESULT);
			LoadFailed();
			return;
		}
		object loadDataOutput4 = null;
		lOAD_RESULT = SaveManager.LoadGameDataByFilePath(m_lineageFilePath, out loadDataOutput4);
		if (lOAD_RESULT != 0)
		{
			Debug.Log("Failed to load backup lineage save file.  Backup has not been loaded. RESULT: " + lOAD_RESULT);
			LoadFailed();
			return;
		}
		object loadDataOutput5 = null;
		lOAD_RESULT = SaveManager.LoadGameDataByFilePath(m_stageFilePath, out loadDataOutput5);
		if (lOAD_RESULT != 0)
		{
			Debug.Log("Failed to load backup stage save file.  Backup has not been loaded. RESULT: " + lOAD_RESULT);
			LoadFailed();
			return;
		}
		try
		{
			SaveManager.ModeSaveData = loadDataOutput as ModeSaveData;
			SaveManager.PlayerSaveData = loadDataOutput2 as PlayerSaveData;
			SaveManager.EquipmentSaveData = loadDataOutput3 as EquipmentSaveData;
			SaveManager.LineageSaveData = loadDataOutput4 as LineageSaveData;
			SaveManager.StageSaveData = loadDataOutput5 as StageSaveData;
			SaveManager.ModeSaveData.UpdateVersion();
			SaveManager.PlayerSaveData.UpdateVersion();
			SaveManager.EquipmentSaveData.UpdateVersion();
			SaveManager.LineageSaveData.UpdateVersion();
			SaveManager.StageSaveData.UpdateVersion();
		}
		catch (Exception ex)
		{
			Debug.Log("Failed to call UpdateVersion() on backup file. Exception: " + ex.Message);
			LoadFailed();
			return;
		}
		SaveManager.LoadingFailed = false;
		SaveManager.IsCopyingBackupFiles = true;
		SaveManager.SaveAllCurrentProfileGameData(SavingType.FileOnly, saveStageData: true, ignoreTimeRestriction: true);
		SaveManager.IsCopyingBackupFiles = false;
		Debug.Log("Successfully loaded backup save file.");
		LoadSuccessful();
	}

	private void LoadFailed()
	{
		InitializeFailedlLoadConfirmMenu();
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: true);
	}

	private void LoadSuccessful()
	{
		InitializeSuccessfulLoadConfirmMenu();
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: true);
	}

	private void CancelConfirmMenuSelection()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
	}

	private void InitializeSuccessfulLoadConfirmMenu()
	{
		ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenu) as ConfirmMenuWindowController;
		obj.SetTitleText("LOC_ID_BACKUP_MENU_LOAD_BACKUP_SUCCESS_TITLE_1", isLocID: true);
		obj.SetDescriptionText("LOC_ID_BACKUP_MENU_LOAD_BACKUP_SUCCESS_DESCRIPTION_1", isLocID: true);
		obj.SetNumberOfButtons(1);
		obj.SetOnCancelAction(m_returnToMainMenu);
		ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
		buttonAtIndex.SetButtonText("LOC_ID_GENERAL_UI_CONTINUE_1", isLocID: true);
		buttonAtIndex.SetOnClickAction(m_returnToMainMenu);
	}

	private void ReturnToMainMenu()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
		WindowManager.SetWindowIsOpen(WindowID.Backup, isOpen: false);
	}

	private void InitializeFailedlLoadConfirmMenu()
	{
		ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenu) as ConfirmMenuWindowController;
		obj.SetTitleText("LOC_ID_BACKUP_MENU_LOAD_BACKUP_FAILED_TITLE_1", isLocID: true);
		obj.SetDescriptionText("LOC_ID_BACKUP_MENU_LOAD_BACKUP_FAILED_DESCRIPTION_1", isLocID: true);
		obj.SetNumberOfButtons(1);
		obj.SetOnCancelAction(m_cancelConfirmMenuSelection);
		ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
		buttonAtIndex.SetButtonText("LOC_ID_GENERAL_UI_CONTINUE_1", isLocID: true);
		buttonAtIndex.SetOnClickAction(m_cancelConfirmMenuSelection);
	}
}
