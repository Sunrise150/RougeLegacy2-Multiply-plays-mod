using System.Collections.Generic;
using UnityEngine;

public class StatusBarController : MonoBehaviour
{
	[SerializeField]
	private bool m_disableAutoPosition;

	[SerializeField]
	private float m_baseScaleOverride;

	private Canvas m_canvas;

	private List<StatusBarEntry> m_activeStatusBarList = new List<StatusBarEntry>();

	public bool Active { get; set; } = true;


	public bool HasActiveStatusBarEntry(StatusBarEntryType uiType)
	{
		foreach (StatusBarEntry activeStatusBar in m_activeStatusBarList)
		{
			if (activeStatusBar.isActiveAndEnabled && activeStatusBar.StatusBarType == uiType)
			{
				return true;
			}
		}
		return false;
	}

	public StatusBarEntry GetActiveStatusBarEntry(StatusBarEntryType uiType)
	{
		foreach (StatusBarEntry activeStatusBar in m_activeStatusBarList)
		{
			if (activeStatusBar.isActiveAndEnabled && activeStatusBar.StatusBarType == uiType)
			{
				return activeStatusBar;
			}
		}
		return null;
	}

	private void Awake()
	{
		m_canvas = GetComponent<Canvas>();
		m_canvas.sortingOrder = 1;
		base.gameObject.SetActive(value: false);
	}

	public void ResetPositionAndScale(BaseCharacterController charController)
	{
		base.transform.localScale = Vector3.one;
		Vector3 lossyScale = base.transform.lossyScale;
		float num = m_baseScaleOverride;
		if (m_baseScaleOverride == 0f)
		{
			num = 1.15f;
		}
		base.transform.localScale = new Vector3(num / lossyScale.x, num / lossyScale.y, 1f);
		if (!m_disableAutoPosition && (bool)charController)
		{
			float y = charController.VisualBounds.max.y + 1f;
			base.transform.position = new Vector3(base.transform.position.x, y, base.transform.position.z);
		}
		base.transform.eulerAngles = Vector3.zero;
	}

	private void OnDisable()
	{
		foreach (StatusBarEntry activeStatusBar in m_activeStatusBarList)
		{
			activeStatusBar.gameObject.SetActive(value: false);
		}
		m_activeStatusBarList.Clear();
		SetCanvasVisible(visible: true);
	}

	public void ApplyUIEffect(StatusBarEntryType uiType)
	{
		if (Active)
		{
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: true);
			}
			GetInitializedStatusBarEntry(uiType).StartNoCounterOrTimer(uiType);
		}
	}

	public void ApplyUIEffect(StatusBarEntryType uiType, float duration)
	{
		if (Active)
		{
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: true);
			}
			if (base.gameObject.activeInHierarchy)
			{
				GetInitializedStatusBarEntry(uiType).StartTimer(uiType, duration);
			}
		}
	}

	public void ApplyUIEffect(StatusBarEntryType uiType, int maxCount, int currentCount)
	{
		if (Active)
		{
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: true);
			}
			GetInitializedStatusBarEntry(uiType).StartCounter(uiType, maxCount, currentCount);
		}
	}

	public void ApplyUIEffect(StatusBarEntryType uiType, float duration, int maxCount, int currentCount)
	{
		if (Active)
		{
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: true);
			}
			if (base.gameObject.activeInHierarchy)
			{
				GetInitializedStatusBarEntry(uiType).StartCounterAndTimer(uiType, duration, maxCount, currentCount);
			}
		}
	}

	private StatusBarEntry GetInitializedStatusBarEntry(StatusBarEntryType uiType)
	{
		StatusBarEntry statusBarEntry = GetActiveStatusBarEntry(uiType);
		if (!statusBarEntry)
		{
			statusBarEntry = StatusBarEntryManager.GetFreeStatusBarEntry();
			statusBarEntry.gameObject.SetActive(value: true);
			statusBarEntry.transform.SetParent(base.transform);
			statusBarEntry.ResetScale();
			statusBarEntry.ResetPosition();
			m_activeStatusBarList.Add(statusBarEntry);
		}
		return statusBarEntry;
	}

	public void StopUIEffect(StatusBarEntryType uiType)
	{
		foreach (StatusBarEntry activeStatusBar in m_activeStatusBarList)
		{
			if (activeStatusBar.isActiveAndEnabled && activeStatusBar.StatusBarType == uiType)
			{
				activeStatusBar.StopTimer();
				activeStatusBar.gameObject.SetActive(value: false);
			}
		}
	}

	public void StopAllUIEffects()
	{
		foreach (StatusBarEntry activeStatusBar in m_activeStatusBarList)
		{
			if (activeStatusBar.isActiveAndEnabled)
			{
				activeStatusBar.StopTimer();
				activeStatusBar.gameObject.SetActive(value: false);
			}
		}
	}

	private void Update()
	{
		if (m_activeStatusBarList.Count > 0)
		{
			for (int i = 0; i < m_activeStatusBarList.Count; i++)
			{
				if (!m_activeStatusBarList[i] || !m_activeStatusBarList[i].gameObject.activeSelf)
				{
					m_activeStatusBarList.RemoveAt(i);
					i--;
				}
			}
		}
		else if (base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public void RemoveStatusBarEntry(StatusBarEntry statusBar)
	{
		m_activeStatusBarList.Remove(statusBar);
	}

	public void SetCanvasVisible(bool visible)
	{
		m_canvas.enabled = visible;
	}
}
