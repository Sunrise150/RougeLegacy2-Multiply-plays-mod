using System;
using System.Collections;
using UnityEngine;

public class VulnerableStatusEffect : BaseStatusEffect
{
	private Action<object, CharacterHitEventArgs> m_onCharacterHit;

	public override StatusEffectType StatusEffectType => StatusEffectType.Enemy_Vulnerable;

	public override float StartingDurationOverride => 2f;

	protected override void Awake()
	{
		base.Awake();
		m_onCharacterHit = OnCharacterHit;
	}

	protected override IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted)
	{
		m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.Vulnerable, base.Duration);
		while (Time.time < base.EndTime)
		{
			yield return null;
		}
		StopEffect();
	}

	public override void StopEffect(bool interrupted = false)
	{
		base.StopEffect(interrupted);
	}

	private void OnCharacterHit(object sender, CharacterHitEventArgs args)
	{
		StopEffect();
	}
}
