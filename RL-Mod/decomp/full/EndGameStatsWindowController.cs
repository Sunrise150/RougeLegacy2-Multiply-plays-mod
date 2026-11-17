using System;
using System.Collections;
using RL_Windows;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndGameStatsWindowController : WindowController
{
	[SerializeField]
	private Image m_ray;

	[SerializeField]
	private GameObject m_biomeLightController;

	[Header("Banner")]
	[SerializeField]
	private TMP_Text m_bannerName;

	[Header("Player")]
	[SerializeField]
	private TMP_Text m_levelText;

	[SerializeField]
	private TMP_Text m_classText;

	[SerializeField]
	private Image m_trait1Icon;

	[SerializeField]
	private Image m_trait2Icon;

	[SerializeField]
	private GameObject m_noTraitsText;

	[SerializeField]
	private Image m_weaponEquipmentIcon;

	[SerializeField]
	private Image m_headEquipmentIcon;

	[SerializeField]
	private Image m_chestEquipmentIcon;

	[SerializeField]
	private Image m_capeEquipmentIcon;

	[SerializeField]
	private Image m_trinketEquipmentIcon;

	[SerializeField]
	private GameObject m_noGearText;

	[SerializeField]
	private PlayerLookController m_playerModel;

	[Header("Stats")]
	[SerializeField]
	private EndGameStatsStatsEntry[] m_statsEntries;

	[Header("Rating")]
	[SerializeField]
	private EndGameStatsStatsEntry m_ratingEntry;

	private CanvasGroup m_canvasGroup;

	private Action<InputActionEventData> m_onCancelButtonDown;

	public override WindowID ID => WindowID.EndGameStats;

	private void Awake()
	{
		m_onCancelButtonDown = OnCancelButtonDown;
	}

	public override void Initialize()
	{
		m_canvasGroup = m_windowCanvas.GetComponent<CanvasGroup>();
		base.Initialize();
		if (m_playerModel.VisualsGameObject != null)
		{
			m_playerModel.VisualsGameObject.SetLayerRecursively(5, includeInactive: true);
		}
	}

	protected override void OnOpen()
	{
		UpdateAllStats();
		m_windowCanvas.gameObject.SetActive(value: true);
		m_playerModel.InitializeLook(SaveManager.PlayerSaveData.CurrentCharacter);
		m_playerModel.Animator.SetBool("Victory", value: true);
		m_playerModel.Animator.Play("Victory", 0, 1f);
		m_biomeLightController.SetActive(value: true);
		StartCoroutine(OnOpenCoroutine());
	}

	private IEnumerator OnOpenCoroutine()
	{
		m_canvasGroup.alpha = 0f;
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		yield return TweenManager.TweenTo_UnscaledTime(m_canvasGroup, 0.25f, Ease.None, "alpha", 1).TweenCoroutine;
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
	}

	protected override void OnClose()
	{
		m_windowCanvas.gameObject.SetActive(value: false);
		m_biomeLightController.SetActive(value: false);
	}

	protected override void OnFocus()
	{
		base.RewiredPlayer.AddInputEventDelegate(m_onCancelButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
		base.RewiredPlayer.AddInputEventDelegate(m_onCancelButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
	}

	protected override void OnLostFocus()
	{
		base.RewiredPlayer.RemoveInputEventDelegate(m_onCancelButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
		base.RewiredPlayer.RemoveInputEventDelegate(m_onCancelButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
	}

	private void OnCancelButtonDown(InputActionEventData data)
	{
		WindowManager.SetWindowIsOpen(WindowID.EndGameStats, isOpen: false);
	}

	private void UpdateAllStats()
	{
		m_bannerName.text = LocalizationManager.GetLocalizedPlayerName(SaveManager.PlayerSaveData.CurrentCharacter);
		m_levelText.text = SkillTreeManager.GetTotalSkillObjLevel().ToString();
		ClassData classData = ClassLibrary.GetClassData(SaveManager.PlayerSaveData.CurrentCharacter.ClassType);
		if ((bool)classData)
		{
			m_classText.text = LocalizationManager.GetString(classData.PassiveData.Title, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
		UpdateTraits();
		bool gearFound = false;
		UpdateGear(EquipmentCategoryType.Weapon, ref gearFound);
		UpdateGear(EquipmentCategoryType.Head, ref gearFound);
		UpdateGear(EquipmentCategoryType.Chest, ref gearFound);
		UpdateGear(EquipmentCategoryType.Cape, ref gearFound);
		UpdateGear(EquipmentCategoryType.Trinket, ref gearFound);
		if (gearFound)
		{
			m_noGearText.SetActive(value: false);
		}
		else
		{
			m_noGearText.SetActive(value: true);
		}
		EndGameStatsStatsEntry[] statsEntries = m_statsEntries;
		for (int i = 0; i < statsEntries.Length; i++)
		{
			statsEntries[i].UpdateStat(animate: true);
		}
		m_ratingEntry.UpdateStat(animate: true);
	}

	private void UpdateTraits()
	{
		bool flag = false;
		if (TraitLibrary.GetTraitData(SaveManager.PlayerSaveData.CurrentCharacter.TraitOne) != null)
		{
			m_trait1Icon.transform.parent.gameObject.SetActive(value: true);
			m_trait1Icon.sprite = IconLibrary.GetTraitIcon(SaveManager.PlayerSaveData.CurrentCharacter.TraitOne);
			flag = true;
		}
		else
		{
			m_trait1Icon.transform.parent.gameObject.SetActive(value: false);
		}
		if (TraitLibrary.GetTraitData(SaveManager.PlayerSaveData.CurrentCharacter.TraitTwo) != null)
		{
			m_trait2Icon.transform.parent.gameObject.SetActive(value: true);
			m_trait2Icon.sprite = IconLibrary.GetTraitIcon(SaveManager.PlayerSaveData.CurrentCharacter.TraitTwo);
			flag = true;
		}
		else
		{
			m_trait2Icon.transform.parent.gameObject.SetActive(value: false);
		}
		if (flag)
		{
			m_noTraitsText.SetActive(value: false);
		}
		else
		{
			m_noTraitsText.SetActive(value: true);
		}
	}

	private void UpdateGear(EquipmentCategoryType categoryType, ref bool gearFound)
	{
		Image image = null;
		switch (categoryType)
		{
		case EquipmentCategoryType.Weapon:
			image = m_weaponEquipmentIcon;
			break;
		case EquipmentCategoryType.Head:
			image = m_headEquipmentIcon;
			break;
		case EquipmentCategoryType.Chest:
			image = m_chestEquipmentIcon;
			break;
		case EquipmentCategoryType.Cape:
			image = m_capeEquipmentIcon;
			break;
		case EquipmentCategoryType.Trinket:
			image = m_trinketEquipmentIcon;
			break;
		}
		EquipmentObj equipped = EquipmentManager.GetEquipped(categoryType);
		if (equipped != null)
		{
			image.transform.parent.gameObject.SetActive(value: true);
			image.sprite = IconLibrary.GetEquipmentIcon(categoryType, equipped.EquipmentType);
			gearFound = true;
		}
		else
		{
			image.transform.parent.gameObject.SetActive(value: false);
		}
	}

	private void Update()
	{
		if (m_windowCanvas.gameObject.activeSelf)
		{
			float num = -5f * Time.unscaledDeltaTime;
			m_ray.transform.SetLocalEulerZ(m_ray.transform.localEulerAngles.z + num);
		}
	}
}
