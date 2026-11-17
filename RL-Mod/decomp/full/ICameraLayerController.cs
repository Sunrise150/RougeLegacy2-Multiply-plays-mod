public interface ICameraLayerController
{
	CameraLayer CameraLayer { get; }

	int SubLayer { get; }

	void SetCameraLayer(CameraLayer value);

	void SetSubLayer(int value, bool isDeco = false);
}
