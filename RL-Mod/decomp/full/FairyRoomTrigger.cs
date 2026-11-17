using System;
using UnityEngine;

public class FairyRoomTrigger : BaseSpecialPropController
{
	public const string OFF_COLOR = "#000000";

	public const string ON_COLOR = "#E7BF51";

	public const string PASSED_COLOR = "#3ACF00";

	public const string FAILED_COLOR = "#CA000B";

	[SerializeField]
	private SpriteRenderer m_bottomSprite;

	[SerializeField]
	private SpriteRenderer m_lightSprite;

	private FairyRoomController m_fairyRoomController;

	private Action<MonoBehaviour, EventArgs> m_onStateChange;

	protected override void Awake()
	{
		base.Awake();
		m_onStateChange = OnStateChange;
	}

	private void OnEnable()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.FairyRoomStateChange, m_onStateChange);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.FairyRoomStateChange, m_onStateChange);
	}

	public override void SetRoom(BaseRoom room)
	{
		base.SetRoom(room);
		m_fairyRoomController = base.Room.gameObject.GetComponent<FairyRoomController>();
	}

	protected override void InitializePooledPropOnEnter()
	{
		InitializeTrigger();
		if (base.IsPropComplete)
		{
			DisableProp(firstTimeDisabled: false);
		}
	}

	public void InitializeTrigger()
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		LayerMask savedPlatformMask = playerController.ControllerCorgi.SavedPlatformMask;
		savedPlatformMask = (int)savedPlatformMask | (int)playerController.ControllerCorgi.OneWayPlatformMask;
		RaycastHit2D raycastHit2D = Physics2D.Raycast(base.transform.position, Vector2.down, 16f, savedPlatformMask);
		if ((bool)raycastHit2D)
		{
			Vector2 size = m_bottomSprite.size;
			size.y = raycastHit2D.distance;
			m_bottomSprite.size = size;
		}
		SetTriggerState(m_fairyRoomController.State);
	}

	private void OnStateChange(object sender, EventArgs args)
	{
		FairyRoomEnteredEventArgs fairyRoomEnteredEventArgs = args as FairyRoomEnteredEventArgs;
		SetTriggerState(fairyRoomEnteredEventArgs.FairyRoomController.State);
	}

	private void SetTriggerState(FairyRoomState state)
	{
		base.Animator.SetBool("On", value: false);
		Color color;
		switch (state)
		{
		case FairyRoomState.Failed:
			ColorUtility.TryParseHtmlString("#CA000B", out color);
			if (!base.IsPropComplete)
			{
				PropComplete();
				DisableProp(firstTimeDisabled: true);
			}
			break;
		case FairyRoomState.Passed:
			ColorUtility.TryParseHtmlString("#3ACF00", out color);
			if (!base.IsPropComplete)
			{
				PropComplete();
				DisableProp(firstTimeDisabled: true);
			}
			break;
		case FairyRoomState.Running:
			ColorUtility.TryParseHtmlString("#E7BF51", out color);
			base.Animator.SetBool("On", value: true);
			if (!base.IsPropComplete)
			{
				DisableProp(firstTimeDisabled: true);
			}
			break;
		default:
			ColorUtility.TryParseHtmlString("#000000", out color);
			break;
		}
		m_lightSprite.color = color;
	}

	public void ActivateTrigger()
	{
		if ((bool)m_fairyRoomController)
		{
			m_fairyRoomController.StartFairyRoom();
		}
	}
}
