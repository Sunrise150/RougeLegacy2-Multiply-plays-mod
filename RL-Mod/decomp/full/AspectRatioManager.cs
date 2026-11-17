using System;
using System.Collections;
using UnityEngine;

public class AspectRatioManager : MonoBehaviour
{
	public const float STANDARD_16_9_ASPECT_RATIO = 1.7777778f;

	private Camera m_camera;

	private float m_currentAspectRatio;

	private Action<MonoBehaviour, EventArgs> m_onResolutionChanged;

	private static AspectRatioManager m_instance;

	public static AspectRatioManager Instance
	{
		get
		{
			if (!m_instance)
			{
				m_instance = CDGHelper.FindStaticInstance<AspectRatioManager>(createNewIfNull: false);
				IsInitialized = true;
			}
			return m_instance;
		}
	}

	public static bool Disable_16_9_Aspect
	{
		get
		{
			if (ForceEnable_16_9)
			{
				return false;
			}
			return SaveManager.ConfigData.Disable_16_9;
		}
	}

	public static float CurrentGameAspectRatio => Instance.m_currentAspectRatio;

	public static float CurrentScreenAspectRatio => (float)GameResolutionManager.Resolution.x / (float)GameResolutionManager.Resolution.y;

	public static bool IsScreen_16_9_AspectRatio
	{
		get
		{
			float b = (float)(int)(CurrentScreenAspectRatio * 100f) / 100f;
			return Mathf.Approximately(1.77f, b);
		}
	}

	public static bool ForceEnable_16_9 { get; set; }

	public static bool IsInitialized { get; private set; }

	private void Awake()
	{
		m_onResolutionChanged = OnResolutionChanged;
	}

	private void OnDestroy()
	{
		IsInitialized = false;
	}

	private IEnumerator Start()
	{
		while (!CameraController.IsInstantiated)
		{
			yield return null;
		}
		OnResolutionChanged(null, null);
	}

	private void OnEnable()
	{
		Messenger<SceneMessenger, SceneEvent>.AddListener(SceneEvent.ResolutionChanged, m_onResolutionChanged);
	}

	private void OnDisable()
	{
		Messenger<SceneMessenger, SceneEvent>.RemoveListener(SceneEvent.ResolutionChanged, m_onResolutionChanged);
	}

	private void OnResolutionChanged(object sender, EventArgs args)
	{
		if (!CameraController.IsInstantiated)
		{
			return;
		}
		float a = (float)Screen.width / (float)Screen.height;
		float num = (float)GameResolutionManager.Resolution.x / (float)GameResolutionManager.Resolution.y;
		float num2 = ((!Disable_16_9_Aspect) ? 1.7777778f : num);
		if (Mathf.Approximately(a, num) && Mathf.Approximately(m_currentAspectRatio, num2))
		{
			return;
		}
		Rect rect = new Rect(0f, 0f, 1f, 1f);
		if (num < num2)
		{
			float num3 = num / num2;
			float y = (1f - num3) / 2f;
			rect.y = y;
			rect.height = num3;
		}
		else if (num > num2)
		{
			float num4 = num2 / num;
			float x = (1f - num4) / 2f;
			rect.x = x;
			rect.width = num4;
		}
		foreach (Camera camera in CameraController.Cameras)
		{
			if (Disable_16_9_Aspect || camera != CameraController.TraitMaskCam)
			{
				camera.aspect = num2;
				camera.rect = rect;
			}
		}
		CameraController.GameCamera.aspect = num2;
		CameraController.GameCamera.rect = rect;
		CameraController.UICamera.aspect = num2;
		CameraController.UICamera.rect = rect;
		CameraController.SoloCam.Camera.aspect = num2;
		CameraController.SoloCam.Camera.rect = rect;
		m_currentAspectRatio = num2;
		Messenger<SceneMessenger, SceneEvent>.Broadcast(SceneEvent.AspectRatioChanged, this, null);
	}
}
