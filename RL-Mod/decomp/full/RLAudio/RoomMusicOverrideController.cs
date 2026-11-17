using UnityEngine;

namespace RLAudio;

public class RoomMusicOverrideController : MonoBehaviour
{
	[SerializeField]
	private SongID m_music;

	public SongID Music => m_music;

	public void SetOverride(SongID songID)
	{
		m_music = songID;
	}
}
