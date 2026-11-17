using UnityEngine;

namespace RLAudio;

public class SurfaceTrigger : MonoBehaviour, ITerrainOnEnterHitResponse, IHitResponse, ITerrainOnExitHitResponse
{
	[SerializeField]
	private SurfaceType m_surfaceType;

	public void TerrainOnEnterHitResponse(IHitboxController otherHBController)
	{
		SurfaceAudioController component = otherHBController.RootGameObject.GetComponent<SurfaceAudioController>();
		if (component != null)
		{
			component.SetSurfaceOverride(m_surfaceType);
		}
	}

	public void TerrainOnExitHitResponse(IHitboxController otherHBController)
	{
		SurfaceAudioController component = otherHBController.RootGameObject.GetComponent<SurfaceAudioController>();
		if (component != null)
		{
			component.SetSurfaceOverride(SurfaceType.None);
		}
	}
}
