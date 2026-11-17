using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class Weather : MonoBehaviour
{
	private const int MAX_PARTICLE_SYSTEM_SEED_VALUE = 1000;

	private ParticleSystem[] m_particleSystems;

	private Coroutine m_initializationCoroutine;

	private bool m_followCamera = true;

	private Action<MonoBehaviour, EventArgs> m_onPlayerEnterRoom;

	public List<BaseRoom> RoomList { get; private set; } = new List<BaseRoom>();


	public bool IsHeirloomWeather { get; set; }

	private void Awake()
	{
		m_particleSystems = GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < m_particleSystems.Length; i++)
		{
			m_particleSystems[i].Stop();
			m_particleSystems[i].Clear();
			m_particleSystems[i].randomSeed = (uint)UnityEngine.Random.Range(0, 1000);
		}
		m_onPlayerEnterRoom = OnPlayerEnterRoom;
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
		base.gameObject.SetActive(value: false);
	}

	private void OnDestroy()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
	}

	private void OnPlayerEnterRoom(MonoBehaviour sender, EventArgs eventArgs)
	{
		if (!(eventArgs is RoomViaDoorEventArgs roomViaDoorEventArgs))
		{
			return;
		}
		if (GetWeatherShouldBeActive(roomViaDoorEventArgs.Room))
		{
			if (!base.gameObject.activeInHierarchy)
			{
				base.gameObject.SetActive(value: true);
			}
		}
		else if (base.gameObject.activeInHierarchy)
		{
			base.gameObject.SetActive(value: false);
		}
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		BaseRoom room = roomViaDoorEventArgs.Room;
		float x = room.Bounds.center.x;
		float y = room.Bounds.center.y;
		if (m_particleSystems != null)
		{
			if (m_initializationCoroutine != null)
			{
				StopCoroutine(m_initializationCoroutine);
				m_initializationCoroutine = null;
			}
			m_initializationCoroutine = StartCoroutine(InitializeParticleSystems(new Vector2(x, y)));
		}
	}

	private bool GetWeatherShouldBeActive(BaseRoom room)
	{
		bool result = false;
		if (RoomList.Contains(room))
		{
			result = true;
		}
		return result;
	}

	private void OnCameraActivated(ICinemachineCamera cinemachineCameraA, ICinemachineCamera cinemachineCameraB)
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		Vector2 position = (cinemachineCameraA as CinemachineVirtualCamera).State.FinalPosition;
		if (m_particleSystems != null)
		{
			if (m_initializationCoroutine != null)
			{
				StopCoroutine(m_initializationCoroutine);
				m_initializationCoroutine = null;
			}
			m_initializationCoroutine = StartCoroutine(InitializeParticleSystems(position));
		}
	}

	private IEnumerator InitializeParticleSystems(Vector2 position)
	{
		m_followCamera = false;
		if (m_particleSystems != null)
		{
			base.transform.position = position;
			for (int i = 0; i < m_particleSystems.Length; i++)
			{
				m_particleSystems[i].Clear();
				m_particleSystems[i].Simulate(10f);
				m_particleSystems[i].Play();
			}
		}
		yield return null;
		m_followCamera = true;
	}

	private void Update()
	{
		if (m_followCamera)
		{
			base.gameObject.transform.localPosition = (Vector2)CameraController.GameCamera.transform.localPosition;
		}
	}
}
