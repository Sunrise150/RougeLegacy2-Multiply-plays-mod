using System;

public class RoomType_RL
{
	private static SpecialRoomType[] m_specialRoomTypeArray;

	private static RoomType[] m_roomTypeArray;

	public static RoomType[] RoomTypeArray
	{
		get
		{
			if (m_roomTypeArray == null)
			{
				m_roomTypeArray = Enum.GetValues(typeof(RoomType)) as RoomType[];
			}
			return m_roomTypeArray;
		}
	}

	public static SpecialRoomType[] SpecialRoomTypeArray
	{
		get
		{
			if (m_specialRoomTypeArray == null)
			{
				m_specialRoomTypeArray = Enum.GetValues(typeof(SpecialRoomType)) as SpecialRoomType[];
			}
			return m_specialRoomTypeArray;
		}
	}
}
