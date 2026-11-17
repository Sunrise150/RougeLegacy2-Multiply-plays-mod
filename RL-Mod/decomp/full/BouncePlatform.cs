using System.Runtime.CompilerServices;
using UnityEngine;

public class BouncePlatform : SpecialPlatform, IRootObj
{
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
