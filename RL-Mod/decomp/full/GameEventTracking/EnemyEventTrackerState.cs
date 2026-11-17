using System;
using System.Collections.Generic;

namespace GameEventTracking;

[Serializable]
public class EnemyEventTrackerState : IEnemyEventTrackerState, IGameEventTrackerState
{
	private List<EnemyTrackerData> m_enemiesKilled;

	public List<EnemyTrackerData> EnemiesKilled
	{
		get
		{
			return m_enemiesKilled;
		}
		private set
		{
			m_enemiesKilled = value;
		}
	}

	public EnemyEventTrackerState(List<EnemyTrackerData> enemiesKilled)
	{
		Initialise(enemiesKilled);
	}

	public void Initialise(List<EnemyTrackerData> enemiesKilled)
	{
		EnemiesKilled = enemiesKilled;
	}
}
