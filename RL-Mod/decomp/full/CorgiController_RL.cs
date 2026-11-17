using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using Sigtrap.Relays;
using UnityEngine;

[AddComponentMenu("Corgi Engine/Character/Core/Corgi Controller RL")]
public class CorgiController_RL : CorgiController, IRootObj
{
	private static Dictionary<GameObject, int> m_platformRevertDict_STATIC = new Dictionary<GameObject, int>();

	private static int m_ignoreRaycastLayer_STATIC = 2;

	protected Character m_character;

	protected Rect m_absBounds;

	private float m_slopeYOffset;

	private int m_boxColliderLayer;

	private float m_lastHeightStandingOn;

	private float m_heightStandingOn;

	private float m_lastAngleStandingOn;

	private float m_angleStandingOn;

	private bool m_disableOnewayCollision;

	private bool m_disablePlatformCollision;

	private GameObject m_previouslyStandingOn;

	private bool m_prevIsFalling;

	private GameObject m_gameObject;

	private bool m_characterExists;

	private bool m_hittingSlopedWall;

	private bool m_isPlayer;

	private Relay<object, GameObject> m_standingOnChangeRelay = new Relay<object, GameObject>();

	private Relay<CorgiController_RL> m_onCorgiCollisionRelay = new Relay<CorgiController_RL>();

	private Relay<CorgiController_RL> m_onCorgiLandedEnterRelay = new Relay<CorgiController_RL>();

	private Relay<CorgiController_RL> m_onCorgiLandedExitRelay = new Relay<CorgiController_RL>();

	private Relay<CorgiController_RL> m_corgiStartFallingRelay = new Relay<CorgiController_RL>();

	[SerializeField]
	private float m_yRaycastOffset;

	[SerializeField]
	private float m_jumpLeewayAmount;

	[SerializeField]
	private bool m_enablePrecisionPointChecks;

	public bool RetainVelocity;

	public bool UseGlobalEVRaycastSettings = true;

	[Tooltip("Disables IStandingOn check.  Greatly reduces performance hit.")]
	public bool DisableStandingOnColliderLogic = true;

	private List<Collider2D> m_standingOnColliderList;

	private List<Collider2D> m_prevStandingOnColliderList;

	private static List<IStandingOnEnter> m_standingOnEnterHelperList = new List<IStandingOnEnter>();

	private static List<IStandingOnExit> m_standingOnExitHelperList = new List<IStandingOnExit>();

	private static List<IHitboxController> m_hitboxControllerHelper_STATIC = new List<IHitboxController>();

	private float m_lastStandingTimeCheck;

	private const float LAST_STANDING_INTERVAL = 0.5f;

	public IRelayLink<object, GameObject> StandingOnChangeRelay => m_standingOnChangeRelay.link;

	public IRelayLink<CorgiController_RL> OnCorgiCollisionRelay => m_onCorgiCollisionRelay;

	public IRelayLink<CorgiController_RL> OnCorgiLandedEnterRelay => m_onCorgiLandedEnterRelay;

	public IRelayLink<CorgiController_RL> OnCorgiLandedExitRelay => m_onCorgiLandedExitRelay;

	public IRelayLink<CorgiController_RL> CorgiStartFallingRelay => m_corgiStartFallingRelay;

	public bool PermanentlyDisableUponTouchingPlatform { get; set; }

	public bool PermanentlyDisabled { get; private set; }

	public bool DisableCastRaysAbove { get; set; }

	public bool DisableYSlopeOffsetCheck { get; set; }

	protected bool IsFlying
	{
		get
		{
			if ((bool)m_character)
			{
				return m_character.MovementState.CurrentState == CharacterStates.MovementStates.Flying;
			}
			return false;
		}
	}

	public List<Collider2D> StandingOnColliderList => m_standingOnColliderList;

	public List<Collider2D> PreviousStandingOnColliderList => m_prevStandingOnColliderList;

	public Vector3 LastStandingPosition { get; private set; }

	public float RayLengthAdd { get; set; }

	public bool DisableOneWayCollision
	{
		get
		{
			return m_disableOnewayCollision;
		}
		set
		{
			m_disableOnewayCollision = value;
			if (!value)
			{
				PlatformMask = (int)PlatformMask | (int)OneWayPlatformMask;
			}
			else
			{
				PlatformMask = (int)PlatformMask & ~(int)OneWayPlatformMask;
			}
		}
	}

	public bool DisablePlatformCollision
	{
		get
		{
			return m_disablePlatformCollision;
		}
		set
		{
			m_disablePlatformCollision = value;
			if (value)
			{
				CollisionsOff();
			}
			else
			{
				CollisionsOn();
			}
		}
	}

	public List<RaycastHit2D> ContactList => _contactList;

	public Rect AbsBounds => m_absBounds;

	public float BoundsWidth => _boundsWidth;

	public float BoundsHeight => _boundsHeight;

	public BoxCollider2D BoxCollider => _boxCollider;

	public bool IsGravityActive => _gravityActive;

	public void SetLastStandingPosition(Vector3 pos)
	{
		LastStandingPosition = pos;
		m_lastStandingTimeCheck = Time.time + 0.5f;
	}

	protected override void Awake()
	{
		base.Awake();
		_transform = base.transform;
		m_gameObject = base.gameObject;
		_contactList = new List<RaycastHit2D>();
		base.State = new CorgiControllerState();
		if (!DisableOneWayCollision)
		{
			PlatformMask = (int)PlatformMask | (int)OneWayPlatformMask;
		}
		m_standingOnColliderList = new List<Collider2D>();
		m_prevStandingOnColliderList = new List<Collider2D>();
		base.CurrentWallCollider = null;
		base.State.Reset();
		m_character = GetComponent<Character>();
		m_characterExists = m_character;
		DefaultParameters.Gravity = -50f;
		DefaultParameters.AscentMultiplier = 1f;
		DefaultParameters.FallMultiplier = 1f;
		DefaultParameters.MaximumSlopeAngle = 46f;
		if (UseGlobalEVRaycastSettings)
		{
			StickWhenWalkingDownSlopes = true;
			StickyRaycastLength = 2f;
		}
		m_isPlayer = CompareTag("Player");
	}

	public void InitializeRays()
	{
		if (UseGlobalEVRaycastSettings)
		{
			int num = 6;
			int num2 = 6;
			float x = _boxCollider.bounds.size.x;
			float y = _boxCollider.bounds.size.y;
			float num3 = 0.5f;
			if (CompareTag("Enemy"))
			{
				num3 *= 2f;
			}
			num = Mathf.CeilToInt(y / num3);
			num = Mathf.Clamp(num, 2, 6);
			num2 = Mathf.CeilToInt(x / 0.5f);
			num2 = Mathf.Clamp(num2, 2, 6);
			NumberOfHorizontalRays = num;
			NumberOfVerticalRays = num2;
		}
		_sideHitsStorage = new RaycastHit2D[NumberOfHorizontalRays];
		_belowHitsStorage = new RaycastHit2D[NumberOfVerticalRays];
		_aboveHitsStorage = new RaycastHit2D[NumberOfVerticalRays];
	}

	protected override void Initialization()
	{
		if ((bool)m_gameObject.GetComponent<DecoController>())
		{
			m_hitboxControllerHelper_STATIC.Clear();
			m_gameObject.GetComponentsInChildren(m_hitboxControllerHelper_STATIC);
			foreach (IHitboxController item in m_hitboxControllerHelper_STATIC)
			{
				if (item.RootGameObject == m_gameObject)
				{
					_boxCollider = (BoxCollider2D)item.GetCollider(HitboxType.Platform);
					break;
				}
			}
		}
		else
		{
			IHitboxController componentInChildren = m_gameObject.GetComponentInChildren<IHitboxController>();
			if (componentInChildren != null)
			{
				_boxCollider = (BoxCollider2D)componentInChildren.GetCollider(HitboxType.Platform);
			}
		}
		if (!_boxCollider)
		{
			throw new Exception("Cannot Initialize Object: " + base.name + ". The Corgi Controller MUST have a BoxCollider2D attached to it.");
		}
		_originalColliderSize = _boxCollider.size;
		_originalColliderOffset = _boxCollider.offset;
		SetRaysParameters();
		m_boxColliderLayer = _boxCollider.gameObject.layer;
		InitializeRays();
		base.IsInitialized = true;
	}

	public override void CollisionsOn()
	{
		SavePlatformMask();
		PlatformMask = _platformMaskSave;
		if (!DisableOneWayCollision)
		{
			PlatformMask = (int)PlatformMask | (int)OneWayPlatformMask;
		}
		PlatformMask = (int)PlatformMask | (int)MovingPlatformMask;
		PlatformMask = (int)PlatformMask | (int)MovingOneWayPlatformMask;
		PlatformMask = (int)PlatformMask | (int)MidHeightOneWayPlatformMask;
	}

	public override void CollisionsOff()
	{
		SavePlatformMask();
		PlatformMask = 0;
	}

	public override void SetRaysParameters()
	{
		base.SetRaysParameters();
		m_absBounds.x = _boundsBottomLeftCorner.x;
		m_absBounds.y = _boundsBottomLeftCorner.y;
		m_absBounds.width = _boundsWidth;
		m_absBounds.height = _boundsHeight;
		_boundsCenter = m_absBounds.center;
	}

	protected override void FrameInitialization()
	{
		_boxCollider.gameObject.layer = m_ignoreRaycastLayer_STATIC;
		m_lastHeightStandingOn = m_heightStandingOn;
		m_lastAngleStandingOn = m_angleStandingOn;
		base.FrameInitialization();
		StandingOn = null;
		base.StandingOnCollider = null;
		m_heightStandingOn = float.MaxValue;
		m_angleStandingOn = 0f;
		base.IsWithinJumpLeeway = false;
	}

	protected override void FrameExit()
	{
		_boxCollider.gameObject.layer = m_boxColliderLayer;
		if (_contactList.Count > 0)
		{
			m_onCorgiCollisionRelay.Dispatch(this);
		}
		base.FrameExit();
	}

	protected override void EveryFrame()
	{
		if (Time.deltaTime == 0f)
		{
			return;
		}
		if (PermanentlyDisabled)
		{
			base.enabled = false;
			return;
		}
		bool isCollidingBelow = base.State.IsCollidingBelow;
		Vector2 velocity = _velocity;
		ApplyGravity();
		FrameInitialization();
		m_slopeYOffset = 0f;
		SetRaysParameters();
		if ((int)PlatformMask == 0)
		{
			_transform.Translate(_newPosition, Space.Self);
			base.State.IsCollidingBelow = false;
			return;
		}
		base.ForcesApplied = _velocity;
		if (NumberOfHorizontalRays > 0)
		{
			m_hittingSlopedWall = false;
			if (!IsFlying)
			{
				RunSlopedWallCheck();
			}
			DetermineMovementDirection();
			if (CastRaysOnBothSides || !base.State.WasGroundedLastFrame)
			{
				if (_newPosition.x < 0f)
				{
					CastRaysToTheLeft();
					CastRaysToTheRight();
				}
				else
				{
					CastRaysToTheRight();
					CastRaysToTheLeft();
				}
			}
			else if (_movementDirection == -1f)
			{
				CastRaysToTheLeft();
			}
			else
			{
				CastRaysToTheRight();
			}
		}
		m_platformRevertDict_STATIC.Clear();
		CastRaysBelow();
		foreach (KeyValuePair<GameObject, int> item in m_platformRevertDict_STATIC)
		{
			item.Key.layer = item.Value;
		}
		if (!DisableCastRaysAbove)
		{
			CastRaysAbove();
		}
		_transform.Translate(_newPosition, Space.Self);
		_newPosition.y -= m_slopeYOffset;
		SetRaysParameters();
		ComputeNewSpeed();
		SetStates();
		_externalForce.x = 0f;
		_externalForce.y = 0f;
		if (RetainVelocity)
		{
			_velocity = velocity;
		}
		FrameExit();
		if (isCollidingBelow != base.State.IsCollidingBelow)
		{
			if (base.State.IsCollidingBelow)
			{
				m_onCorgiLandedEnterRelay.Dispatch(this);
			}
			else
			{
				m_onCorgiLandedExitRelay.Dispatch(this);
			}
		}
		if (m_isPlayer)
		{
			UpdateLastSafeStandingPosition();
		}
	}

	private void UpdateLastSafeStandingPosition()
	{
		if (Time.time > m_lastStandingTimeCheck)
		{
			m_lastStandingTimeCheck = Time.time + 0.5f;
			if (base.State.IsGrounded && !Cloud.HittingCloud && (bool)StandingOn && !StandingOn.CompareTag("PlayerProjectile") && !StandingOn.CompareTag("TriggerHazard") && !StandingOn.CompareTag("MagicPlatform") && !StandingOn.CompareTag("Enemy") && (StandingOn.layer == 8 || StandingOn.layer == 9 || StandingOn.layer == 11))
			{
				LastStandingPosition = _transform.position;
			}
		}
	}

	public void ResetPermanentDisable()
	{
		PermanentlyDisabled = false;
		base.enabled = true;
	}

	protected override void ApplyGravity()
	{
		base.ApplyGravity();
		if (_velocity.y < Global_EV.TerminalVelocity)
		{
			_velocity.y = Global_EV.TerminalVelocity;
		}
	}

	protected void RunSlopedWallCheck()
	{
		Vector2 vector = _transform.up;
		Vector2 vector2 = _transform.right;
		float distance = Mathf.Abs(_velocity.x * Time.deltaTime) + _boundsWidth / 2f + HorizontalRayOffset * 2f;
		_horizontalRayCastFromBottom = (_boundsBottomRightCorner + _boundsBottomLeftCorner) / 2f;
		if (!(_newPosition.y < 0f) || base.State.IsGrounded)
		{
			return;
		}
		Vector2 horizontalRayCastFromBottom = _horizontalRayCastFromBottom;
		horizontalRayCastFromBottom += vector * _newPosition.y;
		horizontalRayCastFromBottom += vector2 * _newPosition.x;
		RaycastHit2D raycastHit2D = Physics2D.Raycast(horizontalRayCastFromBottom, vector2, distance, PlatformMask);
		if (!raycastHit2D || raycastHit2D.distance != 0f)
		{
			return;
		}
		Vector2 direction = horizontalRayCastFromBottom - _horizontalRayCastFromBottom;
		float distance2 = Mathf.Abs(direction.magnitude);
		direction.Normalize();
		RaycastHit2D item = Physics2D.Raycast(_horizontalRayCastFromBottom, direction, distance2, PlatformMask);
		float num = Mathf.Abs(Vector2.Angle(item.normal, vector));
		float num2 = num;
		if (Vector3.Cross(vector, item.normal).z < 0f)
		{
			num2 = 0f - num;
		}
		if (num > base.Parameters.MaximumSlopeAngle && num < 90f && item.distance > 0f)
		{
			base.State.LateralSlopeAngle = num2;
			base.CurrentWallCollider = item.collider.gameObject;
			base.State.SlopeAngleOK = false;
			float num3 = _boundsWidth / 2f + HorizontalRayOffset * 2f;
			float num4 = CDGHelper.VectorToAngle(new Vector2(direction.x, 0f - direction.y));
			if (num4 > 90f)
			{
				num4 = 180f - num4;
			}
			float num5 = item.point.y - horizontalRayCastFromBottom.y;
			float num6 = Mathf.Tan(num * ((float)Math.PI / 180f));
			float num7 = Mathf.Tan(num4 * ((float)Math.PI / 180f));
			float num8 = num5 / num6;
			float num9 = num5 / num7;
			float num10 = num8 + num9;
			num10 += num3;
			if (num2 < 0f)
			{
				_newPosition.x = num10;
				base.State.IsCollidingLeft = true;
				base.State.DistanceToLeftCollider = 0f;
			}
			else
			{
				_newPosition.x = 0f - num10;
				base.State.IsCollidingRight = true;
				base.State.DistanceToRightCollider = 0f;
			}
			if (!base.State.IsGrounded && base.Velocity.y > 0f)
			{
				_newPosition.x = 0f;
			}
			_contactList.Add(item);
			_velocity.x = 0f;
			m_hittingSlopedWall = true;
		}
	}

	protected override void CastRaysToTheSides(float raysDirection)
	{
		Vector2 vector = _transform.up;
		Vector2 vector2 = _transform.right;
		_horizontalRayCastFromBottom = (_boundsBottomRightCorner + _boundsBottomLeftCorner) / 2f;
		_horizontalRayCastToTop = (_boundsTopLeftCorner + _boundsTopRightCorner) / 2f;
		_horizontalRayCastFromBottom += vector * 0.05f;
		_horizontalRayCastToTop -= vector * 0.05f;
		if (m_hittingSlopedWall)
		{
			_horizontalRayCastFromBottom += vector * _newPosition.y;
			_horizontalRayCastToTop += vector * _newPosition.y;
		}
		float num = Mathf.Abs(_velocity.x * Time.deltaTime) + _boundsWidth / 2f + HorizontalRayOffset * 2f;
		float num2 = float.MaxValue;
		if (_sideHitsStorage.Length != NumberOfHorizontalRays)
		{
			_sideHitsStorage = new RaycastHit2D[NumberOfHorizontalRays];
		}
		for (int i = 0; i < NumberOfHorizontalRays; i++)
		{
			Vector2 zero = Vector2.zero;
			zero = ((NumberOfHorizontalRays <= 1) ? Vector2.Lerp(_horizontalRayCastFromBottom, _horizontalRayCastToTop, 0.5f) : Vector2.Lerp(_horizontalRayCastFromBottom, _horizontalRayCastToTop, (float)i / (float)(NumberOfHorizontalRays - 1)));
			_sideHitsStorage[i] = Physics2D.Raycast(zero, raysDirection * vector2, num, (int)PlatformMask & ~(int)OneWayPlatformMask & ~(int)MovingOneWayPlatformMask);
			if (!_sideHitsStorage[i] || !(_sideHitsStorage[i].distance < num2) || _sideHitsStorage[i].collider == _ignoredCollider)
			{
				continue;
			}
			float num3 = Mathf.Abs(Vector2.Angle(_sideHitsStorage[i].normal, vector));
			if (_sideHitsStorage[i].distance <= 0f && base.State.IsGrounded)
			{
				continue;
			}
			if (_movementDirection == raysDirection || _velocity.x == 0f)
			{
				base.State.LateralSlopeAngle = num3;
			}
			if (!(num3 > base.Parameters.MaximumSlopeAngle))
			{
				continue;
			}
			float num4 = _boundsWidth / 2f + HorizontalRayOffset * 2f;
			if (raysDirection < 0f)
			{
				base.State.IsCollidingLeft = true;
				base.State.DistanceToLeftCollider = _sideHitsStorage[i].distance;
			}
			else
			{
				base.State.IsCollidingRight = true;
				base.State.DistanceToRightCollider = _sideHitsStorage[i].distance;
			}
			float num5 = Mathf.Abs(CDGHelper.ToDegrees(Mathf.Atan2(_velocity.y, Mathf.Abs(_velocity.x))));
			if (_sideHitsStorage[i].distance != 0f && _movementDirection != raysDirection && _velocity.x != 0f && (_movementDirection == raysDirection || !base.State.IsFalling || !(num5 >= num3)))
			{
				continue;
			}
			base.CurrentWallCollider = _sideHitsStorage[i].collider.gameObject;
			base.State.SlopeAngleOK = false;
			num2 = _sideHitsStorage[i].distance;
			float num6 = MMMaths.DistanceBetweenPointAndLine(_sideHitsStorage[i].point, _horizontalRayCastFromBottom, _horizontalRayCastToTop);
			if (_sideHitsStorage[i].distance == 0f)
			{
				float num7 = Mathf.Clamp(_boundsWidth / 2f, 0.5f, float.MaxValue);
				float distance = num + num7;
				Vector2 origin;
				if (raysDirection == -1f)
				{
					origin = zero;
					origin.x += num7;
				}
				else
				{
					origin = zero;
					origin.x -= num7;
				}
				_sideHitsStorage[i] = Physics2D.Raycast(origin, raysDirection * vector2, distance, (int)PlatformMask & ~(int)OneWayPlatformMask & ~(int)MovingOneWayPlatformMask);
				if (!_sideHitsStorage[i] || _sideHitsStorage[i].distance == 0f)
				{
					continue;
				}
				num6 = (num7 * 2f - _sideHitsStorage[i].distance) * -1f;
				num4 = 0f;
			}
			if (raysDirection <= 0f)
			{
				_newPosition.x = 0f - num6 + num4;
			}
			else
			{
				_newPosition.x = num6 - num4;
			}
			if (!base.State.IsGrounded && base.Velocity.y > 0f)
			{
				_newPosition.x = 0f;
			}
			_contactList.Add(_sideHitsStorage[i]);
			_velocity.x = 0f;
		}
	}

	public void ResetMovementState()
	{
		if (base.State != null)
		{
			base.State.Reset();
		}
		_velocity = Vector2.zero;
		CastRaysBelow();
	}

	protected override void CastRaysAbove()
	{
		if (_newPosition.y < 0f)
		{
			return;
		}
		float num = (base.State.IsGrounded ? HorizontalRayOffset : _newPosition.y);
		num += _boundsHeight / 2f;
		bool flag = false;
		Vector2 direction = _transform.up;
		Vector2 vector = _transform.right;
		_aboveRayCastStart = (_boundsBottomLeftCorner + _boundsTopLeftCorner) / 2f;
		_aboveRayCastEnd = (_boundsBottomRightCorner + _boundsTopRightCorner) / 2f;
		_aboveRayCastStart += vector * _newPosition.x;
		_aboveRayCastEnd += vector * _newPosition.x;
		_aboveRayCastStart.y += m_slopeYOffset;
		_aboveRayCastEnd.y += m_slopeYOffset;
		if (_aboveHitsStorage.Length != NumberOfVerticalRays)
		{
			_aboveHitsStorage = new RaycastHit2D[NumberOfVerticalRays];
		}
		float num2 = float.MaxValue;
		int num3 = 0;
		for (int i = 0; i < NumberOfVerticalRays; i++)
		{
			Vector2 zero = Vector2.zero;
			zero = ((NumberOfVerticalRays <= 1) ? Vector2.Lerp(_aboveRayCastStart, _aboveRayCastEnd, 0.5f) : Vector2.Lerp(_aboveRayCastStart, _aboveRayCastEnd, (float)i / (float)(NumberOfVerticalRays - 1)));
			_aboveHitsStorage[i] = Physics2D.Raycast(zero, direction, num, (int)PlatformMask & ~(int)OneWayPlatformMask & ~(int)MovingOneWayPlatformMask);
			if ((bool)_aboveHitsStorage[i] && !(_aboveHitsStorage[i].distance <= 0f))
			{
				flag = true;
				if (_aboveHitsStorage[i].collider == _ignoredCollider)
				{
					break;
				}
				if (_aboveHitsStorage[i].distance < num2)
				{
					num2 = _aboveHitsStorage[i].distance;
					num3 = i;
				}
			}
		}
		if (flag)
		{
			if (!base.State.IsGrounded)
			{
				_newPosition.y = num2 - _boundsHeight / 2f;
			}
			if (base.State.IsGrounded && _newPosition.y < 0f)
			{
				_newPosition.y = 0f;
			}
			base.State.IsCollidingAbove = true;
			if (!base.State.WasTouchingTheCeilingLastFrame)
			{
				_velocity = new Vector2(_velocity.x, 0f);
			}
			_contactList.Add(_aboveHitsStorage[num3]);
			if (m_isPlayer && base.State.IsCollidingBelow && _velocity.y > 0.15f && (bool)base.StandingOnCollider && base.StandingOnCollider.gameObject.layer == 11)
			{
				StartCoroutine(DisableCollisionsWithOneWayPlatforms(0.1f));
			}
		}
	}

	protected override void CastRaysBelow()
	{
		_friction = 0f;
		if (_newPosition.y < -0.0001f)
		{
			base.State.IsFalling = true;
		}
		else
		{
			base.State.IsFalling = false;
		}
		if (m_prevIsFalling != base.State.IsFalling)
		{
			if (base.State.IsFalling)
			{
				m_corgiStartFallingRelay.Dispatch(this);
			}
			m_prevIsFalling = base.State.IsFalling;
		}
		if (base.Parameters.Gravity > 0f && !base.State.IsFalling)
		{
			base.State.IsCollidingBelow = false;
			return;
		}
		float num = _boundsHeight / 2f + VerticalRayOffset + m_yRaycastOffset + RayLengthAdd;
		if (_newPosition.y < 0f || base.State.IsCollidingBelow)
		{
			num += Mathf.Abs(_newPosition.y);
		}
		if (m_enablePrecisionPointChecks && (m_character.MovementState.CurrentState == CharacterStates.MovementStates.Dashing || !IsGravityActive || base.Parameters.FallMultiplier == 0f || Time.timeScale < 0.2f))
		{
			num += 0.05f;
		}
		float num2 = m_jumpLeewayAmount + _boundsHeight / 2f;
		float num3 = 0f;
		if (num < num2)
		{
			num3 = num2 - num;
			num += num3;
		}
		Vector2 vector = _transform.up;
		Vector2 vector2 = _transform.right;
		_verticalRayCastFromLeft = (_boundsBottomLeftCorner + _boundsTopLeftCorner) / 2f;
		_verticalRayCastToRight = (_boundsBottomRightCorner + _boundsTopRightCorner) / 2f;
		_verticalRayCastFromLeft.y += m_yRaycastOffset;
		_verticalRayCastToRight.y += m_yRaycastOffset;
		_verticalRayCastFromLeft += vector * VerticalRayOffset;
		_verticalRayCastToRight += vector * VerticalRayOffset;
		_verticalRayCastFromLeft += vector2 * _newPosition.x;
		_verticalRayCastToRight += vector2 * _newPosition.x;
		if (_belowHitsStorage.Length != NumberOfVerticalRays)
		{
			_belowHitsStorage = new RaycastHit2D[NumberOfVerticalRays];
		}
		_raysBelowLayerMaskPlatforms = PlatformMask;
		_raysBelowLayerMaskPlatformsWithoutOneWay = (int)PlatformMask & ~(int)MidHeightOneWayPlatformMask & ~(int)OneWayPlatformMask & ~(int)MovingOneWayPlatformMask;
		_raysBelowLayerMaskPlatformsWithoutMidHeight = (int)_raysBelowLayerMaskPlatforms & ~(int)MidHeightOneWayPlatformMask;
		if ((bool)base.StandingOnLastFrame)
		{
			_savedBelowLayer = base.StandingOnLastFrame.layer;
			if (MidHeightOneWayPlatformMask.Contains(base.StandingOnLastFrame.layer))
			{
				base.StandingOnLastFrame.layer = 8;
			}
		}
		if (base.State.WasGroundedLastFrame && (bool)base.StandingOnLastFrame && !MidHeightOneWayPlatformMask.Contains(base.StandingOnLastFrame.layer))
		{
			_raysBelowLayerMaskPlatforms = _raysBelowLayerMaskPlatformsWithoutMidHeight;
		}
		float num4 = float.MaxValue;
		int num5 = 0;
		bool flag = false;
		int num6 = -1;
		for (int i = 0; i < NumberOfVerticalRays; i++)
		{
			Vector2 zero = Vector2.zero;
			zero = ((NumberOfVerticalRays <= 1) ? Vector2.Lerp(_verticalRayCastFromLeft, _verticalRayCastToRight, 0.5f) : Vector2.Lerp(_verticalRayCastFromLeft, _verticalRayCastToRight, (float)i / (float)(NumberOfVerticalRays - 1)));
			if ((_newPosition.y > 0f && !base.State.WasGroundedLastFrame) || IsFlying)
			{
				_belowHitsStorage[i] = Physics2D.Raycast(zero, -vector, num, _raysBelowLayerMaskPlatformsWithoutOneWay);
			}
			else
			{
				_belowHitsStorage[i] = Physics2D.Raycast(zero, -vector, num, _raysBelowLayerMaskPlatforms);
			}
			if (!_belowHitsStorage[i])
			{
				continue;
			}
			GameObject gameObject = _belowHitsStorage[i].collider.gameObject;
			if (!DisableOneWayCollision && OneWayPlatformMask.Contains(gameObject.layer))
			{
				if (MMMaths.DistanceBetweenPointAndLine(_belowHitsStorage[i].point, _verticalRayCastFromLeft, _verticalRayCastToRight) < 0.0001f)
				{
					if (!m_platformRevertDict_STATIC.ContainsKey(gameObject))
					{
						m_platformRevertDict_STATIC.Add(gameObject, gameObject.layer);
						gameObject.layer = m_ignoreRaycastLayer_STATIC;
						i--;
					}
					continue;
				}
				if ((!base.State.IsGrounded && base.StandingOnLastFrame == gameObject && _belowHitsStorage[i].distance < _boundsHeight / 2f + m_yRaycastOffset) || (base.State.IsGrounded && base.StandingOnLastFrame != gameObject && base.StandingOnLastFrame != null && Mathf.Abs(m_lastHeightStandingOn - _belowHitsStorage[i].point.y) > 0.001f && m_lastAngleStandingOn == 0f))
				{
					if (!m_platformRevertDict_STATIC.ContainsKey(gameObject))
					{
						m_platformRevertDict_STATIC.Add(gameObject, gameObject.layer);
						gameObject.layer = m_ignoreRaycastLayer_STATIC;
						i--;
					}
					continue;
				}
			}
			if (!(Mathf.Abs(Vector2.Angle(_belowHitsStorage[i].normal, vector)) > base.Parameters.MaximumSlopeAngle) && !(_belowHitsStorage[i].collider == _ignoredCollider) && _belowHitsStorage[i].distance <= 0f && !DisableYSlopeOffsetCheck)
			{
				float num7 = Mathf.Clamp(_boundsHeight / 2f, 0.5f, float.MaxValue);
				Vector2 origin = zero;
				origin.y += num7;
				float distance = num + num7;
				_belowHitsStorage[i] = Physics2D.Raycast(origin, -vector, distance, _raysBelowLayerMaskPlatforms);
				if (!(Mathf.Abs(Vector2.Angle(_belowHitsStorage[i].normal, vector)) > base.Parameters.MaximumSlopeAngle) && !(_belowHitsStorage[i].collider == _ignoredCollider))
				{
					m_slopeYOffset = num7;
				}
			}
		}
		for (int j = 0; j < NumberOfVerticalRays; j++)
		{
			if (!_belowHitsStorage[j] || m_platformRevertDict_STATIC.ContainsKey(_belowHitsStorage[j].collider.gameObject) || _belowHitsStorage[j].distance == 0f || (Mathf.Abs(Vector2.Angle(_belowHitsStorage[j].normal, vector)) > base.Parameters.MaximumSlopeAngle && (!base.State.WasGroundedLastFrame || (base.State.WasGroundedLastFrame && !base.State.IsCollidingLeft && !base.State.IsCollidingRight))))
			{
				continue;
			}
			if (_belowHitsStorage[j].distance > num - num3)
			{
				if (!_belowHitsStorage[j].collider.CompareTag("MagicPlatform"))
				{
					base.IsWithinJumpLeeway = true;
				}
				continue;
			}
			if (!_belowHitsStorage[j].collider.CompareTag("MagicPlatform"))
			{
				base.IsWithinJumpLeeway = true;
			}
			flag = true;
			base.State.BelowSlopeAngle = Vector2.Angle(_belowHitsStorage[j].normal, vector);
			_crossBelowSlopeAngle = Vector3.Cross(vector, _belowHitsStorage[j].normal);
			if (_crossBelowSlopeAngle.z < 0f)
			{
				base.State.BelowSlopeAngle = 0f - base.State.BelowSlopeAngle;
			}
			if (_belowHitsStorage[j].distance < num4 || num6 == j)
			{
				num5 = j;
				num4 = _belowHitsStorage[j].distance;
			}
		}
		if (flag)
		{
			StandingOn = _belowHitsStorage[num5].collider.gameObject;
			if (PermanentlyDisableUponTouchingPlatform && StandingOn.CompareTag("Platform"))
			{
				PermanentlyDisabled = true;
				base.enabled = false;
			}
			if (StandingOn != m_previouslyStandingOn)
			{
				m_previouslyStandingOn = StandingOn;
				m_standingOnChangeRelay.Dispatch(this, StandingOn);
			}
			base.StandingOnCollider = _belowHitsStorage[num5].collider;
			m_heightStandingOn = _belowHitsStorage[num5].point.y;
			m_angleStandingOn = Mathf.Abs(Vector2.Angle(_belowHitsStorage[num5].normal, vector));
			_contactList.Add(_belowHitsStorage[num5]);
			float num8 = _boundsHeight / 2f + m_yRaycastOffset - Mathf.Abs(_newPosition.y);
			bool flag2 = Mathf.Approximately(num4, num8);
			if (!base.State.WasGroundedLastFrame && !flag2 && num4 < num8 && (OneWayPlatformMask.Contains(StandingOn.layer) || MovingOneWayPlatformMask.Contains(StandingOn.layer)))
			{
				base.State.IsCollidingBelow = false;
				base.IsWithinJumpLeeway = false;
				return;
			}
			if (base.Velocity.y < 0.1f)
			{
				base.State.IsFalling = false;
				base.State.IsCollidingBelow = true;
			}
			if (_externalForce.y > 0f && _velocity.y > 0f)
			{
				_newPosition.y = _velocity.y * Time.deltaTime;
				base.State.IsCollidingBelow = false;
			}
			else
			{
				float num9 = MMMaths.DistanceBetweenPointAndLine(_belowHitsStorage[num5].point, _verticalRayCastFromLeft, _verticalRayCastToRight);
				_newPosition.y = 0f - num9 + _boundsHeight / 2f + VerticalRayOffset + m_yRaycastOffset + m_slopeYOffset;
			}
			if (!base.State.WasGroundedLastFrame && _velocity.y > 0f)
			{
				_newPosition.y += _velocity.y * Time.deltaTime;
			}
			if (Mathf.Abs(_newPosition.y) < 0.0001f)
			{
				_newPosition.y = 0f;
			}
			if (m_characterExists && m_character.MovementState.CurrentState == CharacterStates.MovementStates.Dashing && base.State.IsCollidingBelow && _newPosition.y < 0f)
			{
				_newPosition.y = 0f;
			}
		}
		else
		{
			base.State.IsCollidingBelow = false;
			if (base.State.OnAMovingPlatform)
			{
				DetachFromMovingPlatform();
			}
		}
		if (IsFlying)
		{
			base.State.IsCollidingBelow = false;
			base.IsWithinJumpLeeway = false;
		}
		if (StickWhenWalkingDownSlopes && !base.State.IsCollidingBelow && IsGravityActive && m_characterExists && m_character.MovementState.CurrentState != CharacterStates.MovementStates.Dashing)
		{
			StickToSlope();
		}
		if (DisableStandingOnColliderLogic)
		{
			return;
		}
		m_prevStandingOnColliderList.Clear();
		m_prevStandingOnColliderList.AddRange(m_standingOnColliderList);
		m_standingOnColliderList.Clear();
		if (base.State.IsCollidingBelow)
		{
			RaycastHit2D[] belowHitsStorage = _belowHitsStorage;
			for (int k = 0; k < belowHitsStorage.Length; k++)
			{
				RaycastHit2D raycastHit2D = belowHitsStorage[k];
				if ((bool)raycastHit2D && raycastHit2D.distance == num4 && !m_standingOnColliderList.Contains(raycastHit2D.collider))
				{
					m_standingOnColliderList.Add(raycastHit2D.collider);
				}
			}
			foreach (Collider2D standingOnCollider in m_standingOnColliderList)
			{
				if (!standingOnCollider || m_prevStandingOnColliderList.Contains(standingOnCollider))
				{
					continue;
				}
				m_standingOnEnterHelperList.Clear();
				standingOnCollider.GetRoot().GetComponentsInChildren(m_standingOnEnterHelperList);
				foreach (IStandingOnEnter standingOnEnterHelper in m_standingOnEnterHelperList)
				{
					standingOnEnterHelper.OnStandingEnter(m_gameObject);
				}
			}
		}
		foreach (Collider2D prevStandingOnCollider in m_prevStandingOnColliderList)
		{
			if (!prevStandingOnCollider || m_standingOnColliderList.Contains(prevStandingOnCollider))
			{
				continue;
			}
			m_standingOnExitHelperList.Clear();
			prevStandingOnCollider.GetRoot().GetComponentsInChildren(m_standingOnExitHelperList);
			foreach (IStandingOnExit standingOnExitHelper in m_standingOnExitHelperList)
			{
				standingOnExitHelper.OnStandingExit(m_gameObject);
			}
		}
		if (!StandingOn || !StandingOn.CompareTag("MagicPlatform"))
		{
			return;
		}
		GameObject root = StandingOn.GetRoot();
		if ((bool)root)
		{
			IForcePlatformCollision component = root.GetComponent<IForcePlatformCollision>();
			if (!component.IsNativeNull())
			{
				component.ForcePlatformCollision();
			}
		}
	}

	protected override void ComputeNewSpeed()
	{
		if (Time.deltaTime > 0f && (!m_characterExists || (m_characterExists && m_character.MovementState.CurrentState != CharacterStates.MovementStates.Jumping)) && base.State.IsGrounded)
		{
			_velocity = _newPosition / Time.deltaTime;
		}
		if (base.State.IsGrounded)
		{
			_velocity.x *= base.Parameters.SlopeAngleSpeedFactor.Evaluate(Mathf.Abs(base.State.BelowSlopeAngle) * Mathf.Sign(_velocity.y));
		}
		if (!base.State.OnAMovingPlatform)
		{
			_velocity.x = Mathf.Clamp(_velocity.x, 0f - base.Parameters.MaxVelocity.x, base.Parameters.MaxVelocity.x);
			_velocity.y = Mathf.Clamp(_velocity.y, 0f - base.Parameters.MaxVelocity.y, base.Parameters.MaxVelocity.y);
		}
	}

	protected override void StickToSlope()
	{
		if ((_newPosition.y >= 0f && !base.State.WasGroundedLastFrame) || (m_characterExists && (m_character.MovementState.CurrentState == CharacterStates.MovementStates.Jumping || m_character.MovementState.CurrentState == CharacterStates.MovementStates.Dashing || m_character.MovementState.CurrentState == CharacterStates.MovementStates.DownStriking)) || !StickWhenWalkingDownSlopes || !base.State.WasGroundedLastFrame || _externalForce.y > 0f || (bool)_movingPlatform)
		{
			return;
		}
		float num = 0f;
		if (StickyRaycastLength == 0f)
		{
			num = _boundsWidth * Mathf.Abs(Mathf.Tan(base.Parameters.MaximumSlopeAngle));
			num += _boundsHeight / 2f + VerticalRayOffset;
		}
		else
		{
			num = StickyRaycastLength + _boundsHeight / 2f;
		}
		if ((bool)m_character && _newPosition.x == 0f)
		{
			_stickRayCastOrigin.x = (m_character.IsFacingRight ? _boundsBottomLeftCorner.x : _boundsTopRightCorner.x);
		}
		else
		{
			_stickRayCastOrigin.x = ((_newPosition.x > 0f) ? _boundsBottomLeftCorner.x : _boundsTopRightCorner.x);
		}
		_stickRayCastOrigin.x += _newPosition.x;
		_stickRayCastOrigin.y = _boundsCenter.y + VerticalRayOffset;
		_stickRaycast = Physics2D.Raycast(_stickRayCastOrigin, -base.transform.up, num, PlatformMask);
		if ((bool)_stickRaycast)
		{
			float num2 = Mathf.Abs(Vector2.Angle(_stickRaycast.normal, base.transform.up));
			if (!(num2 > base.Parameters.MaximumSlopeAngle) && !(num2 <= 0f) && (!base.StandingOnLastFrame || base.StandingOnLastFrame.layer != 11 || !(_stickRaycast.collider.gameObject != base.StandingOnLastFrame.gameObject)) && !(_stickRaycast.collider == _ignoredCollider))
			{
				_newPosition.y = 0f - Mathf.Abs(_stickRaycast.point.y - _stickRayCastOrigin.y) + _boundsHeight / 2f + VerticalRayOffset;
				_contactList.Add(_stickRaycast);
				base.State.IsCollidingBelow = true;
			}
		}
	}

	public void ForceStandingOn(Collider2D collider)
	{
		base.StandingOnCollider = collider;
		StandingOn = collider.gameObject;
		base.State.IsCollidingBelow = true;
		_velocity.y = 0f;
		_newPosition.y = 0f;
	}

	public void ResetState()
	{
		_boxCollider.gameObject.layer = m_boxColliderLayer;
		_newPosition = Vector2.zero;
		_velocity = Vector2.zero;
		_externalForce = Vector2.zero;
		base.State.WasGroundedLastFrame = false;
		base.State.WasTouchingTheCeilingLastFrame = false;
		base.CurrentWallCollider = null;
		base.State.Reset();
		StandingOn = null;
		base.StandingOnLastFrame = null;
		base.StandingOnCollider = null;
		m_heightStandingOn = float.MaxValue;
		m_lastHeightStandingOn = float.MaxValue;
		m_angleStandingOn = 0f;
		m_lastAngleStandingOn = 0f;
	}

	public void OnDestroy()
	{
		m_character = null;
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
