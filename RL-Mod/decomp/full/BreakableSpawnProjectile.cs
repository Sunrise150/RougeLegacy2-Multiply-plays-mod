using UnityEngine;

[RequireComponent(typeof(Breakable))]
public class BreakableSpawnProjectile : MonoBehaviour
{
	[SerializeField]
	private string m_projectileName;

	[SerializeField]
	private GameObject m_projectileSpawnPos;

	[SerializeField]
	private bool m_flipBasedOnAttackerPos;

	private Breakable m_breakable;

	public float BaseDamage => 10f;

	private void Awake()
	{
		m_breakable = GetComponent<Breakable>();
		m_breakable.OnDeathEffectTriggerRelay.AddListener(SpawnProjectile);
	}

	private void OnEnable()
	{
		ProjectileManager.Instance.AddProjectileToPool(m_projectileName);
	}

	private void OnDestroy()
	{
		if ((bool)m_breakable)
		{
			m_breakable.OnDeathEffectTriggerRelay.RemoveListener(SpawnProjectile);
		}
	}

	private void SpawnProjectile(GameObject obj)
	{
		if (!string.IsNullOrEmpty(m_projectileName))
		{
			Vector3 localPosition = m_projectileSpawnPos.transform.localPosition;
			if (m_flipBasedOnAttackerPos && m_breakable.AttackerIsOnRight)
			{
				localPosition.x = 0f - localPosition.x;
			}
			Projectile_RL projectile_RL = ProjectileManager.FireProjectile(PlayerManager.GetPlayerController().gameObject, m_projectileName, localPosition + base.transform.position, matchFacing: false, 0f, 1f, useAbsPos: true);
			projectile_RL.SnapToOwner = false;
			if (m_flipBasedOnAttackerPos && m_breakable.AttackerIsOnRight)
			{
				projectile_RL.Flip();
			}
			projectile_RL.RemoveAllStatusEffects();
		}
	}
}
