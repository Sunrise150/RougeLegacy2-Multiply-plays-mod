using UnityEngine;

namespace RLAudio;

public class Surface : MonoBehaviour
{
	[SerializeField]
	private SurfaceType m_surfaceType;

	public SurfaceType SurfaceType => m_surfaceType;
}
