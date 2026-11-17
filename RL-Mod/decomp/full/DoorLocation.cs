using System;
using UnityEngine;

[Serializable]
public struct DoorLocation : IEquatable<DoorLocation>
{
	[SerializeField]
	private RoomSide m_roomSide;

	[SerializeField]
	private int m_doorNumber;

	public static DoorLocation Empty => new DoorLocation(RoomSide.None, -1);

	public RoomSide RoomSide
	{
		get
		{
			return m_roomSide;
		}
		private set
		{
			m_roomSide = value;
		}
	}

	public int DoorNumber
	{
		get
		{
			return m_doorNumber;
		}
		private set
		{
			m_doorNumber = value;
		}
	}

	public DoorLocation(RoomSide roomSide, int doorNumber)
	{
		m_roomSide = roomSide;
		m_doorNumber = doorNumber;
	}

	public static bool operator ==(DoorLocation doorLocation01, DoorLocation doorLocation02)
	{
		return doorLocation01.Equals(doorLocation02);
	}

	public static bool operator !=(DoorLocation doorLocation01, DoorLocation doorLocation02)
	{
		return !(doorLocation01 == doorLocation02);
	}

	public override string ToString()
	{
		return $"({RoomSide}, {DoorNumber})";
	}

	public override bool Equals(object obj)
	{
		if (!(obj is DoorLocation doorLocation))
		{
			return false;
		}
		if (RoomSide == doorLocation.RoomSide)
		{
			return DoorNumber == doorLocation.DoorNumber;
		}
		return false;
	}

	public bool Equals(DoorLocation other)
	{
		if (RoomSide == other.RoomSide)
		{
			return DoorNumber == other.DoorNumber;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (-785574603 * -1521134295 + RoomSide.GetHashCode()) * -1521134295 + DoorNumber.GetHashCode();
	}
}
