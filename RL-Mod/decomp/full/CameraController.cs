using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
	[SerializeField]
	[Range(1f, 10f)]
	private float m_defaultMaxZoomLevel = 2f;

	[SerializeField]
	private Vector2 m_zoomZPositionRange = new Vector2(-28.5f, -56.5f);

	[SerializeField]
	private int m_audioListenerOffset;

	[SerializeField]
	private Camera m_backgroundPerspFarCam;

	[SerializeField]
	private Camera m_backgroundOrthoCam;

	[SerializeField]
	private Camera m_foregroundPerspCam;

	[SerializeField]
	private Camera m_foregroundOrthoCam;

	[SerializeField]
	private Camera m_gameCam;

	[SerializeField]
	private Camera m_characterCam;

	[SerializeField]
	private SoloCameraController m_soloCam;

	[SerializeField]
	private Camera m_uiCamera;

	[SerializeField]
	private Camera m_traitMaskCamera;

	[SerializeField]
	private CinemachineBrain m_cinemachineBrain;

	[SerializeField]
	private MobilePostProcessing m_foregroundPostProcessing;

	[SerializeField]
	private MobilePostProcessing m_backgroundPostProcessing;

	[SerializeField]
	private Canvas m_koCanvas;

	[SerializeField]
	private Transform m_audioListener;

	[SerializeField]
	private Color m_greenScreenColor = Color.green;

	public const float ABSOLUTE_MAX_ZOOM_LEVEL = 10f;

	public const float MIN_ZOOM_LEVEL = 1f;

	private static Vector2 MIN_ZOOM_ROOM_SIZE = new Vector2(2f, 2f);

	public const float DEFAULT_CAMERA_SIZE = 9f;

	public const float DEFAULT_FIELD_OF_VIEW = 35f;

	private static Vector3 DEFAULT_RENDER_TEXTURE_SCALE = new Vector3(64f, 32f, 1f);

	private static bool INCLUDE_ENEMIES_IN_GREEN_SCREEN = true;

	private static CameraController m_instance = null;

	private BiomeType m_currentBiomeType;

	private static float m_zoomLevel = 1f;

	private static CameraZoomChangeEventArgs m_cameraZoomChangeEventArgs = null;

	private static Vector2 m_fogDistance;

	private static bool m_isGreenScreenOn;

	private static CameraClearFlags m_defaultCharacterCameraClearFlags;

	private static Color m_defaultCharacterCameraBackgroundColor;

	private Action<MonoBehaviour, EventArgs> m_onBiomeChange;

	private Action<MonoBehaviour, EventArgs> m_onQualitySettingsChanged;

	private static Coroutine m_koCoroutine;

	public static CinemachineBrain CinemachineBrain => Instance.m_cinemachineBrain;

	private static CameraController Instance
	{
		get
		{
			return m_instance;
		}
		set
		{
			m_instance = value;
		}
	}

	public BiomeType CurrentBiomeType
	{
		get
		{
			return m_currentBiomeType;
		}
		private set
		{
			m_currentBiomeType = value;
		}
	}

	public static Camera BackgroundPerspCam => Instance.m_backgroundPerspFarCam;

	public static Camera BackgroundOrthoCam => Instance.m_backgroundOrthoCam;

	public static Camera TraitMaskCam => Instance.m_traitMaskCamera;

	public static IEnumerable<Camera> Cameras
	{
		get
		{
			yield return BackgroundPerspCam;
			yield return BackgroundOrthoCam;
			yield return ForegroundPerspCam;
			yield return ForegroundOrthoCam;
			yield return CharacterCamera;
			yield return TraitMaskCam;
		}
	}

	public static Camera GameCamera => Instance.m_gameCam;

	public static Camera ForegroundOrthoCam => Instance.m_foregroundOrthoCam;

	public static Camera ForegroundPerspCam => Instance.m_foregroundPerspCam;

	public static SoloCameraController SoloCam => Instance.m_soloCam;

	public static Camera UICamera => Instance.m_uiCamera;

	public static bool IsInstantiated => m_instance != null;

	public static float ZoomLevel
	{
		get
		{
			return m_zoomLevel;
		}
		private set
		{
			m_zoomLevel = value;
		}
	}

	public static MobilePostProcessing ForegroundPostProcessing => Instance.m_foregroundPostProcessing;

	public static MobilePostProcessing BackgroundPostProcessing => Instance.m_backgroundPostProcessing;

	public static Camera CharacterCamera => Instance.m_characterCam;

	public static Vector2 ZPositionRange => Instance.m_zoomZPositionRange;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			if (m_defaultMaxZoomLevel < 1f)
			{
				throw new ArgumentOutOfRangeException("m_maxZoomLevel", $"| {this} | Max Zoom Level must be greater than 1");
			}
			if (ZPositionRange.x <= ZPositionRange.y)
			{
				throw new ArgumentOutOfRangeException("m_zoomZPositionRange", $"| {this} | Zoom Z Position Range's x value must be greater than its y value");
			}
			SceneManager.sceneLoaded += OnSceneLoaded;
			m_onBiomeChange = OnBiomeChange;
			m_onQualitySettingsChanged = OnQualitySettingsChanged;
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.BiomeEnter, m_onBiomeChange);
		Messenger<SceneMessenger, SceneEvent>.AddListener(SceneEvent.QualitySettingsChanged, m_onQualitySettingsChanged);
		foreach (Camera item in Cameras.Where((Camera camera) => !camera.orthographic))
		{
			item.transparencySortMode = TransparencySortMode.Orthographic;
		}
		m_audioListener.localPosition = new Vector3(0f, 0f, m_audioListenerOffset);
	}

	private void OnBiomeChange(MonoBehaviour sender, EventArgs eventArgs)
	{
		if (eventArgs is BiomeEventArgs biomeEventArgs && biomeEventArgs.Biome != CurrentBiomeType)
		{
			SetupCamera(biomeEventArgs.Biome, null);
		}
	}

	private void SetupCamera(BiomeType biome, BaseRoom room)
	{
		SetCameraSettingsForBiome(biome, room);
		CurrentBiomeType = biome;
		SetPostProcessingProfiles(biome, room);
	}

	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.BiomeEnter, m_onBiomeChange);
		Messenger<SceneMessenger, SceneEvent>.RemoveListener(SceneEvent.QualitySettingsChanged, m_onQualitySettingsChanged);
		m_instance = null;
	}

	public static void UpdateRoomCameraSettings(RoomViaDoorEventArgs roomArgs)
	{
		Instance.OnPlayerEnterRoom(roomArgs);
	}

	private void OnPlayerEnterRoom(RoomViaDoorEventArgs args)
	{
		if (args == null)
		{
			return;
		}
		bool flag = false;
		if (args.Room is Room)
		{
			Room room = args.Room as Room;
			if (room.GridPointManager != null && room.GridPointManager.RoomMetaData != null)
			{
				SpecialRoomType specialRoomType = room.GridPointManager.RoomMetaData.SpecialRoomType;
				flag = ((!GameUtility.IsInLevelEditor) ? (room.GridPointManager.IsTunnelDestination && specialRoomType == SpecialRoomType.Heirloom) : ((bool)room.gameObject.GetComponent<HeirloomBiomeOverrideController>()));
			}
		}
		if (flag)
		{
			SetupCamera(BiomeType.Castle, args.Room);
		}
		else
		{
			BiomeType appearanceBiomeType = args.Room.AppearanceBiomeType;
			if (CurrentBiomeType != appearanceBiomeType || (bool)args.Room.BiomeArtDataOverride)
			{
				SetupCamera(appearanceBiomeType, args.Room);
				if ((bool)args.Room.BiomeArtDataOverride)
				{
					m_currentBiomeType = BiomeType.None;
				}
			}
		}
		CameraZoomController component = args.Room.gameObject.GetComponent<CameraZoomController>();
		float num = 1f;
		if ((bool)component)
		{
			num = ClampZoomLevel(args.Room.CinemachineCamera.ConfinerSize, component.ZoomLevel);
		}
		SetCameraZoomLevel(num);
		args.Room.CinemachineCamera.SetLensSize(GetVirtualCameraLensSize(num));
		CameraOffsetOverrideController component2 = args.Room.gameObject.GetComponent<CameraOffsetOverrideController>();
		if ((bool)component2)
		{
			component2.ApplyOffset(args.Room.CinemachineCamera.VirtualCamera);
		}
		args.Room.CinemachineCamera.SetIsActiveCamera(isActiveCamera: true);
		args.Room.CinemachineCamera.VirtualCamera.InternalUpdateCameraState(Vector3.up, 1f);
		if (CinemachineBrain.enabled)
		{
			CinemachineBrain.UpdateMethod updateMethod = CinemachineBrain.m_UpdateMethod;
			CinemachineBrain.m_UpdateMethod = CinemachineBrain.UpdateMethod.ManualUpdate;
			CinemachineBrain.ManualUpdate();
			CinemachineBrain.m_UpdateMethod = updateMethod;
		}
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
	{
		m_cinemachineBrain.m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.Cut, 0f);
		SoloCam.transform.localPosition = Vector3.zero;
		GameCamera.orthographicSize = GetVirtualCameraLensSize(1f);
		SetCameraZoomLevel(1f);
		CurrentBiomeType = BiomeType.None;
		if (!GameUtility.SceneHasRooms)
		{
			foreach (Camera camera in Cameras)
			{
				if (camera.gameObject.activeSelf)
				{
					camera.gameObject.SetActive(value: false);
				}
			}
		}
		else
		{
			foreach (Camera camera2 in Cameras)
			{
				if (!camera2.gameObject.activeSelf)
				{
					camera2.gameObject.SetActive(value: true);
				}
			}
		}
		StopKOEffect();
	}

	private static void SetPostProcessingProfiles(BiomeType biome, BaseRoom room)
	{
		BiomeArtData biomeArtData = null;
		if (!room.IsNativeNull())
		{
			biomeArtData = room.BiomeArtDataOverride;
		}
		if (!biomeArtData)
		{
			biomeArtData = BiomeArtDataLibrary.GetArtData(biome);
		}
		ForegroundPostProcessing.SetBaseProfile(biomeArtData.PostProcessingData.ForegroundProfile);
		BackgroundPostProcessing.SetBaseProfile(biomeArtData.PostProcessingData.BackgroundProfile);
		Instance.OnQualitySettingsChanged(null, null);
	}

	private void OnQualitySettingsChanged(object sender, EventArgs args)
	{
	}

	public static float GetVirtualCameraLensSize(float zoomLevel)
	{
		return zoomLevel * 9f;
	}

	public static float GetZoomLevel(Vector2 confinerSize)
	{
		float result = 1f;
		if (confinerSize.x > 1f && confinerSize.y > 1f)
		{
			result = ClampZoomLevel(confinerSize, m_instance.m_defaultMaxZoomLevel);
		}
		return result;
	}

	private static float ClampZoomLevel(Vector2 confinerSize, float zoomLevel)
	{
		if (confinerSize.x > 1f && confinerSize.y > 1f)
		{
			float b = Mathf.Min(confinerSize.x, confinerSize.y);
			zoomLevel = Mathf.Min(zoomLevel, b);
		}
		else
		{
			zoomLevel = 1f;
		}
		return zoomLevel;
	}

	public static void ToggleGreenScreenIsOn()
	{
		m_isGreenScreenOn = !m_isGreenScreenOn;
		if (m_defaultCharacterCameraClearFlags == (CameraClearFlags)0)
		{
			m_defaultCharacterCameraClearFlags = CharacterCamera.clearFlags;
			m_defaultCharacterCameraBackgroundColor = CharacterCamera.backgroundColor;
		}
		CameraClearFlags clearFlags = m_defaultCharacterCameraClearFlags;
		Color backgroundColor = m_defaultCharacterCameraBackgroundColor;
		if (m_isGreenScreenOn)
		{
			clearFlags = CameraClearFlags.Color;
			backgroundColor = Instance.m_greenScreenColor;
		}
		CharacterCamera.clearFlags = clearFlags;
		CharacterCamera.backgroundColor = backgroundColor;
		foreach (Camera camera in Cameras)
		{
			if (camera != CharacterCamera)
			{
				camera.enabled = !m_isGreenScreenOn;
			}
		}
		UICamera.enabled = !m_isGreenScreenOn;
		if (!INCLUDE_ENEMIES_IN_GREEN_SCREEN)
		{
			EnemyController[] array = UnityEngine.Object.FindObjectsOfType<EnemyController>();
			for (int i = 0; i < array.Length; i++)
			{
				Renderer[] componentsInChildren = array[i].gameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					componentsInChildren[j].enabled = !m_isGreenScreenOn;
				}
			}
		}
		AddToGreenScreen[] array2 = UnityEngine.Object.FindObjectsOfType<AddToGreenScreen>();
		foreach (AddToGreenScreen addToGreenScreen in array2)
		{
			if (m_isGreenScreenOn)
			{
				addToGreenScreen.gameObject.SetLayerRecursively(29);
			}
			else
			{
				addToGreenScreen.ResetLayers();
			}
		}
	}

	public static void SetCameraSettingsForBiome(BiomeType biome, BaseRoom room)
	{
		BiomeArtData biomeArtData = null;
		if (!room.IsNativeNull())
		{
			biomeArtData = room.BiomeArtDataOverride;
		}
		if (!biomeArtData)
		{
			biomeArtData = BiomeArtDataLibrary.GetArtData(biome);
		}
		if ((bool)biomeArtData)
		{
			if (biomeArtData.LightAndFogData != null)
			{
				AmbientLightAndFogBiomeArtData lightAndFogData = biomeArtData.LightAndFogData;
				RenderSettings.ambientLight = lightAndFogData.AmbientLightColor;
				RenderSettings.fog = lightAndFogData.Fog;
				RenderSettings.fogMode = lightAndFogData.FogMode;
				RenderSettings.fogColor = lightAndFogData.FogColor;
				RenderSettings.fogDensity = lightAndFogData.FogDensity;
				RenderSettings.fogStartDistance = lightAndFogData.FogStartDistance;
				RenderSettings.fogEndDistance = lightAndFogData.FogEndDistance;
				m_fogDistance = new Vector2(lightAndFogData.FogStartDistance, lightAndFogData.FogEndDistance);
				RenderSettings.ambientSkyColor = lightAndFogData.AmbientLightColor;
				RenderSettings.ambientMode = lightAndFogData.AmbientMode;
			}
			else
			{
				Debug.LogFormat("<color=red>Light And Fog Data is null on Biome Art Data ({0})</color>", biomeArtData.name);
			}
		}
		else
		{
			Debug.LogFormat("<color=red>No Biome Art Data for Biome ({0})</color>", biome);
		}
	}

	public static void SetCameraEnabled(int cameraIndex, bool isEnabled)
	{
		Camera camera = null;
		switch (cameraIndex)
		{
		case 0:
			camera = BackgroundPerspCam;
			break;
		case 1:
			camera = BackgroundOrthoCam;
			break;
		case 2:
			camera = GameCamera;
			break;
		case 3:
			camera = ForegroundOrthoCam;
			break;
		case 4:
			camera = ForegroundPerspCam;
			break;
		case 5:
			camera = CharacterCamera;
			break;
		case 6:
			camera = UICamera;
			break;
		case 7:
			camera = TraitMaskCam;
			break;
		}
		camera.enabled = isEnabled;
	}

	private static void SetCameraZoomLevel(float zoomLevel)
	{
		float zoomLevel2 = ZoomLevel;
		ZoomLevel = zoomLevel;
		float virtualCameraLensSize = GetVirtualCameraLensSize(ZoomLevel);
		foreach (Camera item in Cameras.Where((Camera camera) => camera.orthographic))
		{
			item.orthographicSize = virtualCameraLensSize;
		}
		float t = 0f;
		if (m_instance.m_defaultMaxZoomLevel > 1f)
		{
			t = (ZoomLevel - 1f) / 9f;
		}
		foreach (Camera item2 in Cameras.Where((Camera camera) => !camera.orthographic))
		{
			float z = Mathf.Lerp(ZPositionRange.x, ZPositionRange.y, t);
			item2.transform.position = new Vector3(item2.transform.position.x, item2.transform.position.y, z);
		}
		if (GameUtility.SceneHasRooms)
		{
			UpdateFogSettings();
		}
		if (m_cameraZoomChangeEventArgs == null)
		{
			m_cameraZoomChangeEventArgs = new CameraZoomChangeEventArgs(zoomLevel, zoomLevel2);
		}
		else
		{
			m_cameraZoomChangeEventArgs.Initialize(zoomLevel, zoomLevel2);
		}
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.CameraZoomChange, Instance, m_cameraZoomChangeEventArgs);
	}

	private static void UpdateFogSettings()
	{
		RenderSettings.fogStartDistance = ZoomLevel * m_fogDistance.x;
		RenderSettings.fogEndDistance = ZoomLevel * m_fogDistance.y;
	}

	public static void PlayKOEffect(float duration)
	{
		Instance.PlayKOEffect_Internal(duration);
	}

	public static void StopKOEffect()
	{
		Instance.StopKOEffect_Internal();
	}

	private void PlayKOEffect_Internal(float duration)
	{
		if (m_koCoroutine != null)
		{
			Instance.StopCoroutine(m_koCoroutine);
		}
		m_koCoroutine = Instance.StartCoroutine(Instance.KOEffectCoroutine(duration));
	}

	private void StopKOEffect_Internal()
	{
		if (m_koCoroutine != null)
		{
			Instance.StopCoroutine(m_koCoroutine);
		}
		m_koCoroutine = null;
		if (m_koCanvas.gameObject.activeSelf)
		{
			m_koCanvas.gameObject.SetActive(value: false);
		}
	}

	private IEnumerator KOEffectCoroutine(float duration)
	{
		m_koCanvas.gameObject.SetActive(value: true);
		duration += Time.unscaledTime;
		while (Time.unscaledTime < duration)
		{
			yield return null;
		}
		m_koCanvas.gameObject.SetActive(value: false);
	}

	public static void LerpBGPostProcessToProfile(MobilePostProcessingProfile profile, float lerpSpeed)
	{
		BackgroundPostProcessing.LerpToProfile(profile, lerpSpeed);
	}

	public static void LerpFGPostProcessToProfile(MobilePostProcessingProfile profile, float lerpSpeed)
	{
		if (!TraitManager.IsTraitActive(TraitType.NoColor) && !TraitManager.IsTraitActive(TraitType.Oversaturate) && !TraitManager.IsTraitActive(TraitType.OldYellowTint))
		{
			ForegroundPostProcessing.LerpToProfile(profile, lerpSpeed);
		}
	}
}
