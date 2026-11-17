using System;
using UnityEngine;

[RequireComponent(typeof(Projectile_RL))]
public class AttachStatusEffectToProjectile : MonoBehaviour
{
	[Serializable]
	private struct ProjectileStatusEffectEntry
	{
		public StatusEffectType StatusEffect;

		public float Duration;
	}

	[SerializeField]
	private ProjectileStatusEffectEntry[] m_statusEffectsToAttach;

	[SerializeField]
	private bool m_extendDurationWithWeaponsBurnAddRelic;

	private Projectile_RL m_projectile;

	private void Awake()
	{
		m_projectile = GetComponent<Projectile_RL>();
	}

	private void OnEnable()
	{
		ProjectileStatusEffectEntry[] statusEffectsToAttach = m_statusEffectsToAttach;
		for (int i = 0; i < statusEffectsToAttach.Length; i++)
		{
			ProjectileStatusEffectEntry projectileStatusEffectEntry = statusEffectsToAttach[i];
			float num = projectileStatusEffectEntry.Duration;
			if (projectileStatusEffectEntry.StatusEffect == StatusEffectType.Enemy_Burn && m_extendDurationWithWeaponsBurnAddRelic)
			{
				int level = SaveManager.PlayerSaveData.GetRelic(RelicType.WeaponsBurnAdd).Level;
				if (level > 0)
				{
					if (num == 0f)
					{
						num = 3.05f;
					}
					num += 2f * (float)level;
				}
			}
			m_projectile.AttachStatusEffect(projectileStatusEffectEntry.StatusEffect, num);
		}
	}
}
