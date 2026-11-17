using System.Collections.Generic;
using Rooms;
using UnityEngine;

namespace RLWorldCreation;

public class RoomReplacedEventArgs : BiomeCreationEventArgs
{
	private RoomMetaData OriginalMetaData;

	private bool OriginalIsMirrored;

	private Vector2Int OriginalSize;

	private List<DoorLocation> OriginalDoorLocations;

	private RoomMetaData ReplacementMetaData;

	private bool ReplacementIsMirrored;

	private Vector2Int ReplacementSize;

	private IEnumerable<DoorLocation> ReplacementDoorLocations;

	public RoomReplacedEventArgs(RoomMetaData originalMetaData, bool originalIsMirrored, Vector2Int originalSize, List<DoorLocation> originalDoorLocations, RoomMetaData replacementMetaData, bool replacementIsMirrored, Vector2Int replacementSize, IEnumerable<DoorLocation> replacementDoorLocations)
	{
		OriginalMetaData = originalMetaData;
		OriginalIsMirrored = originalIsMirrored;
		OriginalSize = originalSize;
		OriginalDoorLocations = originalDoorLocations;
		ReplacementMetaData = replacementMetaData;
		ReplacementIsMirrored = replacementIsMirrored;
		ReplacementSize = replacementSize;
		ReplacementDoorLocations = replacementDoorLocations;
	}
}
