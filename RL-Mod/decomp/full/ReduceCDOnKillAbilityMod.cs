using System;
using UnityEngine;

[RequireComponent(typeof(BaseAbility_RL))]
public class ReduceCDOnKillAbilityMod : MonoBehaviour
{
	[SerializeField]
	private float m_cooldownReductionAmount;

	[SerializeField]
	private bool m_isFlat;

	private BaseAbility_RL m_ability;

	private Action<MonoBehaviour, EventArgs> m_onEnemyDeath;

	private void Awake()
	{
		m_ability = GetComponent<BaseAbility_RL>();
		m_onEnemyDeath = OnEnemyDeath;
	}

	private void OnEnable()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.EnemyDeath, m_onEnemyDeath);
	}

	private void OnDisable()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.EnemyDeath, m_onEnemyDeath);
	}

	private void OnEnemyDeath(object sender, EventArgs args)
	{
		float num = 0f;
		num = ((!m_isFlat) ? (m_ability.ActualCooldownTime * m_cooldownReductionAmount) : m_cooldownReductionAmount);
		m_ability.ReduceCooldown(num);
	}
}
