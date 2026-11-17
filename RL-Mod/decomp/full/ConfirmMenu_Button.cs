using System;
using System.Collections;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ConfirmMenu_Button : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField]
	private TMP_Text m_text;

	[SerializeField]
	private CanvasGroup m_selectImageCanvasGroup;

	private Action m_onClickAction;

	private Action m_onPointerExitAction;

	private Action<ConfirmMenu_Button> m_onPointerEnterAction;

	private Action<ConfirmMenu_Button> m_onPointerClickAction;

	private Vector3 m_storedScale;

	private bool m_selected;

	private float m_selectStartTime;

	private string m_refreshTextLocID;

	public bool DisableClick { get; set; }

	public int Index { get; private set; }

	public void Initialize(int index, Action<ConfirmMenu_Button> onPointerEnterAction, Action onPointerExitAction, Action<ConfirmMenu_Button> onPointerClickAction)
	{
		Index = index;
		m_onPointerEnterAction = onPointerEnterAction;
		m_onPointerExitAction = onPointerExitAction;
		m_onPointerClickAction = onPointerClickAction;
		m_selectImageCanvasGroup.gameObject.SetActive(value: false);
		m_storedScale = base.transform.localScale;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (RewiredOnStartupController.CurrentActiveControllerType != ControllerType.Joystick)
		{
			OnSelect(null);
			if (m_onPointerEnterAction != null)
			{
				m_onPointerEnterAction(this);
			}
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (RewiredOnStartupController.CurrentActiveControllerType != ControllerType.Joystick && !DisableClick && eventData.button == PointerEventData.InputButton.Left)
		{
			if (m_onPointerClickAction != null)
			{
				m_onPointerClickAction(this);
			}
			ButtonPressed();
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		if (!m_selected)
		{
			m_selected = true;
			m_selectImageCanvasGroup.gameObject.SetActive(value: true);
			m_selectStartTime = Time.unscaledTime;
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		m_selected = false;
		m_selectImageCanvasGroup.gameObject.SetActive(value: false);
	}

	public void ButtonPressed()
	{
		OnDeselect(null);
		RunOnConfirmPressedAnimation();
		StartCoroutine(OnClickCoroutine());
	}

	private IEnumerator OnClickCoroutine()
	{
		yield return null;
		if (m_onClickAction != null)
		{
			m_onClickAction();
		}
	}

	public void SetOnClickAction(Action action)
	{
		m_onClickAction = action;
	}

	public void SetButtonText(string text, bool isLocID)
	{
		if (isLocID)
		{
			m_text.text = LocalizationManager.GetString(text, isFemale: false);
			m_refreshTextLocID = text;
		}
		else
		{
			m_text.text = text;
			m_refreshTextLocID = null;
		}
	}

	protected virtual void RunOnConfirmPressedAnimation()
	{
		Vector3 storedScale = m_storedScale;
		storedScale.x = ((storedScale.x > 0f) ? 0.9f : (-0.9f));
		storedScale.y = ((storedScale.y > 0f) ? 0.9f : (-0.9f));
		base.transform.localScale = storedScale;
		TweenManager.TweenTo_UnscaledTime(base.transform, 0.05f, Ease.None, "localScale.x", (storedScale.x > 0f) ? 1 : (-1), "localScale.y", (storedScale.y > 0f) ? 1 : (-1));
	}

	private void Update()
	{
		if (m_selected)
		{
			m_selectImageCanvasGroup.alpha = Mathf.Abs(Mathf.Cos((Time.unscaledTime - m_selectStartTime) * 3f));
		}
	}

	public void ResetValues()
	{
		m_selectImageCanvasGroup.gameObject.SetActive(value: false);
		m_selected = false;
		m_onClickAction = null;
		m_text.text = "Button";
		base.gameObject.SetActive(value: false);
		if (m_storedScale != Vector3.zero)
		{
			base.transform.localScale = m_storedScale;
		}
	}

	public void ForceRefreshText()
	{
		if (!string.IsNullOrEmpty(m_refreshTextLocID))
		{
			m_text.text = LocalizationManager.GetString(m_refreshTextLocID, isFemale: false);
		}
	}
}
