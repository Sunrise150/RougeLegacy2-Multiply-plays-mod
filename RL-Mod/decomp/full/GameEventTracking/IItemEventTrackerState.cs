using System.Collections.Generic;

namespace GameEventTracking;

public interface IItemEventTrackerState : IGameEventTrackerState
{
	List<ChestTrackerData> ChestsOpened { get; }

	List<ItemTrackerData> ItemsCollected { get; }

	void Initialise(List<ChestTrackerData> chestsOpened, List<ItemTrackerData> itemsCollected);
}
