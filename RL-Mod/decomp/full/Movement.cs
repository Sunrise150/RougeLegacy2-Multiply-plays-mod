using UnityEngine;

public class Movement : MonoBehaviour
{
	public Vector3 movementVector;

	private void Update()
	{
		base.transform.position += movementVector * Time.deltaTime;
	}
}
