using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Level Editor/Room Content")]
public class RoomContentMetaData : ScriptableObject
{
	[SerializeField]
	private RoomContentEntry[] m_contentEntries;

	public RoomContentEntry[] ContentEntries
	{
		get
		{
			return m_contentEntries;
		}
		private set
		{
			m_contentEntries = value;
		}
	}

	public void AddEntry(RoomContentType contentType, Vector2 localPosition, SpawnConditionsEntry[] spawnConditions)
	{
		if (!Application.isPlaying)
		{
			RoomContentEntry item = new RoomContentEntry(contentType, localPosition, spawnConditions);
			if (ContentEntries == null)
			{
				ContentEntries = new RoomContentEntry[0];
			}
			List<RoomContentEntry> list = ContentEntries.ToList();
			list.Add(item);
			ContentEntries = list.ToArray();
		}
	}

	public void ClearEntries()
	{
		if (!Application.isPlaying)
		{
			ContentEntries = new RoomContentEntry[0];
		}
	}
}
