using System;
using System.Collections;
using RLAudio;
using RL_Windows;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class BaseOmniUIWindowController<T, U> : WindowController, IOmniUIWindowController where T : BaseOmniUICategoryEntry where U : BaseOmniUIEntry
{
	[Header("Prefabs")]
	[SerializeField]
	protected bool m_hasCategories;

	[SerializeField]
	[ConditionalHide("m_hasCategories", true)]
	protected T m_categoryEntryPrefab;

	[SerializeField]
	protected U m_entryPrefab;

	[Header("Common Serialized Fields")]
	[SerializeField]
	protected OmniUICommonSerializedFields m_commonFields;

	[SerializeField]
	protected UnityEvent m_changeSelectedOptionEvent;

	[SerializeField]
	protected UnityEvent m_selectOptionEvent;

	protected static Material m_blurMaterial_STATIC;

	protected float m_entryHeight;

	protected bool m_warningMessageVisible;

	protected bool m_updateScrollBar;

	private bool m_keyboardEnabled;

	private bool m_entryNavigationEnabled;

	private Vector3 m_storedWarningBoxScale;

	private PurchaseBoxDialogueEventArgs m_purchaseDialogueArgs = new PurchaseBoxDialogueEventArgs(PurchaseBoxDialogueType.Welcome);

	private Action<MonoBehaviour, EventArgs> m_refreshText;

	private Action<MonoBehaviour, EventArgs> m_updateAllStates;

	private Action<InputActionEventData> m_onConfirmInputHandler;

	private Action<InputActionEventData> m_onCancelInputHandler;

	private Action<InputActionEventData> m_onHorizontalInputHandler;

	private Action<InputActionEventData> m_onVerticalInputHandler;

	private Action<InputActionEventData> m_onLBInputHandler;

	private Action<InputActionEventData> m_onRBInputHandler;

	private Action<InputActionEventData> m_onYButtonResetHandler;

	private Action<InputActionEventData> m_onXButtonInputHandler;

	private Tween m_scrollTween;

	public UnityEvent SelectOptionEvent => m_selectOptionEvent;

	public OmniUICommonSerializedFields CommonFields => m_commonFields;

	protected GameObject ResetTextbox => m_commonFields.ResetTextbox;

	protected CanvasGroup MenuCanvasGroup => m_commonFields.MenuCanvasGroup;

	protected PlayerLookController PlayerModel => m_commonFields.PlayerModel;

	protected GameObject NPCModel => m_commonFields.NPCModel;

	protected NPCController NPCController => m_commonFields.NPCController;

	protected VerticalLayoutGroup CategoryEntryLayoutGroup => m_commonFields.CategoryEntryLayoutGroup;

	protected VerticalLayoutGroup EntryLayoutGroup => m_commonFields.EntryLayoutGroup;

	protected GameObject SelectedCategoryIndicator => m_commonFields.SelectedCategoryIndicator;

	protected TMP_Text ChooseCategoryText => m_commonFields.ChooseCategoryText;

	protected CanvasGroup DescriptionBox => m_commonFields.DescriptionBox;

	protected CanvasGroup PurchaseBox => m_commonFields.PurchaseBox;

	protected Scrollbar ScrollBar => m_commonFields.ScrollBar;

	protected ScrollRect ScrollRect => m_commonFields.ScrollRect;

	protected RectTransform ContentViewport => m_commonFields.ContentViewport;

	protected GameObject TopScrollArrow => m_commonFields.TopScrollArrow;

	protected Image TopScrollNewSymbol => m_commonFields.TopScrollNewSymbol;

	protected Image TopScrollUpgradeSymbol => m_commonFields.TopScrollUpgradeSymbol;

	protected GameObject BottomScrollArrow => m_commonFields.BottomScrollArrow;

	protected Image BottomScrollNewSymbol => m_commonFields.BottomScrollNewSymbol;

	protected Image BottomScrollUpgradeSymbol => m_commonFields.BottomScrollUpgradeSymbol;

	protected GameObject WarningMessageBox => m_commonFields.WarningMessageBox;

	protected CanvasGroup BackgroundCanvasGroup => m_commonFields.BackgroundCanvasGroup;

	public virtual bool CanReset => false;

	public virtual bool CanExit => true;

	public bool IsInCategories
	{
		get
		{
			if (!m_hasCategories)
			{
				return false;
			}
			return SelectedCategoryIndex == -1;
		}
	}

	public T[] CategoryEntryArray { get; set; }

	public U[] EntryArray { get; set; }

	public T[] ActiveCategoryEntryArray { get; protected set; }

	public U[] ActiveEntryArray { get; protected set; }

	public int SelectedEntryIndex { get; protected set; }

	public int SelectedCategoryIndex { get; protected set; }

	public int HighlightedCategoryIndex { get; protected set; }

	public override WindowID ID => WindowID.None;

	protected virtual void Awake()
	{
		Image componentInChildren = BackgroundCanvasGroup.GetComponentInChildren<Image>();
		if (m_blurMaterial_STATIC == null)
		{
			m_blurMaterial_STATIC = UnityEngine.Object.Instantiate(componentInChildren.material);
		}
		componentInChildren.material = m_blurMaterial_STATIC;
		m_refreshText = RefreshText;
		m_updateAllStates = UpdateAllStates;
		m_onConfirmInputHandler = OnConfirmInputHandler;
		m_onCancelInputHandler = OnCancelInputHandler;
		m_onHorizontalInputHandler = OnHorizontalInputHandler;
		m_onVerticalInputHandler = OnVerticalInputHandler;
		m_onLBInputHandler = OnLBInputHandler;
		m_onRBInputHandler = OnRBInputHandler;
		m_onYButtonResetHandler = OnYButtonResetHandler;
		m_onXButtonInputHandler = OnXButtonInputHandler;
	}

	public void SetEntryNavigationEnabled(bool enable)
	{
		U[] activeEntryArray = ActiveEntryArray;
		for (int i = 0; i < activeEntryArray.Length; i++)
		{
			activeEntryArray[i].Interactable = enable;
		}
		m_entryNavigationEnabled = enable;
		ScrollRect.vertical = enable;
		ScrollBar.interactable = enable;
	}

	public void SetKeyboardEnabled(bool enable)
	{
		m_keyboardEnabled = enable;
	}

	public void SetSelectedEntryIndex(int index, bool playAudio, bool usingMouse)
	{
		if (ActiveEntryArray.Length != 0 && SelectedEntryIndex != index)
		{
			if (SelectedEntryIndex > -1 && SelectedEntryIndex < ActiveEntryArray.Length)
			{
				ActiveEntryArray[SelectedEntryIndex].OnDeselect(null);
			}
			bool scrollingUp = index < SelectedEntryIndex;
			if (SelectedEntryIndex == -1)
			{
				scrollingUp = true;
			}
			SelectedEntryIndex = index;
			if (SelectedEntryIndex > -1 && SelectedEntryIndex < ActiveEntryArray.Length)
			{
				ActiveEntryArray[SelectedEntryIndex].OnSelect(null);
				AutoScroll(scrollingUp, usingMouse);
			}
			else
			{
				ScrollBar.value = 1f;
			}
			if (!ActiveEntryArray[SelectedEntryIndex].IsEntryActive)
			{
				m_purchaseDialogueArgs.Initialize(PurchaseBoxDialogueType.GearNotFound);
			}
			else
			{
				m_purchaseDialogueArgs.Initialize(PurchaseBoxDialogueType.GearDescription);
			}
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.UpdatePurchaseBoxDialogue, this, m_purchaseDialogueArgs);
			if (playAudio && m_changeSelectedOptionEvent != null)
			{
				m_changeSelectedOptionEvent.Invoke();
			}
		}
	}

	protected virtual void AutoScroll(bool scrollingUp, bool usingMouse, bool skipAnimation = false)
	{
		GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		if ((bool)currentSelectedGameObject && (bool)currentSelectedGameObject.GetComponent<Scrollbar>())
		{
			EventSystem.current.SetSelectedGameObject(null);
		}
		float num = (float)ActiveEntryArray.Length * m_entryHeight;
		float num2 = 1f - ScrollBar.value;
		float height = ContentViewport.rect.height;
		if (height == 0f)
		{
			ScrollBar.value = 1f;
			return;
		}
		float num3 = height;
		float num4 = num3 - height;
		if (num2 > 0f)
		{
			num3 = height + (num - height) * num2;
			num4 = num3 - height;
		}
		float num5 = (float)(SelectedEntryIndex + 1) * m_entryHeight;
		float num6 = num5 - m_entryHeight;
		float duration = 0.05f;
		if (usingMouse)
		{
			if (num6 < num4)
			{
				float value = 1f - num6 / (num - height);
				value = Mathf.Clamp(value, 0f, 1f);
				if (!skipAnimation)
				{
					if (m_scrollTween != null)
					{
						m_scrollTween.StopTweenWithConditionChecks(runEndHandlers: false, ScrollBar, "ScrollTween");
					}
					m_scrollTween = TweenManager.TweenTo_UnscaledTime(ScrollBar, duration, Ease.Quad.EaseInOut, "value", value);
					m_scrollTween.ID = "ScrollTween";
				}
				else
				{
					ScrollBar.value = value;
				}
			}
			else
			{
				if (!(num5 > num3))
				{
					return;
				}
				float value2 = 1f - (num5 - height) / (num - height);
				value2 = Mathf.Clamp(value2, 0f, 1f);
				if (!skipAnimation)
				{
					if (m_scrollTween != null)
					{
						m_scrollTween.StopTweenWithConditionChecks(runEndHandlers: false, ScrollBar, "ScrollTween");
					}
					m_scrollTween = TweenManager.TweenTo_UnscaledTime(ScrollBar, duration, Ease.Quad.EaseInOut, "value", value2);
					m_scrollTween.ID = "ScrollTween";
				}
				else
				{
					ScrollBar.value = value2;
				}
			}
		}
		else if (num6 < num4 + height / 2f && scrollingUp)
		{
			float value3 = 1f - (num5 - height / 2f) / (num - height);
			value3 = Mathf.Clamp(value3, 0f, 1f);
			if (!skipAnimation)
			{
				if (m_scrollTween != null)
				{
					m_scrollTween.StopTweenWithConditionChecks(runEndHandlers: false, ScrollBar, "ScrollTween");
				}
				m_scrollTween = TweenManager.TweenTo_UnscaledTime(ScrollBar, duration, Ease.Quad.EaseInOut, "value", value3);
				m_scrollTween.ID = "ScrollTween";
			}
			else
			{
				ScrollBar.value = value3;
			}
		}
		else
		{
			if (!(num5 > num3 - height / 2f) || scrollingUp)
			{
				return;
			}
			float value4 = 1f - (num5 - height / 2f) / (num - height);
			value4 = Mathf.Clamp(value4, 0f, 1f);
			if (!skipAnimation)
			{
				if (m_scrollTween != null)
				{
					m_scrollTween.StopTweenWithConditionChecks(runEndHandlers: false, ScrollBar, "ScrollTween");
				}
				m_scrollTween = TweenManager.TweenTo_UnscaledTime(ScrollBar, duration, Ease.Quad.EaseInOut, "value", value4);
				m_scrollTween.ID = "ScrollTween";
			}
			else
			{
				ScrollBar.value = value4;
			}
		}
	}

	public void SetSelectedCategoryIndex(int index, bool playSFX)
	{
		if (SelectedCategoryIndex != index)
		{
			if (SelectedCategoryIndex > -1 && SelectedCategoryIndex < ActiveCategoryEntryArray.Length)
			{
				ActiveCategoryEntryArray[SelectedCategoryIndex].OnDeselect(null);
			}
			SelectedCategoryIndex = index;
			BaseOmniUIEntry.StaticSelectedButtonIndex = 0;
			if (SelectedCategoryIndex > -1 && (!m_hasCategories || SelectedCategoryIndex < ActiveCategoryEntryArray.Length))
			{
				U[] activeEntryArray = ActiveEntryArray;
				foreach (U val in activeEntryArray)
				{
					val.Interactable = true;
					val.OnDeselect(null);
				}
				ScrollRect.vertical = true;
				ScrollBar.interactable = true;
				SetEntryNavigationEnabled(enable: true);
				_ = ActiveEntryArray.LongLength;
				if (m_hasCategories)
				{
					ActiveCategoryEntryArray[SelectedCategoryIndex].OnSelect(null);
				}
				SelectedEntryIndex = -1;
				SetSelectedEntryIndex(GetEquippedIndex(), playAudio: false, usingMouse: false);
				AutoScroll(scrollingUp: false, usingMouse: false, skipAnimation: true);
				ScrollRect.gameObject.SetActive(value: true);
				m_updateScrollBar = true;
			}
			if (playSFX && m_selectOptionEvent != null)
			{
				m_selectOptionEvent.Invoke();
			}
		}
		RunHighlightIndicatorAnimation();
	}

	private void SetDescriptionBoxVisibility(bool visible)
	{
		if (visible)
		{
			if (DescriptionBox.alpha != 1f)
			{
				TweenManager.StopAllTweensContaining(PurchaseBox, runEndHandlers: false);
				TweenManager.TweenTo_UnscaledTime(PurchaseBox.transform, 0.1f, Ease.None, "localPosition.x", 0);
				TweenManager.TweenTo_UnscaledTime(PurchaseBox, 0.1f, Ease.None, "alpha", 1);
				TweenManager.StopAllTweensContaining(DescriptionBox, runEndHandlers: false);
				TweenManager.TweenTo_UnscaledTime(DescriptionBox.transform, 0.1f, Ease.None, "localPosition.x", 0);
				TweenManager.TweenTo_UnscaledTime(DescriptionBox, 0.1f, Ease.None, "alpha", 1);
			}
		}
		else if (DescriptionBox.alpha != 0f)
		{
			TweenManager.StopAllTweensContaining(PurchaseBox, runEndHandlers: false);
			TweenManager.TweenTo_UnscaledTime(PurchaseBox.transform, 0.1f, Ease.None, "localPosition.x", 20);
			TweenManager.TweenTo_UnscaledTime(PurchaseBox, 0.1f, Ease.None, "alpha", 0);
			TweenManager.StopAllTweensContaining(DescriptionBox, runEndHandlers: false);
			TweenManager.TweenTo_UnscaledTime(DescriptionBox.transform, 0.1f, Ease.None, "localPosition.x", 20);
			TweenManager.TweenTo_UnscaledTime(DescriptionBox, 0.1f, Ease.None, "alpha", 0);
		}
	}

	protected void RunHighlightIndicatorAnimation()
	{
		Vector3 localScale = SelectedCategoryIndicator.transform.localScale;
		localScale.x = 0.9f;
		localScale.y = 0.9f;
		SelectedCategoryIndicator.transform.localScale = localScale;
		TweenManager.TweenTo_UnscaledTime(SelectedCategoryIndicator.transform, 0.05f, Ease.None, "localScale.x", 1, "localScale.y", 1);
	}

	public void SetHighlightedCategoryIndex(int index, bool playSFX)
	{
		if (HighlightedCategoryIndex != index)
		{
			HighlightedCategoryIndex = index;
			if (HighlightedCategoryIndex > -1 && HighlightedCategoryIndex < ActiveCategoryEntryArray.Length)
			{
				SelectedCategoryIndicator.transform.localPosition = ActiveCategoryEntryArray[HighlightedCategoryIndex].transform.localPosition;
			}
			UpdateActiveArrays();
			U[] activeEntryArray = ActiveEntryArray;
			foreach (U val in activeEntryArray)
			{
				val.UpdateState();
				val.OnDeselect(null);
				val.Interactable = true;
			}
			ScrollBar.value = 1f;
			if (HighlightedCategoryIndex > -1 && HighlightedCategoryIndex < ActiveCategoryEntryArray.Length)
			{
				SelectedEntryIndex = -1;
				SetSelectedEntryIndex(GetEquippedIndex(), playAudio: false, usingMouse: false);
				AutoScroll(scrollingUp: false, usingMouse: false, skipAnimation: true);
				ActiveEntryArray[SelectedEntryIndex].DeselectAllButtons();
			}
			if (playSFX && m_changeSelectedOptionEvent != null)
			{
				m_changeSelectedOptionEvent.Invoke();
			}
		}
		RunHighlightIndicatorAnimation();
	}

	public void SetHighlightedCategory(BaseOmniUICategoryEntry categoryEntry)
	{
		for (int i = 0; i < ActiveCategoryEntryArray.Length; i++)
		{
			if (categoryEntry == ActiveCategoryEntryArray[i])
			{
				SetHighlightedCategoryIndex(i, playSFX: true);
			}
		}
	}

	protected void SetIndicatorPosition()
	{
		if (SelectedCategoryIndicator != null && ActiveCategoryEntryArray != null && ActiveCategoryEntryArray.Length != 0)
		{
			SelectedCategoryIndicator.transform.localPosition = ActiveCategoryEntryArray[HighlightedCategoryIndex].transform.localPosition;
		}
	}

	protected abstract void CreateCategoryEntries();

	protected abstract void CreateEntries();

	public override void Initialize()
	{
		if (m_hasCategories)
		{
			CreateCategoryEntries();
			LayoutRebuilder.ForceRebuildLayoutImmediate(CategoryEntryLayoutGroup.GetComponent<RectTransform>());
		}
		CreateEntries();
		if (!m_hasCategories)
		{
			CategoryEntryLayoutGroup.gameObject.SetActive(value: false);
			SelectedCategoryIndicator.gameObject.SetActive(value: false);
		}
		UpdateActiveArrays();
		m_entryHeight = m_entryPrefab.GetComponent<RectTransform>().rect.height;
		ScrollBar.onValueChanged.AddListener(SetScrollBarDirty);
		m_storedWarningBoxScale = WarningMessageBox.transform.localScale;
		if ((bool)NPCController)
		{
			Renderer[] componentsInChildren = NPCController.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].material.SetFloat("_OutlineScale", 1.7f);
			}
			NPCController.Animator.updateMode = AnimatorUpdateMode.UnscaledTime;
			NPCController.gameObject.SetLayerRecursively(28, includeInactive: true);
		}
		if ((bool)PlayerModel)
		{
			PlayerModel.VisualsGameObject.SetLayerRecursively(28, includeInactive: true);
		}
		base.Initialize();
	}

	private void SetScrollBarDirty(float scrollAmount = 0f)
	{
		m_updateScrollBar = true;
	}

	private void LateUpdate()
	{
		if (m_updateScrollBar)
		{
			m_updateScrollBar = false;
			UpdateScrollArrows(ScrollBar.value);
		}
	}

	protected virtual void UpdateScrollArrows(float scrollAmount)
	{
		if ((float)ActiveEntryArray.Length * m_entryHeight > ContentViewport.rect.height)
		{
			float num = 1f / (float)ActiveEntryArray.Length;
			if (scrollAmount > num)
			{
				if (!BottomScrollArrow.activeSelf)
				{
					BottomScrollArrow.SetActive(value: true);
				}
			}
			else if (BottomScrollArrow.activeSelf)
			{
				BottomScrollArrow.SetActive(value: false);
			}
			if (scrollAmount < 1f - num)
			{
				if (!TopScrollArrow.activeSelf)
				{
					TopScrollArrow.SetActive(value: true);
				}
			}
			else if (TopScrollArrow.activeSelf)
			{
				TopScrollArrow.SetActive(value: false);
			}
		}
		else
		{
			if (TopScrollArrow.activeSelf)
			{
				TopScrollArrow.SetActive(value: false);
			}
			if (BottomScrollArrow.activeSelf)
			{
				BottomScrollArrow.SetActive(value: false);
			}
		}
	}

	public virtual void UpdateActiveArrays()
	{
		U[] entryArray = EntryArray;
		for (int i = 0; i < entryArray.Length; i++)
		{
			entryArray[i].UpdateActive();
		}
		if (m_hasCategories)
		{
			ActiveCategoryEntryArray = CategoryEntryLayoutGroup.GetComponentsInChildren<T>(includeInactive: false);
		}
		ActiveEntryArray = EntryLayoutGroup.GetComponentsInChildren<U>(includeInactive: false);
		for (int j = 0; j < ActiveEntryArray.Length; j++)
		{
			ActiveEntryArray[j].SetEntryIndex(j);
		}
	}

	public void UpdateAllCategoryEntryStates()
	{
		if (m_hasCategories)
		{
			T[] activeCategoryEntryArray = ActiveCategoryEntryArray;
			for (int i = 0; i < activeCategoryEntryArray.Length; i++)
			{
				activeCategoryEntryArray[i].UpdateState();
			}
		}
	}

	public virtual void UpdateAllEntryStates()
	{
		U[] activeEntryArray = ActiveEntryArray;
		for (int i = 0; i < activeEntryArray.Length; i++)
		{
			activeEntryArray[i].UpdateState();
		}
	}

	public void UpdateAllStates(MonoBehaviour sender, EventArgs args)
	{
		UpdateAllCategoryEntryStates();
		UpdateAllEntryStates();
	}

	protected override void OnOpen()
	{
		AudioManager.SetEnemySFXPaused(pause: true);
		AudioManager.SetPlayerSFXPaused(pause: true);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.LanguageChanged, m_refreshText);
		m_windowCanvas.gameObject.SetActive(value: true);
		CameraController.SoloCam.gameObject.SetActive(value: true);
		if (CanReset && !ResetTextbox.activeSelf)
		{
			ResetTextbox.SetActive(value: true);
		}
		else if (!CanReset && ResetTextbox.activeSelf)
		{
			ResetTextbox.SetActive(value: false);
		}
		m_purchaseDialogueArgs.Initialize(PurchaseBoxDialogueType.Welcome);
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.UpdatePurchaseBoxDialogue, this, m_purchaseDialogueArgs);
		if ((bool)NPCController)
		{
			NPCController.SetNPCState(NPCState.AtAttention, skipTransition: true);
		}
		if ((bool)PlayerModel)
		{
			PlayerModel.InitializeLook(SaveManager.PlayerSaveData.CurrentCharacter);
			PlayerModel.InitializeEquipmentLook(SaveManager.PlayerSaveData.CurrentCharacter);
		}
		Vector3 localPosition = PurchaseBox.transform.localPosition;
		localPosition.x = 20f;
		PurchaseBox.transform.localPosition = localPosition;
		DescriptionBox.transform.localPosition = localPosition;
		PurchaseBox.alpha = 0f;
		DescriptionBox.alpha = 0f;
		WarningMessageBox.transform.localScale = Vector3.zero;
		m_warningMessageVisible = false;
		m_keyboardEnabled = true;
		SetDescriptionBoxVisibility(visible: true);
		ChooseCategoryText.gameObject.SetActive(value: false);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.OmniUI_UpdateState, m_updateAllStates);
		UpdateAllCategoryEntryStates();
		U[] activeEntryArray = ActiveEntryArray;
		for (int i = 0; i < activeEntryArray.Length; i++)
		{
			activeEntryArray[i].OnDeselect(null);
		}
		SelectedEntryIndex = -1;
		if (m_hasCategories)
		{
			HighlightedCategoryIndex = -1;
			SetSelectedCategoryIndex(-1, playSFX: false);
			SetHighlightedCategoryIndex(0, playSFX: false);
		}
		else
		{
			UpdateActiveArrays();
			SelectedCategoryIndex = -1;
			UpdateAllEntryStates();
			SetSelectedCategoryIndex(0, playSFX: false);
			int num = ActiveEntryArray.Length;
			if (num > 0 && num > SelectedEntryIndex)
			{
				ActiveEntryArray[SelectedEntryIndex].SelectRightMostButton();
			}
		}
		TweenManager.RunFunction_UnscaledTime(0.1f, this, "SetIndicatorPosition");
		StartCoroutine(OnEnterAnimCoroutine());
		BackgroundCanvasGroup.alpha = 1f;
		BackgroundCanvasGroup.gameObject.SetActive(value: true);
		Vector3 position = CameraController.GameCamera.transform.position;
		position.z = BackgroundCanvasGroup.transform.position.z;
		BackgroundCanvasGroup.transform.position = position;
		StartCoroutine(FadeBlurCoroutine(0.25f, fadeIn: true));
	}

	private IEnumerator FadeBlurCoroutine(float duration, bool fadeIn)
	{
		float endAmount = 1f;
		duration = Time.unscaledTime + duration;
		float startTime = Time.unscaledTime;
		if (fadeIn)
		{
			m_blurMaterial_STATIC.SetFloat("_Size", 0f);
		}
		else
		{
			m_blurMaterial_STATIC.SetFloat("_Size", endAmount);
		}
		while (Time.unscaledTime < duration)
		{
			float t = (duration - startTime) / (Time.unscaledTime - startTime);
			float value = ((!fadeIn) ? Mathf.Lerp(endAmount, 0f, t) : Mathf.Lerp(0f, endAmount, t));
			m_blurMaterial_STATIC.SetFloat("_Size", value);
			yield return null;
		}
	}

	protected virtual int GetEquippedIndex()
	{
		return 0;
	}

	protected virtual IEnumerator OnEnterAnimCoroutine()
	{
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		CanvasGroup letterBoxGroup = m_commonFields.LetterBoxGroup;
		TweenManager.StopAllTweensContaining(letterBoxGroup, runEndHandlers: false);
		letterBoxGroup.alpha = 0f;
		TweenManager.TweenTo_UnscaledTime(letterBoxGroup, 0.1f, Ease.None, "alpha", 1);
		MenuCanvasGroup.alpha = 0f;
		Vector3 localPosition = MenuCanvasGroup.transform.localPosition;
		localPosition.y += 50f;
		MenuCanvasGroup.transform.localPosition = localPosition;
		float num = 500f;
		float duration = 0.15f;
		Vector3 localPosition2 = PlayerModel.transform.localPosition;
		localPosition2.x = CommonFields.PlayerSpawnPosObj.transform.localPosition.x - num;
		localPosition2.y = CommonFields.PlayerSpawnPosObj.transform.localPosition.y;
		PlayerModel.transform.localPosition = localPosition2;
		Vector3 localPosition3 = NPCModel.transform.localPosition;
		localPosition3.x = CommonFields.NPCSpawnPosObj.transform.localPosition.x + num;
		localPosition3.y = CommonFields.NPCSpawnPosObj.transform.localPosition.y;
		NPCModel.transform.localPosition = localPosition3;
		TweenManager.TweenTo_UnscaledTime(MenuCanvasGroup, duration, Ease.None, "alpha", 1);
		TweenManager.TweenBy_UnscaledTime(PlayerModel.transform, duration, Ease.Quad.EaseOut, "localPosition.x", num);
		TweenManager.TweenBy_UnscaledTime(NPCModel.transform, duration, Ease.Quad.EaseOut, "localPosition.x", 0f - num);
		yield return TweenManager.TweenTo_UnscaledTime(MenuCanvasGroup.transform, duration, Ease.Quad.EaseOut, "localPosition.y", 0).TweenCoroutine;
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
	}

	protected override void OnClose()
	{
		AudioManager.SetEnemySFXPaused(pause: false);
		AudioManager.SetPlayerSFXPaused(pause: false);
		BackgroundCanvasGroup.gameObject.SetActive(value: false);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.LanguageChanged, m_refreshText);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.OmniUI_UpdateState, m_updateAllStates);
		StartCoroutine(FadeBlurCoroutine(0.25f, fadeIn: false));
		StartCoroutine(OnExitAnimCoroutine());
	}

	protected virtual IEnumerator OnExitAnimCoroutine()
	{
		CanvasGroup letterBoxGroup = m_commonFields.LetterBoxGroup;
		TweenManager.StopAllTweensContaining(letterBoxGroup, runEndHandlers: false);
		letterBoxGroup.alpha = 1f;
		TweenManager.TweenTo_UnscaledTime(letterBoxGroup, 0.1f, Ease.None, "alpha", 0);
		float num = 500f;
		float num2 = 0.15f;
		MenuCanvasGroup.alpha = 1f;
		SetDescriptionBoxVisibility(visible: false);
		TweenManager.TweenTo_UnscaledTime(MenuCanvasGroup, num2 / 2f, Ease.None, "delay", num2 / 2f, "alpha", 0);
		TweenManager.TweenTo_UnscaledTime(PlayerModel.transform, num2, Ease.Quad.EaseIn, "localPosition.x", CommonFields.PlayerSpawnPosObj.transform.localPosition.x - num);
		TweenManager.TweenTo_UnscaledTime(NPCModel.transform, num2, Ease.Quad.EaseIn, "localPosition.x", CommonFields.NPCSpawnPosObj.transform.localPosition.x + num);
		yield return TweenManager.TweenTo_UnscaledTime(MenuCanvasGroup.transform, num2, Ease.Quad.EaseIn, "localPosition.y", 50).TweenCoroutine;
		m_windowCanvas.gameObject.SetActive(value: false);
		CameraController.SoloCam.gameObject.SetActive(value: false);
	}

	protected override void OnFocus()
	{
		AddInputListeners();
	}

	protected override void OnLostFocus()
	{
		RemoveInputListeners();
	}

	protected virtual void OnConfirmButtonJustPressed()
	{
		if (m_warningMessageVisible)
		{
			HideWarningMessage();
		}
		else if (IsInCategories)
		{
			SetSelectedCategoryIndex(HighlightedCategoryIndex, playSFX: true);
		}
		else if (ActiveEntryArray.Length != 0)
		{
			ActiveEntryArray[SelectedEntryIndex].OnConfirmButtonPressed();
		}
	}

	protected virtual void OnCancelButtonJustPressed()
	{
		if (IsInCategories)
		{
			if (CanExit)
			{
				WindowManager.SetWindowIsOpen(ID, isOpen: false);
			}
			else if (!m_warningMessageVisible)
			{
				DisplayWarningMessage();
			}
			else
			{
				HideWarningMessage();
			}
		}
		else if (m_hasCategories)
		{
			SetSelectedCategoryIndex(-1, playSFX: true);
			if (ActiveEntryArray.Length != 0)
			{
				ActiveEntryArray[SelectedEntryIndex].DeselectAllButtons();
			}
		}
		else if (CanExit)
		{
			WindowManager.SetWindowIsOpen(ID, isOpen: false);
		}
		else if (!m_warningMessageVisible)
		{
			DisplayWarningMessage();
		}
		else
		{
			HideWarningMessage();
		}
	}

	protected virtual void DisplayWarningMessage()
	{
		m_warningMessageVisible = true;
		TweenManager.StopAllTweensContaining(WarningMessageBox.transform, runEndHandlers: false);
		WarningMessageBox.transform.localScale = Vector3.zero;
		TweenManager.TweenTo_UnscaledTime(WarningMessageBox.transform, 0.25f, Ease.Back.EaseOut, "localScale.x", m_storedWarningBoxScale.x, "localScale.y", m_storedWarningBoxScale.y, "localScale.z", 1);
	}

	protected virtual void HideWarningMessage()
	{
		m_warningMessageVisible = false;
		TweenManager.StopAllTweensContaining(WarningMessageBox.transform, runEndHandlers: false);
		WarningMessageBox.transform.localScale = m_storedWarningBoxScale;
		TweenManager.TweenTo_UnscaledTime(WarningMessageBox.transform, 0.25f, Ease.Back.EaseIn, "localScale.x", 0, "localScale.y", 0, "localScale.z", 0);
	}

	protected virtual void OnLBButtonJustPressed()
	{
		if (!IsInCategories)
		{
			int selectedEntryIndex = SelectedEntryIndex;
			selectedEntryIndex -= 7;
			if (selectedEntryIndex < 0)
			{
				selectedEntryIndex = 0;
			}
			SetSelectedEntryIndex(selectedEntryIndex, playAudio: true, usingMouse: false);
		}
	}

	protected virtual void OnRBButtonJustPressed()
	{
		if (!IsInCategories)
		{
			int selectedEntryIndex = SelectedEntryIndex;
			selectedEntryIndex += 7;
			if (selectedEntryIndex >= ActiveEntryArray.Length)
			{
				selectedEntryIndex = ActiveEntryArray.Length - 1;
			}
			SetSelectedEntryIndex(selectedEntryIndex, playAudio: true, usingMouse: false);
		}
	}

	protected virtual void OnYButtonJustPressed()
	{
	}

	protected virtual void OnXButtonJustPressed()
	{
	}

	protected virtual void OnUpButtonJustPressed()
	{
		if (IsInCategories)
		{
			int highlightedCategoryIndex = HighlightedCategoryIndex;
			highlightedCategoryIndex--;
			if (highlightedCategoryIndex < 0)
			{
				highlightedCategoryIndex = ActiveCategoryEntryArray.Length - 1;
			}
			SetHighlightedCategoryIndex(highlightedCategoryIndex, playSFX: true);
		}
		else
		{
			int selectedEntryIndex = SelectedEntryIndex;
			selectedEntryIndex--;
			if (selectedEntryIndex < 0)
			{
				selectedEntryIndex = ActiveEntryArray.Length - 1;
			}
			SetSelectedEntryIndex(selectedEntryIndex, playAudio: true, usingMouse: false);
		}
	}

	protected virtual void OnDownButtonJustPressed()
	{
		if (IsInCategories)
		{
			int highlightedCategoryIndex = HighlightedCategoryIndex;
			highlightedCategoryIndex++;
			if (highlightedCategoryIndex >= ActiveCategoryEntryArray.Length)
			{
				highlightedCategoryIndex = 0;
			}
			SetHighlightedCategoryIndex(highlightedCategoryIndex, playSFX: true);
		}
		else
		{
			int selectedEntryIndex = SelectedEntryIndex;
			selectedEntryIndex++;
			if (selectedEntryIndex >= ActiveEntryArray.Length)
			{
				selectedEntryIndex = 0;
			}
			SetSelectedEntryIndex(selectedEntryIndex, playAudio: true, usingMouse: false);
		}
	}

	protected virtual void OnLeftButtonJustPressed()
	{
		if (!IsInCategories && ActiveEntryArray.Length != 0)
		{
			ActiveEntryArray[SelectedEntryIndex].SelectButton(selectRight: false);
		}
	}

	protected virtual void OnRightButtonJustPressed()
	{
		if (!IsInCategories && ActiveEntryArray.Length != 0)
		{
			ActiveEntryArray[SelectedEntryIndex].SelectButton(selectRight: true);
		}
	}

	private void AddInputListeners()
	{
		base.RewiredPlayer.AddInputEventDelegate(m_onConfirmInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
		base.RewiredPlayer.AddInputEventDelegate(m_onCancelInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
		base.RewiredPlayer.AddInputEventDelegate(m_onHorizontalInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Horizontal");
		base.RewiredPlayer.AddInputEventDelegate(m_onHorizontalInputHandler, UpdateLoopType.Update, InputActionEventType.NegativeButtonJustPressed, "Window_Horizontal");
		base.RewiredPlayer.AddInputEventDelegate(m_onVerticalInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonRepeating, "Window_Vertical");
		base.RewiredPlayer.AddInputEventDelegate(m_onVerticalInputHandler, UpdateLoopType.Update, InputActionEventType.NegativeButtonRepeating, "Window_Vertical");
		base.RewiredPlayer.AddInputEventDelegate(m_onLBInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_LB");
		base.RewiredPlayer.AddInputEventDelegate(m_onRBInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_RB");
		base.RewiredPlayer.AddInputEventDelegate(m_onYButtonResetHandler, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Y");
		base.RewiredPlayer.AddInputEventDelegate(m_onXButtonInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_X");
	}

	private void RemoveInputListeners()
	{
		base.RewiredPlayer.RemoveInputEventDelegate(m_onConfirmInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
		base.RewiredPlayer.RemoveInputEventDelegate(m_onCancelInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
		base.RewiredPlayer.RemoveInputEventDelegate(m_onHorizontalInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Horizontal");
		base.RewiredPlayer.RemoveInputEventDelegate(m_onHorizontalInputHandler, UpdateLoopType.Update, InputActionEventType.NegativeButtonJustPressed, "Window_Horizontal");
		base.RewiredPlayer.RemoveInputEventDelegate(m_onVerticalInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonRepeating, "Window_Vertical");
		base.RewiredPlayer.RemoveInputEventDelegate(m_onVerticalInputHandler, UpdateLoopType.Update, InputActionEventType.NegativeButtonRepeating, "Window_Vertical");
		base.RewiredPlayer.RemoveInputEventDelegate(m_onLBInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_LB");
		base.RewiredPlayer.RemoveInputEventDelegate(m_onRBInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_RB");
		base.RewiredPlayer.RemoveInputEventDelegate(m_onYButtonResetHandler, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Y");
		base.RewiredPlayer.RemoveInputEventDelegate(m_onXButtonInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_X");
	}

	private void OnLBInputHandler(InputActionEventData eventData)
	{
		if (m_keyboardEnabled && !m_warningMessageVisible)
		{
			OnLBButtonJustPressed();
		}
	}

	private void OnRBInputHandler(InputActionEventData eventData)
	{
		if (m_keyboardEnabled && !m_warningMessageVisible)
		{
			OnRBButtonJustPressed();
		}
	}

	private void OnYButtonResetHandler(InputActionEventData eventData)
	{
		if (m_keyboardEnabled && !m_warningMessageVisible)
		{
			OnYButtonJustPressed();
		}
	}

	private void OnXButtonInputHandler(InputActionEventData eventData)
	{
		if (m_keyboardEnabled && !m_warningMessageVisible)
		{
			OnXButtonJustPressed();
		}
	}

	private void OnVerticalInputHandler(InputActionEventData eventData)
	{
		if (m_keyboardEnabled && !m_warningMessageVisible && (m_entryNavigationEnabled || IsInCategories))
		{
			float num = eventData.GetAxis();
			if (num == 0f)
			{
				num = 0f - eventData.GetAxisPrev();
			}
			if (num > 0f)
			{
				OnUpButtonJustPressed();
			}
			else
			{
				OnDownButtonJustPressed();
			}
		}
	}

	private void OnHorizontalInputHandler(InputActionEventData eventData)
	{
		if (m_keyboardEnabled && !m_warningMessageVisible)
		{
			float num = eventData.GetAxis();
			if (num == 0f)
			{
				num = 0f - eventData.GetAxisPrev();
			}
			if (num > 0f)
			{
				OnRightButtonJustPressed();
			}
			else
			{
				OnLeftButtonJustPressed();
			}
		}
	}

	private void OnConfirmInputHandler(InputActionEventData eventData)
	{
		if (eventData.IsCurrentInputSource(ControllerType.Mouse))
		{
			if (m_warningMessageVisible)
			{
				HideWarningMessage();
			}
		}
		else if (m_keyboardEnabled)
		{
			OnConfirmButtonJustPressed();
		}
	}

	protected virtual void OnCancelInputHandler(InputActionEventData eventData)
	{
		if (m_keyboardEnabled)
		{
			OnCancelButtonJustPressed();
		}
	}

	protected virtual void RefreshText(object sender, EventArgs args)
	{
		UpdateAllEntryStates();
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateDescription, this, ActiveEntryArray[SelectedEntryIndex].EntryEventArgs);
	}
}
