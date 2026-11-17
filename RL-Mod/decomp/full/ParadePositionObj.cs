using UnityEngine;

public class ParadePositionObj : MonoBehaviour
{
	private void OnDrawGizmos()
	{
		Vector3 position = base.transform.position;
		float num = 32f;
		float num2 = 18f;
		position.x -= num * 0.5f;
		position.y -= num2 * 0.5f;
		Gizmos.DrawLine(position, new Vector3(position.x + num, position.y, 0f));
		Gizmos.DrawLine(position, new Vector3(position.x, position.y + num2, 0f));
		position.x += num;
		position.y += num2;
		Gizmos.DrawLine(position, new Vector3(position.x, position.y - num2, 0f));
		Gizmos.DrawLine(position, new Vector3(position.x - num, position.y, 0f));
	}
}
