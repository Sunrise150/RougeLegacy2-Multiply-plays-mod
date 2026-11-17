using System.Collections;

public class AxeSpellProjectileLogic : BaseProjectileLogic
{
	private void OnEnable()
	{
		if (base.SourceProjectile.CompareTag("PlayerProjectile"))
		{
			StartCoroutine(TriggerSkillCritCoroutine());
		}
	}

	private IEnumerator TriggerSkillCritCoroutine()
	{
		yield return null;
		while (base.SourceProjectile.Velocity.y >= 0f)
		{
			yield return null;
		}
		if (base.SourceProjectile.ActualCritChance < 100f)
		{
			base.SourceProjectile.ActualCritChance += 100f;
		}
	}
}
