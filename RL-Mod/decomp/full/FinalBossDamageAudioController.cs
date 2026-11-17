using FMODUnity;
using RLAudio;
using UnityEngine;

public class FinalBossDamageAudioController : DamageAudioController
{
	[SerializeField]
	private StudioEventEmitter m_lightHitEmitter;

	[SerializeField]
	private StudioEventEmitter m_darkHitEmitter;

	private FinalBoss_Basic_AIScript m_logicScript;

	private bool IsInWhiteMode
	{
		get
		{
			if (!m_logicScript)
			{
				EnemyController component = GetComponent<EnemyController>();
				m_logicScript = component.LogicController.LogicScript as FinalBoss_Basic_AIScript;
			}
			return m_logicScript.IsInWhiteMode;
		}
	}

	protected override void OnTakeDamage(GameObject attacker, float damageTaken, bool isCrit)
	{
		base.OnTakeDamage(attacker, damageTaken, isCrit);
		if (IsInWhiteMode)
		{
			AudioManager.Play(this, m_lightHitEmitter);
		}
		else
		{
			AudioManager.Play(this, m_darkHitEmitter);
		}
	}
}
