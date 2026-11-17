using UnityEngine;
using UnityEngine.UI;

public class ScrollRectAutoScroller : MonoBehaviour
{
	[SerializeField]
	private ScrollRect m_scrollRect;

	private Scrollbar m_scrollBar;

	private RectTransform m_viewportRectTransform;

	private RectTransform m_contentRectTransform;

	private int m_arraySize;

	private void Awake()
	{
		m_scrollBar = m_scrollRect.verticalScrollbar;
		m_contentRectTransform = m_scrollRect.content;
		m_viewportRectTransform = m_scrollRect.viewport;
	}

	public void ConfigureScrollBar(int arraySize)
	{
		m_arraySize = arraySize;
	}

	public void SetSelectedIndex(int selectedEntryIndex)
	{
		AutoScroll(selectedEntryIndex);
	}

	protected virtual void AutoScroll(int selectedEntryIndex)
	{
		float height = m_contentRectTransform.rect.height;
		float num = 1f - m_scrollBar.value;
		float height2 = m_viewportRectTransform.rect.height;
		if (height2 == 0f)
		{
			m_scrollBar.value = 1f;
			return;
		}
		float num2 = m_contentRectTransform.rect.height / (float)m_arraySize;
		float num3 = height2;
		float num4 = num3 - height2;
		if (num > 0f)
		{
			num3 = height2 + (height - height2) * num;
			num4 = num3 - height2;
		}
		float num5 = (float)(selectedEntryIndex + 1) * num2;
		float num6 = num5 - num2;
		float duration = 0.1f;
		if (num6 < num4)
		{
			float num7 = 1f - num6 / (height - height2);
			if (num7 >= 1f || num7 <= 0f)
			{
				m_scrollBar.value = Mathf.Clamp(num7, 0f, 1f);
				return;
			}
			TweenManager.TweenTo_UnscaledTime(m_scrollBar, duration, Ease.Quad.EaseInOut, "value", Mathf.Clamp(num7, 0f, 1f));
		}
		else if (num5 > num3)
		{
			float num8 = 1f - (num5 - height2) / (height - height2);
			if (num8 >= 1f || num8 <= 0f)
			{
				m_scrollBar.value = Mathf.Clamp(num8, 0f, 1f);
				return;
			}
			TweenManager.TweenTo_UnscaledTime(m_scrollBar, duration, Ease.Quad.EaseInOut, "value", Mathf.Clamp(num8, 0f, 1f));
		}
	}
}
