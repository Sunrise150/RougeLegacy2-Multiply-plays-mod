using System;
using System.Collections;
using Rewired;
using UnityEngine;

public class PlayerTrigger : MonoBehaviour
{
	[SerializeField]
	private bool m_requireButtonPress;

	[SerializeField]
	private GameObject m_pressButtonIndicator;

	[SerializeField]
	private bool m_printDebug;

	private Collider2D m_collider;

	public bool IsTriggerActive
	{
		get
		{
			if (m_collider != null)
			{
				return m_collider.enabled;
			}
			return false;
		}
	}

	public bool IsPlayerInTrigger { get; private set; }

	public event EventHandler<EventArgs> PlayerEnter;

	public event EventHandler<EventArgs> PlayerExit;

	private IEnumerator Start()
	{
		if (m_requireButtonPress)
		{
			if (m_pressButtonIndicator == null)
			{
				Debug.LogFormat("<color=yellow>{0}: UI Window Trigger on ({1}) requires a Button Press, but you haven't set a Press Button Indicator</color>", Time.frameCount, base.name);
			}
			Debug.LogFormat("{0}: ({1}) is waiting for Rewired...", Time.frameCount, this);
			yield return new WaitUntil(() => ReInput.isReady);
			Debug.LogFormat("{0}: ({1}) finished waiting for Rewired", Time.frameCount, this);
			ReInput.players.GetPlayer(0).AddInputEventDelegate(OnInteractButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Interact");
		}
	}

	private void OnEnable()
	{
		Reset();
	}

	private void OnDestroy()
	{
		if (ReInput.isReady)
		{
			ReInput.players.GetPlayer(0).RemoveInputEventDelegate(OnInteractButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Interact");
		}
	}

	public static PlayerTrigger CreateInstance(Vector2 size, Vector2 position, Transform parent = null)
	{
		GameObject gameObject = new GameObject("Exit");
		gameObject.transform.SetParent(parent);
		gameObject.transform.position = position;
		gameObject.layer = LayerMask.NameToLayer("Terrain_Hitbox");
		PlayerTrigger playerTrigger = gameObject.AddComponent<PlayerTrigger>();
		gameObject.AddComponent<Rigidbody2D>().isKinematic = true;
		playerTrigger.m_collider = gameObject.AddComponent<BoxCollider2D>();
		playerTrigger.m_collider.isTrigger = true;
		(playerTrigger.m_collider as BoxCollider2D).size = size;
		return playerTrigger;
	}

	public void SetTriggerActive(bool isActive)
	{
		if (isActive)
		{
			Reset();
		}
		if (m_collider == null)
		{
			m_collider = GetComponent<Collider2D>();
			if (m_collider == null)
			{
				Debug.LogFormat("<color=red>{0}: No Collider found on GameObject.</color>", Time.frameCount);
				return;
			}
		}
		m_collider.enabled = isActive;
	}

	public void Reset()
	{
		IsPlayerInTrigger = false;
		if (m_pressButtonIndicator != null)
		{
			m_pressButtonIndicator.SetActive(value: false);
		}
	}

	private void OnInteractButtonDown(InputActionEventData inputActionEventData)
	{
		if (m_requireButtonPress && IsPlayerInTrigger && !PlayerManager.GetPlayerController().CharacterCorgi.IsFrozen)
		{
			if (m_printDebug)
			{
				Debug.LogFormat("{0}: Player pressed interact Button on ({1})", Time.frameCount, base.name);
			}
			if (this.PlayerEnter != null)
			{
				this.PlayerEnter(this, EventArgs.Empty);
			}
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Player") || collision.CompareTag("Player_Dodging"))
		{
			PlayerEnterTrigger();
		}
	}

	private void PlayerEnterTrigger()
	{
		if (m_printDebug)
		{
			Debug.LogFormat("<color=green>{0}: Player Entered PlayerTrigger</color>", Time.frameCount);
		}
		IsPlayerInTrigger = true;
		if (m_requireButtonPress && m_pressButtonIndicator != null)
		{
			m_pressButtonIndicator.SetActive(value: true);
		}
		else if (this.PlayerEnter != null)
		{
			this.PlayerEnter(this, EventArgs.Empty);
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (m_printDebug)
		{
			Debug.LogFormat("<color=purple>{0}: Player Exited PlayerTrigger</color>", Time.frameCount);
		}
		if (collision.CompareTag("Player") || collision.CompareTag("Player_Dodging"))
		{
			IsPlayerInTrigger = false;
			if (m_requireButtonPress && m_pressButtonIndicator != null)
			{
				m_pressButtonIndicator.SetActive(value: false);
			}
			else if (this.PlayerExit != null)
			{
				this.PlayerExit(this, EventArgs.Empty);
			}
		}
	}
}
