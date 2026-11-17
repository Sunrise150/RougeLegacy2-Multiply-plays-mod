using System;
using UnityEngine;

namespace RLAudio;

[Serializable]
public class MusicLibraryEntry : AudioLibraryEntry
{
	[SerializeField]
	private SongID[] m_musicTracks;

	public SongID[] MusicTracks => m_musicTracks;
}
