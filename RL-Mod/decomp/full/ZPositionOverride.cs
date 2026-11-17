using UnityEngine;

public class ZPositionOverride : MonoBehaviour
{
	[SerializeField]
	private float m_zPosition;

	public void SetZPosition()
	{
		base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, m_zPosition);
	}
}
