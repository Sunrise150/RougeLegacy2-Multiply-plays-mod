using System;
using FMODUnity;
using UnityEngine;

namespace RLAudio;

[Serializable]
public class AmbientSoundLibraryEntry : AudioLibraryEntry
{
	[SerializeField]
	[EventRef]
	private string[] m_smallRoom;

	[SerializeField]
	private AudioLibraryEventsEntry m_smallRoomEntry;

	[SerializeField]
	[EventRef]
	private string[] m_largeRoom;

	[SerializeField]
	private AudioLibraryEventsEntry m_largeRoomEntry;

	[SerializeField]
	[EventRef]
	private string[] m_transitionRoom;

	[SerializeField]
	private AudioLibraryEventsEntry m_transitionRoomEntry;

	[SerializeField]
	[EventRef]
	private string m_smallRoomSnapshot;

	[SerializeField]
	private AudioLibrarySnapshotEntry m_smallRoomSnapshotEntry;

	[SerializeField]
	[EventRef]
	private string m_largeRoomSnapshot;

	[SerializeField]
	private AudioLibrarySnapshotEntry m_largeRoomSnapshotEntry;

	[SerializeField]
	[EventRef]
	private string m_transitionRoomSnapshot;

	[SerializeField]
	private AudioLibrarySnapshotEntry m_transitionRoomSnapshotEntry;

	public string[] SmallRoom => m_smallRoom;

	public string[] LargeRoom => m_largeRoom;

	public string[] TransitionRoom => m_transitionRoom;

	public string SmallRoomSnapshot => m_smallRoomSnapshot;

	public string LargeRoomSnapshot => m_largeRoomSnapshot;

	public string TransitionRoomSnapshot => m_transitionRoomSnapshot;

	public AudioLibraryEventsEntry SmallRoomEntry => m_smallRoomEntry;

	public AudioLibraryEventsEntry LargeRoomEntry => m_largeRoomEntry;

	public AudioLibraryEventsEntry TransitionRoomEntry => m_transitionRoomEntry;

	public AudioLibrarySnapshotEntry SmallRoomSnapshotEntry => m_smallRoomSnapshotEntry;

	public AudioLibrarySnapshotEntry LargeRoomSnapshotEntry => m_largeRoomSnapshotEntry;

	public AudioLibrarySnapshotEntry TransitionRoomSnapshotEntry => m_transitionRoomSnapshotEntry;
}
