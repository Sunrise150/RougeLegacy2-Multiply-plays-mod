public struct CameraLayerAndSubLayer
{
	public CameraLayer CameraLayer { get; }

	public int SubLayer { get; }

	public CameraLayerAndSubLayer(CameraLayer cameraLayer, int subLayer)
	{
		CameraLayer = cameraLayer;
		SubLayer = subLayer;
	}
}
