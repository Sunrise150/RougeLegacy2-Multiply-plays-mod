using UnityEngine;

public class CameraZoomController : MonoBehaviour
{
	[SerializeField]
	private float m_zoomLevel = 1f;

	[SerializeField]
	private bool m_overrideZoomLevel;

	public bool OverrideZoomLevel
	{
		get
		{
			return m_overrideZoomLevel;
		}
		set
		{
			if (!Application.isPlaying)
			{
				m_overrideZoomLevel = value;
			}
		}
	}

	public float ZoomLevel
	{
		get
		{
			if (OverrideZoomLevel)
			{
				return m_zoomLevel;
			}
			return 1f;
		}
		set
		{
			if (OverrideZoomLevel && !Application.isPlaying)
			{
				m_zoomLevel = Mathf.Clamp(value, 1f, 10f);
			}
		}
	}

	public void SetZoomLevel(float zoomLevel)
	{
		if (!OverrideZoomLevel)
		{
			m_overrideZoomLevel = true;
			m_zoomLevel = zoomLevel;
		}
	}
}
