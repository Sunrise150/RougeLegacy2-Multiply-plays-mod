using UnityEngine;

public abstract class BaseProjectileLogic : MonoBehaviour
{
	public Projectile_RL SourceProjectile { get; protected set; }

	protected virtual void Awake()
	{
		SourceProjectile = GetComponent<Projectile_RL>();
	}
}
