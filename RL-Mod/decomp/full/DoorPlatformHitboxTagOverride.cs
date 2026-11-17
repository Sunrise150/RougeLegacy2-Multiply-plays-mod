using System.Collections;
using UnityEngine;

public class DoorPlatformHitboxTagOverride : MonoBehaviour
{
	private IEnumerator Start()
	{
		HitboxControllerLite m_hitboxController = GetComponent<HitboxControllerLite>();
		while (!m_hitboxController.IsInitialized)
		{
			yield return null;
		}
		m_hitboxController.GetCollider(HitboxType.Platform).gameObject.tag = "OneWay";
	}
}
