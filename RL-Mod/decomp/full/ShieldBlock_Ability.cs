using System;
using System.Collections;
using FMODUnity;
using MoreMountains.CorgiEngine;
using UnityEngine;

public class ShieldBlock_Ability : BaseAbility_RL, ITalent, IAbility
{
	[SerializeField]
	private StudioEventEmitter m_raiseShieldEmitter;

	[SerializeField]
	private StudioEventEmitter m_lowerShieldEmitter;

	[SerializeField]
	private StudioEventEmitter m_blockEmitter;

	[SerializeField]
	private StudioEventEmitter m_perfectBlockEmitter;

	private bool m_stopAbilityFlag;

	private bool m_isPerfectBlock;

	private WaitRL_Yield m_fireProjectileWaitYield;

	private Action<MonoBehaviour, EventArgs> m_onPlayerBlocked;

	public override string ProjectileName
	{
		get
		{
			if (m_isPerfectBlock)
			{
				return "ShieldBlockPerfectTalentProjectile";
			}
			return base.ProjectileName;
		}
	}

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[2] { m_projectileName, "ShieldBlockPerfectTalentProjectile" };
	}

	protected override void Awake()
	{
		base.Awake();
		m_onPlayerBlocked = OnPlayerBlocked;
	}

	public override void PreCastAbility()
	{
		base.PreCastAbility();
		m_raiseShieldEmitter.Play();
	}

	protected override void OnEnterExitLogic()
	{
		base.OnEnterExitLogic();
		m_lowerShieldEmitter.Play();
	}

	public override IEnumerator CastAbility()
	{
		m_stopAbilityFlag = false;
		m_abilityController.PlayerController.IsBlocking = true;
		m_abilityController.PlayerController.BaseStunDefense += 100f;
		m_abilityController.PlayerController.BaseKnockbackDefense += 100f;
		m_abilityController.PlayerController.BlockStartTime = Time.time;
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerBlocked, m_onPlayerBlocked);
		m_animator.ResetTrigger("Shield_Counterattack");
		yield return base.CastAbility();
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		if (m_abilityController.PlayerController.IsBlocking)
		{
			m_abilityController.PlayerController.DisableFriction = false;
		}
		m_abilityController.PlayerController.IsBlocking = false;
		m_abilityController.PlayerController.BaseStunDefense -= 100f;
		m_abilityController.PlayerController.BaseKnockbackDefense -= 100f;
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerBlocked, m_onPlayerBlocked);
		if (m_abilityController.PlayerController.ConditionState == CharacterStates.CharacterConditions.DisableHorizontalMovement)
		{
			m_abilityController.PlayerController.ConditionState = CharacterStates.CharacterConditions.Normal;
		}
		m_abilityController.PlayerController.ControllerCorgi.GravityActive(state: true);
		base.StopAbility(abilityInterrupted);
	}

	private void OnPlayerBlocked(MonoBehaviour sender, EventArgs args)
	{
		if (!m_abilityController.PlayerController.IsDead)
		{
			PlayerBlockedEventArgs playerBlockedEventArgs = args as PlayerBlockedEventArgs;
			if ((m_abilityController.PlayerController.IsFacingRight && playerBlockedEventArgs.DamageObj.gameObject.transform.position.x < m_abilityController.PlayerController.transform.position.x) || (!m_abilityController.PlayerController.IsFacingRight && playerBlockedEventArgs.DamageObj.gameObject.transform.position.x > m_abilityController.PlayerController.transform.position.x))
			{
				m_abilityController.PlayerController.CharacterCorgi.Flip();
			}
			m_isPerfectBlock = Time.time < m_abilityController.PlayerController.BlockStartTime + 0.135f;
			if (m_isPerfectBlock)
			{
				m_perfectBlockEmitter.Play();
			}
			else
			{
				m_blockEmitter.Play();
			}
			Invoke("FireProjectile", 0.15f);
			StartCooldownTimer();
			m_animator.SetTrigger("Shield_Counterattack");
			m_abilityController.PlayerController.CharacterHitResponse.SetInvincibleTime(1.25f, additive: false, playInvincibility: true);
			EffectManager.SetEffectParams("SlowTime_Effect", "m_timeScaleValue", 0.1f);
			EffectManager.PlayEffect(base.gameObject, m_abilityController.PlayerController.Animator, "SlowTime_Effect", Vector3.zero, 0.25f, EffectStopType.Gracefully);
			m_abilityController.PlayerController.ConditionState = CharacterStates.CharacterConditions.DisableHorizontalMovement;
			m_abilityController.PlayerController.DisableFriction = true;
			m_abilityController.PlayerController.ControllerCorgi.GravityActive(state: true);
			if (m_abilityController.PlayerController.IsFacingRight)
			{
				m_abilityController.PlayerController.SetVelocity(0f - Ability_EV.SHIELD_BLOCK_KNOCKBACK_ADD.x, Ability_EV.SHIELD_BLOCK_KNOCKBACK_ADD.y, additive: false);
			}
			else
			{
				m_abilityController.PlayerController.SetVelocity(Ability_EV.SHIELD_BLOCK_KNOCKBACK_ADD.x, Ability_EV.SHIELD_BLOCK_KNOCKBACK_ADD.y, additive: false);
			}
			m_stopAbilityFlag = true;
		}
	}

	protected override void FireProjectile()
	{
		base.FireProjectile();
		m_firedProjectile = null;
	}

	protected override IEnumerator ChangeAnim(float duration)
	{
		if (base.CurrentAbilityAnimState == AbilityAnimState.Tell)
		{
			while (!m_stopAbilityFlag && (Rewired_RL.Player.GetButtonDown(m_abilityController.GetAbilityInputString(base.CastAbilityType)) || Rewired_RL.Player.GetButton(m_abilityController.GetAbilityInputString(base.CastAbilityType))))
			{
				yield return null;
			}
		}
		yield return base.ChangeAnim(duration);
	}
}
