using System.Collections.Generic;
using System.Diagnostics;
using Rooms;
using UnityEngine;

public class DebugCreateRooms : MonoBehaviour
{
	private interface IBuildRoomReport
	{
	}

	private class PotentialRoomsBuildReport : IBuildRoomReport
	{
		public BiomeType Biome { get; }

		public int OriginRoomNumber { get; }

		public Vector2Int OriginRoomCoords { get; }

		public DoorLocation DoorLocation { get; }

		public RoomType RoomType { get; }

		public HashSet<RoomSetEntry> PotentialRooms { get; }

		public int PotentialRoomCount => PotentialRooms.Count;

		public PotentialRoomsBuildReport(GridPointManager originRoom, DoorLocation doorLocation, RoomTypeEntry roomRequirements, HashSet<RoomSetEntry> potentialRooms)
		{
			Biome = originRoom.Biome;
			OriginRoomNumber = originRoom.RoomNumber;
			OriginRoomCoords = originRoom.GridCoordinates;
			DoorLocation = doorLocation;
			RoomType = roomRequirements.RoomType;
			PotentialRooms = potentialRooms;
		}
	}

	private class BuildRoomReport : IBuildRoomReport
	{
		public BiomeType Biome { get; }

		public Vector2Int Coords { get; }

		public DoorLocation DoorLocation { get; }

		public Vector2Int RoomSize { get; }

		public BuildRoomReport(BiomeType biome, Vector2Int coords, DoorLocation doorLocation, RoomSetEntry room)
		{
			Biome = biome;
			Coords = coords;
			DoorLocation = doorLocation;
			RoomSize = room.RoomMetaData.Size;
		}
	}

	private static Queue<IBuildRoomReport> m_buildRoomReports = new Queue<IBuildRoomReport>();

	public static void BuildRoom(BiomeType biome, Vector2Int coords, DoorLocation doorLocation, RoomSetEntry room)
	{
		m_buildRoomReports.Enqueue(new BuildRoomReport(biome, coords, doorLocation, room));
	}

	public static void PotentialRooms(GridPointManager originRoom, DoorLocation doorLocation, RoomTypeEntry roomRequirements, HashSet<RoomSetEntry> potentialRooms)
	{
		m_buildRoomReports.Enqueue(new PotentialRoomsBuildReport(originRoom, doorLocation, roomRequirements, potentialRooms));
	}

	public static void Reset()
	{
		m_buildRoomReports.Clear();
	}

	internal static void MaxIterationCountReached()
	{
		Debugger.Break();
	}
}
