using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Sigtrap.Relays;
using UnityEngine;

public class Cloud : MonoBehaviour, IDamageObj, IRoomConsumer, ITerrainOnEnterHitResponse, IHitResponse, ITerrainOnStayHitResponse
{
	private static bool m_alreadyHitCloud;

	[SerializeField]
	private AnimationCurve m_animCurve;

	private Vector2 m_knockbackMod = new Vector2(1f, 1.8f);

	private float m_knockbackStrength = 99f;

	private float m_baseStunStrength;

	private Vector2 m_playerPositionOnEnterRoom;

	private WaitRL_Yield m_waitYield;

	private IHitboxController m_hbController;

	private int m_charHitFrameCount;

	private int m_cloudHitFrameCount;

	private bool m_disableDamage;

	private Tween m_teleportTween;

	private bool m_playerInputDisabled;

	private Action<object, CharacterHitEventArgs> m_onPlayerJustHit;

	public Relay<Vector2> PlayerHitRelay = new Relay<Vector2>();

	public Relay TeleportStartRelay = new Relay();

	public Relay<Vector2> TeleportCompleteRelay = new Relay<Vector2>();

	public static bool HittingCloud => m_alreadyHitCloud;

	public StatusEffectType[] StatusEffectTypes => null;

	public float[] StatusEffectDurations => null;

	public bool IsDotDamage => false;

	public float ActualCritChance => 0f;

	public float ActualCritDamage => 0f;

	public virtual float ActualDamage
	{
		get
		{
			if (PlayerManager.IsInstantiated)
			{
				PlayerController playerController = PlayerManager.GetPlayerController();
				if (m_disableDamage)
				{
					return 0f;
				}
				float num = 0f;
				Room room = Room as Room;
				if (room != null && room.GridPointManager.RoomMetaData.SpecialRoomType == SpecialRoomType.Heirloom)
				{
					num = 0f;
					return (float)playerController.BaseMaxHealth * num;
				}
				return Hazard_EV.GetDamageAmount(Room);
			}
			return 0f;
		}
	}

	public float ActualKnockbackStrength => BaseKnockbackStrength;

	public float ActualStunStrength => BaseStunStrength;

	public virtual float BaseDamage => ActualDamage;

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

	public float BaseKnockbackStrength
	{
		get
		{
			return m_knockbackStrength;
		}
		set
		{
			m_knockbackStrength = value;
		}
	}

	public Vector2 ExternalKnockbackMod => m_knockbackMod;

	public float KnockbackStrength => m_knockbackStrength;

	public StrikeType StrikeType => StrikeType.Blunt;

	public BaseRoom Room { get; private set; }

	private void Awake()
	{
		m_waitYield = new WaitRL_Yield(0f);
		m_hbController = base.gameObject.GetComponentInChildren<IHitboxController>(includeInactive: true);
		base.tag = "Hazard";
		m_onPlayerJustHit = OnPlayerJustHit;
	}

	private void OnEnable()
	{
		if (PlayerManager.IsInstantiated)
		{
			PlayerManager.GetPlayerController().CharacterHitResponse.OnCharacterHitRelay.AddListener(m_onPlayerJustHit);
		}
	}

	private void OnDisable()
	{
		if (PlayerManager.IsInstantiated && !PlayerManager.IsDisposed)
		{
			PlayerController playerController = PlayerManager.GetPlayerController();
			playerController.CharacterHitResponse.OnCharacterHitRelay.RemoveListener(m_onPlayerJustHit);
			if (m_teleportTween != null)
			{
				m_teleportTween.StopTweenWithConditionChecks(runEndHandlers: false, playerController.transform);
			}
			if (m_alreadyHitCloud)
			{
				playerController.Visuals.SetActive(value: true);
				playerController.ControllerCorgi.GravityActive(state: true);
				playerController.ControllerCorgi.CollisionsOn();
				playerController.HitboxController.SetHitboxActiveState(HitboxType.Body, active: true);
			}
			if (m_playerInputDisabled)
			{
				if (playerController.CurrentHealth > 0f)
				{
					playerController.CharacterHitResponse.SetInvincibleTime(1f, additive: false, playInvincibility: false);
					RewiredMapController.SetMapEnabled(GameInputMode.Game, enabled: true);
				}
				m_playerInputDisabled = false;
			}
		}
		m_alreadyHitCloud = false;
	}

	private void OnPlayerJustHit(object sender, EventArgs args)
	{
		m_charHitFrameCount = Time.frameCount;
	}

	private void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		m_playerPositionOnEnterRoom = PlayerManager.GetPlayer().transform.position;
		if (m_hbController != null && m_hbController.IsInitialized)
		{
			m_hbController.RepeatHitDuration = 0f;
		}
	}

	private void OnPlayerHitCloud()
	{
		if (!m_alreadyHitCloud && !LocalTeleporterController.IsTeleporting)
		{
			m_cloudHitFrameCount = Time.frameCount;
			StopAllCoroutines();
			StartCoroutine(HitCloudCoroutine());
		}
	}

	private IEnumerator HitCloudCoroutine()
	{
		Vector3 position = PlayerManager.GetPlayerController().transform.position;
		PlayerHitRelay.Dispatch(position);
		m_alreadyHitCloud = true;
		m_disableDamage = false;
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.StopActiveAbilities(stopPersistentAbilities: false);
		bool disableInput = true;
		if (m_charHitFrameCount != m_cloudHitFrameCount && (playerController.CharacterHitResponse.IsInvincible || playerController.CharacterHitResponse.IsStunned))
		{
			m_disableDamage = true;
			disableInput = false;
			playerController.CharacterHitResponse.StartHitResponse(m_hbController.RootGameObject, m_hbController.DamageObj);
		}
		yield return null;
		RewiredMapController.SetMapEnabled(GameInputMode.Game, !disableInput);
		m_playerInputDisabled = true;
		if (!m_disableDamage)
		{
			m_waitYield.CreateNew(0.25f);
			yield return m_waitYield;
		}
		if (!LocalTeleporterController.IsTeleporting)
		{
			playerController.ControllerCorgi.CollisionsOff();
			playerController.SetVelocity(0f, 0f, additive: false);
			playerController.ControllerCorgi.GravityActive(state: false);
			playerController.HitboxController.SetHitboxActiveState(HitboxType.Body, active: false);
			EffectManager.PlayEffect(playerController.gameObject, playerController.Animator, "CloudHazardPoof_Effect", playerController.Midpoint, 0f, EffectStopType.Gracefully);
			playerController.Visuals.SetActive(value: false);
			playerController.CharacterHitResponse.SetInvincibleTime(999f, additive: false, playInvincibility: false);
			TeleportStartRelay.Dispatch();
			BaseEffect trailEffect = EffectManager.PlayEffect(playerController.gameObject, playerController.Animator, "CloudHazardSpark_Effect", playerController.Midpoint, 0f, EffectStopType.Gracefully);
			trailEffect.transform.SetParent(playerController.transform);
			Vector3 localPosition = playerController.Midpoint - playerController.transform.position;
			localPosition.z = trailEffect.transform.localPosition.z;
			trailEffect.transform.localPosition = localPosition;
			StartCoroutine(TrailAnimCurveCoroutine(0.25f, 5f, trailEffect.transform));
			Vector3 previousPosition2 = PlayerManager.GetPlayerController().ControllerCorgi.LastStandingPosition;
			previousPosition2 = FindSaferPosition(previousPosition2);
			if (m_teleportTween != null)
			{
				m_teleportTween.StopTweenWithConditionChecks(runEndHandlers: false, playerController.transform);
			}
			m_teleportTween = TweenManager.TweenTo(playerController.transform, 0.25f, Ease.Quad.EaseInOut, "position.x", previousPosition2.x, "position.y", previousPosition2.y);
			yield return m_teleportTween.TweenCoroutine;
			TeleportCompleteRelay.Dispatch(previousPosition2);
			playerController.Visuals.SetActive(value: true);
			playerController.ControllerCorgi.GravityActive(state: true);
			playerController.ControllerCorgi.CollisionsOn();
			playerController.HitboxController.SetHitboxActiveState(HitboxType.Body, active: true);
			playerController.CharacterJump.ResetNumberOfJumps();
			playerController.CharacterDash.ResetNumberOfDashes();
			EffectManager.PlayEffect(playerController.gameObject, playerController.Animator, "CloudHazardPoof_Effect", playerController.Midpoint, 0f, EffectStopType.Gracefully);
			trailEffect.Stop(EffectStopType.Gracefully);
			m_alreadyHitCloud = false;
			m_waitYield.CreateNew(0.25f);
			yield return m_waitYield;
			if (playerController.CurrentHealth > 0f)
			{
				playerController.CharacterHitResponse.SetInvincibleTime(1f, additive: false, playInvincibility: false);
				RewiredMapController.SetMapEnabled(GameInputMode.Game, enabled: true);
			}
			m_playerInputDisabled = false;
		}
	}

	private Vector3 FindSaferPosition(Vector3 currentPos)
	{
		Collider2D collider = m_hbController.GetCollider(HitboxType.Terrain);
		CorgiController_RL controllerCorgi = PlayerManager.GetPlayerController().ControllerCorgi;
		float boundsWidth = controllerCorgi.BoundsWidth;
		float num = boundsWidth / 2f;
		float num2 = controllerCorgi.BoundsHeight / 2f;
		float distance = num2 + 0.1f;
		Vector2 origin = currentPos;
		origin.y += num2;
		bool flag = false;
		bool flag2 = false;
		LayerMask savedPlatformMask = controllerCorgi.SavedPlatformMask;
		savedPlatformMask = (int)savedPlatformMask | (int)controllerCorgi.OneWayPlatformMask;
		origin.x = currentPos.x - boundsWidth;
		RaycastHit2D raycastHit2D = Physics2D.Raycast(origin, Vector2.down, distance, savedPlatformMask);
		Vector2 vector = currentPos;
		if ((bool)raycastHit2D && raycastHit2D.distance > 0f)
		{
			Vector2 point;
			Vector2 point2 = (point = raycastHit2D.point);
			point2.x -= num;
			point.x += num;
			if (!collider.OverlapPoint(point2) && !collider.OverlapPoint(point) && Mathf.Abs(Vector2.Angle(raycastHit2D.normal, base.transform.up)) <= controllerCorgi.Parameters.MaximumSlopeAngle)
			{
				flag2 = true;
				vector = raycastHit2D.point;
			}
		}
		origin.x = currentPos.x + boundsWidth;
		raycastHit2D = Physics2D.Raycast(origin, Vector2.down, distance, savedPlatformMask);
		Vector2 vector2 = currentPos;
		if ((bool)raycastHit2D && raycastHit2D.distance > 0f)
		{
			Vector2 point;
			Vector2 point2 = (point = raycastHit2D.point);
			point2.x -= num;
			point.x += num;
			if (!collider.OverlapPoint(point2) && !collider.OverlapPoint(point) && Mathf.Abs(Vector2.Angle(raycastHit2D.normal, base.transform.up)) <= controllerCorgi.Parameters.MaximumSlopeAngle)
			{
				flag = true;
				vector2 = raycastHit2D.point;
			}
		}
		if (!flag2 && flag)
		{
			currentPos = vector2;
		}
		else if (flag2 && !flag)
		{
			currentPos = vector;
		}
		return currentPos;
	}

	private IEnumerator TrailAnimCurveCoroutine(float duration, float intensity, Transform transform)
	{
		float startTime = Time.time;
		while (Time.time - startTime < duration)
		{
			float time = (Time.time - startTime) / duration;
			Vector3 localPosition = transform.localPosition;
			localPosition.y = m_animCurve.Evaluate(time) * intensity;
			transform.localPosition = localPosition;
			yield return null;
		}
	}

	private void OnRoomDestroyed(object sender, EventArgs eventArgs)
	{
		ClearEventHandlers();
	}

	private void ClearEventHandlers()
	{
		if ((bool)Room)
		{
			Room.PlayerEnterRelay.RemoveListener(OnPlayerEnterRoom);
			Room.RoomDestroyedRelay.RemoveListener(OnRoomDestroyed);
		}
	}

	private void OnDestroy()
	{
		ClearEventHandlers();
	}

	public void SetRoom(BaseRoom room)
	{
		ClearEventHandlers();
		Room = room;
		Room.PlayerEnterRelay.AddListener(OnPlayerEnterRoom);
		Room.RoomDestroyedRelay.AddListener(OnRoomDestroyed);
	}

	public void TerrainOnEnterHitResponse(IHitboxController otherHBController)
	{
		if (otherHBController.RootGameObject.CompareTag("Player") || otherHBController.RootGameObject.CompareTag("Player_Dodging"))
		{
			OnPlayerHitCloud();
		}
	}

	public void TerrainOnStayHitResponse(IHitboxController otherHBController)
	{
		TerrainOnEnterHitResponse(otherHBController);
	}

	[SpecialName]
	GameObject IDamageObj.get_gameObject()
	{
		return base.gameObject;
	}
}
