using System;
using System.Collections;
using MoreMountains.CorgiEngine;
using RLAudio;
using Rewired;
using Sigtrap.Relays;
using UnityEngine;

public abstract class BaseAbility_RL : MonoBehaviour, ICooldown, IAbility, IHasProjectileNameArray, IAudioEventEmitter
{
	protected const string CHANGE_ABILITY_ANIM_PARAM = "Change_Ability_Anim";

	[SerializeField]
	private AbilityType m_abilityType = AbilityType.FireballSpell;

	[Header("Ability Data")]
	[SerializeField]
	protected AbilityData m_abilityData;

	[SerializeField]
	[Tooltip("This field does not affect the actual damage of the ability. It's just used to know if it can be cast with the Pacifist trait active.")]
	protected bool m_dealsNoDamage;

	[SerializeField]
	protected string m_abilityTellIntroName;

	[SerializeField]
	protected Vector2 m_critHitTimingWindow;

	[SerializeField]
	protected Vector2 m_critEffectOffset;

	[SerializeField]
	protected bool m_critsWhenDashing;

	[SerializeField]
	protected bool m_disableAbilityQueuing;

	[Header("Projectile Data")]
	[SerializeField]
	protected string m_projectileName;

	[SerializeField]
	protected Vector2 m_projectileOffset;

	[SerializeField]
	protected bool m_hasAttackFlipCheck;

	protected bool m_consumedSpellOrb;

	protected CastAbility_RL m_abilityController;

	protected float m_cooldownTimer;

	protected float m_lockoutTimer;

	protected int m_currentAmmo;

	protected Projectile_RL m_firedProjectile;

	protected bool m_hasAnimation;

	protected Animator m_animator;

	private CooldownEventArgs m_cooldownEventArgs;

	private AbilityCooldownOverEventArgs m_cooldownOverEventArgs;

	private Coroutine m_attackFlipCheckCoroutine;

	private float m_attackFlipCheckTimer;

	private bool m_enemyHit;

	private bool m_isAnimationComplete;

	protected Coroutine m_changeAnimCoroutine;

	private PlayerAmmoChangeEventArgs m_ammoChangeEventArgs;

	private RaycastHit2D[] m_attackFlipRaycastArray;

	protected float m_highestBounceAmount;

	protected bool m_triggerBounce;

	private string m_abilityNameCasted;

	protected float m_abilityCastStartTime;

	private Projectile_RL m_persistentCollListenerProj;

	protected GameObject m_critEffectGO;

	private Action<Projectile_RL, GameObject> m_reduceCooldownEvent;

	private Action<Projectile_RL, GameObject> m_increaseManaEvent;

	private Action<Projectile_RL, GameObject> m_onProjectileHitDisableAttackFlip;

	protected Action<Projectile_RL, GameObject> m_bounce;

	private Action<AbilityAnimState> m_exitAnimationState;

	private Action<AbilityAnimState> m_enterAnimationState;

	private Action<object, EventArgs> m_onEnemyHitCancelAttackFlip;

	protected Relay m_beginCastingRelay = new Relay();

	protected Relay m_stopCastingRelay = new Relay();

	private Relay<object, CooldownEventArgs> m_onCooldownRelay = new Relay<object, CooldownEventArgs>();

	[NonSerialized]
	protected string[] m_projectileNameArray;

	private bool m_critDisplayOn;

	public virtual bool IgnoreStuckCheck => false;

	public IRelayLink BeginCastingRelay => m_beginCastingRelay.link;

	public IRelayLink StopCastingRelay => m_stopCastingRelay.link;

	public bool DisableAbilityQueuing => m_disableAbilityQueuing;

	public bool CritsWhenDashing => m_critsWhenDashing;

	public bool ForceTriggerCrit { get; set; }

	public IRelayLink<object, CooldownEventArgs> OnBeginCooldownRelay => m_onCooldownRelay.link;

	public CooldownRegenType CooldownRegenType { get; protected set; }

	public virtual bool IsAiming => false;

	public string Description => ToString();

	public virtual string[] ProjectileNameArray
	{
		get
		{
			if (m_projectileNameArray == null)
			{
				InitializeProjectileNameArray();
			}
			return m_projectileNameArray;
		}
	}

	protected virtual bool IsInCritWindow
	{
		get
		{
			if (m_critHitTimingWindow.x == 0f && m_critHitTimingWindow.y == 0f)
			{
				return false;
			}
			if (Time.time >= m_abilityCastStartTime + m_critHitTimingWindow.x)
			{
				return Time.time <= m_abilityCastStartTime + m_critHitTimingWindow.y;
			}
			return false;
		}
	}

	public bool DealsNoDamage => m_dealsNoDamage;

	public AbilityType AbilityType => m_abilityType;

	protected virtual float AttackBounceHeight => 0f;

	protected virtual float TellIntroAnimSpeed => 1f;

	protected virtual float TellAnimSpeed => 1f;

	protected virtual float AttackIntroAnimSpeed => 1f;

	protected virtual float AttackAnimSpeed => 1f;

	protected virtual float ExitAnimSpeed => 1f;

	protected virtual float TellIntroAnimExitDelay => 0f;

	protected virtual float TellAnimExitDelay => 0f;

	protected virtual float AttackIntroAnimExitDelay => 0f;

	protected virtual float AttackAnimExitDelay => 0f;

	protected virtual float ExitAnimExitDelay => 0f;

	public virtual Vector2 ProjectileOffset => m_projectileOffset;

	public virtual bool HasAttackFlipCheck => m_hasAttackFlipCheck;

	public virtual string ProjectileName => m_projectileName;

	protected virtual int CurrentAnimationLayer => 1;

	public bool HasAnimation => m_hasAnimation;

	public virtual string AbilityTellIntroName => m_abilityTellIntroName;

	public bool IsAnimationComplete
	{
		get
		{
			return m_isAnimationComplete;
		}
		protected set
		{
			m_isAnimationComplete = value;
		}
	}

	public bool AbilityActive { get; protected set; }

	public CastAbilityType EVAssignedCastAbilityType => AbilityData.Type;

	public CastAbilityType CastAbilityType { get; private set; }

	public int BaseCost => AbilityData.BaseCost;

	public int ActualCost
	{
		get
		{
			int baseCost = BaseCost;
			if (TraitManager.IsTraitActive(TraitType.ManaCostAndDamageUp))
			{
				return (int)((float)baseCost * 2f);
			}
			return baseCost;
		}
	}

	public int MaxAmmo => AbilityData.MaxAmmo;

	public float BaseCooldownTime => AbilityData.CooldownTime;

	public float ActualCooldownTime
	{
		get
		{
			float abilityCoolDownMod = m_abilityController.PlayerController.AbilityCoolDownMod;
			float num = 0f;
			if (CastAbilityType == CastAbilityType.Weapon)
			{
				if (AbilityType != AbilityType.PistolWeapon && AbilityType != AbilityType.ReloadTalent)
				{
					int level = SaveManager.PlayerSaveData.GetRelic(RelicType.AttackCooldown).Level;
					num += 2f * (float)level;
				}
				if (AbilityType == AbilityType.LanceWeapon && (this as Lance_Ability).DashAttacking)
				{
					num += 0f;
				}
			}
			float num2 = (BaseCooldownTime + num) * (1f - abilityCoolDownMod);
			if (TraitManager.IsTraitActive(TraitType.LongerCD) && AbilityType != AbilityType.ReloadTalent && (CastAbilityType == CastAbilityType.Spell || CastAbilityType == CastAbilityType.Talent))
			{
				return (float)Mathf.RoundToInt(num2 * 1.25f) + 5f;
			}
			if (TraitManager.IsTraitActive(TraitType.ManaCostAndDamageUp) && CastAbilityType == CastAbilityType.Spell)
			{
				return num2 * 1f;
			}
			return num2;
		}
	}

	public bool DisplayPausedAbilityCooldown { get; set; }

	public float LockoutTime => AbilityData.LockoutTime;

	public bool DecreaseCooldownWhenHit
	{
		get
		{
			return AbilityData.CooldownDecreasePerHit;
		}
		set
		{
			AbilityData.CooldownDecreasePerHit = value;
		}
	}

	public bool DecreaseCooldownOverTime
	{
		get
		{
			return AbilityData.CooldownDecreaseOverTime;
		}
		set
		{
			AbilityData.CooldownDecreaseOverTime = value;
		}
	}

	public bool GainManaWhenHit => AbilityData.ManaGainPerHit;

	public bool CooldownRefreshesAllAmmo => AbilityData.CooldownRefreshesAllAmmo;

	public virtual bool LockDirectionWhenCasting
	{
		get
		{
			if (TraitManager.IsTraitActive(TraitType.DisableAttackLock))
			{
				return false;
			}
			return AbilityData.LockDirection;
		}
	}

	public virtual float AirMovementMod => AbilityData.AirMovementMod;

	public virtual float MovementMod => AbilityData.MovementMod;

	public bool CanCastWhileDashing => AbilityData.CanCastWhileDashing;

	public bool WeaponInterruptable => AbilityData.AttackCancel;

	public bool SpellInterruptable => AbilityData.SpellCancel;

	public bool TalentInterruptable => AbilityData.TalentCancel;

	public bool DashInterruptable => AbilityData.DashCancel;

	public virtual bool JumpInterruptable => AbilityData.JumpCancel;

	public float CooldownTimer => m_cooldownTimer;

	public float LockoutTimer
	{
		get
		{
			return m_lockoutTimer;
		}
		set
		{
			m_lockoutTimer = value;
		}
	}

	public int CurrentAmmo
	{
		get
		{
			return m_currentAmmo;
		}
		set
		{
			float num = m_currentAmmo;
			m_currentAmmo = value;
			m_currentAmmo = Mathf.Clamp(m_currentAmmo, 0, MaxAmmo);
			if (num != (float)m_currentAmmo)
			{
				if (m_ammoChangeEventArgs == null)
				{
					m_ammoChangeEventArgs = new PlayerAmmoChangeEventArgs(this, m_currentAmmo, num);
				}
				else
				{
					m_ammoChangeEventArgs.Initialize(this, m_currentAmmo, num);
				}
				Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerAmmoChange, this, m_ammoChangeEventArgs);
				if (m_currentAmmo <= 0)
				{
					StartCooldownTimer();
				}
			}
		}
	}

	public bool IsOnCooldown => CooldownTimer > 0f;

	public virtual AbilityData AbilityData => m_abilityData;

	public AbilityAnimState CurrentAbilityAnimState { get; protected set; }

	protected virtual void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[1] { m_projectileName };
	}

	protected virtual void Awake()
	{
		m_hasAnimation = !string.IsNullOrEmpty(m_abilityTellIntroName);
		if (Application.isPlaying)
		{
			if ((bool)m_abilityData)
			{
				m_abilityData = UnityEngine.Object.Instantiate(m_abilityData);
			}
			m_cooldownOverEventArgs = new AbilityCooldownOverEventArgs(this);
			m_attackFlipRaycastArray = new RaycastHit2D[5];
			m_reduceCooldownEvent = ReduceCooldownEvent;
			m_increaseManaEvent = IncreaseManaEvent;
			m_onProjectileHitDisableAttackFlip = OnProjectileHitDisableAttackFlip;
			m_bounce = Bounce;
			m_exitAnimationState = ExitAnimationState;
			m_enterAnimationState = EnterAnimationState;
			m_onEnemyHitCancelAttackFlip = OnEnemyHitCancelAttackFlip;
		}
	}

	public virtual void Initialize(CastAbility_RL abilityController, CastAbilityType castAbilityType)
	{
		m_abilityController = abilityController;
		m_animator = m_abilityController.Animator;
		if (!m_animator)
		{
			throw new Exception("Animator null on ability: " + base.name);
		}
		CastAbilityType = castAbilityType;
		CurrentAmmo = MaxAmmo;
		SetCooldownRegenType();
		if (m_critHitTimingWindow != Vector2.zero)
		{
			m_critEffectGO = new GameObject("CritEffectGO");
			m_critEffectGO.transform.SetParent(m_abilityController.PlayerController.Pivot.transform, worldPositionStays: false);
			m_critEffectGO.transform.localScale = new Vector3(1f, 1f, 1f);
			m_critEffectGO.transform.localPosition = new Vector3(0f, m_critEffectOffset.y, 0f);
		}
	}

	public virtual void Reinitialize()
	{
	}

	private void SetCooldownRegenType()
	{
		if (DecreaseCooldownOverTime)
		{
			CooldownRegenType = CooldownRegenType.Timer_Small;
		}
		else if (DecreaseCooldownWhenHit)
		{
			CooldownRegenType = CooldownRegenType.EnemyHit_Small;
		}
		else if ((bool)GetComponent<ReduceCDOnKillAbilityMod>())
		{
			CooldownRegenType = CooldownRegenType.EnemyKilled_Small;
		}
		else if ((bool)GetComponent<ReduceCDOnPlayerHitAbilityMod>())
		{
			CooldownRegenType = CooldownRegenType.PlayerHit_Small;
		}
		else if ((bool)GetComponent<ReduceCDOnItemDropAbilityMod>())
		{
			CooldownRegenType = CooldownRegenType.MeatPickedUp_Small;
		}
		else if ((bool)GetComponent<ReduceCDOnSpinKickAbilityMod>())
		{
			CooldownRegenType = CooldownRegenType.SpinKick;
		}
	}

	public virtual void PreCastAbility()
	{
		AbilityActive = true;
		IsAnimationComplete = false;
		m_animator.SetFloat("NaturalWalk", 1f);
		m_abilityCastStartTime = Time.time;
		bool flag = false;
		if (SaveManager.ConfigData.ToggleMouseAttackFlip)
		{
			ControllerType lastActiveControllerType = ReInput.controllers.GetLastActiveControllerType();
			if (lastActiveControllerType == ControllerType.Mouse || lastActiveControllerType == ControllerType.Keyboard)
			{
				flag = true;
			}
		}
		if (!flag && ((m_abilityController.HorizontalInput < 0f && m_abilityController.PlayerController.IsFacingRight) || (m_abilityController.HorizontalInput > 0f && !m_abilityController.PlayerController.IsFacingRight)))
		{
			m_abilityController.PlayerController.CharacterCorgi.Flip();
		}
		if (HasAnimation)
		{
			AbilityAnimBehaviourManager.OnAnimStateEnterRelay.AddListener(m_enterAnimationState);
			AbilityAnimBehaviourManager.OnAnimStateExitRelay.AddListener(m_exitAnimationState);
			m_abilityNameCasted = AbilityTellIntroName;
			m_animator.SetTrigger(m_abilityNameCasted);
			if (HasAttackFlipCheck)
			{
				m_enemyHit = false;
				PerformAttackFlipCheck();
			}
		}
		else
		{
			IsAnimationComplete = true;
		}
	}

	public void PerformAttackFlipCheck()
	{
		if (m_attackFlipCheckCoroutine != null)
		{
			StopCoroutine(m_attackFlipCheckCoroutine);
		}
		m_attackFlipCheckCoroutine = StartCoroutine(UpdateAttackFlipCheck());
	}

	private void OnEnemyHitCancelAttackFlip(object sender, EventArgs args)
	{
		if (args is CharacterHitEventArgs characterHitEventArgs && characterHitEventArgs.Attacker == m_firedProjectile && m_attackFlipCheckCoroutine != null)
		{
			StopCoroutine(m_attackFlipCheckCoroutine);
		}
	}

	protected virtual void EnterAnimationState(AbilityAnimState animState)
	{
		CurrentAbilityAnimState = animState;
		float value = 0f;
		switch (animState)
		{
		case AbilityAnimState.TellIntro:
			value = TellIntroAnimSpeed;
			OnEnterTellIntroLogic();
			break;
		case AbilityAnimState.Tell:
			value = TellAnimSpeed;
			OnEnterTellLogic();
			break;
		case AbilityAnimState.Attack_Intro:
			value = AttackIntroAnimSpeed;
			OnEnterAttackIntroLogic();
			break;
		case AbilityAnimState.Attack:
			value = AttackAnimSpeed;
			OnEnterAttackLogic();
			break;
		case AbilityAnimState.Exit:
			value = ExitAnimSpeed;
			OnEnterExitLogic();
			break;
		}
		if (!m_animator)
		{
			throw new Exception("ability animator should not be null.  AbilityName: " + base.name + " WasDestroyed: " + !this);
		}
		m_animator.SetFloat("Ability_Anim_Speed", value);
		float duration = CalculateTotalAnimationDelay();
		m_changeAnimCoroutine = StartCoroutine(ChangeAnim(duration));
	}

	protected void CancelChangeAnimCoroutine()
	{
		m_animator.ResetTrigger("Change_Ability_Anim");
		if (m_changeAnimCoroutine != null)
		{
			StopCoroutine(m_changeAnimCoroutine);
		}
	}

	protected virtual float CalculateTotalAnimationDelay()
	{
		float num = 0f;
		float num2 = 0f;
		switch (CurrentAbilityAnimState)
		{
		case AbilityAnimState.TellIntro:
			num = TellIntroAnimSpeed;
			num2 = TellIntroAnimExitDelay;
			break;
		case AbilityAnimState.Tell:
			num = TellAnimSpeed;
			num2 = TellAnimExitDelay;
			break;
		case AbilityAnimState.Attack_Intro:
			num = AttackIntroAnimSpeed;
			num2 = AttackIntroAnimExitDelay;
			break;
		case AbilityAnimState.Attack:
			num = AttackAnimSpeed;
			num2 = AttackAnimExitDelay;
			break;
		case AbilityAnimState.Exit:
			num = ExitAnimSpeed;
			num2 = ExitAnimExitDelay;
			break;
		}
		float num3 = m_animator.GetCurrentAnimatorStateInfo(CurrentAnimationLayer).length / num;
		return 0f + num3 + num2 - Time.deltaTime;
	}

	protected virtual void ExitAnimationState(AbilityAnimState animState)
	{
		switch (animState)
		{
		case AbilityAnimState.TellIntro:
			OnExitTellIntroLogic();
			break;
		case AbilityAnimState.Tell:
			OnExitTellLogic();
			break;
		case AbilityAnimState.Attack_Intro:
			OnExitAttackIntroLogic();
			break;
		case AbilityAnimState.Attack:
			OnExitAttackLogic();
			break;
		case AbilityAnimState.Exit:
			OnExitExitLogic();
			break;
		}
	}

	protected virtual void OnEnterTellIntroLogic()
	{
	}

	protected virtual void OnEnterTellLogic()
	{
	}

	protected virtual void OnEnterAttackIntroLogic()
	{
	}

	protected virtual void OnEnterAttackLogic()
	{
		m_abilityController.BroadcastAbilityCastEvents(CastAbilityType);
		FireProjectile();
		StartCooldownTimer();
	}

	protected virtual void OnEnterExitLogic()
	{
	}

	protected virtual void OnExitTellIntroLogic()
	{
	}

	protected virtual void OnExitTellLogic()
	{
	}

	protected virtual void OnExitAttackIntroLogic()
	{
	}

	protected virtual void OnExitAttackLogic()
	{
		if (!m_animator)
		{
			throw new Exception("ability animator should not be null.  AbilityName: " + base.name + " WasDestroyed: " + !this);
		}
		m_animator.SetFloat("NaturalWalk", 0f);
	}

	protected virtual void OnExitExitLogic()
	{
		IsAnimationComplete = true;
	}

	protected virtual IEnumerator ChangeAnim(float duration)
	{
		if (duration <= 0f)
		{
			yield return null;
		}
		while (duration > 0f)
		{
			duration -= Time.deltaTime;
			yield return null;
		}
		m_animator.SetTrigger("Change_Ability_Anim");
		PerformTurnAnimCheck();
	}

	protected void PerformTurnAnimCheck()
	{
		if (CurrentAbilityAnimState != AbilityAnimState.Exit || !LockDirectionWhenCasting || !(MovementMod > 0f))
		{
			return;
		}
		PlayerController playerController = m_abilityController.PlayerController;
		Character characterCorgi = playerController.CharacterCorgi;
		if (playerController.IsGrounded)
		{
			float axis = characterCorgi.REPlayer.GetAxis("MoveHorizontal");
			if (axis > 0.1f && !characterCorgi.IsFacingRight)
			{
				characterCorgi.Flip();
			}
			else if (axis < -0.1f && characterCorgi.IsFacingRight)
			{
				characterCorgi.Flip();
			}
		}
	}

	public virtual IEnumerator CastAbility()
	{
		m_beginCastingRelay.Dispatch();
		if (m_hasAnimation)
		{
			while (!IsAnimationComplete)
			{
				yield return null;
			}
		}
		else
		{
			OnEnterAttackLogic();
		}
	}

	public virtual void StopAbility(bool abilityInterrupted)
	{
		m_stopCastingRelay.Dispatch();
		m_animator.SetFloat("NaturalWalk", 0f);
		StopAllCoroutines();
		IsAnimationComplete = false;
		m_attackFlipCheckTimer = 0f;
		m_animator.SetFloat("Ability_Anim_Speed", 1f);
		ForceTriggerCrit = false;
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.EnemyHit, m_onEnemyHitCancelAttackFlip);
		if (m_critDisplayOn)
		{
			m_abilityController.PlayerController.BlinkPulseEffect.ResetRendererArrayColor();
			m_critDisplayOn = false;
		}
		if ((bool)m_firedProjectile)
		{
			m_firedProjectile = null;
		}
		if (!AbilityActive)
		{
			return;
		}
		if (HasAnimation)
		{
			AbilityAnimBehaviourManager.OnAnimStateEnterRelay.RemoveListener(m_enterAnimationState);
			AbilityAnimBehaviourManager.OnAnimStateExitRelay.RemoveListener(m_exitAnimationState);
			if (abilityInterrupted)
			{
				m_abilityController.Animator.SetTrigger("Cancel_Ability_Anim");
			}
			m_animator.ResetTrigger("Change_Ability_Anim");
			m_animator.ResetTrigger(m_abilityNameCasted);
			AbilityAnimBehaviourManager.ResetManager();
		}
		AbilityActive = false;
	}

	protected virtual void Update()
	{
		if (DecreaseCooldownOverTime && CooldownTimer > 0f)
		{
			m_cooldownTimer -= Time.deltaTime;
			if (CooldownTimer <= 0f)
			{
				EndCooldownTimer();
			}
		}
		if (LockoutTimer > 0f)
		{
			m_lockoutTimer -= Time.deltaTime;
		}
		if (AbilityActive)
		{
			CriticalHitTimingDisplay();
		}
	}

	protected void CriticalHitTimingDisplay()
	{
		if (IsInCritWindow && !m_critDisplayOn && (bool)m_critEffectGO)
		{
			StartCoroutine(CritWindowCoroutine());
		}
	}

	private IEnumerator CritWindowCoroutine()
	{
		m_critDisplayOn = true;
		GenericEffect genericEffect = EffectManager.PlayEffect(m_abilityController.gameObject, null, "WeaponShaPing_Effect", Vector3.zero, 1f, EffectStopType.Gracefully) as GenericEffect;
		genericEffect.transform.SetParent(m_critEffectGO.transform, worldPositionStays: false);
		genericEffect.transform.localPosition = new Vector3(m_critEffectOffset.x, 0f, 0f);
		PlayCritWindowAudio();
		float num = m_critHitTimingWindow.y - m_critHitTimingWindow.x;
		ParticleSystem.MainModule mainPartSys = genericEffect.ParticleSystem.main;
		float simulationSpeed = mainPartSys.duration / num;
		float storedSimSpeed = mainPartSys.simulationSpeed;
		mainPartSys.simulationSpeed = simulationSpeed;
		float delay = Time.time + num;
		while (Time.time < delay)
		{
			yield return null;
		}
		mainPartSys.simulationSpeed = storedSimSpeed;
		m_critDisplayOn = false;
	}

	protected virtual void PlayCritWindowAudio()
	{
	}

	protected virtual void PlayDashCritAudio()
	{
	}

	protected virtual void PlayFreeCritReleaseAudio()
	{
		AudioManager.PlayOneShotAttached(this, "event:/SFX/Weapons/sfx_weapon_crit_charged_release", m_abilityController.gameObject);
	}

	protected virtual void FireProjectile()
	{
		if (!string.IsNullOrEmpty(ProjectileName))
		{
			m_firedProjectile = ProjectileManager.FireProjectile(m_abilityController.gameObject, ProjectileName, ProjectileOffset);
			m_abilityController.InitializeProjectile(m_firedProjectile);
			ApplyAbilityCosts();
		}
	}

	protected virtual void ApplyAbilityCosts()
	{
		if (m_abilityController.PlayerController.SpellOrbs > 0)
		{
			m_consumedSpellOrb = true;
			m_abilityController.PlayerController.SpellOrbs--;
			m_abilityController.PlayerController.SetMana(0f, additive: true, runEvents: true);
			return;
		}
		if (MaxAmmo > 0)
		{
			CurrentAmmo--;
		}
		if (ActualCost <= 0)
		{
			return;
		}
		RelicObj relic = SaveManager.PlayerSaveData.GetRelic(RelicType.FreeCastSpell);
		if (relic.Level > 0)
		{
			int relicMaxStack = Relic_EV.GetRelicMaxStack(relic.RelicType, relic.Level);
			if (relic.IntValue >= relicMaxStack)
			{
				relic.SetIntValue(0, additive: false);
				return;
			}
			relic.SetIntValue(1, additive: true);
		}
		m_abilityController.PlayerController.SetMana(-ActualCost, additive: true, runEvents: true);
	}

	public virtual void StartCooldownTimer()
	{
		if (m_consumedSpellOrb)
		{
			m_consumedSpellOrb = false;
			return;
		}
		if (MaxAmmo <= 0 || CooldownTimer <= 0f)
		{
			m_cooldownTimer = ActualCooldownTime;
		}
		if (MaxAmmo <= 0 || CooldownTimer <= 0f || (MaxAmmo > 0 && CurrentAmmo <= 0))
		{
			if (m_cooldownEventArgs == null)
			{
				m_cooldownEventArgs = new CooldownEventArgs(this);
			}
			else
			{
				m_cooldownEventArgs.Initialize(this);
			}
			m_onCooldownRelay.Dispatch(this, m_cooldownEventArgs);
		}
	}

	public void EndCooldownTimer(bool displayCooldownResetText = true)
	{
		int currentAmmo = CurrentAmmo;
		RegenerateAmmo();
		m_cooldownTimer = 0f;
		if (AbilityType != AbilityType.LightningSpell && displayCooldownResetText && currentAmmo == 0 && !CutsceneManager.IsCutsceneActive)
		{
			AudioManager.PlayOneShot(null, "event:/UI/FrontEnd/ui_fe_deathRecap_discoveries", base.transform.position);
			string @string = LocalizationManager.GetString("LOC_ID_STATUS_EFFECT_COOLDOWN_CHARGED_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			TextPopupManager.DisplayTextDefaultPos(TextPopupType.DownstrikeAmmoGain, @string, m_abilityController.PlayerController, attachToSource: true);
		}
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.AbilityCooldownOver, this, m_cooldownOverEventArgs);
		if (CurrentAmmo < MaxAmmo && MaxAmmo > 0)
		{
			StartCooldownTimer();
		}
	}

	public virtual void RegenerateAmmo()
	{
		if (CooldownRefreshesAllAmmo)
		{
			CurrentAmmo = MaxAmmo;
		}
		else
		{
			CurrentAmmo++;
		}
	}

	protected void StartLockoutTimer()
	{
		m_lockoutTimer = LockoutTime;
	}

	public void InitializeProjectile(Projectile_RL projectile)
	{
		projectile.OnCollisionRelay.AddOnce(m_onProjectileHitDisableAttackFlip);
		if (DecreaseCooldownWhenHit)
		{
			if (projectile.HasCDReductionDelay)
			{
				if ((bool)m_persistentCollListenerProj)
				{
					m_persistentCollListenerProj.OnCollisionRelay.RemoveListener(m_reduceCooldownEvent);
				}
				m_persistentCollListenerProj = projectile;
				projectile.OnCollisionRelay.AddListener(m_reduceCooldownEvent);
			}
			else
			{
				projectile.OnCollisionRelay.AddOnce(m_reduceCooldownEvent);
			}
		}
		if (GainManaWhenHit)
		{
			projectile.OnCollisionRelay.AddOnce(m_increaseManaEvent);
		}
		if (projectile.CastAbilityType == CastAbilityType)
		{
			if (CastAbilityType == CastAbilityType.Weapon)
			{
				if (m_abilityController.PlayerController.CharacterClass.ClassType == ClassType.LadleClass && !(projectile is DownstrikeProjectile_RL))
				{
					float num = 3.05f;
					int level = SaveManager.PlayerSaveData.GetRelic(RelicType.WeaponsBurnAdd).Level;
					if (level > 0)
					{
						num += 2f * (float)level;
					}
					projectile.AttachStatusEffect(StatusEffectType.Enemy_Burn, num);
				}
				if (m_abilityController.PlayerController.StatusEffectController.HasStatusEffect(StatusEffectType.Player_FreeCrit))
				{
					PlayFreeCritReleaseAudio();
					m_abilityController.PlayerController.StatusEffectController.StopStatusEffect(StatusEffectType.Player_FreeCrit, interrupted: false);
					ForceTriggerCrit = true;
				}
				if (m_abilityController.PlayerController.StatusEffectController.HasStatusEffect(StatusEffectType.Player_Combo))
				{
					BaseStatusEffect statusEffect = m_abilityController.PlayerController.StatusEffectController.GetStatusEffect(StatusEffectType.Player_Combo);
					if ((bool)statusEffect && statusEffect.IsPlaying && statusEffect.TimesStacked >= 15)
					{
						ForceTriggerCrit = true;
					}
				}
				if (m_abilityController.PlayerController.CloakInterrupted)
				{
					projectile.AttachStatusEffect(StatusEffectType.Enemy_Vulnerable, 2f);
					ForceTriggerCrit = true;
				}
			}
			if (CritsWhenDashing && m_abilityController.PlayerController.MovementState == CharacterStates.MovementStates.Dashing)
			{
				PlayDashCritAudio();
				ForceTriggerCrit = true;
			}
		}
		if (IsInCritWindow)
		{
			ForceTriggerCrit = true;
			if ((bool)m_critEffectGO)
			{
				BaseEffect baseEffect = EffectManager.PlayEffect(m_abilityController.gameObject, null, "CriticalSuccess_Effect", Vector3.zero, 1f, EffectStopType.Gracefully);
				baseEffect.transform.SetParent(m_critEffectGO.transform, worldPositionStays: false);
				baseEffect.transform.localPosition = new Vector3(m_critEffectOffset.x, 0f, 0f);
			}
		}
		if (projectile.CastAbilityType != CastAbilityType)
		{
			return;
		}
		m_abilityCastStartTime = 0f;
		if (ForceTriggerCrit)
		{
			projectile.ActualCritChance += 100f;
			if (projectile.CastAbilityType == CastAbilityType.Weapon)
			{
				projectile.ChangeSpriteRendererColor(ProjectileLibrary.GetBuffColor(ProjectileBuffType.PlayerDashAttack));
			}
			ForceTriggerCrit = false;
		}
	}

	protected void OnProjectileHitDisableAttackFlip(Projectile_RL projectile, GameObject colliderObj)
	{
		m_enemyHit = true;
	}

	protected void ReduceCooldownEvent(Projectile_RL projectile, GameObject colliderObj)
	{
		if (!projectile.isActiveAndEnabled)
		{
			return;
		}
		if (colliderObj.CompareTag("Enemy"))
		{
			if (!projectile.HasCDReductionDelay || Time.time > projectile.CDReducDelay)
			{
				EnemyController component = colliderObj.GetComponent<EnemyController>();
				if (!component || !component.DisableHPMPBonuses)
				{
					projectile.CDReducDelay = Time.time + 0.5f;
					ReduceCooldown(projectile.CooldownReductionPerHit);
				}
			}
		}
		else
		{
			if (TraitManager.IsTraitActive(TraitType.BreakPropsForMana) && (colliderObj.CompareTag("Breakable") || colliderObj.CompareTag("FlimsyBreakable")))
			{
				ReduceCooldown(0f);
			}
			if (!projectile.HasCDReductionDelay)
			{
				projectile.OnCollisionRelay.AddOnce(m_reduceCooldownEvent);
			}
		}
	}

	public void ReduceCooldown(float amount)
	{
		if (CooldownTimer > 0f)
		{
			m_cooldownTimer -= amount;
			if (CooldownTimer <= 0f)
			{
				EndCooldownTimer();
			}
		}
	}

	public void ReduceFlatCooldown(int flatAmount)
	{
		for (int i = 0; i < flatAmount; i++)
		{
			EndCooldownTimer();
		}
	}

	protected void IncreaseManaEvent(Projectile_RL projectile, GameObject colliderObj)
	{
		_ = projectile.isActiveAndEnabled;
	}

	protected IEnumerator UpdateAttackFlipCheck()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.EnemyHit, m_onEnemyHitCancelAttackFlip);
		m_attackFlipCheckTimer = 0.285f;
		PlayerController playerController = m_abilityController.PlayerController;
		while (m_attackFlipCheckTimer > 0f && !m_enemyHit)
		{
			float num = 0.01f;
			Vector2 vector = playerController.ControllerCorgi.BoundsBottomLeftCorner;
			Vector2 vector2 = playerController.ControllerCorgi.BoundsTopLeftCorner;
			vector.x -= num;
			vector2.x -= num;
			Vector2 vector3 = playerController.ControllerCorgi.BoundsBottomRightCorner;
			Vector2 vector4 = playerController.ControllerCorgi.BoundsTopRightCorner;
			vector3.x += num;
			vector4.x += num;
			while (!m_firedProjectile || !m_firedProjectile.isActiveAndEnabled)
			{
				yield return null;
			}
			Collider2D collider = m_firedProjectile.HitboxController.GetCollider(HitboxType.Weapon);
			float num2 = collider.bounds.center.x - playerController.Midpoint.x;
			Vector3 center = collider.bounds.center;
			Vector2 boxSize = collider.bounds.size;
			num2 += 0.625f;
			boxSize.x += 1.25f;
			boxSize.y += 1.6f;
			bool collidingFront;
			bool collidingBack;
			if (playerController.IsFacingRight)
			{
				collidingFront = CheckAttackFlip(center, boxSize, 1);
				center.x -= num2 * 2f;
				collidingBack = CheckAttackFlip(center, boxSize, -1);
			}
			else
			{
				collidingBack = CheckAttackFlip(center, boxSize, 1);
				center.x -= num2 * 2f;
				collidingFront = CheckAttackFlip(center, boxSize, -1);
			}
			m_attackFlipCheckTimer -= Time.deltaTime;
			if (playerController.MovementState != CharacterStates.MovementStates.Dashing)
			{
				if (IsAttackFlipValid(playerController, collidingBack, collidingFront))
				{
					playerController.CharacterCorgi.Flip();
					break;
				}
				yield return null;
				continue;
			}
			break;
		}
	}

	protected bool CheckAttackFlip(Vector2 boxOrigin, Vector2 boxSize, sbyte direction)
	{
		Vector2 direction2 = Vector2.right;
		if (direction < 0)
		{
			direction2 = Vector2.left;
		}
		int num = Physics2D.BoxCastNonAlloc(boxOrigin, boxSize, 0f, direction2, m_attackFlipRaycastArray, 0.01f, 2097152);
		for (int i = 0; i < num; i++)
		{
			RaycastHit2D raycastHit2D = m_attackFlipRaycastArray[i];
			if (raycastHit2D.collider.CompareTag("Enemy"))
			{
				return true;
			}
			if (raycastHit2D.collider.CompareTag("EnemyProjectile"))
			{
				Projectile_RL component = raycastHit2D.collider.GetComponent<HitboxInfo>().RootGameObj.GetComponent<Projectile_RL>();
				if ((bool)component && (component.CollisionFlags & ProjectileCollisionFlag.WeaponStrikeable) != 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	protected bool IsAttackFlipValid(PlayerController playerController, bool collidingBack, bool collidingFront)
	{
		float axis = Rewired_RL.Player.GetAxis("MoveHorizontal");
		if (axis < 0f && playerController.IsFacingRight && collidingBack && !collidingFront)
		{
			return true;
		}
		if (axis > 0f && !playerController.IsFacingRight && !collidingBack && collidingFront)
		{
			return true;
		}
		return false;
	}

	protected void Bounce(Projectile_RL projectile, GameObject colliderObj)
	{
		CharacterStates.CharacterConditions conditionState = m_abilityController.PlayerController.ConditionState;
		if (conditionState != CharacterStates.CharacterConditions.Stunned && conditionState != CharacterStates.CharacterConditions.Dead)
		{
			GameObject root = colliderObj.GetRoot();
			DownstrikeKnockbackOverride component = root.GetComponent<DownstrikeKnockbackOverride>();
			BoundsObj component2 = root.GetComponent<BoundsObj>();
			float num = AttackBounceHeight;
			if (component != null)
			{
				num = component.Value;
			}
			if (component2 != null)
			{
				float num2 = num * num / (2f * Mathf.Abs(-50f));
				float num3 = component2.Top - projectile.Owner.transform.position.y;
				num = Mathf.Sqrt(2f * Mathf.Abs(-50f) * (num2 + num3));
			}
			if (num > m_highestBounceAmount)
			{
				m_highestBounceAmount = num;
				m_triggerBounce = true;
			}
		}
	}

	private void TriggerBounce()
	{
		m_triggerBounce = false;
		float highestBounceAmount = m_highestBounceAmount;
		m_highestBounceAmount = float.MinValue;
		CharacterStates.CharacterConditions conditionState = m_abilityController.PlayerController.ConditionState;
		if (conditionState != CharacterStates.CharacterConditions.Stunned && conditionState != CharacterStates.CharacterConditions.Dead)
		{
			m_abilityController.PlayerController.ControllerCorgi.GravityActive(state: true);
			if ((bool)m_firedProjectile)
			{
				m_firedProjectile.OnCollisionRelay.RemoveListener(m_bounce);
			}
			m_abilityController.PlayerController.SetVelocityY(highestBounceAmount, additive: false);
			if (m_abilityController.PlayerController.CharacterJump != null)
			{
				m_abilityController.PlayerController.CharacterJump.ResetBrakeForce();
				m_abilityController.PlayerController.CharacterJump.StartJumpTime();
			}
		}
	}

	private void LateUpdate()
	{
		if (m_triggerBounce)
		{
			TriggerBounce();
		}
	}

	protected virtual void OnDestroy()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.EnemyHit, m_onEnemyHitCancelAttackFlip);
		AbilityAnimBehaviourManager.OnAnimStateEnterRelay.RemoveListener(m_enterAnimationState);
		AbilityAnimBehaviourManager.OnAnimStateExitRelay.RemoveListener(m_exitAnimationState);
		if ((bool)m_persistentCollListenerProj)
		{
			m_persistentCollListenerProj.OnCollisionRelay.RemoveListener(m_reduceCooldownEvent);
		}
		m_persistentCollListenerProj = null;
	}

	public virtual void OnPreDestroy()
	{
	}
}
