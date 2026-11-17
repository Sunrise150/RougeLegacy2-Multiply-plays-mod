using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/MapTerrain Library")]
public class MapTerrainLibrary : ScriptableObject
{
	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/MapTerrainLibrary";

	private const string ASSET_PATH = "Assets/Content/Scriptable Objects/Libraries/MapTerrainLibrary.asset";

	[HelpBox("The elements must be in ascending Y size order. E.g. Element 0 must have a Y size of 1, element 1 must have a Y size of 2, etc.", HelpBoxMessageType.None)]
	[SerializeField]
	private Ferr2DT_PathTerrain[] m_oneWidthMapTerrains;

	[SerializeField]
	private Ferr2DT_PathTerrain[] m_twoWidthMapTerrains;

	[SerializeField]
	private Ferr2DT_PathTerrain[] m_threeWidthMapTerrains;

	[SerializeField]
	private Ferr2DT_PathTerrain m_defaultMapTerrain;

	private static MapTerrainLibrary m_instance;

	private static MapTerrainLibrary Instance
	{
		get
		{
			if (m_instance == null && Application.isPlaying)
			{
				m_instance = CDGResources.Load<MapTerrainLibrary>("Scriptable Objects/Libraries/MapTerrainLibrary");
			}
			return m_instance;
		}
	}

	public static Ferr2DT_PathTerrain GetMapTerrain(int width, int height)
	{
		Ferr2DT_PathTerrain[] array = null;
		switch (width)
		{
		case 1:
			array = Instance.m_oneWidthMapTerrains;
			break;
		case 2:
			array = Instance.m_twoWidthMapTerrains;
			break;
		case 3:
			array = Instance.m_threeWidthMapTerrains;
			break;
		}
		if (array != null && array.Length > height - 1)
		{
			return array[height - 1];
		}
		return null;
	}

	public static Ferr2DT_PathTerrain GetDefaultMapTerrain()
	{
		return Instance.m_defaultMapTerrain;
	}
}
