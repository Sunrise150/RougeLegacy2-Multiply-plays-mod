using System.Collections;
using UnityEngine;

public class MoveCamera_ParadeAction : MonoBehaviour, IParadeAction
{
	[SerializeField]
	private Transform m_camPos;

	[SerializeField]
	private float m_duration = 1f;

	[SerializeField]
	private bool m_waitTillMovementComplete;

	public IEnumerator TriggerAction(ParadeController controller)
	{
		float duration = m_duration;
		duration = Mathf.Min(duration, 2f);
		if (m_waitTillMovementComplete)
		{
			yield return TweenManager.TweenTo(controller.VirtualCamera.transform, duration, Ease.Quad.EaseInOut, "localPosition.x", m_camPos.position.x, "localPosition.y", m_camPos.position.y);
		}
		else
		{
			TweenManager.TweenTo(controller.VirtualCamera.transform, duration, Ease.Quad.EaseInOut, "localPosition.x", m_camPos.position.x, "localPosition.y", m_camPos.position.y);
		}
	}
}
