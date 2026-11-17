using System;
using System.Collections;
using RLAudio;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EarthShiftPlatformController : MonoBehaviour, IRoomConsumer
{
	public enum EarthShiftPlatformSize
	{
		Small,
		Medium,
		Large,
		Final
	}

	private const string TELEPORT_HIT_SFX_NAME = "event:/SFX/Weapons/sfx_player_splash_return";

	[SerializeField]
	private GameObject m_destination;

	[SerializeField]
	private float m_timeToReachDestination;

	[SerializeField]
	private PropSpawnController m_triggerSpawnController;

	[SerializeField]
	private EarthShiftPlatformSize m_platformSize;

	[SerializeField]
	private string m_sfxNameLoopOverride;

	[SerializeField]
	private string m_sfxNameStopOverride;

	[SerializeField]
	private bool m_movesThroughWater;

	private Vector3 m_roomRelativeStartingPos;

	private float m_progress;

	private Collider2D m_collider;

	private Collider2D[] m_overlappingColliders = new Collider2D[1];

	private WaitRL_Yield m_waitYield;

	private Tween m_teleportTween;

	private MeshRenderer m_renderer;

	private Action<MonoBehaviour, EventArgs> m_onPlayerEnterRoom;

	public string SFXNameLoopOverride => m_sfxNameLoopOverride;

	public string SFXNameStopOverride => m_sfxNameStopOverride;

	public bool MovesThroughWater => m_movesThroughWater;

	public EarthShiftPlatformSize PlatformSize => m_platformSize;

	public MeshRenderer Renderer => m_renderer;

	public float Progress => m_progress;

	public BaseRoom Room { get; private set; }

	public void SetRoom(BaseRoom room)
	{
		Room = room;
		m_roomRelativeStartingPos = base.transform.position - room.transform.position;
	}

	private void Awake()
	{
		if (!m_triggerSpawnController)
		{
			throw new Exception("No trigger spawn controller was assigned for this EarthShiftPlatformController!");
		}
		if (!m_destination)
		{
			throw new Exception("EarthShiftPlatformController is missing a destination!");
		}
		if (!GetComponent<Ferr2DT_PathTerrain>().DisableMerging)
		{
			throw new Exception("EarthShiftPlatformController is attached to a platform that does NOT have merging disabled!");
		}
		m_collider = GetComponent<Collider2D>();
		m_renderer = GetComponent<MeshRenderer>();
		GetComponent<Rigidbody2D>().isKinematic = true;
		m_onPlayerEnterRoom = OnPlayerEnterRoom;
		m_waitYield = new WaitRL_Yield(0f);
	}

	private void OnEnable()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
	}

	private void OnPlayerEnterRoom(MonoBehaviour sender, EventArgs args)
	{
		base.gameObject.tag = "MagicPlatform";
		ResetToOrigin();
		m_triggerSpawnController.PropInstance.GetComponent<EarthShiftTriggerController>().SetPlatform(this);
	}

	private void OnDisable()
	{
		ResetToOrigin();
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
	}

	private void ResetToOrigin()
	{
		m_progress = 0f;
		if ((bool)Room)
		{
			base.transform.position = Room.transform.position + m_roomRelativeStartingPos;
		}
	}

	public void Move()
	{
		m_progress += Time.deltaTime / m_timeToReachDestination;
		m_progress = Mathf.Clamp(m_progress, 0f, 1f);
		base.transform.position = Vector3.Lerp(Room.transform.position + m_roomRelativeStartingPos, m_destination.transform.position, m_progress);
	}

	public void ResetPosition()
	{
		ResetToOrigin();
		bool flag = false;
		ContactFilter2D contactFilter = default(ContactFilter2D);
		contactFilter.NoFilter();
		contactFilter.SetLayerMask(1048576);
		if (Physics2D.OverlapCollider(m_collider, contactFilter, m_overlappingColliders) > 0 && m_overlappingColliders[0].CompareTag("Player"))
		{
			flag = true;
		}
		if (flag)
		{
			StartCoroutine(TeleportPlayerCoroutine());
		}
	}

	private IEnumerator TeleportPlayerCoroutine()
	{
		RewiredMapController.SetMapEnabled(GameInputMode.Game, enabled: false);
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.ControllerCorgi.CollisionsOff();
		playerController.SetVelocity(0f, 0f, additive: false);
		playerController.ControllerCorgi.GravityActive(state: false);
		playerController.HitboxController.SetHitboxActiveState(HitboxType.Body, active: false);
		EffectManager.PlayEffect(playerController.gameObject, playerController.Animator, "CloudHazardPoof_Effect", playerController.Midpoint, 0f, EffectStopType.Gracefully);
		playerController.Visuals.SetActive(value: false);
		playerController.CharacterHitResponse.SetInvincibleTime(999f, additive: false, playInvincibility: false);
		BaseEffect trailEffect = EffectManager.PlayEffect(playerController.gameObject, playerController.Animator, "CloudHazardSpark_Effect", playerController.Midpoint, 0f, EffectStopType.Gracefully);
		trailEffect.transform.SetParent(playerController.transform);
		Vector3 localPosition = playerController.Midpoint - playerController.transform.position;
		localPosition.z = trailEffect.transform.localPosition.z;
		trailEffect.transform.localPosition = localPosition;
		Vector3 position = m_triggerSpawnController.PropInstance.transform.position;
		playerController.ControllerCorgi.SetLastStandingPosition(position);
		AudioManager.PlayOneShot(null, "event:/SFX/Weapons/sfx_player_splash_return", playerController.Midpoint);
		if (m_teleportTween != null)
		{
			m_teleportTween.StopTweenWithConditionChecks(runEndHandlers: false, playerController.transform);
		}
		m_teleportTween = TweenManager.TweenTo(playerController.transform, 0.25f, Ease.Quad.EaseInOut, "position.x", position.x, "position.y", position.y);
		yield return m_teleportTween.TweenCoroutine;
		playerController.Visuals.SetActive(value: true);
		playerController.ControllerCorgi.GravityActive(state: true);
		playerController.ControllerCorgi.CollisionsOn();
		playerController.HitboxController.SetHitboxActiveState(HitboxType.Body, active: true);
		playerController.CharacterJump.ResetNumberOfJumps();
		playerController.CharacterDash.ResetNumberOfDashes();
		EffectManager.PlayEffect(playerController.gameObject, playerController.Animator, "CloudHazardPoof_Effect", playerController.Midpoint, 0f, EffectStopType.Gracefully);
		trailEffect.Stop(EffectStopType.Gracefully);
		m_waitYield.CreateNew(0.25f);
		yield return m_waitYield;
		if (playerController.CurrentHealth > 0f)
		{
			playerController.CharacterHitResponse.SetInvincibleTime(1f, additive: false, playInvincibility: false);
			RewiredMapController.SetMapEnabled(GameInputMode.Game, enabled: true);
		}
	}
}
