using System;
using System.Collections;
using Rewired;
using UnityEngine;

public class TriggerSwitchPlatform : SpecialPlatform
{
	private bool m_platformVisible = true;

	private BoxCollider2D m_platformCollider;

	private int m_storedTerrainLayer;

	private int m_storedPlatformLayer;

	private bool m_isInitialized;

	private Animator m_animator;

	private Action<InputActionEventData> m_onSwitch;

	private void OnEnable()
	{
		Player player = Rewired_RL.Player;
		if (ReInput.isReady && Rewired_RL.Player != null)
		{
			player.AddInputEventDelegate(m_onSwitch, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Interact");
		}
	}

	private void OnDisable()
	{
		Player player = Rewired_RL.Player;
		if (ReInput.isReady && Rewired_RL.Player != null)
		{
			player.RemoveInputEventDelegate(m_onSwitch, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Interact");
		}
	}

	protected override void Awake()
	{
		m_onSwitch = OnSwitch;
		base.Awake();
		m_animator = GetComponent<Animator>();
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

	private void OnSwitch(InputActionEventData data)
	{
		m_platformVisible = !m_platformVisible;
		SetPlatformState();
	}

	private void SetPlatformState()
	{
		if (m_platformVisible)
		{
			m_animator.SetBool("Active", value: true);
			m_platformCollider.gameObject.layer = m_storedPlatformLayer;
		}
		else
		{
			m_animator.SetBool("Active", value: false);
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
