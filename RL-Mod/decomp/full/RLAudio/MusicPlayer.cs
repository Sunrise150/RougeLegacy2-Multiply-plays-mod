using UnityEngine;

namespace RLAudio;

public class MusicPlayer : MonoBehaviour
{
	[SerializeField]
	private SongID m_song;

	[SerializeField]
	private bool m_stopOnDestroy = true;

	private void Start()
	{
		MusicManager.PlayMusic(m_song, isOverride: false);
	}

	private void OnDestroy()
	{
		if (m_stopOnDestroy)
		{
			Stop();
		}
	}

	public void Stop()
	{
		MusicManager.StopMusic();
	}
}
