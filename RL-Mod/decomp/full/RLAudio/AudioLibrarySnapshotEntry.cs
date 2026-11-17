using System;
using FMODUnity;
using UnityEngine;

namespace RLAudio;

[Serializable]
public class AudioLibrarySnapshotEntry
{
	[SerializeField]
	[EventRef]
	private string m_snapshotPath;

	[SerializeField]
	private AudioSource m_source;

	public string SnapshotPath => m_snapshotPath;

	public AudioSource Source => m_source;
}
