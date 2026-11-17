using System.Collections.Generic;

public class RoomSpawnControllerManager
{
	private static List<ISpawnController> m_spawnControllerHelper_STATIC = new List<ISpawnController>();

	private static List<PropSpawnController> m_propSpawnerHelper_STATIC = new List<PropSpawnController>();

	private static List<EnemySpawnController> m_enemySpawnerHelper_STATIC = new List<EnemySpawnController>();

	private static List<ISimpleSpawnController> m_noPropSimpleSpawnerHelper_STATIC = new List<ISimpleSpawnController>();

	private static List<ChestSpawnController> m_chestSpawnerHelper_STATIC = new List<ChestSpawnController>();

	private static List<TunnelSpawnController> m_tunnelSpawnerHelper_STATIC = new List<TunnelSpawnController>();

	private static List<IHazardSpawnController> m_hazardSpawnerHelper_STATIC = new List<IHazardSpawnController>();

	private static List<SpawnLogicController> m_spawnLogicHelper_STATIC = new List<SpawnLogicController>();

	public BaseRoom Room { get; }

	public ISpawnController[] SpawnControllers { get; private set; }

	public SpawnLogicController[] SpawnLogicControllers { get; private set; }

	public EnemySpawnController[] EnemySpawnControllers { get; private set; }

	public ISimpleSpawnController[] SimpleSpawnControllers_NoProps { get; private set; }

	public TunnelSpawnController[] TunnelSpawnControllers { get; private set; }

	public PropSpawnController[] PropSpawnControllers { get; private set; }

	public ChestSpawnController[] ChestSpawnControllers { get; private set; }

	public IHazardSpawnController[] HazardSpawnControllers { get; private set; }

	public RoomSpawnControllerManager(BaseRoom room)
	{
		Room = room;
		InitializeProperties();
	}

	private void InitializeProperties()
	{
		GenerateAllSpawnControllers();
		for (int i = 0; i < EnemySpawnControllers.Length; i++)
		{
			EnemySpawnControllers[i].SetEnemyIndex(i);
		}
		for (int j = 0; j < ChestSpawnControllers.Length; j++)
		{
			ChestSpawnControllers[j].SetChestIndex(j);
		}
	}

	private void GenerateAllSpawnControllers()
	{
		m_spawnControllerHelper_STATIC.Clear();
		m_propSpawnerHelper_STATIC.Clear();
		m_enemySpawnerHelper_STATIC.Clear();
		m_noPropSimpleSpawnerHelper_STATIC.Clear();
		m_chestSpawnerHelper_STATIC.Clear();
		m_tunnelSpawnerHelper_STATIC.Clear();
		m_hazardSpawnerHelper_STATIC.Clear();
		m_spawnLogicHelper_STATIC.Clear();
		SpawnControllers = Room.gameObject.GetComponentsInChildren<ISpawnController>(includeInactive: true);
		ISpawnController[] spawnControllers = SpawnControllers;
		foreach (ISpawnController spawnController in spawnControllers)
		{
			if ((bool)spawnController.SpawnLogicController)
			{
				m_spawnLogicHelper_STATIC.Add(spawnController.SpawnLogicController);
			}
			PropSpawnController propSpawnController = spawnController as PropSpawnController;
			if ((bool)propSpawnController)
			{
				m_propSpawnerHelper_STATIC.Add(propSpawnController);
				continue;
			}
			if (spawnController is ISimpleSpawnController item)
			{
				m_noPropSimpleSpawnerHelper_STATIC.Add(item);
			}
			EnemySpawnController enemySpawnController = spawnController as EnemySpawnController;
			if ((bool)enemySpawnController)
			{
				m_enemySpawnerHelper_STATIC.Add(enemySpawnController);
				continue;
			}
			ChestSpawnController chestSpawnController = spawnController as ChestSpawnController;
			if ((bool)chestSpawnController)
			{
				m_chestSpawnerHelper_STATIC.Add(chestSpawnController);
				continue;
			}
			TunnelSpawnController tunnelSpawnController = spawnController as TunnelSpawnController;
			if ((bool)tunnelSpawnController)
			{
				m_tunnelSpawnerHelper_STATIC.Add(tunnelSpawnController);
			}
			else if (spawnController is IHazardSpawnController item2)
			{
				m_hazardSpawnerHelper_STATIC.Add(item2);
			}
		}
		PropSpawnControllers = m_propSpawnerHelper_STATIC.ToArray();
		SimpleSpawnControllers_NoProps = m_noPropSimpleSpawnerHelper_STATIC.ToArray();
		EnemySpawnControllers = m_enemySpawnerHelper_STATIC.ToArray();
		TunnelSpawnControllers = m_tunnelSpawnerHelper_STATIC.ToArray();
		ChestSpawnControllers = m_chestSpawnerHelper_STATIC.ToArray();
		HazardSpawnControllers = m_hazardSpawnerHelper_STATIC.ToArray();
		SpawnLogicControllers = m_spawnLogicHelper_STATIC.ToArray();
		BaseRoom room = Room;
		PropSpawnController[] propSpawnControllers = PropSpawnControllers;
		for (int i = 0; i < propSpawnControllers.Length; i++)
		{
			propSpawnControllers[i].SetRoom(room);
		}
		EnemySpawnController[] enemySpawnControllers = EnemySpawnControllers;
		for (int i = 0; i < enemySpawnControllers.Length; i++)
		{
			enemySpawnControllers[i].SetRoom(room);
		}
		ChestSpawnController[] chestSpawnControllers = ChestSpawnControllers;
		for (int i = 0; i < chestSpawnControllers.Length; i++)
		{
			chestSpawnControllers[i].SetRoom(room);
		}
		TunnelSpawnController[] tunnelSpawnControllers = TunnelSpawnControllers;
		for (int i = 0; i < tunnelSpawnControllers.Length; i++)
		{
			tunnelSpawnControllers[i].SetRoom(room);
		}
		SpawnLogicController[] spawnLogicControllers = SpawnLogicControllers;
		for (int i = 0; i < spawnLogicControllers.Length; i++)
		{
			spawnLogicControllers[i].SetRoom(room);
		}
	}
}
