using RLAudio;
using UnityEngine;

public class BardAimedAbilityAudioEventController : AimedAbilityAudioEventController
{
	[SerializeField]
	private bool m_isElectricLute;

	private AimedAbility_RL m_ability;

	private const float LUTE_PITCH_DIFF = 22.5f;

	private void Awake()
	{
		m_ability = GetComponent<AimedAbility_RL>();
	}

	protected override void OnAbilityFired(Projectile_RL projectile)
	{
		m_isAbilityStarted = false;
		int num = Mathf.RoundToInt((m_ability.AimAngle + 90f) / 22.5f);
		if (m_abilityFiredEventInstance.isValid())
		{
			m_abilityFiredEventInstance.setParameterByName("bardShootNote", num);
			AudioManager.PlayAttached(this, m_abilityFiredEventInstance, base.gameObject);
		}
		if (m_isElectricLute)
		{
			projectile.GetComponent<KineticBowProjectileAudioEventEmitter>().SetPitch(num);
		}
		AudioManager.Stop(m_aimEventInstance);
	}
}
