using System.Collections.Generic;

namespace GameEventTracking;

public interface IGameEventTracker<T> where T : IGameEventTrackerState
{
	IEnumerable<IGameEventData> GetGameEvents();

	void Reset();

	void RestoreState(T state);

	T SaveState();
}
