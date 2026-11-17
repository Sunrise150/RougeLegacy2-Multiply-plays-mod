using System;
using System.Collections;
using UnityEngine;

public class EnemyExplodeStatusEffect : BaseStatusEffect
{
	private static string[] m_projectileNameArray = new string[2] { "StatusEffectExplosionProjectile", "StatusEffectExplosionWarningProjectile" };

	private float m_explodeCooldownTimer;

	private bool m_warningExploded;

	private bool m_isExploding;

	private Projectile_RL m_warningProjectile;

	private Coroutine m_explodeCoroutine;

	private Action<object, CharacterHitEventArgs> m_onEnemyHit;

	public override string[] ProjectileNameArray => m_projectileNameArray;

	public override StatusEffectType StatusEffectType => StatusEffectType.Enemy_Explode;

	public override float StartingDurationOverride => float.MaxValue;

	protected override void Awake()
	{
		base.Awake();
		m_onEnemyHit = OnEnemyHit;
	}

	protected override IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted)
	{
		m_explodeCooldownTimer = 0f;
		m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.Explode);
		m_charController.CharacterHitResponse.OnCharacterHitRelay.AddListener(m_onEnemyHit);
		while (Time.time < base.EndTime)
		{
			yield return null;
		}
		StopEffect();
	}

	private void OnEnemyHit(object sender, CharacterHitEventArgs args)
	{
		if (!m_isExploding && m_explodeCooldownTimer < Time.time)
		{
			m_explodeCoroutine = StartCoroutine(ExplodeCoroutine());
		}
	}

	private IEnumerator ExplodeCoroutine()
	{
		m_isExploding = true;
		m_warningProjectile = ProjectileManager.FireProjectile(m_charController.gameObject, "StatusEffectExplosionWarningProjectile", m_charController.Midpoint - m_charController.transform.localPosition, matchFacing: false);
		float num = m_warningProjectile.transform.localScale.x * 11f * 2f / m_warningProjectile.transform.lossyScale.x;
		m_warningProjectile.transform.localScale = new Vector3(num, num, m_warningProjectile.transform.localScale.z);
		m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.Explode, 1.25f);
		float warningDelay = Time.time + 1.25f;
		while (Time.time < warningDelay)
		{
			yield return null;
		}
		if ((bool)m_warningProjectile && !m_warningProjectile.IsFreePoolObj && m_warningProjectile.OwnerController == m_charController)
		{
			m_warningProjectile.FlagForDestruction();
		}
		Projectile_RL projectile_RL = ProjectileManager.FireProjectile(m_charController.gameObject, "StatusEffectExplosionProjectile", m_charController.Midpoint - m_charController.transform.localPosition, matchFacing: false);
		num = projectile_RL.transform.localScale.x * 11f * 2f / projectile_RL.transform.lossyScale.x;
		projectile_RL.transform.localScale = new Vector3(num, num, projectile_RL.transform.localScale.z);
		m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.Explode);
		m_explodeCooldownTimer = Time.time + 4.25f;
		m_isExploding = false;
	}

	public override void StopEffect(bool interrupted = false)
	{
		base.StopEffect(interrupted);
		m_charController.CharacterHitResponse.OnCharacterHitRelay.RemoveListener(m_onEnemyHit);
		m_isExploding = false;
		if (m_explodeCoroutine != null)
		{
			StopCoroutine(m_explodeCoroutine);
		}
		if ((bool)m_warningProjectile && !m_warningProjectile.IsFreePoolObj && m_warningProjectile.OwnerController == m_charController)
		{
			m_warningProjectile.FlagForDestruction();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if ((bool)m_charController)
		{
			m_charController.CharacterHitResponse.OnCharacterHitRelay.RemoveListener(m_onEnemyHit);
		}
	}
}
