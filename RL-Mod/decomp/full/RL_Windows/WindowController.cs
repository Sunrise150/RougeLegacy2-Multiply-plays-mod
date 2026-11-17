using Rewired;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace RL_Windows;

public abstract class WindowController : MonoBehaviour
{
	[SerializeField]
	private bool m_pauseGameWhenOpen = true;

	[SerializeField]
	private bool m_openOnSceneLoad;

	[SerializeField]
	protected Canvas m_windowCanvas;

	[SerializeField]
	[FormerlySerializedAs("m_windowOpenedUnityEvent")]
	public UnityEvent WindowOpenedEvent;

	[SerializeField]
	[FormerlySerializedAs("m_windowOpenedUnityEvent")]
	public UnityEvent WindowClosedEvent;

	public virtual int SortOrderOverride => -1;

	public Canvas WindowCanvas => m_windowCanvas;

	public abstract WindowID ID { get; }

	public Player RewiredPlayer => Rewired_RL.Player;

	public bool PauseGameWhenOpen
	{
		get
		{
			return m_pauseGameWhenOpen;
		}
		protected set
		{
			m_pauseGameWhenOpen = value;
		}
	}

	public bool OpenOnSceneLoad
	{
		get
		{
			return m_openOnSceneLoad;
		}
		protected set
		{
			m_openOnSceneLoad = value;
		}
	}

	public bool IsInitialized { get; protected set; }

	public virtual void Initialize()
	{
		if ((bool)m_windowCanvas)
		{
			if (m_windowCanvas.renderMode != RenderMode.WorldSpace)
			{
				m_windowCanvas.renderMode = RenderMode.ScreenSpaceCamera;
				m_windowCanvas.worldCamera = CameraController.UICamera;
			}
			m_windowCanvas.gameObject.SetActive(value: false);
		}
		IsInitialized = true;
	}

	public virtual void SetIsOpen(bool isOpen)
	{
		if (isOpen)
		{
			if (WindowOpenedEvent != null)
			{
				WindowOpenedEvent.Invoke();
			}
			base.enabled = true;
			if ((bool)m_windowCanvas)
			{
				CanvasScaler component = m_windowCanvas.GetComponent<CanvasScaler>();
				if ((bool)component)
				{
					component.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
				}
			}
			OnOpen();
		}
		else
		{
			if (WindowClosedEvent != null)
			{
				WindowClosedEvent.Invoke();
			}
			base.enabled = false;
			OnClose();
		}
	}

	public virtual void SetHasFocus(bool hasFocus)
	{
		if (hasFocus)
		{
			OnFocus();
		}
		else
		{
			OnLostFocus();
		}
	}

	protected abstract void OnOpen();

	protected abstract void OnClose();

	protected abstract void OnFocus();

	protected abstract void OnLostFocus();

	protected virtual void OnPause()
	{
	}

	protected virtual void OnUnpause()
	{
	}
}
