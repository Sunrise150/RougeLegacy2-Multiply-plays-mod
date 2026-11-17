using System;
using Sigtrap.Relays;
using UnityEngine;

public abstract class BaseSpecialPropController : MonoBehaviour, IRoomConsumer
{
	[SerializeField]
	protected Interactable m_interactable;

	private Action m_initializeProp;

	private Relay<bool> m_disableSpecialPropRelay = new Relay<bool>();

	public BaseRoom Room { get; private set; }

	protected BaseSpecialRoomController SpecialRoomController { get; private set; }

	public Animator Animator { get; private set; }

	public bool IsDisabled { get; protected set; }

	protected RoomSaveData RoomSaveData { get; private set; }

	public IRelayLink<bool> DisableSpecialPropRelay => m_disableSpecialPropRelay.link;

	public bool IsPropComplete
	{
		get
		{
			if ((bool)SpecialRoomController)
			{
				return SpecialRoomController.IsRoomComplete;
			}
			return false;
		}
	}

	protected string GetRoomMiscData(string id)
	{
		if ((bool)SpecialRoomController)
		{
			return SpecialRoomController.GetRoomMiscData(id);
		}
		return null;
	}

	protected void SetRoomMiscData(string id, string value)
	{
		if ((bool)SpecialRoomController)
		{
			SpecialRoomController.SetRoomMiscData(id, value);
		}
	}

	protected virtual void DisableProp(bool firstTimeDisabled)
	{
		if ((bool)m_interactable)
		{
			m_interactable.SetIsInteractableActive(isActive: false);
		}
		bool isDisabled = IsDisabled;
		IsDisabled = true;
		if (!isDisabled)
		{
			m_disableSpecialPropRelay.Dispatch(firstTimeDisabled);
		}
	}

	protected virtual void Awake()
	{
		Animator = GetComponent<Animator>();
		m_initializeProp = InitializeProp;
	}

	protected abstract void InitializePooledPropOnEnter();

	private void InitializeProp()
	{
		RoomSaveData = SaveManager.StageSaveData.GetRoomSaveData(Room.BiomeType, Room.BiomeControllerIndex);
		IsDisabled = false;
		if ((bool)m_interactable)
		{
			m_interactable.SetIsInteractableActive(isActive: true);
		}
		InitializePooledPropOnEnter();
		if ((bool)m_interactable)
		{
			m_interactable.ForceUpdateSpeechBubbleState();
		}
		if (IsPropComplete)
		{
			DisableProp(firstTimeDisabled: false);
		}
	}

	public virtual void SetRoom(BaseRoom room)
	{
		Room = room;
		if ((bool)Room)
		{
			SpecialRoomController = room.gameObject.GetComponent<BaseSpecialRoomController>();
			if ((bool)SpecialRoomController)
			{
				SpecialRoomController.SpecialRoomInitializedRelay.AddListener(m_initializeProp);
			}
			else
			{
				InitializeProp();
			}
		}
	}

	protected virtual void PropComplete()
	{
		if ((bool)SpecialRoomController && !SpecialRoomController.IsRoomComplete)
		{
			SpecialRoomController.RoomCompleted();
		}
	}

	protected virtual void OnDisable()
	{
		if ((bool)SpecialRoomController)
		{
			SpecialRoomController.SpecialRoomInitializedRelay.RemoveListener(m_initializeProp);
		}
	}
}
