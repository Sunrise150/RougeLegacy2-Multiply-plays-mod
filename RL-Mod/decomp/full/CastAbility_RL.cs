using System;
using System.Collections;
using MoreMountains.CorgiEngine;
using Rewired;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterClass))]
public class CastAbility_RL : CharacterAbility
{
	[Header("Ability Prefabs")]
	[SerializeField]
	[ReadOnly]
	protected BaseAbility_RL m_weaponAbility;

	[SerializeField]
	[ReadOnly]
	protected BaseAbility_RL m_spellAbility;

	[SerializeField]
	[ReadOnly]
	protected BaseAbility_RL m_talentAbility;

	[SerializeField]
	private UnityEvent m_notEnoughManaUnityEvent;

	[SerializeField]
	private UnityEvent m_inCooldownUnityEvent;

	protected bool m_weaponAbilityInProgress;

	protected bool m_spellAbilityInProgress;

	protected bool m_talentAbilityInProgress;

	private bool m_isWeaponAbilityOverridden;

	private BaseAbility_RL m_weaponAbilityOverride;

	protected Coroutine m_weaponCoroutine;

	protected Coroutine m_spellCoroutine;

	protected Coroutine m_talentCoroutine;

	private Coroutine m_weaponQueueCoroutine;

	private Coroutine m_spellQueueCoroutine;

	private Coroutine m_talentQueueCoroutine;

	private AbilityUsedEventArgs m_abilityUsedEventArgs;

	private ChangeAbilityEventArgs m_changeAbilityArgs;

	private Projectile_RL m_damageProjectileHelper;

	private Action<MonoBehaviour, EventArgs> m_onGamePaused;

	private float m_textPopupDelay;

	private CastAbilityType m_lastCastAbilityPermittedCheck;

	private const string EMPTY_STATE_NAME = "Empty";

	private bool m_performingStuckCheck;

	private float m_stuckCheckDelayTimer;

	public bool OnPauseResetInput { get; private set; }

	public AbilityType WeaponAbilityType { get; private set; }

	public AbilityType SpellAbilityType { get; private set; }

	public AbilityType TalentAbilityType { get; private set; }

	public float HorizontalInput => _horizontalInput;

	public bool IsAiming
	{
		get
		{
			bool num = (bool)m_weaponAbility && m_weaponAbility.IsAiming;
			bool flag = (bool)m_spellAbility && m_spellAbility.IsAiming;
			bool flag2 = (bool)m_talentAbility && m_talentAbility.IsAiming;
			return num || flag || flag2;
		}
	}

	public CastAbilityType LastCastAbilityTypeCasted { get; private set; }

	public PlayerController PlayerController => m_playerController;

	public bool ActiveAbilitiesAreJumpBreakable
	{
		get
		{
			BaseAbility_RL ability = GetAbility(CastAbilityType.Weapon);
			if ((bool)ability && AbilityInProgress(CastAbilityType.Weapon) && !ability.JumpInterruptable)
			{
				return false;
			}
			BaseAbility_RL ability2 = GetAbility(CastAbilityType.Spell);
			if ((bool)ability2 && AbilityInProgress(CastAbilityType.Spell) && !ability2.JumpInterruptable)
			{
				return false;
			}
			BaseAbility_RL ability3 = GetAbility(CastAbilityType.Talent);
			if ((bool)ability3 && AbilityInProgress(CastAbilityType.Talent) && !ability3.JumpInterruptable)
			{
				return false;
			}
			return true;
		}
	}

	public bool AnyAbilityInProgress
	{
		get
		{
			if (!AbilityInProgress(CastAbilityType.Weapon) && !AbilityInProgress(CastAbilityType.Spell))
			{
				return AbilityInProgress(CastAbilityType.Talent);
			}
			return true;
		}
	}

	public Animator Animator => _animator;

	public bool IsInitialized => base.AbilityInitialized;

	public string GetAbilityInputString(CastAbilityType castAbilityType)
	{
		return castAbilityType switch
		{
			CastAbilityType.Spell => "Spell", 
			CastAbilityType.Talent => "Talent", 
			_ => "Attack", 
		};
	}

	public void SetLastCastAbilityTypeOverride(CastAbilityType castAbilityTypeOverride)
	{
		LastCastAbilityTypeCasted = castAbilityTypeOverride;
	}

	public bool AbilityInProgress(CastAbilityType castAbilityType)
	{
		return castAbilityType switch
		{
			CastAbilityType.Weapon => m_weaponAbilityInProgress, 
			CastAbilityType.Spell => m_spellAbilityInProgress, 
			CastAbilityType.Talent => m_talentAbilityInProgress, 
			_ => false, 
		};
	}

	public BaseAbility_RL GetAbility(CastAbilityType abilityType, bool ignoreOverrides = false)
	{
		switch (abilityType)
		{
		case CastAbilityType.Weapon:
			if (m_isWeaponAbilityOverridden && !ignoreOverrides)
			{
				return m_weaponAbilityOverride;
			}
			return m_weaponAbility;
		case CastAbilityType.Spell:
			return m_spellAbility;
		case CastAbilityType.Talent:
			return m_talentAbility;
		default:
			Debug.LogFormat("<color=red>{0}: No Ability found matching Ability Type ({1})</color>", Time.frameCount, abilityType);
			return null;
		}
	}

	public void SetAbility(CastAbilityType castAbilityType, AbilityType abilityType, bool destroyOldAbility = true)
	{
		BaseAbility_RL abilityInstance = CreateAbilityInstance(castAbilityType, abilityType);
		SetAbility(castAbilityType, abilityInstance, destroyOldAbility);
	}

	public BaseAbility_RL CreateAbilityInstance(CastAbilityType castAbilityType, AbilityType abilityType)
	{
		BaseAbility_RL ability = AbilityLibrary.GetAbility(abilityType);
		BaseAbility_RL baseAbility_RL = null;
		if ((bool)ability)
		{
			baseAbility_RL = UnityEngine.Object.Instantiate(ability);
			baseAbility_RL.Initialize(this, castAbilityType);
			baseAbility_RL.transform.position = Vector3.zero;
			baseAbility_RL.transform.SetParent(base.transform, worldPositionStays: false);
		}
		return baseAbility_RL;
	}

	public void SetAbility(CastAbilityType castAbilityType, BaseAbility_RL abilityInstance, bool destroyOldAbility = true)
	{
		switch (castAbilityType)
		{
		case CastAbilityType.Weapon:
			if ((bool)m_weaponAbility && destroyOldAbility)
			{
				BaseEffect[] componentsInChildren = m_weaponAbility.GetComponentsInChildren<BaseEffect>(includeInactive: true);
				foreach (BaseEffect obj2 in componentsInChildren)
				{
					obj2.transform.SetParent(null);
					obj2.gameObject.SetActive(value: false);
				}
				m_weaponAbility.OnPreDestroy();
				UnityEngine.Object.Destroy(m_weaponAbility.gameObject);
			}
			m_weaponAbility = abilityInstance;
			if ((bool)m_weaponAbility)
			{
				WeaponAbilityType = abilityInstance.AbilityType;
			}
			else
			{
				WeaponAbilityType = AbilityType.None;
			}
			break;
		case CastAbilityType.Spell:
			if ((bool)m_spellAbility && destroyOldAbility)
			{
				BaseEffect[] componentsInChildren = m_spellAbility.GetComponentsInChildren<BaseEffect>(includeInactive: true);
				foreach (BaseEffect obj3 in componentsInChildren)
				{
					obj3.transform.SetParent(null);
					obj3.gameObject.SetActive(value: false);
				}
				m_spellAbility.OnPreDestroy();
				UnityEngine.Object.Destroy(m_spellAbility.gameObject);
			}
			m_spellAbility = abilityInstance;
			if ((bool)m_spellAbility)
			{
				SpellAbilityType = abilityInstance.AbilityType;
			}
			else
			{
				SpellAbilityType = AbilityType.None;
			}
			break;
		case CastAbilityType.Talent:
			if ((bool)m_talentAbility && destroyOldAbility)
			{
				BaseEffect[] componentsInChildren = m_talentAbility.GetComponentsInChildren<BaseEffect>(includeInactive: true);
				foreach (BaseEffect obj in componentsInChildren)
				{
					obj.transform.SetParent(null);
					obj.gameObject.SetActive(value: false);
				}
				m_talentAbility.OnPreDestroy();
				UnityEngine.Object.Destroy(m_talentAbility.gameObject);
			}
			m_talentAbility = abilityInstance;
			if ((bool)m_talentAbility)
			{
				TalentAbilityType = abilityInstance.AbilityType;
			}
			else
			{
				TalentAbilityType = AbilityType.None;
			}
			break;
		}
		m_changeAbilityArgs.Initialise(castAbilityType, abilityInstance);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.ChangeAbility, this, m_changeAbilityArgs);
	}

	public void ReinitializeAbility(CastAbilityType castAbilityType)
	{
		switch (castAbilityType)
		{
		case CastAbilityType.Weapon:
			if ((bool)m_weaponAbility)
			{
				m_weaponAbility.Reinitialize();
			}
			break;
		case CastAbilityType.Spell:
			if ((bool)m_spellAbility)
			{
				m_spellAbility.Reinitialize();
			}
			break;
		case CastAbilityType.Talent:
			if ((bool)m_talentAbility)
			{
				m_talentAbility.Reinitialize();
			}
			break;
		}
	}

	private void Awake()
	{
		m_changeAbilityArgs = new ChangeAbilityEventArgs(CastAbilityType.Weapon, null);
		m_abilityUsedEventArgs = new AbilityUsedEventArgs(null);
		m_damageProjectileHelper = UnityEngine.Object.Instantiate(ProjectileLibrary.GetProjectile("SwordWeaponProjectile"), base.transform);
		m_damageProjectileHelper.name = "Damage Projectile Helper";
		m_damageProjectileHelper.gameObject.SetActive(value: false);
		ProjectileManager.ActiveProjectileCount++;
		m_onGamePaused = OnGamePaused;
	}

	protected override void HandleInput()
	{
		if (m_playerController.ConditionState != CharacterStates.CharacterConditions.ControlledMovement)
		{
			if ((bool)m_weaponAbility && _character.REPlayer.GetButtonDown(GetAbilityInputString(CastAbilityType.Weapon)))
			{
				StartAbility(CastAbilityType.Weapon, castAbilityOverride: false, allowQueuing: true);
			}
			if ((bool)m_spellAbility && _character.REPlayer.GetButtonDown(GetAbilityInputString(CastAbilityType.Spell)))
			{
				StartAbility(CastAbilityType.Spell, castAbilityOverride: false, allowQueuing: true);
			}
			if ((bool)m_talentAbility && _character.REPlayer.GetButtonDown(GetAbilityInputString(CastAbilityType.Talent)))
			{
				StartAbility(CastAbilityType.Talent, castAbilityOverride: false, allowQueuing: true);
			}
		}
	}

	public bool IsAbilityPermitted(CastAbilityType abilityType)
	{
		BaseAbility_RL ability = GetAbility(abilityType);
		return IsAbilityPermitted(ability);
	}

	public bool IsAbilityPermitted(BaseAbility_RL ability)
	{
		if (!AbilityPermitted)
		{
			return false;
		}
		Vector2 vector = m_playerController.Midpoint;
		vector.y += m_playerController.CollisionBounds.height / 2f;
		bool flag = false;
		if (Time.time >= m_textPopupDelay || m_lastCastAbilityPermittedCheck != ability.CastAbilityType)
		{
			flag = true;
			m_textPopupDelay = Time.time + 1f;
			m_lastCastAbilityPermittedCheck = ability.CastAbilityType;
		}
		if (m_playerController.StatusEffectController.HasStatusEffect(StatusEffectType.Player_Disarmed))
		{
			if (m_inCooldownUnityEvent != null)
			{
				m_inCooldownUnityEvent.Invoke();
			}
			if (flag)
			{
				TextPopupManager.DisplayTextAtAbsPos(TextPopupType.OutOfAmmo, LocalizationManager.GetString("LOC_ID_STATUS_EFFECT_ABILITY_DISABLED_1", isFemale: false), vector);
			}
			return false;
		}
		if (m_playerController.CharacterDownStrike.AttackButtonTriggersSpinKick && !_controller.State.IsGrounded && m_playerController.CharacterDownStrike.IsHoldingDownStrikeAngle)
		{
			return false;
		}
		if ((m_playerController.ConditionState != 0 && m_playerController.ConditionState != CharacterStates.CharacterConditions.DisableHorizontalMovement) || m_playerController.MovementState == CharacterStates.MovementStates.DownStriking)
		{
			return false;
		}
		if (!ability)
		{
			return false;
		}
		if (TraitManager.IsTraitActive(TraitType.CantAttack) && !ability.DealsNoDamage)
		{
			return false;
		}
		if (m_playerController.MovementState == CharacterStates.MovementStates.Dashing && !ability.CanCastWhileDashing)
		{
			return false;
		}
		if (ability.LockoutTimer > 0f)
		{
			return false;
		}
		BaseAbility_RL ability2 = GetAbility(CastAbilityType.Spell);
		BaseAbility_RL ability3 = GetAbility(CastAbilityType.Weapon);
		BaseAbility_RL ability4 = GetAbility(CastAbilityType.Talent);
		switch (ability.EVAssignedCastAbilityType)
		{
		case CastAbilityType.Weapon:
			if (m_weaponAbilityInProgress && (bool)ability3 && !ability3.WeaponInterruptable)
			{
				return false;
			}
			if (m_spellAbilityInProgress && (bool)ability2 && !ability2.WeaponInterruptable)
			{
				return false;
			}
			if (m_talentAbilityInProgress && (bool)ability4 && !ability4.WeaponInterruptable)
			{
				return false;
			}
			break;
		case CastAbilityType.Spell:
			if (m_weaponAbilityInProgress && (bool)ability3 && !ability3.SpellInterruptable)
			{
				return false;
			}
			if (m_spellAbilityInProgress && (bool)ability2 && !ability2.SpellInterruptable)
			{
				return false;
			}
			if (m_talentAbilityInProgress && (bool)ability4 && !ability4.SpellInterruptable)
			{
				return false;
			}
			break;
		case CastAbilityType.Talent:
			if (m_weaponAbilityInProgress && (bool)ability3 && !ability3.TalentInterruptable)
			{
				return false;
			}
			if (m_spellAbilityInProgress && (bool)ability2 && !ability2.TalentInterruptable)
			{
				return false;
			}
			if (m_talentAbilityInProgress && (bool)ability4 && !ability4.TalentInterruptable)
			{
				return false;
			}
			break;
		}
		if (ability.CooldownTimer > 0f && ability.MaxAmmo <= 0)
		{
			if (m_inCooldownUnityEvent != null)
			{
				m_inCooldownUnityEvent.Invoke();
			}
			if (ability.AbilityType == AbilityType.CrowsNestTalent && (ability as CrowsNest_Ability).CrowsNestActive)
			{
				flag = false;
			}
			if (flag)
			{
				TextPopupManager.DisplayLocIDText(TextPopupType.SpellOnCooldown, "LOC_ID_STATUS_EFFECT_NO_COOLDOWN_1", StringGenderType.UsePlayerData, vector, 0.5f);
			}
			return false;
		}
		if (ability.ActualCost > m_playerController.CurrentManaAsInt && !(ability is IPersistentAbility { IsPersistentActive: not false }))
		{
			bool flag2 = false;
			RelicObj relic = SaveManager.PlayerSaveData.GetRelic(RelicType.FreeCastSpell);
			if (relic.Level > 0)
			{
				int relicMaxStack = Relic_EV.GetRelicMaxStack(relic.RelicType, relic.Level);
				if (relic.IntValue >= relicMaxStack)
				{
					flag2 = true;
				}
			}
			if (!flag2)
			{
				if (m_notEnoughManaUnityEvent != null)
				{
					m_notEnoughManaUnityEvent.Invoke();
				}
				if (flag)
				{
					TextPopupManager.DisplayLocIDText(TextPopupType.NoMana, "LOC_ID_STATUS_EFFECT_NO_MANA_1", StringGenderType.UsePlayerData, vector, 0.5f);
				}
				return false;
			}
		}
		if (ability.MaxAmmo > 0 && ability.CurrentAmmo <= 0 && !(ability is IPersistentAbility { IsPersistentActive: not false }))
		{
			if (m_inCooldownUnityEvent != null)
			{
				m_inCooldownUnityEvent.Invoke();
			}
			if (flag)
			{
				TextPopupManager.DisplayLocIDText(TextPopupType.OutOfAmmo, "LOC_ID_STATUS_EFFECT_NO_AMMO_KICK_1", StringGenderType.UsePlayerData, vector, 0.5f);
			}
			return false;
		}
		return true;
	}

	public void StartQueueCoroutine(CastAbilityType castAbilityType)
	{
		StopQueueCoroutine(castAbilityType);
		switch (castAbilityType)
		{
		case CastAbilityType.Weapon:
			m_weaponQueueCoroutine = StartCoroutine(AbilityQueueCoroutine(castAbilityType));
			break;
		case CastAbilityType.Spell:
			m_spellQueueCoroutine = StartCoroutine(AbilityQueueCoroutine(castAbilityType));
			break;
		case CastAbilityType.Talent:
			m_talentQueueCoroutine = StartCoroutine(AbilityQueueCoroutine(castAbilityType));
			break;
		}
	}

	private IEnumerator AbilityQueueCoroutine(CastAbilityType castAbilityType)
	{
		float duration = Time.time + 0.175f;
		while (Time.time < duration)
		{
			if (IsAbilityPermitted(castAbilityType))
			{
				StartAbility(castAbilityType, castAbilityOverride: false, allowQueuing: true);
				break;
			}
			yield return null;
		}
	}

	public void StopQueueCoroutine(CastAbilityType castAbilityType)
	{
		switch (castAbilityType)
		{
		case CastAbilityType.Weapon:
			if (m_weaponQueueCoroutine != null)
			{
				StopCoroutine(m_weaponQueueCoroutine);
			}
			break;
		case CastAbilityType.Spell:
			if (m_spellQueueCoroutine != null)
			{
				StopCoroutine(m_spellQueueCoroutine);
			}
			break;
		case CastAbilityType.Talent:
			if (m_talentQueueCoroutine != null)
			{
				StopCoroutine(m_talentQueueCoroutine);
			}
			break;
		}
	}

	public void StopAllQueueCoroutines()
	{
		StopQueueCoroutine(CastAbilityType.Weapon);
		StopQueueCoroutine(CastAbilityType.Spell);
		StopQueueCoroutine(CastAbilityType.Talent);
	}

	private void FixedUpdate()
	{
		PerformAbilityStuckCheck();
	}

	private void PerformAbilityStuckCheck()
	{
		if (m_performingStuckCheck)
		{
			if (!(Time.time > m_stuckCheckDelayTimer))
			{
				return;
			}
			if (Animator.GetCurrentAnimatorStateInfo(1).IsName("Empty"))
			{
				if (m_weaponAbilityInProgress && !m_weaponAbility.IgnoreStuckCheck)
				{
					StopAbility(CastAbilityType.Weapon, abilityInterrrupted: false);
				}
				if (m_spellAbilityInProgress && !m_spellAbility.IgnoreStuckCheck)
				{
					StopAbility(CastAbilityType.Spell, abilityInterrrupted: false);
				}
				if (m_talentAbilityInProgress && !m_talentAbility.IgnoreStuckCheck)
				{
					StopAbility(CastAbilityType.Talent, abilityInterrrupted: false);
				}
			}
			m_performingStuckCheck = false;
		}
		else if (Animator.GetCurrentAnimatorStateInfo(1).IsName("Empty") && ((m_weaponAbilityInProgress && (bool)m_weaponAbility && !m_weaponAbility.IgnoreStuckCheck) || (m_spellAbilityInProgress && (bool)m_spellAbility && !m_spellAbility.IgnoreStuckCheck) || (m_talentAbilityInProgress && (bool)m_talentAbility && !m_talentAbility.IgnoreStuckCheck)))
		{
			m_stuckCheckDelayTimer = Time.time + 0.05f;
			m_performingStuckCheck = true;
		}
	}

	public void StartAbility(CastAbilityType abilityType, bool castAbilityOverride = false, bool allowQueuing = false)
	{
		if (!m_isWeaponAbilityOverridden && abilityType == CastAbilityType.Weapon && (bool)m_weaponAbility && m_weaponAbility.AbilityType == AbilityType.PistolWeapon && (m_weaponAbility.CurrentAmmo <= 0 || castAbilityOverride))
		{
			m_weaponAbilityOverride = (m_weaponAbility as PistolWeapon_Ability).ReloadAbility;
			m_isWeaponAbilityOverridden = true;
			if (!IsAbilityPermitted(abilityType))
			{
				m_weaponAbilityOverride = null;
				m_isWeaponAbilityOverridden = false;
			}
		}
		bool isWeaponAbilityOverridden = m_isWeaponAbilityOverridden;
		BaseAbility_RL ability = GetAbility(abilityType);
		if (!IsAbilityPermitted(abilityType))
		{
			if (allowQueuing && (bool)ability && !ability.DisableAbilityQueuing)
			{
				StartQueueCoroutine(abilityType);
			}
			return;
		}
		LastCastAbilityTypeCasted = abilityType;
		if (ability.HasAnimation)
		{
			StopAllAbilities(stopPersistentAbilities: false);
		}
		m_isWeaponAbilityOverridden = isWeaponAbilityOverridden;
		switch (abilityType)
		{
		case CastAbilityType.Weapon:
			m_weaponCoroutine = StartCoroutine(CastAbility(abilityType));
			break;
		case CastAbilityType.Spell:
			m_spellCoroutine = StartCoroutine(CastAbility(abilityType));
			break;
		case CastAbilityType.Talent:
			m_talentCoroutine = StartCoroutine(CastAbility(abilityType));
			break;
		}
	}

	protected IEnumerator CastAbility(CastAbilityType abilityType)
	{
		BaseAbility_RL ability = GetAbility(abilityType);
		switch (abilityType)
		{
		case CastAbilityType.Weapon:
			m_weaponAbilityInProgress = true;
			break;
		case CastAbilityType.Spell:
			m_spellAbilityInProgress = true;
			break;
		case CastAbilityType.Talent:
			m_talentAbilityInProgress = true;
			if (m_playerController.CharacterClass.ClassType == ClassType.SaberClass)
			{
				m_playerController.StatusEffectController.StartStatusEffect(StatusEffectType.Player_FreeCrit, 1f, null);
			}
			break;
		}
		OnPauseResetInput = false;
		if (SaveManager.ConfigData.ToggleMouseAttackFlip)
		{
			ControllerType lastActiveControllerType = ReInput.controllers.GetLastActiveControllerType();
			if (lastActiveControllerType == ControllerType.Mouse || lastActiveControllerType == ControllerType.Keyboard)
			{
				Vector2 vector = m_playerController.Midpoint;
				Vector2 screenPosition = ReInput.controllers.Mouse.screenPosition;
				Vector2 vector2 = CameraController.GameCamera.ScreenToWorldPoint(screenPosition);
				if ((vector2.x < vector.x && m_playerController.IsFacingRight) || (vector2.x > vector.x && !m_playerController.IsFacingRight))
				{
					m_playerController.CharacterCorgi.Flip();
				}
			}
		}
		ability.PreCastAbility();
		yield return ability.CastAbility();
		StopAbility(abilityType, abilityInterrrupted: false);
	}

	public void BroadcastAbilityCastEvents(CastAbilityType abilityType)
	{
		switch (abilityType)
		{
		case CastAbilityType.Weapon:
			if (m_isWeaponAbilityOverridden && (bool)m_weaponAbilityOverride)
			{
				m_abilityUsedEventArgs.Initialise(m_weaponAbilityOverride);
			}
			else
			{
				m_abilityUsedEventArgs.Initialise(m_weaponAbility);
			}
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerWeaponAbilityCast, this, m_abilityUsedEventArgs);
			break;
		case CastAbilityType.Spell:
			m_abilityUsedEventArgs.Initialise(m_spellAbility);
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerSpellAbilityCast, this, m_abilityUsedEventArgs);
			break;
		case CastAbilityType.Talent:
			m_abilityUsedEventArgs.Initialise(m_talentAbility);
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerTalentAbilityCast, this, m_abilityUsedEventArgs);
			break;
		}
	}

	public void StopAbility(CastAbilityType abilityType, bool abilityInterrrupted)
	{
		BaseAbility_RL ability = GetAbility(abilityType);
		switch (abilityType)
		{
		case CastAbilityType.Weapon:
			if (m_weaponCoroutine != null)
			{
				StopCoroutine(m_weaponCoroutine);
			}
			m_weaponCoroutine = null;
			m_weaponAbilityInProgress = false;
			break;
		case CastAbilityType.Spell:
			if (m_spellCoroutine != null)
			{
				StopCoroutine(m_spellCoroutine);
			}
			m_spellCoroutine = null;
			m_spellAbilityInProgress = false;
			break;
		case CastAbilityType.Talent:
			if (m_talentCoroutine != null)
			{
				StopCoroutine(m_talentCoroutine);
			}
			m_talentCoroutine = null;
			m_talentAbilityInProgress = false;
			break;
		}
		if ((bool)ability && ability.AbilityActive)
		{
			ability.StopAbility(abilityInterrrupted);
		}
		if (abilityType == CastAbilityType.Weapon)
		{
			m_isWeaponAbilityOverridden = false;
		}
	}

	public void StopPersistentAbility(CastAbilityType abilityType)
	{
		BaseAbility_RL ability = GetAbility(abilityType);
		if ((bool)ability && ability is IPersistentAbility { IsPersistentActive: not false } persistentAbility)
		{
			persistentAbility.StopPersistentAbility();
		}
	}

	public void StopAllAbilities(bool stopPersistentAbilities)
	{
		if (IsInitialized)
		{
			StopAllCoroutines();
		}
		StopAbility(CastAbilityType.Weapon, abilityInterrrupted: true);
		StopAbility(CastAbilityType.Spell, abilityInterrrupted: true);
		StopAbility(CastAbilityType.Talent, abilityInterrrupted: true);
		StopAllQueueCoroutines();
		if (stopPersistentAbilities)
		{
			StopPersistentAbility(CastAbilityType.Weapon);
			StopPersistentAbility(CastAbilityType.Spell);
			StopPersistentAbility(CastAbilityType.Talent);
		}
	}

	public void ResetAbilityCooldowns(CastAbilityType abilityType, bool ignoreAbilityOverride = false)
	{
		BaseAbility_RL ability = GetAbility(abilityType, ignoreAbilityOverride);
		if ((bool)ability)
		{
			ability.EndCooldownTimer(displayCooldownResetText: false);
		}
	}

	public void ResetAbilityAmmo(CastAbilityType abilityType, bool ignoreAbilityOverride = false)
	{
		BaseAbility_RL ability = GetAbility(abilityType, ignoreAbilityOverride);
		if ((bool)ability)
		{
			for (int i = 0; i < ability.MaxAmmo; i++)
			{
				ability.RegenerateAmmo();
			}
		}
	}

	public void SetAbilityAmmo(CastAbilityType abilityType, int amount, bool ignoreAbilityOverride = false)
	{
		BaseAbility_RL ability = GetAbility(abilityType, ignoreAbilityOverride);
		if ((bool)ability)
		{
			if (ability.AbilityType == AbilityType.KineticBowWeapon && amount <= 0)
			{
				amount = 1;
			}
			ability.CurrentAmmo = amount;
		}
	}

	public void InitializeProjectile(Projectile_RL projectile)
	{
		projectile.CastAbilityType = LastCastAbilityTypeCasted;
		if ((bool)m_spellAbility)
		{
			m_spellAbility.InitializeProjectile(projectile);
		}
		if ((bool)m_weaponAbility)
		{
			m_weaponAbility.InitializeProjectile(projectile);
		}
		if ((bool)m_talentAbility)
		{
			m_talentAbility.InitializeProjectile(projectile);
		}
		m_playerController.CloakInterrupted = false;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.GamePauseStateChange, m_onGamePaused);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.GamePauseStateChange, m_onGamePaused);
	}

	private void OnGamePaused(object sender, EventArgs args)
	{
		OnPauseResetInput = true;
	}
}
