using System;
using System.Collections;
using RLAudio;
using Rewired;
using Sigtrap.Relays;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RL_Windows;

public class SpecialItemDropWindowController : WindowController, ILocalizable
{
	private const int ITEM_DROP_ENTRY_POOLSIZE = 5;

	[SerializeField]
	private CanvasGroup m_bgCanvasGroup;

	[SerializeField]
	private CanvasGroup m_textBGCanvasGroup;

	[Header("Text Objects")]
	[SerializeField]
	private TMP_Text m_itemFoundText;

	[SerializeField]
	private TMP_Text m_itemNameText;

	[SerializeField]
	private TMP_Text m_descriptionText;

	[Header("Icons")]
	[SerializeField]
	private Image m_iconImage;

	[SerializeField]
	private Image m_iconFrameImage;

	[SerializeField]
	private Sprite m_genericIconFrameSprite;

	[SerializeField]
	private Sprite m_runeIconFrameSprite;

	[SerializeField]
	private Animator m_animator;

	private ISpecialItemDrop[] m_specialItemDropArray;

	private int m_currentEntryIndex;

	private int m_numActiveEntries;

	private bool m_displayNextEntry;

	private WaitRL_Yield m_waitYield;

	private WaitUntil m_displayNextYield;

	private Action<MonoBehaviour, EventArgs> m_refreshText;

	public Relay<SpecialItemType> WindowOpenedRelay = new Relay<SpecialItemType>();

	public Relay<SpecialItemType> WindowClosedRelay = new Relay<SpecialItemType>();

	public Relay<SpecialItemType> ItemRevealedRelay = new Relay<SpecialItemType>();

	private Action<InputActionEventData> m_onConfirmButtonDown;

	public override WindowID ID => WindowID.SpecialItemDrop;

	private void Awake()
	{
		m_refreshText = RefreshText;
		m_onConfirmButtonDown = OnConfirmButtonDown;
	}

	public override void Initialize()
	{
		base.Initialize();
		m_specialItemDropArray = new ISpecialItemDrop[5];
		m_waitYield = new WaitRL_Yield(0f, useUnscaledTime: true);
		m_displayNextYield = new WaitUntil(() => m_displayNextEntry);
	}

	public void AddSpecialItemDrop(ISpecialItemDrop specialItemDrop)
	{
		if (specialItemDrop != null)
		{
			m_specialItemDropArray[m_numActiveEntries] = specialItemDrop;
			m_numActiveEntries++;
		}
	}

	private void ClearSpecialItemEntryArray()
	{
		m_numActiveEntries = 0;
		for (int i = 0; i < 5; i++)
		{
			m_specialItemDropArray[i] = null;
		}
	}

	protected override void OnFocus()
	{
		if (ReInput.isReady && base.RewiredPlayer != null)
		{
			base.RewiredPlayer.AddInputEventDelegate(m_onConfirmButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
			base.RewiredPlayer.AddInputEventDelegate(m_onConfirmButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
		}
	}

	protected override void OnLostFocus()
	{
		if (ReInput.isReady && base.RewiredPlayer != null)
		{
			base.RewiredPlayer.RemoveInputEventDelegate(m_onConfirmButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
			base.RewiredPlayer.RemoveInputEventDelegate(m_onConfirmButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
		}
	}

	private void OnConfirmButtonDown(InputActionEventData obj)
	{
		if (m_currentEntryIndex >= m_numActiveEntries - 1)
		{
			RewiredMapController.SetCurrentMapEnabled(enabled: false);
			StartCoroutine(OnExitCoroutine());
		}
		else
		{
			m_displayNextEntry = true;
		}
	}

	protected override void OnOpen()
	{
		AudioManager.SetEnemySFXPaused(pause: true);
		AudioManager.SetPlayerSFXPaused(pause: true);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.LanguageChanged, m_refreshText);
		AudioManager.PlayOneShot(null, "event:/Stingers/sting_chestItem", CameraController.GameCamera.transform.position);
		SetAnimatorToIdle();
		m_windowCanvas.gameObject.SetActive(value: true);
		m_currentEntryIndex = 0;
		StopAllCoroutines();
		if (m_numActiveEntries > 0)
		{
			StartCoroutine(RunOnEnterAnimation());
		}
	}

	protected override void OnClose()
	{
		AudioManager.SetEnemySFXPaused(pause: false);
		AudioManager.SetPlayerSFXPaused(pause: false);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.LanguageChanged, m_refreshText);
		m_windowCanvas.gameObject.SetActive(value: false);
		ClearSpecialItemEntryArray();
		SaveFileSystem.SaveBatch saveBatch = SaveFileSystem.BeginSaveBatch(SaveManager.CurrentProfile);
		SaveManager.SaveCurrentProfileGameData(SaveDataType.Stage, SavingType.FileOnly, ignoreTimeRestriction: true);
		SaveManager.SaveCurrentProfileGameData(SaveDataType.Equipment, SavingType.FileOnly, ignoreTimeRestriction: true);
		saveBatch.End();
	}

	private void Update()
	{
		m_textBGCanvasGroup.alpha = m_descriptionText.alpha;
	}

	private IEnumerator RunOnEnterAnimation()
	{
		m_bgCanvasGroup.alpha = 0f;
		TweenManager.TweenTo_UnscaledTime(m_bgCanvasGroup, 0.25f, Ease.Quad.EaseOut, "alpha", 0.75f);
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
		while (m_currentEntryIndex < m_numActiveEntries)
		{
			ISpecialItemDrop specialItem = m_specialItemDropArray[m_currentEntryIndex];
			yield return DisplayEntryCoroutine(specialItem);
			yield return m_displayNextYield;
			yield return FadeOutAnimationCoroutine();
			m_displayNextEntry = false;
			m_currentEntryIndex++;
		}
	}

	private IEnumerator FadeOutAnimationCoroutine()
	{
		m_animator.SetTrigger("Exit");
		m_waitYield.CreateNew(0.25f, useUnscaledTime: true);
		yield return m_waitYield;
	}

	private IEnumerator DisplayEntryCoroutine(ISpecialItemDrop specialItem)
	{
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		InitializeSpecialItemEntry(specialItem);
		m_waitYield.CreateNew(2.5f, useUnscaledTime: true);
		yield return m_waitYield;
		ItemRevealedRelay.Dispatch(specialItem.SpecialItemType);
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
	}

	private void InitializeSpecialItemEntry(ISpecialItemDrop specialItem)
	{
		string text = "";
		string text2 = "";
		string text3 = "";
		m_iconFrameImage.gameObject.SetActive(value: true);
		m_iconFrameImage.gameObject.transform.localScale = Vector3.one;
		m_iconImage.transform.localScale = new Vector3(1f, 1f, 1f);
		switch (specialItem.SpecialItemType)
		{
		case SpecialItemType.Rune:
		{
			IRuneDrop runeDrop = specialItem as IRuneDrop;
			RuneData runeData = RuneLibrary.GetRuneData(runeDrop.RuneType);
			text = LocalizationManager.GetString("LOC_ID_RUNE_UI_RUNE_FOUND_TITLE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			text2 = LocalizationManager.GetString(runeData.Title, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			text3 = LocalizationManager.GetString(runeData.Description, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			m_iconFrameImage.sprite = m_runeIconFrameSprite;
			m_iconImage.sprite = IconLibrary.GetRuneIcon(runeDrop.RuneType);
			m_animator.SetTrigger("Rune");
			break;
		}
		case SpecialItemType.Blueprint:
		{
			IBlueprintDrop blueprintDrop = specialItem as IBlueprintDrop;
			EquipmentObj equipment = EquipmentManager.GetEquipment(blueprintDrop.CategoryType, blueprintDrop.EquipmentType);
			EquipmentData equipmentData = EquipmentLibrary.GetEquipmentData(blueprintDrop.CategoryType, blueprintDrop.EquipmentType);
			text = LocalizationManager.GetString("LOC_ID_BLUEPRINT_UI_BLUEPRINT_FOUND_TITLE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			if (equipment.UpgradeBlueprintsFound > 0)
			{
				text2 = Equipment_EV.GetFormattedEquipmentName(equipment.CategoryType, equipment.EquipmentType);
				text2 = text2 + " +" + equipment.UpgradeBlueprintsFound;
			}
			else
			{
				text2 = Equipment_EV.GetFormattedEquipmentName(equipment.CategoryType, equipment.EquipmentType);
			}
			text3 = LocalizationManager.GetString(equipmentData.Description, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			m_iconFrameImage.sprite = m_genericIconFrameSprite;
			m_iconImage.sprite = IconLibrary.GetEquipmentIcon(blueprintDrop.CategoryType, blueprintDrop.EquipmentType);
			m_animator.SetTrigger("Blueprint");
			break;
		}
		case SpecialItemType.Relic:
		{
			IRelicDrop relicDrop = specialItem as IRelicDrop;
			RelicData relicData = RelicLibrary.GetRelicData(relicDrop.RelicType);
			text = LocalizationManager.GetString("LOC_ID_RELIC_UI_RELIC_FOUND_TITLE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			if (relicDrop.RelicModType == RelicModType.DoubleRelic)
			{
				text += " <size=70%>X</size><size=130%>2</size>";
			}
			text2 = LocalizationManager.GetString(relicData.Title, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			text3 = LocalizationManager.GetString(relicData.Description, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale, out var isFormatString);
			if (isFormatString)
			{
				int num = SaveManager.PlayerSaveData.GetRelic(relicDrop.RelicType).Level;
				if (relicDrop.RelicModType == RelicModType.DoubleRelic)
				{
					num++;
				}
				float altValue;
				float relicFormatString = Relic_EV.GetRelicFormatString(relicDrop.RelicType, num + 1, out altValue);
				text3 = string.Format(text3, relicFormatString.ToCIString(), altValue.ToCIString());
			}
			m_iconFrameImage.gameObject.SetActive(value: false);
			m_iconImage.sprite = IconLibrary.GetRelicSprite(relicDrop.RelicType, getLargeIcon: true);
			m_iconImage.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
			m_animator.SetTrigger("Item");
			break;
		}
		case SpecialItemType.Heirloom:
		{
			IHeirloomDrop heirloomDrop = specialItem as IHeirloomDrop;
			HeirloomData heirloomData = HeirloomLibrary.GetHeirloomData(heirloomDrop.HeirloomType);
			text = LocalizationManager.GetString("LOC_ID_HEIRLOOM_UI_HEIRLOOM_FOUND_TITLE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			text2 = LocalizationManager.GetString(heirloomData.TitleLocID, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			text3 = LocalizationManager.GetString(heirloomData.DescriptionLocID, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			m_iconFrameImage.gameObject.SetActive(value: false);
			m_iconImage.sprite = IconLibrary.GetHeirloomSprite(heirloomDrop.HeirloomType);
			m_iconImage.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
			m_animator.SetTrigger("Item");
			break;
		}
		case SpecialItemType.Ability:
		{
			IAbilityDrop abilityDrop = specialItem as IAbilityDrop;
			AbilityData abilityData = AbilityLibrary.GetAbility(abilityDrop.AbilityType).AbilityData;
			text = LocalizationManager.GetString("LOC_ID_RELIC_UI_SWAP_FOUND_TITLE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			text2 = LocalizationManager.GetString(abilityData.Title, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			text3 = LocalizationManager.GetString(abilityData.Description, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			m_iconFrameImage.gameObject.SetActive(value: false);
			m_iconImage.sprite = IconLibrary.GetLargeAbilityIcon(abilityDrop.AbilityType, returnNullIfNotFound: true);
			m_iconImage.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
			m_animator.SetTrigger("Item");
			break;
		}
		case SpecialItemType.Challenge:
		{
			IChallengeDrop challengeDrop = specialItem as IChallengeDrop;
			ChallengeData challengeData = ChallengeLibrary.GetChallengeData(challengeDrop.ChallengeType);
			text = LocalizationManager.GetString("LOC_ID_CHALLENGE_UI_CHALLENGE_FOUND_TITLE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			text2 = LocalizationManager.GetString(challengeData.Title, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			text3 = LocalizationManager.GetString("LOC_ID_CHALLENGE_UI_CHALLENGE_FOUND_TEXT_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			m_iconFrameImage.sprite = m_genericIconFrameSprite;
			m_iconImage.sprite = IconLibrary.GetChallengeIcon(challengeDrop.ChallengeType, ChallengeLibrary.ChallengeIconEntryType.Challenge);
			m_animator.SetTrigger("Challenge");
			break;
		}
		default:
			text = LocalizationManager.GetString("LOC_ID_DEFAULT_ITEM_UI_DEFAULT_FOUND_TITLE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			text2 = LocalizationManager.GetString("LOC_ID_CHEST_UI_DEFAULT_ITEM_NAME_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			text3 = LocalizationManager.GetString("LOC_ID_CHEST_UI_DEFAULT_DESCRIPTION_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			m_iconFrameImage.sprite = m_genericIconFrameSprite;
			m_animator.SetTrigger("Item");
			break;
		}
		m_itemFoundText.text = text;
		m_itemNameText.text = text2;
		m_descriptionText.text = text3;
	}

	private IEnumerator OnExitCoroutine()
	{
		TweenManager.TweenTo_UnscaledTime(m_bgCanvasGroup, 0.25f, Ease.None, "alpha", 0);
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		m_animator.SetTrigger("Exit");
		m_waitYield.CreateNew(0.25f, useUnscaledTime: true);
		yield return m_waitYield;
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
		WindowManager.SetWindowIsOpen(WindowID.SpecialItemDrop, isOpen: false);
	}

	private void SetAnimatorToIdle()
	{
		AnimatorControllerParameter[] parameters = m_animator.parameters;
		foreach (AnimatorControllerParameter animatorControllerParameter in parameters)
		{
			switch (animatorControllerParameter.type)
			{
			case AnimatorControllerParameterType.Bool:
				m_animator.SetBool(animatorControllerParameter.name, value: false);
				break;
			case AnimatorControllerParameterType.Trigger:
				m_animator.ResetTrigger(animatorControllerParameter.name);
				break;
			}
		}
	}

	public void RefreshText(object sender, EventArgs args)
	{
		if (m_currentEntryIndex >= 0 && m_currentEntryIndex < m_specialItemDropArray.Length)
		{
			ISpecialItemDrop specialItem = m_specialItemDropArray[m_currentEntryIndex];
			InitializeSpecialItemEntry(specialItem);
		}
	}
}
