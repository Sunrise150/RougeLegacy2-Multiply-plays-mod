using System.Collections;
using UnityEngine;

public class RumbleControllerEffect : BaseEffect
{
	[SerializeField]
	private float m_rumbleAmount;

	public override void Play(float duration = 0f, EffectStopType stopType = EffectStopType.Gracefully)
	{
		base.Play(duration, stopType);
		StartCoroutine(PlayTimedRumble(duration));
	}

	private IEnumerator PlayTimedRumble(float duration)
	{
		RumbleManager.StartRumble(useLeftMotor: true, useRightMotor: true, m_rumbleAmount, duration);
		duration += Time.time;
		while (Time.time < duration)
		{
			yield return null;
		}
		Stop(EffectStopType.Gracefully);
	}

	public override void Stop(EffectStopType stopType)
	{
		RumbleManager.StopRumble();
		PlayComplete();
	}

	protected override void OnDisable()
	{
		RumbleManager.StopRumble();
		base.OnDisable();
	}
}
