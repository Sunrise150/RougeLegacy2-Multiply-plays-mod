using System;
using System.Collections;
using FMOD.Studio;
using RLAudio;
using UnityEngine;

public class Comet_Ability : BaseAbility_RL, ITalent, IAbility, IPersistentAbility
{
	private const string SFX_COMET_LOOP_NAME = "event:/SFX/Spells/sfx_spell_astromancer_cometForm_loop";

	private bool m_isComet;

	private WaitRL_Yield m_waitYield;

	private Action<MonoBehaviour, EventArgs> m_onPlayerJump;

	private Action<MonoBehaviour, EventArgs> m_onPlayerManaChange;

	private float m_manaRequiredForNextCharge;

	private EventInstance m_cometSFXInstance;

	private bool m_cometSFXParamIsMoving;

	public override bool IgnoreStuckCheck => true;

	public bool IsPersistentActive => m_isComet;

	protected override void Awake()
	{
		m_onPlayerJump = OnPlayerJump;
		m_waitYield = new WaitRL_Yield(0f);
		base.Awake();
	}

	public override void Initialize(CastAbility_RL abilityController, CastAbilityType castAbilityType)
	{
		base.Initialize(abilityController, castAbilityType);
		m_manaRequiredForNextCharge = 1000f;
		if (!m_cometSFXInstance.isValid())
		{
			m_cometSFXInstance = AudioUtility.GetEventInstance("event:/SFX/Spells/sfx_spell_astromancer_cometForm_loop", base.transform);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (m_cometSFXInstance.isValid())
		{
			m_cometSFXInstance.release();
		}
	}

	public override IEnumerator CastAbility()
	{
		m_beginCastingRelay.Dispatch();
		PlayerController playerController = m_abilityController.PlayerController;
		if (!m_isComet)
		{
			m_isComet = true;
			m_abilityController.StopAbility(CastAbilityType.Weapon, abilityInterrrupted: true);
			m_abilityController.StopAbility(CastAbilityType.Spell, abilityInterrrupted: true);
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerJump, m_onPlayerJump);
			float num = 1.5f;
			playerController.CharacterHitResponse.StopInvincibleTime();
			playerController.CharacterHitResponse.SetInvincibleTime(num, additive: false, playInvincibility: false);
			playerController.HitboxController.SetHitboxActiveState(HitboxType.Body, active: false);
			playerController.Visuals.SetActive(value: false);
			FireProjectile();
			m_abilityController.BroadcastAbilityCastEvents(base.CastAbilityType);
			if (!playerController.CharacterFlight.IsFlying)
			{
				playerController.CharacterFlight.StartFlight(num, 1f);
			}
			else
			{
				playerController.CharacterFlight.MovementSpeedMultiplier += 1f;
			}
			playerController.StatusBarController.ApplyUIEffect(StatusBarEntryType.Cloak, num);
			m_waitYield.CreateNew(1.5f);
			yield return m_waitYield;
			_ = m_isComet;
		}
	}

	private void OnPlayerJump(MonoBehaviour sender, EventArgs args)
	{
		StopAbility(abilityInterrupted: true);
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		if (m_isComet)
		{
			PlayerController playerController = m_abilityController.PlayerController;
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerJump, m_onPlayerJump);
			playerController.CharacterHitResponse.SetInvincibleTime(0.1f, additive: false, playInvincibility: false);
			playerController.HitboxController.SetHitboxActiveState(HitboxType.Body, active: true);
			playerController.Visuals.SetActive(value: true);
			if ((bool)m_firedProjectile)
			{
				m_firedProjectile.FlagForDestruction();
			}
			if (!playerController.CharacterFlight.IsAssistFlying)
			{
				playerController.CharacterFlight.StopFlight();
			}
			playerController.CharacterFlight.MovementSpeedMultiplier -= 1f;
			playerController.StatusBarController.StopUIEffect(StatusBarEntryType.Cloak);
		}
		m_isComet = false;
		m_cometSFXParamIsMoving = false;
		if (m_cometSFXInstance.isValid())
		{
			AudioManager.Stop(m_cometSFXInstance);
		}
		base.StopAbility(abilityInterrupted);
	}

	protected override void FireProjectile()
	{
		base.FireProjectile();
		if (m_cometSFXInstance.isValid())
		{
			AudioManager.PlayAttached(null, m_cometSFXInstance, m_abilityController.PlayerController.gameObject);
			m_cometSFXInstance.setParameterByName("cometFormSpeed", 0f);
			m_cometSFXParamIsMoving = false;
		}
		if (base.DecreaseCooldownOverTime)
		{
			StartCooldownTimer();
			base.DecreaseCooldownOverTime = false;
			base.DisplayPausedAbilityCooldown = true;
			if ((bool)m_firedProjectile)
			{
				m_firedProjectile.OnDeathRelay.AddOnce(ResumeCooldown);
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!base.AbilityActive || !m_cometSFXInstance.isValid())
		{
			return;
		}
		m_cometSFXInstance.getPlaybackState(out var state);
		if (state == PLAYBACK_STATE.PLAYING)
		{
			if (m_cometSFXParamIsMoving && m_abilityController.PlayerController.Velocity.x == 0f && m_abilityController.PlayerController.Velocity.y == 0f)
			{
				m_cometSFXParamIsMoving = false;
				m_cometSFXInstance.setParameterByName("cometFormSpeed", 0f);
			}
			else if (!m_cometSFXParamIsMoving && (m_abilityController.PlayerController.Velocity.x != 0f || m_abilityController.PlayerController.Velocity.y != 0f))
			{
				m_cometSFXParamIsMoving = true;
				m_cometSFXInstance.setParameterByName("cometFormSpeed", 1f);
			}
		}
	}

	public void StopPersistentAbility()
	{
		StopAbility(abilityInterrupted: true);
	}

	private void ResumeCooldown(Projectile_RL proj, GameObject obj)
	{
		base.DecreaseCooldownOverTime = true;
		base.DisplayPausedAbilityCooldown = false;
	}
}
