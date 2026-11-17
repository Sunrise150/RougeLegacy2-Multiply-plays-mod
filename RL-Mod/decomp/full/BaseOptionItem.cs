using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class BaseOptionItem : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, ISelectHandler, IDeselectHandler, IPointerClickHandler, ILocalizable
{
	protected static Color ACTIVATED_COLOR = new Color(1f, 0.88235295f, 0f);

	protected static Color SELECTED_COLOR = Color.white;

	protected static Color UNSELECTED_COLOR = new Color(0.35686275f, 0.23921569f, 0.28627452f);

	[SerializeField]
	protected TMP_Text m_incrementValueText;

	[SerializeField]
	protected TMP_Text m_titleText;

	[SerializeField]
	protected bool m_isSuboptionItem;

	[SerializeField]
	[ConditionalHide("m_isSuboptionItem", true)]
	protected GameObject m_selectedBG;

	[SerializeField]
	[ConditionalHide("m_isSuboptionItem", true)]
	protected GameObject m_selectedIndicator;

	[SerializeField]
	[ConditionalHide("m_isSuboptionItem", true)]
	protected GameObject m_leftArrow;

	[SerializeField]
	[ConditionalHide("m_isSuboptionItem", true)]
	protected GameObject m_rightArrow;

	[SerializeField]
	[ConditionalHide("m_isSuboptionItem", true)]
	protected GameObject m_incrementBar;

	[SerializeField]
	[ConditionalHide("m_isSuboptionItem", true)]
	protected RectTransform m_incrementMaskRectTransform;

	[SerializeField]
	[ConditionalHide("m_isSuboptionItem", true)]
	protected GameObject m_disclaimerObj;

	private Action<MonoBehaviour, EventArgs> m_refreshText;

	private Image m_bgImage;

	public TMP_Text IncrementValueText => m_incrementValueText;

	public OptionItemSelectedHandler OptionItemSelected { get; set; }

	public OptionItemSelectedHandler OptionItemActivated { get; set; }

	public OptionItemSelectedHandler OptionItemDeactivated { get; set; }

	public bool HasArrows => m_leftArrow != null;

	public bool IsInitialized { get; private set; }

	public bool Interactable { get; set; }

	public bool IsActivated { get; protected set; }

	public abstract bool PressAndHoldEnabled { get; }

	public abstract OptionsControlType OptionsControlType { get; }

	protected virtual void Awake()
	{
		m_refreshText = RefreshText;
	}

	protected virtual void OnEnable()
	{
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.LanguageChanged, m_refreshText);
	}

	protected virtual void OnDisable()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.LanguageChanged, m_refreshText);
	}

	public virtual void RefreshText(object sender, EventArgs args)
	{
		Initialize();
	}

	public virtual void InvokeIncrement()
	{
		if (m_isSuboptionItem && HasArrows)
		{
			m_rightArrow.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
			TweenManager.TweenTo_UnscaledTime(m_rightArrow.transform, 0.05f, Ease.None, "localScale.x", 1, "localScale.y", 1);
		}
	}

	public virtual void InvokeDecrement()
	{
		if (m_isSuboptionItem && HasArrows)
		{
			m_leftArrow.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
			TweenManager.TweenTo_UnscaledTime(m_leftArrow.transform, 0.05f, Ease.None, "localScale.x", 1, "localScale.y", 1);
		}
	}

	public virtual void Initialize()
	{
		if (m_isSuboptionItem)
		{
			m_bgImage = m_selectedBG.GetComponent<Image>();
			Color color = m_bgImage.color;
			color.a = 0.5f;
			m_bgImage.color = color;
			m_selectedBG.SetActive(value: false);
			m_selectedIndicator.SetActive(value: false);
			if (HasArrows)
			{
				m_leftArrow.SetActive(value: false);
				m_rightArrow.SetActive(value: false);
			}
			if ((bool)m_disclaimerObj)
			{
				m_disclaimerObj.SetActive(value: false);
			}
		}
		IsInitialized = true;
	}

	public virtual void OnPointerClick(PointerEventData eventData)
	{
		if (Interactable && !IsActivated && eventData.button == PointerEventData.InputButton.Left)
		{
			OnSelect(eventData);
			ActivateOption();
		}
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		OnSelect(eventData);
	}

	public virtual void OnSelect(BaseEventData eventData)
	{
		if (!Interactable || IsActivated)
		{
			return;
		}
		if (m_isSuboptionItem)
		{
			m_titleText.color = SELECTED_COLOR;
			m_incrementValueText.color = SELECTED_COLOR;
			m_selectedBG.gameObject.SetActive(value: true);
			m_selectedIndicator.SetActive(value: true);
			if ((bool)m_disclaimerObj)
			{
				m_disclaimerObj.SetActive(value: true);
			}
		}
		if (OptionItemSelected != null)
		{
			OptionItemSelected(this);
		}
	}

	public virtual void OnDeselect(BaseEventData eventData)
	{
		if (m_isSuboptionItem)
		{
			m_titleText.color = UNSELECTED_COLOR;
			m_incrementValueText.color = UNSELECTED_COLOR;
			m_selectedBG.gameObject.SetActive(value: false);
			m_selectedIndicator.SetActive(value: false);
			if ((bool)m_disclaimerObj)
			{
				m_disclaimerObj.SetActive(value: false);
			}
		}
	}

	public virtual void ActivateOption()
	{
		IsActivated = true;
		if (m_isSuboptionItem)
		{
			m_titleText.color = ACTIVATED_COLOR;
			m_incrementValueText.color = ACTIVATED_COLOR;
			Color color = m_bgImage.color;
			color.a = 1f;
			m_bgImage.color = color;
			if (HasArrows)
			{
				m_leftArrow.SetActive(value: true);
				m_rightArrow.SetActive(value: true);
			}
		}
		if (OptionItemActivated != null)
		{
			OptionItemActivated(this);
		}
	}

	public abstract void ConfirmOptionChange();

	public abstract void CancelOptionChange();

	public virtual void DeactivateOption(bool confirmOptionChange)
	{
		if (m_isSuboptionItem)
		{
			m_titleText.color = SELECTED_COLOR;
			m_incrementValueText.color = SELECTED_COLOR;
			Color color = m_bgImage.color;
			color.a = 0.5f;
			m_bgImage.color = color;
			if (HasArrows)
			{
				m_leftArrow.SetActive(value: false);
				m_rightArrow.SetActive(value: false);
			}
		}
		ConfirmOptionChange();
		IsActivated = false;
		if (OptionItemDeactivated != null)
		{
			OptionItemDeactivated(this);
		}
	}
}
