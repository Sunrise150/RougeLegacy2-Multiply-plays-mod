using UnityEngine;

public static class CDGTransformExtensions
{
	public static void SetPositionX(this Transform transform, float xPos)
	{
		Vector3 position = transform.position;
		position.x = xPos;
		transform.position = position;
	}

	public static void SetPositionY(this Transform transform, float yPos)
	{
		Vector3 position = transform.position;
		position.y = yPos;
		transform.position = position;
	}

	public static void SetPositionZ(this Transform transform, float zPos)
	{
		Vector3 position = transform.position;
		position.z = zPos;
		transform.position = position;
	}

	public static void SetLocalPositionX(this Transform transform, float xPos)
	{
		Vector3 localPosition = transform.localPosition;
		localPosition.x = xPos;
		transform.localPosition = localPosition;
	}

	public static void SetLocalPositionY(this Transform transform, float yPos)
	{
		Vector3 localPosition = transform.localPosition;
		localPosition.y = yPos;
		transform.localPosition = localPosition;
	}

	public static void SetLocalPositionZ(this Transform transform, float zPos)
	{
		Vector3 localPosition = transform.localPosition;
		localPosition.z = zPos;
		transform.localPosition = localPosition;
	}

	public static void SetLocalEulerX(this Transform transform, float xRot)
	{
		Vector3 localEulerAngles = transform.localEulerAngles;
		localEulerAngles.x = xRot;
		transform.localEulerAngles = localEulerAngles;
	}

	public static void SetLocalEulerY(this Transform transform, float yRot)
	{
		Vector3 localEulerAngles = transform.localEulerAngles;
		localEulerAngles.y = yRot;
		transform.localEulerAngles = localEulerAngles;
	}

	public static void SetLocalEulerZ(this Transform transform, float zRot)
	{
		Vector3 localEulerAngles = transform.localEulerAngles;
		localEulerAngles.z = zRot;
		transform.localEulerAngles = localEulerAngles;
	}

	public static void SetLocalScaleX(this Transform transform, float xScale)
	{
		Vector3 localScale = transform.localScale;
		localScale.x = xScale;
		transform.localScale = localScale;
	}

	public static void SetLocalScaleY(this Transform transform, float yScale)
	{
		Vector3 localScale = transform.localScale;
		localScale.y = yScale;
		transform.localScale = localScale;
	}

	public static void SetLocalScaleZ(this Transform transform, float zScale)
	{
		Vector3 localScale = transform.localScale;
		localScale.z = zScale;
		transform.localScale = localScale;
	}

	public static void SetLocalRotationX(this Transform transform, float xRotation)
	{
		Quaternion localRotation = transform.localRotation;
		localRotation.x = xRotation;
		transform.localRotation = localRotation;
	}

	public static void SetLocalRotationY(this Transform transform, float yRotation)
	{
		Quaternion localRotation = transform.localRotation;
		localRotation.y = yRotation;
		transform.localRotation = localRotation;
	}

	public static void SetLocalRotationZ(this Transform transform, float zRotation)
	{
		Quaternion localRotation = transform.localRotation;
		localRotation.z = zRotation;
		transform.localRotation = localRotation;
	}

	public static void SetLocalRotationW(this Transform transform, float wRotation)
	{
		Quaternion localRotation = transform.localRotation;
		localRotation.w = wRotation;
		transform.localRotation = localRotation;
	}
}
