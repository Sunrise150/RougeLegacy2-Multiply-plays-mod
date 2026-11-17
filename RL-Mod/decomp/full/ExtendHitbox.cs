using System.Collections;
using UnityEngine;

public class ExtendHitbox : MonoBehaviour
{
	[SerializeField]
	private HitboxType m_hitboxTypeToAlter;

	[SerializeField]
	private float m_deltaWidth;

	[SerializeField]
	private float m_deltaHeight;

	private IEnumerator Start()
	{
		IHitboxController hbController = this.GetRoot().GetComponentInChildren<IHitboxController>();
		while (!hbController.IsInitialized)
		{
			yield return null;
		}
		BoxCollider2D boxCollider2D = hbController.GetCollider(m_hitboxTypeToAlter) as BoxCollider2D;
		if (boxCollider2D != null)
		{
			Vector3 localEulerAngles = boxCollider2D.transform.localEulerAngles;
			boxCollider2D.transform.localEulerAngles = Vector3.zero;
			Vector2 size = boxCollider2D.size;
			size.x += m_deltaWidth;
			size.y += m_deltaHeight;
			boxCollider2D.size = size;
			boxCollider2D.transform.localEulerAngles = localEulerAngles;
		}
	}
}
