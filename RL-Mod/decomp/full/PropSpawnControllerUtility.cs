using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Rogue Legacy 2/Prop Spawn Controller Utility")]
public class PropSpawnControllerUtility : ScriptableObject
{
	[Serializable]
	private class PropSpawnControllerColors
	{
		public Color BackgroundPerspFarMaxColor = new Color(0.6f, 0.9f, 0.8f);

		public Color BackgroundPerspFarMinColor = new Color(0.4f, 0.6f, 0.8f);

		public Color BackgroundPerspNearMaxColor = new Color(0.6f, 0.9f, 0.8f);

		public Color BackgroundPerspNearMinColor = new Color(0.4f, 0.6f, 0.8f);

		public Color BackgroundOrthoColor = new Color(0.8f, 0.6f, 0.6f);

		public Color GameColor = new Color(0.53f, 0.05f, 0.63f);

		public Color ForegroundOrthoColor = new Color(0.55f, 0.55f, 0.6f);

		public Color ForegroundPerspMaxColor = new Color(0.4f, 0.4f, 0.45f);

		public Color ForegroundPerspMinColor = new Color(0f, 0f, 0.1f);
	}

	[Serializable]
	private class PropSpawnControllerSortOrder
	{
		public float DefaultZPosition = -0.5f;

		public float BackgroundZPosition = 1f;

		public float ForegroundZPosition = -1f;
	}

	[SerializeField]
	private PropSpawnControllerColors m_colors;

	[SerializeField]
	private PropSpawnControllerSortOrder m_sortOrders;

	private const string GAME_SORTING_LAYER_NAME = "GameEditorProps";

	private const string FOREGROUND_SORTING_LAYER_NAME = "ForegroundEditorProps";

	private const string BACKGROUND_SORTING_LAYER_NAME = "BackgroundEditorProps";

	private const string FOREGROUND_PERSP_SORTING_LAYER_NAME = "ForegroundPerspEditorProps";

	private const string BACKGROUND_PERSP_NEAR_SORTING_LAYER_NAME = "BackgroundPerspNearEditorProps";

	private const string BACKGROUND_PERSP_FAR_SORTING_LAYER_NAME = "BackgroundPerspFarEditorProps";

	private const string RESOURCES_PATH = "Scriptable Objects/PropSpawnControllerUtility";

	public const string EDITOR_PATH = "Assets/Content/Scriptable Objects/PropSpawnControllerUtility.asset";

	private static PropSpawnControllerUtility m_instance;

	private static PropSpawnControllerUtility Instance
	{
		get
		{
			if (m_instance == null)
			{
				if (Application.isPlaying)
				{
					m_instance = CDGResources.Load<PropSpawnControllerUtility>("Scriptable Objects/PropSpawnControllerUtility");
				}
				if (m_instance == null)
				{
					Debug.LogFormat("<color=red>| PropSpawnControllerUtility | Failed to find PropSpawnControllerUtility SO at Path ({0})</color>", "Scriptable Objects/PropSpawnControllerUtility");
				}
			}
			return m_instance;
		}
	}

	public static Color GetColor(CameraLayer cameraLayer, int subLayer)
	{
		Color result = Color.white;
		switch (cameraLayer)
		{
		case CameraLayer.Game:
			result = Instance.m_colors.GameColor;
			break;
		case CameraLayer.Foreground_ORTHO:
			result = Instance.m_colors.ForegroundOrthoColor;
			break;
		case CameraLayer.Background_ORTHO:
			result = Instance.m_colors.BackgroundOrthoColor;
			break;
		case CameraLayer.Foreground_PERSP:
		case CameraLayer.Background_Far_PERSP:
		case CameraLayer.Background_Near_PERSP:
		{
			Vector2Int zero = Vector2Int.zero;
			Color a;
			Color b;
			switch (cameraLayer)
			{
			case CameraLayer.Background_Far_PERSP:
				zero = CameraLayerUtility.GetSubLayerRange(CameraLayer.Background_Far_PERSP);
				a = Instance.m_colors.BackgroundPerspFarMaxColor;
				b = Instance.m_colors.BackgroundPerspFarMinColor;
				break;
			case CameraLayer.Background_Near_PERSP:
				zero = CameraLayerUtility.GetSubLayerRange(CameraLayer.Background_Near_PERSP);
				a = Instance.m_colors.BackgroundPerspNearMaxColor;
				b = Instance.m_colors.BackgroundPerspNearMinColor;
				break;
			default:
				zero = CameraLayerUtility.GetSubLayerRange(CameraLayer.Foreground_PERSP);
				a = Instance.m_colors.ForegroundPerspMaxColor;
				b = Instance.m_colors.ForegroundPerspMinColor;
				break;
			}
			result = Color.Lerp(a, b, (float)subLayer / (float)(zero.y - zero.x - 1));
			break;
		}
		}
		return result;
	}

	public static Vector2Int GetGameSubLayerRange(CameraLayer cameraLayer)
	{
		return CameraLayerUtility.GetSubLayerRange(cameraLayer);
	}

	public static int GetSortingLayerID(CameraLayer cameraLayer)
	{
		return cameraLayer switch
		{
			CameraLayer.Game => SortingLayer.NameToID("GameEditorProps"), 
			CameraLayer.Foreground_PERSP => SortingLayer.NameToID("ForegroundPerspEditorProps"), 
			CameraLayer.Foreground_ORTHO => SortingLayer.NameToID("ForegroundEditorProps"), 
			CameraLayer.Background_ORTHO => SortingLayer.NameToID("BackgroundEditorProps"), 
			CameraLayer.Background_Near_PERSP => SortingLayer.NameToID("BackgroundPerspNearEditorProps"), 
			CameraLayer.Background_Far_PERSP => SortingLayer.NameToID("BackgroundPerspFarEditorProps"), 
			_ => SortingLayer.NameToID("GameEditorProps"), 
		};
	}

	public static int GetSortOrder(CameraLayer cameraLayer, int subLayer)
	{
		return subLayer;
	}

	public static float GetZPosition(CameraLayer cameraLayer, int subLayer)
	{
		if (cameraLayer == CameraLayer.Foreground_ORTHO || cameraLayer == CameraLayer.Foreground_PERSP || (cameraLayer == CameraLayer.Game && subLayer > 0))
		{
			return Instance.m_sortOrders.ForegroundZPosition;
		}
		if (cameraLayer == CameraLayer.Background_ORTHO || cameraLayer == CameraLayer.Background_Far_PERSP || cameraLayer == CameraLayer.Background_Near_PERSP || (cameraLayer == CameraLayer.Game && subLayer < 0))
		{
			return Instance.m_sortOrders.BackgroundZPosition;
		}
		return Instance.m_sortOrders.DefaultZPosition;
	}
}
