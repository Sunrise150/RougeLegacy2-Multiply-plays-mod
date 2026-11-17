using UnityEngine;

public class SoloCameraController : MonoBehaviour
{
	[SerializeField]
	private Camera m_camera;

	private bool m_isInitialized;

	private float m_defaultDepth;

	public Camera Camera => m_camera;

	private void Awake()
	{
		Initialize();
		base.gameObject.SetActive(value: false);
	}

	public void AddToCameraLayer(GameObject obj)
	{
		obj.SetLayerRecursively(28);
		foreach (BaseTrait activeTrait in TraitManager.ActiveTraitList)
		{
			if (activeTrait.TraitMask != null)
			{
				activeTrait.TraitMask.gameObject.gameObject.layer = 27;
			}
		}
	}

	private void Initialize()
	{
		m_defaultDepth = m_camera.depth;
		m_isInitialized = true;
	}

	public void ResetController()
	{
		if (m_isInitialized)
		{
			SetCameraDepth(m_defaultDepth);
		}
	}

	public void SetCameraDepth(float depth)
	{
		if (!m_isInitialized)
		{
			Initialize();
		}
		m_camera.depth = depth;
	}
}
