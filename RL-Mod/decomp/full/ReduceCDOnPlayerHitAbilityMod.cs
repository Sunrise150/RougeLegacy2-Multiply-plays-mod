using System;
using UnityEngine;

[RequireComponent(typeof(BaseAbility_RL))]
public class ReduceCDOnPlayerHitAbilityMod : MonoBehaviour
{
	[SerializeField]
	private float m_cooldownReductionAmount;

	[SerializeField]
	private bool m_isFlat;

	private BaseAbility_RL m_ability;

	private Action<MonoBehaviour, EventArgs> m_onPlayerHit;

	private void Awake()
	{
		m_ability = GetComponent<BaseAbility_RL>();
		m_onPlayerHit = OnPlayerHit;
	}

	private void OnEnable()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerHit, m_onPlayerHit);
	}

	private void OnDisable()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerHit, m_onPlayerHit);
	}

	private void OnPlayerHit(object sender, EventArgs args)
	{
		float num = 0f;
		num = ((!m_isFlat) ? (m_ability.ActualCooldownTime * m_cooldownReductionAmount) : m_cooldownReductionAmount);
		m_ability.ReduceCooldown(num);
	}
}
