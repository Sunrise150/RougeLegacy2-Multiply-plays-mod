using System.Collections;
using UnityEngine;

public class SporeBurstStatusEffect : BaseStatusEffect
{
	private const string MULTIPLY_COLOR = "#9AA633";

	private const string PULSE_ON_COLOR = "#3E4500";

	private const string PULSE_OFF_COLOR = "#000000";

	private bool m_sporeEffectFired;

	private GenericEffect m_sporeEffect;

	private Color m_multiplyColor;

	private Color m_addColorPulseOn;

	private Color m_addColorPulseOff;

	private bool m_pulseOn;

	private float m_burnPulseRate = 0.1f;

	public override StatusEffectType StatusEffectType => StatusEffectType.Enemy_SporeBurst;

	public override float StartingDurationOverride => 1.25f;

	public override void Initialize(StatusEffectController statusEffectController, BaseCharacterController charController)
	{
		base.Initialize(statusEffectController, charController);
		ColorUtility.TryParseHtmlString("#9AA633", out m_multiplyColor);
		ColorUtility.TryParseHtmlString("#3E4500", out m_addColorPulseOn);
		ColorUtility.TryParseHtmlString("#000000", out m_addColorPulseOff);
		base.AppliesTint = true;
	}

	public override void StartEffect(float duration, IDamageObj caster)
	{
		m_sporeEffectFired = false;
		if (!m_statusEffectController.HasStatusEffect(StatusEffectType.Enemy_SporeBurst))
		{
			base.StartEffect(duration, caster);
		}
	}

	protected override IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted)
	{
		m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.SporeBurst, base.Duration);
		StartCoroutine(SporeEffectCoroutine());
		m_pulseOn = false;
		foreach (Renderer item in m_charController.RendererArray)
		{
			item.GetPropertyBlock(BaseStatusEffect.m_matBlockHelper_STATIC);
			BaseStatusEffect.m_matBlockHelper_STATIC.SetColor(ShaderID_RL._MultiplyColor, m_multiplyColor);
			BaseStatusEffect.m_matBlockHelper_STATIC.SetColor(ShaderID_RL._AddColor, m_addColorPulseOff);
			item.SetPropertyBlock(BaseStatusEffect.m_matBlockHelper_STATIC);
		}
		StartCoroutine(PulseCoroutine());
		while (Time.time < base.EndTime)
		{
			yield return null;
		}
		Projectile_RL projectile_RL = ProjectileManager.FireProjectile(PlayerManager.GetPlayerController().gameObject, "SporeBurstProjectile", m_charController.Midpoint, matchFacing: false, 0f, 1f, useAbsPos: true);
		Projectile_RL component = caster.gameObject.GetComponent<Projectile_RL>();
		if ((bool)component)
		{
			projectile_RL.CastAbilityType = component.CastAbilityType;
		}
		m_sporeEffectFired = true;
		if (m_sporeEffect != null && m_sporeEffect.isActiveAndEnabled)
		{
			m_sporeEffect.Animator.SetFloat("tellPercent", 1f);
		}
		StopEffect();
	}

	private IEnumerator SporeEffectCoroutine()
	{
		float startTime = Time.time;
		m_sporeEffect = EffectManager.PlayEffect(m_charController.gameObject, m_charController.Animator, "EnemySpores_Effect", Vector3.zero, base.Duration, EffectStopType.Gracefully) as GenericEffect;
		m_sporeEffect.transform.SetParent(m_charController.transform, worldPositionStays: false);
		m_sporeEffect.transform.position = m_charController.Midpoint;
		while (!m_sporeEffectFired)
		{
			float value = (Time.time - startTime) / base.Duration;
			if ((bool)m_sporeEffect && m_sporeEffect.isActiveAndEnabled)
			{
				m_sporeEffect.Animator.SetFloat("tellPercent", value);
			}
			yield return null;
		}
	}

	private IEnumerator PulseCoroutine()
	{
		while (true)
		{
			m_waitYield.CreateNew(m_burnPulseRate);
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

	public override void StopEffect(bool interrupted = false)
	{
		base.StopEffect(interrupted);
		if (!interrupted)
		{
			if ((bool)m_sporeEffect && m_sporeEffect.isActiveAndEnabled)
			{
				m_sporeEffect.Animator.SetFloat("tellPercent", 1f);
			}
			if (m_charController.IsDead && !m_sporeEffectFired && PlayerManager.IsInstantiated)
			{
				ProjectileManager.FireProjectile(PlayerManager.GetPlayerController().gameObject, "SporeBurstProjectile", m_charController.Midpoint, matchFacing: false, 0f, 1f, useAbsPos: true);
			}
		}
		else if ((bool)m_sporeEffect && m_sporeEffect.isActiveAndEnabled)
		{
			m_sporeEffect.Stop(EffectStopType.Immediate);
		}
	}
}
