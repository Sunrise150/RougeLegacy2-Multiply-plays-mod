using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CrumblePlatform : SpecialPlatform, ITerrainOnStayHitResponse, IHitResponse, ITerrainOnEnterHitResponse, ITerrainOnExitHitResponse, IRootObj, IRoomConsumer, IForcePlatformCollision
{
	[SerializeField]
	private SpriteRenderer m_dottedOutlineSprite;

	protected BoxCollider2D m_terrainCollider;

	protected BoxCollider2D m_platformCollider;

	protected bool m_crumbling;

	protected int m_storedPlatformLayer;

	protected int m_storedTerrainLayer;

	protected WaitRL_Yield m_waitYield;

	protected Animator m_animator;

	protected Coroutine m_crumbleCoroutine;

	protected Coroutine m_reformCoroutine;

	private bool m_isTouchingPlatform;

	public BaseRoom Room { get; private set; }

	private void OnPlayerExitRoom(object sender, EventArgs args)
	{
		ResetCrumble();
	}

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
		m_platformCollider.tag = "MagicPlatform";
		if (base.Width > 0f)
		{
			SpriteRenderer componentInChildren = GetComponentInChildren<SpriteRenderer>();
			componentInChildren.size = new Vector2(base.Width, componentInChildren.size.y);
			m_terrainCollider.size = new Vector2(base.Width, m_terrainCollider.size.y);
			m_platformCollider.size = new Vector2(base.Width, m_platformCollider.size.y);
			m_dottedOutlineSprite.size = new Vector2(base.Width, m_dottedOutlineSprite.size.y);
		}
		m_storedTerrainLayer = m_terrainCollider.gameObject.layer;
		m_storedPlatformLayer = m_platformCollider.gameObject.layer;
		yield break;
	}

	public void ForcePlatformCollision()
	{
		TerrainOnStayHitResponse(null);
	}

	public void TerrainOnEnterHitResponse(IHitboxController otherHBController)
	{
		TerrainOnStayHitResponse(otherHBController);
	}

	public void TerrainOnStayHitResponse(IHitboxController otherHBController)
	{
		if (m_crumbling)
		{
			return;
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		if (playerController.IsGrounded && playerController.ControllerCorgi.StandingOnCollider == m_platformCollider)
		{
			m_platformCollider.tag = "TriggerHazard";
			m_isTouchingPlatform = true;
			m_crumbling = true;
			m_animator.SetTrigger("Cracked");
			if (m_crumbleCoroutine != null)
			{
				StopCoroutine(m_crumbleCoroutine);
				m_crumbleCoroutine = null;
			}
			m_crumbleCoroutine = StartCoroutine(StartCrumbleCoroutine());
		}
	}

	public void TerrainOnExitHitResponse(IHitboxController otherHBController)
	{
		m_isTouchingPlatform = false;
		m_platformCollider.tag = "MagicPlatform";
	}

	protected virtual IEnumerator StartCrumbleCoroutine()
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		while (playerController.IsGrounded && m_isTouchingPlatform)
		{
			yield return null;
		}
		m_platformCollider.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
		m_terrainCollider.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
		m_animator.SetTrigger("Crumble");
		if (m_reformCoroutine != null)
		{
			StopCoroutine(m_reformCoroutine);
			m_reformCoroutine = null;
		}
		m_reformCoroutine = StartCoroutine(ReformCoroutine(2f));
	}

	protected IEnumerator ReformCoroutine(float duration)
	{
		m_waitYield.CreateNew(duration);
		yield return m_waitYield;
		m_platformCollider.gameObject.layer = m_storedPlatformLayer;
		m_terrainCollider.gameObject.layer = m_storedTerrainLayer;
		m_animator.SetTrigger("Reform");
		m_crumbling = false;
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
	}

	public override void SetState(StateID state)
	{
	}

	public void SetRoom(BaseRoom room)
	{
		Room = room;
		Room.PlayerExitRelay.AddListener(OnPlayerExitRoom);
	}

	private void OnDestroy()
	{
		if ((bool)Room)
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
