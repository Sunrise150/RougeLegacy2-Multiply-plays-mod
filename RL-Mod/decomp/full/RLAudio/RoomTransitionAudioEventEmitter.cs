using System;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

namespace RLAudio;

public class RoomTransitionAudioEventEmitter : MonoBehaviour, IAudioEventEmitter
{
	[SerializeField]
	[EventRef]
	private string m_enterLeftTransitionPath;

	[SerializeField]
	[EventRef]
	private string m_enterRightTransitionPath;

	[SerializeField]
	[EventRef]
	private string m_enterCenterTransitionPath;

	[SerializeField]
	[EventRef]
	private string m_bossEntranceRoomDiscoveredPath;

	[SerializeField]
	[EventRef]
	private string m_enterTransitionRoomPath;

	private string m_description;

	private List<BiomeType> m_hasBossRoomBeenDiscovered = new List<BiomeType>();

	private List<BiomeType> m_hasTransitionRoomBeenDiscovered = new List<BiomeType>();

	private Action<MonoBehaviour, EventArgs> m_onPlayerEnterRoom;

	public string Description
	{
		get
		{
			if (string.IsNullOrEmpty(m_description))
			{
				m_description = ToString();
			}
			return m_description;
		}
	}

	private void Awake()
	{
		m_onPlayerEnterRoom = OnPlayerEnterRoom;
	}

	private void Start()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
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
		if (roomViaDoorEventArgs.ViaDoor != null)
		{
			PlayTransition(roomViaDoorEventArgs.ViaDoor.Side);
		}
		if (roomViaDoorEventArgs.Room.RoomType == RoomType.BossEntrance)
		{
			if (!m_hasBossRoomBeenDiscovered.Contains(roomViaDoorEventArgs.Room.AppearanceBiomeType))
			{
				m_hasBossRoomBeenDiscovered.Add(roomViaDoorEventArgs.Room.AppearanceBiomeType);
				AudioManager.PlayOneShot(this, m_bossEntranceRoomDiscoveredPath);
			}
		}
		else if (roomViaDoorEventArgs.Room.RoomType == RoomType.Transition && !m_hasTransitionRoomBeenDiscovered.Contains(roomViaDoorEventArgs.Room.AppearanceBiomeType))
		{
			m_hasTransitionRoomBeenDiscovered.Add(roomViaDoorEventArgs.Room.AppearanceBiomeType);
			AudioManager.PlayOneShot(this, m_enterTransitionRoomPath);
		}
	}

	private void PlayTransition(RoomSide side)
	{
		switch (side)
		{
		case RoomSide.Top:
		case RoomSide.Bottom:
			AudioManager.PlayOneShot(this, m_enterCenterTransitionPath);
			break;
		case RoomSide.Left:
			AudioManager.PlayOneShot(this, m_enterLeftTransitionPath);
			break;
		case RoomSide.Right:
			AudioManager.PlayOneShot(this, m_enterRightTransitionPath);
			break;
		}
	}
}
