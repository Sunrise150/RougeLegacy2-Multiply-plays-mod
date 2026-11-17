using UnityEngine;

public class LQASwapEnemyRoomController : MonoBehaviour
{
	[SerializeField]
	private EnemySpawnController m_enemySpawner;

	private EnemyType[] m_exclusionArray = new EnemyType[6]
	{
		EnemyType.Eggplant,
		EnemyType.Ninja,
		EnemyType.Ghost,
		EnemyType.RogueKnight,
		EnemyType.JumpWallStick,
		EnemyType.StealthAssassin
	};

	private void SwapEnemyType(bool swapUp)
	{
		int num = EnemyTypes_RL.TypeArray.IndexOf(m_enemySpawner.Type);
		if (num == -1)
		{
			return;
		}
		int num2 = 0;
		bool flag = false;
		if (swapUp)
		{
			do
			{
				num++;
				num2++;
				if (num >= EnemyTypes_RL.TypeArray.Length - 1)
				{
					num = 1;
				}
				flag = m_exclusionArray.IndexOf(EnemyTypes_RL.TypeArray[num]) != -1;
			}
			while ((!EnemyLibrary.GetEnemyPrefab(m_enemySpawner.Type, m_enemySpawner.Rank) || flag) && num2 < 100);
		}
		else
		{
			do
			{
				num--;
				num2++;
				if (num <= 0)
				{
					num = EnemyTypes_RL.TypeArray.Length - 2;
				}
				flag = m_exclusionArray.IndexOf(EnemyTypes_RL.TypeArray[num]) != -1;
			}
			while ((!EnemyLibrary.GetEnemyPrefab(m_enemySpawner.Type, m_enemySpawner.Rank) || flag) && num2 < 100);
		}
		m_enemySpawner.Type = EnemyTypes_RL.TypeArray[num];
		m_enemySpawner.Rank = EnemyRank.Basic;
		UpdateSpawnedEnemy();
	}

	private void SwapEnemyRank(bool swapUp)
	{
		int num = EnemyTypes_RL.RankArray.IndexOf(m_enemySpawner.Rank);
		if (num == -1)
		{
			return;
		}
		int num2 = 0;
		if (swapUp)
		{
			do
			{
				num++;
				num2++;
				if (num >= EnemyTypes_RL.RankArray.Length - 2)
				{
					num = 0;
				}
			}
			while (!EnemyLibrary.GetEnemyPrefab(m_enemySpawner.Type, m_enemySpawner.Rank) && num2 < 100);
		}
		else
		{
			do
			{
				num--;
				num2++;
				if (num < 0)
				{
					num = EnemyTypes_RL.RankArray.Length - 3;
				}
			}
			while (!EnemyLibrary.GetEnemyPrefab(m_enemySpawner.Type, m_enemySpawner.Rank) && num2 < 100);
		}
		m_enemySpawner.Rank = EnemyTypes_RL.RankArray[num];
		UpdateSpawnedEnemy();
	}

	private void UpdateSpawnedEnemy()
	{
		if ((bool)EnemyLibrary.GetEnemyPrefab(m_enemySpawner.Type, m_enemySpawner.Rank))
		{
			if ((bool)m_enemySpawner.EnemyInstance && m_enemySpawner.EnemyInstance.gameObject.activeInHierarchy)
			{
				m_enemySpawner.EnemyInstance.gameObject.SetActive(value: false);
			}
			EnemyManager.CreateBiomePools(GetComponent<Room>().BiomeType);
			m_enemySpawner.InitializeEnemyInstance();
		}
	}

	private void Update()
	{
		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
		{
			if (Input.GetKeyDown(KeyCode.Equals))
			{
				SwapEnemyType(swapUp: true);
			}
			else if (Input.GetKeyDown(KeyCode.Minus))
			{
				SwapEnemyType(swapUp: false);
			}
			else if (Input.GetKeyDown(KeyCode.RightBracket))
			{
				SwapEnemyRank(swapUp: true);
			}
			else if (Input.GetKeyDown(KeyCode.LeftBracket))
			{
				SwapEnemyRank(swapUp: false);
			}
		}
	}
}
