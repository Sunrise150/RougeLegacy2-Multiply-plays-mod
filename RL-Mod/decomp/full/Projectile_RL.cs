using System;
using System.Collections;
using System.Runtime.CompilerServices;
using MoreMountains.CorgiEngine;
using RLAudio;
using Sigtrap.Relays;
using UnityEngine;

public class Projectile_RL : MonoBehaviour, IDamageObj, ITerrainOnEnterHitResponse, IHitResponse, ITerrainOnStayHitResponse, IWeaponOnEnterHitResponse, IWeaponOnStayHitResponse, IBodyOnEnterHitResponse, IBodyOnStayHitResponse, IRootObj, IMidpointObj, IHeading, IEffectVelocity, IEffectTriggerEvent_OnDeath, IEffectTriggerEvent_OnSpawn, IEffectTriggerEvent_OnTimeout, IGenericPoolObj, IPreOnDisable, IPlayHitEffect, IPivotObj
{
	protected enum ProjectileDestructionType
	{
		None,
		TerrainCollision,
		CharacterCollision,
		VoidDashCollision
	}

	private const int REPEAT_HIT_ARRAY_SIZE = 5;

	[Header("General Settings")]
	[Tooltip("The Pivot GO on the projectile. Once set, this usually doesn't need to be changed.")]
	[SerializeField]
	protected GameObject m_pivot;

	[Tooltip("The XML data for the projectile.")]
	[SerializeField]
	[ReadOnlyOnPlay]
	protected ProjectileData m_projectileData;

	[Tooltip("Determines whether the projectile grows or shrinks based off the Owner's scale. NOTE: If the Owner is a BaseCharacterController, it scales off the BaseScaleToOffsetWith.")]
	[SerializeField]
	[ReadOnlyOnPlay]
	private bool m_scaleWithOwner;

	[Tooltip("Parents the Owner to the projectile, so that the projectile follows wherever the Owner goes. If the Owner flips (scale.x == -1) so does the projectile. NOTE: BaseAbility can override the flip by changing its 'Projectile Match Facing' property.")]
	[SerializeField]
	private bool m_snapToOwner;

	[Tooltip("Determines if the projectile gets destroyed when the player exits the room.")]
	[SerializeField]
	private bool m_destroyOnRoomChange = true;

	[Header("Visual Settings")]
	[Tooltip("Determines whether the Visual pivot of the projectile follows the heading it's going in (i.e. does the object visually rotate to match the direction it's going in). NOTE: This field overrides Visual Rotation Speed.")]
	[SerializeField]
	private bool m_pivotFollowsOrientation;

	[Tooltip("How quickly (in degrees per second) the projectile visually spins in the air. Only the Visual pivot spins, so the projectile does not actually spin. Useful for visuals like a spinning bone projectile. NOTE: If you want to rotate the projectile, use the projectile's Rotation transform.")]
	[SerializeField]
	[ConditionalHide("m_pivotFollowsOrientation", true, true)]
	private float m_visualRotationSpeed;

	[Tooltip("We use velocity to determine how effects should occur when two objects interact (for ex. walking past curtains). When a projectile is fired, it uses its own velocity for the effect.  This field uses the Owner's velocity instead.")]
	[SerializeField]
	[ReadOnlyOnPlay]
	private bool m_useOwnerVelocityForEffects;

	[Tooltip("Determines what type of strike effect should play when the projectile hits something.")]
	[SerializeField]
	private StrikeType m_strikeType = StrikeType.Blunt;

	[Tooltip("Determines if Offscreen warnings appear for this projectile.")]
	[SerializeField]
	private bool m_disableOffscreenWarnings;

	[Header("Animation Settings")]
	[SerializeField]
	[Tooltip("Setting this to true means the projectile will not be collidable until its intro has played")]
	private bool m_hasAnimStates;

	[SerializeField]
	[ConditionalHide("m_hasAnimStates", true)]
	private bool m_hasIntro;

	[SerializeField]
	[ConditionalHide("m_hasAnimStates", true)]
	private bool m_playOuttroOnTimeoutOnly;

	[SerializeField]
	[ConditionalHide("m_hasAnimStates", true)]
	private string m_outtroTriggerName;

	[SerializeField]
	[ConditionalHide("m_hasAnimStates", true)]
	private int m_animLayer;

	[SerializeField]
	[Tooltip("Determines whether the projectile keeps moving during its death anim when dashed through")]
	private bool m_keepMovingWhenDashedThrough;

	[Header("Collision Settings")]
	[Tooltip("Does this projectile deal dot damage. I.e. does it deal damage but not trigger invincibility.")]
	[SerializeField]
	private bool m_isDotDamage;

	[Tooltip("Does this projectile deal hazard damage. This will override the projectile data's strength with Hazard_EV.GetDamageAmount().")]
	[SerializeField]
	private bool m_dealsHazardDamage;

	[Tooltip("Used to determine whether we should use the Owner or the projectile as the point of collision. Mostly used for positioning the strike effect and knockback. For ex. if an enemy is knocked back, does he push away from the projectile or the Owner. NOTE: SnapToOwner might be able to replace this field entirely.")]
	[SerializeField]
	[ReadOnlyOnPlay]
	private bool m_useOwnerCollisionPoint = true;

	[Tooltip("Determines whether a projectile can hit its owner or not. 99.9% of the time this is false.")]
	[SerializeField]
	[ReadOnlyOnPlay]
	private bool m_canHitOwner;

	[Tooltip("Determines if the projectile can hit breakables.")]
	[SerializeField]
	[ReadOnlyOnPlay]
	private bool m_canHitBreakables;

	[Tooltip("Determines if the projectile only hits flimsy breakables.")]
	[SerializeField]
	[ConditionalHide("m_canHitBreakables", true)]
	[ReadOnlyOnPlay]
	private bool m_hitsFlimsyBreakablesOnly;

	[Tooltip("Determines if the projectile is allowed to collide with walls.")]
	[SerializeField]
	[ReadOnlyOnPlay]
	private bool m_canHitWall;

	[Tooltip("Determines if the projectile is destroyed when colliding with walls.")]
	[SerializeField]
	[ConditionalHide("m_canHitWall", true)]
	private bool m_dieOnWallCollision;

	[Tooltip("Determines if the projectile is allowed to collide with walls.")]
	[SerializeField]
	[ConditionalHide("m_dieOnWallCollision", true)]
	private bool m_dieOnRoomBoundsCollision;

	[Tooltip("Determines if the projectile is destroyed when colliding with characters.  This includes breakables.")]
	[SerializeField]
	[ReadOnlyOnPlay]
	private bool m_dieOnCharacterCollision;

	[Header("Projectile-on-Projectile Collisions")]
	[Tooltip("The type of projectile I am. This must match the CanCollideWithFlag of the oncoming projectile for a collision to occur.")]
	[SerializeField]
	private ProjectileCollisionFlag m_collisionFlag;

	[Tooltip("Which projectiles I can kill. This must match the CollisionFlag of the oncoming projectile for a collision to occur. Right now only player projectiles can kill other projectiles.")]
	[SerializeField]
	private ProjectileCollisionFlag m_canCollideWithFlag;

	[Tooltip("Determines if the on-hit CD logic has a delay. Only applies to AxeSpinner")]
	[SerializeField]
	private bool m_hasCDReductionDelay;

	[Header("Physics Settings")]
	[Tooltip("Determines if the projectile falls due to gravity or not.")]
	[SerializeField]
	private bool m_isWeighted;

	[Tooltip("The amount of time before gravity kicks in for a projectile. NOTE: This setting only works if 'Is Weighted' == true.")]
	[SerializeField]
	[ConditionalHide("m_isWeighted", true)]
	private float m_gravityKickInDelay;

	[Tooltip("A modifier on how much gravity pushes down on the projectile when it is going up. NOTE: This setting only works if 'Is Weighted' == true.")]
	[SerializeField]
	[ConditionalHide("m_isWeighted", true)]
	private float m_ascentMultiplierOverride = 1f;

	[Tooltip("A modifier on how much gravity pushes down on the projectile when it is going down. NOTE: This setting only works if 'Is Weighted' == true.")]
	[SerializeField]
	[ConditionalHide("m_isWeighted", true)]
	private float m_fallMultiplierOverride = 1f;

	[Tooltip("Determines whether the projectile should keep sliding when it hits the ground or if it should stop. NOTE: This only works if the projectile has a CorgiController attached to control its physics (necessary for projectiles that 'bounce' off walls) and 'Is Weighted' == true.")]
	[SerializeField]
	[ConditionalHide("m_isWeighted", true)]
	private bool m_disableFriction;

	[Header("Ricochet Settings")]
	[Tooltip("Certain enemies knock back the player when the Player's projectile hits the enemy. Set this to false to disable the ricochet logic (For ex. the bow's arrow projectile). NOTE: Affects Player only.")]
	[SerializeField]
	private bool m_canBeRicocheted;

	[Tooltip("If this is on, the projectile will knock back the Player regardless of whether the enemy is set to ricochet the Player or not. Useful for a trait. NOTE: Only works if 'Can Be Ricocheted' == true and affects Player only.")]
	[SerializeField]
	[ConditionalHide("m_canBeRicocheted", true)]
	private bool m_ricochetsOwnerWhenHits;

	[Tooltip("The knockback distance of the ricochet.")]
	[SerializeField]
	[ConditionalHide("m_canBeRicocheted", true)]
	private Vector2 m_ricochetKnockbackDistance = Player_EV.PLAYER_BASE_KNOCKBACK_DISTANCE;

	[Tooltip("The control lockout duration of the ricochet.")]
	[SerializeField]
	[ConditionalHide("m_canBeRicocheted", true)]
	private float m_ricochetLockoutDuration = 0.45f;

	[Header("Misc Settings - Programmers only")]
	[Tooltip("This is purely for artists.  Do not touch.")]
	[SerializeField]
	private bool m_ignoreTrailRendererYScaleMatching;

	protected float m_lifeSpanTimer;

	protected GameObject m_owner;

	protected BaseCharacterController m_ownerController;

	protected IHitboxController m_hbController;

	private Animator m_animator;

	protected GameObject m_collidedObj;

	private PlayerController m_player;

	private CorgiController_RL m_corgiController;

	private SpriteRenderer m_spriteRenderer;

	private Color m_startingColor;

	private bool m_wasSpawned;

	protected Vector2 m_movement;

	protected Vector2 m_heading = new Vector2(1f, 0f);

	protected float m_orientation;

	protected Vector2 m_acceleration;

	protected float m_gravityKickInTimer;

	private Vector3 m_initialScale;

	private Vector3 m_initialRotation;

	private float m_initialRotationSpeed;

	private bool m_flagToDestroy;

	private bool m_ignoreDamageScale;

	private bool m_isDying;

	private bool m_hasVelocityParams;

	private bool m_collisionPointChanged;

	private Action<Projectile_RL, GameObject> m_ricochetPlayer;

	private bool m_detachOnDestruction;

	private BaseProjectileLogic[] m_projectileLogicArray;

	private StatusEffectType[] m_statusEffectTypeArray;

	private float[] m_statusEffectDurationArray;

	private VoidDissolveComponent m_voidDissolve;

	protected ProjectileDestructionType m_destructionType;

	private bool m_hasSearchedForAudioEventEmitter;

	private ProjectileEventEmitter m_audioEventEmitter;

	protected Relay<Projectile_RL, GameObject> m_onCollisionRelay = new Relay<Projectile_RL, GameObject>();

	protected Relay<Projectile_RL, GameObject> m_onSpawnRelay = new Relay<Projectile_RL, GameObject>();

	protected Relay<Projectile_RL, GameObject> m_onDeathRelay = new Relay<Projectile_RL, GameObject>();

	protected Relay<GameObject> m_onDeathEffectTriggerRelay = new Relay<GameObject>();

	protected Relay<GameObject> m_onSpawnEffectTriggerRelay = new Relay<GameObject>();

	protected Relay<GameObject> m_onTimeoutEffectTriggerRelay = new Relay<GameObject>();

	private Relay<IPreOnDisable> m_preOnDisableRelay = new Relay<IPreOnDisable>();

	private WaitForFixedUpdate m_fixedUpdateYield = new WaitForFixedUpdate();

	private Coroutine m_changeCollPointCoroutine;

	private WaitRL_Yield m_canHitOwnerWaitYield;

	private bool m_checkedForRenderer;

	public bool MatchFacing { get; set; }

	public float InitialRotationSpeed => m_initialRotationSpeed;

	public bool IsPersistentProjectile { get; set; }

	public bool HasIntro => m_hasIntro;

	public float CDReducDelay { get; set; }

	public bool HasCDReductionDelay => m_hasCDReductionDelay;

	public Vector2 CollisionPointOffset { get; set; } = Vector2.zero;


	public bool IsFreePoolObj { get; set; }

	public ProjectileCollisionFlag CanCollideWithFlags => m_canCollideWithFlag;

	public ProjectileCollisionFlag CollisionFlags => m_collisionFlag;

	public bool IsAwakeCalled { get; protected set; }

	public ProjectileEventEmitter AudioEventEmitter
	{
		get
		{
			if (!m_audioEventEmitter && !m_hasSearchedForAudioEventEmitter)
			{
				m_audioEventEmitter = GetComponent<ProjectileEventEmitter>();
				m_hasSearchedForAudioEventEmitter = true;
			}
			return m_audioEventEmitter;
		}
	}

	public CastAbilityType CastAbilityType { get; set; }

	public bool DestroyOnRoomChange => m_destroyOnRoomChange;

	public float[] StatusEffectDurations => m_statusEffectDurationArray;

	public StatusEffectType[] StatusEffectTypes => m_statusEffectTypeArray;

	public bool IsDotDamage => m_isDotDamage;

	public bool PlayHitEffect => true;

	public bool PlayDirectionalHitEffect => true;

	public string EffectNameOverride => null;

	public IRelayLink<Projectile_RL, GameObject> OnCollisionRelay => m_onCollisionRelay.link;

	public IRelayLink<Projectile_RL, GameObject> OnSpawnRelay => m_onSpawnRelay.link;

	public IRelayLink<Projectile_RL, GameObject> OnDeathRelay => m_onDeathRelay.link;

	public IRelayLink<GameObject> OnDeathEffectTriggerRelay => m_onDeathEffectTriggerRelay.link;

	public IRelayLink<GameObject> OnSpawnEffectTriggerRelay => m_onSpawnEffectTriggerRelay.link;

	public IRelayLink<GameObject> OnTimeoutEffectTriggerRelay => m_onTimeoutEffectTriggerRelay.link;

	public IRelayLink<IPreOnDisable> PreOnDisableRelay => m_preOnDisableRelay.link;

	public IHitboxController HitboxController => m_hbController;

	public Animator Animator => m_animator;

	public float GravityKickInDelay => m_gravityKickInDelay;

	public float FallMultiplierOverride => m_fallMultiplierOverride;

	public ProjectileData ProjectileData => m_projectileData;

	public float LifespanTimer
	{
		get
		{
			return m_lifeSpanTimer;
		}
		set
		{
			m_lifeSpanTimer = value;
		}
	}

	public StrikeType StrikeType => m_strikeType;

	public Vector2 Velocity
	{
		get
		{
			if (!m_corgiController)
			{
				return m_movement;
			}
			return m_corgiController.Velocity;
		}
	}

	public Vector2 EffectVelocity
	{
		get
		{
			Vector2 result = Vector2.zero;
			if (SnapToOwner && m_useOwnerVelocityForEffects)
			{
				if ((bool)OwnerController)
				{
					result = OwnerController.Velocity;
				}
			}
			else
			{
				result = Heading * Speed;
			}
			if (result.x == 0f)
			{
				if (!IsFlipped)
				{
					result.x = 1f;
				}
				else
				{
					result.x = -1f;
				}
			}
			return result;
		}
	}

	public Vector3 Midpoint
	{
		get
		{
			if ((bool)m_ownerController && UseOwnerCollisionPoint)
			{
				return m_ownerController.Midpoint;
			}
			return base.transform.position;
		}
	}

	public bool ScaleWithOwner
	{
		get
		{
			return m_scaleWithOwner;
		}
		set
		{
			m_scaleWithOwner = value;
		}
	}

	public bool CanBeRicocheted
	{
		get
		{
			return m_canBeRicocheted;
		}
		set
		{
			m_canBeRicocheted = value;
		}
	}

	public bool RicochetsOwnerWhenHits
	{
		get
		{
			return m_ricochetsOwnerWhenHits;
		}
		set
		{
			m_ricochetsOwnerWhenHits = value;
		}
	}

	public GameObject Pivot => m_pivot;

	public bool PivotFollowsOrientation
	{
		get
		{
			return m_pivotFollowsOrientation;
		}
		set
		{
			m_pivotFollowsOrientation = value;
		}
	}

	public virtual bool IgnoreDamageScale
	{
		get
		{
			return m_ignoreDamageScale;
		}
		set
		{
			m_ignoreDamageScale = value;
		}
	}

	public virtual DamageType DamageType => m_projectileData.DamageType;

	public virtual float StrengthScale { get; set; }

	public virtual float MagicScale { get; set; }

	public virtual float CooldownReductionPerHit => m_projectileData.CooldownReductionPerHit;

	public virtual float ManaGainPerHit => m_projectileData.ManaGainPerHit;

	public virtual float BaseStunStrength { get; set; }

	public virtual float ActualStunStrength => BaseStunStrength;

	public virtual float BaseKnockbackStrength { get; set; }

	public virtual float ActualKnockbackStrength => BaseKnockbackStrength;

	public virtual Vector2 ExternalKnockbackMod { get; set; }

	public virtual float Speed { get; set; }

	public virtual float TurnSpeed { get; set; }

	public virtual float RotationSpeed
	{
		get
		{
			return m_visualRotationSpeed;
		}
		set
		{
			m_visualRotationSpeed = value;
		}
	}

	public virtual bool IsWeighted
	{
		get
		{
			return m_isWeighted;
		}
		set
		{
			m_isWeighted = value;
		}
	}

	public virtual bool SnapToOwner
	{
		get
		{
			return m_snapToOwner;
		}
		set
		{
			m_snapToOwner = value;
		}
	}

	public virtual bool CanHitOwner
	{
		get
		{
			return m_canHitOwner;
		}
		set
		{
			m_canHitOwner = value;
		}
	}

	public virtual bool CanHitWall
	{
		get
		{
			return m_canHitWall;
		}
		set
		{
			m_canHitWall = value;
			SetCanHitWall(value);
		}
	}

	public bool DieOnRoomBoundsCollision
	{
		get
		{
			return m_dieOnRoomBoundsCollision;
		}
		set
		{
			m_dieOnRoomBoundsCollision = value;
		}
	}

	public virtual bool CanHitBreakables
	{
		get
		{
			return m_canHitBreakables;
		}
		set
		{
			m_canHitBreakables = value;
		}
	}

	public virtual bool HitsFlimsyBreakablesOnly
	{
		get
		{
			return m_hitsFlimsyBreakablesOnly;
		}
		set
		{
			m_hitsFlimsyBreakablesOnly = value;
		}
	}

	public virtual bool DieOnWallCollision
	{
		get
		{
			if (m_dieOnWallCollision)
			{
				return m_canHitWall;
			}
			return false;
		}
		set
		{
			m_dieOnWallCollision = value;
		}
	}

	public virtual bool DieOnCharacterCollision
	{
		get
		{
			return m_dieOnCharacterCollision;
		}
		set
		{
			m_dieOnCharacterCollision = value;
		}
	}

	public virtual float RepeatHitDuration => m_projectileData.RepeatHitCheckTimer;

	public virtual float Lifespan => m_projectileData.LifeSpan;

	public bool UseOwnerCollisionPoint
	{
		get
		{
			return m_useOwnerCollisionPoint;
		}
		set
		{
			m_useOwnerCollisionPoint = value;
		}
	}

	public bool IsFlipped
	{
		get
		{
			if (SnapToOwner && (bool)Owner)
			{
				if ((bool)OwnerController)
				{
					return !OwnerController.IsFacingRight;
				}
				return Owner.transform.localScale.x < 0f;
			}
			return base.transform.localScale.x < 0f;
		}
	}

	public Vector2 Acceleration
	{
		get
		{
			return m_acceleration;
		}
		set
		{
			m_acceleration = value;
		}
	}

	public float AccelerationX
	{
		get
		{
			return m_acceleration.x;
		}
		set
		{
			m_acceleration.x = value;
		}
	}

	public float AccelerationY
	{
		get
		{
			return m_acceleration.y;
		}
		set
		{
			m_acceleration.y = value;
		}
	}

	public Vector2 Heading
	{
		get
		{
			return m_heading;
		}
		set
		{
			if (m_heading != value)
			{
				m_heading = value;
				m_heading.Normalize();
				m_orientation = Mathf.Atan2(m_heading.y, m_heading.x);
			}
		}
	}

	public float HeadingX
	{
		get
		{
			return m_heading.x;
		}
		set
		{
			if (m_heading.x != value)
			{
				m_heading.x = value;
				m_heading.Normalize();
				m_orientation = Mathf.Atan2(m_heading.y, m_heading.x);
			}
		}
	}

	public float HeadingY
	{
		get
		{
			return m_heading.y;
		}
		set
		{
			if (m_heading.y != value)
			{
				m_heading.y = value;
				m_heading.Normalize();
				m_orientation = Mathf.Atan2(m_heading.y, m_heading.x);
			}
		}
	}

	public float Orientation
	{
		get
		{
			return m_orientation;
		}
		set
		{
			if (m_orientation != value)
			{
				m_orientation = value;
				m_heading.x = Mathf.Cos(m_orientation);
				m_heading.y = Mathf.Sin(m_orientation);
			}
		}
	}

	public GameObject Owner
	{
		get
		{
			return m_owner;
		}
		set
		{
			m_owner = value;
			if ((bool)m_owner)
			{
				m_ownerController = m_owner.GetComponent<BaseCharacterController>();
			}
			else
			{
				m_ownerController = null;
			}
		}
	}

	public BaseCharacterController OwnerController => m_ownerController;

	public float BaseDamage
	{
		get
		{
			if (IgnoreDamageScale)
			{
				return Strength + Magic;
			}
			return Strength * StrengthScale + Magic * MagicScale;
		}
	}

	public virtual float ActualDamage
	{
		get
		{
			if (m_dealsHazardDamage)
			{
				return Hazard_EV.GetDamageAmount(PlayerManager.GetCurrentPlayerRoom()) * ProjectileData.StrengthScale;
			}
			return BaseDamage;
		}
	}

	public virtual float DamageMod { get; set; }

	public virtual float Magic { get; set; }

	public virtual float Strength { get; set; }

	public virtual float ActualCritChance { get; set; }

	public virtual float ActualCritDamage { get; set; }

	public bool DontFireDeathRelay { get; set; }

	public static bool OwnsProjectile(GameObject go, Projectile_RL projectile)
	{
		if ((bool)projectile && !projectile.m_flagToDestroy && projectile.isActiveAndEnabled)
		{
			return projectile.Owner == go;
		}
		return false;
	}

	public static bool CollisionFlagAllowed(Projectile_RL proj1, Projectile_RL proj2)
	{
		if ((proj1.CanCollideWithFlags & proj2.CollisionFlags) == 0)
		{
			return (proj2.CanCollideWithFlags & proj1.CollisionFlags) != 0;
		}
		return true;
	}

	public void ForceDispatchOnCollisionRelay(GameObject otherObj)
	{
		m_onCollisionRelay.Dispatch(this, otherObj);
	}

	public void SetProjectileData(ProjectileData projData)
	{
		m_projectileData = projData;
	}

	private void OnEnable()
	{
		ProjectileManager.ActiveProjectileCount++;
	}

	private void OnDisable()
	{
		BaseProjectileLogic[] projectileLogicArray = m_projectileLogicArray;
		foreach (BaseProjectileLogic obj in projectileLogicArray)
		{
			obj.enabled = false;
			obj.StopAllCoroutines();
		}
		ProjectileManager.ActiveProjectileCount--;
		if (m_wasSpawned)
		{
			DisablePooledObjectManager.DisablePooledObject(this);
		}
		if ((bool)Animator)
		{
			Animator.WriteDefaultValues();
		}
		StopChangeCollisionPointCoroutine();
	}

	private void Awake()
	{
		if (Application.isPlaying)
		{
			Initialize();
			PointLightController componentInChildren = GetComponentInChildren<PointLightController>();
			if ((bool)componentInChildren)
			{
				componentInChildren.SetLocation(PointLightLocation.Front);
				componentInChildren.UpdateLocation(CameraLayer.Foreground_ORTHO);
			}
			IsAwakeCalled = true;
		}
		m_detachOnDestruction = m_hasAnimStates && !string.IsNullOrEmpty(m_outtroTriggerName);
	}

	private void Start()
	{
		m_player = PlayerManager.GetPlayerController();
		bool disableAllCollisions = m_hbController.DisableAllCollisions;
		bool hitboxActiveState = m_hbController.GetHitboxActiveState(HitboxType.Weapon);
		bool hitboxActiveState2 = m_hbController.GetHitboxActiveState(HitboxType.Body);
		bool hitboxActiveState3 = m_hbController.GetHitboxActiveState(HitboxType.Terrain);
		bool hitboxActiveState4 = m_hbController.GetHitboxActiveState(HitboxType.Platform);
		CameraLayerController component = GetComponent<CameraLayerController>();
		component.SetCameraLayer(CameraLayer.Foreground_ORTHO);
		int subLayer = ProjectileSubLayerManager.GetSubLayer(base.name);
		component.SetSubLayer(subLayer);
		m_hbController.DisableAllCollisions = disableAllCollisions;
		m_hbController.SetHitboxActiveState(HitboxType.Weapon, hitboxActiveState);
		m_hbController.SetHitboxActiveState(HitboxType.Body, hitboxActiveState2);
		m_hbController.SetHitboxActiveState(HitboxType.Terrain, hitboxActiveState3);
		m_hbController.SetHitboxActiveState(HitboxType.Platform, hitboxActiveState4);
	}

	private void Initialize()
	{
		if (!m_ignoreTrailRendererYScaleMatching)
		{
			TrailRenderer[] componentsInChildren = GetComponentsInChildren<TrailRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].widthMultiplier = base.transform.localScale.y;
			}
		}
		m_projectileLogicArray = GetComponents<BaseProjectileLogic>();
		BaseProjectileLogic[] projectileLogicArray = m_projectileLogicArray;
		for (int i = 0; i < projectileLogicArray.Length; i++)
		{
			projectileLogicArray[i].enabled = false;
		}
		m_initialScale = base.transform.localScale;
		m_initialScale.z = Mathf.Clamp(m_initialScale.z, 0f, 1f);
		m_initialRotationSpeed = RotationSpeed;
		if ((bool)Pivot)
		{
			m_initialRotation = Pivot.transform.localEulerAngles;
		}
		else
		{
			m_initialRotation = base.transform.localEulerAngles;
		}
		m_hbController = GetComponentInChildren<IHitboxController>();
		m_hbController.RepeatHitDuration = RepeatHitDuration;
		if (!m_hbController.IsInitialized)
		{
			m_hbController.Initialize();
		}
		InitializeCollisionSettings();
		m_animator = GetComponentInChildren<Animator>();
		if ((bool)m_animator)
		{
			m_hasVelocityParams = AnimatorUtility.HasParameter(m_animator, "xVelocity") && AnimatorUtility.HasParameter(m_animator, "yVelocity");
		}
		m_corgiController = GetComponent<CorgiController_RL>();
		if ((bool)m_corgiController)
		{
			m_corgiController.GravityActive(state: false);
		}
		ResetValues();
	}

	private void InitializeCollisionSettings()
	{
		SetCanHitWall(CanHitWall);
		CollisionType collisionType = m_hbController.WeaponCollidesWithType;
		if (CanHitBreakables)
		{
			if (!HitsFlimsyBreakablesOnly)
			{
				collisionType |= CollisionType.Breakable;
			}
			collisionType |= CollisionType.FlimsyBreakable;
		}
		else
		{
			collisionType &= ~CollisionType.Breakable;
			collisionType &= ~CollisionType.FlimsyBreakable;
		}
		if (!CompareTag("PlayerProjectile"))
		{
			collisionType |= CollisionType.Player_Dodging;
			m_voidDissolve = GetComponent<VoidDissolveComponent>();
		}
		m_hbController.ChangeCanCollideWith(HitboxType.Weapon, collisionType);
		if (CanBeRicocheted && CanHitWall)
		{
			if (m_ricochetPlayer == null)
			{
				m_ricochetPlayer = RicochetPlayer;
			}
			OnCollisionRelay.AddListener(m_ricochetPlayer);
		}
	}

	private void SetCanHitWall(bool value)
	{
		if (m_hbController == null)
		{
			return;
		}
		Collider2D collider = m_hbController.GetCollider(HitboxType.Terrain);
		if ((bool)collider)
		{
			if (value)
			{
				collider.gameObject.layer = 18;
			}
			else
			{
				collider.gameObject.layer = 13;
			}
		}
	}

	private void OnDestroy()
	{
		BaseProjectileLogic[] projectileLogicArray = m_projectileLogicArray;
		foreach (BaseProjectileLogic obj in projectileLogicArray)
		{
			obj.enabled = false;
			obj.StopAllCoroutines();
		}
		m_preOnDisableRelay.Dispatch(this);
		OnCollisionRelay.RemoveListener(m_ricochetPlayer);
	}

	protected virtual void FixedUpdate()
	{
		if (!m_isDying)
		{
			UpdateDeathState();
		}
		if (m_lifeSpanTimer > 0f && !m_isDying)
		{
			m_lifeSpanTimer -= Time.deltaTime;
			if (m_lifeSpanTimer <= 0f)
			{
				Death(null);
			}
		}
		UpdateMovement();
		if (CanHitWall && DieOnWallCollision && DieOnRoomBoundsCollision && !PlayerManager.GetCurrentPlayerRoom().Bounds.Contains(base.transform.position))
		{
			FlagForDestruction();
		}
		if (m_hasVelocityParams)
		{
			Vector2 vector = Heading * Speed;
			if (!IsFlipped)
			{
				m_animator.SetFloat("xVelocity", vector.x);
			}
			else
			{
				m_animator.SetFloat("xVelocity", 0f - vector.x);
			}
			m_animator.SetFloat("yVelocity", vector.y);
		}
	}

	protected virtual bool UpdateDeathState()
	{
		if (m_flagToDestroy)
		{
			m_flagToDestroy = false;
			Death(m_collidedObj);
			return true;
		}
		return false;
	}

	public virtual void SetCorgiVelocity(Vector2 velocity)
	{
		if ((bool)m_corgiController)
		{
			m_corgiController.SetForce(velocity);
		}
	}

	public virtual void SetCorgiVelocity(float x, float y)
	{
		if ((bool)m_corgiController)
		{
			m_corgiController.SetHorizontalForce(x);
			m_corgiController.SetHorizontalForce(y);
		}
	}

	public virtual void UpdateMovement()
	{
		if (TurnSpeed > 0f && (bool)m_player)
		{
			Orientation = CDGHelper.TurnToFaceRadians(Midpoint, m_player.Midpoint, CDGHelper.ToRadians(TurnSpeed), Orientation, Time.deltaTime, convertToHalfRadius: false);
		}
		float radians = Orientation;
		if ((bool)m_corgiController)
		{
			if (m_corgiController.IsInitialized)
			{
				if (m_gravityKickInTimer <= 0f && !m_corgiController.IsGravityActive && IsWeighted)
				{
					m_corgiController.GravityActive(state: true);
				}
				if (m_corgiController.Velocity.x != 0f && m_corgiController.State.IsGrounded && !m_disableFriction)
				{
					m_corgiController.SetHorizontalForce(0f);
				}
				m_movement = m_corgiController.Velocity;
			}
		}
		else
		{
			m_movement = Heading * (Speed * Time.deltaTime);
			if (m_gravityKickInTimer <= 0f && IsWeighted)
			{
				float num = -50f;
				num = ((!((0f - AccelerationY) * Time.deltaTime < m_movement.y)) ? (num * 1f * m_fallMultiplierOverride) : (num * (1f * m_ascentMultiplierOverride)));
				float terminalVelocity = Global_EV.TerminalVelocity;
				AccelerationY += num * Time.deltaTime;
				m_movement.y += AccelerationY * Time.deltaTime;
				if (m_movement.y < terminalVelocity)
				{
					m_movement.y = terminalVelocity;
				}
				radians = Mathf.Atan2(m_movement.y, m_movement.x);
			}
			base.transform.Translate(m_movement, Space.Self);
		}
		if (m_gravityKickInTimer > 0f)
		{
			m_gravityKickInTimer -= Time.deltaTime;
		}
		if (!Pivot)
		{
			return;
		}
		if (!PivotFollowsOrientation && RotationSpeed != 0f)
		{
			Vector3 localEulerAngles = Pivot.transform.localEulerAngles;
			localEulerAngles.z += RotationSpeed * Time.deltaTime;
			Pivot.transform.localEulerAngles = localEulerAngles;
		}
		else
		{
			if (!PivotFollowsOrientation)
			{
				return;
			}
			Vector3 localEulerAngles2 = Pivot.transform.localEulerAngles;
			float num2 = CDGHelper.ToDegrees(CDGHelper.WrapAngleRadians(radians, convertToHalfRadius: false));
			if (MatchFacing)
			{
				if (num2 > 90f && num2 < 270f)
				{
					if (!IsFlipped)
					{
						Flip();
					}
					num2 = (num2 - 180f) * -1f;
				}
				else if (IsFlipped)
				{
					Flip();
				}
			}
			localEulerAngles2.z = num2;
			Pivot.transform.localEulerAngles = localEulerAngles2;
		}
	}

	public virtual void Flip()
	{
		Vector3 localScale = base.transform.localScale;
		localScale.x = 0f - localScale.x;
		base.transform.localScale = localScale;
	}

	public virtual void ResetValues()
	{
		Owner = null;
		Strength = 1f;
		Magic = 1f;
		ActualCritChance = 0f;
		ActualCritDamage = 0f;
		DamageMod = 0f;
		m_wasSpawned = false;
		m_isDying = false;
		MatchFacing = false;
		BaseKnockbackStrength = m_projectileData.KnockbackStrength;
		BaseStunStrength = m_projectileData.StunStrength;
		Speed = m_projectileData.Speed;
		TurnSpeed = m_projectileData.TurnSpeed;
		ExternalKnockbackMod = new Vector2(m_projectileData.KnockbackModX, m_projectileData.KnockbackModY);
		m_flagToDestroy = false;
		m_collidedObj = null;
		m_destructionType = ProjectileDestructionType.None;
		base.transform.localScale = m_initialScale;
		m_lifeSpanTimer = Lifespan;
		m_gravityKickInTimer = m_gravityKickInDelay;
		Acceleration = Vector2.zero;
		Heading = Vector2.zero;
		RotationSpeed = m_initialRotationSpeed;
		if ((bool)Pivot)
		{
			Pivot.transform.localEulerAngles = m_initialRotation;
			if (SnapToOwner)
			{
				base.transform.localEulerAngles = Vector3.zero;
			}
		}
		else
		{
			base.transform.localEulerAngles = m_initialRotation;
		}
		StopAllCoroutines();
		IgnoreDamageScale = false;
		if (m_hbController != null)
		{
			m_hbController.DisableAllCollisions = false;
		}
		if ((bool)m_corgiController && m_corgiController.IsInitialized)
		{
			m_corgiController.ResetState();
		}
		RemoveAllStatusEffects();
		if ((bool)m_corgiController)
		{
			m_corgiController.State.IsCollidingBelow = false;
		}
		ResetSpriteRendererColor();
	}

	public void RemoveAllStatusEffects()
	{
		if (m_statusEffectTypeArray != null && m_statusEffectDurationArray != null)
		{
			for (int i = 0; i < 5; i++)
			{
				m_statusEffectTypeArray[i] = StatusEffectType.None;
				m_statusEffectDurationArray[i] = 0f;
			}
		}
	}

	public void Spawn()
	{
		m_wasSpawned = true;
		m_onSpawnRelay.Dispatch(this, null);
		m_onSpawnEffectTriggerRelay.Dispatch(base.gameObject);
		BaseProjectileLogic[] projectileLogicArray = m_projectileLogicArray;
		for (int i = 0; i < projectileLogicArray.Length; i++)
		{
			projectileLogicArray[i].enabled = true;
		}
		if (!CompareTag("PlayerProjectile") && !m_disableOffscreenWarnings)
		{
			ProjectileManager.AttachOffscreenIconToProjectile(this);
		}
		StartCoroutine(SpawnProjectileCoroutine());
		StopChangeCollisionPointCoroutine();
		if (!UseOwnerCollisionPoint)
		{
			m_changeCollPointCoroutine = StartCoroutine(ChangeCollisionPointCoroutine());
		}
	}

	private IEnumerator SpawnProjectileCoroutine()
	{
		if (m_hasAnimStates && m_hasIntro)
		{
			m_hbController.DisableAllCollisions = true;
			int currentAnimatorHash = m_animator.GetCurrentAnimatorStateInfo(m_animLayer).shortNameHash;
			while (m_animator.GetCurrentAnimatorStateInfo(m_animLayer).shortNameHash == currentAnimatorHash)
			{
				yield return null;
			}
			m_hbController.DisableAllCollisions = false;
		}
	}

	private IEnumerator ChangeCollisionPointCoroutine()
	{
		m_collisionPointChanged = true;
		UseOwnerCollisionPoint = true;
		yield return m_fixedUpdateYield;
		m_collisionPointChanged = false;
		UseOwnerCollisionPoint = false;
	}

	public void StopChangeCollisionPointCoroutine()
	{
		if (m_collisionPointChanged)
		{
			if (m_changeCollPointCoroutine != null)
			{
				StopCoroutine(m_changeCollPointCoroutine);
			}
			m_collisionPointChanged = false;
			UseOwnerCollisionPoint = false;
		}
	}

	protected virtual void Death(GameObject attacker)
	{
		m_isDying = true;
		if (!DontFireDeathRelay)
		{
			m_onDeathRelay.Dispatch(this, attacker);
		}
		if (m_lifeSpanTimer <= 0f)
		{
			m_onTimeoutEffectTriggerRelay.Dispatch(base.gameObject);
		}
		else if ((bool)attacker)
		{
			m_onDeathEffectTriggerRelay.Dispatch(attacker);
		}
		else
		{
			m_onDeathEffectTriggerRelay.Dispatch(base.gameObject);
		}
		if (m_hbController != null)
		{
			m_hbController.DisableAllCollisions = true;
		}
		StartCoroutine(DestroyProjectileCoroutine(animate: true));
	}

	private IEnumerator CanHitSelfCoroutine()
	{
		yield break;
	}

	protected virtual IEnumerator DestroyProjectileCoroutine(bool animate = false)
	{
		if (SaveManager.PlayerSaveData.GetRelic(RelicType.SpellsDamageCloud).Level > 0 && CompareTag("PlayerProjectile") && CastAbilityType == CastAbilityType.Spell)
		{
			ProjectileManager.FireProjectile(Owner, "SuperFartProjectile", base.transform.position, matchFacing: false, 0f, 1f, useAbsPos: true);
		}
		if (m_destructionType != ProjectileDestructionType.VoidDashCollision || (m_destructionType == ProjectileDestructionType.VoidDashCollision && !m_keepMovingWhenDashedThrough))
		{
			SetCorgiVelocity(Vector2.zero);
			Speed = 0f;
		}
		if ((bool)m_voidDissolve && ((CollisionFlags & ProjectileCollisionFlag.VoidDashable) != 0 || (CollisionFlags & ProjectileCollisionFlag.WeaponStrikeable) != 0))
		{
			yield return m_voidDissolve.DissolveCoroutine(m_destructionType == ProjectileDestructionType.VoidDashCollision);
		}
		if (m_hasAnimStates && !string.IsNullOrEmpty(m_outtroTriggerName) && animate && (!m_playOuttroOnTimeoutOnly || (m_playOuttroOnTimeoutOnly && m_lifeSpanTimer <= 0f)))
		{
			if ((bool)base.gameObject.transform.parent)
			{
				base.gameObject.transform.SetParent(null);
			}
			int currentAnimatorHash2 = m_animator.GetCurrentAnimatorStateInfo(m_animLayer).shortNameHash;
			m_animator.SetTrigger(m_outtroTriggerName);
			while (m_animator.GetCurrentAnimatorStateInfo(m_animLayer).shortNameHash == currentAnimatorHash2)
			{
				yield return null;
			}
			currentAnimatorHash2 = m_animator.GetCurrentAnimatorStateInfo(m_animLayer).shortNameHash;
			while (true)
			{
				AnimatorStateInfo currentAnimatorStateInfo = m_animator.GetCurrentAnimatorStateInfo(m_animLayer);
				if (currentAnimatorStateInfo.shortNameHash != currentAnimatorHash2 || currentAnimatorStateInfo.normalizedTime >= 1f)
				{
					break;
				}
				yield return null;
			}
		}
		m_preOnDisableRelay.Dispatch(this);
		base.gameObject.SetActive(value: false);
		yield return null;
	}

	public void WeaponOnStayHitResponse(IHitboxController otherHBController)
	{
		WeaponOnEnterHitResponse(otherHBController);
	}

	public virtual void WeaponOnEnterHitResponse(IHitboxController otherHBController)
	{
		if (CollisionType_RL.IsProjectile(otherHBController.RootGameObject))
		{
			Projectile_RL component = otherHBController.RootGameObject.GetComponent<Projectile_RL>();
			if (!component || !CollisionFlagAllowed(this, component))
			{
				return;
			}
		}
		if (Owner == otherHBController.RootGameObject && !CanHitOwner)
		{
			return;
		}
		m_onCollisionRelay.Dispatch(this, otherHBController.RootGameObject);
		if (DieOnCharacterCollision)
		{
			bool flag = true;
			if (otherHBController.CollisionType == CollisionType.FlimsyBreakable)
			{
				flag = false;
			}
			if (flag)
			{
				m_collidedObj = otherHBController.RootGameObject;
				FlagForDestruction();
				m_destructionType = ProjectileDestructionType.CharacterCollision;
				if (otherHBController.RootGameObject.CompareTag("Player") && PlayerManager.GetPlayerController().CharacterDash.IsDashing && SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.UnlockVoidDash) > 0)
				{
					m_destructionType = ProjectileDestructionType.VoidDashCollision;
				}
			}
		}
		if ((bool)Owner && (bool)PlayerManager.GetPlayerController())
		{
			GameObject rootGameObject = otherHBController.RootGameObject;
			EnemyController component2 = rootGameObject.GetComponent<EnemyController>();
			if ((bool)component2 && (component2.RicochetsAttackerOnHit || RicochetsOwnerWhenHits) && CanBeRicocheted)
			{
				RicochetPlayer(null, rootGameObject);
			}
		}
		PerformHitEffectCheck(otherHBController);
	}

	protected void RicochetPlayer(Projectile_RL projectile, GameObject colliderObj)
	{
		if (!colliderObj.CompareTag("Enemy") && !colliderObj.CompareTag("Platform"))
		{
			return;
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.CharacterMove.DisableMovement(m_ricochetLockoutDuration);
		playerController.KnockedIntoAir = true;
		playerController.StopActiveAbilities(stopPersistentAbilities: false);
		playerController.MovementState = CharacterStates.MovementStates.Jumping;
		playerController.CharacterJump.ResetBrakeForce();
		playerController.Animator.SetTrigger("AirJump");
		if (colliderObj.CompareTag("Platform"))
		{
			Vector3 vector = Midpoint + (Vector3)CollisionPointOffset;
			if ((bool)HitboxController.LastCollidedWith)
			{
				vector = HitboxController.LastCollidedWith.ClosestPoint(vector);
			}
			if (vector.x < playerController.Midpoint.x)
			{
				playerController.SetVelocity(m_ricochetKnockbackDistance.x, m_ricochetKnockbackDistance.y, additive: false);
			}
			else
			{
				playerController.SetVelocity(0f - m_ricochetKnockbackDistance.x, m_ricochetKnockbackDistance.y, additive: false);
			}
		}
		else if (colliderObj.transform.position.x < playerController.Midpoint.x)
		{
			playerController.SetVelocity(m_ricochetKnockbackDistance.x, m_ricochetKnockbackDistance.y, additive: false);
		}
		else
		{
			playerController.SetVelocity(0f - m_ricochetKnockbackDistance.x, m_ricochetKnockbackDistance.y, additive: false);
		}
	}

	public void TerrainOnStayHitResponse(IHitboxController otherHBController)
	{
		TerrainOnEnterHitResponse(otherHBController);
	}

	public void TerrainOnEnterHitResponse(IHitboxController otherHBController)
	{
		if (CanHitWall)
		{
			if (otherHBController == null)
			{
				m_onCollisionRelay.Dispatch(this, HitboxController.LastCollidedWith.gameObject);
				m_collidedObj = HitboxController.LastCollidedWith.gameObject;
			}
			else
			{
				m_onCollisionRelay.Dispatch(this, otherHBController.RootGameObject);
				m_collidedObj = otherHBController.RootGameObject;
			}
			if (DieOnWallCollision)
			{
				m_destructionType = ProjectileDestructionType.TerrainCollision;
				FlagForDestruction();
			}
			PerformHitEffectCheck(otherHBController);
		}
	}

	public void BodyOnStayHitResponse(IHitboxController otherHBController)
	{
		BodyOnEnterHitResponse(otherHBController);
	}

	public void BodyOnEnterHitResponse(IHitboxController otherHBController)
	{
		Projectile_RL projectile_RL = null;
		if (CollisionType_RL.IsProjectile(otherHBController.RootGameObject))
		{
			projectile_RL = otherHBController.RootGameObject.GetComponent<Projectile_RL>();
			if (!projectile_RL || !CollisionFlagAllowed(this, projectile_RL))
			{
				return;
			}
		}
		m_onCollisionRelay.Dispatch(this, otherHBController.RootGameObject);
		if (DieOnCharacterCollision || ((bool)projectile_RL && (projectile_RL.CanCollideWithFlags & (ProjectileCollisionFlag.ReflectWeak | ProjectileCollisionFlag.ReflectStrong)) != 0 && !CompareTag("PlayerProjectile")))
		{
			FlagForDestruction();
			if (otherHBController != null)
			{
				m_collidedObj = otherHBController.RootGameObject;
			}
			else
			{
				m_collidedObj = HitboxController.LastCollidedWith.gameObject;
			}
			otherHBController.RemoveFromRepeatHitChecks(base.gameObject);
		}
	}

	public void PerformHitEffectCheck(IHitboxController otherHBController)
	{
		Vector3 vector = Midpoint + (Vector3)CollisionPointOffset;
		if ((bool)HitboxController.LastCollidedWith)
		{
			vector = HitboxController.LastCollidedWith.ClosestPoint(vector);
		}
		IPlayHitEffect playHitEffect = otherHBController?.RootGameObject.GetComponent<IPlayHitEffect>();
		bool flag = playHitEffect.IsNativeNull();
		if (otherHBController == null || (!flag && playHitEffect.PlayHitEffect))
		{
			if (!flag)
			{
				EffectManager.PlayHitEffect(this, vector, playHitEffect.EffectNameOverride, StrikeType);
			}
			else
			{
				EffectManager.PlayHitEffect(this, vector, null, StrikeType);
			}
		}
		if (!flag && playHitEffect.PlayDirectionalHitEffect)
		{
			EffectManager.PlayDirectionalHitEffect(this, HitboxController.RootGameObject, vector);
		}
	}

	public void FlagForDestruction(GameObject collidedObj = null)
	{
		if (base.gameObject.activeSelf)
		{
			m_collidedObj = collidedObj;
			m_flagToDestroy = true;
			if (m_detachOnDestruction && (!m_playOuttroOnTimeoutOnly || (m_playOuttroOnTimeoutOnly && m_lifeSpanTimer <= 0f)) && (bool)base.gameObject.transform.parent)
			{
				base.gameObject.transform.SetParent(null);
			}
		}
	}

	public void AttachStatusEffect(StatusEffectType type, float duration = 0f)
	{
		if (m_statusEffectTypeArray == null)
		{
			m_statusEffectTypeArray = new StatusEffectType[5];
			m_statusEffectDurationArray = new float[5];
			for (int i = 0; i < 5; i++)
			{
				m_statusEffectTypeArray[i] = StatusEffectType.None;
				m_statusEffectDurationArray[i] = 0f;
			}
		}
		int num = m_statusEffectTypeArray.IndexOf(type);
		if (num == -1)
		{
			int num2 = m_statusEffectTypeArray.IndexOf(StatusEffectType.None);
			if (num2 != -1)
			{
				m_statusEffectTypeArray[num2] = type;
				m_statusEffectDurationArray[num2] = duration;
				return;
			}
			Debug.Log("<color=yellow>WARNING: Cannot add status effect: " + type.ToString() + " to projectile. Projectile has reached max limit of " + 5 + "</color>");
		}
		else if (m_statusEffectDurationArray[num] < duration)
		{
			m_statusEffectDurationArray[num] = duration;
		}
	}

	public void RemoveStatusEffect(StatusEffectType statusEffect)
	{
		if (m_statusEffectTypeArray != null)
		{
			int num = m_statusEffectTypeArray.IndexOf(statusEffect);
			if (num != -1)
			{
				m_statusEffectTypeArray[num] = StatusEffectType.None;
				m_statusEffectDurationArray[num] = 0f;
			}
		}
	}

	public void ChangeSpriteRendererColor(Color newColor)
	{
		if (!m_checkedForRenderer)
		{
			m_checkedForRenderer = true;
			m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
			if ((bool)m_spriteRenderer)
			{
				m_startingColor = m_spriteRenderer.color;
			}
		}
		if ((bool)m_spriteRenderer)
		{
			m_spriteRenderer.color = newColor;
		}
	}

	public void ResetSpriteRendererColor()
	{
		if ((bool)m_spriteRenderer)
		{
			m_spriteRenderer.color = m_startingColor;
		}
	}

	[SpecialName]
	GameObject IDamageObj.get_gameObject()
	{
		return base.gameObject;
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}

	[SpecialName]
	GameObject IEffectVelocity.get_gameObject()
	{
		return base.gameObject;
	}

	[SpecialName]
	GameObject IGenericPoolObj.get_gameObject()
	{
		return base.gameObject;
	}

	[SpecialName]
	GameObject IPreOnDisable.get_gameObject()
	{
		return base.gameObject;
	}
}
