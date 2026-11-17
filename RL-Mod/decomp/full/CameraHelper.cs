using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraHelper : MonoBehaviour
{
	private Camera m_camera;

	private void Awake()
	{
		m_camera = GetComponent<Camera>();
		m_camera.eventMask = 0;
	}
}
