using System;
using RL_Windows;
using Rewired;
using UnityEngine;
using UnityEngine.Events;

public class ProfileSelectWindowController : WindowController
{
	[SerializeField]
	private ProfileSlotButton m_profileSlotPrefab;

	[SerializeField]
	private CanvasGroup m_profileSlotCanvasGroup;

	[SerializeField]
	private CanvasGroup m_fadeBGCanvasGroup;

	[SerializeField]
	private GameObject m_playerCard;

	[SerializeField]
	private CanvasGroup m_playerCardCanvasGroup;

	[SerializeField]
	private Animator m_playerCardAnimator;

	[SerializeField]
	private PlayerLookController m_playerCardLookController;

	[SerializeField]
	private UnityEvent m_selectionChangeEvent;

	[SerializeField]
	private UnityEvent m_selectEvent;

	private float m_storedPlayerCardYPos;

	private int m_currentSelectedIndex;

	private ProfileSlotButton[] m_profileSlotArray;

	private PlayerCardOpenedEventArgs m_playerCardEventArgs;

	private Action m_cancelConfirmMenuSelection;

	private Action m_confirmDeleteProfile;

	private Action<InputActionEventData> m_onConfirmInputHandler;

	private Action<InputActionEventData> m_onDeleteProfileInputHandler;

	private Action<InputActionEventData> m_onVerticalInputHandler;

	private Action<InputActionEventData> m_onCancelButtonDown;

	public int OnEnter_PreviousHighestNGPlusBeaten { get; private set; } = -1;


	public override WindowID ID => WindowID.ProfileSelect;

	private void Awake()
	{
		m_cancelConfirmMenuSelection = CancelConfirmMenuSelection;
		m_confirmDeleteProfile = ConfirmDeleteProfile;
		m_onConfirmInputHandler = OnConfirmInputHandler;
		m_onDeleteProfileInputHandler = OnDeleteProfileInputHandler;
		m_onVerticalInputHandler = OnVerticalInputHandler;
		m_onCancelButtonDown = OnCancelButtonDown;
	}

	public override void Initialize()
	{
		base.Initialize();
		m_profileSlotArray = new ProfileSlotButton[5];
		for (int i = 0; i < 5; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_profileSlotPrefab.gameObject);
			gameObject.transform.SetParent(m_profileSlotCanvasGroup.transform);
			gameObject.transform.localScale = Vector3.one;
			gameObject.name = "Slot " + (i + 1);
			Vector3 localPosition = gameObject.transform.localPosition;
			localPosition.z = 0f;
			gameObject.transform.localPosition = localPosition;
			m_profileSlotArray[i] = gameObject.GetComponent<ProfileSlotButton>();
		}
		for (int j = 0; j < m_profileSlotArray.Length; j++)
		{
			ProfileSlotButton obj = m_profileSlotArray[j];
			obj.ProfileSlotSelected = (ProfileSlotSelectedHandler)Delegate.Combine(obj.ProfileSlotSelected, new ProfileSlotSelectedHandler(UpdateSelectedOptionItem));
			obj.Initialize((byte)j, this);
			obj.Interactable = true;
		}
		m_playerCardEventArgs = new PlayerCardOpenedEventArgs(null);
		m_storedPlayerCardYPos = m_playerCard.transform.position.y;
		m_playerCard.SetActive(value: false);
		if (m_playerCardLookController.VisualsGameObject != null)
		{
			m_playerCardLookController.VisualsGameObject.SetLayerRecursively(5, includeInactive: true);
		}
	}

	protected override void OnOpen()
	{
		m_windowCanvas.gameObject.SetActive(value: true);
		m_profileSlotArray[0].OnSelect(null);
		m_fadeBGCanvasGroup.alpha = 0f;
		m_profileSlotCanvasGroup.alpha = 0f;
		TweenManager.TweenTo_UnscaledTime(m_fadeBGCanvasGroup, 0.15f, Ease.None, "alpha", 1);
		TweenManager.TweenTo_UnscaledTime(m_profileSlotCanvasGroup, 0.15f, Ease.None, "alpha", 1);
		OnEnter_PreviousHighestNGPlusBeaten = SaveManager.PlayerSaveData.HighestNGPlusBeaten;
	}

	protected override void OnClose()
	{
		m_windowCanvas.gameObject.SetActive(value: false);
		PlaySelectedSFX(null);
	}

	protected override void OnFocus()
	{
		AddInputListeners();
	}

	protected override void OnLostFocus()
	{
		RemoveInputListeners();
	}

	protected void AddInputListeners()
	{
		if (ReInput.isReady && base.RewiredPlayer != null)
		{
			base.RewiredPlayer.AddInputEventDelegate(m_onConfirmInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
			base.RewiredPlayer.AddInputEventDelegate(m_onDeleteProfileInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Y");
			base.RewiredPlayer.AddInputEventDelegate(m_onVerticalInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonRepeating, "Window_Vertical");
			base.RewiredPlayer.AddInputEventDelegate(m_onVerticalInputHandler, UpdateLoopType.Update, InputActionEventType.NegativeButtonRepeating, "Window_Vertical");
			base.RewiredPlayer.AddInputEventDelegate(m_onCancelButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
		}
	}

	protected void RemoveInputListeners()
	{
		if (ReInput.isReady)
		{
			base.RewiredPlayer.RemoveInputEventDelegate(m_onConfirmInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
			base.RewiredPlayer.RemoveInputEventDelegate(m_onDeleteProfileInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Y");
			base.RewiredPlayer.RemoveInputEventDelegate(m_onVerticalInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonRepeating, "Window_Vertical");
			base.RewiredPlayer.RemoveInputEventDelegate(m_onVerticalInputHandler, UpdateLoopType.Update, InputActionEventType.NegativeButtonRepeating, "Window_Vertical");
			base.RewiredPlayer.RemoveInputEventDelegate(m_onCancelButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
		}
	}

	private void OnVerticalInputHandler(InputActionEventData eventData)
	{
		int currentSelectedIndex = m_currentSelectedIndex;
		int num = -1;
		float num2 = eventData.GetAxis();
		if (num2 == 0f)
		{
			num2 = 0f - eventData.GetAxisPrev();
		}
		num = ((!(num2 > 0f)) ? ((m_currentSelectedIndex + 1 < m_profileSlotArray.Length) ? (m_currentSelectedIndex + 1) : 0) : ((m_currentSelectedIndex - 1 < 0) ? (m_profileSlotArray.Length - 1) : (m_currentSelectedIndex - 1)));
		if (currentSelectedIndex != num)
		{
			m_profileSlotArray[num].OnSelect(null);
		}
	}

	private void OnConfirmInputHandler(InputActionEventData eventData)
	{
		if (!eventData.IsCurrentInputSource(ControllerType.Mouse) && m_profileSlotArray.Length != 0)
		{
			m_profileSlotArray[m_currentSelectedIndex].ExecuteButton();
			PlaySelectedSFX(null);
		}
	}

	protected virtual void UpdateSelectedOptionItem(ProfileSlotButton profileSlotItem)
	{
		if (profileSlotItem.SlotNumber != m_currentSelectedIndex)
		{
			if (m_profileSlotArray[m_currentSelectedIndex] != null)
			{
				m_profileSlotArray[m_currentSelectedIndex].OnDeselect(null);
			}
			m_currentSelectedIndex = profileSlotItem.SlotNumber;
			if (m_selectionChangeEvent != null)
			{
				m_selectionChangeEvent.Invoke();
			}
		}
	}

	protected virtual void OnCancelButtonDown(InputActionEventData eventData)
	{
		WindowManager.SetWindowIsOpen(ID, isOpen: false);
		WindowManager.SetWindowIsOpen(WindowID.MainMenu, isOpen: true);
	}

	protected virtual void OnDeleteProfileInputHandler(InputActionEventData eventData)
	{
		if (SaveManager.DoesSaveExist(m_profileSlotArray[m_currentSelectedIndex].SlotNumber, SaveDataType.Player, checkForBackup: false))
		{
			InitializeDeleteProfileConfirmMenu();
			WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: true);
		}
	}

	public void SetPlayerCardActive(bool active)
	{
		m_playerCard.SetActive(active);
		if (active)
		{
			m_playerCardAnimator.SetBool("Victory", value: true);
			m_playerCardAnimator.Play("Victory", 0, 1f);
			m_playerCardAnimator.Update(1f);
			m_playerCardAnimator.Update(1f);
			m_playerCardCanvasGroup.alpha = 0f;
			Vector3 position = m_playerCard.transform.position;
			position.y = m_storedPlayerCardYPos + 1f;
			m_playerCard.transform.position = position;
			TweenManager.StopAllTweensContaining(m_playerCardCanvasGroup, runEndHandlers: false);
			TweenManager.StopAllTweensContaining(m_playerCard.transform, runEndHandlers: false);
			TweenManager.TweenTo(m_playerCardCanvasGroup, 0.1f, Ease.None, "alpha", 1);
			TweenManager.TweenBy(m_playerCard.transform, 0.2f, Ease.Quad.EaseOut, "position.y", -1);
		}
	}

	public void SetPlayerCardCharData(CharacterData charData)
	{
		m_playerCardLookController.InitializeLook(charData);
	}

	private void InitializeDeleteProfileConfirmMenu()
	{
		ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenu) as ConfirmMenuWindowController;
		obj.SetTitleText("LOC_ID_PROFILE_SELECT_DELETE_PROFILE_TITLE_1", isLocID: true);
		obj.SetDescriptionText("LOC_ID_PROFILE_SELECT_DELETE_PROFILE_DESCRIPTION_1", isLocID: true);
		obj.SetNumberOfButtons(2);
		obj.SetOnCancelAction(m_cancelConfirmMenuSelection);
		obj.ApplyMATHack();
		ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
		buttonAtIndex.SetButtonText("LOC_ID_GENERAL_UI_YES_1", isLocID: true);
		buttonAtIndex.SetOnClickAction(m_confirmDeleteProfile);
		ConfirmMenu_Button buttonAtIndex2 = obj.GetButtonAtIndex(1);
		buttonAtIndex2.SetButtonText("LOC_ID_GENERAL_UI_NO_1", isLocID: true);
		buttonAtIndex2.SetOnClickAction(m_cancelConfirmMenuSelection);
		obj.SetStartingSelectedButton(1);
	}

	private void ConfirmDeleteProfile()
	{
		SaveFileSystem.SaveBatch saveBatch = SaveFileSystem.BeginSaveBatch(m_currentSelectedIndex);
		SaveManager.DeleteSaveFile(saveBatch, m_currentSelectedIndex, SaveDataType.Player);
		SaveManager.DeleteSaveFile(saveBatch, m_currentSelectedIndex, SaveDataType.Equipment);
		SaveManager.DeleteSaveFile(saveBatch, m_currentSelectedIndex, SaveDataType.Lineage);
		SaveManager.DeleteSaveFile(saveBatch, m_currentSelectedIndex, SaveDataType.Stage);
		SaveManager.DeleteSaveFile(saveBatch, m_currentSelectedIndex, SaveDataType.GameMode);
		saveBatch.End();
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
		m_profileSlotArray[m_currentSelectedIndex].UpdateProfileSlotSaveData();
		m_profileSlotArray[m_currentSelectedIndex].OnSelect(null);
	}

	private void CancelConfirmMenuSelection()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
	}

	public void ForceUpdateProfileSlot(int index)
	{
		if (index >= 0 && index < m_profileSlotArray.Length)
		{
			m_profileSlotArray[index].UpdateProfileSlotSaveData();
		}
	}

	protected virtual void PlaySelectedSFX(BaseOptionItem menuItem)
	{
		if (m_selectEvent != null)
		{
			m_selectEvent.Invoke();
		}
	}

	public void SetAllSlotsInteractable(bool interactable)
	{
		ProfileSlotButton[] profileSlotArray = m_profileSlotArray;
		for (int i = 0; i < profileSlotArray.Length; i++)
		{
			profileSlotArray[i].Interactable = interactable;
		}
	}
}
