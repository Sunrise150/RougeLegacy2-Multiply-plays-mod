using System.Collections;
using UnityEngine;

public class EnemyCurseProjectileStatusEffect : BaseStatusEffect
{
	private static string[] m_projectileNameArray = new string[1] { "StatusEffectCurseProjectile" };

	private float m_hitCooldownTimer;

	public override string[] ProjectileNameArray => m_projectileNameArray;

	public override StatusEffectType StatusEffectType => StatusEffectType.Enemy_Curse_Projectile;

	public override float StartingDurationOverride => float.MaxValue;

	protected override IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted)
	{
		m_hitCooldownTimer = Time.time + 3.5f;
		m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.CurseProjectile, 3.5f);
		while (Time.time < base.EndTime)
		{
			float num = CDGHelper.DistanceBetweenPts(PlayerManager.GetPlayerController().Midpoint, m_charController.Midpoint);
			if (Time.time > m_hitCooldownTimer && num < 35f * CameraController.ZoomLevel)
			{
				if (!base.IsHidden)
				{
					float angleInDeg = CDGHelper.VectorToAngle(m_charController.Midpoint - PlayerManager.GetPlayerController().Midpoint);
					ProjectileManager.FireProjectile(m_charController.gameObject, "StatusEffectCurseProjectile", m_charController.Midpoint, matchFacing: false, angleInDeg, 1f, useAbsPos: true);
				}
				m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.CurseProjectile, 3.5f);
				m_hitCooldownTimer = Time.time + 3.5f;
			}
			yield return null;
		}
		StopEffect();
	}
}
