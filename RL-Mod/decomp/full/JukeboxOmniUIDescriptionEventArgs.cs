using System;

public class JukeboxOmniUIDescriptionEventArgs : EventArgs
{
	public SongID SongType { get; private set; }

	public bool IsPlaying { get; private set; }

	public JukeboxOmniUIDescriptionEventArgs(SongID songType, bool isPlaying)
	{
		Initialize(songType, isPlaying);
	}

	public void Initialize(SongID songType, bool isPlaying)
	{
		SongType = songType;
		IsPlaying = isPlaying;
	}
}
