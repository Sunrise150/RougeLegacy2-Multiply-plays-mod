using System;
using UnityEngine;

public class FadeOutHUDCollider : MonoBehaviour
{
	[SerializeField]
	private Collider2D m_collider;

	[SerializeField]
	private float m_fadeInAmount;

	[SerializeField]
	private float m_fadeOutAmount;

	[SerializeField]
	private bool m_isMinimapHUD;

	[SerializeField]
	private bool m_isFairyHUD;

	private float m_screenToWorldScale;

	private CanvasGroup m_canvasGroup;

	private bool m_isFaded;

	private bool m_collisionDetected;

	private Vector3 m_storedPos;

	private bool m_inDeactivatedFairyRoom;

	private Action<MonoBehaviour, EventArgs> m_onPlayerEnterRoom;

	private Action<MonoBehaviour, EventArgs> m_onPlayerEnterFairyRoom;

	private Action<MonoBehaviour, EventArgs> m_onPlayerFairyRoomTriggered;

	private Action<MonoBehaviour, EventArgs> m_onPlayerExitFairyRoom;

	private Action<MonoBehaviour, EventArgs> m_onAspectRatioChanged;

	public Collider2D Collider => m_collider;

	private void Awake()
	{
		RectTransform rectTransform = base.transform.parent.transform as RectTransform;
		m_screenToWorldScale = 1f / 60f * rectTransform.localScale.x;
		Vector3 vector = CameraController.GameCamera.ScreenToWorldPoint(base.transform.position) - CameraController.GameCamera.transform.position;
		base.transform.SetParent(CameraController.GameCamera.transform, worldPositionStays: true);
		base.transform.localPosition = vector;
		base.transform.localScale = new Vector3(m_screenToWorldScale, m_screenToWorldScale, 1f);
		m_storedPos = vector;
		m_onPlayerEnterRoom = OnPlayerEnterRoom;
		m_onPlayerEnterFairyRoom = OnPlayerEnterFairyRoom;
		m_onPlayerFairyRoomTriggered = OnPlayerFairyRoomTriggered;
		m_onPlayerExitFairyRoom = OnPlayerExitFairyRoom;
		m_onAspectRatioChanged = OnAspectRatioChanged;
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterFairyRoom, m_onPlayerEnterFairyRoom);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerFairyRoomTriggered, m_onPlayerFairyRoomTriggered);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerExitFairyRoom, m_onPlayerExitFairyRoom);
		Messenger<SceneMessenger, SceneEvent>.AddListener(SceneEvent.AspectRatioChanged, m_onAspectRatioChanged);
		if (m_isFairyHUD)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnDestroy()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterFairyRoom, m_onPlayerEnterFairyRoom);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerFairyRoomTriggered, m_onPlayerFairyRoomTriggered);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerExitFairyRoom, m_onPlayerExitFairyRoom);
		Messenger<SceneMessenger, SceneEvent>.RemoveListener(SceneEvent.AspectRatioChanged, m_onAspectRatioChanged);
	}

	private void OnPlayerEnterRoom(object sender, EventArgs args)
	{
		float zoomLevel = CameraController.ZoomLevel;
		float num = m_screenToWorldScale * zoomLevel;
		base.transform.localScale = new Vector3(num, num, 1f);
		Vector3 storedPos = m_storedPos;
		if (!AspectRatioManager.Disable_16_9_Aspect)
		{
			float num2 = 1.7777778f / AspectRatioManager.CurrentScreenAspectRatio;
			storedPos.x *= num2;
		}
		Vector3 localPosition = new Vector3(storedPos.x * zoomLevel, storedPos.y * zoomLevel, storedPos.z);
		if (TraitManager.IsTraitActive(TraitType.UpsideDown))
		{
			localPosition.y *= -1f;
		}
		base.transform.localPosition = localPosition;
	}

	private void OnPlayerEnterFairyRoom(object sender, EventArgs args)
	{
		m_inDeactivatedFairyRoom = true;
		if (m_isFairyHUD && !base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
		}
	}

	private void OnPlayerFairyRoomTriggered(object sender, EventArgs args)
	{
		if (args is FairyRoomEnteredEventArgs fairyRoomEnteredEventArgs && !fairyRoomEnteredEventArgs.FairyRoomController.IsRoomComplete)
		{
			m_inDeactivatedFairyRoom = false;
		}
	}

	private void OnPlayerExitFairyRoom(object sender, EventArgs args)
	{
		m_inDeactivatedFairyRoom = false;
		if (m_isFairyHUD && base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnAspectRatioChanged(MonoBehaviour sender, EventArgs args)
	{
		OnPlayerEnterRoom(null, null);
	}

	public void SetCanvasGroup(CanvasGroup canvasGroup)
	{
		m_canvasGroup = canvasGroup;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (IsValidCollider(collision))
		{
			m_collisionDetected = true;
		}
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		if (IsValidCollider(collision))
		{
			m_collisionDetected = true;
		}
	}

	private bool IsValidCollider(Collider2D collider)
	{
		if (SaveManager.ConfigData.DisableHUDFadeOut)
		{
			return false;
		}
		if (collider.CompareTag("EnemyProjectile"))
		{
			return true;
		}
		if (collider.CompareTag("Enemy") && !m_inDeactivatedFairyRoom)
		{
			return true;
		}
		if (collider.CompareTag("Player"))
		{
			return true;
		}
		if (collider.CompareTag("Player_Dodging"))
		{
			return true;
		}
		if (collider.CompareTag("Hazard"))
		{
			return true;
		}
		if (collider.CompareTag("TriggerHazard"))
		{
			return true;
		}
		if (collider.CompareTag("Chest"))
		{
			return true;
		}
		if (collider.CompareTag("NPC"))
		{
			return true;
		}
		return false;
	}

	private void FixedUpdate()
	{
		if (!m_canvasGroup || (m_isMinimapHUD && TraitManager.IsTraitActive(TraitType.MapReveal)) || m_canvasGroup.alpha == 0f)
		{
			return;
		}
		if (m_isFaded && !m_canvasGroup.isActiveAndEnabled)
		{
			m_isFaded = false;
			m_canvasGroup.alpha = m_fadeInAmount;
		}
		if (m_collisionDetected)
		{
			if (!m_isFaded)
			{
				m_canvasGroup.alpha = m_fadeOutAmount;
				m_isFaded = true;
			}
		}
		else if (m_isFaded)
		{
			m_isFaded = false;
			m_canvasGroup.alpha = m_fadeInAmount;
		}
		m_collisionDetected = false;
	}
}
