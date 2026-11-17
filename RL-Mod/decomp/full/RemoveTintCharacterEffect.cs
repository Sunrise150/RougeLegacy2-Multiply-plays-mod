using UnityEngine;

public class RemoveTintCharacterEffect : BaseEffect
{
	private MaterialPropertyBlock m_matPropertyBlock;

	private bool m_useUnscaledTime;

	protected override void Awake()
	{
		m_matPropertyBlock = new MaterialPropertyBlock();
		base.Awake();
	}

	public override void Play(float duration, EffectStopType stopType)
	{
		base.Play(duration, stopType);
		BlinkPulseEffect componentInChildren = base.Source.GetComponentInChildren<BlinkPulseEffect>();
		if (!componentInChildren)
		{
			Debug.Log("Cannot perform TintCharacterEffect.  No BlinkPulseEffect component found on " + base.Source.name);
			return;
		}
		componentInChildren.DisableBlackFill(BlackFillType.TintEffect, duration);
		PlayComplete();
	}

	public override void Stop(EffectStopType stopType)
	{
		PlayComplete();
	}

	public override void ResetValues()
	{
		m_useUnscaledTime = false;
		base.ResetValues();
	}
}
