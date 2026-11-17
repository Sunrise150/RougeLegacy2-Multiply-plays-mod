using System.Collections.Generic;
using UnityEngine;

public class LineageScreen_RoomBuilder : MonoBehaviour
{
	public const int NUM_PORTRAIT_ROWS = 7;

	public const int NUM_PORTRAIT_COLUMNS = 3;

	public const int NUM_PORTRAITS_PER_ROOM = 21;

	private int DEBUG_NUM_PORTRAITS = 40;

	[SerializeField]
	private Room m_startingRoom;

	[SerializeField]
	private Room m_portraitRoom;

	[SerializeField]
	private GameObject m_portrait;

	private List<GameObject> m_portraitList;

	private List<Vector3> m_roomPositionList;

	private Vector3 m_portraitStartingPosOffset = new Vector3(3f, -2f, 0f);

	public List<GameObject> PortraitList => m_portraitList;

	public List<Vector3> RoomPositionList => m_roomPositionList;

	private void Awake()
	{
		m_portraitList = new List<GameObject>();
		ConstructPortraitRooms(DEBUG_NUM_PORTRAITS);
		m_startingRoom.gameObject.SetActive(value: false);
		m_portraitRoom.gameObject.SetActive(value: false);
		m_portrait.gameObject.SetActive(value: false);
	}

	private void ConstructPortraitRooms(int numPortraits)
	{
		float num = 21f;
		int num2 = Mathf.CeilToInt((float)numPortraits / num);
		Room room = Object.Instantiate(m_startingRoom);
		room.Initialise(BiomeType.Castle, RoomType.Standard);
		List<Room> list = new List<Room>();
		list.Add(room);
		m_roomPositionList = new List<Vector3>();
		for (int i = 0; i < num2; i++)
		{
			Room room2 = Object.Instantiate(m_portraitRoom);
			room2.Initialise(BiomeType.Castle, RoomType.Boss, 1);
			Vector3 position = room.gameObject.transform.position;
			position.x -= (i + 1) * room2.UnitDimensions.x;
			room2.gameObject.transform.position = position;
			m_roomPositionList.Add(position);
			list.Add(room2);
		}
		Vector3 vector = room.Coordinates;
		vector.y += room.UnitDimensions.y;
		vector.z = 1f;
		vector += m_portraitStartingPosOffset;
		GameObject gameObject = new GameObject();
		gameObject.name = "Portrait Container";
		gameObject.transform.position = vector;
		float num3 = (float)room.UnitDimensions.x / 7f;
		float num4 = 4f;
		int num5 = 0;
		for (int j = 0; j < numPortraits; j++)
		{
			GameObject gameObject2 = Object.Instantiate(m_portrait);
			gameObject2.transform.SetParent(gameObject.transform, worldPositionStays: false);
			Vector3 position2 = vector;
			if (j % 3 == 0)
			{
				num5++;
			}
			position2.x -= num3 * (float)num5;
			position2.y -= num4 * ((float)j % 3f);
			gameObject2.transform.position = position2;
			m_portraitList.Add(gameObject2);
		}
		Vector3 position3 = gameObject.transform.position;
		position3.z = m_portrait.transform.position.z;
		gameObject.transform.position = position3;
	}
}
