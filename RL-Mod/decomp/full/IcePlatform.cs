using System.Collections;
using UnityEngine;

public class IcePlatform : SpecialPlatform, ITerrainOnStayHitResponse, IHitResponse, ITerrainOnExitHitResponse
{
	protected BoxCollider2D m_terrainCollider;

	protected BoxCollider2D m_platformCollider;

	protected override IEnumerator Start()
	{
		yield return base.Start();
		m_hbController.RepeatHitDuration = 0f;
		m_terrainCollider = m_hbController.GetCollider(HitboxType.Terrain) as BoxCollider2D;
		m_platformCollider = m_hbController.GetCollider(HitboxType.Platform) as BoxCollider2D;
		if (base.Width > 0f)
		{
			SpriteRenderer componentInChildren = GetComponentInChildren<SpriteRenderer>();
			componentInChildren.size = new Vector2(base.Width, componentInChildren.size.y);
			m_terrainCollider.size = new Vector2(base.Width, m_terrainCollider.size.y);
			m_platformCollider.size = new Vector2(base.Width, m_platformCollider.size.y);
		}
	}

	public void TerrainOnStayHitResponse(IHitboxController otherHBController)
	{
		if (otherHBController.RootGameObject.CompareTag("Player"))
		{
			PlayerManager.GetPlayerController().DisableFriction = true;
		}
	}

	public void TerrainOnExitHitResponse(IHitboxController otherHBController)
	{
		if (otherHBController.RootGameObject.CompareTag("Player"))
		{
			PlayerManager.GetPlayerController().DisableFriction = false;
		}
	}

	public override void SetState(StateID state)
	{
	}
}
