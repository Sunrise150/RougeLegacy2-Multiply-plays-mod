using System;
using System.Collections;
using System.Collections.Generic;
using RL_Windows;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

public class GearCardWindowController : WindowController, ILocalizable
{
	private const int STARTING_ARRAY_SIZE = 5;

	[SerializeField]
	private GearCardEntry m_gearCardEntryPrefab;

	[SerializeField]
	private GameObject m_leftSideContentGO;

	[SerializeField]
	private GameObject m_rightSideContentGO;

	[SerializeField]
	private CanvasGroup m_leftCardCanvasGroup;

	[SerializeField]
	private CanvasGroup m_rightCardCanvasGroup;

	[SerializeField]
	private GameObject m_noEquipmentGO;

	[SerializeField]
	private GameObject m_noUnityGO;

	[SerializeField]
	private GameObject m_noRunesGO;

	[SerializeField]
	private GameObject m_noHeirloomsGO;

	[SerializeField]
	private GameObject m_noRelicsGO;

	[SerializeField]
	private Image m_leftScrollArrow;

	[SerializeField]
	private Image m_rightScrollArrow;

	[SerializeField]
	private ScrollBarInput_RL m_leftScrollBarInput;

	[SerializeField]
	private ScrollBarInput_RL m_rightScrollBarInput;

	private GearCardEntry[] m_equipmentEntryArray;

	private GearCardEntry[] m_unityEntryArray;

	private GearCardEntry[] m_runeEntryArray;

	private GearCardEntry[] m_heirloomEntryArray;

	private GearCardEntry[] m_relicEntryArray;

	private ScrollBarInput_RL m_selectedScrollBarInput;

	private Action<MonoBehaviour, EventArgs> m_refreshText;

	private Action<InputActionEventData> m_onCancelButtonDown;

	private Action<InputActionEventData> m_onHorizontalInputHandler;

	private static List<Vector2> m_activeUnityHelper = new List<Vector2>();

	private static List<RuneType> m_activeRuneHelper = new List<RuneType>();

	private static List<HeirloomType> m_activeHeirloomHelper = new List<HeirloomType>();

	private static List<RelicType> m_activeRelicHelper = new List<RelicType>();

	public override WindowID ID => WindowID.GearCard;

	private void Awake()
	{
		m_refreshText = RefreshText;
		m_onCancelButtonDown = OnCancelButtonDown;
		m_onHorizontalInputHandler = OnHorizontalInputHandler;
	}

	public override void Initialize()
	{
		m_equipmentEntryArray = new GearCardEntry[5];
		m_unityEntryArray = new GearCardEntry[5];
		m_runeEntryArray = new GearCardEntry[5];
		m_heirloomEntryArray = new GearCardEntry[5];
		m_relicEntryArray = new GearCardEntry[5];
		int siblingIndex = m_noEquipmentGO.transform.GetSiblingIndex();
		for (int i = 0; i < 5; i++)
		{
			GearCardEntry gearCardEntry = InstantiateGearCardEntry(m_leftSideContentGO.transform, siblingIndex);
			gearCardEntry.SetAsEquipment(EquipmentCategoryType.Cape, EquipmentType.GEAR_ARMOR);
			m_equipmentEntryArray[i] = gearCardEntry;
		}
		int siblingIndex2 = m_noUnityGO.transform.GetSiblingIndex();
		for (int j = 0; j < 5; j++)
		{
			GearCardEntry gearCardEntry2 = InstantiateGearCardEntry(m_leftSideContentGO.transform, siblingIndex2);
			RectTransform component = gearCardEntry2.GetComponent<RectTransform>();
			Vector2 sizeDelta = component.sizeDelta;
			sizeDelta.y *= 0.2f;
			component.sizeDelta = sizeDelta;
			m_unityEntryArray[j] = gearCardEntry2;
		}
		int siblingIndex3 = m_noRunesGO.transform.GetSiblingIndex();
		for (int k = 0; k < 5; k++)
		{
			GearCardEntry gearCardEntry3 = InstantiateGearCardEntry(m_leftSideContentGO.transform, siblingIndex3);
			m_runeEntryArray[k] = gearCardEntry3;
		}
		int siblingIndex4 = m_noRelicsGO.transform.GetSiblingIndex();
		for (int l = 0; l < 5; l++)
		{
			GearCardEntry gearCardEntry4 = InstantiateGearCardEntry(m_rightSideContentGO.transform, siblingIndex4);
			m_relicEntryArray[l] = gearCardEntry4;
		}
		int siblingIndex5 = m_noHeirloomsGO.transform.GetSiblingIndex();
		for (int m = 0; m < 5; m++)
		{
			GearCardEntry gearCardEntry5 = InstantiateGearCardEntry(m_rightSideContentGO.transform, siblingIndex5);
			m_heirloomEntryArray[m] = gearCardEntry5;
		}
		base.Initialize();
	}

	private GearCardEntry InstantiateGearCardEntry(Transform parentTransform, int siblingIndex)
	{
		GearCardEntry gearCardEntry = UnityEngine.Object.Instantiate(m_gearCardEntryPrefab, parentTransform, worldPositionStays: false);
		gearCardEntry.gameObject.SetActive(value: false);
		gearCardEntry.transform.SetSiblingIndex(siblingIndex);
		return gearCardEntry;
	}

	private void UpdateArrays()
	{
		UpdateEquipmentEntry(EquipmentCategoryType.Weapon, 0);
		UpdateEquipmentEntry(EquipmentCategoryType.Head, 1);
		UpdateEquipmentEntry(EquipmentCategoryType.Chest, 2);
		UpdateEquipmentEntry(EquipmentCategoryType.Cape, 3);
		UpdateEquipmentEntry(EquipmentCategoryType.Trinket, 4);
		bool flag = false;
		GearCardEntry[] equipmentEntryArray = m_equipmentEntryArray;
		for (int i = 0; i < equipmentEntryArray.Length; i++)
		{
			if (equipmentEntryArray[i].gameObject.activeSelf)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			m_noEquipmentGO.SetActive(value: false);
		}
		else
		{
			m_noEquipmentGO.SetActive(value: true);
		}
		if (UpdateUnityEntries())
		{
			m_noUnityGO.SetActive(value: false);
		}
		else
		{
			m_noUnityGO.SetActive(value: true);
		}
		if (UpdateRuneEntries_V2())
		{
			m_noRunesGO.SetActive(value: false);
		}
		else
		{
			m_noRunesGO.SetActive(value: true);
		}
		if (UpdateHeirloomEntries_V2())
		{
			m_noHeirloomsGO.SetActive(value: false);
		}
		else
		{
			m_noHeirloomsGO.SetActive(value: true);
		}
		if (UpdateRelicEntries_V2())
		{
			m_noRelicsGO.SetActive(value: false);
		}
		else
		{
			m_noRelicsGO.SetActive(value: true);
		}
	}

	private void UpdateEquipmentEntry(EquipmentCategoryType category, int arrayIndex)
	{
		EquipmentObj equipped = EquipmentManager.GetEquipped(category);
		GearCardEntry gearCardEntry = m_equipmentEntryArray[arrayIndex];
		if (equipped != null)
		{
			if (!gearCardEntry.gameObject.activeSelf)
			{
				gearCardEntry.gameObject.SetActive(value: true);
			}
			gearCardEntry.SetAsEquipment(equipped.CategoryType, equipped.EquipmentType);
		}
		else if (gearCardEntry.gameObject.activeSelf)
		{
			gearCardEntry.gameObject.SetActive(value: false);
		}
	}

	private bool UpdateUnityEntries()
	{
		m_activeUnityHelper.Clear();
		EquipmentSetBonusType[] typeArray = EquipmentSetBonusType_RL.TypeArray;
		foreach (EquipmentSetBonusType equipmentSetBonusType in typeArray)
		{
			if (equipmentSetBonusType != 0)
			{
				float num = EquipmentManager.Get_EquipmentSet_BonusTypeStatGain(equipmentSetBonusType);
				if (num > 0f)
				{
					m_activeUnityHelper.Add(new Vector2((float)equipmentSetBonusType, num));
				}
			}
		}
		int count = m_activeUnityHelper.Count;
		if (count > m_unityEntryArray.Length)
		{
			int num2 = m_unityEntryArray.Length;
			m_unityEntryArray = ExpandGearCardArray(m_unityEntryArray, count + 5);
			int siblingIndex = m_noUnityGO.transform.GetSiblingIndex();
			for (int j = num2; j < m_unityEntryArray.Length; j++)
			{
				GearCardEntry gearCardEntry = InstantiateGearCardEntry(m_leftSideContentGO.transform, siblingIndex);
				RectTransform component = gearCardEntry.GetComponent<RectTransform>();
				Vector2 sizeDelta = component.sizeDelta;
				sizeDelta.y *= 0.25f;
				component.sizeDelta = sizeDelta;
				m_unityEntryArray[j] = gearCardEntry;
			}
		}
		for (int k = 0; k < m_unityEntryArray.Length; k++)
		{
			GearCardEntry gearCardEntry2 = m_unityEntryArray[k];
			if (k < count)
			{
				gearCardEntry2.SetAsUnity((EquipmentSetBonusType)m_activeUnityHelper[k].x, m_activeUnityHelper[k].y);
				if (!gearCardEntry2.gameObject.activeSelf)
				{
					gearCardEntry2.gameObject.SetActive(value: true);
				}
			}
			else if (gearCardEntry2.gameObject.activeSelf)
			{
				gearCardEntry2.gameObject.SetActive(value: false);
			}
		}
		return count > 0;
	}

	private bool UpdateRuneEntries_V2()
	{
		m_activeRuneHelper.Clear();
		RuneType[] typeArray = RuneType_RL.TypeArray;
		foreach (RuneType runeType in typeArray)
		{
			if (runeType != 0 && RuneManager.GetRuneEquippedLevel(runeType) > 0)
			{
				m_activeRuneHelper.Add(runeType);
			}
		}
		int count = m_activeRuneHelper.Count;
		if (count > m_runeEntryArray.Length)
		{
			int num = m_runeEntryArray.Length;
			m_runeEntryArray = ExpandGearCardArray(m_runeEntryArray, count + 5);
			int siblingIndex = m_noRunesGO.transform.GetSiblingIndex();
			for (int j = num; j < m_runeEntryArray.Length; j++)
			{
				GearCardEntry gearCardEntry = InstantiateGearCardEntry(m_leftSideContentGO.transform, siblingIndex);
				m_runeEntryArray[j] = gearCardEntry;
			}
		}
		for (int k = 0; k < m_runeEntryArray.Length; k++)
		{
			GearCardEntry gearCardEntry2 = m_runeEntryArray[k];
			if (k < count)
			{
				gearCardEntry2.SetAsRune(m_activeRuneHelper[k]);
				if (!gearCardEntry2.gameObject.activeSelf)
				{
					gearCardEntry2.gameObject.SetActive(value: true);
				}
			}
			else if (gearCardEntry2.gameObject.activeSelf)
			{
				gearCardEntry2.gameObject.SetActive(value: false);
			}
		}
		return count > 0;
	}

	private bool UpdateHeirloomEntries_V2()
	{
		m_activeHeirloomHelper.Clear();
		HeirloomType[] typeArray = HeirloomType_RL.TypeArray;
		foreach (HeirloomType heirloomType in typeArray)
		{
			if (heirloomType != 0 && SaveManager.PlayerSaveData.GetHeirloomLevel(heirloomType) > 0 && HeirloomLibrary.GetHeirloomData(heirloomType) != null)
			{
				m_activeHeirloomHelper.Add(heirloomType);
			}
		}
		int count = m_activeHeirloomHelper.Count;
		if (count > m_heirloomEntryArray.Length)
		{
			int num = m_heirloomEntryArray.Length;
			m_heirloomEntryArray = ExpandGearCardArray(m_heirloomEntryArray, count + 5);
			int siblingIndex = m_noHeirloomsGO.transform.GetSiblingIndex();
			for (int j = num; j < m_heirloomEntryArray.Length; j++)
			{
				GearCardEntry gearCardEntry = InstantiateGearCardEntry(m_rightSideContentGO.transform, siblingIndex);
				m_heirloomEntryArray[j] = gearCardEntry;
			}
		}
		for (int k = 0; k < m_heirloomEntryArray.Length; k++)
		{
			GearCardEntry gearCardEntry2 = m_heirloomEntryArray[k];
			if (k < count)
			{
				gearCardEntry2.SetAsHeirloom(m_activeHeirloomHelper[k]);
				if (!gearCardEntry2.gameObject.activeSelf)
				{
					gearCardEntry2.gameObject.SetActive(value: true);
				}
			}
			else if (gearCardEntry2.gameObject.activeSelf)
			{
				gearCardEntry2.gameObject.SetActive(value: false);
			}
		}
		return count > 0;
	}

	private bool UpdateRelicEntries_V2()
	{
		m_activeRelicHelper.Clear();
		RelicType[] typeArray = RelicType_RL.TypeArray;
		foreach (RelicType relicType in typeArray)
		{
			if (relicType != 0 && SaveManager.PlayerSaveData.GetRelic(relicType).Level > 0)
			{
				m_activeRelicHelper.Add(relicType);
			}
		}
		int count = m_activeRelicHelper.Count;
		if (count > m_relicEntryArray.Length)
		{
			int num = m_relicEntryArray.Length;
			m_relicEntryArray = ExpandGearCardArray(m_relicEntryArray, count + 5);
			int siblingIndex = m_noRelicsGO.transform.GetSiblingIndex();
			for (int j = num; j < m_relicEntryArray.Length; j++)
			{
				GearCardEntry gearCardEntry = InstantiateGearCardEntry(m_rightSideContentGO.transform, siblingIndex);
				m_relicEntryArray[j] = gearCardEntry;
			}
		}
		for (int k = 0; k < m_relicEntryArray.Length; k++)
		{
			GearCardEntry gearCardEntry2 = m_relicEntryArray[k];
			if (k < count)
			{
				gearCardEntry2.SetAsRelic(m_activeRelicHelper[k]);
				if (!gearCardEntry2.gameObject.activeSelf)
				{
					gearCardEntry2.gameObject.SetActive(value: true);
				}
			}
			else if (gearCardEntry2.gameObject.activeSelf)
			{
				gearCardEntry2.gameObject.SetActive(value: false);
			}
		}
		return count > 0;
	}

	private GearCardEntry[] ExpandGearCardArray(GearCardEntry[] arr, int size)
	{
		GearCardEntry[] array = new GearCardEntry[size];
		for (int i = 0; i < arr.Length; i++)
		{
			array[i] = arr[i];
		}
		return array;
	}

	protected void SelectScrollBar(bool selectLeft)
	{
		if (selectLeft)
		{
			m_selectedScrollBarInput = m_leftScrollBarInput;
			m_leftScrollArrow.gameObject.SetActive(value: true);
			m_rightScrollArrow.gameObject.SetActive(value: false);
			m_leftScrollBarInput.AssignButtonToScroll(Rewired_RL.WindowInputActionType.Window_Vertical);
			m_rightScrollBarInput.RemoveButtonToScroll(Rewired_RL.WindowInputActionType.Window_Vertical);
		}
		else
		{
			m_selectedScrollBarInput = m_rightScrollBarInput;
			m_rightScrollArrow.gameObject.SetActive(value: true);
			m_leftScrollArrow.gameObject.SetActive(value: false);
			m_leftScrollBarInput.RemoveButtonToScroll(Rewired_RL.WindowInputActionType.Window_Vertical);
			m_rightScrollBarInput.AssignButtonToScroll(Rewired_RL.WindowInputActionType.Window_Vertical);
		}
	}

	protected override void OnOpen()
	{
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.LanguageChanged, m_refreshText);
		UpdateArrays();
		SelectScrollBar(selectLeft: true);
		m_windowCanvas.gameObject.SetActive(value: true);
		StartCoroutine(RunOpenAnimation());
	}

	private IEnumerator RunOpenAnimation()
	{
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		m_leftCardCanvasGroup.alpha = 0f;
		m_rightCardCanvasGroup.alpha = 0f;
		float num = 50f;
		float duration = 0.15f;
		RectTransform component = m_leftCardCanvasGroup.GetComponent<RectTransform>();
		Vector3 vector = component.anchoredPosition;
		vector.y += num;
		component.anchoredPosition = vector;
		TweenManager.TweenBy_UnscaledTime(component, duration, Ease.Quad.EaseOut, "anchoredPosition.y", 0f - num);
		TweenManager.TweenTo_UnscaledTime(m_leftCardCanvasGroup, duration, Ease.None, "alpha", 1);
		RectTransform component2 = m_rightCardCanvasGroup.GetComponent<RectTransform>();
		Vector3 vector2 = component2.anchoredPosition;
		vector2.y += num;
		component2.anchoredPosition = vector2;
		TweenManager.TweenBy_UnscaledTime(component2, duration, Ease.Quad.EaseOut, "anchoredPosition.y", 0f - num);
		yield return TweenManager.TweenTo_UnscaledTime(m_rightCardCanvasGroup, duration, Ease.None, "alpha", 1).TweenCoroutine;
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
	}

	protected override void OnClose()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.LanguageChanged, m_refreshText);
		m_windowCanvas.gameObject.SetActive(value: false);
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
			base.RewiredPlayer.AddInputEventDelegate(m_onCancelButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
			base.RewiredPlayer.AddInputEventDelegate(m_onHorizontalInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Horizontal");
			base.RewiredPlayer.AddInputEventDelegate(m_onHorizontalInputHandler, UpdateLoopType.Update, InputActionEventType.NegativeButtonJustPressed, "Window_Horizontal");
		}
	}

	protected void RemoveInputListeners()
	{
		if (ReInput.isReady && base.RewiredPlayer != null)
		{
			base.RewiredPlayer.RemoveInputEventDelegate(m_onCancelButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
			base.RewiredPlayer.RemoveInputEventDelegate(m_onHorizontalInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Horizontal");
			base.RewiredPlayer.RemoveInputEventDelegate(m_onHorizontalInputHandler, UpdateLoopType.Update, InputActionEventType.NegativeButtonJustPressed, "Window_Horizontal");
		}
	}

	protected virtual void OnCancelButtonDown(InputActionEventData obj)
	{
		if (WindowManager.GetIsWindowOpen(WindowID.Pause))
		{
			WindowManager.CloseAllOpenWindows();
		}
		else
		{
			WindowManager.SetWindowIsOpen(ID, isOpen: false);
		}
	}

	private void OnHorizontalInputHandler(InputActionEventData eventData)
	{
		SelectScrollBar(m_selectedScrollBarInput != m_leftScrollBarInput);
	}

	public void RefreshText(object sender, EventArgs args)
	{
		UpdateArrays();
	}
}
