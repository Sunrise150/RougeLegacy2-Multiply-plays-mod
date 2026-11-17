using System;
using System.Collections.Generic;
using Rooms;
using UnityEngine;

[Serializable]
public class CompiledScene_ScriptableObject : ScriptableObject
{
	[ReadOnly]
	[SerializeField]
	private string m_sceneID;

	[ReadOnly]
	[SerializeField]
	private List<RoomMetaData> m_roomMetaData;

	private string m_debugColour = $"#{85:X2}{85:X2}{85:X2}";

	private List<Room> m_roomList;

	private bool m_roomListIsDirty;

	public string SceneID => m_sceneID;

	public List<Room> RoomList
	{
		get
		{
			if (m_roomList == null || m_roomList.Count == 0 || m_roomListIsDirty)
			{
				if (RoomMetaData != null)
				{
					m_roomList = new List<Room>();
					m_roomListIsDirty = false;
					for (int i = 0; i < RoomMetaData.Count; i++)
					{
						Room prefab = RoomMetaData[i].GetPrefab(throwOnNull: false);
						if (prefab != null)
						{
							m_roomList.Add(prefab);
							continue;
						}
						Debug.LogFormat("{0}: Room GameObject in Room GameObject List at index ({1}) in Compiled Scene SO ({2}) is null", Time.frameCount, i, base.name);
					}
				}
				else
				{
					m_roomList = new List<Room>();
					Debug.LogFormat("{0}: RoomMetaData list is null in Compiled Scene SO ({1})", Time.frameCount, base.name);
				}
			}
			return m_roomList;
		}
	}

	public List<RoomMetaData> RoomMetaData
	{
		get
		{
			return m_roomMetaData;
		}
		set
		{
			m_roomMetaData = value;
		}
	}
}
