using UnityEngine;

public class DestroyOnOneWayCollisionProjectileLogic : BaseProjectileLogic
{
	[SerializeField]
	private bool m_passThroughBreakables;

	private Collider2D m_collider;

	private void Start()
	{
		m_collider = base.SourceProjectile.HitboxController.GetCollider(HitboxType.Terrain);
	}

	private void FixedUpdate()
	{
		RaycastHit2D raycastHit2D = Physics2D.Raycast(m_collider.bounds.min, Vector2.right, m_collider.bounds.extents.x * 2f, LayerMask.GetMask("Platform_OneWay"));
		if ((bool)raycastHit2D)
		{
			bool flag = true;
			if (m_passThroughBreakables && (raycastHit2D.collider.CompareTag("Breakable") || raycastHit2D.collider.CompareTag("FlimsyBreakable")))
			{
				flag = false;
			}
			if (flag)
			{
				base.SourceProjectile.HitboxController.LastCollidedWith = raycastHit2D.collider;
				base.SourceProjectile.PerformHitEffectCheck(null);
				base.SourceProjectile.FlagForDestruction();
			}
		}
	}
}
