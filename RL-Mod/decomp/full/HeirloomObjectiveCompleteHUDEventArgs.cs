public class HeirloomObjectiveCompleteHUDEventArgs : ObjectiveCompleteHUDEventArgs
{
	public HeirloomType HeirloomType { get; private set; }

	public HeirloomObjectiveCompleteHUDEventArgs(HeirloomType heirloomType, float displayDuration = 5f, string titleTextOverride = null, string subtitleTextOverride = null, string descriptionTextOverride = null)
		: base(ObjectiveCompleteHUDType.Heirloom, displayDuration)
	{
		Initialize(heirloomType, displayDuration, titleTextOverride, subtitleTextOverride, descriptionTextOverride);
	}

	public void Initialize(HeirloomType heirloomType, float displayDuration = 5f, string titleTextOverride = null, string subtitleTextOverride = null, string descriptionTextOverride = null)
	{
		Initialize(ObjectiveCompleteHUDType.Heirloom, displayDuration, titleTextOverride, subtitleTextOverride, descriptionTextOverride);
		HeirloomType = heirloomType;
	}
}
