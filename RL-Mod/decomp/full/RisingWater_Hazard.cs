using System.Collections;
using UnityEngine;

public class RisingWater_Hazard : Hazard, ITerrainOnEnterHitResponse, IHitResponse
{
	public override float BaseDamage => 999999f;

	public override void SetIsCulled(bool culled)
	{
	}

	public override void Initialize(HazardArgs hazardArgs)
	{
		base.Initialize(hazardArgs);
		StartCoroutine(RiseWaterCoroutine());
	}

	public void TerrainOnEnterHitResponse(IHitboxController otherHBController)
	{
		if (otherHBController.RootGameObject.CompareTag("Player") || otherHBController.RootGameObject.CompareTag("Player_Dodging"))
		{
			PlayerManager.GetPlayerController().CharacterHitResponse.StartHitResponse(base.gameObject, this, 999999f, trueDamage: true);
		}
	}

	private IEnumerator RiseWaterCoroutine()
	{
		float delay = 3f + Time.time;
		while (Time.time < delay)
		{
			yield return null;
		}
		while (true)
		{
			Vector3 localPosition = base.transform.localPosition;
			localPosition.y += 5f * Time.deltaTime;
			base.transform.localPosition = localPosition;
			yield return null;
		}
	}

	public override void ResetHazard()
	{
	}
}
