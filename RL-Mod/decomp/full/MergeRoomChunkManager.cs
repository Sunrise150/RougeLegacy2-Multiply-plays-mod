using System;
using System.Collections.Generic;
using Sigtrap.Relays;
using UnityEngine;

public class MergeRoomChunkManager : MonoBehaviour
{
	private MergeRoom m_mergeRoom;

	private Relay<Vector2> m_playerEnteredChunkRelay = new Relay<Vector2>();

	private Vector2[] m_mergeRoomChunks;

	private PlayerController m_playerController;

	private int m_currentChunkIndex;

	private Action<object, EventArgs> m_onEnterMergeRoom;

	private Action<object, EventArgs> m_onExitMergeRoom;

	private bool m_runManager;

	public IRelayLink<Vector2> PlayerEnteredChunkRelay => m_playerEnteredChunkRelay.link;

	public bool IsInitialized { get; private set; }

	public MergeRoom MergeRoom
	{
		get
		{
			return m_mergeRoom;
		}
		private set
		{
			m_mergeRoom = value;
		}
	}

	private void Awake()
	{
		m_onEnterMergeRoom = OnEnterMergeRoom;
		m_onExitMergeRoom = OnExitMergeRoom;
	}

	private void OnDestroy()
	{
		if ((bool)MergeRoom)
		{
			MergeRoom.PlayerEnterRelay.RemoveListener(m_onEnterMergeRoom);
			MergeRoom.PlayerExitRelay.RemoveListener(m_onExitMergeRoom);
		}
	}

	private void OnEnterMergeRoom(object sender, EventArgs args)
	{
		m_runManager = true;
		m_currentChunkIndex = -1;
	}

	private void OnExitMergeRoom(object sender, EventArgs args)
	{
		m_runManager = false;
		m_currentChunkIndex = -1;
	}

	public void Initialize(MergeRoom mergeRoom, List<Room> rooms)
	{
		MergeRoom = mergeRoom;
		MergeRoom.PlayerEnterRelay.AddListener(m_onEnterMergeRoom);
		MergeRoom.PlayerExitRelay.AddListener(m_onExitMergeRoom);
		List<Vector2> list = new List<Vector2>(rooms.Count);
		for (int i = 0; i < rooms.Count; i++)
		{
			Room room = rooms[i];
			list.Add(new Vector2(room.Bounds.min.x, room.Bounds.max.x));
		}
		list.Sort(SortChunks);
		m_mergeRoomChunks = list.ToArray();
		m_playerController = PlayerManager.GetPlayerController();
		m_currentChunkIndex = -1;
		IsInitialized = true;
	}

	private int SortChunks(Vector2 a, Vector2 b)
	{
		if (a.x < b.x)
		{
			return -1;
		}
		if (a.x > b.x)
		{
			return 1;
		}
		return 0;
	}

	private void FixedUpdate()
	{
		if (!IsInitialized || !m_runManager)
		{
			return;
		}
		int num = m_mergeRoomChunks.Length;
		if (num <= 1)
		{
			return;
		}
		if (m_currentChunkIndex < 0)
		{
			UpdateChunkIndex();
			return;
		}
		float x = m_playerController.CollisionBounds.min.x;
		float x2 = m_playerController.CollisionBounds.max.x;
		bool num2 = m_currentChunkIndex > 0;
		bool flag = m_currentChunkIndex < num - 1;
		bool flag2 = false;
		if (num2)
		{
			Vector2 t = m_mergeRoomChunks[m_currentChunkIndex - 1];
			if (x2 < t.x)
			{
				UpdateChunkIndex();
				flag2 = true;
			}
			else if (x2 < t.y)
			{
				m_playerEnteredChunkRelay.Dispatch(t);
				m_currentChunkIndex--;
				flag2 = true;
			}
		}
		if (flag && !flag2)
		{
			Vector2 t2 = m_mergeRoomChunks[m_currentChunkIndex + 1];
			if (x > t2.y)
			{
				UpdateChunkIndex();
			}
			else if (x >= t2.x)
			{
				m_playerEnteredChunkRelay.Dispatch(t2);
				m_currentChunkIndex++;
			}
		}
	}

	private void UpdateChunkIndex()
	{
		int num = m_mergeRoomChunks.Length;
		float x = m_playerController.Midpoint.x;
		int currentChunkIndex = m_currentChunkIndex;
		bool flag = false;
		if (x < m_mergeRoomChunks[0].x)
		{
			m_currentChunkIndex = 0;
			flag = true;
		}
		else if (x > m_mergeRoomChunks[m_mergeRoomChunks.Length - 1].y)
		{
			m_currentChunkIndex = m_mergeRoomChunks.Length - 1;
			flag = true;
		}
		if (!flag)
		{
			for (int i = 0; i < num; i++)
			{
				if (i < num - 1)
				{
					float x2 = m_mergeRoomChunks[i].x;
					float x3 = m_mergeRoomChunks[i + 1].x;
					if (x >= x2 && x < x3)
					{
						m_currentChunkIndex = i;
						break;
					}
					continue;
				}
				m_currentChunkIndex = i;
				break;
			}
		}
		if (m_currentChunkIndex != currentChunkIndex)
		{
			m_playerEnteredChunkRelay.Dispatch(m_mergeRoomChunks[m_currentChunkIndex]);
		}
	}
}
