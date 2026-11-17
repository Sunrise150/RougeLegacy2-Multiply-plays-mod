using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameEventTracking;

public class EnemyEventTracker : MonoBehaviour, IGameEventTracker<IEnemyEventTrackerState>
{
	private IEnemyEventTrackerState m_state;

	private List<EnemyTrackerData> m_enemiesKilled = new List<EnemyTrackerData>();

	private string m_playerKiller = "UNDEFINED";

	private string m_slainByText = "UNDEFINED";

	private string m_playerKillerLocID;

	private string m_slainByLocID;

	private Action<MonoBehaviour, EventArgs> m_onEnemyKilled;

	private Action<MonoBehaviour, EventArgs> m_onPlayerKilled;

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

	private void Awake()
	{
		m_onEnemyKilled = OnEnemyKilled;
		m_onPlayerKilled = OnPlayerKilled;
	}

	private void Start()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.EnemyDeath, m_onEnemyKilled);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerDeath, m_onPlayerKilled);
	}

	private void OnDestroy()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.EnemyDeath, m_onEnemyKilled);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerDeath, m_onPlayerKilled);
	}

	public IEnumerable<IGameEventData> GetGameEvents()
	{
		foreach (EnemyTrackerData item in EnemiesKilled)
		{
			yield return item;
		}
	}

	public string GetPlayerKiller()
	{
		return m_playerKiller;
	}

	public string GetSlainBy()
	{
		return m_slainByText;
	}

	private void OnEnemyKilled(MonoBehaviour sender, EventArgs eventArgs)
	{
		if (eventArgs is EnemyDeathEventArgs)
		{
			EnemyController victim = (eventArgs as EnemyDeathEventArgs).Victim;
			if (!victim.IsSummoned || victim.IsBoss)
			{
				EnemiesKilled.Add(new EnemyTrackerData(victim.Room.BiomeType, victim.Room.BiomeControllerIndex, victim.EnemyType, victim.EnemyRank, victim.EnemyIndex));
			}
		}
		else
		{
			Debug.LogFormat("<color=red>| {0} | Failed to cast event args as EnemyDeathEventArgs</color>", this);
		}
	}

	private void OnPlayerKilled(MonoBehaviour sender, EventArgs eventArgs)
	{
		m_slainByLocID = null;
		m_playerKillerLocID = null;
		if (SaveManager.PlayerSaveData.CurrentCharacter.IsRetired)
		{
			m_slainByText = LocalizationManager.GetString("LOC_ID_DEATH_UI_RETIRED_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			m_playerKiller = "";
		}
		else if (eventArgs is PlayerDeathEventArgs)
		{
			PlayerDeathEventArgs playerDeathEventArgs = eventArgs as PlayerDeathEventArgs;
			bool flag = false;
			if (playerDeathEventArgs.Killer != null)
			{
				if (playerDeathEventArgs.Killer.CompareTag("Enemy"))
				{
					EnemyController component = playerDeathEventArgs.Killer.GetComponent<EnemyController>();
					EnemyData enemyData = component.EnemyData;
					m_playerKillerLocID = enemyData.Title;
					if (!component.IsBoss)
					{
						m_slainByLocID = "LOC_ID_DEATH_UI_SLAIN_BY_ENEMY_1";
					}
					else
					{
						m_slainByLocID = "LOC_ID_DEATH_UI_SLAIN_BY_BOSS_1";
					}
				}
				else if (playerDeathEventArgs.Killer.CompareTag("Hazard"))
				{
					m_slainByLocID = "LOC_ID_DEATH_UI_SLAIN_BY_ENEMY_1";
					m_playerKillerLocID = "LOC_ID_ENEMY_TITLE_Hazard_Basic_1";
					m_slainByText = LocalizationManager.GetString("LOC_ID_DEATH_UI_SLAIN_BY_ENEMY_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
					m_playerKiller = LocalizationManager.GetString("LOC_ID_ENEMY_TITLE_Hazard_Basic_1", isFemale: false);
				}
				else
				{
					m_slainByLocID = "LOC_ID_DEATH_UI_SLAIN_BY_ENEMY_1";
					m_playerKiller = LocalizationManager.GetString("LOC_ID_DEATH_UI_MYSTERIOUS_FORCE_1", isFemale: false);
					flag = true;
				}
			}
			else
			{
				m_slainByLocID = "LOC_ID_DEATH_UI_SLAIN_BY_ENEMY_1";
				m_playerKiller = LocalizationManager.GetString("LOC_ID_DEATH_UI_MYSTERIOUS_FORCE_1", isFemale: false);
				flag = true;
			}
			if (!flag)
			{
				m_playerKiller = LocalizationManager.GetString(m_playerKillerLocID, isFemale: false);
			}
			m_slainByText = LocalizationManager.GetString(m_slainByLocID, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
		else
		{
			Debug.LogFormat("<color=red>| {0} | Failed to cast event args as PlayerDeathEventArgs</color>", this);
		}
	}

	public void Reset()
	{
		if (EnemiesKilled != null)
		{
			EnemiesKilled.Clear();
		}
		m_playerKiller = "UNDEFINED";
		m_slainByText = "UNDEFINED";
	}

	public void RestoreState(IEnemyEventTrackerState state)
	{
		EnemiesKilled = state.EnemiesKilled;
	}

	public IEnemyEventTrackerState SaveState()
	{
		if (m_state == null)
		{
			m_state = new EnemyEventTrackerState(EnemiesKilled);
		}
		else
		{
			m_state.Initialise(EnemiesKilled);
		}
		return m_state;
	}

	public void ForceRefreshText()
	{
		if (!string.IsNullOrEmpty(m_playerKillerLocID))
		{
			m_playerKiller = LocalizationManager.GetString(m_playerKillerLocID, isFemale: false);
		}
		if (!string.IsNullOrEmpty(m_slainByLocID))
		{
			m_slainByText = LocalizationManager.GetString(m_slainByLocID, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
	}
}
