using System.Collections;
using UnityEngine;

public class EnemyFlamerStatusEffect : BaseStatusEffect
{
	private static string[] m_projectileNameArray = new string[1] { "StatusEffectFireballProjectile" };

	private float m_intervalTimer;

	public override string[] ProjectileNameArray => m_projectileNameArray;

	public override StatusEffectType StatusEffectType => StatusEffectType.Enemy_Flamer;

	public override float StartingDurationOverride => 2.1474836E+09f;

	protected override IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted)
	{
		m_intervalTimer = Time.time + 2.25f;
		m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.Flamer, 2.25f);
		while (Time.time < base.EndTime)
		{
			float num = CDGHelper.DistanceBetweenPts(PlayerManager.GetPlayerController().Midpoint, m_charController.Midpoint);
			if (Time.time > m_intervalTimer && num < 35f * CameraController.ZoomLevel)
			{
				if (!base.IsHidden)
				{
					yield return FireProjectileCoroutine();
				}
				m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.Flamer, 2.25f);
				m_intervalTimer = Time.time + 2.25f;
			}
			yield return null;
		}
		StopEffect();
	}

	private IEnumerator FireProjectileCoroutine()
	{
		for (int i = 0; i < 2; i++)
		{
			float num = CDGHelper.VectorToAngle(PlayerManager.GetPlayerController().Midpoint - m_charController.Midpoint);
			float num2 = Random.Range(StatusEffect_EV.ENEMY_FLAME_PROJECTILE_OFFSET.x, StatusEffect_EV.ENEMY_FLAME_PROJECTILE_OFFSET.y);
			num += num2;
			ProjectileManager.FireProjectile(m_charController.gameObject, "StatusEffectFireballProjectile", m_charController.Midpoint, matchFacing: false, num, 1f, useAbsPos: true);
			float delay = Time.time + 0.25f;
			while (Time.time < delay)
			{
				yield return null;
			}
		}
	}
}
