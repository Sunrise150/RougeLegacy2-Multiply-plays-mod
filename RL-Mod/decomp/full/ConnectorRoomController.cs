using UnityEngine;

public class ConnectorRoomController : MonoBehaviour
{
	[SerializeField]
	private Room m_room;

	[SerializeField]
	private TunnelSpawnController m_tunnelSpawnController;

	public TunnelSpawnController TunnelSpawnController
	{
		get
		{
			return m_tunnelSpawnController;
		}
		set
		{
			m_tunnelSpawnController = value;
		}
	}

	public Room Room
	{
		get
		{
			return m_room;
		}
		set
		{
			m_room = value;
		}
	}

	private void Awake()
	{
		TunnelSpawnController.GameObject.SetActive(value: false);
	}
}
