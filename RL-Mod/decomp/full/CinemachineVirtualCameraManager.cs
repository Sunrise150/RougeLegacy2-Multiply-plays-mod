using Cinemachine;
using UnityEngine;

public class CinemachineVirtualCameraManager : MonoBehaviour
{
	[SerializeField]
	private CinemachineVirtualCamera m_virtualCamera;

	private bool m_isActiveVirtualCamera;

	private bool m_isInitialised;

	private Vector2 m_confinerSize;

	public Vector2 ConfinerSize
	{
		get
		{
			if (!IsInitialised)
			{
				Initialise();
			}
			return m_confinerSize;
		}
		private set
		{
			m_confinerSize = value;
		}
	}

	public bool IsActiveVirtualCamera
	{
		get
		{
			return m_isActiveVirtualCamera;
		}
		private set
		{
			m_isActiveVirtualCamera = value;
		}
	}

	public bool IsInitialised
	{
		get
		{
			return m_isInitialised;
		}
		private set
		{
			m_isInitialised = value;
		}
	}

	public CinemachineVirtualCamera VirtualCamera => m_virtualCamera;

	private void Start()
	{
	}

	private void OnDisable()
	{
		IsActiveVirtualCamera = false;
		SetFollowTarget(null);
	}

	private void Initialise()
	{
		BaseRoom componentInParent = GetComponentInParent<BaseRoom>();
		if ((bool)componentInParent)
		{
			CinemachineConfiner_RL component = GetComponent<CinemachineConfiner_RL>();
			if ((bool)component)
			{
				Collider2D collider2D = componentInParent.Collider2D;
				if ((bool)collider2D)
				{
					component.m_BoundingShape2D = collider2D;
					ConfinerSize = collider2D.bounds.size / new Vector2(32f, 18f);
				}
				else
				{
					Debug.LogFormat("<color=red>| {0} |: Unable to find Collider2D on Room Root. This is required by the Cinemachine Virtual Camera's Confiner Component.</color>", this);
				}
			}
		}
		IsInitialised = true;
	}

	public void SetIsActiveCamera(bool isActiveCamera)
	{
		if (!IsInitialised)
		{
			Initialise();
		}
		IsActiveVirtualCamera = isActiveCamera;
		if (isActiveCamera)
		{
			if (PlayerManager.IsInstantiated && VirtualCamera.Follow == null)
			{
				SetFollowTarget(PlayerManager.GetPlayerController().FollowTargetGO.transform);
			}
		}
		else
		{
			SetFollowTarget(null);
		}
		if (isActiveCamera)
		{
			VirtualCamera.MoveToTopOfPrioritySubqueue();
		}
	}

	public void SetFollowTarget(Transform target)
	{
		VirtualCamera.Follow = target;
	}

	public void SetLensSize(float size)
	{
		VirtualCamera.m_Lens.OrthographicSize = size;
	}
}
