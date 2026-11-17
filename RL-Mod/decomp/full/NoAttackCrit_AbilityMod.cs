using System.Collections;
using UnityEngine;

public class NoAttackCrit_AbilityMod : MonoBehaviour
{
	[SerializeField]
	private float m_noAttackCritDuration = 1f;

	private BaseAbility_RL m_ability;

	private Coroutine m_critWindowTimerCoroutine;

	private bool m_critDebugOn;

	private void Awake()
	{
		m_ability = GetComponent<BaseAbility_RL>();
		m_ability.BeginCastingRelay.AddListener(StopCritWindowTimer);
		m_ability.StopCastingRelay.AddListener(StartCritWindowTimer);
	}

	private void OnDestroy()
	{
		if ((bool)m_ability)
		{
			m_ability.BeginCastingRelay.RemoveListener(StopCritWindowTimer);
			m_ability.StopCastingRelay.RemoveListener(StartCritWindowTimer);
		}
	}

	private void OnEnable()
	{
		StartCritWindowTimer();
	}

	private void StartCritWindowTimer()
	{
		StopCritWindowTimer();
		m_critWindowTimerCoroutine = StartCoroutine(CritWindowTimerCoroutine());
	}

	private IEnumerator CritWindowTimerCoroutine()
	{
		float delay2 = Time.time + m_noAttackCritDuration;
		while (Time.time < delay2)
		{
			yield return null;
		}
		m_ability.ForceTriggerCrit = true;
		PlayerManager.GetPlayerController().BlinkPulseEffect.SetRendererArrayColor(Player_EV.PLAYER_GUARANTEED_CRIT_COLOR);
		delay2 = Time.time + 0.05f;
		m_critDebugOn = true;
		while (Time.time < delay2)
		{
			yield return null;
		}
		PlayerManager.GetPlayerController().BlinkPulseEffect.ResetRendererArrayColor();
		m_critDebugOn = false;
	}

	private void StopCritWindowTimer()
	{
		if (m_critWindowTimerCoroutine != null)
		{
			StopCoroutine(m_critWindowTimerCoroutine);
		}
		if (m_critDebugOn)
		{
			m_critDebugOn = false;
			PlayerManager.GetPlayerController().BlinkPulseEffect.ResetRendererArrayColor();
		}
	}
}
