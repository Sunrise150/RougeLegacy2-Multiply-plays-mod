using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseTabButton : Button
{
	[SerializeField]
	private WindowID m_windowToDisplay;

	public PauseTabSelectedHandler OnSelectEvent { get; set; }

	public WindowID WindowToDisplay
	{
		get
		{
			return m_windowToDisplay;
		}
		set
		{
			m_windowToDisplay = value;
		}
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		if (OnSelectEvent != null)
		{
			OnSelectEvent(this);
		}
	}
}
