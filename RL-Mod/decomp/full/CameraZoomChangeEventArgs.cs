using System;

public class CameraZoomChangeEventArgs : EventArgs
{
	public float ZoomLevel { get; private set; }

	public float PreviousZoomLevel { get; private set; }

	public CameraZoomChangeEventArgs(float zoomLevel, float previousZoomLevel)
	{
		Initialize(zoomLevel, previousZoomLevel);
	}

	public void Initialize(float zoomLevel, float previousZoomLevel)
	{
		ZoomLevel = zoomLevel;
		PreviousZoomLevel = previousZoomLevel;
	}
}
