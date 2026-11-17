using System.Collections;
using Sigtrap.Relays;
using UnityEngine;

public abstract class BaseStatusEffect : MonoBehaviour
{
	protected const float PULSE_EFFECT_RATE = 0.1f;

	public static MaterialPropertyBlock m_matBlockHelper_STATIC;

	protected WaitRL_Yield m_waitYield;

	protected BaseCharacterController m_charController;

	protected StatusEffectController m_statusEffectController;

	public Relay<BaseStatusEffect> StartEffectRelay = new Relay<BaseStatusEffect>();

	public Relay<BaseStatusEffect> StopEffectRelay = new Relay<BaseStatusEffect>();

	public abstract float StartingDurationOverride { get; }

	public abstract StatusEffectType StatusEffectType { get; }

	public StatusBarEntryType StatusBarType => StatusEffectType_RL.GetStatusBarType(StatusEffectType);

	public bool AppliesTint { get; protected set; }

	public float StartTime { get; private set; }

	public float Duration { get; protected set; }

	public float EndTime => StartTime + Duration;

	public bool IsPlaying { get; protected set; }

	public int TimesStacked { get; protected set; }

	public virtual int StacksPerHit => 1;

	public bool IsHidden { get; protected set; }

	public virtual string[] ProjectileNameArray => null;

	protected virtual void Awake()
	{
		m_waitYield = new WaitRL_Yield(0f);
		if (m_matBlockHelper_STATIC == null)
		{
			m_matBlockHelper_STATIC = new MaterialPropertyBlock();
		}
	}

	public virtual void Initialize(StatusEffectController statusEffectController, BaseCharacterController charController)
	{
		m_statusEffectController = statusEffectController;
		m_charController = charController;
	}

	public virtual void StartEffect(float duration, IDamageObj caster)
	{
		StartEffectRelay.Dispatch(this);
		bool justCasted = false;
		if (IsPlaying)
		{
			StopEffect(interrupted: true);
			TimesStacked += StacksPerHit;
		}
		else
		{
			StopAllCoroutines();
			TimesStacked = StacksPerHit;
			justCasted = true;
		}
		StartTime = Time.time;
		if (StatusEffectType != StatusEffectType.Enemy_SporeBurst && caster != null && (caster.gameObject.CompareTag("PlayerProjectile") || caster.gameObject.CompareTag("Player")))
		{
			float num = 1f + RuneLogicHelper.GetStatusEffectDurationMod();
			duration *= num;
		}
		Duration = duration;
		IsPlaying = true;
		StartCoroutine(StartEffectCoroutine(caster, justCasted));
	}

	protected abstract IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted);

	public virtual void SetIsHidden(bool hide)
	{
		IsHidden = hide;
	}

	public virtual void StopEffect(bool interrupted = false)
	{
		StopEffectRelay.Dispatch(this);
		StopAllCoroutines();
		StartTime = 0f;
		Duration = 0f;
		IsPlaying = false;
		IsHidden = false;
		m_charController.StatusBarController.StopUIEffect(StatusBarType);
		if (AppliesTint && !m_statusEffectController.HasAnyActiveTintedStatusEffect())
		{
			ResetColorTints();
		}
	}

	protected virtual void OnDisable()
	{
		if (IsPlaying && !GameManager.IsApplicationClosing)
		{
			StopEffect();
		}
	}

	protected void ResetColorTints()
	{
		for (int i = 0; i < m_charController.RendererArray.Count; i++)
		{
			Renderer renderer = m_charController.RendererArray[i];
			renderer.GetPropertyBlock(m_matBlockHelper_STATIC);
			m_matBlockHelper_STATIC.SetColor(ShaderID_RL._MultiplyColor, m_charController.RendererArrayDefaultTint[i].DefaultMultiplyColor);
			m_matBlockHelper_STATIC.SetColor(ShaderID_RL._AddColor, m_charController.RendererArrayDefaultTint[i].DefaultAddColor);
			renderer.SetPropertyBlock(m_matBlockHelper_STATIC);
		}
	}
}
