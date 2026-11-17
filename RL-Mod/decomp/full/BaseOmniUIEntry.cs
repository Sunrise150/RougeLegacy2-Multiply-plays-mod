using System;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class BaseOmniUIEntry : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, ISelectHandler, IDeselectHandler
{
	public static int StaticSelectedButtonIndex;

	[SerializeField]
	protected Image m_icon;

	[SerializeField]
	protected GameObject m_iconGO;

	[SerializeField]
	protected GameObject m_inactiveIconGO;

	[SerializeField]
	protected TMP_Text m_titleText;

	[SerializeField]
	protected Image m_selectedSprite;

	[SerializeField]
	protected Image m_newSymbol;

	protected IOmniUIWindowController m_windowController;

	protected OmniUIButton[] m_buttonArray;

	protected int m_localSelectedButtonIndex;

	private bool m_interactable;

	public abstract bool IsEntryActive { get; }

	public abstract EventArgs EntryEventArgs { get; }

	public bool Interactable
	{
		get
		{
			return m_interactable;
		}
		set
		{
			if (m_buttonArray != null)
			{
				OmniUIButton[] buttonArray = m_buttonArray;
				for (int i = 0; i < buttonArray.Length; i++)
				{
					buttonArray[i].Interactable = value;
				}
			}
			m_interactable = value;
		}
	}

	public bool HasActiveButtons
	{
		get
		{
			if (m_buttonArray.Length != 0)
			{
				OmniUIButton[] buttonArray = m_buttonArray;
				foreach (OmniUIButton omniUIButton in buttonArray)
				{
					if (omniUIButton.gameObject.activeInHierarchy && omniUIButton.IsButtonActive)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public int EntryIndex { get; protected set; }

	public IOmniUIWindowController WindowController => m_windowController;

	public int GetFirstActiveButtonIndex()
	{
		if (HasActiveButtons)
		{
			for (int i = 0; i < m_buttonArray.Length; i++)
			{
				if (m_buttonArray[i].gameObject.activeInHierarchy && m_buttonArray[i].IsButtonActive)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public int GetLastActiveButtonIndex()
	{
		if (HasActiveButtons)
		{
			for (int num = m_buttonArray.Length - 1; num >= 0; num--)
			{
				if (m_buttonArray[num].gameObject.activeInHierarchy && m_buttonArray[num].IsButtonActive)
				{
					return num;
				}
			}
		}
		return -1;
	}

	public virtual void Initialize(IOmniUIWindowController windowController)
	{
		m_windowController = windowController;
		m_buttonArray = GetComponentsInChildren<OmniUIButton>();
		for (int i = 0; i < m_buttonArray.Length; i++)
		{
			m_buttonArray[i].ButtonIndex = i;
			OmniUIButton obj = m_buttonArray[i];
			obj.OnSelectEvent = (OmniUIButtonHandler)Delegate.Combine(obj.OnSelectEvent, new OmniUIButtonHandler(OnButtonSelected));
			m_buttonArray[i].OnConfirmPressedRelay.AddListener(PlayConfirmPressedSFX);
		}
	}

	public virtual void SetEntryIndex(int index)
	{
		EntryIndex = index;
	}

	private void OnButtonSelected(OmniUIButton button)
	{
		if (m_localSelectedButtonIndex != button.ButtonIndex)
		{
			if (m_localSelectedButtonIndex < m_buttonArray.Length)
			{
				m_buttonArray[m_localSelectedButtonIndex].OnDeselect(null);
			}
			m_localSelectedButtonIndex = button.ButtonIndex;
		}
	}

	public abstract void UpdateActive();

	public virtual void UpdateState()
	{
		if (!IsEntryActive)
		{
			m_titleText.text = "???";
		}
		if (!IsEntryActive)
		{
			if ((bool)m_iconGO && m_iconGO.activeSelf)
			{
				m_iconGO.SetActive(value: false);
			}
			if ((bool)m_inactiveIconGO && !m_inactiveIconGO.activeSelf)
			{
				m_inactiveIconGO.SetActive(value: true);
			}
		}
		else
		{
			if ((bool)m_iconGO && !m_iconGO.activeSelf)
			{
				m_iconGO.SetActive(value: true);
			}
			if ((bool)m_inactiveIconGO && m_inactiveIconGO.activeSelf)
			{
				m_inactiveIconGO.SetActive(value: false);
			}
		}
		if (m_buttonArray == null || m_buttonArray.Length == 0)
		{
			return;
		}
		OmniUIButton[] buttonArray = m_buttonArray;
		foreach (OmniUIButton omniUIButton in buttonArray)
		{
			GameObject gameObject = omniUIButton.gameObject;
			if (omniUIButton.transform.parent.gameObject != base.gameObject)
			{
				gameObject = omniUIButton.transform.parent.gameObject;
			}
			if (!IsEntryActive)
			{
				if (gameObject.activeSelf)
				{
					gameObject.SetActive(value: false);
				}
				continue;
			}
			if (!gameObject.activeSelf)
			{
				gameObject.SetActive(value: true);
			}
			omniUIButton.UpdateState();
		}
		if (m_windowController.SelectedEntryIndex != EntryIndex)
		{
			return;
		}
		OmniUIButton omniUIButton2 = m_buttonArray[m_localSelectedButtonIndex];
		if (!m_buttonArray[m_localSelectedButtonIndex].IsButtonActive)
		{
			if (GetFirstActiveButtonIndex() < m_localSelectedButtonIndex)
			{
				SelectButton(selectRight: false);
			}
			else if (GetLastActiveButtonIndex() > m_localSelectedButtonIndex)
			{
				SelectButton(selectRight: true);
			}
		}
		if (omniUIButton2 == m_buttonArray[m_localSelectedButtonIndex] && !omniUIButton2.IsButtonActive)
		{
			omniUIButton2.OnDeselect(null);
		}
	}

	public virtual void DeselectAllButtons()
	{
		OmniUIButton[] buttonArray = m_buttonArray;
		for (int i = 0; i < buttonArray.Length; i++)
		{
			buttonArray[i].OnDeselect(null);
		}
	}

	public void SelectRightMostButton()
	{
		for (int i = 0; i < m_buttonArray.Length; i++)
		{
			SelectButton(selectRight: true);
		}
	}

	public void SelectLeftMostButton()
	{
		for (int i = 0; i < m_buttonArray.Length; i++)
		{
			SelectButton(selectRight: false);
		}
	}

	public void SelectButton(bool selectRight)
	{
		if (!Interactable || !HasActiveButtons || !IsEntryActive)
		{
			return;
		}
		int firstActiveButtonIndex = GetFirstActiveButtonIndex();
		int lastActiveButtonIndex = GetLastActiveButtonIndex();
		int localSelectedButtonIndex = m_localSelectedButtonIndex;
		if (selectRight)
		{
			while (StaticSelectedButtonIndex < lastActiveButtonIndex)
			{
				StaticSelectedButtonIndex++;
				if (m_buttonArray[StaticSelectedButtonIndex].IsButtonActive)
				{
					break;
				}
			}
		}
		else
		{
			while (StaticSelectedButtonIndex > firstActiveButtonIndex)
			{
				StaticSelectedButtonIndex--;
				if (m_buttonArray[StaticSelectedButtonIndex].IsButtonActive)
				{
					break;
				}
			}
		}
		if (selectRight)
		{
			while (m_localSelectedButtonIndex < lastActiveButtonIndex)
			{
				m_localSelectedButtonIndex++;
				if (m_buttonArray[m_localSelectedButtonIndex].IsButtonActive)
				{
					break;
				}
			}
		}
		else
		{
			while (m_localSelectedButtonIndex > firstActiveButtonIndex)
			{
				m_localSelectedButtonIndex--;
				if (m_buttonArray[m_localSelectedButtonIndex].IsButtonActive)
				{
					break;
				}
			}
		}
		if (localSelectedButtonIndex != m_localSelectedButtonIndex)
		{
			m_buttonArray[m_localSelectedButtonIndex].OnSelect(null);
			m_buttonArray[localSelectedButtonIndex].OnDeselect(null);
		}
	}

	public virtual void OnSelect(BaseEventData eventData)
	{
		if (!Interactable)
		{
			return;
		}
		if (!m_selectedSprite.gameObject.activeSelf)
		{
			m_selectedSprite.gameObject.SetActive(value: true);
		}
		if (HasActiveButtons)
		{
			m_localSelectedButtonIndex = StaticSelectedButtonIndex;
			int firstActiveButtonIndex = GetFirstActiveButtonIndex();
			int lastActiveButtonIndex = GetLastActiveButtonIndex();
			if (m_localSelectedButtonIndex >= lastActiveButtonIndex)
			{
				m_localSelectedButtonIndex = lastActiveButtonIndex;
			}
			if (m_localSelectedButtonIndex < firstActiveButtonIndex)
			{
				m_localSelectedButtonIndex = firstActiveButtonIndex;
			}
			while (m_localSelectedButtonIndex > firstActiveButtonIndex && m_localSelectedButtonIndex < lastActiveButtonIndex && !m_buttonArray[m_localSelectedButtonIndex].IsButtonActive)
			{
				if (m_buttonArray[m_localSelectedButtonIndex - 1].IsButtonActive)
				{
					m_localSelectedButtonIndex--;
				}
				else
				{
					m_localSelectedButtonIndex++;
				}
			}
			m_buttonArray[m_localSelectedButtonIndex].OnSelect(null);
		}
		if (!IsEntryActive)
		{
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateDescription, this, null);
		}
		else if (!HasActiveButtons)
		{
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateDescription, this, EntryEventArgs);
		}
	}

	public virtual void OnDeselect(BaseEventData eventData)
	{
		if (Interactable)
		{
			if (m_selectedSprite.gameObject.activeSelf)
			{
				m_selectedSprite.gameObject.SetActive(value: false);
			}
			OmniUIButton[] buttonArray = m_buttonArray;
			for (int i = 0; i < buttonArray.Length; i++)
			{
				buttonArray[i].OnDeselect(null);
			}
		}
	}

	private void PlayConfirmPressedSFX(OmniUIButton button)
	{
		if (m_windowController.SelectOptionEvent != null)
		{
			m_windowController.SelectOptionEvent.Invoke();
		}
	}

	public virtual void OnConfirmButtonPressed()
	{
		if (Interactable && HasActiveButtons)
		{
			m_buttonArray[m_localSelectedButtonIndex].OnConfirmButtonPressed();
		}
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		if (RewiredOnStartupController.CurrentActiveControllerType == ControllerType.Mouse && Interactable)
		{
			m_windowController.SetSelectedEntryIndex(EntryIndex, playSFX: true, usingMouse: true);
		}
	}

	private void OnDestroy()
	{
		if (m_buttonArray != null)
		{
			OmniUIButton[] buttonArray = m_buttonArray;
			foreach (OmniUIButton obj in buttonArray)
			{
				obj.OnSelectEvent = (OmniUIButtonHandler)Delegate.Remove(obj.OnSelectEvent, new OmniUIButtonHandler(OnButtonSelected));
				obj.OnConfirmPressedRelay.RemoveListener(PlayConfirmPressedSFX);
			}
		}
	}
}
