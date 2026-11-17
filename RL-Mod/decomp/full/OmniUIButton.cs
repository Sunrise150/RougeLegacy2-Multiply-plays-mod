using System;
using System.Collections;
using Rewired;
using Sigtrap.Relays;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class OmniUIButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, ISelectHandler, IDeselectHandler, IPointerClickHandler, IPointerExitHandler
{
	[SerializeField]
	protected Image m_selectedSprite;

	[SerializeField]
	protected Image m_deselectedSprite;

	private Vector3 m_storedScale;

	protected Relay<OmniUIButton> m_onConfirmPressedRelay = new Relay<OmniUIButton>();

	public bool Interactable { get; set; }

	public int ButtonIndex { get; set; }

	public OmniUIButtonHandler OnSelectEvent { get; set; }

	public OmniUIButtonHandler OnClickEvent { get; set; }

	public abstract EventArgs ButtonEventArgs { get; }

	public IRelayLink<OmniUIButton> OnConfirmPressedRelay => m_onConfirmPressedRelay.link;

	public virtual bool IsButtonActive { get; protected set; } = true;


	public abstract void UpdateState();

	protected virtual void Awake()
	{
		m_storedScale = base.transform.localScale;
	}

	protected abstract void InitializeButtonEventArgs();

	public virtual void OnConfirmButtonPressed()
	{
		if (base.gameObject.activeSelf)
		{
			BaseOmniUIEntry.StaticSelectedButtonIndex = ButtonIndex;
		}
		m_onConfirmPressedRelay.Dispatch(this);
	}

	protected virtual void RunOnConfirmPressedAnimation()
	{
		Vector3 localScale = m_storedScale * 0.9f;
		base.transform.localScale = localScale;
		TweenManager.TweenTo_UnscaledTime(base.transform, 0.05f, Ease.None, "localScale.x", m_storedScale.x, "localScale.y", m_storedScale.y);
	}

	protected virtual IEnumerator ShakeAnimCoroutine()
	{
		int shakeCount = 0;
		float z = 5f;
		float shakeDelay = 0.05f;
		float shakeTime = Time.unscaledTime + shakeDelay;
		Vector3 shakeEuler = base.transform.localEulerAngles;
		shakeEuler.z = z;
		base.transform.localEulerAngles = shakeEuler;
		while (shakeCount < 3)
		{
			if (Time.unscaledTime >= shakeTime)
			{
				shakeEuler.z *= -1f;
				base.transform.localEulerAngles = shakeEuler;
				shakeTime = Time.unscaledTime + shakeDelay;
				shakeCount++;
			}
			yield return null;
		}
		shakeEuler.z = 0f;
		base.transform.localEulerAngles = shakeEuler;
	}

	public virtual void OnSelect(BaseEventData eventData)
	{
		if (Interactable)
		{
			if ((bool)m_selectedSprite && !m_selectedSprite.gameObject.activeSelf)
			{
				m_selectedSprite.gameObject.SetActive(value: true);
			}
			if ((bool)m_deselectedSprite && m_deselectedSprite.gameObject.activeSelf)
			{
				m_deselectedSprite.gameObject.SetActive(value: false);
			}
			if (OnSelectEvent != null)
			{
				OnSelectEvent(this);
			}
			InitializeButtonEventArgs();
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateDescription, this, ButtonEventArgs);
		}
	}

	public virtual void OnDeselect(BaseEventData eventData)
	{
		if (Interactable)
		{
			if ((bool)m_selectedSprite && m_selectedSprite.gameObject.activeSelf)
			{
				m_selectedSprite.gameObject.SetActive(value: false);
			}
			if ((bool)m_deselectedSprite && !m_deselectedSprite.gameObject.activeSelf)
			{
				m_deselectedSprite.gameObject.SetActive(value: true);
			}
		}
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		if (Interactable && IsButtonActive && RewiredOnStartupController.CurrentActiveControllerType == ControllerType.Mouse)
		{
			OnSelect(null);
			BaseOmniUIEntry.StaticSelectedButtonIndex = ButtonIndex;
		}
	}

	public virtual void OnPointerExit(PointerEventData eventData)
	{
		if (Interactable && IsButtonActive && RewiredOnStartupController.CurrentActiveControllerType == ControllerType.Mouse)
		{
			OnDeselect(null);
		}
	}

	public virtual void OnPointerClick(PointerEventData eventData)
	{
		if (Interactable && IsButtonActive && eventData.button == PointerEventData.InputButton.Left && RewiredOnStartupController.CurrentActiveControllerType == ControllerType.Mouse)
		{
			OnConfirmButtonPressed();
			if (OnClickEvent != null)
			{
				OnClickEvent(this);
			}
		}
	}
}
