using System;

public class ObjectiveCompleteHUDEventArgs : EventArgs
{
	public ObjectiveCompleteHUDType HUDType { get; private set; }

	public float DisplayDuration { get; private set; }

	public string TitleTextOverride { get; private set; }

	public string SubTitleTextOverride { get; private set; }

	public string DescriptionTextOverride { get; private set; }

	public ObjectiveCompleteHUDEventArgs(ObjectiveCompleteHUDType hudType, float displayDuration = 5f, string titleTextOverride = null, string subtitleTextOverride = null, string descriptionTextOverride = null)
	{
		Initialize(hudType, displayDuration, titleTextOverride, subtitleTextOverride, descriptionTextOverride);
	}

	public void Initialize(ObjectiveCompleteHUDType hudType, float displayDuration = 5f, string titleTextOverride = null, string subtitleTextOverride = null, string descriptionTextOverride = null)
	{
		HUDType = hudType;
		DisplayDuration = displayDuration;
		TitleTextOverride = titleTextOverride;
		SubTitleTextOverride = subtitleTextOverride;
		DescriptionTextOverride = descriptionTextOverride;
	}
}
