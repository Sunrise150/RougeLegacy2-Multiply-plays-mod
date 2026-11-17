using System.Collections;
using UnityEngine;

public class RemoveGuaranteedCritAfterHitProjectileLogic : BaseProjectileLogic
{
	private void OnEnable()
	{
		base.SourceProjectile.OnCollisionRelay.AddListener(DisableGuaranteedCrit);
	}

	private void DisableGuaranteedCrit(Projectile_RL projectile, GameObject obj)
	{
		StartCoroutine(DisableGuaranteedCritCoroutine());
	}

	private IEnumerator DisableGuaranteedCritCoroutine()
	{
		yield return null;
		if (base.SourceProjectile.ActualCritChance >= 100f)
		{
			base.SourceProjectile.ActualCritChance -= 100f;
		}
	}
}
