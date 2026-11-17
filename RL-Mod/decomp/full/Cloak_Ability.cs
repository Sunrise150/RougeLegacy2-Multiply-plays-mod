using System.Collections;
using FMODUnity;
using UnityEngine;

public class Cloak_Ability : BaseAbility_RL, ITalent, IAbility
{
	[SerializeField]
	private StudioEventEmitter m_cloakOnAudioEmitter;

	[SerializeField]
	private StudioEventEmitter m_cloakOffAudioEmitter;

	private const float BLINK_DURATION = 1f;

	private const float BLINK_INTERVAL = 0.1f;

	private Color m_eyeTint = new Color(0.6f, 0f, 0f, 1f);

	private WaitRL_Yield m_waitYield;

	private bool m_isCloaked;

	private PlayerLookController m_lookController;

	private BaseEffect m_cloakEffect;

	private bool m_baseEffectOn;

	private Coroutine m_blinkCoroutine;

	private bool m_isCloakStriking;

	private Color m_storedEyeColor;

	private Color m_storedAlphaBlendEyeColor;

	private Color m_storedRimEyeColor;

	private MaterialPropertyBlock m_matPropertyBlock;

	public bool IsCloaked => m_isCloaked;

	public override bool IgnoreStuckCheck => true;

	protected override void Awake()
	{
		base.Awake();
		m_matPropertyBlock = new MaterialPropertyBlock();
	}

	private IEnumerator Start()
	{
		while (!m_abilityController)
		{
			yield return null;
		}
		m_lookController = m_abilityController.PlayerController.LookController;
		m_waitYield = new WaitRL_Yield(0f);
	}

	public override IEnumerator CastAbility()
	{
		m_beginCastingRelay.Dispatch();
		PlayerController playerController = m_abilityController.PlayerController;
		if (!m_isCloaked)
		{
			m_isCloaked = true;
			m_abilityController.StopAbility(CastAbilityType.Weapon, abilityInterrrupted: true);
			m_abilityController.StopAbility(CastAbilityType.Spell, abilityInterrrupted: true);
			playerController.CharacterHitResponse.StopInvincibleTime();
			playerController.CharacterHitResponse.SetInvincibleTime(3f, additive: false, playInvincibility: false);
			playerController.HitboxController.SetHitboxActiveState(HitboxType.Body, active: false);
			playerController.StatusBarController.ApplyUIEffect(StatusBarEntryType.Cloak, 3f);
			playerController.MovementSpeedMod += 0.5f;
			ApplyCloakEffect();
			m_waitYield.CreateNew(3f);
			yield return m_waitYield;
		}
		else
		{
			if (m_blinkCoroutine != null)
			{
				StopCoroutine(m_blinkCoroutine);
				m_blinkCoroutine = null;
			}
			playerController.CharacterHitResponse.StopInvincibleTime();
			playerController.HitboxController.SetHitboxActiveState(HitboxType.Body, active: true);
			playerController.MovementSpeedMod -= 0.5f;
			RemoveCloakEffect();
			yield return null;
		}
	}

	private IEnumerator BlinkCoroutine()
	{
		float startTime = Time.time + 2f;
		while (Time.time < startTime)
		{
			yield return null;
		}
		float blinkTimer = Time.time + 1f;
		float toggleTimer = 0.1f;
		while (Time.time < blinkTimer && blinkTimer - Time.time > 0.1f)
		{
			if (toggleTimer <= 0f)
			{
				if (!m_baseEffectOn)
				{
					ApplyCloakEffect(applyBaseEffect: false);
				}
				else
				{
					RemoveCloakEffect(removeBaseEffect: false);
				}
				toggleTimer = 0.1f;
			}
			else
			{
				toggleTimer -= Time.deltaTime;
			}
			yield return null;
		}
		if (!m_baseEffectOn)
		{
			ApplyCloakEffect(applyBaseEffect: false);
		}
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		if (!base.IsOnCooldown)
		{
			StartCooldownTimer();
		}
		m_isCloaked = false;
		m_abilityController.PlayerController.CharacterHitResponse.SetInvincibleTime(0.1f, additive: false, playInvincibility: false);
		m_abilityController.PlayerController.HitboxController.SetHitboxActiveState(HitboxType.Body, active: true);
		m_abilityController.PlayerController.StatusBarController.StopUIEffect(StatusBarEntryType.Cloak);
		if (m_baseEffectOn)
		{
			m_abilityController.PlayerController.MovementSpeedMod -= 0.5f;
			RemoveCloakEffect();
		}
		m_isCloakStriking = false;
		m_abilityController.PlayerController.CloakInterrupted = abilityInterrupted;
		if (m_abilityController.PlayerController.CastAbility.LastCastAbilityTypeCasted != 0)
		{
			m_abilityController.PlayerController.CloakInterrupted = false;
		}
		base.StopAbility(abilityInterrupted);
	}

	private void ApplyCloakEffect(bool applyBaseEffect = true)
	{
		m_baseEffectOn = true;
		m_lookController.LeftEyeGeo.GetPropertyBlock(m_matPropertyBlock);
		m_storedEyeColor = m_matPropertyBlock.GetColor(ShaderID_RL._MainColor);
		m_storedAlphaBlendEyeColor = m_matPropertyBlock.GetColor(ShaderID_RL._AlphaBlendColor);
		m_storedRimEyeColor = m_matPropertyBlock.GetColor(ShaderID_RL._RimLightColor);
		m_abilityController.PlayerController.BlinkPulseEffect.ActivateBlackFill(BlackFillType.Cloak, 0f);
		m_matPropertyBlock.SetColor(ShaderID_RL._MainColor, m_eyeTint);
		m_matPropertyBlock.SetColor(ShaderID_RL._AlphaBlendColor, m_storedAlphaBlendEyeColor);
		m_matPropertyBlock.SetColor(ShaderID_RL._RimLightColor, m_storedRimEyeColor);
		m_lookController.LeftEyeGeo.SetPropertyBlock(m_matPropertyBlock);
		m_lookController.RightEyeGeo.SetPropertyBlock(m_matPropertyBlock);
		if (m_cloakEffect == null && applyBaseEffect)
		{
			EffectManager.PlayEffect(m_abilityController.PlayerController.gameObject, m_abilityController.PlayerController.Animator, "CloakPuff_Effect", m_abilityController.PlayerController.Midpoint, -1f, EffectStopType.Gracefully);
			m_cloakEffect = EffectManager.PlayEffect(m_abilityController.PlayerController.gameObject, m_abilityController.PlayerController.Animator, "CloakTrail_Effect", Vector3.zero, 100000000f, EffectStopType.Gracefully);
			m_cloakEffect.transform.SetParent(m_abilityController.PlayerController.transform, worldPositionStays: false);
			m_cloakEffect.transform.position = m_abilityController.PlayerController.Midpoint;
			m_cloakEffect.DisableDestroyOnRoomChange = true;
		}
		m_cloakOnAudioEmitter.Play();
	}

	private void RemoveCloakEffect(bool removeBaseEffect = true)
	{
		m_baseEffectOn = false;
		m_abilityController.PlayerController.BlinkPulseEffect.DisableBlackFill(BlackFillType.Cloak, 0f);
		m_lookController.LeftEyeGeo.GetPropertyBlock(m_matPropertyBlock);
		m_matPropertyBlock.SetColor(ShaderID_RL._MainColor, m_storedEyeColor);
		m_matPropertyBlock.SetColor(ShaderID_RL._AlphaBlendColor, m_storedAlphaBlendEyeColor);
		m_matPropertyBlock.SetColor(ShaderID_RL._RimLightColor, m_storedRimEyeColor);
		m_lookController.LeftEyeGeo.SetPropertyBlock(m_matPropertyBlock);
		m_lookController.RightEyeGeo.SetPropertyBlock(m_matPropertyBlock);
		if (removeBaseEffect && m_cloakEffect != null && m_cloakEffect.IsPlaying && m_cloakEffect.Source == m_abilityController.PlayerController.gameObject)
		{
			EffectManager.PlayEffect(m_abilityController.PlayerController.gameObject, m_abilityController.PlayerController.Animator, "CloakPuff_Effect", m_abilityController.PlayerController.Midpoint, -1f, EffectStopType.Gracefully);
			EffectManager.StopEffect(m_cloakEffect, EffectStopType.Gracefully);
			m_cloakEffect = null;
		}
		m_cloakOnAudioEmitter.Stop();
		m_cloakOffAudioEmitter.Play();
	}
}
