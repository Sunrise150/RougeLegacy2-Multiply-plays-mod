using System;
using System.Collections;
using UnityEngine;

public class DownstrikeProjectile_RL : Projectile_RL
{
	private Action<Projectile_RL, GameObject> m_bounce;

	public static int ConsecutiveStrikes { get; set; }

	public override float ActualDamage
	{
		get
		{
			float num = 1f * SkillTreeLogicHelper.GetConsecutiveDownstrikeDamageMod();
			float num2 = 0f;
			num2 = ((!IgnoreDamageScale) ? (Strength * StrengthScale + Magic * (MagicScale + num)) : ((Strength + Magic) * num));
			float num3 = 1f;
			num3 += Mastery_EV.GetTotalMasteryBonus(MasteryBonusType.SpinKickDamage_Up);
			return num2 * num3;
		}
	}

	protected override IEnumerator DestroyProjectileCoroutine(bool animate = false)
	{
		base.HitboxController.DisableAllCollisions = false;
		if (m_bounce == null)
		{
			PlayerController playerController = PlayerManager.GetPlayerController();
			m_bounce = playerController.CharacterDownStrike.Bounce;
		}
		base.OnCollisionRelay.AddListener(m_bounce);
		yield return null;
		base.OnCollisionRelay.RemoveListener(m_bounce);
		base.HitboxController.DisableAllCollisions = true;
		yield return base.DestroyProjectileCoroutine(animate);
	}
}
