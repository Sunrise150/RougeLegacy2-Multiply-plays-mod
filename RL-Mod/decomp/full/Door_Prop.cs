using UnityEngine;

public class Door_Prop : Prop, IMirror
{
	public override void Mirror()
	{
		float z = base.transform.rotation.eulerAngles.z;
		z *= -1f;
		Quaternion rotation = Quaternion.Euler(0f, 0f, z);
		base.transform.rotation = rotation;
	}
}
