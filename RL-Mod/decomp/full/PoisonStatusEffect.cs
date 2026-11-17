using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PoisonStatusEffect : BaseStatusEffect, IDamageObj
{
	private const string MULTIPLY_COLOR = "#65A633";

	private const string PULSE_ON_COLOR = "#2B4500";

	private const string PULSE_OFF_COLOR = "#000000";

	private Color m_multiplyColor;

	private Color m_addColorPulseOn;

	private Color m_addColorPulseOff;

	private bool m_pulseOn;

	private float m_poisonTickRate = 0.5f;

	private float m_poisonPulseRate = 0.1f;

	private float m_poisonMagicDmg;

	private BaseEffect m_poisonEffect;

	private float m_poisonSkillCritTime;

	private float m_poisonTicRemaining;

	public override StatusEffectType StatusEffectType => StatusEffectType.Enemy_Poison;

	public StatusEffectType[] StatusEffectTypes => null;

	public float[] StatusEffectDurations => null;

	public override float StartingDurationOverride => 4f;

	public bool IsDotDamage => true;

	public float BaseDamage
	{
		get
		{
			float num = 0.05f;
			return m_poisonMagicDmg * num;
		}
	}

	public float ActualDamage => BaseDamage * (float)base.TimesStacked;

	public float ActualCritChance
	{
		get
		{
			if (Time.time >= m_poisonSkillCritTime)
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

	public override int StacksPerHit
	{
		get
		{
			int result = base.StacksPerHit;
			int level = SaveManager.PlayerSaveData.GetRelic(RelicType.WeaponsPoisonAdd).Level;
			if (level > 0)
			{
				result = level;
			}
			return result;
		}
	}

	public override void Initialize(StatusEffectController statusEffectController, BaseCharacterController charController)
	{
		base.Initialize(statusEffectController, charController);
		ColorUtility.TryParseHtmlString("#65A633", out m_multiplyColor);
		ColorUtility.TryParseHtmlString("#2B4500", out m_addColorPulseOn);
		ColorUtility.TryParseHtmlString("#000000", out m_addColorPulseOff);
		base.AppliesTint = true;
	}

	private IEnumerator PulseCoroutine()
	{
		while (true)
		{
			m_waitYield.CreateNew(m_poisonPulseRate);
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
		base.Duration += 0.05f;
		while (true)
		{
			float delayTime = Time.time + m_poisonTickRate - m_poisonTicRemaining;
			while (Time.time < delayTime)
			{
				m_poisonTicRemaining = m_poisonTickRate - (delayTime - Time.time);
				yield return null;
			}
			m_poisonTicRemaining = 0f;
			m_charController.HitboxController.LastCollidedWith = null;
			m_charController.CharacterHitResponse.StartHitResponse(base.gameObject, this);
		}
	}

	protected override IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted)
	{
		if (justCasted)
		{
			m_poisonSkillCritTime = Time.time + 6f;
		}
		base.TimesStacked = Mathf.Clamp(base.TimesStacked, 0, 10);
		Projectile_RL projectile_RL = caster as Projectile_RL;
		PlayerController playerController = (projectile_RL ? (projectile_RL.OwnerController as PlayerController) : (caster as PlayerController));
		if ((bool)playerController)
		{
			m_poisonMagicDmg = playerController.ActualMagic;
			if ((bool)projectile_RL)
			{
				m_poisonMagicDmg *= 1f + projectile_RL.DamageMod;
			}
			float actualFocus = playerController.ActualFocus;
			float actualMagicCritDamage = playerController.ActualMagicCritDamage;
			float num = 0.55f;
			int num2 = Mathf.RoundToInt(actualFocus * num * actualMagicCritDamage);
			ActualCritDamage = num2;
		}
		else if ((bool)projectile_RL)
		{
			m_poisonMagicDmg = projectile_RL.Magic * (1f + projectile_RL.DamageMod);
			ActualCritDamage = 0f;
		}
		else
		{
			m_poisonMagicDmg = 0f;
			ActualCritDamage = 0f;
		}
		m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.Poison, base.Duration, 10, base.TimesStacked);
		m_pulseOn = false;
		foreach (Renderer item in m_charController.RendererArray)
		{
			item.GetPropertyBlock(BaseStatusEffect.m_matBlockHelper_STATIC);
			BaseStatusEffect.m_matBlockHelper_STATIC.SetColor(ShaderID_RL._MultiplyColor, m_multiplyColor);
			BaseStatusEffect.m_matBlockHelper_STATIC.SetColor(ShaderID_RL._AddColor, m_addColorPulseOff);
			item.SetPropertyBlock(BaseStatusEffect.m_matBlockHelper_STATIC);
		}
		m_poisonEffect = EffectManager.PlayEffect(m_charController.gameObject, m_charController.Animator, "EnemyPoisonStatus_Effect", Vector3.zero, base.Duration, EffectStopType.Gracefully);
		m_poisonEffect.transform.SetParent(m_charController.transform, worldPositionStays: false);
		m_poisonEffect.transform.position = m_charController.Midpoint;
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
			m_poisonTicRemaining = 0f;
		}
		if ((bool)m_poisonEffect && m_poisonEffect.isActiveAndEnabled)
		{
			m_poisonEffect.Stop(EffectStopType.Gracefully);
		}
	}

	[SpecialName]
	GameObject IDamageObj.get_gameObject()
	{
		return base.gameObject;
	}
}
