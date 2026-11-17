using System.Collections.Generic;

namespace GameEventTracking;

public interface IEnemyEventTrackerState : IGameEventTrackerState
{
	List<EnemyTrackerData> EnemiesKilled { get; }

	void Initialise(List<EnemyTrackerData> enemiesKilled);
}
