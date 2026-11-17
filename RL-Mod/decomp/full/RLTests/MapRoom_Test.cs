using Rooms;
using UnityEngine;

namespace RLTests;

public class MapRoom_Test : MonoBehaviour
{
	[SerializeField]
	private Vector2Int m_gridCoords;

	[SerializeField]
	private Vector2Int m_roomSize = Vector2Int.one;

	[SerializeField]
	private RoomType m_roomType;

	[SerializeField]
	private BiomeType m_biome = BiomeType.Castle;

	[SerializeField]
	private RoomMetaData m_roomMetaData;

	[SerializeField]
	private int m_roomNumber;

	[SerializeField]
	private bool m_isMirrored;

	[SerializeField]
	private bool m_isTunnelDestination;

	[SerializeField]
	private Vector2 m_roomCoords;

	private void Start()
	{
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			RunTest();
		}
		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			RunTest2();
		}
	}

	private void RunTest2()
	{
		Debug.Log(GridController.GetWorldPositionFromRoomCoordinates(m_roomCoords, m_roomSize));
	}

	private void RunTest()
	{
		MapController.CreateMapRoomEntryForGridPoint(new GridPointManager(m_roomNumber, m_gridCoords, m_biome, m_roomType, m_roomMetaData, m_isMirrored, m_isTunnelDestination)).gameObject.SetActive(value: true);
	}
}
