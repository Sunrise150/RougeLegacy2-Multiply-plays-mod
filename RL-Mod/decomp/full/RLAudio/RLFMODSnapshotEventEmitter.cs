namespace RLAudio;

public class RLFMODSnapshotEventEmitter : BaseFMODEventEmitter
{
	public override void Play()
	{
		if (m_eventInstance.isValid())
		{
			AudioManager.Play(this, m_eventInstance);
		}
	}
}
