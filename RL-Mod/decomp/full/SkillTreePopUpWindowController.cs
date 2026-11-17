using System;
using System.Collections;
using RLAudio;
using RL_Windows;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreePopUpWindowController : WindowController, IAudioEventEmitter
{
	[SerializeField]
	private Image m_ray;

	[SerializeField]
	private Image m_popupSprite;

	[SerializeField]
	private LocalizationItem m_titleTextLocItem;

	[SerializeField]
	private LocalizationItem m_subTitleTextLocItem;

	[SerializeField]
	private LocalizationItem m_descriptionTextLocItem;

	[SerializeField]
	private CanvasGroup m_popupCanvasGroup;

	[SerializeField]
	private CanvasGroup m_rayCanvasGroup;

	private string m_description = string.Empty;

	private bool m_displayedClassChange;

	private ClassType m_classToChangeTo;

	private Action m_cancelClassChange;

	private Action m_confirmClassChange;

	private Action<InputActionEventData> m_onConfirmButtonPressed;

	private const string GENERIC_OPEN_AUDIO_PATH = "event:/UI/FrontEnd/ui_fe_generic_popUp";

	private const string CLASS_UNLOCK_OPEN_AUDIO_PATH = "event:/UI/FrontEnd/ui_fe_castle_upgrade_popup_open";

	private const string CLASS_UNLOCK_CLOSE_AUDIO_PATH = "event:/UI/FrontEnd/ui_fe_castle_upgrade_popup_close";

	public SkillTreeType PopupType { get; private set; }

	public override WindowID ID => WindowID.SkillTreePopUp;

	public string Description
	{
		get
		{
			if (m_description == string.Empty)
			{
				m_description = ToString();
			}
			return m_description;
		}
	}

	private void Awake()
	{
		m_cancelClassChange = CancelClassChange;
		m_confirmClassChange = ConfirmClassChange;
		m_onConfirmButtonPressed = OnConfirmButtonPressed;
	}

	protected override void OnFocus()
	{
		AddListeners();
	}

	protected override void OnLostFocus()
	{
		RemoveListeners();
	}

	public void SetPopupType(SkillTreeType popupType)
	{
		PopupType = popupType;
		SkillTreePopupData popupData = SkillTreePopupLibrary.GetPopupData(popupType);
		if (popupData != null)
		{
			m_titleTextLocItem.SetString(popupData.TitleLocID);
			m_subTitleTextLocItem.SetString(popupData.SubtitleLocID);
			m_descriptionTextLocItem.SetString(popupData.DescriptionLocID);
			m_popupSprite.sprite = popupData.PopupSprite;
		}
	}

	protected override void OnOpen()
	{
		PlayOpenAndCloseAudioSFX(isOpen: true);
		m_displayedClassChange = false;
		StartCoroutine(OnOpenAnimCoroutine());
	}

	private void PlayOpenAndCloseAudioSFX(bool isOpen)
	{
		string text = string.Empty;
		if (isOpen)
		{
			text = "event:/UI/FrontEnd/ui_fe_generic_popUp";
			if (PopupType.ToString().ToLower().Contains("class_unlock"))
			{
				text = "event:/UI/FrontEnd/ui_fe_castle_upgrade_popup_open";
			}
		}
		else if (PopupType.ToString().ToLower().Contains("class_unlock"))
		{
			text = "event:/UI/FrontEnd/ui_fe_castle_upgrade_popup_close";
		}
		if (text != string.Empty)
		{
			AudioManager.PlayOneShot(this, text);
		}
	}

	private IEnumerator OnOpenAnimCoroutine()
	{
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		m_rayCanvasGroup.alpha = 0f;
		m_popupCanvasGroup.alpha = 0.5f;
		Vector3 localEulerAngles = m_popupCanvasGroup.transform.localEulerAngles;
		localEulerAngles.z = 40f;
		m_popupCanvasGroup.transform.localEulerAngles = localEulerAngles;
		Vector3 localScale = m_popupCanvasGroup.transform.localScale;
		localScale *= 0.25f;
		m_popupCanvasGroup.transform.localScale = localScale;
		m_rayCanvasGroup.transform.localScale = Vector3.zero;
		m_windowCanvas.gameObject.SetActive(value: true);
		float duration = 0.5f;
		TweenManager.TweenTo_UnscaledTime(m_rayCanvasGroup, 0.15f, Ease.None, "delay", 0.1f, "alpha", 1);
		TweenManager.TweenTo_UnscaledTime(m_rayCanvasGroup.transform, duration, Ease.Back.EaseOut, "localScale.x", 1, "localScale.y", 1, "localScale.z", 1);
		TweenManager.TweenTo_UnscaledTime(m_popupCanvasGroup, 0.15f, Ease.None, "alpha", 1);
		TweenManager.TweenTo_UnscaledTime(m_popupCanvasGroup.transform, duration, Ease.Back.EaseOut, "localEulerAngles.z", 0);
		yield return TweenManager.TweenTo_UnscaledTime(m_popupCanvasGroup.transform, duration, Ease.Back.EaseOut, "localScale.x", 1, "localScale.y", 1, "localScale.z", 1).TweenCoroutine;
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
	}

	protected override void OnClose()
	{
		PlayOpenAndCloseAudioSFX(isOpen: false);
		m_windowCanvas.gameObject.SetActive(value: false);
	}

	private void AddListeners()
	{
		base.RewiredPlayer.AddInputEventDelegate(m_onConfirmButtonPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
		base.RewiredPlayer.AddInputEventDelegate(m_onConfirmButtonPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
	}

	private void RemoveListeners()
	{
		base.RewiredPlayer.RemoveInputEventDelegate(m_onConfirmButtonPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
		base.RewiredPlayer.RemoveInputEventDelegate(m_onConfirmButtonPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
	}

	private void OnConfirmButtonPressed(InputActionEventData eventData)
	{
		m_classToChangeTo = SkillTreeLogicHelper.GetClassTypeFromSkill(PopupType);
		ClassData classData = ClassLibrary.GetClassData(m_classToChangeTo);
		if (!m_displayedClassChange && m_classToChangeTo != 0 && classData != null)
		{
			m_displayedClassChange = true;
			InitializeConfirmMenu();
			WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: true);
		}
		else
		{
			WindowManager.SetWindowIsOpen(WindowID.SkillTreePopUp, isOpen: false);
		}
	}

	private void InitializeConfirmMenu()
	{
		if (!WindowManager.GetIsWindowLoaded(WindowID.ConfirmMenu))
		{
			WindowManager.LoadWindow(WindowID.ConfirmMenu);
		}
		string @string = LocalizationManager.GetString(ClassLibrary.GetClassData(m_classToChangeTo).PassiveData.Title, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenu) as ConfirmMenuWindowController;
		obj.SetTitleText("LOC_ID_SKILL_TREE_UI_CHANGE_CHARACTER_TITLE_1", isLocID: true);
		obj.SetDescriptionText(string.Format(LocalizationManager.GetString("LOC_ID_SKILL_TREE_UI_CHANGE_CHARACTER_TEXT_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), @string), isLocID: false);
		obj.SetNumberOfButtons(2);
		obj.SetOnCancelAction(m_cancelClassChange);
		ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
		buttonAtIndex.SetButtonText("LOC_ID_GENERAL_UI_YES_1", isLocID: true);
		buttonAtIndex.SetOnClickAction(m_confirmClassChange);
		ConfirmMenu_Button buttonAtIndex2 = obj.GetButtonAtIndex(1);
		buttonAtIndex2.SetButtonText("LOC_ID_GENERAL_UI_NO_1", isLocID: true);
		buttonAtIndex2.SetOnClickAction(m_cancelClassChange);
	}

	private void ConfirmClassChange()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
		WindowManager.SetWindowIsOpen(WindowID.SkillTreePopUp, isOpen: false);
		AbilityType spell = SaveManager.PlayerSaveData.CurrentCharacter.Spell;
		AbilityType talent = SaveManager.PlayerSaveData.CurrentCharacter.Talent;
		CharacterCreator.GenerateClass(m_classToChangeTo, SaveManager.PlayerSaveData.CurrentCharacter);
		AbilityType abilityType = SaveManager.PlayerSaveData.CurrentCharacter.Talent;
		if (talent == AbilityType.SuperFart)
		{
			abilityType = talent;
		}
		if (TraitManager.IsTraitActive(TraitType.CantAttack))
		{
			SaveManager.PlayerSaveData.CurrentCharacter.Weapon = AbilityType.PacifistWeapon;
		}
		if (abilityType == spell)
		{
			AbilityType[] availableTalents = CharacterCreator.GetAvailableTalents(m_classToChangeTo);
			AbilityType abilityType2 = ((availableTalents.Length != 0) ? availableTalents[RNGManager.GetRandomNumber(RngID.Lineage, "GetRandomTalent", 0, availableTalents.Length)] : AbilityType.None);
			int num = 0;
			while (abilityType2 != 0 && abilityType2 == spell && num < 50)
			{
				num++;
				abilityType2 = availableTalents[RNGManager.GetRandomNumber(RngID.Lineage, "GetRandomTalent", 0, availableTalents.Length)];
			}
			if (num >= 50)
			{
				Debug.LogWarning("<color=yellow>Could not find non-duplicate talent in SkillTreePopupWindowController.</color>");
			}
			abilityType = abilityType2;
		}
		SaveManager.PlayerSaveData.CurrentCharacter.Talent = abilityType;
		SaveManager.PlayerSaveData.CurrentCharacter.Spell = spell;
		SaveManager.PlayerSaveData.SetSpellSeenState(SaveManager.PlayerSaveData.CurrentCharacter.Spell, seenState: true);
		SaveManager.PlayerSaveData.SetSpellSeenState(SaveManager.PlayerSaveData.CurrentCharacter.Talent, seenState: true);
		LineageWindowController.CharacterLoadedFromLineage = true;
		PlayerManager.GetPlayerController().ResetCharacter();
		LineageWindowController.CharacterLoadedFromLineage = false;
	}

	private void CancelClassChange()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
		WindowManager.SetWindowIsOpen(WindowID.SkillTreePopUp, isOpen: false);
	}

	private void Update()
	{
		Vector3 localEulerAngles = m_ray.transform.localEulerAngles;
		localEulerAngles.z -= 10f * Time.unscaledDeltaTime;
		m_ray.transform.localEulerAngles = localEulerAngles;
	}
}
