using System.Collections;
using System.Collections.Generic;
using Sigtrap.Relays;
using UnityEngine;

public abstract class DualChoicePropController : BaseSpecialPropController
{
	[SerializeField]
	private GenericInfoTextBox m_leftInfoTextBox;

	[SerializeField]
	private GameObject m_leftBoxBGObj;

	[SerializeField]
	private SpriteRenderer m_leftIcon;

	[SerializeField]
	private GenericInfoTextBox m_rightInfoTextBox;

	[SerializeField]
	private GameObject m_rightBoxBGObj;

	[SerializeField]
	private SpriteRenderer m_rightIcon;

	public Relay<GameObject> LeftHoverRelay = new Relay<GameObject>();

	public Relay<GameObject> RightHoverRelay = new Relay<GameObject>();

	protected float m_startingIconY;

	private float m_startingInfoBoxY;

	private Coroutine m_textBoxLeftInCoroutine;

	private Coroutine m_textBoxLeftOutCoroutine;

	private Coroutine m_textBoxRightInCoroutine;

	private Coroutine m_textBoxRightOutCoroutine;

	private float m_storedLeftTextBoxAlpha;

	private float m_storedRightTextBoxAlpha;

	private List<Interactable> m_leftBoxInteractableList = new List<Interactable>(2);

	private List<Interactable> m_rightBoxInteractableList = new List<Interactable>(2);

	public GenericInfoTextBox LeftInfoTextBox => m_leftInfoTextBox;

	public SpriteRenderer LeftIcon => m_leftIcon;

	public GenericInfoTextBox RightInfoTextBox => m_rightInfoTextBox;

	public SpriteRenderer RightIcon => m_rightIcon;

	protected override void Awake()
	{
		base.Awake();
		m_startingIconY = LeftIcon.transform.localPosition.y;
		m_startingInfoBoxY = m_leftBoxBGObj.transform.localPosition.y;
		if ((bool)LeftInfoTextBox)
		{
			LeftInfoTextBox.GetComponents(m_leftBoxInteractableList);
			LeftInfoTextBox.HideOnPause = true;
		}
		if ((bool)RightInfoTextBox)
		{
			RightInfoTextBox.GetComponents(m_rightBoxInteractableList);
			RightInfoTextBox.HideOnPause = true;
		}
	}

	protected override void InitializePooledPropOnEnter()
	{
		LeftInfoTextBox.CanvasGroup.alpha = 0f;
		RightInfoTextBox.CanvasGroup.alpha = 0f;
		LeftInfoTextBox.gameObject.SetActive(value: true);
		RightInfoTextBox.gameObject.SetActive(value: true);
		m_leftIcon.gameObject.SetActive(value: true);
		m_rightIcon.gameObject.SetActive(value: true);
		foreach (Interactable leftBoxInteractable in m_leftBoxInteractableList)
		{
			leftBoxInteractable.SetIsInteractableActive(isActive: true);
		}
		foreach (Interactable rightBoxInteractable in m_rightBoxInteractableList)
		{
			rightBoxInteractable.SetIsInteractableActive(isActive: true);
		}
	}

	public override void SetRoom(BaseRoom room)
	{
		base.SetRoom(room);
		Canvas[] componentsInChildren = GetComponentsInChildren<Canvas>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].sortingOrder = -1;
		}
	}

	public void DisplayLeftTextBox()
	{
		if (!base.IsDisabled)
		{
			if (m_textBoxLeftInCoroutine != null)
			{
				StopCoroutine(m_textBoxLeftInCoroutine);
			}
			if (m_textBoxLeftOutCoroutine != null)
			{
				StopCoroutine(m_textBoxLeftOutCoroutine);
			}
			TweenManager.StopAllTweensContaining(LeftInfoTextBox.CanvasGroup, runEndHandlers: false);
			TweenManager.StopAllTweensContaining(m_leftBoxBGObj.transform, runEndHandlers: false);
			LeftHoverRelay.Dispatch(m_leftIcon.gameObject);
			m_textBoxLeftInCoroutine = StartCoroutine(TextBoxAnimInCoroutine(LeftInfoTextBox, m_leftBoxBGObj));
		}
	}

	public void DisplayRightTextBox()
	{
		if (!base.IsDisabled)
		{
			if (m_textBoxRightInCoroutine != null)
			{
				StopCoroutine(m_textBoxRightInCoroutine);
			}
			if (m_textBoxRightOutCoroutine != null)
			{
				StopCoroutine(m_textBoxRightOutCoroutine);
			}
			TweenManager.StopAllTweensContaining(RightInfoTextBox.CanvasGroup, runEndHandlers: false);
			TweenManager.StopAllTweensContaining(m_rightBoxBGObj.transform, runEndHandlers: false);
			RightHoverRelay.Dispatch(m_rightIcon.gameObject);
			m_textBoxRightInCoroutine = StartCoroutine(TextBoxAnimInCoroutine(RightInfoTextBox, m_rightBoxBGObj));
		}
	}

	public void HideLeftTextBox()
	{
		if (!base.IsDisabled)
		{
			if (m_textBoxLeftInCoroutine != null)
			{
				StopCoroutine(m_textBoxLeftInCoroutine);
			}
			if (m_textBoxLeftOutCoroutine != null)
			{
				StopCoroutine(m_textBoxLeftOutCoroutine);
			}
			TweenManager.StopAllTweensContaining(LeftInfoTextBox.CanvasGroup, runEndHandlers: false);
			TweenManager.StopAllTweensContaining(m_leftBoxBGObj.transform, runEndHandlers: false);
			m_textBoxLeftOutCoroutine = StartCoroutine(TextBoxAnimOutCoroutine(LeftInfoTextBox, m_leftBoxBGObj));
		}
	}

	public void HideRightTextBox()
	{
		if (!base.IsDisabled)
		{
			if (m_textBoxRightInCoroutine != null)
			{
				StopCoroutine(m_textBoxRightInCoroutine);
			}
			if (m_textBoxRightOutCoroutine != null)
			{
				StopCoroutine(m_textBoxRightOutCoroutine);
			}
			TweenManager.StopAllTweensContaining(RightInfoTextBox.CanvasGroup, runEndHandlers: false);
			TweenManager.StopAllTweensContaining(m_rightBoxBGObj.transform, runEndHandlers: false);
			m_textBoxRightOutCoroutine = StartCoroutine(TextBoxAnimOutCoroutine(RightInfoTextBox, m_rightBoxBGObj));
		}
	}

	private IEnumerator TextBoxAnimInCoroutine(GenericInfoTextBox textBox, GameObject textBoxNoHB)
	{
		textBox.CanvasGroup.alpha = 0f;
		TweenManager.TweenTo(textBox.CanvasGroup, 0.15f, Ease.None, "alpha", 1);
		Vector3 localPosition = textBoxNoHB.transform.localPosition;
		localPosition.y = m_startingInfoBoxY - 0.5f;
		textBoxNoHB.transform.localPosition = localPosition;
		yield return TweenManager.TweenBy(textBoxNoHB.transform, 0.15f, Ease.Quad.EaseOut, "localPosition.y", 0.5f).TweenCoroutine;
	}

	private IEnumerator TextBoxAnimOutCoroutine(GenericInfoTextBox textBox, GameObject textBoxNoHB)
	{
		textBox.CanvasGroup.alpha = 1f;
		TweenManager.TweenTo(textBox.CanvasGroup, 0.15f, Ease.None, "alpha", 0);
		yield return TweenManager.TweenBy(textBoxNoHB.transform, 0.15f, Ease.Quad.EaseIn, "localPosition.y", 0.5f).TweenCoroutine;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (!GameManager.IsApplicationClosing && TweenManager.IsInitialized)
		{
			TweenManager.StopAllTweensContaining(LeftInfoTextBox.CanvasGroup, runEndHandlers: false);
			TweenManager.StopAllTweensContaining(m_leftBoxBGObj.transform, runEndHandlers: false);
			TweenManager.StopAllTweensContaining(RightInfoTextBox.CanvasGroup, runEndHandlers: false);
			TweenManager.StopAllTweensContaining(m_rightBoxBGObj.transform, runEndHandlers: false);
		}
	}

	private IEnumerator DisableTextboxAnimOutCoroutine()
	{
		m_leftIcon.gameObject.SetActive(value: false);
		m_rightIcon.gameObject.SetActive(value: false);
		TweenManager.TweenTo(LeftInfoTextBox.CanvasGroup, 0.15f, Ease.None, "alpha", 0);
		TweenManager.TweenBy(m_leftBoxBGObj.transform, 0.15f, Ease.Quad.EaseIn, "localPosition.y", 0.5f);
		TweenManager.TweenTo(RightInfoTextBox.CanvasGroup, 0.15f, Ease.None, "alpha", 0);
		yield return TweenManager.TweenBy(m_rightBoxBGObj.transform, 0.15f, Ease.Quad.EaseIn, "localPosition.y", 0.5f).TweenCoroutine;
		LeftInfoTextBox.gameObject.SetActive(value: false);
		RightInfoTextBox.gameObject.SetActive(value: false);
	}

	protected override void DisableProp(bool firstTimeDisabled)
	{
		foreach (Interactable leftBoxInteractable in m_leftBoxInteractableList)
		{
			leftBoxInteractable.SetIsInteractableActive(isActive: false);
		}
		foreach (Interactable rightBoxInteractable in m_rightBoxInteractableList)
		{
			rightBoxInteractable.SetIsInteractableActive(isActive: false);
		}
		if (firstTimeDisabled && !base.IsDisabled)
		{
			if (m_textBoxLeftInCoroutine != null)
			{
				StopCoroutine(m_textBoxLeftInCoroutine);
			}
			if (m_textBoxLeftOutCoroutine != null)
			{
				StopCoroutine(m_textBoxLeftOutCoroutine);
			}
			if (m_textBoxRightInCoroutine != null)
			{
				StopCoroutine(m_textBoxRightInCoroutine);
			}
			if (m_textBoxRightOutCoroutine != null)
			{
				StopCoroutine(m_textBoxRightOutCoroutine);
			}
			TweenManager.StopAllTweensContaining(RightInfoTextBox.CanvasGroup, runEndHandlers: false);
			TweenManager.StopAllTweensContaining(m_rightBoxBGObj.transform, runEndHandlers: false);
			TweenManager.StopAllTweensContaining(LeftInfoTextBox.CanvasGroup, runEndHandlers: false);
			TweenManager.StopAllTweensContaining(m_leftBoxBGObj.transform, runEndHandlers: false);
			StartCoroutine(DisableTextboxAnimOutCoroutine());
		}
		else
		{
			m_leftIcon.gameObject.SetActive(value: false);
			m_rightIcon.gameObject.SetActive(value: false);
			LeftInfoTextBox.gameObject.SetActive(value: false);
			RightInfoTextBox.gameObject.SetActive(value: false);
		}
		base.DisableProp(firstTimeDisabled);
	}

	protected virtual void Update()
	{
		bool flag = m_leftIcon;
		bool flag2 = m_rightIcon;
		if (flag || flag2)
		{
			float y = m_startingIconY + Mathf.Sin(Time.timeSinceLevelLoad * 2f) / 8f;
			if (flag)
			{
				Vector3 localPosition = m_leftIcon.transform.localPosition;
				localPosition.y = y;
				m_leftIcon.gameObject.transform.localPosition = localPosition;
			}
			if (flag2)
			{
				Vector3 localPosition2 = m_rightIcon.transform.localPosition;
				localPosition2.y = y;
				m_rightIcon.gameObject.transform.localPosition = localPosition2;
			}
		}
	}
}
