using UnityEngine;

public class InteractIconController : MonoBehaviour
{
	[SerializeField]
	private GameObject m_interactIcon;

	private Tween m_iconDisplayTween;

	private Vector3 m_storedScale;

	public bool IsIconVisible { get; private set; }

	private void Awake()
	{
		m_storedScale = m_interactIcon.transform.localScale;
		m_interactIcon.SetActive(value: false);
	}

	public void SetIconVisible(bool visible)
	{
		if (visible)
		{
			if (!IsIconVisible)
			{
				m_interactIcon.SetActive(value: true);
				m_interactIcon.transform.localScale = new Vector3(m_storedScale.x * 0.5f, m_storedScale.y * 0.5f, m_storedScale.z * 0.5f);
				if (m_iconDisplayTween != null)
				{
					m_iconDisplayTween.StopTweenWithConditionChecks(this, m_interactIcon.transform);
				}
				m_iconDisplayTween = TweenManager.TweenTo(m_interactIcon.transform, 0.15f, Ease.Back.EaseOutLarge, "localScale.x", m_storedScale.x, "localScale.y", m_storedScale.y, "localScale.z", m_storedScale.z);
				IsIconVisible = true;
			}
		}
		else if (IsIconVisible)
		{
			if (m_iconDisplayTween != null)
			{
				m_iconDisplayTween.StopTweenWithConditionChecks(this, m_interactIcon.transform);
			}
			m_interactIcon.SetActive(value: false);
			IsIconVisible = false;
		}
	}
}
