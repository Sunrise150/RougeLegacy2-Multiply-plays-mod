using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HolidayGOController : MonoBehaviour, IRoomConsumer
{
	[SerializeField]
	private bool m_triggerOnSceneLoaded;

	[SerializeField]
	private bool m_triggerOnRoomEnter;

	[SerializeField]
	private HolidayType m_holidayType;

	[SerializeField]
	private bool m_enableOnActive = true;

	private Action<object, RoomViaDoorEventArgs> m_onPlayerEnter;

	public BaseRoom Room { get; private set; }

	public void SetRoom(BaseRoom room)
	{
		if (m_triggerOnRoomEnter)
		{
			Room = room;
			Room.PlayerEnterRelay.AddListener(m_onPlayerEnter);
		}
	}

	private void Awake()
	{
		if (m_triggerOnSceneLoaded)
		{
			SceneManager.sceneLoaded += OnSceneLoaded;
		}
		m_onPlayerEnter = OnPlayerEnter;
	}

	private void OnDisable()
	{
		if (m_triggerOnRoomEnter && (bool)Room)
		{
			Room.PlayerEnterRelay.RemoveListener(m_onPlayerEnter);
		}
	}

	private void OnDestroy()
	{
		if (m_triggerOnSceneLoaded)
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}
	}

	private void OnPlayerEnter(object sender, EventArgs args)
	{
		UpdateGO();
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		UpdateGO();
	}

	private void UpdateGO()
	{
		if (!this)
		{
			return;
		}
		if (HolidayLookController.IsHoliday(m_holidayType))
		{
			if (m_enableOnActive)
			{
				base.gameObject.SetActive(value: true);
			}
			else
			{
				base.gameObject.SetActive(value: false);
			}
		}
		else if (m_enableOnActive)
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			base.gameObject.SetActive(value: true);
		}
	}
}
