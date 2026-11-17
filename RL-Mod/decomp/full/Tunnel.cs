using System;
using System.Collections;
using System.Runtime.CompilerServices;
using SceneManagement_RL;
using UnityEngine;
using UnityEngine.Events;

public class Tunnel : MonoBehaviour, IRoomConsumer, IRootObj
{
	private static Vector3 OFFSET = new Vector3(0f, 0.1f, 0f);

	[SerializeField]
	protected Interactable m_interactable;

	[SerializeField]
	private GameObject m_visuals;

	[SerializeField]
	private UnityEvent m_enterUnityEvent;

	[SerializeField]
	private UnityEvent m_exitUnityEvent;

	private BaseRoom m_room;

	private Tunnel m_destination;

	protected Tunnel m_tempDestinationOverride;

	public Interactable Interactable => m_interactable;

	public bool IsInstantiated { get; private set; }

	protected bool IsCutsceneTeleport => m_tempDestinationOverride;

	public Tunnel Destination
	{
		get
		{
			if (!m_tempDestinationOverride)
			{
				return m_destination;
			}
			return m_tempDestinationOverride;
		}
		set
		{
			m_destination = value;
		}
	}

	public GameObject GameObject => base.gameObject;

	public int Index { get; private set; }

	public bool LeadsToRoot { get; private set; }

	public bool IsLocked { get; protected set; }

	public TransitionID TransitionType { get; private set; }

	public BaseRoom Room
	{
		get
		{
			return m_room;
		}
		private set
		{
			m_room = value;
		}
	}

	public Tunnel Root { get; private set; }

	public Animator Animator { get; private set; }

	public event EventHandler<EventArgs> DestroyedEvent;

	public event EventHandler<EventArgs> PlayerEnteredEvent;

	public event EventHandler<EventArgs> PlayerExitedEvent;

	protected virtual void Awake()
	{
		Animator = GetComponent<Animator>();
	}

	private void OnDestroy()
	{
		if (this.DestroyedEvent != null)
		{
			this.DestroyedEvent(this, EventArgs.Empty);
		}
	}

	private void OnDisable()
	{
		if ((bool)m_interactable)
		{
			m_interactable.TriggerOnEnterEvent.RemoveListener(OnPlayerInteractedWithTunnel);
		}
	}

	protected virtual void OnEnable()
	{
		if ((bool)m_interactable)
		{
			m_interactable.TriggerOnEnterEvent.AddListener(OnPlayerInteractedWithTunnel);
		}
	}

	public void Initialise()
	{
		IsInstantiated = true;
	}

	public virtual void ForceEnterTunnel(bool animate, Tunnel tempDestinationOverride = null)
	{
		m_tempDestinationOverride = tempDestinationOverride;
		RoomSaveController.DisableCutsceneSaving = tempDestinationOverride;
		if (animate)
		{
			OnPlayerInteractedWithTunnel(null);
		}
		else
		{
			EnterTunnel();
		}
	}

	protected virtual void OnPlayerInteractedWithTunnel(GameObject otherObj)
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		if (!playerController.IsGrounded)
		{
			playerController.ControllerCorgi.enabled = false;
		}
		StopAllCoroutines();
		StartCoroutine(EnterTunnelCoroutine());
	}

	protected virtual IEnumerator EnterTunnelCoroutine()
	{
		if (IsCutsceneTeleport)
		{
			SceneLoader_RL.RunTransitionWithLogic(delegate
			{
				EnterTunnel();
			}, TransitionID.QuickSwipe, cleanup: false);
		}
		else
		{
			SceneLoader_RL.RunTransitionWithLogic(delegate
			{
				EnterTunnel();
			}, TransitionType, cleanup: false);
		}
		yield break;
	}

	public void SetRoot(Tunnel root)
	{
		if (root == null)
		{
			Debug.LogFormat("<color=red>| {0} | Given Tunnel <b>root</b> argument is null, but shouldn't be.</color>", this);
			base.gameObject.SetActive(value: false);
			return;
		}
		Root = root;
		if (LeadsToRoot)
		{
			Destination = Root;
		}
	}

	public void SetIsDestinationRoot(bool leadsToRoot)
	{
		if (Root == this)
		{
			Debug.LogFormat("<color=red>| {0} | Given Tunnel is set to <b>Lead To Root</b>, but it <b>is</b> the Root. This should not happen.</color>", this);
			base.gameObject.SetActive(value: false);
		}
		else
		{
			LeadsToRoot = leadsToRoot;
		}
	}

	protected void EnterTunnel()
	{
		if (PlayerManager.GetPlayerController().CurrentHealth <= 0f && !ChallengeManager.IsInChallenge)
		{
			return;
		}
		if ((bool)Destination)
		{
			if (m_enterUnityEvent != null)
			{
				m_enterUnityEvent.Invoke();
			}
			Room.PlayerExit(null);
			LocalTeleporterController.StopTeleportPlayer();
			PlayerController playerController = PlayerManager.GetPlayerController();
			CutsceneManager.SetTraitsEnabled(enabled: true);
			playerController.StopActiveAbilities(stopPersistentAbilities: true);
			playerController.StatusBarController.SetCanvasVisible(visible: true);
			playerController.Visuals.SetActive(value: true);
			playerController.ControllerCorgi.enabled = true;
			playerController.HitboxController.DisableAllCollisions = false;
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayPlayerHUD, this, null);
			Destination.PlacePlayerHere();
			if (this.PlayerEnteredEvent != null)
			{
				this.PlayerEnteredEvent(this, EventArgs.Empty);
			}
		}
		else
		{
			Debug.LogFormat("<color=red>| {0} | Destination is null</color>", this);
		}
		m_tempDestinationOverride = null;
		RoomSaveController.DisableCutsceneSaving = false;
	}

	public void PlacePlayerHere()
	{
		if ((bool)Room)
		{
			if (!Room.gameObject.activeInHierarchy)
			{
				Room.gameObject.SetActive(value: true);
			}
			Room room = Room as Room;
			if ((bool)room)
			{
				room.CalculateRoomLevel();
			}
			Vector3 vector = Room.gameObject.transform.InverseTransformPoint(base.transform.position);
			PlayerManager.GetPlayerController().DisableEffectsOnEnterTunnel();
			Room.PlacePlayerInRoom(vector + OFFSET);
			if (this.PlayerExitedEvent != null)
			{
				this.PlayerExitedEvent(this, EventArgs.Empty);
			}
			if (m_exitUnityEvent != null)
			{
				m_exitUnityEvent.Invoke();
			}
		}
		else if (Room == null)
		{
			Debug.LogFormat("<color=red>[{0}] Room is null</color>", this);
		}
	}

	public void SetDestination(Tunnel tunnel)
	{
		Destination = tunnel;
	}

	public void SetIndex(int index)
	{
		Index = index;
	}

	public void SetTransitionType(TransitionID transitionType)
	{
		TransitionType = transitionType;
	}

	public virtual void SetIsLocked(bool isLocked)
	{
		IsLocked = isLocked;
		if ((bool)m_interactable)
		{
			m_interactable.SetIsInteractableActive(!isLocked);
		}
	}

	public void SetRoom(BaseRoom room)
	{
		Room = room;
	}

	public void SetIsVisible(bool isVisible)
	{
		if ((bool)m_visuals)
		{
			m_visuals.SetActive(isVisible);
		}
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
