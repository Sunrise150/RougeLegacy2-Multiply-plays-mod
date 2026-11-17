using System;
using RLAudio;
using UnityEngine;

[RequireComponent(typeof(BaseAbility_RL))]
public class ReduceCDOnSpinKickAbilityMod : MonoBehaviour, IAudioEventEmitter
{
	[SerializeField]
	private float m_cooldownReductionAmount;

	[SerializeField]
	private bool m_isFlat;

	[SerializeField]
	private bool m_grantChargeStatusAfterSpinkick;

	private BaseAbility_RL m_ability;

	private Action<MonoBehaviour, EventArgs> m_onPlayerDownstrikeBounce;

	public string Description => ToString();

	private void Awake()
	{
		m_ability = GetComponent<BaseAbility_RL>();
		m_onPlayerDownstrikeBounce = OnPlayerDownstrikeBounce;
	}

	private void OnEnable()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerDownstrikeBounce, m_onPlayerDownstrikeBounce);
	}

	private void OnDisable()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerDownstrikeBounce, m_onPlayerDownstrikeBounce);
	}

	private void OnPlayerDownstrikeBounce(object sender, EventArgs args)
	{
		float num = 0f;
		num = ((!m_isFlat) ? (m_ability.ActualCooldownTime * m_cooldownReductionAmount) : m_cooldownReductionAmount);
		float cooldownTimer = m_ability.CooldownTimer;
		m_ability.ReduceCooldown(num);
		if (m_ability.CooldownTimer == 0f && cooldownTimer != 0f)
		{
			AudioManager.PlayOneShotAttached(this, "event:/SFX/Interactables/sfx_pickups_ammoGain", base.gameObject);
		}
		if (m_grantChargeStatusAfterSpinkick)
		{
			PlayerManager.GetPlayerController().StatusEffectController.StartStatusEffect(StatusEffectType.Player_FreeCrit, 1f, null);
		}
	}
}
