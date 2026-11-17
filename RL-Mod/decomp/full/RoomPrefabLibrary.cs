using Cinemachine;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Room Prefab Library")]
public class RoomPrefabLibrary : ScriptableObject
{
	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/RoomPrefabLibrary";

	[SerializeField]
	private Ferr2DT_PathTerrain m_bottomDoorOneWayPrefab;

	[SerializeField]
	private Ferr2DT_PathTerrain m_doorSealPrefab;

	[SerializeField]
	private Ferr2DT_PathTerrain m_decoTerrainPrefab;

	[SerializeField]
	private CinemachineVirtualCamera m_cinemachineVirtualCameraPrefab;

	[SerializeField]
	private Room_DebugUI m_room_DebugUI_Prefab;

	[SerializeField]
	private Ferr2DT_Material m_defaultDontMergeFerr2DMaterial;

	[SerializeField]
	private Ferr2DT_Material m_studyDontMergeFerr2DMaterial;

	private static RoomPrefabLibrary m_instance;

	private static RoomPrefabLibrary Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = CDGResources.Load<RoomPrefabLibrary>("Scriptable Objects/Libraries/RoomPrefabLibrary");
			}
			return m_instance;
		}
	}

	public static Ferr2DT_PathTerrain BottomDoorOneWayPrefab => Instance.m_bottomDoorOneWayPrefab;

	public static Ferr2DT_PathTerrain DoorSealPrefab => Instance.m_doorSealPrefab;

	public static Ferr2DT_PathTerrain DecoTerrainPrefab => Instance.m_decoTerrainPrefab;

	public static CinemachineVirtualCamera CinemachineVirtualCameraPrefab => Instance.m_cinemachineVirtualCameraPrefab;

	public static Room_DebugUI Room_DebugUI_Prefab => Instance.m_room_DebugUI_Prefab;

	public static Ferr2DT_Material DefaultDontMergeFerr2DMaterial => Instance.m_defaultDontMergeFerr2DMaterial;

	public static Ferr2DT_Material StudyDontMergeFerr2DMaterial => Instance.m_studyDontMergeFerr2DMaterial;
}
