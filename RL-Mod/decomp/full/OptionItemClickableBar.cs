using UnityEngine;
using UnityEngine.EventSystems;

public class OptionItemClickableBar : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerEnterHandler, IPointerClickHandler
{
	private RectTransform m_clickableRect;

	private Canvas m_canvas;

	private bool m_pointerDown;

	private bool m_previousPointerDown;

	private IncrementDecrementOptionItem m_incrementOptionItem;

	private void Awake()
	{
		m_incrementOptionItem = GetComponentInParent<IncrementDecrementOptionItem>();
		m_clickableRect = GetComponent<RectTransform>();
		m_canvas = GetComponentInParent<Canvas>();
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		m_pointerDown = false;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (m_incrementOptionItem.Interactable && m_incrementOptionItem.IsActivated)
		{
			m_pointerDown = true;
			Vector3 vector = CameraController.UICamera.WorldToScreenPoint(new Vector3(m_clickableRect.position.x, m_clickableRect.position.y, CameraController.UICamera.transform.position.z));
			float num = m_clickableRect.sizeDelta.x * m_clickableRect.localScale.x * m_canvas.scaleFactor;
			float num2 = vector.x - num / 2f;
			float value = (eventData.position.x - num2) / num;
			m_incrementOptionItem.SetCurrentIncrementValue(value, useNormalizedValue: true);
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		m_incrementOptionItem.OnPointerClick(eventData);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		m_incrementOptionItem.OnPointerEnter(eventData);
	}

	public void Update()
	{
		if ((bool)m_incrementOptionItem && m_incrementOptionItem.Interactable && m_incrementOptionItem.IsActivated && m_pointerDown)
		{
			Vector3 vector = CameraController.UICamera.WorldToScreenPoint(new Vector3(m_clickableRect.position.x, m_clickableRect.position.y, CameraController.UICamera.transform.position.z));
			float num = m_clickableRect.sizeDelta.x * m_clickableRect.localScale.x * m_canvas.scaleFactor;
			float num2 = vector.x - num / 2f;
			float value = (Input.mousePosition.x - num2) / num;
			m_incrementOptionItem.SetCurrentIncrementValue(value, useNormalizedValue: true);
		}
	}
}
