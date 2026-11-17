using System;
using System.Collections;
using System.Runtime.CompilerServices;
using RLAudio;
using UnityEngine;

public class SegmentedBreakablePlatform : SpecialPlatform, ITerrainOnStayHitResponse, IHitResponse, ITerrainOnEnterHitResponse, ITerrainOnExitHitResponse, IRootObj, IRoomConsumer, IBodyOnEnterHitResponse, IBodyOnStayHitResponse
{
	private const string SFX_SPAWN_NAME = "event:/SFX/Interactables/sfx_env_prop_mushroomPlatform_spawn";

	private const string SFX_BREAK_NAME = "event:/SFX/Interactables/sfx_env_prop_mushroomPlatform_destroyed";

	[SerializeField]
	private SpriteRenderer m_dottedOutlineSprite;

	protected BoxCollider2D m_terrainCollider;

	protected BoxCollider2D m_platformCollider;

	protected BoxCollider2D m_bodyCollider;

	protected int m_storedPlatformLayer;

	protected int m_storedTerrainLayer;

	protected int m_storedBodyLayer;

	protected bool m_crumbling;

	protected WaitRL_Yield m_waitYield;

	protected Animator m_animator;

	protected Coroutine m_crumbleCoroutine;

	protected Coroutine m_reformCoroutine;

	public BaseRoom Room { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		m_animator = GetComponent<Animator>();
		m_waitYield = new WaitRL_Yield(0f);
	}

	protected override IEnumerator Start()
	{
		m_hbController.RepeatHitDuration = 0f;
		m_terrainCollider = m_hbController.GetCollider(HitboxType.Terrain) as BoxCollider2D;
		m_platformCollider = m_hbController.GetCollider(HitboxType.Platform) as BoxCollider2D;
		m_bodyCollider = m_hbController.GetCollider(HitboxType.Body) as BoxCollider2D;
		m_platformCollider.tag = "TriggerHazard";
		if (base.Width > 0f)
		{
			SpriteRenderer componentInChildren = GetComponentInChildren<SpriteRenderer>();
			componentInChildren.size = new Vector2(base.Width, componentInChildren.size.y);
			m_terrainCollider.size = new Vector2(base.Width, m_terrainCollider.size.y);
			m_platformCollider.size = new Vector2(base.Width, m_platformCollider.size.y);
			m_bodyCollider.size = new Vector2(base.Width, m_bodyCollider.size.y);
			m_dottedOutlineSprite.size = new Vector2(base.Width, m_dottedOutlineSprite.size.y);
		}
		m_storedTerrainLayer = m_terrainCollider.gameObject.layer;
		m_storedPlatformLayer = m_platformCollider.gameObject.layer;
		m_storedBodyLayer = m_bodyCollider.gameObject.layer;
		yield break;
	}

	private void HandleProjectileCollision(IHitboxController otherHBController)
	{
		Projectile_RL component = otherHBController.RootGameObject.GetComponent<Projectile_RL>();
		if ((bool)component && (component.DieOnCharacterCollision || component.DieOnWallCollision))
		{
			component.FlagForDestruction();
		}
		m_crumbling = true;
		m_platformCollider.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
		m_terrainCollider.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
		m_bodyCollider.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
		m_animator.SetTrigger("Cracked");
		m_animator.SetTrigger("Crumble");
		AudioManager.PlayOneShot(null, "event:/SFX/Interactables/sfx_env_prop_mushroomPlatform_destroyed", base.transform.position);
		if (m_reformCoroutine != null)
		{
			StopCoroutine(m_reformCoroutine);
			m_reformCoroutine = null;
		}
		m_reformCoroutine = StartCoroutine(ReformCoroutine(4f));
	}

	public void BodyOnEnterHitResponse(IHitboxController otherHBController)
	{
		BodyOnStayHitResponse(otherHBController);
	}

	public void BodyOnStayHitResponse(IHitboxController otherHBController)
	{
		if (!m_crumbling)
		{
			HandleProjectileCollision(otherHBController);
		}
	}

	public void TerrainOnEnterHitResponse(IHitboxController otherHBController)
	{
		TerrainOnStayHitResponse(otherHBController);
	}

	public void TerrainOnStayHitResponse(IHitboxController otherHBController)
	{
		if (!m_crumbling)
		{
			PlayerController playerController = PlayerManager.GetPlayerController();
			if (playerController.IsGrounded && playerController.ControllerCorgi.StandingOnCollider == m_platformCollider)
			{
				m_platformCollider.tag = "TriggerHazard";
			}
			if (otherHBController.CollisionType != CollisionType.Player)
			{
				HandleProjectileCollision(otherHBController);
			}
		}
	}

	public void TerrainOnExitHitResponse(IHitboxController otherHBController)
	{
		m_platformCollider.tag = "TriggerHazard";
	}

	protected IEnumerator ReformCoroutine(float duration)
	{
		m_waitYield.CreateNew(duration);
		yield return m_waitYield;
		m_crumbling = false;
		m_animator.SetTrigger("Reform");
		AudioManager.PlayOneShot(null, "event:/SFX/Interactables/sfx_env_prop_mushroomPlatform_spawn", base.transform.position);
		m_platformCollider.gameObject.layer = m_storedPlatformLayer;
		m_terrainCollider.gameObject.layer = m_storedTerrainLayer;
		m_bodyCollider.gameObject.layer = m_storedBodyLayer;
	}

	public virtual void ResetCrumble()
	{
		if (m_crumbling)
		{
			m_animator.SetTrigger("Reset");
		}
		StopAllCoroutines();
		m_crumbleCoroutine = null;
		m_reformCoroutine = null;
		m_crumbling = false;
		m_animator.ResetTrigger("Cracked");
		m_animator.ResetTrigger("Reform");
		m_animator.ResetTrigger("Crumble");
		m_platformCollider.gameObject.layer = m_storedPlatformLayer;
		m_terrainCollider.gameObject.layer = m_storedTerrainLayer;
		m_bodyCollider.gameObject.layer = m_storedBodyLayer;
	}

	public override void SetState(StateID state)
	{
	}

	private void OnPlayerExitRoom(object sender, EventArgs args)
	{
		ResetCrumble();
	}

	public void SetRoom(BaseRoom room)
	{
		Room = room;
		Room.PlayerExitRelay.AddListener(OnPlayerExitRoom);
	}

	private void OnDestroy()
	{
		if (Room != null)
		{
			Room.PlayerExitRelay.RemoveListener(OnPlayerExitRoom);
		}
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
