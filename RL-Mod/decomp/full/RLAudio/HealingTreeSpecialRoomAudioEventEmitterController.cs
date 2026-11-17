using System.Collections;
using FMOD.Studio;

namespace RLAudio;

public class HealingTreeSpecialRoomAudioEventEmitterController : DualChoiceSpecialRoomAudioEventEmitterController
{
	private AmbientSoundOverride m_ambientSoundOverride;

	protected override void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		base.OnPlayerEnterRoom(sender, eventArgs);
		if (m_ambientSoundOverride == null)
		{
			m_ambientSoundOverride = GetComponent<AmbientSoundOverride>();
		}
		StartCoroutine(WaitUntilAmbientSoundIsSet());
	}

	protected override void OnRoomComplete()
	{
		base.OnRoomComplete();
		SetAmbientParameter(AmbientSoundController.GetCurrentEventInstance());
	}

	private IEnumerator WaitUntilAmbientSoundIsSet()
	{
		bool isSet = false;
		EventInstance currentEventInstance = default(EventInstance);
		while (!isSet)
		{
			currentEventInstance = AmbientSoundController.GetCurrentEventInstance();
			if (currentEventInstance.isValid())
			{
				currentEventInstance.getDescription(out var description);
				description.getPath(out var path);
				if (path.Contains("amb_treeRoom") || path.Contains("amb_relicRoom"))
				{
					isSet = true;
				}
			}
			yield return null;
		}
		SetAmbientParameter(currentEventInstance);
	}

	private void SetAmbientParameter(EventInstance currentEventInstance)
	{
		float value = 0f;
		if (m_isRoomComplete)
		{
			value = 0.51f;
		}
		if (currentEventInstance.isValid())
		{
			currentEventInstance.setParameterByName("Tree", value);
		}
	}
}
