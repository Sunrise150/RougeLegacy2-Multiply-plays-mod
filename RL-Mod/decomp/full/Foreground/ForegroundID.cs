namespace Foreground;

public struct ForegroundID
{
	public ForegroundLocation Location { get; }

	public float ZoomLevel { get; }

	public ForegroundID(ForegroundLocation location, float zoomLevel)
	{
		Location = location;
		ZoomLevel = zoomLevel;
	}
}
