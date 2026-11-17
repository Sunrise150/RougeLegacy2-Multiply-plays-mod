using System.Collections;
using UnityEngine;

public class DeathDelayStatusEffect : BaseStatusEffect
{
	public override StatusEffectType StatusEffectType => StatusEffectType.Enemy_DeathDelay;

	public override float StartingDurationOverride => 3f;

	protected override IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted)
	{
		if (m_charController.CurrentHealth <= 0f)
		{
			while (Time.time < base.EndTime)
			{
				yield return null;
			}
		}
		StopEffect();
	}

	public override void StopEffect(bool interrupted = false)
	{
		base.StopEffect(interrupted);
		if (PlayerManager.IsInstantiated && m_charController.CurrentHealth <= 0f)
		{
			m_charController.ResetIsDead();
			m_charController.KillCharacter(PlayerManager.GetPlayer(), broadcastEvent: true);
		}
	}
}
