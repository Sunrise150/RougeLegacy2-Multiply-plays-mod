using UnityEngine;

public class MatchWeaponGeoMaterial : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer m_meshRenderer;

	private void OnEnable()
	{
		if (PlayerManager.IsInstantiated && (bool)PlayerManager.GetPlayerController().LookController.SecondaryWeaponGeo)
		{
			m_meshRenderer.sharedMaterial = PlayerManager.GetPlayerController().LookController.SecondaryWeaponGeo.sharedMaterial;
		}
	}
}
