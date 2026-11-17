using UnityEngine;

public class DrawHitboxColliders : MonoBehaviour
{
	[SerializeField]
	private bool m_disableOnPlay;

	[Space(10f)]
	[SerializeField]
	private bool m_drawWeaponHitboxes;

	[SerializeField]
	private bool m_drawBodyHitboxes;

	[SerializeField]
	private bool m_drawTerrainHitboxes;

	[SerializeField]
	private bool m_drawPlatformHitbox;
}
