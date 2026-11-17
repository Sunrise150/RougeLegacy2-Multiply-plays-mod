using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BurnStatusEffect : BaseStatusEffect, IDamageObj
{
	private const string MULTIPLY_COLOR = "#753B3B";

	private const string PULSE_ON_COLOR = "#451200";

	private const string PULSE_OFF_COLOR = "#000000";

	private Color m_multiplyColor;

	private Color m_addColorPulseOn;

	private Color m_addColorPulseOff;

	private bool m_pulseOn;

	private float m_burnMagicDmg;

	private BaseEffect m_burnEffect;

	private float m_burnTicRemaining;

	private bool m_isCritHit;

	public StatusEffectType[] StatusEffectTypes => null;

	public float[] StatusEffectDurations => null;

	public override StatusEffectType StatusEffectType => StatusEffectType.Enemy_Burn;

	public override float StartingDurationOverride => 3.05f;

	public bool IsDotDamage => true;

	public float BaseDamage
	{
		get
		{
			float num = 0.55f;
			return m_burnMagicDmg * num;
		}
	}

	public float ActualDamage => BaseDamage;

	public float ActualCritChance
	{
		get
		{
			if (m_isCritHit)
			{
				return 100f;
			}
			return 0f;
		}
	}

	public float ActualCritDamage { get; private set; }

	public Vector2 ExternalKnockbackMod => Vector2.zero;

	public float ActualKnockbackStrength => BaseKnockbackStrength;

	public float ActualStunStrength => BaseStunStrength;

	public float BaseStunStrength
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	public float BaseKnockbackStrength
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	public float KnockbackStrength => 0f;

	public StrikeType StrikeType => StrikeType.Blunt;

	public float StatusEffectDuration => 0f;

	public override void Initialize(StatusEffectController statusEffectController, BaseCharacterController charController)
	{
		base.Initialize(statusEffectController, charController);
		ColorUtility.TryParseHtmlString("#753B3B", out m_multiplyColor);
		ColorUtility.TryParseHtmlString("#451200", out m_addColorPulseOn);
		ColorUtility.TryParseHtmlString("#000000", out m_addColorPulseOff);
		base.AppliesTint = true;
	}

	public override void StartEffect(float duration, IDamageObj caster)
	{
		if (!m_statusEffectController.HasStatusEffect(StatusEffectType.Enemy_Burn_Immunity))
		{
			base.StartEffect(duration, caster);
		}
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

	private IEnumerator DealDamageCoroutine()
	{
		int totalBurnCount = (int)((base.Duration + m_burnTicRemaining) / 0.5f);
		int totalCritCount = (int)(base.Duration * 0.35f / 0.5f);
		int burnCount = 0;
		base.Duration += 0.05f;
		while (true)
		{
			float delayTime = Time.time + 0.5f - m_burnTicRemaining;
			while (Time.time < delayTime)
			{
				m_burnTicRemaining = 0.5f - (delayTime - Time.time);
				yield return null;
			}
			burnCount++;
			if (totalBurnCount - burnCount < totalCritCount)
			{
				m_isCritHit = true;
			}
			else
			{
				m_isCritHit = false;
			}
			m_burnTicRemaining = 0f;
			m_charController.HitboxController.LastCollidedWith = null;
			m_charController.CharacterHitResponse.StartHitResponse(base.gameObject, this);
		}
	}

	protected override IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted)
	{
		Projectile_RL projectile_RL = caster as Projectile_RL;
		PlayerController playerController = (projectile_RL ? (projectile_RL.OwnerController as PlayerController) : (caster as PlayerController));
		if ((bool)playerController)
		{
			m_burnMagicDmg = playerController.ActualMagic;
			if ((bool)projectile_RL)
			{
				m_burnMagicDmg *= 1f + projectile_RL.DamageMod;
			}
			float actualFocus = playerController.ActualFocus;
			float actualMagicCritDamage = playerController.ActualMagicCritDamage;
			float num = 0.55f;
			int num2 = Mathf.RoundToInt(actualFocus * num * actualMagicCritDamage);
			ActualCritDamage = num2;
		}
		else if ((bool)projectile_RL)
		{
			m_burnMagicDmg = projectile_RL.Magic * (1f + projectile_RL.DamageMod);
			ActualCritDamage = 0f;
		}
		else
		{
			m_burnMagicDmg = 0f;
			ActualCritDamage = 0f;
		}
		m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.Burn, base.Duration);
		m_pulseOn = false;
		foreach (Renderer item in m_charController.RendererArray)
		{
			item.GetPropertyBlock(BaseStatusEffect.m_matBlockHelper_STATIC);
			BaseStatusEffect.m_matBlockHelper_STATIC.SetColor(ShaderID_RL._MultiplyColor, m_multiplyColor);
			BaseStatusEffect.m_matBlockHelper_STATIC.SetColor(ShaderID_RL._AddColor, m_addColorPulseOff);
			item.SetPropertyBlock(BaseStatusEffect.m_matBlockHelper_STATIC);
		}
		m_burnEffect = EffectManager.PlayEffect(m_charController.gameObject, m_charController.Animator, "EnemyBurnFlames_Effect", Vector3.zero, base.Duration, EffectStopType.Gracefully);
		m_burnEffect.transform.SetParent(m_charController.transform, worldPositionStays: false);
		m_burnEffect.transform.position = m_charController.Midpoint;
		StartCoroutine(PulseCoroutine());
		StartCoroutine(DealDamageCoroutine());
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
			m_burnTicRemaining = 0f;
		}
		if ((bool)m_burnEffect && m_burnEffect.isActiveAndEnabled)
		{
			m_burnEffect.Stop(EffectStopType.Gracefully);
		}
	}

	[SpecialName]
	GameObject IDamageObj.get_gameObject()
	{
		return base.gameObject;
	}
}
