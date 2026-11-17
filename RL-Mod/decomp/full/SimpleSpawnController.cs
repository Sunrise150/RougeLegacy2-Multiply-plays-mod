using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SimpleSpawnController : MonoBehaviour, IRoomConsumer, ISimpleSpawnController, ISpawnController
{
	public static List<IRoomConsumer> m_roomConsumerListHelper_STATIC = new List<IRoomConsumer>();

	[SerializeField]
	protected bool m_instantiatePrefab;

	[SerializeField]
	protected GameObject m_prefab;

	private BaseRoom m_room;

	private SpawnLogicController m_spawnLogic;

	private bool m_hasCheckedForSpawnLogicController;

	public GameObject GameObject => base.gameObject;

	public BaseRoom Room => m_room;

	public bool ShouldSpawn
	{
		get
		{
			if (SpawnLogicController != null)
			{
				return SpawnLogicController.ShouldSpawn;
			}
			return true;
		}
	}

	public SpawnLogicController SpawnLogicController
	{
		get
		{
			if (!m_hasCheckedForSpawnLogicController)
			{
				m_hasCheckedForSpawnLogicController = true;
				m_spawnLogic = GetComponent<SpawnLogicController>();
			}
			return m_spawnLogic;
		}
		private set
		{
			m_spawnLogic = value;
		}
	}

	public void SetRoom(BaseRoom value)
	{
		m_room = value;
	}

	public bool Spawn()
	{
		if (ShouldSpawn)
		{
			DoIsSpawned();
			return true;
		}
		DoIsNotSpawned();
		return false;
	}

	protected virtual void DoIsSpawned()
	{
		if (m_instantiatePrefab)
		{
			if ((bool)m_prefab)
			{
				GameObject obj = Object.Instantiate(m_prefab, base.transform.parent);
				obj.transform.position = base.transform.position;
				List<IRoomConsumer> roomConsumerListHelper_STATIC = m_roomConsumerListHelper_STATIC;
				roomConsumerListHelper_STATIC.Clear();
				obj.GetComponentsInChildren(roomConsumerListHelper_STATIC);
				foreach (IRoomConsumer item in roomConsumerListHelper_STATIC)
				{
					item.SetRoom(Room);
				}
			}
			else
			{
				Debug.LogFormat("<color=red>m_prefab Field is null on ({0})</color>", this);
			}
			base.gameObject.SetActive(value: false);
		}
		else if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
		}
	}

	protected virtual void DoIsNotSpawned()
	{
		base.gameObject.SetActive(value: false);
	}

	[SpecialName]
	GameObject ISpawnController.get_gameObject()
	{
		return base.gameObject;
	}
}
