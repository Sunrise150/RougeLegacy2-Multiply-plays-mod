using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Sigtrap.Relays;
using UnityEngine;

public abstract class BaseItemDrop : MonoBehaviour, ITerrainOnEnterHitResponse, IHitResponse, ITerrainOnStayHitResponse, IEffectTriggerEvent_OnTimeout, IEffectTriggerEvent_OnDeath, IGenericPoolObj, IEffectTriggerEvent_OnLanded
{
	private static ItemCollectedEventArgs m_itemArgs_STATIC;

	private const float MAGNETISM_DELAY_TIMER = 1f;

	[SerializeField]
	private float m_lifetime = 30f;

	protected float m_collectLockDuration = 0.1f;

	private float m_collectLockTimer;

	protected bool m_collected;

	protected CorgiController_RL m_corgiController;

	private WaitRL_Yield m_waitYield;

	protected bool m_hasTouchedGroundOnce;

	private float m_spawnTime;

	private IHitboxController m_hbController;

	private Action<MonoBehaviour, EventArgs> m_onPlayerExitRoom;

	private Relay<GameObject> m_onDeathEffectTriggerRelay = new Relay<GameObject>();

	private Relay<GameObject> m_onTimeoutEffectTriggerRelay = new Relay<GameObject>();

	public Relay CollectedRelay = new Relay();

	private float m_magnetizeStartTime;

	public bool IsFreePoolObj { get; set; }

	public int ValueOverride { get; set; } = -1;


	public bool IsAwakeCalled { get; protected set; }

	public BaseItemDrop ItemDropPrefab { get; set; }

	public bool Magnetized { get; private set; }

	public bool ForceMagnetizedAlways { get; private set; }

	public bool ForceMagnetized { get; private set; }

	public IRelayLink<GameObject> OnDeathEffectTriggerRelay => m_onDeathEffectTriggerRelay.link;

	public IRelayLink<GameObject> OnTimeoutEffectTriggerRelay => m_onTimeoutEffectTriggerRelay.link;

	public IRelayLink<CorgiController_RL> OnLandedEffectTriggerRelay => m_corgiController.OnCorgiLandedEnterRelay;

	public abstract ItemDropType ItemDropType { get; }

	public Vector2 Speed => m_corgiController.Velocity;

	public CorgiController_RL CorgiController => m_corgiController;

	protected void Awake()
	{
		m_corgiController = GetComponent<CorgiController_RL>();
		m_hbController = GetComponentInChildren<IHitboxController>();
		m_waitYield = new WaitRL_Yield(0f);
		if (m_itemArgs_STATIC == null)
		{
			m_itemArgs_STATIC = new ItemCollectedEventArgs(null);
		}
		IsAwakeCalled = true;
		m_onPlayerExitRoom = OnPlayerExitRoom;
		ResetValues();
	}

	protected virtual void Update()
	{
		if (!m_hasTouchedGroundOnce && m_corgiController.State.IsGrounded)
		{
			m_hasTouchedGroundOnce = true;
		}
		UpdateMagnetism();
		if (!Magnetized && m_corgiController.State.IsGrounded && m_corgiController.Velocity.x != 0f)
		{
			m_corgiController.SetHorizontalForce(0f);
		}
		if (m_collectLockTimer > 0f)
		{
			m_collectLockTimer -= Time.deltaTime;
		}
		ConstrainItemDropToRoom();
	}

	private void UpdateMagnetism()
	{
		bool flag = ItemDropType == ItemDropType.HealthDrop || ItemDropType == ItemDropType.PizzaDrop;
		if ((flag && TraitManager.IsTraitActive(TraitType.NoMeat)) || (!ForceMagnetizedAlways && (flag || ItemDropType == ItemDropType.ManaDrop || ItemDropType == ItemDropType.CandyDrop || ItemDropType == ItemDropType.CookieDrop || ItemDropType == ItemDropType.MilkManaDrop)) || !PlayerManager.IsInstantiated)
		{
			return;
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		if (m_hasTouchedGroundOnce || Time.time >= m_spawnTime + 1f)
		{
			float magnetDistance = RuneLogicHelper.GetMagnetDistance();
			if (CDGHelper.DistanceBetweenPts(base.transform.localPosition, playerController.Midpoint) <= magnetDistance)
			{
				if (!Magnetized)
				{
					m_magnetizeStartTime = Time.time;
				}
				Magnetized = true;
			}
			else
			{
				Magnetized = false;
			}
		}
		MagnetizeTowardsPosition(playerController.Midpoint);
	}

	private void MagnetizeTowardsPosition(Vector2 pos)
	{
		if (!(m_corgiController != null))
		{
			return;
		}
		if (Magnetized || ForceMagnetized || ForceMagnetizedAlways)
		{
			if (m_corgiController.IsGravityActive)
			{
				m_corgiController.GravityActive(state: false);
			}
			if (!m_corgiController.DisablePlatformCollision)
			{
				m_corgiController.DisablePlatformCollision = true;
			}
			Vector2 vector = CDGHelper.VectorBetweenPts(base.transform.localPosition, pos);
			vector.Normalize();
			Vector2 force = vector * Economy_EV.ITEM_DROP_ACCELERATION * Time.deltaTime;
			force.x = Mathf.Lerp(0f, vector.x * Economy_EV.ITEM_DROP_ACCELERATION.x, Time.time - m_magnetizeStartTime);
			force.y = Mathf.Lerp(0f, vector.y * Economy_EV.ITEM_DROP_ACCELERATION.y, Time.time - m_magnetizeStartTime);
			force.x = Mathf.Clamp(force.x, 0f - Economy_EV.ITEM_DROP_MAX_MAGNET_SPEED.x, Economy_EV.ITEM_DROP_MAX_MAGNET_SPEED.x);
			force.y = Mathf.Clamp(force.y, 0f - Economy_EV.ITEM_DROP_MAX_MAGNET_SPEED.y, Economy_EV.ITEM_DROP_MAX_MAGNET_SPEED.y);
			m_corgiController.SetForce(force);
		}
		else
		{
			if (!m_corgiController.IsGravityActive)
			{
				m_corgiController.GravityActive(state: true);
			}
			if (m_corgiController.DisablePlatformCollision)
			{
				m_corgiController.DisablePlatformCollision = false;
			}
		}
	}

	private void OnPlayerExitRoom(object sender, EventArgs args)
	{
		base.gameObject.SetActive(value: false);
	}

	protected virtual void ConstrainItemDropToRoom()
	{
		Rect absBounds = CorgiController.AbsBounds;
		if (absBounds.width == 0f || absBounds.height == 0f)
		{
			Debug.Log("<color=yellow>Cannot constrain item to room.  Bounds width or height is 0.</color>");
			return;
		}
		BaseRoom currentPlayerRoom = PlayerManager.GetCurrentPlayerRoom();
		if ((bool)currentPlayerRoom)
		{
			Rect boundsRect = currentPlayerRoom.BoundsRect;
			float num = 0f;
			float num2 = 0f;
			if (absBounds.xMin < boundsRect.xMin)
			{
				num = boundsRect.xMin - absBounds.xMin;
			}
			else if (absBounds.xMax > boundsRect.xMax)
			{
				num = boundsRect.xMax - absBounds.xMax;
			}
			if (absBounds.yMin < boundsRect.yMin)
			{
				ForceMagnetize(forceMagnetizeAlways: true);
				num2 = boundsRect.yMin - absBounds.yMin;
			}
			else if (absBounds.yMax > boundsRect.yMax)
			{
				num2 = boundsRect.yMax - absBounds.yMax;
			}
			if (num != 0f || num2 != 0f)
			{
				base.gameObject.transform.position += new Vector3(num, num2);
				UpdateBounds();
			}
		}
	}

	public void UpdateBounds()
	{
		if ((bool)CorgiController && CorgiController.IsInitialized)
		{
			CorgiController.SetRaysParameters();
		}
	}

	protected virtual void Collect(GameObject collector)
	{
		m_itemArgs_STATIC.Initialize(this);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.ItemCollected, this, m_itemArgs_STATIC);
		CollectedRelay.Dispatch();
		m_collected = true;
		m_onDeathEffectTriggerRelay.Dispatch(collector);
		base.gameObject.SetActive(value: false);
	}

	public virtual void OnSpawnCollectCollisionCheck()
	{
		if (!PlayerManager.IsInstantiated)
		{
			return;
		}
		float y = PlayerManager.GetCurrentPlayerRoom().Bounds.max.y;
		if (base.transform.position.y > y)
		{
			base.transform.position = new Vector3(base.transform.position.x, y, base.transform.position.z);
		}
		int layerMask = (int)CorgiController.PlatformMask & ~(int)CorgiController.OneWayPlatformMask;
		Collider2D collider = m_hbController.GetCollider(HitboxType.Platform);
		if ((bool)Physics2D.OverlapBox(collider.bounds.center, collider.bounds.size, 0f, layerMask))
		{
			if (ItemDropType != ItemDropType.HealthDrop && ItemDropType != ItemDropType.ManaDrop && ItemDropType != ItemDropType.PizzaDrop && ItemDropType != ItemDropType.MushroomDrop && ItemDropType != ItemDropType.CandyDrop && ItemDropType != ItemDropType.CookieDrop && ItemDropType != ItemDropType.MilkManaDrop)
			{
				ForceMagnetize(forceMagnetizeAlways: false);
			}
			else
			{
				base.gameObject.SetActive(value: false);
			}
		}
	}

	public void ForceMagnetize(bool forceMagnetizeAlways)
	{
		if (!ForceMagnetized && !ForceMagnetizedAlways)
		{
			if (forceMagnetizeAlways)
			{
				ForceMagnetizedAlways = true;
			}
			else
			{
				ForceMagnetized = true;
			}
			m_magnetizeStartTime = Time.time;
		}
	}

	public void ForceMagnetizeOnGrounded()
	{
		StartCoroutine(ForceMagnetizeOnGroundCoroutine());
	}

	private IEnumerator ForceMagnetizeOnGroundCoroutine()
	{
		while (!m_hasTouchedGroundOnce)
		{
			yield return null;
		}
		ForceMagnetize(forceMagnetizeAlways: false);
	}

	public void TerrainOnEnterHitResponse(IHitboxController otherHBController)
	{
		if (!m_collected)
		{
			if (otherHBController.CollisionType == CollisionType.Hazard && (bool)otherHBController.RootGameObject.GetComponent<Cloud>())
			{
				ForceMagnetize(forceMagnetizeAlways: true);
			}
			else if (!(m_collectLockTimer > 0f) && (otherHBController.RootGameObject.CompareTag("Player") || otherHBController.RootGameObject.CompareTag("Player_Dodging")))
			{
				Collect(otherHBController.RootGameObject);
			}
		}
	}

	public void TerrainOnStayHitResponse(IHitboxController otherHBController)
	{
		TerrainOnEnterHitResponse(otherHBController);
	}

	public void ResetValues()
	{
		m_collected = false;
		m_collectLockTimer = m_collectLockDuration;
		if ((bool)m_corgiController)
		{
			m_corgiController.GravityActive(state: true);
			m_corgiController.DisablePlatformCollision = false;
		}
		m_magnetizeStartTime = 0f;
		ForceMagnetized = false;
		ForceMagnetizedAlways = false;
		Magnetized = false;
		m_hasTouchedGroundOnce = false;
		ValueOverride = -1;
		StopAllCoroutines();
	}

	protected virtual void OnEnable()
	{
		m_spawnTime = Time.time;
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerExitRoom, m_onPlayerExitRoom);
	}

	protected virtual void OnDisable()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerExitRoom, m_onPlayerExitRoom);
		DisablePooledObjectManager.DisablePooledObject(this);
	}

	[SpecialName]
	GameObject IGenericPoolObj.get_gameObject()
	{
		return base.gameObject;
	}
}
