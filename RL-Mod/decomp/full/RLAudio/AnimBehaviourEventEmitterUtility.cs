using FMOD.Studio;

namespace RLAudio;

public static class AnimBehaviourEventEmitterUtility
{
	public static PARAMETER_ID GetParameterID(EventDescription eventDescription, string parameterName)
	{
		eventDescription.getParameterDescriptionByName(parameterName, out var parameter);
		return parameter.id;
	}
}
