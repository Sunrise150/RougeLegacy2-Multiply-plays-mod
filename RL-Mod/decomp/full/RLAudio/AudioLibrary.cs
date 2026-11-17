using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace RLAudio;

public abstract class AudioLibrary<T> : ScriptableObject where T : AudioLibraryEntry
{
	[SerializeField]
	[FormerlySerializedAs("entries")]
	protected List<T> m_entries;

	[SerializeField]
	protected T m_default;

	protected Dictionary<string, T> m_entryTable;

	protected T Default => m_default;

	public List<T> Entries
	{
		get
		{
			return m_entries;
		}
		set
		{
			if (!Application.isPlaying)
			{
				m_entries = value;
			}
		}
	}

	protected T GetAudioLibraryEntry(string key)
	{
		if (m_entryTable == null)
		{
			InitializeTable();
		}
		if (m_entryTable.ContainsKey(key))
		{
			return m_entryTable[key];
		}
		return Default;
	}

	private void InitializeTable()
	{
		m_entryTable = new Dictionary<string, T>();
		foreach (T entry in Entries)
		{
			m_entryTable.Add(entry.Key, entry);
		}
	}

	public bool GetContainsEntry(string key)
	{
		if (m_entryTable == null)
		{
			InitializeTable();
		}
		bool result = false;
		if (m_entryTable != null)
		{
			result = m_entryTable.ContainsKey(key);
		}
		return result;
	}
}
