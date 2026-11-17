using System.Collections;
using UnityEngine;

public class GroundBow_Ability : Bow_Ability
{
	private bool m_onGround;

	protected override float TellIntroAnimSpeed => 0.7f;

	protected override float TellIntroAnimExitDelay => 0f;

	protected override float TellAnimSpeed => 1f;

	protected override float TellAnimExitDelay => 0f;

	protected override float AttackIntroAnimSpeed => 1f;

	protected override float AttackIntroAnimExitDelay => 0f;

	protected override float AttackAnimSpeed => 1f;

	protected override float AttackAnimExitDelay => 0.25f;

	protected override float ExitAnimSpeed => 1.15f;

	protected override float ExitAnimExitDelay => 0f;

	protected override float GravityReduction => 1f;

	protected override Vector2 BowPushbackAmount => new Vector2(17f, 18f);

	public override void PreCastAbility()
	{
		m_onGround = PlayerManager.GetPlayerController().IsGrounded;
		if (!m_onGround)
		{
			StopAbility(abilityInterrupted: true);
		}
		else
		{
			base.PreCastAbility();
		}
	}

	public override IEnumerator CastAbility()
	{
		if (m_onGround)
		{
			yield return base.CastAbility();
		}
	}
}
