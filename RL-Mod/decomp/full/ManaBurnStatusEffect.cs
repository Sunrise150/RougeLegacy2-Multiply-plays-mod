using System.Collections;
using UnityEngine;

public class ManaBurnStatusEffect : BaseStatusEffect
{
	private ForceManaRegenEventArgs m_regenEventArgs;

	private static Color m_addColorPulseOn = new Color(0f, 0.14f, 0.3f, 1f);

	private static Color m_addColorPulseOff = new Color(0f, 0f, 0f, 1f);

	private bool m_pulseOn;

	private BaseEffect m_manaBurnEffect;

	private float m_manaBurnTicRemaining;

	public override StatusEffectType StatusEffectType => StatusEffectType.Enemy_ManaBurn;

	public override float StartingDurationOverride => 2.55f;

	public override void Initialize(StatusEffectController statusEffectController, BaseCharacterController charController)
	{
		base.Initialize(statusEffectController, charController);
		m_regenEventArgs = new ForceManaRegenEventArgs(0f, usePlayerRegenMods: false);
	}

	private IEnumerator PulseCoroutine()
	{
		while (true)
		{
			m_waitYield.CreateNew(0.1f);
			yield return m_waitYield;
			if (m_pulseOn)
			{
				foreach (Renderer item in m_charController.RendererArray)
				{
					item.GetPropertyBlock(BaseStatusEffect.m_matBlockHelper_STATIC);
					BaseStatusEffect.m_matBlockHelper_STATIC.SetColor(ShaderID_RL._AddColor, m_addColorPulseOn);
					item.SetPropertyBlock(BaseStatusEffect.m_matBlockHelper_STATIC);
				}
			}
			else
			{
				foreach (Renderer item2 in m_charController.RendererArray)
				{
					item2.GetPropertyBlock(BaseStatusEffect.m_matBlockHelper_STATIC);
					BaseStatusEffect.m_matBlockHelper_STATIC.SetColor(ShaderID_RL._AddColor, m_addColorPulseOff);
					item2.SetPropertyBlock(BaseStatusEffect.m_matBlockHelper_STATIC);
				}
			}
			m_pulseOn = !m_pulseOn;
		}
	}

	private IEnumerator BurnManaCoroutine()
	{
		while (true)
		{
			float delayTime = Time.time + 0.1f - m_manaBurnTicRemaining;
			while (Time.time < delayTime)
			{
				m_manaBurnTicRemaining = 0.1f - (delayTime - Time.time);
				yield return null;
			}
			m_manaBurnTicRemaining = 0f;
			m_regenEventArgs.Initialise(1f, usePlayerRegenMods: false);
			EffectManager.PlayEffect(m_charController.gameObject, null, "ManaRegenBurst_Effect", Vector3.zero, 0f, EffectStopType.Gracefully);
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerForceManaRegen, this, m_regenEventArgs);
		}
	}

	protected override IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted)
	{
		m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.ManaBurn, base.Duration);
		m_manaBurnEffect = EffectManager.PlayEffect(m_charController.gameObject, m_charController.Animator, "EnemyBurnBlueFlames_Effect", Vector3.zero, base.Duration, EffectStopType.Gracefully);
		m_manaBurnEffect.transform.SetParent(m_charController.transform, worldPositionStays: false);
		m_manaBurnEffect.transform.position = m_charController.Midpoint;
		StartCoroutine(BurnManaCoroutine());
		while (Time.time < base.EndTime)
		{
			yield return null;
		}
		StopEffect();
	}

	public override void StopEffect(bool interrupted = false)
	{
		base.StopEffect(interrupted);
		if (!interrupted)
		{
			m_manaBurnTicRemaining = 0f;
			if (m_charController.IsDead)
			{
				m_regenEventArgs.Initialise(20f, usePlayerRegenMods: false);
				EffectManager.PlayEffect(m_charController.gameObject, null, "ManaRegenBurst_Effect", Vector3.zero, 0f, EffectStopType.Gracefully);
				Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerForceManaRegen, this, m_regenEventArgs);
			}
		}
		if ((bool)m_manaBurnEffect && m_manaBurnEffect.isActiveAndEnabled)
		{
			m_manaBurnEffect.Stop(EffectStopType.Gracefully);
		}
	}
}
