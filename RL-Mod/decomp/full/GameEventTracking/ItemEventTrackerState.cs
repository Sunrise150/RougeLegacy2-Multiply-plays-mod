using System.Collections.Generic;

namespace GameEventTracking;

public class ItemEventTrackerState : IItemEventTrackerState, IGameEventTrackerState
{
	private List<ChestTrackerData> m_chestsOpened;

	private List<ItemTrackerData> m_goldCollected;

	public List<ChestTrackerData> ChestsOpened
	{
		get
		{
			return m_chestsOpened;
		}
		private set
		{
			m_chestsOpened = value;
		}
	}

	public List<ItemTrackerData> ItemsCollected
	{
		get
		{
			return m_goldCollected;
		}
		private set
		{
			m_goldCollected = value;
		}
	}

	public ItemEventTrackerState(List<ChestTrackerData> chestsOpened, List<ItemTrackerData> goldCollected)
	{
		Initialise(chestsOpened, goldCollected);
	}

	public void Initialise(List<ChestTrackerData> chestsOpened, List<ItemTrackerData> goldCollected)
	{
		ChestsOpened = chestsOpened;
		ItemsCollected = goldCollected;
	}
}
