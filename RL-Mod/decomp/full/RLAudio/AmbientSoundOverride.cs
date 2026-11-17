using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace RLAudio;

public class AmbientSoundOverride : MonoBehaviour
{
	[SerializeField]
	[EventRef]
	private string[] m_audioPaths;

	[SerializeField]
	[EventRef]
	private string m_snapshotPath;

	private EventInstance[] m_audioInstances;

	private EventInstance m_snapshotInstance;

	public EventInstance[] Audio
	{
		get
		{
			if (m_audioInstances == null)
			{
				m_audioInstances = new EventInstance[m_audioPaths.Length];
				for (int i = 0; i < m_audioInstances.Length; i++)
				{
					try
					{
						m_audioInstances[i] = RuntimeManager.CreateInstance(m_audioPaths[i]);
					}
					catch (EventNotFoundException)
					{
						Debug.LogFormat("<color=red>| {0} | The path specified at index <b>{1}</b> in the Audio Paths collection is invalid</color>", this, i);
						m_audioInstances[i] = default(EventInstance);
					}
				}
			}
			return m_audioInstances;
		}
	}

	public bool HasAudioOverride => m_audioPaths.Length != 0;

	public bool HasSnapshotOverride => !string.IsNullOrEmpty(m_snapshotPath);

	public EventInstance Snapshot
	{
		get
		{
			if (!string.IsNullOrEmpty(m_snapshotPath) && m_snapshotPath.Equals(null))
			{
				m_snapshotInstance = RuntimeManager.CreateInstance(m_snapshotPath);
			}
			return m_snapshotInstance;
		}
	}
}
