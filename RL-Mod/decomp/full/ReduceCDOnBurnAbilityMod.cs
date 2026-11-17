using System;
using UnityEngine;

[RequireComponent(typeof(BaseAbility_RL))]
public class ReduceCDOnBurnAbilityMod : MonoBehaviour
{
	[SerializeField]
	private float m_cooldownReductionAmount;

	[SerializeField]
	private bool m_isFlat;

	private BaseAbility_RL m_ability;

	private Action<MonoBehaviour, EventArgs> m_onEnemyHit;

	private void Awake()
	{
		m_ability = GetComponent<BaseAbility_RL>();
		m_onEnemyHit = OnEnemyHit;
	}

	private void OnEnable()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.EnemyHit, m_onEnemyHit);
	}

	private void OnDisable()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.EnemyHit, m_onEnemyHit);
	}

	private void OnEnemyHit(object sender, EventArgs args)
	{
		if ((args as CharacterHitEventArgs).Victim.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_Burn))
		{
			float num = 0f;
			num = ((!m_isFlat) ? (m_ability.ActualCooldownTime * m_cooldownReductionAmount) : m_cooldownReductionAmount);
			m_ability.ReduceCooldown(num);
		}
	}
}
