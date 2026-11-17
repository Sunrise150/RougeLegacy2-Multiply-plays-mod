public interface IJukeboxOmniUIButton
{
	SongID SongType { get; set; }

	bool IsPlayingSong { get; }

	JukeboxOmniUIWindowController JukeboxWindowController { get; set; }

	JukeboxOmniUIEntry JukeboxEntry { get; set; }
}
