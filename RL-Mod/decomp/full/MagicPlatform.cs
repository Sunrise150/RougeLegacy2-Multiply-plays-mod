using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Sigtrap.Relays;
using UnityEngine;

public class MagicPlatform : SpecialPlatform, ITerrainOnStayHitResponse, IHitResponse, ITerrainOnEnterHitResponse, IRootObj, IRoomConsumer, IForcePlatformCollision
{
	[SerializeField]
	private float m_spawnedPlatformWidth = 4f;

	private BoxCollider2D m_terrainCollider;

	private BoxCollider2D m_platformCollider;

	private SpriteRenderer m_outlineSprite;

	private SpriteRenderer m_platformSprite;

	private Vector3 m_origColliderPosition;

	private bool m_active;

	private Animator m_animator;

	private bool m_playerIsInRoom;

	private Coroutine m_setInactiveCoroutine;

	private Action<MonoBehaviour, EventArgs> m_onPlayerJump;

	private static Relay<MonoBehaviour, EventArgs> SpawnPlatformRelay = new Relay<MonoBehaviour, EventArgs>();

	public BaseRoom Room { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		m_animator = GetComponent<Animator>();
		m_onPlayerJump = OnPlayerJump;
	}

	protected override IEnumerator Start()
	{
		m_hbController.RepeatHitDuration = 0f;
		m_terrainCollider = m_hbController.GetCollider(HitboxType.Terrain) as BoxCollider2D;
		m_platformCollider = m_hbController.GetCollider(HitboxType.Platform) as BoxCollider2D;
		m_platformCollider.tag = "MagicPlatform";
		if (base.Width > 0f)
		{
			SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
			m_outlineSprite = componentsInChildren[0];
			m_platformSprite = componentsInChildren[1];
			m_outlineSprite.size = new Vector2(base.Width, m_outlineSprite.size.y);
			m_platformSprite.size = new Vector2(m_spawnedPlatformWidth + 2f, m_platformSprite.size.y);
			m_origColliderPosition = m_terrainCollider.transform.position;
			m_terrainCollider.size = new Vector2(base.Width, m_terrainCollider.size.y);
			m_platformCollider.size = new Vector2(base.Width, m_platformCollider.size.y);
		}
		SpawnPlatformRelay.AddListener(OnSpawnPlatform);
		yield break;
	}

	public void ForcePlatformCollision()
	{
	}

	public void TerrainOnEnterHitResponse(IHitboxController otherHBController)
	{
		TerrainOnStayHitResponse(otherHBController);
	}

	public void TerrainOnStayHitResponse(IHitboxController otherHBController)
	{
		if (!m_active)
		{
			PlayerController playerController = PlayerManager.GetPlayerController();
			if (playerController.IsGrounded && playerController.ControllerCorgi.StandingOnCollider == m_platformCollider)
			{
				SpawnPlatformRelay.Dispatch(this, null);
				m_active = true;
				m_animator.SetBool("Active", value: true);
				float num = base.Width / 2f - m_spawnedPlatformWidth / 2f;
				float x = Mathf.Clamp(PlayerManager.GetPlayer().transform.position.x, base.transform.position.x - num, base.transform.position.x + num);
				m_platformSprite.transform.position = new Vector2(x, m_platformSprite.transform.position.y);
				m_terrainCollider.size = new Vector2(m_spawnedPlatformWidth, m_terrainCollider.size.y);
				m_terrainCollider.transform.position = new Vector2(x, m_terrainCollider.transform.position.y);
				m_platformCollider.size = new Vector2(m_spawnedPlatformWidth, m_platformCollider.size.y);
				m_platformCollider.transform.position = new Vector2(x, m_platformCollider.transform.position.y);
				m_platformCollider.gameObject.layer = 8;
			}
		}
	}

	private void OnPlayerEnterRoom(object sender, EventArgs args)
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerJump, m_onPlayerJump);
		m_playerIsInRoom = true;
	}

	private void OnPlayerExitRoom(object sender, EventArgs args)
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerJump, m_onPlayerJump);
		m_playerIsInRoom = false;
		SetInactive();
	}

	private void OnPlayerJump(object sender, EventArgs eventArgs)
	{
		SetInactive();
	}

	private void OnSpawnPlatform(object sender, EventArgs eventArgs)
	{
		SetInactive();
	}

	private IEnumerator SetInactiveCoroutine(float delay)
	{
		if (m_active)
		{
			delay = Time.time + delay;
			while (Time.time < delay)
			{
				yield return null;
			}
			SetInactive();
		}
	}

	private void SetInactive()
	{
		if (m_active)
		{
			m_active = false;
			m_terrainCollider.size = new Vector2(base.Width, m_terrainCollider.size.y);
			m_terrainCollider.transform.position = m_origColliderPosition;
			m_platformCollider.size = new Vector2(base.Width, m_platformCollider.size.y);
			m_platformCollider.transform.position = m_origColliderPosition;
			m_platformCollider.gameObject.layer = 11;
			m_animator.SetBool("Active", value: false);
		}
	}

	public override void SetState(StateID state)
	{
	}

	public void SetRoom(BaseRoom room)
	{
		Room = room;
		Room.PlayerEnterRelay.AddListener(OnPlayerEnterRoom);
		Room.PlayerExitRelay.AddListener(OnPlayerExitRoom);
	}

	private void OnDestroy()
	{
		if ((bool)Room)
		{
			Room.PlayerEnterRelay.RemoveListener(OnPlayerEnterRoom);
			Room.PlayerExitRelay.RemoveListener(OnPlayerExitRoom);
		}
		SpawnPlatformRelay.RemoveListener(OnSpawnPlatform);
		if (m_playerIsInRoom)
		{
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerJump, OnPlayerJump);
		}
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
