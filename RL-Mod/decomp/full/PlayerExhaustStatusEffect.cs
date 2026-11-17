using System;
using System.Collections;
using UnityEngine;

[Obsolete("Replaced with CurrentExhaust directly on the PlayerController.")]
public class PlayerExhaustStatusEffect : BaseStatusEffect
{
	private bool m_hitListenerAdded;

	private Action<MonoBehaviour, EventArgs> m_onPlayerHit;

	public override StatusEffectType StatusEffectType => StatusEffectType.Player_Exhaust;

	public override float StartingDurationOverride => 2.5f;

	protected override void Awake()
	{
		base.Awake();
		m_onPlayerHit = OnPlayerHit;
	}

	public void SetTimesStacked(int timesStacked)
	{
		base.TimesStacked = Mathf.Clamp(timesStacked, 0, int.MaxValue);
		m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.Exhaust, base.Duration, int.MaxValue, base.TimesStacked);
	}

	public override void StartEffect(float duration, IDamageObj caster)
	{
		base.StartEffect(duration, caster);
		if (!m_hitListenerAdded)
		{
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerHit, m_onPlayerHit);
			m_hitListenerAdded = true;
		}
	}

	protected override IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted)
	{
		SetTimesStacked(base.TimesStacked);
		while (Time.time < base.EndTime)
		{
			yield return null;
		}
		StopEffect();
	}

	private void OnPlayerHit(object sender, EventArgs args)
	{
	}

	public override void StopEffect(bool interrupted = false)
	{
		base.StopEffect(interrupted);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerHit, m_onPlayerHit);
		m_hitListenerAdded = false;
	}
}
