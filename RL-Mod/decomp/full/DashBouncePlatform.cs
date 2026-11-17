using System.Runtime.CompilerServices;
using UnityEngine;

public class DashBouncePlatform : SpecialPlatform, IRootObj
{
	private string m_bounceEffectName = "BounceProjectileIndicatorBounced_Effect";

	public override string EffectNameOverride => m_bounceEffectName;

	public override bool PlayDirectionalHitEffect => false;

	public override void SetState(StateID state)
	{
	}

	public void Bounce()
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.CharacterDownStrike.ForceTriggerBounce(43.5f);
		EffectManager.PlayEffect(playerController.gameObject, playerController.Animator, m_bounceEffectName, base.transform.position, -1f, EffectStopType.Gracefully);
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
