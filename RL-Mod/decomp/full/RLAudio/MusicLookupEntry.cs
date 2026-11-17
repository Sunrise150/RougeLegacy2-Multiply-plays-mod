using System;
using FMODUnity;
using UnityEngine;

namespace RLAudio;

[Serializable]
public class MusicLookupEntry
{
	[SerializeField]
	private SongID m_id;

	[SerializeField]
	[EventRef]
	private string m_eventRef;

	public SongID ID => m_id;

	public string EventRef => m_eventRef;
}
