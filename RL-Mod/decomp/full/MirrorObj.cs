using UnityEngine;

public class MirrorObj : MonoBehaviour, IMirror
{
	[SerializeField]
	private bool m_mirrorZRotation;

	[SerializeField]
	private bool m_mirrorXPosition;

	[SerializeField]
	private bool m_mirrorYPosition;

	[SerializeField]
	private bool m_mirrorXScale;

	[SerializeField]
	private bool m_mirrorYScale;

	public void Mirror()
	{
		Vector3 localEulerAngles = base.gameObject.transform.localEulerAngles;
		Vector3 localPosition = base.gameObject.transform.localPosition;
		Vector3 localScale = base.gameObject.transform.localScale;
		if (m_mirrorZRotation)
		{
			localEulerAngles.z += 180f;
		}
		if (m_mirrorXPosition)
		{
			localPosition.x = 0f - localPosition.x;
		}
		if (m_mirrorYPosition)
		{
			localPosition.y = 0f - localPosition.y;
		}
		if (m_mirrorXScale)
		{
			localScale.x = 0f - localScale.x;
		}
		if (m_mirrorYScale)
		{
			localScale.y = 0f - localScale.y;
		}
		base.gameObject.transform.localEulerAngles = localEulerAngles;
		base.gameObject.transform.localPosition = localPosition;
		base.gameObject.transform.localScale = localScale;
	}
}
