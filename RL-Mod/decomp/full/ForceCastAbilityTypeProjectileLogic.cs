using System.Collections;
using UnityEngine;

public class ForceCastAbilityTypeProjectileLogic : BaseProjectileLogic
{
	[SerializeField]
	private CastAbilityType m_forcedCastAbilityType;

	private void OnEnable()
	{
		StartCoroutine(SetCastAbilityType());
	}

	private IEnumerator SetCastAbilityType()
	{
		yield return null;
		base.SourceProjectile.CastAbilityType = m_forcedCastAbilityType;
	}
}
