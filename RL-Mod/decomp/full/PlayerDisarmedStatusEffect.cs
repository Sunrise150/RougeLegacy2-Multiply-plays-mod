using System.Collections;
using UnityEngine;

public class PlayerDisarmedStatusEffect : BaseStatusEffect
{
	public override StatusEffectType StatusEffectType => StatusEffectType.Player_Disarmed;

	public override float StartingDurationOverride => 3f;

	protected override IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted)
	{
		m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.Disarmed, base.Duration);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.UpdateAbilityDisarmState, this, null);
		while (Time.time < base.EndTime)
		{
			yield return null;
		}
		StopEffect();
	}

	public override void StopEffect(bool interrupted = false)
	{
		base.StopEffect(interrupted);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.UpdateAbilityDisarmState, this, null);
	}
}
