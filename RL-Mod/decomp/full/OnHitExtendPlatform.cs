using System;
using System.Collections;
using UnityEngine;

public class OnHitExtendPlatform : SpecialPlatform, IBodyOnEnterHitResponse, IHitResponse, IRoomConsumer
{
	[SerializeField]
	private SpriteRenderer m_platformSprite;

	private BoxCollider2D m_platformCollider;

	private Collider2D m_bodyCollider;

	private BlinkPulseEffect m_blinkPulseEffect;

	private Animator m_animator;

	private int m_storedTerrainLayer;

	private int m_storedPlatformLayer;

	private int m_storedBodyLayer;

	private bool m_platformExtended;

	private bool m_isInitialized;

	public BaseRoom Room { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		m_animator = GetComponent<Animator>();
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		m_blinkPulseEffect = GetComponent<BlinkPulseEffect>();
		m_hbController.RepeatHitDuration = 0f;
		m_platformCollider = m_hbController.GetCollider(HitboxType.Platform) as BoxCollider2D;
		m_bodyCollider = m_hbController.GetCollider(HitboxType.Body);
		if (base.Width > 0f)
		{
			m_platformSprite.size = new Vector2(base.Width, m_platformSprite.size.y);
			m_platformCollider.size = new Vector2(base.Width, m_platformCollider.size.y);
		}
		m_storedPlatformLayer = m_platformCollider.gameObject.layer;
		m_storedBodyLayer = m_bodyCollider.gameObject.layer;
		m_isInitialized = true;
		SetPlatformState(m_platformExtended);
	}

	public override void SetState(StateID state)
	{
	}

	private void SetPlatformState(bool platformExtended)
	{
		m_platformExtended = platformExtended;
		if (m_platformExtended)
		{
			m_animator.SetBool("Open", value: true);
			m_platformCollider.gameObject.layer = m_storedPlatformLayer;
			m_bodyCollider.gameObject.layer = 2;
		}
		else
		{
			m_animator.SetBool("Open", value: false);
			m_animator.SetBool("Wobble", value: false);
			m_platformCollider.gameObject.layer = 2;
			m_bodyCollider.gameObject.layer = m_storedBodyLayer;
		}
	}

	public void BodyOnEnterHitResponse(IHitboxController otherHBController)
	{
		SetPlatformState(!m_platformExtended);
		StopAllCoroutines();
		m_blinkPulseEffect.StopInvincibilityEffect();
		StartCoroutine(RetractPlatformCoroutine());
	}

	private IEnumerator RetractPlatformCoroutine()
	{
		float startTime = Time.time;
		bool isBlinking = false;
		while (Time.time < startTime + 3.5f)
		{
			if (!isBlinking && Time.time > startTime + 3.5f - 0.5f)
			{
				m_blinkPulseEffect.StartInvincibilityEffect();
				m_animator.SetBool("Wobble", value: true);
				isBlinking = true;
			}
			yield return null;
		}
		m_blinkPulseEffect.StopInvincibilityEffect();
		SetPlatformState(!m_platformExtended);
	}

	private void OnPlayerExitRoom(object sender, EventArgs args)
	{
		ResetPlatform();
	}

	private void ResetPlatform()
	{
		StopAllCoroutines();
		m_blinkPulseEffect.StopInvincibilityEffect();
		m_platformExtended = false;
		m_animator.SetBool("Open", value: false);
		m_animator.SetBool("Wobble", value: false);
		m_platformCollider.gameObject.layer = 2;
		m_bodyCollider.gameObject.layer = m_storedBodyLayer;
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
}
