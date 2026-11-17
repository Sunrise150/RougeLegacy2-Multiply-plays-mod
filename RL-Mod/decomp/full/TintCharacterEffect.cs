using UnityEngine;

public class TintCharacterEffect : BaseEffect
{
	[SerializeField]
	private Color m_tintColor;

	private bool m_useUnscaledTime;

	private MaterialPropertyBlock m_matPropertyBlock;

	protected override void Awake()
	{
		m_matPropertyBlock = new MaterialPropertyBlock();
		base.Awake();
	}

	public override void Play(float duration, EffectStopType stopType)
	{
		if (PlayerManager.IsInstantiated)
		{
			PlayerController playerController = PlayerManager.GetPlayerController();
			if (Time.time - playerController.TimeEnteredRoom < 0.1f)
			{
				duration = 0f;
			}
		}
		base.Play(duration, stopType);
		BlinkPulseEffect componentInChildren = base.Source.GetComponentInChildren<BlinkPulseEffect>();
		if (!componentInChildren)
		{
			Debug.Log("Cannot perform TintCharacterEffect.  No BlinkPulseEffect component found on " + base.Source.name);
			return;
		}
		componentInChildren.ActivateBlackFill(BlackFillType.TintEffect, duration);
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
