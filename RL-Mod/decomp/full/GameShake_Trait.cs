using System.Collections;
using UnityEngine;

public class GameShake_Trait : BaseTrait
{
	private WaitRL_Yield m_waitYield;

	private bool m_isShaking;

	private BaseEffect m_shakeEffect;

	public override TraitType TraitType => TraitType.GameShake;

	private void Start()
	{
		m_waitYield = new WaitRL_Yield(0f);
		if (!m_isShaking && !base.IsPaused)
		{
			StartCoroutine(RandomShakeCoroutine());
		}
	}

	private IEnumerator RandomShakeCoroutine()
	{
		m_isShaking = true;
		while (true)
		{
			float waitTime = Random.Range(Trait_EV.GAME_SHAKE_TIME_RANDOMIZER.x, Trait_EV.GAME_SHAKE_TIME_RANDOMIZER.y);
			m_waitYield.CreateNew(waitTime);
			yield return m_waitYield;
			float num = Random.Range(Trait_EV.GAME_SHAKE_INTENSITY.x, Trait_EV.GAME_SHAKE_INTENSITY.y);
			EffectManager.SetEffectParams("CameraShake_Effect", "m_shakeSpeed", num, "m_shakeAmplitude", num);
			float num2 = Random.Range(Trait_EV.GAME_SHAKE_DURATION.x, Trait_EV.GAME_SHAKE_DURATION.y);
			m_shakeEffect = EffectManager.PlayEffect(PlayerManager.GetCurrentPlayerRoom().gameObject, null, "CameraShake_Effect", Vector3.zero, num2, EffectStopType.Gracefully);
			m_waitYield.CreateNew(num2);
			yield return m_waitYield;
			m_shakeEffect = null;
		}
	}

	public override void SetPaused(bool paused)
	{
		base.SetPaused(paused);
		if (paused)
		{
			StopAllCoroutines();
			if (m_shakeEffect != null)
			{
				EffectManager.StopEffect(m_shakeEffect, EffectStopType.Immediate);
			}
			m_isShaking = false;
		}
		else if (!m_isShaking)
		{
			StartCoroutine(RandomShakeCoroutine());
		}
	}
}
