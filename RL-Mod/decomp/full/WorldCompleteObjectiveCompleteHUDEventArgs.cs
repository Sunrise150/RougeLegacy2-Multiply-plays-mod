public class WorldCompleteObjectiveCompleteHUDEventArgs : ObjectiveCompleteHUDEventArgs
{
	public WorldCompleteObjectiveCompleteHUDEventArgs(float displayDuration = 5f, string titleTextOverride = null, string subtitleTextOverride = null, string descriptionTextOverride = null)
		: base(ObjectiveCompleteHUDType.WorldComplete, displayDuration)
	{
		Initialize(displayDuration, titleTextOverride, subtitleTextOverride, descriptionTextOverride);
	}

	public void Initialize(float displayDuration = 5f, string titleTextOverride = null, string subtitleTextOverride = null, string descriptionTextOverride = null)
	{
		Initialize(ObjectiveCompleteHUDType.WorldComplete, displayDuration, titleTextOverride, subtitleTextOverride, descriptionTextOverride);
	}
}
