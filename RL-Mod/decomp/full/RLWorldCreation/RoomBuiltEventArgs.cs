namespace RLWorldCreation;

public class RoomBuiltEventArgs : BiomeCreationEventArgs
{
	public GridPointManager GridPointManager { get; }

	public RoomBuiltEventArgs(GridPointManager gridPointManager)
	{
		GridPointManager = gridPointManager;
	}
}
