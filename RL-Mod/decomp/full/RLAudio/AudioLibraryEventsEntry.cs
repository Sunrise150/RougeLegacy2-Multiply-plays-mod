using System;
using FMODUnity;
using UnityEngine;

namespace RLAudio;

[Serializable]
public class AudioLibraryEventsEntry
{
	[SerializeField]
	[EventRef]
	private string[] m_eventPaths;

	[SerializeField]
	private AudioSource m_source;

	public string[] EventPaths => m_eventPaths;

	public AudioSource Source => m_source;
}
