using System.Collections;
using System.Collections.Generic;
using MoreMountains.CorgiEngine;
using Sigtrap.Relays;
using UnityEngine;

public abstract class BaseCharacterHitResponse : MonoBehaviour, ITerrainOnEnterHitResponse, IHitResponse, ITerrainOnStayHitResponse, IBodyOnEnterHitResponse, IBodyOnStayHitResponse, IEffectTriggerEvent_OnDamage
{
	[Header("On-Hit Settings")]
	[Space(10f)]
	[SerializeField]
	[Tooltip("Immediately cancels the stun state after landing if knocked back. If this is true and no knockback is set, the character will immediately break out of their stun state.")]
	protected bool m_cancelStunWhenLanding;

	[Header("Invincibility Settings")]
	[SerializeField]
	[Tooltip("Makes invincibility only trigger after the character's stun state has completed. Otherwise they kick in at the same time.")]
	protected bool m_triggerInvincibilityAfterStun = true;

	[SerializeField]
	[Tooltip("Makes the character still hittable while in a stunned state.  Trigger Invincibility After Stun takes precendence over this.")]
	protected bool m_takeDamageWhileStunned;

	[SerializeField]
	protected bool m_blinkOnHit = true;

	[Header("Debug Timers (Read-only)")]
	[SerializeField]
	[ReadOnly]
	protected float m_stunTimer;

	[SerializeField]
	[ReadOnly]
	protected float m_invincibilityTimer;

	public static List<string> GLYPHS_TO_ADD_TO_ENEMY_DAMAGE_HELPER = new List<string>();

	public static List<string> GLYPHS_TO_ADD_TO_PLAYER_DAMAGE_HELPER = new List<string>();

	protected BaseCharacterController m_charController;

	protected BlinkPulseEffect m_hitEffect;

	protected CharacterHitEventArgs m_characterHitArgs;

	protected float m_landCheckDelay = 0.05f;

	protected float m_landCheckDelayTimer;

	private Relay<object, CharacterHitEventArgs> m_onCharacterHitRelay = new Relay<object, CharacterHitEventArgs>();

	private Relay<GameObject, float, bool> m_onDamageEffectTriggerRelay = new Relay<GameObject, float, bool>();

	public IRelayLink<object, CharacterHitEventArgs> OnCharacterHitRelay => m_onCharacterHitRelay.link;

	public IRelayLink<GameObject, float, bool> OnDamageEffectTriggerRelay => m_onDamageEffectTriggerRelay.link;

	public bool InvincibilityEffectPlaying { get; protected set; }

	public virtual bool TakesDamageWhileStunned => m_takeDamageWhileStunned;

	public virtual bool TriggerInvincibilityAfterStun => m_triggerInvincibilityAfterStun;

	public virtual bool IsInvincible => m_invincibilityTimer > 0f;

	public float InvincibleTimer => m_invincibilityTimer;

	public bool IsStunned => m_stunTimer > 0f;

	public BlinkPulseEffect BlinkPulseEffect => m_hitEffect;

	public string StunnedAnimParamName { get; set; } = "Stunned";


	public bool AnimateHitEffectsOnUnscaledTime { get; set; }

	protected virtual void Awake()
	{
		GameObject root = this.GetRoot();
		m_charController = root.GetComponent<BaseCharacterController>();
		m_hitEffect = root.GetComponent<BlinkPulseEffect>();
		m_characterHitArgs = new CharacterHitEventArgs(null, null, 0f);
	}

	protected virtual void HandleHitResponse(IHitboxController otherHBController)
	{
		if (otherHBController == null || !m_charController)
		{
			return;
		}
		switch (m_charController.ConditionState)
		{
		case CharacterStates.CharacterConditions.Normal:
		case CharacterStates.CharacterConditions.Frozen:
		case CharacterStates.CharacterConditions.Stunned:
		case CharacterStates.CharacterConditions.DisableHorizontalMovement:
			if (m_charController.ConditionState != CharacterStates.CharacterConditions.Stunned || (TakesDamageWhileStunned && !TriggerInvincibilityAfterStun))
			{
				StartHitResponse(otherHBController.RootGameObject, otherHBController.DamageObj);
			}
			break;
		case CharacterStates.CharacterConditions.ControlledMovement:
		case CharacterStates.CharacterConditions.Paused:
			break;
		}
	}

	public virtual void StartHitResponse(GameObject otherRootGameObj, IDamageObj damageObj, float damageOverride = -1f, bool trueDamage = false, bool fireEvents = true)
	{
		if (m_charController.IsDead || IsInvincible)
		{
			return;
		}
		Projectile_RL projectile_RL = damageObj as Projectile_RL;
		if (!projectile_RL)
		{
			projectile_RL = otherRootGameObj.GetComponentInParent<Projectile_RL>();
		}
		if ((bool)projectile_RL && (bool)projectile_RL.Owner)
		{
			otherRootGameObj = projectile_RL.Owner;
		}
		if ((bool)projectile_RL && projectile_RL.Owner == base.gameObject && !projectile_RL.CanHitOwner)
		{
			return;
		}
		m_landCheckDelayTimer = m_landCheckDelay;
		_ = Vector2.one;
		_ = m_charController.InternalKnockbackMod;
		float num = 1f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 1f;
		float num5 = 0f;
		float damageBlocked = 0f;
		CriticalStrikeType critType = CriticalStrikeType.None;
		if (damageObj != null)
		{
			_ = damageObj.ExternalKnockbackMod;
			num = damageObj.ActualKnockbackStrength;
			num2 = m_charController.ActualKnockbackDefense;
			num3 = damageObj.ActualStunStrength;
			num4 = m_charController.ActualStunDefense;
			num5 = CharacterDamaged(damageObj, otherRootGameObj, out critType, out damageBlocked, damageOverride, trueDamage);
		}
		if (fireEvents)
		{
			m_characterHitArgs.Initialize(damageObj, m_charController, num5);
			m_onCharacterHitRelay.Dispatch(this, m_characterHitArgs);
			if (m_charController is PlayerController)
			{
				Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerHit, this, m_characterHitArgs);
			}
			else
			{
				Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.EnemyHit, this, m_characterHitArgs);
			}
		}
		if (m_charController.CurrentHealth <= 0f)
		{
			if ((bool)projectile_RL && (bool)projectile_RL.Owner)
			{
				CharacterKilled(damageObj, projectile_RL.Owner);
			}
			else
			{
				CharacterKilled(damageObj, otherRootGameObj);
			}
		}
		if (fireEvents)
		{
			m_onDamageEffectTriggerRelay.Dispatch(damageObj.gameObject, num5, critType != CriticalStrikeType.None);
		}
		if (damageObj.IsDotDamage)
		{
			return;
		}
		if (m_blinkOnHit && num5 > 0f)
		{
			if ((m_charController.CurrentHealth <= 0f || AnimateHitEffectsOnUnscaledTime) && m_charController.CompareTag("Player"))
			{
				m_hitEffect.UseUnscaledTime = true;
				m_hitEffect.SingleBlinkDuration = 0.2f;
			}
			else
			{
				m_hitEffect.UseUnscaledTime = false;
				m_hitEffect.SingleBlinkDuration = Player_EV.CHARACTER_HIT_SINGLE_BLINK_DURATION;
			}
			m_hitEffect.StartSingleBlinkEffect();
		}
		if (num3 > num4 && m_charController.StunDuration > 0f)
		{
			CharacterStunned(damageObj, otherRootGameObj);
		}
		bool flag = num > num2 && m_charController.CurrentHealth > 0f;
		if (m_charController is EnemyController && m_charController.CurrentHealth <= 0f)
		{
			flag = (m_charController as EnemyController).LogicController.LogicScript.ForceDeathAnimation;
		}
		if (flag)
		{
			if ((bool)projectile_RL && projectile_RL.UseOwnerCollisionPoint && (bool)projectile_RL.Owner)
			{
				CharacterKnockedBack(damageObj, projectile_RL.Owner);
			}
			else
			{
				CharacterKnockedBack(damageObj, damageObj.gameObject);
			}
			m_charController.KnockedIntoAir = true;
		}
	}

	protected virtual void PerformKnockbackPush(GameObject otherRootObj, Vector2 knockbackAmount)
	{
		int num = 0;
		num = ((!otherRootObj.CompareTag("Hazard")) ? ((otherRootObj.transform.position.x < base.gameObject.transform.position.x) ? 1 : (-1)) : ((m_charController.Velocity.x == 0f) ? ((!m_charController.IsFacingRight) ? 1 : (-1)) : ((m_charController.Velocity.x < 0f) ? 1 : (-1))));
		if (m_charController.ConditionState == CharacterStates.CharacterConditions.Normal)
		{
			StopAllCoroutines();
			StartCoroutine(DisableMovementCoroutine());
		}
		m_charController.SetVelocity(knockbackAmount.x * (float)num, knockbackAmount.y, additive: false);
	}

	private IEnumerator DisableMovementCoroutine()
	{
		m_charController.ConditionState = CharacterStates.CharacterConditions.DisableHorizontalMovement;
		float disableDuration = 0.25f;
		float startTime = Time.time;
		while (startTime + disableDuration > Time.time)
		{
			yield return null;
		}
		if (m_charController.ConditionState == CharacterStates.CharacterConditions.DisableHorizontalMovement)
		{
			m_charController.ConditionState = CharacterStates.CharacterConditions.Normal;
		}
	}

	protected virtual void CharacterStunned(IDamageObj damageObj, GameObject otherRootObj)
	{
		m_stunTimer = m_charController.StunDuration;
		m_charController.Animator.SetBool(StunnedAnimParamName, value: true);
		m_charController.ConditionState = CharacterStates.CharacterConditions.Stunned;
		m_charController.DisableGroundedState();
	}

	public void StopCharacterStunned()
	{
		m_stunTimer = 0f;
		m_charController.Animator.SetBool(StunnedAnimParamName, value: false);
		if (!m_charController.IsDead)
		{
			m_charController.ConditionState = CharacterStates.CharacterConditions.Normal;
			if (TriggerInvincibilityAfterStun)
			{
				SetInvincibleTime(m_charController.ActualInvincibilityDuration, additive: false, playInvincibility: true);
			}
		}
	}

	protected virtual float CharacterDamaged(IDamageObj damageObj, GameObject otherRootObj, out CriticalStrikeType critType, out float damageBlocked, float damageOverride = -1f, bool trueDamage = false)
	{
		GLYPHS_TO_ADD_TO_ENEMY_DAMAGE_HELPER.Clear();
		GLYPHS_TO_ADD_TO_PLAYER_DAMAGE_HELPER.Clear();
		float num = m_charController.CalculateDamageTaken(damageObj, out critType, out damageBlocked, damageOverride, trueDamage, pureCalculation: false);
		m_charController.SetHealth(0f - num, additive: true, runEvents: true);
		PlayHitEffect(damageObj, num <= 0f, critType != CriticalStrikeType.None);
		if (damageObj.StatusEffectTypes != null && damageObj.StatusEffectDurations != null)
		{
			for (int i = 0; i < damageObj.StatusEffectTypes.Length; i++)
			{
				StatusEffectType statusEffectType = damageObj.StatusEffectTypes[i];
				float duration = damageObj.StatusEffectDurations[i];
				switch (statusEffectType)
				{
				case StatusEffectType.Player_Combo:
					PlayerManager.GetPlayerController().StatusEffectController.StartStatusEffect(statusEffectType, duration, damageObj);
					break;
				default:
					m_charController.StatusEffectController.StartStatusEffect(statusEffectType, duration, damageObj);
					break;
				case StatusEffectType.None:
					break;
				}
			}
		}
		return num;
	}

	protected abstract void CharacterKnockedBack(IDamageObj damageObj, GameObject otherRootObj);

	protected virtual void CharacterKilled(IDamageObj damageObj, GameObject otherRootObj)
	{
		m_charController.KillCharacter(otherRootObj, broadcastEvent: true);
	}

	public void SetInvincibleTime(float amount, bool additive, bool playInvincibility)
	{
		InvincibilityEffectPlaying = false;
		if (!(amount <= 0f) || additive)
		{
			if (additive)
			{
				m_invincibilityTimer += amount;
			}
			else
			{
				m_invincibilityTimer = amount;
			}
			if (m_invincibilityTimer < 0f)
			{
				m_invincibilityTimer = 0f;
			}
			if (playInvincibility)
			{
				InvincibilityEffectPlaying = true;
				m_hitEffect.StartInvincibilityEffect();
			}
		}
	}

	public void StopInvincibleTime()
	{
		m_hitEffect.StopInvincibilityEffect();
		m_invincibilityTimer = 0f;
		InvincibilityEffectPlaying = false;
	}

	protected virtual void Update()
	{
		UpdateStunTimer();
		UpdateInvincibilityTimer();
	}

	protected virtual void UpdateStunTimer()
	{
		if (m_landCheckDelayTimer > 0f)
		{
			m_landCheckDelayTimer -= Time.deltaTime;
		}
		if (m_stunTimer > 0f)
		{
			m_stunTimer -= Time.deltaTime;
			if (m_stunTimer <= 0f || (m_cancelStunWhenLanding && m_charController != null && m_charController.IsGrounded && m_landCheckDelayTimer <= 0f))
			{
				StopCharacterStunned();
			}
		}
	}

	protected virtual void UpdateInvincibilityTimer()
	{
		if (m_invincibilityTimer > 0f)
		{
			m_invincibilityTimer -= Time.deltaTime;
			if (m_invincibilityTimer <= 0f)
			{
				StopInvincibleTime();
			}
		}
	}

	public void BodyOnEnterHitResponse(IHitboxController otherHBController)
	{
		HandleHitResponse(otherHBController);
	}

	public void BodyOnStayHitResponse(IHitboxController otherHBController)
	{
		HandleHitResponse(otherHBController);
	}

	public void TerrainOnEnterHitResponse(IHitboxController otherHBController)
	{
		HandleHitResponse(otherHBController);
	}

	public void TerrainOnStayHitResponse(IHitboxController otherHBController)
	{
		HandleHitResponse(otherHBController);
	}

	protected virtual void PlayHitEffect(IDamageObj damageObj, bool noDamageTaken, bool isCrit)
	{
		Vector3 collisionPoint = m_charController.Midpoint;
		if (m_charController.HitboxController.LastCollidedWith != null)
		{
			collisionPoint = m_charController.HitboxController.LastCollidedWith.ClosestPoint(m_charController.Midpoint);
		}
		BaseEffect baseEffect = null;
		if (m_charController.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_Invuln))
		{
			baseEffect = EffectManager.PlayHitEffect(damageObj, collisionPoint, null, StrikeType.Invincible_StatusEffect);
			BurstEffect burstEffect = EffectManager.PlayEffect(m_charController.gameObject, m_charController.Animator, "InvulnShieldBurst_Effect", Vector3.zero, 0f, EffectStopType.Gracefully) as BurstEffect;
			GameObject source = (m_charController.StatusEffectController.GetStatusEffect(StatusEffectType.Enemy_Invuln) as EnemyInvulnStatusEffect).Source;
			if ((bool)source)
			{
				IMidpointObj component = source.GetComponent<IMidpointObj>();
				if (component != null)
				{
					burstEffect.DestinationMidpointOverride = component;
				}
				else
				{
					burstEffect.DestinationOverride = source.transform;
				}
			}
		}
		else
		{
			bool isUnscaled = false;
			if ((m_charController.CurrentHealth <= 0f || AnimateHitEffectsOnUnscaledTime) && m_charController.CompareTag("Player"))
			{
				isUnscaled = true;
			}
			if (noDamageTaken)
			{
				baseEffect = EffectManager.PlayHitEffect(damageObj, collisionPoint, null, StrikeType.NoDamage, isUnscaled);
			}
			else if (isCrit)
			{
				baseEffect = EffectManager.PlayHitEffect(damageObj, collisionPoint, null, StrikeType.Critical, isUnscaled);
			}
			else
			{
				if (damageObj.StrikeType == StrikeType.OnHitAreaRelic)
				{
					collisionPoint = m_charController.Midpoint;
				}
				baseEffect = EffectManager.PlayHitEffect(damageObj, collisionPoint, null, damageObj.StrikeType, isUnscaled);
			}
		}
		if ((m_charController.CurrentHealth <= 0f || AnimateHitEffectsOnUnscaledTime) && m_charController.CompareTag("Player"))
		{
			CameraController.SoloCam.AddToCameraLayer(baseEffect.gameObject);
		}
	}

	protected void PlayDirectionalHitEffect(IDamageObj damageObj, GameObject otherRootObj)
	{
		Vector3 collisionPoint = m_charController.Midpoint;
		if (m_charController.HitboxController.LastCollidedWith != null)
		{
			collisionPoint = m_charController.HitboxController.LastCollidedWith.ClosestPoint(m_charController.Midpoint);
		}
		EffectManager.PlayDirectionalHitEffect(damageObj, otherRootObj, collisionPoint);
	}
}
