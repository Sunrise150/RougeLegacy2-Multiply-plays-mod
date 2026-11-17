using System;
using System.Collections;
using UnityEngine;

public class AlternateJumpPlatform : SpecialPlatform
{
	private bool m_platformVisible = true;

	private Renderer m_renderer;

	private BoxCollider2D m_platformCollider;

	private int m_storedTerrainLayer;

	private int m_storedPlatformLayer;

	private bool m_isInitialized;

	private Action<MonoBehaviour, EventArgs> m_onPlayerJump;

	private void OnEnable()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerJump, m_onPlayerJump);
	}

	private void OnDisable()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerJump, m_onPlayerJump);
	}

	protected override void Awake()
	{
		base.Awake();
		m_renderer = GetComponentInChildren<Renderer>();
		m_onPlayerJump = OnPlayerJump;
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		m_hbController.RepeatHitDuration = 0f;
		m_platformCollider = m_hbController.GetCollider(HitboxType.Platform) as BoxCollider2D;
		if (base.Width > 0f)
		{
			SpriteRenderer componentInChildren = GetComponentInChildren<SpriteRenderer>();
			componentInChildren.size = new Vector2(base.Width, componentInChildren.size.y);
			m_platformCollider.size = new Vector2(base.Width, m_platformCollider.size.y);
		}
		m_storedPlatformLayer = m_platformCollider.gameObject.layer;
		m_isInitialized = true;
		SetPlatformState();
	}

	private void OnPlayerJump(MonoBehaviour sender, EventArgs args)
	{
		m_platformVisible = !m_platformVisible;
		SetPlatformState();
	}

	private void SetPlatformState()
	{
		if (m_platformVisible)
		{
			m_renderer.enabled = true;
			m_platformCollider.gameObject.layer = m_storedPlatformLayer;
		}
		else
		{
			m_renderer.enabled = false;
			m_platformCollider.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
		}
	}

	public override void SetState(StateID state)
	{
		switch (state)
		{
		case StateID.One:
			m_platformVisible = true;
			break;
		case StateID.Two:
			m_platformVisible = false;
			break;
		case StateID.Random:
			if (CDGHelper.RandomPlusMinus() > 0)
			{
				m_platformVisible = true;
			}
			else
			{
				m_platformVisible = false;
			}
			break;
		}
		if (m_isInitialized)
		{
			SetPlatformState();
		}
	}
}
