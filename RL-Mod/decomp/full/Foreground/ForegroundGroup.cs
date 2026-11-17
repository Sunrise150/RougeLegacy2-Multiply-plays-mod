using UnityEngine;

namespace Foreground;

public class ForegroundGroup : MonoBehaviour
{
	[SerializeField]
	[Range(1f, 2f)]
	private float m_zoomLevel = 1f;

	private PropSpawnController[] m_propSpawnControllers;

	public PropSpawnController[] PropSpawnControllers => m_propSpawnControllers;

	public float ZoomLevel
	{
		get
		{
			return m_zoomLevel;
		}
		set
		{
			if (!Application.isPlaying)
			{
				m_zoomLevel = value;
			}
		}
	}

	private void Awake()
	{
		m_propSpawnControllers = GetComponentsInChildren<PropSpawnController>();
	}
}
