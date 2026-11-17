using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MoreMountains.CorgiEngine;
using RL_Windows;
using SceneManagement_RL;
using Sigtrap.Relays;
using UnityEngine;

public class EnemyController : BaseCharacterController, IRoomConsumer, ILevelConsumer, ISummoner, IHealth, IDamageObj, IGenericPoolObj
{
	private const float CHASE_DISTANCE_THRESHOLD = 0.1f;

	private static int LAND_ANIMATION_STATE_NAME = Animator.StringToHash("Land");

	private static int TURN_ANIMATION_TRIGGER_NAME = Animator.StringToHash("Turn");

	private static RelicChangedEventArgs m_relicChangedEventArgs_STATIC = new RelicChangedEventArgs(RelicType.None);

	[Header("Enemy Values")]
	[SerializeField]
	[HideInInspector]
	private EnemyType m_enemyType = EnemyType.Skeleton;

	[SerializeField]
	[HideInInspector]
	private EnemyRank m_enemyRank;

	[Space(10f)]
	[SerializeField]
	private float m_flightKnockbackDecelerationMod = 1f;

	[SerializeField]
	private bool m_constrainToRoom = true;

	[SerializeField]
	private bool m_knocksBackOnHit;

	[SerializeField]
	private StrikeType m_strikeType = StrikeType.Blunt;

	[SerializeField]
	private bool m_isBoss;

	[SerializeField]
	private bool m_forceActivate;

	[SerializeField]
	private bool m_disableXPBonuses;

	[SerializeField]
	private bool m_disableHPMPBonuses;

	[SerializeField]
	private bool m_alwaysDealsContactDamage;

	[SerializeField]
	private GameObject[] m_culledObjectsArray;

	private static Dictionary<EnemyTypeAndRank, EnemyData> EnemyData_StaticInstanceDict = new Dictionary<EnemyTypeAndRank, EnemyData>();

	private float m_baseSpeed;

	private float m_baseTurnSpeed;

	private Vector2 m_baseRestCooldown;

	private bool m_isFlying;

	private bool m_alwaysFace;

	private bool m_collidesWithPlatforms;

	private float m_baseKnockbackStrength;

	private float m_baseStunStrength;

	private float m_baseDropOdds;

	private float m_closeRadius;

	private float m_mediumRadius;

	private float m_farRadius;

	protected GameObject m_target;

	protected BaseCharacterController m_targetController;

	private Vector3 m_followOffset;

	private float m_baseScaleToOffsetWith = 1f;

	private LogicController m_logicController;

	private bool m_pivotFollowsOrientation;

	private EnemyData m_enemyData;

	private int m_level = 1;

	private float m_previousAirVelocityX;

	private PreventPlatformDrop m_preventPlatformDropObj;

	private bool m_fallLedge;

	[NonSerialized]
	public Vector2 FlightKnockbackAcceleration;

	[NonSerialized]
	public bool CanIncrementRelicHitCounter = true;

	[NonSerialized]
	public bool AttackingWithContactDamage;

	[NonSerialized]
	public bool DisableDeath;

	private static EnemyDeathEventArgs m_enemyDeathEventArgs_STATIC;

	private static EnemyActivationStateChangedEventArgs m_enemyActivationStateChangedEventArgs_STATIC;

	private int m_enemyIndex = -1;

	private bool m_enemyInitializing;

	private bool m_touchingLeftRoomEdge;

	private bool m_touchingRightRoomEdge;

	private bool m_touchingTopRoomEdge;

	private bool m_touchingBottomRoomEdge;

	private int m_commanderStatusEffectFlags;

	private DamageType m_lastDamageTypeTaken;

	private Relay<object, EnemyActivationStateChangedEventArgs> m_onActivatedRelay = new Relay<object, EnemyActivationStateChangedEventArgs>();

	private Relay<object, EnemyActivationStateChangedEventArgs> m_onDeactivatedRelay = new Relay<object, EnemyActivationStateChangedEventArgs>();

	private Relay<object, EnemyActivationStateChangedEventArgs> m_onReactivationTimedOutRelay = new Relay<object, EnemyActivationStateChangedEventArgs>();

	private Relay<object, EnemyDeathEventArgs> m_onEnemyDeathRelay = new Relay<object, EnemyDeathEventArgs>();

	private Relay<object, EventArgs> m_onResetPositionRelay = new Relay<object, EventArgs>();

	private Relay<object, EventArgs> m_onPositionedForSummoningRelay = new Relay<object, EventArgs>();

	private Relay m_onDisableRelay = new Relay();

	public EnemyManager.EnemyPreSummonedState PreSummonedState { get; set; }

	public bool IsCulled { get; set; }

	public bool KilledByKnockout { get; private set; }

	public bool DisableDistanceThresholdCheck { get; set; }

	public bool IsCommander { get; set; }

	public int LastDamageHitCount { get; set; }

	public DamageType LastDamageTakenType
	{
		get
		{
			return m_lastDamageTypeTaken;
		}
		set
		{
			if (m_lastDamageTypeTaken != value)
			{
				LastDamageHitCount = 0;
			}
			m_lastDamageTypeTaken = value;
		}
	}

	public PreventPlatformDrop PreventPlatformDropObj => m_preventPlatformDropObj;

	public bool FallLedge
	{
		get
		{
			if (IsFlying)
			{
				return true;
			}
			return m_fallLedge;
		}
		set
		{
			if (IsFlying)
			{
				value = true;
			}
			if ((bool)m_preventPlatformDropObj)
			{
				m_fallLedge = value;
				if (m_preventPlatformDropObj.enabled != !value)
				{
					m_preventPlatformDropObj.enabled = !value;
				}
			}
		}
	}

	public bool IsBeingSummoned { get; set; }

	public bool IsFreePoolObj { get; set; }

	public bool IsAwakeCalled { get; protected set; }

	public bool DisableXPBonuses
	{
		get
		{
			return m_disableXPBonuses;
		}
		set
		{
			m_disableXPBonuses = value;
		}
	}

	public bool DisableHPMPBonuses
	{
		get
		{
			return m_disableHPMPBonuses;
		}
		set
		{
			m_disableHPMPBonuses = value;
		}
	}

	public bool IsVulnerableToLifeSteal => !m_disableHPMPBonuses;

	public bool ResetToNeutralWhenUnculling { get; set; } = true;


	public bool DisableCulling { get; set; }

	public GameObject[] CulledObjectsArray => m_culledObjectsArray;

	public bool LeftSidePlatformDropPrevented { get; set; }

	public bool RightSidePlatformDropPrevented { get; set; }

	public StatusEffectType[] StatusEffectTypes => null;

	public float[] StatusEffectDurations => null;

	public float ModeshiftDamageMod { get; set; } = 1f;


	public bool IsDotDamage => false;

	public override float ActualStrength
	{
		get
		{
			float num = 1f;
			num += GetInsightBossDamageMod() - 1f;
			float actualStrength = base.ActualStrength;
			int level = SaveManager.PlayerSaveData.GetRelic(RelicType.DamageReductionStatusEffect).Level;
			if (level > 0 && base.StatusEffectController.HasAnyActiveStatusEffect(includeCommanderBuffs: false))
			{
				float num2 = 0.15f * (float)level - 1f;
				num += num2;
			}
			if (IsCommander)
			{
				num += 0f;
			}
			if (base.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_Size))
			{
				num += 0.5f;
			}
			if (base.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_Aggro))
			{
				num += (float)(base.StatusEffectController.GetStatusEffect(StatusEffectType.Enemy_Aggro).TimesStacked - 1) * 0.08f;
			}
			num = Mathf.Clamp(num, 0f, float.MaxValue);
			return actualStrength * num;
		}
	}

	public override int ActualMaxHealth
	{
		get
		{
			if (EnemyType == EnemyType.Target)
			{
				return 1;
			}
			float num = (float)(BaseMaxHealth + base.MaxHealthAdd + base.MaxHealthTemporaryAdd) * (1f + base.MaxHealthMod + base.MaxHealthTemporaryMod);
			if (SaveManager.PlayerSaveData.EnableHouseRules)
			{
				num *= SaveManager.PlayerSaveData.Assist_EnemyHealthMod;
			}
			return Mathf.Clamp(Mathf.FloorToInt(num), 1, int.MaxValue);
		}
	}

	public EnemySpawnController EnemySpawnController { get; set; }

	public ISummoner Summoner { get; set; }

	public bool IsSummoned => Summoner != null;

	public float GroundHorizontalVelocity { get; set; }

	public float JumpHorizontalVelocity { get; set; }

	public bool IsBoss
	{
		get
		{
			return m_isBoss;
		}
		set
		{
			m_isBoss = value;
		}
	}

	public override float BaseScaleToOffsetWith => m_baseScaleToOffsetWith;

	public IRelayLink<object, EnemyActivationStateChangedEventArgs> OnActivatedRelay => m_onActivatedRelay.link;

	public IRelayLink<object, EnemyActivationStateChangedEventArgs> OnDeactivatedRelay => m_onDeactivatedRelay.link;

	public IRelayLink<object, EnemyActivationStateChangedEventArgs> OnReactivationTimedOutRelay => m_onReactivationTimedOutRelay.link;

	public IRelayLink<object, EnemyDeathEventArgs> OnEnemyDeathRelay => m_onEnemyDeathRelay.link;

	public IRelayLink<object, EventArgs> OnResetPositionRelay => m_onResetPositionRelay.link;

	public IRelayLink OnDisableRelay => m_onDisableRelay.link;

	public Relay<object, EventArgs> OnPositionedForSummoningRelay => m_onPositionedForSummoningRelay;

	public override bool DisableFriction
	{
		get
		{
			if (IsFlying)
			{
				return true;
			}
			return base.DisableFriction;
		}
		set
		{
			base.DisableFriction = value;
		}
	}

	public bool ForceActivate
	{
		get
		{
			return m_forceActivate;
		}
		set
		{
			m_forceActivate = value;
		}
	}

	public StrikeType StrikeType
	{
		get
		{
			return m_strikeType;
		}
		set
		{
			m_strikeType = value;
		}
	}

	public bool AlwaysDealsContactDamage
	{
		get
		{
			return m_alwaysDealsContactDamage;
		}
		set
		{
			m_alwaysDealsContactDamage = value;
		}
	}

	public float FlightKnockbackDecelerationMod
	{
		get
		{
			return m_flightKnockbackDecelerationMod;
		}
		set
		{
			m_flightKnockbackDecelerationMod = value;
		}
	}

	public bool RicochetsAttackerOnHit
	{
		get
		{
			return m_knocksBackOnHit;
		}
		set
		{
			m_knocksBackOnHit = value;
		}
	}

	public EnemyData EnemyData
	{
		get
		{
			return m_enemyData;
		}
		set
		{
			m_enemyData = value;
			if (Application.isPlaying)
			{
				InitializeEnemyData();
			}
		}
	}

	public bool ConstrainToRoom
	{
		get
		{
			return m_constrainToRoom;
		}
		set
		{
			m_constrainToRoom = value;
		}
	}

	public LogicController LogicController
	{
		get
		{
			return m_logicController;
		}
		private set
		{
			m_logicController = value;
		}
	}

	public GameObject GameObject => base.gameObject;

	public bool IsTargetToMyRight
	{
		get
		{
			if ((bool)Target && Target.transform.position.x > base.transform.position.x)
			{
				return true;
			}
			return false;
		}
	}

	public bool FollowTarget { get; set; }

	public Vector3 FollowOffset
	{
		get
		{
			return m_followOffset;
		}
		set
		{
			m_followOffset = value;
		}
	}

	public FlyingMovementType FlyingMovementType { get; set; }

	public bool PivotFollowsOrientation
	{
		get
		{
			return m_pivotFollowsOrientation;
		}
		set
		{
			m_pivotFollowsOrientation = value;
			if (IsFlying && !AlwaysFacing && m_pivotFollowsOrientation && !base.IsFacingRight)
			{
				base.CharacterCorgi.Flip(IgnoreFlipOnDirectionChange: false, ignoreTurnParam: true);
			}
		}
	}

	public virtual bool AlwaysFacing
	{
		get
		{
			return m_alwaysFace;
		}
		set
		{
			m_alwaysFace = value;
		}
	}

	public GameObject Target
	{
		get
		{
			return m_target;
		}
		set
		{
			m_target = value;
			m_targetController = m_target.GetComponent<BaseCharacterController>();
		}
	}

	public BaseCharacterController TargetController => m_targetController;

	public virtual float ActualSummonValue => Enemy_EV.GetSummonValue(EnemyRank);

	public float BaseSpeed
	{
		get
		{
			return m_baseSpeed;
		}
		set
		{
			m_baseSpeed = value;
		}
	}

	public float SpeedMod { get; protected set; }

	public float SpeedAdd { get; protected set; }

	public virtual float ActualSpeed => (BaseSpeed + SpeedAdd) * (1f + SpeedMod);

	public float BaseTurnSpeed
	{
		get
		{
			return m_baseTurnSpeed;
		}
		set
		{
			m_baseTurnSpeed = value;
		}
	}

	public virtual float ActualTurnSpeed => BaseTurnSpeed;

	public Vector2 BaseRestCooldown
	{
		get
		{
			return m_baseRestCooldown;
		}
		set
		{
			m_baseRestCooldown = value;
		}
	}

	public virtual Vector2 ActualRestCooldown => BaseRestCooldown;

	public virtual bool IsFlying
	{
		get
		{
			return m_isFlying;
		}
		set
		{
			m_isFlying = value;
			if ((bool)PreventPlatformDropObj && PreventPlatformDropObj.enabled != !value && (value || (!value && !FallLedge)))
			{
				PreventPlatformDropObj.enabled = !value;
			}
			if (base.IsInitialized)
			{
				base.ControllerCorgi.GravityActive(!value);
				base.ControllerCorgi.StickWhenWalkingDownSlopes = !value;
				base.ControllerCorgi.DisableOneWayCollision = value;
			}
		}
	}

	public virtual bool CollidesWithPlatforms
	{
		get
		{
			return m_collidesWithPlatforms;
		}
		set
		{
			m_collidesWithPlatforms = value;
			if ((bool)base.ControllerCorgi)
			{
				base.ControllerCorgi.DisablePlatformCollision = !value;
			}
		}
	}

	public float BaseStunStrength
	{
		get
		{
			return m_baseStunStrength;
		}
		set
		{
			m_baseStunStrength = value;
		}
	}

	public float ActualStunStrength => BaseStunStrength;

	public float BaseKnockbackStrength
	{
		get
		{
			return m_baseKnockbackStrength;
		}
		set
		{
			m_baseKnockbackStrength = value;
		}
	}

	public float ActualKnockbackStrength => BaseKnockbackStrength;

	public float BaseDropOdds
	{
		get
		{
			return m_baseDropOdds;
		}
		set
		{
			m_baseDropOdds = value;
		}
	}

	public virtual float ActualDropOdds => BaseDropOdds;

	public virtual float CloseRangeRadius
	{
		get
		{
			return m_closeRadius;
		}
		set
		{
			m_closeRadius = value;
			m_closeRadius = Mathf.Clamp(m_closeRadius, 0f, m_mediumRadius);
		}
	}

	public virtual float MediumRangeRadius
	{
		get
		{
			return m_mediumRadius;
		}
		set
		{
			m_mediumRadius = value;
			m_mediumRadius = Mathf.Clamp(m_mediumRadius, m_closeRadius, m_farRadius);
		}
	}

	public virtual float FarRangeRadius
	{
		get
		{
			if (CameraController.ZoomLevel == 1f)
			{
				return m_farRadius;
			}
			float num = (CameraController.ZoomLevel - 1f) * 0.5f + 1f;
			if (num < 1f)
			{
				num = 1f;
			}
			return num * m_farRadius;
		}
		set
		{
			m_farRadius = value;
			m_farRadius = Mathf.Clamp(m_farRadius, m_mediumRadius, float.MaxValue);
		}
	}

	public int Level => m_level;

	public EnemyType EnemyType
	{
		get
		{
			return m_enemyType;
		}
		set
		{
			m_enemyType = value;
			LogicController component = GetComponent<LogicController>();
			if ((bool)component)
			{
				component.UpdateLogicReferences();
			}
			UpdateEnemyDataScale();
		}
	}

	public EnemyRank EnemyRank
	{
		get
		{
			return m_enemyRank;
		}
		set
		{
			m_enemyRank = value;
			LogicController component = GetComponent<LogicController>();
			if ((bool)component)
			{
				component.UpdateLogicReferences();
			}
			UpdateEnemyDataScale();
		}
	}

	public BaseRoom Room { get; private set; }

	public int EnemyIndex
	{
		get
		{
			return m_enemyIndex;
		}
		private set
		{
			m_enemyIndex = value;
		}
	}

	public bool TouchingLeftRoomEdge => m_touchingLeftRoomEdge;

	public bool TouchingRightRoomEdge => m_touchingRightRoomEdge;

	public bool TouchingTopRoomEdge => m_touchingTopRoomEdge;

	public bool TouchingBottomRoomEdge => m_touchingBottomRoomEdge;

	public bool ActivatedByFairyRoomTrigger { get; set; }

	public bool PreserveRotationWhenDeactivated { get; set; }

	public bool ForceDisableSummonOffset { get; set; }

	public bool InvisibleDuringSummonAnim { get; set; }

	public override void SetVelocity(float velocityX, float velocityY, bool additive)
	{
		m_previousAirVelocityX = velocityX;
		base.SetVelocity(velocityX, velocityY, additive);
	}

	public override void SetVelocityX(float velocity, bool additive)
	{
		m_previousAirVelocityX = velocity;
		base.SetVelocityX(velocity, additive);
	}

	public override void SetHealth(float value, bool additive, bool runEvents)
	{
		base.SetHealth(value, additive, runEvents);
		if (runEvents)
		{
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.EnemyHealthChange, this, m_healthChangeEventArgs);
		}
	}

	public void SetLevel(int value)
	{
		m_level = value;
		InjectLevel();
		if (base.IsInitialized)
		{
			InitializeLevelMods();
			ResetHealth();
		}
		m_level = Mathf.Clamp(m_level, 1, int.MaxValue);
	}

	protected override void Awake()
	{
		base.Awake();
		m_preventPlatformDropObj = GetComponent<PreventPlatformDrop>();
		if (m_enemyDeathEventArgs_STATIC == null)
		{
			m_enemyDeathEventArgs_STATIC = new EnemyDeathEventArgs(null, null);
			m_enemyActivationStateChangedEventArgs_STATIC = new EnemyActivationStateChangedEventArgs(null);
		}
		EnemyClassData enemyClassData = EnemyClassLibrary.GetEnemyClassData(EnemyType);
		EnemyData enemyData = (enemyClassData ? enemyClassData.GetEnemyData(EnemyRank) : null);
		if ((bool)m_controllerCorgi && (!enemyData || ((bool)enemyData && !enemyData.CollidesWithPlatforms)))
		{
			m_controllerCorgi.DisablePlatformCollision = true;
		}
		CameraLayerController component = GetComponent<CameraLayerController>();
		component.SetCameraLayer(CameraLayer.Game);
		int subLayer = EnemySubLayerManager.GetSubLayer(new EnemyTypeAndRank(EnemyType, EnemyRank));
		component.SetSubLayer(subLayer);
		m_logicController = GetComponent<LogicController>();
		if (!m_logicController)
		{
			throw new MissingComponentException("LogicController");
		}
		IsAwakeCalled = true;
	}

	protected override IEnumerator Start()
	{
		m_enemyInitializing = true;
		while (!m_controllerCorgi.IsInitialized || !m_characterCorgi.IsInitialized)
		{
			yield return null;
		}
		InitializeEnemyData();
		m_controllerCorgi.SetRaysParameters();
		if (!IsFlying)
		{
			float num = m_controllerCorgi.BoundsWidth / 2f;
			if (Mathf.Abs(m_controllerCorgi.BoxCollider.offset.x) + num > 1f)
			{
				m_controllerCorgi.CastRaysOnBothSides = true;
			}
		}
		if (Application.isPlaying)
		{
			base.ControllerCorgi.GravityActive(!IsFlying);
			base.ControllerCorgi.StickWhenWalkingDownSlopes = !IsFlying;
			base.ControllerCorgi.DisableOneWayCollision = IsFlying;
			if (IsFlying)
			{
				SetVelocity(0f, 0f, additive: false);
			}
		}
		m_target = PlayerManager.GetPlayer();
		m_targetController = PlayerManager.GetPlayerController();
		yield return base.Start();
		m_enemyInitializing = false;
	}

	public void ForceFaceTarget()
	{
		if ((bool)Target)
		{
			base.Heading = TargetController.Midpoint - base.Midpoint;
		}
	}

	public void InitializeEnemyData()
	{
		if (EnemyType == EnemyType.None)
		{
			return;
		}
		EnemyClassData enemyClassData = EnemyClassLibrary.GetEnemyClassData(EnemyType);
		if ((bool)enemyClassData)
		{
			EnemyData enemyData = enemyClassData.GetEnemyData(EnemyRank);
			if ((bool)enemyData)
			{
				EnemyData value = null;
				EnemyTypeAndRank key = new EnemyTypeAndRank(EnemyType, EnemyRank);
				if (!EnemyData_StaticInstanceDict.TryGetValue(key, out value))
				{
					value = UnityEngine.Object.Instantiate(enemyData);
					EnemyData_StaticInstanceDict.Add(key, value);
				}
				m_enemyData = value;
			}
		}
		if ((bool)m_enemyData)
		{
			ResetBaseValues();
			ResetHealth();
		}
	}

	private void OnEnable()
	{
		if (!base.IsInitialized && m_enemyInitializing)
		{
			StartCoroutine(Start());
		}
		DisableEffectsOnSpawn();
	}

	private void OnDisable()
	{
		DisablePooledObjectManager.DisablePooledObject(this);
		FlightKnockbackAcceleration = Vector2.zero;
		if (!GameManager.IsApplicationClosing)
		{
			m_onDisableRelay.Dispatch();
		}
	}

	public void UpdateEnemyDataScale()
	{
		if ((bool)m_enemyData)
		{
			base.transform.localScale = new Vector3(m_enemyData.Scale, m_enemyData.Scale, Mathf.Clamp(m_enemyData.Scale, 0f, 1f));
		}
		else
		{
			EnemyClassData enemyClassData = EnemyClassLibrary.GetEnemyClassData(EnemyType);
			if ((bool)enemyClassData)
			{
				EnemyData enemyData = enemyClassData.GetEnemyData(EnemyRank);
				if ((bool)enemyData)
				{
					base.transform.localScale = new Vector3(enemyData.Scale, enemyData.Scale, Mathf.Clamp(enemyData.Scale, 0f, 1f));
				}
			}
		}
		if (Application.isPlaying)
		{
			if (base.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_Size))
			{
				base.transform.localScale *= 1.25f;
			}
			UpdateBounds();
			base.StatusBarController.ResetPositionAndScale(this);
		}
	}

	public void ForceStandingOn(Collider2D collider)
	{
		if ((bool)m_controllerCorgi)
		{
			m_controllerCorgi.ForceStandingOn(collider);
		}
	}

	public void DisableEffectsOnSpawn()
	{
		StartCoroutine(DisableEffectsOnSpawnCoroutine(0.1f));
	}

	private IEnumerator DisableEffectsOnSpawnCoroutine(float duration)
	{
		if ((bool)m_animator)
		{
			EffectManager.AddAnimatorToDisableList(m_animator);
			float delay = Time.time + duration;
			while (!base.IsInitialized)
			{
				yield return null;
			}
			if (AnimatorUtility.HasState(m_animator, LAND_ANIMATION_STATE_NAME))
			{
				m_animator.Play(LAND_ANIMATION_STATE_NAME, AnimatorUtility.GetLayerIndex(m_animator, LAND_ANIMATION_STATE_NAME), 1f);
				m_animator.Update(1f);
			}
			while (Time.time < delay)
			{
				yield return null;
			}
			EffectManager.RemoveAnimatorFromDisableList(m_animator);
		}
	}

	public void ResetCollisionState()
	{
		if ((bool)m_controllerCorgi)
		{
			m_controllerCorgi.ResetState();
		}
	}

	public void UpdateBounds()
	{
		if ((bool)m_controllerCorgi)
		{
			m_controllerCorgi.SetRaysParameters();
		}
	}

	public override void ResetBaseValues()
	{
		if (GameManager.IsApplicationClosing)
		{
			return;
		}
		base.ResetBaseValues();
		if (!(m_enemyData == null))
		{
			BaseMaxHealth = m_enemyData.Health;
			base.BaseStrength = m_enemyData.WeaponDamage;
			base.BaseMagic = m_enemyData.MagicDamage;
			BaseSpeed = m_enemyData.Speed;
			BaseTurnSpeed = m_enemyData.TurnSpeed;
			BaseRestCooldown = new Vector2(m_enemyData.RestMinCooldown, m_enemyData.RestMaxCooldown);
			bool isGravityActive = base.ControllerCorgi.IsGravityActive;
			IsFlying = m_enemyData.IsFlying;
			base.ControllerCorgi.GravityActive(isGravityActive);
			AlwaysFacing = m_enemyData.AlwaysFace;
			FallLedge = m_enemyData.FallLedge;
			CollidesWithPlatforms = m_enemyData.CollidesWithPlatforms;
			BaseStunDefense = m_enemyData.StunDefence;
			BaseKnockbackDefense = m_enemyData.KnockbackDefence;
			base.StunDuration = m_enemyData.StunDuration;
			InternalKnockbackMod = new Vector2(m_enemyData.KnockbackModX, m_enemyData.KnockbackModY);
			base.BaseScale = m_enemyData.Scale;
			m_baseScaleToOffsetWith = EnemyClassLibrary.GetEnemyClassData(EnemyType).GetEnemyData(EnemyRank.Basic).Scale;
			BaseDropOdds = m_enemyData.DropOdds;
			FarRangeRadius = m_enemyData.FarRadius;
			MediumRangeRadius = m_enemyData.ProjectileRadius;
			CloseRangeRadius = m_enemyData.MeleeRadius;
			ModeshiftDamageMod = 1f;
			UpdateEnemyDataScale();
			JumpHorizontalVelocity = 0f;
			GroundHorizontalVelocity = 0f;
			InitializeLevelMods();
			float num = 1f;
			if (base.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_Speed))
			{
				num += 1.25f;
			}
			if (base.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_Aggro))
			{
				num += (float)(base.StatusEffectController.GetStatusEffect(StatusEffectType.Enemy_Aggro).TimesStacked - 1) * 0.12f;
			}
			BaseSpeed *= num;
			BaseTurnSpeed *= num;
		}
	}

	public void InitializeLevelMods()
	{
		int num = Level;
		if (num < 1)
		{
			Debug.LogFormat("<color=red>| {0} | Enemy Level should be greater than 0, but isn't</color>", this);
			num = 0;
		}
		base.MaxHealthAdd = Mathf.RoundToInt((float)Mathf.Clamp(num - 1, 0, int.MaxValue) * 0.0625f * (float)BaseMaxHealth);
		if (base.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_Size))
		{
			base.MaxHealthAdd += Mathf.RoundToInt(1f * (float)BaseMaxHealth);
		}
		base.StrengthAdd = Mathf.RoundToInt((float)Mathf.Clamp(num - 1, 0, int.MaxValue) * 0.0575f * base.BaseStrength);
		base.MaxHealthMod = 0f;
		base.StrengthMod = 0f;
		SpeedAdd = 0f;
		float burdenStatGain = BurdenManager.GetBurdenStatGain(BurdenType.EnemyHealth);
		base.MaxHealthMod += burdenStatGain;
		float num2 = 0.01f * (float)SaveManager.PlayerSaveData.NewGamePlusLevel;
		base.MaxHealthMod += num2;
		if (IsBoss)
		{
			base.MaxHealthMod += 0.14999998f;
		}
		if (IsCommander)
		{
			base.MaxHealthMod += 0.75f;
		}
		base.MaxHealthMod += EnemyHPModNGZero_BiomeRule.MaxHealthMod - 1f;
		float burdenStatGain2 = BurdenManager.GetBurdenStatGain(BurdenType.EnemyDamage);
		base.StrengthMod += burdenStatGain2;
		float num3 = 0.007f * (float)SaveManager.PlayerSaveData.NewGamePlusLevel;
		base.StrengthMod += num3;
		base.StrengthMod += EnemyDamageModNGZero_BiomeRule.StrengthMod - 1f;
		if (EnemyType != EnemyType.Eggplant)
		{
			float burdenStatGain3 = BurdenManager.GetBurdenStatGain(BurdenType.EnemySpeed);
			if (BaseSpeed > 0f)
			{
				SpeedAdd += burdenStatGain3;
			}
		}
	}

	public void AddCommanderStatusEffect(StatusEffectType statusEffect)
	{
		int num = StatusEffect_EV.COMMANDER_STATUS_EFFECT_ARRAY.IndexOf(statusEffect);
		if (num != -1)
		{
			m_commanderStatusEffectFlags |= 1 << num;
		}
	}

	public void RemoveCommanderStatusEffect(StatusEffectType statusEffect)
	{
		int num = StatusEffect_EV.COMMANDER_STATUS_EFFECT_ARRAY.IndexOf(statusEffect);
		if (num != -1)
		{
			m_commanderStatusEffectFlags &= ~(1 << num);
		}
	}

	public bool HasCommanderStatusEffect(StatusEffectType statusEffect)
	{
		int num = StatusEffect_EV.COMMANDER_STATUS_EFFECT_ARRAY.IndexOf(statusEffect);
		if (num != -1)
		{
			return (m_commanderStatusEffectFlags & (1 << num)) != 0;
		}
		return false;
	}

	public void RemoveAllCommanderStatusEffects()
	{
		m_commanderStatusEffectFlags = 0;
	}

	public void AddRandomCommanderStatusEffect()
	{
		int num = RNGManager.GetRandomNumber(RngID.SpecialProps_RoomSeed, "EnemyController.CreateRandomCommanderStatusEffect", 0, StatusEffect_EV.COMMANDER_STATUS_EFFECT_ARRAY.Length);
		int num2 = 0;
		while ((m_commanderStatusEffectFlags & (1 << num)) != 0 && num2 < StatusEffect_EV.COMMANDER_STATUS_EFFECT_ARRAY.Length)
		{
			num++;
			num2++;
			if (num >= StatusEffect_EV.COMMANDER_STATUS_EFFECT_ARRAY.Length)
			{
				num = 0;
			}
		}
		if (num2 < StatusEffect_EV.COMMANDER_STATUS_EFFECT_ARRAY.Length)
		{
			AddCommanderStatusEffect(StatusEffect_EV.COMMANDER_STATUS_EFFECT_ARRAY[num]);
		}
	}

	public void InitializeCommanderStatusEffects()
	{
		RemoveAllCommanderStatusEffects();
		if (EnemyType == EnemyType.BouncySpike || EnemyType == EnemyType.Wisp || EnemyType == EnemyType.Slug || EnemyType == EnemyType.Dummy || EnemyType == EnemyType.Target || EnemyType == EnemyType.Eggplant)
		{
			IsCommander = false;
			return;
		}
		if (IsCommander)
		{
			AddRandomCommanderStatusEffect();
			int num = Mathf.RoundToInt(BurdenManager.GetBurdenStatGain(BurdenType.CommanderTraits));
			for (int i = 0; i < num; i++)
			{
				AddRandomCommanderStatusEffect();
			}
		}
		if (IsBoss)
		{
			int num2 = Mathf.RoundToInt(BurdenManager.GetBurdenStatGain(BurdenType.BossPower));
			for (int j = 0; j < num2; j++)
			{
				AddRandomCommanderStatusEffect();
			}
		}
		if (SceneLoader_RL.CurrentScene != SceneLoadingUtility.GetSceneName(SceneID.Tutorial) && !IsCommander && !IsBoss && EnemyRank != EnemyRank.Miniboss && !IsSummoned && EnemyType != EnemyType.Eggplant)
		{
			float burdenStatGain = BurdenManager.GetBurdenStatGain(BurdenType.EnemyAdapt);
			float randomNumber = RNGManager.GetRandomNumber(RngID.SpecialProps_RoomSeed, "EnemyController.EnemyAdapt Burden", 0f, 1f);
			if (randomNumber <= burdenStatGain && randomNumber > 0f)
			{
				AddRandomCommanderStatusEffect();
			}
		}
		StartCoroutine(ApplyCommanderStatusEffects());
		ResetHealth();
	}

	private IEnumerator ApplyCommanderStatusEffects()
	{
		while (!base.IsInitialized)
		{
			yield return null;
		}
		if (IsCommander && !base.StatusBarController.HasActiveStatusBarEntry(StatusBarEntryType.Commander))
		{
			base.StatusBarController.ApplyUIEffect(StatusBarEntryType.Commander);
		}
		if (m_commanderStatusEffectFlags != 0)
		{
			for (int i = 0; i < StatusEffect_EV.COMMANDER_STATUS_EFFECT_ARRAY.Length; i++)
			{
				if ((m_commanderStatusEffectFlags & (1 << i)) != 0)
				{
					StatusEffectType statusEffectType = StatusEffect_EV.COMMANDER_STATUS_EFFECT_ARRAY[i];
					m_statusEffectController.StopStatusEffect(statusEffectType, interrupted: false);
					m_statusEffectController.StartStatusEffect(statusEffectType, 0f, null);
				}
			}
		}
		if (EnemyType == EnemyType.Zombie || IsSummoned || (Room.SpecialRoomType == SpecialRoomType.Fairy && !Room.GetComponent<FairyRoomController>().IsHiddenFairyRoom))
		{
			m_statusEffectController.SetAllStatusEffectsHidden(hide: true);
		}
	}

	protected override void FixedUpdate()
	{
		if (!base.IsInitialized)
		{
			return;
		}
		if (ConstrainToRoom)
		{
			ConstrainEnemyMovementToRoom();
		}
		base.FixedUpdate();
		if (!base.IsDead)
		{
			UpdateFacing();
			if (IsFlying)
			{
				if ((bool)LogicController && LogicController.LogicIsActivated)
				{
					UpdateFlightMovement();
					UpdateFlightAcceleration();
				}
			}
			else
			{
				if (!base.IsGrounded && JumpHorizontalVelocity != 0f && Mathf.Approximately(base.Velocity.x, 0f))
				{
					SetVelocityX(JumpHorizontalVelocity, additive: false);
				}
				if (base.IsGrounded && !base.KnockedIntoAir && GroundHorizontalVelocity != 0f && base.Velocity.x != GroundHorizontalVelocity && ((GroundHorizontalVelocity > 0f && !RightSidePlatformDropPrevented) || (GroundHorizontalVelocity < 0f && !LeftSidePlatformDropPrevented)))
				{
					SetVelocityX(GroundHorizontalVelocity, additive: false);
				}
			}
			if ((bool)LogicController && !LogicController.LogicIsActivated && !base.KnockedIntoAir && !base.ControllerCorgi.RetainVelocity)
			{
				if (base.Velocity.x != 0f)
				{
					SetVelocityX(0f, additive: false);
				}
				if (IsFlying && base.Velocity.y != 0f)
				{
					SetVelocityY(0f, additive: false);
				}
			}
		}
		if ((bool)LogicController && LogicController.enabled)
		{
			LogicController.InternalUpdate();
		}
	}

	protected void UpdateFacing()
	{
		if (AlwaysFacing && (bool)Target && !base.LockFlip && ((base.IsFacingRight && base.transform.localPosition.x > Target.transform.localPosition.x) || (!base.IsFacingRight && base.transform.localPosition.x < Target.transform.localPosition.x)))
		{
			if (PivotFollowsOrientation)
			{
				base.CharacterCorgi.Flip(IgnoreFlipOnDirectionChange: false, ignoreTurnParam: true);
			}
			else
			{
				base.CharacterCorgi.Flip();
			}
		}
	}

	protected void UpdateJumpAcceleration()
	{
		if (!base.IsGrounded && m_previousAirVelocityX != 0f)
		{
			SetVelocityX(m_previousAirVelocityX, additive: false);
		}
		m_previousAirVelocityX = base.Velocity.x;
		if (base.IsGrounded)
		{
			m_previousAirVelocityX = 0f;
		}
	}

	protected void UpdateFlightMovement()
	{
		if (base.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_Freeze) || !Target)
		{
			return;
		}
		Vector3 vector = Target.transform.position;
		if ((bool)m_targetController)
		{
			vector = m_targetController.Midpoint;
		}
		Vector3 followOffset = FollowOffset;
		vector += followOffset;
		if (AlwaysFacing)
		{
			if (FollowTarget)
			{
				base.Orientation = CDGHelper.TurnToFaceRadians(base.Midpoint, vector, CDGHelper.ToRadians(ActualTurnSpeed), base.Orientation, Time.fixedDeltaTime, convertToHalfRadius: false);
			}
			float num = CDGHelper.DistanceBetweenPts(vector, base.Midpoint);
			if (FollowTarget && !DisableDistanceThresholdCheck && num <= 0.1f && !base.LockFlip)
			{
				SetVelocity(0f, 0f, additive: false);
			}
			else if (FlightKnockbackAcceleration != Vector2.zero && FlyingMovementType == FlyingMovementType.Override)
			{
				SetVelocity(0f, 0f, additive: false);
			}
			else if (FlyingMovementType == FlyingMovementType.Towards)
			{
				SetVelocity(base.HeadingX * ActualSpeed, base.HeadingY * ActualSpeed, additive: false);
			}
			else if (FlyingMovementType == FlyingMovementType.Away)
			{
				SetVelocity(base.HeadingX * (0f - ActualSpeed), base.HeadingY * (0f - ActualSpeed), additive: false);
			}
			else if (FlyingMovementType == FlyingMovementType.Stop)
			{
				SetVelocity(0f, 0f, additive: false);
			}
		}
		else
		{
			if (FollowTarget)
			{
				if (FlyingMovementType == FlyingMovementType.Away)
				{
					vector = base.Midpoint - vector;
					vector *= 10f;
				}
				base.Orientation = CDGHelper.TurnToFaceRadians(base.Midpoint, vector, CDGHelper.ToRadians(ActualTurnSpeed), base.Orientation, Time.fixedDeltaTime, convertToHalfRadius: false);
			}
			if (FlyingMovementType == FlyingMovementType.Away || FlyingMovementType == FlyingMovementType.Towards)
			{
				SetVelocity(base.HeadingX * ActualSpeed, base.HeadingY * ActualSpeed, additive: false);
			}
			else if (FlyingMovementType == FlyingMovementType.Stop)
			{
				SetVelocity(0f, 0f, additive: false);
			}
		}
		if (!base.Pivot || !PivotFollowsOrientation)
		{
			return;
		}
		if (AlwaysFacing)
		{
			float currentAngle = CDGHelper.ToDegrees(base.Orientation);
			currentAngle = CDGHelper.GetLockedAngle(currentAngle, 40);
			if (base.IsFacingRight)
			{
				if (currentAngle > 270f)
				{
					currentAngle -= 360f;
				}
			}
			else
			{
				currentAngle -= 180f;
			}
			Vector3 localEulerAngles = base.Pivot.transform.localEulerAngles;
			localEulerAngles.z = currentAngle;
			base.Pivot.transform.localEulerAngles = localEulerAngles;
		}
		else
		{
			float currentAngle2 = CDGHelper.ToDegrees(base.Orientation);
			currentAngle2 = CDGHelper.GetLockedAngle(currentAngle2, 40);
			Vector3 localEulerAngles2 = base.Pivot.transform.localEulerAngles;
			localEulerAngles2.z = currentAngle2;
			base.Pivot.transform.localEulerAngles = localEulerAngles2;
		}
	}

	protected void UpdateFlightAcceleration()
	{
		float num = 15f * Time.fixedDeltaTime * FlightKnockbackDecelerationMod;
		if (base.Velocity == Vector2.zero)
		{
			num *= 1.5f;
		}
		if (FlightKnockbackAcceleration.x > 0f)
		{
			FlightKnockbackAcceleration.x -= num;
		}
		if (FlightKnockbackAcceleration.x < 0f)
		{
			FlightKnockbackAcceleration.x += num;
		}
		if (Mathf.Abs(FlightKnockbackAcceleration.x) < num)
		{
			FlightKnockbackAcceleration.x = 0f;
		}
		if (FlightKnockbackAcceleration.y > 0f)
		{
			FlightKnockbackAcceleration.y -= num;
		}
		if (FlightKnockbackAcceleration.y < 0f)
		{
			FlightKnockbackAcceleration.y += num;
		}
		if (Mathf.Abs(FlightKnockbackAcceleration.y) < num)
		{
			FlightKnockbackAcceleration.y = 0f;
		}
		SetVelocity(FlightKnockbackAcceleration.x, FlightKnockbackAcceleration.y, additive: true);
	}

	public void OrientToTarget()
	{
		float x = Target.transform.position.x - base.Midpoint.x;
		float y = Target.transform.position.y - base.Midpoint.y;
		if (m_targetController != null)
		{
			x = m_targetController.Midpoint.x - base.Midpoint.x;
			y = m_targetController.Midpoint.y - base.Midpoint.y;
		}
		base.Heading = new Vector2(x, y);
	}

	public void GenerateRandomFollowOffset(Vector2 randomXOffsets, Vector2 randomYOffsets)
	{
		float x = UnityEngine.Random.Range(randomXOffsets.x, randomXOffsets.y);
		float y = UnityEngine.Random.Range(randomYOffsets.x, randomYOffsets.y);
		m_followOffset = new Vector3(x, y, 0f);
		if (Target != null)
		{
			Vector3 vector = Target.transform.position;
			if ((bool)m_targetController)
			{
				vector = m_targetController.Midpoint;
			}
			if (vector.x > base.Midpoint.x)
			{
				m_followOffset.x = 0f - m_followOffset.x;
			}
		}
	}

	public void ConstrainEnemyMovementToRoom()
	{
		if (!ConstrainToRoom)
		{
			return;
		}
		Rect collisionBounds = base.CollisionBounds;
		if (collisionBounds.width == 0f || collisionBounds.height == 0f)
		{
			Debug.Log("<color=yellow>Cannot constrain enemy to room.  Bounds width or height is 0.</color>");
			return;
		}
		m_touchingLeftRoomEdge = false;
		m_touchingRightRoomEdge = false;
		m_touchingBottomRoomEdge = false;
		m_touchingTopRoomEdge = false;
		BaseRoom room = Room;
		if ((bool)room)
		{
			Rect boundsRect = room.BoundsRect;
			float num = 0f;
			float num2 = 0f;
			if (collisionBounds.xMin < boundsRect.xMin)
			{
				num = boundsRect.xMin - collisionBounds.xMin;
				m_touchingLeftRoomEdge = true;
			}
			else if (collisionBounds.xMax > boundsRect.xMax)
			{
				num = boundsRect.xMax - collisionBounds.xMax;
				m_touchingRightRoomEdge = true;
			}
			if (collisionBounds.yMin < boundsRect.yMin)
			{
				num2 = boundsRect.yMin - collisionBounds.yMin;
				m_touchingBottomRoomEdge = true;
			}
			else if (collisionBounds.yMax > boundsRect.yMax)
			{
				num2 = boundsRect.yMax - collisionBounds.yMax;
				m_touchingTopRoomEdge = true;
			}
			if (num != 0f || num2 != 0f)
			{
				base.gameObject.transform.position += new Vector3(num, num2);
				UpdateBounds();
			}
		}
	}

	public override float CalculateDamageTaken(IDamageObj damageObj, out CriticalStrikeType critType, out float damageBlocked, float damageOverride = -1f, bool trueDamage = false, bool pureCalculation = true)
	{
		critType = CriticalStrikeType.None;
		if (PlayerManager.GetPlayerController().CurrentHealth <= 0f)
		{
			damageBlocked = 0f;
			return 0f;
		}
		damageBlocked = 0f;
		float num = 1f;
		if (base.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_Invuln))
		{
			return 0f;
		}
		if (base.TakesNoDamage)
		{
			return 0f;
		}
		if (base.StatusEffectController.HasInvulnStack)
		{
			return 0f;
		}
		bool flag = damageObj.gameObject.CompareTag("Hazard");
		if (flag && !IsBoss)
		{
			return Mathf.RoundToInt((float)ActualMaxHealth * 0.2f);
		}
		if (TraitManager.IsTraitActive(TraitType.CantAttack))
		{
			if (EnemyType == EnemyType.Target)
			{
				return 1f;
			}
			return 0f;
		}
		Projectile_RL projectile_RL = damageObj as Projectile_RL;
		float num2 = damageObj.ActualDamage;
		if (damageOverride != -1f)
		{
			num2 = damageOverride;
			if (trueDamage)
			{
				return num2;
			}
		}
		else
		{
			float num3 = damageObj.ActualCritChance;
			if (base.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_Freeze) && num3 < 100f)
			{
				num3 += 100f;
			}
			if (base.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_Vulnerable) && num3 < 100f)
			{
				num3 += 100f;
			}
			BaseStatusEffect statusEffect = base.StatusEffectController.GetStatusEffect(StatusEffectType.Enemy_Combo);
			if ((bool)statusEffect && statusEffect.IsPlaying && statusEffect.TimesStacked >= 15 && num3 < 100f)
			{
				num3 += 100f;
			}
			float num4 = UnityEngine.Random.Range(0f, 1f);
			if (num3 > 0f && num3 >= num4)
			{
				num2 += damageObj.ActualCritDamage;
				if (num3 >= 100f)
				{
					critType = CriticalStrikeType.Guaranteed;
				}
				else
				{
					critType = CriticalStrikeType.Regular;
				}
			}
			if (critType == CriticalStrikeType.Guaranteed && num3 >= 100f)
			{
				float num5 = num3 - 100f;
				num5 *= 0.5f;
				RelicObj relic = SaveManager.PlayerSaveData.GetRelic(RelicType.SuperCritChanceUp);
				num5 += (float)relic.Level * 0.2f;
				if (PlayerManager.GetPlayerController().CharacterClass.ClassType == ClassType.DualBladesClass)
				{
					num5 += 0.1f;
				}
				num5 += RuneLogicHelper.GetSuperCritChanceAdd();
				float num6 = UnityEngine.Random.Range(0f, 1f);
				if (num5 > 0f && num5 >= num6)
				{
					num2 += num2 * (0.35f + RuneLogicHelper.GetSuperCritDamageAdd());
					critType = CriticalStrikeType.Super;
				}
			}
			if ((bool)projectile_RL)
			{
				num += projectile_RL.DamageMod;
			}
			if ((!projectile_RL || !(projectile_RL is DownstrikeProjectile_RL)) && TraitManager.IsTraitActive(TraitType.SkillCritsOnly) && critType != CriticalStrikeType.Guaranteed && critType != CriticalStrikeType.Super)
			{
				num2 = 0f;
			}
		}
		if (critType != 0 && (bool)projectile_RL)
		{
			if (projectile_RL.CastAbilityType == CastAbilityType.Weapon)
			{
				RuneObj rune = RuneManager.GetRune(RuneType.WeaponCritDot);
				if (rune.EquippedLevel > 0)
				{
					float currentStatModTotal_ = rune.CurrentStatModTotal_1;
					base.StatusEffectController.StartStatusEffect(StatusEffectType.Enemy_MagicBreak, currentStatModTotal_, projectile_RL);
				}
				if (critType != CriticalStrikeType.Regular)
				{
					RelicObj relic2 = SaveManager.PlayerSaveData.GetRelic(RelicType.SkillCritBonus);
					if (relic2.Level > 0)
					{
						float duration = 2.5f + 2.5f * (float)(relic2.Level - 1);
						base.StatusEffectController.StartStatusEffect(StatusEffectType.Enemy_MagicBreak, duration, projectile_RL);
					}
				}
			}
			else
			{
				RuneObj rune2 = RuneManager.GetRune(RuneType.MagicCritDot);
				if (rune2.EquippedLevel > 0)
				{
					float currentStatModTotal_2 = rune2.CurrentStatModTotal_1;
					base.StatusEffectController.StartStatusEffect(StatusEffectType.Enemy_ArmorBreak, currentStatModTotal_2, projectile_RL);
				}
			}
		}
		if (!flag)
		{
			if (base.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_ArmorBreak) && (bool)projectile_RL && projectile_RL.CastAbilityType == CastAbilityType.Weapon)
			{
				num += 0.20000005f;
			}
			if (base.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_MagicBreak) && (!projectile_RL || ((bool)projectile_RL && projectile_RL.CastAbilityType != 0)))
			{
				num += 0.20000005f;
			}
			if (base.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_Combo))
			{
				num += 0.02f * (float)base.StatusEffectController.GetStatusEffect(StatusEffectType.Enemy_Combo).TimesStacked;
			}
			RelicObj relic3 = SaveManager.PlayerSaveData.GetRelic(RelicType.BonusDamageOnNextHit);
			if (relic3.Level > 0 && relic3.IntValue >= 6)
			{
				relic3.SetIntValue(0, additive: false);
				float num7 = 0.75f * (float)relic3.Level + 1f;
				BaseCharacterHitResponse.GLYPHS_TO_ADD_TO_ENEMY_DAMAGE_HELPER.Add(RelicType.BonusDamageOnNextHit.ToString());
				num += num7 - 1f;
			}
			if (!IsBoss && EnemyRank != EnemyRank.Miniboss && !damageObj.gameObject.CompareTag("Player"))
			{
				RelicObj relic4 = SaveManager.PlayerSaveData.GetRelic(RelicType.FreeEnemyKill);
				if (relic4.Level > 0)
				{
					int num8 = 6;
					num8 -= relic4.Level - 1;
					if (relic4.IntValue >= num8)
					{
						num2 = CurrentHealth;
						BaseCharacterHitResponse.GLYPHS_TO_ADD_TO_ENEMY_DAMAGE_HELPER.Add(RelicType.FreeEnemyKill.ToString());
						return num2;
					}
				}
			}
			RelicObj relic5 = SaveManager.PlayerSaveData.GetRelic(RelicType.DamageBuffStatusEffect);
			if (relic5.Level > 0 && base.StatusEffectController.HasAnyActiveStatusEffect(includeCommanderBuffs: false) && !damageObj.gameObject.CompareTag("Player"))
			{
				float num9 = (float)relic5.Level * 0.2f;
				num += num9;
				BaseCharacterHitResponse.GLYPHS_TO_ADD_TO_ENEMY_DAMAGE_HELPER.Add(RelicType.DamageBuffStatusEffect.ToString());
			}
			if ((bool)projectile_RL && projectile_RL.CastAbilityType == CastAbilityType.Weapon && !(projectile_RL is DownstrikeProjectile_RL))
			{
				bool flag2 = projectile_RL.StrengthScale > 0f || projectile_RL.MagicScale > 0f;
				RelicObj relic6 = SaveManager.PlayerSaveData.GetRelic(RelicType.NoAttackDamageBonus);
				if (relic6.Level > 0 && flag2 && relic6.IntValue >= Relic_EV.GetRelicMaxStack(RelicType.NoAttackDamageBonus, relic6.Level))
				{
					float num10 = 0.75f;
					num += num10;
					BaseCharacterHitResponse.GLYPHS_TO_ADD_TO_ENEMY_DAMAGE_HELPER.Add(RelicType.NoAttackDamageBonus.ToString());
					relic6.SetIntValue(0, additive: false);
					PlayerManager.GetPlayerController().StartNoAttackDamageBonusTimer();
				}
			}
			RelicObj relic7 = SaveManager.PlayerSaveData.GetRelic(RelicType.RangeDamageBonusCurse);
			if (SaveManager.PlayerSaveData.GetRelic(RelicType.RangeDamageBonusCurse).Level > 0)
			{
				num = ((!(CDGHelper.DistanceBetweenPts(base.Midpoint, PlayerManager.GetPlayerController().Midpoint) >= 9f)) ? (num + 0f) : (num + 0.125f * (float)relic7.Level));
			}
			if (SaveManager.PlayerSaveData.GetRelic(RelicType.DamageNoHitChallenge).Level > 0)
			{
				BaseCharacterHitResponse.GLYPHS_TO_ADD_TO_ENEMY_DAMAGE_HELPER.Add(RelicType.DamageNoHitChallenge.ToString());
			}
			if (base.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_Aggro))
			{
				num -= (float)(base.StatusEffectController.GetStatusEffect(StatusEffectType.Enemy_Aggro).TimesStacked - 1) * 0.15f;
			}
			float insightPlayerDamageMod = GetInsightPlayerDamageMod();
			num += insightPlayerDamageMod - 1f;
		}
		num = Mathf.Clamp(num, 0f, float.MaxValue);
		num2 *= num;
		num2 *= ModeshiftDamageMod;
		return Mathf.Floor(num2);
	}

	protected float GetInsightPlayerDamageMod()
	{
		switch (EnemyType)
		{
		case EnemyType.SpellswordBoss:
			if (ChallengeManager.IsInChallenge || SaveManager.PlayerSaveData.GetInsightState(InsightType.SpellSwordBossCombatBonus) >= InsightState.ResolvedButNotViewed)
			{
				return 1.15f;
			}
			break;
		case EnemyType.DancingBoss:
			if (ChallengeManager.IsInChallenge || SaveManager.PlayerSaveData.GetInsightState(InsightType.DancingBossCombatBonus) >= InsightState.ResolvedButNotViewed)
			{
				return 1.15f;
			}
			break;
		case EnemyType.SkeletonBossA:
		case EnemyType.SkeletonBossB:
			if (ChallengeManager.IsInChallenge || SaveManager.PlayerSaveData.GetInsightState(InsightType.SkeletonBossCombatBonus) >= InsightState.ResolvedButNotViewed)
			{
				return 1.15f;
			}
			break;
		case EnemyType.StudyBoss:
		case EnemyType.MimicChestBoss:
			if (ChallengeManager.IsInChallenge || SaveManager.PlayerSaveData.GetInsightState(InsightType.StudyBossCombatBonus) >= InsightState.ResolvedButNotViewed)
			{
				return 1.15f;
			}
			break;
		case EnemyType.EyeballBoss_Left:
		case EnemyType.EyeballBoss_Right:
		case EnemyType.EyeballBoss_Bottom:
		case EnemyType.EyeballBoss_Middle:
			if (ChallengeManager.IsInChallenge || SaveManager.PlayerSaveData.GetInsightState(InsightType.TowerBossCombatBonus) >= InsightState.ResolvedButNotViewed)
			{
				return 1.15f;
			}
			break;
		case EnemyType.CaveBoss:
			if (ChallengeManager.IsInChallenge || SaveManager.PlayerSaveData.GetInsightState(InsightType.CaveBossCombatBonus) >= InsightState.ResolvedButNotViewed)
			{
				return 1.15f;
			}
			break;
		case EnemyType.TraitorBoss:
		{
			float num2 = 1f;
			if (ChallengeManager.IsInChallenge || SaveManager.PlayerSaveData.GetInsightState(InsightType.TraitorBoss_HPReduce_SpellSwordBoss) >= InsightState.ResolvedButNotViewed)
			{
				num2 += 0.05f;
			}
			if (ChallengeManager.IsInChallenge || SaveManager.PlayerSaveData.GetInsightState(InsightType.TraitorBoss_HPReduce_DancingBoss) >= InsightState.ResolvedButNotViewed)
			{
				num2 += 0.05f;
			}
			if (ChallengeManager.IsInChallenge || SaveManager.PlayerSaveData.GetInsightState(InsightType.TraitorBoss_HPReduce_StudyBoss) >= InsightState.ResolvedButNotViewed)
			{
				num2 += 0.05f;
			}
			return num2;
		}
		case EnemyType.FinalBoss:
		{
			float num = 1f;
			if (ChallengeManager.IsInChallenge || SaveManager.PlayerSaveData.GetInsightState(InsightType.FinalBoss_HPReduce_SkeletonBoss) >= InsightState.ResolvedButNotViewed)
			{
				num += 0.05f;
			}
			if (ChallengeManager.IsInChallenge || SaveManager.PlayerSaveData.GetInsightState(InsightType.FinalBoss_HPReduce_TowerBoss) >= InsightState.ResolvedButNotViewed)
			{
				num += 0.05f;
			}
			if (ChallengeManager.IsInChallenge || SaveManager.PlayerSaveData.GetInsightState(InsightType.FinalBoss_HPReduce_CaveBoss) >= InsightState.ResolvedButNotViewed)
			{
				num += 0.05f;
			}
			return num;
		}
		}
		return 1f;
	}

	protected float GetInsightBossDamageMod()
	{
		switch (EnemyType)
		{
		case EnemyType.SpellswordBoss:
			SaveManager.PlayerSaveData.GetInsightState(InsightType.SpellSwordBossCombatBonus);
			_ = 3;
			return 1f;
		case EnemyType.DancingBoss:
			SaveManager.PlayerSaveData.GetInsightState(InsightType.DancingBossCombatBonus);
			_ = 3;
			return 1f;
		case EnemyType.SkeletonBossA:
		case EnemyType.SkeletonBossB:
			SaveManager.PlayerSaveData.GetInsightState(InsightType.SkeletonBossCombatBonus);
			_ = 3;
			return 1f;
		case EnemyType.StudyBoss:
		case EnemyType.MimicChestBoss:
			SaveManager.PlayerSaveData.GetInsightState(InsightType.StudyBossCombatBonus);
			_ = 3;
			return 1f;
		case EnemyType.EyeballBoss_Left:
		case EnemyType.EyeballBoss_Right:
		case EnemyType.EyeballBoss_Bottom:
		case EnemyType.EyeballBoss_Middle:
			SaveManager.PlayerSaveData.GetInsightState(InsightType.TowerBossCombatBonus);
			_ = 3;
			return 1f;
		case EnemyType.CaveBoss:
			SaveManager.PlayerSaveData.GetInsightState(InsightType.CaveBossCombatBonus);
			_ = 3;
			return 1f;
		case EnemyType.TraitorBoss:
			return 1f;
		case EnemyType.FinalBoss:
			return 1f;
		default:
			return 1f;
		}
	}

	public override void KillCharacter(GameObject killer, bool broadcastEvent)
	{
		if (base.IsDead)
		{
			return;
		}
		if (DisableDeath)
		{
			SetHealth(1f, additive: false, runEvents: true);
			return;
		}
		if (EnemyType == EnemyType.Eggplant)
		{
			SetHealth(ActualMaxHealth, additive: false, runEvents: true);
			return;
		}
		if (EnemyType == EnemyType.Dummy)
		{
			SetHealth(ActualMaxHealth, additive: false, runEvents: true);
			NPCController component = GetComponent<NPCController>();
			DialogueManager.StartNewDialogue(component);
			DialogueManager.AddDialogue(NPCDialogue_EV.GetNPCTitleLocID(component.NPCType), "LOC_ID_DUMMY_DIALOGUE_DEALING_MAX_DAMAGE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
			return;
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		BaseAbility_RL ability = playerController.CastAbility.GetAbility(playerController.CastAbility.LastCastAbilityTypeCasted);
		if ((bool)ability && ability.AbilityType == AbilityType.KnockoutTalent)
		{
			KilledByKnockout = true;
		}
		if (base.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_DeathDelay))
		{
			LogicController.StopAllLogic(resetCurrentState: true);
			base.IsDead = true;
			if (AnimatorUtility.HasParameter(base.Animator, "Stunned"))
			{
				base.Animator.SetBool("Stunned", value: true);
			}
			base.ConditionState = CharacterStates.CharacterConditions.Dead;
			return;
		}
		if (!ChallengeManager.IsInChallenge)
		{
			SaveManager.PlayerSaveData.EnemiesKilled++;
			SaveManager.ModeSaveData.SetEnemiesDefeated(GameModeType.Regular, EnemyType, EnemyRank, 1, additive: true);
		}
		RelicObj relic = SaveManager.PlayerSaveData.GetRelic(RelicType.ResolveCombatChallenge);
		if (relic.Level > 0 && !DisableHPMPBonuses)
		{
			if (IsBoss && EnemyRank != EnemyRank.Miniboss)
			{
				relic.SetIntValue(10, additive: false);
			}
			else
			{
				relic.SetIntValue(1, additive: true);
			}
			if (relic.IntValue >= 10)
			{
				relic.SetIntValue(0, additive: false);
				relic.SetLevel(-1, additive: true);
				SaveManager.PlayerSaveData.GetRelic(RelicType.ResolveCombatChallengeUsed).SetLevel(1, additive: true);
				m_relicChangedEventArgs_STATIC.Initialize(RelicType.ResolveCombatChallengeUsed);
				Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.RelicPurified, this, m_relicChangedEventArgs_STATIC);
				playerController.InitializeHealthMods();
			}
		}
		RelicObj relic2 = SaveManager.PlayerSaveData.GetRelic(RelicType.GoldCombatChallenge);
		if (relic2.Level > 0 && !DisableHPMPBonuses)
		{
			if (IsBoss && EnemyRank != EnemyRank.Miniboss)
			{
				relic2.SetIntValue(15, additive: false);
			}
			else
			{
				relic2.SetIntValue(1, additive: true);
			}
			if (relic2.IntValue >= 15)
			{
				relic2.SetIntValue(0, additive: false);
				relic2.SetLevel(-1, additive: true);
				SaveManager.PlayerSaveData.GetRelic(RelicType.GoldCombatChallengeUsed).SetLevel(1, additive: true);
				m_relicChangedEventArgs_STATIC.Initialize(RelicType.GoldCombatChallengeUsed);
				Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.RelicPurified, this, m_relicChangedEventArgs_STATIC);
				Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.GoldChanged, null, null);
			}
		}
		if (SaveManager.PlayerSaveData.GetRelic(RelicType.SporeburstKillAdd).Level > 0)
		{
			ProjectileManager.FireProjectile(PlayerManager.GetPlayerController().gameObject, "SporeBurstProjectile", base.Midpoint, matchFacing: false, 0f, 1f, useAbsPos: true).CastAbilityType = CastAbilityType.Spell;
		}
		base.KillCharacter(killer, broadcastEvent);
		m_onPreDisableRelay.Dispatch(this);
		SetVelocity(0f, 0f, additive: false);
		if (!killer.CompareTag("Player"))
		{
			killer.CompareTag("PlayerProjectile");
		}
		if (!IsBoss && TraitManager.IsTraitActive(TraitType.ExplosiveEnemies) && EnemyType != EnemyType.Target)
		{
			float angleInDeg = UnityEngine.Random.Range(Trait_EV.EXPLOSIVE_ENEMIES_PROJECTILE_ANGLE.x, Trait_EV.EXPLOSIVE_ENEMIES_PROJECTILE_ANGLE.y);
			ProjectileManager.FireProjectile(base.gameObject, "ExplosiveEnemiesPotionProjectile", base.Midpoint, matchFacing: false, angleInDeg, 1f, useAbsPos: true);
		}
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(DeathCoroutine(killer, broadcastEvent));
		}
	}

	private IEnumerator DeathCoroutine(GameObject killer, bool broadcastEvent)
	{
		LogicController.TriggerDeath();
		if (IsBoss || LogicController.LogicScript.ForceDeathAnimation)
		{
			yield return LogicController.LogicScript.DeathAnim();
		}
		if (broadcastEvent)
		{
			m_enemyDeathEventArgs_STATIC.Initialize(this, killer);
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.EnemyDeath, this, m_enemyDeathEventArgs_STATIC);
			m_onEnemyDeathRelay.Dispatch(this, m_enemyDeathEventArgs_STATIC);
			if (EnemyType != EnemyType.TraitorBoss && EnemyType != EnemyType.FinalBoss)
			{
				m_onDeathEffectTriggerRelay.Dispatch(killer.gameObject);
			}
			DropReward();
		}
		if (EnemyType != EnemyType.TraitorBoss && EnemyType != EnemyType.FinalBoss)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void DropReward()
	{
		if (IsBoss || ChallengeManager.IsInChallenge || !Room.AllowItemDrops || IsSummoned || Room.SpecialRoomType == SpecialRoomType.Fairy)
		{
			return;
		}
		Vector3 midpoint = base.Midpoint;
		midpoint.y += 1f;
		bool largeSpurt = TraitManager.IsTraitActive(TraitType.ItemsGoFlying);
		if (IsCommander)
		{
			int amount = Economy_EV.EXPERT_ENEMY_BASE_ORE_DROP + Mathf.FloorToInt((float)Level * Economy_EV.EXPERT_ENEMY_ORE_SCALING_PER_ENEMY_LEVEL);
			ItemDropManager.DropItem(ItemDropType.EquipmentOre, amount, midpoint, largeSpurt, IsBoss);
		}
		float actualDropOdds = ActualDropOdds;
		if (UnityEngine.Random.Range(0f, 1f) < actualDropOdds)
		{
			int num = UnityEngine.Random.Range(Economy_EV.ENEMY_BASE_GOLD_DROP_AMOUNT.x, Economy_EV.ENEMY_BASE_GOLD_DROP_AMOUNT.x + 1);
			float num2 = UnityEngine.Random.Range(Economy_EV.ENEMY_GOLD_DROP_PER_LEVEL_ADD.x, Economy_EV.ENEMY_GOLD_DROP_PER_LEVEL_ADD.y);
			num2 *= (float)(Level - 1);
			int num3 = (int)EnemyRank;
			if (IsBoss)
			{
				num3 = 4;
			}
			float num4 = Economy_EV.ENEMY_TYPE_GOLD_MOD[num3];
			int newGamePlusLevel = SaveManager.PlayerSaveData.NewGamePlusLevel;
			ItemDropManager.DropGold((int)(((float)num + num2 - (float)newGamePlusLevel) * num4) * Economy_EV.GetItemDropValue(ItemDropType.Coin), midpoint, largeSpurt, IsBoss);
		}
		float num5 = 0f;
		if (UnityEngine.Random.Range(0f, 1f) < num5)
		{
			if (CDGHelper.RandomPlusMinus() > 0)
			{
				ItemDropManager.DropItem(ItemDropType.HealthDrop, 1, midpoint, largeSpurt, IsBoss);
			}
			else
			{
				ItemDropManager.DropItem(ItemDropType.ManaDrop, 1, midpoint, largeSpurt, IsBoss);
			}
		}
		int level = SaveManager.PlayerSaveData.GetRelic(RelicType.EnemiesDropMeat).Level;
		float num6 = 0.08f * (float)level;
		if (num6 > 0f && UnityEngine.Random.Range(0f, 1f) < num6)
		{
			ItemDropManager.DropItem(ItemDropType.HealthDrop, 1, midpoint, largeSpurt, IsBoss);
		}
	}

	public void InjectLevel()
	{
		ILevelConsumer[] componentsInChildren = GetComponentsInChildren<ILevelConsumer>();
		foreach (ILevelConsumer levelConsumer in componentsInChildren)
		{
			if (levelConsumer != this)
			{
				levelConsumer.SetLevel(Level);
			}
		}
	}

	public override void ResetStates()
	{
		base.ResetStates();
		LogicController.ResetLogic();
		FollowOffset = Vector2.zero;
		JumpHorizontalVelocity = 0f;
		LeftSidePlatformDropPrevented = false;
		RightSidePlatformDropPrevented = false;
	}

	public void ResetPivotRotation()
	{
		if ((bool)base.Pivot)
		{
			base.Pivot.transform.eulerAngles = Vector3.zero;
		}
	}

	public void ResetTurnTrigger()
	{
		if ((bool)base.Animator && AnimatorUtility.HasParameter(base.Animator, TURN_ANIMATION_TRIGGER_NAME))
		{
			base.Animator.ResetTrigger(TURN_ANIMATION_TRIGGER_NAME);
		}
	}

	public override void ResetMods()
	{
		base.ResetMods();
		SpeedMod = 0f;
		SpeedAdd = 0f;
	}

	public void ResetPositionForSpawnController(Vector3 spawnPoint, Collider2D spawnCollider)
	{
		base.transform.localPosition = spawnPoint;
		if (base.IsInitialized)
		{
			UpdateBounds();
			ResetCollisionState();
		}
		if ((bool)spawnCollider)
		{
			ForceStandingOn(spawnCollider);
		}
		ForceFaceTarget();
		m_onResetPositionRelay.Dispatch(null, null);
	}

	public void ActivateEnemy()
	{
		LogicController.ResetLogic();
		m_enemyActivationStateChangedEventArgs_STATIC.Initialize(this);
		m_onActivatedRelay.Dispatch(this, m_enemyActivationStateChangedEventArgs_STATIC);
	}

	public void DeactivateEnemy()
	{
		ResetStates();
		if (!PreserveRotationWhenDeactivated)
		{
			ResetPivotRotation();
		}
		ResetTurnTrigger();
		m_enemyActivationStateChangedEventArgs_STATIC.Initialize(this);
		m_onDeactivatedRelay.Dispatch(this, m_enemyActivationStateChangedEventArgs_STATIC);
	}

	public void EnemyTimedOut()
	{
		m_enemyActivationStateChangedEventArgs_STATIC.Initialize(this);
		m_onReactivationTimedOutRelay.Dispatch(this, m_enemyActivationStateChangedEventArgs_STATIC);
	}

	public void SetRoom(BaseRoom room)
	{
		Room = room;
	}

	public void SetEnemyIndex(int index)
	{
		EnemyIndex = index;
	}

	public void ResetValues()
	{
		ResetCharacter();
	}

	public override void ResetHealth()
	{
		SetHealth(ActualMaxHealth, additive: false, runEvents: false);
	}

	public override void ResetCharacter()
	{
		base.ResetCharacter();
		ResetRendererArrayColor();
		base.BlinkPulseEffect.ResetAllBlackFills();
		InitializeLevelMods();
		ResetHealth();
		IsBeingSummoned = false;
		KilledByKnockout = false;
		if (base.IsInitialized)
		{
			DisableDeath = false;
		}
		base.StatusBarController.StopUIEffect(StatusBarEntryType.Commander);
		LastDamageTakenType = DamageType.Strength;
		LastDamageHitCount = 0;
		if (!IsFlying && (bool)base.ControllerCorgi)
		{
			base.ControllerCorgi.GravityActive(state: true);
		}
		GroundHorizontalVelocity = 0f;
		JumpHorizontalVelocity = 0f;
		if (TraitManager.IsTraitActive(TraitType.EnemiesBlackFill))
		{
			base.BlinkPulseEffect.ActivateBlackFill(BlackFillType.EnemiesBlackFill_Trait, 0f);
		}
	}

	[SpecialName]
	GameObject ISummoner.get_gameObject()
	{
		return base.gameObject;
	}

	[SpecialName]
	GameObject IDamageObj.get_gameObject()
	{
		return base.gameObject;
	}

	[SpecialName]
	GameObject IGenericPoolObj.get_gameObject()
	{
		return base.gameObject;
	}
}
