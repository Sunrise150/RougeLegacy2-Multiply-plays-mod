using System.Runtime.CompilerServices;
using UnityEngine;

public class SuperBouncePlatform : SpecialPlatform, IRootObj
{
	private string m_bounceEffectName = "BounceProjectileIndicatorBounced_Effect";

	public override string EffectNameOverride => m_bounceEffectName;

	public override bool PlayDirectionalHitEffect => false;

	public override void SetState(StateID state)
	{
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
