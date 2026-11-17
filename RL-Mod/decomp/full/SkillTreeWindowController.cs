using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using RL_Windows;
using Rewired;
using Rewired.Integration.UnityUI;
using SceneManagement_RL;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeWindowController : WindowController, ILocalizable
{
	public const int MULTIPLE_PURCHASE_COUNT = 5;

	[SerializeField]
	private SkillTreeSlot m_startingHighlightedButton;

	[SerializeField]
	private Image m_infoPlateIcon;

	[SerializeField]
	private GameObject m_castleParentObj;

	[SerializeField]
	private Animator m_castleAnimator;

	[SerializeField]
	private CanvasGroup m_skillTreeIconsCanvasGroup;

	[SerializeField]
	private CanvasGroup m_descriptionBoxCanvasGroup;

	[SerializeField]
	private RectTransform m_shopSignRectTransform;

	[SerializeField]
	private GameObject m_labourCostGO;

	[SerializeField]
	private TMP_Text m_labourCostText;

	[SerializeField]
	private TMP_Text m_levelText;

	[SerializeField]
	private TMP_Text m_affordableItemsText;

	[SerializeField]
	private GameObject m_navigationObj;

	private SkillTreeSlot m_selectedSkillTreeButton;

	private Dictionary<SkillTreeType, SkillTreeSlot> m_skillTreeButtonDict;

	private RewiredStandaloneInputModule m_standaloneInputModule;

	private Vector2 m_shopSignStartingPos;

	private bool m_shopSignDisplayed;

	private Coroutine m_currentAnimCoroutine;

	private bool m_skyIsReparenting;

	private bool m_transitionStateEnabled;

	private HashSet<SkillTreeType> m_availableSkillsSet;

	private Action<MonoBehaviour, EventArgs> m_onHighlightedSkillChanged;

	private Action<MonoBehaviour, EventArgs> m_onSkillLevelChanged;

	private Action<MonoBehaviour, EventArgs> m_refreshText;

	private Action m_cancelResetWarning;

	private Action<InputActionEventData> m_onCancelButtonPressed;

	private Action<InputActionEventData> m_onPurchaseButtonPressed;

	private Action<InputActionEventData> m_onToggleCastleViewPressed;

	private Action<InputActionEventData> m_onPurchaseMultipleButtonPressed;

	public static bool CastleViewEnabled;

	private Tween m_purchaseTween;

	public int SkillCount => m_availableSkillsSet.Count;

	public HashSet<SkillTreeType> AvailableSkills => m_availableSkillsSet;

	public GameObject SkillTreeCastleParentObj => m_castleParentObj;

	public override WindowID ID => WindowID.SkillTree;

	public Animator CastleAnimator
	{
		get
		{
			return m_castleAnimator;
		}
		private set
		{
			m_castleAnimator = value;
		}
	}

	private void Awake()
	{
		m_onHighlightedSkillChanged = OnHighlightedSkillChanged;
		m_refreshText = RefreshText;
		m_onSkillLevelChanged = OnSkillLevelChanged;
		m_cancelResetWarning = CancelResetWarning;
		m_onCancelButtonPressed = OnCancelButtonPressed;
		m_onPurchaseButtonPressed = OnPurchaseButtonPressed;
		m_onToggleCastleViewPressed = OnToggleCastleViewPressed;
		m_onPurchaseMultipleButtonPressed = OnPurchaseMultipleButtonPressed;
	}

	public override void Initialize()
	{
		m_standaloneInputModule = UnityEngine.Object.FindObjectOfType<RewiredStandaloneInputModule>();
		m_skillTreeButtonDict = new Dictionary<SkillTreeType, SkillTreeSlot>();
		List<SkillTreeSlot> list = new List<SkillTreeSlot>();
		int num = 1;
		SkillTreeSlot[] componentsInChildren = GetComponentsInChildren<SkillTreeSlot>();
		m_availableSkillsSet = new HashSet<SkillTreeType>();
		SkillTreeSlot[] array = componentsInChildren;
		foreach (SkillTreeSlot skillTreeSlot in array)
		{
			skillTreeSlot.Initialize(num, CastleAnimator);
			num++;
			if (!skillTreeSlot.HasData)
			{
				skillTreeSlot.gameObject.SetActive(value: false);
				continue;
			}
			if (m_skillTreeButtonDict.ContainsKey(skillTreeSlot.SkillTreeType))
			{
				throw new Exception("Duplicate SkillTreeType: " + skillTreeSlot.SkillTreeType.ToString() + " found.  Offending indices are: " + m_skillTreeButtonDict[skillTreeSlot.SkillTreeType].SlotIndex + " and " + num);
			}
			m_skillTreeButtonDict.Add(skillTreeSlot.SkillTreeType, skillTreeSlot);
			if (SkillTreeManager.GetSkillObjLevel(skillTreeSlot.SkillTreeType) > 0)
			{
				skillTreeSlot.gameObject.SetActive(value: true);
				list.Add(skillTreeSlot);
			}
			else
			{
				skillTreeSlot.gameObject.SetActive(value: false);
			}
			skillTreeSlot.UpdateSkillTreeType();
			if (skillTreeSlot.SkillTreeType != 0)
			{
				m_availableSkillsSet.Add(skillTreeSlot.SkillTreeType);
			}
		}
		foreach (SkillTreeSlot item in list)
		{
			item.UnlockConnectedSkillSlots();
		}
		m_shopSignStartingPos = m_shopSignRectTransform.anchoredPosition;
		base.Initialize();
	}

	protected override void OnFocus()
	{
		m_infoPlateIcon.sprite = IconLibrary.GetSkillTreeIcon(m_selectedSkillTreeButton.SkillTreeType);
		m_selectedSkillTreeButton.gameObject.SetActive(value: true);
		m_selectedSkillTreeButton.Select(dispatchSelectedEvent: false);
		AddListeners();
	}

	protected override void OnLostFocus()
	{
		RemoveListeners();
	}

	protected override void OnPause()
	{
	}

	protected override void OnUnpause()
	{
	}

	private void SetTraitsVisible(bool visible)
	{
		foreach (BaseTrait activeTrait in TraitManager.ActiveTraitList)
		{
			activeTrait.SetPaused(!visible);
			if (activeTrait.PostProcessOverrideController != null)
			{
				if (visible)
				{
					activeTrait.PostProcessOverrideController.enabled = true;
				}
				else
				{
					activeTrait.PostProcessOverrideController.enabled = false;
				}
			}
		}
	}

	public void EnableGardenTransitionState(bool enableTransitionState)
	{
		m_transitionStateEnabled = enableTransitionState;
		foreach (KeyValuePair<SkillTreeType, SkillTreeSlot> item in m_skillTreeButtonDict)
		{
			item.Value.Button.interactable = !enableTransitionState;
		}
		m_descriptionBoxCanvasGroup.gameObject.SetActive(!enableTransitionState);
		m_navigationObj.SetActive(!enableTransitionState);
		m_levelText.gameObject.SetActive(!enableTransitionState);
		m_labourCostGO.SetActive(!enableTransitionState);
		m_shopSignRectTransform.gameObject.SetActive(!enableTransitionState);
	}

	protected override void OnOpen()
	{
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.LanguageChanged, m_refreshText);
		StopAllStatusEffects();
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.HideEnemyHUD, null, null);
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SeenParade))
		{
			m_affordableItemsText.text = LocalizationManager.GetString("LOC_ID_SKILL_TREE_UI_SIGN_BUY_TOPSIDE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
		else
		{
			m_affordableItemsText.text = LocalizationManager.GetString("LOC_ID_SKILL_TREE_UI_SIGN_BUY_DOCKS_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
		m_windowCanvas.gameObject.SetActive(value: true);
		m_selectedSkillTreeButton = m_startingHighlightedButton;
		m_infoPlateIcon.sprite = IconLibrary.GetSkillTreeIcon(m_selectedSkillTreeButton.SkillTreeType);
		m_selectedSkillTreeButton.gameObject.SetActive(value: true);
		m_selectedSkillTreeButton.Select(dispatchSelectedEvent: false);
		SetTraitsVisible(visible: false);
		CastleViewEnabled = false;
		foreach (KeyValuePair<SkillTreeType, SkillTreeSlot> item in m_skillTreeButtonDict)
		{
			item.Value.Button.interactable = true;
		}
		m_skillTreeIconsCanvasGroup.alpha = 1f;
		m_descriptionBoxCanvasGroup.alpha = 1f;
		if (!m_transitionStateEnabled)
		{
			CinemachineVirtualCamera obj = CameraController.CinemachineBrain.ActiveVirtualCamera as CinemachineVirtualCamera;
			obj.GetComponent<CinemachineConfiner_RL>().enabled = false;
			obj.Follow = null;
			Vector3 position = obj.transform.position;
			position.x -= 50f;
			position.y += 50f;
			obj.transform.position = position;
		}
		else
		{
			Vector3 position2 = CameraController.GameCamera.transform.position;
			position2.x = -100f;
			position2.y += 100f;
			CameraController.GameCamera.transform.position = position2;
			RewiredMapController.SetCurrentMapEnabled(enabled: false);
		}
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.SkillTree_Opened, this, null);
		EffectTriggerAnimBehaviour.DISABLE_GLOBALLY = true;
		RefreshAllButtons(updateAnimParams: true);
		m_skillTreeIconsCanvasGroup.gameObject.SetActive(!m_transitionStateEnabled);
		CastleAnimator.Update(1f);
		CastleAnimator.Update(1f);
		StartCoroutine(ReparentSkillTreeToSky());
		m_shopSignDisplayed = false;
		Vector2 shopSignStartingPos = m_shopSignStartingPos;
		shopSignStartingPos.y -= 500f;
		m_shopSignRectTransform.anchoredPosition = shopSignStartingPos;
		UpdateShopSign();
		if (!m_transitionStateEnabled)
		{
			UpdateLabourCosts();
		}
		UpdateLevelText();
		if (HasAllSkills())
		{
			StoreAPIManager.GiveAchievement(AchievementType.AllSkills, StoreType.All);
		}
	}

	public bool HasAllSkills()
	{
		bool result = true;
		foreach (SkillTreeType item in m_availableSkillsSet)
		{
			if (SkillTreeManager.GetSkillObjLevel(item) <= 0)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	private void StopAllStatusEffects()
	{
		BaseRoom currentPlayerRoom = PlayerManager.GetCurrentPlayerRoom();
		if (!currentPlayerRoom)
		{
			return;
		}
		EnemySpawnController[] enemySpawnControllers = currentPlayerRoom.SpawnControllerManager.EnemySpawnControllers;
		foreach (EnemySpawnController enemySpawnController in enemySpawnControllers)
		{
			if ((bool)enemySpawnController.EnemyInstance)
			{
				enemySpawnController.EnemyInstance.StatusEffectController.StopAllStatusEffects(interrupted: false);
			}
		}
	}

	private void UpdateLabourCosts()
	{
		if (SkillTreeManager.GetTotalSkillObjLevel() > 18)
		{
			if (!m_labourCostGO.activeSelf)
			{
				m_labourCostGO.SetActive(value: true);
			}
			int num = Mathf.Clamp(SkillTreeManager.GetTotalSkillObjLevel() - 18, 0, int.MaxValue);
			num = (int)(Math.Floor((float)num * 14f / 5f) * 5.0);
			m_labourCostText.text = num.ToString();
		}
		else if (m_labourCostGO.activeSelf)
		{
			m_labourCostGO.SetActive(value: false);
		}
	}

	private void UpdateLevelText()
	{
		int totalSkillObjLevel = SkillTreeManager.GetTotalSkillObjLevel();
		m_levelText.text = string.Format(LocalizationManager.GetString("LOC_ID_HUD_TITLE_CHARACTER_LEVEL_1", isFemale: false), totalSkillObjLevel.ToString());
	}

	private void UpdateShopSign()
	{
		if (m_transitionStateEnabled)
		{
			return;
		}
		bool flag = false;
		if (SkillTreeManager.GetSkillObjLevel(SkillTreeType.Smithy) > 0)
		{
			EquipmentCategoryType[] categoryTypeArray = EquipmentType_RL.CategoryTypeArray;
			foreach (EquipmentCategoryType equipmentCategoryType in categoryTypeArray)
			{
				if (equipmentCategoryType == EquipmentCategoryType.None)
				{
					continue;
				}
				EquipmentType[] typeArray = EquipmentType_RL.TypeArray;
				foreach (EquipmentType equipmentType in typeArray)
				{
					if (equipmentType != 0)
					{
						if (EquipmentManager.CanPurchaseEquipment(equipmentCategoryType, equipmentType))
						{
							flag = true;
							break;
						}
						if (flag)
						{
							break;
						}
					}
				}
			}
		}
		if (!flag && SkillTreeManager.GetSkillObjLevel(SkillTreeType.Enchantress) > 0)
		{
			RuneType[] typeArray2 = RuneType_RL.TypeArray;
			foreach (RuneType runeType in typeArray2)
			{
				if (runeType != 0 && RuneManager.CanPurchaseRune(runeType))
				{
					flag = true;
					break;
				}
			}
		}
		TweenManager.StopAllTweensContaining(m_shopSignRectTransform, runEndHandlers: false);
		if (flag && !m_shopSignDisplayed)
		{
			m_shopSignDisplayed = true;
			Vector2 shopSignStartingPos = m_shopSignStartingPos;
			shopSignStartingPos.y -= 500f;
			m_shopSignRectTransform.anchoredPosition = shopSignStartingPos;
			TweenManager.TweenTo(m_shopSignRectTransform, 0.25f, Ease.Back.EaseOutSmall, "anchoredPosition.y", m_shopSignStartingPos.y);
		}
		else if (!flag && m_shopSignDisplayed)
		{
			m_shopSignDisplayed = false;
			Vector2 shopSignStartingPos2 = m_shopSignStartingPos;
			m_shopSignRectTransform.anchoredPosition = shopSignStartingPos2;
			TweenManager.TweenTo(m_shopSignRectTransform, 0.25f, Ease.Back.EaseInSmall, "anchoredPosition.y", m_shopSignStartingPos.y - 500f);
		}
	}

	private IEnumerator ReparentSkillTreeToSky()
	{
		m_skyIsReparenting = true;
		float startTime = Time.time;
		while (Time.time < startTime + 0.5f)
		{
			yield return null;
		}
		Sky sky = UnityEngine.Object.FindObjectOfType<Sky>();
		if ((bool)sky)
		{
			CastleAnimator.gameObject.transform.SetParent(sky.transform, worldPositionStays: true);
			Vector3 localScale = CastleAnimator.gameObject.transform.localScale;
			localScale.z = 1f;
			CastleAnimator.gameObject.transform.localScale = localScale;
			Vector3 localPosition = CastleAnimator.gameObject.transform.localPosition;
			localPosition.z += 1f;
			CastleAnimator.gameObject.transform.localPosition = localPosition;
		}
		else
		{
			Debug.Log("Failed to parent skilltree to sky.");
		}
		m_skyIsReparenting = false;
	}

	protected override void OnClose()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.LanguageChanged, m_refreshText);
		SetTraitsVisible(visible: true);
		if (!m_transitionStateEnabled)
		{
			CinemachineVirtualCamera obj = CameraController.CinemachineBrain.ActiveVirtualCamera as CinemachineVirtualCamera;
			obj.GetComponent<CinemachineConfiner_RL>().enabled = true;
			obj.Follow = PlayerManager.GetPlayerController().FollowTargetGO.transform;
		}
		CastleAnimator.gameObject.transform.localPosition = Vector3.zero;
		CastleAnimator.gameObject.transform.SetParent(SkillTreeCastleParentObj.transform, worldPositionStays: false);
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.SkillTree_Closed, this, null);
		m_windowCanvas.gameObject.SetActive(value: false);
		if (m_currentAnimCoroutine != null)
		{
			StopCoroutine(m_currentAnimCoroutine);
		}
		EffectTriggerAnimBehaviour.DISABLE_GLOBALLY = false;
		SaveManager.PlayerSaveData.UpdateCachedData();
	}

	private void AddListeners()
	{
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.SkillTree_HighlightedSkillChanged, m_onHighlightedSkillChanged);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.SkillLevelChanged, m_onSkillLevelChanged);
		base.RewiredPlayer.AddInputEventDelegate(m_onCancelButtonPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
		base.RewiredPlayer.AddInputEventDelegate(m_onPurchaseButtonPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
		base.RewiredPlayer.AddInputEventDelegate(m_onToggleCastleViewPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Y");
		base.RewiredPlayer.AddInputEventDelegate(m_onPurchaseMultipleButtonPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_X");
	}

	private void RemoveListeners()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.SkillTree_HighlightedSkillChanged, m_onHighlightedSkillChanged);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.SkillLevelChanged, m_onSkillLevelChanged);
		base.RewiredPlayer.RemoveInputEventDelegate(m_onCancelButtonPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
		base.RewiredPlayer.RemoveInputEventDelegate(m_onPurchaseButtonPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
		base.RewiredPlayer.RemoveInputEventDelegate(m_onToggleCastleViewPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Y");
		base.RewiredPlayer.RemoveInputEventDelegate(m_onPurchaseMultipleButtonPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_X");
	}

	private void OnHighlightedSkillChanged(MonoBehaviour sender, EventArgs args)
	{
		HighlightedSkillChangedEventArgs highlightedSkillChangedEventArgs = args as HighlightedSkillChangedEventArgs;
		if (highlightedSkillChangedEventArgs.SkillTreeType == SkillTreeType.None)
		{
			return;
		}
		SkillTreeSlot skillTreeSlot = m_skillTreeButtonDict[highlightedSkillChangedEventArgs.SkillTreeType];
		if (m_selectedSkillTreeButton != skillTreeSlot)
		{
			m_selectedSkillTreeButton = skillTreeSlot;
			SkillTreeObj skillTreeObj = SkillTreeManager.GetSkillTreeObj(highlightedSkillChangedEventArgs.SkillTreeType);
			if (skillTreeObj.IsSoulLocked)
			{
				m_infoPlateIcon.sprite = IconLibrary.GetSkillTreeSoulLockedIcon();
			}
			else if (skillTreeObj.IsLocked)
			{
				m_infoPlateIcon.sprite = IconLibrary.GetSkillTreeLockedIcon();
			}
			else
			{
				m_infoPlateIcon.sprite = IconLibrary.GetSkillTreeIcon(highlightedSkillChangedEventArgs.SkillTreeType);
			}
		}
	}

	private void OnToggleCastleViewPressed(InputActionEventData eventData)
	{
		CastleViewEnabled = !CastleViewEnabled;
		if (!CastleViewEnabled)
		{
			foreach (KeyValuePair<SkillTreeType, SkillTreeSlot> item in m_skillTreeButtonDict)
			{
				item.Value.Button.interactable = true;
			}
			m_skillTreeIconsCanvasGroup.alpha = 1f;
			m_descriptionBoxCanvasGroup.alpha = 1f;
			m_selectedSkillTreeButton.Select(dispatchSelectedEvent: false);
			return;
		}
		foreach (KeyValuePair<SkillTreeType, SkillTreeSlot> item2 in m_skillTreeButtonDict)
		{
			item2.Value.Button.interactable = false;
		}
		m_skillTreeIconsCanvasGroup.alpha = 0f;
		m_descriptionBoxCanvasGroup.alpha = 0f;
	}

	private void OnPurchaseButtonPressed(InputActionEventData eventData)
	{
		if (!eventData.IsCurrentInputSource(ControllerType.Mouse) && !CastleViewEnabled)
		{
			m_selectedSkillTreeButton.PurchaseSkillUpgrade(1);
		}
	}

	private void OnPurchaseMultipleButtonPressed(InputActionEventData eventData)
	{
		if (!eventData.IsCurrentInputSource(ControllerType.Mouse) && !CastleViewEnabled)
		{
			m_selectedSkillTreeButton.PurchaseSkillUpgrade(5);
		}
	}

	private void OnSkillLevelChanged(object sender, EventArgs args)
	{
		if (args == null)
		{
			return;
		}
		UpdateShopSign();
		UpdateLabourCosts();
		UpdateLevelText();
		SkillLevelChangedEventArgs skillLevelChangedEventArgs = args as SkillLevelChangedEventArgs;
		if (skillLevelChangedEventArgs.PrevLevel >= skillLevelChangedEventArgs.NewLevel)
		{
			return;
		}
		SkillTreeSlot skillTreeSlot = m_skillTreeButtonDict[skillLevelChangedEventArgs.SkillTreeType];
		skillTreeSlot.UnlockConnectedSkillSlots();
		RefreshAllButtons(updateAnimParams: false);
		if (!skillTreeSlot.HasAnimated)
		{
			if (SkillTreePopupLibrary.GetPopupData(skillLevelChangedEventArgs.SkillTreeType) != null)
			{
				if (m_currentAnimCoroutine != null)
				{
					StopCoroutine(m_currentAnimCoroutine);
				}
				m_currentAnimCoroutine = StartCoroutine(DisplaySkillTreePopUp(skillLevelChangedEventArgs.SkillTreeType, skillTreeSlot));
				return;
			}
			if (m_currentAnimCoroutine != null)
			{
				StopCoroutine(m_currentAnimCoroutine);
			}
			m_currentAnimCoroutine = StartCoroutine(AnimateBGImageCoroutine(skillTreeSlot));
			RunPurchaseAnim(skillTreeSlot);
		}
		else
		{
			RunPurchaseAnim(skillTreeSlot);
			StartCoroutine(UnlockLabourCostAnimCoroutine());
		}
	}

	private void RunPurchaseAnim(SkillTreeSlot slot)
	{
		BaseEffect baseEffect = EffectManager.PlayEffect(slot.gameObject, null, "Purchase_Effect", Vector3.zero, 0f, EffectStopType.Gracefully);
		baseEffect.transform.SetParent(slot.transform, worldPositionStays: false);
		baseEffect.transform.SetParent(null, worldPositionStays: true);
		if (m_purchaseTween != null)
		{
			m_purchaseTween.StopTweenWithConditionChecks(runEndHandlers: false, slot.transform);
		}
		Vector3 storedScale = slot.StoredScale;
		slot.transform.localScale = storedScale + new Vector3(0.1f, 0.1f, 0.1f);
		m_purchaseTween = TweenManager.TweenTo_UnscaledTime(slot.transform, 0.25f, Ease.Back.EaseOut, "localScale.x", storedScale.x, "localScale.y", storedScale.y, "localScale.z", storedScale.z);
	}

	private IEnumerator AnimateBGImageCoroutine(SkillTreeSlot changedSkillSlot)
	{
		EffectTriggerAnimBehaviour.DISABLE_GLOBALLY = false;
		if (!m_transitionStateEnabled)
		{
			RewiredMapController.SetCurrentMapEnabled(enabled: false);
		}
		m_standaloneInputModule.allowMouseInput = false;
		if (changedSkillSlot.HasAnimParam)
		{
			yield return TweenManager.TweenTo(m_skillTreeIconsCanvasGroup, 0.1f, Ease.None, "alpha", 0).TweenCoroutine;
		}
		yield return changedSkillSlot.AnimateBGImage();
		if (!m_transitionStateEnabled)
		{
			RewiredMapController.SetCurrentMapEnabled(enabled: true);
		}
		m_standaloneInputModule.allowMouseInput = true;
		if (changedSkillSlot.HasAnimParam)
		{
			yield return TweenManager.TweenTo(m_skillTreeIconsCanvasGroup, 0.25f, Ease.None, "alpha", 1).TweenCoroutine;
		}
		EffectTriggerAnimBehaviour.DISABLE_GLOBALLY = true;
		yield return UnlockLabourCostAnimCoroutine();
	}

	private void OnCancelButtonPressed(InputActionEventData eventData)
	{
		if (!m_skyIsReparenting)
		{
			WindowManager.SetWindowIsOpen(WindowID.SkillTree, isOpen: false, TransitionID.QuickSwipe);
		}
	}

	private IEnumerator DisplaySkillTreePopUp(SkillTreeType skillTreeType, SkillTreeSlot changedSkillSlot)
	{
		if (!WindowManager.GetIsWindowLoaded(WindowID.SkillTreePopUp))
		{
			WindowManager.LoadWindow(WindowID.SkillTreePopUp);
		}
		if (!m_transitionStateEnabled)
		{
			RewiredMapController.SetCurrentMapEnabled(enabled: false);
		}
		m_standaloneInputModule.allowMouseInput = false;
		RunPurchaseAnim(changedSkillSlot);
		float startTime = Time.time;
		while (Time.time < startTime + 0.5f)
		{
			yield return null;
		}
		m_standaloneInputModule.allowMouseInput = true;
		(WindowManager.GetWindowController(WindowID.SkillTreePopUp) as SkillTreePopUpWindowController).SetPopupType(skillTreeType);
		WindowManager.SetWindowIsOpen(WindowID.SkillTreePopUp, isOpen: true);
		while (WindowManager.ActiveWindow != this)
		{
			yield return null;
		}
		EffectTriggerAnimBehaviour.DISABLE_GLOBALLY = false;
		if (!m_transitionStateEnabled)
		{
			RewiredMapController.SetCurrentMapEnabled(enabled: false);
		}
		m_standaloneInputModule.allowMouseInput = false;
		if (changedSkillSlot.HasAnimParam)
		{
			yield return TweenManager.TweenTo(m_skillTreeIconsCanvasGroup, 0.1f, Ease.None, "alpha", 0).TweenCoroutine;
		}
		yield return changedSkillSlot.AnimateBGImage();
		if (!m_transitionStateEnabled)
		{
			RewiredMapController.SetCurrentMapEnabled(enabled: true);
		}
		m_standaloneInputModule.allowMouseInput = true;
		if (changedSkillSlot.HasAnimParam)
		{
			yield return TweenManager.TweenTo(m_skillTreeIconsCanvasGroup, 0.25f, Ease.None, "alpha", 1).TweenCoroutine;
		}
		EffectTriggerAnimBehaviour.DISABLE_GLOBALLY = true;
		yield return UnlockLabourCostAnimCoroutine();
	}

	private IEnumerator UnlockLabourCostAnimCoroutine()
	{
		if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.LabourCostsUnlocked) && SkillTreeManager.GetTotalSkillObjLevel() > 18)
		{
			SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.LabourCostsUnlocked, value: true);
			if (!WindowManager.GetIsWindowLoaded(WindowID.SkillTreePopUp))
			{
				WindowManager.LoadWindow(WindowID.SkillTreePopUp);
			}
			(WindowManager.GetWindowController(WindowID.SkillTreePopUp) as SkillTreePopUpWindowController).SetPopupType(SkillTreeType.LabourCosts_Unlocked);
			WindowManager.SetWindowIsOpen(WindowID.SkillTreePopUp, isOpen: true);
			while (WindowManager.ActiveWindow != this)
			{
				yield return null;
			}
		}
		UpdateLabourCosts();
	}

	private void RunFarShoresResetWarning()
	{
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.PlayFarShoresWarning))
		{
			SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.PlayFarShoresWarning, value: false);
			if (!WindowManager.GetIsWindowLoaded(WindowID.ConfirmMenu))
			{
				WindowManager.LoadWindow(WindowID.ConfirmMenu);
			}
			ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenu) as ConfirmMenuWindowController;
			obj.SetButtonDelayTime(3f);
			obj.SetTitleText("Far Shores Content Reset", isLocID: false);
			obj.SetDescriptionText("Our very first content release has arrived, and with it a ton of changes.  To keep things in check, all skills and gear have been reset. However, the gold spent has been refunded, and the discovery of all gear is preserved.\n\n<size=125%>SPEND YOUR GOLD!!!</size>", isLocID: false);
			obj.SetNumberOfButtons(1);
			obj.SetOnCancelAction(m_cancelResetWarning);
			ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
			buttonAtIndex.SetButtonText("Aww nuts...", isLocID: false);
			buttonAtIndex.SetOnClickAction(m_cancelResetWarning);
			WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: true);
		}
	}

	private void RunArcaneHallowsResetWarning()
	{
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.PlayArcaneHallowsWarning))
		{
			SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.PlayArcaneHallowsWarning, value: false);
			if (!WindowManager.GetIsWindowLoaded(WindowID.ConfirmMenu))
			{
				WindowManager.LoadWindow(WindowID.ConfirmMenu);
			}
			ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenu) as ConfirmMenuWindowController;
			obj.SetButtonDelayTime(3f);
			obj.SetTitleText("Arcane Hallows Content Reset", isLocID: false);
			obj.SetDescriptionText("The second content release is out, and once again the changes are massive.\n\nTo keep things in check, all skills have been reset. But fret not!  All gold spent on skills has also been refunded.\n\nEverything else has been preserved.", isLocID: false);
			obj.SetNumberOfButtons(1);
			obj.SetOnCancelAction(m_cancelResetWarning);
			ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
			buttonAtIndex.SetButtonText("SPEND YOUR GOLD!!!", isLocID: false);
			buttonAtIndex.SetOnClickAction(m_cancelResetWarning);
			WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: true);
		}
	}

	private void RunDriftingWorldsResetWarning()
	{
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.PlayDriftingWorldsWarning))
		{
			SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.PlayDriftingWorldsWarning, value: false);
			if (!WindowManager.GetIsWindowLoaded(WindowID.ConfirmMenu))
			{
				WindowManager.LoadWindow(WindowID.ConfirmMenu);
			}
			ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenu) as ConfirmMenuWindowController;
			obj.SetButtonDelayTime(3f);
			obj.SetTitleText("Drifting Worlds Content Reset", isLocID: false);
			obj.SetDescriptionText("A new content patch is out, which means once again equipped runes, gear, and all skills are reset. The majority of gold spent has been refunded, and the discovery of all gear and runes is preserved.", isLocID: false);
			obj.SetNumberOfButtons(1);
			obj.SetOnCancelAction(m_cancelResetWarning);
			ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
			buttonAtIndex.SetButtonText("SPEND YOUR GOLD!!!", isLocID: false);
			buttonAtIndex.SetOnClickAction(m_cancelResetWarning);
			WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: true);
		}
	}

	private void RunPizzaMundiResetWarning()
	{
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.PlayPizzaMundiWarning))
		{
			SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.PlayPizzaMundiWarning, value: false);
			if (!WindowManager.GetIsWindowLoaded(WindowID.ConfirmMenu))
			{
				WindowManager.LoadWindow(WindowID.ConfirmMenu);
			}
			ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenu) as ConfirmMenuWindowController;
			obj.SetButtonDelayTime(3f);
			obj.SetTitleText("Pizza Mundi Content Reset", isLocID: false);
			obj.SetDescriptionText("!!! IMPORTANT !!! \nLeveling up Runes and Gear must now be unlocked in the new Soul Shop (this was our original vision for the game).\n\nALL EQUIPMENT HAS BEEN UNEQUIPPED BUT NOT LOST.", isLocID: false);
			obj.SetNumberOfButtons(1);
			obj.SetOnCancelAction(m_cancelResetWarning);
			ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
			buttonAtIndex.SetButtonText("JUST GIVE ME PIZZA!", isLocID: false);
			buttonAtIndex.SetOnClickAction(m_cancelResetWarning);
			WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: true);
		}
	}

	private void RunDragonsVowResetWarning()
	{
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.PlayDragonsVowWarning))
		{
			SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.PlayDragonsVowWarning, value: false);
			if (!WindowManager.GetIsWindowLoaded(WindowID.ConfirmMenu))
			{
				WindowManager.LoadWindow(WindowID.ConfirmMenu);
			}
			ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenu) as ConfirmMenuWindowController;
			obj.SetButtonDelayTime(5f);
			obj.SetTitleText("Dragon's Vow Content Reset", isLocID: false);
			obj.SetDescriptionText("This is a BIG balance patch, so all Gear and Runes have been unequipped.\n\nThe Skill Tree and Soul Shop have also been reset, with their resources refunded.\n\nDon't forget!  You can now purchase 5 skills at a time in the Skill Tree.", isLocID: false);
			obj.SetNumberOfButtons(1);
			obj.SetOnCancelAction(m_cancelResetWarning);
			ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
			buttonAtIndex.SetButtonText("START EXPLORING!", isLocID: false);
			buttonAtIndex.SetOnClickAction(m_cancelResetWarning);
			WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: true);
		}
	}

	private void CancelResetWarning()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
	}

	private void RefreshAllButtons(bool updateAnimParams)
	{
		foreach (KeyValuePair<SkillTreeType, SkillTreeSlot> item in m_skillTreeButtonDict)
		{
			if (item.Value.isActiveAndEnabled && item.Value.HasData)
			{
				item.Value.UpdateSkillTreeType();
				item.Value.RefreshSlotState(updateAnimParams);
				item.Value.UpdateNavigationNodes();
			}
		}
	}

	public void ForceUpdateSkillTreeAnimatorParams()
	{
		foreach (KeyValuePair<SkillTreeType, SkillTreeSlot> item in m_skillTreeButtonDict)
		{
			if (item.Value.HasData)
			{
				item.Value.UpdateAnimatorParams();
			}
		}
	}

	public void RefreshText(object sender, EventArgs args)
	{
		UpdateLevelText();
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SeenParade))
		{
			m_affordableItemsText.text = LocalizationManager.GetString("LOC_ID_SKILL_TREE_UI_SIGN_BUY_TOPSIDE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
		else
		{
			m_affordableItemsText.text = LocalizationManager.GetString("LOC_ID_SKILL_TREE_UI_SIGN_BUY_DOCKS_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
	}
}
