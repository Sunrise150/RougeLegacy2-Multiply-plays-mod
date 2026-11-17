using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.CorgiEngine;

[AddComponentMenu("Corgi Engine/Character/Core/Corgi Controller")]
public class CorgiController : MonoBehaviour
{
	public enum RaycastDirections
	{
		up,
		down,
		left,
		right
	}

	public enum DetachmentMethods
	{
		Layer,
		Object
	}

	[Header("Default Parameters")]
	public CorgiControllerParameters DefaultParameters;

	[Header("Collision Masks")]
	[Information("You need to define what layer(s) this character will consider a walkable platform/moving platform etc. By default, you want Platforms, MovingPlatforms, OneWayPlatforms, MovingOneWayPlatforms, in this order.", InformationAttribute.InformationType.Info, false)]
	public LayerMask PlatformMask = 0;

	public LayerMask MovingPlatformMask = 0;

	public LayerMask OneWayPlatformMask = 0;

	public LayerMask MovingOneWayPlatformMask = 0;

	public LayerMask MidHeightOneWayPlatformMask = 0;

	public DetachmentMethods DetachmentMethod;

	[ReadOnly]
	public GameObject StandingOn;

	[Header("Raycasting")]
	[Information("Here you can define how many rays are cast horizontally and vertically. You'll want them as far as possible from each other, but close enough that no obstacle or enemy can fit between 2 rays.", InformationAttribute.InformationType.Info, false)]
	public int NumberOfHorizontalRays = 4;

	public int NumberOfVerticalRays = 4;

	public float HorizontalRayOffset = 0.05f;

	[Tooltip("Honestly don't know what this is for. If character is not hooking to slopes when running up them, tweak this number until they do.")]
	public float VerticalRayOffset;

	public float CrouchedRaycastLengthMultiplier = 1f;

	public bool CastRaysOnBothSides = true;

	[Header("Stickiness")]
	[Information("Here you can define whether or not you want your character stick to slopes when walking down them, and how long the raycast handling that should be (0 means automatic length).", InformationAttribute.InformationType.Info, false)]
	public bool StickWhenWalkingDownSlopes = true;

	public float StickyRaycastLength = 1.25f;

	protected CorgiControllerParameters _overrideParameters;

	protected Vector2 _velocity;

	protected float _friction;

	protected float _fallSlowFactor;

	protected float _currentGravity;

	protected Vector2 _externalForce;

	protected Vector2 _newPosition;

	protected Transform _transform;

	protected BoxCollider2D _boxCollider;

	protected LayerMask _platformMaskSave;

	protected LayerMask _raysBelowLayerMaskPlatforms;

	protected LayerMask _raysBelowLayerMaskPlatformsWithoutOneWay;

	protected LayerMask _raysBelowLayerMaskPlatformsWithoutMidHeight;

	protected int _savedBelowLayer;

	protected MMPathMovement _movingPlatform;

	protected float _movingPlatformCurrentGravity;

	protected bool _gravityActive = true;

	protected Collider2D _ignoredCollider;

	protected const float _smallValue = 0.0001f;

	protected const float _obstacleHeightTolerance = 0.05f;

	protected const float _movingPlatformsGravity = -500f;

	protected Vector2 _originalColliderSize;

	protected Vector2 _originalColliderOffset;

	protected Vector2 _originalSizeRaycastOrigin;

	protected Vector3 _crossBelowSlopeAngle;

	protected RaycastHit2D[] _sideHitsStorage;

	protected RaycastHit2D[] _belowHitsStorage;

	protected RaycastHit2D[] _aboveHitsStorage;

	protected RaycastHit2D _stickRaycast;

	protected float _movementDirection;

	protected float _storedMovementDirection = 1f;

	protected const float _movementDirectionThreshold = 0.0001f;

	protected Vector2 _horizontalRayCastFromBottom = Vector2.zero;

	protected Vector2 _horizontalRayCastToTop = Vector2.zero;

	protected Vector2 _verticalRayCastFromLeft = Vector2.zero;

	protected Vector2 _verticalRayCastToRight = Vector2.zero;

	protected Vector2 _aboveRayCastStart = Vector2.zero;

	protected Vector2 _aboveRayCastEnd = Vector2.zero;

	protected Vector2 _stickRayCastOrigin = Vector2.zero;

	protected Vector3 _colliderBottomCenterPosition;

	protected Vector3 _colliderLeftCenterPosition;

	protected Vector3 _colliderRightCenterPosition;

	protected Vector3 _colliderTopCenterPosition;

	protected MMPathMovement _movingPlatformTest;

	protected RaycastHit2D[] _raycastNonAlloc = new RaycastHit2D[0];

	protected Vector2 _boundsTopLeftCorner;

	protected Vector2 _boundsBottomLeftCorner;

	protected Vector2 _boundsTopRightCorner;

	protected Vector2 _boundsBottomRightCorner;

	protected Vector2 _boundsCenter;

	protected float _boundsWidth;

	protected float _boundsHeight;

	protected List<RaycastHit2D> _contactList;

	protected bool m_savePlatformMaskSet;

	private WaitRL_Yield m_waitYield;

	private WaitRL_Yield m_oneWayWaitYield;

	private WaitRL_Yield m_platformWaitYield;

	public CorgiControllerState State { get; protected set; }

	public CorgiControllerParameters Parameters => _overrideParameters ?? DefaultParameters;

	public GameObject StandingOnLastFrame { get; protected set; }

	public Collider2D StandingOnCollider { get; protected set; }

	public Vector2 Velocity => _velocity;

	public Vector2 ForcesApplied { get; protected set; }

	public GameObject CurrentWallCollider { get; protected set; }

	public Vector3 ColliderSize => Vector3.Scale(base.transform.localScale, _boxCollider.size);

	public Vector3 ColliderCenterPosition => _boxCollider.bounds.center;

	public virtual Vector3 ColliderBottomPosition
	{
		get
		{
			_colliderBottomCenterPosition.x = _boxCollider.bounds.center.x;
			_colliderBottomCenterPosition.y = _boxCollider.bounds.min.y;
			_colliderBottomCenterPosition.z = 0f;
			return _colliderBottomCenterPosition;
		}
	}

	public virtual Vector3 ColliderLeftPosition
	{
		get
		{
			_colliderLeftCenterPosition.x = _boxCollider.bounds.min.x;
			_colliderLeftCenterPosition.y = _boxCollider.bounds.center.y;
			_colliderLeftCenterPosition.z = 0f;
			return _colliderLeftCenterPosition;
		}
	}

	public virtual Vector3 ColliderTopPosition
	{
		get
		{
			_colliderTopCenterPosition.x = _boxCollider.bounds.center.x;
			_colliderTopCenterPosition.y = _boxCollider.bounds.max.y;
			_colliderTopCenterPosition.z = 0f;
			return _colliderTopCenterPosition;
		}
	}

	public virtual Vector3 ColliderRightPosition
	{
		get
		{
			_colliderRightCenterPosition.x = _boxCollider.bounds.max.x;
			_colliderRightCenterPosition.y = _boxCollider.bounds.center.y;
			_colliderRightCenterPosition.z = 0f;
			return _colliderRightCenterPosition;
		}
	}

	public float Friction => _friction;

	public virtual Vector3 BoundsTopLeftCorner => _boundsTopLeftCorner;

	public virtual Vector3 BoundsBottomLeftCorner => _boundsBottomLeftCorner;

	public virtual Vector3 BoundsTopRightCorner => _boundsTopRightCorner;

	public virtual Vector3 BoundsBottomRightCorner => _boundsBottomRightCorner;

	public virtual Vector3 BoundsTop => (_boundsTopLeftCorner + _boundsTopRightCorner) / 2f;

	public virtual Vector3 BoundsBottom => (_boundsBottomLeftCorner + _boundsBottomRightCorner) / 2f;

	public virtual Vector3 BoundsRight => (_boundsTopRightCorner + _boundsBottomRightCorner) / 2f;

	public virtual Vector3 BoundsLeft => (_boundsTopLeftCorner + _boundsBottomLeftCorner) / 2f;

	public virtual Vector3 BoundsCenter => _boundsCenter;

	public LayerMask SavedPlatformMask => _platformMaskSave;

	public bool IsWithinJumpLeeway { get; protected set; }

	public bool IsInitialized { get; protected set; }

	protected virtual void Awake()
	{
		SavePlatformMask();
		PlatformMask = (int)PlatformMask | (int)OneWayPlatformMask;
		PlatformMask = (int)PlatformMask | (int)MovingPlatformMask;
		PlatformMask = (int)PlatformMask | (int)MovingOneWayPlatformMask;
		PlatformMask = (int)PlatformMask | (int)MidHeightOneWayPlatformMask;
	}

	public void SavePlatformMask()
	{
		if (!m_savePlatformMaskSet)
		{
			_platformMaskSave = PlatformMask;
			m_savePlatformMaskSet = true;
		}
	}

	protected virtual void Start()
	{
		Initialization();
	}

	protected virtual void Initialization()
	{
		_transform = base.transform;
		_boxCollider = GetComponent<BoxCollider2D>();
		_originalColliderSize = _boxCollider.size;
		_originalColliderOffset = _boxCollider.offset;
		if (_boxCollider.offset.x != 0f && Parameters.DisplayWarnings)
		{
			Debug.LogWarning("The boxcollider for " + base.gameObject.name + " should have an x offset set to zero. Right now this may cause issues when you change direction close to a wall.");
		}
		_contactList = new List<RaycastHit2D>();
		State = new CorgiControllerState();
		_sideHitsStorage = new RaycastHit2D[NumberOfHorizontalRays];
		_belowHitsStorage = new RaycastHit2D[NumberOfVerticalRays];
		_aboveHitsStorage = new RaycastHit2D[NumberOfVerticalRays];
		CurrentWallCollider = null;
		State.Reset();
		SetRaysParameters();
	}

	public virtual void AddForce(Vector2 force)
	{
		_velocity += force;
		_externalForce += force;
	}

	public virtual void AddHorizontalForce(float x)
	{
		_velocity.x += x;
		_externalForce.x += x;
	}

	public virtual void AddVerticalForce(float y)
	{
		_velocity.y += y;
		_externalForce.y += y;
	}

	public virtual void SetForce(Vector2 force)
	{
		_velocity = force;
		_externalForce = force;
	}

	public virtual void SetHorizontalForce(float x)
	{
		_velocity.x = x;
		_externalForce.x = x;
	}

	public virtual void SetVerticalForce(float y)
	{
		_velocity.y = y;
		_externalForce.y = y;
	}

	protected virtual void LateUpdate()
	{
		EveryFrame();
	}

	protected virtual void EveryFrame()
	{
		ApplyGravity();
		FrameInitialization();
		SetRaysParameters();
		HandleMovingPlatforms();
		ForcesApplied = _velocity;
		DetermineMovementDirection();
		if (CastRaysOnBothSides)
		{
			CastRaysToTheLeft();
			CastRaysToTheRight();
		}
		else if (_movementDirection == -1f)
		{
			CastRaysToTheLeft();
		}
		else
		{
			CastRaysToTheRight();
		}
		CastRaysBelow();
		CastRaysAbove();
		_transform.Translate(_newPosition, Space.Self);
		SetRaysParameters();
		ComputeNewSpeed();
		SetStates();
		_externalForce.x = 0f;
		_externalForce.y = 0f;
		FrameExit();
	}

	protected virtual void FrameInitialization()
	{
		_contactList.Clear();
		_newPosition = Velocity * Time.deltaTime;
		State.WasGroundedLastFrame = State.IsCollidingBelow;
		StandingOnLastFrame = StandingOn;
		State.WasTouchingTheCeilingLastFrame = State.IsCollidingAbove;
		CurrentWallCollider = null;
		State.Reset();
	}

	protected virtual void FrameExit()
	{
		if ((bool)StandingOnLastFrame)
		{
			StandingOnLastFrame.layer = _savedBelowLayer;
		}
	}

	protected virtual void DetermineMovementDirection()
	{
		_movementDirection = _storedMovementDirection;
		if (_velocity.x < -0.0001f || _externalForce.x < -0.0001f)
		{
			_movementDirection = -1f;
		}
		else if (_velocity.x > 0.0001f || _externalForce.x > 0.0001f)
		{
			_movementDirection = 1f;
		}
		_storedMovementDirection = _movementDirection;
	}

	protected virtual void ApplyGravity()
	{
		_currentGravity = Parameters.Gravity;
		if (_velocity.y > 0f)
		{
			_currentGravity *= Parameters.AscentMultiplier;
		}
		else
		{
			_currentGravity *= Parameters.FallMultiplier;
		}
		if (_gravityActive)
		{
			_velocity.y += (_currentGravity + _movingPlatformCurrentGravity) * Time.deltaTime;
		}
		if (_fallSlowFactor != 0f)
		{
			_velocity.y *= _fallSlowFactor;
		}
	}

	protected virtual void HandleMovingPlatforms()
	{
		if (_movingPlatform != null)
		{
			if (!float.IsNaN(_movingPlatform.CurrentSpeed.x) && !float.IsNaN(_movingPlatform.CurrentSpeed.y) && !float.IsNaN(_movingPlatform.CurrentSpeed.z))
			{
				_transform.Translate(_movingPlatform.CurrentSpeed * Time.deltaTime);
			}
			if (Time.timeScale != 0f && !float.IsNaN(_movingPlatform.CurrentSpeed.x) && !float.IsNaN(_movingPlatform.CurrentSpeed.y) && !float.IsNaN(_movingPlatform.CurrentSpeed.z) && !(Time.deltaTime <= 0f))
			{
				State.OnAMovingPlatform = true;
				GravityActive(state: false);
				_movingPlatformCurrentGravity = -500f;
				_newPosition.y = _movingPlatform.CurrentSpeed.y * Time.deltaTime;
				_velocity = -_newPosition / Time.deltaTime;
				_velocity.x = 0f - _velocity.x;
				SetRaysParameters();
			}
		}
	}

	public virtual void DetachFromMovingPlatform()
	{
		if (!(_movingPlatform == null))
		{
			GravityActive(state: true);
			State.OnAMovingPlatform = false;
			_movingPlatform = null;
			_movingPlatformCurrentGravity = 0f;
		}
	}

	public virtual bool CastRays(RaycastDirections direction, float rayLength, Color color, ref RaycastHit2D[] storageArray)
	{
		bool result = false;
		switch (direction)
		{
		case RaycastDirections.left:
		{
			_horizontalRayCastFromBottom = (_boundsBottomRightCorner + _boundsBottomLeftCorner) / 2f;
			_horizontalRayCastToTop = (_boundsTopLeftCorner + _boundsTopRightCorner) / 2f;
			_horizontalRayCastFromBottom += (Vector2)base.transform.up * 0.05f;
			_horizontalRayCastToTop -= (Vector2)base.transform.up * 0.05f;
			for (int k = 0; k < NumberOfHorizontalRays; k++)
			{
				Vector2 rayOriginPoint3 = Vector2.Lerp(_horizontalRayCastFromBottom, _horizontalRayCastToTop, (float)k / (float)(NumberOfHorizontalRays - 1));
				storageArray[k] = MMDebug.RayCast(rayOriginPoint3, -base.transform.right, rayLength, (int)PlatformMask & ~(int)OneWayPlatformMask & ~(int)MovingOneWayPlatformMask, color, Parameters.DrawRaycastsGizmos);
				if ((bool)storageArray[k])
				{
					result = true;
				}
			}
			return result;
		}
		case RaycastDirections.right:
		{
			_horizontalRayCastFromBottom = (_boundsBottomRightCorner + _boundsBottomLeftCorner) / 2f;
			_horizontalRayCastToTop = (_boundsTopLeftCorner + _boundsTopRightCorner) / 2f;
			_horizontalRayCastFromBottom += (Vector2)base.transform.up * 0.05f;
			_horizontalRayCastToTop -= (Vector2)base.transform.up * 0.05f;
			for (int j = 0; j < NumberOfHorizontalRays; j++)
			{
				Vector2 rayOriginPoint2 = Vector2.Lerp(_horizontalRayCastFromBottom, _horizontalRayCastToTop, (float)j / (float)(NumberOfHorizontalRays - 1));
				storageArray[j] = MMDebug.RayCast(rayOriginPoint2, base.transform.right, rayLength, (int)PlatformMask & ~(int)OneWayPlatformMask & ~(int)MovingOneWayPlatformMask, color, Parameters.DrawRaycastsGizmos);
				if ((bool)storageArray[j])
				{
					result = true;
				}
			}
			return result;
		}
		case RaycastDirections.down:
		{
			_verticalRayCastFromLeft = (_boundsBottomLeftCorner + _boundsTopLeftCorner) / 2f;
			_verticalRayCastToRight = (_boundsBottomRightCorner + _boundsTopRightCorner) / 2f;
			_verticalRayCastFromLeft += (Vector2)base.transform.up * HorizontalRayOffset;
			_verticalRayCastToRight += (Vector2)base.transform.up * HorizontalRayOffset;
			_verticalRayCastFromLeft += (Vector2)base.transform.right * _newPosition.x;
			_verticalRayCastToRight += (Vector2)base.transform.right * _newPosition.x;
			for (int l = 0; l < NumberOfVerticalRays; l++)
			{
				Vector2 rayOriginPoint4 = Vector2.Lerp(_verticalRayCastFromLeft, _verticalRayCastToRight, (float)l / (float)(NumberOfVerticalRays - 1));
				if (_newPosition.y > 0f && !State.WasGroundedLastFrame)
				{
					storageArray[l] = MMDebug.RayCast(rayOriginPoint4, -base.transform.up, rayLength, (int)PlatformMask & ~(int)OneWayPlatformMask & ~(int)MovingOneWayPlatformMask, color, Parameters.DrawRaycastsGizmos);
					if ((bool)storageArray[l])
					{
						result = true;
					}
				}
			}
			return result;
		}
		case RaycastDirections.up:
		{
			_verticalRayCastFromLeft = (_boundsBottomLeftCorner + _boundsTopLeftCorner) / 2f;
			_verticalRayCastToRight = (_boundsBottomRightCorner + _boundsTopRightCorner) / 2f;
			_verticalRayCastFromLeft += (Vector2)base.transform.up * HorizontalRayOffset;
			_verticalRayCastToRight += (Vector2)base.transform.up * HorizontalRayOffset;
			_verticalRayCastFromLeft += (Vector2)base.transform.right * _newPosition.x;
			_verticalRayCastToRight += (Vector2)base.transform.right * _newPosition.x;
			for (int i = 0; i < NumberOfVerticalRays; i++)
			{
				Vector2 rayOriginPoint = Vector2.Lerp(_verticalRayCastFromLeft, _verticalRayCastToRight, (float)i / (float)(NumberOfVerticalRays - 1));
				if (_newPosition.y > 0f && !State.WasGroundedLastFrame)
				{
					storageArray[i] = MMDebug.RayCast(rayOriginPoint, base.transform.up, rayLength, (int)PlatformMask & ~(int)OneWayPlatformMask & ~(int)MovingOneWayPlatformMask, color, Parameters.DrawRaycastsGizmos);
					if ((bool)storageArray[i])
					{
						result = true;
					}
				}
			}
			return result;
		}
		default:
			return false;
		}
	}

	protected virtual void CastRaysToTheLeft()
	{
		CastRaysToTheSides(-1f);
	}

	protected virtual void CastRaysToTheRight()
	{
		CastRaysToTheSides(1f);
	}

	protected virtual void CastRaysToTheSides(float raysDirection)
	{
		_horizontalRayCastFromBottom = (_boundsBottomRightCorner + _boundsBottomLeftCorner) / 2f;
		_horizontalRayCastToTop = (_boundsTopLeftCorner + _boundsTopRightCorner) / 2f;
		_horizontalRayCastFromBottom += (Vector2)base.transform.up * 0.05f;
		_horizontalRayCastToTop -= (Vector2)base.transform.up * 0.05f;
		float rayDistance = Mathf.Abs(_velocity.x * Time.deltaTime) + _boundsWidth / 2f + HorizontalRayOffset * 2f;
		if (_sideHitsStorage.Length != NumberOfHorizontalRays)
		{
			_sideHitsStorage = new RaycastHit2D[NumberOfHorizontalRays];
		}
		for (int i = 0; i < NumberOfHorizontalRays; i++)
		{
			Vector2 rayOriginPoint = Vector2.Lerp(_horizontalRayCastFromBottom, _horizontalRayCastToTop, (float)i / (float)(NumberOfHorizontalRays - 1));
			if (State.WasGroundedLastFrame && i == 0)
			{
				_sideHitsStorage[i] = MMDebug.RayCast(rayOriginPoint, raysDirection * base.transform.right, rayDistance, PlatformMask, Colors.Indigo, Parameters.DrawRaycastsGizmos);
			}
			else
			{
				_sideHitsStorage[i] = MMDebug.RayCast(rayOriginPoint, raysDirection * base.transform.right, rayDistance, (int)PlatformMask & ~(int)OneWayPlatformMask & ~(int)MovingOneWayPlatformMask, Colors.Indigo, Parameters.DrawRaycastsGizmos);
			}
			if (!(_sideHitsStorage[i].distance > 0f))
			{
				continue;
			}
			if (_sideHitsStorage[i].collider == _ignoredCollider)
			{
				break;
			}
			float num = Mathf.Abs(Vector2.Angle(_sideHitsStorage[i].normal, base.transform.up));
			if (_movementDirection == raysDirection)
			{
				State.LateralSlopeAngle = num;
			}
			if (!(num > Parameters.MaximumSlopeAngle))
			{
				continue;
			}
			if (raysDirection < 0f)
			{
				State.IsCollidingLeft = true;
				State.DistanceToLeftCollider = _sideHitsStorage[i].distance;
			}
			else
			{
				State.IsCollidingRight = true;
				State.DistanceToRightCollider = _sideHitsStorage[i].distance;
			}
			if (_movementDirection == raysDirection)
			{
				CurrentWallCollider = _sideHitsStorage[i].collider.gameObject;
				State.SlopeAngleOK = false;
				float num2 = MMMaths.DistanceBetweenPointAndLine(_sideHitsStorage[i].point, _horizontalRayCastFromBottom, _horizontalRayCastToTop);
				if (raysDirection <= 0f)
				{
					_newPosition.x = 0f - num2 + _boundsWidth / 2f + HorizontalRayOffset * 2f;
				}
				else
				{
					_newPosition.x = num2 - _boundsWidth / 2f - HorizontalRayOffset * 2f;
				}
				if (!State.IsGrounded && Velocity.y > 0f)
				{
					_newPosition.x = 0f;
				}
				_contactList.Add(_sideHitsStorage[i]);
				_velocity.x = 0f;
			}
			break;
		}
	}

	protected virtual void CastRaysBelow()
	{
		_friction = 0f;
		if (_newPosition.y < -0.0001f)
		{
			State.IsFalling = true;
		}
		else
		{
			State.IsFalling = false;
		}
		if (Parameters.Gravity > 0f && !State.IsFalling)
		{
			State.IsCollidingBelow = false;
			return;
		}
		float num = _boundsHeight / 2f + HorizontalRayOffset;
		if (State.OnAMovingPlatform)
		{
			num *= 2f;
		}
		if (_newPosition.y < 0f)
		{
			num += Mathf.Abs(_newPosition.y);
		}
		_verticalRayCastFromLeft = (_boundsBottomLeftCorner + _boundsTopLeftCorner) / 2f;
		_verticalRayCastToRight = (_boundsBottomRightCorner + _boundsTopRightCorner) / 2f;
		_verticalRayCastFromLeft += (Vector2)base.transform.up * HorizontalRayOffset;
		_verticalRayCastToRight += (Vector2)base.transform.up * HorizontalRayOffset;
		_verticalRayCastFromLeft += (Vector2)base.transform.right * _newPosition.x;
		_verticalRayCastToRight += (Vector2)base.transform.right * _newPosition.x;
		if (_belowHitsStorage.Length != NumberOfVerticalRays)
		{
			_belowHitsStorage = new RaycastHit2D[NumberOfVerticalRays];
		}
		_raysBelowLayerMaskPlatforms = PlatformMask;
		_raysBelowLayerMaskPlatformsWithoutOneWay = (int)PlatformMask & ~(int)MidHeightOneWayPlatformMask & ~(int)OneWayPlatformMask & ~(int)MovingOneWayPlatformMask;
		_raysBelowLayerMaskPlatformsWithoutMidHeight = (int)_raysBelowLayerMaskPlatforms & ~(int)MidHeightOneWayPlatformMask;
		if (StandingOnLastFrame != null)
		{
			_savedBelowLayer = StandingOnLastFrame.layer;
			if (MidHeightOneWayPlatformMask.Contains(StandingOnLastFrame.layer))
			{
				StandingOnLastFrame.layer = 8;
			}
		}
		if (State.WasGroundedLastFrame && StandingOnLastFrame != null && !MidHeightOneWayPlatformMask.Contains(StandingOnLastFrame.layer))
		{
			_raysBelowLayerMaskPlatforms = _raysBelowLayerMaskPlatformsWithoutMidHeight;
		}
		float num2 = float.MaxValue;
		int num3 = 0;
		bool flag = false;
		for (int i = 0; i < NumberOfVerticalRays; i++)
		{
			Vector2 rayOriginPoint = Vector2.Lerp(_verticalRayCastFromLeft, _verticalRayCastToRight, (float)i / (float)(NumberOfVerticalRays - 1));
			if (_newPosition.y > 0f && !State.WasGroundedLastFrame)
			{
				_belowHitsStorage[i] = MMDebug.RayCast(rayOriginPoint, -base.transform.up, num, _raysBelowLayerMaskPlatformsWithoutOneWay, Color.blue, Parameters.DrawRaycastsGizmos);
			}
			else
			{
				_belowHitsStorage[i] = MMDebug.RayCast(rayOriginPoint, -base.transform.up, num, _raysBelowLayerMaskPlatforms, Color.blue, Parameters.DrawRaycastsGizmos);
			}
			if (MMMaths.DistanceBetweenPointAndLine(_belowHitsStorage[num3].point, _verticalRayCastFromLeft, _verticalRayCastToRight) < 0.0001f)
			{
				break;
			}
			if ((bool)_belowHitsStorage[i] && !(_belowHitsStorage[i].collider == _ignoredCollider))
			{
				flag = true;
				State.BelowSlopeAngle = Vector2.Angle(_belowHitsStorage[i].normal, base.transform.up);
				_crossBelowSlopeAngle = Vector3.Cross(base.transform.up, _belowHitsStorage[i].normal);
				if (_crossBelowSlopeAngle.z < 0f)
				{
					State.BelowSlopeAngle = 0f - State.BelowSlopeAngle;
				}
				if (_belowHitsStorage[i].distance < num2)
				{
					num3 = i;
					num2 = _belowHitsStorage[i].distance;
				}
			}
		}
		if (flag)
		{
			StandingOn = _belowHitsStorage[num3].collider.gameObject;
			StandingOnCollider = _belowHitsStorage[num3].collider;
			if (!State.WasGroundedLastFrame && num2 < _boundsHeight / 2f && (OneWayPlatformMask.Contains(StandingOn.layer) || MovingOneWayPlatformMask.Contains(StandingOn.layer)))
			{
				State.IsCollidingBelow = false;
				return;
			}
			State.IsFalling = false;
			State.IsCollidingBelow = true;
			if (_externalForce.y > 0f && _velocity.y > 0f)
			{
				_newPosition.y = _velocity.y * Time.deltaTime;
				State.IsCollidingBelow = false;
			}
			else
			{
				float num4 = MMMaths.DistanceBetweenPointAndLine(_belowHitsStorage[num3].point, _verticalRayCastFromLeft, _verticalRayCastToRight);
				_newPosition.y = 0f - num4 + _boundsHeight / 2f + HorizontalRayOffset;
			}
			if (!State.WasGroundedLastFrame && _velocity.y > 0f)
			{
				_newPosition.y += _velocity.y * Time.deltaTime;
			}
			if (Mathf.Abs(_newPosition.y) < 0.0001f)
			{
				_newPosition.y = 0f;
			}
			_movingPlatformTest = _belowHitsStorage[num3].collider.gameObject.GetComponentNoAlloc<MMPathMovement>();
			if (_movingPlatformTest != null && State.IsGrounded)
			{
				_movingPlatform = _movingPlatformTest.GetComponent<MMPathMovement>();
			}
			else
			{
				DetachFromMovingPlatform();
			}
		}
		else
		{
			State.IsCollidingBelow = false;
			if (State.OnAMovingPlatform)
			{
				DetachFromMovingPlatform();
			}
		}
		if (StickWhenWalkingDownSlopes)
		{
			StickToSlope();
		}
	}

	protected virtual void CastRaysAbove()
	{
		if (_newPosition.y < 0f)
		{
			return;
		}
		float num = (State.IsGrounded ? HorizontalRayOffset : _newPosition.y);
		num += _boundsHeight / 2f;
		bool flag = false;
		_aboveRayCastStart = (_boundsBottomLeftCorner + _boundsTopLeftCorner) / 2f;
		_aboveRayCastEnd = (_boundsBottomRightCorner + _boundsTopRightCorner) / 2f;
		_aboveRayCastStart += (Vector2)base.transform.right * _newPosition.x;
		_aboveRayCastEnd += (Vector2)base.transform.right * _newPosition.x;
		if (_aboveHitsStorage.Length != NumberOfVerticalRays)
		{
			_aboveHitsStorage = new RaycastHit2D[NumberOfVerticalRays];
		}
		float num2 = float.MaxValue;
		for (int i = 0; i < NumberOfVerticalRays; i++)
		{
			Vector2 rayOriginPoint = Vector2.Lerp(_aboveRayCastStart, _aboveRayCastEnd, (float)i / (float)(NumberOfVerticalRays - 1));
			_aboveHitsStorage[i] = MMDebug.RayCast(rayOriginPoint, base.transform.up, num, (int)PlatformMask & ~(int)OneWayPlatformMask & ~(int)MovingOneWayPlatformMask, Colors.Cyan, Parameters.DrawRaycastsGizmos);
			if ((bool)_aboveHitsStorage[i])
			{
				flag = true;
				if (_aboveHitsStorage[i].collider == _ignoredCollider)
				{
					break;
				}
				if (_aboveHitsStorage[i].distance < num2)
				{
					num2 = _aboveHitsStorage[i].distance;
				}
			}
		}
		if (flag)
		{
			_newPosition.y = num2 - _boundsHeight / 2f;
			if (State.IsGrounded && _newPosition.y < 0f)
			{
				_newPosition.y = 0f;
			}
			State.IsCollidingAbove = true;
			if (!State.WasTouchingTheCeilingLastFrame)
			{
				_velocity = new Vector2(_velocity.x, 0f);
			}
		}
	}

	protected virtual void StickToSlope()
	{
		if (!(_newPosition.y >= 0f) && StickWhenWalkingDownSlopes && State.WasGroundedLastFrame && !(_externalForce.y > 0f) && !(_movingPlatform != null))
		{
			float num = 0f;
			if (StickyRaycastLength == 0f)
			{
				num = _boundsWidth * Mathf.Abs(Mathf.Tan(Parameters.MaximumSlopeAngle));
				num += _boundsHeight / 2f + HorizontalRayOffset;
			}
			else
			{
				num = StickyRaycastLength;
			}
			_stickRayCastOrigin.x = ((_newPosition.x > 0f) ? _boundsBottomLeftCorner.x : _boundsTopRightCorner.x);
			_stickRayCastOrigin.x += _newPosition.x;
			_stickRayCastOrigin.y = _boundsCenter.y + HorizontalRayOffset;
			_stickRaycast = MMDebug.RayCast(_stickRayCastOrigin, -base.transform.up, num, PlatformMask, Colors.LightBlue, Parameters.DrawRaycastsGizmos);
			if ((bool)_stickRaycast && !(_stickRaycast.collider == _ignoredCollider))
			{
				_newPosition.y = 0f - Mathf.Abs(_stickRaycast.point.y - _stickRayCastOrigin.y) + _boundsHeight / 2f + HorizontalRayOffset;
				State.IsCollidingBelow = true;
			}
		}
	}

	protected virtual void ComputeNewSpeed()
	{
		if (Time.deltaTime > 0f)
		{
			_velocity = _newPosition / Time.deltaTime;
		}
		if (State.IsGrounded)
		{
			_velocity.x *= Parameters.SlopeAngleSpeedFactor.Evaluate(Mathf.Abs(State.BelowSlopeAngle) * Mathf.Sign(_velocity.y));
		}
		if (!State.OnAMovingPlatform)
		{
			_velocity.x = Mathf.Clamp(_velocity.x, 0f - Parameters.MaxVelocity.x, Parameters.MaxVelocity.x);
			_velocity.y = Mathf.Clamp(_velocity.y, 0f - Parameters.MaxVelocity.y, Parameters.MaxVelocity.y);
		}
	}

	protected virtual void SetStates()
	{
		if (!State.WasGroundedLastFrame && State.IsCollidingBelow)
		{
			State.JustGotGrounded = true;
		}
		if (State.IsCollidingLeft || State.IsCollidingRight || State.IsCollidingBelow || State.IsCollidingAbove)
		{
			OnCorgiColliderHit();
		}
	}

	public virtual void SetRaysParameters()
	{
		Vector2 offset = _boxCollider.offset;
		Vector2 size = _boxCollider.size;
		float y = offset.y + size.y / 2f;
		float y2 = offset.y - size.y / 2f;
		float x = offset.x - size.x / 2f;
		float x2 = offset.x + size.x / 2f;
		_boundsTopLeftCorner.x = x;
		_boundsTopLeftCorner.y = y;
		_boundsTopRightCorner.x = x2;
		_boundsTopRightCorner.y = y;
		_boundsBottomLeftCorner.x = x;
		_boundsBottomLeftCorner.y = y2;
		_boundsBottomRightCorner.x = x2;
		_boundsBottomRightCorner.y = y2;
		_boundsTopLeftCorner = base.transform.TransformPoint(_boundsTopLeftCorner);
		_boundsTopRightCorner = base.transform.TransformPoint(_boundsTopRightCorner);
		_boundsBottomLeftCorner = base.transform.TransformPoint(_boundsBottomLeftCorner);
		_boundsBottomRightCorner = base.transform.TransformPoint(_boundsBottomRightCorner);
		_boundsCenter = (_boundsTopRightCorner + _boundsBottomLeftCorner) / 2f;
		_boundsWidth = Vector2.Distance(_boundsBottomLeftCorner, _boundsBottomRightCorner);
		_boundsHeight = Vector2.Distance(_boundsBottomLeftCorner, _boundsTopLeftCorner);
	}

	public virtual void SetIgnoreCollider(Collider2D newIgnoredCollider)
	{
		_ignoredCollider = newIgnoredCollider;
	}

	public virtual IEnumerator DisableCollisions(float duration)
	{
		if (m_waitYield == null)
		{
			m_waitYield = new WaitRL_Yield(duration);
		}
		else
		{
			m_waitYield.CreateNew(duration);
		}
		CollisionsOff();
		yield return m_waitYield;
		CollisionsOn();
	}

	public virtual void CollisionsOn()
	{
		PlatformMask = _platformMaskSave;
		PlatformMask = (int)PlatformMask | (int)OneWayPlatformMask;
		PlatformMask = (int)PlatformMask | (int)MovingPlatformMask;
		PlatformMask = (int)PlatformMask | (int)MovingOneWayPlatformMask;
		PlatformMask = (int)PlatformMask | (int)MidHeightOneWayPlatformMask;
	}

	public virtual void CollisionsOff()
	{
		PlatformMask = 0;
	}

	public virtual IEnumerator DisableCollisionsWithOneWayPlatforms(float duration)
	{
		if (DetachmentMethod == DetachmentMethods.Layer)
		{
			CollisionsOffWithOneWayPlatformsLayer();
			if (m_oneWayWaitYield == null)
			{
				m_oneWayWaitYield = new WaitRL_Yield(duration);
			}
			else
			{
				m_oneWayWaitYield.CreateNew(duration);
			}
			yield return m_oneWayWaitYield;
			CollisionsOn();
		}
		else
		{
			SetIgnoreCollider(StandingOnCollider);
			if (m_oneWayWaitYield == null)
			{
				m_oneWayWaitYield = new WaitRL_Yield(duration);
			}
			else
			{
				m_oneWayWaitYield.CreateNew(duration);
			}
			yield return m_oneWayWaitYield;
			SetIgnoreCollider(null);
		}
	}

	public virtual IEnumerator DisableCollisionsWithMovingPlatforms(float duration)
	{
		if (DetachmentMethod == DetachmentMethods.Layer)
		{
			CollisionsOffWithMovingPlatformsLayer();
			if (m_platformWaitYield == null)
			{
				m_platformWaitYield = new WaitRL_Yield(duration);
			}
			else
			{
				m_platformWaitYield.CreateNew(duration);
			}
			yield return m_platformWaitYield;
			CollisionsOn();
		}
		else
		{
			SetIgnoreCollider(StandingOnCollider);
			if (m_platformWaitYield == null)
			{
				m_platformWaitYield = new WaitRL_Yield(duration);
			}
			else
			{
				m_platformWaitYield.CreateNew(duration);
			}
			yield return m_platformWaitYield;
			SetIgnoreCollider(null);
		}
	}

	public virtual void CollisionsOffWithOneWayPlatformsLayer()
	{
		PlatformMask = (int)PlatformMask - (int)OneWayPlatformMask;
		PlatformMask = (int)PlatformMask - (int)MovingOneWayPlatformMask;
		PlatformMask = (int)PlatformMask - (int)MidHeightOneWayPlatformMask;
	}

	public virtual void CollisionsOffWithMovingPlatformsLayer()
	{
		PlatformMask = (int)PlatformMask - (int)MovingPlatformMask;
		PlatformMask = (int)PlatformMask - (int)MovingOneWayPlatformMask;
	}

	public virtual void ResetParameters()
	{
		_overrideParameters = DefaultParameters;
	}

	public virtual void SlowFall(float factor)
	{
		_fallSlowFactor = factor;
	}

	public virtual void GravityActive(bool state)
	{
		if (state)
		{
			_gravityActive = true;
		}
		else
		{
			_gravityActive = false;
		}
	}

	public virtual void ResizeCollider(Vector2 newSize)
	{
		float num = _originalColliderOffset.y - (_originalColliderSize.y - newSize.y) / 2f;
		_boxCollider.size = newSize;
		_boxCollider.offset = num * Vector3.up;
		SetRaysParameters();
		State.ColliderResized = true;
	}

	public virtual void ResetColliderSize()
	{
		_boxCollider.size = _originalColliderSize;
		_boxCollider.offset = _originalColliderOffset;
		SetRaysParameters();
		State.ColliderResized = false;
	}

	public virtual bool CanGoBackToOriginalSize()
	{
		if (_boxCollider.size == _originalColliderSize)
		{
			return true;
		}
		float rayDistance = _originalColliderSize.y * base.transform.localScale.y * CrouchedRaycastLengthMultiplier;
		_originalSizeRaycastOrigin = _boundsTopLeftCorner + (Vector2)base.transform.up * 0.0001f;
		bool num = MMDebug.RayCast(_originalSizeRaycastOrigin, base.transform.up, rayDistance, (int)PlatformMask - (int)OneWayPlatformMask, Colors.LightSlateGray, drawGizmo: true);
		_originalSizeRaycastOrigin = _boundsTopRightCorner + (Vector2)base.transform.up * 0.0001f;
		bool flag = MMDebug.RayCast(_originalSizeRaycastOrigin, base.transform.up, rayDistance, (int)PlatformMask - (int)OneWayPlatformMask, Colors.LightSlateGray, drawGizmo: true);
		if (num || flag)
		{
			return false;
		}
		return true;
	}

	public virtual float Width()
	{
		return _boundsWidth;
	}

	public virtual float Height()
	{
		return _boundsHeight;
	}

	protected virtual void OnCorgiColliderHit()
	{
		foreach (RaycastHit2D contact in _contactList)
		{
			if (Parameters.Physics2DInteraction)
			{
				Rigidbody2D attachedRigidbody = contact.collider.attachedRigidbody;
				if (attachedRigidbody == null || attachedRigidbody.isKinematic)
				{
					break;
				}
				attachedRigidbody.velocity = new Vector3(_externalForce.x, 0f, 0f).normalized * Parameters.Physics2DPushForce;
			}
		}
	}

	protected virtual void OnTriggerEnter2D(Collider2D collider)
	{
		CorgiControllerPhysicsVolume2D componentNoAlloc = collider.gameObject.GetComponentNoAlloc<CorgiControllerPhysicsVolume2D>();
		if (componentNoAlloc != null)
		{
			_overrideParameters = componentNoAlloc.ControllerParameters;
			if (componentNoAlloc.ResetForcesOnEntry)
			{
				SetForce(Vector2.zero);
			}
			if (componentNoAlloc.MultiplyForcesOnEntry)
			{
				SetForce(Vector2.Scale(componentNoAlloc.ForceMultiplierOnEntry, Velocity));
			}
		}
	}

	protected virtual void OnTriggerStay2D(Collider2D collider)
	{
	}

	protected virtual void OnTriggerExit2D(Collider2D collider)
	{
		if (collider.gameObject.GetComponentNoAlloc<CorgiControllerPhysicsVolume2D>() != null)
		{
			_overrideParameters = null;
		}
	}
}
