using System.Collections.Generic;

namespace RLWorldCreation;

public class ChooseRandomDoorLocationsEventArgs : BiomeCreationEventArgs
{
	public GridPointManager Room { get; private set; }

	public IEnumerable<DoorLocation> PotentialLocations { get; private set; }

	public IEnumerable<DoorLocation> ChosenLocations { get; private set; }

	public ChooseRandomDoorLocationsEventArgs(GridPointManager room, IEnumerable<DoorLocation> potentialLocations, IEnumerable<DoorLocation> chosenLocations)
	{
		Initialize(room, potentialLocations, chosenLocations);
	}

	public void Initialize(GridPointManager room, IEnumerable<DoorLocation> potentialLocations, IEnumerable<DoorLocation> chosenLocations)
	{
		Room = room;
		PotentialLocations = potentialLocations;
		ChosenLocations = chosenLocations;
	}
}
