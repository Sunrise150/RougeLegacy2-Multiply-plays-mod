using System;
using System.Collections;
using UnityEngine;

public class EnemyAggroStatusEffect : BaseStatusEffect
{
	private Action<object, CharacterHitEventArgs> m_onEnemyHit;

	public override StatusEffectType StatusEffectType => StatusEffectType.Enemy_Aggro;

	public override float StartingDurationOverride => float.MaxValue;

	protected override void Awake()
	{
		base.Awake();
		m_onEnemyHit = OnEnemyHit;
	}

	protected override IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted)
	{
		m_charController.CharacterHitResponse.OnCharacterHitRelay.AddListener(m_onEnemyHit);
		base.TimesStacked = Mathf.Min(base.TimesStacked, 4);
		if (base.TimesStacked <= 1)
		{
			m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.Aggro);
		}
		else
		{
			while (true)
			{
				float delay = Time.time + 2.75f;
				m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.Aggro, 2.75f, 3, base.TimesStacked - 1);
				while (Time.time < delay)
				{
					yield return null;
				}
				base.TimesStacked = 1;
				if (base.TimesStacked <= 1)
				{
					break;
				}
				yield return null;
			}
			m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.Aggro);
		}
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
	}

	private void OnEnemyHit(object sender, CharacterHitEventArgs args)
	{
		if (args != null && args.Attacker.BaseDamage > 0f)
		{
			m_charController.StatusEffectController.StartStatusEffect(StatusEffectType.Enemy_Aggro, 0f, null);
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
