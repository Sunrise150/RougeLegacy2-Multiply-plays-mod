using System.Collections;
using UnityEngine;

public class Wait_ParadeAction : MonoBehaviour, IParadeAction
{
	[SerializeField]
	private float m_waitDuration;

	public IEnumerator TriggerAction(ParadeController controller)
	{
		float totalWaitDuration2 = Mathf.Min(m_waitDuration, 0.1f);
		totalWaitDuration2 += Time.time;
		while (Time.time < totalWaitDuration2)
		{
			yield return null;
		}
	}
}
