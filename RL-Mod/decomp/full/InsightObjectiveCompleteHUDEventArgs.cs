public class InsightObjectiveCompleteHUDEventArgs : ObjectiveCompleteHUDEventArgs
{
	public InsightType InsightType { get; private set; }

	public bool Discovered { get; private set; }

	public InsightObjectiveCompleteHUDEventArgs(InsightType insightType, bool discovered, float displayDuration = 5f, string titleTextOverride = null, string subtitleTextOverride = null, string descriptionTextOverride = null)
		: base(ObjectiveCompleteHUDType.Insight, displayDuration)
	{
		Initialize(insightType, discovered, displayDuration, titleTextOverride, subtitleTextOverride, descriptionTextOverride);
	}

	public void Initialize(InsightType insightType, bool discovered, float displayDuration = 5f, string titleTextOverride = null, string subtitleTextOverride = null, string descriptionTextOverride = null)
	{
		Initialize(ObjectiveCompleteHUDType.Insight, displayDuration, titleTextOverride, subtitleTextOverride, descriptionTextOverride);
		InsightType = insightType;
		Discovered = discovered;
	}
}
