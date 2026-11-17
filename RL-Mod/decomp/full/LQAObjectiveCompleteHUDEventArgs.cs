public class LQAObjectiveCompleteHUDEventArgs : ObjectiveCompleteHUDEventArgs
{
	public bool DisplayPlayer { get; private set; }

	public LQAObjectiveCompleteHUDEventArgs(float displayDuration = 5f, string titleTextOverride = null, string subtitleTextOverride = null, string descriptionTextOverride = null, bool displayPlayer = false)
		: base(ObjectiveCompleteHUDType.LQA)
	{
		Initialize(displayDuration, titleTextOverride, subtitleTextOverride, descriptionTextOverride, displayPlayer);
	}

	public void Initialize(float displayDuration = 5f, string titleTextOverride = null, string subtitleTextOverride = null, string descriptionTextOverride = null, bool displayPlayer = false)
	{
		Initialize(ObjectiveCompleteHUDType.LQA, displayDuration, titleTextOverride, subtitleTextOverride, descriptionTextOverride);
		DisplayPlayer = displayPlayer;
	}
}
