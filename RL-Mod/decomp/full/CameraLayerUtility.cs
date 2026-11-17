using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Rogue Legacy 2/Camera Layer Utility")]
public class CameraLayerUtility : ScriptableObject
{
	[Serializable]
	private class BiomeSubLayerMultiplierEntry
	{
		public BiomeType Biome;

		public float ForegroundMultiplier = 1f;

		public float BackgroundMultiplier = 1f;
	}

	private struct LayerID
	{
		public CameraLayer CameraLayer;

		public int SubLayer;

		public LayerID(CameraLayer cameraLayer, int subLayer)
		{
			CameraLayer = cameraLayer;
			SubLayer = subLayer;
		}
	}

	[Header("Game Layer Settings")]
	[SerializeField]
	private Vector2Int m_gameSubLayerRange = new Vector2Int(-500, 500);

	[SerializeField]
	private Vector2 m_gameZPositionRange = new Vector2(-10f, 10f);

	[SerializeField]
	private Vector2 m_propGameZPositionRange = new Vector2(-1f, 1f);

	[Header("Background Settings")]
	[SerializeField]
	private Vector2Int m_backgroundSubLayerRange = new Vector2Int(0, 500);

	[SerializeField]
	private Vector2 m_backgroundPerspNearZPositionRange = new Vector2(0.5f, 10f);

	[SerializeField]
	private Vector2 m_backgroundPerspFarZPositionRange = new Vector2(20f, 50f);

	[SerializeField]
	private Vector2 m_backgroundOrthoZPositionRange = new Vector2(0.5f, 5f);

	[Header("Foreground Settings")]
	[SerializeField]
	private Vector2Int m_foregroundSubLayerRange = new Vector2Int(0, 500);

	[SerializeField]
	private Vector2 m_foregroundPerspZPositionRange = new Vector2(-5f, -0.5f);

	[SerializeField]
	private Vector2 m_foregroundOrthoZPositionRange = new Vector2(-5f, -0.5f);

	[Header("Vertical Tuck Settings")]
	[SerializeField]
	private Vector2 m_defaultCameraSizeTuckRange = new Vector2(-0.25f, -0.5f);

	[Header("Default Sub Layers")]
	[SerializeField]
	private int m_defaultPlayerSubLayer = 5000;

	[SerializeField]
	private int m_defaultEnemySubLayer = 2000;

	[SerializeField]
	private int m_defaultProjectileSubLayer = 9000;

	private float m_distanceBetweenForegroundPerspSubLayers = -1f;

	private float m_distanceBetweenBackgroundNearSubLayers = -1f;

	private float m_distanceBetweenForegroundOrthoSubLayers = -1f;

	private float m_distanceBetweenBackgroundOrthoSubLayers = -1f;

	private float m_distanceBetweenBackgroundFarSubLayers = -1f;

	private float m_gameInitialZPosition = -1f;

	private float m_distanceBetweenGameSubLayers = -1f;

	private float m_distanceBetweenPropGameSubLayers = -1f;

	private static CameraLayerUtility m_instance;

	private const float DECO_OFFSET = -1E-05f;

	private const string RESOURCES_PATH = "Scriptable Objects/CameraLayerUtility";

	public const string ASSET_PATH = "Assets/Content/Scriptable Objects/CameraLayerUtility.asset";

	private static Dictionary<LayerID, int> m_zPositionTable;

	public static int DefaultEnemySubLayer => Instance.m_defaultEnemySubLayer;

	public static int DefaultPlayerSubLayer => Instance.m_defaultPlayerSubLayer;

	public static int DefaultProjectileSubLayer => Instance.m_defaultProjectileSubLayer;

	private static CameraLayerUtility Instance
	{
		get
		{
			if (m_instance == null)
			{
				if (Application.isPlaying)
				{
					m_instance = CDGResources.Load<CameraLayerUtility>("Scriptable Objects/CameraLayerUtility");
				}
				if (m_instance == null)
				{
					Debug.LogFormat("<color=red>| CameraLayerUtility | Failed to find CameraLayerUtility SO at Path ({0})</color>", "Scriptable Objects/CameraLayerUtility");
				}
			}
			return m_instance;
		}
	}

	private static float GetForegroundOrthoOffset(int order)
	{
		order = Mathf.Clamp(order, Instance.m_foregroundSubLayerRange.x, Instance.m_foregroundSubLayerRange.y);
		int num = Instance.m_backgroundSubLayerRange.y - Instance.m_backgroundSubLayerRange.x;
		if (Instance.m_distanceBetweenForegroundOrthoSubLayers == -1f)
		{
			Instance.m_distanceBetweenForegroundOrthoSubLayers = (Instance.m_foregroundOrthoZPositionRange.y - Instance.m_foregroundOrthoZPositionRange.x) / (float)num;
		}
		return (float)(num - 1 - order) * Instance.m_distanceBetweenForegroundOrthoSubLayers;
	}

	private static float GetForegroundPerspOffset(int order)
	{
		order = Mathf.Clamp(order, Instance.m_foregroundSubLayerRange.x, Instance.m_foregroundSubLayerRange.y);
		int num = Instance.m_backgroundSubLayerRange.y - Instance.m_backgroundSubLayerRange.x;
		if (Instance.m_distanceBetweenForegroundPerspSubLayers == -1f)
		{
			Instance.m_distanceBetweenForegroundPerspSubLayers = (Instance.m_foregroundPerspZPositionRange.y - Instance.m_foregroundPerspZPositionRange.x) / (float)num;
		}
		return (float)(num - 1 - order) * Instance.m_distanceBetweenForegroundPerspSubLayers;
	}

	private static float GetGameOffset(int order, bool isProp)
	{
		order = Mathf.Clamp(order, Instance.m_gameSubLayerRange.x, Instance.m_gameSubLayerRange.y);
		int num = Instance.m_gameSubLayerRange.y - Instance.m_gameSubLayerRange.x;
		float num2 = 0f;
		if (isProp)
		{
			if (Instance.m_distanceBetweenPropGameSubLayers == -1f)
			{
				Instance.m_distanceBetweenPropGameSubLayers = (Instance.m_propGameZPositionRange.y - Instance.m_propGameZPositionRange.x) / (float)num;
			}
			num2 = Instance.m_distanceBetweenPropGameSubLayers;
		}
		else
		{
			if (Instance.m_distanceBetweenGameSubLayers == -1f)
			{
				Instance.m_distanceBetweenGameSubLayers = (Instance.m_gameZPositionRange.y - Instance.m_gameZPositionRange.x) / (float)num;
			}
			num2 = Instance.m_distanceBetweenGameSubLayers;
		}
		return (float)(-1 * order) * num2;
	}

	private static float GetBackgroundFarOffset(int order)
	{
		order = Mathf.Clamp(order, Instance.m_backgroundSubLayerRange.x, Instance.m_backgroundSubLayerRange.y);
		int num = Instance.m_backgroundSubLayerRange.y - Instance.m_backgroundSubLayerRange.x;
		if (Instance.m_distanceBetweenBackgroundFarSubLayers == -1f)
		{
			Instance.m_distanceBetweenBackgroundFarSubLayers = (Instance.m_backgroundPerspFarZPositionRange.y - Instance.m_backgroundPerspFarZPositionRange.x) / (float)num;
		}
		return (float)(num - 1 - order) * Instance.m_distanceBetweenBackgroundFarSubLayers;
	}

	private static float GetBackgroundNearOffset(int order)
	{
		order = Mathf.Clamp(order, Instance.m_backgroundSubLayerRange.x, Instance.m_backgroundSubLayerRange.y);
		int num = Instance.m_backgroundSubLayerRange.y - Instance.m_backgroundSubLayerRange.x;
		if (Instance.m_distanceBetweenBackgroundNearSubLayers == -1f)
		{
			Instance.m_distanceBetweenBackgroundNearSubLayers = (Instance.m_backgroundPerspNearZPositionRange.y - Instance.m_backgroundPerspNearZPositionRange.x) / (float)num;
		}
		return (float)(num - 1 - order) * Instance.m_distanceBetweenBackgroundNearSubLayers;
	}

	private static float GetBackgroundOrthoOffset(int order)
	{
		order = Mathf.Clamp(order, Instance.m_backgroundSubLayerRange.x, Instance.m_backgroundSubLayerRange.y);
		int num = Instance.m_backgroundSubLayerRange.y - Instance.m_backgroundSubLayerRange.x;
		if (Instance.m_distanceBetweenBackgroundOrthoSubLayers == -1f)
		{
			Instance.m_distanceBetweenBackgroundOrthoSubLayers = (Instance.m_backgroundOrthoZPositionRange.y - Instance.m_backgroundOrthoZPositionRange.x) / (float)num;
		}
		return (float)(num - 1 - order) * Instance.m_distanceBetweenBackgroundOrthoSubLayers;
	}

	public static Vector2Int GetSubLayerRange(CameraLayer cameraLayer)
	{
		switch (cameraLayer)
		{
		case CameraLayer.Game:
			return Instance.m_gameSubLayerRange;
		case CameraLayer.Foreground_PERSP:
		case CameraLayer.Foreground_ORTHO:
			return Instance.m_foregroundSubLayerRange;
		case CameraLayer.Background_Far_PERSP:
		case CameraLayer.Background_ORTHO:
		case CameraLayer.Background_Near_PERSP:
			return Instance.m_foregroundSubLayerRange;
		default:
			return Vector2Int.zero;
		}
	}

	public static LayerMask GetLayer(CameraLayer cameraLayer)
	{
		switch (cameraLayer)
		{
		case CameraLayer.Game:
			return LayerMask.NameToLayer("Default");
		case CameraLayer.Foreground_PERSP:
			return LayerMask.NameToLayer("Foreground_Persp");
		case CameraLayer.Background_Far_PERSP:
			return LayerMask.NameToLayer("Background_Persp_Far");
		case CameraLayer.Background_Near_PERSP:
		case CameraLayer.Background_Wall:
			return LayerMask.NameToLayer("Background_Persp_Near");
		case CameraLayer.Foreground_ORTHO:
			return LayerMask.NameToLayer("Foreground_Ortho");
		case CameraLayer.Background_ORTHO:
			return LayerMask.NameToLayer("Background_Ortho");
		case CameraLayer.ForegroundLights:
			return LayerMask.NameToLayer("Foreground_Lights");
		default:
			throw new ArgumentOutOfRangeException("cameraLayer", $"No case for Camera Layer ({cameraLayer})");
		}
	}

	public static float GetTuckYOffset(CameraLayer cameraLayer, int subLayer)
	{
		float zDepth = GetZDepth(cameraLayer, subLayer, isProp: false);
		float zDepth2 = GetZDepth(CameraLayer.Background_Near_PERSP, 0, isProp: false);
		float t = zDepth / zDepth2;
		return Mathf.Lerp(Instance.m_defaultCameraSizeTuckRange.x, Instance.m_defaultCameraSizeTuckRange.y, t);
	}

	public static int GetSpriteOrderInLayer(CameraLayer cameraLayer, int order)
	{
		if (m_zPositionTable == null)
		{
			m_zPositionTable = new Dictionary<LayerID, int>();
		}
		LayerID key = new LayerID(cameraLayer, order);
		if (m_zPositionTable.ContainsKey(key))
		{
			m_zPositionTable[key]++;
		}
		else
		{
			m_zPositionTable.Add(key, 0);
		}
		return m_zPositionTable[key];
	}

	public static float GetZDepth(CameraLayer cameraLayer, int order, bool isProp, bool isDeco = false)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 1f;
		switch (cameraLayer)
		{
		case CameraLayer.Game:
			if (Instance.m_gameInitialZPosition == -1f)
			{
				Instance.m_gameInitialZPosition = Instance.m_gameZPositionRange.y - (Instance.m_gameZPositionRange.y - Instance.m_gameZPositionRange.x) / 2f;
			}
			num2 = Instance.m_gameInitialZPosition;
			num = GetGameOffset(order, isProp);
			break;
		case CameraLayer.Background_Wall:
			num2 = Instance.m_backgroundPerspNearZPositionRange.y;
			num = 0f;
			break;
		case CameraLayer.Foreground_PERSP:
			num2 = Instance.m_foregroundPerspZPositionRange.x;
			num = GetForegroundPerspOffset(order);
			break;
		case CameraLayer.Background_Far_PERSP:
			num2 = Instance.m_backgroundPerspFarZPositionRange.x;
			num = GetBackgroundFarOffset(order);
			break;
		case CameraLayer.Background_Near_PERSP:
			num2 = Instance.m_backgroundPerspNearZPositionRange.x;
			num = GetBackgroundNearOffset(order);
			break;
		case CameraLayer.Foreground_ORTHO:
			num2 = Instance.m_foregroundOrthoZPositionRange.x;
			num = GetForegroundOrthoOffset(order);
			break;
		case CameraLayer.Background_ORTHO:
			num2 = Instance.m_backgroundOrthoZPositionRange.x;
			num = GetBackgroundOrthoOffset(order);
			break;
		case CameraLayer.ForegroundLights:
			num = (float)(-1 * order) * Instance.m_distanceBetweenForegroundPerspSubLayers;
			break;
		default:
			throw new ArgumentOutOfRangeException("cameraLayer: " + cameraLayer);
		}
		float num4 = 0f;
		if (isDeco)
		{
			num4 = -1E-05f;
		}
		return num2 + num3 * num + num4;
	}

	private void Reset()
	{
		if (Application.isPlaying)
		{
			m_gameInitialZPosition = -1f;
			m_distanceBetweenGameSubLayers = -1f;
			m_distanceBetweenBackgroundFarSubLayers = -1f;
			m_distanceBetweenBackgroundNearSubLayers = -1f;
			m_distanceBetweenForegroundPerspSubLayers = -1f;
			m_distanceBetweenForegroundOrthoSubLayers = -1f;
			m_distanceBetweenBackgroundOrthoSubLayers = -1f;
		}
	}
}
