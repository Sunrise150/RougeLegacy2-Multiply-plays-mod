using System;
using System.Collections.Generic;
using RL_Windows;
using Rewired;
using UnityEngine;

public class BackupWindowController : WindowController
{
	[SerializeField]
	private BackupSlotEntry m_backupSlotPrefab;

	[SerializeField]
	private Transform m_contentParentTransform;

	private BackupSlotEntry[] m_backupSlotArray;

	private int m_currentSelectedIndex;

	private Action<InputActionEventData> m_onConfirmInputHandler;

	private Action<InputActionEventData> m_onVerticalInputHandler;

	public override WindowID ID => WindowID.Backup;

	private void Awake()
	{
		m_onConfirmInputHandler = OnConfirmInputHandler;
		m_onVerticalInputHandler = OnVerticalInputHandler;
	}

	public override void Initialize()
	{
		m_backupSlotArray = new BackupSlotEntry[6];
		for (int i = 0; i < m_backupSlotArray.Length - 1; i++)
		{
			BackupSlotEntry backupSlotEntry = UnityEngine.Object.Instantiate(m_backupSlotPrefab, m_contentParentTransform);
			backupSlotEntry.name = "Backup Slot Entry " + i;
			backupSlotEntry.BackupSlotSelected = (BackupSlotSelectedHandler)Delegate.Combine(backupSlotEntry.BackupSlotSelected, new BackupSlotSelectedHandler(UpdateSelectedBackupEntry));
			backupSlotEntry.Initialize(i, null, null, null, null, null);
			backupSlotEntry.gameObject.SetActive(value: false);
			m_backupSlotArray[i] = backupSlotEntry;
		}
		BackupSlotEntry backupSlotEntry2 = UnityEngine.Object.Instantiate(m_backupSlotPrefab, m_contentParentTransform);
		backupSlotEntry2.name = "Change Profile Slot";
		backupSlotEntry2.BackupSlotSelected = (BackupSlotSelectedHandler)Delegate.Combine(backupSlotEntry2.BackupSlotSelected, new BackupSlotSelectedHandler(UpdateSelectedBackupEntry));
		backupSlotEntry2.InitializeAsChangeProfileSlotOption(m_backupSlotArray.Length - 1);
		m_backupSlotArray[m_backupSlotArray.Length - 1] = backupSlotEntry2;
		base.Initialize();
	}

	private void InitializeBackupArray()
	{
		int num = 0;
		List<string> list = new List<string>(SaveManager.GetBackupPathes(SaveManager.CurrentProfile, SaveDataType.GameMode));
		num = list.Count;
		List<string> list2 = new List<string>(SaveManager.GetBackupPathes(SaveManager.CurrentProfile, SaveDataType.Player));
		if (list2.Count < num)
		{
			num = list2.Count;
		}
		List<string> list3 = new List<string>(SaveManager.GetBackupPathes(SaveManager.CurrentProfile, SaveDataType.Equipment));
		if (list3.Count < num)
		{
			num = list3.Count;
		}
		List<string> list4 = new List<string>(SaveManager.GetBackupPathes(SaveManager.CurrentProfile, SaveDataType.Lineage));
		if (list4.Count < num)
		{
			num = list4.Count;
		}
		List<string> list5 = new List<string>(SaveManager.GetBackupPathes(SaveManager.CurrentProfile, SaveDataType.Stage));
		if (list5.Count < num)
		{
			num = list5.Count;
		}
		for (int i = 0; i < m_backupSlotArray.Length - 1; i++)
		{
			BackupSlotEntry backupSlotEntry = m_backupSlotArray[i];
			if (!backupSlotEntry.IsChangeProfileOption)
			{
				if (i < num)
				{
					backupSlotEntry.gameObject.SetActive(value: true);
					backupSlotEntry.Initialize(i, list[i], list2[i], list3[i], list4[i], list5[i]);
				}
				else
				{
					backupSlotEntry.gameObject.SetActive(value: false);
				}
			}
		}
	}

	protected override void OnOpen()
	{
		InitializeBackupArray();
		int num = -1;
		for (int i = 0; i < m_backupSlotArray.Length; i++)
		{
			if (m_backupSlotArray[i].gameObject.activeSelf)
			{
				num = i;
				break;
			}
		}
		m_backupSlotArray[num].OnSelect(null);
		m_windowCanvas.gameObject.SetActive(value: true);
	}

	protected override void OnClose()
	{
		m_windowCanvas.gameObject.SetActive(value: false);
	}

	private void OnVerticalInputHandler(InputActionEventData eventData)
	{
		if (m_backupSlotArray.Length <= 1)
		{
			return;
		}
		int currentSelectedIndex = m_currentSelectedIndex;
		int num = m_currentSelectedIndex;
		float num2 = eventData.GetAxis();
		if (num2 == 0f)
		{
			num2 = 0f - eventData.GetAxisPrev();
		}
		if (num2 > 0f)
		{
			for (int i = 0; i < m_backupSlotArray.Length; i++)
			{
				num--;
				if (num < 0)
				{
					num = m_backupSlotArray.Length - 1;
				}
				if (m_backupSlotArray[num].gameObject.activeSelf)
				{
					break;
				}
			}
		}
		else
		{
			for (int j = 0; j < m_backupSlotArray.Length; j++)
			{
				num++;
				if (num >= m_backupSlotArray.Length)
				{
					num = 0;
				}
				if (m_backupSlotArray[num].gameObject.activeSelf)
				{
					break;
				}
			}
		}
		if (currentSelectedIndex != num)
		{
			m_backupSlotArray[num].OnSelect(null);
		}
	}

	private void OnConfirmInputHandler(InputActionEventData eventData)
	{
		if (!eventData.IsCurrentInputSource(ControllerType.Mouse) && m_backupSlotArray.Length != 0)
		{
			m_backupSlotArray[m_currentSelectedIndex].ExecuteButton();
		}
	}

	protected virtual void UpdateSelectedBackupEntry(BackupSlotEntry backupSlot)
	{
		if (backupSlot.Index != m_currentSelectedIndex)
		{
			if (m_backupSlotArray[m_currentSelectedIndex] != null)
			{
				m_backupSlotArray[m_currentSelectedIndex].OnDeselect(null);
			}
			m_currentSelectedIndex = backupSlot.Index;
		}
	}

	protected override void OnFocus()
	{
		AddInputListeners();
	}

	protected override void OnLostFocus()
	{
		RemoveInputListeners();
	}

	private void AddInputListeners()
	{
		base.RewiredPlayer.AddInputEventDelegate(m_onConfirmInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
		base.RewiredPlayer.AddInputEventDelegate(m_onVerticalInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonRepeating, "Window_Vertical");
		base.RewiredPlayer.AddInputEventDelegate(m_onVerticalInputHandler, UpdateLoopType.Update, InputActionEventType.NegativeButtonRepeating, "Window_Vertical");
	}

	private void RemoveInputListeners()
	{
		base.RewiredPlayer.RemoveInputEventDelegate(m_onConfirmInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
		base.RewiredPlayer.RemoveInputEventDelegate(m_onVerticalInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonRepeating, "Window_Vertical");
		base.RewiredPlayer.RemoveInputEventDelegate(m_onVerticalInputHandler, UpdateLoopType.Update, InputActionEventType.NegativeButtonRepeating, "Window_Vertical");
	}
}
