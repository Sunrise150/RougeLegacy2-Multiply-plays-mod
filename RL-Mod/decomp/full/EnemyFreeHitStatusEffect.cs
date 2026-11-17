using System;
using System.Collections;
using UnityEngine;

public class EnemyFreeHitStatusEffect : BaseStatusEffect
{
	private float m_freeHitRegenerationAmount;

	private int m_hitCount;

	private int m_totalHitCount;

	private Coroutine m_rechargeCoroutine;

	private Action<object, CharacterHitEventArgs> m_onEnemyHit;

	public override StatusEffectType StatusEffectType => StatusEffectType.Enemy_FreeHit;

	public override float StartingDurationOverride => float.MaxValue;

	public float FreeHitRegenerationOverride { get; set; } = -1f;


	public int FreeHitCountOverride { get; set; } = -1;


	public int HitCount => m_hitCount;

	protected override void Awake()
	{
		base.Awake();
		m_onEnemyHit = OnEnemyHit;
	}

	protected override IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted)
	{
		m_totalHitCount = 4;
		if (FreeHitCountOverride != -1)
		{
			m_totalHitCount = FreeHitCountOverride;
		}
		m_freeHitRegenerationAmount = 3.5f;
		if (FreeHitRegenerationOverride != -1f)
		{
			m_freeHitRegenerationAmount = FreeHitRegenerationOverride;
		}
		m_hitCount = m_totalHitCount;
		m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.FreeHit, m_totalHitCount, m_totalHitCount);
		m_charController.CharacterHitResponse.OnCharacterHitRelay.AddListener(m_onEnemyHit);
		m_statusEffectController.AddStatusEffectInvulnStack(StatusEffectType);
		while (Time.time < base.EndTime)
		{
			yield return null;
		}
		StopEffect();
	}

	public override void StopEffect(bool interrupted = false)
	{
		base.StopEffect(interrupted);
		m_charController.CharacterHitResponse.OnCharacterHitRelay.RemoveListener(m_onEnemyHit);
		m_statusEffectController.RemoveStatusEffectInvulnStack(StatusEffectType);
		if (m_rechargeCoroutine != null)
		{
			StopCoroutine(m_rechargeCoroutine);
		}
	}

	private void OnEnemyHit(object sender, CharacterHitEventArgs args)
	{
		if (args != null && args.Attacker.BaseDamage > 0f)
		{
			if (m_rechargeCoroutine != null)
			{
				StopCoroutine(m_rechargeCoroutine);
			}
			m_rechargeCoroutine = StartCoroutine(EnemyHitCoroutine());
		}
	}

	private IEnumerator EnemyHitCoroutine()
	{
		m_hitCount--;
		if (m_hitCount <= 0)
		{
			m_statusEffectController.RemoveStatusEffectInvulnStack(StatusEffectType);
			m_hitCount = 0;
		}
		m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.FreeHit, m_freeHitRegenerationAmount, m_totalHitCount, m_hitCount);
		float rechargeDelay = Time.time + m_freeHitRegenerationAmount;
		while (Time.time < rechargeDelay)
		{
			yield return null;
		}
		m_hitCount = m_totalHitCount;
		m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.FreeHit, m_totalHitCount, m_hitCount);
		m_statusEffectController.AddStatusEffectInvulnStack(StatusEffectType);
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
