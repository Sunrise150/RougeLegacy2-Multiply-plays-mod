using Cinemachine;
using UnityEngine;

public class CameraOffsetOverrideController : MonoBehaviour
{
	[Tooltip("Ranges from -0.5f to 1.5f (set by Cinemachine)")]
	public Vector2 Offset;

	private Vector2 m_storedOffset;

	private CinemachineFramingTransposer m_framingTransposer;

	private bool m_isInitialized;

	private void Initialize(CinemachineVirtualCamera vcam)
	{
		if (!m_isInitialized)
		{
			m_framingTransposer = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
			if ((bool)m_framingTransposer)
			{
				m_storedOffset = new Vector2(m_framingTransposer.m_ScreenX, m_framingTransposer.m_ScreenY);
			}
			m_isInitialized = true;
		}
	}

	public void ApplyOffset(CinemachineVirtualCamera vcam)
	{
		Initialize(vcam);
		if ((bool)m_framingTransposer)
		{
			m_framingTransposer.m_ScreenX = m_storedOffset.x + Offset.x;
			m_framingTransposer.m_ScreenY = m_storedOffset.y + Offset.y;
		}
	}

	public void ResetOffset()
	{
		if ((bool)m_framingTransposer)
		{
			m_framingTransposer.m_ScreenX = m_storedOffset.x;
			m_framingTransposer.m_ScreenY = m_storedOffset.y;
		}
	}
}
