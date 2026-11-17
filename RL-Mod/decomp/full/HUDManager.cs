using System.Collections;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
	[SerializeField]
	private Canvas m_canvas;

	[SerializeField]
	private float m_planeDistance = 150f;

	private IEnumerator Start()
	{
		while (!CameraController.IsInstantiated)
		{
			yield return null;
		}
		m_canvas.renderMode = RenderMode.ScreenSpaceCamera;
		m_canvas.worldCamera = CameraController.UICamera;
		m_canvas.planeDistance = m_planeDistance;
	}
}
