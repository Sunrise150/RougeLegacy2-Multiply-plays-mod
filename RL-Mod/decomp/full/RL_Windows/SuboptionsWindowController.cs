using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rewired;
using UnityEngine;

namespace RL_Windows;

public class SuboptionsWindowController : OptionsWindowController
{
	[Header("Suboption Window Fields")]
	[SerializeField]
	private CanvasGroup m_optionItemsCanvasGroup;

	[SerializeField]
	private RectTransform m_optionItemsRectTransform;

	[SerializeField]
	private SuboptionTypePrefabDictionary m_suboptionPrefabDict;

	[SerializeField]
	private CanvasGroup m_suboptionFadeBG;

	private SuboptionType m_currentSuboptionType;

	private ScrollRectAutoScroller m_scrollRectController;

	public override WindowID ID => WindowID.Suboptions;

	public override void Initialize()
	{
		m_windowCanvas.sortingOrder = 495;
		foreach (SuboptionType item in m_suboptionPrefabDict.Keys.ToList())
		{
			GameObject gameObject = m_suboptionPrefabDict[item];
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, m_optionItemsCanvasGroup.transform, worldPositionStays: false);
			InitializeSuboptionPrefab(gameObject2);
			gameObject2.name = gameObject.name;
			m_suboptionPrefabDict[item] = gameObject2;
		}
		ChangeSuboptionDisplayed(SuboptionType.Graphics);
		m_suboptionFadeBG.alpha = 0f;
		base.Initialize();
	}

	private void InitializeSuboptionPrefab(GameObject suboptionPrefab)
	{
		if (!(suboptionPrefab == null))
		{
			BaseOptionItem[] componentsInChildren = suboptionPrefab.GetComponentsInChildren<BaseOptionItem>();
			foreach (BaseOptionItem obj in componentsInChildren)
			{
				obj.Initialize();
				obj.OptionItemSelected = (OptionItemSelectedHandler)Delegate.Combine(obj.OptionItemSelected, new OptionItemSelectedHandler(UpdateSelectedOptionItem));
				obj.OptionItemActivated = (OptionItemSelectedHandler)Delegate.Combine(obj.OptionItemActivated, new OptionItemSelectedHandler(PlaySelectedSFX));
				obj.OptionItemDeactivated = (OptionItemSelectedHandler)Delegate.Combine(obj.OptionItemDeactivated, new OptionItemSelectedHandler(PlaySelectedSFX));
				obj.OptionItemActivated = (OptionItemSelectedHandler)Delegate.Combine(obj.OptionItemActivated, new OptionItemSelectedHandler(DisableButtons));
				obj.OptionItemDeactivated = (OptionItemSelectedHandler)Delegate.Combine(obj.OptionItemDeactivated, new OptionItemSelectedHandler(EnableButtons));
			}
		}
	}

	public void ChangeSuboptionDisplayed(SuboptionType suboptionType)
	{
		if (m_optionItemList != null)
		{
			foreach (BaseOptionItem optionItem in m_optionItemList)
			{
				optionItem.OptionItemSelected = (OptionItemSelectedHandler)Delegate.Remove(optionItem.OptionItemSelected, new OptionItemSelectedHandler(UpdateSelectedOptionItem));
				optionItem.OptionItemDeactivated = (OptionItemSelectedHandler)Delegate.Remove(optionItem.OptionItemDeactivated, new OptionItemSelectedHandler(PlaySelectedSFX));
				optionItem.OptionItemActivated = (OptionItemSelectedHandler)Delegate.Remove(optionItem.OptionItemActivated, new OptionItemSelectedHandler(PlaySelectedSFX));
				optionItem.OptionItemActivated = (OptionItemSelectedHandler)Delegate.Remove(optionItem.OptionItemActivated, new OptionItemSelectedHandler(DisableButtons));
				optionItem.OptionItemDeactivated = (OptionItemSelectedHandler)Delegate.Remove(optionItem.OptionItemDeactivated, new OptionItemSelectedHandler(EnableButtons));
				optionItem.Interactable = false;
			}
		}
		else
		{
			m_optionItemList = new List<BaseOptionItem>();
		}
		foreach (KeyValuePair<SuboptionType, GameObject> item in m_suboptionPrefabDict)
		{
			item.Value.SetActive(value: false);
		}
		if (m_suboptionPrefabDict.TryGetValue(suboptionType, out var value))
		{
			value.SetActive(value: true);
			value.GetComponentsInChildren(m_optionItemList);
			foreach (BaseOptionItem optionItem2 in m_optionItemList)
			{
				optionItem2.OptionItemSelected = (OptionItemSelectedHandler)Delegate.Combine(optionItem2.OptionItemSelected, new OptionItemSelectedHandler(UpdateSelectedOptionItem));
				optionItem2.OptionItemActivated = (OptionItemSelectedHandler)Delegate.Combine(optionItem2.OptionItemActivated, new OptionItemSelectedHandler(PlaySelectedSFX));
				optionItem2.OptionItemActivated = (OptionItemSelectedHandler)Delegate.Combine(optionItem2.OptionItemActivated, new OptionItemSelectedHandler(DisableButtons));
				optionItem2.OptionItemDeactivated = (OptionItemSelectedHandler)Delegate.Combine(optionItem2.OptionItemDeactivated, new OptionItemSelectedHandler(EnableButtons));
				optionItem2.OptionItemDeactivated = (OptionItemSelectedHandler)Delegate.Combine(optionItem2.OptionItemDeactivated, new OptionItemSelectedHandler(PlaySelectedSFX));
				optionItem2.Interactable = true;
			}
			m_optionItemList[0].OnSelect(null);
		}
		m_currentSuboptionType = suboptionType;
		m_scrollRectController = value.GetComponentInChildren<ScrollRectAutoScroller>();
		if (m_scrollRectController != null && m_optionItemList.Count > 0)
		{
			m_scrollRectController.ConfigureScrollBar(m_optionItemList.Count);
		}
		WindowController windowController = WindowManager.GetWindowController(WindowID.Pause);
		PauseWindowController pauseWindowController = ((windowController != null) ? (windowController as PauseWindowController) : null);
		if (m_currentSuboptionType == SuboptionType.Audio && (bool)pauseWindowController)
		{
			pauseWindowController.EnableSnapshotEmitter(enable: false);
		}
	}

	private void DisableButtons(BaseOptionItem optionItem)
	{
		if (optionItem is ExecuteImmediateOptionItem)
		{
			return;
		}
		foreach (BaseOptionItem optionItem2 in m_optionItemList)
		{
			if (optionItem2 != optionItem)
			{
				optionItem2.Interactable = false;
			}
		}
	}

	private void EnableButtons(BaseOptionItem optionItem)
	{
		foreach (BaseOptionItem optionItem2 in m_optionItemList)
		{
			optionItem2.Interactable = true;
		}
	}

	protected override void OnOpen()
	{
		WindowController windowController = WindowManager.GetWindowController(WindowID.Pause);
		PauseWindowController pauseWindowController = (windowController ? (windowController as PauseWindowController) : null);
		if ((bool)pauseWindowController)
		{
			pauseWindowController.EnableControls(enable: false);
		}
		StartCoroutine(RunOnOpenAnimation());
	}

	private IEnumerator RunOnOpenAnimation()
	{
		m_windowCanvas.gameObject.SetActive(value: true);
		UpdateSelectedOptionItem(m_optionItemList[0]);
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		Vector2 anchoredPosition = m_optionItemsRectTransform.anchoredPosition;
		anchoredPosition.y = 100f;
		m_optionItemsRectTransform.anchoredPosition = anchoredPosition;
		m_optionItemsCanvasGroup.alpha = 0f;
		TweenManager.TweenTo_UnscaledTime(m_suboptionFadeBG, 0.1f, Ease.None, "alpha", 1);
		TweenManager.TweenTo_UnscaledTime(m_optionItemsCanvasGroup, 0.1f, Ease.None, "alpha", 1);
		yield return TweenManager.TweenTo_UnscaledTime(m_optionItemsRectTransform, 0.15f, Ease.Quad.EaseOut, "anchoredPosition.y", 0).TweenCoroutine;
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
	}

	protected override void OnClose()
	{
		if (m_currentSuboptionType == SuboptionType.Audio)
		{
			WindowController windowController = WindowManager.GetWindowController(WindowID.Pause);
			if ((bool)windowController)
			{
				(windowController as PauseWindowController).EnableSnapshotEmitter(enable: true);
			}
		}
		SaveManager.SaveConfigFile();
		PlaySelectedSFX(null);
		StartCoroutine(RunOnCloseAnimation());
	}

	private IEnumerator RunOnCloseAnimation()
	{
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		TweenManager.TweenTo_UnscaledTime(m_suboptionFadeBG, 0.15f, Ease.None, "alpha", 0);
		TweenManager.TweenTo_UnscaledTime(m_optionItemsCanvasGroup, 0.15f, Ease.None, "alpha", 0);
		yield return TweenManager.TweenTo_UnscaledTime(m_optionItemsRectTransform, 0.15f, Ease.Quad.EaseIn, "anchoredPosition.y", 100).TweenCoroutine;
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
		m_windowCanvas.gameObject.SetActive(value: false);
		foreach (BaseOptionItem optionItem in m_optionItemList)
		{
			optionItem.OnDeselect(null);
		}
		WindowController windowController = WindowManager.GetWindowController(WindowID.Pause);
		PauseWindowController pauseWindowController = (windowController ? (windowController as PauseWindowController) : null);
		if ((bool)pauseWindowController)
		{
			pauseWindowController.EnableControls(enable: true);
		}
	}

	public void ForceCancelButtonDown(InputActionEventData eventData)
	{
		OnCancelButtonDown(eventData);
	}

	protected override void OnCancelButtonDown(InputActionEventData eventData)
	{
		if ((bool)m_currentSelectedOptionItem && m_currentSelectedOptionItem.IsActivated)
		{
			m_currentSelectedOptionItem.DeactivateOption(confirmOptionChange: false);
		}
		else
		{
			WindowManager.SetWindowIsOpen(ID, isOpen: false);
		}
	}

	protected override void UpdateSelectedOptionItem(BaseOptionItem menuItem)
	{
		if (!(menuItem == m_currentSelectedOptionItem))
		{
			base.UpdateSelectedOptionItem(menuItem);
			if ((bool)m_scrollRectController)
			{
				m_scrollRectController.SetSelectedIndex(m_optionItemList.IndexOf(menuItem));
			}
		}
	}
}
